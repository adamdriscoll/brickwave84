using System;
using System.Collections.Generic;
using System.Globalization;
using GetBricked.Gameplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    internal enum BreakoutRunSetupField
    {
        Seed = 0,
        Difficulty = 1,
        ScoreMode = 2,
        BallsPerServe = 3,
        PaddleWidth = 4,
        BallSpeed = 5,
        BrickDurability = 6,
        DropPool = 7,
        CapsuleParty = 8,
        LevelGlitches = 9,
        Theme = 10,
        PlayerCount = 11,
        HotSeatMode = 12,
        HotSeatTurnLimit = 13,
        HotSeatLives = 14,
        HotSeatDifficulty = 15,
    }

    internal sealed class BreakoutRunSetupState
    {
        public BreakoutRunSetupState(string defaultThemeId, Func<int> seedGenerator)
        {
            Reset(defaultThemeId, seedGenerator, generateNewSeed: true);
        }

        public int Seed { get; private set; }

        public RunDifficultyPreset DifficultyPreset { get; private set; } = RunDifficultyPreset.Standard;

        public RunScoringMode ScoringMode { get; private set; } = RunScoringMode.Classic;

        public int BallsPerServe { get; private set; } = 1;

        public int PaddleWidthStep { get; private set; }

        public int BallSpeedStep { get; private set; }

        public int BrickDurabilityStep { get; private set; }

        public DropPoolMode DropPoolMode { get; private set; } = DropPoolMode.Mixed;

        public bool IsCapsulePartyEnabled { get; private set; }

        public bool AreLevelGlitchesEnabled => SelectedLevelGlitch != LevelGlitchSelection.Off;

        public LevelGlitchSelection SelectedLevelGlitch { get; private set; }

        public string ThemeId { get; private set; } = string.Empty;

        public string PendingSeedText { get; private set; } = string.Empty;

        public void Reset(string defaultThemeId, Func<int> seedGenerator, bool generateNewSeed)
        {
            DifficultyPreset = RunDifficultyPreset.Standard;
            ScoringMode = RunScoringMode.Classic;
            BallsPerServe = 1;
            PaddleWidthStep = 0;
            BallSpeedStep = 0;
            BrickDurabilityStep = 0;
            DropPoolMode = DropPoolMode.Mixed;
            IsCapsulePartyEnabled = true;
            SelectedLevelGlitch = LevelGlitchSelection.Off;
            ThemeId = defaultThemeId ?? string.Empty;
            Seed = generateNewSeed ? GenerateSeed(seedGenerator) : Seed;
            PendingSeedText = Seed.ToString(CultureInfo.InvariantCulture);
        }

        public void Restore(
            int seed,
            string pendingSeedText,
            RunDifficultyPreset difficultyPreset,
            RunScoringMode scoringMode,
            int ballsPerServe,
            int paddleWidthStep,
            int ballSpeedStep,
            int brickDurabilityStep,
            DropPoolMode dropPoolMode,
            bool isCapsulePartyEnabled,
            bool areLevelGlitchesEnabled,
            string themeId)
        {
            RestoreWithLevelGlitchSelection(
                seed,
                pendingSeedText,
                difficultyPreset,
                scoringMode,
                ballsPerServe,
                paddleWidthStep,
                ballSpeedStep,
                brickDurabilityStep,
                dropPoolMode,
                isCapsulePartyEnabled,
                areLevelGlitchesEnabled ? LevelGlitchSelection.WarpGates : LevelGlitchSelection.Off,
                themeId);
        }

        public void RestoreWithLevelGlitchSelection(
            int seed,
            string pendingSeedText,
            RunDifficultyPreset difficultyPreset,
            RunScoringMode scoringMode,
            int ballsPerServe,
            int paddleWidthStep,
            int ballSpeedStep,
            int brickDurabilityStep,
            DropPoolMode dropPoolMode,
            bool isCapsulePartyEnabled,
            LevelGlitchSelection levelGlitchSelection,
            string themeId)
        {
            Seed = Mathf.Max(0, seed);
            PendingSeedText = pendingSeedText ?? string.Empty;
            DifficultyPreset = difficultyPreset;
            ScoringMode = scoringMode;
            BallsPerServe = Mathf.Clamp(ballsPerServe, 1, 4);
            PaddleWidthStep = Mathf.Clamp(paddleWidthStep, -2, 2);
            BallSpeedStep = Mathf.Clamp(ballSpeedStep, -2, 2);
            BrickDurabilityStep = Mathf.Clamp(brickDurabilityStep, -2, 2);
            DropPoolMode = dropPoolMode;
            IsCapsulePartyEnabled = isCapsulePartyEnabled;
            SelectedLevelGlitch = ClampLevelGlitchSelection(levelGlitchSelection);
            ThemeId = themeId ?? string.Empty;
        }

        public void AdjustField(
            BreakoutRunSetupField field,
            int direction,
            Func<int> seedGenerator,
            Func<string, int, string> shiftThemeId)
        {
            switch (field)
            {
                case BreakoutRunSetupField.Seed:
                    Seed = ParsePendingSeed(commitSeedText: false, seedGenerator);
                    Seed = Mathf.Max(0, Seed + direction);
                    PendingSeedText = Seed.ToString(CultureInfo.InvariantCulture);
                    break;
                case BreakoutRunSetupField.Difficulty:
                    DifficultyPreset = (RunDifficultyPreset)Mathf.Clamp(
                        (int)DifficultyPreset + direction,
                        (int)RunDifficultyPreset.Casual,
                        (int)RunDifficultyPreset.Brutal);
                    break;
                case BreakoutRunSetupField.ScoreMode:
                    ScoringMode = (RunScoringMode)Mathf.Clamp(
                        (int)ScoringMode + direction,
                        (int)RunScoringMode.Classic,
                        (int)RunScoringMode.HighScore);
                    break;
                case BreakoutRunSetupField.BallsPerServe:
                    BallsPerServe = Mathf.Clamp(BallsPerServe + direction, 1, 4);
                    break;
                case BreakoutRunSetupField.PaddleWidth:
                    PaddleWidthStep = Mathf.Clamp(PaddleWidthStep + direction, -2, 2);
                    break;
                case BreakoutRunSetupField.BallSpeed:
                    BallSpeedStep = Mathf.Clamp(BallSpeedStep + direction, -2, 2);
                    break;
                case BreakoutRunSetupField.BrickDurability:
                    BrickDurabilityStep = Mathf.Clamp(BrickDurabilityStep + direction, -2, 2);
                    break;
                case BreakoutRunSetupField.DropPool:
                    DropPoolMode = (DropPoolMode)Mathf.Clamp(
                        (int)DropPoolMode + direction,
                        (int)Gameplay.Data.DropPoolMode.Mixed,
                        (int)Gameplay.Data.DropPoolMode.Disabled);
                    break;
                case BreakoutRunSetupField.CapsuleParty:
                    IsCapsulePartyEnabled = !IsCapsulePartyEnabled;
                    break;
                case BreakoutRunSetupField.LevelGlitches:
                    SelectedLevelGlitch = (LevelGlitchSelection)Mathf.Clamp(
                        (int)SelectedLevelGlitch + direction,
                        (int)LevelGlitchSelection.Off,
                        (int)LevelGlitchSelection.CapsuleRoulette);
                    break;
                case BreakoutRunSetupField.Theme:
                    ThemeId = shiftThemeId != null ? shiftThemeId(ThemeId, direction) : ThemeId;
                    break;
            }
        }

        public void RandomizeSeed(Func<int> seedGenerator)
        {
            Seed = GenerateSeed(seedGenerator);
            PendingSeedText = Seed.ToString(CultureInfo.InvariantCulture);
        }

        public void BackspaceSeed()
        {
            if (PendingSeedText.Length > 0)
            {
                PendingSeedText = PendingSeedText.Substring(0, PendingSeedText.Length - 1);
            }
        }

        public void ClearSeedText()
        {
            PendingSeedText = string.Empty;
        }

        public bool AppendPressedSeedDigit(Keyboard keyboard)
        {
            if (PendingSeedText.Length >= 9)
            {
                return false;
            }

            if (!TryGetPressedDigit(keyboard, out var digit))
            {
                return false;
            }

            PendingSeedText += digit;
            return true;
        }

        public RunSettings BuildRunSettings(
            int startingLives,
            int lifeLossScorePenalty,
            ThemeDefinition selectedTheme,
            Func<int> seedGenerator,
            out string validationMessage,
            bool commitSeedText = false)
        {
            var seed = ParsePendingSeed(commitSeedText, seedGenerator);
            Seed = seed;
            ThemeId = selectedTheme != null ? selectedTheme.ThemeId : string.Empty;

            var lives = Mathf.Max(1, startingLives);
            var paddleWidthMultiplier = 1f;
            var ballSpeedMultiplier = 1f;
            var brickDurabilityMultiplier = 1f;
            var dropChanceMultiplier = 1f;

            switch (DifficultyPreset)
            {
                case RunDifficultyPreset.Casual:
                    lives += 1;
                    paddleWidthMultiplier *= 1.15f;
                    ballSpeedMultiplier *= 0.92f;
                    brickDurabilityMultiplier *= 0.9f;
                    dropChanceMultiplier *= 1.15f;
                    break;
                case RunDifficultyPreset.Brutal:
                    lives = Mathf.Max(1, lives - 1);
                    paddleWidthMultiplier *= 0.9f;
                    ballSpeedMultiplier *= 1.12f;
                    brickDurabilityMultiplier *= 1.2f;
                    dropChanceMultiplier *= 0.9f;
                    break;
            }

            paddleWidthMultiplier *= 1f + (PaddleWidthStep * 0.12f);
            ballSpeedMultiplier *= 1f + (BallSpeedStep * 0.08f);
            brickDurabilityMultiplier *= 1f + (BrickDurabilityStep * 0.16f);

            paddleWidthMultiplier = Mathf.Clamp(paddleWidthMultiplier, 0.7f, 1.55f);
            ballSpeedMultiplier = Mathf.Clamp(ballSpeedMultiplier, 0.78f, 1.45f);
            brickDurabilityMultiplier = Mathf.Clamp(brickDurabilityMultiplier, 0.8f, 1.9f);

            var challengeIndex = (ballSpeedMultiplier * brickDurabilityMultiplier) / paddleWidthMultiplier;
            var warnings = new List<string>();

            if (challengeIndex > 1.75f)
            {
                var adjustedBallSpeed = Mathf.Clamp((1.75f * paddleWidthMultiplier) / brickDurabilityMultiplier, 0.78f, ballSpeedMultiplier);

                if (adjustedBallSpeed < ballSpeedMultiplier)
                {
                    ballSpeedMultiplier = adjustedBallSpeed;
                    warnings.Add("Ball speed was capped to keep the preset fair.");
                }
            }

            if (ScoringMode == RunScoringMode.HighScore)
            {
                warnings.Add($"High Score mode live: every life loss costs {lifeLossScorePenalty:0000} points.");
            }

            if (DropPoolMode == Gameplay.Data.DropPoolMode.Disabled)
            {
                warnings.Add("Drops disabled for this run.");
            }
            else if (IsCapsulePartyEnabled)
            {
                warnings.Add("Capsule Party live: every eligible brick drops a capsule.");
            }

            if (AreLevelGlitchesEnabled)
            {
                warnings.Add($"{BuildLevelGlitchWarningLabel(SelectedLevelGlitch)} armed: glitched stages pay bonus score.");
            }

            validationMessage = warnings.Count > 0
                ? string.Join(" ", warnings)
                : "Run validated. Same seed will replay the same level transforms, launch rolls, and drop rolls.";

            return new RunSettings(
                seed,
                DifficultyPreset,
                ScoringMode,
                lives,
                lifeLossScorePenalty,
                BallsPerServe,
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                brickDurabilityMultiplier,
                dropChanceMultiplier,
                DropPoolMode,
                IsCapsulePartyEnabled,
                selectedTheme,
                levelGlitchesEnabled: AreLevelGlitchesEnabled,
                levelGlitchSelection: SelectedLevelGlitch);
        }

        public int ParsePendingSeed(bool commitSeedText, Func<int> seedGenerator)
        {
            int parsedSeed;

            if (string.IsNullOrWhiteSpace(PendingSeedText))
            {
                parsedSeed = commitSeedText
                    ? GenerateSeed(seedGenerator)
                    : Seed > 0
                        ? Seed
                        : GenerateSeed(seedGenerator);
            }
            else if (!int.TryParse(PendingSeedText, NumberStyles.None, CultureInfo.InvariantCulture, out parsedSeed))
            {
                parsedSeed = Seed > 0 ? Seed : GenerateSeed(seedGenerator);
            }

            if (commitSeedText)
            {
                PendingSeedText = parsedSeed.ToString(CultureInfo.InvariantCulture);
            }

            return parsedSeed;
        }

        private static int GenerateSeed(Func<int> seedGenerator)
        {
            return seedGenerator != null ? seedGenerator() : 0;
        }

        private static LevelGlitchSelection ClampLevelGlitchSelection(LevelGlitchSelection selection)
        {
            return (LevelGlitchSelection)Mathf.Clamp(
                (int)selection,
                (int)LevelGlitchSelection.Off,
                (int)LevelGlitchSelection.CapsuleRoulette);
        }

        private static string BuildLevelGlitchWarningLabel(LevelGlitchSelection selection)
        {
            return selection switch
            {
                LevelGlitchSelection.WarpGates => "Warp Gates",
                LevelGlitchSelection.TurboRail => "Turbo Rail",
                LevelGlitchSelection.MirrorGrid => "Mirror Grid",
                LevelGlitchSelection.GravityPocket => "Gravity Pocket",
                LevelGlitchSelection.TokenStorm => "Token Storm",
                LevelGlitchSelection.StaticWall => "Static Wall",
                LevelGlitchSelection.RowRewrite => "Row Rewrite",
                LevelGlitchSelection.PrismLanes => "Prism Lanes",
                LevelGlitchSelection.SwitchbackRails => "Switchback Rails",
                LevelGlitchSelection.CapsuleRoulette => "Capsule Roulette",
                _ => "Random glitches",
            };
        }

        private static bool TryGetPressedDigit(Keyboard keyboard, out char digit)
        {
            if (keyboard != null)
            {
                if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame)
                {
                    digit = '0';
                    return true;
                }

                if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
                {
                    digit = '1';
                    return true;
                }

                if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
                {
                    digit = '2';
                    return true;
                }

                if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
                {
                    digit = '3';
                    return true;
                }

                if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
                {
                    digit = '4';
                    return true;
                }

                if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
                {
                    digit = '5';
                    return true;
                }

                if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
                {
                    digit = '6';
                    return true;
                }

                if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame)
                {
                    digit = '7';
                    return true;
                }

                if (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame)
                {
                    digit = '8';
                    return true;
                }

                if (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame)
                {
                    digit = '9';
                    return true;
                }
            }

            digit = default;
            return false;
        }
    }
}
