using System;
using System.Text;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum BreakoutContentRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
    }

    public static class BreakoutRarityRules
    {
        public static BreakoutContentRarity Clamp(BreakoutContentRarity rarity)
        {
            return (BreakoutContentRarity)Mathf.Clamp((int)rarity, (int)BreakoutContentRarity.Common, (int)BreakoutContentRarity.Epic);
        }

        public static int GetLadderUnlockIntensity(BreakoutContentRarity rarity)
        {
            return Clamp(rarity) switch
            {
                BreakoutContentRarity.Uncommon => 8,
                BreakoutContentRarity.Rare => 18,
                BreakoutContentRarity.Epic => 32,
                _ => BreakoutRunProgression.MinRogueIntensity,
            };
        }

        public static bool IsUnlockedForLadderIntensity(BreakoutContentRarity rarity, int intensity)
        {
            return BreakoutRunProgression.ClampRogueIntensity(intensity) >= GetLadderUnlockIntensity(rarity);
        }

        public static float GetDropWeightMultiplier(BreakoutContentRarity rarity)
        {
            return Clamp(rarity) switch
            {
                BreakoutContentRarity.Uncommon => 0.62f,
                BreakoutContentRarity.Rare => 0.32f,
                BreakoutContentRarity.Epic => 0.16f,
                _ => 1f,
            };
        }

        public static float GetDraftWeightMultiplier(BreakoutContentRarity rarity)
        {
            return Clamp(rarity) switch
            {
                BreakoutContentRarity.Uncommon => 0.72f,
                BreakoutContentRarity.Rare => 0.45f,
                BreakoutContentRarity.Epic => 0.28f,
                _ => 1f,
            };
        }

        public static string GetLabel(BreakoutContentRarity rarity)
        {
            return Clamp(rarity) switch
            {
                BreakoutContentRarity.Uncommon => "Uncommon",
                BreakoutContentRarity.Rare => "Rare",
                BreakoutContentRarity.Epic => "Epic",
                _ => "Common",
            };
        }
    }

    public enum PowerUpEffectType
    {
        PaddleWidthMultiplier = 0,
        BallSpeedMultiplier = 1,
        MultiBallBurst = 2,
        WavyPaddle = 3,
        StickyPaddle = 4,
        LaserPaddle = 5,
        ShieldWall = 6,
        PhaseBall = 7,
        ChainLightning = 8,
        ReverseControls = 9,
        SplitPaddle = 10,
        GravityWell = 11,
        FogOfWar = 12,
        LagSpike = 13,
        ActiveDropMultiplier = 14,
        BrickMagnet = 15,
        ScoreMultiplier = 16,
        PaddleClone = 17,
        BrickJammer = 18,
        HotPotatoBall = 19,
        ExplosiveBall = 20,
        BallSizeMultiplier = 21,
        RandomHarmfulDrop = 22,
        VectorSight = 23,
        CapsuleMagnet = 24,
        MirrorImagePaddle = 25,
        BankBonus = 26,
        PrismPop = 27,
        CleanCatch = 28,
    }

    [CreateAssetMenu(menuName = "Get Bricked/Power-Up Definition", fileName = "PowerUpDefinition")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Power-Up";
        [SerializeField] private string hudLabel = "POWER";
        [SerializeField] private string powerUpId = string.Empty;
        [SerializeField] private string iconResourcePath = string.Empty;
        [SerializeField] private PowerUpEffectType effectType = PowerUpEffectType.PaddleWidthMultiplier;
        [SerializeField] private bool beneficial = true;
        [SerializeField] private BreakoutContentRarity rarity = BreakoutContentRarity.Common;
        [SerializeField, Min(0)] private int ladderUnlockIntensityOverride;
        [SerializeField, Min(0f)] private float durationSeconds = 10f;
        [SerializeField, Min(0.1f)] private float scalar = 1.25f;
        [SerializeField, Min(0)] private int extraBallCount = 2;
        [SerializeField] private Color pickupColor = Color.white;
        [SerializeField] private ThemeVisualSlot themeSlot = ThemeVisualSlot.Auto;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public string HudLabel => string.IsNullOrWhiteSpace(hudLabel) ? DisplayName : hudLabel;

        public string PowerUpId => string.IsNullOrWhiteSpace(powerUpId) ? string.Empty : powerUpId.Trim();

        public PowerUpEffectType EffectType => effectType;

        public bool IsBeneficial => beneficial;

        public BreakoutContentRarity Rarity => BreakoutRarityRules.Clamp(rarity);

        public string RarityLabel => BreakoutRarityRules.GetLabel(Rarity);

        public int LadderUnlockIntensity => ladderUnlockIntensityOverride > 0
            ? BreakoutRunProgression.ClampRogueIntensity(ladderUnlockIntensityOverride)
            : BreakoutRarityRules.GetLadderUnlockIntensity(Rarity);

        public float DurationSeconds => Mathf.Max(0f, durationSeconds);

        public float Scalar => Mathf.Max(0.1f, scalar);

        public float VisibilityScalar => Mathf.Clamp01(scalar);

        public int ExtraBallCount => Mathf.Max(0, extraBallCount);

        public Color PickupColor => pickupColor;

        public bool IsTimed => effectType != PowerUpEffectType.MultiBallBurst
            && effectType != PowerUpEffectType.ActiveDropMultiplier
            && effectType != PowerUpEffectType.RandomHarmfulDrop
            && DurationSeconds > 0f;

        public bool IsUnlockedForLadderIntensity(int intensity)
        {
            return BreakoutRunProgression.ClampRogueIntensity(intensity) >= LadderUnlockIntensity;
        }

        public string ResolvePickupSpriteResourcePath()
        {
            if (!string.IsNullOrWhiteSpace(iconResourcePath))
            {
                return NormalizeResourcePath(iconResourcePath);
            }

            if (string.IsNullOrWhiteSpace(powerUpId))
            {
                return string.Empty;
            }

            var normalizedId = NormalizeIconName(powerUpId);
            return string.IsNullOrWhiteSpace(normalizedId) ? string.Empty : $"Sprites/{normalizedId}";
        }

        public ThemeVisualSlot ResolveThemeSlot()
        {
            if (themeSlot != ThemeVisualSlot.Auto)
            {
                return themeSlot;
            }

            if (effectType == PowerUpEffectType.MultiBallBurst
                || effectType == PowerUpEffectType.ActiveDropMultiplier
                || effectType == PowerUpEffectType.ScoreMultiplier
                || effectType == PowerUpEffectType.HotPotatoBall
                || effectType == PowerUpEffectType.ExplosiveBall
                || effectType == PowerUpEffectType.BallSizeMultiplier
                || effectType == PowerUpEffectType.BankBonus
                || effectType == PowerUpEffectType.PrismPop)
            {
                return ThemeVisualSlot.PickupBurst;
            }

            return beneficial ? ThemeVisualSlot.PickupBeneficial : ThemeVisualSlot.PickupHarmful;
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
