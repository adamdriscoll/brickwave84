using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutDeveloperLaunchField
    {
        Encounter = 0,
        Paddle = 1,
        Lives = 2,
        Heat = 3,
        Upgrade = 4,
        DropUnlock = 5,
    }

    internal readonly struct BreakoutDeveloperEncounter
    {
        public BreakoutDeveloperEncounter(int levelIndex)
        {
            LevelIndex = Mathf.Clamp(levelIndex, 0, BreakoutRunProgression.TargetLevelCount - 1);
        }

        public int LevelIndex { get; }

        public string DisplayName => $"Stage {LevelIndex + 1:00}";
    }

    internal sealed class BreakoutDeveloperLaunchState
    {
        public const int TotalEncounterCount = BreakoutRunProgression.TargetLevelCount;

        private readonly HashSet<string> selectedUpgradeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> selectedDropIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int EncounterIndex { get; private set; }

        public int PaddleIndex { get; private set; }

        public int LivesRemaining { get; private set; } = 3;

        public int Intensity { get; private set; } = BreakoutRunProgression.MinRogueIntensity;

        public int UpgradeIndex { get; private set; }

        public int DropUnlockIndex { get; private set; }

        public int SelectedUpgradeCount => selectedUpgradeIds.Count;

        public int SelectedDropUnlockCount => selectedDropIds.Count;

        public void Reset()
        {
            EncounterIndex = 0;
            PaddleIndex = 0;
            LivesRemaining = 3;
            Intensity = BreakoutRunProgression.MinRogueIntensity;
            UpgradeIndex = 0;
            DropUnlockIndex = 0;
            selectedUpgradeIds.Clear();
            selectedDropIds.Clear();
        }

        public void AdjustField(
            BreakoutDeveloperLaunchField field,
            int direction,
            IReadOnlyList<RunUpgradeDefinition> upgrades,
            IReadOnlyList<PowerUpDefinition> drops)
        {
            if (direction == 0)
            {
                return;
            }

            switch (field)
            {
                case BreakoutDeveloperLaunchField.Encounter:
                    EncounterIndex = Wrap(EncounterIndex + direction, TotalEncounterCount);
                    break;
                case BreakoutDeveloperLaunchField.Paddle:
                    PaddleIndex = Wrap(PaddleIndex + direction, BreakoutRoguePaddleCatalog.AllPaddles.Count);
                    break;
                case BreakoutDeveloperLaunchField.Lives:
                    LivesRemaining = Mathf.Clamp(LivesRemaining + direction, 1, 9);
                    break;
                case BreakoutDeveloperLaunchField.Heat:
                    Intensity = BreakoutRunProgression.ClampRogueIntensity(Intensity + direction);
                    break;
                case BreakoutDeveloperLaunchField.Upgrade:
                    UpgradeIndex = Wrap(UpgradeIndex + direction, Mathf.Max(1, upgrades?.Count ?? 0));
                    break;
                case BreakoutDeveloperLaunchField.DropUnlock:
                    DropUnlockIndex = Wrap(DropUnlockIndex + direction, Mathf.Max(1, drops?.Count ?? 0));
                    break;
            }
        }

        public void ToggleCurrentUpgrade(IReadOnlyList<RunUpgradeDefinition> upgrades)
        {
            var upgrade = ResolveCurrentUpgrade(upgrades);

            if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.UpgradeId))
            {
                return;
            }

            ToggleId(selectedUpgradeIds, upgrade.UpgradeId);
        }

        public void ToggleCurrentDropUnlock(IReadOnlyList<PowerUpDefinition> drops)
        {
            var drop = ResolveCurrentDropUnlock(drops);
            var dropId = BreakoutPowerUpIdentity.GetStableId(drop);

            if (drop == null || string.IsNullOrWhiteSpace(dropId))
            {
                return;
            }

            ToggleId(selectedDropIds, dropId);
        }

        public void ClearBuild()
        {
            selectedUpgradeIds.Clear();
            selectedDropIds.Clear();
        }

        public BreakoutDeveloperEncounter ResolveEncounter()
        {
            var normalizedIndex = Wrap(EncounterIndex, TotalEncounterCount);
            return new BreakoutDeveloperEncounter(normalizedIndex);
        }

        public BreakoutRoguePaddleDefinition ResolvePaddle()
        {
            var paddles = BreakoutRoguePaddleCatalog.AllPaddles;
            PaddleIndex = Wrap(PaddleIndex, paddles.Count);
            return paddles[PaddleIndex];
        }

        public RunUpgradeDefinition ResolveCurrentUpgrade(IReadOnlyList<RunUpgradeDefinition> upgrades)
        {
            if (upgrades == null || upgrades.Count == 0)
            {
                return null;
            }

            UpgradeIndex = Wrap(UpgradeIndex, upgrades.Count);
            return upgrades[UpgradeIndex];
        }

        public PowerUpDefinition ResolveCurrentDropUnlock(IReadOnlyList<PowerUpDefinition> drops)
        {
            if (drops == null || drops.Count == 0)
            {
                return null;
            }

            DropUnlockIndex = Wrap(DropUnlockIndex, drops.Count);
            return drops[DropUnlockIndex];
        }

        public bool IsUpgradeSelected(RunUpgradeDefinition upgrade)
        {
            return upgrade != null
                && !string.IsNullOrWhiteSpace(upgrade.UpgradeId)
                && selectedUpgradeIds.Contains(upgrade.UpgradeId);
        }

        public bool IsDropUnlockSelected(PowerUpDefinition drop)
        {
            var dropId = BreakoutPowerUpIdentity.GetStableId(drop);
            return !string.IsNullOrWhiteSpace(dropId) && selectedDropIds.Contains(dropId);
        }

        public bool ShouldApplyUpgrade(RunUpgradeDefinition upgrade)
        {
            return IsUpgradeSelected(upgrade);
        }

        public bool ShouldApplyDropUnlock(PowerUpDefinition drop)
        {
            return IsDropUnlockSelected(drop);
        }

        private static void ToggleId(HashSet<string> ids, string id)
        {
            if (ids.Contains(id))
            {
                ids.Remove(id);
                return;
            }

            ids.Add(id);
        }

        private static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return ((value % count) + count) % count;
        }
    }
}
