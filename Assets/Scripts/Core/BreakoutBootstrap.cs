using GetBricked.Gameplay;
using UnityEngine;

namespace GetBricked.Core
{
    public static class BreakoutBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameController()
        {
            if (Object.FindFirstObjectByType<BreakoutGameController>() != null)
            {
                return;
            }

            var gameRoot = new GameObject("Breakout Game");
            gameRoot.AddComponent<BreakoutGameController>();
        }
    }
}
