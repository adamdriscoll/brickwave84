using System;
using GetBricked.Gameplay.Data;

namespace GetBricked.Gameplay
{
    internal enum BreakoutMainMenuAction
    {
        Rogue,
        Progression,
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
        public string SoloMarathonScoreMultiplierLabel = "x1.35";
        public string SoloMarathonBestForHeatSummary = string.Empty;
        public string SoloMarathonBestOverallSummary = string.Empty;
    }

    internal sealed class BreakoutMainMenuService
    {
        private static readonly BreakoutMainMenuAction[] ActionCatalog =
        {
            BreakoutMainMenuAction.Rogue,
            BreakoutMainMenuAction.Progression,
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
                Subtitle = "Choose the cabinet channel. Neon Ladder climbs a fixed 10-stage unlock run; Neon Marathon opens heat select and top scores; Custom Game keeps the full tape-tuning bench.",
                SectionTitle = "Mode Select",
                ActionLabels = BuildActionLabels(actions),
                ActionGroupLabels = BuildActionGroupLabels(actions),
                SelectedActionIndex = context.SelectedActionIndex,
                PreviewTitle = BuildPreviewTitle(selectedAction),
                PreviewLines = BuildPreviewLines(selectedAction, context),
                ValidationText = BuildValidationText(selectedAction, context),
                FooterText = BuildFooterText(selectedAction),
                HintText = "Up/Down selects. Left/Right adjusts the highlighted mode. Space confirms. Neon Marathon opens heat select.",
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
                BreakoutMainMenuAction.Rogue => "Neon Ladder climbs 10 stages with draft rewards, unlocks, and cabinet heat.",
                BreakoutMainMenuAction.Progression => "Progression opens the cabinet service screen for Ladder goals, drops, and glitches.",
                BreakoutMainMenuAction.SoloMarathon => "Neon Marathon opens heat select, top scores, and a solo high-score chase.",
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
                    BreakoutMainMenuAction.Rogue => "Neon Ladder",
                    BreakoutMainMenuAction.Progression => "Progression",
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
                    BreakoutMainMenuAction.Progression => "Singleplayer",
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
                BreakoutMainMenuAction.Rogue => "Neon Ladder",
                BreakoutMainMenuAction.Progression => "Cabinet Progress",
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
                    return new[]
                    {
                        $"Heat {context.AvailableRogueIntensity:00}/50. 10-stage ladder. Draft rewards after clears.",
                        "Starter loadout is fixed while the cabinet tunes the next build layer.",
                        "Clear stages, draft rewards, and push the next heat signal.",
                        string.IsNullOrWhiteSpace(context.LastRogueResultSummary)
                            ? "Last Run: no Neon Ladder tape recorded yet."
                            : context.LastRogueResultSummary,
                    };
                case BreakoutMainMenuAction.Progression:
                    return new[]
                    {
                        $"Heat {context.AvailableRogueIntensity:00}/50 is the next Neon Ladder goal.",
                        "Drops and glitches show Default, placeholder Unlocked, Locked, and Unknown slots.",
                        "Marathon and multiplayer badges preview where earned content will appear later.",
                        "Status: presentation shell. Unlock effects are not wired into gameplay pools yet.",
                    };
                case BreakoutMainMenuAction.SoloMarathon:
                    return new[]
                    {
                        $"Heat Level: {context.SoloMarathonDifficultyLabel} | Score {context.SoloMarathonScoreMultiplierLabel}",
                        "Five balls. Random Tape ID. One player.",
                        "Clear stages until the cabinet finally wins.",
                        "Score mode: High Score. Higher heat pays more.",
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
                        "Jump directly into Neon Ladder stages.",
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
                BreakoutMainMenuAction.Rogue => "Neon Ladder launches a fixed 10-stage climb with draft rewards and saved unlock progress.",
                BreakoutMainMenuAction.Progression => "Progression opens a cabinet service screen for Ladder history, future unlock placeholders, and mode availability.",
                BreakoutMainMenuAction.SoloMarathon => "Neon Marathon opens its heat bench with top scores for each heat level.",
                BreakoutMainMenuAction.SoundSettings => "Sound controls are staged here for mixer work.",
                BreakoutMainMenuAction.GraphicsSettings => "Graphics controls are staged here for display, glow, and readability options.",
                BreakoutMainMenuAction.TurnBased => "Hot Seat opens player count, mode, and heat setup before the match starts.",
                BreakoutMainMenuAction.DeveloperMode => "Developer Mode opens the local jump bench for stages and tuning.",
                _ => "This cabinet channel is staged for later.",
            };
        }
    }
}
