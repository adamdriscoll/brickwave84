using System;
using GetBricked.Gameplay.Data;

namespace GetBricked.Gameplay
{
    internal enum BreakoutMainMenuAction
    {
        Rogue,
        SoloMarathon,
        CustomGame,
        DualSticks,
        Coop,
        TurnBased,
        SoundSettings,
        GraphicsSettings,
        DeveloperMode,
    }

    internal sealed class BreakoutMainMenuContext
    {
        public int SelectedActionIndex;
        public string PendingSeedDisplay = string.Empty;
        public string DifficultyPresetLabel = string.Empty;
        public int BallsPerServe;
        public RunSettings PreviewSettings;
        public string ScoreModeSummaryLabel = string.Empty;
        public string RetrySummaryLabel = string.Empty;
        public string DropSummaryLabel = string.Empty;
        public string PreviewValidation = string.Empty;
        public string PendingValidationMessage = string.Empty;
        public string LastRogueResultSummary = string.Empty;
        public int AvailableRogueIntensity = 1;
        public string SoloMarathonDifficultyLabel = "Gnarly";
        public string SoloMarathonBestForHeatSummary = string.Empty;
        public string SoloMarathonBestOverallSummary = string.Empty;
        public string SelectedRoguePaddleLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;
        public string SelectedRoguePaddleIdentity = string.Empty;
        public string SelectedRoguePaddleStrength = string.Empty;
        public string SelectedRoguePaddleDrawback = string.Empty;
        public string NextRoguePaddleUnlockLabel = string.Empty;
        public string NextRoguePaddleUnlockRequirementLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;
        public int UnlockedRoguePaddleCount = 1;
        public int TotalRoguePaddleCount = 1;
    }

    internal sealed class BreakoutMainMenuService
    {
        private static readonly BreakoutMainMenuAction[] ActionCatalog =
        {
            BreakoutMainMenuAction.Rogue,
            BreakoutMainMenuAction.SoloMarathon,
            BreakoutMainMenuAction.CustomGame,
            BreakoutMainMenuAction.TurnBased,
            BreakoutMainMenuAction.SoundSettings,
            BreakoutMainMenuAction.GraphicsSettings,
            BreakoutMainMenuAction.DeveloperMode,
        };

        public BreakoutMainMenuAction[] BuildActions()
        {
            return (BreakoutMainMenuAction[])ActionCatalog.Clone();
        }

        public BreakoutUiMenuView BuildView(BreakoutMainMenuContext context)
        {
            context ??= new BreakoutMainMenuContext();

            var actions = BuildActions();
            var selectedAction = ResolveAction(context.SelectedActionIndex);

            return new BreakoutUiMenuView
            {
                Title = "Brickwave '84",
                Subtitle = "Choose the cabinet channel. Neon Marathon chases one solo score; Rogue starts a fixed 10-stage mixtape; Custom Game keeps the full tape-tuning bench.",
                SectionTitle = "Mode Select",
                ActionLabels = BuildActionLabels(actions),
                ActionGroupLabels = BuildActionGroupLabels(actions),
                SelectedActionIndex = context.SelectedActionIndex,
                PreviewTitle = BuildPreviewTitle(selectedAction),
                PreviewLines = BuildPreviewLines(selectedAction, context),
                ValidationText = BuildValidationText(selectedAction, context),
                FooterText = BuildFooterText(selectedAction),
                HintText = "Up/Down selects. Left/Right adjusts the highlighted mode. Space confirms. Custom Game opens setup.",
            };
        }

        public BreakoutMainMenuAction ResolveAction(int selectedActionIndex)
        {
            var clampedIndex = Math.Max(0, Math.Min(ActionCatalog.Length - 1, selectedActionIndex));
            return ActionCatalog[clampedIndex];
        }

        public string BuildPlaceholderMessage(BreakoutMainMenuAction action)
        {
            return action switch
            {
                BreakoutMainMenuAction.Rogue => "Rogue mode is staged for progression, unlocks, intensities, and cabinet heat.",
                BreakoutMainMenuAction.SoloMarathon => "Neon Marathon launches a solo high-score chase with five balls and a random Tape ID.",
                BreakoutMainMenuAction.DualSticks => "Dual Sticks is staged for side-by-side versus runs, sabotage drops, and brick sends.",
                BreakoutMainMenuAction.Coop => "Co-op is staged for two paddles, two balls, and one-keyboard shared survival.",
                BreakoutMainMenuAction.TurnBased => "Hot Seat opens player count, match mode, and heat setup.",
                BreakoutMainMenuAction.SoundSettings => "Sound Settings are staged for master, music, and cabinet SFX volume controls.",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics Settings are staged for future display, glow, scanline, and readability controls.",
                _ => string.Empty,
            };
        }

