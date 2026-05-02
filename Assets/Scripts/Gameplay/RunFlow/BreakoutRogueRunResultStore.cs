using System;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [Serializable]
    internal sealed class BreakoutRogueRunResult
    {
        public int Version = BreakoutRogueRunResultStore.CurrentVersion;
        public bool Completed;
        public int StageReached;
        public string SelectedPaddle = BreakoutRogueRunResultStore.DefaultPaddleLabel;
        public int Seed;
        public int Score;
    }

    internal static class BreakoutRogueRunResultStore
    {
        public const int CurrentVersion = 1;
        public const string DefaultPaddleLabel = "Classic Paddle";
        private const string LastResultKey = "GetBricked.Rogue.LastResult";

        public static BreakoutRogueRunResult BuildResult(RunSettings settings, bool completed, int stageReached, string selectedPaddle, int score)
        {
            var resolvedStage = completed
                ? BreakoutRunProgression.TargetLevelCount
                : Mathf.Clamp(stageReached, 1, BreakoutRunProgression.TargetLevelCount);

            return new BreakoutRogueRunResult
            {
                Completed = completed,
                StageReached = resolvedStage,
                SelectedPaddle = string.IsNullOrWhiteSpace(selectedPaddle) ? DefaultPaddleLabel : selectedPaddle.Trim(),
                Seed = settings != null ? settings.Seed : 0,
                Score = score,
            };
        }

        public static void Save(BreakoutRogueRunResult result)
        {
            if (result == null)
            {
                return;
            }

            result.Version = CurrentVersion;
            PlayerPrefs.SetString(LastResultKey, JsonUtility.ToJson(result));
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out BreakoutRogueRunResult result)
        {
            result = null;

            if (!PlayerPrefs.HasKey(LastResultKey))
            {
                return false;
            }

            var json = PlayerPrefs.GetString(LastResultKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var loaded = JsonUtility.FromJson<BreakoutRogueRunResult>(json);

                if (loaded == null || loaded.Version != CurrentVersion)
                {
                    return false;
                }

                loaded.StageReached = Mathf.Clamp(loaded.StageReached, 1, BreakoutRunProgression.TargetLevelCount);
                loaded.SelectedPaddle = string.IsNullOrWhiteSpace(loaded.SelectedPaddle)
                    ? DefaultPaddleLabel
                    : loaded.SelectedPaddle.Trim();
                result = loaded;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load last Rogue result. {exception.Message}");
                return false;
            }
        }

        public static string BuildSummary(BreakoutRogueRunResult result)
        {
            if (result == null)
            {
                return "Last Run: no Rogue tape recorded yet.";
            }

            var outcome = result.Completed ? "Cleared" : "Wiped Out";
            return $"{outcome} | Stage {result.StageReached:00}/{BreakoutRunProgression.TargetLevelCount:00} | {result.SelectedPaddle} | Tape ID {result.Seed} | Score {FormatScore(result.Score)}";
        }

        private static string FormatScore(int score)
        {
            if (score < 0)
            {
                var absoluteValue = -(long)score;
                return $"-{absoluteValue:0000}";
            }

            return score.ToString("0000", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
