using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    public sealed class BreakoutRunStatsSnapshot
    {
        public string GameModeLabel = string.Empty;
        public bool RunCompleted;
        public int RunsRecorded;
        public int BallsLaunched;
        public int LivesLost;
        public int LevelsCleared;
        public int WallsHit;
        public int PaddleHits;
        public float PaddleDistanceTraveled;
        public float BallsDistanceTraveled;
        public int BricksHit;
        public int BricksDestroyed;
        public int TotalDropsPickedUp;
        public int TotalDropsDropped;
        public int HelpfulDropsPickedUp;
        public int HelpfulDropsDropped;
        public int HarmfulDropsPickedUp;
        public int HarmfulDropsDropped;
        public float TimePlayedSeconds;
        public int GlitchesEncountered;

        public BreakoutRunStatsSnapshot Clone()
        {
            return (BreakoutRunStatsSnapshot)MemberwiseClone();
        }

        public void Add(BreakoutRunStatsSnapshot runStats)
        {
            if (runStats == null)
            {
                return;
            }

            BallsLaunched += Mathf.Max(0, runStats.BallsLaunched);
            LivesLost += Mathf.Max(0, runStats.LivesLost);
            LevelsCleared += Mathf.Max(0, runStats.LevelsCleared);
            WallsHit += Mathf.Max(0, runStats.WallsHit);
            PaddleHits += Mathf.Max(0, runStats.PaddleHits);
            PaddleDistanceTraveled += Mathf.Max(0f, runStats.PaddleDistanceTraveled);
            BallsDistanceTraveled += Mathf.Max(0f, runStats.BallsDistanceTraveled);
            BricksHit += Mathf.Max(0, runStats.BricksHit);
            BricksDestroyed += Mathf.Max(0, runStats.BricksDestroyed);
            TotalDropsPickedUp += Mathf.Max(0, runStats.TotalDropsPickedUp);
            TotalDropsDropped += Mathf.Max(0, runStats.TotalDropsDropped);
            HelpfulDropsPickedUp += Mathf.Max(0, runStats.HelpfulDropsPickedUp);
            HelpfulDropsDropped += Mathf.Max(0, runStats.HelpfulDropsDropped);
            HarmfulDropsPickedUp += Mathf.Max(0, runStats.HarmfulDropsPickedUp);
            HarmfulDropsDropped += Mathf.Max(0, runStats.HarmfulDropsDropped);
            TimePlayedSeconds += Mathf.Max(0f, runStats.TimePlayedSeconds);
            GlitchesEncountered += Mathf.Max(0, runStats.GlitchesEncountered);
        }
    }

    public sealed class BreakoutRunStatsService
    {
        private const string DefaultLifetimeKeyPrefix = "Brickwave84.Stats.Lifetime.";

        private readonly string lifetimeKeyPrefix;
        private readonly Dictionary<int, Vector2> lastBallPositions = new Dictionary<int, Vector2>();
        private readonly List<int> staleBallIds = new List<int>();
        private BreakoutRunStatsSnapshot currentRun = new BreakoutRunStatsSnapshot();
        private BreakoutRunStatsSnapshot lifetimeStats;
        private int lastPaddleInstanceId;
        private Vector2 lastPaddlePosition;
        private bool hasLastPaddlePosition;
        private bool hasActiveRun;
        private bool runFinalized;

        public BreakoutRunStatsService(string lifetimeKeyPrefix = DefaultLifetimeKeyPrefix)
        {
            this.lifetimeKeyPrefix = string.IsNullOrWhiteSpace(lifetimeKeyPrefix)
                ? DefaultLifetimeKeyPrefix
                : lifetimeKeyPrefix;
            lifetimeStats = LoadLifetimeStats();
        }

        public BreakoutRunStatsSnapshot CurrentRunStats => currentRun.Clone();

        public BreakoutRunStatsSnapshot LifetimeStats => lifetimeStats.Clone();

        public void BeginRun(RunSettings settings)
        {
            currentRun = new BreakoutRunStatsSnapshot
            {
                GameModeLabel = settings?.GameModeLabel ?? "Custom Game",
            };
            lastBallPositions.Clear();
            ResetPaddleDistanceTracking();
            hasActiveRun = settings != null;
            runFinalized = false;
        }

        public BreakoutRunStatsSnapshot FinalizeRun(bool completed, bool persistLifetime)
        {
            if (!hasActiveRun || runFinalized)
            {
                return currentRun.Clone();
            }

            currentRun.RunCompleted = completed;
            runFinalized = true;
            hasActiveRun = false;
            lastBallPositions.Clear();
            ResetPaddleDistanceTracking();

            if (persistLifetime)
            {
                lifetimeStats.RunsRecorded++;
                lifetimeStats.Add(currentRun);
                SaveLifetimeStats(lifetimeStats);
            }

            return currentRun.Clone();
        }

        public void TrackFrame(
            float deltaTime,
            PaddleController paddle,
            IReadOnlyList<BallController> activeBalls,
            bool trackTime,
            bool trackDistances)
        {
            if (!hasActiveRun || runFinalized)
            {
                return;
            }

            if (trackTime)
            {
                currentRun.TimePlayedSeconds += Mathf.Max(0f, deltaTime);
            }

            TrackPaddleDistance(paddle, trackDistances);
            TrackBallDistance(activeBalls, trackDistances);
        }

        public void ResetMovementTracking()
        {
            lastBallPositions.Clear();
            ResetPaddleDistanceTracking();
        }

        public void RegisterBallLaunched()
        {
            if (CanRecordRunEvent())
            {
                currentRun.BallsLaunched++;
            }
        }

        public void RegisterLifeLost()
        {
            if (CanRecordRunEvent())
            {
                currentRun.LivesLost++;
            }
        }

        public void RegisterLevelCleared()
        {
            if (CanRecordRunEvent())
            {
                currentRun.LevelsCleared++;
            }
        }

        public void RegisterWallHit()
        {
            if (CanRecordRunEvent())
            {
                currentRun.WallsHit++;
            }
        }

        public void RegisterPaddleHit()
        {
            if (CanRecordRunEvent())
            {
                currentRun.PaddleHits++;
            }
        }

        public void RegisterBrickHit()
        {
            if (CanRecordRunEvent())
            {
                currentRun.BricksHit++;
            }
        }

        public void RegisterBrickDestroyed()
        {
            if (!CanRecordRunEvent())
            {
                return;
            }

            currentRun.BricksHit++;
            currentRun.BricksDestroyed++;
        }

        public void RegisterDropDropped(PowerUpDefinition definition)
        {
            if (!CanRecordRunEvent() || definition == null)
            {
                return;
            }

            currentRun.TotalDropsDropped++;

            if (definition.IsBeneficial)
            {
                currentRun.HelpfulDropsDropped++;
            }
            else
            {
                currentRun.HarmfulDropsDropped++;
            }
        }

        public void RegisterDropPickedUp(PowerUpDefinition definition)
        {
            if (!CanRecordRunEvent() || definition == null)
            {
                return;
            }

            currentRun.TotalDropsPickedUp++;

            if (definition.IsBeneficial)
            {
                currentRun.HelpfulDropsPickedUp++;
            }
            else
            {
                currentRun.HarmfulDropsPickedUp++;
            }
        }

        public void RegisterGlitchEncountered()
        {
            if (CanRecordRunEvent())
            {
                currentRun.GlitchesEncountered++;
            }
        }

        public void ClearLifetimeStats()
        {
            lifetimeStats = new BreakoutRunStatsSnapshot();
            DeleteLifetimeStats();
        }

        public static string[] BuildRunStatsLines(BreakoutRunStatsSnapshot stats, bool includeRunCount)
        {
            stats ??= new BreakoutRunStatsSnapshot();

            var lines = new List<string>();

            if (includeRunCount)
            {
                lines.Add($"Runs Recorded {stats.RunsRecorded:00} | Time Played {FormatDuration(stats.TimePlayedSeconds)}");
            }
            else
            {
                lines.Add($"Time Played {FormatDuration(stats.TimePlayedSeconds)} | Glitches Encountered {stats.GlitchesEncountered:00}");
            }

            lines.Add($"Balls Launched {stats.BallsLaunched:00} | Lives Lost {stats.LivesLost:00} | Levels Cleared {stats.LevelsCleared:00}");
            lines.Add($"Walls Hit {stats.WallsHit:00} | Paddle Hits {stats.PaddleHits:00} | Bricks Hit {stats.BricksHit:00} | Bricks Destroyed {stats.BricksDestroyed:00}");
            lines.Add($"Paddle Distance {FormatDistance(stats.PaddleDistanceTraveled)} | Balls Distance {FormatDistance(stats.BallsDistanceTraveled)}");
            lines.Add($"Drops Picked {stats.TotalDropsPickedUp:00}/{stats.TotalDropsDropped:00} | Helpful {stats.HelpfulDropsPickedUp:00}/{stats.HelpfulDropsDropped:00} | Harmful {stats.HarmfulDropsPickedUp:00}/{stats.HarmfulDropsDropped:00}");

            if (includeRunCount)
            {
                lines.Add($"Glitches Encountered {stats.GlitchesEncountered:00}");
            }

            return lines.ToArray();
        }

        private bool CanRecordRunEvent()
        {
            return hasActiveRun && !runFinalized;
        }

        private void TrackPaddleDistance(PaddleController paddle, bool trackDistances)
        {
            if (paddle == null)
            {
                ResetPaddleDistanceTracking();
                return;
            }

            var instanceId = paddle.GetInstanceID();
            var position = (Vector2)paddle.transform.position;

            if (!hasLastPaddlePosition || instanceId != lastPaddleInstanceId)
            {
                lastPaddleInstanceId = instanceId;
                lastPaddlePosition = position;
                hasLastPaddlePosition = true;
                return;
            }

            if (trackDistances)
            {
                currentRun.PaddleDistanceTraveled += Vector2.Distance(lastPaddlePosition, position);
            }

            lastPaddlePosition = position;
        }

        private void TrackBallDistance(IReadOnlyList<BallController> activeBalls, bool trackDistances)
        {
            if (activeBalls == null)
            {
                lastBallPositions.Clear();
                return;
            }

            staleBallIds.Clear();
            foreach (var trackedId in lastBallPositions.Keys)
            {
                staleBallIds.Add(trackedId);
            }

            for (var index = 0; index < activeBalls.Count; index++)
            {
                var ball = activeBalls[index];

                if (ball == null)
                {
                    continue;
                }

                var instanceId = ball.GetInstanceID();
                staleBallIds.Remove(instanceId);
                var position = (Vector2)ball.transform.position;

                if (!ball.HasLaunched)
                {
                    lastBallPositions[instanceId] = position;
                    continue;
                }

                if (trackDistances && lastBallPositions.TryGetValue(instanceId, out var lastPosition))
                {
                    currentRun.BallsDistanceTraveled += Vector2.Distance(lastPosition, position);
                }

                lastBallPositions[instanceId] = position;
            }

            for (var index = 0; index < staleBallIds.Count; index++)
            {
                lastBallPositions.Remove(staleBallIds[index]);
            }
        }

        private void ResetPaddleDistanceTracking()
        {
            lastPaddleInstanceId = 0;
            lastPaddlePosition = Vector2.zero;
            hasLastPaddlePosition = false;
        }

        private BreakoutRunStatsSnapshot LoadLifetimeStats()
        {
            return new BreakoutRunStatsSnapshot
            {
                RunsRecorded = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.RunsRecorded)), 0),
                BallsLaunched = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BallsLaunched)), 0),
                LivesLost = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.LivesLost)), 0),
                LevelsCleared = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.LevelsCleared)), 0),
                WallsHit = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.WallsHit)), 0),
                PaddleHits = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.PaddleHits)), 0),
                PaddleDistanceTraveled = PlayerPrefs.GetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.PaddleDistanceTraveled)), 0f),
                BallsDistanceTraveled = PlayerPrefs.GetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.BallsDistanceTraveled)), 0f),
                BricksHit = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BricksHit)), 0),
                BricksDestroyed = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BricksDestroyed)), 0),
                TotalDropsPickedUp = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.TotalDropsPickedUp)), 0),
                TotalDropsDropped = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.TotalDropsDropped)), 0),
                HelpfulDropsPickedUp = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HelpfulDropsPickedUp)), 0),
                HelpfulDropsDropped = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HelpfulDropsDropped)), 0),
                HarmfulDropsPickedUp = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HarmfulDropsPickedUp)), 0),
                HarmfulDropsDropped = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HarmfulDropsDropped)), 0),
                TimePlayedSeconds = PlayerPrefs.GetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.TimePlayedSeconds)), 0f),
                GlitchesEncountered = PlayerPrefs.GetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.GlitchesEncountered)), 0),
            };
        }

        private void SaveLifetimeStats(BreakoutRunStatsSnapshot stats)
        {
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.RunsRecorded)), stats.RunsRecorded);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BallsLaunched)), stats.BallsLaunched);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.LivesLost)), stats.LivesLost);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.LevelsCleared)), stats.LevelsCleared);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.WallsHit)), stats.WallsHit);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.PaddleHits)), stats.PaddleHits);
            PlayerPrefs.SetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.PaddleDistanceTraveled)), stats.PaddleDistanceTraveled);
            PlayerPrefs.SetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.BallsDistanceTraveled)), stats.BallsDistanceTraveled);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BricksHit)), stats.BricksHit);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.BricksDestroyed)), stats.BricksDestroyed);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.TotalDropsPickedUp)), stats.TotalDropsPickedUp);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.TotalDropsDropped)), stats.TotalDropsDropped);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HelpfulDropsPickedUp)), stats.HelpfulDropsPickedUp);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HelpfulDropsDropped)), stats.HelpfulDropsDropped);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HarmfulDropsPickedUp)), stats.HarmfulDropsPickedUp);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.HarmfulDropsDropped)), stats.HarmfulDropsDropped);
            PlayerPrefs.SetFloat(BuildKey(nameof(BreakoutRunStatsSnapshot.TimePlayedSeconds)), stats.TimePlayedSeconds);
            PlayerPrefs.SetInt(BuildKey(nameof(BreakoutRunStatsSnapshot.GlitchesEncountered)), stats.GlitchesEncountered);
            PlayerPrefs.Save();
        }

        private void DeleteLifetimeStats()
        {
            var keys = new[]
            {
                nameof(BreakoutRunStatsSnapshot.RunsRecorded),
                nameof(BreakoutRunStatsSnapshot.BallsLaunched),
                nameof(BreakoutRunStatsSnapshot.LivesLost),
                nameof(BreakoutRunStatsSnapshot.LevelsCleared),
                nameof(BreakoutRunStatsSnapshot.WallsHit),
                nameof(BreakoutRunStatsSnapshot.PaddleHits),
                nameof(BreakoutRunStatsSnapshot.PaddleDistanceTraveled),
                nameof(BreakoutRunStatsSnapshot.BallsDistanceTraveled),
                nameof(BreakoutRunStatsSnapshot.BricksHit),
                nameof(BreakoutRunStatsSnapshot.BricksDestroyed),
                nameof(BreakoutRunStatsSnapshot.TotalDropsPickedUp),
                nameof(BreakoutRunStatsSnapshot.TotalDropsDropped),
                nameof(BreakoutRunStatsSnapshot.HelpfulDropsPickedUp),
                nameof(BreakoutRunStatsSnapshot.HelpfulDropsDropped),
                nameof(BreakoutRunStatsSnapshot.HarmfulDropsPickedUp),
                nameof(BreakoutRunStatsSnapshot.HarmfulDropsDropped),
                nameof(BreakoutRunStatsSnapshot.TimePlayedSeconds),
                nameof(BreakoutRunStatsSnapshot.GlitchesEncountered),
            };

            for (var index = 0; index < keys.Length; index++)
            {
                PlayerPrefs.DeleteKey(BuildKey(keys[index]));
            }

            PlayerPrefs.Save();
        }

        private string BuildKey(string key)
        {
            return lifetimeKeyPrefix + key;
        }

        public static string FormatDistance(float distance)
        {
            return $"{Mathf.Max(0f, distance):0.0}u";
        }

        public static string FormatDuration(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds / 60) % 60;
            var remainingSeconds = totalSeconds % 60;
            return hours > 0
                ? $"{hours:0}:{minutes:00}:{remainingSeconds:00}"
                : $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
