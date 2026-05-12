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
        public int CurrentIntensity = BreakoutRunProgression.MinRogueIntensity;
        public int Seed;
        public int Score;
    }

    [Serializable]
    internal sealed class BreakoutRogueIntensityStageRecord
    {
        public int Intensity = BreakoutRunProgression.MinRogueIntensity;
        public int BestStageReached;
        public int BestScore;
    }

    [Serializable]
    internal sealed class BreakoutRoguePaddleIntensityProgress
    {
        public string SelectedPaddle = BreakoutRogueRunResultStore.DefaultPaddleLabel;
        public int HighestCompletedIntensity;
        public BreakoutRogueIntensityStageRecord[] StageRecords = Array.Empty<BreakoutRogueIntensityStageRecord>();
    }

    [Serializable]
    internal sealed class BreakoutRogueIntensityProgress
    {
        public int Version = BreakoutRogueIntensityProgressStore.CurrentVersion;
        public BreakoutRoguePaddleIntensityProgress[] PaddleProgress = Array.Empty<BreakoutRoguePaddleIntensityProgress>();
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
                CurrentIntensity = settings != null ? BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity) : BreakoutRunProgression.MinRogueIntensity,
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
            BreakoutRogueIntensityProgressStore.RecordResult(result);
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
                loaded.CurrentIntensity = BreakoutRunProgression.ClampRogueIntensity(loaded.CurrentIntensity);
                result = loaded;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load last Neon Ladder result. {exception.Message}");
                return false;
            }
        }

        public static string BuildSummary(BreakoutRogueRunResult result)
        {
            if (result == null)
            {
                return "Last Run: no Neon Ladder tape recorded yet.";
            }

            var outcome = result.Completed ? "Cleared" : "Wiped Out";
            return $"{outcome} | Stage {result.StageReached:00}/{BreakoutRunProgression.TargetLevelCount:00} | Heat {result.CurrentIntensity:00} | {result.SelectedPaddle} | Tape ID {result.Seed} | Score {FormatScore(result.Score)}";
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

    internal static class BreakoutRogueIntensityProgressStore
    {
        public const int CurrentVersion = 1;
        internal const string ProgressKey = "GetBricked.Rogue.IntensityProgress";

        public static int GetAvailableIntensity(string selectedPaddle)
        {
            return BreakoutRunProgression.ClampRogueIntensity(GetHighestCompletedIntensity(selectedPaddle) + 1);
        }

        public static int GetHighestCompletedIntensity(string selectedPaddle)
        {
            var progress = LoadOrCreateProgress();
            var paddleProgress = FindPaddleProgress(progress, selectedPaddle);
            return paddleProgress != null
                ? Mathf.Clamp(paddleProgress.HighestCompletedIntensity, 0, BreakoutRunProgression.MaxRogueIntensity)
                : 0;
        }

        public static int GetBestStageReached(string selectedPaddle, int intensity)
        {
            var progress = LoadOrCreateProgress();
            var paddleProgress = FindPaddleProgress(progress, selectedPaddle);

            if (paddleProgress == null || paddleProgress.StageRecords == null)
            {
                return 0;
            }

            var clampedIntensity = BreakoutRunProgression.ClampRogueIntensity(intensity);

            for (var index = 0; index < paddleProgress.StageRecords.Length; index++)
            {
                var record = paddleProgress.StageRecords[index];

                if (record != null && record.Intensity == clampedIntensity)
                {
                    return Mathf.Clamp(record.BestStageReached, 0, BreakoutRunProgression.TargetLevelCount);
                }
            }

            return 0;
        }

        public static void RecordResult(BreakoutRogueRunResult result)
        {
            if (result == null)
            {
                return;
            }

            var progress = LoadOrCreateProgress();
            var paddleProgress = FindOrAddPaddleProgress(progress, result.SelectedPaddle);
            var intensity = BreakoutRunProgression.ClampRogueIntensity(result.CurrentIntensity);
            var stageReached = result.Completed
                ? BreakoutRunProgression.TargetLevelCount
                : Mathf.Clamp(result.StageReached, 1, BreakoutRunProgression.TargetLevelCount);
            var record = FindOrAddStageRecord(paddleProgress, intensity);

            if (result.Completed)
            {
                paddleProgress.HighestCompletedIntensity = Mathf.Max(paddleProgress.HighestCompletedIntensity, intensity);
            }

            if (stageReached > record.BestStageReached || (stageReached == record.BestStageReached && result.Score > record.BestScore))
            {
                record.BestStageReached = stageReached;
                record.BestScore = result.Score;
            }

            progress.Version = CurrentVersion;
            PlayerPrefs.SetString(ProgressKey, JsonUtility.ToJson(progress));
        }

        private static BreakoutRogueIntensityProgress LoadOrCreateProgress()
        {
            if (!PlayerPrefs.HasKey(ProgressKey))
            {
                return new BreakoutRogueIntensityProgress();
            }

            var json = PlayerPrefs.GetString(ProgressKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new BreakoutRogueIntensityProgress();
            }

            try
            {
                var progress = JsonUtility.FromJson<BreakoutRogueIntensityProgress>(json);

                if (progress == null || progress.Version != CurrentVersion)
                {
                    return new BreakoutRogueIntensityProgress();
                }

                progress.PaddleProgress ??= Array.Empty<BreakoutRoguePaddleIntensityProgress>();
                return progress;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load Neon Ladder intensity progress. {exception.Message}");
                return new BreakoutRogueIntensityProgress();
            }
        }

        private static BreakoutRoguePaddleIntensityProgress FindPaddleProgress(BreakoutRogueIntensityProgress progress, string selectedPaddle)
        {
            if (progress == null || progress.PaddleProgress == null)
            {
                return null;
            }

            var normalizedPaddle = NormalizePaddleLabel(selectedPaddle);

            for (var index = 0; index < progress.PaddleProgress.Length; index++)
            {
                var paddleProgress = progress.PaddleProgress[index];

                if (paddleProgress != null && string.Equals(NormalizePaddleLabel(paddleProgress.SelectedPaddle), normalizedPaddle, StringComparison.OrdinalIgnoreCase))
                {
                    paddleProgress.SelectedPaddle = normalizedPaddle;
                    paddleProgress.StageRecords ??= Array.Empty<BreakoutRogueIntensityStageRecord>();
                    return paddleProgress;
                }
            }

            return null;
        }

        private static BreakoutRoguePaddleIntensityProgress FindOrAddPaddleProgress(BreakoutRogueIntensityProgress progress, string selectedPaddle)
        {
            var existing = FindPaddleProgress(progress, selectedPaddle);

            if (existing != null)
            {
                return existing;
            }

            var paddleProgress = new BreakoutRoguePaddleIntensityProgress
            {
                SelectedPaddle = NormalizePaddleLabel(selectedPaddle),
                StageRecords = Array.Empty<BreakoutRogueIntensityStageRecord>(),
            };
            var current = progress.PaddleProgress ?? Array.Empty<BreakoutRoguePaddleIntensityProgress>();
            Array.Resize(ref current, current.Length + 1);
            current[current.Length - 1] = paddleProgress;
            progress.PaddleProgress = current;
            return paddleProgress;
        }

        private static BreakoutRogueIntensityStageRecord FindOrAddStageRecord(BreakoutRoguePaddleIntensityProgress paddleProgress, int intensity)
        {
            paddleProgress.StageRecords ??= Array.Empty<BreakoutRogueIntensityStageRecord>();

            for (var index = 0; index < paddleProgress.StageRecords.Length; index++)
            {
                var record = paddleProgress.StageRecords[index];

                if (record != null && record.Intensity == intensity)
                {
                    return record;
                }
            }

            var records = paddleProgress.StageRecords;
            var newRecord = new BreakoutRogueIntensityStageRecord
            {
                Intensity = intensity,
            };
            Array.Resize(ref records, records.Length + 1);
            records[records.Length - 1] = newRecord;
            paddleProgress.StageRecords = records;
            return newRecord;
        }

        private static string NormalizePaddleLabel(string selectedPaddle)
        {
            return string.IsNullOrWhiteSpace(selectedPaddle)
                ? BreakoutRogueRunResultStore.DefaultPaddleLabel
                : selectedPaddle.Trim();
        }
    }
}
