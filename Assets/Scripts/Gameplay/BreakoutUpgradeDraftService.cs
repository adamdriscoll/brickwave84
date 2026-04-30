using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutUpgradeDraftService
    {
        private readonly List<RunUpgradeDefinition> loadedDefinitions;

        public BreakoutUpgradeDraftService(List<RunUpgradeDefinition> loadedDefinitions)
        {
            this.loadedDefinitions = loadedDefinitions ?? new List<RunUpgradeDefinition>();
        }

        public RunUpgradeDefinition[] GenerateDraft(BreakoutRunState runState, int runSeed, int levelIndex, int offerCount)
        {
            if (runState == null || loadedDefinitions.Count == 0 || offerCount <= 0)
            {
                return Array.Empty<RunUpgradeDefinition>();
            }

            var available = new List<RunUpgradeDefinition>();

            for (var index = 0; index < loadedDefinitions.Count; index++)
            {
                var definition = loadedDefinitions[index];

                if (runState.CanOffer(definition))
                {
                    available.Add(definition);
                }
            }

            if (available.Count == 0)
            {
                return Array.Empty<RunUpgradeDefinition>();
            }

            var draftRandom = CreateDraftRandom(runState, runSeed, levelIndex);
            var offers = new List<RunUpgradeDefinition>();

            while (offers.Count < offerCount && available.Count > 0)
            {
                var selectedIndex = WeightedPickIndex(available, draftRandom);
                offers.Add(available[selectedIndex]);
                available.RemoveAt(selectedIndex);
            }

            return offers.ToArray();
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

            return new DeterministicRandomService(combinedSeed);
        }

        private static int WeightedPickIndex(List<RunUpgradeDefinition> definitions, DeterministicRandomService draftRandom)
        {
            var totalWeight = 0f;

            for (var index = 0; index < definitions.Count; index++)
            {
                totalWeight += definitions[index].DraftWeight;
            }

            var roll = draftRandom.Range(0f, totalWeight);

            for (var index = 0; index < definitions.Count; index++)
            {
                roll -= definitions[index].DraftWeight;

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
