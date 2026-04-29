using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum LevelCompletionRule
    {
        ClearRequiredBricks = 0,
        ReachTargetScore = 1,
    }

    public enum BrickMovementDirection
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        UpLeft = 4,
        UpRight = 5,
        DownLeft = 6,
        DownRight = 7,
    }

    public enum BrickMovementModifier
    {
        None = 0,
        AlternateByRow = 1,
        AlternateByColumn = 2,
        Checkerboard = 3,
        OutwardFromCenter = 4,
        InwardToCenter = 5,
        ClockwiseAroundCenter = 6,
        CounterClockwiseAroundCenter = 7,
    }

    [Serializable]
    public struct LevelBrickLegendEntry
    {
        [SerializeField] private string symbol;
        [SerializeField] private BrickDefinition brickDefinition;

        public char Symbol => string.IsNullOrEmpty(symbol) ? '\0' : symbol[0];

        public BrickDefinition BrickDefinition => brickDefinition;
    }

    [Serializable]
    public struct LevelBrickMovementEntry
    {
        [SerializeField] private string symbol;
        [SerializeField] private BrickMovementDirection direction;
        [SerializeField] private BrickMovementModifier modifier;
        [SerializeField, Min(0f)] private float speed;

        public char Symbol => string.IsNullOrEmpty(symbol) ? '\0' : symbol[0];

        public BrickMovementDirection Direction => direction;

        public BrickMovementModifier Modifier => modifier;

        public float Speed => Mathf.Max(0f, speed);

        public bool HasMotion => Symbol != '\0' && Symbol != '.' && !char.IsWhiteSpace(Symbol) && Speed > 0.01f;
    }

    [CreateAssetMenu(menuName = "Get Bricked/Level Definition", fileName = "LevelDefinition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField, Min(0)] private int sequenceIndex;
        [SerializeField] private string displayName = "Level";
        [SerializeField, Min(0.5f)] private float ballSpeedMultiplier = 1f;
        [SerializeField, Min(0.5f)] private float paddleSpeedMultiplier = 1f;
        [SerializeField, Min(0.5f)] private float topInset = 1.5f;
        [SerializeField] private LevelCompletionRule completionRule = LevelCompletionRule.ClearRequiredBricks;
        [SerializeField, Min(0)] private int targetScore;
        [SerializeField] private string[] layoutRows = Array.Empty<string>();
        [SerializeField] private LevelBrickLegendEntry[] legend = Array.Empty<LevelBrickLegendEntry>();
        [SerializeField] private LevelBrickMovementEntry[] brickMovement = Array.Empty<LevelBrickMovementEntry>();

        public int SequenceIndex => Mathf.Max(0, sequenceIndex);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public float BallSpeedMultiplier => Mathf.Max(0.5f, ballSpeedMultiplier);

        public float PaddleSpeedMultiplier => Mathf.Max(0.5f, paddleSpeedMultiplier);

        public float TopInset => Mathf.Max(0.5f, topInset);

        public LevelCompletionRule CompletionRule => completionRule;

        public int TargetScore => Mathf.Max(0, targetScore);

        public string[] LayoutRows => layoutRows ?? Array.Empty<string>();

        public LevelBrickLegendEntry[] Legend => legend ?? Array.Empty<LevelBrickLegendEntry>();

        public LevelBrickMovementEntry[] BrickMovement => brickMovement ?? Array.Empty<LevelBrickMovementEntry>();
    }
}
