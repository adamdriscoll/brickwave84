using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutProgressionPlaceholderItem
    {
        public BreakoutProgressionPlaceholderItem(
            string title,
            string kind,
            string family,
            string description,
            int unlockIntensity,
            Color accent)
        {
            Title = title ?? string.Empty;
            Kind = kind ?? string.Empty;
            Family = family ?? string.Empty;
            Description = description ?? string.Empty;
            UnlockIntensity = BreakoutRunProgression.ClampRogueIntensity(unlockIntensity);
            Accent = accent;
        }

        public string Title { get; }

        public string Kind { get; }

        public string Family { get; }

        public string Description { get; }

        public int UnlockIntensity { get; }

        public Color Accent { get; }
    }

    internal sealed class BreakoutProgressionPageService
    {
        private const int PlannedDropUnlockCount = 50;
        private const int PlannedGlitchUnlockCount = 50;
        private const int DefaultGlitchCount = 2;

        private static readonly Color ControlAccent = new Color(0.45f, 0.95f, 0.72f, 1f);
        private static readonly Color PrecisionAccent = new Color(1f, 0.87f, 0.36f, 1f);
        private static readonly Color DamageAccent = new Color(1f, 0.49f, 0.86f, 1f);
        private static readonly Color SplitAccent = new Color(0.01f, 0.93f, 0.98f, 1f);
        private static readonly Color HazardAccent = new Color(0.99f, 0.27f, 0.31f, 1f);
        private static readonly Color LayoutAccent = new Color(0.72f, 0.62f, 1f, 1f);

        private readonly Dictionary<string, Sprite> powerUpSpriteCache = new Dictionary<string, Sprite>();

        private static readonly BreakoutProgressionPlaceholderItem[] PlaceholderDrops =
        {
            new BreakoutProgressionPlaceholderItem("Chrome Rail", "Drop", "Control", "Paddle widens slightly and sends cleaner bank angles.", 2, ControlAccent),
            new BreakoutProgressionPlaceholderItem("Clean Catch", "Drop", "Control", "Next paddle hit catches, then releases with stronger aim.", 4, ControlAccent),
            new BreakoutProgressionPlaceholderItem("Vector Sight", "Drop", "Control", "Shows a short aim preview near the paddle.", 6, ControlAccent),
            new BreakoutProgressionPlaceholderItem("Bank Bonus", "Drop", "Precision", "Wall bounces charge bonus points until the next brick hit.", 8, PrecisionAccent),
            new BreakoutProgressionPlaceholderItem("Solar Shot", "Drop", "Damage", "Ball burns through the next weak brick it touches.", 12, DamageAccent),
            new BreakoutProgressionPlaceholderItem("Prism Pop", "Drop", "Split", "First brick hit splits a short-lived copy ball.", 14, SplitAccent),
            new BreakoutProgressionPlaceholderItem("Capsule Magnet", "Drop", "Pickup", "Nearby helpful capsules drift toward the paddle.", 18, ControlAccent),
            new BreakoutProgressionPlaceholderItem("Bogus Tape", "Drop", "Hazard", "Looks helpful until collected, then rolls a minor hazard.", 24, HazardAccent),
        };

        private static readonly BreakoutProgressionPlaceholderItem[] PlaceholderGlitches =
        {
            new BreakoutProgressionPlaceholderItem("Mirror Grid", "Glitch", "Layout", "Brick layout mirrors horizontally halfway through the stage.", 8, LayoutAccent),
            new BreakoutProgressionPlaceholderItem("Row Rewrite", "Glitch", "Layout", "One row rerolls into a new brick pattern after a timer.", 10, LayoutAccent),
            new BreakoutProgressionPlaceholderItem("Prism Lanes", "Glitch", "Precision", "Marked lanes refract the ball into sharper angles.", 16, PrecisionAccent),
            new BreakoutProgressionPlaceholderItem("Token Storm", "Glitch", "Pickup", "More capsules spawn, but fall at mixed speeds.", 20, ControlAccent),
            new BreakoutProgressionPlaceholderItem("Gravity Pocket", "Glitch", "Speed", "A visible pocket bends nearby ball paths.", 26, SplitAccent),
            new BreakoutProgressionPlaceholderItem("Static Wall", "Glitch", "Paddle", "One side wall flickers between normal and weak bounce.", 34, HazardAccent),
        };

        public BreakoutUiProgressionView BuildView(IReadOnlyList<PowerUpDefinition> loadedPowerUps, BreakoutThemeService themeService = null)
        {
            var progressPaddleLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;
            var selectedHighest = BreakoutRogueIntensityProgressStore.GetHighestCompletedIntensity(progressPaddleLabel);
            var selectedAvailable = BreakoutRogueIntensityProgressStore.GetAvailableIntensity(progressPaddleLabel);
            var selectedBestStage = BreakoutRogueIntensityProgressStore.GetBestStageReached(progressPaddleLabel, selectedAvailable);
            var defaultDropCount = CountDefaultDrops(loadedPowerUps);
            var unlockedPlaceholderDrops = CountEarnedPlaceholders(PlaceholderDrops, selectedHighest);
            var unlockedPlaceholderGlitches = CountEarnedPlaceholders(PlaceholderGlitches, selectedHighest);
            var lockedDropCount = Mathf.Max(0, PlannedDropUnlockCount - unlockedPlaceholderDrops);
            var lockedGlitchCount = Mathf.Max(0, PlannedGlitchUnlockCount - unlockedPlaceholderGlitches);

            return new BreakoutUiProgressionView
            {
                Title = "Progression",
                Subtitle = "Neon Ladder history, rarity gates, and where earned content appears next.",
                LadderTitle = "Ladder Progress",
                LadderLines = new[]
                {
                    $"Highest Clear: Heat {selectedHighest:00}/{BreakoutRunProgression.MaxRogueIntensity:00}",
                    $"Next Goal: {BuildNextGoalLine(selectedAvailable, selectedBestStage)}",
                    $"Ladder: {BreakoutRunProgression.TargetLevelCount:00} stages",
                },
                MeterLines = new[]
                {
                    $"Drops: {defaultDropCount:00} Default | {unlockedPlaceholderDrops:00} Placeholder Unlocked | {lockedDropCount:00} Locked",
                    $"Glitches: {DefaultGlitchCount:00} Default | {unlockedPlaceholderGlitches:00} Placeholder Unlocked | {lockedGlitchCount:00} Locked",
                    "Modes: Ladder rarity affects unlock order and capsule odds.",
                },
                NextSignal = BuildNextSignal(selectedAvailable, selectedBestStage),
                IntensityGauge = new BreakoutUiIntensityGaugeView
                {
                    IsVisible = true,
                    Intensity = selectedAvailable,
                    MaxIntensity = BreakoutRunProgression.MaxRogueIntensity,
                    Progress = BreakoutRunProgression.GetRogueIntensityProgress(selectedAvailable),
                    PulseRate = 1.35f,
                    Color = BreakoutRunProgression.GetRogueIntensityGaugeColor(selectedAvailable),
                },
                Cards = BuildCards(loadedPowerUps, selectedHighest, themeService),
                FooterText = "Esc returns to Mode Select. Scroll drops and glitches; rarity gates live content and placeholder cards.",
            };
        }

        private BreakoutUiProgressionCardView[] BuildCards(
            IReadOnlyList<PowerUpDefinition> loadedPowerUps,
            int highestCompletedIntensity,
            BreakoutThemeService themeService)
        {
            var cards = new List<BreakoutUiProgressionCardView>();
            AppendDefaultDropCards(cards, loadedPowerUps, themeService);
            cards.Add(BuildDefaultGlitchCard("Warp Gates", "Layout", "Linked portals reroute ball paths.", LayoutAccent));
            cards.Add(BuildDefaultGlitchCard("Turbo Rail", "Speed", "A hot wall rail accelerates rebounds.", SplitAccent));
            AppendPlaceholderCards(cards, PlaceholderDrops, highestCompletedIntensity);
            AppendPlaceholderCards(cards, PlaceholderGlitches, highestCompletedIntensity);
            cards.Add(BuildHiddenSlotCard("Future Drop Slot", "Drop", "Hidden", "Reserved for later Ladder bands or bundle work."));
            cards.Add(BuildHiddenSlotCard("Future Glitch Slot", "Glitch", "Hidden", "Reserved for later Ladder bands or surprise content."));
            return cards.ToArray();
        }

        private void AppendDefaultDropCards(
            List<BreakoutUiProgressionCardView> cards,
            IReadOnlyList<PowerUpDefinition> loadedPowerUps,
            BreakoutThemeService themeService)
        {
            if (loadedPowerUps == null)
            {
                return;
            }

            for (var index = 0; index < loadedPowerUps.Count; index++)
            {
                var definition = loadedPowerUps[index];

                if (definition == null)
                {
                    continue;
                }

                var style = ResolvePowerUpStyle(definition, themeService);
                cards.Add(new BreakoutUiProgressionCardView
                {
                    Title = definition.DisplayName,
                    Kind = "Drop",
                    Family = $"{definition.RarityLabel} {(definition.IsBeneficial ? "Helpful" : "Hazard")}",
                    Description = definition.IsBeneficial
                        ? "Authored default capsule. Always available when the mode allows helpful drops."
                        : "Authored default hazard. Always available when the mode allows harmful drops.",
                    UnlockHint = "Default content",
                    StateLabel = "Default",
                    ModeAvailability = "Ladder | Marathon | Multiplayer",
                    UnlockState = BreakoutUiProgressionUnlockState.Default,
                    Accent = definition.IsBeneficial ? ControlAccent : HazardAccent,
                    Icon = style.Sprite,
                    IconColor = style.PrimaryColor,
                });
            }
        }

        private ThemeVisualStyle ResolvePowerUpStyle(PowerUpDefinition definition, BreakoutThemeService themeService)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.white, null);
            }

            if (themeService != null)
            {
                return themeService.ResolvePowerUpStyle(definition);
            }

            return new ThemeVisualStyle(definition.PickupColor, definition.PickupColor, ResolvePowerUpSprite(definition));
        }

        private Sprite ResolvePowerUpSprite(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resourcePath = definition.ResolvePickupSpriteResourcePath();

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            if (!powerUpSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            {
                cachedSprite = Resources.Load<Sprite>(resourcePath);
                powerUpSpriteCache[resourcePath] = cachedSprite;
            }

            return cachedSprite;
        }

        private static BreakoutUiProgressionCardView BuildDefaultGlitchCard(string title, string family, string description, Color accent)
        {
            return new BreakoutUiProgressionCardView
            {
                Title = title,
                Kind = "Glitch",
                Family = family,
                Description = description,
                UnlockHint = "Default glitch archetype",
                StateLabel = "Default",
                ModeAvailability = "Ladder | Marathon | Multiplayer",
                UnlockState = BreakoutUiProgressionUnlockState.Default,
                Accent = accent,
            };
        }

        private static void AppendPlaceholderCards(
            List<BreakoutUiProgressionCardView> cards,
            BreakoutProgressionPlaceholderItem[] placeholders,
            int highestCompletedIntensity)
        {
            for (var index = 0; index < placeholders.Length; index++)
            {
                var placeholder = placeholders[index];
                var isEarnedForPreview = highestCompletedIntensity >= placeholder.UnlockIntensity;
                cards.Add(new BreakoutUiProgressionCardView
                {
                    Title = placeholder.Title,
                    Kind = placeholder.Kind,
                    Family = placeholder.Family,
                    Description = placeholder.Description,
                    UnlockHint = isEarnedForPreview
                        ? "Placeholder earned for UI preview only"
                        : $"Clear Heat {placeholder.UnlockIntensity:00} in Neon Ladder",
                    StateLabel = isEarnedForPreview ? "Unlocked*" : "Locked",
                    ModeAvailability = isEarnedForPreview
                        ? "Ladder | Marathon | Multiplayer"
                        : "Ladder goal",
                    UnlockState = isEarnedForPreview
                        ? BreakoutUiProgressionUnlockState.Unlocked
                        : BreakoutUiProgressionUnlockState.SeenLocked,
                    Accent = placeholder.Accent,
                });
            }
        }

        private static BreakoutUiProgressionCardView BuildHiddenSlotCard(
            string title,
            string kind,
            string family,
            string description)
        {
            return new BreakoutUiProgressionCardView
            {
                Title = title,
                Kind = kind,
                Family = family,
                Description = description,
                UnlockHint = "Hidden locked slot",
                StateLabel = "Unknown",
                ModeAvailability = "Future bundle",
                UnlockState = BreakoutUiProgressionUnlockState.HiddenLocked,
                Accent = LayoutAccent,
            };
        }

        private static int CountDefaultDrops(IReadOnlyList<PowerUpDefinition> loadedPowerUps)
        {
            var count = 0;

            if (loadedPowerUps == null)
            {
                return count;
            }

            for (var index = 0; index < loadedPowerUps.Count; index++)
            {
                if (loadedPowerUps[index] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEarnedPlaceholders(BreakoutProgressionPlaceholderItem[] placeholders, int highestCompletedIntensity)
        {
            var count = 0;

            for (var index = 0; index < placeholders.Length; index++)
            {
                if (highestCompletedIntensity >= placeholders[index].UnlockIntensity)
                {
                    count++;
                }
            }

            return count;
        }

        private static string BuildNextGoalLine(int availableIntensity, int bestStage)
        {
            return bestStage > 0
                ? $"Heat {availableIntensity:00}, best Stage {bestStage:00}/{BreakoutRunProgression.TargetLevelCount:00}"
                : $"Clear Heat {availableIntensity:00}";
        }

        private static string BuildNextSignal(int availableIntensity, int bestStage)
        {
            if (bestStage >= BreakoutRunProgression.TargetLevelCount)
            {
                return $"Next Signal: Heat {availableIntensity:00} is ready.";
            }

            if (bestStage > 0)
            {
                return $"Next Signal: push past Stage {bestStage:00} on Heat {availableIntensity:00}.";
            }

            return $"Next Signal: clear Intensity {availableIntensity:00}.";
        }
    }
}
