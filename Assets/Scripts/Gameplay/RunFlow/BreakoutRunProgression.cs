using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutRunProgression
    {
        public const int TargetLevelCount = 10;
        public const int MinRogueIntensity = 1;
        public const int MaxRogueIntensity = 50;

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

        public static int GetCompletedUnlockIntensityForRun(int activeIntensity)
        {
            return Mathf.Clamp(activeIntensity - 1, 0, MaxRogueIntensity);
        }

        public static float GetRogueIntensityProgress(int intensity)
        {
            return Mathf.InverseLerp(MinRogueIntensity, MaxRogueIntensity, ClampRogueIntensity(intensity));
        }

        public static float GetRogueIntensityBallSpeedMultiplier(int intensity)
        {
            return Mathf.Lerp(1f, 1.18f, GetRogueIntensityProgress(intensity));
        }

        public static int GetRogueHeatComplexityOffset(int intensity)
        {
            return Mathf.FloorToInt(GetRogueIntensityProgress(intensity) * 5.01f);
        }

        public static bool IsFinalStage(int levelIndex)
        {
            return levelIndex >= TargetLevelCount - 1;
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

    }
}
