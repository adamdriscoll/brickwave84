using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutRogueRunController
    {
        private const int StartingLives = 3;
        private const int DraftOfferCount = 3;

        private static readonly string[] StartingDropIds =
        {
            "large_paddle",
            "multi_ball",
            "slow_ball",
            "shield_wall",
        };

        private readonly List<PowerUpDefinition> loadedPowerUpDefinitions;
        private readonly BreakoutUpgradeDraftService draftService;

        public BreakoutRogueRunController(
            List<RunUpgradeDefinition> loadedRunUpgradeDefinitions,
            List<PowerUpDefinition> loadedPowerUpDefinitions)
        {
            this.loadedPowerUpDefinitions = loadedPowerUpDefinitions ?? new List<PowerUpDefinition>();
            draftService = new BreakoutUpgradeDraftService(loadedRunUpgradeDefinitions, this.loadedPowerUpDefinitions);
        }

        public RunSettings BuildRunSettings(int seed, int lifeLossScorePenalty, ThemeDefinition themeDefinition)
        {
            return new RunSettings(
                seed,
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                StartingLives,
                lifeLossScorePenalty,
                1,
                1f,
                1f,
                1f,
                1f,
                DropPoolMode.Mixed,
                false,
                themeDefinition,
                RunGameMode.Rogue);
        }

        public void InitializeRunState(BreakoutRunState runState)
        {
            if (runState == null)
            {
                return;
            }

            runState.SetInitialDropUnlocks(BuildStartingDropPool());
        }

        public BreakoutRunDraftOffer[] GenerateDraft(BreakoutRunState runState, RunSettings runSettings, int levelIndex)
        {
            if (runSettings == null)
            {
                return Array.Empty<BreakoutRunDraftOffer>();
            }

            return draftService.GenerateDraft(runState, runSettings, runSettings.Seed, levelIndex, DraftOfferCount);
        }

        public float GetStageBallSpeedMultiplier(int levelIndex)
        {
            return BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(levelIndex);
        }

        public bool IsDropAllowed(BreakoutRunState runState, PowerUpDefinition definition)
        {
            return runState != null && runState.IsDropUnlocked(definition);
        }

        public BreakoutRogueRunResult BuildResult(RunSettings settings, bool completed, int stageReached, int score)
        {
            return BreakoutRogueRunResultStore.BuildResult(
                settings,
                completed,
                stageReached,
                BreakoutRogueRunResultStore.DefaultPaddleLabel,
                score);
        }

        private List<PowerUpDefinition> BuildStartingDropPool()
        {
            var startingDrops = new List<PowerUpDefinition>();

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (IsStartingDrop(definition))
                {
                    startingDrops.Add(definition);
                }
            }

            if (startingDrops.Count > 0)
            {
                return startingDrops;
            }

            for (var index = 0; index < loadedPowerUpDefinitions.Count && startingDrops.Count < 3; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (definition != null && definition.IsBeneficial)
                {
                    startingDrops.Add(definition);
                }
            }

            return startingDrops;
        }

        private static bool IsStartingDrop(PowerUpDefinition definition)
        {
            var candidateId = BreakoutPowerUpIdentity.GetStableId(definition);

            if (string.IsNullOrWhiteSpace(candidateId))
            {
                return false;
            }

            for (var index = 0; index < StartingDropIds.Length; index++)
            {
                if (string.Equals(candidateId, StartingDropIds[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
