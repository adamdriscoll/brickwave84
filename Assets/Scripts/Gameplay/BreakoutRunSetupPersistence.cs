using System;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutRunSetupPersistence
    {
        private const string PersistedRunSetupKey = "GetBricked.RunSetup";
        private const int PersistedRunSetupVersion = 1;

        [Serializable]
        private sealed class PersistedRunSetup
        {
            public int Version = PersistedRunSetupVersion;
            public int Seed;
            public string PendingSeedText = string.Empty;
            public int DifficultyPreset = (int)RunDifficultyPreset.Standard;
            public int BallsPerServe = 1;
            public int PaddleWidthStep;
            public int BallSpeedStep;
            public int BrickDurabilityStep;
            public int DropPoolModeValue = (int)DropPoolMode.Mixed;
            public string ThemeId = string.Empty;
        }

        public static void Load(
            BreakoutRunSetupState runSetupState,
            Func<int> seedGenerator,
            Func<string, string> resolveThemeIdOrDefault)
        {
            if (runSetupState == null || !PlayerPrefs.HasKey(PersistedRunSetupKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(PersistedRunSetupKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                var persistedRunSetup = JsonUtility.FromJson<PersistedRunSetup>(json);

                if (persistedRunSetup == null || persistedRunSetup.Version != PersistedRunSetupVersion)
                {
                    return;
                }

                runSetupState.Restore(
                    persistedRunSetup.Seed > 0 ? persistedRunSetup.Seed : GenerateSeed(seedGenerator),
                    persistedRunSetup.PendingSeedText ?? string.Empty,
                    (RunDifficultyPreset)Mathf.Clamp(
                        persistedRunSetup.DifficultyPreset,
                        (int)RunDifficultyPreset.Casual,
                        (int)RunDifficultyPreset.Brutal),
                    Mathf.Clamp(persistedRunSetup.BallsPerServe, 1, 4),
                    Mathf.Clamp(persistedRunSetup.PaddleWidthStep, -2, 2),
                    Mathf.Clamp(persistedRunSetup.BallSpeedStep, -2, 2),
                    Mathf.Clamp(persistedRunSetup.BrickDurabilityStep, -2, 2),
                    (DropPoolMode)Mathf.Clamp(
                        persistedRunSetup.DropPoolModeValue,
                        (int)DropPoolMode.Mixed,
                        (int)DropPoolMode.Disabled),
                    resolveThemeIdOrDefault != null
                        ? resolveThemeIdOrDefault(persistedRunSetup.ThemeId)
                        : persistedRunSetup.ThemeId);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load persisted run setup. Using defaults instead. {exception.Message}");
            }
        }

        public static void Save(
            BreakoutRunSetupState runSetupState,
            Func<int> seedGenerator,
            Func<string, string> resolveThemeIdOrDefault)
        {
            if (runSetupState == null)
            {
                return;
            }

            var persistedRunSetup = new PersistedRunSetup
            {
                Seed = runSetupState.Seed > 0 ? runSetupState.Seed : GenerateSeed(seedGenerator),
                PendingSeedText = runSetupState.PendingSeedText ?? string.Empty,
                DifficultyPreset = (int)runSetupState.DifficultyPreset,
                BallsPerServe = runSetupState.BallsPerServe,
                PaddleWidthStep = runSetupState.PaddleWidthStep,
                BallSpeedStep = runSetupState.BallSpeedStep,
                BrickDurabilityStep = runSetupState.BrickDurabilityStep,
                DropPoolModeValue = (int)runSetupState.DropPoolMode,
                ThemeId = resolveThemeIdOrDefault != null
                    ? resolveThemeIdOrDefault(runSetupState.ThemeId)
                    : runSetupState.ThemeId,
            };

            PlayerPrefs.SetString(PersistedRunSetupKey, JsonUtility.ToJson(persistedRunSetup));
            PlayerPrefs.Save();
        }

        private static int GenerateSeed(Func<int> seedGenerator)
        {
            return seedGenerator != null ? seedGenerator() : 0;
        }
    }
}
