using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum PowerUpEffectType
    {
        PaddleWidthMultiplier = 0,
        BallSpeedMultiplier = 1,
        MultiBallBurst = 2,
        WavyPaddle = 3,
    }

    [CreateAssetMenu(menuName = "Get Bricked/Power-Up Definition", fileName = "PowerUpDefinition")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Power-Up";
        [SerializeField] private string hudLabel = "POWER";
        [SerializeField] private PowerUpEffectType effectType = PowerUpEffectType.PaddleWidthMultiplier;
        [SerializeField] private bool beneficial = true;
        [SerializeField, Min(0f)] private float durationSeconds = 10f;
        [SerializeField, Min(0.1f)] private float scalar = 1.25f;
        [SerializeField, Min(0)] private int extraBallCount = 2;
        [SerializeField] private Color pickupColor = Color.white;
        [SerializeField] private ThemeVisualSlot themeSlot = ThemeVisualSlot.Auto;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public string HudLabel => string.IsNullOrWhiteSpace(hudLabel) ? DisplayName : hudLabel;

        public PowerUpEffectType EffectType => effectType;

        public bool IsBeneficial => beneficial;

        public float DurationSeconds => Mathf.Max(0f, durationSeconds);

        public float Scalar => Mathf.Max(0.1f, scalar);

        public int ExtraBallCount => Mathf.Max(0, extraBallCount);

        public Color PickupColor => pickupColor;

        public bool IsTimed => effectType != PowerUpEffectType.MultiBallBurst && DurationSeconds > 0f;

        public ThemeVisualSlot ResolveThemeSlot()
        {
            if (themeSlot != ThemeVisualSlot.Auto)
            {
                return themeSlot;
            }

            if (effectType == PowerUpEffectType.MultiBallBurst)
            {
                return ThemeVisualSlot.PickupBurst;
            }

            return beneficial ? ThemeVisualSlot.PickupBeneficial : ThemeVisualSlot.PickupHarmful;
        }
    }
}
