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

    public sealed class RunSettings
    {
        public RunSettings(
            int seed,
            RunDifficultyPreset difficultyPreset,
            int startingLives,
            int ballsPerServe,
            float paddleWidthMultiplier,
            float ballSpeedMultiplier,
            float brickDurabilityMultiplier,
            float dropChanceMultiplier,
            DropPoolMode dropPoolMode)
        {
            Seed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);
            DifficultyPreset = difficultyPreset;
            StartingLives = Mathf.Max(1, startingLives);
            BallsPerServe = Mathf.Clamp(ballsPerServe, 1, 4);
            PaddleWidthMultiplier = Mathf.Clamp(paddleWidthMultiplier, 0.6f, 1.8f);
            BallSpeedMultiplier = Mathf.Clamp(ballSpeedMultiplier, 0.6f, 1.75f);
            BrickDurabilityMultiplier = Mathf.Clamp(brickDurabilityMultiplier, 0.75f, 2.5f);
            DropChanceMultiplier = Mathf.Clamp(dropChanceMultiplier, 0f, 2f);
            DropPoolMode = dropPoolMode;
        }

        public int Seed { get; }

        public RunDifficultyPreset DifficultyPreset { get; }

        public int StartingLives { get; }

        public int BallsPerServe { get; }

        public float PaddleWidthMultiplier { get; }

        public float BallSpeedMultiplier { get; }

        public float BrickDurabilityMultiplier { get; }

        public float DropChanceMultiplier { get; }

        public DropPoolMode DropPoolMode { get; }

        public string DifficultyLabel => DifficultyPreset.ToString();

        public string DropPoolLabel => DropPoolMode switch
        {
            DropPoolMode.HelpfulOnly => "Helpful Only",
            DropPoolMode.HarmfulOnly => "Harmful Only",
            DropPoolMode.Disabled => "Disabled",
            _ => "Mixed",
        };
    }
}
