using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum RunDifficultyPreset
    {
        Casual = 0,
        Standard = 1,
        Brutal = 2,
    }

    public enum DropPoolMode
    {
        Mixed = 0,
        HelpfulOnly = 1,
        HarmfulOnly = 2,
        Disabled = 3,
    }

    public enum RunScoringMode
    {
        Classic = 0,
        HighScore = 1,
    }

    public enum RunGameMode
    {
        CustomGame = 0,
        Rogue = 1,
        TurnBased = 2,
        SoloMarathon = 3,
    }

    public enum LevelGlitchSelection
    {
        Off = 0,
        WarpGates = 1,
        TurboRail = 2,
        MirrorGrid = 3,
        GravityPocket = 4,
        TokenStorm = 5,
        StaticWall = 6,
        RowRewrite = 7,
        Random = 8,
        PrismLanes = 9,
        SwitchbackRails = 10,
    }

    public sealed class RunSettings
    {
        public RunSettings(
            int seed,
            RunDifficultyPreset difficultyPreset,
            RunScoringMode scoringMode,
            int startingLives,
            int lifeLossScorePenalty,
            int ballsPerServe,
            float paddleWidthMultiplier,
            float ballSpeedMultiplier,
            float brickDurabilityMultiplier,
            float dropChanceMultiplier,
            DropPoolMode dropPoolMode,
            bool forcePickupDropsOnBreak,
            ThemeDefinition themeDefinition,
            RunGameMode gameMode = RunGameMode.CustomGame,
            int rogueIntensity = 1,
            string selectedPaddleLabel = null,
            float paddleSpeedMultiplier = 1f,
            string difficultyLabel = null,
            bool levelGlitchesEnabled = false,
            float levelGlitchChanceMultiplier = 1f,
            LevelGlitchSelection levelGlitchSelection = LevelGlitchSelection.Random,
            bool forceLevelGlitchRoll = false,
            bool ignoreLevelGlitchUnlocks = false,
            int levelGlitchUnlockIntensityOverride = -1)
        {
            Seed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);
            DifficultyPreset = difficultyPreset;
            CustomDifficultyLabel = string.IsNullOrWhiteSpace(difficultyLabel)
                ? string.Empty
                : difficultyLabel.Trim();
            ScoringMode = scoringMode;
            GameMode = gameMode;
            RogueIntensity = Mathf.Clamp(rogueIntensity, 1, 50);
            SelectedPaddleLabel = string.IsNullOrWhiteSpace(selectedPaddleLabel)
                ? "Classic Paddle"
                : selectedPaddleLabel.Trim();
            StartingLives = Mathf.Max(1, startingLives);
            LifeLossScorePenalty = Mathf.Max(0, lifeLossScorePenalty);
            BallsPerServe = Mathf.Clamp(ballsPerServe, 1, 4);
            PaddleWidthMultiplier = Mathf.Clamp(paddleWidthMultiplier, 0.6f, 1.8f);
            PaddleSpeedMultiplier = Mathf.Clamp(paddleSpeedMultiplier, 0.5f, 1.6f);
            BallSpeedMultiplier = Mathf.Clamp(ballSpeedMultiplier, 0.6f, 1.75f);
            BrickDurabilityMultiplier = Mathf.Clamp(brickDurabilityMultiplier, 0.75f, 2.5f);
            DropChanceMultiplier = Mathf.Clamp(dropChanceMultiplier, 0f, 2f);
            DropPoolMode = dropPoolMode;
            ForcePickupDropsOnBreak = forcePickupDropsOnBreak;
            ThemeDefinition = themeDefinition;
            SelectedLevelGlitch = NormalizeLevelGlitchSelection(levelGlitchesEnabled, levelGlitchSelection);
            LevelGlitchesEnabled = SelectedLevelGlitch != LevelGlitchSelection.Off;
            LevelGlitchChanceMultiplier = Mathf.Clamp(levelGlitchChanceMultiplier, 0f, 3f);
            ForceLevelGlitchRoll = forceLevelGlitchRoll && LevelGlitchesEnabled;
            IgnoreLevelGlitchUnlocks = ignoreLevelGlitchUnlocks;
            LevelGlitchUnlockIntensity = levelGlitchUnlockIntensityOverride >= 0
                ? Mathf.Clamp(levelGlitchUnlockIntensityOverride, 0, BreakoutRunProgression.MaxRogueIntensity)
                : BreakoutRunProgression.GetCompletedUnlockIntensityForRun(RogueIntensity);
        }

        public int Seed { get; }

        public RunDifficultyPreset DifficultyPreset { get; }

        private string CustomDifficultyLabel { get; }

        public RunScoringMode ScoringMode { get; }

        public RunGameMode GameMode { get; }

        public int RogueIntensity { get; }

        public string SelectedPaddleLabel { get; }

        public int StartingLives { get; }

        public int LifeLossScorePenalty { get; }

        public int BallsPerServe { get; }

        public float PaddleWidthMultiplier { get; }

        public float PaddleSpeedMultiplier { get; }

        public float BallSpeedMultiplier { get; }

        public float BrickDurabilityMultiplier { get; }

        public float DropChanceMultiplier { get; }

        public DropPoolMode DropPoolMode { get; }

        public bool ForcePickupDropsOnBreak { get; }

        public ThemeDefinition ThemeDefinition { get; }

        public bool LevelGlitchesEnabled { get; }

        public LevelGlitchSelection SelectedLevelGlitch { get; }

        public float LevelGlitchChanceMultiplier { get; }

        public bool ForceLevelGlitchRoll { get; }

        public bool IgnoreLevelGlitchUnlocks { get; }

        public int LevelGlitchUnlockIntensity { get; }

        public string DifficultyLabel => !string.IsNullOrWhiteSpace(CustomDifficultyLabel)
            ? CustomDifficultyLabel
            : DifficultyPreset switch
            {
                RunDifficultyPreset.Casual => "Chill",
                RunDifficultyPreset.Standard => "Rad",
                RunDifficultyPreset.Brutal => "Bogus",
                _ => DifficultyPreset.ToString(),
            };

        public string ScoringModeLabel => ScoringMode switch
        {
            RunScoringMode.HighScore => "High Score",
            _ => "Classic",
        };

        public string GameModeLabel => GameMode switch
        {
            RunGameMode.Rogue => "Neon Ladder",
            RunGameMode.TurnBased => "Hot Seat",
            RunGameMode.SoloMarathon => "Neon Marathon",
            _ => "Custom Game",
        };

        public bool IsRogueMode => GameMode == RunGameMode.Rogue;

        public bool IsTurnBasedMode => GameMode == RunGameMode.TurnBased;

        public bool IsSoloMarathonMode => GameMode == RunGameMode.SoloMarathon;

        public bool UsesLifeLossScorePenalty => ScoringMode == RunScoringMode.HighScore && LifeLossScorePenalty > 0;

        public string DropPoolLabel => DropPoolMode switch
        {
            DropPoolMode.HelpfulOnly => "Helpful Only",
            DropPoolMode.HarmfulOnly => "Harmful Only",
            DropPoolMode.Disabled => "Disabled",
            _ => "Mixed",
        };

        public string DropCadenceLabel => ForcePickupDropsOnBreak ? "Capsule Party" : "Standard";

        public string ThemeLabel => ThemeDefinition != null ? ThemeDefinition.DisplayName : "Fallback";

        public string LevelGlitchLabel => SelectedLevelGlitch switch
        {
            LevelGlitchSelection.Random => "Random Glitches",
            LevelGlitchSelection.WarpGates => "Warp Gates Armed",
            LevelGlitchSelection.TurboRail => "Turbo Rail Armed",
            LevelGlitchSelection.MirrorGrid => "Mirror Grid Armed",
            LevelGlitchSelection.GravityPocket => "Gravity Pocket Armed",
            LevelGlitchSelection.TokenStorm => "Token Storm Armed",
            LevelGlitchSelection.StaticWall => "Static Wall Armed",
            LevelGlitchSelection.RowRewrite => "Row Rewrite Armed",
            LevelGlitchSelection.PrismLanes => "Prism Lanes Armed",
            LevelGlitchSelection.SwitchbackRails => "Switchback Rails Armed",
            _ => "Clean Walls",
        };

        private static LevelGlitchSelection NormalizeLevelGlitchSelection(
            bool levelGlitchesEnabled,
            LevelGlitchSelection levelGlitchSelection)
        {
            var normalized = (LevelGlitchSelection)Mathf.Clamp(
                (int)levelGlitchSelection,
                (int)LevelGlitchSelection.Off,
                (int)LevelGlitchSelection.SwitchbackRails);

            if (!levelGlitchesEnabled && normalized == LevelGlitchSelection.Random)
            {
                return LevelGlitchSelection.Off;
            }

            return normalized == LevelGlitchSelection.Off && levelGlitchesEnabled
                ? LevelGlitchSelection.Random
                : normalized;
        }
    }
}
