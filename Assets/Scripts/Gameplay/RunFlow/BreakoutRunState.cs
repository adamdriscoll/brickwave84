using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutRunUpgradeModifiers
    {
        public BreakoutRunUpgradeModifiers(
            float paddleWidthMultiplier,
            float ballSpeedMultiplier,
            float dropChanceMultiplier,
            float pickupFallSpeedMultiplier,
            float wavyPaddleStrength,
            float brickMagnetStrength,
            float specialBrickEffectMultiplier,
            int extraBallsPerServe,
            int tiltWarningSavesPerLevel,
            int spareFuseSavesPerRun,
            int freeMissileShotsPerLevel,
            float riskRebateHelpfulEffectExtensionSeconds,
            int maxActiveHazardTimedEffectStacks,
            float warpHandleSpeedTrimRatio)
        {
            PaddleWidthMultiplier = paddleWidthMultiplier;
            BallSpeedMultiplier = ballSpeedMultiplier;
            DropChanceMultiplier = dropChanceMultiplier;
            PickupFallSpeedMultiplier = pickupFallSpeedMultiplier;
            WavyPaddleStrength = wavyPaddleStrength;
            BrickMagnetStrength = brickMagnetStrength;
            SpecialBrickEffectMultiplier = specialBrickEffectMultiplier;
            ExtraBallsPerServe = extraBallsPerServe;
            TiltWarningSavesPerLevel = tiltWarningSavesPerLevel;
            SpareFuseSavesPerRun = spareFuseSavesPerRun;
            FreeMissileShotsPerLevel = freeMissileShotsPerLevel;
            RiskRebateHelpfulEffectExtensionSeconds = riskRebateHelpfulEffectExtensionSeconds;
            MaxActiveHazardTimedEffectStacks = maxActiveHazardTimedEffectStacks;
            WarpHandleSpeedTrimRatio = warpHandleSpeedTrimRatio;
        }

        public float PaddleWidthMultiplier { get; }

        public float BallSpeedMultiplier { get; }

        public float DropChanceMultiplier { get; }

        public float PickupFallSpeedMultiplier { get; }

        public float WavyPaddleStrength { get; }

        public float BrickMagnetStrength { get; }

        public float SpecialBrickEffectMultiplier { get; }

        public int ExtraBallsPerServe { get; }

        public int TiltWarningSavesPerLevel { get; }

        public int SpareFuseSavesPerRun { get; }

        public int FreeMissileShotsPerLevel { get; }

        public float RiskRebateHelpfulEffectExtensionSeconds { get; }

        public int MaxActiveHazardTimedEffectStacks { get; }

        public float WarpHandleSpeedTrimRatio { get; }
    }

    internal sealed class BreakoutRunState
    {
        private readonly List<RunUpgradeDefinition> chosenUpgrades = new List<RunUpgradeDefinition>();
        private readonly List<PowerUpDefinition> chosenDropUnlocks = new List<PowerUpDefinition>();
        private readonly List<PowerUpDefinition> unlockedDropDefinitions = new List<PowerUpDefinition>();
        private readonly List<BreakoutRunDraftOffer> pendingDraftOffers = new List<BreakoutRunDraftOffer>();
        private readonly Dictionary<string, int> upgradeStacks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> unlockedDropIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<RunUpgradeDefinition> ChosenUpgrades => chosenUpgrades;

        public IReadOnlyList<PowerUpDefinition> ChosenDropUnlocks => chosenDropUnlocks;

        public IReadOnlyList<PowerUpDefinition> UnlockedDropDefinitions => unlockedDropDefinitions;

        public IReadOnlyList<BreakoutRunDraftOffer> PendingDraftOffers => pendingDraftOffers;

        public int ClearedLevelCount { get; private set; }

        public bool HasActiveBuild => chosenUpgrades.Count > 0 || chosenDropUnlocks.Count > 0;

        public void Reset()
        {
            chosenUpgrades.Clear();
            chosenDropUnlocks.Clear();
            unlockedDropDefinitions.Clear();
            pendingDraftOffers.Clear();
            upgradeStacks.Clear();
            unlockedDropIds.Clear();
            ClearedLevelCount = 0;
        }

        public void RegisterLevelClear()
        {
            ClearedLevelCount++;
        }

        public void SetInitialDropUnlocks(IList<PowerUpDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            for (var index = 0; index < definitions.Count; index++)
            {
                AddUnlockedDrop(definitions[index], countAsChosenReward: false);
            }
        }

        public bool IsDropUnlocked(PowerUpDefinition definition)
        {
            var dropId = BreakoutPowerUpIdentity.GetStableId(definition);
            return !string.IsNullOrWhiteSpace(dropId) && unlockedDropIds.Contains(dropId);
        }

        public bool UnlockDrop(PowerUpDefinition definition)
        {
            return AddUnlockedDrop(definition, countAsChosenReward: false);
        }

        public void SetPendingDraftOffers(IList<BreakoutRunDraftOffer> offers)
        {
            pendingDraftOffers.Clear();

            if (offers == null)
            {
                return;
            }

            for (var index = 0; index < offers.Count; index++)
            {
                if (offers[index] != null)
                {
                    pendingDraftOffers.Add(offers[index]);
                }
            }
        }

        public void ClearPendingDraftOffers()
        {
            pendingDraftOffers.Clear();
        }

        public int GetStackCount(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            return GetStackCount(definition.UpgradeId);
        }

        public int GetStackCount(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            return upgradeStacks.TryGetValue(upgradeId.Trim(), out var count) ? count : 0;
        }

        public bool HasConflict(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            var candidateId = definition.UpgradeId;

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var chosen = chosenUpgrades[index];

                if (chosen == null)
                {
                    continue;
                }

                if (UpgradeListsConflict(candidateId, definition.ExcludedUpgradeIds, chosen.UpgradeId, chosen.ExcludedUpgradeIds))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanOffer(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return GetStackCount(definition) < definition.MaxStacks && !HasConflict(definition);
        }

        public bool CanOffer(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return false;
            }

            return offer.Kind switch
            {
                BreakoutRunDraftOfferKind.RunUpgrade => CanOffer(offer.UpgradeDefinition),
                BreakoutRunDraftOfferKind.DropUnlock => !IsDropUnlocked(offer.DropUnlockDefinition)
                    && !string.IsNullOrWhiteSpace(BreakoutPowerUpIdentity.GetStableId(offer.DropUnlockDefinition)),
                _ => false,
            };
        }

        public bool TryApplyPendingDraftOffer(int offerIndex, out BreakoutRunDraftOffer appliedOffer)
        {
            appliedOffer = null;

            if (offerIndex < 0 || offerIndex >= pendingDraftOffers.Count)
            {
                return false;
            }

            var selectedUpgrade = pendingDraftOffers[offerIndex];

            if (!CanOffer(selectedUpgrade))
            {
                return false;
            }

            ApplyOffer(selectedUpgrade);
            appliedOffer = selectedUpgrade;
            pendingDraftOffers.Clear();
            return true;
        }

        public BreakoutRunUpgradeModifiers CalculateModifiers()
        {
            var paddleWidthMultiplier = 1f;
            var ballSpeedMultiplier = 1f;
            var dropChanceMultiplier = 1f;
            var pickupFallSpeedMultiplier = 1f;
            var wavyPaddleStrength = 0f;
            var brickMagnetStrength = 0f;
            var specialBrickEffectMultiplier = 1f;
            var extraBallsPerServe = 0;
            var tiltWarningSavesPerLevel = 0;
            var spareFuseSavesPerRun = 0;
            var freeMissileShotsPerLevel = 0;
            var riskRebateHelpfulEffectExtensionSeconds = 0f;
            var maxActiveHazardTimedEffectStacks = 0;
            var warpHandleSpeedTrimRatio = 0f;

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var upgrade = chosenUpgrades[index];

                if (upgrade == null)
                {
                    continue;
                }

                paddleWidthMultiplier *= upgrade.PaddleWidthMultiplier;
                ballSpeedMultiplier *= upgrade.BallSpeedMultiplier;
                dropChanceMultiplier *= upgrade.DropChanceMultiplier;
                pickupFallSpeedMultiplier *= upgrade.PickupFallSpeedMultiplier;
                wavyPaddleStrength = Mathf.Max(wavyPaddleStrength, upgrade.WavyPaddleStrength);
                brickMagnetStrength = Mathf.Max(brickMagnetStrength, upgrade.BrickMagnetStrength);
                specialBrickEffectMultiplier *= upgrade.SpecialBrickEffectMultiplier;
                extraBallsPerServe += upgrade.ExtraBallsPerServe;
                tiltWarningSavesPerLevel += upgrade.TiltWarningSavesPerLevel;
                spareFuseSavesPerRun += upgrade.SpareFuseSavesPerRun;
                freeMissileShotsPerLevel += upgrade.FreeMissileShotsPerLevel;
                riskRebateHelpfulEffectExtensionSeconds += upgrade.RiskRebateHelpfulEffectExtensionSeconds;
                warpHandleSpeedTrimRatio = Mathf.Max(warpHandleSpeedTrimRatio, upgrade.WarpHandleSpeedTrimRatio);

                if (upgrade.MaxActiveHazardTimedEffectStacks > 0)
                {
                    maxActiveHazardTimedEffectStacks = maxActiveHazardTimedEffectStacks <= 0
                        ? upgrade.MaxActiveHazardTimedEffectStacks
                        : Mathf.Min(maxActiveHazardTimedEffectStacks, upgrade.MaxActiveHazardTimedEffectStacks);
                }
            }

            return new BreakoutRunUpgradeModifiers(
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                dropChanceMultiplier,
                pickupFallSpeedMultiplier,
                wavyPaddleStrength,
                brickMagnetStrength,
                specialBrickEffectMultiplier,
                extraBallsPerServe,
                tiltWarningSavesPerLevel,
                spareFuseSavesPerRun,
                freeMissileShotsPerLevel,
                riskRebateHelpfulEffectExtensionSeconds,
                maxActiveHazardTimedEffectStacks,
                warpHandleSpeedTrimRatio);
        }

        private void ApplyUpgrade(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            chosenUpgrades.Add(definition);
            var upgradeId = definition.UpgradeId;
            upgradeStacks[upgradeId] = GetStackCount(upgradeId) + 1;
        }

        private void ApplyOffer(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return;
            }

            if (offer.Kind == BreakoutRunDraftOfferKind.RunUpgrade)
            {
                ApplyUpgrade(offer.UpgradeDefinition);
                return;
            }

            AddUnlockedDrop(offer.DropUnlockDefinition, countAsChosenReward: true);
        }

        private bool AddUnlockedDrop(PowerUpDefinition definition, bool countAsChosenReward)
        {
            var dropId = BreakoutPowerUpIdentity.GetStableId(definition);

            if (definition == null || string.IsNullOrWhiteSpace(dropId) || !unlockedDropIds.Add(dropId))
            {
                return false;
            }

            unlockedDropDefinitions.Add(definition);

            if (countAsChosenReward)
            {
                chosenDropUnlocks.Add(definition);
            }

            return true;
        }

        private static bool UpgradeListsConflict(
            string candidateId,
            string[] candidateExclusions,
            string chosenId,
            string[] chosenExclusions)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || string.IsNullOrWhiteSpace(chosenId))
            {
                return false;
            }

            return ArrayContains(candidateExclusions, chosenId) || ArrayContains(chosenExclusions, candidateId);
        }

        private static bool ArrayContains(string[] values, string candidate)
        {
            if (values == null || string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            for (var index = 0; index < values.Length; index++)
            {
                if (string.Equals(values[index], candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
