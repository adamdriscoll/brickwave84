using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutBossGateType
    {
        PaddlePunk = 0,
        BrickosaurusWrecks = 1,
        MainframeManiac = 2,
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
            BreakoutBossGateType.BrickosaurusWrecks => $"Boss Gate {GateIndex + 1} - Brickosaurus Wrecks",
            BreakoutBossGateType.MainframeManiac => $"Boss Gate {GateIndex + 1} - Mainframe Maniac",
            BreakoutBossGateType.PaddlePunk => $"Boss Gate {GateIndex + 1} - The Paddle Punk",
            _ => $"Boss Gate {GateIndex + 1}",
        };

        public string HudLabel => BossType switch
        {
            BreakoutBossGateType.BrickosaurusWrecks => "BRICKOSAURUS",
            BreakoutBossGateType.MainframeManiac => "MAINFRAME",
            BreakoutBossGateType.PaddlePunk => "PADDLE PUNK",
            _ => "BOSS GATE",
        };
    }

    internal static class BreakoutRunProgression
    {
        public const int TargetLevelCount = 10;
        public const int MinRogueIntensity = 1;
        public const int MaxRogueIntensity = 50;

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

        public static int ClampRogueIntensity(int intensity)
        {
            return Mathf.Clamp(intensity, MinRogueIntensity, MaxRogueIntensity);
        }

        public static float GetRogueIntensityProgress(int intensity)
        {
            return Mathf.InverseLerp(MinRogueIntensity, MaxRogueIntensity, ClampRogueIntensity(intensity));
        }

        public static float GetRogueIntensityBallSpeedMultiplier(int intensity)
        {
            return Mathf.Lerp(1f, 1.18f, GetRogueIntensityProgress(intensity));
        }

        public static Color GetRogueIntensityGaugeColor(int intensity)
        {
            var progress = GetRogueIntensityProgress(intensity);
            var mint = new Color(0.45f, 0.95f, 0.72f, 1f);
            var yellow = new Color(1f, 0.87f, 0.36f, 1f);
            var red = new Color(0.99f, 0.27f, 0.31f, 1f);

            return progress < 0.5f
                ? Color.Lerp(mint, yellow, progress / 0.5f)
                : Color.Lerp(yellow, red, (progress - 0.5f) / 0.5f);
        }

        public static bool TryGetBossGateAfterLevel(int levelIndex, out BreakoutBossGate bossGate)
        {
            for (var index = 0; index < BossGateTriggerLevelIndexes.Length; index++)
            {
                if (BossGateTriggerLevelIndexes[index] == levelIndex)
                {
                    bossGate = new BreakoutBossGate(index, levelIndex, ResolveBossGateType(index));
                    return true;
                }
            }

            bossGate = default;
            return false;
        }

        private static BreakoutBossGateType ResolveBossGateType(int gateIndex)
        {
            return gateIndex switch
            {
                1 => BreakoutBossGateType.BrickosaurusWrecks,
                2 => BreakoutBossGateType.MainframeManiac,
                _ => BreakoutBossGateType.PaddlePunk,
            };
        }

        public static float GetBossGateBallSpeedMultiplier(BreakoutBossGate bossGate)
        {
            return 0.96f + (Mathf.Clamp(bossGate.GateIndex, 0, 2) * 0.06f);
        }
    }
}
