using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutTurnSwitchReason
    {
        None,
        BallLost,
        LevelCleared,
        GameOver,
    }

    internal enum BreakoutHotSeatMode
    {
        TopScore,
        Outlast,
    }

    internal enum BreakoutHotSeatDifficulty
    {
        Chill,
        Rad,
        Gnarly,
        Mondo,
        Bogus,
    }

    internal readonly struct BreakoutTurnAdvanceResult
    {
        public BreakoutTurnAdvanceResult(bool isRunComplete, BreakoutTurnPlayer winner)
        {
            IsRunComplete = isRunComplete;
            Winner = winner;
        }

        public bool IsRunComplete { get; }

        public BreakoutTurnPlayer Winner { get; }
    }

    internal readonly struct BreakoutTurnLeaderboardEntry
    {
        public BreakoutTurnLeaderboardEntry(
            int rank,
            bool isCurrentPlayer,
            string playerName,
            int score,
            int stage,
            int turns,
            int ballsLost,
            string status)
        {
            Rank = rank;
            IsCurrentPlayer = isCurrentPlayer;
            PlayerName = playerName ?? string.Empty;
            Score = score;
            Stage = Mathf.Max(1, stage);
            Turns = Mathf.Max(0, turns);
            BallsLost = Mathf.Max(0, ballsLost);
            Status = status ?? string.Empty;
        }

        public int Rank { get; }

        public bool IsCurrentPlayer { get; }

        public string PlayerName { get; }

        public int Score { get; }

        public int Stage { get; }

        public int Turns { get; }

        public int BallsLost { get; }

        public string Status { get; }
    }

    internal sealed class BreakoutTurnPlayer
    {
        private readonly Dictionary<int, BreakoutBrickState[]> levelBrickStates = new Dictionary<int, BreakoutBrickState[]>();

        public BreakoutTurnPlayer(int playerNumber, string displayName, int startingLives)
        {
            PlayerNumber = Mathf.Max(1, playerNumber);
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? $"Player {PlayerNumber}"
                : displayName.Trim();
            LivesRemaining = Mathf.Max(1, startingLives);
        }

        public int PlayerNumber { get; }

        public string DisplayName { get; }

        public int Score { get; private set; }

        public int TurnsTaken { get; private set; }

        public int BallsLost { get; private set; }

        public int LevelsCleared { get; private set; }

        public int CurrentLevelIndex { get; private set; }

        public int LivesRemaining { get; private set; }

        public bool IsEliminated { get; private set; }

        public void AddTurnScore(int scoreDelta, BreakoutTurnSwitchReason reason, bool countTurn, bool loseLife)
        {
            Score += scoreDelta;

            if (countTurn)
            {
                TurnsTaken++;
            }

            if (reason == BreakoutTurnSwitchReason.BallLost || reason == BreakoutTurnSwitchReason.GameOver)
            {
                BallsLost++;
            }

            if (loseLife)
            {
                LivesRemaining = Mathf.Max(0, LivesRemaining - 1);
                IsEliminated = LivesRemaining <= 0;
            }

            if (reason == BreakoutTurnSwitchReason.LevelCleared)
            {
                LevelsCleared++;
                CurrentLevelIndex++;
            }
        }

        public bool HasReachedTurnLimit(int turnLimit)
        {
            return TurnsTaken >= Mathf.Max(1, turnLimit);
        }

        public void SaveBrickState(int levelIndex, BreakoutBrickState[] brickStates)
        {
            if (levelIndex < 0)
            {
                return;
            }

            levelBrickStates[levelIndex] = brickStates ?? Array.Empty<BreakoutBrickState>();
        }

        public bool TryGetBrickState(int levelIndex, out BreakoutBrickState[] brickStates)
        {
            if (levelIndex < 0)
            {
                brickStates = Array.Empty<BreakoutBrickState>();
                return false;
            }

            return levelBrickStates.TryGetValue(levelIndex, out brickStates);
        }
    }

    internal sealed class BreakoutTurnBasedMultiplayerController
    {
        public const int MinimumPlayerCount = 2;
        public const int MaximumPlayerCount = 10;
        public const int DefaultTopScoreTurnLimit = 5;
        public const int MinimumTopScoreTurnLimit = 1;
        public const int MaximumTopScoreTurnLimit = 20;
        public const int DefaultOutlastLives = 3;
        public const int MinimumOutlastLives = 1;
        public const int MaximumOutlastLives = 9;

        private static readonly string[] NamePrefixes =
        {
            "Neon",
            "Chrome",
            "Turbo",
            "Vector",
            "Laser",
            "Prism",
            "Rad",
            "Fresh",
            "Mondo",
            "Static",
            "Arcade",
            "VHS",
        };

        private static readonly string[] NameNouns =
        {
            "Ace",
            "Rider",
            "Cruiser",
            "Spark",
            "Bandit",
            "Pilot",
            "Breaker",
            "Wizard",
            "Comet",
            "Drifter",
            "Captain",
            "Phantom",
        };

        private readonly List<BreakoutTurnPlayer> players = new List<BreakoutTurnPlayer>();

        private int turnStartScore;
        private bool hasActiveTurn;
        private BreakoutHotSeatMode activeMode = BreakoutHotSeatMode.TopScore;
        private int activeTurnLimit = DefaultTopScoreTurnLimit;
        private int activeStartingLives = DefaultOutlastLives;

        public int SelectedPlayerCount { get; private set; } = 2;

        public BreakoutHotSeatMode SelectedMode { get; private set; } = BreakoutHotSeatMode.TopScore;

        public BreakoutHotSeatDifficulty SelectedDifficulty { get; private set; } = BreakoutHotSeatDifficulty.Gnarly;

        public int SelectedTopScoreTurnLimit { get; private set; } = DefaultTopScoreTurnLimit;

        public int SelectedOutlastLives { get; private set; } = DefaultOutlastLives;

        public int CurrentPlayerIndex { get; private set; }

        public BreakoutTurnPlayer CurrentPlayer => players.Count == 0
            ? null
            : players[Mathf.Clamp(CurrentPlayerIndex, 0, players.Count - 1)];

        public BreakoutTurnSwitchReason LastSwitchReason { get; private set; }

        public string LastCompletedPlayerName { get; private set; } = string.Empty;

        public IReadOnlyList<BreakoutTurnPlayer> Players => players;

        public bool IsActive => players.Count > 0;

        public bool IsRunComplete { get; private set; }

        public BreakoutTurnPlayer Winner { get; private set; }

        public void ResetSetup()
        {
            SelectedPlayerCount = MinimumPlayerCount;
            SelectedMode = BreakoutHotSeatMode.TopScore;
            SelectedDifficulty = BreakoutHotSeatDifficulty.Gnarly;
            SelectedTopScoreTurnLimit = DefaultTopScoreTurnLimit;
            SelectedOutlastLives = DefaultOutlastLives;
            ClearRun();
        }

        public void AdjustSelectedPlayerCount(int direction)
        {
            SelectedPlayerCount = Mathf.Clamp(SelectedPlayerCount + direction, MinimumPlayerCount, MaximumPlayerCount);
        }

        public void AdjustSelectedMode(int direction)
        {
            SelectedMode = (BreakoutHotSeatMode)Mathf.Clamp(
                (int)SelectedMode + direction,
                (int)BreakoutHotSeatMode.TopScore,
                (int)BreakoutHotSeatMode.Outlast);
        }

        public void AdjustSelectedDifficulty(int direction)
        {
            SelectedDifficulty = (BreakoutHotSeatDifficulty)Mathf.Clamp(
                (int)SelectedDifficulty + direction,
                (int)BreakoutHotSeatDifficulty.Chill,
                (int)BreakoutHotSeatDifficulty.Bogus);
        }

        public void AdjustSelectedTopScoreTurnLimit(int direction)
        {
            SelectedTopScoreTurnLimit = Mathf.Clamp(
                SelectedTopScoreTurnLimit + direction,
                MinimumTopScoreTurnLimit,
                MaximumTopScoreTurnLimit);
        }

        public void AdjustSelectedOutlastLives(int direction)
        {
            SelectedOutlastLives = Mathf.Clamp(
                SelectedOutlastLives + direction,
                MinimumOutlastLives,
                MaximumOutlastLives);
        }

        public void StartRun(int seed, int currentScore)
        {
            players.Clear();
            activeMode = SelectedMode;
            activeTurnLimit = SelectedTopScoreTurnLimit;
            activeStartingLives = SelectedOutlastLives;

            var nameSeed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < SelectedPlayerCount; index++)
            {
                var name = GenerateName(nameSeed, index, usedNames);
                players.Add(new BreakoutTurnPlayer(index + 1, name, activeStartingLives));
            }

            CurrentPlayerIndex = 0;
            LastSwitchReason = BreakoutTurnSwitchReason.None;
            LastCompletedPlayerName = string.Empty;
            IsRunComplete = false;
            Winner = null;
            BeginTurn(currentScore);
        }

        public void ClearRun()
        {
            players.Clear();
            CurrentPlayerIndex = 0;
            turnStartScore = 0;
            hasActiveTurn = false;
            LastSwitchReason = BreakoutTurnSwitchReason.None;
            LastCompletedPlayerName = string.Empty;
            IsRunComplete = false;
            Winner = null;
        }

        public BreakoutTurnAdvanceResult CompleteTurnAndAdvance(int currentScore, BreakoutTurnSwitchReason reason)
        {
            if (!IsActive || CurrentPlayer == null)
            {
                return new BreakoutTurnAdvanceResult(IsRunComplete, Winner);
            }

            CompleteCurrentTurn(currentScore, reason);

            if (IsHotSeatComplete())
            {
                IsRunComplete = true;
                Winner = ResolveWinner();
                return new BreakoutTurnAdvanceResult(true, Winner);
            }

            CurrentPlayerIndex = FindNextEligiblePlayerIndex(CurrentPlayerIndex);
            BeginTurn(CurrentPlayer?.Score ?? 0);
            return new BreakoutTurnAdvanceResult(false, CurrentPlayer);
        }

        public string BuildCurrentPlayerHudLabel()
        {
            var currentPlayer = CurrentPlayer;
            return currentPlayer == null
                ? string.Empty
                : $"Player {currentPlayer.PlayerNumber:00}: {currentPlayer.DisplayName} | Stage {currentPlayer.CurrentLevelIndex + 1:00}";
        }

        public string BuildSetupPreviewLine()
        {
            var modeLimit = SelectedMode == BreakoutHotSeatMode.TopScore
                ? $"{SelectedTopScoreTurnLimit:00} turns each"
                : $"{SelectedOutlastLives:00} lives each";
            return $"{SelectedPlayerCount:00} players | {GetModeLabel(SelectedMode)} | {modeLimit} | {GetDifficultyLabel(SelectedDifficulty)}";
        }

        public string[] BuildLeaderboardLines(int maxLines = 10)
        {
            var entries = BuildLeaderboardEntries(maxLines);

            if (entries.Length == 0)
            {
                return Array.Empty<string>();
            }

            var lines = new string[entries.Length];

            for (var index = 0; index < entries.Length; index++)
            {
                var entry = entries[index];
                var activeMarker = entry.IsCurrentPlayer ? ">" : " ";
                lines[index] = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1:00}. {2}  {3:0000}  ST{4:00} T{5:00} B{6:00} {7}",
                    activeMarker,
                    entry.Rank,
                    entry.PlayerName,
                    entry.Score,
                    entry.Stage,
                    entry.Turns,
                    entry.BallsLost,
                    entry.Status);
            }

            return lines;
        }

        public BreakoutTurnLeaderboardEntry[] BuildLeaderboardEntries(int maxLines = 10)
        {
            if (players.Count == 0)
            {
                return Array.Empty<BreakoutTurnLeaderboardEntry>();
            }

            var sortedPlayers = new List<BreakoutTurnPlayer>(players);
            sortedPlayers.Sort(ComparePlayersByRank);
            var lineCount = Mathf.Clamp(maxLines, 1, sortedPlayers.Count);
            var entries = new BreakoutTurnLeaderboardEntry[lineCount];

            for (var index = 0; index < lineCount; index++)
            {
                var player = sortedPlayers[index];
                entries[index] = new BreakoutTurnLeaderboardEntry(
                    index + 1,
                    player == CurrentPlayer,
                    player.DisplayName,
                    player.Score,
                    player.CurrentLevelIndex + 1,
                    player.TurnsTaken,
                    player.BallsLost,
                    BuildPlayerStatusLabel(player));
            }

            return entries;
        }

        public string BuildSwitchTitle()
        {
            var currentPlayer = CurrentPlayer;

            if (currentPlayer == null)
            {
                return "Up Next";
            }

            return $"Up Next: {currentPlayer.DisplayName}";
        }

        public string BuildSwitchSummaryLine()
        {
            var reason = LastSwitchReason switch
            {
                BreakoutTurnSwitchReason.BallLost => "Ball lost",
                BreakoutTurnSwitchReason.LevelCleared => "Stage clear",
                BreakoutTurnSwitchReason.GameOver => "Game over",
                _ => "Ready",
            };

            return string.IsNullOrWhiteSpace(LastCompletedPlayerName)
                ? $"{reason}. {BuildCurrentPlayerHudLabel()} is on deck."
                : $"{LastCompletedPlayerName}: {reason}. {BuildCurrentPlayerHudLabel()} is on deck.";
        }

        public int GetCurrentPlayerScore()
        {
            return CurrentPlayer?.Score ?? 0;
        }

        public int GetCurrentPlayerLevelIndex()
        {
            return CurrentPlayer?.CurrentLevelIndex ?? 0;
        }

        public int GetCurrentPlayerLivesRemaining()
        {
            return CurrentPlayer?.LivesRemaining ?? activeStartingLives;
        }

        public int GetCurrentPlayerTurnsRemaining()
        {
            return Mathf.Max(0, activeTurnLimit - (CurrentPlayer?.TurnsTaken ?? 0));
        }

        public string GetSelectedModeLabel()
        {
            return GetModeLabel(SelectedMode);
        }

        public string GetSelectedDifficultyLabel()
        {
            return GetDifficultyLabel(SelectedDifficulty);
        }

        public bool IsTopScoreMode => activeMode == BreakoutHotSeatMode.TopScore;

        public bool IsOutlastMode => activeMode == BreakoutHotSeatMode.Outlast;

        public void SaveCurrentPlayerBrickState(int levelIndex, BreakoutBrickState[] brickStates)
        {
            CurrentPlayer?.SaveBrickState(levelIndex, brickStates);
        }

        public bool TryGetCurrentPlayerBrickState(int levelIndex, out BreakoutBrickState[] brickStates)
        {
            if (CurrentPlayer != null)
            {
                return CurrentPlayer.TryGetBrickState(levelIndex, out brickStates);
            }

            brickStates = Array.Empty<BreakoutBrickState>();
            return false;
        }

        private void BeginTurn(int currentScore)
        {
            turnStartScore = currentScore;
            hasActiveTurn = true;
        }

        private void CompleteCurrentTurn(int currentScore, BreakoutTurnSwitchReason reason)
        {
            if (!hasActiveTurn || CurrentPlayer == null)
            {
                return;
            }

            var countTurn = reason == BreakoutTurnSwitchReason.BallLost
                || reason == BreakoutTurnSwitchReason.LevelCleared
                || reason == BreakoutTurnSwitchReason.GameOver;
            var loseLife = activeMode == BreakoutHotSeatMode.Outlast
                && (reason == BreakoutTurnSwitchReason.BallLost || reason == BreakoutTurnSwitchReason.GameOver);
            CurrentPlayer.AddTurnScore(currentScore - turnStartScore, reason, countTurn, loseLife);
            LastCompletedPlayerName = CurrentPlayer.DisplayName;
            LastSwitchReason = reason;
            hasActiveTurn = false;
        }

        private bool IsHotSeatComplete()
        {
            if (players.Count == 0)
            {
                return true;
            }

            if (activeMode == BreakoutHotSeatMode.Outlast)
            {
                return CountOutlastPlayersAlive() <= 1;
            }

            for (var index = 0; index < players.Count; index++)
            {
                if (!players[index].HasReachedTurnLimit(activeTurnLimit))
                {
                    return false;
                }
            }

            return true;
        }

        private int CountOutlastPlayersAlive()
        {
            var count = 0;

            for (var index = 0; index < players.Count; index++)
            {
                if (!players[index].IsEliminated)
                {
                    count++;
                }
            }

            return count;
        }

        private int FindNextEligiblePlayerIndex(int startIndex)
        {
            if (players.Count == 0)
            {
                return 0;
            }

            for (var offset = 1; offset <= players.Count; offset++)
            {
                var index = (startIndex + offset) % players.Count;

                if (IsPlayerEligible(players[index]))
                {
                    return index;
                }
            }

            return Mathf.Clamp(startIndex, 0, players.Count - 1);
        }

        private bool IsPlayerEligible(BreakoutTurnPlayer player)
        {
            if (player == null)
            {
                return false;
            }

            return activeMode == BreakoutHotSeatMode.Outlast
                ? !player.IsEliminated
                : !player.HasReachedTurnLimit(activeTurnLimit);
        }

        private BreakoutTurnPlayer ResolveWinner()
        {
            if (players.Count == 0)
            {
                return null;
            }

            var sortedPlayers = new List<BreakoutTurnPlayer>(players);
            sortedPlayers.Sort(ComparePlayersByRank);
            return sortedPlayers[0];
        }

        private static string GenerateName(int seed, int index, HashSet<string> usedNames)
        {
            var prefixIndex = Mathf.Abs(seed + (index * 7)) % NamePrefixes.Length;
            var nounIndex = Mathf.Abs((seed / 13) + (index * 5)) % NameNouns.Length;
            var candidate = $"{NamePrefixes[prefixIndex]} {NameNouns[nounIndex]}";

            if (usedNames.Add(candidate))
            {
                return candidate;
            }

            for (var offset = 1; offset <= NamePrefixes.Length * NameNouns.Length; offset++)
            {
                prefixIndex = (prefixIndex + 1) % NamePrefixes.Length;
                nounIndex = (nounIndex + 3) % NameNouns.Length;
                candidate = $"{NamePrefixes[prefixIndex]} {NameNouns[nounIndex]}";

                if (usedNames.Add(candidate))
                {
                    return candidate;
                }
            }

            candidate = $"Player {index + 1:00}";
            usedNames.Add(candidate);
            return candidate;
        }

        private int ComparePlayersByRank(BreakoutTurnPlayer left, BreakoutTurnPlayer right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            if (activeMode == BreakoutHotSeatMode.Outlast)
            {
                var aliveComparison = left.IsEliminated.CompareTo(right.IsEliminated);

                if (aliveComparison != 0)
                {
                    return aliveComparison;
                }

                var outlastLevelComparison = right.LevelsCleared.CompareTo(left.LevelsCleared);

                if (outlastLevelComparison != 0)
                {
                    return outlastLevelComparison;
                }
            }

            var scoreComparison = right.Score.CompareTo(left.Score);

            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            var levelComparison = right.LevelsCleared.CompareTo(left.LevelsCleared);

            if (levelComparison != 0)
            {
                return levelComparison;
            }

            return left.PlayerNumber.CompareTo(right.PlayerNumber);
        }

        private string BuildPlayerStatusLabel(BreakoutTurnPlayer player)
        {
            if (player == null)
            {
                return string.Empty;
            }

            if (activeMode == BreakoutHotSeatMode.Outlast)
            {
                return player.IsEliminated ? "OUT" : $"L{player.LivesRemaining:00}";
            }

            return $"R{Mathf.Max(0, activeTurnLimit - player.TurnsTaken):00}";
        }

        private static string GetModeLabel(BreakoutHotSeatMode mode)
        {
            return mode switch
            {
                BreakoutHotSeatMode.Outlast => "Outlast",
                _ => "Top Score",
            };
        }

        private static string GetDifficultyLabel(BreakoutHotSeatDifficulty difficulty)
        {
            return difficulty switch
            {
                BreakoutHotSeatDifficulty.Chill => "Chill",
                BreakoutHotSeatDifficulty.Rad => "Rad",
                BreakoutHotSeatDifficulty.Mondo => "Mondo",
                BreakoutHotSeatDifficulty.Bogus => "Bogus",
                _ => "Gnarly",
            };
        }
    }
}
