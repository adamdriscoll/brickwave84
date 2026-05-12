using System;
using System.Globalization;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [Serializable]
    internal sealed class BreakoutSoloMarathonRecord
    {
        public int Version = BreakoutSoloMarathonScoreStore.CurrentVersion;
        public int Score;
        public int StageReached;
        public int Seed;
        public int DifficultyValue = (int)BreakoutHotSeatDifficulty.Gnarly;
        public string DifficultyLabel = "Gnarly";
    }

    internal static class BreakoutSoloMarathonScoreStore
    {
        public const int CurrentVersion = 1;
        private const string BestScoreKeyPrefix = "GetBricked.SoloMarathon.BestScore.";

        public static bool TrySaveBest(
            BreakoutHotSeatDifficulty difficulty,
            string difficultyLabel,
            int score,
            int stageReached,
            int seed,
            out BreakoutSoloMarathonRecord record)
        {
            var existing = Load(difficulty);

            if (existing != null && existing.Score >= score)
            {
                record = existing;
                return false;
            }

            record = new BreakoutSoloMarathonRecord
            {
                Score = score,
                StageReached = Mathf.Max(1, stageReached),
                Seed = Mathf.Max(0, seed),
                DifficultyValue = (int)difficulty,
                DifficultyLabel = string.IsNullOrWhiteSpace(difficultyLabel) ? difficulty.ToString() : difficultyLabel.Trim(),
            };

            PlayerPrefs.SetString(BuildKey(difficulty), JsonUtility.ToJson(record));
            PlayerPrefs.Save();
            return true;
        }

        public static BreakoutSoloMarathonRecord Load(BreakoutHotSeatDifficulty difficulty)
        {
            var key = BuildKey(difficulty);

            if (!PlayerPrefs.HasKey(key))
            {
                return null;
            }

            var json = PlayerPrefs.GetString(key, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var record = JsonUtility.FromJson<BreakoutSoloMarathonRecord>(json);

                if (record == null || record.Version != CurrentVersion)
                {
                    return null;
                }

                record.StageReached = Mathf.Max(1, record.StageReached);
                record.Seed = Mathf.Max(0, record.Seed);
                record.DifficultyValue = Mathf.Clamp(
                    record.DifficultyValue,
                    (int)BreakoutHotSeatDifficulty.Chill,
                    (int)BreakoutHotSeatDifficulty.Bogus);
                record.DifficultyLabel = string.IsNullOrWhiteSpace(record.DifficultyLabel)
                    ? ((BreakoutHotSeatDifficulty)record.DifficultyValue).ToString()
                    : record.DifficultyLabel.Trim();
                return record;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load Neon Marathon best score. {exception.Message}");
                return null;
            }
        }

        public static BreakoutSoloMarathonRecord LoadBestOverall()
        {
            BreakoutSoloMarathonRecord best = null;

            for (var value = (int)BreakoutHotSeatDifficulty.Chill; value <= (int)BreakoutHotSeatDifficulty.Bogus; value++)
            {
                var record = Load((BreakoutHotSeatDifficulty)value);

                if (record != null && (best == null || record.Score > best.Score))
                {
                    best = record;
                }
            }

            return best;
        }

        public static string BuildSummary(BreakoutSoloMarathonRecord record)
        {
            return record == null
                ? "Best Score: no Neon Marathon record yet."
                : $"Best Score: {FormatScore(record.Score)} | Heat {record.DifficultyLabel} | Stage {record.StageReached:00} | Tape ID {record.Seed}";
        }

        internal static string BuildKey(BreakoutHotSeatDifficulty difficulty)
        {
            var clampedDifficulty = (BreakoutHotSeatDifficulty)Mathf.Clamp(
                (int)difficulty,
                (int)BreakoutHotSeatDifficulty.Chill,
                (int)BreakoutHotSeatDifficulty.Bogus);
            return BestScoreKeyPrefix + ((int)clampedDifficulty).ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatScore(int score)
        {
            if (score < 0)
            {
                var absoluteValue = -(long)score;
                return $"-{absoluteValue:0000}";
            }

            return score.ToString("0000", CultureInfo.InvariantCulture);
        }
    }
}
