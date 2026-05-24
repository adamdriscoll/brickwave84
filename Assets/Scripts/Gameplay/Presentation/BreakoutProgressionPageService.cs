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
        private const int DefaultGlitchCount = 1;

        private static readonly string[] DefaultDropIds =
        {
            "large_paddle",
            "multi_ball",
            "slow_ball",
            "shield_wall",
        };

        private static readonly Color ControlAccent = new Color(0.45f, 0.95f, 0.72f, 1f);
        private static readonly Color PrecisionAccent = new Color(1f, 0.87f, 0.36f, 1f);
        private static readonly Color DamageAccent = new Color(1f, 0.49f, 0.86f, 1f);
        private static readonly Color SplitAccent = new Color(0.01f, 0.93f, 0.98f, 1f);
        private static readonly Color HazardAccent = new Color(0.99f, 0.27f, 0.31f, 1f);
        private static readonly Color LayoutAccent = new Color(0.72f, 0.62f, 1f, 1f);

        private readonly Dictionary<string, Sprite> powerUpSpriteCache = new Dictionary<string, Sprite>();

        private static readonly BreakoutProgressionPlaceholderItem[] PlaceholderDrops =
        {
        };

        private static readonly BreakoutProgressionPlaceholderItem[] PlaceholderGlitches =
        {
            new BreakoutProgressionPlaceholderItem("Row Rewrite", "Glitch", "Layout", "One row rerolls into a new brick pattern after a timer.", 6, LayoutAccent),
            new BreakoutProgressionPlaceholderItem("Prism Lanes", "Glitch", "Precision", "Marked lanes refract the ball into sharper angles.", 7, PrecisionAccent),
        };

        public BreakoutUiProgressionView BuildView(IReadOnlyList<PowerUpDefinition> loadedPowerUps, BreakoutThemeService themeService = null)
        {
            var progressPaddleLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;
            var selectedHighest = BreakoutRogueIntensityProgressStore.GetHighestCompletedIntensity(progressPaddleLabel);
            var selectedAvailable = BreakoutRogueIntensityProgressStore.GetAvailableIntensity(progressPaddleLabel);
            var selectedBestStage = BreakoutRogueIntensityProgressStore.GetBestStageReached(progressPaddleLabel, selectedAvailable);
            var defaultDropCount = CountDefaultDrops(loadedPowerUps);
            var unlockedLiveDrops = CountEarnedLiveDrops(loadedPowerUps, selectedHighest);
            var unlockedLiveGlitches = CountEarnedLiveGlitches(selectedHighest);
            var unlockedPlaceholderDrops = CountEarnedPlaceholders(PlaceholderDrops, selectedHighest);
            var unlockedPlaceholderGlitches = CountEarnedPlaceholders(PlaceholderGlitches, selectedHighest);
            var lockedDropCount = Mathf.Max(0, PlannedDropUnlockCount - unlockedLiveDrops - unlockedPlaceholderDrops);
            var lockedGlitchCount = Mathf.Max(0, PlannedGlitchUnlockCount - unlockedLiveGlitches - unlockedPlaceholderGlitches);

            return new BreakoutUiProgressionView
            {
                Title = "Progression",
                Subtitle = "Neon Ladder history, Heat unlocks, and where earned content appears next.",
                LadderTitle = "Ladder Progress",
                LadderLines = new[]
                {
                    $"Highest Clear: Heat {selectedHighest:00}/{BreakoutRunProgression.MaxRogueIntensity:00}",
                    $"Next Goal: {BuildNextGoalLine(selectedAvailable, selectedBestStage)}",
                    $"Ladder: {BreakoutRunProgression.TargetLevelCount:00} stages",
                },
                MeterLines = new[]
                {
                    $"Drops: {defaultDropCount:00} Default | {unlockedLiveDrops + unlockedPlaceholderDrops:00} Earned | {lockedDropCount:00} Locked",
                    $"Glitches: {DefaultGlitchCount:00} Default | {unlockedLiveGlitches + unlockedPlaceholderGlitches:00} Earned | {lockedGlitchCount:00} Locked",
                    "Modes: each cleared Heat adds one drop and one glitch signal.",
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
                FooterText = "Space starts. Esc backs out. Mouse wheel scrolls.",
            };
        }

        private BreakoutUiProgressionCardView[] BuildCards(
            IReadOnlyList<PowerUpDefinition> loadedPowerUps,
            int highestCompletedIntensity,
            BreakoutThemeService themeService)
        {
            var cards = new List<BreakoutUiProgressionCardView>();
            AppendDropCards(cards, loadedPowerUps, highestCompletedIntensity, themeService);
            cards.Add(BuildDefaultGlitchCard("Warp Gates", "Layout", "Linked portals reroute ball paths.", LayoutAccent));
            cards.Add(BuildUnlockableGlitchCard(
                "Turbo Rail",
                "Speed Rare Glitch",
                "A hot wall rail accelerates rebounds.",
                BreakoutLevelGlitchPlanner.TurboRailLadderUnlockIntensity,
                highestCompletedIntensity,
                SplitAccent));
            cards.Add(BuildUnlockableGlitchCard(
                "Mirror Grid",
                "Layout Rare Glitch",
                "Brick layout mirrors horizontally halfway through the stage.",
                BreakoutLevelGlitchPlanner.MirrorGridLadderUnlockIntensity,
                highestCompletedIntensity,
                LayoutAccent));
            cards.Add(BuildUnlockableGlitchCard(
                "Token Storm",
                "Pickup Epic Glitch",
                "More capsules spawn, but fall at mixed speeds.",
                BreakoutLevelGlitchPlanner.TokenStormLadderUnlockIntensity,
                highestCompletedIntensity,
                ControlAccent));
            cards.Add(BuildUnlockableGlitchCard(
                "Gravity Pocket",
                "Speed Epic Glitch",
                "A slow drifting pocket bends nearby ball paths.",
                BreakoutLevelGlitchPlanner.GravityPocketLadderUnlockIntensity,
                highestCompletedIntensity,
                SplitAccent));
            cards.Add(BuildUnlockableGlitchCard(
                "Static Wall",
                "Paddle Epic Glitch",
                "One side wall flickers between normal and weak bounce.",
                BreakoutLevelGlitchPlanner.StaticWallLadderUnlockIntensity,
                highestCompletedIntensity,
                HazardAccent));
            AppendPlaceholderCards(cards, PlaceholderDrops, highestCompletedIntensity);
            AppendPlaceholderCards(cards, PlaceholderGlitches, highestCompletedIntensity);
            cards.Add(BuildHiddenSlotCard("Future Drop Slot", "Drop", "Hidden", "Future drop signal pending."));
            cards.Add(BuildHiddenSlotCard("Future Glitch Slot", "Glitch", "Hidden", "Future glitch signal pending."));
            return cards.ToArray();
        }

        private void AppendDropCards(
            List<BreakoutUiProgressionCardView> cards,
            IReadOnlyList<PowerUpDefinition> loadedPowerUps,
            int highestCompletedIntensity,
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
                var isDefault = IsDefaultDrop(definition);
                var unlockIntensity = definition.LadderUnlockIntensity;
                var isUnlocked = isDefault || definition.IsEarnedForCompletedLadderIntensity(highestCompletedIntensity);
                cards.Add(new BreakoutUiProgressionCardView
                {
                    Title = definition.DisplayName,
                    Kind = "Drop",
                    Family = $"{definition.RarityLabel} {(definition.IsBeneficial ? "Helpful" : "Hazard")}",
                    Description = BuildDropDescription(definition),
                    UnlockHint = isDefault
                        ? "Default content"
                        : isUnlocked
                            ? $"Cleared Heat {unlockIntensity:00}"
                            : $"Clear Heat {unlockIntensity:00} in Neon Ladder",
                    StateLabel = isDefault ? "Default" : isUnlocked ? "Unlocked" : "Locked",
                    ModeAvailability = isUnlocked ? "Ladder | Marathon | Multiplayer" : "Ladder goal",
                    UnlockState = isDefault
                        ? BreakoutUiProgressionUnlockState.Default
                        : isUnlocked
                            ? BreakoutUiProgressionUnlockState.Unlocked
                            : BreakoutUiProgressionUnlockState.SeenLocked,
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

        private static string BuildDropDescription(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            return definition.EffectType switch
            {
                PowerUpEffectType.PaddleWidthMultiplier => $"Paddle width x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.BallSpeedMultiplier => $"Ball speed x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.BallSizeMultiplier => $"Ball size x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.MultiBallBurst => $"+{Mathf.Max(1, definition.ExtraBallCount)} balls from an active ball.",
                PowerUpEffectType.WavyPaddle => $"Adds paddle sway for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.PaddleHitTilt => $"Each paddle hit tilts the rail {definition.Scalar:0.#}deg for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.PaddleWrap => $"Paddle exits one side wall and enters the other for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.StickyPaddle => $"Catch and relaunch the next paddle ball for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.CleanCatch => $"Next paddle hit catches, then releases with aim x{definition.Scalar:0.00}.",
                PowerUpEffectType.LaserPaddle => $"Paddle fires lasers for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ShieldWall => $"Adds {Mathf.Max(1, definition.ExtraBallCount > 0 ? definition.ExtraBallCount : Mathf.RoundToInt(definition.Scalar))} bottom rescue charge.",
                PowerUpEffectType.PhaseBall => $"Ball phases through breakable bricks for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ChainLightning => $"Broken bricks chain damage nearby bricks for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ReverseControls => $"Reverses paddle controls for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.SplitPaddle => $"Opens a center paddle gap for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.GravityWell => $"Pulls balls toward the arena midpoint for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.FogOfWar => definition.VisibilityScalar <= 0f
                    ? $"Blacks out brick visibility for {definition.DurationSeconds:0.#}s."
                    : $"Reduces brick and pickup visibility for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.LagSpike => $"Stalls paddle response in bursts for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ActiveDropMultiplier => $"Active timed effects x{definition.Scalar:0.00}.",
                PowerUpEffectType.BrickMagnet => $"Pulls the ball toward nearby bricks for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ScoreMultiplier => $"Score x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.PaddleClone => $"Adds a clone rail for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.MirrorImagePaddle => $"Adds an opposite-moving mirror paddle for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.BrickJammer => $"Jams brick response for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.HotPotatoBall => $"Ball speed and score x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.ExplosiveBall => $"Ball explosions damage nearby bricks for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.VectorSight => $"Shows a short paddle aim preview for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.RandomHarmfulDrop => "Looks helpful, then rolls a random hazard.",
                PowerUpEffectType.CapsuleMagnet => $"Nearby helpful capsules drift toward the paddle for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.BankBonus => $"Wall bounces bank +{Mathf.Max(1, Mathf.RoundToInt(definition.Scalar))} points until the next brick hit for {definition.DurationSeconds:0.#}s.",
                PowerUpEffectType.PrismPop => $"First brick hit within {definition.DurationSeconds:0.#}s splits a short-lived copy ball.",
                PowerUpEffectType.SolarShot => "Next weak brick touched by the ball burns away without bouncing it.",
                PowerUpEffectType.RandomMixedDrop => "Rolls one random unlocked helpful drop and one random unlocked hazard.",
                _ => $"{definition.HudLabel} for {definition.DurationSeconds:0.#}s.",
            };
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
                cachedSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(resourcePath);
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

        private static BreakoutUiProgressionCardView BuildUnlockableGlitchCard(
            string title,
            string family,
            string description,
            int unlockIntensity,
            int highestCompletedIntensity,
            Color accent)
        {
            var isUnlocked = Mathf.Clamp(highestCompletedIntensity, 0, BreakoutRunProgression.MaxRogueIntensity)
                >= BreakoutRunProgression.ClampRogueIntensity(unlockIntensity);

            return new BreakoutUiProgressionCardView
            {
                Title = title,
                Kind = "Glitch",
                Family = family,
                Description = description,
                UnlockHint = isUnlocked
                    ? $"Cleared Heat {unlockIntensity:00}"
                    : $"Clear Heat {unlockIntensity:00} in Neon Ladder",
                StateLabel = isUnlocked ? "Unlocked" : "Locked",
                ModeAvailability = isUnlocked ? "Ladder | Marathon | Multiplayer" : "Ladder goal",
                UnlockState = isUnlocked
                    ? BreakoutUiProgressionUnlockState.Unlocked
                    : BreakoutUiProgressionUnlockState.SeenLocked,
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
                if (IsDefaultDrop(loadedPowerUps[index]))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEarnedLiveDrops(IReadOnlyList<PowerUpDefinition> loadedPowerUps, int highestCompletedIntensity)
        {
            var count = 0;

            if (loadedPowerUps == null)
            {
                return count;
            }

            for (var index = 0; index < loadedPowerUps.Count; index++)
            {
                var definition = loadedPowerUps[index];

                if (definition != null
                    && !IsDefaultDrop(definition)
                    && definition.IsEarnedForCompletedLadderIntensity(highestCompletedIntensity))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEarnedLiveGlitches(int highestCompletedIntensity)
        {
            var completedIntensity = Mathf.Clamp(highestCompletedIntensity, 0, BreakoutRunProgression.MaxRogueIntensity);
            var count = 0;

            if (completedIntensity >= BreakoutLevelGlitchPlanner.TurboRailLadderUnlockIntensity)
            {
                count++;
            }

            if (completedIntensity >= BreakoutLevelGlitchPlanner.MirrorGridLadderUnlockIntensity)
            {
                count++;
            }

            if (completedIntensity >= BreakoutLevelGlitchPlanner.TokenStormLadderUnlockIntensity)
            {
                count++;
            }

            if (completedIntensity >= BreakoutLevelGlitchPlanner.GravityPocketLadderUnlockIntensity)
            {
                count++;
            }

            if (completedIntensity >= BreakoutLevelGlitchPlanner.StaticWallLadderUnlockIntensity)
            {
                count++;
            }

            return count;
        }

        private static bool IsDefaultDrop(PowerUpDefinition definition)
        {
            var candidateId = BreakoutPowerUpIdentity.GetStableId(definition);

            if (string.IsNullOrWhiteSpace(candidateId))
            {
                return false;
            }

            for (var index = 0; index < DefaultDropIds.Length; index++)
            {
                if (string.Equals(candidateId, DefaultDropIds[index], System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
