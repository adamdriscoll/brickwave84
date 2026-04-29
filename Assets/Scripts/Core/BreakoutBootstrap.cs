using GetBricked.Gameplay;
using UnityEngine;

namespace GetBricked.Core
{
    public static class BreakoutBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePrototypeController()
        {
            if (Object.FindFirstObjectByType<BreakoutGameController>() != null)
            {
                return;
            }

            var prototypeRoot = new GameObject("Breakout Prototype");
            prototypeRoot.AddComponent<BreakoutGameController>();
        }
    }
}
