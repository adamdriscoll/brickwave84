using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutUiRenderer
    {
        private const int RetroUiFontSize = 18;
        private const int IconTextureSize = 256;
        private const float SplitOverlaySummaryMinHeight = 26f;
        private const float SplitOverlaySummaryGap = 4f;
        private const float SplitOverlayFooterMinHeight = 30f;
        private const float SplitOverlayFooterGap = 4f;
        private const float SplitOverlayActionLineHeight = 42f;
        private const float BrickCounterPulseDuration = 0.55f;
        internal const float LifeLossAnimationDuration = 1.05f;
        private const float ScorePopDuration = 0.34f;
        private const int SpeedGaugeTickCount = 48;
        private const float SpeedGaugeStartAngle = -142f;
        private const float SpeedGaugeSweepAngle = 284f;
        private static readonly string[] RetroUiFontNames = { "Consolas", "Courier New", "monospace" };

        private readonly Dictionary<Sprite, Texture2D> iconTextureCache = new Dictionary<Sprite, Texture2D>();
        private Vector2 progressionContentScroll;
        private Vector2 statsTableScroll;
        private int previousBrickCounterValue = -1;
        private float brickCounterPulseStartTime = -100f;
        private int previousHudScoreValue = int.MinValue;
        private float scorePopStartTime = -100f;
        private int previousHudLifeCount = -1;
        private LifeLossHudAnimation lifeLossAnimation;

        private GUIStyle hudStyle;
        private GUIStyle messageStyle;
        private GUIStyle pickupStyle;
        private GUIStyle setupTitleStyle;
        private GUIStyle setupSelectedStyle;
        private GUIStyle setupHintStyle;
        private GUIStyle overlayTitleStyle;
        private GUIStyle overlayBodyStyle;
        private GUIStyle overlayMetricStyle;
        private GUIStyle overlayActionStyle;
        private GUIStyle overlaySelectedActionStyle;
        private GUIStyle draftOptionTitleStyle;
        private GUIStyle draftOptionBodyStyle;
        private GUIStyle draftOptionDetailStyle;
        private GUIStyle draftHintStyle;
        private GUIStyle hudButtonStyle;
        private GUIStyle hudActiveButtonStyle;
        private GUIStyle speedMeterCaptionStyle;
        private GUIStyle speedMeterValueStyle;
        private GUIStyle modifierPanelTitleStyle;
        private GUIStyle modifierPanelLabelStyle;
        private GUIStyle modifierPanelTimerStyle;
        private GUIStyle progressionCardDescriptionStyle;
        private GUIStyle progressionBadgeStyle;
        private GUIStyle brickCounterLabelStyle;
        private GUIStyle brickCounterValueStyle;
        private GUIStyle upgradePanelStackStyle;
        private GUIStyle upgradeTooltipTitleStyle;
        private GUIStyle upgradeTooltipBodyStyle;
        private GUIStyle floatingScoreStyle;
        private GUIStyle floatingScoreTagStyle;
        private GUIStyle capsuleMadnessStyle;
        private Font retroUiFont;
        private Material vectorIconMaterial;
        private Material spriteIconMaterial;
        private Texture2D fallbackLifeIconTexture;
        private BreakoutUiThemePalette palette = new BreakoutUiThemePalette();

        private readonly struct ActionGroupRange
        {
            public ActionGroupRange(int startIndex, int count)
            {
                StartIndex = startIndex;
                Count = count;
            }

            public int StartIndex { get; }

            public int Count { get; }
        }

        public void ConfigureTheme(BreakoutUiThemePalette themePalette)
        {
            palette = themePalette ?? new BreakoutUiThemePalette();
        }

        public void Dispose()
        {
            foreach (var cachedTexture in iconTextureCache.Values)
            {
                if (cachedTexture != null)
                {
                    DestroyRuntimeObject(cachedTexture);
                }
            }

            iconTextureCache.Clear();

            if (vectorIconMaterial != null)
            {
                DestroyRuntimeObject(vectorIconMaterial);
                vectorIconMaterial = null;
            }

            if (spriteIconMaterial != null)
            {
                DestroyRuntimeObject(spriteIconMaterial);
                spriteIconMaterial = null;
            }

            if (fallbackLifeIconTexture != null)
            {
                DestroyRuntimeObject(fallbackLifeIconTexture);
                fallbackLifeIconTexture = null;
            }
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
            var boxHeight = Mathf.Min(620f, Screen.height - 120f);

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
            var contentTop = boxRect.y + 32f;
            var hintHeight = 42f;
            var footerHeight = 46f;
            var hintY = boxRect.yMax - hintHeight - 18f;
            var footerY = hintY - footerHeight - 12f;
            var contentBottom = footerY - 16f;
            var contentRect = new Rect(
                boxRect.x + outerPadding,
                contentTop,
                boxRect.width - (outerPadding * 2f),
                Mathf.Max(180f, contentBottom - contentTop));
            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);

            DrawActionGroupRows(
                view.ActionLabels,
                view.ActionGroupLabels,
                view.SelectedActionIndex,
                contentRect,
                onActionClicked);

            if (!string.IsNullOrWhiteSpace(view.ValidationText))
            {
                DrawTextWithShadow(new Rect(boxRect.x + outerPadding, footerY, boxRect.width - (outerPadding * 2f), footerHeight), view.ValidationText, setupHintStyle, palette.TextMuted, 0.35f);
            }
            else
            {
                DrawTextWithShadow(new Rect(boxRect.x + outerPadding, footerY, boxRect.width - (outerPadding * 2f), footerHeight), view.FooterText, setupHintStyle, palette.TextMuted, 0.35f);
            }

            DrawHintBand(new Rect(boxRect.x + 28f, hintY, boxRect.width - 56f, hintHeight), view.HintText, draftHintStyle);
        }

        public void DrawProgressionPage(BreakoutUiProgressionView view, Action onStartClicked, Action onBackClicked)
        {
            EnsureStyles();

            if (view == null)
            {
                return;
            }

            var boxWidth = Mathf.Min(1180f, Screen.width - 48f);
            var boxHeight = Mathf.Min(690f, Screen.height - 92f);

            if (boxWidth < 980f)
            {
                boxWidth = Screen.width - 32f;
            }

            if (boxHeight < 560f)
            {
                boxHeight = Screen.height - 76f;
            }

            var boxRect = new Rect((Screen.width - boxWidth) * 0.5f, (Screen.height - boxHeight) * 0.5f, boxWidth, boxHeight);
            var padding = 30f;
            var headerHeight = 82f;
            var footerHeight = 44f;
            var gap = 22f;
            var contentTop = boxRect.y + headerHeight + 18f;
            var contentBottom = boxRect.yMax - footerHeight - 20f;
            var contentHeight = Mathf.Max(220f, contentBottom - contentTop);
            var leftWidth = Mathf.Clamp(boxRect.width * 0.34f, 330f, 390f);
            var leftRect = new Rect(boxRect.x + padding, contentTop, leftWidth, contentHeight);
            var rightRect = new Rect(leftRect.xMax + gap, contentTop, boxRect.xMax - padding - leftRect.xMax - gap, contentHeight);

            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + padding, boxRect.y + 24f, boxRect.width - (padding * 2f) - 126f, 36f), view.Title, setupTitleStyle, palette.TextPrimary);
            DrawTextWithShadow(new Rect(boxRect.x + padding, boxRect.y + 62f, boxRect.width - (padding * 2f), 24f), view.Subtitle, setupHintStyle, palette.TextMuted, 0.35f);

            if (DrawArcadeButton(new Rect(boxRect.xMax - padding - 104f, boxRect.y + 27f, 104f, 38f), "Back", false))
            {
                onBackClicked?.Invoke();
            }

            DrawPanel(leftRect, palette.AccentWarm, palette.AccentSecondary, false);
            DrawPanel(rightRect, palette.AccentPrimary, palette.AccentWarm, false);
            DrawProgressionLadderPanel(leftRect, view, onStartClicked);
            DrawProgressionContentGrid(rightRect, view.Cards);
            DrawHintBand(new Rect(boxRect.x + 28f, boxRect.yMax - footerHeight - 12f, boxRect.width - 56f, footerHeight), view.FooterText, draftHintStyle);
        }

        public void ScrollProgressionContent(float deltaY)
        {
            progressionContentScroll.y = Mathf.Max(0f, progressionContentScroll.y + deltaY);
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
            var hintHeight = 58f;
            var hintY = boxRect.yMax - hintHeight - 24f;
            var contentTop = boxRect.y + (boxRect.height < 548f ? 98f : 116f);
            var contentHeight = Mathf.Max(180f, hintY - contentTop - 18f);
            var availableColumnWidth = Mathf.Max(320f, boxRect.width - (outerPadding * 2f) - contentGap);
            var previewMinimumWidth = boxRect.width < 760f ? 160f : 250f;
            var listMaximumWidth = Mathf.Max(260f, availableColumnWidth - previewMinimumWidth);
            var listUpperWidth = Mathf.Min(562f, listMaximumWidth);
            var listMinimumWidth = Mathf.Min(300f, listUpperWidth);
            var listWidth = Mathf.Clamp(boxRect.width * 0.56f, listMinimumWidth, listUpperWidth);
            var previewWidth = Mathf.Max(140f, availableColumnWidth - listWidth);
            var listRect = new Rect(boxRect.x + outerPadding, contentTop, listWidth, contentHeight);
            var previewRect = new Rect(listRect.xMax + contentGap, contentTop, previewWidth, contentHeight);
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
            if (view.FieldLines.Length > 10)
            {
                lineHeight = Mathf.Clamp((listRect.height - 56f) / Mathf.Max(1, view.FieldLines.Length), 26f, 34f);
            }

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

            if (view == null)
            {
                return;
            }

            var buttonsX = Screen.width - 258f;
            var buttonsY = 20f;
            var showIntensityGauge = view.IntensityGauge != null && view.IntensityGauge.IsVisible;
            var gaugeRect = new Rect(buttonsX - 100f, 12f, 84f, 84f);
            var statusMaxX = showIntensityGauge ? gaugeRect.x - 14f : buttonsX - 18f;
            var hasBottomLine = !string.IsNullOrWhiteSpace(view.BottomLine);
            var statusHeight = hasBottomLine ? 72f : 46f;
            var statusRect = new Rect(18f, 18f, Mathf.Max(320f, statusMaxX - 18f), statusHeight);
            var diagnosticsLabel = view.IsDiagnosticsVisible ? "DBG ON" : "DBG";
            var menuLabel = view.IsPaused ? "RESUME" : "MENU";
            var topLineY = statusRect.y + (hasBottomLine ? 14f : 11f);
            var topLineRect = new Rect(statusRect.x + 20f, topLineY, statusRect.width - 40f, 24f);
            var ladderMinWidth = view.StageLadder != null && view.StageLadder.IsVisible ? 116f : 0f;
            var ladderGap = ladderMinWidth > 0f ? 18f : 0f;
            var readoutRect = new Rect(topLineRect.x, topLineRect.y, Mathf.Max(120f, topLineRect.width - ladderMinWidth - ladderGap), topLineRect.height);

            DrawPanel(statusRect, palette.AccentPrimary, palette.AccentSecondary, false);
            var readoutEndX = DrawHudReadout(readoutRect, view);

            if (hasBottomLine)
            {
                DrawTextWithShadow(new Rect(statusRect.x + 20f, statusRect.y + 40f, statusRect.width - 40f, 22f), view.BottomLine, overlayBodyStyle, palette.TextMuted, 0.3f);
            }

            if (view.StageLadder != null && view.StageLadder.IsVisible)
            {
                var ladderX = readoutEndX + 18f;
                var ladderWidth = Mathf.Clamp(topLineRect.xMax - ladderX, ladderMinWidth, 288f);

                if (ladderX + ladderWidth > topLineRect.xMax)
                {
                    ladderX = topLineRect.xMax - ladderWidth;
                }

                DrawStageLadder(new Rect(ladderX, topLineY + 5f, ladderWidth, 14f), view.StageLadder);
            }

            DrawBallSpeedMeter(view.SpeedMeter);
            DrawIntensityGauge(gaugeRect, view.IntensityGauge);
            DrawBrickCounter(view);
            DrawLifeLossAnimation();

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

            if (HasLeaderboardRows(view))
            {
                DrawSplitOverlay(view, onActionClicked);
                return;
            }

            if (HasStatsRows(view))
            {
                DrawStatsTableOverlay(view, onActionClicked);
                return;
            }

            var boxWidth = view.IsCompact ? 780f : 760f;
            var minBoxHeight = view.IsCompact ? 470f : 404f;
            var summarySpacing = view.EmphasizeSummary ? 36f : 28f;
            var summaryLineHeight = view.EmphasizeSummary ? 32f : 24f;
            var footerLineHeight = 34f;
            var boxHeight = Mathf.Max(
                minBoxHeight,
                174f + (view.SummaryLines.Length * summarySpacing) + (view.ActionLabels.Length * 44f) + (view.FooterLines.Length * footerLineHeight));
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
            var actionLineHeight = 44f;
            var footerBlockHeight = view.FooterLines.Length * footerLineHeight;
            var footerStartY = boxRect.yMax - outerPadding - footerBlockHeight;
            var actionBlockHeight = view.ActionLabels.Length * actionLineHeight;
            var actionStartY = footerStartY - 18f - actionBlockHeight;
            var summaryBottomY = summaryY + (view.SummaryLines.Length * summarySpacing) + 18f;

            if (actionStartY < summaryBottomY)
            {
                actionStartY = summaryBottomY;
                footerStartY = actionStartY + actionBlockHeight + 18f;
            }

            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + outerPadding, titleY, boxRect.width - (outerPadding * 2f), 40f), view.Title, overlayTitleStyle, palette.TextPrimary);

            for (var index = 0; index < view.SummaryLines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(boxRect.x + 36f, summaryY + (index * summarySpacing), boxRect.width - 72f, summaryLineHeight),
                    view.SummaryLines[index],
                    view.EmphasizeSummary ? overlayMetricStyle : overlayBodyStyle,
                    view.EmphasizeSummary || index == 0 ? palette.TextPrimary : palette.TextMuted,
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
                    new Rect(boxRect.x + 34f, footerStartY + (index * footerLineHeight), boxRect.width - 68f, footerLineHeight),
                    view.FooterLines[index],
                    setupHintStyle,
                    palette.TextMuted,
                    0.3f);
            }
        }

        public void ScrollStatsTable(float deltaY)
        {
            statsTableScroll.y = Mathf.Max(0f, statsTableScroll.y + deltaY);
        }

        private void DrawStatsTableOverlay(BreakoutUiOverlayView view, Action<int> onActionClicked)
        {
            if (view == null)
            {
                return;
            }

            var boxWidth = Mathf.Min(900f, Screen.width - 48f);
            var boxHeight = Mathf.Min(680f, Screen.height - 92f);

            if (boxWidth < 680f)
            {
                boxWidth = Screen.width - 32f;
            }

            if (boxHeight < 500f)
            {
                boxHeight = Screen.height - 72f;
            }

            var boxRect = new Rect((Screen.width - boxWidth) * 0.5f, (Screen.height - boxHeight) * 0.5f, boxWidth, boxHeight);
            var padding = 30f;
            var titleY = boxRect.y + 24f;
            var tableTop = boxRect.y + 84f;
            var actionLineHeight = 44f;
            var footerLineHeight = 28f;
            var actionBlockHeight = Mathf.Max(0, view.ActionLabels.Length) * actionLineHeight;
            var footerBlockHeight = Mathf.Max(0, view.FooterLines.Length) * footerLineHeight;
            var actionStartY = boxRect.yMax - padding - footerBlockHeight - actionBlockHeight - 16f;
            var footerStartY = actionStartY + actionBlockHeight + 10f;
            var tableRect = new Rect(boxRect.x + padding, tableTop, boxRect.width - (padding * 2f), Mathf.Max(140f, actionStartY - tableTop - 18f));

            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + padding, titleY, boxRect.width - (padding * 2f), 40f), view.Title, overlayTitleStyle, palette.TextPrimary);
            DrawStatsTable(tableRect, view.StatsRows);

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
                    new Rect(boxRect.x + 34f, footerStartY + (index * footerLineHeight), boxRect.width - 68f, footerLineHeight),
                    view.FooterLines[index],
                    setupHintStyle,
                    palette.TextMuted,
                    0.3f);
            }
        }

        private void DrawStatsTable(Rect rect, BreakoutUiStatsRowView[] rows)
        {
            rows ??= Array.Empty<BreakoutUiStatsRowView>();
            DrawPanel(rect, palette.AccentPrimary, palette.AccentWarm, false);

            var headerHeight = 32f;
            var rowHeight = 34f;
            var innerPadding = 14f;
            var scrollBarWidth = 18f;
            var contentWidth = rect.width - (innerPadding * 2f) - scrollBarWidth;
            var labelWidth = Mathf.Clamp(contentWidth * 0.56f, 220f, 430f);
            var valueWidth = Mathf.Max(130f, contentWidth - labelWidth - 16f);
            var headerRect = new Rect(rect.x + innerPadding, rect.y + 12f, contentWidth, headerHeight);
            var viewport = new Rect(rect.x + innerPadding, headerRect.yMax + 8f, contentWidth + scrollBarWidth, Mathf.Max(60f, rect.yMax - headerRect.yMax - 22f));
            var contentHeight = Mathf.Max(viewport.height, rows.Length * rowHeight);
            var contentRect = new Rect(0f, 0f, contentWidth, contentHeight);

            DrawSolidRect(new Rect(headerRect.x, headerRect.yMax - 2f, headerRect.width, 2f), WithAlpha(palette.AccentPrimary, 0.4f));
            DrawTextWithShadow(new Rect(headerRect.x, headerRect.y + 4f, labelWidth, 20f), "STAT", speedMeterCaptionStyle, palette.AccentPrimary, 0.22f);
            DrawTextWithShadow(new Rect(headerRect.x + labelWidth + 16f, headerRect.y + 4f, valueWidth, 20f), "TOTAL", speedMeterCaptionStyle, palette.AccentWarm, 0.22f);

            statsTableScroll = GUI.BeginScrollView(viewport, statsTableScroll, contentRect, false, contentHeight > viewport.height);

            for (var index = 0; index < rows.Length; index++)
            {
                var y = index * rowHeight;

                if (index % 2 == 0)
                {
                    DrawSolidRect(new Rect(0f, y, contentWidth, rowHeight - 2f), WithAlpha(palette.AccentSecondary, 0.06f));
                }

                DrawSolidRect(new Rect(labelWidth + 8f, y + 5f, 1f, rowHeight - 10f), WithAlpha(palette.TextPrimary, 0.08f));
                DrawTextWithShadow(new Rect(10f, y + 6f, labelWidth - 18f, 22f), rows[index]?.Label ?? string.Empty, overlayBodyStyle, palette.TextMuted, 0.25f);
                DrawTextWithShadow(new Rect(labelWidth + 22f, y + 6f, valueWidth - 10f, 22f), rows[index]?.Value ?? string.Empty, overlayMetricStyle, palette.TextPrimary, 0.25f);
            }

            GUI.EndScrollView();
        }

        private void DrawSplitOverlay(BreakoutUiOverlayView view, Action<int> onActionClicked)
        {
            if (view == null)
            {
                return;
            }

            var useStackedLayout = Screen.width < 1060f;
            var outerPadding = 28f;
            var gap = 22f;
            var availableWidth = Mathf.Min(Screen.width - 48f, 1160f);
            var roundInfoWidth = useStackedLayout
                ? availableWidth
                : Mathf.Clamp(availableWidth * 0.48f, 440f, 540f);
            var leaderboardWidth = useStackedLayout
                ? availableWidth
                : availableWidth - roundInfoWidth - gap;
            var lineHeight = 27f;
            var leaderboardRowCount = GetLeaderboardRowCount(view);
            var leaderboardHeight = 104f + (leaderboardRowCount * lineHeight);
            var roundInfoContentWidth = (useStackedLayout ? availableWidth : roundInfoWidth) - (outerPadding * 2f);
            var roundInfoHeight = CalculateSplitRoundInfoHeight(view, roundInfoContentWidth);

            if (useStackedLayout)
            {
                var stackedHeight = Mathf.Min(
                    Screen.height - 96f,
                    84f + roundInfoHeight + leaderboardHeight + gap);
                var stackRect = new Rect(
                    (Screen.width - availableWidth) * 0.5f,
                    (Screen.height - stackedHeight) * 0.5f,
                    availableWidth,
                    stackedHeight);
                DrawTextWithShadow(new Rect(stackRect.x, stackRect.y, stackRect.width, 48f), view.Title, overlayTitleStyle, palette.TextPrimary);

                var infoHeight = Mathf.Min(roundInfoHeight, (stackRect.height - 78f - gap) * 0.56f);
                var infoRect = new Rect(stackRect.x, stackRect.y + 60f, stackRect.width, infoHeight);
                var boardRect = new Rect(stackRect.x, infoRect.yMax + gap, stackRect.width, stackRect.yMax - infoRect.yMax - gap);
                DrawRoundInfoPanel(infoRect, view, outerPadding, onActionClicked);
                DrawLeaderboardPanel(boardRect, view, outerPadding, lineHeight);
                return;
            }

            var panelHeight = Mathf.Min(Screen.height - 132f, Mathf.Max(486f, Mathf.Max(roundInfoHeight, leaderboardHeight)));
            var totalWidth = roundInfoWidth + gap + leaderboardWidth;
            var left = (Screen.width - totalWidth) * 0.5f;
            var top = (Screen.height - panelHeight) * 0.5f;
            var titleRect = new Rect(left, top - 54f, totalWidth, 46f);
            DrawTextWithShadow(titleRect, view.Title, overlayTitleStyle, palette.TextPrimary);

            var infoRectWide = new Rect(left, top, roundInfoWidth, panelHeight);
            var leaderboardRectWide = new Rect(infoRectWide.xMax + gap, top, leaderboardWidth, panelHeight);
            DrawRoundInfoPanel(infoRectWide, view, outerPadding, onActionClicked);
            DrawLeaderboardPanel(leaderboardRectWide, view, outerPadding, lineHeight);
        }

        private void DrawRoundInfoPanel(Rect rect, BreakoutUiOverlayView view, float padding, Action<int> onActionClicked)
        {
            DrawPanel(rect, palette.AccentSecondary, palette.AccentPrimary, true);

            var y = rect.y + 22f;
            var title = string.IsNullOrWhiteSpace(view.SummaryTitle) ? "Round Info" : view.SummaryTitle;
            var contentWidth = rect.width - (padding * 2f);
            DrawTextWithShadow(new Rect(rect.x + padding, y, rect.width - (padding * 2f), 24f), title, hudStyle, palette.AccentPrimary, 0.28f);
            y += 38f;

            for (var index = 0; index < view.SummaryLines.Length; index++)
            {
                var lineHeight = CalculateWrappedTextHeight(
                    overlayBodyStyle,
                    view.SummaryLines[index],
                    contentWidth,
                    SplitOverlaySummaryMinHeight);
                DrawTextWithShadow(
                    new Rect(rect.x + padding, y, contentWidth, lineHeight),
                    view.SummaryLines[index],
                    overlayBodyStyle,
                    index == 0 ? palette.TextPrimary : palette.TextMuted,
                    0.35f);
                y += lineHeight + SplitOverlaySummaryGap;
            }

            y += 14f;
            DrawActionList(
                view.ActionLabels,
                view.SelectedActionIndex,
                rect.x + padding,
                y,
                contentWidth,
                SplitOverlayActionLineHeight,
                onActionClicked);

            y += (view.ActionLabels.Length * SplitOverlayActionLineHeight) + 16f;

            for (var index = 0; index < view.FooterLines.Length; index++)
            {
                var lineHeight = CalculateWrappedTextHeight(
                    setupHintStyle,
                    view.FooterLines[index],
                    contentWidth,
                    SplitOverlayFooterMinHeight);
                DrawTextWithShadow(
                    new Rect(rect.x + padding, y, contentWidth, lineHeight),
                    view.FooterLines[index],
                    setupHintStyle,
                    palette.TextMuted,
                    0.3f);
                y += lineHeight + SplitOverlayFooterGap;
            }
        }

        private float CalculateSplitRoundInfoHeight(BreakoutUiOverlayView view, float contentWidth)
        {
            if (view == null)
            {
                return 486f;
            }

            var summaryHeight = CalculateWrappedTextStackHeight(
                view.SummaryLines,
                overlayBodyStyle,
                contentWidth,
                SplitOverlaySummaryMinHeight,
                SplitOverlaySummaryGap);
            var footerHeight = CalculateWrappedTextStackHeight(
                view.FooterLines,
                setupHintStyle,
                contentWidth,
                SplitOverlayFooterMinHeight,
                SplitOverlayFooterGap);
            var legacyHeight = 212f
                + (view.SummaryLines.Length * 30f)
                + ((view.ActionLabels?.Length ?? 0) * 44f)
                + (view.FooterLines.Length * 34f);
            var measuredHeight = 134f
                + summaryHeight
                + ((view.ActionLabels?.Length ?? 0) * SplitOverlayActionLineHeight)
                + footerHeight;

            return Mathf.Max(legacyHeight, measuredHeight);
        }

        private static float CalculateWrappedTextStackHeight(
            string[] lines,
            GUIStyle style,
            float width,
            float minLineHeight,
            float lineGap)
        {
            if (lines == null || lines.Length == 0)
            {
                return 0f;
            }

            var height = 0f;

            for (var index = 0; index < lines.Length; index++)
            {
                height += CalculateWrappedTextHeight(style, lines[index], width, minLineHeight) + lineGap;
            }

            return height;
        }

        private static float CalculateWrappedTextHeight(GUIStyle style, string text, float width, float minHeight)
        {
            if (style == null)
            {
                return minHeight;
            }

            var measuredHeight = style.CalcHeight(
                new GUIContent(text ?? string.Empty),
                Mathf.Max(1f, width));
            return Mathf.Max(minHeight, measuredHeight);
        }

        private void DrawLeaderboardPanel(Rect rect, BreakoutUiOverlayView view, float padding, float lineHeight)
        {
            DrawPanel(rect, palette.AccentPrimary, palette.AccentWarm, true);

            var title = string.IsNullOrWhiteSpace(view.LeaderboardTitle) ? "Scoreboard" : view.LeaderboardTitle;
            DrawTextWithShadow(new Rect(rect.x + padding, rect.y + 22f, rect.width - (padding * 2f), 32f), title, overlayTitleStyle, palette.TextPrimary, 0.32f);
            var headerRect = new Rect(rect.x + 18f, rect.y + 64f, rect.width - 36f, 22f);
            DrawLeaderboardHeader(headerRect);

            var firstLineY = headerRect.yMax + 8f;
            var leaderboardRowCount = GetLeaderboardRowCount(view);
            var visibleCount = Mathf.Min(leaderboardRowCount, Mathf.Max(1, Mathf.FloorToInt((rect.yMax - firstLineY - 18f) / lineHeight)));

            for (var index = 0; index < visibleCount; index++)
            {
                var rowRect = new Rect(rect.x + 18f, firstLineY + (index * lineHeight), rect.width - 36f, lineHeight - 2f);
                var isLeader = index == 0;

                if (isLeader)
                {
                    DrawSolidRect(rowRect, WithAlpha(palette.AccentWarm, 0.12f));
                    DrawOutline(rowRect, WithAlpha(palette.AccentWarm, 0.72f), 1f);
                }
                else if ((index & 1) == 1)
                {
                    DrawSolidRect(rowRect, WithAlpha(palette.BezelDark, 0.2f));
                }

                if (view.LeaderboardEntries != null && view.LeaderboardEntries.Length > index)
                {
                    DrawLeaderboardEntry(rowRect, view.LeaderboardEntries[index], isLeader);
                }
                else
                {
                    DrawTextWithShadow(
                        new Rect(rowRect.x + 10f, rowRect.y + 2f, rowRect.width - 20f, rowRect.height),
                        view.LeaderboardLines[index],
                        overlayBodyStyle,
                        isLeader ? palette.TextPrimary : palette.TextMuted,
                        isLeader ? 0.36f : 0.22f);
                }
            }
        }

        private static bool HasLeaderboardRows(BreakoutUiOverlayView view)
        {
            if (view == null)
            {
                return false;
            }

            return (view.LeaderboardEntries != null && view.LeaderboardEntries.Length > 0)
                || (view.LeaderboardLines != null && view.LeaderboardLines.Length > 0);
        }

        private static bool HasStatsRows(BreakoutUiOverlayView view)
        {
            return view?.StatsRows != null && view.StatsRows.Length > 0;
        }

        private static int GetLeaderboardRowCount(BreakoutUiOverlayView view)
        {
            if (view == null)
            {
                return 0;
            }

            return view.LeaderboardEntries != null && view.LeaderboardEntries.Length > 0
                ? view.LeaderboardEntries.Length
                : view.LeaderboardLines?.Length ?? 0;
        }

        private void DrawLeaderboardHeader(Rect contentRect)
        {
            DrawLeaderboardCell(contentRect, 0f, 0.14f, "RANK", 0.22f, TextAnchor.MiddleCenter, palette.AccentWarm);
            DrawLeaderboardCell(contentRect, 0.14f, 0.58f, "PLAYER", 0.22f, TextAnchor.MiddleLeft, palette.AccentWarm);
            DrawLeaderboardCell(contentRect, 0.72f, 0.28f, "SCORE", 0.22f, TextAnchor.MiddleRight, palette.AccentWarm);
        }

        private void DrawLeaderboardEntry(Rect rowRect, BreakoutTurnLeaderboardEntry entry, bool isLeader)
        {
            var color = isLeader ? palette.TextPrimary : palette.TextMuted;
            var shadow = isLeader ? 0.36f : 0.22f;
            DrawLeaderboardCell(rowRect, 0f, 0.14f, entry.IsCurrentPlayer ? $">{entry.Rank:00}" : $"{entry.Rank:00}", shadow, TextAnchor.MiddleCenter, color);
            DrawLeaderboardCell(rowRect, 0.14f, 0.58f, entry.PlayerName, shadow, TextAnchor.MiddleLeft, color);
            DrawLeaderboardCell(rowRect, 0.72f, 0.28f, FormatScoreValueForLeaderboard(entry.Score), shadow, TextAnchor.MiddleRight, color);
        }

        private void DrawLeaderboardCell(Rect rowRect, float normalizedX, float normalizedWidth, string text, float shadowAlpha, TextAnchor alignment, Color color)
        {
            var rect = new Rect(
                rowRect.x + (rowRect.width * normalizedX) + 6f,
                rowRect.y + 2f,
                (rowRect.width * normalizedWidth) - 12f,
                rowRect.height - 2f);
            var previousAlignment = overlayBodyStyle.alignment;
            overlayBodyStyle.alignment = alignment;
            DrawTextWithShadow(rect, text, overlayBodyStyle, color, shadowAlpha);
            overlayBodyStyle.alignment = previousAlignment;
        }

        private static string FormatScoreValueForLeaderboard(int score)
        {
            return score >= 100000
                ? score.ToString("000000", System.Globalization.CultureInfo.InvariantCulture)
                : score.ToString("0000", System.Globalization.CultureInfo.InvariantCulture);
        }

        public void DrawUpgradeDraft(BreakoutUiUpgradeDraftView view, Action<int> onOptionClicked)
        {
            EnsureStyles();

            if (view == null || view.Options == null || view.Options.Length == 0)
            {
                return;
            }

            var boxWidth = Mathf.Min(1180f, Mathf.Max(980f, Screen.width - 64f));
            var boxHeight = Mathf.Min(650f, Mathf.Max(560f, Screen.height - 56f));
            var boxRect = new Rect((Screen.width * 0.5f) - (boxWidth * 0.5f), (Screen.height * 0.5f) - (boxHeight * 0.5f), boxWidth, boxHeight);
            DrawPanel(boxRect, palette.AccentSecondary, palette.AccentPrimary, true);
            DrawTextWithShadow(new Rect(boxRect.x + 36f, boxRect.y + 24f, boxRect.width - 72f, 42f), view.Title, overlayTitleStyle, palette.TextPrimary);
            DrawTextWithShadow(new Rect(boxRect.x + 36f, boxRect.y + 68f, boxRect.width - 72f, 28f), view.Subtitle, setupHintStyle, palette.TextMuted, 0.35f);
            DrawTextWithShadow(new Rect(boxRect.x + 36f, boxRect.y + 102f, boxRect.width - 72f, 28f), view.BuildLine, hudStyle, palette.TextPrimary, 0.25f);

            var optionCount = view.Options.Length;
            var optionSpacing = 24f;
            var optionWidth = Mathf.Min(340f, (boxRect.width - 112f - ((optionCount - 1) * optionSpacing)) / optionCount);
            var totalWidth = (optionCount * optionWidth) + ((optionCount - 1) * optionSpacing);
            var startX = boxRect.x + ((boxRect.width - totalWidth) * 0.5f);
            var optionY = boxRect.y + 148f;
            var hintHeight = 78f;
            var hintY = boxRect.yMax - hintHeight - 34f;
            var optionHeight = Mathf.Max(268f, hintY - optionY - 26f);

            for (var index = 0; index < optionCount; index++)
            {
                var option = view.Options[index];
                var isSelected = index == Mathf.Clamp(view.SelectedOptionIndex, 0, optionCount - 1);
                var optionRect = new Rect(startX + (index * (optionWidth + optionSpacing)), optionY, optionWidth, optionHeight);
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

                var iconRect = new Rect(optionRect.x + 20f, optionRect.y + 48f, 68f, 68f);
                DrawSectionLabel(new Rect(optionRect.x + 18f, optionRect.y + 16f, optionRect.width - 36f, 20f), $"PICK {index + 1}", accent);
                DrawIconTile(iconRect, option.Icon, accent, isSelected);
                DrawTextWithShadow(new Rect(optionRect.x + 104f, optionRect.y + 42f, optionRect.width - 124f, 78f), option.Title, draftOptionTitleStyle, palette.TextPrimary, 0.3f);
                DrawTextWithShadow(new Rect(optionRect.x + 20f, optionRect.y + 132f, optionRect.width - 40f, Mathf.Max(96f, optionRect.height - 254f)), option.Description, draftOptionBodyStyle, palette.TextMuted, 0.22f);
                DrawHorizontalGradient(new Rect(optionRect.x + 20f, optionRect.yMax - 110f, optionRect.width - 40f, 2f), WithAlpha(accent, 0.55f), WithAlpha(palette.AccentPrimary, 0.18f), 12);
                DrawTextWithShadow(new Rect(optionRect.x + 20f, optionRect.yMax - 98f, optionRect.width - 40f, 84f), FormatDraftCardDetail(option.Detail), draftOptionDetailStyle, palette.TextPrimary, 0.25f);
            }

            DrawHintBand(new Rect(boxRect.x + 36f, hintY, boxRect.width - 72f, hintHeight), view.HintText, draftHintStyle);
        }

        public void DrawMessageOverlay(string message)
        {
            EnsureStyles();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var lineCount = Mathf.Max(1, message.Split('\n').Length);
            var boxWidth = Mathf.Min(720f, Screen.width - 48f);
            var boxHeight = Mathf.Clamp(46f + (lineCount * 28f), 84f, Screen.height - 104f);
            var boxRect = new Rect((Screen.width * 0.5f) - (boxWidth * 0.5f), (Screen.height * 0.5f) - (boxHeight * 0.5f), boxWidth, boxHeight);
            DrawPanel(boxRect, palette.AccentWarm, palette.AccentSecondary, false);
            DrawTextWithShadow(new Rect(boxRect.x + 18f, boxRect.y + 12f, boxRect.width - 36f, boxRect.height - 24f), message, messageStyle, palette.TextPrimary);
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

        public void DrawCapsuleMadness(BreakoutUiCapsuleMadnessView view)
        {
            EnsureStyles();

            if (view == null
                || view.Timer <= 0f
                || view.Duration <= 0f
                || string.IsNullOrWhiteSpace(view.Text)
                || view.PlayfieldRect.width <= 2f
                || view.PlayfieldRect.height <= 2f)
            {
                return;
            }

            var remainingRatio = Mathf.Clamp01(view.Timer / view.Duration);
            var lifeRatio = 1f - remainingRatio;
            var alpha = Mathf.Min(
                Mathf.InverseLerp(0f, 0.12f, lifeRatio),
                Mathf.InverseLerp(0f, 0.22f, remainingRatio));

            if (alpha <= 0.001f)
            {
                return;
            }

            var playfield = view.PlayfieldRect;
            var rectWidth = Mathf.Min(playfield.width * 0.96f, 980f);
            var rectHeight = Mathf.Min(206f, playfield.height * 0.38f);
            var center = new Vector2(playfield.center.x, Mathf.Lerp(playfield.y + (playfield.height * 0.34f), playfield.center.y, 0.45f));
            var textRect = new Rect(center.x - (rectWidth * 0.5f), center.y - (rectHeight * 0.5f), rectWidth, rectHeight);
            var time = Time.unscaledTime;
            var pulse = 1f + (Mathf.Sin(time * 17f) * 0.065f) + (Mathf.Sin(time * 31f) * 0.025f);
            var rotation = Mathf.Sin(time * 9.5f) * 5.5f;
            var previousMatrix = GUI.matrix;

            GUIUtility.ScaleAroundPivot(new Vector2(pulse, pulse), center);
            GUIUtility.RotateAroundPivot(rotation, center);

            DrawSolidRect(Inflate(textRect, 10f), WithAlpha(palette.BezelDark, 0.055f * alpha));
            DrawTextWithShadow(
                new Rect(textRect.x - 5f, textRect.y + 4f, textRect.width, textRect.height),
                ToArcadeLabel(view.Text),
                capsuleMadnessStyle,
                WithAlpha(palette.AccentSecondary, 0.24f * alpha),
                0.08f * alpha);
            DrawTextWithShadow(
                new Rect(textRect.x + 5f, textRect.y - 4f, textRect.width, textRect.height),
                ToArcadeLabel(view.Text),
                capsuleMadnessStyle,
                WithAlpha(palette.AccentPrimary, 0.2f * alpha),
                0.08f * alpha);
            DrawTextWithShadow(
                textRect,
                ToArcadeLabel(view.Text),
                capsuleMadnessStyle,
                WithAlpha(view.Color, 0.42f * alpha),
                0.2f * alpha);

            GUI.matrix = previousMatrix;
        }

        public void DrawAutoSaveBurst(BreakoutUiCapsuleMadnessView view)
        {
            DrawCapsuleMadness(view);
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
                var primaryRect = new Rect(position.x - 112f, position.y - 24f, 224f, 36f);
                var secondaryWidth = Mathf.Min(560f, Mathf.Max(352f, Screen.width - 32f));
                var secondaryX = Mathf.Clamp(
                    position.x - (secondaryWidth * 0.5f),
                    16f,
                    Mathf.Max(16f, Screen.width - secondaryWidth - 16f));
                var secondaryRect = new Rect(secondaryX, position.y + 12f, secondaryWidth, 30f);
                var scoreColor = WithAlpha(view.Color, view.Alpha);
                var tagColor = WithAlpha(Color.Lerp(palette.AccentSecondary, palette.AccentPrimary, 0.45f), view.Alpha * 0.92f);

                DrawTextWithShadow(primaryRect, view.PrimaryText, floatingScoreStyle, scoreColor, 0.22f * view.Alpha);

                if (!string.IsNullOrWhiteSpace(view.SecondaryText))
                {
                    DrawTextWithShadow(secondaryRect, view.SecondaryText, floatingScoreTagStyle, tagColor, 0.18f * view.Alpha);
                }
            }
        }

        public void DrawRunUpgradePanel(BreakoutUiRunUpgradePanelView view)
        {
            EnsureStyles();

            if (view == null || view.Items == null || view.Items.Length == 0)
            {
                return;
            }

            var iconSize = 42f;
            var iconSpacing = 8f;
            var visibleCount = Mathf.Min(view.Items.Length, Mathf.Max(1, Mathf.FloorToInt((Screen.width - 76f) / (iconSize + iconSpacing))));
            var panelWidth = Mathf.Clamp(34f + visibleCount * iconSize + Mathf.Max(0, visibleCount - 1) * iconSpacing, 112f, Screen.width - 36f);
            var panelHeight = 76f;
            var bottomMargin = view.IsDiagnosticsVisible ? 198f : 22f;
            var panelRect = new Rect(18f, Mathf.Max(104f, Screen.height - panelHeight - bottomMargin), panelWidth, panelHeight);
            BreakoutUiRunUpgradePanelItemView hoveredItem = null;

            DrawPanel(panelRect, palette.AccentSecondary, palette.AccentPrimary, false, 1.5f);
            DrawSectionLabel(new Rect(panelRect.x + 12f, panelRect.y + 8f, panelRect.width - 24f, 14f), "BUILD", palette.AccentWarm);

            var iconX = panelRect.x + 14f;
            var iconY = panelRect.y + 26f;

            for (var index = 0; index < visibleCount; index++)
            {
                var item = view.Items[index];
                var iconRect = new Rect(iconX + index * (iconSize + iconSpacing), iconY, iconSize, iconSize);
                var isHovered = iconRect.Contains(Event.current.mousePosition);

                DrawIconTile(iconRect, item.Icon, item.Accent, isHovered);
                DrawTextWithShadow(new Rect(iconRect.x, iconRect.yMax - 13f, iconRect.width, 12f), item.Label, modifierPanelLabelStyle, palette.TextPrimary, 0.2f);

                if (item.StackCount > 1)
                {
                    DrawSolidRect(new Rect(iconRect.xMax - 18f, iconRect.y - 1f, 20f, 16f), WithAlpha(palette.BezelDark, 0.92f));
                    DrawOutline(new Rect(iconRect.xMax - 18f, iconRect.y - 1f, 20f, 16f), item.Accent, 1f);
                    DrawTextWithShadow(new Rect(iconRect.xMax - 18f, iconRect.y - 1f, 20f, 16f), $"x{item.StackCount}", upgradePanelStackStyle, palette.TextPrimary, 0.18f);
                }

                if (isHovered)
                {
                    hoveredItem = item;
                }
            }

            if (view.Items.Length > visibleCount)
            {
                DrawTextWithShadow(new Rect(panelRect.xMax - 32f, panelRect.y + 8f, 24f, 14f), $"+{view.Items.Length - visibleCount}", speedMeterValueStyle, palette.TextMuted, 0.2f);
            }

            if (hoveredItem != null)
            {
                DrawUpgradeTooltip(hoveredItem, panelRect);
            }
        }

        private void DrawBrickCounter(BreakoutUiHudView view)
        {
            if (view == null || !view.HasBrickCounter)
            {
                previousBrickCounterValue = -1;
                return;
            }

            if (previousBrickCounterValue >= 0 && view.BricksRemaining < previousBrickCounterValue)
            {
                brickCounterPulseStartTime = Time.unscaledTime;
            }

            previousBrickCounterValue = view.BricksRemaining;

            var pulseAge = Time.unscaledTime - brickCounterPulseStartTime;
            var pulse = Mathf.Clamp01(1f - (pulseAge / BrickCounterPulseDuration));
            var pulseWave = Mathf.Sin((1f - pulse) * Mathf.PI);
            var panelWidth = Mathf.Clamp(Screen.width * 0.16f, 148f, 206f);
            var panelHeight = 88f;
            var bottomMargin = view.IsDiagnosticsVisible ? 198f : 22f;
            var expansion = Mathf.Lerp(0f, 8f, pulseWave);
            var panelRect = new Rect(
                Screen.width - panelWidth - 18f,
                Mathf.Max(104f, Screen.height - panelHeight - bottomMargin),
                panelWidth,
                panelHeight);
            var animatedRect = Inflate(panelRect, expansion);
            var accent = Color.Lerp(palette.AccentPrimary, palette.AccentWarm, pulseWave);
            var rightAccent = Color.Lerp(palette.AccentSecondary, palette.TextPrimary, pulseWave * 0.45f);

            if (pulse > 0f)
            {
                DrawSolidRect(Inflate(panelRect, Mathf.Lerp(8f, 18f, pulseWave)), WithAlpha(accent, Mathf.Lerp(0.04f, 0.18f, pulseWave)));
            }

            DrawPanel(animatedRect, accent, rightAccent, pulse > 0f, 1.8f + (pulseWave * 1.1f));
            DrawTextWithShadow(
                new Rect(animatedRect.x + 14f, animatedRect.y + 10f, animatedRect.width - 28f, 18f),
                "BRICKS",
                brickCounterLabelStyle,
                accent,
                0.25f);

            var valueColor = pulse > 0f
                ? Color.Lerp(palette.TextPrimary, palette.AccentWarm, pulseWave * 0.85f)
                : palette.TextPrimary;
            DrawTextWithShadow(
                new Rect(animatedRect.x + 14f, animatedRect.y + 29f, animatedRect.width - 28f, 44f),
                view.BricksRemaining.ToString("00", System.Globalization.CultureInfo.InvariantCulture),
                brickCounterValueStyle,
                valueColor,
                0.28f + (pulseWave * 0.18f));

            var meterRect = new Rect(animatedRect.x + 16f, animatedRect.yMax - 16f, animatedRect.width - 32f, 4f);
            DrawHorizontalGradient(meterRect, WithAlpha(accent, 0.82f), WithAlpha(palette.AccentSecondary, 0.62f), 12);
        }

        private void EnsureStyles()
        {
            retroUiFont ??= Font.CreateDynamicFontFromOSFont(RetroUiFontNames, RetroUiFontSize);

            hudStyle ??= new GUIStyle(GUI.skin.label)
            {
                font = retroUiFont,
                fontSize = 18,
            };
            messageStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                font = retroUiFont,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            pickupStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                font = retroUiFont,
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
            overlayMetricStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            overlayActionStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
            };
            overlaySelectedActionStyle ??= new GUIStyle(overlayActionStyle);
            draftOptionTitleStyle ??= new GUIStyle(overlayActionStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 21,
                wordWrap = true,
            };
            draftOptionBodyStyle ??= new GUIStyle(overlayBodyStyle)
            {
                fontSize = 18,
                wordWrap = true,
            };
            draftOptionDetailStyle ??= new GUIStyle(setupHintStyle)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            draftHintStyle ??= new GUIStyle(setupHintStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                wordWrap = true,
            };
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
            progressionCardDescriptionStyle ??= new GUIStyle(setupHintStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                wordWrap = true,
            };
            progressionBadgeStyle ??= new GUIStyle(speedMeterCaptionStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
            };
            brickCounterLabelStyle ??= new GUIStyle(speedMeterCaptionStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
            };
            brickCounterValueStyle ??= new GUIStyle(pickupStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
            };
            upgradePanelStackStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
            };
            upgradeTooltipTitleStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
            };
            upgradeTooltipBodyStyle ??= new GUIStyle(setupHintStyle)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                wordWrap = true,
            };
            floatingScoreStyle ??= new GUIStyle(pickupStyle)
            {
                fontSize = 30,
            };
            floatingScoreTagStyle ??= new GUIStyle(hudStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
            };
            capsuleMadnessStyle ??= new GUIStyle(pickupStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 72,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };

            ApplyRetroFont();

            hudStyle.normal.textColor = palette.TextPrimary;
            messageStyle.normal.textColor = palette.TextPrimary;
            pickupStyle.normal.textColor = palette.TextPrimary;
            setupTitleStyle.normal.textColor = palette.TextPrimary;
            setupSelectedStyle.normal.textColor = palette.TextPrimary;
            setupHintStyle.normal.textColor = palette.TextMuted;
            overlayTitleStyle.normal.textColor = palette.TextPrimary;
            overlayBodyStyle.normal.textColor = palette.TextMuted;
            overlayMetricStyle.normal.textColor = palette.TextPrimary;
            overlayActionStyle.normal.textColor = palette.TextMuted;
            overlaySelectedActionStyle.normal.textColor = palette.TextPrimary;
            draftOptionTitleStyle.normal.textColor = palette.TextPrimary;
            draftOptionBodyStyle.normal.textColor = palette.TextMuted;
            draftOptionDetailStyle.normal.textColor = palette.TextPrimary;
            draftHintStyle.normal.textColor = palette.TextMuted;
            hudButtonStyle.normal.textColor = palette.TextMuted;
            hudActiveButtonStyle.normal.textColor = palette.TextPrimary;
            speedMeterCaptionStyle.normal.textColor = palette.TextPrimary;
            speedMeterValueStyle.normal.textColor = palette.TextMuted;
            modifierPanelTitleStyle.normal.textColor = palette.TextMuted;
            modifierPanelLabelStyle.normal.textColor = palette.TextPrimary;
            modifierPanelTimerStyle.normal.textColor = palette.TextMuted;
            progressionCardDescriptionStyle.normal.textColor = palette.TextMuted;
            progressionBadgeStyle.normal.textColor = palette.TextPrimary;
            brickCounterLabelStyle.normal.textColor = palette.AccentWarm;
            brickCounterValueStyle.normal.textColor = palette.TextPrimary;
            upgradePanelStackStyle.normal.textColor = palette.TextPrimary;
            upgradeTooltipTitleStyle.normal.textColor = palette.TextPrimary;
            upgradeTooltipBodyStyle.normal.textColor = palette.TextMuted;
            floatingScoreStyle.normal.textColor = palette.AccentWarm;
            floatingScoreTagStyle.normal.textColor = palette.AccentPrimary;
            capsuleMadnessStyle.normal.textColor = palette.AccentWarm;
        }

        private void ApplyRetroFont()
        {
            if (retroUiFont == null)
            {
                return;
            }

            hudStyle.font = retroUiFont;
            messageStyle.font = retroUiFont;
            pickupStyle.font = retroUiFont;
            setupTitleStyle.font = retroUiFont;
            setupSelectedStyle.font = retroUiFont;
            setupHintStyle.font = retroUiFont;
            overlayTitleStyle.font = retroUiFont;
            overlayBodyStyle.font = retroUiFont;
            overlayMetricStyle.font = retroUiFont;
            overlayActionStyle.font = retroUiFont;
            overlaySelectedActionStyle.font = retroUiFont;
            draftOptionTitleStyle.font = retroUiFont;
            draftOptionBodyStyle.font = retroUiFont;
            draftOptionDetailStyle.font = retroUiFont;
            draftHintStyle.font = retroUiFont;
            hudButtonStyle.font = retroUiFont;
            hudActiveButtonStyle.font = retroUiFont;
            speedMeterCaptionStyle.font = retroUiFont;
            speedMeterValueStyle.font = retroUiFont;
            modifierPanelTitleStyle.font = retroUiFont;
            modifierPanelLabelStyle.font = retroUiFont;
            modifierPanelTimerStyle.font = retroUiFont;
            progressionCardDescriptionStyle.font = retroUiFont;
            progressionBadgeStyle.font = retroUiFont;
            brickCounterLabelStyle.font = retroUiFont;
            brickCounterValueStyle.font = retroUiFont;
            upgradePanelStackStyle.font = retroUiFont;
            upgradeTooltipTitleStyle.font = retroUiFont;
            upgradeTooltipBodyStyle.font = retroUiFont;
            floatingScoreStyle.font = retroUiFont;
            floatingScoreTagStyle.font = retroUiFont;
            capsuleMadnessStyle.font = retroUiFont;
        }

        private void DrawBallSpeedMeter(BreakoutUiSpeedMeterView view)
        {
            if (view == null)
            {
                return;
            }

            var gaugeSize = 112f;
            var gaugeX = Mathf.Clamp(
                view.BounceZoneLeftScreen - gaugeSize - 18f,
                12f,
                Mathf.Max(12f, Screen.width - gaugeSize - 12f));
            var gaugeY = view.IsDiagnosticsVisible ? 86f : 104f;
            var gaugeRect = new Rect(gaugeX, gaugeY, gaugeSize, gaugeSize);
            var center = gaugeRect.center;
            var progress = Mathf.Clamp01(view.SpeedRatio);
            var pulse = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * Mathf.Lerp(2.2f, 7.4f, progress)));
            var leadColor = ResolveSpeedGaugeColor(progress);
            var activeTickCount = Mathf.Clamp(Mathf.CeilToInt(progress * SpeedGaugeTickCount), 1, SpeedGaugeTickCount);
            var radius = gaugeSize * 0.42f;

            DrawSpeedGaugeGlow(center, radius, leadColor, progress, pulse);
            DrawSpeedGaugeTicks(center, radius, activeTickCount, progress, pulse);
            DrawSpeedGaugeNeedle(center, radius, progress, leadColor);

            var valueStyle = new GUIStyle(speedMeterValueStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
            };
            var captionStyle = new GUIStyle(speedMeterCaptionStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
            };

            DrawTextWithShadow(new Rect(gaugeRect.x, center.y - 15f, gaugeRect.width, 24f), $"{view.Speed:0.00}u", valueStyle, palette.TextPrimary, 0.28f);
            DrawTextWithShadow(new Rect(gaugeRect.x, center.y + 8f, gaugeRect.width, 16f), "SPD", captionStyle, WithAlpha(leadColor, 0.9f), 0.2f);
        }

        private void DrawSpeedGaugeGlow(Vector2 center, float radius, Color leadColor, float progress, float pulse)
        {
            var glowAlpha = Mathf.Lerp(0.08f, 0.2f, pulse) + (progress * 0.08f);

            for (var index = 0; index < 24; index++)
            {
                var angle = SpeedGaugeStartAngle + (SpeedGaugeSweepAngle * (index / 23f));
                DrawRadialTick(center, radius + 10f + (pulse * 3f), angle, 6f, 18f, WithAlpha(leadColor, glowAlpha * 0.32f));
            }

            for (var index = 0; index < 18; index++)
            {
                var angle = index * (360f / 18f);
                DrawRadialTick(center, radius * 0.45f, angle, 2f, 8f, WithAlpha(leadColor, 0.08f + (pulse * 0.04f)));
            }
        }

        private void DrawSpeedGaugeTicks(Vector2 center, float radius, int activeTickCount, float progress, float pulse)
        {
            for (var index = 0; index < SpeedGaugeTickCount; index++)
            {
                var tickProgress = index / (float)(SpeedGaugeTickCount - 1);
                var isActive = index < activeTickCount;
                var isMajor = index % 6 == 0;
                var tickColor = isActive
                    ? ResolveSpeedGaugeColor(tickProgress)
                    : WithAlpha(palette.TextMuted, 0.32f);
                var tickAlpha = isActive ? Mathf.Lerp(0.72f, 1f, pulse) : 0.2f;
                var tickWidth = isMajor ? 3.2f : 2.1f;
                var tickHeight = isActive
                    ? (isMajor ? 14f : 11f) + (pulse * Mathf.Lerp(0.5f, 2f, progress))
                    : isMajor ? 9f : 7f;
                var angle = SpeedGaugeStartAngle + (SpeedGaugeSweepAngle * tickProgress);

                DrawRadialTick(center, radius, angle, tickWidth, tickHeight, WithAlpha(tickColor, tickAlpha));
            }
        }

        private void DrawSpeedGaugeNeedle(Vector2 center, float radius, float progress, Color leadColor)
        {
            var angle = SpeedGaugeStartAngle + (SpeedGaugeSweepAngle * progress);
            DrawRadialTick(center, radius * 0.73f, angle, 3f, 24f, WithAlpha(leadColor, 0.78f));
            DrawRadialTick(center, radius * 0.34f, angle, 2f, 13f, WithAlpha(palette.TextPrimary, 0.42f));
        }

        private static void DrawRadialTick(Vector2 center, float radius, float angleDegrees, float width, float height, Color color)
        {
            var previousMatrix = GUI.matrix;
            var tickRect = new Rect(center.x - (width * 0.5f), center.y - radius - height, width, height);
            GUIUtility.RotateAroundPivot(angleDegrees, center);
            DrawSolidRect(tickRect, color);
            GUI.matrix = previousMatrix;
        }

        private static Color ResolveSpeedGaugeColor(float progress)
        {
            var green = new Color(0.36f, 1f, 0.55f, 1f);
            var yellow = new Color(1f, 0.88f, 0.24f, 1f);
            var red = new Color(1f, 0.19f, 0.18f, 1f);
            var t = Mathf.Clamp01(progress);

            return t < 0.58f
                ? Color.Lerp(green, yellow, Mathf.SmoothStep(0f, 1f, t / 0.58f))
                : Color.Lerp(yellow, red, Mathf.SmoothStep(0f, 1f, (t - 0.58f) / 0.42f));
        }

        private void DrawIntensityGauge(Rect rect, BreakoutUiIntensityGaugeView view)
        {
            if (view == null || !view.IsVisible)
            {
                return;
            }

            var progress = Mathf.Clamp01(view.Progress);
            var gaugeColor = view.Color;
            gaugeColor.a = 1f;
            var pulseRate = Mathf.Max(0.5f, view.PulseRate);
            var pulse = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * pulseRate * Mathf.PI * 2f));
            var glowAlpha = Mathf.Lerp(0.05f, 0.18f, pulse) + (progress * 0.07f);
            var center = rect.center;
            var radius = Mathf.Min(rect.width, rect.height) * 0.42f;
            var activeTickCount = Mathf.CeilToInt(Mathf.Lerp(1f, 40f, progress));

            DrawSolidRect(Inflate(rect, Mathf.Lerp(4f, 12f, pulse)), WithAlpha(gaugeColor, glowAlpha));
            DrawSolidRect(new Rect(center.x - 25f, center.y - 25f, 50f, 50f), WithAlpha(palette.BezelDark, 0.92f));
            DrawOutline(new Rect(center.x - 25f, center.y - 25f, 50f, 50f), WithAlpha(gaugeColor, 0.75f + (pulse * 0.25f)), 2f);

            const int tickCount = 40;

            for (var index = 0; index < tickCount; index++)
            {
                var isActive = index < activeTickCount;
                var tickAlpha = isActive ? Mathf.Lerp(0.58f, 1f, pulse) : 0.16f;
                var tickHeight = isActive ? Mathf.Lerp(7f, 11f, pulse) : 5f;
                var tickRect = new Rect(center.x - 1.5f, center.y - radius - tickHeight, 3f, tickHeight);
                var previousMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(index * (360f / tickCount), center);
                DrawSolidRect(tickRect, WithAlpha(isActive ? gaugeColor : palette.TextMuted, tickAlpha));
                GUI.matrix = previousMatrix;
            }

            for (var index = 0; index < 8; index++)
            {
                var tickRect = new Rect(center.x - 1f, center.y - radius - 16f - (pulse * 6f), 2f, 5f + (pulse * 7f));
                var previousMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(index * 45f, center);
                DrawSolidRect(tickRect, WithAlpha(gaugeColor, Mathf.Lerp(0.05f, 0.18f, pulse)));
                GUI.matrix = previousMatrix;
            }

            DrawTextWithShadow(new Rect(rect.x, rect.y + 5f, rect.width, 18f), "HEAT", speedMeterCaptionStyle, gaugeColor, 0.25f);
            DrawTextWithShadow(new Rect(rect.x, center.y - 16f, rect.width, 34f), $"{Mathf.Clamp(view.Intensity, 1, Mathf.Max(1, view.MaxIntensity)):00}", pickupStyle, palette.TextPrimary, 0.28f);
        }

        private float DrawHudReadout(Rect rect, BreakoutUiHudView view)
        {
            var scoreText = !string.IsNullOrWhiteSpace(view.ScoreText) ? view.ScoreText : view.TopLine;
            var lifeCount = Mathf.Clamp(view.LifeCount, 0, 99);
            var hasLadder = view.StageLadder != null && view.StageLadder.IsVisible;
            var dividerGap = 13f;
            var dividerWidth = 1.5f;
            var iconGap = lifeCount > 8 ? 3f : 5f;
            var minIconSize = 8f;
            var maxIconSize = 18f;
            var minLifeWidth = lifeCount > 0
                ? (lifeCount * minIconSize) + (Mathf.Max(0, lifeCount - 1) * iconGap)
                : minIconSize;
            var dividerTotalWidth = (dividerGap * (hasLadder ? 4f : 2f)) + (dividerWidth * (hasLadder ? 2f : 1f));
            var measuredScoreWidth = hudStyle.CalcSize(new GUIContent(scoreText)).x;
            var availableScoreWidth = rect.width - minLifeWidth - dividerTotalWidth;
            var scoreWidth = Mathf.Min(measuredScoreWidth, Mathf.Max(64f, availableScoreWidth));
            var x = rect.x;
            var scoreRect = new Rect(x, rect.y, scoreWidth, rect.height);

            if (previousHudScoreValue != int.MinValue && view.ScoreValue > previousHudScoreValue)
            {
                scorePopStartTime = Time.unscaledTime;
            }

            previousHudScoreValue = view.ScoreValue;

            DrawScoreReadout(scoreRect, scoreText);
            x += scoreWidth + dividerGap;
            DrawHudDivider(new Rect(x, rect.y + 3f, dividerWidth, rect.height - 6f));
            x += dividerWidth + dividerGap;

            var availableLifeWidth = Mathf.Max(minIconSize, rect.xMax - x - (hasLadder ? dividerGap + dividerWidth : 0f));
            var iconSize = lifeCount > 0
                ? Mathf.Clamp((availableLifeWidth - (Mathf.Max(0, lifeCount - 1) * iconGap)) / lifeCount, minIconSize, maxIconSize)
                : minIconSize;
            var lifeWidth = lifeCount > 0
                ? (lifeCount * iconSize) + (Mathf.Max(0, lifeCount - 1) * iconGap)
                : minIconSize;
            var lifeRect = new Rect(x, rect.y + 2f, lifeWidth, rect.height - 4f);

            if (previousHudLifeCount >= 0 && previousHudLifeCount - lifeCount == 1)
            {
                var lostIconIndex = Mathf.Clamp(lifeCount, 0, Mathf.Max(0, previousHudLifeCount - 1));
                var lostIconRect = new Rect(
                    lifeRect.x + (lostIconIndex * (iconSize + iconGap)),
                    lifeRect.y + ((lifeRect.height - iconSize) * 0.5f),
                    iconSize,
                    iconSize);
                BeginLifeLossAnimation(lostIconRect, view, iconSize);
            }

            previousHudLifeCount = lifeCount;

            DrawLifeIndicators(lifeRect, view, iconSize, iconGap);
            x += lifeWidth;

            if (!hasLadder)
            {
                return x;
            }

            x += dividerGap;
            DrawHudDivider(new Rect(x, rect.y + 3f, dividerWidth, rect.height - 6f));
            return x + dividerWidth;
        }

        private void DrawScoreReadout(Rect rect, string scoreText)
        {
            var popAge = Time.unscaledTime - scorePopStartTime;
            var pop = Mathf.Clamp01(1f - (popAge / ScorePopDuration));
            var wave = Mathf.Sin(pop * Mathf.PI);
            var baseColor = pop > 0f
                ? Color.Lerp(palette.TextPrimary, palette.AccentWarm, wave * 0.5f)
                : palette.TextPrimary;

            if (pop > 0f)
            {
                DrawSolidRect(Inflate(rect, Mathf.Lerp(2f, 8f, wave)), WithAlpha(palette.AccentWarm, 0.08f * wave));
            }

            DrawTextWithShadow(rect, scoreText, hudStyle, baseColor, 0.35f + (0.18f * wave));

            if (pop <= 0f)
            {
                return;
            }

            var previousMatrix = GUI.matrix;
            var scale = Mathf.Lerp(1f, 1.18f, wave);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
            DrawTextWithShadow(rect, scoreText, hudStyle, WithAlpha(palette.AccentWarm, 0.72f * wave), 0.2f * wave);
            GUI.matrix = previousMatrix;
        }

        private void DrawLifeIndicators(Rect rect, BreakoutUiHudView view, float iconSize, float iconGap)
        {
            var lifeCount = Mathf.Clamp(view.LifeCount, 0, 99);

            for (var index = 0; index < lifeCount; index++)
            {
                var iconRect = new Rect(
                    rect.x + (index * (iconSize + iconGap)),
                    rect.y + ((rect.height - iconSize) * 0.5f),
                    iconSize,
                    iconSize);
                DrawLifeIndicator(iconRect, view.LifeIcon, view.LifeIconColor);
            }
        }

        private void BeginLifeLossAnimation(Rect startRect, BreakoutUiHudView view, float iconSize)
        {
            var target = view.HasPaddleScreenTarget
                ? view.PaddleScreenTarget
                : new Vector2(Screen.width * 0.5f, Screen.height - 72f);
            target.x = Mathf.Clamp(target.x, 24f, Screen.width - 24f);
            target.y = Mathf.Clamp(target.y, 72f, Screen.height - 24f);

            lifeLossAnimation = new LifeLossHudAnimation
            {
                IsActive = true,
                StartTime = Time.unscaledTime,
                StartCenter = startRect.center,
                TargetCenter = target,
                IconSize = Mathf.Clamp(iconSize * 1.2f, 14f, 24f),
                Icon = view.LifeIcon,
                Color = view.LifeIconColor,
            };
        }

        private void DrawLifeLossAnimation()
        {
            if (!lifeLossAnimation.IsActive)
            {
                return;
            }

            var age = Time.unscaledTime - lifeLossAnimation.StartTime;
            var progress = Mathf.Clamp01(age / LifeLossAnimationDuration);

            if (progress >= 1f)
            {
                lifeLossAnimation.IsActive = false;
                return;
            }

            var alpha = progress > 0.82f ? Mathf.Lerp(1f, 0f, (progress - 0.82f) / 0.18f) : 1f;
            var size = lifeLossAnimation.IconSize * Mathf.Lerp(1f, 1.18f, Mathf.Sin(progress * Mathf.PI));
            var rotation = progress * 760f;
            var position = ResolveLifeLossAnimationPosition(progress);

            for (var ghost = 2; ghost >= 1; ghost--)
            {
                var ghostProgress = Mathf.Clamp01(progress - (ghost * 0.055f));
                var ghostPosition = ResolveLifeLossAnimationPosition(ghostProgress);
                var ghostSize = size * Mathf.Lerp(0.72f, 0.9f, ghostProgress);
                var ghostRect = new Rect(ghostPosition.x - (ghostSize * 0.5f), ghostPosition.y - (ghostSize * 0.5f), ghostSize, ghostSize);
                DrawLifeIndicator(ghostRect, lifeLossAnimation.Icon, WithAlpha(lifeLossAnimation.Color, alpha * (0.16f / ghost)), rotation - (ghost * 34f));
            }

            DrawSolidRect(new Rect(position.x - 2f, position.y - 2f, 4f, 4f), WithAlpha(palette.AccentWarm, alpha * 0.35f));
            var iconRect = new Rect(position.x - (size * 0.5f), position.y - (size * 0.5f), size, size);
            DrawLifeIndicator(iconRect, lifeLossAnimation.Icon, WithAlpha(lifeLossAnimation.Color, alpha), rotation);
        }

        private Vector2 ResolveLifeLossAnimationPosition(float progress)
        {
            var start = lifeLossAnimation.StartCenter;
            var target = lifeLossAnimation.TargetCenter;
            var edgeX = target.x < Screen.width * 0.5f ? 24f : Screen.width - 24f;
            var edgeStart = new Vector2(edgeX, start.y);
            var edgeDrop = new Vector2(edgeX, Mathf.Max(start.y + 46f, target.y - 18f));

            if (progress < 0.24f)
            {
                return Vector2.Lerp(start, edgeStart, Smooth01(progress / 0.24f));
            }

            if (progress < 0.72f)
            {
                return Vector2.Lerp(edgeStart, edgeDrop, Smooth01((progress - 0.24f) / 0.48f));
            }

            return Vector2.Lerp(edgeDrop, target, Smooth01((progress - 0.72f) / 0.28f));
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - (2f * value));
        }

        private void DrawLifeIndicator(Rect rect, Sprite icon, Color accent, float rotationDegrees = 0f)
        {
            var previousMatrix = GUI.matrix;

            if (!Mathf.Approximately(rotationDegrees, 0f))
            {
                GUIUtility.RotateAroundPivot(rotationDegrees, rect.center);
            }

            DrawSolidRect(Inflate(rect, 2f), WithAlpha(accent, 0.08f));

            var iconTexture = ResolveIconTexture(icon, out var textureCoords);

            if (iconTexture == null)
            {
                DrawFallbackLifeIcon(rect, accent);
            }
            else
            {
                var previousGuiColor = GUI.color;
                GUI.color = accent;
                GUI.DrawTextureWithTexCoords(rect, iconTexture, textureCoords, true);
                GUI.color = previousGuiColor;
            }

            GUI.matrix = previousMatrix;
        }

        private void DrawHudDivider(Rect rect)
        {
            DrawSolidRect(rect, WithAlpha(palette.AccentPrimary, 0.42f));
            DrawSolidRect(new Rect(rect.x + 2f, rect.y, 1f, rect.height), WithAlpha(palette.AccentSecondary, 0.18f));
        }

        private void DrawStageLadder(Rect rect, BreakoutUiStageLadderView view)
        {
            if (view == null || !view.IsVisible)
            {
                return;
            }

            var totalStages = Mathf.Clamp(view.TotalStages, 1, 20);
            var currentStage = Mathf.Clamp(view.CurrentStage, 1, totalStages);
            var completedStages = Mathf.Clamp(view.CompletedStages, 0, totalStages);
            var trackX = rect.x;
            var trackWidth = rect.width;
            var gap = 5f;
            var stepWidth = Mathf.Max(10f, (trackWidth - (gap * (totalStages - 1))) / totalStages);
            var stepHeight = rect.height;
            var stepY = rect.y;

            for (var index = 0; index < totalStages; index++)
            {
                var stepRect = new Rect(trackX + (index * (stepWidth + gap)), stepY, stepWidth, stepHeight);
                var stageNumber = index + 1;
                var isComplete = stageNumber <= completedStages;
                var isCurrent = stageNumber == currentStage;
                var fillColor = isComplete
                    ? palette.Success
                    : isCurrent
                        ? palette.AccentWarm
                        : WithAlpha(palette.TextMuted, 0.18f);
                var outlineColor = isCurrent
                    ? palette.AccentWarm
                    : isComplete
                        ? palette.Success
                        : WithAlpha(palette.TextMuted, 0.26f);

                DrawSolidRect(stepRect, WithAlpha(fillColor, isCurrent ? 0.9f : isComplete ? 0.72f : 0.34f));
                DrawOutline(stepRect, outlineColor, isCurrent ? 2f : 1f);
            }
        }

        private void DrawProgressionLadderPanel(Rect rect, BreakoutUiProgressionView view, Action onStartClicked)
        {
            var innerX = rect.x + 18f;
            var width = rect.width - 36f;
            var y = rect.y + 16f;
            var buttonHeight = 42f;
            var buttonY = rect.yMax - buttonHeight - 18f;
            DrawSectionLabel(new Rect(innerX, y, width, 20f), view.LadderTitle, palette.AccentWarm);
            y += 32f;

            for (var index = 0; index < view.LadderLines.Length; index++)
            {
                DrawTextWithShadow(
                    new Rect(innerX, y, width, 24f),
                    view.LadderLines[index],
                    index == 0 ? hudStyle : setupHintStyle,
                    index == 0 ? palette.TextPrimary : palette.TextMuted,
                    0.26f);
                y += index == 0 ? 30f : 25f;
            }

            y += 18f;
            var gaugeSize = Mathf.Clamp(width * 0.52f, 138f, 188f);
            var gaugeRect = new Rect(innerX + ((width - gaugeSize) * 0.5f), y, gaugeSize, gaugeSize);
            DrawIntensityGauge(gaugeRect, view.IntensityGauge);
            y = gaugeRect.yMax + 18f;

            DrawSelectionBar(new Rect(innerX, y, width, 36f));
            DrawTextWithShadow(new Rect(innerX + 12f, y + 6f, width - 24f, 24f), view.NextSignal, setupHintStyle, palette.TextPrimary, 0.25f);

            if (DrawArcadeButton(new Rect(innerX, buttonY, Mathf.Min(168f, width), buttonHeight), "Start", true))
            {
                onStartClicked?.Invoke();
            }
        }

        private void DrawProgressionContentGrid(Rect rect, BreakoutUiProgressionCardView[] cards)
        {
            DrawSectionLabel(new Rect(rect.x + 18f, rect.y + 16f, rect.width - 36f, 20f), "Drops And Glitches", palette.AccentPrimary);

            if (cards == null || cards.Length == 0)
            {
                return;
            }

            var gridX = rect.x + 18f;
            var gridY = rect.y + 48f;
            var gridWidth = rect.width - 36f;
            var gridHeight = Mathf.Max(120f, rect.yMax - gridY - 18f);
            var gap = 14f;
            var cardHeight = 124f;
            var needsScroll = cards.Length * (cardHeight + gap) > gridHeight;
            var scrollbarWidth = needsScroll ? 18f : 0f;
            var cardWidth = gridWidth - scrollbarWidth;
            var contentHeight = Mathf.Max(gridHeight, (cards.Length * cardHeight) + ((cards.Length - 1) * gap));
            var viewportRect = new Rect(gridX, gridY, gridWidth, gridHeight);
            var contentRect = new Rect(0f, 0f, cardWidth, contentHeight);

            progressionContentScroll.y = Mathf.Clamp(progressionContentScroll.y, 0f, Mathf.Max(0f, contentHeight - gridHeight));
            progressionContentScroll = GUI.BeginScrollView(viewportRect, progressionContentScroll, contentRect, false, needsScroll);

            for (var index = 0; index < cards.Length; index++)
            {
                var cardRect = new Rect(
                    0f,
                    index * (cardHeight + gap),
                    cardWidth,
                    cardHeight);
                DrawProgressionCard(cardRect, cards[index]);
            }

            GUI.EndScrollView();

            if (needsScroll)
            {
                var scrollRatio = contentHeight <= gridHeight
                    ? 0f
                    : progressionContentScroll.y / (contentHeight - gridHeight);
                var trackRect = new Rect(rect.xMax - 22f, gridY, 3f, gridHeight);
                var thumbHeight = Mathf.Max(38f, gridHeight * (gridHeight / contentHeight));
                var thumbY = Mathf.Lerp(trackRect.y, trackRect.yMax - thumbHeight, scrollRatio);
                DrawSolidRect(trackRect, WithAlpha(palette.TextMuted, 0.16f));
                DrawSolidRect(new Rect(trackRect.x - 2f, thumbY, 7f, thumbHeight), WithAlpha(palette.AccentPrimary, 0.72f));
            }

            DrawProgressionBadgeTooltip();
        }

        private void DrawProgressionCard(Rect rect, BreakoutUiProgressionCardView card)
        {
            var accent = ResolveProgressionCardAccent(card);
            var emphasize = card.UnlockState == BreakoutUiProgressionUnlockState.Unlocked;
            DrawPanel(rect, accent, palette.AccentPrimary, emphasize, emphasize ? 2f : 1f);

            if (card.UnlockState == BreakoutUiProgressionUnlockState.SeenLocked)
            {
                DrawSolidRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), WithAlpha(palette.BezelDark, 0.28f));
            }
            else if (card.UnlockState == BreakoutUiProgressionUnlockState.HiddenLocked)
            {
                DrawSolidRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), WithAlpha(palette.BezelDark, 0.48f));
                DrawScanlines(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f), 5f, WithAlpha(palette.AccentSecondary, 0.035f));
            }

            var stateColor = card.UnlockState == BreakoutUiProgressionUnlockState.Default
                ? palette.Success
                : card.UnlockState == BreakoutUiProgressionUnlockState.Unlocked
                    ? palette.AccentWarm
                    : WithAlpha(palette.TextMuted, 0.82f);
            var iconRect = new Rect(rect.x + 16f, rect.y + 24f, 76f, 76f);
            var textX = iconRect.xMax + 18f;
            var textWidth = rect.xMax - textX - 16f;
            DrawProgressionUnlockIcon(iconRect, card.Icon, card.Icon != null ? card.IconColor : accent);
            DrawTextWithShadow(new Rect(textX, rect.y + 12f, textWidth, 24f), card.Title, hudStyle, palette.TextPrimary, 0.24f);
            DrawProgressionPropertyBadges(new Rect(textX, rect.y + 42f, textWidth, 24f), card, accent, stateColor);
            DrawTextWithShadow(new Rect(textX, rect.y + 70f, textWidth, rect.height - 84f), card.Description, progressionCardDescriptionStyle, palette.TextMuted, 0.15f);
        }

        private void DrawProgressionUnlockIcon(Rect rect, Sprite icon, Color accent)
        {
            if (icon != null)
            {
                DrawIconTile(rect, icon, accent, false);
                return;
            }

            DrawSolidRect(Inflate(rect, 3f), WithAlpha(accent, 0.06f));
            DrawSolidRect(rect, WithAlpha(palette.BezelDark, 0.62f));
            DrawOutline(rect, WithAlpha(accent, 0.45f), 1.5f);
            DrawOutline(new Rect(rect.x + 9f, rect.y + 9f, rect.width - 18f, rect.height - 18f), WithAlpha(accent, 0.18f), 1f);
            DrawScanlines(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), 6f, WithAlpha(accent, 0.035f));
        }

        private void DrawProgressionPropertyBadges(Rect rect, BreakoutUiProgressionCardView card, Color accent, Color stateColor)
        {
            const float badgeWidth = 54f;
            const float badgeHeight = 22f;
            const float gap = 8f;

            var x = rect.x;
            x = DrawProgressionBadge(new Rect(x, rect.y, badgeWidth, badgeHeight), ResolveProgressionKindIcon(card.Kind), accent, ResolveProgressionKindLabel(card.Kind)) + gap;
            x = DrawProgressionBadge(new Rect(x, rect.y, badgeWidth, badgeHeight), ResolveProgressionRarityIcon(card.Family), accent, ResolveProgressionRarityLabel(card.Family)) + gap;

            var familyLabel = ResolveProgressionContentFamilyLabel(card.Family);
            if (!string.IsNullOrWhiteSpace(familyLabel))
            {
                x = DrawProgressionBadge(new Rect(x, rect.y, badgeWidth, badgeHeight), ResolveProgressionContentFamilyIcon(familyLabel), accent, familyLabel) + gap;
            }

            x = DrawProgressionBadge(new Rect(x, rect.y, badgeWidth, badgeHeight), ResolveProgressionPolarityIcon(card), ResolveProgressionPolarityColor(card, accent), ResolveProgressionPolarityLabel(card)) + gap;
            DrawProgressionBadge(new Rect(x, rect.y, badgeWidth + 10f, badgeHeight), ResolveProgressionStateIcon(card.UnlockState), stateColor, ResolveProgressionStateTooltip(card));
        }

        private float DrawProgressionBadge(Rect rect, string text, Color accent, string tooltip)
        {
            DrawSolidRect(Inflate(rect, 2f), WithAlpha(accent, 0.08f));
            DrawSolidRect(rect, WithAlpha(palette.BezelDark, 0.58f));
            DrawOutline(rect, WithAlpha(accent, 0.62f), 1f);
            DrawTextWithShadow(rect, ToArcadeLabel(text), progressionBadgeStyle, accent, 0.16f);
            GUI.Label(rect, new GUIContent(string.Empty, tooltip ?? string.Empty), GUIStyle.none);
            return rect.xMax;
        }

        private void DrawProgressionBadgeTooltip()
        {
            if (string.IsNullOrWhiteSpace(GUI.tooltip))
            {
                return;
            }

            var mousePosition = Event.current?.mousePosition ?? Vector2.zero;
            var tooltipWidth = Mathf.Min(360f, Screen.width - 36f);
            var tooltipHeight = 58f;
            var tooltipX = Mathf.Clamp(mousePosition.x + 18f, 18f, Mathf.Max(18f, Screen.width - tooltipWidth - 18f));
            var tooltipY = Mathf.Clamp(mousePosition.y + 20f, 18f, Mathf.Max(18f, Screen.height - tooltipHeight - 18f));
            var tooltipRect = new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight);

            DrawPanel(tooltipRect, palette.AccentPrimary, palette.AccentSecondary, false, 1.5f);
            DrawTextWithShadow(new Rect(tooltipRect.x + 14f, tooltipRect.y + 9f, tooltipRect.width - 28f, tooltipRect.height - 18f), GUI.tooltip, setupHintStyle, palette.TextPrimary, 0.18f);
        }

        private static string ResolveProgressionKindIcon(string kind)
        {
            return string.Equals(kind, "Glitch", StringComparison.OrdinalIgnoreCase) ? "G" : "D";
        }

        private static string ResolveProgressionKindLabel(string kind)
        {
            return string.Equals(kind, "Glitch", StringComparison.OrdinalIgnoreCase) ? "Glitch" : "Drop";
        }

        private static string ResolveProgressionRarityIcon(string family)
        {
            return ResolveProgressionRarityLabel(family) switch
            {
                "Common" => "C",
                "Uncommon" => "U",
                "Rare" => "R",
                "Epic" => "E",
                _ => "-",
            };
        }

        private static string ResolveProgressionRarityLabel(string family)
        {
            if (string.IsNullOrWhiteSpace(family))
            {
                return "No rarity";
            }

            var trimmedFamily = family.Trim();

            if (trimmedFamily.StartsWith("Common", StringComparison.OrdinalIgnoreCase))
            {
                return "Common";
            }

            if (trimmedFamily.StartsWith("Uncommon", StringComparison.OrdinalIgnoreCase))
            {
                return "Uncommon";
            }

            if (trimmedFamily.StartsWith("Rare", StringComparison.OrdinalIgnoreCase))
            {
                return "Rare";
            }

            if (trimmedFamily.StartsWith("Epic", StringComparison.OrdinalIgnoreCase))
            {
                return "Epic";
            }

            return "No rarity";
        }

        private static string ResolveProgressionContentFamilyIcon(string family)
        {
            return string.IsNullOrWhiteSpace(family) ? "-" : family.Trim().Substring(0, 1);
        }

        private static string ResolveProgressionContentFamilyLabel(string family)
        {
            if (string.IsNullOrWhiteSpace(family))
            {
                return string.Empty;
            }

            var trimmedFamily = family.Trim();
            var tokens = trimmedFamily.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var familyTokens = new List<string>();

            for (var index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];

                if (IsProgressionRarityToken(token) || IsProgressionPolarityToken(token))
                {
                    continue;
                }

                familyTokens.Add(token);
            }

            return familyTokens.Count == 0 ? string.Empty : string.Join(" ", familyTokens);
        }

        private static bool IsProgressionRarityToken(string token)
        {
            return string.Equals(token, "Common", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Uncommon", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Rare", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Epic", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsProgressionPolarityToken(string token)
        {
            return string.Equals(token, "Helpful", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Hazard", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveProgressionPolarityIcon(BreakoutUiProgressionCardView card)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.Family))
            {
                return "-";
            }

            if (card.Family.IndexOf("Hazard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "!";
            }

            if (card.Family.IndexOf("Helpful", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(card.Kind, "Drop", StringComparison.OrdinalIgnoreCase))
            {
                return "+";
            }

            return "-";
        }

        private static string ResolveProgressionPolarityLabel(BreakoutUiProgressionCardView card)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.Family))
            {
                return "Neutral";
            }

            if (card.Family.IndexOf("Hazard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Hazard";
            }

            if (card.Family.IndexOf("Helpful", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(card.Kind, "Drop", StringComparison.OrdinalIgnoreCase))
            {
                return "Helpful";
            }

            return "Neutral";
        }

        private Color ResolveProgressionPolarityColor(BreakoutUiProgressionCardView card, Color fallback)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.Family))
            {
                return fallback;
            }

            if (card.Family.IndexOf("Hazard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return palette.Danger;
            }

            if (card.Family.IndexOf("Helpful", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(card.Kind, "Drop", StringComparison.OrdinalIgnoreCase))
            {
                return palette.Success;
            }

            return fallback;
        }

        private static string ResolveProgressionStateIcon(BreakoutUiProgressionUnlockState state)
        {
            return state switch
            {
                BreakoutUiProgressionUnlockState.Default => "DEF",
                BreakoutUiProgressionUnlockState.Unlocked => "ON",
                BreakoutUiProgressionUnlockState.SeenLocked => "LOCK",
                BreakoutUiProgressionUnlockState.HiddenLocked => "???",
                _ => "?",
            };
        }

        private static string ResolveProgressionStateTooltip(BreakoutUiProgressionCardView card)
        {
            if (card == null)
            {
                return "Unknown";
            }

            return string.IsNullOrWhiteSpace(card.StateLabel) ? "Unknown" : card.StateLabel;
        }

        private Color ResolveProgressionCardAccent(BreakoutUiProgressionCardView card)
        {
            if (card == null)
            {
                return palette.TextMuted;
            }

            if (card.UnlockState == BreakoutUiProgressionUnlockState.HiddenLocked)
            {
                return WithAlpha(palette.TextMuted, 0.48f);
            }

            if (card.UnlockState == BreakoutUiProgressionUnlockState.SeenLocked)
            {
                return WithAlpha(card.Accent, 0.58f);
            }

            return card.Accent;
        }

        private void DrawActionGroupRows(
            string[] labels,
            string[] groupLabels,
            int selectedIndex,
            Rect rect,
            Action<int> onActionClicked)
        {
            if (labels == null || labels.Length == 0)
            {
                return;
            }

            var groupRanges = BuildSequentialGroupRanges(groupLabels, labels.Length);
            var groupGap = 18f;
            var availableHeight = rect.height - (groupGap * Mathf.Max(0, groupRanges.Count - 1));
            var singleHeight = availableHeight * 0.36f;
            var multiplayerHeight = availableHeight * 0.28f;
            var settingsHeight = availableHeight - singleHeight - multiplayerHeight;
            var currentY = rect.y;

            for (var groupIndex = 0; groupIndex < groupRanges.Count; groupIndex++)
            {
                var group = groupRanges[groupIndex];
                var rowHeight = groupIndex == 0
                    ? singleHeight
                    : groupIndex == 1
                        ? multiplayerHeight
                        : settingsHeight;
                var rowRect = new Rect(rect.x, currentY, rect.width, Mathf.Max(94f, rowHeight));
                DrawActionGroupRow(labels, groupLabels, selectedIndex, group.StartIndex, group.Count, rowRect, onActionClicked);
                currentY = rowRect.yMax + groupGap;
            }
        }

        private void DrawActionGroupRow(
            string[] labels,
            string[] groupLabels,
            int selectedIndex,
            int startIndex,
            int count,
            Rect rect,
            Action<int> onActionClicked)
        {
            var group = groupLabels != null && startIndex < groupLabels.Length
                ? groupLabels[startIndex] ?? string.Empty
                : string.Empty;
            var accent = string.Equals(group, "Cabinet", StringComparison.Ordinal)
                ? palette.AccentWarm
                : string.Equals(group, "Multiplayer", StringComparison.Ordinal)
                    ? palette.AccentPrimary
                    : palette.AccentSecondary;

            DrawPanel(rect, accent, palette.AccentPrimary, false);
            DrawSectionLabel(new Rect(rect.x + 20f, rect.y + 14f, rect.width - 40f, 20f), group, accent);

            var buttonGap = 14f;
            var buttonY = rect.y + 48f;
            var usesCabinetGrid = string.Equals(group, "Cabinet", StringComparison.Ordinal) && count > 2;
            var columnCount = usesCabinetGrid ? 2 : count;
            var rowCount = usesCabinetGrid ? Mathf.CeilToInt(count / 2f) : 1;
            var availableButtonHeight = Mathf.Max(38f, rect.yMax - buttonY - 18f);
            var buttonHeight = usesCabinetGrid
                ? Mathf.Max(34f, (availableButtonHeight - (buttonGap * (rowCount - 1))) / rowCount)
                : availableButtonHeight;
            var buttonAreaWidth = rect.width - 40f;
            var buttonWidth = count == 1
                ? Mathf.Max(170f, (buttonAreaWidth - (buttonGap * 2f)) / 3f)
                : (buttonAreaWidth - (buttonGap * (columnCount - 1))) / columnCount;

            for (var offset = 0; offset < count; offset++)
            {
                var index = startIndex + offset;
                var isSelected = index == Mathf.Clamp(selectedIndex, 0, Math.Max(0, labels.Length - 1));
                var column = usesCabinetGrid ? offset % columnCount : offset;
                var row = usesCabinetGrid ? offset / columnCount : 0;
                var actionRect = new Rect(
                    rect.x + 20f + (column * (buttonWidth + buttonGap)),
                    buttonY + (row * (buttonHeight + buttonGap)),
                    buttonWidth,
                    buttonHeight);

                if (DrawArcadeButton(actionRect, labels[index], isSelected))
                {
                    onActionClicked?.Invoke(index);
                }
            }
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

        private void DrawGroupedActionList(
            string[] labels,
            string[] groupLabels,
            int selectedIndex,
            float x,
            float y,
            float width,
            float lineHeight,
            float groupHeight,
            Action<int> onActionClicked)
        {
            if (labels == null)
            {
                return;
            }

            var currentY = y;
            var previousGroup = string.Empty;

            for (var index = 0; index < labels.Length; index++)
            {
                var group = groupLabels != null && index < groupLabels.Length
                    ? groupLabels[index] ?? string.Empty
                    : string.Empty;

                if (!string.Equals(group, previousGroup, StringComparison.Ordinal))
                {
                    DrawSectionLabel(new Rect(x, currentY, width, 16f), group, palette.AccentWarm);
                    currentY += groupHeight;
                    previousGroup = group;
                }

                var isSelected = index == Mathf.Clamp(selectedIndex, 0, Math.Max(0, labels.Length - 1));
                var actionRect = new Rect(x, currentY, width, lineHeight - 4f);

                if (DrawArcadeButton(actionRect, labels[index], isSelected))
                {
                    onActionClicked?.Invoke(index);
                }

                currentY += lineHeight;
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

        private void DrawIconTile(Rect rect, Sprite icon, Color accent, bool emphasize)
        {
            DrawSolidRect(Inflate(rect, emphasize ? 5f : 3f), WithAlpha(accent, emphasize ? 0.16f : 0.07f));
            DrawSolidRect(rect, WithAlpha(palette.BezelDark, 0.72f));
            DrawOutline(rect, WithAlpha(accent, emphasize ? 0.75f : 0.42f), emphasize ? 2f : 1f);

            var iconTexture = ResolveIconTexture(icon, out var textureCoords);

            if (iconTexture == null)
            {
                DrawFallbackIconGlyph(new Rect(rect.x + 11f, rect.y + 11f, rect.width - 22f, rect.height - 22f), accent, emphasize);
                return;
            }

            var previousGuiColor = GUI.color;
            GUI.color = accent;
            GUI.DrawTextureWithTexCoords(new Rect(rect.x + 7f, rect.y + 7f, rect.width - 14f, rect.height - 14f), iconTexture, textureCoords, true);
            GUI.color = previousGuiColor;
        }

        private void DrawUpgradeTooltip(BreakoutUiRunUpgradePanelItemView item, Rect panelRect)
        {
            var tooltipWidth = Mathf.Min(392f, Screen.width - 36f);
            var tooltipHeight = 124f;
            var tooltipX = Mathf.Clamp(panelRect.x, 18f, Mathf.Max(18f, Screen.width - tooltipWidth - 18f));
            var tooltipY = Mathf.Max(18f, panelRect.y - tooltipHeight - 10f);
            var tooltipRect = new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
            var title = item.StackCount > 1 ? $"{item.Title} x{item.StackCount}" : item.Title;

            DrawPanel(tooltipRect, item.Accent, palette.AccentPrimary, false, 1.5f);
            DrawTextWithShadow(new Rect(tooltipRect.x + 16f, tooltipRect.y + 12f, tooltipRect.width - 32f, 20f), title, upgradeTooltipTitleStyle, palette.TextPrimary, 0.25f);
            DrawTextWithShadow(new Rect(tooltipRect.x + 16f, tooltipRect.y + 38f, tooltipRect.width - 32f, 48f), item.Description, upgradeTooltipBodyStyle, palette.TextMuted, 0.18f);
            DrawTextWithShadow(new Rect(tooltipRect.x + 16f, tooltipRect.yMax - 30f, tooltipRect.width - 32f, 18f), item.Detail, modifierPanelTimerStyle, item.Accent, 0.18f);
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

        private void DrawHintBand(Rect rect, string text, GUIStyle textStyle = null)
        {
            DrawPanel(rect, palette.AccentPrimary, palette.AccentSecondary, false, 1f);
            DrawTextWithShadow(new Rect(rect.x + 14f, rect.y + 5f, rect.width - 28f, rect.height - 10f), text, textStyle ?? setupHintStyle, palette.TextMuted, 0.2f);
        }

        private static int CountSequentialGroups(string[] groupLabels, int actionCount)
        {
            if (groupLabels == null || actionCount <= 0)
            {
                return 0;
            }

            var groupCount = 0;
            var previousGroup = string.Empty;

            for (var index = 0; index < actionCount; index++)
            {
                var group = index < groupLabels.Length
                    ? groupLabels[index] ?? string.Empty
                    : string.Empty;

                if (string.Equals(group, previousGroup, StringComparison.Ordinal))
                {
                    continue;
                }

                groupCount++;
                previousGroup = group;
            }

            return groupCount;
        }

        private static List<ActionGroupRange> BuildSequentialGroupRanges(string[] groupLabels, int actionCount)
        {
            var ranges = new List<ActionGroupRange>();

            if (actionCount <= 0)
            {
                return ranges;
            }

            var startIndex = 0;
            var previousGroup = groupLabels != null && groupLabels.Length > 0
                ? groupLabels[0] ?? string.Empty
                : string.Empty;

            for (var index = 1; index < actionCount; index++)
            {
                var group = groupLabels != null && index < groupLabels.Length
                    ? groupLabels[index] ?? string.Empty
                    : string.Empty;

                if (string.Equals(group, previousGroup, StringComparison.Ordinal))
                {
                    continue;
                }

                ranges.Add(new ActionGroupRange(startIndex, index - startIndex));
                startIndex = index;
                previousGroup = group;
            }

            ranges.Add(new ActionGroupRange(startIndex, actionCount - startIndex));
            return ranges;
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

        private static Rect GetSpriteTexCoords(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            var textureRect = sprite.textureRect;
            return new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);
        }

        private Texture ResolveIconTexture(Sprite sprite, out Rect textureCoords)
        {
            textureCoords = new Rect(0f, 0f, 1f, 1f);

            if (sprite == null)
            {
                return null;
            }

            var rasterizedTexture = ResolveRasterizedIconTexture(sprite);

            if (rasterizedTexture != null)
            {
                return rasterizedTexture;
            }

            if (sprite.texture == null || !HasVisiblePixels(sprite.texture, sprite.textureRect))
            {
                return null;
            }

            textureCoords = GetSpriteTexCoords(sprite);
            return sprite.texture;
        }

        private Texture2D ResolveRasterizedIconTexture(Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            if (iconTextureCache.TryGetValue(sprite, out var cachedTexture))
            {
                return cachedTexture;
            }

            Texture2D renderedTexture = null;
            var material = ResolveVectorIconMaterial();

            if (material != null)
            {
                try
                {
                    renderedTexture = VectorUtils.RenderSpriteToTexture2D(sprite, IconTextureSize, IconTextureSize, material, 4);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Could not rasterize UI icon sprite '{sprite.name}': {exception.Message}");
                }
            }

            if (renderedTexture != null && !HasVisiblePixels(renderedTexture))
            {
                DestroyRuntimeObject(renderedTexture);
                renderedTexture = null;
            }

            if (renderedTexture == null)
            {
                renderedTexture = RenderSpriteIconWithCamera(sprite);
            }

            if (renderedTexture != null && !HasVisiblePixels(renderedTexture))
            {
                DestroyRuntimeObject(renderedTexture);
                renderedTexture = null;
            }

            if (renderedTexture != null)
            {
                renderedTexture.name = $"{sprite.name}UiIconTexture";
                renderedTexture.hideFlags = HideFlags.DontSave;
            }

            iconTextureCache[sprite] = renderedTexture;
            return renderedTexture;
        }

        private Material ResolveVectorIconMaterial()
        {
            if (vectorIconMaterial != null)
            {
                return vectorIconMaterial;
            }

            vectorIconMaterial = BreakoutRuntimeVisualFactory.CreateMaterialFromResource(
                "Materials/RuntimeUiIconVector",
                "RuntimeUiIconVector");

            if (vectorIconMaterial != null)
            {
                return vectorIconMaterial;
            }

            var shader = Shader.Find("Unlit/Vector");

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            vectorIconMaterial = new Material(shader)
            {
                name = "RuntimeUiIconVectorMaterial",
                hideFlags = HideFlags.DontSave,
            };
            return vectorIconMaterial;
        }

        private Material ResolveSpriteIconMaterial()
        {
            if (spriteIconMaterial != null)
            {
                return spriteIconMaterial;
            }

            spriteIconMaterial = BreakoutRuntimeVisualFactory.CreateMaterialFromResource(
                "Materials/RuntimeSpriteUnlit",
                "RuntimeUiIconSpriteMaterial");

            if (spriteIconMaterial != null)
            {
                return spriteIconMaterial;
            }

            spriteIconMaterial = BreakoutRuntimeVisualFactory.CreateSpriteUnlitMaterial();
            return spriteIconMaterial;
        }

        private Texture2D RenderSpriteIconWithCamera(Sprite sprite)
        {
            var material = ResolveSpriteIconMaterial();

            if (sprite == null || material == null)
            {
                return null;
            }

            var previousActive = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(IconTextureSize, IconTextureSize, 24, RenderTextureFormat.ARGB32);
            var root = new GameObject("Runtime UI Icon Rasterizer")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            var cameraObject = new GameObject("Runtime UI Icon Camera")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            var spriteObject = new GameObject("Runtime UI Icon Sprite")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            Texture2D renderedTexture = null;

            try
            {
                cameraObject.transform.SetParent(root.transform, false);
                spriteObject.transform.SetParent(root.transform, false);

                var origin = new Vector3(10000f, 10000f, 0f);
                spriteObject.transform.position = origin;

                var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
                spriteRenderer.sharedMaterial = material;
                spriteRenderer.sortingOrder = 0;

                var boundsSize = sprite.bounds.size;
                var visibleSize = Mathf.Max(0.01f, boundsSize.x, boundsSize.y);
                var camera = cameraObject.AddComponent<Camera>();
                camera.transform.position = origin + new Vector3(0f, 0f, -10f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.orthographic = true;
                camera.orthographicSize = visibleSize * 0.58f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 30f;
                camera.allowHDR = false;
                camera.allowMSAA = true;
                camera.enabled = false;
                camera.targetTexture = renderTexture;
                camera.Render();

                RenderTexture.active = renderTexture;
                renderedTexture = new Texture2D(IconTextureSize, IconTextureSize, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave,
                };
                renderedTexture.ReadPixels(new Rect(0f, 0f, IconTextureSize, IconTextureSize), 0, 0);
                renderedTexture.Apply(false, false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not render UI icon sprite '{sprite.name}' with the SpriteRenderer fallback: {exception.Message}");

                if (renderedTexture != null)
                {
                    DestroyRuntimeObject(renderedTexture);
                    renderedTexture = null;
                }
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                DestroyRuntimeObject(root);
            }

            return renderedTexture;
        }

        private void DrawFallbackLifeIcon(Rect rect, Color accent)
        {
            var texture = ResolveFallbackLifeIconTexture();

            if (texture == null)
            {
                DrawSolidRect(Inflate(rect, -rect.width * 0.28f), WithAlpha(accent, 0.95f));
                DrawOutline(Inflate(rect, -rect.width * 0.18f), WithAlpha(palette.TextPrimary, 0.2f), 1f);
                return;
            }

            var previousGuiColor = GUI.color;
            GUI.color = WithAlpha(accent, 0.28f);
            GUI.DrawTexture(Inflate(rect, rect.width * 0.18f), texture, ScaleMode.ScaleToFit, true);
            GUI.color = accent;
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            GUI.color = WithAlpha(palette.TextPrimary, 0.26f);
            GUI.DrawTexture(Inflate(rect, -rect.width * 0.26f), texture, ScaleMode.ScaleToFit, true);
            GUI.color = previousGuiColor;
        }

        private Texture2D ResolveFallbackLifeIconTexture()
        {
            if (fallbackLifeIconTexture != null)
            {
                return fallbackLifeIconTexture;
            }

            const int size = 64;
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            var radius = size * 0.38f;
            var outerRadius = size * 0.49f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    var body = 1f - Mathf.Clamp01((distance - radius) / Mathf.Max(0.01f, outerRadius - radius));
                    var highlightDistance = Mathf.Sqrt(((x - (size * 0.36f)) * (x - (size * 0.36f))) + ((y - (size * 0.32f)) * (y - (size * 0.32f))));
                    var highlight = Mathf.Clamp01(1f - (highlightDistance / (size * 0.18f)));
                    var alpha = Mathf.Clamp01(body);
                    var value = (byte)Mathf.RoundToInt(Mathf.Lerp(180f, 255f, highlight) * alpha);
                    pixels[(y * size) + x] = new Color32(value, value, value, (byte)Mathf.RoundToInt(255f * alpha));
                }
            }

            fallbackLifeIconTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "RuntimeFallbackLifeIcon",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            fallbackLifeIconTexture.SetPixels32(pixels);
            fallbackLifeIconTexture.Apply(false, false);
            return fallbackLifeIconTexture;
        }

        private static bool HasVisiblePixels(Texture2D texture)
        {
            if (texture == null)
            {
                return false;
            }

            return HasVisiblePixels(texture, new Rect(0f, 0f, texture.width, texture.height));
        }

        private static bool HasVisiblePixels(Texture2D texture, Rect textureRect)
        {
            if (texture == null)
            {
                return false;
            }

            try
            {
                var pixels = texture.GetPixels32();
                var minX = Mathf.Clamp(Mathf.FloorToInt(textureRect.xMin), 0, texture.width - 1);
                var maxX = Mathf.Clamp(Mathf.CeilToInt(textureRect.xMax), minX + 1, texture.width);
                var minY = Mathf.Clamp(Mathf.FloorToInt(textureRect.yMin), 0, texture.height - 1);
                var maxY = Mathf.Clamp(Mathf.CeilToInt(textureRect.yMax), minY + 1, texture.height);
                var width = maxX - minX;
                var height = maxY - minY;
                var sampleCount = Mathf.Max(1, Mathf.Min(2048, width * height));
                var step = Mathf.Max(1, (width * height) / sampleCount);
                var visited = 0;

                for (var y = minY; y < maxY; y++)
                {
                    for (var x = minX; x < maxX; x++)
                    {
                        if ((visited++ % step) != 0)
                        {
                            continue;
                        }

                        var pixel = pixels[(y * texture.width) + x];

                        if (pixel.a > 8 && pixel.r + pixel.g + pixel.b > 18)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (UnityException)
            {
                return true;
            }

            return false;
        }

        private void DrawFallbackIconGlyph(Rect rect, Color accent, bool emphasize)
        {
            var center = rect.center;
            var coreSize = Mathf.Min(rect.width, rect.height) * (emphasize ? 0.56f : 0.5f);
            var coreRect = new Rect(center.x - (coreSize * 0.5f), center.y - (coreSize * 0.5f), coreSize, coreSize);
            DrawSolidRect(coreRect, WithAlpha(accent, 0.86f));
            DrawOutline(coreRect, WithAlpha(palette.TextPrimary, 0.18f), 1f);
            DrawSolidRect(new Rect(center.x - 1.5f, rect.y + 3f, 3f, rect.height - 6f), WithAlpha(palette.TextPrimary, 0.2f));
            DrawSolidRect(new Rect(rect.x + 3f, center.y - 1.5f, rect.width - 6f, 3f), WithAlpha(palette.TextPrimary, 0.2f));
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
                return;
            }

            UnityEngine.Object.DestroyImmediate(target);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static string ToArcadeLabel(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.ToUpperInvariant();
        }

        private static string FormatDraftCardDetail(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Replace(" | ", "\n");
        }

        private void DrawTextWithShadow(Rect rect, string text, GUIStyle style, Color color, float shadowAlpha = 0.5f)
        {
            var previousGuiColor = GUI.color;
            var previousTextColor = style.normal.textColor;
            style.normal.textColor = Color.white;

            GUI.color = new Color(0f, 0f, 0f, shadowAlpha);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 2f, rect.width, rect.height), text, style);
            GUI.color = color;
            GUI.Label(rect, text, style);

            style.normal.textColor = previousTextColor;
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

        private struct LifeLossHudAnimation
        {
            public bool IsActive;
            public float StartTime;
            public Vector2 StartCenter;
            public Vector2 TargetCenter;
            public float IconSize;
            public Sprite Icon;
            public Color Color;
        }
    }
}
