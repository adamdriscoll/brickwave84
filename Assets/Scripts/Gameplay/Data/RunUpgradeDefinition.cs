using System;
using System.Text;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    [CreateAssetMenu(menuName = "Get Bricked/Run Upgrade Definition", fileName = "RunUpgradeDefinition")]
    public sealed class RunUpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId = "run-upgrade";
        [SerializeField] private string displayName = "Run Upgrade";
        [SerializeField] private string hudLabel = "MOD";
        [SerializeField] private string iconResourcePath = string.Empty;
        [SerializeField, TextArea(2, 4)] private string description = "Permanent upgrade for the rest of the run.";
        [SerializeField, Min(0.05f)] private float draftWeight = 1f;
        [SerializeField, Min(1)] private int maxStacks = 3;
        [SerializeField] private string[] excludedUpgradeIds = Array.Empty<string>();
        [SerializeField] private ThemeVisualSlot themeSlot = ThemeVisualSlot.PickupBeneficial;
        [SerializeField] private Color accentColor = Color.white;
        [SerializeField, Min(0.1f)] private float paddleWidthMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float ballSpeedMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float dropChanceMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float pickupFallSpeedMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float wavyPaddleStrength;
        [SerializeField, Range(0f, 1f)] private float brickMagnetStrength;
        [SerializeField, Min(0.1f)] private float specialBrickEffectMultiplier = 1f;
        [SerializeField, Min(0)] private int extraBallsPerServe;
        [SerializeField, Min(0)] private int bonusLives;
        [SerializeField, Min(0)] private int tiltWarningSavesPerLevel;
        [SerializeField, Min(0)] private int spareFuseSavesPerRun;
        [SerializeField, Min(0)] private int freeMissileShotsPerLevel;

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

        public float PickupFallSpeedMultiplier => Mathf.Max(0.1f, pickupFallSpeedMultiplier);

        public float WavyPaddleStrength => Mathf.Clamp01(wavyPaddleStrength);

        public float BrickMagnetStrength => Mathf.Clamp01(brickMagnetStrength);

        public float SpecialBrickEffectMultiplier => Mathf.Max(0.1f, specialBrickEffectMultiplier);

        public int ExtraBallsPerServe => Mathf.Max(0, extraBallsPerServe);

        public int BonusLives => Mathf.Max(0, bonusLives);

        public int TiltWarningSavesPerLevel => Mathf.Max(0, tiltWarningSavesPerLevel);

        public int SpareFuseSavesPerRun => Mathf.Max(0, spareFuseSavesPerRun);

        public int FreeMissileShotsPerLevel => Mathf.Max(0, freeMissileShotsPerLevel);

        public string ResolveIconSpriteResourcePath()
        {
            if (!string.IsNullOrWhiteSpace(iconResourcePath))
            {
                return NormalizeResourcePath(iconResourcePath);
            }

            var normalizedId = NormalizeIconName(UpgradeId);
            return string.IsNullOrWhiteSpace(normalizedId) ? string.Empty : $"Sprites/{normalizedId}";
        }

        private static string NormalizeResourcePath(string resourcePath)
        {
            var normalizedPath = resourcePath.Trim().Replace('\\', '/');

            if (normalizedPath.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring("Assets/Resources/".Length);
            }

            var extensionIndex = normalizedPath.LastIndexOf('.');

            if (extensionIndex > normalizedPath.LastIndexOf('/'))
            {
                normalizedPath = normalizedPath.Substring(0, extensionIndex);
            }

            return normalizedPath;
        }

        private static string NormalizeIconName(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(rawValue.Length);
            var needsSeparator = false;

            for (var index = 0; index < rawValue.Length; index++)
            {
                var current = rawValue[index];

                if (char.IsLetterOrDigit(current))
                {
                    if (needsSeparator && builder.Length > 0 && builder[builder.Length - 1] != '-')
                    {
                        builder.Append('-');
                    }

                    builder.Append(char.ToLowerInvariant(current));
                    needsSeparator = false;
                    continue;
                }

                needsSeparator = builder.Length > 0;
            }

            return builder.ToString().Trim('-');
        }
    }
}
