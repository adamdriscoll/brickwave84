using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutBossGateType
    {
        PaddlePunk = 0,
    }

    internal readonly struct BreakoutBossGate
    {
        public BreakoutBossGate(int gateIndex, int triggerLevelIndex, BreakoutBossGateType bossType)
        {
            GateIndex = Mathf.Max(0, gateIndex);
            TriggerLevelIndex = Mathf.Max(0, triggerLevelIndex);
            BossType = bossType;
        }

        public int GateIndex { get; }

        public int TriggerLevelIndex { get; }

        public BreakoutBossGateType BossType { get; }

        public string DisplayName => BossType switch
        {
            BreakoutBossGateType.PaddlePunk => $"Boss Gate {GateIndex + 1} - The Paddle Punk",
            _ => $"Boss Gate {GateIndex + 1}",
        };

        public string HudLabel => BossType switch
        {
            BreakoutBossGateType.PaddlePunk => "PADDLE PUNK",
            _ => "BOSS GATE",
        };
    }

    internal static class BreakoutRunProgression
    {
        public const int TargetLevelCount = 10;

        private static readonly int[] BossGateTriggerLevelIndexes =
        {
            2,
            5,
            9,
        };

        public static bool HasNextLevel(LevelDefinition currentLevel, int currentLevelIndex, int loadedLevelCount)
        {
            return currentLevel != null
                && loadedLevelCount > 0
                && currentLevelIndex < TargetLevelCount - 1;
        }

        public static float GetLevelProgress(int levelIndex)
        {
            return Mathf.Clamp01(Mathf.Max(0, levelIndex) / (float)(TargetLevelCount - 1));
        }

        public static float GetRogueStageBallSpeedMultiplier(int levelIndex)
        {
            return Mathf.Lerp(1f, 1.1f, GetLevelProgress(levelIndex));
        }

        public static bool TryGetBossGateAfterLevel(int levelIndex, out BreakoutBossGate bossGate)
        {
            for (var index = 0; index < BossGateTriggerLevelIndexes.Length; index++)
            {
                if (BossGateTriggerLevelIndexes[index] == levelIndex)
                {
                    bossGate = new BreakoutBossGate(index, levelIndex, BreakoutBossGateType.PaddlePunk);
                    return true;
                }
            }

            bossGate = default;
            return false;
        }

        public static float GetBossGateBallSpeedMultiplier(BreakoutBossGate bossGate)
        {
            return 0.96f + (Mathf.Clamp(bossGate.GateIndex, 0, 2) * 0.06f);
        }
    }
}
