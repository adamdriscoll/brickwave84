using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    public enum ThemeVisualSlot
    {
        Auto = 0,
        Background = 1,
        Wall = 2,
        Paddle = 3,
        Ball = 4,
        BrickPrimary = 5,
        BrickSecondary = 6,
        BrickTertiary = 7,
        BrickObstacle = 8,
        PickupBeneficial = 9,
        PickupHarmful = 10,
        PickupBurst = 11,
    }

    [Serializable]
    public struct ThemeVisualStyle
    {
        [SerializeField] private Color primaryColor;
        [SerializeField] private Color secondaryColor;
        [SerializeField] private Sprite sprite;

        public ThemeVisualStyle(Color primary, Color secondary, Sprite visualSprite)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            sprite = visualSprite;
        }

        public Color PrimaryColor => primaryColor;

        public Color SecondaryColor => secondaryColor;

        public Sprite Sprite => sprite;
    }

    [Serializable]
    public struct ThemeVisualEntry
    {
        [SerializeField] private ThemeVisualSlot slot;
        [SerializeField] private ThemeVisualStyle style;

        public ThemeVisualSlot Slot => slot;

        public ThemeVisualStyle Style => style;
    }

    [CreateAssetMenu(menuName = "Get Bricked/Theme Definition", fileName = "ThemeDefinition")]
    public sealed class ThemeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Theme";
        [SerializeField] private ThemeVisualEntry[] visualEntries = Array.Empty<ThemeVisualEntry>();

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public string ThemeId => name;

        public ThemeVisualStyle ResolveStyle(ThemeVisualSlot slot, ThemeVisualStyle fallbackStyle)
        {
            if (visualEntries == null)
            {
                return fallbackStyle;
            }

            for (var index = 0; index < visualEntries.Length; index++)
            {
                var entry = visualEntries[index];

                if (entry.Slot != slot)
                {
                    continue;
                }

                var resolvedStyle = entry.Style;
                return new ThemeVisualStyle(
                    resolvedStyle.PrimaryColor,
                    resolvedStyle.SecondaryColor,
                    resolvedStyle.Sprite != null ? resolvedStyle.Sprite : fallbackStyle.Sprite);
            }

            return fallbackStyle;
        }
    }
}