        private static string[] BuildActionLabels(BreakoutMainMenuAction[] actions)
        {
            var labels = new string[actions.Length];

            for (var index = 0; index < actions.Length; index++)
            {
                labels[index] = actions[index] switch
                {
                    BreakoutMainMenuAction.Rogue => "Rogue",
                    BreakoutMainMenuAction.SoloMarathon => "Neon Marathon",
                    BreakoutMainMenuAction.CustomGame => "Custom Game",
                    BreakoutMainMenuAction.DualSticks => "Dual Sticks",
                    BreakoutMainMenuAction.Coop => "Co-op",
                    BreakoutMainMenuAction.TurnBased => "Hot Seat",
                    BreakoutMainMenuAction.SoundSettings => "Sound",
                    BreakoutMainMenuAction.GraphicsSettings => "Graphics",
                    BreakoutMainMenuAction.DeveloperMode => "Dev",
                    _ => actions[index].ToString(),
                };
            }

            return labels;
        }

        private static string[] BuildActionGroupLabels(BreakoutMainMenuAction[] actions)
        {
            var labels = new string[actions.Length];

            for (var index = 0; index < actions.Length; index++)
            {
                labels[index] = actions[index] switch
                {
                    BreakoutMainMenuAction.Rogue => "Singleplayer",
                    BreakoutMainMenuAction.SoloMarathon => "Singleplayer",
                    BreakoutMainMenuAction.CustomGame => "Singleplayer",
                    BreakoutMainMenuAction.DualSticks => "Multiplayer",
                    BreakoutMainMenuAction.Coop => "Multiplayer",
                    BreakoutMainMenuAction.TurnBased => "Multiplayer",
                    BreakoutMainMenuAction.SoundSettings => "Settings",
                    BreakoutMainMenuAction.GraphicsSettings => "Settings",
                    BreakoutMainMenuAction.DeveloperMode => "Settings",
                    _ => string.Empty,
                };
            }

            return labels;
        }

        private static string BuildPreviewTitle(BreakoutMainMenuAction action)
        {
            return action switch
            {
                BreakoutMainMenuAction.Rogue => "Rogue Run",
                BreakoutMainMenuAction.SoloMarathon => "Neon Marathon",
                BreakoutMainMenuAction.CustomGame => "Custom Game Loadout",
                BreakoutMainMenuAction.DualSticks => "Versus Shell",
                BreakoutMainMenuAction.Coop => "Co-op Shell",
                BreakoutMainMenuAction.TurnBased => "Hot Seat",
                BreakoutMainMenuAction.SoundSettings => "Sound Settings Shell",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics Shell",
                BreakoutMainMenuAction.DeveloperMode => "Developer Jump",
                _ => "Cabinet Readout",
            };
        }

