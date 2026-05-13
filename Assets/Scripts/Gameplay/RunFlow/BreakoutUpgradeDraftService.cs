using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutRunDraftOfferKind
    {
        RunUpgrade,
        DropUnlock,
    }

    internal sealed class BreakoutRunDraftOffer
    {
        private const float HelpfulDropUnlockDraftWeight = 0.85f;
        private const float HarmfulDropUnlockDraftWeight = 0.45f;

        private BreakoutRunDraftOffer(
            BreakoutRunDraftOfferKind kind,
            RunUpgradeDefinition upgradeDefinition,
            PowerUpDefinition dropUnlockDefinition)
        {
            Kind = kind;
            UpgradeDefinition = upgradeDefinition;
            DropUnlockDefinition = dropUnlockDefinition;
        }

        public BreakoutRunDraftOfferKind Kind { get; }

        public RunUpgradeDefinition UpgradeDefinition { get; }

        public PowerUpDefinition DropUnlockDefinition { get; }

        public string OfferId => Kind == BreakoutRunDraftOfferKind.RunUpgrade
            ? UpgradeDefinition?.UpgradeId ?? string.Empty
            : BreakoutPowerUpIdentity.GetStableId(DropUnlockDefinition);

        public string DisplayName => Kind == BreakoutRunDraftOfferKind.RunUpgrade
            ? UpgradeDefinition?.DisplayName ?? "Missing Upgrade"
            : DropUnlockDefinition?.DisplayName ?? "Missing Drop";

        public float DraftWeight => Kind == BreakoutRunDraftOfferKind.RunUpgrade
            ? UpgradeDefinition?.DraftWeight ?? 0f
            : ResolveDropUnlockDraftWeight(DropUnlockDefinition);

        public static BreakoutRunDraftOffer FromRunUpgrade(RunUpgradeDefinition definition)
        {
            return new BreakoutRunDraftOffer(BreakoutRunDraftOfferKind.RunUpgrade, definition, null);
        }

        public static BreakoutRunDraftOffer FromDropUnlock(PowerUpDefinition definition)
        {
            return new BreakoutRunDraftOffer(BreakoutRunDraftOfferKind.DropUnlock, null, definition);
        }

        private static float ResolveDropUnlockDraftWeight(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            var polarityWeight = definition.IsBeneficial ? HelpfulDropUnlockDraftWeight : HarmfulDropUnlockDraftWeight;
            return polarityWeight * BreakoutRarityRules.GetDraftWeightMultiplier(definition.Rarity);
        }
    }

    internal static class BreakoutPowerUpIdentity
    {
        public static string GetStableId(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(definition.PowerUpId))
            {
                return definition.PowerUpId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(definition.name))
            {
                return Normalize(definition.name);
            }

            return Normalize(definition.DisplayName);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Replace(' ', '_').Replace('-', '_').ToLowerInvariant();
        }
    }

    internal sealed class BreakoutUpgradeDraftService
    {
        private readonly List<RunUpgradeDefinition> loadedUpgradeDefinitions;
        private readonly List<PowerUpDefinition> loadedDropUnlockDefinitions;

        public BreakoutUpgradeDraftService(
            List<RunUpgradeDefinition> loadedUpgradeDefinitions,
            List<PowerUpDefinition> loadedDropUnlockDefinitions = null)
        {
            this.loadedUpgradeDefinitions = loadedUpgradeDefinitions ?? new List<RunUpgradeDefinition>();
            this.loadedDropUnlockDefinitions = loadedDropUnlockDefinitions ?? new List<PowerUpDefinition>();
        }

        public BreakoutRunDraftOffer[] GenerateDraft(BreakoutRunState runState, RunSettings runSettings, int runSeed, int levelIndex, int offerCount)
        {
            if (runState == null || offerCount <= 0)
            {
                return Array.Empty<BreakoutRunDraftOffer>();
            }

            var available = new List<BreakoutRunDraftOffer>();

            for (var index = 0; index < loadedUpgradeDefinitions.Count; index++)
            {
                var definition = loadedUpgradeDefinitions[index];

                if (runState.CanOffer(definition) && IsUpgradeCompatibleWithRun(definition, runSettings))
                {
                    available.Add(BreakoutRunDraftOffer.FromRunUpgrade(definition));
                }
            }

            for (var index = 0; index < loadedDropUnlockDefinitions.Count; index++)
            {
                var definition = loadedDropUnlockDefinitions[index];
                var offer = BreakoutRunDraftOffer.FromDropUnlock(definition);

                if (runState.CanOffer(offer) && IsDropUnlockCompatibleWithRun(definition, runSettings))
                {
                    available.Add(offer);
                }
            }

            if (available.Count == 0)
            {
                return Array.Empty<BreakoutRunDraftOffer>();
            }

            var draftRandom = CreateDraftRandom(runState, runSeed, levelIndex);
            var offers = new List<BreakoutRunDraftOffer>();

            while (offers.Count < offerCount && available.Count > 0)
            {
                var selectedIndex = WeightedPickIndex(available, draftRandom);
                offers.Add(available[selectedIndex]);
                available.RemoveAt(selectedIndex);
            }

            return offers.ToArray();
        }

        private static bool IsDropUnlockCompatibleWithRun(PowerUpDefinition definition, RunSettings runSettings)
        {
            if (definition == null || runSettings == null || !runSettings.IsRogueMode)
            {
                return false;
            }

            if (runSettings.DropPoolMode == DropPoolMode.Disabled)
            {
                return false;
            }

            return definition.IsBeneficial
                && definition.IsUnlockedForLadderIntensity(runSettings.RogueIntensity);
        }

        private static bool IsUpgradeCompatibleWithRun(RunUpgradeDefinition definition, RunSettings runSettings)
        {
            if (definition == null)
            {
                return false;
            }

            if (runSettings != null
                && runSettings.ScoringMode == RunScoringMode.HighScore
                && definition.BonusLives > 0)
            {
                return false;
            }

            return true;
        }

        private static DeterministicRandomService CreateDraftRandom(BreakoutRunState runState, int runSeed, int levelIndex)
        {
            var combinedSeed = DeterministicRandomService.CombineSeed(runSeed, levelIndex + 1);
            combinedSeed = DeterministicRandomService.CombineSeed(combinedSeed, runState.ClearedLevelCount + 1);
            var chosenUpgrades = runState.ChosenUpgrades;

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var definition = chosenUpgrades[index];
                combinedSeed = DeterministicRandomService.CombineSeed(combinedSeed, StableHash(definition != null ? definition.UpgradeId : string.Empty));
                combinedSeed = DeterministicRandomService.CombineSeed(combinedSeed, index + 1);
            }

            var chosenDropUnlocks = runState.ChosenDropUnlocks;

            for (var index = 0; index < chosenDropUnlocks.Count; index++)
            {
                var definition = chosenDropUnlocks[index];
                combinedSeed = DeterministicRandomService.CombineSeed(combinedSeed, StableHash(BreakoutPowerUpIdentity.GetStableId(definition)));
                combinedSeed = DeterministicRandomService.CombineSeed(combinedSeed, index + 101);
            }

            return new DeterministicRandomService(combinedSeed);
        }

        private static int WeightedPickIndex(List<BreakoutRunDraftOffer> definitions, DeterministicRandomService draftRandom)
        {
            var totalWeight = 0f;

            for (var index = 0; index < definitions.Count; index++)
            {
                totalWeight += Mathf.Max(0f, definitions[index].DraftWeight);
            }

            if (totalWeight <= 0f)
            {
                return definitions.Count - 1;
            }

            var roll = draftRandom.Range(0f, totalWeight);

            for (var index = 0; index < definitions.Count; index++)
            {
                roll -= Mathf.Max(0f, definitions[index].DraftWeight);

                if (roll <= 0f)
                {
                    return index;
                }
            }

            return definitions.Count - 1;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = (int)2166136261;
                var resolvedValue = value ?? string.Empty;

                for (var index = 0; index < resolvedValue.Length; index++)
                {
                    hash ^= resolvedValue[index];
                    hash *= 16777619;
                }

                return hash;
            }
        }
    }
}
