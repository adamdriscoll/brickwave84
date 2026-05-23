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

        public RunSettings BuildRunSettings(
            int seed,
            int lifeLossScorePenalty,
            ThemeDefinition themeDefinition,
            string selectedPaddleLabel = null,
            int? intensityOverride = null,
            LevelGlitchSelection levelGlitchSelection = LevelGlitchSelection.Random,
            bool forceLevelGlitchRoll = false,
            bool ignoreLevelGlitchUnlocks = false)
        {
            var selectedPaddle = BreakoutRoguePaddleCatalog.Resolve(selectedPaddleLabel);
            var intensity = intensityOverride.HasValue
                ? BreakoutRunProgression.ClampRogueIntensity(intensityOverride.Value)
                : BreakoutRogueIntensityProgressStore.GetAvailableIntensity(selectedPaddle.DisplayName);
            return new RunSettings(
                seed,
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                StartingLives,
                lifeLossScorePenalty,
                1,
                selectedPaddle.WidthMultiplier,
                BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(intensity),
                1f,
                1f,
                DropPoolMode.Mixed,
                false,
                themeDefinition,
                RunGameMode.Rogue,
                intensity,
                selectedPaddle.DisplayName,
                selectedPaddle.SpeedMultiplier,
                levelGlitchesEnabled: true,
                levelGlitchChanceMultiplier: 1f,
                levelGlitchSelection: levelGlitchSelection,
                forceLevelGlitchRoll: forceLevelGlitchRoll,
                ignoreLevelGlitchUnlocks: ignoreLevelGlitchUnlocks);
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

        public int UnlockHazardsForClearedLevel(BreakoutRunState runState, RunSettings runSettings)
        {
            if (runState == null)
            {
                return 0;
            }

            var intensity = runSettings != null
                ? runSettings.RogueIntensity
                : BreakoutRunProgression.MinRogueIntensity;
            var hazards = BuildSortedHazardDropPool(intensity);
            var targetHazardCount = Mathf.Min(hazards.Count, GetAutoHazardUnlockCount(runState.ClearedLevelCount, intensity));
            var unlockedCount = 0;

            for (var index = 0; index < targetHazardCount; index++)
            {
                if (runState.UnlockDrop(hazards[index]))
                {
                    unlockedCount++;
                }
            }

            return unlockedCount;
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
                settings != null ? settings.SelectedPaddleLabel : BreakoutRogueRunResultStore.DefaultPaddleLabel,
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

        private List<PowerUpDefinition> BuildSortedHazardDropPool(int intensity)
        {
            var hazards = new List<PowerUpDefinition>();

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (definition != null
                    && !definition.IsBeneficial
                    && definition.IsUnlockedForLadderIntensity(intensity))
                {
                    hazards.Add(definition);
                }
            }

            hazards.Sort(CompareHazardUnlockOrder);
            return hazards;
        }

        private static int CompareHazardUnlockOrder(PowerUpDefinition left, PowerUpDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var rarityComparison = left.Rarity.CompareTo(right.Rarity);

            return rarityComparison != 0
                ? rarityComparison
                : string.Compare(
                    BreakoutPowerUpIdentity.GetStableId(left),
                    BreakoutPowerUpIdentity.GetStableId(right),
                    StringComparison.OrdinalIgnoreCase);
        }

        internal static int GetAutoHazardUnlockCount(int clearedLevelCount, int intensity)
        {
            if (clearedLevelCount <= 0)
            {
                return 0;
            }

            var stagePressure = clearedLevelCount / 3;
            var heatPressure = Mathf.FloorToInt(BreakoutRunProgression.GetRogueIntensityProgress(intensity) * 3.01f);
            return stagePressure + heatPressure;
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
