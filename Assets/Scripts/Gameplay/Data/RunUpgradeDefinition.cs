using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    [CreateAssetMenu(menuName = "Get Bricked/Run Upgrade Definition", fileName = "RunUpgradeDefinition")]
    public sealed class RunUpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId = "run-upgrade";
        [SerializeField] private string displayName = "Run Upgrade";
        [SerializeField] private string hudLabel = "MOD";
        [SerializeField, TextArea(2, 4)] private string description = "Permanent upgrade for the rest of the run.";
        [SerializeField, Min(0.05f)] private float draftWeight = 1f;
        [SerializeField, Min(1)] private int maxStacks = 3;
        [SerializeField] private string[] excludedUpgradeIds = Array.Empty<string>();
        [SerializeField] private ThemeVisualSlot themeSlot = ThemeVisualSlot.PickupBeneficial;
        [SerializeField] private Color accentColor = Color.white;
        [SerializeField, Min(0.1f)] private float paddleWidthMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float ballSpeedMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float dropChanceMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float wavyPaddleStrength;
        [SerializeField, Min(0)] private int extraBallsPerServe;
        [SerializeField, Min(0)] private int bonusLives;

        public string UpgradeId => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId.Trim();

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public string HudLabel => string.IsNullOrWhiteSpace(hudLabel) ? DisplayName : hudLabel;

        public string Description => string.IsNullOrWhiteSpace(description)
            ? "Permanent upgrade for the rest of the run."
            : description.Trim();

        public float DraftWeight => Mathf.Max(0.05f, draftWeight);

        public int MaxStacks => Mathf.Max(1, maxStacks);

        public string[] ExcludedUpgradeIds => excludedUpgradeIds ?? Array.Empty<string>();

        public ThemeVisualSlot ThemeSlot => themeSlot;

        public Color AccentColor => accentColor;

        public float PaddleWidthMultiplier => Mathf.Max(0.1f, paddleWidthMultiplier);

        public float BallSpeedMultiplier => Mathf.Max(0.1f, ballSpeedMultiplier);

        public float DropChanceMultiplier => Mathf.Max(0.1f, dropChanceMultiplier);

        public float WavyPaddleStrength => Mathf.Clamp01(wavyPaddleStrength);

        public int ExtraBallsPerServe => Mathf.Max(0, extraBallsPerServe);

        public int BonusLives => Mathf.Max(0, bonusLives);
    }
}
