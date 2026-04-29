using System;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutUiMenuView
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string SectionTitle = string.Empty;
        public string[] ActionLabels = Array.Empty<string>();
        public int SelectedActionIndex;
        public string PreviewTitle = string.Empty;
        public string[] PreviewLines = Array.Empty<string>();
        public string ValidationText = string.Empty;
        public string FooterText = string.Empty;
        public string HintText = string.Empty;
    }

    internal sealed class BreakoutUiRunSetupView
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string[] FieldLines = Array.Empty<string>();
        public int SelectedFieldIndex;
        public string PreviewLine = string.Empty;
        public string ValidationText = string.Empty;
        public string HintText = string.Empty;
    }

    internal sealed class BreakoutUiSpeedMeterView
    {
        public float BounceZoneLeftScreen;
        public bool IsDiagnosticsVisible;
        public float Speed;
        public float SpeedRatio;
    }

    internal sealed class BreakoutUiHudView
    {
        public string TopLine = string.Empty;
        public string BottomLine = string.Empty;
        public bool ShowMenuButton;
        public bool IsPaused;
        public bool IsDiagnosticsVisible;
        public BreakoutUiSpeedMeterView SpeedMeter = new BreakoutUiSpeedMeterView();
    }

    internal sealed class BreakoutUiOverlayView
    {
        public string Title = string.Empty;
        public string[] SummaryLines = Array.Empty<string>();
        public string[] ActionLabels = Array.Empty<string>();
        public int SelectedActionIndex;
        public string[] FooterLines = Array.Empty<string>();
        public bool IsCompact;
    }

    internal sealed class BreakoutUiModifierView
    {
        public string Label = string.Empty;
        public float RemainingDuration;
        public float DurationRatio = 1f;
        public Color Color = Color.white;
    }

    internal sealed class BreakoutUiDiagnosticsView
    {
        public string[] Lines = Array.Empty<string>();
    }

    internal sealed class BreakoutUiBannerView
    {
        public string Text = string.Empty;
        public float Timer;
        public Color Color = Color.white;
    }

    internal sealed class BreakoutUiRenderer
    {
        private GUIStyle hudStyle;
        private GUIStyle messageStyle;
        private GUIStyle pickupStyle;
        private GUIStyle setupTitleStyle;
        private GUIStyle setupSelectedStyle;
        private GUIStyle setupHintStyle;
        private GUIStyle overlayTitleStyle;
        private GUIStyle overlayBodyStyle;
        private GUIStyle overlayActionStyle;
        private GUIStyle overlaySelectedActionStyle;
        private GUIStyle hudButtonStyle;
        private GUIStyle hudActiveButtonStyle;
        private GUIStyle speedMeterCaptionStyle;
        private GUIStyle speedMeterValueStyle;
        private GUIStyle modifierPanelTitleStyle;
        private GUIStyle modifierPanelLabelStyle;
        private GUIStyle modifierPanelTimerStyle;
        private GUIStyle modifierPanelEmptyStyle;

        public void DrawMainMenu(BreakoutUiMenuView view, Action<int> onActionClicked)
        {
            EnsureStyles();

            var boxRect = new Rect((Screen.width * 0.5f) - 410f, (Screen.height * 0.5f) - 220f, 820f, 440f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 18f, boxRect.width - 48f, 36f), view.Title, overlayTitleStyle);
            GUI.Label(new Rect(boxRect.x + 32f, boxRect.y + 58f, boxRect.width - 64f, 24f), view.Subtitle, setupHintStyle);

            GUI.Label(new Rect(boxRect.x + 42f, boxRect.y + 112f, 280f, 28f), view.SectionTitle, setupSelectedStyle);
            DrawActionList(view.ActionLabels, view.SelectedActionIndex, boxRect.x + 42f, boxRect.y + 150f, 270f, 36f, onActionClicked);

            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 112f, 410f, 28f), view.PreviewTitle, setupSelectedStyle);

            for (var index = 0; index < view.PreviewLines.Length; index++)
            {
                GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 150f + (index * 30f), 410f, 26f), view.PreviewLines[index], hudStyle);
            }

            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 304f, 410f, 52f), view.ValidationText, setupHintStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 356f, 410f, 38f), view.FooterText, setupHintStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 395f, boxRect.width - 56f, 24f), view.HintText, setupHintStyle);
        }

        public void DrawRunSetup(BreakoutUiRunSetupView view)
        {
            EnsureStyles();

            var boxRect = new Rect((Screen.width * 0.5f) - 360f, (Screen.height * 0.5f) - 231f, 720f, 462f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 18f, boxRect.width - 48f, 34f), view.Title, setupTitleStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 56f, boxRect.width - 56f, 22f), view.Subtitle, setupHintStyle);

            var fieldX = boxRect.x + 36f;
            var fieldWidth = boxRect.width - 72f;
            var lineHeight = 30f;
            var startY = boxRect.y + 96f;

            for (var index = 0; index < view.FieldLines.Length; index++)
            {
                var isSelected = index == Mathf.Clamp(view.SelectedFieldIndex, 0, Math.Max(0, view.FieldLines.Length - 1));
                GUI.Label(
                    new Rect(fieldX, startY + (lineHeight * index), fieldWidth, 26f),
                    $"{(isSelected ? "> " : "  ")}{view.FieldLines[index]}",
                    isSelected ? setupSelectedStyle : hudStyle);
            }

            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 338f, boxRect.width - 56f, 24f), view.PreviewLine, hudStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 368f, boxRect.width - 56f, 40f), view.ValidationText, setupHintStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 408f, boxRect.width - 56f, 36f), view.HintText, setupHintStyle);
        }

        public void DrawGameplayHud(BreakoutUiHudView view, Action onToggleDiagnostics, Action onToggleMenu)
        {
            EnsureStyles();

            var topBarRect = new Rect(0f, 0f, Screen.width, 76f);
            var contentWidth = Mathf.Max(320f, topBarRect.width - 256f);
            var buttonsX = topBarRect.xMax - 232f;
            var buttonsY = topBarRect.y + 18f;
            var diagnosticsLabel = view.IsDiagnosticsVisible ? "Hide Diagnostics" : "Diagnostics";
            var menuLabel = view.IsPaused ? "Resume" : "Menu";

            DrawSolidRect(topBarRect, new Color(0.05f, 0.07f, 0.11f, 0.96f));
            GUI.Label(new Rect(20f, topBarRect.y + 12f, contentWidth, 24f), view.TopLine, hudStyle);
            GUI.Label(new Rect(20f, topBarRect.y + 40f, contentWidth, 24f), view.BottomLine, hudStyle);
            DrawBallSpeedMeter(view.SpeedMeter);

            if (GUI.Button(
                    new Rect(buttonsX, buttonsY, 108f, 38f),
                    diagnosticsLabel,
                    view.IsDiagnosticsVisible ? hudActiveButtonStyle : hudButtonStyle))
            {
                onToggleDiagnostics?.Invoke();
            }

            if (view.ShowMenuButton
                && GUI.Button(
                    new Rect(buttonsX + 116f, buttonsY, 96f, 38f),
                    menuLabel,
                    view.IsPaused ? hudActiveButtonStyle : hudButtonStyle))
            {
                onToggleMenu?.Invoke();
            }
        }

        public void DrawOverlay(BreakoutUiOverlayView view, Action<int> onActionClicked)
        {
            EnsureStyles();

            var boxRect = view.IsCompact
                ? new Rect((Screen.width * 0.5f) - 320f, (Screen.height * 0.5f) - 180f, 640f, 360f)
                : new Rect((Screen.width * 0.5f) - 320f, (Screen.height * 0.5f) - 162f, 640f, 324f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 20f, boxRect.width - 48f, 36f), view.Title, overlayTitleStyle);

            for (var index = 0; index < view.SummaryLines.Length; index++)
            {
                GUI.Label(
                    new Rect(boxRect.x + 32f, boxRect.y + 64f + (index * 28f), boxRect.width - 64f, 24f),
                    view.SummaryLines[index],
                    overlayBodyStyle);
            }

            DrawActionList(
                view.ActionLabels,
                view.SelectedActionIndex,
                view.IsCompact ? boxRect.x + 90f : boxRect.x + 94f,
                view.IsCompact ? boxRect.y + 132f : boxRect.y + 146f,
                view.IsCompact ? boxRect.width - 180f : boxRect.width - 188f,
                view.IsCompact ? 34f : 36f,
                onActionClicked);

            for (var index = 0; index < view.FooterLines.Length; index++)
            {
                GUI.Label(
                    new Rect(
                        view.IsCompact ? boxRect.x + 42f : boxRect.x + 28f,
                        view.IsCompact ? boxRect.y + 262f + (index * 52f) : boxRect.y + 274f + (index * 32f),
                        view.IsCompact ? boxRect.width - 84f : boxRect.width - 56f,
                        view.IsCompact ? 44f : 32f),
                    view.FooterLines[index],
                    setupHintStyle);
            }
        }

        public void DrawMessageOverlay(string message)
        {
            EnsureStyles();

            var boxRect = new Rect((Screen.width * 0.5f) - 230f, (Screen.height * 0.5f) - 32f, 460f, 64f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(boxRect, message, messageStyle);
        }

        public void DrawDiagnosticsOverlay(BreakoutUiDiagnosticsView view)
        {
            EnsureStyles();

            var overlayHeight = view.Lines.Length <= 3 ? 116f : 144f;
            var overlayRect = new Rect(0f, Screen.height - overlayHeight, Screen.width, overlayHeight);
            DrawSolidRect(overlayRect, new Color(0.03f, 0.04f, 0.08f, 0.94f));

            for (var index = 0; index < view.Lines.Length; index++)
            {
                GUI.Label(
                    new Rect(20f, overlayRect.y + 14f + (index * 28f), overlayRect.width - 40f, 24f),
                    view.Lines[index],
                    hudStyle);
            }
        }

        public void DrawModifierIndicator(BreakoutUiModifierView[] modifiers, bool isDiagnosticsVisible)
        {
            EnsureStyles();

            if (modifiers == null || modifiers.Length == 0)
            {
                return;
            }

            var slotCount = modifiers.Length;
            var slotWidth = 116f;
            var slotSpacing = 8f;
            var panelWidth = Mathf.Min(Screen.width - 32f, 84f + (slotCount * (slotWidth + slotSpacing)));
            var panelHeight = 60f;
            var panelX = Mathf.Clamp((Screen.width - panelWidth) * 0.5f, 8f, Mathf.Max(8f, Screen.width - panelWidth - 8f));
            var bottomMargin = isDiagnosticsVisible ? 156f : 14f;
            var panelY = Mathf.Max(84f, Screen.height - panelHeight - bottomMargin);
            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            DrawSolidRect(panelRect, new Color(0.03f, 0.05f, 0.08f, 0.86f));
            DrawSolidRect(new Rect(panelRect.x + 3f, panelRect.y + 3f, panelRect.width - 6f, panelRect.height - 6f), new Color(1f, 1f, 1f, 0.04f));
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 7f, 72f, 14f), "MODS", modifierPanelTitleStyle);

            var effectX = panelRect.x + 84f;

            for (var index = 0; index < modifiers.Length; index++)
            {
                var modifier = modifiers[index];
                var effectRect = new Rect(effectX, panelRect.y + 9f, slotWidth, 42f);
                var fillRect = new Rect(effectRect.x, effectRect.yMax - 5f, effectRect.width * Mathf.Clamp01(modifier.DurationRatio), 5f);

                DrawSolidRect(effectRect, new Color(modifier.Color.r, modifier.Color.g, modifier.Color.b, 0.16f));
                DrawSolidRect(new Rect(effectRect.x, effectRect.y, effectRect.width, 3f), modifier.Color);
                DrawSolidRect(new Rect(effectRect.x, effectRect.yMax - 5f, effectRect.width, 5f), new Color(1f, 1f, 1f, 0.08f));

                if (fillRect.width > 0.5f)
                {
                    DrawSolidRect(fillRect, modifier.Color);
                }

                GUI.Label(new Rect(effectRect.x + 2f, effectRect.y + 7f, effectRect.width - 4f, 12f), modifier.Label, modifierPanelLabelStyle);
                GUI.Label(new Rect(effectRect.x + 2f, effectRect.y + 20f, effectRect.width - 4f, 12f), $"{modifier.RemainingDuration:0.0}s", modifierPanelTimerStyle);
                effectX += slotWidth + slotSpacing;
            }
        }

        public void DrawPickupBanner(BreakoutUiBannerView view)
        {
            EnsureStyles();

            if (view == null || view.Timer <= 0f || string.IsNullOrWhiteSpace(view.Text))
            {
                return;
            }

            var rect = new Rect((Screen.width * 0.5f) - 170f, 226f, 340f, 36f);
            var previousGuiColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = view.Color;
            GUI.Label(rect, view.Text, pickupStyle);
            GUI.color = previousGuiColor;
        }

        private void EnsureStyles()
        {
            if (hudStyle != null
                && messageStyle != null
                && pickupStyle != null
                && setupTitleStyle != null
                && setupSelectedStyle != null
                && setupHintStyle != null
                && overlayTitleStyle != null
                && overlayBodyStyle != null
                && overlayActionStyle != null
                && overlaySelectedActionStyle != null
                && hudButtonStyle != null
                && hudActiveButtonStyle != null
                && speedMeterCaptionStyle != null
                && speedMeterValueStyle != null
                && modifierPanelTitleStyle != null
                && modifierPanelLabelStyle != null
                && modifierPanelTimerStyle != null
                && modifierPanelEmptyStyle != null)
            {
                return;
            }

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = Color.white },
            };

            messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                normal = { textColor = Color.white },
            };

            pickupStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };

            setupTitleStyle = new GUIStyle(messageStyle)
            {
                fontSize = 28,
            };

            setupSelectedStyle = new GUIStyle(hudStyle)
            {
                fontStyle = FontStyle.Bold,
            };
            setupSelectedStyle.normal.textColor = new Color(1f, 0.92f, 0.58f, 1f);

            setupHintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = new Color(0.88f, 0.9f, 0.96f, 1f) },
            };

            overlayTitleStyle = new GUIStyle(messageStyle)
            {
                fontSize = 30,
            };

            overlayBodyStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };

            overlayActionStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
            };

            overlaySelectedActionStyle = new GUIStyle(overlayActionStyle)
            {
                fontStyle = FontStyle.Bold,
            };
            overlaySelectedActionStyle.normal.textColor = new Color(1f, 0.92f, 0.58f, 1f);

            hudButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 10, 6, 6),
            };

            hudActiveButtonStyle = new GUIStyle(hudButtonStyle);
            hudActiveButtonStyle.normal.textColor = new Color(1f, 0.95f, 0.72f, 1f);

            speedMeterCaptionStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
            };

            speedMeterValueStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
            };

            modifierPanelTitleStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
            };

            modifierPanelLabelStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold,
            };

            modifierPanelTimerStyle = new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };

            modifierPanelEmptyStyle = new GUIStyle(modifierPanelTimerStyle)
            {
                fontStyle = FontStyle.Bold,
            };
            modifierPanelEmptyStyle.normal.textColor = new Color(0.78f, 0.83f, 0.92f, 1f);
        }

        private void DrawBallSpeedMeter(BreakoutUiSpeedMeterView view)
        {
            if (view == null)
            {
                return;
            }

            var panelWidth = 44f;
            var panelHeight = Mathf.Clamp(Screen.height - (view.IsDiagnosticsVisible ? 260f : 204f), 170f, 320f);
            var panelX = Mathf.Clamp(view.BounceZoneLeftScreen - panelWidth - 10f, 8f, Mathf.Max(8f, Screen.width - panelWidth - 8f));
            var panelY = 90f;
            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            var trackRect = new Rect(panelRect.x + 14f, panelRect.y + 34f, 16f, panelRect.height - 78f);
            var fillHeight = Mathf.Lerp(0f, trackRect.height, Mathf.Clamp01(view.SpeedRatio));
            var fillRect = new Rect(trackRect.x, trackRect.yMax - fillHeight, trackRect.width, fillHeight);
            var meterColor = Color.Lerp(new Color(0.18f, 0.72f, 1f, 1f), new Color(1f, 0.28f, 0.16f, 1f), Mathf.Clamp01(view.SpeedRatio));

            DrawSolidRect(panelRect, new Color(0.03f, 0.05f, 0.08f, 0.82f));
            DrawSolidRect(trackRect, new Color(0.1f, 0.14f, 0.2f, 0.95f));

            if (fillHeight > 0.5f)
            {
                DrawSolidRect(fillRect, meterColor);
                DrawSolidRect(
                    new Rect(fillRect.x + 2f, fillRect.y + 2f, Mathf.Max(2f, fillRect.width - 4f), Mathf.Max(2f, fillRect.height - 4f)),
                    new Color(1f, 1f, 1f, 0.12f));
            }

            GUI.Label(new Rect(panelRect.x, panelRect.y + 8f, panelRect.width, 18f), "SPD", speedMeterCaptionStyle);
            GUI.Label(new Rect(panelRect.x - 8f, panelRect.yMax - 36f, panelRect.width + 16f, 18f), $"{view.Speed:0.00}", speedMeterValueStyle);
            GUI.Label(new Rect(panelRect.x - 8f, panelRect.yMax - 20f, panelRect.width + 16f, 16f), "u/s", speedMeterValueStyle);
        }

        private void DrawActionList(string[] labels, int selectedIndex, float x, float y, float width, float lineHeight, Action<int> onActionClicked)
        {
            if (labels == null)
            {
                return;
            }

            for (var index = 0; index < labels.Length; index++)
            {
                var isSelected = index == Mathf.Clamp(selectedIndex, 0, Math.Max(0, labels.Length - 1));
                var actionRect = new Rect(x, y + (lineHeight * index), width, lineHeight);

                if (GUI.Button(actionRect, labels[index], isSelected ? overlaySelectedActionStyle : overlayActionStyle))
                {
                    onActionClicked?.Invoke(index);
                }
            }
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            var previousGuiColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousGuiColor;
        }
    }
}
