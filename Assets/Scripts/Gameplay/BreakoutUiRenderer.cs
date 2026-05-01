using System;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutUiThemePalette
    {
        public Color BackgroundBase = new Color(0.07f, 0.03f, 0.08f, 1f);
        public Color BackgroundGlow = new Color(0.16f, 0.11f, 0.21f, 1f);
        public Color PanelFill = new Color(0.08f, 0.05f, 0.12f, 0.9f);
        public Color PanelFillSecondary = new Color(0.14f, 0.1f, 0.19f, 0.92f);
        public Color BezelDark = new Color(0.02f, 0.02f, 0.03f, 0.92f);
        public Color AccentPrimary = new Color(0.01f, 0.93f, 0.98f, 1f);
        public Color AccentSecondary = new Color(1f, 0.49f, 0.86f, 1f);
        public Color AccentWarm = new Color(1f, 0.87f, 0.36f, 1f);
        public Color Success = new Color(0.45f, 0.95f, 0.72f, 1f);
        public Color Danger = new Color(0.99f, 0.27f, 0.31f, 1f);
        public Color TextPrimary = new Color(0.99f, 0.99f, 1f, 1f);
        public Color TextMuted = new Color(0.78f, 0.83f, 0.92f, 1f);
        public Color ScreenTint = new Color(0.17f, 0.12f, 0.24f, 0.18f);
        public Color Scanline = new Color(1f, 1f, 1f, 0.03f);
    }

    internal sealed class BreakoutUiChromeView
    {
        public Rect PlayfieldRect;
        public bool ShowPlayfieldFrame = true;
        public bool IsMenuLike;
        public bool UseDynamicPlayfieldScanlines;
        public float DynamicScanlineSpacing = 4f;
        public float DynamicScanlineTravelSpeed = 20f;
        public float DynamicScanlineAlpha = 0.026f;
        public float DynamicSweepAlpha = 0.045f;
        public string MarqueeTitle = string.Empty;
        public string MarqueeSubtitle = string.Empty;
    }

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

    internal sealed class BreakoutUiUpgradeDraftOptionView
    {
        public string Title = string.Empty;
        public string Description = string.Empty;
        public string Detail = string.Empty;
        public Color Accent = Color.white;
    }

    internal sealed class BreakoutUiUpgradeDraftView
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string BuildLine = string.Empty;
        public BreakoutUiUpgradeDraftOptionView[] Options = Array.Empty<BreakoutUiUpgradeDraftOptionView>();
        public int SelectedOptionIndex;
        public string HintText = string.Empty;
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

    internal sealed class BreakoutUiFloatingScoreView
    {
        public Vector2 ScreenPosition;
        public string PrimaryText = string.Empty;
        public string SecondaryText = string.Empty;
        public Color Color = Color.white;
        public float Alpha = 1f;
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
        private GUIStyle floatingScoreStyle;
        private GUIStyle floatingScoreTagStyle;
        private BreakoutUiThemePalette palette = new BreakoutUiThemePalette();

        public void ConfigureTheme(BreakoutUiThemePalette themePalette)
        {
            palette = themePalette ?? new BreakoutUiThemePalette();
        }

        public void DrawCabinetBackdrop(BreakoutUiChromeView view)
        {
            EnsureStyles();

            if (view == null)
            {
                return;
            }

            var fullRect = new Rect(0f, 0f, Screen.width, Screen.height);

            if (view.IsMenuLike)
            {
                DrawSolidRect(fullRect, WithAlpha(palette.ScreenTint, 0.24f));
            }

            var topGlowRect = new Rect(0f, 0f, Screen.width, Screen.height * 0.46f);
            var bottomGlowRect = new Rect(0f, Screen.height * 0.42f, Screen.width, Screen.height * 0.58f);
            DrawVerticalGradient(topGlowRect, Color.clear, WithAlpha(palette.AccentSecondary, view.IsMenuLike ? 0.1f : 0.035f), 20);
            DrawVerticalGradient(bottomGlowRect, WithAlpha(palette.AccentWarm, view.IsMenuLike ? 0.14f : 0.055f), Color.clear, 24);

            DrawSolidRect(new Rect(0f, 0f, Screen.width, 14f), WithAlpha(palette.BezelDark, 0.96f));
            DrawSolidRect(new Rect(0f, Screen.height - 16f, Screen.width, 16f), WithAlpha(palette.BezelDark, 0.98f));

            if (view.ShowPlayfieldFrame && view.PlayfieldRect.width > 1f && view.PlayfieldRect.height > 1f)
            {
                DrawOutsidePlayfieldShade(view.PlayfieldRect, view.IsMenuLike);
                DrawPerspectiveGrid(view.PlayfieldRect);
                DrawPlayfieldFrame(view.PlayfieldRect);
                DrawDynamicPlayfieldScanlines(view);
            }

            if (!string.IsNullOrWhiteSpace(view.MarqueeTitle))
            {
                DrawMarquee(view.MarqueeTitle, view.MarqueeSubtitle);
            }

            DrawScanlines(fullRect, 4f, view.IsMenuLike ? palette.Scanline : WithAlpha(palette.Scanline, 0.01f));
        }

        public void DrawMainMenu(BreakoutUiMenuView view, Action<int> onActionClicked)
        {
            EnsureStyles();

            var boxWidth = Mathf.Min(1040f, Screen.width - 56f);
            var boxHeight = Mathf.Min(548f, Screen.height - 150f);

            if (boxWidth < 920f)
            {
                boxWidth = Screen.width - 32f;
            }

            if (boxHeight < 500f)
            {
                boxHeight = Screen.height - 104f;
            }

            var boxRect = new Rect((Screen.width - boxWidth) * 0.5f, (Screen.height - boxHeight) * 0.5f, boxWidth, boxHeight);
            var outerPadding = 34f;
            var contentGap = 24f;
            var headerTop = boxRect.y + 28f;
            var contentTop = boxRect.y + 112f;
            var hintHeight = 42f;
            var footerHeight = 54f;
            var hintY = boxRect.yMax - hintHeight - 18f;
            var footerY = hintY - footerHeight - 14f;
            var contentBottom = footerY - 18f;
            var contentHeight = Mathf.Max(252f, contentBottom - contentTop);
            var leftWidth = Mathf.Clamp(boxRect.width * 0.31f, 292f, 328f);
            var rightWidth = boxRect.width - (outerPadding * 2f) - contentGap - leftWidth;
            var leftRect = new Rect(boxRect.x + outerPadding, contentTop, leftWidth, contentHeight);
            var rightRect = new Rect(leftRect.xMax + contentGap, contentTop, rightWidth, contentHeight);
            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawPanel(leftRect, palette.AccentSecondary, palette.AccentWarm, false);
            DrawPanel(rightRect, palette.AccentPrimary, palette.AccentWarm, false);

            DrawTextWithShadow(new Rect(boxRect.x + outerPadding, headerTop, boxRect.width - (outerPadding * 2f), 40f), view.Title, overlayTitleStyle, palette.TextPrimary);
            DrawTextWithShadow(new Rect(boxRect.x + outerPadding, headerTop + 38f, boxRect.width - (outerPadding * 2f), 36f), view.Subtitle, setupHintStyle, palette.TextMuted, 0.45f);
            DrawSectionLabel(new Rect(leftRect.x + 18f, leftRect.y + 14f, leftRect.width - 36f, 22f), view.SectionTitle, palette.AccentWarm);
            DrawSectionLabel(new Rect(rightRect.x + 18f, rightRect.y + 14f, rightRect.width - 36f, 22f), view.PreviewTitle, palette.AccentPrimary);

            DrawActionList(view.ActionLabels, view.SelectedActionIndex, leftRect.x + 18f, leftRect.y + 52f, leftRect.width - 36f, 44f, onActionClicked);

            for (var index = 0; index < view.PreviewLines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(rightRect.x + 20f, rightRect.y + 50f + (index * 34f), rightRect.width - 40f, 28f),
                    view.PreviewLines[index],
                    hudStyle,
                    palette.TextPrimary,
                    0.35f);
            }

            var validationY = rightRect.y + 54f + (view.PreviewLines.Length * 34f);
            var validationHeight = Mathf.Max(46f, rightRect.yMax - validationY - 16f);
            DrawTextWithShadow(new Rect(rightRect.x + 18f, validationY, rightRect.width - 36f, validationHeight), view.ValidationText, setupHintStyle, palette.TextMuted, 0.35f);
            DrawTextWithShadow(new Rect(boxRect.x + outerPadding, footerY, boxRect.width - (outerPadding * 2f), footerHeight), view.FooterText, setupHintStyle, palette.TextMuted, 0.35f);
            DrawHintBand(new Rect(boxRect.x + 28f, hintY, boxRect.width - 56f, hintHeight), view.HintText);
        }

        public void DrawRunSetup(BreakoutUiRunSetupView view)
        {
            EnsureStyles();

            var boxWidth = Mathf.Min(1040f, Screen.width - 56f);
            var boxHeight = Mathf.Min(620f, Screen.height - 140f);

            if (boxWidth < 900f)
            {
                boxWidth = Screen.width - 32f;
            }

            if (boxHeight < 548f)
            {
                boxHeight = Screen.height - 96f;
            }

            var boxRect = new Rect((Screen.width - boxWidth) * 0.5f, (Screen.height - boxHeight) * 0.5f, boxWidth, boxHeight);
            var outerPadding = 34f;
            var contentGap = 28f;
            var headerTop = boxRect.y + 26f;
            var contentTop = boxRect.y + 116f;
            var hintHeight = 58f;
            var contentHeight = Mathf.Max(314f, boxRect.height - 212f);
            var listWidth = Mathf.Clamp(boxRect.width * 0.56f, 458f, 562f);
            var previewWidth = boxRect.width - (outerPadding * 2f) - contentGap - listWidth;
            var listRect = new Rect(boxRect.x + outerPadding, contentTop, listWidth, contentHeight);
            var previewRect = new Rect(listRect.xMax + contentGap, contentTop, previewWidth, contentHeight);
            var hintY = boxRect.yMax - hintHeight - 24f;
            DrawPanel(boxRect, palette.AccentPrimary, palette.AccentSecondary, true);

            DrawTextWithShadow(new Rect(boxRect.x + 30f, headerTop, boxRect.width - 60f, 40f), view.Title, setupTitleStyle, palette.TextPrimary);
            DrawTextWithShadow(new Rect(boxRect.x + 30f, headerTop + 40f, boxRect.width - 60f, 36f), view.Subtitle, setupHintStyle, palette.TextMuted, 0.4f);
            DrawPanel(listRect, palette.AccentSecondary, palette.AccentPrimary, false);
            DrawPanel(previewRect, palette.AccentWarm, palette.AccentPrimary, false);
            DrawSectionLabel(new Rect(listRect.x + 16f, listRect.y + 12f, listRect.width - 32f, 20f), "CONFIG", palette.AccentWarm);
            DrawSectionLabel(new Rect(previewRect.x + 16f, previewRect.y + 12f, previewRect.width - 32f, 20f), "SYSTEM READOUT", palette.AccentPrimary);

            var fieldX = listRect.x + 16f;
            var fieldWidth = listRect.width - 32f;
            var lineHeight = Mathf.Clamp((listRect.height - 56f) / Mathf.Max(1, view.FieldLines.Length), 30f, 38f);
            var startY = listRect.y + 44f;

            for (var index = 0; index < view.FieldLines.Length; index++)
            {
                var isSelected = index == Mathf.Clamp(view.SelectedFieldIndex, 0, Math.Max(0, view.FieldLines.Length - 1));
                var lineRect = new Rect(fieldX, startY + (lineHeight * index), fieldWidth, lineHeight - 4f);

                if (isSelected)
                {
                    DrawSelectionBar(new Rect(lineRect.x - 4f, lineRect.y - 2f, lineRect.width + 8f, lineRect.height + 4f));
                }

                DrawTextWithShadow(
                    lineRect,
                    $"{(isSelected ? ">" : " ")} {view.FieldLines[index]}",
                    isSelected ? setupSelectedStyle : hudStyle,
                    isSelected ? palette.TextPrimary : palette.TextMuted,
                    0.3f);
            }

            var previewBodyHeight = Mathf.Max(116f, previewRect.height * 0.48f);
            var previewBodyRect = new Rect(previewRect.x + 16f, previewRect.y + 50f, previewRect.width - 32f, previewBodyHeight);
            var validationRect = new Rect(previewRect.x + 16f, previewBodyRect.yMax + 14f, previewRect.width - 32f, previewRect.yMax - previewBodyRect.yMax - 26f);
            DrawTextWithShadow(previewBodyRect, view.PreviewLine, overlayBodyStyle, palette.TextPrimary, 0.3f);
            DrawTextWithShadow(validationRect, view.ValidationText, setupHintStyle, palette.TextMuted, 0.3f);
            DrawHintBand(new Rect(boxRect.x + 30f, hintY, boxRect.width - 60f, hintHeight), view.HintText);
        }

        public void DrawGameplayHud(BreakoutUiHudView view, Action onToggleDiagnostics, Action onToggleMenu)
        {
            EnsureStyles();

            var statusRect = new Rect(18f, 18f, Mathf.Max(320f, Screen.width - 320f), 72f);
            var buttonsX = Screen.width - 258f;
            var buttonsY = 20f;
            var diagnosticsLabel = view.IsDiagnosticsVisible ? "DBG ON" : "DBG";
            var menuLabel = view.IsPaused ? "RESUME" : "MENU";

            DrawPanel(statusRect, palette.AccentPrimary, palette.AccentSecondary, false);
            DrawTextWithShadow(new Rect(statusRect.x + 20f, statusRect.y + 14f, statusRect.width - 40f, 24f), view.TopLine, hudStyle, palette.TextPrimary, 0.35f);
            DrawTextWithShadow(new Rect(statusRect.x + 20f, statusRect.y + 40f, statusRect.width - 40f, 22f), view.BottomLine, overlayBodyStyle, palette.TextMuted, 0.3f);
            DrawBallSpeedMeter(view.SpeedMeter);

            if (DrawArcadeButton(new Rect(buttonsX, buttonsY, 122f, 42f), diagnosticsLabel, view.IsDiagnosticsVisible))
            {
                onToggleDiagnostics?.Invoke();
            }

            if (view.ShowMenuButton && DrawArcadeButton(new Rect(buttonsX + 130f, buttonsY, 110f, 42f), menuLabel, view.IsPaused))
            {
                onToggleMenu?.Invoke();
            }
        }

        public void DrawOverlay(BreakoutUiOverlayView view, Action<int> onActionClicked)
        {
            EnsureStyles();

            var boxWidth = view.IsCompact ? 780f : 760f;
            var minBoxHeight = view.IsCompact ? 470f : 404f;
            var boxHeight = Mathf.Max(
                minBoxHeight,
                164f + (view.SummaryLines.Length * 30f) + (view.ActionLabels.Length * 44f) + (view.FooterLines.Length * 38f));
            boxWidth = Mathf.Min(boxWidth, Screen.width - 56f);
            boxHeight = Mathf.Min(boxHeight, Screen.height - 132f);

            if (boxWidth < 680f)
            {
                boxWidth = Screen.width - 32f;
            }

            var boxRect = new Rect((Screen.width - boxWidth) * 0.5f, (Screen.height - boxHeight) * 0.5f, boxWidth, boxHeight);
            var outerPadding = 30f;
            var titleY = boxRect.y + 24f;
            var summaryY = boxRect.y + 72f;
            var summarySpacing = 28f;
            var actionStartY = summaryY + (view.SummaryLines.Length * summarySpacing) + 18f;
            var actionLineHeight = 44f;
            var footerStartY = actionStartY + (view.ActionLabels.Length * actionLineHeight) + 18f;
            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + outerPadding, titleY, boxRect.width - (outerPadding * 2f), 40f), view.Title, overlayTitleStyle, palette.TextPrimary);

            for (var index = 0; index < view.SummaryLines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(boxRect.x + 36f, summaryY + (index * summarySpacing), boxRect.width - 72f, 24f),
                    view.SummaryLines[index],
                    overlayBodyStyle,
                    index == 0 ? palette.TextPrimary : palette.TextMuted,
                    0.35f);
            }

            DrawActionList(
                view.ActionLabels,
                view.SelectedActionIndex,
                boxRect.x + 88f,
                actionStartY,
                boxRect.width - 176f,
                actionLineHeight,
                onActionClicked);

            for (var index = 0; index < view.FooterLines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(boxRect.x + 34f, footerStartY + (index * 34f), boxRect.width - 68f, 34f),
                    view.FooterLines[index],
                    setupHintStyle,
                    palette.TextMuted,
                    0.3f);
            }
        }

        public void DrawUpgradeDraft(BreakoutUiUpgradeDraftView view, Action<int> onOptionClicked)
        {
            EnsureStyles();

            if (view == null || view.Options == null || view.Options.Length == 0)
            {
                return;
            }

            var boxRect = new Rect((Screen.width * 0.5f) - 520f, (Screen.height * 0.5f) - 266f, 1040f, 532f);
            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + 30f, boxRect.y + 22f, boxRect.width - 60f, 40f), view.Title, overlayTitleStyle, palette.TextPrimary);
            DrawTextWithShadow(new Rect(boxRect.x + 30f, boxRect.y + 64f, boxRect.width - 60f, 26f), view.Subtitle, setupHintStyle, palette.TextMuted, 0.35f);
            DrawTextWithShadow(new Rect(boxRect.x + 30f, boxRect.y + 96f, boxRect.width - 60f, 26f), view.BuildLine, hudStyle, palette.TextPrimary, 0.25f);

            var optionCount = view.Options.Length;
            var optionWidth = 300f;
            var optionSpacing = 20f;
            var totalWidth = (optionCount * optionWidth) + ((optionCount - 1) * optionSpacing);
            var startX = boxRect.x + ((boxRect.width - totalWidth) * 0.5f);
            var optionY = boxRect.y + 142f;

            for (var index = 0; index < optionCount; index++)
            {
                var option = view.Options[index];
                var isSelected = index == Mathf.Clamp(view.SelectedOptionIndex, 0, optionCount - 1);
                var optionRect = new Rect(startX + (index * (optionWidth + optionSpacing)), optionY, optionWidth, 246f);
                var accent = option.Accent;
                accent.a = 1f;

                DrawPanel(optionRect, accent, isSelected ? palette.AccentWarm : palette.AccentPrimary, false, isSelected ? 2.4f : 1.5f);

                if (isSelected)
                {
                    DrawSelectionBar(new Rect(optionRect.x - 4f, optionRect.y - 4f, optionRect.width + 8f, optionRect.height + 8f));
                }

                if (GUI.Button(optionRect, GUIContent.none, GUIStyle.none))
                {
                    onOptionClicked?.Invoke(index);
                }

                DrawSectionLabel(new Rect(optionRect.x + 16f, optionRect.y + 14f, optionRect.width - 32f, 18f), $"PICK {index + 1}", accent);
                DrawTextWithShadow(new Rect(optionRect.x + 16f, optionRect.y + 42f, optionRect.width - 32f, 52f), option.Title, overlayActionStyle, palette.TextPrimary, 0.3f);
                DrawTextWithShadow(new Rect(optionRect.x + 16f, optionRect.y + 100f, optionRect.width - 32f, 92f), option.Description, overlayBodyStyle, palette.TextMuted, 0.22f);
                DrawTextWithShadow(new Rect(optionRect.x + 16f, optionRect.y + 200f, optionRect.width - 32f, 28f), option.Detail, setupHintStyle, palette.TextPrimary, 0.25f);
            }

            DrawHintBand(new Rect(boxRect.x + 30f, boxRect.y + 418f, boxRect.width - 60f, 72f), view.HintText);
        }

        public void DrawMessageOverlay(string message)
        {
            EnsureStyles();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var boxRect = new Rect((Screen.width * 0.5f) - 274f, (Screen.height * 0.5f) - 42f, 548f, 84f);
            DrawPanel(boxRect, palette.AccentWarm, palette.AccentSecondary, false);
            DrawTextWithShadow(boxRect, message, messageStyle, palette.TextPrimary);
        }

        public void DrawDiagnosticsOverlay(BreakoutUiDiagnosticsView view)
        {
            EnsureStyles();

            if (view == null || view.Lines.Length == 0)
            {
                return;
            }

            var overlayHeight = Mathf.Max(124f, 52f + (view.Lines.Length * 30f));
            var overlayRect = new Rect(18f, Screen.height - overlayHeight - 18f, Screen.width - 36f, overlayHeight);
            DrawPanel(overlayRect, palette.AccentPrimary, palette.AccentWarm, false);
            DrawSectionLabel(new Rect(overlayRect.x + 16f, overlayRect.y + 10f, 180f, 18f), "DIAGNOSTICS", palette.AccentPrimary);

            for (var index = 0; index < view.Lines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(overlayRect.x + 20f, overlayRect.y + 34f + (index * 28f), overlayRect.width - 40f, 24f),
                    view.Lines[index],
                    hudStyle,
                    palette.TextMuted,
                    0.25f);
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
            var slotWidth = 128f;
            var slotSpacing = 10f;
            var panelWidth = Mathf.Min(Screen.width - 36f, 98f + (slotCount * (slotWidth + slotSpacing)));
            var panelHeight = 72f;
            var panelX = Mathf.Clamp((Screen.width - panelWidth) * 0.5f, 18f, Mathf.Max(18f, Screen.width - panelWidth - 18f));
            var bottomMargin = isDiagnosticsVisible ? 182f : 20f;
            var panelY = Mathf.Max(100f, Screen.height - panelHeight - bottomMargin);
            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            DrawPanel(panelRect, palette.AccentPrimary, palette.AccentSecondary, false);
            DrawSectionLabel(new Rect(panelRect.x + 14f, panelRect.y + 9f, 74f, 14f), "MODS", palette.AccentWarm);

            var effectX = panelRect.x + 92f;

            for (var index = 0; index < modifiers.Length; index++)
            {
                var modifier = modifiers[index];
                var effectRect = new Rect(effectX, panelRect.y + 12f, slotWidth, 46f);
                var fillRect = new Rect(effectRect.x + 6f, effectRect.yMax - 9f, (effectRect.width - 12f) * Mathf.Clamp01(modifier.DurationRatio), 4f);

                DrawPanel(effectRect, modifier.Color, palette.AccentPrimary, false, 1.5f);
                DrawSolidRect(new Rect(effectRect.x + 6f, effectRect.yMax - 9f, effectRect.width - 12f, 4f), WithAlpha(palette.TextPrimary, 0.08f));

                if (fillRect.width > 0.5f)
                {
                    DrawSolidRect(fillRect, modifier.Color);
                }

                DrawTextWithShadow(new Rect(effectRect.x + 4f, effectRect.y + 8f, effectRect.width - 8f, 12f), modifier.Label, modifierPanelLabelStyle, palette.TextPrimary, 0.25f);
                DrawTextWithShadow(new Rect(effectRect.x + 4f, effectRect.y + 22f, effectRect.width - 8f, 12f), $"{modifier.RemainingDuration:0.0}s", modifierPanelTimerStyle, palette.TextMuted, 0.2f);
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

            var rect = new Rect((Screen.width * 0.5f) - 212f, 128f, 424f, 42f);
            DrawPanel(rect, view.Color, palette.AccentWarm, false, 1.5f);
            DrawTextWithShadow(rect, view.Text, pickupStyle, view.Color, 0.3f);
        }

        public void DrawFloatingScorePopups(BreakoutUiFloatingScoreView[] views)
        {
            EnsureStyles();

            if (views == null || views.Length == 0)
            {
                return;
            }

            for (var index = 0; index < views.Length; index++)
            {
                var view = views[index];

                if (view == null || view.Alpha <= 0.001f || string.IsNullOrWhiteSpace(view.PrimaryText))
                {
                    continue;
                }

                var position = view.ScreenPosition;
                var primaryRect = new Rect(position.x - 96f, position.y - 18f, 192f, 30f);
                var secondaryRect = new Rect(position.x - 176f, position.y + 10f, 352f, 22f);
                var scoreColor = WithAlpha(view.Color, view.Alpha);
                var tagColor = WithAlpha(Color.Lerp(palette.AccentSecondary, palette.AccentPrimary, 0.45f), view.Alpha * 0.92f);

                DrawTextWithShadow(primaryRect, view.PrimaryText, floatingScoreStyle, scoreColor, 0.22f * view.Alpha);

                if (!string.IsNullOrWhiteSpace(view.SecondaryText))
                {
                    DrawTextWithShadow(secondaryRect, view.SecondaryText, floatingScoreTagStyle, tagColor, 0.18f * view.Alpha);
                }
            }
        }

        private void EnsureStyles()
        {
            hudStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
            };
            messageStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            pickupStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
            };
            setupTitleStyle ??= new GUIStyle(messageStyle)
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleLeft,
            };
            setupSelectedStyle ??= new GUIStyle(hudStyle)
            {
                fontStyle = FontStyle.Bold,
            };
            setupHintStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                wordWrap = true,
            };
            overlayTitleStyle ??= new GUIStyle(messageStyle)
            {
                fontSize = 32,
            };
            overlayBodyStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
            };
            overlayActionStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
            };
            overlaySelectedActionStyle ??= new GUIStyle(overlayActionStyle);
            hudButtonStyle ??= new GUIStyle(overlayActionStyle)
            {
                fontSize = 14,
            };
            hudActiveButtonStyle ??= new GUIStyle(hudButtonStyle);
            speedMeterCaptionStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
            };
            speedMeterValueStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
            };
            modifierPanelTitleStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
            };
            modifierPanelLabelStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold,
            };
            modifierPanelTimerStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };
            floatingScoreStyle ??= new GUIStyle(pickupStyle)
            {
                fontSize = 26,
            };
            floatingScoreTagStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
            };

            hudStyle.normal.textColor = palette.TextPrimary;
            messageStyle.normal.textColor = palette.TextPrimary;
            pickupStyle.normal.textColor = palette.TextPrimary;
            setupTitleStyle.normal.textColor = palette.TextPrimary;
            setupSelectedStyle.normal.textColor = palette.TextPrimary;
            setupHintStyle.normal.textColor = palette.TextMuted;
            overlayTitleStyle.normal.textColor = palette.TextPrimary;
            overlayBodyStyle.normal.textColor = palette.TextMuted;
            overlayActionStyle.normal.textColor = palette.TextMuted;
            overlaySelectedActionStyle.normal.textColor = palette.TextPrimary;
            hudButtonStyle.normal.textColor = palette.TextMuted;
            hudActiveButtonStyle.normal.textColor = palette.TextPrimary;
            speedMeterCaptionStyle.normal.textColor = palette.TextPrimary;
            speedMeterValueStyle.normal.textColor = palette.TextMuted;
            modifierPanelTitleStyle.normal.textColor = palette.TextMuted;
            modifierPanelLabelStyle.normal.textColor = palette.TextPrimary;
            modifierPanelTimerStyle.normal.textColor = palette.TextMuted;
            floatingScoreStyle.normal.textColor = palette.AccentWarm;
            floatingScoreTagStyle.normal.textColor = palette.AccentPrimary;
        }

        private void DrawBallSpeedMeter(BreakoutUiSpeedMeterView view)
        {
            if (view == null)
            {
                return;
            }

            var panelWidth = 56f;
            var panelHeight = Mathf.Clamp(Screen.height - (view.IsDiagnosticsVisible ? 286f : 216f), 184f, 334f);
            var panelX = Mathf.Clamp(view.BounceZoneLeftScreen - panelWidth - 18f, 12f, Mathf.Max(12f, Screen.width - panelWidth - 12f));
            var panelY = 104f;
            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            var trackRect = new Rect(panelRect.x + 17f, panelRect.y + 38f, 22f, panelRect.height - 92f);
            var fillHeight = Mathf.Lerp(0f, trackRect.height, Mathf.Clamp01(view.SpeedRatio));
            var fillRect = new Rect(trackRect.x + 3f, trackRect.yMax - fillHeight + 3f, trackRect.width - 6f, Mathf.Max(0f, fillHeight - 6f));
            var meterColor = Color.Lerp(palette.AccentPrimary, palette.AccentWarm, Mathf.Clamp01(view.SpeedRatio));

            DrawPanel(panelRect, palette.AccentPrimary, palette.AccentWarm, false);
            DrawSolidRect(trackRect, WithAlpha(palette.BezelDark, 0.95f));
            DrawOutline(trackRect, WithAlpha(palette.TextPrimary, 0.08f), 1f);

            if (fillRect.height > 1f)
            {
                DrawSolidRect(fillRect, meterColor);
            }

            DrawTextWithShadow(new Rect(panelRect.x, panelRect.y + 10f, panelRect.width, 18f), "SPD", speedMeterCaptionStyle, palette.TextPrimary, 0.25f);
            DrawTextWithShadow(new Rect(panelRect.x - 10f, panelRect.yMax - 38f, panelRect.width + 20f, 18f), $"{view.Speed:0.00}", speedMeterValueStyle, palette.TextPrimary, 0.2f);
            DrawTextWithShadow(new Rect(panelRect.x - 10f, panelRect.yMax - 22f, panelRect.width + 20f, 16f), "u/s", speedMeterValueStyle, palette.TextMuted, 0.2f);
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
                var actionRect = new Rect(x, y + (lineHeight * index), width, lineHeight - 4f);

                if (DrawArcadeButton(actionRect, labels[index], isSelected))
                {
                    onActionClicked?.Invoke(index);
                }
            }
        }

        private bool DrawArcadeButton(Rect rect, string label, bool isSelected)
        {
            var leftAccent = isSelected ? palette.AccentSecondary : WithAlpha(palette.AccentSecondary, 0.45f);
            var rightAccent = isSelected ? palette.AccentPrimary : WithAlpha(palette.AccentPrimary, 0.45f);
            DrawPanel(rect, leftAccent, rightAccent, isSelected, 1.5f);

            if (isSelected)
            {
                DrawSolidRect(new Rect(rect.x + 8f, rect.y + rect.height - 7f, rect.width - 16f, 3f), palette.AccentWarm);
            }

            DrawTextWithShadow(
                rect,
                ToArcadeLabel(label),
                isSelected ? overlaySelectedActionStyle : overlayActionStyle,
                isSelected ? palette.TextPrimary : palette.TextMuted,
                0.25f);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        private void DrawPanel(Rect rect, Color leftAccent, Color rightAccent, bool emphasize, float borderThickness = 2f)
        {
            var glowRect = Inflate(rect, emphasize ? 10f : 6f);
            DrawSolidRect(glowRect, WithAlpha(Color.Lerp(leftAccent, rightAccent, 0.5f), emphasize ? 0.12f : 0.05f));
            DrawSolidRect(rect, palette.PanelFill);
            DrawSolidRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), palette.PanelFillSecondary);
            DrawHorizontalGradient(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 4f), leftAccent, rightAccent, 18);
            DrawOutline(rect, WithAlpha(palette.TextPrimary, 0.1f), borderThickness);
            DrawOutline(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f), WithAlpha(rightAccent, emphasize ? 0.28f : 0.12f), 1f);
            DrawCornerBrackets(rect, leftAccent, rightAccent);
        }

        private void DrawPlayfieldFrame(Rect rect)
        {
            var outer = Inflate(rect, 22f);
            var innerBezel = Inflate(rect, 12f);
            var glowBand = Inflate(rect, 8f);
            DrawFrameBands(outer, innerBezel, WithAlpha(palette.BezelDark, 0.86f));
            DrawOutline(outer, WithAlpha(palette.TextPrimary, 0.04f), 2f);

            DrawFrameBands(innerBezel, glowBand, WithAlpha(palette.BackgroundGlow, 0.28f));
            DrawOutline(glowBand, WithAlpha(palette.AccentSecondary, 0.18f), 2f);
            DrawOutline(rect, WithAlpha(palette.AccentPrimary, 0.36f), 2f);

            DrawHorizontalGradient(new Rect(glowBand.x + 8f, glowBand.y + 8f, glowBand.width - 16f, 3f), palette.AccentSecondary, palette.AccentPrimary, 24);
            DrawVerticalGradient(new Rect(rect.x, rect.y, rect.width, 12f), WithAlpha(palette.TextPrimary, 0.035f), Color.clear, 8);
        }

        private void DrawOutsidePlayfieldShade(Rect playfieldRect, bool isMenuLike)
        {
            var alpha = isMenuLike ? 0.46f : 0.28f;
            DrawSolidRect(new Rect(0f, 0f, Screen.width, Mathf.Max(0f, playfieldRect.y - 20f)), WithAlpha(palette.BezelDark, alpha));
            DrawSolidRect(new Rect(0f, playfieldRect.yMax + 20f, Screen.width, Mathf.Max(0f, Screen.height - playfieldRect.yMax - 20f)), WithAlpha(palette.BezelDark, alpha + 0.06f));
            DrawSolidRect(new Rect(0f, playfieldRect.y - 20f, Mathf.Max(0f, playfieldRect.x - 20f), playfieldRect.height + 40f), WithAlpha(palette.BezelDark, alpha + 0.04f));
            DrawSolidRect(new Rect(playfieldRect.xMax + 20f, playfieldRect.y - 20f, Mathf.Max(0f, Screen.width - playfieldRect.xMax - 20f), playfieldRect.height + 40f), WithAlpha(palette.BezelDark, alpha + 0.04f));
        }

        private void DrawPerspectiveGrid(Rect playfieldRect)
        {
            var horizonY = Mathf.Min(Screen.height - 120f, playfieldRect.yMax + 56f);
            var gridBottom = Screen.height - 18f;
            var centerX = Screen.width * 0.5f;

            DrawHorizontalGradient(new Rect(0f, horizonY - 3f, Screen.width, 3f), WithAlpha(palette.AccentSecondary, 0f), WithAlpha(palette.AccentPrimary, 0.28f), 30);

            for (var index = 0; index < 9; index++)
            {
                var t = (index + 1f) / 9f;
                var y = Mathf.Lerp(horizonY + 12f, gridBottom, t * t);
                DrawSolidRect(new Rect(0f, y, Screen.width, 1f), WithAlpha(palette.AccentPrimary, Mathf.Lerp(0.03f, 0.12f, t)));
            }

            for (var index = -7; index <= 7; index++)
            {
                var xBottom = centerX + (index * 84f);
                DrawSolidRect(new Rect(xBottom, horizonY, 1f, gridBottom - horizonY), WithAlpha(palette.AccentSecondary, 0.03f + (0.01f * Mathf.Abs(index))));
            }
        }

        private void DrawMarquee(string title, string subtitle)
        {
            var rect = new Rect((Screen.width * 0.5f) - 290f, 26f, 580f, 72f);
            DrawPanel(rect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(rect.x, rect.y + 8f, rect.width, 28f), ToArcadeLabel(title), overlayTitleStyle, palette.TextPrimary, 0.35f);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                DrawTextWithShadow(new Rect(rect.x, rect.y + 40f, rect.width, 18f), ToArcadeLabel(subtitle), speedMeterValueStyle, palette.TextMuted, 0.2f);
            }
        }

        private void DrawHintBand(Rect rect, string text)
        {
            DrawPanel(rect, palette.AccentPrimary, palette.AccentSecondary, false, 1f);
            DrawTextWithShadow(rect, text, setupHintStyle, palette.TextMuted, 0.2f);
        }

        private void DrawSectionLabel(Rect rect, string text, Color accent)
        {
            DrawTextWithShadow(rect, ToArcadeLabel(text), speedMeterCaptionStyle, accent, 0.25f);
        }

        private void DrawSelectionBar(Rect rect)
        {
            DrawSolidRect(rect, WithAlpha(palette.AccentSecondary, 0.16f));
            DrawHorizontalGradient(new Rect(rect.x, rect.y, rect.width, 3f), palette.AccentSecondary, palette.AccentPrimary, 20);
            DrawOutline(rect, WithAlpha(palette.TextPrimary, 0.08f), 1f);
        }

        private static Rect Inflate(Rect rect, float amount)
        {
            return new Rect(rect.x - amount, rect.y - amount, rect.width + (amount * 2f), rect.height + (amount * 2f));
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static string ToArcadeLabel(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.ToUpperInvariant();
        }

        private void DrawTextWithShadow(Rect rect, string text, GUIStyle style, Color color, float shadowAlpha = 0.5f)
        {
            var previousGuiColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, shadowAlpha);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 2f, rect.width, rect.height), text, style);
            GUI.color = color;
            GUI.Label(rect, text, style);
            GUI.color = previousGuiColor;
        }

        private void DrawCornerBrackets(Rect rect, Color leftAccent, Color rightAccent)
        {
            const float length = 14f;
            const float thickness = 2f;

            DrawSolidRect(new Rect(rect.x, rect.y, length, thickness), leftAccent);
            DrawSolidRect(new Rect(rect.x, rect.y, thickness, length), leftAccent);
            DrawSolidRect(new Rect(rect.xMax - length, rect.y, length, thickness), rightAccent);
            DrawSolidRect(new Rect(rect.xMax - thickness, rect.y, thickness, length), rightAccent);
            DrawSolidRect(new Rect(rect.x, rect.yMax - thickness, length, thickness), leftAccent);
            DrawSolidRect(new Rect(rect.x, rect.yMax - length, thickness, length), leftAccent);
            DrawSolidRect(new Rect(rect.xMax - length, rect.yMax - thickness, length, thickness), rightAccent);
            DrawSolidRect(new Rect(rect.xMax - thickness, rect.yMax - length, thickness, length), rightAccent);
        }

        private void DrawVerticalGradient(Rect rect, Color topColor, Color bottomColor, int steps)
        {
            if (steps <= 0)
            {
                DrawSolidRect(rect, topColor);
                return;
            }

            var stepHeight = rect.height / steps;

            for (var index = 0; index < steps; index++)
            {
                var t = steps == 1 ? 0f : index / (float)(steps - 1);
                var color = Color.Lerp(topColor, bottomColor, t);
                DrawSolidRect(new Rect(rect.x, rect.y + (stepHeight * index), rect.width, stepHeight + 1f), color);
            }
        }

        private void DrawHorizontalGradient(Rect rect, Color leftColor, Color rightColor, int steps)
        {
            if (steps <= 0)
            {
                DrawSolidRect(rect, leftColor);
                return;
            }

            var stepWidth = rect.width / steps;

            for (var index = 0; index < steps; index++)
            {
                var t = steps == 1 ? 0f : index / (float)(steps - 1);
                var color = Color.Lerp(leftColor, rightColor, t);
                DrawSolidRect(new Rect(rect.x + (stepWidth * index), rect.y, stepWidth + 1f, rect.height), color);
            }
        }

        private void DrawOutline(Rect rect, Color color, float thickness)
        {
            DrawSolidRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawSolidRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawSolidRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawSolidRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void DrawScanlines(Rect rect, float spacing, Color color)
        {
            if (spacing <= 1f)
            {
                return;
            }

            for (var y = rect.y; y < rect.yMax; y += spacing)
            {
                DrawSolidRect(new Rect(rect.x, y, rect.width, 1f), color);
            }
        }

        private void DrawDynamicPlayfieldScanlines(BreakoutUiChromeView view)
        {
            if (view == null || !view.UseDynamicPlayfieldScanlines)
            {
                return;
            }

            var spacing = Mathf.Max(2f, view.DynamicScanlineSpacing);
            var rect = new Rect(
                view.PlayfieldRect.x + 3f,
                view.PlayfieldRect.y + 3f,
                Mathf.Max(0f, view.PlayfieldRect.width - 6f),
                Mathf.Max(0f, view.PlayfieldRect.height - 6f));

            if (rect.width < 2f || rect.height < 2f)
            {
                return;
            }

            var time = Time.unscaledTime;
            var phaseOffset = Mathf.Repeat(time * view.DynamicScanlineTravelSpeed, spacing);
            var baseColor = WithAlpha(palette.Scanline, view.DynamicScanlineAlpha);
            var sweepColor = WithAlpha(palette.AccentPrimary, view.DynamicSweepAlpha);

            for (var y = rect.y - spacing + phaseOffset; y < rect.yMax + spacing; y += spacing)
            {
                var normalized = Mathf.InverseLerp(rect.y, rect.yMax, y);
                var shimmer = 0.6f + (0.4f * Mathf.Sin((normalized * 17f) - (time * 7.5f)));
                var color = WithAlpha(baseColor, baseColor.a * shimmer);
                DrawSolidRect(new Rect(rect.x, y, rect.width, 1f), color);
            }

            DrawScanlineSweep(rect, time, view.DynamicScanlineTravelSpeed, sweepColor);
        }

        private void DrawScanlineSweep(Rect rect, float time, float travelSpeed, Color color)
        {
            var sweepHeight = Mathf.Clamp(rect.height * 0.14f, 28f, 72f);
            var travel = rect.height + (sweepHeight * 2f);
            var sweepTop = rect.y - sweepHeight + Mathf.Repeat(time * (travelSpeed * 2.4f), travel);
            var bandCount = Mathf.CeilToInt(sweepHeight);

            for (var index = 0; index < bandCount; index++)
            {
                var y = sweepTop + index;

                if (y < rect.y || y >= rect.yMax)
                {
                    continue;
                }

                var distance = Mathf.Abs(index - (bandCount * 0.5f)) / Mathf.Max(1f, bandCount * 0.5f);
                var falloff = 1f - Mathf.Clamp01(distance);
                DrawSolidRect(new Rect(rect.x, y, rect.width, 1f), WithAlpha(color, color.a * falloff));
            }
        }

        private void DrawFrameBands(Rect outer, Rect inner, Color color)
        {
            DrawSolidRect(new Rect(outer.x, outer.y, outer.width, Mathf.Max(0f, inner.y - outer.y)), color);
            DrawSolidRect(new Rect(outer.x, inner.yMax, outer.width, Mathf.Max(0f, outer.yMax - inner.yMax)), color);
            DrawSolidRect(new Rect(outer.x, inner.y, Mathf.Max(0f, inner.x - outer.x), inner.height), color);
            DrawSolidRect(new Rect(inner.xMax, inner.y, Mathf.Max(0f, outer.xMax - inner.xMax), inner.height), color);
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
