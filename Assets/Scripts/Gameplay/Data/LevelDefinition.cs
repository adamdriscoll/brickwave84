using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum LevelCompletionRule
    {
        ClearRequiredBricks = 0,
        ReachTargetScore = 1,
    }

    [Serializable]
    public struct LevelBrickLegendEntry
    {
        [SerializeField] private string symbol;
        [SerializeField] private BrickDefinition brickDefinition;

        public char Symbol => string.IsNullOrEmpty(symbol) ? '\0' : symbol[0];

        public BrickDefinition BrickDefinition => brickDefinition;
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

        public int SequenceIndex => Mathf.Max(0, sequenceIndex);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public float BallSpeedMultiplier => Mathf.Max(0.5f, ballSpeedMultiplier);

        public float PaddleSpeedMultiplier => Mathf.Max(0.5f, paddleSpeedMultiplier);

        public float TopInset => Mathf.Max(0.5f, topInset);

        public LevelCompletionRule CompletionRule => completionRule;

        public int TargetScore => Mathf.Max(0, targetScore);

        public string[] LayoutRows => layoutRows ?? Array.Empty<string>();

        public LevelBrickLegendEntry[] Legend => legend ?? Array.Empty<LevelBrickLegendEntry>();
    }
}
