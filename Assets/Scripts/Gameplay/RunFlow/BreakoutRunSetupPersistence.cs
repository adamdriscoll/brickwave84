using System;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutRunSetupPersistence
    {
        private const string PersistedRunSetupKey = "GetBricked.RunSetup";
        private const int PersistedRunSetupVersion = 12;

        [Serializable]
        private sealed class PersistedRunSetup
        {
            public int Version = PersistedRunSetupVersion;
            public int Seed;
            public string PendingSeedText = string.Empty;
            public int DifficultyPreset = (int)RunDifficultyPreset.Standard;
            public int ScoringModeValue = (int)RunScoringMode.Classic;
            public int BallsPerServe = 1;
            public int PaddleWidthStep;
            public int BallSpeedStep;
            public int BrickDurabilityStep;
            public int DropPoolModeValue = (int)DropPoolMode.Mixed;
            public bool IsCapsulePartyEnabled;
            public bool AreLevelGlitchesEnabled;
            public int LevelGlitchSelection = (int)Gameplay.Data.LevelGlitchSelection.Off;
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

                if (persistedRunSetup == null
                    || (persistedRunSetup.Version != 1
                        && persistedRunSetup.Version != 2
                        && persistedRunSetup.Version != 3
                        && persistedRunSetup.Version != 4
                        && persistedRunSetup.Version != 5
                        && persistedRunSetup.Version != 6
                        && persistedRunSetup.Version != 7
                        && persistedRunSetup.Version != 8
                        && persistedRunSetup.Version != 9
                        && persistedRunSetup.Version != 10
                        && persistedRunSetup.Version != 11
                        && persistedRunSetup.Version != PersistedRunSetupVersion))
                {
                    return;
                }

                runSetupState.RestoreWithLevelGlitchSelection(
                    persistedRunSetup.Seed > 0 ? persistedRunSetup.Seed : GenerateSeed(seedGenerator),
                    persistedRunSetup.PendingSeedText ?? string.Empty,
                    (RunDifficultyPreset)Mathf.Clamp(
                        persistedRunSetup.DifficultyPreset,
                        (int)RunDifficultyPreset.Casual,
                        (int)RunDifficultyPreset.Brutal),
                    persistedRunSetup.Version >= 3
                        ? (RunScoringMode)Mathf.Clamp(
                            persistedRunSetup.ScoringModeValue,
                            (int)RunScoringMode.Classic,
                            (int)RunScoringMode.HighScore)
                        : RunScoringMode.Classic,
                    Mathf.Clamp(persistedRunSetup.BallsPerServe, 1, 4),
                    Mathf.Clamp(persistedRunSetup.PaddleWidthStep, -2, 2),
                    Mathf.Clamp(persistedRunSetup.BallSpeedStep, -2, 2),
                    Mathf.Clamp(persistedRunSetup.BrickDurabilityStep, -2, 2),
                    (DropPoolMode)Mathf.Clamp(
                        persistedRunSetup.DropPoolModeValue,
                        (int)DropPoolMode.Mixed,
                        (int)DropPoolMode.Disabled),
                    persistedRunSetup.Version >= 2 && persistedRunSetup.IsCapsulePartyEnabled,
                    persistedRunSetup.Version >= 5
                        ? NormalizePersistedLevelGlitchSelection(
                            persistedRunSetup.Version,
                            persistedRunSetup.LevelGlitchSelection)
                        : persistedRunSetup.Version >= 4 && persistedRunSetup.AreLevelGlitchesEnabled
                            ? LevelGlitchSelection.WarpGates
                            : LevelGlitchSelection.Off,
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
                ScoringModeValue = (int)runSetupState.ScoringMode,
                BallsPerServe = runSetupState.BallsPerServe,
                PaddleWidthStep = runSetupState.PaddleWidthStep,
                BallSpeedStep = runSetupState.BallSpeedStep,
                BrickDurabilityStep = runSetupState.BrickDurabilityStep,
                DropPoolModeValue = (int)runSetupState.DropPoolMode,
                IsCapsulePartyEnabled = runSetupState.IsCapsulePartyEnabled,
                AreLevelGlitchesEnabled = runSetupState.AreLevelGlitchesEnabled,
                LevelGlitchSelection = (int)runSetupState.SelectedLevelGlitch,
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

        private static LevelGlitchSelection NormalizePersistedLevelGlitchSelection(int version, int persistedSelection)
        {
            if (version < 6 && persistedSelection == 3)
            {
                return LevelGlitchSelection.Random;
            }

            if (version < 7 && persistedSelection == 4)
            {
                return LevelGlitchSelection.Random;
            }

            if (version < 8 && persistedSelection == 5)
            {
                return LevelGlitchSelection.Random;
            }

            if (version < 9 && persistedSelection == 6)
            {
                return LevelGlitchSelection.Random;
            }

            if (version < 10 && persistedSelection == 7)
            {
                return LevelGlitchSelection.Random;
            }

            return (LevelGlitchSelection)Mathf.Clamp(
                persistedSelection,
                (int)LevelGlitchSelection.Off,
                (int)LevelGlitchSelection.CabinetTilt);
        }
    }
}
