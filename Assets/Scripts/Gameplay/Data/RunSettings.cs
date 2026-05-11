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
            string difficultyLabel = null)
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
            RunGameMode.Rogue => "Rogue",
            RunGameMode.TurnBased => "Hot Seat",
            _ => "Custom Game",
        };

        public bool IsRogueMode => GameMode == RunGameMode.Rogue;

        public bool IsTurnBasedMode => GameMode == RunGameMode.TurnBased;

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
    }
}
