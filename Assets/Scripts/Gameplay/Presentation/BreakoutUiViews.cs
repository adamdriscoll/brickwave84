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
        public string[] ActionGroupLabels = Array.Empty<string>();
        public BreakoutUiMenuActionTone[] ActionTones = Array.Empty<BreakoutUiMenuActionTone>();
        public BreakoutUiMenuActionIcon[] ActionIcons = Array.Empty<BreakoutUiMenuActionIcon>();
        public int SelectedActionIndex;
        public string PreviewTitle = string.Empty;
        public string[] PreviewLines = Array.Empty<string>();
        public string ValidationText = string.Empty;
        public string FooterText = string.Empty;
        public string HintText = string.Empty;
    }

    internal enum BreakoutUiMenuActionTone
    {
        Standard,
        Danger,
    }

    internal enum BreakoutUiMenuActionIcon
    {
        None,
        Power,
    }

    internal enum BreakoutUiProgressionUnlockState
    {
        Default,
        Unlocked,
        SeenLocked,
        HiddenLocked,
    }

    internal enum BreakoutUiProgressionSortMode
    {
        UnlockLevel,
        Rarity,
        Type,
    }

    internal enum BreakoutUiProgressionRarityFilter
    {
        All,
        Common,
        Uncommon,
        Rare,
        Epic,
        NoRarity,
    }

    internal enum BreakoutUiProgressionTypeFilter
    {
        All,
        Drop,
        Glitch,
    }

    internal enum BreakoutUiProgressionLockFilter
    {
        All,
        Unlocked,
        Locked,
    }

    internal enum BreakoutUiProgressionSelectorKind
    {
        Sort,
        Rarity,
        Type,
        LockState,
    }

    internal sealed class BreakoutUiProgressionPaddleView
    {
        public string Label = string.Empty;
        public string ProgressLine = string.Empty;
        public string DetailLine = string.Empty;
        public float Progress;
        public bool IsSelected;
        public bool IsUnlocked;
    }

    internal sealed class BreakoutUiProgressionCardView
    {
        public string Title = string.Empty;
        public string Kind = string.Empty;
        public string Family = string.Empty;
        public string Description = string.Empty;
        public string UnlockHint = string.Empty;
        public string StateLabel = string.Empty;
        public string ModeAvailability = string.Empty;
        public int UnlockIntensity;
        public int RarityRank = -1;
        public string RarityLabel = string.Empty;
        public string TypeLabel = string.Empty;
        public BreakoutUiProgressionUnlockState UnlockState;
        public Color Accent = Color.white;
        public Sprite Icon;
        public Color IconColor = Color.white;
    }

    internal sealed class BreakoutUiProgressionView
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string LadderTitle = string.Empty;
        public string[] LadderLines = Array.Empty<string>();
        public string[] MeterLines = Array.Empty<string>();
        public string NextSignal = string.Empty;
        public BreakoutUiIntensityGaugeView IntensityGauge = new BreakoutUiIntensityGaugeView();
        public BreakoutUiProgressionPaddleView[] Paddles = Array.Empty<BreakoutUiProgressionPaddleView>();
        public BreakoutUiProgressionCardView[] Cards = Array.Empty<BreakoutUiProgressionCardView>();
        public BreakoutUiProgressionSortMode SortMode = BreakoutUiProgressionSortMode.UnlockLevel;
        public BreakoutUiProgressionRarityFilter RarityFilter = BreakoutUiProgressionRarityFilter.All;
        public BreakoutUiProgressionTypeFilter TypeFilter = BreakoutUiProgressionTypeFilter.All;
        public BreakoutUiProgressionLockFilter LockFilter = BreakoutUiProgressionLockFilter.All;
        public int VisibleCardCount;
        public int TotalCardCount;
        public string FooterText = string.Empty;
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

    internal sealed class BreakoutUiIntensityGaugeView
    {
        public bool IsVisible;
        public int Intensity = 1;
        public int MaxIntensity = 50;
        public float Progress;
        public float PulseRate = 2f;
        public Color Color = Color.white;
    }

    internal sealed class BreakoutUiStageLadderView
    {
        public bool IsVisible;
        public int CurrentStage = 1;
        public int TotalStages = 10;
        public int CompletedStages;
    }

    internal sealed class BreakoutUiStatsRowView
    {
        public string Label = string.Empty;
        public string Value = string.Empty;
    }

    internal sealed class BreakoutUiHudView
    {
        public string TopLine = string.Empty;
        public string ScoreText = string.Empty;
        public int ScoreValue;
        public bool IsScoreLeakActive;
        public float ScoreLeakIntensity;
        public int LifeCount;
        public Sprite LifeIcon;
        public Color LifeIconColor = Color.white;
        public int MissileCount;
        public Sprite MissileIcon;
        public Color MissileIconColor = Color.white;
        public bool HasPaddleScreenTarget;
        public Vector2 PaddleScreenTarget;
        public string BottomLine = string.Empty;
        public bool HasBrickCounter;
        public int BricksRemaining;
        public bool ShowMenuButton;
        public bool IsPaused;
        public bool IsDiagnosticsVisible;
        public BreakoutUiSpeedMeterView SpeedMeter = new BreakoutUiSpeedMeterView();
        public BreakoutUiIntensityGaugeView IntensityGauge = new BreakoutUiIntensityGaugeView();
        public BreakoutUiStageLadderView StageLadder = new BreakoutUiStageLadderView();
    }

    internal sealed class BreakoutUiOverlayView
    {
        public string Title = string.Empty;
        public string SummaryTitle = string.Empty;
        public string[] SummaryLines = Array.Empty<string>();
        public string LeaderboardTitle = string.Empty;
        public BreakoutTurnLeaderboardEntry[] LeaderboardEntries = Array.Empty<BreakoutTurnLeaderboardEntry>();
        public string[] LeaderboardLines = Array.Empty<string>();
        public string[] ActionLabels = Array.Empty<string>();
        public int SelectedActionIndex;
        public string[] FooterLines = Array.Empty<string>();
        public BreakoutUiStatsRowView[] StatsRows = Array.Empty<BreakoutUiStatsRowView>();
        public bool IsCompact;
        public bool EmphasizeSummary;
    }

    internal sealed class BreakoutUiUpgradeDraftOptionView
    {
        public string Title = string.Empty;
        public string Description = string.Empty;
        public string Detail = string.Empty;
        public Sprite Icon;
        public string IconLabel = string.Empty;
        public Color Accent = Color.white;
    }

    internal sealed class BreakoutUiUpgradeDraftView
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string BuildLine = string.Empty;
        public BreakoutUiUpgradeDraftOptionView[] Options = Array.Empty<BreakoutUiUpgradeDraftOptionView>();
        public int SelectedOptionIndex;
        public string MissilePurchaseLabel = string.Empty;
        public string MissilePurchaseDetail = string.Empty;
        public bool CanPurchaseMissile;
        public Sprite MissilePurchaseIcon;
        public Color MissilePurchaseColor = Color.white;
        public string HintText = string.Empty;
    }

    internal sealed class BreakoutUiModifierView
    {
        public string Label = string.Empty;
        public float RemainingDuration;
        public float DurationRatio = 1f;
        public Color Color = Color.white;
    }

    internal sealed class BreakoutUiRunUpgradePanelItemView
    {
        public string Label = string.Empty;
        public string Title = string.Empty;
        public string Description = string.Empty;
        public string Detail = string.Empty;
        public int StackCount = 1;
        public Sprite Icon;
        public Color Accent = Color.white;
    }

    internal sealed class BreakoutUiRunUpgradePanelView
    {
        public BreakoutUiRunUpgradePanelItemView[] Items = Array.Empty<BreakoutUiRunUpgradePanelItemView>();
        public bool IsDiagnosticsVisible;
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

    internal sealed class BreakoutUiCapsuleMadnessView
    {
        public string Text = string.Empty;
        public Rect PlayfieldRect;
        public float Timer;
        public float Duration;
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
}