        private static string[] BuildPreviewLines(BreakoutMainMenuAction action, BreakoutMainMenuContext context)
        {
            switch (action)
            {
                case BreakoutMainMenuAction.Rogue:
                    var unlockLine = string.IsNullOrWhiteSpace(context.NextRoguePaddleUnlockLabel)
                        ? "All starter paddles unlocked for the bench."
                        : $"Next unlock: clear with {context.NextRoguePaddleUnlockRequirementLabel} to wake {context.NextRoguePaddleUnlockLabel}.";
                    return new[]
                    {
                        $"Paddle: {context.SelectedRoguePaddleLabel} ({context.UnlockedRoguePaddleCount}/{context.TotalRoguePaddleCount} unlocked).",
                        context.SelectedRoguePaddleIdentity,
                        context.SelectedRoguePaddleStrength,
                        $"Tradeoff: {context.SelectedRoguePaddleDrawback}",
                        $"Heat {context.AvailableRogueIntensity:00}/50. 10 stages. Draft rewards after clears.",
                        unlockLine,
                        string.IsNullOrWhiteSpace(context.LastRogueResultSummary)
                            ? "Last Run: no Rogue tape recorded yet."
                            : context.LastRogueResultSummary,
                    };
                case BreakoutMainMenuAction.SoloMarathon:
                    return new[]
                    {
                        $"Heat Level: {context.SoloMarathonDifficultyLabel}",
                        "Five balls. Random Tape ID. One player.",
                        "Clear stages until the cabinet finally wins.",
                        "Score mode: High Score. No custom modifiers.",
                        string.IsNullOrWhiteSpace(context.SoloMarathonBestForHeatSummary)
                            ? "Selected Heat: no record yet."
                            : context.SoloMarathonBestForHeatSummary,
                        string.IsNullOrWhiteSpace(context.SoloMarathonBestOverallSummary)
                            ? "All-Time: no record yet."
                            : context.SoloMarathonBestOverallSummary,
                    };
                case BreakoutMainMenuAction.CustomGame:
                    var previewSettings = context.PreviewSettings;
                    return new[]
                    {
                        $"Tape ID: {context.PendingSeedDisplay}",
                        $"Difficulty: {context.DifficultyPresetLabel} | Score Mode: {previewSettings?.ScoringModeLabel ?? "Offline"}",
                        $"Balls/Serve: {context.BallsPerServe} | {context.RetrySummaryLabel}",
                        $"Theme: {previewSettings?.ThemeLabel ?? "Offline"}",
                        $"Paddle x{previewSettings?.PaddleWidthMultiplier ?? 1f:0.00} | Ball x{previewSettings?.BallSpeedMultiplier ?? 1f:0.00}",
                        $"Brick durability x{previewSettings?.BrickDurabilityMultiplier ?? 1f:0.00} | {context.ScoreModeSummaryLabel}",
                        $"Drops: {context.DropSummaryLabel}",
                    };
                case BreakoutMainMenuAction.DualSticks:
                    return new[]
                    {
                        "Side-by-side versus action.",
                        "Race to knock out your wall before the other player.",
                        "Future: send negative drops and extra bricks across the cabinet.",
                        "Status: placeholder shell.",
                    };
                case BreakoutMainMenuAction.Coop:
                    return new[]
                    {
                        "Side-by-side shared run.",
                        "Two paddles, two balls, one keyboard.",
                        "Future: custom-game rules with shared survival pressure.",
                        "Status: placeholder shell.",
                    };
                case BreakoutMainMenuAction.TurnBased:
                    return new[]
                    {
                        "Pass-the-cabinet play for 2-10 players.",
                        "Top Score: most points after fixed turns.",
                        "Outlast: last player with balls wins.",
                        "Random Tape ID and shared heat level per match.",
                    };
                case BreakoutMainMenuAction.SoundSettings:
                    return new[]
                    {
                        "Master Volume: placeholder",
                        "Synth Volume: placeholder",
                        "Cabinet SFX: placeholder",
                        "Future: persisted mixer controls and audio test cues.",
                    };
                case BreakoutMainMenuAction.GraphicsSettings:
                    return new[]
                    {
                        "Display: placeholder",
                        "Neon Glow: placeholder",
                        "Scanlines: placeholder",
                        "Future: bloom, CRT treatment, and readability options.",
                    };
                case BreakoutMainMenuAction.DeveloperMode:
                    return new[]
                    {
                        "Jump directly into Rogue stages or boss gates.",
                        "Pick paddle, lives, run upgrades, and drop unlocks before launch.",
                        "For local tuning only. No progression result is protected here yet.",
                        "Status: active debug bench.",
                    };
                default:
                    return Array.Empty<string>();
            }
        }

        private static string BuildValidationText(BreakoutMainMenuAction action, BreakoutMainMenuContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.PendingValidationMessage))
            {
                return context.PendingValidationMessage;
            }

            return action == BreakoutMainMenuAction.CustomGame
                ? context.PreviewValidation
                : string.Empty;
        }

        private static string BuildFooterText(BreakoutMainMenuAction action)
        {
            return action switch
            {
                BreakoutMainMenuAction.CustomGame => "Custom Game opens the full tape-tuning bench: Tape ID, score rules, modifiers, drops, and theme.",
                BreakoutMainMenuAction.Rogue => "Rogue launches a fixed 10-stage mixtape with draft rewards and saved results.",
                BreakoutMainMenuAction.SoloMarathon => "Neon Marathon launches immediately with five balls, a random Tape ID, and only Heat Level to tune.",
                BreakoutMainMenuAction.SoundSettings => "Sound controls are staged here for mixer work.",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics controls are staged here for display, glow, and readability options.",
                BreakoutMainMenuAction.TurnBased => "Hot Seat opens player count, mode, and heat setup before the match starts.",
                BreakoutMainMenuAction.DeveloperMode => "Developer Mode opens the local jump bench for Rogue stages, boss gates, and tuning.",
                _ => "This cabinet channel is staged for later.",
            };
        }
    }
}
