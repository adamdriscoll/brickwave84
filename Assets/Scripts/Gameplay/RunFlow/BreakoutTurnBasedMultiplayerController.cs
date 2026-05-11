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

    internal sealed class BreakoutTurnPlayer
    {
        public BreakoutTurnPlayer(int playerNumber, string displayName)
        {
            PlayerNumber = Mathf.Max(1, playerNumber);
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? $"Player {PlayerNumber}"
                : displayName.Trim();
        }

        public int PlayerNumber { get; }

        public string DisplayName { get; }

        public int Score { get; private set; }

        public int TurnsTaken { get; private set; }

        public int BallsLost { get; private set; }

        public int LevelsCleared { get; private set; }

        public void AddTurnScore(int scoreDelta, BreakoutTurnSwitchReason reason)
        {
            Score += scoreDelta;
            TurnsTaken++;

            if (reason == BreakoutTurnSwitchReason.BallLost || reason == BreakoutTurnSwitchReason.GameOver)
            {
                BallsLost++;
            }

            if (reason == BreakoutTurnSwitchReason.LevelCleared)
            {
                LevelsCleared++;
            }
        }
    }

    internal sealed class BreakoutTurnBasedMultiplayerController
    {
        public const int MinimumPlayerCount = 2;
        public const int MaximumPlayerCount = 10;

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

        public int SelectedPlayerCount { get; private set; } = 2;

        public int CurrentPlayerIndex { get; private set; }

        public BreakoutTurnPlayer CurrentPlayer => players.Count == 0
            ? null
            : players[Mathf.Clamp(CurrentPlayerIndex, 0, players.Count - 1)];

        public BreakoutTurnSwitchReason LastSwitchReason { get; private set; }

        public string LastCompletedPlayerName { get; private set; } = string.Empty;

        public IReadOnlyList<BreakoutTurnPlayer> Players => players;

        public bool IsActive => players.Count > 0;

        public void ResetSetup()
        {
            SelectedPlayerCount = MinimumPlayerCount;
            ClearRun();
        }

        public void AdjustSelectedPlayerCount(int direction)
        {
            SelectedPlayerCount = Mathf.Clamp(SelectedPlayerCount + direction, MinimumPlayerCount, MaximumPlayerCount);
        }

        public void StartRun(int seed, int currentScore)
        {
            players.Clear();

            var nameSeed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < SelectedPlayerCount; index++)
            {
                var name = GenerateName(nameSeed, index, usedNames);
                players.Add(new BreakoutTurnPlayer(index + 1, name));
            }

            CurrentPlayerIndex = 0;
            LastSwitchReason = BreakoutTurnSwitchReason.None;
            LastCompletedPlayerName = string.Empty;
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
        }

        public void CompleteTurnAndAdvance(int currentScore, BreakoutTurnSwitchReason reason)
        {
            if (!IsActive || CurrentPlayer == null)
            {
                return;
            }

            CompleteCurrentTurn(currentScore, reason);

            if (reason != BreakoutTurnSwitchReason.GameOver)
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;
                BeginTurn(currentScore);
            }
        }

        public string BuildCurrentPlayerHudLabel()
        {
            var currentPlayer = CurrentPlayer;
            return currentPlayer == null
                ? string.Empty
                : $"Player {currentPlayer.PlayerNumber:00}: {currentPlayer.DisplayName}";
        }

        public string BuildSetupPreviewLine()
        {
            return $"{SelectedPlayerCount:00} players | Random cabinet names | Switch on ball losses and stage clears";
        }

        public string[] BuildLeaderboardLines(int maxLines = 10)
        {
            if (players.Count == 0)
            {
                return Array.Empty<string>();
            }

            var sortedPlayers = new List<BreakoutTurnPlayer>(players);
            sortedPlayers.Sort(ComparePlayersByRank);
            var lineCount = Mathf.Clamp(maxLines, 1, sortedPlayers.Count);
            var lines = new string[lineCount];

            for (var index = 0; index < lineCount; index++)
            {
                var player = sortedPlayers[index];
                var activeMarker = player == CurrentPlayer ? ">" : " ";
                lines[index] = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1:00}. {2}  {3:0000}  T{4:00} L{5:00} C{6:00}",
                    activeMarker,
                    index + 1,
                    player.DisplayName,
                    player.Score,
                    player.TurnsTaken,
                    player.BallsLost,
                    player.LevelsCleared);
            }

            return lines;
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

            CurrentPlayer.AddTurnScore(currentScore - turnStartScore, reason);
            LastCompletedPlayerName = CurrentPlayer.DisplayName;
            LastSwitchReason = reason;
            hasActiveTurn = false;
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

        private static int ComparePlayersByRank(BreakoutTurnPlayer left, BreakoutTurnPlayer right)
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

            var scoreComparison = right.Score.CompareTo(left.Score);

            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            return left.PlayerNumber.CompareTo(right.PlayerNumber);
        }
    }
}
