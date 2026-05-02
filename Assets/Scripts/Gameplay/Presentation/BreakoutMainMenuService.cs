using System;
using GetBricked.Gameplay.Data;

namespace GetBricked.Gameplay
{
    internal enum BreakoutMainMenuAction
    {
        Rogue,
        CustomGame,
        DualSticks,
        Coop,
        TurnBased,
        SoundSettings,
        GraphicsSettings,
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
    }

    internal sealed class BreakoutMainMenuService
    {
        private static readonly BreakoutMainMenuAction[] ActionCatalog =
        {
            BreakoutMainMenuAction.Rogue,
            BreakoutMainMenuAction.CustomGame,
            BreakoutMainMenuAction.DualSticks,
            BreakoutMainMenuAction.Coop,
            BreakoutMainMenuAction.TurnBased,
            BreakoutMainMenuAction.SoundSettings,
            BreakoutMainMenuAction.GraphicsSettings,
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
                Title = "Get Bricked",
                Subtitle = "Choose the cabinet channel. Rogue starts a fixed 10-stage mixtape; Custom Game keeps the full tape-tuning bench.",
                SectionTitle = "Mode Select",
                ActionLabels = BuildActionLabels(actions),
                ActionGroupLabels = BuildActionGroupLabels(actions),
                SelectedActionIndex = context.SelectedActionIndex,
                PreviewTitle = BuildPreviewTitle(selectedAction),
                PreviewLines = BuildPreviewLines(selectedAction, context),
                ValidationText = BuildValidationText(selectedAction, context),
                FooterText = BuildFooterText(selectedAction),
                HintText = "Up/Down selects. Space confirms. Rogue starts now. Custom Game opens seeded setup. Other channels are staged for upcoming sessions.",
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
                BreakoutMainMenuAction.DualSticks => "Dual Sticks is staged for side-by-side versus runs, sabotage drops, and brick sends.",
                BreakoutMainMenuAction.Coop => "Co-op is staged for two paddles, two balls, and one-keyboard shared survival.",
                BreakoutMainMenuAction.TurnBased => "Turn-Based is staged for custom-game handoffs across lives and levels.",
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
                    BreakoutMainMenuAction.CustomGame => "Custom Game",
                    BreakoutMainMenuAction.DualSticks => "Dual Sticks",
                    BreakoutMainMenuAction.Coop => "Co-op",
                    BreakoutMainMenuAction.TurnBased => "Turn-Based",
                    BreakoutMainMenuAction.SoundSettings => "Sound",
                    BreakoutMainMenuAction.GraphicsSettings => "Graphics",
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
                    BreakoutMainMenuAction.CustomGame => "Singleplayer",
                    BreakoutMainMenuAction.DualSticks => "Multiplayer",
                    BreakoutMainMenuAction.Coop => "Multiplayer",
                    BreakoutMainMenuAction.TurnBased => "Multiplayer",
                    BreakoutMainMenuAction.SoundSettings => "Settings",
                    BreakoutMainMenuAction.GraphicsSettings => "Settings",
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
                BreakoutMainMenuAction.CustomGame => "Custom Game Loadout",
                BreakoutMainMenuAction.DualSticks => "Versus Shell",
                BreakoutMainMenuAction.Coop => "Co-op Shell",
                BreakoutMainMenuAction.TurnBased => "Turn-Based Shell",
                BreakoutMainMenuAction.SoundSettings => "Sound Settings Shell",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics Shell",
                _ => "Cabinet Readout",
            };
        }

        private static string[] BuildPreviewLines(BreakoutMainMenuAction action, BreakoutMainMenuContext context)
        {
            switch (action)
            {
                case BreakoutMainMenuAction.Rogue:
                    return new[]
                    {
                        "10 stages. 3 balls. One fresh Tape ID.",
                        "Draft run upgrades or unlock new capsules after each cleared stage.",
                        "Boss gates, intensity, and paddle unlocks are staged next.",
                        string.IsNullOrWhiteSpace(context.LastRogueResultSummary)
                            ? "Last Run: no Rogue tape recorded yet."
                            : context.LastRogueResultSummary,
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
                        "Custom Game with player handoffs.",
                        "Switch turns on lives and level clears.",
                        "Future: shared Tape ID, fair scoring, and turn readouts.",
                        "Status: placeholder shell.",
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
                BreakoutMainMenuAction.CustomGame => "Custom Game is the current playable setup path with Tape ID editing, score mode tuning, modifier tweaks, and theme cycling.",
                BreakoutMainMenuAction.Rogue => "Rogue launches the first progression-forward singleplayer run: fixed rules, seeded stages, draft rewards, and a saved result.",
                BreakoutMainMenuAction.SoundSettings => "Sound controls are staged here so mixer work has a clear home.",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics controls are staged here so display and readability options have a clear home.",
                _ => "Multiplayer channels are staged as menu shells until the shared-screen architecture is ready.",
            };
        }
    }
}
