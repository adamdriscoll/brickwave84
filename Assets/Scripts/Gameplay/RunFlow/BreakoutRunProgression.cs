using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutRunProgression
    {
        public const int TargetLevelCount = 10;

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
    }
}
