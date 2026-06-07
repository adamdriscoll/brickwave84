using System;
using System.Collections.Generic;
using System.Globalization;
using GetBricked.Gameplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GetBricked.Gameplay
{
    public sealed class BreakoutGameController : MonoBehaviour
    {
        private const string BallSpriteResourcePath = "Sprites/ball";
        private const string BrickSpriteResourcePath = "Sprites/brick";
        private const string PaddleSpriteResourcePath = "Sprites/paddle";
        private const string PowerUpSpriteResourcePath = "Sprites/powerup";
        private const string MissileSpriteResourcePath = "Sprites/brick-missile";
        private const int StartingMissileCount = 3;
        private const int MissileRewardPurchaseScoreCost = 5000;
        private const float MissileShotCooldownSeconds = 0.45f;
        private const float MissileSpeed = 10.5f;
        private const float MissileRadius = 0.045f;
        private const float MissileExplosionRadius = 1.45f;
        private const float LaserShotCooldownSeconds = 0.3f;
        private const float LaserBeamLifetimeSeconds = 0.16f;
        private const float LaserRainBaseIntervalSeconds = 1.18f;
        private const float LaserRainMinimumIntervalSeconds = 0.72f;
        private const float LaserRainWarningSeconds = 0.46f;
        private const float LaserRainBeamLifetimeSeconds = 0.2f;
        private const float LaserRainLaneHalfWidth = 0.18f;
        private const float LaserRainLaneHorizontalPadding = 0.45f;
        private const float LaserRainBrickHitPadding = 0.42f;
        private const float LaserRainBallHitPadding = 0.16f;
        private const float LaserRainBallSideKick = 0.48f;
        private const float LaserRainBallVerticalKick = 0.34f;
        private const float LaserRainBallMinimumVertical = 0.5f;
        private const int LaserRainMaximumLaneCount = 3;
        private const float ShieldWallYOffset = 0.38f;
        private const float ShieldWallThickness = 0.16f;
        private const float VectorSightPreviewRange = 2.45f;
        private const float VectorSightPreviewLength = 1.85f;
        private const float ExplosiveBallMinimumRadius = 1.25f;
        private const float ExplosiveBallMaximumRadius = 2.05f;
        private const float ExplosiveBallMinimumSpeedBurstMultiplier = 1.1f;
        private const float ExplosiveBallMaximumSpeedBurstMultiplier = 1.22f;
        private const float ExplosiveBallSpeedBurstDuration = 1.45f;
        private const int ExplosiveBrickSplitBallCount = 3;
        private const float ExplosiveBrickSplitBallSizeMultiplier = 0.55f;
        private const int BrickBloomBonusBrickCount = 2;
        private const float BrickBloomPulseRadius = 0.85f;
        private const float FuseBurstPulseRadius = 1.05f;
        private const float PrismPopCopyBallSizeMultiplier = 0.72f;
        private const float PrismPopFallbackCopyLifetimeSeconds = 4f;
        private const float PrismPopLaunchOffsetMultiplier = 1.15f;
        private const float PrismPopMinimumHorizontalDirection = 0.22f;
        private const float DoubleTapCopyBallSizeMultiplier = 0.86f;
        private const float DoubleTapLaunchOffsetMultiplier = 1.2f;
        private const float MicroSparkPopStackThreshold = 4f;
        private const float MirrorServeMinimumHorizontalDirection = 0.28f;
        private const float MirrorServeLaunchOffsetMultiplier = 1.35f;
        private const float TurboRailSpeedBurstMultiplier = 1.35f;
        private const float TurboRailSpeedBurstDuration = 4f;
        private const float TurboRailSpeedBurstStackMultiplier = 0.12f;
        private const float TurboRailSpeedBurstMaximumMultiplier = 1.85f;
        private const float TurboRailSpeedBurstStackDuration = 1.25f;
        private const float TurboRailSpeedBurstMaximumDuration = 7.5f;
        private const float MeltdownCorePulseRadius = 0.95f;
        private const int StaticServeRuleCount = 3;
        private const float StaticWallThickness = 0.22f;
        private const float StaticWallBottomInset = 1.15f;
        private const float StaticWallTopInset = 0.55f;
        private const int StaticJackpotZoneSortingOrder = 7;
        private const float BallArenaEscapeMargin = 1.1f;
        private const float ThinAirEscapeMargin = 0.55f;
        private const float GravityPocketArenaHorizontalPadding = 1.35f;
        private const float GravityPocketArenaBottomPadding = 2.1f;
        private const float GravityPocketArenaTopPadding = 1.2f;
        private const float PrismLaneBottomInset = 1.25f;
        private const float PrismLaneTopInset = 1.05f;
        private const float SplitHorizonLineThickness = 0.08f;
        private const float SplitHorizonGlowThickness = 0.34f;
        private const float SplitHorizonCooldownSeconds = 0.13f;
        private const float VhsTearLineThickness = 0.12f;
        private const float VhsTearTriggerThickness = 0.34f;
        private const float SwitchbackRailThickness = 0.22f;
        private const float SwitchbackRailBottomInset = 1.55f;
        private const float SwitchbackRailTopInset = 0.95f;
        private const float HotCornerInset = 0.18f;
        private const int AutoSaveBaseScoreCost = 10000;
        private const int AutoSaveBaseCostMaximumRogueHeat = 10;
        private const int AutoSaveScoreCostIncreasePerHeat = 1000;
        private const int AutoSaveMaximumRogueHeat = 40;
        private const float AutoSaveBurstDurationSeconds = 2.6f;
        private const float SpareFuseBurstDurationSeconds = 2.4f;
        private const float TiltWarningNearMissPaddleWidthMultiplier = 1.55f;
        private const float TiltWarningRescueHeightPadding = 0.16f;
        private const float TiltNudgeHorizontalStrength = 0.24f;
        private const string DefaultRoguePaddleLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;
        private const float MenuAttractRestartDelaySeconds = 0.2f;
        private const float MenuAttractPickupLeadDistance = 1.35f;
        private const float MenuAttractFallbackSweepSpeed = 0.55f;
        private const float MenuAttractLaunchHorizontalRange = 0.48f;
        private static readonly BreakoutRunSetupField[] HotSeatTopScoreSetupFields =
        {
            BreakoutRunSetupField.PlayerCount,
            BreakoutRunSetupField.HotSeatMode,
            BreakoutRunSetupField.HotSeatTurnLimit,
            BreakoutRunSetupField.HotSeatDifficulty,
        };

        private static readonly BreakoutRunSetupField[] HotSeatOutlastSetupFields =
        {
            BreakoutRunSetupField.PlayerCount,
            BreakoutRunSetupField.HotSeatMode,
            BreakoutRunSetupField.HotSeatLives,
            BreakoutRunSetupField.HotSeatDifficulty,
        };

        private static readonly BreakoutHotSeatDifficulty[] SoloMarathonHeatLevels =
        {
            BreakoutHotSeatDifficulty.Chill,
            BreakoutHotSeatDifficulty.Rad,
            BreakoutHotSeatDifficulty.Gnarly,
            BreakoutHotSeatDifficulty.Mondo,
            BreakoutHotSeatDifficulty.Bogus,
        };

        private const int SoloMarathonLaunchActionIndex = 5;
        private const int SoloMarathonBackActionIndex = 6;

        private enum RoundState
        {
            MainMenu,
            Progression,
            LifetimeStats,
            SoloMarathonSetup,
            RunSetup,
            DeveloperMenu,
            UpgradeDraft,
            ReadyToServe,
            Playing,
            Paused,
            LifeLost,
            LevelComplete,
            GameOver,
        }

        private enum OverlayAction
        {
            Resume,
            RestartRun,
            NextLevel,
            ReturnToRunSetup,
            ReturnToMainMenu,
            QuitGame,
        }

        private enum StaticServeRailRule
        {
            TurboRail = 0,
            StaticWall = 1,
            SwitchbackRails = 2,
        }

        private sealed class TemporaryBallLifetime
        {
            public TemporaryBallLifetime(BallController ball, float remainingSeconds)
            {
                Ball = ball;
                RemainingSeconds = Mathf.Max(0f, remainingSeconds);
            }

            public BallController Ball { get; }

            public float RemainingSeconds { get; set; }
        }

        private sealed class PendingLaserRainLane
        {
            public PendingLaserRainLane(float worldX, BreakoutLaserRainLaneVisual visual)
            {
                WorldX = worldX;
                Visual = visual;
                WarningTimer = LaserRainWarningSeconds;
            }

            public float WorldX { get; }

            public BreakoutLaserRainLaneVisual Visual { get; }

            public float WarningTimer { get; set; }
        }

        [Header("Camera")]
        [SerializeField] private float cameraHalfHeight = 5.2f;
        [SerializeField] private Color backgroundColor = new Color(0.07f, 0.03f, 0.08f, 1f);
        [SerializeField, Range(0f, 1f)] private float backgroundImageTintStrength = 0.18f;
        [SerializeField, Range(0f, 1f)] private float backgroundImageAlpha = 0.82f;
        [SerializeField, Range(0f, 1f)] private float backgroundHazeAlpha = 0.22f;
        [SerializeField, Range(0f, 1f)] private float scanlineAlpha = 0.12f;
        [SerializeField, Min(0f)] private float bloomThreshold = 0.82f;
        [SerializeField, Min(0f)] private float bloomIntensity = 0.8f;
        [SerializeField, Range(0f, 1f)] private float bloomScatter = 0.72f;

        [Header("Screen FX")]
        [SerializeField, Range(0f, 0.1f)] private float playfieldScanlineOverlayAlpha = 0.026f;
        [SerializeField, Range(0f, 0.15f)] private float playfieldScanlineSweepAlpha = 0.045f;
        [SerializeField, Range(2f, 8f)] private float playfieldScanlineSpacing = 4f;
        [SerializeField, Range(4f, 48f)] private float playfieldScanlineTravelSpeed = 18f;

        [Header("Playfield")]
        [SerializeField] private float playfieldPadding = 0.6f;
        [SerializeField, Min(1f)] private float maximumGameplayAspectRatio = BreakoutPlayfieldGeometry.DefaultMaximumGameplayAspectRatio;
        [SerializeField] private float wallThickness = 0.45f;
        [SerializeField] private Color wallColor = new Color(0.14f, 0.11f, 0.19f, 1f);

        [Header("Paddle")]
        [SerializeField] private Vector2 paddleSize = new Vector2(2.1f, 0.74f);
        [SerializeField] private float paddleSpeed = 12f;
        [SerializeField] private float paddleFloorOffset = 0.8f;
        [SerializeField] private Color paddleColor = new Color(0.56f, 0.96f, 1f, 1f);

        [Header("Ball")]
        [SerializeField] private float ballRadius = 0.18f;
        [SerializeField] private float ballSpeed = 7.5f;
        [SerializeField, Range(0.15f, 0.95f)] private float minimumVerticalDirection = 0.35f;
        [SerializeField] private Color ballColor = new Color(1f, 0.87f, 0.36f, 1f);

        [Header("Ball Speed Control")]
        [SerializeField, Range(0.02f, 0.25f)] private float manualBallSpeedStep = 0.08f;
        [SerializeField, Range(0.05f, 0.5f)] private float manualBallSpeedHoldDelay = 0.2f;
        [SerializeField, Range(0.02f, 0.2f)] private float manualBallSpeedHoldInterval = 0.06f;
        [SerializeField, Range(1f, 2.5f)] private float manualBallSpeedMaxMultiplier = 1.75f;

        [Header("Run Rules")]
        [SerializeField, Min(1)] private int startingLives = 3;
        [SerializeField, Min(0)] private int lifeLossScorePenalty = 500;

        [Header("Brick Wall")]
        [SerializeField] private Vector2 brickSize = new Vector2(1.15f, 0.58f);
        [SerializeField] private Vector2 brickSpacing = new Vector2(0.24f, 0.18f);

        [Header("Power-Ups")]
        [SerializeField] private Vector2 pickupSize = new Vector2(0.55f, 0.55f);
        [SerializeField] private float pickupFallSpeed = 3.2f;
        [SerializeField, Range(5f, 35f)] private float multiBallSpreadAngle = 18f;

        private readonly List<Brick> bricks = new List<Brick>();
        private readonly List<LevelDefinition> loadedLevels = new List<LevelDefinition>();
        private readonly List<BrickDefinition> loadedBrickDefinitions = new List<BrickDefinition>();
        private readonly List<ThemeDefinition> loadedThemes = new List<ThemeDefinition>();
        private readonly List<RunUpgradeDefinition> loadedRunUpgradeDefinitions = new List<RunUpgradeDefinition>();
        private readonly List<PowerUpDefinition> loadedPowerUpDefinitions = new List<PowerUpDefinition>();
        private readonly List<BallController> activeBalls = new List<BallController>();
        private readonly List<TemporaryBallLifetime> temporaryBallLifetimes = new List<TemporaryBallLifetime>();
        private readonly List<BallController> mirrorServeBalls = new List<BallController>();
        private readonly List<BreakoutMissileProjectile> activeMissiles = new List<BreakoutMissileProjectile>();
        private readonly List<PendingLaserRainLane> pendingLaserRainLanes = new List<PendingLaserRainLane>();
        private readonly List<SpriteRenderer> wallRenderers = new List<SpriteRenderer>();
        private readonly Dictionary<string, Sprite> runUpgradeSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Sprite> powerUpIconSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private readonly BreakoutBrickEffectResolver brickEffectResolver = new BreakoutBrickEffectResolver();

        private Camera activeCamera;
        private Transform runtimeRoot;
        private Transform backgroundRoot;
        private Transform wallsRoot;
        private Transform ballsRoot;
        private Transform bricksRoot;
        private Transform pickupsRoot;
        private Transform effectsRoot;
        private Transform glitchesRoot;
        private PaddleController paddle;
        private Collider2D paddleCollider;
        private SpriteRenderer paddleSpriteRenderer;
        private BallController serveBall;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Sprite triangleSprite;
        private Sprite backgroundHazeSprite;
        private Sprite backgroundScanlineSprite;
        private Sprite ballSprite;
        private Sprite brickSprite;
        private Sprite paddleSprite;
        private Sprite powerUpSprite;
        private Sprite missileSprite;
        private Sprite warpGateRingSprite;
        private Sprite warpGateVortexSprite;
        private PhysicsMaterial2D bounceMaterial;
        private Material spriteUnlitMaterial;
        private Material additiveSpriteMaterial;
        private Material additiveLineMaterial;
        private VolumeProfile runtimeVolumeProfile;
        private BreakoutThemeService themeService;
        private BreakoutPowerUpService powerUpService;
        private BreakoutAudioService audioService;
        private BreakoutBrickService brickService;
        private BreakoutRowRewriteService rowRewriteService;
        private BreakoutGhostRowService ghostRowService;
        private BreakoutRewindWallService rewindWallService;
        private BreakoutBrickLockService brickLockService;
        private BreakoutPaddleSpawnService paddleSpawnService;
        private BreakoutBallSpawnService ballSpawnService;
        private BreakoutMainMenuService mainMenuService;
        private BreakoutProgressionPageService progressionPageService;
        private BreakoutUiRenderer uiRenderer;
        private IBreakoutScoreService scoreService;
        private BreakoutBackgroundLibrary backgroundLibrary;
        private BreakoutBackgroundPresenter backgroundPresenter;
        private BreakoutRunState activeRunState;
        private BreakoutUpgradeDraftService upgradeDraftService;
        private BreakoutRogueRunController rogueRunController;
        private BreakoutTurnBasedMultiplayerController turnBasedMultiplayerController;
        private BreakoutRunStatsService runStatsService;
        private readonly BreakoutTiltAlarmState tiltAlarmState = new BreakoutTiltAlarmState();
        private RoundState roundState;
        private int livesRemaining;
        private int lifeLossCount;
        private int score;
        private float autoSaveBurstTimer;
        private float spareFuseBurstTimer;
        private bool lastLifeLossUsedAutoSave;
        private bool lastLifeLossUsedSpareFuse;
        private int lastAutoSaveScoreCost = AutoSaveBaseScoreCost;
        private int tiltWarningSavesRemaining;
        private int spareFuseSavesUsed;
        private int requiredBricksRemaining;
        private float arenaLeft;
        private float arenaRight;
        private float arenaTop;
        private float arenaBottom;
        private LevelDefinition currentLevel;
        private int currentLevelIndex;
        private float currentLevelBallSpeed;
        private float currentLevelPaddleSpeed;
        private BreakoutLevelPlanner levelPlanner;
        private BreakoutRunSetupState pendingRunSetup;
        private BreakoutDeveloperLaunchState developerLaunchState;
        private BreakoutDeveloperLaunchField selectedDeveloperLaunchField;
        private RunSettings activeRunSettings;
        private DeterministicRandomService gameplayRandom;
        private BreakoutRunSetupField selectedRunSetupField;
        private RoundState pausedFromState;
        private int selectedMainMenuActionIndex;
        private int selectedRoguePaddleIndex;
        private BreakoutUiProgressionSortMode selectedProgressionSortMode = BreakoutUiProgressionSortMode.UnlockLevel;
        private BreakoutUiProgressionRarityFilter selectedProgressionRarityFilter = BreakoutUiProgressionRarityFilter.All;
        private BreakoutUiProgressionTypeFilter selectedProgressionTypeFilter = BreakoutUiProgressionTypeFilter.All;
        private BreakoutUiProgressionLockFilter selectedProgressionLockFilter = BreakoutUiProgressionLockFilter.All;
        private BreakoutHotSeatDifficulty selectedSoloMarathonDifficulty = BreakoutHotSeatDifficulty.Gnarly;
        private int selectedSoloMarathonSetupActionIndex = (int)BreakoutHotSeatDifficulty.Gnarly;
        private int selectedOverlayActionIndex;
        private string currentLevelVariationLabel = "Stage mix: not started";
        private string pendingValidationMessage = string.Empty;
        private float manualBallSpeedMultiplier = 1f;
        private int manualBallSpeedHoldDirection;
        private float manualBallSpeedHoldTimer;
        private bool isDiagnosticsOverlayVisible;
        private int selectedUpgradeDraftIndex;
        private bool activeRunResultRecorded;
        private bool activeSoloMarathonResultRecorded;
        private bool activeSoloMarathonNewHighScore;
        private BreakoutSoloMarathonRecord activeSoloMarathonRecord;
        private bool isTurnBasedSetupActive;
        private BreakoutEffectModifiers activeEffectModifiers;
        private BallController stickyCaughtBall;
        private bool stickyCaughtBallUsesCleanCatch;
        private float cleanCatchReleaseOffsetNormalized;
        private float cleanCatchReleaseAimMultiplier = 1f;
        private SpriteRenderer shieldWallRenderer;
        private Collider2D shieldWallCollider;
        private BreakoutShieldWallVisual shieldWallVisual;
        private BreakoutVectorSightVisual vectorSightVisual;
        private SpriteRenderer leftWallRenderer;
        private SpriteRenderer rightWallRenderer;
        private BoxCollider2D leftWallCollider;
        private BoxCollider2D rightWallCollider;
        private int shieldWallCharges;
        private float laserShotCooldownTimer;
        private float laserRainStrikeTimer;
        private int availableMissiles;
        private int freeMissilesFiredThisLevel;
        private int rewardMissileBonusPurchasesUsedThisDraft;
        private bool warpHandleUsedThisLevel;
        private float missileShotCooldownTimer;
        private float activeLevelElapsedSeconds;
        private BreakoutLevelGlitchPlan activeLevelGlitchPlan = BreakoutLevelGlitchPlan.None;
        private bool hasLockstepRowsPaddleAnchor;
        private float lockstepRowsLastPaddleX;
        private float nextBrickLockBlockedBannerTime;
        private float nextBrickquakeTriggerTime;
        private float nextMeltdownCoreBannerTime;
        private Brick activeMeltdownCoreBrick;
        private BreakoutWarpGateController activeWarpGateController;
        private BreakoutTurboRailSection activeTurboRailSection;
        private BreakoutStaticWallSection activeStaticWallSection;
        private BreakoutThinAirWallSection activeThinAirWallSection;
        private BreakoutGravityPocketVisual activeGravityPocketVisual;
        private readonly List<BreakoutGravityPocketVisual> activeMagnetStormVisuals = new List<BreakoutGravityPocketVisual>();
        private float gravitySwapPullSign = 1f;
        private GameObject activePrismLaneField;
        private GameObject activeSwitchbackRailField;
        private GameObject activeHotCornerField;
        private GameObject activeSplitHorizonField;
        private GameObject activeVhsTearField;
        private GameObject activeStaticJackpotField;
        private GameObject activeStaticServeRailField;
        private BreakoutMirrorGridVisual activeMirrorGridVisual;
        private bool isMirrorGridArmed;
        private bool hasMirrorGridTriggered;
        private int mirrorGridInitialRequiredBricks;
        private bool isRowRewriteArmed;
        private bool hasRowRewriteTriggered;
        private bool hasShownRowRewriteWarning;
        private float rowRewriteTimer;
        private BreakoutRowRewriteSpec activeRowRewriteSpec;
        private float scoreLeakGraceTimer;
        private float scoreLeakAccumulator;
        private float lastNeonFloodTriggerTime = float.NegativeInfinity;
        private int cassetteSkipPaddleHits;
        private int lastStaticServeRuleIndex = -1;
        private int currentLevelRowCount;
        private int currentLevelColumnCount;
        private bool isDeveloperRunActive;
        private bool isMenuAttractModeActive;
        private float menuAttractRestartTimer;
        private float serveBallRevealDelayTimer;
        private int menuAttractLevelCounter;

        public Collider2D PaddleCollider => paddleCollider;

        private void Awake()
        {
            LoadBrickDefinitions();
            LoadLevelDefinitions();
            LoadThemeDefinitions();
            LoadRunUpgradeDefinitions();
            LoadPowerUpDefinitions();
            levelPlanner = new BreakoutLevelPlanner(loadedLevels, loadedBrickDefinitions, GenerateSeed);
            ConfigureCamera();
            CreateRuntimeAssets();
            ConfigurePostProcessing();
            themeService = new BreakoutThemeService(
                backgroundColor,
                wallColor,
                paddleColor,
                ballColor,
                ResolveBackgroundSpriteForCurrentLevel(),
                squareSprite,
                paddleSprite,
                ballSprite,
                brickSprite,
                powerUpSprite);
            powerUpService = new BreakoutPowerUpService(pickupSize, pickupFallSpeed, multiBallSpreadAngle, additiveSpriteMaterial);
            mainMenuService = new BreakoutMainMenuService();
            progressionPageService = new BreakoutProgressionPageService();
            uiRenderer = new BreakoutUiRenderer();
            scoreService = new BreakoutScoreService();
            activeRunState = new BreakoutRunState();
            developerLaunchState = new BreakoutDeveloperLaunchState();
            upgradeDraftService = new BreakoutUpgradeDraftService(loadedRunUpgradeDefinitions);
            rogueRunController = new BreakoutRogueRunController(loadedRunUpgradeDefinitions, loadedPowerUpDefinitions);
            turnBasedMultiplayerController = new BreakoutTurnBasedMultiplayerController();
            runStatsService = new BreakoutRunStatsService();
            CreateRuntimeRoots();
            CreateAudioService();
            CreateActorSpawnServices();
            CreateBrickService();
            CreateBackground();
            CreateBounds();
            CreateShieldWallVisual();
            CreateVectorSightVisual();
            CreatePaddle();
            currentLevelBallSpeed = ballSpeed;
            currentLevelPaddleSpeed = paddleSpeed;
            ResetPendingRunSetup(generateNewSeed: true);
            LoadPersistedRunSetup();
            EnterMainMenu();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;

            if (squareSprite != null)
            {
                Destroy(squareSprite);
            }

            if (circleSprite != null)
            {
                Destroy(circleSprite);
            }

            if (triangleSprite != null)
            {
                Destroy(triangleSprite);
            }

            if (warpGateRingSprite != null)
            {
                Destroy(warpGateRingSprite);
            }

            if (warpGateVortexSprite != null)
            {
                Destroy(warpGateVortexSprite);
            }

            if (backgroundHazeSprite != null)
            {
                Destroy(backgroundHazeSprite);
            }

            if (backgroundScanlineSprite != null)
            {
                Destroy(backgroundScanlineSprite);
            }

            backgroundLibrary?.DestroyRuntimeSprites();

            if (bounceMaterial != null)
            {
                Destroy(bounceMaterial);
            }

            if (spriteUnlitMaterial != null)
            {
                Destroy(spriteUnlitMaterial);
            }

            if (additiveSpriteMaterial != null)
            {
                Destroy(additiveSpriteMaterial);
            }

            if (additiveLineMaterial != null)
            {
                Destroy(additiveLineMaterial);
            }

            if (runtimeVolumeProfile != null)
            {
                Destroy(runtimeVolumeProfile);
            }

            uiRenderer?.Dispose();
        }

        private void Update()
        {
            UpdateActiveLevelElapsedSeconds();
            UpdateTimedEffects();
            UpdateJammedRailsPulse();
            UpdateCapsuleRoulettePickups();
            UpdateTemporaryBallLifetimes();
            RefreshBrickMagnetTargets();
            RefreshCapsuleMagnetTargets();
            UpdateVectorSightVisual();
            UpdateLaserRain();
            UpdatePickupBanner();
            UpdateAutoSaveBurst();
            UpdateSpareFuseBurst();
            UpdateTiltAlarmState();
            TrackRunStatsFrame();
            audioService?.Update(Time.unscaledDeltaTime);
            scoreService?.UpdateFloatingScorePopups(Time.unscaledDeltaTime);
            laserShotCooldownTimer = Mathf.Max(0f, laserShotCooldownTimer - Time.deltaTime);
            missileShotCooldownTimer = Mathf.Max(0f, missileShotCooldownTimer - Time.deltaTime);
            UpdateServeBallRevealDelay();
            UpdateMenuAttractMode();
            UpdateGravityPocketInfluence();
            UpdateMagnetStormInfluence();
            UpdateLockstepRows();
            UpdateRowRewriteTimer();
            UpdateGhostRow();
            UpdateRewindWall();
            UpdateScoreLeak();

            var keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (roundState == RoundState.MainMenu
                || roundState == RoundState.Progression
                || roundState == RoundState.LifetimeStats
                || roundState == RoundState.SoloMarathonSetup
                || roundState == RoundState.Paused
                || roundState == RoundState.LevelComplete
                || roundState == RoundState.GameOver)
            {
                ResetManualBallSpeedHold();
                HandleOverlayMenuInput(keyboard);
                return;
            }

            if (roundState == RoundState.UpgradeDraft)
            {
                ResetManualBallSpeedHold();
                HandleUpgradeDraftInput(keyboard);
                return;
            }

            if (roundState == RoundState.DeveloperMenu)
            {
                ResetManualBallSpeedHold();
                HandleDeveloperMenuInput(keyboard);
                return;
            }

            if (roundState == RoundState.RunSetup)
            {
                ResetManualBallSpeedHold();
                HandleRunSetupInput(keyboard);
                return;
            }

            if ((keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame) && CanPauseRoundState(roundState))
            {
                PauseGameplay();
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ReturnToLaunchSurface();
                return;
            }

            HandleManualBallSpeedInput(keyboard);

            if (keyboard.tKey.wasPressedThisFrame && TryTriggerTiltNudge(keyboard))
            {
                return;
            }

            var actionPressed = keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            var missilePressed = keyboard.mKey.wasPressedThisFrame;

            if (!actionPressed && !missilePressed)
            {
                return;
            }

            if (roundState == RoundState.Playing)
            {
                if (actionPressed && ReleaseStickyCaughtBall())
                {
                    return;
                }

                if (actionPressed && FireLaserVolley())
                {
                    return;
                }

                if (FireMissile())
                {
                    return;
                }
            }

            if (actionPressed && (roundState == RoundState.ReadyToServe || roundState == RoundState.LifeLost))
            {
                LaunchServe();
            }
        }

        public void HandleBrickDestroyed(Brick brick, BallController scoringBall, BrickDestructionCause destructionCause)
        {
            if (brick == null)
            {
                return;
            }

            var brickDefinition = brick.Definition;
            var shouldExplode = brickDefinition != null && brickDefinition.IsExplosive;
            var shouldTriggerMissileExplosion = destructionCause == BrickDestructionCause.Missile;
            var shouldTriggerExplosiveBall = scoringBall != null
                && scoringBall.IsExplosiveBall
                && destructionCause == BrickDestructionCause.Impact;
            var explosionCenter = (Vector2)brick.transform.position;
            var wasMeltdownCore = brick == activeMeltdownCoreBrick;

            if (brickService == null || !brickService.RemoveBrick(brick))
            {
                return;
            }

            if (destructionCause == BrickDestructionCause.Impact)
            {
                TryApplySpeedSteps(scoringBall);
            }

            if (wasMeltdownCore)
            {
                TryClearMeltdownCore(brick);
            }
            else
            {
                TryApplyMeltdownCoreOverclock(brick, scoringBall);
            }

            ResetScoreLeakOnBrickBreak();
            TryTriggerGhostRow(brick);
            TryTriggerRewindWall(brick);
            TryTriggerBrickLock(brick);
            runStatsService?.RegisterBrickDestroyed();
            audioService?.PlayBrickDestroyed(brickDefinition);

            if (shouldTriggerMissileExplosion)
            {
                audioService?.PlayExplosion();
            }

            var scoreAward = scoreService != null
                ? scoreService.BuildBrickScoreAward(brick, scoringBall, destructionCause, BuildScoreContext())
                : default;
            var staticJackpotPoints = ResolveStaticJackpotBonusPoints(
                brick,
                scoringBall,
                destructionCause,
                scoreAward.BasePoints);
            var turboTaxBonusPoints = ResolveTurboTaxBonusPoints(
                scoringBall,
                destructionCause,
                scoreAward.BasePoints);
            var bankBonusPoints = 0;
            powerUpService?.TryConsumeBankBonus(out bankBonusPoints);
            score += scoreAward.TotalPoints + staticJackpotPoints + turboTaxBonusPoints + bankBonusPoints;

            var combinedBonusPoints = scoreAward.BonusPoints + staticJackpotPoints + turboTaxBonusPoints + bankBonusPoints;
            var combinedBonusLabel = CombineBonusLabels(
                CombineBonusLabels(
                    CombineBonusLabels(scoreAward.BonusLabel, staticJackpotPoints > 0 ? "STATIC JACKPOT" : string.Empty),
                    turboTaxBonusPoints > 0 ? "TURBO TAX" : string.Empty),
                bankBonusPoints > 0 ? "BANK BONUS" : string.Empty);

            if (combinedBonusPoints > 0)
            {
                audioService?.PlayBonusScore();
                scoreService?.CreateFloatingScorePopup(
                    explosionCenter,
                    combinedBonusPoints,
                    combinedBonusLabel,
                    ResolveComboPopupColor());
            }

            scoreService?.RegisterBrickScoreEvent(scoringBall, scoreAward.BasePoints > 0, Time.time);
            TryTriggerPrismPop(scoringBall, explosionCenter, destructionCause);
            TrySpawnPickup(brick);
            TrySpawnTurboTaxHazardPickup(brick, scoringBall, destructionCause);
            TryTriggerNeonFlood(brick, scoreAward.SlamChainCount);
            ClearStaticServeRailField(showBanner: true);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining - 1);
            }

            TryTriggerBrickBloom(explosionCenter);
            brickService.DisableAndDestroyBrick(brick);
            TryTriggerMirrorGrid();

            var explosionHitCount = 0;

            if (shouldExplode)
            {
                var explosionRadius = brickDefinition.ExplosionRadius * GetEffectiveSpecialBrickEffectMultiplier();
                SpawnExplosionVisual(explosionCenter, explosionRadius);
                SplitBallsCaughtInExplosion(
                    explosionCenter,
                    explosionRadius,
                    scoringBall,
                    ResolveSpecialBrickSpeedBurstMultiplier(brickDefinition.ExplosionSpeedMultiplier, GetEffectiveSpecialBrickEffectMultiplier()),
                    brickDefinition.ExplosionSpeedDuration);

                if (destructionCause == BrickDestructionCause.Impact && scoringBall != null)
                {
                    var specialBrickEffectMultiplier = GetEffectiveSpecialBrickEffectMultiplier();
                    SplitBallFromExplosiveBrick(
                        scoringBall,
                        explosionCenter,
                        ResolveSpecialBrickSpeedBurstMultiplier(brickDefinition.ExplosionSpeedMultiplier, specialBrickEffectMultiplier),
                        brickDefinition.ExplosionSpeedDuration);
                }

                explosionHitCount += brickService.DestroyBricksInExplosionRadius(
                    explosionCenter,
                    explosionRadius,
                    brick,
                    scoringBall);
            }

            if (shouldTriggerMissileExplosion)
            {
                SpawnExplosionVisual(explosionCenter, MissileExplosionRadius);
                SplitBallsCaughtInExplosion(
                    explosionCenter,
                    MissileExplosionRadius,
                    null,
                    ResolveSpecialBrickSpeedBurstMultiplier(brickDefinition != null ? brickDefinition.ExplosionSpeedMultiplier : 1.16f, GetEffectiveSpecialBrickEffectMultiplier()),
                    brickDefinition != null ? brickDefinition.ExplosionSpeedDuration : 1.4f);
                explosionHitCount += brickService.DestroyBricksInExplosionRadius(
                    explosionCenter,
                    MissileExplosionRadius,
                    brick,
                    null);
            }

            if (shouldTriggerExplosiveBall)
            {
                var explosionRadius = ResolveExplosiveBallExplosionRadius(scoringBall.ExplosiveBallStrength);
                SpawnExplosionVisual(explosionCenter, explosionRadius);
                explosionHitCount += brickService.DestroyBricksInExplosionRadius(
                    explosionCenter,
                    explosionRadius,
                    brick,
                    scoringBall);

                if (explosionHitCount > 0)
                {
                    scoringBall.ApplySpeedBurst(
                        ResolveExplosiveBallSpeedBurstMultiplier(scoringBall.ExplosiveBallStrength),
                        ExplosiveBallSpeedBurstDuration);
                }
            }

            TryTriggerChainLightning(explosionCenter, brick, scoringBall, destructionCause);
            requiredBricksRemaining += brickService.SpawnSplitBricks(
                brickDefinition,
                explosionCenter,
                GetEffectiveSpecialBrickEffectMultiplier());
            RefreshMeltdownCoreMarks();
            TryTriggerBrickquake(explosionCenter, scoringBall, destructionCause);

            TryExpireMirrorServeBall(scoringBall);
            EvaluateLevelCompletion();
        }

        private void TryTriggerBrickBloom(Vector2 bloomCenter)
        {
            if (powerUpService == null
                || brickService == null
                || powerUpService.BrickBloomCharges <= 0)
            {
                return;
            }

            var bloomDefinition = ResolveBrickBloomBonusDefinition();

            if (bloomDefinition == null || !powerUpService.TryConsumeBrickBloomCharge(out var brickBloomPowerUp))
            {
                return;
            }

            var spawnedRequiredBricks = brickService.SpawnBonusBricks(
                bloomDefinition,
                bloomCenter,
                ResolveBrickBloomBonusBrickCount(brickBloomPowerUp));

            if (spawnedRequiredBricks <= 0)
            {
                return;
            }

            requiredBricksRemaining += spawnedRequiredBricks;
            SpawnExplosionVisual(bloomCenter, BrickBloomPulseRadius);
            powerUpService.ShowStatusBanner("BRICK BLOOM!", ResolveBrickBloomColor(), 1.8f);
        }

        private static int ResolveBrickBloomBonusBrickCount(PowerUpDefinition brickBloomPowerUp)
        {
            return Mathf.Clamp(
                brickBloomPowerUp != null && brickBloomPowerUp.ExtraBallCount > 0
                    ? brickBloomPowerUp.ExtraBallCount
                    : BrickBloomBonusBrickCount,
                1,
                8);
        }

        private BrickDefinition ResolveBrickBloomBonusDefinition()
        {
            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition == null || !definition.IsBreakable)
                {
                    continue;
                }

                if (string.Equals(definition.DisplayName, "Tiny Brick", StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }

            BrickDefinition smallestDefinition = null;

            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition == null || !definition.IsBreakable)
                {
                    continue;
                }

                if (smallestDefinition == null || definition.SizeMultiplier < smallestDefinition.SizeMultiplier)
                {
                    smallestDefinition = definition;
                }
            }

            return smallestDefinition;
        }

        public bool TryHandleSolarShot(BallController scoringBall, Brick brick)
        {
            if (scoringBall == null
                || brick == null
                || brick.IsPendingRemoval
                || brick.IsBrickLockShielded
                || powerUpService == null
                || powerUpService.SolarShotCharges <= 0
                || !IsSolarShotVulnerable(brick))
            {
                return false;
            }

            if (!powerUpService.TryConsumeSolarShotCharge())
            {
                return false;
            }

            scoringBall.ContinueThroughBrickImpact();
            brick.ApplyEffectHit(scoringBall, BrickDestructionCause.Impact, Mathf.Max(1, brick.HitPointsRemaining));
            powerUpService.ShowStatusBanner("SOLAR SHOT!", ResolveSolarShotColor(), 1.25f);
            ApplySolarShotVisualState();
            return true;
        }

        public void HandleBallLost(BallController lostBall)
        {
            if (!IsGameplaySimulationActive() || lostBall == null)
            {
                return;
            }

            RemoveTemporaryBallLifetime(lostBall);
            mirrorServeBalls.Remove(lostBall);
            var isServeBall = lostBall == serveBall;
            if (lostBall == stickyCaughtBall)
            {
                stickyCaughtBall = null;
                stickyCaughtBallUsesCleanCatch = false;
                cleanCatchReleaseOffsetNormalized = 0f;
                cleanCatchReleaseAimMultiplier = 1f;
            }

            activeBalls.Remove(lostBall);

            if (activeBalls.Count > 0)
            {
                audioService?.PlayBallLost(hasOtherActiveBalls: true);

                if (isServeBall)
                {
                    lostBall.gameObject.SetActive(false);
                }
                else
                {
                    DestroyRuntimeObject(lostBall.gameObject);
                }

                return;
            }

            if (isMenuAttractModeActive)
            {
                audioService?.PlayBallLost(hasOtherActiveBalls: false);

                if (isServeBall)
                {
                    lostBall.gameObject.SetActive(false);
                }
                else
                {
                    DestroyRuntimeObject(lostBall.gameObject);
                }

                ScheduleMenuAttractRestart();
                return;
            }

            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                HandleTurnBasedBallLost(lostBall, isServeBall);
                return;
            }

            if (UsesHighScoreMode() && !UsesFiniteHighScoreLives())
            {
                lifeLossCount += 1;
                runStatsService?.RegisterLifeLost();
                ApplyLifeLossScorePenalty();

                if (!isServeBall)
                {
                    DestroyRuntimeObject(lostBall.gameObject);
                }

                PrepareServe(RoundState.LifeLost);
                return;
            }

            audioService?.PlayBallLost(hasOtherActiveBalls: false);

            lastLifeLossUsedAutoSave = false;
            lastLifeLossUsedSpareFuse = false;
            livesRemaining = Mathf.Max(0, livesRemaining - 1);
            runStatsService?.RegisterLifeLost();

            if (livesRemaining <= 0)
            {
                if (TryApplySpareFuse())
                {
                    if (!isServeBall)
                    {
                        DestroyRuntimeObject(lostBall.gameObject);
                    }

                    PrepareServe(RoundState.LifeLost);
                    return;
                }

                if (TryApplyAutoSave())
                {
                    if (!isServeBall)
                    {
                        DestroyRuntimeObject(lostBall.gameObject);
                    }

                    PrepareServe(RoundState.LifeLost);
                    return;
                }

                ApplyLifeLossScorePenalty();
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                ClearPickups();
                RecordRogueRunResult(completed: false);
                RecordSoloMarathonResult();
                FinalizeActiveRunStats(completed: false);

                if (!isServeBall)
                {
                    DestroyRuntimeObject(lostBall.gameObject);
                }

                return;
            }

            ApplyLifeLossScorePenalty();

            if (!isServeBall)
            {
                DestroyRuntimeObject(lostBall.gameObject);
            }

            PrepareServe(RoundState.LifeLost);
        }

        private void HandleTurnBasedBallLost(BallController lostBall, bool isServeBall)
        {
            audioService?.PlayBallLost(hasOtherActiveBalls: false);
            runStatsService?.RegisterLifeLost();

            if (!isServeBall && lostBall != null)
            {
                DestroyRuntimeObject(lostBall.gameObject);
            }

            CompleteAndAdvanceTurnBasedTurn(BreakoutTurnSwitchReason.BallLost);
        }

        public bool TryRescueBallWithShield(BallController ball, bool requireImpactThreshold = true)
        {
            if (!IsGameplaySimulationActive() || ball == null || shieldWallCharges <= 0)
            {
                return false;
            }

            if (requireImpactThreshold && !IsBallAtShieldWallImpact(ball))
            {
                return false;
            }

            shieldWallCharges = Mathf.Max(0, shieldWallCharges - 1);
            shieldWallVisual?.PlayImpactFlash();
            ball.BounceFromShield(GetShieldWallBallCenterY());
            UpdateShieldWallVisual();

            if (powerUpService != null)
            {
                var shieldColor = themeService != null
                    ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBeneficial, new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite).PrimaryColor
                    : new Color(0.45f, 0.95f, 0.72f, 1f);
                powerUpService.ShowStatusBanner("+ SHIELD SAVE", shieldColor, 1.2f);
            }

            return true;
        }

        public bool TryRescueBallWithTiltWarning(BallController ball)
        {
            if (!IsGameplaySimulationActive()
                || isMenuAttractModeActive
                || ball == null
                || tiltWarningSavesRemaining <= 0
                || !IsBallAtTiltWarningNearMiss(ball))
            {
                return false;
            }

            tiltWarningSavesRemaining = Mathf.Max(0, tiltWarningSavesRemaining - 1);
            ball.BounceFromShield(GetTiltWarningRescueY());
            powerUpService?.ShowStatusBanner("TILT WARNING!", ResolveTiltWarningColor(), 1.35f);
            return true;
        }

        private bool IsBallAtTiltWarningNearMiss(BallController ball)
        {
            if (ball == null || paddleCollider == null || ball.CurrentVelocity.y >= -0.01f)
            {
                return false;
            }

            var paddleBounds = paddleCollider.bounds;
            var ballPosition = ball.transform.position;
            var halfRescueWidth = Mathf.Max(
                paddleBounds.extents.x,
                paddleBounds.extents.x * TiltWarningNearMissPaddleWidthMultiplier);

            return ballPosition.y < paddleBounds.max.y
                && Mathf.Abs(ballPosition.x - paddleBounds.center.x) <= halfRescueWidth;
        }

        private float GetTiltWarningRescueY()
        {
            if (paddleCollider == null)
            {
                return arenaBottom + paddleFloorOffset + TiltWarningRescueHeightPadding;
            }

            return paddleCollider.bounds.max.y + ballRadius + TiltWarningRescueHeightPadding;
        }

        private bool TryTriggerTiltNudge(Keyboard keyboard)
        {
            if (roundState != RoundState.Playing)
            {
                return false;
            }

            var result = tiltAlarmState.RegisterNudge();

            if (result == BreakoutTiltAlarmTriggerResult.Locked)
            {
                powerUpService?.ShowStatusBanner("TILT ALARM!", ResolveTiltAlarmColor(), 0.75f);
                audioService?.PlayPowerDown();
                return true;
            }

            var direction = ResolveTiltNudgeDirection(keyboard);
            ApplyTiltNudgeToActiveBalls(direction);
            paddle?.StartBreakWiggle();

            switch (result)
            {
                case BreakoutTiltAlarmTriggerResult.Alarm:
                    powerUpService?.ShowStatusBanner("TILT ALARM!", ResolveTiltAlarmColor(), 1.45f);
                    audioService?.PlayPowerDown();
                    ApplyActiveEffects();
                    break;
                case BreakoutTiltAlarmTriggerResult.Warning:
                    powerUpService?.ShowStatusBanner("TILT WARNING!", ResolveTiltWarningColor(), 1.1f);
                    audioService?.PlayPowerDown();
                    break;
                default:
                    powerUpService?.ShowStatusBanner("NUDGE!", ResolveTiltNudgeColor(), 0.72f);
                    break;
            }

            return true;
        }

        private void UpdateTiltAlarmState()
        {
            if (tiltAlarmState.Update(Time.deltaTime))
            {
                powerUpService?.ShowStatusBanner("TILT CLEAR", ResolveTiltNudgeColor(), 0.9f);
                ApplyActiveEffects();
            }
        }

        private int ApplyTiltNudgeToActiveBalls(float horizontalDirection)
        {
            var nudgedCount = 0;
            var nudgeStrength = Mathf.Sign(horizontalDirection) * TiltNudgeHorizontalStrength;

            if (serveBall != null && serveBall.ApplyCabinetNudge(nudgeStrength))
            {
                nudgedCount++;
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                if (activeBall == serveBall)
                {
                    continue;
                }

                if (activeBall.ApplyCabinetNudge(nudgeStrength))
                {
                    nudgedCount++;
                }
            }

            return nudgedCount;
        }

        private float ResolveTiltNudgeDirection(Keyboard keyboard)
        {
            if (keyboard != null)
            {
                var leftPressed = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
                var rightPressed = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;

                if (leftPressed != rightPressed)
                {
                    return leftPressed ? -1f : 1f;
                }
            }

            if (paddle != null && Mathf.Abs(paddle.transform.position.x) > 0.1f)
            {
                return paddle.transform.position.x > 0f ? -1f : 1f;
            }

            return NextGameplayRandomBool() ? -1f : 1f;
        }

        public bool TryHandleBallPaddleCollision(BallController ball, PaddleController hitPaddle, Collision2D collision)
        {
            if (ball == null || hitPaddle == null || collision == null)
            {
                return false;
            }

            var contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)ball.transform.position;

            if (activeEffectModifiers.SplitPaddleGapNormalized > 0f && hitPaddle.IsPointInsideSplitGap(contactPoint.x))
            {
                ball.PassThroughPaddle(Mathf.Sign(contactPoint.x - hitPaddle.transform.position.x));
                return true;
            }

            TryTriggerDoubleTap(ball, hitPaddle, contactPoint);

            if (activeEffectModifiers.CleanCatchAimMultiplier > 1f
                && stickyCaughtBall == null
                && powerUpService != null
                && powerUpService.TryConsumeCleanCatchCharge(out var cleanCatchDefinition, out var aimMultiplier))
            {
                ApplyPaddleHitTilt(hitPaddle, contactPoint.x, ball.CurrentVelocity);
                ball.RecordPaddleHitRewindAnchor(hitPaddle, contactPoint.x);
                stickyCaughtBall = ball;
                stickyCaughtBallUsesCleanCatch = true;
                cleanCatchReleaseOffsetNormalized = ResolvePaddleHitOffset(hitPaddle, contactPoint.x);
                cleanCatchReleaseAimMultiplier = Mathf.Max(1f, aimMultiplier);
                ball.AttachToPaddle();
                powerUpService.ShowStatusBanner("CLEAN CATCH!", ResolvePowerUpAccentColor(cleanCatchDefinition), 1.25f);
                ApplyActiveEffects();
                return true;
            }

            if (activeEffectModifiers.StickyPaddleEnabled && stickyCaughtBall == null)
            {
                ApplyPaddleHitTilt(hitPaddle, contactPoint.x, ball.CurrentVelocity);
                ball.RecordPaddleHitRewindAnchor(hitPaddle, contactPoint.x);
                stickyCaughtBall = ball;
                stickyCaughtBallUsesCleanCatch = false;
                cleanCatchReleaseOffsetNormalized = 0f;
                cleanCatchReleaseAimMultiplier = 1f;
                ball.AttachToPaddle();
                return true;
            }

            return false;
        }

        public bool TryRescueBallWithRewindCatch(BallController ball)
        {
            if (!IsGameplaySimulationActive()
                || ball == null
                || powerUpService == null
                || powerUpService.RewindCatchCharges <= 0
                || !ball.TryRewindToLastPaddleHit())
            {
                return false;
            }

            if (powerUpService.TryConsumeRewindCatchCharge(out var rewindCatchDefinition))
            {
                powerUpService.ShowStatusBanner("REWIND CATCH!", ResolvePowerUpAccentColor(rewindCatchDefinition), 1.35f);
            }

            return true;
        }

        public bool TryHandleBallExitedThinAir(BallController ball)
        {
            if (!IsGameplaySimulationActive()
                || ball == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ThinAir)
                || activeThinAirWallSection == null)
            {
                return false;
            }

            var ballX = ball.transform.position.x;
            var escaped = activeThinAirWallSection.Wall == BreakoutWarpGateWall.Right
                ? ballX > arenaRight + ThinAirEscapeMargin
                : ballX < arenaLeft - ThinAirEscapeMargin;

            if (!escaped)
            {
                return false;
            }

            powerUpService?.ShowStatusBanner("THIN AIR!", new Color(0.03f, 0.93f, 0.98f, 1f), 1.1f);
            ball.Stop();
            HandleBallLost(ball);
            return true;
        }

        public bool TryHandleBallEscapedPlayableArena(BallController ball)
        {
            if (!IsGameplaySimulationActive() || ball == null)
            {
                return false;
            }

            var arenaBounds = Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop);

            if (!HasBallEscapedPlayableArena(ball.transform.position, arenaBounds, BallArenaEscapeMargin))
            {
                return false;
            }

            ball.Stop();
            HandleBallLost(ball);
            return true;
        }

        internal static bool HasBallEscapedPlayableArena(Vector2 ballPosition, Rect arenaBounds, float escapeMargin)
        {
            if (arenaBounds.width <= 0.001f || arenaBounds.height <= 0.001f)
            {
                return false;
            }

            var margin = Mathf.Max(0f, escapeMargin);
            return ballPosition.x < arenaBounds.xMin - margin
                || ballPosition.x > arenaBounds.xMax + margin
                || ballPosition.y > arenaBounds.yMax + margin;
        }

        public void ApplyPaddleHitTilt(PaddleController hitPaddle, float contactWorldX)
        {
            ApplyPaddleHitTilt(hitPaddle, contactWorldX, Vector2.zero);
        }

        public void ApplyPaddleHitTilt(PaddleController hitPaddle, float contactWorldX, Vector2 incomingVelocity)
        {
            if (hitPaddle == null || activeEffectModifiers.PaddleHitTiltDegrees <= 0.001f)
            {
                return;
            }

            hitPaddle.ApplyHitTilt(contactWorldX, incomingVelocity);
        }

        private bool IsBallAtShieldWallImpact(BallController ball)
        {
            if (ball == null)
            {
                return false;
            }

            var impactY = GetShieldWallBallCenterY();
            var currentY = ball.transform.position.y;
            var downwardTravelAllowance = Mathf.Max(0.04f, ball.CurrentSpeed * Time.fixedDeltaTime);
            return currentY <= impactY + downwardTravelAllowance;
        }

        private float GetShieldWallBallCenterY()
        {
            return arenaBottom + ShieldWallYOffset + (ShieldWallThickness * 0.5f) + ballRadius + 0.02f;
        }

        public void HandleBallHitPaddle(BallController hitBall = null)
        {
            TryResetSpeedSteps(hitBall);
            runStatsService?.RegisterPaddleHit();
            audioService?.PlayBallHitPaddle();
        }

        public void HandleBallHitWall()
        {
            runStatsService?.RegisterWallHit();
            audioService?.PlayBallHitWall();
            powerUpService?.ChargeBankBonusFromWallBounce();
            TryFlipGravitySwapPolarity();
        }

        public bool TryApplyWarpHandle(BallController ball)
        {
            var trimRatio = GetPersistentRunUpgradeModifiers().WarpHandleSpeedTrimRatio;

            if (ball == null || warpHandleUsedThisLevel || trimRatio <= 0.001f)
            {
                return false;
            }

            warpHandleUsedThisLevel = true;

            if (!ball.TrimTemporarySpeedTowardBaseline(trimRatio))
            {
                return false;
            }

            powerUpService?.ShowStatusBanner("WARP HANDLE!", new Color(0.03f, 0.93f, 0.98f, 1f), 1.2f);
            return true;
        }

        public bool TryApplyBogusBounce(BallController ball)
        {
            if (ball == null || powerUpService == null)
            {
                return false;
            }

            var currentVelocity = ball.CurrentVelocity;

            if (currentVelocity.sqrMagnitude <= 0.01f
                || !powerUpService.TryConsumeBogusBounceCharge(out var wildAngleDegrees))
            {
                return false;
            }

            var angleMagnitude = NextGameplayRandomFloat(wildAngleDegrees * 0.45f, wildAngleDegrees);
            var angleSign = NextGameplayRandomBool() ? 1f : -1f;
            var wildDirection = (Vector2)(Quaternion.Euler(0f, 0f, angleMagnitude * angleSign) * currentVelocity.normalized);
            ball.ApplyCollisionResponse(wildDirection, 0.06f);
            return true;
        }

        public void HandleBallLaunched()
        {
            runStatsService?.RegisterBallLaunched();
        }

        public void HandleBrickHit(Brick brick, BallController scoringBall = null)
        {
            runStatsService?.RegisterBrickHit();
            audioService?.PlayBrickHit(brick?.Definition);
            TryApplySpeedSteps(scoringBall);
            TryApplyMeltdownCoreOverclock(brick, scoringBall);
            AwardBankBonusIfAvailable(brick != null ? (Vector2)brick.transform.position : Vector2.zero);
            TryTriggerPrismPop(scoringBall, brick != null ? (Vector2)brick.transform.position : Vector2.zero, BrickDestructionCause.Impact);
            TryTriggerGhostRow(brick);
            TryTriggerBrickquake(brick != null ? (Vector2)brick.transform.position : Vector2.zero, scoringBall, BrickDestructionCause.Impact);
            TryExpireMirrorServeBall(scoringBall);
        }

        public void HandleBrickLockBlocked(Brick brick, BallController scoringBall = null)
        {
            runStatsService?.RegisterBrickHit();
            audioService?.PlayBrickHit(brick?.Definition);
            TryExpireMirrorServeBall(scoringBall);

            if (Time.time < nextBrickLockBlockedBannerTime)
            {
                return;
            }

            nextBrickLockBlockedBannerTime = Time.time + 0.85f;
            powerUpService?.ShowStatusBanner("LOCKED!", new Color(0.03f, 0.93f, 0.98f, 1f), 0.9f);
        }

        private void TryTriggerBrickquake(Vector2 epicenter, BallController scoringBall, BrickDestructionCause destructionCause)
        {
            if (brickService == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.Brickquake)
                || Time.time < nextBrickquakeTriggerTime
                || !IsBrickquakeHeavyHit(scoringBall, destructionCause, activeLevelGlitchPlan.Brickquake))
            {
                return;
            }

            var spec = activeLevelGlitchPlan.Brickquake;
            var nudgedCount = brickService.ApplyBrickquake(epicenter, spec, NextGameplayRandomFloat);

            if (nudgedCount <= 0)
            {
                return;
            }

            nextBrickquakeTriggerTime = Time.time + spec.CooldownSeconds;
            SpawnExplosionVisual(epicenter, Mathf.Max(0.65f, spec.Radius * 0.42f));
            powerUpService?.ShowStatusBanner("BRICKQUAKE!", new Color(1f, 0.49f, 0.15f, 1f), 1.05f);
        }

        private static bool IsBrickquakeHeavyHit(
            BallController scoringBall,
            BrickDestructionCause destructionCause,
            BreakoutBrickquakeSpec spec)
        {
            if (destructionCause != BrickDestructionCause.Impact)
            {
                return true;
            }

            return scoringBall != null
                && (scoringBall.IsExplosiveBall || scoringBall.CurrentSpeed >= spec.HeavySpeedThreshold);
        }

        public void HandlePickupCaught(PowerUpPickup pickup)
        {
            if (!IsGameplaySimulationActive() || pickup == null)
            {
                return;
            }

            var pickupPosition = (Vector2)pickup.transform.position;
            var awardCapsuleMadnessBonus = powerUpService != null && powerUpService.IsCapsuleMadnessActive;
            runStatsService?.RegisterDropPickedUp(pickup.Definition);
            powerUpService.RemovePickup(pickup);
            audioService?.PlayPickupCollected(pickup.Definition);

            if (awardCapsuleMadnessBonus)
            {
                AwardCapsuleMadnessPickupBonus(pickupPosition);
            }

            TriggerCapsuleBlackoutFromPickupCatch();
            QueueRiskRebateForPickup(pickup.Definition);

            var primaryDefinition = pickup.PrimaryPayloadDefinition != null
                ? pickup.PrimaryPayloadDefinition
                : pickup.Definition;
            ApplyPowerUp(primaryDefinition);

            if (pickup.SecondaryPayloadDefinition != null)
            {
                ApplyPowerUp(pickup.SecondaryPayloadDefinition);
            }

            pickup.gameObject.SetActive(false);
            DestroyRuntimeObject(pickup.gameObject);
        }

        public void HandlePickupMissed(PowerUpPickup pickup)
        {
            if (pickup == null)
            {
                return;
            }

            powerUpService.RemovePickup(pickup);
            pickup.gameObject.SetActive(false);
            DestroyRuntimeObject(pickup.gameObject);
        }

        private void StartNewRun()
        {
            StartNewRun(null, -1, applyDeveloperSelections: false);
        }

        private void StartNewRun(BreakoutDeveloperEncounter? developerEncounter, int developerLivesRemaining, bool applyDeveloperSelections)
        {
            FinalizeActiveRunStats(completed: false);
            StopMenuAttractMode(clearRunSettings: false);

            if (activeRunSettings == null)
            {
                activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
            }

            isDeveloperRunActive = applyDeveloperSelections;
            runStatsService?.BeginRun(activeRunSettings);
            ApplyTheme(activeRunSettings.ThemeDefinition);
            isDiagnosticsOverlayVisible = false;
            manualBallSpeedMultiplier = 1f;
            SetSimulationPaused(false);
            selectedOverlayActionIndex = 0;
            gameplayRandom = new DeterministicRandomService(activeRunSettings.Seed);
            livesRemaining = developerLivesRemaining > 0
                ? Mathf.Clamp(developerLivesRemaining, 1, 9)
                : activeRunSettings.StartingLives;
            lifeLossCount = 0;
            score = 0;
            availableMissiles = StartingMissileCount;
            freeMissilesFiredThisLevel = 0;
            rewardMissileBonusPurchasesUsedThisDraft = 0;
            warpHandleUsedThisLevel = false;
            autoSaveBurstTimer = 0f;
            spareFuseBurstTimer = 0f;
            lastLifeLossUsedAutoSave = false;
            lastLifeLossUsedSpareFuse = false;
            tiltWarningSavesRemaining = 0;
            spareFuseSavesUsed = 0;
            tiltAlarmState.Reset();
            activeRunResultRecorded = false;
            activeSoloMarathonResultRecorded = false;
            activeSoloMarathonNewHighScore = false;
            activeSoloMarathonRecord = null;
            if (activeRunSettings.IsTurnBasedMode)
            {
                turnBasedMultiplayerController?.StartRun(activeRunSettings.Seed, score);
                score = turnBasedMultiplayerController?.GetCurrentPlayerScore() ?? 0;
                livesRemaining = turnBasedMultiplayerController?.GetCurrentPlayerLivesRemaining() ?? livesRemaining;
            }
            else
            {
                turnBasedMultiplayerController?.ClearRun();
            }

            currentLevelIndex = activeRunSettings.IsTurnBasedMode
                ? turnBasedMultiplayerController?.GetCurrentPlayerLevelIndex() ?? 0
                : developerEncounter?.LevelIndex ?? 0;
            currentLevelVariationLabel = "Stage mix: pending";
            shieldWallCharges = 0;
            laserShotCooldownTimer = 0f;
            missileShotCooldownTimer = 0f;
            stickyCaughtBall = null;
            scoreService?.ResetComboTracking(clearPopups: true);
            activeRunState?.Reset();
            if (activeRunSettings.IsRogueMode)
            {
                rogueRunController?.InitializeRunState(activeRunState);
            }

            if (applyDeveloperSelections && activeRunSettings.IsRogueMode)
            {
                rogueRunController?.UnlockDropsThroughHeat(activeRunState, activeRunSettings.RogueIntensity);
            }

            if (applyDeveloperSelections)
            {
                ApplyDeveloperSelectionsToRunState();
            }

            ClearTimedEffects();
            ClearPickups();
            ClearMissiles();
            UpdateShieldWallVisual();

            LoadLevel(currentLevelIndex, RoundState.ReadyToServe);

            Debug.Log(
                $"Starting {(isDeveloperRunActive ? "Developer " : string.Empty)}{activeRunSettings.GameModeLabel} run | seed {activeRunSettings.Seed} | preset {activeRunSettings.DifficultyLabel} | " +
                $"score mode {activeRunSettings.ScoringModeLabel} | life loss penalty {activeRunSettings.LifeLossScorePenalty} | " +
                $"balls/serve {activeRunSettings.BallsPerServe} | paddle {activeRunSettings.SelectedPaddleLabel} width x{activeRunSettings.PaddleWidthMultiplier:0.00} speed x{activeRunSettings.PaddleSpeedMultiplier:0.00} | " +
                $"ball speed x{activeRunSettings.BallSpeedMultiplier:0.00} | brick durability x{activeRunSettings.BrickDurabilityMultiplier:0.00} | " +
                $"drops {activeRunSettings.DropPoolLabel} | theme {activeRunSettings.ThemeLabel}");
        }

        private void EnterMainMenu()
        {
            FinalizeActiveRunStats(completed: false);
            roundState = RoundState.MainMenu;
            selectedMainMenuActionIndex = 0;
            selectedOverlayActionIndex = 0;
            selectedSoloMarathonSetupActionIndex = GetSoloMarathonHeatIndex(selectedSoloMarathonDifficulty);
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Stage mix: pending";
            isDiagnosticsOverlayVisible = false;
            isDeveloperRunActive = false;
            isTurnBasedSetupActive = false;
            turnBasedMultiplayerController?.ClearRun();
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void EnterProgressionPage()
        {
            roundState = RoundState.Progression;
            selectedOverlayActionIndex = 0;
            pendingValidationMessage = string.Empty;
            isDiagnosticsOverlayVisible = false;
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void EnterLifetimeStatsPage()
        {
            FinalizeActiveRunStats(completed: false);
            roundState = RoundState.LifetimeStats;
            selectedOverlayActionIndex = 0;
            pendingValidationMessage = string.Empty;
            isDiagnosticsOverlayVisible = false;
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        public float NextGameplayRandomFloat(float minInclusive, float maxInclusive)
        {
            return gameplayRandom != null
                ? gameplayRandom.Range(minInclusive, maxInclusive)
                : UnityEngine.Random.Range(minInclusive, maxInclusive);
        }

        public bool NextGameplayRandomBool()
        {
            return gameplayRandom != null ? gameplayRandom.NextBool() : UnityEngine.Random.value < 0.5f;
        }

        private void EnterRunSetup(bool turnBased = false)
        {
            if (pendingRunSetup == null)
            {
                ResetPendingRunSetup(generateNewSeed: true);
            }

            roundState = RoundState.RunSetup;
            selectedRunSetupField = turnBased ? BreakoutRunSetupField.PlayerCount : BreakoutRunSetupField.Seed;
            isTurnBasedSetupActive = turnBased;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Stage mix: pending";
            isDiagnosticsOverlayVisible = false;
            isDeveloperRunActive = false;
            turnBasedMultiplayerController?.ClearRun();
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void EnterSoloMarathonSetup()
        {
            roundState = RoundState.SoloMarathonSetup;
            selectedSoloMarathonSetupActionIndex = GetSoloMarathonHeatIndex(selectedSoloMarathonDifficulty);
            selectedOverlayActionIndex = 0;
            isTurnBasedSetupActive = false;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Stage mix: pending";
            isDiagnosticsOverlayVisible = false;
            isDeveloperRunActive = false;
            turnBasedMultiplayerController?.ClearRun();
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void EnterDeveloperMenu()
        {
            developerLaunchState ??= new BreakoutDeveloperLaunchState();
            roundState = RoundState.DeveloperMenu;
            selectedDeveloperLaunchField = BreakoutDeveloperLaunchField.Encounter;
            isTurnBasedSetupActive = false;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Stage mix: pending";
            isDiagnosticsOverlayVisible = false;
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            StartMenuAttractMode();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void ResetRuntimeForMetaFlow()
        {
            isMenuAttractModeActive = false;
            menuAttractRestartTimer = 0f;
            serveBallRevealDelayTimer = 0f;
            SetSimulationPaused(false);
            manualBallSpeedMultiplier = 1f;
            brickService?.ClearBricks();
            ClearPickups();
            ClearTimedEffects();
            ClearLevelGlitches();
            StopAllBalls();
            DestroyAdditionalBalls();
            temporaryBallLifetimes.Clear();
            activeBalls.Clear();
            ClearMissiles();
            activeRunState?.ClearPendingDraftOffers();
            selectedUpgradeDraftIndex = 0;
            rewardMissileBonusPurchasesUsedThisDraft = 0;
            stickyCaughtBall = null;
            shieldWallCharges = 0;
            tiltWarningSavesRemaining = 0;
            tiltAlarmState.Reset();
            laserShotCooldownTimer = 0f;
            missileShotCooldownTimer = 0f;
            scoreService?.ResetComboTracking(clearPopups: true);
            UpdateShieldWallVisual();

            if (serveBall != null)
            {
                serveBall.Stop();
                serveBall.gameObject.SetActive(false);
            }

            paddle.ResetToStart();
            paddle.ClearAutopilotTarget();
        }

        private void StartMenuAttractMode()
        {
            if (!IsMenuAttractState(roundState))
            {
                return;
            }

            isMenuAttractModeActive = true;
            menuAttractRestartTimer = 0f;
            menuAttractLevelCounter = 0;
            RestartMenuAttractLevel();
        }

        private void StopMenuAttractMode(bool clearRunSettings)
        {
            isMenuAttractModeActive = false;
            menuAttractRestartTimer = 0f;
            paddle?.ClearAutopilotTarget();

            if (clearRunSettings)
            {
                activeRunSettings = null;
            }
        }

        private void UpdateMenuAttractMode()
        {
            if (!isMenuAttractModeActive)
            {
                return;
            }

            if (!IsMenuAttractState(roundState))
            {
                StopMenuAttractMode(clearRunSettings: false);
                return;
            }

            if (menuAttractRestartTimer > 0f)
            {
                menuAttractRestartTimer = Mathf.Max(0f, menuAttractRestartTimer - Time.deltaTime);

                if (menuAttractRestartTimer <= 0f)
                {
                    RestartMenuAttractLevel();
                }

                return;
            }

            if (currentLevel == null || activeBalls.Count == 0)
            {
                ScheduleMenuAttractRestart();
                return;
            }

            if (stickyCaughtBall != null)
            {
                ReleaseStickyCaughtBall();
            }

            FireLaserVolley();
            paddle?.SetAutopilotTarget(ResolveMenuAttractPaddleTargetX());
        }

        private void ScheduleMenuAttractRestart()
        {
            if (!isMenuAttractModeActive || menuAttractRestartTimer > 0f)
            {
                return;
            }

            menuAttractRestartTimer = MenuAttractRestartDelaySeconds;
            ClearPickups();
            StopAllBalls();
            DestroyAdditionalBalls();
            temporaryBallLifetimes.Clear();
            activeBalls.Clear();
            stickyCaughtBall = null;
        }

        private void RestartMenuAttractLevel()
        {
            if (!IsMenuAttractState(roundState))
            {
                StopMenuAttractMode(clearRunSettings: false);
                return;
            }

            var menuState = roundState;
            ConfigureMenuAttractRun();

            if (loadedLevels.Count > 0)
            {
                currentLevelIndex = gameplayRandom != null
                    ? gameplayRandom.Range(0, loadedLevels.Count)
                    : UnityEngine.Random.Range(0, loadedLevels.Count);
            }

            LoadLevel(currentLevelIndex, menuState);
            roundState = menuState;

            if (currentLevel != null && requiredBricksRemaining > 0)
            {
                LaunchMenuAttractServe();
                paddle?.SetAutopilotTarget(ResolveMenuAttractPaddleTargetX());
            }

            menuAttractLevelCounter++;
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void ConfigureMenuAttractRun()
        {
            var seed = GenerateSeed();
            var selectedPaddle = ResolveSelectedRoguePaddle();
            var intensity = BreakoutRogueIntensityProgressStore.GetAvailableIntensity(selectedPaddle.DisplayName);
            var themeDefinition = ResolvePendingThemeDefinition();
            activeRunSettings = rogueRunController != null
                ? rogueRunController.BuildRunSettings(seed, lifeLossScorePenalty, themeDefinition, selectedPaddle.DisplayName, intensity)
                : new RunSettings(
                    seed,
                    RunDifficultyPreset.Standard,
                    RunScoringMode.Classic,
                    1,
                    lifeLossScorePenalty,
                    1,
                    selectedPaddle.WidthMultiplier,
                    BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(intensity),
                    1f,
                    1f,
                    DropPoolMode.Mixed,
                    false,
                    themeDefinition,
                    RunGameMode.Rogue,
                    intensity,
                    selectedPaddle.DisplayName,
                    selectedPaddle.SpeedMultiplier,
                    levelGlitchesEnabled: true);

            gameplayRandom = new DeterministicRandomService(activeRunSettings.Seed);
            livesRemaining = 1;
            lifeLossCount = 0;
            score = 0;
            activeRunResultRecorded = false;
            activeSoloMarathonResultRecorded = false;
            activeSoloMarathonNewHighScore = false;
            activeSoloMarathonRecord = null;
            shieldWallCharges = 0;
            tiltWarningSavesRemaining = 0;
            laserShotCooldownTimer = 0f;
            stickyCaughtBall = null;
            manualBallSpeedMultiplier = 1f;
            scoreService?.ResetComboTracking(clearPopups: true);
            activeRunState?.Reset();
            InitializeMenuAttractUnlocks(activeRunSettings.RogueIntensity);
            ClearTimedEffects();
            ClearPickups();
            UpdateShieldWallVisual();
            ApplyTheme(themeDefinition);
        }

        private void InitializeMenuAttractUnlocks(int intensity)
        {
            if (activeRunState == null)
            {
                return;
            }

            rogueRunController?.InitializeRunState(activeRunState);

            var completedUnlockIntensity = BreakoutRunProgression.GetCompletedUnlockIntensityForRun(intensity);

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (definition != null && definition.IsEarnedForCompletedLadderIntensity(completedUnlockIntensity))
                {
                    activeRunState.UnlockDrop(definition);
                }
            }
        }

        private void LaunchMenuAttractServe()
        {
            if (serveBall == null)
            {
                return;
            }

            SetSimulationPaused(false);
            var horizontalLaunch = NextGameplayRandomFloat(-MenuAttractLaunchHorizontalRange, MenuAttractLaunchHorizontalRange);
            serveBall.Launch(new Vector2(horizontalLaunch, 1f));
            SpawnConfiguredServeBalls();
        }

        private float ResolveMenuAttractPaddleTargetX()
        {
            if (TryResolveMenuAttractBallTarget(out var ballTargetX))
            {
                return ballTargetX;
            }

            if (TryResolveMenuAttractPickupTarget(out var pickupTargetX))
            {
                return pickupTargetX;
            }

            var sweep = Mathf.Sin((Time.time + menuAttractLevelCounter) * MenuAttractFallbackSweepSpeed);
            return Mathf.Lerp(arenaLeft + paddle.HalfWidthWorld, arenaRight - paddle.HalfWidthWorld, (sweep + 1f) * 0.5f);
        }

        private bool TryResolveMenuAttractBallTarget(out float targetX)
        {
            targetX = 0f;

            if (paddle == null)
            {
                return false;
            }

            var paddleY = paddle.transform.position.y;
            var bestTime = float.PositiveInfinity;

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var ball = activeBalls[index];

                if (ball == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                var velocity = ball.CurrentVelocity;
                var position = (Vector2)ball.transform.position;

                if (velocity.y >= -0.05f)
                {
                    continue;
                }

                var timeToPaddle = (paddleY - position.y) / velocity.y;

                if (timeToPaddle < 0f || timeToPaddle >= bestTime)
                {
                    continue;
                }

                bestTime = timeToPaddle;
                targetX = ReflectMenuAttractTargetX(position.x + (velocity.x * timeToPaddle));
            }

            return bestTime < float.PositiveInfinity;
        }

        private bool TryResolveMenuAttractPickupTarget(out float targetX)
        {
            targetX = 0f;

            if (paddle == null || powerUpService == null)
            {
                return false;
            }

            var paddleY = paddle.transform.position.y;
            var bestY = float.PositiveInfinity;

            for (var index = powerUpService.ActivePickups.Count - 1; index >= 0; index--)
            {
                var pickup = powerUpService.ActivePickups[index];

                if (pickup == null)
                {
                    powerUpService.ActivePickups.RemoveAt(index);
                    continue;
                }

                if (pickup.Definition == null || !pickup.Definition.IsBeneficial)
                {
                    continue;
                }

                var pickupPosition = pickup.transform.position;

                if (pickupPosition.y < paddleY || pickupPosition.y > paddleY + MenuAttractPickupLeadDistance || pickupPosition.y >= bestY)
                {
                    continue;
                }

                bestY = pickupPosition.y;
                targetX = ReflectMenuAttractTargetX(pickupPosition.x);
            }

            return bestY < float.PositiveInfinity;
        }

        private float ReflectMenuAttractTargetX(float x)
        {
            if (paddle == null)
            {
                return Mathf.Clamp(x, arenaLeft, arenaRight);
            }

            var minX = arenaLeft + paddle.HalfWidthWorld;
            var maxX = arenaRight - paddle.HalfWidthWorld;

            if (maxX <= minX)
            {
                return 0f;
            }

            for (var reflection = 0; reflection < 8 && (x < minX || x > maxX); reflection++)
            {
                if (x < minX)
                {
                    x = minX + (minX - x);
                }

                if (x > maxX)
                {
                    x = maxX - (x - maxX);
                }
            }

            return Mathf.Clamp(x, minX, maxX);
        }

        private static bool IsMenuAttractState(RoundState state)
        {
            return state == RoundState.MainMenu
                || state == RoundState.Progression
                || state == RoundState.LifetimeStats
                || state == RoundState.SoloMarathonSetup
                || state == RoundState.RunSetup
                || state == RoundState.DeveloperMenu;
        }

        private bool IsGameplaySimulationActive()
        {
            return roundState == RoundState.Playing || isMenuAttractModeActive;
        }

        private void TrackRunStatsFrame()
        {
            if (runStatsService == null)
            {
                return;
            }

            var trackTime = roundState == RoundState.ReadyToServe
                || roundState == RoundState.Playing
                || roundState == RoundState.LifeLost
                || roundState == RoundState.UpgradeDraft;
            var trackDistance = roundState == RoundState.ReadyToServe
                || roundState == RoundState.Playing
                || roundState == RoundState.LifeLost;
            runStatsService.TrackFrame(Time.deltaTime, paddle, activeBalls, trackTime, trackDistance);
        }

        private void FinalizeActiveRunStats(bool completed)
        {
            runStatsService?.FinalizeRun(completed, activeRunSettings != null && !isDeveloperRunActive);
        }

        private void ResetPendingRunSetup(bool generateNewSeed)
        {
            pendingRunSetup ??= new BreakoutRunSetupState(GetDefaultThemeId(), GenerateSeed);
            pendingRunSetup.Reset(GetDefaultThemeId(), GenerateSeed, generateNewSeed);
        }

        private void LoadPersistedRunSetup()
        {
            pendingRunSetup ??= new BreakoutRunSetupState(GetDefaultThemeId(), GenerateSeed);
            BreakoutRunSetupPersistence.Load(pendingRunSetup, GenerateSeed, ResolveThemeIdOrDefault);
        }

        private void SavePersistedRunSetup()
        {
            BreakoutRunSetupPersistence.Save(pendingRunSetup, GenerateSeed, ResolveThemeIdOrDefault);
        }

        private void HandleRunSetupInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return;
            }

            var setupChanged = false;

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                SelectAdjacentRunSetupField(-1);
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                SelectAdjacentRunSetupField(1);
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                AdjustSelectedSetupField(-1);
                setupChanged = true;
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                AdjustSelectedSetupField(1);
                setupChanged = true;
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                pendingRunSetup.RandomizeSeed(GenerateSeed);
                setupChanged = true;
            }

            if (keyboard.backspaceKey.wasPressedThisFrame
                && selectedRunSetupField == BreakoutRunSetupField.Seed
                && pendingRunSetup.PendingSeedText.Length > 0)
            {
                pendingRunSetup.BackspaceSeed();
                setupChanged = true;
            }

            if (keyboard.deleteKey.wasPressedThisFrame && selectedRunSetupField == BreakoutRunSetupField.Seed)
            {
                pendingRunSetup.ClearSeedText();
                setupChanged = true;
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                ResetPendingRunSetup(generateNewSeed: true);
                turnBasedMultiplayerController?.ResetSetup();
                selectedRunSetupField = isTurnBasedSetupActive ? BreakoutRunSetupField.PlayerCount : BreakoutRunSetupField.Seed;
                setupChanged = true;
            }

            if (selectedRunSetupField == BreakoutRunSetupField.Seed)
            {
                setupChanged |= AppendPressedSeedDigit(keyboard);
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (setupChanged)
                {
                    SavePersistedRunSetup();
                }

                EnterMainMenu();
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
                SavePersistedRunSetup();
                StartNewRun();
                return;
            }

            if (setupChanged)
            {
                ApplyPendingThemePreview();
                SavePersistedRunSetup();
            }
        }

        private void HandleUpgradeDraftInput(Keyboard keyboard)
        {
            var offers = activeRunState?.PendingDraftOffers;

            if (keyboard == null || offers == null || offers.Count == 0)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ReturnToLaunchSurface();
                return;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                selectedUpgradeDraftIndex = (selectedUpgradeDraftIndex + offers.Count - 1) % offers.Count;
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                selectedUpgradeDraftIndex = (selectedUpgradeDraftIndex + 1) % offers.Count;
            }

            if (keyboard.mKey.wasPressedThisFrame || keyboard.bKey.wasPressedThisFrame)
            {
                TryPurchaseRewardMissile();
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                HandleUpgradeDraftChoice(selectedUpgradeDraftIndex);
            }
        }

        private void HandleOverlayMenuInput(Keyboard keyboard)
        {
            if (roundState == RoundState.MainMenu)
            {
                HandleMainMenuInput(keyboard);
                return;
            }

            if (roundState == RoundState.Progression)
            {
                HandleProgressionInput(keyboard);
                return;
            }

            if (roundState == RoundState.SoloMarathonSetup)
            {
                HandleSoloMarathonSetupInput(keyboard);
                return;
            }

            if (roundState == RoundState.LifetimeStats && keyboard.escapeKey.wasPressedThisFrame)
            {
                EnterMainMenu();
                return;
            }

            if (roundState == RoundState.LifetimeStats)
            {
                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                {
                    uiRenderer?.ScrollStatsTable(96f);
                }

                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                {
                    uiRenderer?.ScrollStatsTable(-96f);
                }
            }

            var actions = GetOverlayActionsForState(roundState);

            if (actions.Length == 0)
            {
                return;
            }

            if ((keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame) && roundState == RoundState.Paused)
            {
                ResumeGameplay();
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedOverlayActionIndex = (selectedOverlayActionIndex + actions.Length - 1) % actions.Length;
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedOverlayActionIndex = (selectedOverlayActionIndex + 1) % actions.Length;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                selectedOverlayActionIndex = Mathf.Clamp(selectedOverlayActionIndex, 0, actions.Length - 1);
                PerformOverlayAction(actions[selectedOverlayActionIndex]);
            }
        }

        private void HandleProgressionInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                uiRenderer?.ScrollProgressionContent(128f);
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                uiRenderer?.ScrollProgressionContent(-128f);
            }

            if (keyboard.pageDownKey.wasPressedThisFrame)
            {
                uiRenderer?.ScrollProgressionContent(360f);
            }

            if (keyboard.pageUpKey.wasPressedThisFrame)
            {
                uiRenderer?.ScrollProgressionContent(-360f);
            }

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                AdjustProgressionSelector(BreakoutUiProgressionSelectorKind.Sort, 1);
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                AdjustProgressionSelector(BreakoutUiProgressionSelectorKind.Rarity, 1);
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                AdjustProgressionSelector(BreakoutUiProgressionSelectorKind.Type, 1);
            }

            if (keyboard.lKey.wasPressedThisFrame)
            {
                AdjustProgressionSelector(BreakoutUiProgressionSelectorKind.LockState, 1);
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                EnterMainMenu();
                return;
            }
        }

        private void HandleSoloMarathonSetupInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return;
            }

            var actionCount = SoloMarathonBackActionIndex + 1;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                EnterMainMenu();
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedSoloMarathonSetupActionIndex = (selectedSoloMarathonSetupActionIndex + actionCount - 1) % actionCount;
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedSoloMarathonSetupActionIndex = (selectedSoloMarathonSetupActionIndex + 1) % actionCount;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                AdjustSelectedSoloMarathonDifficulty(-1);
                selectedSoloMarathonSetupActionIndex = GetSoloMarathonHeatIndex(selectedSoloMarathonDifficulty);
                pendingValidationMessage = BuildSoloMarathonHeatSelectionMessage();
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                AdjustSelectedSoloMarathonDifficulty(1);
                selectedSoloMarathonSetupActionIndex = GetSoloMarathonHeatIndex(selectedSoloMarathonDifficulty);
                pendingValidationMessage = BuildSoloMarathonHeatSelectionMessage();
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                HandleSoloMarathonSetupActionClick(selectedSoloMarathonSetupActionIndex);
            }
        }

        private void HandleDeveloperMenuInput(Keyboard keyboard)
        {
            if (keyboard == null || developerLaunchState == null)
            {
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedDeveloperLaunchField = (BreakoutDeveloperLaunchField)Mathf.Max(0, (int)selectedDeveloperLaunchField - 1);
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedDeveloperLaunchField = (BreakoutDeveloperLaunchField)Mathf.Min((int)BreakoutDeveloperLaunchField.ForcedGlitch, (int)selectedDeveloperLaunchField + 1);
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                developerLaunchState.AdjustField(selectedDeveloperLaunchField, -1, loadedRunUpgradeDefinitions, loadedPowerUpDefinitions);
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                developerLaunchState.AdjustField(selectedDeveloperLaunchField, 1, loadedRunUpgradeDefinitions, loadedPowerUpDefinitions);
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                ToggleDeveloperMenuSelection();
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                developerLaunchState.ClearBuild();
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                EnterMainMenu();
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                StartDeveloperRun();
            }
        }

        private void HandleMainMenuInput(Keyboard keyboard)
        {
            var actions = mainMenuService?.BuildActions() ?? Array.Empty<BreakoutMainMenuAction>();

            if (keyboard == null || actions.Length == 0)
            {
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                selectedMainMenuActionIndex = (selectedMainMenuActionIndex + actions.Length - 1) % actions.Length;
                pendingValidationMessage = string.Empty;
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedMainMenuActionIndex = (selectedMainMenuActionIndex + 1) % actions.Length;
                pendingValidationMessage = string.Empty;
            }

            if (mainMenuService.ResolveAction(selectedMainMenuActionIndex) == BreakoutMainMenuAction.Rogue)
            {
                if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                {
                    AdjustSelectedRoguePaddle(-1);
                    pendingValidationMessage = string.Empty;
                }

                if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                {
                    AdjustSelectedRoguePaddle(1);
                    pendingValidationMessage = string.Empty;
                }
            }
            else if (mainMenuService.ResolveAction(selectedMainMenuActionIndex) == BreakoutMainMenuAction.SoloMarathon)
            {
                if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                {
                    AdjustSelectedSoloMarathonDifficulty(-1);
                    pendingValidationMessage = string.Empty;
                }

                if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                {
                    AdjustSelectedSoloMarathonDifficulty(1);
                    pendingValidationMessage = string.Empty;
                }
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                selectedMainMenuActionIndex = Mathf.Clamp(selectedMainMenuActionIndex, 0, actions.Length - 1);
                PerformMainMenuAction(actions[selectedMainMenuActionIndex]);
            }
        }

        private OverlayAction[] GetOverlayActionsForState(RoundState state)
        {
            var returnsToMainMenuOnly = activeRunSettings != null
                && (activeRunSettings.IsRogueMode || activeRunSettings.IsSoloMarathonMode);

            return state switch
            {
                RoundState.LifetimeStats => new[]
                {
                    OverlayAction.ReturnToMainMenu,
                },
                RoundState.Paused => returnsToMainMenuOnly
                    ? new[]
                    {
                        OverlayAction.Resume,
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToMainMenu,
                        OverlayAction.QuitGame,
                    }
                    : new[]
                    {
                        OverlayAction.Resume,
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToRunSetup,
                        OverlayAction.ReturnToMainMenu,
                        OverlayAction.QuitGame,
                    },
                RoundState.LevelComplete => HasNextLevel()
                    ? new[]
                    {
                        OverlayAction.NextLevel,
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToMainMenu,
                    }
                    : returnsToMainMenuOnly
                        ? new[]
                        {
                            OverlayAction.RestartRun,
                            OverlayAction.ReturnToMainMenu,
                        }
                        : new[]
                    {
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToRunSetup,
                        OverlayAction.ReturnToMainMenu,
                    },
                RoundState.GameOver => returnsToMainMenuOnly
                    ? new[]
                    {
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToMainMenu,
                    }
                    : new[]
                    {
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToRunSetup,
                        OverlayAction.ReturnToMainMenu,
                    },
                _ => Array.Empty<OverlayAction>(),
            };
        }

        private void PerformOverlayAction(OverlayAction action)
        {
            switch (action)
            {
                case OverlayAction.ReturnToRunSetup:
                    ReturnToLaunchSurface();
                    break;
                case OverlayAction.Resume:
                    ResumeGameplay();
                    break;
                case OverlayAction.RestartRun:
                    if (activeRunSettings != null && activeRunSettings.IsSoloMarathonMode)
                    {
                        StartSoloMarathonRun();
                    }
                    else if (ShouldRebuildRogueRunOnRestart(activeRunSettings))
                    {
                        StartRogueRun();
                    }
                    else
                    {
                        StartNewRun();
                    }

                    break;
                case OverlayAction.NextLevel:
                    if (HasNextLevel())
                    {
                        LoadLevel(currentLevelIndex + 1, RoundState.ReadyToServe);
                    }
                    else
                    {
                        StartNewRun();
                    }

                    break;
                case OverlayAction.ReturnToMainMenu:
                    EnterMainMenu();
                    break;
                case OverlayAction.QuitGame:
                    QuitGame();
                    break;
            }
        }

        private void PerformMainMenuAction(BreakoutMainMenuAction action)
        {
            if (action == BreakoutMainMenuAction.Rogue)
            {
                StartRogueRun();
                return;
            }

            if (action == BreakoutMainMenuAction.Progression)
            {
                EnterProgressionPage();
                return;
            }

            if (action == BreakoutMainMenuAction.SoloMarathon)
            {
                EnterSoloMarathonSetup();
                return;
            }

            if (action == BreakoutMainMenuAction.CustomGame)
            {
                EnterRunSetup();
                return;
            }

            if (action == BreakoutMainMenuAction.LifetimeStats)
            {
                EnterLifetimeStatsPage();
                return;
            }

            if (action == BreakoutMainMenuAction.TurnBased)
            {
                EnterRunSetup(turnBased: true);
                return;
            }

            if (action == BreakoutMainMenuAction.DeveloperMode)
            {
                EnterDeveloperMenu();
                return;
            }

            if (action == BreakoutMainMenuAction.QuitGame)
            {
                QuitGame();
                return;
            }

            pendingValidationMessage = mainMenuService?.BuildPlaceholderMessage(action) ?? string.Empty;
        }

        private void ReturnToLaunchSurface()
        {
            FinalizeActiveRunStats(completed: false);

            if (activeRunSettings != null
                && (activeRunSettings.IsRogueMode || activeRunSettings.IsSoloMarathonMode))
            {
                EnterMainMenu();
                return;
            }

            EnterRunSetup(activeRunSettings != null && activeRunSettings.IsTurnBasedMode);
        }

        private void StartRogueRun()
        {
            var selectedPaddle = ResolveSelectedRoguePaddle();
            var intensity = BreakoutRogueIntensityProgressStore.GetAvailableIntensity(selectedPaddle.DisplayName);
            activeRunSettings = rogueRunController != null
                ? rogueRunController.BuildRunSettings(GenerateSeed(), lifeLossScorePenalty, ResolvePendingThemeDefinition(), selectedPaddle.DisplayName)
                : new RunSettings(
                    GenerateSeed(),
                    RunDifficultyPreset.Standard,
                    RunScoringMode.Classic,
                    3,
                    lifeLossScorePenalty,
                    1,
                    selectedPaddle.WidthMultiplier,
                    BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(intensity),
                    1f,
                    1f,
                    DropPoolMode.Mixed,
                    false,
                    ResolvePendingThemeDefinition(),
                    RunGameMode.Rogue,
                    intensity,
                    selectedPaddle.DisplayName,
                    selectedPaddle.SpeedMultiplier,
                    levelGlitchesEnabled: true);
            pendingValidationMessage = $"Neon Ladder loaded: Heat {activeRunSettings.RogueIntensity:00}/50, 10 stages, 3 balls, draft rewards, hotter drops.";
            StartNewRun();
        }

        private void StartSoloMarathonRun()
        {
            activeRunSettings = BuildSoloMarathonRunSettings(GenerateSeed(), ResolvePendingThemeDefinition());
            pendingValidationMessage = $"Neon Marathon loaded: Heat {activeRunSettings.DifficultyLabel}, score x{GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty):0.00}, 5 balls, random Tape ID {activeRunSettings.Seed}.";
            StartNewRun();
        }

        private void StartDeveloperRun()
        {
            developerLaunchState ??= new BreakoutDeveloperLaunchState();
            var selectedPaddle = developerLaunchState.ResolvePaddle();
            var intensity = BreakoutRunProgression.ClampRogueIntensity(developerLaunchState.Intensity);
            var forcedGlitch = developerLaunchState.ForcedLevelGlitchSelection;
            var shouldForceGlitch = developerLaunchState.ForcedLevelGlitchEnabled;
            var selectedGlitch = shouldForceGlitch
                ? forcedGlitch
                : LevelGlitchSelection.Random;
            activeRunSettings = rogueRunController != null
                ? rogueRunController.BuildRunSettings(
                    GenerateSeed(),
                    lifeLossScorePenalty,
                    ResolvePendingThemeDefinition(),
                    selectedPaddle.DisplayName,
                    intensity,
                    selectedGlitch,
                    shouldForceGlitch,
                    shouldForceGlitch,
                    intensity)
                : new RunSettings(
                    GenerateSeed(),
                    RunDifficultyPreset.Standard,
                    RunScoringMode.Classic,
                    3,
                    lifeLossScorePenalty,
                    1,
                    selectedPaddle.WidthMultiplier,
                    BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(intensity),
                    1f,
                    1f,
                    DropPoolMode.Mixed,
                    false,
                    ResolvePendingThemeDefinition(),
                    RunGameMode.Rogue,
                    intensity,
                    selectedPaddle.DisplayName,
                    selectedPaddle.SpeedMultiplier,
                    levelGlitchesEnabled: true,
                    levelGlitchSelection: selectedGlitch,
                    forceLevelGlitchRoll: shouldForceGlitch,
                    ignoreLevelGlitchUnlocks: shouldForceGlitch,
                    levelGlitchUnlockIntensityOverride: intensity);

            var encounter = developerLaunchState.ResolveEncounter();
            pendingValidationMessage = $"Dev jump loaded: {encounter.DisplayName} with {activeRunSettings.SelectedPaddleLabel} at Heat {activeRunSettings.RogueIntensity:00}. {BuildDeveloperForcedGlitchMessage(shouldForceGlitch, forcedGlitch)}";
            StartNewRun(encounter, developerLaunchState.LivesRemaining, applyDeveloperSelections: true);
        }

        private void ToggleDeveloperMenuSelection()
        {
            if (developerLaunchState == null)
            {
                return;
            }

            switch (selectedDeveloperLaunchField)
            {
                case BreakoutDeveloperLaunchField.Upgrade:
                    developerLaunchState.ToggleCurrentUpgrade(loadedRunUpgradeDefinitions);
                    break;
                case BreakoutDeveloperLaunchField.DropUnlock:
                    developerLaunchState.ToggleCurrentDropUnlock(loadedPowerUpDefinitions);
                    break;
                case BreakoutDeveloperLaunchField.ForcedDrop:
                    developerLaunchState.ToggleForcedDrop();
                    break;
                case BreakoutDeveloperLaunchField.ForcedGlitch:
                    developerLaunchState.ToggleForcedLevelGlitch();
                    break;
            }
        }

        private void ApplyDeveloperSelectionsToRunState()
        {
            if (developerLaunchState == null || activeRunState == null)
            {
                return;
            }

            for (var index = 0; index < loadedRunUpgradeDefinitions.Count; index++)
            {
                var upgrade = loadedRunUpgradeDefinitions[index];

                if (upgrade == null || !developerLaunchState.ShouldApplyUpgrade(upgrade))
                {
                    continue;
                }

                var offer = BreakoutRunDraftOffer.FromRunUpgrade(upgrade);

                if (!activeRunState.CanOffer(offer))
                {
                    continue;
                }

                activeRunState.SetPendingDraftOffers(new[] { offer });
                activeRunState.TryApplyPendingDraftOffer(0, out _);
            }

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var dropUnlock = loadedPowerUpDefinitions[index];

                if (dropUnlock == null || !developerLaunchState.ShouldApplyDropUnlock(dropUnlock))
                {
                    continue;
                }

                var offer = BreakoutRunDraftOffer.FromDropUnlock(dropUnlock);

                if (!activeRunState.CanOffer(offer))
                {
                    continue;
                }

                activeRunState.SetPendingDraftOffers(new[] { offer });
                activeRunState.TryApplyPendingDraftOffer(0, out _);
            }

            activeRunState.ClearPendingDraftOffers();
        }

        private bool CanPauseRoundState(RoundState state)
        {
            return state == RoundState.ReadyToServe || state == RoundState.Playing || state == RoundState.LifeLost;
        }

        private void PauseGameplay()
        {
            if (!CanPauseRoundState(roundState))
            {
                return;
            }

            pausedFromState = roundState;
            roundState = RoundState.Paused;
            selectedOverlayActionIndex = 0;
            SetSimulationPaused(true);
        }

        private void ResumeGameplay()
        {
            roundState = CanPauseRoundState(pausedFromState) ? pausedFromState : RoundState.ReadyToServe;
            selectedOverlayActionIndex = 0;
            SetSimulationPaused(false);
        }

        private static void SetSimulationPaused(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private void AdjustSelectedSetupField(int direction)
        {
            if (pendingRunSetup == null)
            {
                return;
            }

            if (isTurnBasedSetupActive)
            {
                AdjustSelectedHotSeatSetupField(direction);
                return;
            }

            pendingRunSetup.AdjustField(selectedRunSetupField, direction, GenerateSeed, ShiftThemeId);
        }

        private void SelectAdjacentRunSetupField(int direction)
        {
            if (!isTurnBasedSetupActive)
            {
                selectedRunSetupField = (BreakoutRunSetupField)Mathf.Clamp(
                    (int)selectedRunSetupField + direction,
                    0,
                    GetLastRunSetupFieldIndex());
                return;
            }

            var fields = GetHotSeatSetupFields();
            var selectedIndex = Array.IndexOf(fields, selectedRunSetupField);

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            selectedRunSetupField = fields[Mathf.Clamp(selectedIndex + direction, 0, fields.Length - 1)];
        }

        private void AdjustSelectedHotSeatSetupField(int direction)
        {
            switch (selectedRunSetupField)
            {
                case BreakoutRunSetupField.PlayerCount:
                    turnBasedMultiplayerController?.AdjustSelectedPlayerCount(direction);
                    break;
                case BreakoutRunSetupField.HotSeatMode:
                    turnBasedMultiplayerController?.AdjustSelectedMode(direction);
                    break;
                case BreakoutRunSetupField.HotSeatTurnLimit:
                    turnBasedMultiplayerController?.AdjustSelectedTopScoreTurnLimit(direction);
                    break;
                case BreakoutRunSetupField.HotSeatLives:
                    turnBasedMultiplayerController?.AdjustSelectedOutlastLives(direction);
                    break;
                case BreakoutRunSetupField.HotSeatDifficulty:
                    turnBasedMultiplayerController?.AdjustSelectedDifficulty(direction);
                    break;
            }
        }

        private BreakoutRunSetupField[] GetHotSeatSetupFields()
        {
            return turnBasedMultiplayerController != null
                && turnBasedMultiplayerController.SelectedMode == BreakoutHotSeatMode.Outlast
                    ? HotSeatOutlastSetupFields
                    : HotSeatTopScoreSetupFields;
        }

        private int GetLastRunSetupFieldIndex()
        {
            return isTurnBasedSetupActive
                ? GetHotSeatSetupFields().Length - 1
                : (int)BreakoutRunSetupField.Theme;
        }

        private void AdjustSelectedSoloMarathonDifficulty(int direction)
        {
            selectedSoloMarathonDifficulty = (BreakoutHotSeatDifficulty)Mathf.Clamp(
                (int)selectedSoloMarathonDifficulty + direction,
                (int)BreakoutHotSeatDifficulty.Chill,
                (int)BreakoutHotSeatDifficulty.Bogus);
        }

        private void AdjustSelectedRoguePaddle(int direction)
        {
            var unlockedPaddles = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();

            if (direction == 0 || unlockedPaddles.Length <= 1)
            {
                selectedRoguePaddleIndex = 0;
                return;
            }

            selectedRoguePaddleIndex = WrapIndex(selectedRoguePaddleIndex + direction, unlockedPaddles.Length);
        }

        private void AdjustProgressionSelector(BreakoutUiProgressionSelectorKind selectorKind, int direction)
        {
            if (direction == 0)
            {
                return;
            }

            switch (selectorKind)
            {
                case BreakoutUiProgressionSelectorKind.Sort:
                    selectedProgressionSortMode = (BreakoutUiProgressionSortMode)WrapIndex(
                        (int)selectedProgressionSortMode + direction,
                        Enum.GetValues(typeof(BreakoutUiProgressionSortMode)).Length);
                    break;
                case BreakoutUiProgressionSelectorKind.Rarity:
                    selectedProgressionRarityFilter = (BreakoutUiProgressionRarityFilter)WrapIndex(
                        (int)selectedProgressionRarityFilter + direction,
                        Enum.GetValues(typeof(BreakoutUiProgressionRarityFilter)).Length);
                    break;
                case BreakoutUiProgressionSelectorKind.Type:
                    selectedProgressionTypeFilter = (BreakoutUiProgressionTypeFilter)WrapIndex(
                        (int)selectedProgressionTypeFilter + direction,
                        Enum.GetValues(typeof(BreakoutUiProgressionTypeFilter)).Length);
                    break;
                case BreakoutUiProgressionSelectorKind.LockState:
                    selectedProgressionLockFilter = (BreakoutUiProgressionLockFilter)WrapIndex(
                        (int)selectedProgressionLockFilter + direction,
                        Enum.GetValues(typeof(BreakoutUiProgressionLockFilter)).Length);
                    break;
            }

            uiRenderer?.ScrollProgressionContent(-100000f);
        }

        private BreakoutRoguePaddleDefinition ResolveSelectedRoguePaddle()
        {
            var unlockedPaddles = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();

            if (unlockedPaddles.Length == 0)
            {
                selectedRoguePaddleIndex = 0;
                return BreakoutRoguePaddleCatalog.DefaultPaddle;
            }

            selectedRoguePaddleIndex = Mathf.Clamp(selectedRoguePaddleIndex, 0, unlockedPaddles.Length - 1);
            return unlockedPaddles[selectedRoguePaddleIndex];
        }

        private static int WrapIndex(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return ((value % count) + count) % count;
        }

        private void AdjustManualBallSpeed(int direction)
        {
            if (direction == 0 || CanPauseRoundState(roundState) == false)
            {
                return;
            }

            var previousMultiplier = manualBallSpeedMultiplier;
            manualBallSpeedMultiplier = Mathf.Clamp(
                manualBallSpeedMultiplier + (direction * manualBallSpeedStep),
                1f,
                Mathf.Max(1f, manualBallSpeedMaxMultiplier));

            if (Mathf.Approximately(previousMultiplier, manualBallSpeedMultiplier))
            {
                return;
            }

            ApplyActiveEffects();
        }

        private void HandleManualBallSpeedInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                ResetManualBallSpeedHold();
                return;
            }

            var direction = 0;

            if (keyboard.upArrowKey.isPressed)
            {
                direction += 1;
            }

            if (keyboard.downArrowKey.isPressed)
            {
                direction -= 1;
            }

            if (direction == 0)
            {
                ResetManualBallSpeedHold();
                return;
            }

            if (direction != manualBallSpeedHoldDirection)
            {
                manualBallSpeedHoldDirection = direction;
                manualBallSpeedHoldTimer = Mathf.Max(0.02f, manualBallSpeedHoldDelay);
                AdjustManualBallSpeed(direction);
                return;
            }

            manualBallSpeedHoldTimer -= Time.unscaledDeltaTime;

            while (manualBallSpeedHoldTimer <= 0f)
            {
                AdjustManualBallSpeed(direction);
                manualBallSpeedHoldTimer += Mathf.Max(0.02f, manualBallSpeedHoldInterval);
            }
        }

        private void ResetManualBallSpeedHold()
        {
            manualBallSpeedHoldDirection = 0;
            manualBallSpeedHoldTimer = 0f;
        }

        private bool AppendPressedSeedDigit(Keyboard keyboard)
        {
            return pendingRunSetup != null && pendingRunSetup.AppendPressedSeedDigit(keyboard);
        }

        private RunSettings BuildRunSettingsFromPending(out string validationMessage, bool commitSeedText = false)
        {
            pendingRunSetup ??= new BreakoutRunSetupState(GetDefaultThemeId(), GenerateSeed);
            var selectedTheme = ResolvePendingThemeDefinition();
            var settings = pendingRunSetup.BuildRunSettings(startingLives, lifeLossScorePenalty, selectedTheme, GenerateSeed, out validationMessage, commitSeedText);

            if (!isTurnBasedSetupActive)
            {
                return settings;
            }

            var hotSeatSettings = BuildHotSeatRunSettings(
                commitSeedText ? GenerateSeed() : settings.Seed,
                selectedTheme);
            validationMessage = $"Hot Seat live: {turnBasedMultiplayerController?.BuildSetupPreviewLine() ?? "02 players"}. Random Tape ID rolls on start.";
            return hotSeatSettings;
        }

        private RunSettings BuildHotSeatRunSettings(int seed, ThemeDefinition selectedTheme)
        {
            var mode = turnBasedMultiplayerController?.SelectedMode ?? BreakoutHotSeatMode.TopScore;
            var difficulty = turnBasedMultiplayerController?.SelectedDifficulty ?? BreakoutHotSeatDifficulty.Gnarly;
            var preset = difficulty == BreakoutHotSeatDifficulty.Bogus
                ? RunDifficultyPreset.Brutal
                : difficulty == BreakoutHotSeatDifficulty.Chill
                    ? RunDifficultyPreset.Casual
                    : RunDifficultyPreset.Standard;
            var scoringMode = mode == BreakoutHotSeatMode.TopScore
                ? RunScoringMode.HighScore
                : RunScoringMode.Classic;
            var startingLives = mode == BreakoutHotSeatMode.Outlast
                ? turnBasedMultiplayerController?.SelectedOutlastLives ?? BreakoutTurnBasedMultiplayerController.DefaultOutlastLives
                : 1;
            var ballSpeedMultiplier = 1f;
            var paddleWidthMultiplier = 1f;
            var brickDurabilityMultiplier = 1f;
            var dropChanceMultiplier = 1f;
            var dropPoolMode = DropPoolMode.Mixed;
            var capsuleParty = false;
            var levelGlitchesEnabled = false;
            var levelGlitchChanceMultiplier = 1f;
            var difficultyLabel = turnBasedMultiplayerController?.GetSelectedDifficultyLabel() ?? "Gnarly";

            switch (difficulty)
            {
                case BreakoutHotSeatDifficulty.Chill:
                    paddleWidthMultiplier = 1.18f;
                    ballSpeedMultiplier = 0.82f;
                    brickDurabilityMultiplier = 0.86f;
                    dropChanceMultiplier = 1.25f;
                    dropPoolMode = DropPoolMode.HelpfulOnly;
                    capsuleParty = true;
                    break;
                case BreakoutHotSeatDifficulty.Rad:
                    paddleWidthMultiplier = 1.08f;
                    ballSpeedMultiplier = 0.92f;
                    brickDurabilityMultiplier = 0.94f;
                    dropChanceMultiplier = 1.1f;
                    dropPoolMode = DropPoolMode.HelpfulOnly;
                    break;
                case BreakoutHotSeatDifficulty.Mondo:
                    paddleWidthMultiplier = 0.94f;
                    ballSpeedMultiplier = 1.12f;
                    brickDurabilityMultiplier = 1.16f;
                    dropChanceMultiplier = 1.05f;
                    capsuleParty = true;
                    levelGlitchesEnabled = true;
                    levelGlitchChanceMultiplier = 1f;
                    break;
                case BreakoutHotSeatDifficulty.Bogus:
                    paddleWidthMultiplier = 0.88f;
                    ballSpeedMultiplier = 1.28f;
                    brickDurabilityMultiplier = 1.32f;
                    dropChanceMultiplier = 1.2f;
                    dropPoolMode = DropPoolMode.HarmfulOnly;
                    capsuleParty = true;
                    levelGlitchesEnabled = true;
                    levelGlitchChanceMultiplier = 1.35f;
                    break;
                default:
                    ballSpeedMultiplier = 1f;
                    brickDurabilityMultiplier = 1f;
                    dropChanceMultiplier = 1f;
                    dropPoolMode = DropPoolMode.Mixed;
                    levelGlitchesEnabled = difficulty == BreakoutHotSeatDifficulty.Gnarly;
                    levelGlitchChanceMultiplier = 0.65f;
                    break;
            }

            return new RunSettings(
                seed,
                preset,
                scoringMode,
                startingLives,
                0,
                1,
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                brickDurabilityMultiplier,
                dropChanceMultiplier,
                dropPoolMode,
                capsuleParty,
                selectedTheme,
                RunGameMode.TurnBased,
                1,
                "Classic Paddle",
                1f,
                difficultyLabel,
                levelGlitchesEnabled,
                levelGlitchChanceMultiplier);
        }

        private RunSettings BuildSoloMarathonRunSettings(int seed, ThemeDefinition selectedTheme)
        {
            var difficulty = selectedSoloMarathonDifficulty;
            var preset = difficulty == BreakoutHotSeatDifficulty.Bogus
                ? RunDifficultyPreset.Brutal
                : difficulty == BreakoutHotSeatDifficulty.Chill
                    ? RunDifficultyPreset.Casual
                    : RunDifficultyPreset.Standard;
            var ballSpeedMultiplier = 1f;
            var paddleWidthMultiplier = 1f;
            var brickDurabilityMultiplier = 1f;
            var dropChanceMultiplier = 1f;
            var dropPoolMode = DropPoolMode.Mixed;
            var capsuleParty = false;
            var levelGlitchesEnabled = false;
            var levelGlitchChanceMultiplier = 1f;
            var difficultyLabel = FormatSoloMarathonDifficultyLabel(difficulty);

            switch (difficulty)
            {
                case BreakoutHotSeatDifficulty.Chill:
                    paddleWidthMultiplier = 1.18f;
                    ballSpeedMultiplier = 0.82f;
                    brickDurabilityMultiplier = 0.86f;
                    dropChanceMultiplier = 1.25f;
                    dropPoolMode = DropPoolMode.HelpfulOnly;
                    capsuleParty = true;
                    break;
                case BreakoutHotSeatDifficulty.Rad:
                    paddleWidthMultiplier = 1.08f;
                    ballSpeedMultiplier = 0.92f;
                    brickDurabilityMultiplier = 0.94f;
                    dropChanceMultiplier = 1.1f;
                    dropPoolMode = DropPoolMode.HelpfulOnly;
                    break;
                case BreakoutHotSeatDifficulty.Mondo:
                    paddleWidthMultiplier = 0.94f;
                    ballSpeedMultiplier = 1.12f;
                    brickDurabilityMultiplier = 1.16f;
                    dropChanceMultiplier = 1.05f;
                    capsuleParty = true;
                    levelGlitchesEnabled = true;
                    levelGlitchChanceMultiplier = 1f;
                    break;
                case BreakoutHotSeatDifficulty.Bogus:
                    paddleWidthMultiplier = 0.88f;
                    ballSpeedMultiplier = 1.28f;
                    brickDurabilityMultiplier = 1.32f;
                    dropChanceMultiplier = 1.2f;
                    dropPoolMode = DropPoolMode.HarmfulOnly;
                    capsuleParty = true;
                    levelGlitchesEnabled = true;
                    levelGlitchChanceMultiplier = 1.35f;
                    break;
                default:
                    levelGlitchesEnabled = difficulty == BreakoutHotSeatDifficulty.Gnarly;
                    levelGlitchChanceMultiplier = 0.65f;
                    break;
            }

            return new RunSettings(
                seed,
                preset,
                RunScoringMode.HighScore,
                5,
                0,
                1,
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                brickDurabilityMultiplier,
                dropChanceMultiplier,
                dropPoolMode,
                capsuleParty,
                selectedTheme,
                RunGameMode.SoloMarathon,
                1,
                "Classic Paddle",
                1f,
                difficultyLabel,
                levelGlitchesEnabled,
                levelGlitchChanceMultiplier);
        }

        private int ParsePendingSeed(bool commitSeedText)
        {
            pendingRunSetup ??= new BreakoutRunSetupState(GetDefaultThemeId(), GenerateSeed);
            return pendingRunSetup.ParsePendingSeed(commitSeedText, GenerateSeed);
        }

        private static int GenerateSeed()
        {
            unchecked
            {
                var tickSeed = (int)DateTime.UtcNow.Ticks;
                var salt = Environment.TickCount;
                var combined = tickSeed ^ salt ^ 0x2D1C4EAD;
                return combined == int.MinValue ? int.MaxValue : Mathf.Abs(combined);
            }
        }

        private void LaunchServe()
        {
            if (!IsServeBallReadyToLaunch())
            {
                return;
            }

            roundState = RoundState.Playing;
            SetSimulationPaused(false);
            serveBall.Launch();
            SpawnConfiguredServeBalls();
            SpawnMirrorServeBall();
        }

        private void SpawnConfiguredServeBalls()
        {
            var ballsPerServe = GetEffectiveBallsPerServe();

            if (ballsPerServe <= 1 || serveBall == null)
            {
                return;
            }

            var sourceVelocity = serveBall.GetComponent<Rigidbody2D>().linearVelocity;
            var sourceDirection = sourceVelocity.sqrMagnitude > 0.01f
                ? sourceVelocity.normalized
                : Vector2.up;
            var extraBallCount = ballsPerServe - 1;

            for (var index = 0; index < extraBallCount; index++)
            {
                var angle = extraBallCount == 1
                    ? 0f
                    : Mathf.Lerp(-multiBallSpreadAngle * 0.75f, multiBallSpreadAngle * 0.75f, index / (extraBallCount - 1f));
                var direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * sourceDirection);
                var extraBall = CreateBall(false);
                extraBall.SetWorldPosition(serveBall.transform.position);
                extraBall.Launch(direction);
                activeBalls.Add(extraBall);
            }
        }

        private void SpawnMirrorServeBall()
        {
            if (!IsMirrorServeActive() || serveBall == null || ballSpawnService == null)
            {
                return;
            }

            var sourceDirection = ResolveBallTravelDirection(serveBall);
            var mirrorDirection = BuildMirrorServeDirection(sourceDirection);
            var mirrorBall = CreateBall(false);
            mirrorBall.SetWorldPosition((Vector2)serveBall.transform.position + (mirrorDirection * ResolveMirrorServeLaunchOffset()));
            mirrorBall.Launch(mirrorDirection);
            activeBalls.Add(mirrorBall);
            mirrorServeBalls.Add(mirrorBall);
            powerUpService?.ShowStatusBanner("MIRROR SERVE!", new Color(0.01f, 0.93f, 0.98f, 1f), 1.15f);
        }

        private bool IsMirrorServeActive()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MirrorServe);
        }

        private Vector2 BuildMirrorServeDirection(Vector2 sourceDirection)
        {
            var resolvedDirection = sourceDirection.sqrMagnitude > 0.01f ? sourceDirection.normalized : Vector2.up;
            var mirroredDirection = new Vector2(-resolvedDirection.x, resolvedDirection.y);

            if (Mathf.Abs(mirroredDirection.x) < MirrorServeMinimumHorizontalDirection)
            {
                mirroredDirection.x = (NextGameplayRandomBool() ? -1f : 1f) * MirrorServeMinimumHorizontalDirection;
            }

            if (mirroredDirection.y <= 0.05f)
            {
                mirroredDirection.y = Mathf.Max(0.4f, Mathf.Abs(resolvedDirection.y));
            }

            return mirroredDirection.sqrMagnitude > 0.01f ? mirroredDirection.normalized : Vector2.up;
        }

        private float ResolveMirrorServeLaunchOffset()
        {
            return Mathf.Max(0.04f, ballRadius * MirrorServeLaunchOffsetMultiplier);
        }

        private void PrepareServe(RoundState nextState)
        {
            roundState = nextState;
            selectedOverlayActionIndex = 0;
            SetSimulationPaused(false);
            ClearPickups();
            paddle.ResetToStart();
            stickyCaughtBall = null;
            runStatsService?.ResetMovementTracking();
            EnsureServeBallExists();
            DestroyAdditionalBalls();
            temporaryBallLifetimes.Clear();
            mirrorServeBalls.Clear();
            activeBalls.Clear();
            serveBall.SetMovementSpeed(GetCurrentBallSpeed());
            RefreshServeBallVisualStyle();
            serveBall.ResetToPaddle();
            activeBalls.Add(serveBall);
            ArmStaticServeRailForServe();
            BeginServeBallRevealDelayIfNeeded(nextState);
            scoreService?.ResetComboTracking(clearPopups: false);
            powerUpService?.ClearBankBonusCharge();
        }

        private void BeginServeBallRevealDelayIfNeeded(RoundState nextState)
        {
            if (ShouldDelayServeBallReveal(nextState))
            {
                serveBallRevealDelayTimer = BreakoutUiRenderer.LifeLossAnimationDuration;
                serveBall.gameObject.SetActive(false);
                return;
            }

            serveBallRevealDelayTimer = 0f;
            serveBall.gameObject.SetActive(true);
        }

        private void LoadLevelDefinitions()
        {
            loadedLevels.Clear();
            loadedLevels.AddRange(Resources.LoadAll<LevelDefinition>("Levels"));
            loadedLevels.Sort(CompareLevels);

            if (loadedLevels.Count == 0)
            {
                Debug.LogError("No level definitions were found in Resources/Levels. Chunk 03 content cannot load.");
            }
        }

        private void LoadBrickDefinitions()
        {
            loadedBrickDefinitions.Clear();
            loadedBrickDefinitions.AddRange(Resources.LoadAll<BrickDefinition>("Bricks"));
            loadedBrickDefinitions.Sort(CompareBrickDefinitions);

            if (loadedBrickDefinitions.Count == 0)
            {
                Debug.LogWarning("No brick definitions were found in Resources/Bricks. Procedural generation will fall back poorly.");
            }
        }

        private void LoadThemeDefinitions()
        {
            loadedThemes.Clear();
            loadedThemes.AddRange(Resources.LoadAll<ThemeDefinition>("Themes"));
            loadedThemes.Sort(CompareThemes);

            if (loadedThemes.Count == 0)
            {
                Debug.LogWarning("No theme definitions were found in Resources/Themes. Runtime visuals will use serialized fallback colors.");
            }
        }

        private void LoadRunUpgradeDefinitions()
        {
            loadedRunUpgradeDefinitions.Clear();
            loadedRunUpgradeDefinitions.AddRange(Resources.LoadAll<RunUpgradeDefinition>("Upgrades"));
            loadedRunUpgradeDefinitions.Sort(CompareRunUpgradeDefinitions);

            if (loadedRunUpgradeDefinitions.Count == 0)
            {
                Debug.LogWarning("No run upgrade definitions were found in Resources/Upgrades. Between-level drafts will be skipped.");
            }
        }

        private void LoadPowerUpDefinitions()
        {
            loadedPowerUpDefinitions.Clear();
            loadedPowerUpDefinitions.AddRange(Resources.LoadAll<PowerUpDefinition>("PowerUps"));
            loadedPowerUpDefinitions.Sort(ComparePowerUpDefinitions);

            if (loadedPowerUpDefinitions.Count == 0)
            {
                Debug.LogWarning("No power-up definitions were found in Resources/PowerUps. Neon Ladder drop unlocks will be skipped.");
            }
        }

        private void LoadLevel(int levelIndex, RoundState serveState)
        {
            brickService?.ClearBricks();
            ClearPickups();
            stickyCaughtBall = null;
            ClearTimedEffects();
            ClearLevelGlitches();
            scoreService?.ResetComboTracking(clearPopups: true);
            tiltWarningSavesRemaining = GetEffectiveTiltWarningSavesPerLevel();
            freeMissilesFiredThisLevel = 0;
            warpHandleUsedThisLevel = false;
            tiltAlarmState.Reset();

            if (loadedLevels.Count == 0 || levelIndex < 0)
            {
                currentLevel = null;
                currentLevelIndex = 0;
                requiredBricksRemaining = 0;
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                StopAllBalls();
                UpdateBackgroundVisuals();
                return;
            }

            currentLevelIndex = levelIndex;
            currentLevel = ResolveLevelTemplate(levelIndex);

            if (currentLevel == null)
            {
                currentLevelIndex = 0;
                requiredBricksRemaining = 0;
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                StopAllBalls();
                UpdateBackgroundVisuals();
                return;
            }

            UpdateBackgroundVisuals();
            PlayMusicForCurrentLevel();
            var levelPlan = BuildLevelLayoutPlan(currentLevel);

            ApplyLevelTuning(levelPlan);
            BuildBrickWall(levelPlan);
            activeLevelElapsedSeconds = 0f;
            ApplyLevelGlitchPlan(levelPlan.GlitchPlan);
            PrepareServe(serveState);
            EvaluateLevelCompletion();
        }

        private void ApplyLevelTuning(BreakoutLevelLayoutPlan levelPlan)
        {
            var persistentModifiers = GetPersistentRunUpgradeModifiers();

            if (levelPlan == null)
            {
                currentLevelPaddleSpeed = paddleSpeed;
                currentLevelBallSpeed = ballSpeed * GetModeBallSpeedMultiplier() * persistentModifiers.BallSpeedMultiplier;
                ApplyActiveEffects();
                return;
            }

            currentLevelPaddleSpeed = paddleSpeed * Mathf.Max(0.5f, levelPlan.PaddleSpeedMultiplier);
            currentLevelBallSpeed = ballSpeed
                * Mathf.Max(0.5f, levelPlan.BallSpeedMultiplier)
                * (activeRunSettings?.BallSpeedMultiplier ?? 1f)
                * GetModeBallSpeedMultiplier()
                * persistentModifiers.BallSpeedMultiplier;
            ApplyActiveEffects();
        }

        private void ConfigureCamera()
        {
            activeCamera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();

            if (activeCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                activeCamera = cameraObject.AddComponent<Camera>();
                activeCamera.orthographic = true;
            }

            activeCamera.transform.position = new Vector3(0f, 0f, -10f);
            activeCamera.orthographic = true;
            activeCamera.orthographicSize = cameraHalfHeight;
            activeCamera.backgroundColor = backgroundColor;

            var arenaBounds = BreakoutPlayfieldGeometry.CalculateArenaBounds(
                cameraHalfHeight,
                activeCamera.aspect,
                playfieldPadding,
                maximumGameplayAspectRatio);
            arenaLeft = arenaBounds.Left;
            arenaRight = arenaBounds.Right;
            arenaTop = arenaBounds.Top;
            arenaBottom = arenaBounds.Bottom;
        }

        private void CreateRuntimeAssets()
        {
            squareSprite = BreakoutRuntimeVisualFactory.CreateSquareSprite();
            circleSprite = BreakoutRuntimeVisualFactory.CreateCircleSprite();
            triangleSprite = BreakoutRuntimeVisualFactory.CreateTriangleSprite();
            warpGateRingSprite = BreakoutRuntimeVisualFactory.CreateRingSprite();
            warpGateVortexSprite = BreakoutRuntimeVisualFactory.CreateVortexSprite();
            backgroundHazeSprite = BreakoutRuntimeVisualFactory.CreateBackgroundHazeSprite();
            backgroundScanlineSprite = BreakoutRuntimeVisualFactory.CreateBackgroundScanlineSprite();
            backgroundLibrary = new BreakoutBackgroundLibrary();
            backgroundLibrary.LoadFromResources();
            ballSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(BallSpriteResourcePath, circleSprite);
            brickSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(BrickSpriteResourcePath, squareSprite);
            paddleSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(PaddleSpriteResourcePath, squareSprite);
            powerUpSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(PowerUpSpriteResourcePath, squareSprite);
            missileSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(MissileSpriteResourcePath, triangleSprite);
            spriteUnlitMaterial = BreakoutRuntimeVisualFactory.CreateSpriteUnlitMaterial();
            additiveSpriteMaterial = BreakoutRuntimeVisualFactory.CreateAdditiveSpriteMaterial();
            additiveLineMaterial = BreakoutRuntimeVisualFactory.CreateAdditiveLineMaterial();
            bounceMaterial = new PhysicsMaterial2D("BreakoutBounce")
            {
                bounciness = 1f,
                friction = 0f,
            };
        }

        private void ConfigurePostProcessing()
        {
            if (activeCamera == null)
            {
                return;
            }

            var cameraData = activeCamera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;

            var volumeObject = new GameObject("Global Post Processing");
            volumeObject.transform.SetParent(activeCamera.transform, false);

            var globalVolume = volumeObject.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 10f;
            runtimeVolumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            globalVolume.profile = runtimeVolumeProfile;

            var bloom = runtimeVolumeProfile.Add<Bloom>(true);
            bloom.active = true;
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(bloomScatter);
            bloom.clamp.Override(65472f);
            bloom.tint.Override(Color.white);
            // Keep this aligned with the project volume profile so release builds keep the needed URP bloom variant.
            bloom.highQualityFiltering.Override(false);
            bloom.downscale.Override(BloomDownscaleMode.Half);
            bloom.maxIterations.Override(6);
        }

        private void CreateRuntimeRoots()
        {
            runtimeRoot = new GameObject("Runtime Game").transform;
            runtimeRoot.SetParent(transform, false);

            backgroundRoot = new GameObject("Background").transform;
            backgroundRoot.SetParent(runtimeRoot, false);

            wallsRoot = new GameObject("Bounds").transform;
            wallsRoot.SetParent(runtimeRoot, false);

            ballsRoot = new GameObject("Balls").transform;
            ballsRoot.SetParent(runtimeRoot, false);

            bricksRoot = new GameObject("Bricks").transform;
            bricksRoot.SetParent(runtimeRoot, false);

            pickupsRoot = new GameObject("Pickups").transform;
            pickupsRoot.SetParent(runtimeRoot, false);

            effectsRoot = new GameObject("Effects").transform;
            effectsRoot.SetParent(runtimeRoot, false);

            glitchesRoot = new GameObject("Level Glitches").transform;
            glitchesRoot.SetParent(runtimeRoot, false);
        }

        private void CreateAudioService()
        {
            audioService = BreakoutAudioService.Create(runtimeRoot);
        }

        private void CreateBrickService()
        {
            brickService = new BreakoutBrickService(
                this,
                bricks,
                bricksRoot,
                brickSize,
                brickSpacing,
                squareSprite,
                spriteUnlitMaterial,
                bounceMaterial,
                () => activeRunSettings,
                ResolveBrickMovementBounds,
                ResolveBrickVisualStyle,
                DestroyRuntimeObject);
            rowRewriteService = new BreakoutRowRewriteService(bricks, loadedBrickDefinitions);
            ghostRowService = new BreakoutGhostRowService(bricks);
            rewindWallService = new BreakoutRewindWallService(bricks);
            brickLockService = new BreakoutBrickLockService(bricks);
        }

        private Rect ResolveBrickMovementBounds()
        {
            var brickFloorY = Mathf.Min(ResolveBrickMovementFloorY(), arenaTop);
            return Rect.MinMaxRect(arenaLeft, brickFloorY, arenaRight, arenaTop);
        }

        private float ResolveBrickMovementFloorY()
        {
            if (paddleCollider != null)
            {
                return paddleCollider.bounds.max.y;
            }

            return arenaBottom + paddleFloorOffset + (paddleSize.y * 0.5f);
        }

        private void CreateActorSpawnServices()
        {
            paddleSpawnService = new BreakoutPaddleSpawnService(
                this,
                runtimeRoot,
                paddleSize,
                paddleColor,
                paddleSprite,
                spriteUnlitMaterial,
                bounceMaterial);
            ballSpawnService = new BreakoutBallSpawnService(
                this,
                ballsRoot,
                ballRadius,
                paddleSize,
                minimumVerticalDirection,
                additiveSpriteMaterial,
                bounceMaterial,
                () => paddle,
                GetCurrentBallSpeed,
                ResolveBallVisualStyle,
                ResolveBallGravityWellCenter,
                () => activeEffectModifiers);
        }

        private ThemeVisualStyle ResolveBrickVisualStyle(BrickDefinition definition)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.gray, squareSprite);
            }

            return themeService != null
                ? themeService.ResolveBrickStyle(definition)
                : new ThemeVisualStyle(definition.BaseColor, definition.DamagedColor, squareSprite);
        }

        private ThemeVisualStyle ResolveBallVisualStyle()
        {
            return themeService != null
                ? themeService.ResolveBallStyle()
                : new ThemeVisualStyle(ballColor, ballColor, circleSprite);
        }

        private Vector2 ResolveBallGravityWellCenter()
        {
            return new Vector2(0f, (arenaTop + arenaBottom) * 0.5f);
        }

        private void CreateBackground()
        {
            backgroundPresenter = new BreakoutBackgroundPresenter(
                activeCamera,
                cameraHalfHeight,
                backgroundColor,
                wallColor,
                backgroundImageTintStrength,
                backgroundImageAlpha,
                backgroundHazeAlpha,
                scanlineAlpha,
                backgroundHazeSprite,
                backgroundScanlineSprite);
            backgroundPresenter.Create(backgroundRoot, spriteUnlitMaterial);
            UpdateBackgroundVisuals();
        }

        private void CreateBounds()
        {
            var leftWall = CreateBoundary(
                "Left Wall",
                new Vector2(arenaLeft - (wallThickness * 0.5f), 0f),
                new Vector2(wallThickness, cameraHalfHeight * 2f),
                wallColor);
            leftWallRenderer = leftWall.Renderer;
            leftWallCollider = leftWall.Collider;

            var rightWall = CreateBoundary(
                "Right Wall",
                new Vector2(arenaRight + (wallThickness * 0.5f), 0f),
                new Vector2(wallThickness, cameraHalfHeight * 2f),
                wallColor);
            rightWallRenderer = rightWall.Renderer;
            rightWallCollider = rightWall.Collider;

            CreateBoundary(
                "Top Wall",
                new Vector2(0f, arenaTop + (wallThickness * 0.5f)),
                new Vector2((arenaRight - arenaLeft) + (wallThickness * 2f), wallThickness),
                wallColor);
        }

        private (SpriteRenderer Renderer, BoxCollider2D Collider) CreateBoundary(string wallName, Vector2 position, Vector2 size, Color color)
        {
            var wallObject = new GameObject(wallName);
            wallObject.transform.SetParent(wallsRoot, false);
            wallObject.transform.position = position;
            wallObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            var spriteRenderer = wallObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = -5;
            spriteRenderer.sharedMaterial = spriteUnlitMaterial;
            wallRenderers.Add(spriteRenderer);

            var collider = wallObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;
            return (spriteRenderer, collider);
        }

        private void CreateShieldWallVisual()
        {
            var shieldObject = new GameObject("Shield Wall");
            shieldObject.transform.SetParent(runtimeRoot, false);
            shieldObject.transform.position = new Vector2(0f, arenaBottom + ShieldWallYOffset);
            shieldObject.transform.localScale = new Vector3((arenaRight - arenaLeft) - 0.3f, ShieldWallThickness, 1f);

            shieldWallRenderer = shieldObject.AddComponent<SpriteRenderer>();
            shieldWallRenderer.sprite = squareSprite;
            shieldWallRenderer.sharedMaterial = additiveSpriteMaterial;
            shieldWallRenderer.sortingOrder = 12;
            shieldWallRenderer.enabled = false;

            shieldWallCollider = shieldObject.AddComponent<BoxCollider2D>();
            shieldWallCollider.sharedMaterial = bounceMaterial;
            shieldWallCollider.enabled = false;

            shieldWallVisual = shieldObject.AddComponent<BreakoutShieldWallVisual>();
            shieldWallVisual.Configure(
                shieldWallRenderer,
                squareSprite,
                additiveSpriteMaterial,
                (arenaRight - arenaLeft) - 0.3f,
                ShieldWallThickness);
        }

        private void CreateVectorSightVisual()
        {
            var sightObject = new GameObject("Vector Sight");
            sightObject.transform.SetParent(effectsRoot != null ? effectsRoot : runtimeRoot, false);
            vectorSightVisual = sightObject.AddComponent<BreakoutVectorSightVisual>();
            vectorSightVisual.Configure(additiveLineMaterial);
            vectorSightVisual.Hide();
        }

        private void CreatePaddle()
        {
            var paddleSpawn = paddleSpawnService.CreatePaddle(
                paddleSpeed,
                arenaLeft,
                arenaRight,
                arenaBottom + paddleFloorOffset);
            paddle = paddleSpawn.Paddle;
            paddleCollider = paddleSpawn.Collider;
            paddleSpriteRenderer = paddleSpawn.SpriteRenderer;
        }

        private BallController CreateBall(bool followsPaddleWhenIdle)
        {
            var ball = ballSpawnService.CreateBall(followsPaddleWhenIdle, arenaBottom - 1f);
            ApplyGravityPocketToBall(ball);
            ApplySplitHorizonToBall(ball);
            ApplyCabinetTiltToBall(ball);
            return ball;
        }

        private BreakoutLevelLayoutPlan BuildLevelLayoutPlan(LevelDefinition level)
        {
            levelPlanner ??= new BreakoutLevelPlanner(loadedLevels, loadedBrickDefinitions, GenerateSeed);
            var plan = levelPlanner.BuildPlan(level, currentLevelIndex, gameplayRandom, activeRunSettings);
            currentLevelVariationLabel = plan.VariationSummary;
            return plan;
        }

        private void BuildBrickWall(BreakoutLevelLayoutPlan layoutPlan)
        {
            currentLevelRowCount = layoutPlan?.BrickRows?.Length ?? 0;
            currentLevelColumnCount = GetLayoutPlanColumnCount(layoutPlan);

            if (activeRunSettings != null
                && activeRunSettings.IsTurnBasedMode
                && turnBasedMultiplayerController != null
                && turnBasedMultiplayerController.TryGetCurrentPlayerBrickState(currentLevelIndex, out var savedBrickStates))
            {
                ResolveSavedBrickStateDimensions(savedBrickStates);
                requiredBricksRemaining = brickService != null
                    ? brickService.BuildBrickWall(savedBrickStates)
                    : 0;
                return;
            }

            requiredBricksRemaining = brickService != null
                ? brickService.BuildBrickWall(layoutPlan, arenaTop)
                : 0;
        }

        private static int GetLayoutPlanColumnCount(BreakoutLevelLayoutPlan layoutPlan)
        {
            if (layoutPlan == null || layoutPlan.BrickRows == null)
            {
                return 0;
            }

            var columnCount = 0;

            for (var rowIndex = 0; rowIndex < layoutPlan.BrickRows.Length; rowIndex++)
            {
                columnCount = Mathf.Max(columnCount, layoutPlan.BrickRows[rowIndex]?.Length ?? 0);
            }

            return columnCount;
        }

        private void ResolveSavedBrickStateDimensions(BreakoutBrickState[] savedBrickStates)
        {
            if (savedBrickStates == null || savedBrickStates.Length == 0)
            {
                return;
            }

            for (var index = 0; index < savedBrickStates.Length; index++)
            {
                currentLevelRowCount = Mathf.Max(currentLevelRowCount, savedBrickStates[index].Row + 1);
                currentLevelColumnCount = Mathf.Max(currentLevelColumnCount, savedBrickStates[index].Column + 1);
            }
        }

        private void ApplyLevelGlitchPlan(BreakoutLevelGlitchPlan glitchPlan)
        {
            ClearLevelGlitches();
            activeLevelGlitchPlan = glitchPlan ?? BreakoutLevelGlitchPlan.None;

            if (!activeLevelGlitchPlan.IsActive)
            {
                return;
            }

            runStatsService?.RegisterGlitchEncountered();

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.WarpJam))
            {
                CreateWarpGates(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("WARP JAM!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }
            else if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.WarpGates))
            {
                CreateWarpGates(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("WARP GATES!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RogueGate))
            {
                CreateRogueGate(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("ROGUE GATE!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TurboRail))
            {
                CreateTurboRail(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("TURBO RAIL!", new Color(1f, 0.22f, 0.84f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MirrorGrid))
            {
                ArmMirrorGrid();
                powerUpService?.ShowStatusBanner("MIRROR GRID!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.StaticJackpot))
            {
                CreateStaticJackpotZones(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("STATIC JACKPOT!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.JammedRails))
            {
                UpdateJammedRailsPulse();
                powerUpService?.ShowStatusBanner("JAMMED RAILS!", new Color(0.99f, 0.27f, 0.31f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravityPocket))
            {
                CreateGravityPocket(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("GRAVITY POCKET!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravitySwap))
            {
                CreateGravityPocket(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("GRAVITY SWAP!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TokenStorm))
            {
                powerUpService?.ShowStatusBanner("TOKEN STORM!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.NeonFlood))
            {
                powerUpService?.ShowStatusBanner("NEON FLOOD!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CapsuleRoulette))
            {
                powerUpService?.ShowStatusBanner("CAPSULE ROULETTE!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CapsuleBlackout))
            {
                powerUpService?.ShowStatusBanner("CAPSULE BLACKOUT!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.Brickquake))
            {
                powerUpService?.ShowStatusBanner("BRICKQUAKE!", new Color(1f, 0.49f, 0.15f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CabinetTilt))
            {
                powerUpService?.ShowStatusBanner("CABINET TILT!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.PickupPinball))
            {
                powerUpService?.ShowStatusBanner("PICKUP PINBALL!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.DropTide))
            {
                powerUpService?.ShowStatusBanner("DROP TIDE!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.BrickLock))
            {
                ArmBrickLock(activeLevelGlitchPlan.BrickLock);
                powerUpService?.ShowStatusBanner("BRICK LOCK!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MeltdownCore))
            {
                ArmMeltdownCore(activeLevelGlitchPlan.MeltdownCore);
                powerUpService?.ShowStatusBanner("MELTDOWN CORE!", new Color(1f, 0.49f, 0.15f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SpeedSteps))
            {
                powerUpService?.ShowStatusBanner("SPEED STEPS!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MagnetStorm))
            {
                CreateMagnetStorm(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("MAGNET STORM!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.DriftRows))
            {
                ApplyDriftRows(activeLevelGlitchPlan.DriftRows);
                powerUpService?.ShowStatusBanner("DRIFT ROWS!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.BrickConveyor))
            {
                ApplyBrickConveyor(activeLevelGlitchPlan.BrickConveyor);
                powerUpService?.ShowStatusBanner("BRICK CONVEYOR!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.LockstepRows))
            {
                ArmLockstepRows();
                powerUpService?.ShowStatusBanner("LOCKSTEP ROWS!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.StaticServe))
            {
                powerUpService?.ShowStatusBanner("STATIC SERVE!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.StaticWall))
            {
                CreateStaticWall(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("STATIC WALL!", new Color(0.99f, 0.27f, 0.31f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ThinAir))
            {
                CreateThinAirWall(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("THIN AIR!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RowRewrite))
            {
                ArmRowRewrite(activeLevelGlitchPlan.RowRewrite);
                powerUpService?.ShowStatusBanner("ROW REWRITE!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.PrismLanes))
            {
                CreatePrismLanes(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("PRISM LANES!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.PrismShuffle))
            {
                ApplyPrismShuffle(activeLevelGlitchPlan.PrismShuffle);
                powerUpService?.ShowStatusBanner("PRISM SHUFFLE!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CloneStatic))
            {
                paddle?.SetCloneStaticPaddleEnabled(true, activeLevelGlitchPlan.CloneStatic);
                powerUpService?.ShowStatusBanner("CLONE STATIC!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SwitchbackRails))
            {
                CreateSwitchbackRails(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("SWITCHBACK!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.HotCorners))
            {
                CreateHotCorners(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("HOT CORNERS!", new Color(1f, 0.49f, 0.15f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.FlickerBricks))
            {
                ApplyFlickerBricks(activeLevelGlitchPlan.FlickerBricks);
                powerUpService?.ShowStatusBanner("FLICKER BRICKS!", new Color(1f, 0.87f, 0.36f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.BlacklightBricks))
            {
                ApplyBlacklightBricks();
                powerUpService?.ShowStatusBanner("BLACKLIGHT!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RewindWall))
            {
                ArmRewindWall(activeLevelGlitchPlan.RewindWall);
                powerUpService?.ShowStatusBanner("REWIND WALL!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ScoreLeak))
            {
                ArmScoreLeak(activeLevelGlitchPlan.ScoreLeak);
                powerUpService?.ShowStatusBanner("SCORE LEAK!", new Color(0.99f, 0.27f, 0.31f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.LaserRain))
            {
                laserRainStrikeTimer = 0f;
                powerUpService?.ShowStatusBanner("LASER RAIN!", new Color(1f, 0.16f, 0.66f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CassetteSkip))
            {
                powerUpService?.ShowStatusBanner("CASSETTE SKIP!", new Color(1f, 0.49f, 0.86f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GhostRow))
            {
                ArmGhostRow(activeLevelGlitchPlan.GhostRow);
                powerUpService?.ShowStatusBanner("GHOST ROW!", new Color(0.72f, 0.62f, 1f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SplitHorizon))
            {
                CreateSplitHorizon(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("SPLIT HORIZON!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.VhsTear))
            {
                CreateVhsTear(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("VHS TEAR!", new Color(1f, 0.22f, 0.84f, 1f), 2.2f);
            }
        }

        private void ClearLevelGlitches()
        {
            activeLevelGlitchPlan = BreakoutLevelGlitchPlan.None;
            activeLevelElapsedSeconds = 0f;
            isMirrorGridArmed = false;
            hasMirrorGridTriggered = false;
            mirrorGridInitialRequiredBricks = 0;
            isRowRewriteArmed = false;
            hasRowRewriteTriggered = false;
            hasShownRowRewriteWarning = false;
            rowRewriteTimer = 0f;
            activeRowRewriteSpec = default;
            hasLockstepRowsPaddleAnchor = false;
            lockstepRowsLastPaddleX = 0f;
            scoreLeakGraceTimer = 0f;
            scoreLeakAccumulator = 0f;
            lastNeonFloodTriggerTime = float.NegativeInfinity;
            laserRainStrikeTimer = 0f;
            nextBrickquakeTriggerTime = 0f;
            nextMeltdownCoreBannerTime = 0f;
            activeMeltdownCoreBrick = null;
            cassetteSkipPaddleHits = 0;
            lastStaticServeRuleIndex = -1;
            gravitySwapPullSign = 1f;
            ghostRowService?.Clear();
            rewindWallService?.Clear();
            brickLockService?.Clear();
            ClearPendingLaserRainLanes();
            powerUpService?.ClearCapsuleBlackout();
            paddle?.SetCloneStaticPaddleEnabled(false);

            if (activeWarpGateController != null)
            {
                DestroyRuntimeObject(activeWarpGateController.gameObject);
                activeWarpGateController = null;
            }

            if (activeTurboRailSection != null)
            {
                DestroyRuntimeObject(activeTurboRailSection.gameObject);
                activeTurboRailSection = null;
            }

            if (activeStaticWallSection != null)
            {
                DestroyRuntimeObject(activeStaticWallSection.gameObject);
                activeStaticWallSection = null;
            }

            if (activeThinAirWallSection != null)
            {
                activeThinAirWallSection.Restore();
                activeThinAirWallSection.enabled = false;
                DestroyRuntimeObject(activeThinAirWallSection);
                activeThinAirWallSection = null;
            }

            if (activeGravityPocketVisual != null)
            {
                DestroyRuntimeObject(activeGravityPocketVisual.gameObject);
                activeGravityPocketVisual = null;
            }

            ClearMagnetStormVisuals();

            if (activePrismLaneField != null)
            {
                DestroyRuntimeObject(activePrismLaneField);
                activePrismLaneField = null;
            }

            if (activeSwitchbackRailField != null)
            {
                DestroyRuntimeObject(activeSwitchbackRailField);
                activeSwitchbackRailField = null;
            }

            if (activeHotCornerField != null)
            {
                DestroyRuntimeObject(activeHotCornerField);
                activeHotCornerField = null;
            }

            if (activeSplitHorizonField != null)
            {
                DestroyRuntimeObject(activeSplitHorizonField);
                activeSplitHorizonField = null;
            }

            if (activeVhsTearField != null)
            {
                DestroyRuntimeObject(activeVhsTearField);
                activeVhsTearField = null;
            }

            if (activeStaticJackpotField != null)
            {
                DestroyRuntimeObject(activeStaticJackpotField);
                activeStaticJackpotField = null;
            }

            ClearStaticServeRailField(showBanner: false);

            if (activeMirrorGridVisual != null)
            {
                DestroyRuntimeObject(activeMirrorGridVisual.gameObject);
                activeMirrorGridVisual = null;
            }

            brickService?.ClearFlickerBricks();
            brickService?.ClearBlacklightBricks();
            brickService?.ClearPrismShuffle();
            ClearMeltdownCoreMarks();
            ClearGravityPocketFromBalls();
            ClearMagnetStormFromPickups();
            ClearSplitHorizonFromBalls();
            ClearCabinetTiltFromBalls();
            ClearSpeedStepsFromBalls();

            if (paddle != null && activeEffectModifiers.PaddleWidthMultiplier > 0f)
            {
                paddle.SetWidthMultiplier(activeEffectModifiers.PaddleWidthMultiplier);
            }
        }

        private void ArmScoreLeak(BreakoutScoreLeakSpec scoreLeak)
        {
            scoreLeakGraceTimer = scoreLeak.GraceSeconds;
            scoreLeakAccumulator = 0f;
        }

        private void UpdateScoreLeak()
        {
            if (activeLevelGlitchPlan == null || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ScoreLeak))
            {
                return;
            }

            var scoreLeak = activeLevelGlitchPlan.ScoreLeak;

            if (roundState != RoundState.Playing)
            {
                scoreLeakGraceTimer = Mathf.Max(scoreLeakGraceTimer, scoreLeak.GraceSeconds);
                scoreLeakAccumulator = 0f;
                return;
            }

            if (scoreLeakGraceTimer > 0f)
            {
                scoreLeakGraceTimer = Mathf.Max(0f, scoreLeakGraceTimer - Time.deltaTime);
                return;
            }

            var penalty = BreakoutScoreLeakCalculator.CalculatePenalty(
                score,
                scoreLeak.PointsPerSecond,
                Time.deltaTime,
                ref scoreLeakAccumulator);

            if (penalty > 0)
            {
                score -= penalty;
            }
        }

        private void ResetScoreLeakOnBrickBreak()
        {
            if (activeLevelGlitchPlan == null || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ScoreLeak))
            {
                return;
            }

            scoreLeakGraceTimer = activeLevelGlitchPlan.ScoreLeak.GraceSeconds;
            scoreLeakAccumulator = 0f;
        }

        private bool IsScoreLeakDraining()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.ScoreLeak)
                && roundState == RoundState.Playing
                && scoreLeakGraceTimer <= 0f
                && score > 0;
        }

        private float GetScoreLeakVisualIntensity()
        {
            if (!IsScoreLeakDraining())
            {
                return 0f;
            }

            return Mathf.Clamp01(activeLevelGlitchPlan.ScoreLeak.PointsPerSecond / 16f);
        }

        private void ApplyDriftRows(BreakoutDriftRowsSpec driftRows)
        {
            brickService?.ApplyRowDrift(driftRows);
        }

        private void ApplyBrickConveyor(BreakoutBrickConveyorSpec brickConveyor)
        {
            brickService?.ApplyBrickConveyor(brickConveyor);
        }

        private void ArmLockstepRows()
        {
            hasLockstepRowsPaddleAnchor = paddle != null;
            lockstepRowsLastPaddleX = paddle != null ? paddle.transform.position.x : 0f;
        }

        private void UpdateLockstepRows()
        {
            if (paddle == null
                || brickService == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.LockstepRows))
            {
                hasLockstepRowsPaddleAnchor = false;
                return;
            }

            var currentPaddleX = paddle.transform.position.x;

            if (!IsGameplaySimulationActive())
            {
                lockstepRowsLastPaddleX = currentPaddleX;
                hasLockstepRowsPaddleAnchor = true;
                return;
            }

            if (!hasLockstepRowsPaddleAnchor)
            {
                lockstepRowsLastPaddleX = currentPaddleX;
                hasLockstepRowsPaddleAnchor = true;
                return;
            }

            var paddleDeltaX = currentPaddleX - lockstepRowsLastPaddleX;
            lockstepRowsLastPaddleX = currentPaddleX;
            brickService.ApplyLockstepRows(paddleDeltaX, activeLevelGlitchPlan.LockstepRows);
        }

        private void ApplyFlickerBricks(BreakoutFlickerBricksSpec flickerBricks)
        {
            brickService?.ApplyFlickerBricks(flickerBricks);
        }

        private void ApplyBlacklightBricks()
        {
            brickService?.ApplyBlacklightBricks();
        }

        private void ApplyPrismShuffle(BreakoutPrismShuffleSpec prismShuffle)
        {
            brickService?.ApplyPrismShuffle(prismShuffle);
        }

        private void ArmBrickLock(BreakoutBrickLockSpec brickLock)
        {
            brickLockService?.Arm(brickLock, currentLevelRowCount, currentLevelColumnCount);
        }

        private void ArmMeltdownCore(BreakoutMeltdownCoreSpec meltdownCore)
        {
            activeMeltdownCoreBrick = ResolveMeltdownCoreBrick(meltdownCore);
            nextMeltdownCoreBannerTime = 0f;
            RefreshMeltdownCoreMarks();
        }

        private Brick ResolveMeltdownCoreBrick(BreakoutMeltdownCoreSpec meltdownCore)
        {
            Brick bestBrick = null;
            var bestDistanceSquared = float.PositiveInfinity;
            var maxRow = Mathf.Max(1, currentLevelRowCount - 1);
            var maxColumn = Mathf.Max(1, currentLevelColumnCount - 1);
            var targetRow = meltdownCore.TargetRow * maxRow;
            var targetColumn = meltdownCore.TargetColumn * maxColumn;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (!IsMeltdownCoreCandidate(brick))
                {
                    continue;
                }

                var state = brick.CaptureState();
                var rowDistance = state.Row - targetRow;
                var columnDistance = state.Column - targetColumn;
                var distanceSquared = (rowDistance * rowDistance) + (columnDistance * columnDistance);

                if (distanceSquared >= bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                bestBrick = brick;
            }

            return bestBrick;
        }

        private static bool IsMeltdownCoreCandidate(Brick brick)
        {
            return brick != null
                && brick.Definition != null
                && brick.Definition.IsBreakable
                && brick.CountsTowardLevelCompletion;
        }

        private void RefreshMeltdownCoreMarks()
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MeltdownCore)
                || activeMeltdownCoreBrick == null)
            {
                ClearMeltdownCoreMarks();
                return;
            }

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (brick == activeMeltdownCoreBrick)
                {
                    brick.SetMeltdownCore(true);
                    continue;
                }

                brick.SetMeltdownOverclocked(brick.Definition != null && brick.Definition.IsBreakable);
            }
        }

        private void ClearMeltdownCoreMarks()
        {
            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.ClearMeltdownState();
            }
        }

        private void TryApplyMeltdownCoreOverclock(Brick brick, BallController scoringBall)
        {
            if (brick == null
                || scoringBall == null
                || brick.Definition == null
                || !brick.Definition.IsBreakable
                || activeMeltdownCoreBrick == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MeltdownCore)
                || brick == activeMeltdownCoreBrick)
            {
                return;
            }

            var meltdownCore = activeLevelGlitchPlan.MeltdownCore;
            scoringBall.ApplySpeedBurst(meltdownCore.SpeedBurstMultiplier, meltdownCore.SpeedBurstDurationSeconds);

            if (Time.time < nextMeltdownCoreBannerTime)
            {
                return;
            }

            nextMeltdownCoreBannerTime = Time.time + meltdownCore.BannerCooldownSeconds;
            powerUpService?.ShowStatusBanner("WALL OVERCLOCK!", new Color(1f, 0.49f, 0.15f, 1f), 0.9f);
        }

        private void TryClearMeltdownCore(Brick brick)
        {
            if (brick == null || brick != activeMeltdownCoreBrick)
            {
                return;
            }

            activeMeltdownCoreBrick = null;
            ClearMeltdownCoreMarks();
            SpawnExplosionVisual(brick.transform.position, MeltdownCorePulseRadius);
            powerUpService?.ShowStatusBanner("CORE COOLED!", new Color(1f, 0.87f, 0.36f, 1f), 1.35f);
        }

        private void ArmGhostRow(BreakoutGhostRowSpec ghostRow)
        {
            ghostRowService?.Arm(ghostRow, currentLevelRowCount);
        }

        private void UpdateGhostRow()
        {
            if (roundState != RoundState.Playing)
            {
                return;
            }

            ghostRowService?.Update(Time.deltaTime);
        }

        private void TryTriggerGhostRow(Brick brick)
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GhostRow)
                || ghostRowService == null)
            {
                return;
            }

            if (ghostRowService.TryTriggerFromHit(brick))
            {
                powerUpService?.ShowStatusBanner("ROW PHASE!", new Color(0.72f, 0.62f, 1f, 1f), 1.1f);
            }
        }

        private void ArmRewindWall(BreakoutRewindWallSpec rewindWall)
        {
            var removedObjectiveCount = rewindWallService?.Arm(rewindWall, currentLevelRowCount) ?? 0;

            if (removedObjectiveCount <= 0)
            {
                return;
            }

            requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining - removedObjectiveCount);
            EvaluateLevelCompletion();
        }

        private void UpdateRewindWall()
        {
            if (roundState != RoundState.Playing
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RewindWall)
                || rewindWallService == null)
            {
                return;
            }

            if (!rewindWallService.Update(Time.deltaTime, out var showWarning, out var rebuildStates))
            {
                if (showWarning)
                {
                    powerUpService?.ShowStatusBanner("TAPE REVERSING!", new Color(1f, 0.87f, 0.36f, 1f), 1.1f);
                }

                return;
            }

            if (brickService == null || rebuildStates == null || rebuildStates.Length == 0)
            {
                return;
            }

            brickService.BuildBrickWall(rebuildStates);
            ReapplyMotionGlitchesToNewBricks();
            RefreshMeltdownCoreMarks();
            powerUpService?.ShowStatusBanner("WALL REBUILT!", new Color(0.72f, 0.62f, 1f, 1f), 1.6f);
        }

        private void TryTriggerRewindWall(Brick brick)
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RewindWall)
                || rewindWallService == null)
            {
                return;
            }

            if (rewindWallService.TryRegisterDestroyedBrick(brick))
            {
                powerUpService?.ShowStatusBanner("REWIND ARMED!", new Color(1f, 0.87f, 0.36f, 1f), 1.1f);
            }
        }

        private void TryTriggerBrickLock(Brick brick)
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.BrickLock)
                || brickLockService == null)
            {
                return;
            }

            if (brickLockService.TryRegisterDestroyedBrick(brick))
            {
                powerUpService?.ShowStatusBanner("LOCK OPEN!", new Color(0.03f, 0.93f, 0.98f, 1f), 1.35f);
            }
        }

        private void TryApplySpeedSteps(BallController ball)
        {
            if (ball == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SpeedSteps))
            {
                return;
            }

            ball.ApplySpeedStep(
                activeLevelGlitchPlan.SpeedSteps.StepMultiplierIncrease,
                activeLevelGlitchPlan.SpeedSteps.MaximumMultiplier);
        }

        private void TryResetSpeedSteps(BallController ball)
        {
            if (ball == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SpeedSteps))
            {
                return;
            }

            ball.ResetSpeedSteps();
        }

        private void ClearSpeedStepsFromBalls()
        {
            serveBall?.ResetSpeedSteps();

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                activeBall.ResetSpeedSteps();
            }
        }

        private void ReapplyMotionGlitchesToNewBricks()
        {
            if (brickService == null || activeLevelGlitchPlan == null)
            {
                return;
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.DriftRows))
            {
                brickService.ApplyRowDrift(activeLevelGlitchPlan.DriftRows);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.BrickConveyor))
            {
                brickService.ApplyBrickConveyor(activeLevelGlitchPlan.BrickConveyor);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.LockstepRows))
            {
                hasLockstepRowsPaddleAnchor = false;
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.PrismShuffle))
            {
                brickService.ApplyPrismShuffle(activeLevelGlitchPlan.PrismShuffle);
            }

            if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MeltdownCore))
            {
                RefreshMeltdownCoreMarks();
            }
        }

        public void TryApplyCassetteSkip(BallController ball)
        {
            if (!IsGameplaySimulationActive()
                || ball == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CassetteSkip))
            {
                return;
            }

            var cassetteSkip = activeLevelGlitchPlan.CassetteSkip;
            cassetteSkipPaddleHits++;

            if (cassetteSkipPaddleHits < cassetteSkip.PaddleHitsPerSkip)
            {
                return;
            }

            cassetteSkipPaddleHits = 0;
            var velocity = ball.CurrentVelocity;
            var direction = velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector2.up;
            var targetPosition = (Vector2)ball.transform.position + (direction * cassetteSkip.SkipDistance);
            targetPosition.x = Mathf.Clamp(targetPosition.x, arenaLeft + ballRadius, arenaRight - ballRadius);
            targetPosition.y = Mathf.Clamp(targetPosition.y, arenaBottom + ballRadius, arenaTop - ballRadius);
            ball.SetWorldPosition(targetPosition);
            powerUpService?.ShowStatusBanner("TAPE SKIP!", new Color(1f, 0.49f, 0.86f, 1f), 0.95f);
        }

        private void ArmMirrorGrid()
        {
            mirrorGridInitialRequiredBricks = Mathf.Max(0, requiredBricksRemaining);
            isMirrorGridArmed = mirrorGridInitialRequiredBricks > 1;
            hasMirrorGridTriggered = false;
        }

        private void TryTriggerMirrorGrid()
        {
            if (!isMirrorGridArmed || hasMirrorGridTriggered || mirrorGridInitialRequiredBricks <= 1)
            {
                return;
            }

            var destroyedRequiredBricks = mirrorGridInitialRequiredBricks - Mathf.Max(0, requiredBricksRemaining);
            var triggerCount = Mathf.CeilToInt(mirrorGridInitialRequiredBricks * 0.5f);

            if (destroyedRequiredBricks < triggerCount)
            {
                return;
            }

            hasMirrorGridTriggered = true;
            isMirrorGridArmed = false;
            CreateMirrorGridBurst();
            brickService?.MirrorBrickGridHorizontally(0f);
            powerUpService?.ShowStatusBanner("GRID FLIP!", new Color(0.72f, 0.62f, 1f, 1f), 1.8f);
        }

        private void CreateMirrorGridBurst()
        {
            if (squareSprite == null || additiveSpriteMaterial == null)
            {
                return;
            }

            if (activeMirrorGridVisual != null)
            {
                DestroyRuntimeObject(activeMirrorGridVisual.gameObject);
                activeMirrorGridVisual = null;
            }

            var visualObject = new GameObject("Mirror Grid Burst");
            visualObject.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            activeMirrorGridVisual = visualObject.AddComponent<BreakoutMirrorGridVisual>();
            activeMirrorGridVisual.Configure(
                squareSprite,
                additiveSpriteMaterial,
                Rect.MinMaxRect(arenaLeft, arenaBottom + 0.35f, arenaRight, arenaTop));
        }

        private void CreateStaticJackpotZones(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || circleSprite == null)
            {
                return;
            }

            if (activeStaticJackpotField != null)
            {
                DestroyRuntimeObject(activeStaticJackpotField);
                activeStaticJackpotField = null;
            }

            activeStaticJackpotField = new GameObject("Static Jackpot Zones");
            activeStaticJackpotField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            var playfieldBounds = Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop);
            var zones = glitchPlan.StaticJackpot.Zones ?? Array.Empty<BreakoutStaticJackpotZoneSpec>();

            for (var index = 0; index < zones.Length; index++)
            {
                CreateStaticJackpotZoneVisual(activeStaticJackpotField.transform, playfieldBounds, zones[index], index);
            }
        }

        private void CreateStaticJackpotZoneVisual(
            Transform parent,
            Rect playfieldBounds,
            BreakoutStaticJackpotZoneSpec zone,
            int zoneIndex)
        {
            var zoneObject = new GameObject($"Static Jackpot Zone {zoneIndex + 1:00}");
            zoneObject.transform.SetParent(parent, false);
            zoneObject.transform.position = BreakoutStaticJackpotCalculator.ResolveZoneCenter(playfieldBounds, zone);
            var diameter = Mathf.Max(0.1f, zone.Radius * 2f);

            var glowRenderer = CreateStaticJackpotZoneLayer(
                zoneObject.transform,
                "Glow",
                diameter * 1.42f,
                ResolveStaticJackpotGlowColor(0.18f),
                StaticJackpotZoneSortingOrder - 1);
            var coreRenderer = CreateStaticJackpotZoneLayer(
                zoneObject.transform,
                "Core",
                diameter,
                ResolveStaticJackpotCoreColor(0.28f),
                StaticJackpotZoneSortingOrder);

            glowRenderer.sharedMaterial = additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial;
            coreRenderer.sharedMaterial = additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial;
        }

        private SpriteRenderer CreateStaticJackpotZoneLayer(
            Transform parent,
            string layerName,
            float diameter,
            Color color,
            int sortingOrder)
        {
            var layerObject = new GameObject(layerName);
            layerObject.transform.SetParent(parent, false);
            layerObject.transform.localScale = new Vector3(diameter, diameter, 1f);
            var renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private Color ResolveStaticJackpotCoreColor(float alpha)
        {
            var color = themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(1f, 0.28f, 0.66f, 1f),
                    circleSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private Color ResolveStaticJackpotGlowColor(float alpha)
        {
            var color = themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.BrickPrimary,
                    new Color(1f, 0.28f, 0.66f, 1f),
                    new Color(0.03f, 0.93f, 0.98f, 1f),
                    circleSprite).PrimaryColor
                : new Color(1f, 0.28f, 0.66f, 1f);
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private void ArmRowRewrite(BreakoutRowRewriteSpec rowRewrite)
        {
            activeRowRewriteSpec = rowRewrite;
            rowRewriteTimer = rowRewrite.TriggerSeconds;
            isRowRewriteArmed = true;
            hasRowRewriteTriggered = false;
            hasShownRowRewriteWarning = false;
        }

        private void UpdateRowRewriteTimer()
        {
            if (!isRowRewriteArmed
                || hasRowRewriteTriggered
                || roundState != RoundState.Playing
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.RowRewrite))
            {
                return;
            }

            rowRewriteTimer = Mathf.Max(0f, rowRewriteTimer - Time.deltaTime);

            if (!hasShownRowRewriteWarning && rowRewriteTimer <= activeRowRewriteSpec.WarningSeconds)
            {
                hasShownRowRewriteWarning = true;
                powerUpService?.ShowStatusBanner("ROW SHIFTING!", new Color(1f, 0.87f, 0.36f, 1f), 1.3f);
            }

            if (rowRewriteTimer > 0f)
            {
                return;
            }

            hasRowRewriteTriggered = true;
            isRowRewriteArmed = false;
            RewriteActiveBrickRow();
        }

        private void RewriteActiveBrickRow()
        {
            if (brickService == null
                || rowRewriteService == null
                || bricks.Count == 0
                || !rowRewriteService.TryBuildRewrite(
                    activeRowRewriteSpec,
                    currentLevelRowCount,
                    currentLevelColumnCount,
                    out var rowIndex,
                    out var rewrittenCells))
            {
                return;
            }

            var requiredBrickDelta = brickService.RewriteRow(rowIndex, rewrittenCells);

            requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining + requiredBrickDelta);
            if (activeLevelGlitchPlan != null && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.DriftRows))
            {
                brickService.ApplyRowDrift(activeLevelGlitchPlan.DriftRows);
            }

            powerUpService?.ShowStatusBanner("REWRITE LIVE!", new Color(0.72f, 0.62f, 1f, 1f), 1.8f);
            EvaluateLevelCompletion();
        }

        private void CreateWarpGates(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || glitchPlan.WarpGates.Length < 2 || squareSprite == null)
            {
                return;
            }

            var gateRoot = new GameObject("Warp Gates");
            gateRoot.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            activeWarpGateController = gateRoot.AddComponent<BreakoutWarpGateController>();
            activeWarpGateController.Configure(this, glitchPlan.WarpJam);

            for (var index = 0; index < glitchPlan.WarpGates.Length; index++)
            {
                CreateWarpGatePortal(gateRoot.transform, index, glitchPlan.WarpGates[index]);
            }
        }

        private void CreateRogueGate(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || glitchPlan.WarpGates.Length <= 0 || squareSprite == null)
            {
                return;
            }

            var gateRoot = new GameObject("Rogue Gate");
            gateRoot.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            activeWarpGateController = gateRoot.AddComponent<BreakoutWarpGateController>();
            activeWarpGateController.ConfigureRogueGate(this, ResolveWarpGatePosition, ResolveWarpGateExitPosition);
            CreateWarpGatePortal(gateRoot.transform, 0, glitchPlan.WarpGates[0]);
        }

        private void CreateWarpGatePortal(Transform gateRoot, int portalIndex, BreakoutWarpGateSpec spec)
        {
            var portalObject = new GameObject($"Warp Gate {portalIndex + 1:00}");
            portalObject.transform.SetParent(gateRoot, false);
            portalObject.transform.position = ResolveWarpGatePosition(spec);

            var portal = portalObject.AddComponent<BreakoutWarpGatePortal>();
            portal.Configure(activeWarpGateController, portalIndex, spec, ResolveWarpGateExitPosition(spec));
            activeWarpGateController?.RegisterPortal(portal);

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = ResolveWarpGateTriggerSize(spec.Wall);

            var visual = portalObject.AddComponent<BreakoutWarpGateVisual>();
            visual.Configure(
                circleSprite,
                warpGateVortexSprite,
                warpGateRingSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                spec.Wall,
                portalIndex);
        }

        private Vector2 ResolveWarpGatePosition(BreakoutWarpGateSpec spec)
        {
            return spec.Wall switch
            {
                BreakoutWarpGateWall.Left => new Vector2(arenaLeft + 0.18f, Mathf.Lerp(arenaBottom + 1.45f, arenaTop - 0.8f, spec.NormalizedPosition)),
                BreakoutWarpGateWall.Right => new Vector2(arenaRight - 0.18f, Mathf.Lerp(arenaBottom + 1.45f, arenaTop - 0.8f, spec.NormalizedPosition)),
                BreakoutWarpGateWall.Top => new Vector2(Mathf.Lerp(arenaLeft + 0.95f, arenaRight - 0.95f, spec.NormalizedPosition), arenaTop - 0.18f),
                _ => Vector2.zero,
            };
        }

        private Vector2 ResolveWarpGateExitPosition(BreakoutWarpGateSpec spec)
        {
            var position = ResolveWarpGatePosition(spec);

            return spec.Wall switch
            {
                BreakoutWarpGateWall.Left => new Vector2(arenaLeft + 0.58f, position.y),
                BreakoutWarpGateWall.Right => new Vector2(arenaRight - 0.58f, position.y),
                BreakoutWarpGateWall.Top => new Vector2(position.x, arenaTop - 0.58f),
                _ => position,
            };
        }

        private static Vector2 ResolveWarpGateTriggerSize(BreakoutWarpGateWall wall)
        {
            return wall == BreakoutWarpGateWall.Top
                ? new Vector2(0.78f, 0.54f)
                : new Vector2(0.54f, 0.78f);
        }

        private void CreateTurboRail(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            var railObject = new GameObject("Turbo Rail");
            railObject.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            railObject.transform.position = ResolveTurboRailPosition(glitchPlan.TurboRail);

            var collider = railObject.AddComponent<BoxCollider2D>();
            collider.size = ResolveTurboRailSize(glitchPlan.TurboRail);
            collider.sharedMaterial = bounceMaterial;

            var visual = railObject.AddComponent<BreakoutTurboRailVisual>();
            visual.Configure(
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                glitchPlan.TurboRail.Wall,
                collider.size,
                0);

            activeTurboRailSection = railObject.AddComponent<BreakoutTurboRailSection>();
            activeTurboRailSection.Configure(
                this,
                glitchPlan.TurboRail.Wall,
                TurboRailSpeedBurstMultiplier,
                TurboRailSpeedBurstDuration,
                TurboRailSpeedBurstStackMultiplier,
                TurboRailSpeedBurstMaximumMultiplier,
                TurboRailSpeedBurstStackDuration,
                TurboRailSpeedBurstMaximumDuration,
                visual);
        }

        private void ArmStaticServeRailForServe()
        {
            ClearStaticServeRailField(showBanner: false);

            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.StaticServe)
                || squareSprite == null)
            {
                return;
            }

            var rule = ResolveNextStaticServeRailRule();
            activeStaticServeRailField = new GameObject("Static Serve Rails");
            activeStaticServeRailField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            switch (rule)
            {
                case StaticServeRailRule.StaticWall:
                    CreateStaticServeStaticWall(activeStaticServeRailField.transform);
                    powerUpService?.ShowStatusBanner("STATIC SERVE: WALL!", new Color(0.99f, 0.27f, 0.31f, 1f), 1.45f);
                    break;
                case StaticServeRailRule.SwitchbackRails:
                    CreateStaticServeSwitchbackRails(activeStaticServeRailField.transform);
                    powerUpService?.ShowStatusBanner("STATIC SERVE: SWITCH!", new Color(0.03f, 0.93f, 0.98f, 1f), 1.45f);
                    break;
                default:
                    CreateStaticServeTurboRail(activeStaticServeRailField.transform);
                    powerUpService?.ShowStatusBanner("STATIC SERVE: TURBO!", new Color(1f, 0.22f, 0.84f, 1f), 1.45f);
                    break;
            }
        }

        private StaticServeRailRule ResolveNextStaticServeRailRule()
        {
            var ruleCount = activeLevelGlitchPlan != null && activeLevelGlitchPlan.StaticServe.RuleCount > 0
                ? activeLevelGlitchPlan.StaticServe.RuleCount
                : StaticServeRuleCount;
            ruleCount = Mathf.Clamp(ruleCount, 1, StaticServeRuleCount);

            if (ruleCount <= 1)
            {
                lastStaticServeRuleIndex = 0;
                return StaticServeRailRule.TurboRail;
            }

            if (lastStaticServeRuleIndex < 0)
            {
                lastStaticServeRuleIndex = Mathf.Clamp(Mathf.FloorToInt(NextGameplayRandomFloat(0f, ruleCount)), 0, ruleCount - 1);
                return (StaticServeRailRule)lastStaticServeRuleIndex;
            }

            var step = 1 + Mathf.Clamp(Mathf.FloorToInt(NextGameplayRandomFloat(0f, ruleCount - 1)), 0, ruleCount - 2);
            lastStaticServeRuleIndex = (lastStaticServeRuleIndex + step) % ruleCount;
            return (StaticServeRailRule)lastStaticServeRuleIndex;
        }

        private void CreateStaticServeTurboRail(Transform root)
        {
            var spec = new BreakoutTurboRailSpec(
                ResolveStaticServeRailWall(allowTopRail: true),
                NextGameplayRandomFloat(0.24f, 0.76f),
                NextGameplayRandomFloat(0.24f, 0.36f));
            var railObject = new GameObject("Static Serve Turbo Rail");
            railObject.transform.SetParent(root, false);
            railObject.transform.position = ResolveTurboRailPosition(spec);

            var collider = railObject.AddComponent<BoxCollider2D>();
            collider.size = ResolveTurboRailSize(spec);
            collider.sharedMaterial = bounceMaterial;

            var visual = railObject.AddComponent<BreakoutTurboRailVisual>();
            visual.Configure(
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                spec.Wall,
                collider.size,
                0);

            var rail = railObject.AddComponent<BreakoutTurboRailSection>();
            rail.Configure(
                this,
                spec.Wall,
                TurboRailSpeedBurstMultiplier,
                TurboRailSpeedBurstDuration,
                TurboRailSpeedBurstStackMultiplier,
                TurboRailSpeedBurstMaximumMultiplier,
                TurboRailSpeedBurstStackDuration,
                TurboRailSpeedBurstMaximumDuration,
                visual);
        }

        private void CreateStaticServeStaticWall(Transform root)
        {
            var spec = new BreakoutStaticWallSpec(
                ResolveStaticServeRailWall(allowTopRail: false),
                NextGameplayRandomFloat(1.7f, 2.25f),
                NextGameplayRandomFloat(0.58f, 0.82f),
                NextGameplayRandomFloat(0f, 2.25f));
            var staticWallObject = new GameObject("Static Serve Wall");
            staticWallObject.transform.SetParent(root, false);
            staticWallObject.transform.position = ResolveStaticWallPosition(spec);

            var size = ResolveStaticWallSize();
            staticWallObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = staticWallObject.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sharedMaterial = additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial;
            renderer.sortingOrder = 12;

            var collider = staticWallObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            var staticWall = staticWallObject.AddComponent<BreakoutStaticWallSection>();
            staticWall.Configure(
                this,
                spec.Wall,
                renderer,
                size,
                spec.WeakCycleSeconds,
                spec.WeakDurationSeconds,
                spec.PhaseOffsetSeconds);
        }

        private void CreateStaticServeSwitchbackRails(Transform root)
        {
            var spec = new BreakoutSwitchbackRailSpec(
                NextGameplayRandomFloat(0.38f, 0.7f),
                NextGameplayRandomFloat(0.28f, 0.42f),
                NextGameplayRandomFloat(2f, 2.75f),
                NextGameplayRandomFloat(0f, 2.75f));
            CreateSwitchbackRail(root, 0, BreakoutWarpGateWall.Left, spec);
            CreateSwitchbackRail(root, 1, BreakoutWarpGateWall.Right, spec);
        }

        private BreakoutWarpGateWall ResolveStaticServeRailWall(bool allowTopRail)
        {
            var wallCount = allowTopRail ? 3 : 2;
            var wallIndex = Mathf.Clamp(Mathf.FloorToInt(NextGameplayRandomFloat(0f, wallCount)), 0, wallCount - 1);
            return wallIndex switch
            {
                1 => BreakoutWarpGateWall.Right,
                2 => BreakoutWarpGateWall.Top,
                _ => BreakoutWarpGateWall.Left,
            };
        }

        private void ClearStaticServeRailField(bool showBanner)
        {
            if (activeStaticServeRailField == null)
            {
                return;
            }

            DestroyRuntimeObject(activeStaticServeRailField);
            activeStaticServeRailField = null;

            if (showBanner)
            {
                powerUpService?.ShowStatusBanner("STATIC CLEARED!", new Color(0.72f, 0.62f, 1f, 1f), 0.9f);
            }
        }

        private Vector2 ResolveTurboRailPosition(BreakoutTurboRailSpec spec)
        {
            return spec.Wall switch
            {
                BreakoutWarpGateWall.Left => new Vector2(arenaLeft + 0.06f, Mathf.Lerp(arenaBottom + 1.35f, arenaTop - 0.95f, spec.NormalizedPosition)),
                BreakoutWarpGateWall.Right => new Vector2(arenaRight - 0.06f, Mathf.Lerp(arenaBottom + 1.35f, arenaTop - 0.95f, spec.NormalizedPosition)),
                BreakoutWarpGateWall.Top => new Vector2(Mathf.Lerp(arenaLeft + 1.05f, arenaRight - 1.05f, spec.NormalizedPosition), arenaTop - 0.06f),
                _ => Vector2.zero,
            };
        }

        private Vector2 ResolveTurboRailSize(BreakoutTurboRailSpec spec)
        {
            if (spec.Wall == BreakoutWarpGateWall.Top)
            {
                return new Vector2(Mathf.Lerp(1.9f, 3.1f, spec.NormalizedLength), 0.22f);
            }

            return new Vector2(0.22f, Mathf.Lerp(1.7f, 2.85f, spec.NormalizedLength));
        }

        private void CreateStaticWall(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            var staticWallObject = new GameObject("Static Wall");
            staticWallObject.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            staticWallObject.transform.position = ResolveStaticWallPosition(glitchPlan.StaticWall);

            var size = ResolveStaticWallSize();
            staticWallObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = staticWallObject.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sharedMaterial = additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial;
            renderer.sortingOrder = 11;

            var collider = staticWallObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            activeStaticWallSection = staticWallObject.AddComponent<BreakoutStaticWallSection>();
            activeStaticWallSection.Configure(
                this,
                glitchPlan.StaticWall.Wall,
                renderer,
                size,
                glitchPlan.StaticWall.WeakCycleSeconds,
                glitchPlan.StaticWall.WeakDurationSeconds,
                glitchPlan.StaticWall.PhaseOffsetSeconds);
        }

        private Vector2 ResolveStaticWallPosition(BreakoutStaticWallSpec spec)
        {
            var x = spec.Wall == BreakoutWarpGateWall.Right
                ? arenaRight - (StaticWallThickness * 0.5f)
                : arenaLeft + (StaticWallThickness * 0.5f);
            var minY = arenaBottom + StaticWallBottomInset;
            var maxY = arenaTop - StaticWallTopInset;

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return new Vector2(x, (minY + maxY) * 0.5f);
        }

        private Vector2 ResolveStaticWallSize()
        {
            var height = Mathf.Max(1f, (arenaTop - StaticWallTopInset) - (arenaBottom + StaticWallBottomInset));
            return new Vector2(StaticWallThickness, height);
        }

        private void CreateThinAirWall(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null)
            {
                return;
            }

            var wall = glitchPlan.ThinAir.Wall;
            var renderer = wall == BreakoutWarpGateWall.Right ? rightWallRenderer : leftWallRenderer;
            var collider = wall == BreakoutWarpGateWall.Right ? rightWallCollider : leftWallCollider;

            if (renderer == null || collider == null)
            {
                return;
            }

            activeThinAirWallSection = collider.gameObject.AddComponent<BreakoutThinAirWallSection>();
            activeThinAirWallSection.Configure(
                wall,
                renderer,
                collider,
                glitchPlan.ThinAir.OpenCycleSeconds,
                glitchPlan.ThinAir.OpenDurationSeconds,
                glitchPlan.ThinAir.WarningSeconds,
                glitchPlan.ThinAir.PhaseOffsetSeconds);
        }

        private void CreatePrismLanes(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || glitchPlan.PrismLanes.Length <= 0 || squareSprite == null)
            {
                return;
            }

            activePrismLaneField = new GameObject("Prism Lanes");
            activePrismLaneField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            for (var index = 0; index < glitchPlan.PrismLanes.Length; index++)
            {
                CreatePrismLane(activePrismLaneField.transform, index, glitchPlan.PrismLanes[index]);
            }
        }

        private void CreatePrismLane(Transform laneRoot, int laneIndex, BreakoutPrismLaneSpec spec)
        {
            var laneObject = new GameObject($"Prism Lane {laneIndex + 1:00}");
            laneObject.transform.SetParent(laneRoot, false);
            laneObject.transform.position = ResolvePrismLanePosition(spec);

            var laneSize = ResolvePrismLaneSize(spec);
            var collider = laneObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = laneSize;

            var visual = laneObject.AddComponent<BreakoutPrismLaneVisual>();
            visual.Configure(
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                laneSize,
                laneIndex,
                spec.RefractionSign);

            var lane = laneObject.AddComponent<BreakoutPrismLaneSection>();
            lane.Configure(spec.RefractionSign, visual);
        }

        private Vector2 ResolvePrismLanePosition(BreakoutPrismLaneSpec spec)
        {
            var x = Mathf.Lerp(arenaLeft + 0.95f, arenaRight - 0.95f, spec.NormalizedX);
            var minY = arenaBottom + PrismLaneBottomInset;
            var maxY = arenaTop - PrismLaneTopInset;

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return new Vector2(x, (minY + maxY) * 0.5f);
        }

        private Vector2 ResolvePrismLaneSize(BreakoutPrismLaneSpec spec)
        {
            var width = Mathf.Lerp(0.22f, 0.52f, spec.NormalizedWidth / 0.14f);
            var height = Mathf.Max(1f, (arenaTop - PrismLaneTopInset) - (arenaBottom + PrismLaneBottomInset));
            return new Vector2(width, height);
        }

        private void CreateSplitHorizon(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            activeSplitHorizonField = new GameObject("Split Horizon");
            activeSplitHorizonField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            activeSplitHorizonField.transform.position = new Vector2(0f, ResolveSplitHorizonY(glitchPlan.SplitHorizon));

            var width = Mathf.Max(1f, arenaRight - arenaLeft);
            CreateSplitHorizonLayer(
                "Split Horizon Glow",
                activeSplitHorizonField.transform,
                width,
                SplitHorizonGlowThickness,
                new Color(0.03f, 0.93f, 0.98f, 0.16f),
                13,
                additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial);
            CreateSplitHorizonLayer(
                "Split Horizon Core",
                activeSplitHorizonField.transform,
                width,
                SplitHorizonLineThickness,
                new Color(0.98f, 1f, 1f, 0.74f),
                18,
                additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial);
            CreateSplitHorizonLayer(
                "Split Horizon Magenta Trace",
                activeSplitHorizonField.transform,
                width,
                SplitHorizonLineThickness * 0.55f,
                new Color(1f, 0.22f, 0.84f, 0.72f),
                19,
                additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial,
                new Vector2(0f, SplitHorizonLineThickness * 1.35f));
            CreateSplitHorizonLayer(
                "Split Horizon Cyan Trace",
                activeSplitHorizonField.transform,
                width,
                SplitHorizonLineThickness * 0.55f,
                new Color(0.03f, 0.93f, 0.98f, 0.72f),
                19,
                additiveSpriteMaterial != null ? additiveSpriteMaterial : spriteUnlitMaterial,
                new Vector2(0f, -SplitHorizonLineThickness * 1.35f));

            ApplySplitHorizonToBalls();
        }

        private void CreateVhsTear(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            activeVhsTearField = new GameObject("VHS Tear");
            activeVhsTearField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            activeVhsTearField.transform.position = new Vector2(0f, ResolveVhsTearY(glitchPlan.VhsTear));

            var width = Mathf.Max(1f, arenaRight - arenaLeft);
            var collider = activeVhsTearField.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(width, VhsTearTriggerThickness);

            var visual = activeVhsTearField.AddComponent<BreakoutVhsTearVisual>();
            visual.Configure(
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                width,
                VhsTearLineThickness,
                glitchPlan.VhsTear.JitterStrength);

            var tear = activeVhsTearField.AddComponent<BreakoutVhsTearSection>();
            tear.Configure(
                glitchPlan.VhsTear.DeflectionDegrees,
                glitchPlan.VhsTear.JitterStrength,
                glitchPlan.VhsTear.CooldownSeconds,
                visual);
        }

        private void CreateSplitHorizonLayer(
            string layerName,
            Transform root,
            float width,
            float height,
            Color color,
            int sortingOrder,
            Material material,
            Vector2 localPosition = default)
        {
            var layer = new GameObject(layerName);
            layer.transform.SetParent(root, false);
            layer.transform.localPosition = localPosition;
            layer.transform.localScale = new Vector3(width, height, 1f);

            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
        }

        private float ResolveSplitHorizonY(BreakoutSplitHorizonSpec spec)
        {
            var minY = arenaBottom + 1.45f;
            var maxY = arenaTop - 1.65f;

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return Mathf.Lerp(minY, maxY, spec.NormalizedY);
        }

        private float ResolveVhsTearY(BreakoutVhsTearSpec spec)
        {
            var minY = arenaBottom + 1.65f;
            var maxY = arenaTop - 1.45f;

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return Mathf.Lerp(minY, maxY, spec.NormalizedY);
        }

        private void ApplySplitHorizonToBalls()
        {
            ApplySplitHorizonToBall(serveBall);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                ApplySplitHorizonToBall(activeBalls[index]);
            }
        }

        private void ApplySplitHorizonToBall(BallController ball)
        {
            if (ball == null)
            {
                return;
            }

            if (activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.SplitHorizon))
            {
                var spec = activeLevelGlitchPlan.SplitHorizon;
                ball.SetSplitHorizon(ResolveSplitHorizonY(spec), spec.BendDegrees, spec.CooldownSeconds);
                return;
            }

            ball.SetSplitHorizon(0f, 0f, SplitHorizonCooldownSeconds);
        }

        private void ClearSplitHorizonFromBalls()
        {
            if (serveBall != null)
            {
                serveBall.SetSplitHorizon(0f, 0f, SplitHorizonCooldownSeconds);
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                activeBalls[index]?.SetSplitHorizon(0f, 0f, SplitHorizonCooldownSeconds);
            }
        }

        private void ApplyCabinetTiltToBall(BallController ball)
        {
            if (ball == null)
            {
                return;
            }

            if (activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CabinetTilt))
            {
                var spec = activeLevelGlitchPlan.CabinetTilt;
                ball.SetCabinetTilt(spec.Strength, spec.PhaseOffsetSeconds, spec.CycleSeconds);
                return;
            }

            ball.SetCabinetTilt(0f, 0f, 3.2f);
        }

        private void ClearCabinetTiltFromBalls()
        {
            serveBall?.SetCabinetTilt(0f, 0f, 3.2f);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                activeBalls[index]?.SetCabinetTilt(0f, 0f, 3.2f);
            }
        }

        private void CreateSwitchbackRails(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            activeSwitchbackRailField = new GameObject("Switchback Rails");
            activeSwitchbackRailField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            CreateSwitchbackRail(activeSwitchbackRailField.transform, 0, BreakoutWarpGateWall.Left, glitchPlan.SwitchbackRails);
            CreateSwitchbackRail(activeSwitchbackRailField.transform, 1, BreakoutWarpGateWall.Right, glitchPlan.SwitchbackRails);
        }

        private void CreateSwitchbackRail(Transform railRoot, int railIndex, BreakoutWarpGateWall wall, BreakoutSwitchbackRailSpec spec)
        {
            var railObject = new GameObject($"Switchback Rail {railIndex + 1:00}");
            railObject.transform.SetParent(railRoot, false);
            railObject.transform.position = ResolveSwitchbackRailPosition(wall, spec);

            var size = ResolveSwitchbackRailSize(spec);
            var collider = railObject.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = bounceMaterial;

            var visual = railObject.AddComponent<BreakoutSwitchbackRailVisual>();
            visual.Configure(
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                wall,
                size,
                railIndex,
                spec.SwitchCycleSeconds,
                spec.PhaseOffsetSeconds + (railIndex * spec.SwitchCycleSeconds * 0.5f));

            var rail = railObject.AddComponent<BreakoutSwitchbackRailSection>();
            rail.Configure(
                this,
                wall,
                spec.SwitchCycleSeconds,
                spec.PhaseOffsetSeconds + (railIndex * spec.SwitchCycleSeconds * 0.5f),
                visual);
        }

        private Vector2 ResolveSwitchbackRailPosition(BreakoutWarpGateWall wall, BreakoutSwitchbackRailSpec spec)
        {
            var x = wall == BreakoutWarpGateWall.Right
                ? arenaRight - (SwitchbackRailThickness * 0.5f)
                : arenaLeft + (SwitchbackRailThickness * 0.5f);
            var minY = arenaBottom + SwitchbackRailBottomInset;
            var maxY = arenaTop - SwitchbackRailTopInset;

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return new Vector2(x, Mathf.Lerp(minY, maxY, spec.NormalizedPosition));
        }

        private Vector2 ResolveSwitchbackRailSize(BreakoutSwitchbackRailSpec spec)
        {
            var minY = arenaBottom + SwitchbackRailBottomInset;
            var maxY = arenaTop - SwitchbackRailTopInset;
            var usableHeight = Mathf.Max(1f, maxY - minY);
            return new Vector2(SwitchbackRailThickness, Mathf.Clamp(usableHeight * spec.NormalizedLength, 1.35f, 3.6f));
        }

        private void CreateHotCorners(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || squareSprite == null)
            {
                return;
            }

            activeHotCornerField = new GameObject("Hot Corners");
            activeHotCornerField.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);

            CreateHotCorner(activeHotCornerField.transform, true, glitchPlan.HotCorners);
            CreateHotCorner(activeHotCornerField.transform, false, glitchPlan.HotCorners);
        }

        private void CreateHotCorner(Transform cornerRoot, bool isLeftCorner, BreakoutHotCornersSpec spec)
        {
            var cornerObject = new GameObject(isLeftCorner ? "Hot Corner Left" : "Hot Corner Right");
            cornerObject.transform.SetParent(cornerRoot, false);
            cornerObject.transform.position = ResolveHotCornerPosition(isLeftCorner, spec);
            cornerObject.transform.rotation = Quaternion.Euler(0f, 0f, isLeftCorner ? 45f : -45f);

            var size = Vector2.one * spec.BumperSize;
            var collider = cornerObject.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = bounceMaterial;

            var bumper = cornerObject.AddComponent<BreakoutHotCornerBumper>();
            bumper.Configure(
                this,
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                size,
                ResolveHotCornerTargetPoint(),
                spec.SpeedBurstMultiplier,
                spec.SpeedBurstDurationSeconds);
        }

        private Vector2 ResolveHotCornerPosition(bool isLeftCorner, BreakoutHotCornersSpec spec)
        {
            var halfSize = spec.BumperSize * 0.5f;
            var x = isLeftCorner
                ? arenaLeft + halfSize + HotCornerInset
                : arenaRight - halfSize - HotCornerInset;
            var y = arenaTop - halfSize - HotCornerInset;
            return new Vector2(x, y);
        }

        private Vector2 ResolveHotCornerTargetPoint()
        {
            return new Vector2(0f, Mathf.Lerp(arenaBottom + 1.4f, arenaTop - 1.8f, 0.52f));
        }

        private void CreateGravityPocket(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || circleSprite == null)
            {
                return;
            }

            var pocketObject = new GameObject("Gravity Pocket");
            pocketObject.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
            pocketObject.transform.position = ResolveGravityPocketPosition(glitchPlan.GravityPocket);

            activeGravityPocketVisual = pocketObject.AddComponent<BreakoutGravityPocketVisual>();
            activeGravityPocketVisual.Configure(
                circleSprite,
                squareSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                glitchPlan.GravityPocket.Radius,
                ResolveGravityPocketMovementBounds(),
                glitchPlan.GravityPocket.DriftSpeed,
                glitchPlan.GravityPocket.DriftPhase);

            ApplyGravityPocketToBalls();
        }

        private void TryFlipGravitySwapPolarity()
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravitySwap)
                || activeGravityPocketVisual == null)
            {
                return;
            }

            gravitySwapPullSign = gravitySwapPullSign > 0f ? -1f : 1f;
            activeGravityPocketVisual.SetPullDirectionSign(gravitySwapPullSign);
            ApplyGravityPocketToBalls();
        }

        private void CreateMagnetStorm(BreakoutLevelGlitchPlan glitchPlan)
        {
            if (glitchPlan == null || circleSprite == null || glitchPlan.MagnetStormPockets.Length <= 0)
            {
                return;
            }

            ClearMagnetStormVisuals();

            for (var index = 0; index < glitchPlan.MagnetStormPockets.Length; index++)
            {
                var spec = glitchPlan.MagnetStormPockets[index];
                var pocketObject = new GameObject($"Magnet Storm Pocket {index + 1:00}");
                pocketObject.transform.SetParent(glitchesRoot != null ? glitchesRoot : runtimeRoot, false);
                pocketObject.transform.position = ResolveGravityPocketPosition(spec);

                var visual = pocketObject.AddComponent<BreakoutGravityPocketVisual>();
                visual.Configure(
                    circleSprite,
                    squareSprite,
                    spriteUnlitMaterial,
                    additiveSpriteMaterial,
                    spec.Radius,
                    ResolveGravityPocketMovementBounds(),
                    spec.DriftSpeed,
                    spec.DriftPhase);
                activeMagnetStormVisuals.Add(visual);
            }

            ApplyGravityPocketToBalls();
            RefreshMagnetStormPickupTargets();
        }

        private void ClearMagnetStormVisuals()
        {
            for (var index = activeMagnetStormVisuals.Count - 1; index >= 0; index--)
            {
                var visual = activeMagnetStormVisuals[index];

                if (visual != null)
                {
                    DestroyRuntimeObject(visual.gameObject);
                }
            }

            activeMagnetStormVisuals.Clear();
        }

        private Vector2 ResolveGravityPocketPosition(BreakoutGravityPocketSpec spec)
        {
            var minX = arenaLeft + GravityPocketArenaHorizontalPadding;
            var maxX = arenaRight - GravityPocketArenaHorizontalPadding;
            var minY = arenaBottom + GravityPocketArenaBottomPadding;
            var maxY = arenaTop - GravityPocketArenaTopPadding;

            if (maxX <= minX)
            {
                minX = arenaLeft;
                maxX = arenaRight;
            }

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return new Vector2(
                Mathf.Lerp(minX, maxX, spec.NormalizedX),
                Mathf.Lerp(minY, maxY, spec.NormalizedY));
        }

        private Rect ResolveGravityPocketMovementBounds()
        {
            var minX = arenaLeft + GravityPocketArenaHorizontalPadding;
            var maxX = arenaRight - GravityPocketArenaHorizontalPadding;
            var minY = arenaBottom + GravityPocketArenaBottomPadding;
            var maxY = arenaTop - GravityPocketArenaTopPadding;

            if (maxX <= minX)
            {
                minX = arenaLeft;
                maxX = arenaRight;
            }

            if (maxY <= minY)
            {
                minY = arenaBottom;
                maxY = arenaTop;
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private void UpdateGravityPocketInfluence()
        {
            if (activeGravityPocketVisual == null
                || activeLevelGlitchPlan == null
                || (!activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravityPocket)
                    && !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravitySwap)))
            {
                return;
            }

            activeGravityPocketVisual.Tick(Time.time);
            ApplyGravityPocketToBalls();
        }

        private void UpdateMagnetStormInfluence()
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MagnetStorm)
                || activeMagnetStormVisuals.Count == 0)
            {
                return;
            }

            for (var index = activeMagnetStormVisuals.Count - 1; index >= 0; index--)
            {
                var visual = activeMagnetStormVisuals[index];

                if (visual == null)
                {
                    activeMagnetStormVisuals.RemoveAt(index);
                    continue;
                }

                visual.Tick(Time.time);
            }

            ApplyGravityPocketToBalls();
            RefreshMagnetStormPickupTargets();
        }

        private void ApplyGravityPocketToBalls()
        {
            ApplyGravityPocketToBall(serveBall);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                ApplyGravityPocketToBall(activeBalls[index]);
            }
        }

        private void ApplyGravityPocketToBall(BallController ball)
        {
            if (ball == null)
            {
                return;
            }

            if (TryResolveStrongestPocket(ball.transform.position, out var centerPoint, out var radius, out var strength))
            {
                ball.SetGravityPocket(centerPoint, radius, strength);
                return;
            }

            ball.SetGravityPocket(Vector2.zero, 0f, 0f);
        }

        private void ClearGravityPocketFromBalls()
        {
            if (serveBall != null)
            {
                serveBall.SetGravityPocket(Vector2.zero, 0f, 0f);
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                activeBalls[index]?.SetGravityPocket(Vector2.zero, 0f, 0f);
            }
        }

        private bool TryResolveStrongestPocket(
            Vector2 worldPosition,
            out Vector2 centerPoint,
            out float radius,
            out float strength)
        {
            centerPoint = Vector2.zero;
            radius = 0f;
            strength = 0f;
            var bestInfluence = 0f;

            if (activeLevelGlitchPlan != null
                && (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravityPocket)
                    || activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravitySwap))
                && activeGravityPocketVisual != null)
            {
                var pocketStrength = activeLevelGlitchPlan.GravityPocket.Strength;
                if (activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.GravitySwap))
                {
                    pocketStrength *= gravitySwapPullSign;
                }

                TryUseStrongerPocket(
                    worldPosition,
                    activeGravityPocketVisual.transform.position,
                    activeLevelGlitchPlan.GravityPocket.Radius,
                    pocketStrength,
                    ref centerPoint,
                    ref radius,
                    ref strength,
                    ref bestInfluence);
            }

            if (activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.MagnetStorm))
            {
                var pocketCount = Mathf.Min(activeMagnetStormVisuals.Count, activeLevelGlitchPlan.MagnetStormPockets.Length);

                for (var index = 0; index < pocketCount; index++)
                {
                    var visual = activeMagnetStormVisuals[index];

                    if (visual == null)
                    {
                        continue;
                    }

                    var spec = activeLevelGlitchPlan.MagnetStormPockets[index];
                    TryUseStrongerPocket(
                        worldPosition,
                        visual.transform.position,
                        spec.Radius,
                        spec.Strength,
                        ref centerPoint,
                        ref radius,
                        ref strength,
                        ref bestInfluence);
                }
            }

            return bestInfluence > 0.001f;
        }

        private static void TryUseStrongerPocket(
            Vector2 worldPosition,
            Vector2 pocketCenter,
            float pocketRadius,
            float pocketStrength,
            ref Vector2 bestCenter,
            ref float bestRadius,
            ref float bestStrength,
            ref float bestInfluence)
        {
            if (pocketRadius <= 0.001f || Mathf.Abs(pocketStrength) <= 0.001f)
            {
                return;
            }

            var distance = Vector2.Distance(worldPosition, pocketCenter);

            if (distance > pocketRadius)
            {
                return;
            }

            var falloff = 1f - Mathf.Clamp01(distance / pocketRadius);
            var influence = Mathf.Abs(pocketStrength) * Mathf.Lerp(0.35f, 1f, falloff);

            if (influence <= bestInfluence)
            {
                return;
            }

            bestInfluence = influence;
            bestCenter = pocketCenter;
            bestRadius = pocketRadius;
            bestStrength = pocketStrength;
        }

        private void RefreshMagnetStormPickupTargets()
        {
            var activePickups = powerUpService?.ActivePickups;

            if (activePickups == null)
            {
                return;
            }

            for (var index = activePickups.Count - 1; index >= 0; index--)
            {
                var pickup = activePickups[index];

                if (pickup == null)
                {
                    activePickups.RemoveAt(index);
                    continue;
                }

                if (TryResolveStrongestPocket(pickup.transform.position, out var centerPoint, out var radius, out var strength))
                {
                    pickup.SetMagnetStormPocket(centerPoint, radius, strength);
                    continue;
                }

                pickup.SetMagnetStormPocket(Vector2.zero, 0f, 0f);
            }
        }

        private void ClearMagnetStormFromPickups()
        {
            var activePickups = powerUpService?.ActivePickups;

            if (activePickups == null)
            {
                return;
            }

            for (var index = activePickups.Count - 1; index >= 0; index--)
            {
                activePickups[index]?.SetMagnetStormPocket(Vector2.zero, 0f, 0f);
            }
        }

        private LevelDefinition ResolveLevelTemplate(int levelIndex)
        {
            if (loadedLevels.Count == 0 || levelIndex < 0)
            {
                return null;
            }

            return loadedLevels[levelIndex % loadedLevels.Count];
        }

        private void EvaluateLevelCompletion()
        {
            if (currentLevel == null)
            {
                return;
            }

            var levelCleared = brickService != null && !brickService.HasBreakableBricksRemaining();

            if (!levelCleared)
            {
                return;
            }

            SetSimulationPaused(false);
            ClearPickups();
            StopAllBalls();
            stickyCaughtBall = null;
            audioService?.PlayLevelComplete();

            if (isMenuAttractModeActive)
            {
                ScheduleMenuAttractRestart();
                return;
            }

            runStatsService?.RegisterLevelCleared();

            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                CompleteAndAdvanceTurnBasedTurn(BreakoutTurnSwitchReason.LevelCleared);
                return;
            }

            if (HasNextLevel() && TryOpenUpgradeDraft())
            {
                return;
            }

            roundState = RoundState.LevelComplete;
            selectedOverlayActionIndex = 0;
            RecordRogueRunResult(completed: !HasNextLevel());
            if (!HasNextLevel())
            {
                FinalizeActiveRunStats(completed: true);
            }
        }

        private bool HasNextLevel()
        {
            if (activeRunSettings != null && activeRunSettings.IsSoloMarathonMode)
            {
                return currentLevel != null && loadedLevels.Count > 0;
            }

            return BreakoutRunProgression.HasNextLevel(currentLevel, currentLevelIndex, loadedLevels.Count);
        }

        private void PlayMusicForCurrentLevel()
        {
            if (currentLevel == null)
            {
                audioService?.PlayMusic(BreakoutMusicTrack.Menu);
                return;
            }

            var isIntenseLevel = currentLevelIndex >= 2
                || currentLevel.BallSpeedMultiplier >= 1.2f
                || (activeRunSettings != null
                    && activeRunSettings.ScoringMode == RunScoringMode.HighScore
                    && currentLevelIndex > 0);
            audioService?.PlayMusic(isIntenseLevel ? BreakoutMusicTrack.Intense : BreakoutMusicTrack.Gameplay);
        }

        private static int CompareLevels(LevelDefinition left, LevelDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var sequenceComparison = left.SequenceIndex.CompareTo(right.SequenceIndex);

            if (sequenceComparison != 0)
            {
                return sequenceComparison;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }

        private static int CompareBrickDefinitions(BrickDefinition left, BrickDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var rankComparison = GetProceduralBrickRank(left).CompareTo(GetProceduralBrickRank(right));

            if (rankComparison != 0)
            {
                return rankComparison;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }

        private static int GetProceduralBrickRank(BrickDefinition definition)
        {
            if (definition == null)
            {
                return int.MaxValue;
            }

            if (!definition.IsBreakable)
            {
                return 4;
            }

            if (definition.IsExplosive)
            {
                return 3;
            }

            if (definition.SplitsOnBreak)
            {
                return 2;
            }

            return definition.HitPoints switch
            {
                <= 1 => 0,
                2 => 1,
                _ => 2,
            };
        }

        private static int CompareThemes(ThemeDefinition left, ThemeDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }

        private static int CompareRunUpgradeDefinitions(RunUpgradeDefinition left, RunUpgradeDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }

        private static int ComparePowerUpDefinitions(PowerUpDefinition left, PowerUpDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }

        private ThemeDefinition ResolvePendingThemeDefinition()
        {
            return ResolveThemeDefinition(pendingRunSetup?.ThemeId);
        }

        private ThemeDefinition ResolveThemeDefinition(string themeId)
        {
            if (loadedThemes.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(themeId))
            {
                for (var index = 0; index < loadedThemes.Count; index++)
                {
                    var candidate = loadedThemes[index];

                    if (candidate != null && string.Equals(candidate.ThemeId, themeId, StringComparison.OrdinalIgnoreCase))
                    {
                        return candidate;
                    }
                }
            }

            return loadedThemes[0];
        }

        private string ResolveThemeIdOrDefault(string themeId)
        {
            return ResolveThemeDefinition(themeId)?.ThemeId ?? string.Empty;
        }

        private string GetDefaultThemeId()
        {
            return loadedThemes.Count > 0 && loadedThemes[0] != null ? loadedThemes[0].ThemeId : string.Empty;
        }

        private string ShiftThemeId(string currentThemeId, int direction)
        {
            if (loadedThemes.Count == 0 || direction == 0)
            {
                return ResolveThemeIdOrDefault(currentThemeId);
            }

            var currentTheme = ResolveThemeDefinition(currentThemeId);
            var currentIndex = 0;

            for (var index = 0; index < loadedThemes.Count; index++)
            {
                if (loadedThemes[index] == currentTheme)
                {
                    currentIndex = index;
                    break;
                }
            }

            var nextIndex = Mathf.Clamp(currentIndex + direction, 0, loadedThemes.Count - 1);
            return loadedThemes[nextIndex] != null ? loadedThemes[nextIndex].ThemeId : ResolveThemeIdOrDefault(currentThemeId);
        }

        private void ApplyPendingThemePreview()
        {
            ApplyTheme(ResolvePendingThemeDefinition());
        }

        private void ApplyTheme(ThemeDefinition theme)
        {
            themeService?.ApplyTheme(
                theme,
                activeCamera,
                wallRenderers,
                paddleSpriteRenderer,
                serveBall,
                activeBalls,
                bricks,
                powerUpService?.ActivePickups);
            UpdateBackgroundVisuals();
            ApplyVisualEffectState();
            UpdateShieldWallVisual();
        }

        private BreakoutUiThemePalette BuildUiThemePalette()
        {
            var backgroundStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.Background, backgroundColor, backgroundColor, null)
                : new ThemeVisualStyle(backgroundColor, backgroundColor, null);
            var wallStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.Wall, wallColor, wallColor, squareSprite)
                : new ThemeVisualStyle(wallColor, wallColor, squareSprite);
            var paddleStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.Paddle, paddleColor, paddleColor, squareSprite)
                : new ThemeVisualStyle(paddleColor, paddleColor, squareSprite);
            var ballStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.Ball, ballColor, ballColor, circleSprite)
                : new ThemeVisualStyle(ballColor, ballColor, circleSprite);
            var brickPrimaryStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.BrickPrimary, new Color(1f, 0.49f, 0.86f, 1f), new Color(0.63f, 0.12f, 0.48f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(1f, 0.49f, 0.86f, 1f), new Color(0.63f, 0.12f, 0.48f, 1f), squareSprite);
            var brickTertiaryStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.BrickTertiary, new Color(0.01f, 0.93f, 0.98f, 1f), new Color(0.03f, 0.36f, 0.55f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(0.01f, 0.93f, 0.98f, 1f), new Color(0.03f, 0.36f, 0.55f, 1f), squareSprite);
            var beneficialStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBeneficial, new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite);
            var harmfulStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupHarmful, new Color(0.99f, 0.27f, 0.31f, 1f), new Color(0.99f, 0.27f, 0.31f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(0.99f, 0.27f, 0.31f, 1f), new Color(0.99f, 0.27f, 0.31f, 1f), squareSprite);
            var burstStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBurst, ballColor, ballColor, squareSprite)
                : new ThemeVisualStyle(ballColor, ballColor, squareSprite);
            var panelFill = Color.Lerp(backgroundStyle.PrimaryColor, Color.black, 0.35f);
            var panelFillSecondary = Color.Lerp(wallStyle.PrimaryColor, backgroundStyle.PrimaryColor, 0.45f);

            panelFill.a = 0.92f;
            panelFillSecondary.a = 0.94f;

            return new BreakoutUiThemePalette
            {
                BackgroundBase = backgroundStyle.PrimaryColor,
                BackgroundGlow = Color.Lerp(backgroundStyle.SecondaryColor, wallStyle.PrimaryColor, 0.55f),
                PanelFill = panelFill,
                PanelFillSecondary = panelFillSecondary,
                BezelDark = Color.Lerp(Color.black, backgroundStyle.PrimaryColor, 0.22f),
                AccentPrimary = Color.Lerp(paddleStyle.PrimaryColor, brickTertiaryStyle.PrimaryColor, 0.35f),
                AccentSecondary = Color.Lerp(brickPrimaryStyle.PrimaryColor, harmfulStyle.PrimaryColor, 0.24f),
                AccentWarm = Color.Lerp(ballStyle.PrimaryColor, burstStyle.PrimaryColor, 0.45f),
                Success = beneficialStyle.PrimaryColor,
                Danger = harmfulStyle.PrimaryColor,
                TextPrimary = new Color(0.99f, 0.99f, 1f, 1f),
                TextMuted = Color.Lerp(new Color(0.76f, 0.82f, 0.92f, 1f), wallStyle.PrimaryColor, 0.18f),
                ScreenTint = new Color(backgroundStyle.PrimaryColor.r, backgroundStyle.PrimaryColor.g, backgroundStyle.PrimaryColor.b, 0.18f),
                Scanline = new Color(1f, 1f, 1f, 0.028f),
            };
        }

        private BreakoutUiChromeView BuildChromeView(string marqueeTitle, string marqueeSubtitle, bool isMenuLike)
        {
            Rect playfieldRect;

            if (activeCamera != null)
            {
                var left = activeCamera.WorldToScreenPoint(new Vector3(arenaLeft, 0f, 0f)).x;
                var right = activeCamera.WorldToScreenPoint(new Vector3(arenaRight, 0f, 0f)).x;
                var top = Screen.height - activeCamera.WorldToScreenPoint(new Vector3(0f, arenaTop, 0f)).y;
                var bottom = Screen.height - activeCamera.WorldToScreenPoint(new Vector3(0f, arenaBottom, 0f)).y;
                playfieldRect = Rect.MinMaxRect(
                    Mathf.Min(left, right),
                    Mathf.Min(top, bottom),
                    Mathf.Max(left, right),
                    Mathf.Max(top, bottom));
            }
            else
            {
                playfieldRect = new Rect(Screen.width * 0.2f, Screen.height * 0.18f, Screen.width * 0.6f, Screen.height * 0.62f);
            }

            return new BreakoutUiChromeView
            {
                PlayfieldRect = playfieldRect,
                ShowPlayfieldFrame = true,
                IsMenuLike = isMenuLike,
                UseDynamicPlayfieldScanlines = !isMenuLike,
                DynamicScanlineSpacing = playfieldScanlineSpacing,
                DynamicScanlineTravelSpeed = playfieldScanlineTravelSpeed,
                DynamicScanlineAlpha = playfieldScanlineOverlayAlpha,
                DynamicSweepAlpha = playfieldScanlineSweepAlpha,
                MarqueeTitle = marqueeTitle,
                MarqueeSubtitle = marqueeSubtitle,
            };
        }

        private void UpdateBackgroundVisuals()
        {
            backgroundPresenter?.UpdateVisuals(themeService, ResolveBackgroundSpriteForCurrentLevel());
        }

        private Sprite ResolveBackgroundSpriteForCurrentLevel()
        {
            return backgroundLibrary != null
                ? backgroundLibrary.ResolveSpriteForLevel(currentLevelIndex)
                : null;
        }

        private void OnGUI()
        {
            if (uiRenderer == null)
            {
                return;
            }

            uiRenderer.ConfigureTheme(BuildUiThemePalette());

            if (roundState == RoundState.MainMenu)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Brickwave '84", "Synthwave Cabinet", true));
                uiRenderer.DrawMainMenu(BuildMainMenuView(), HandleMainMenuActionClick);
                return;
            }

            if (roundState == RoundState.Progression)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Unlock Ladder", "Cabinet Service", true));
                uiRenderer.DrawProgressionPage(BuildProgressionPageView(), AdjustProgressionSelector, EnterMainMenu);
                return;
            }

            if (roundState == RoundState.LifetimeStats)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Stats", "Cabinet Totals", true));
                uiRenderer.DrawOverlay(BuildLifetimeStatsOverlayView(), HandleOverlayActionClick);
                return;
            }

            if (roundState == RoundState.SoloMarathonSetup)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Neon Marathon", "Heat Select", true));
                uiRenderer.DrawOverlay(BuildSoloMarathonSetupView(), HandleSoloMarathonSetupActionClick);
                return;
            }

            if (roundState == RoundState.RunSetup)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Run Setup", ResolvePendingThemeDefinition()?.DisplayName ?? "Theme Preview", true));
                uiRenderer.DrawRunSetup(BuildRunSetupView());
                return;
            }

            if (roundState == RoundState.DeveloperMenu)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Dev Mode", ResolvePendingThemeDefinition()?.DisplayName ?? "Theme Preview", true));
                uiRenderer.DrawRunSetup(BuildDeveloperMenuView());
                return;
            }

            if (roundState == RoundState.UpgradeDraft)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Reward Draft", "Choose one run reward", true));
                uiRenderer.DrawUpgradeDraft(BuildUpgradeDraftView(), HandleUpgradeDraftChoice, TryPurchaseRewardMissile);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            var gameplayChromeView = BuildChromeView(string.Empty, string.Empty, false);
            uiRenderer.DrawCabinetBackdrop(gameplayChromeView);
            uiRenderer.DrawGameplayHud(BuildHudView(gameplayChromeView.PlayfieldRect), ToggleDiagnosticsOverlay, ToggleHudMenuOverlay);
            uiRenderer.DrawModifierIndicator(BuildModifierViews(), isDiagnosticsOverlayVisible, gameplayChromeView.PlayfieldRect);
            uiRenderer.DrawRunUpgradePanel(BuildRunUpgradePanelView(gameplayChromeView.PlayfieldRect));
            uiRenderer.DrawCapsuleMadness(BuildCapsuleMadnessView(gameplayChromeView.PlayfieldRect));
            uiRenderer.DrawAutoSaveBurst(BuildAutoSaveBurstView(gameplayChromeView.PlayfieldRect));
            uiRenderer.DrawAutoSaveBurst(BuildSpareFuseBurstView(gameplayChromeView.PlayfieldRect));
            uiRenderer.DrawFloatingScorePopups(scoreService?.BuildFloatingScoreViews(activeCamera, Screen.height) ?? Array.Empty<BreakoutUiFloatingScoreView>());

            if (isDiagnosticsOverlayVisible)
            {
                uiRenderer.DrawDiagnosticsOverlay(BuildDiagnosticsView(gameplayChromeView.PlayfieldRect));
            }

            if (roundState == RoundState.Playing)
            {
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            if (roundState == RoundState.Paused)
            {
                uiRenderer.DrawOverlay(BuildPauseOverlayView(), HandleOverlayActionClick);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            if (roundState == RoundState.LevelComplete || roundState == RoundState.GameOver)
            {
                uiRenderer.DrawOverlay(BuildEndStateOverlayView(), HandleOverlayActionClick);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            if (activeRunSettings != null
                && activeRunSettings.IsTurnBasedMode
                && (roundState == RoundState.ReadyToServe || roundState == RoundState.LifeLost))
            {
                uiRenderer.DrawOverlay(BuildTurnBasedServeOverlayView(), HandleOverlayActionClick);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            var message = roundState switch
            {
                RoundState.ReadyToServe => BuildReadyToServeMessage(),
                RoundState.LifeLost => BuildLifeLostMessage(),
                _ => string.Empty,
            };
            uiRenderer.DrawMessageOverlay(message);
            uiRenderer.DrawPickupBanner(BuildPickupBannerView());
        }

        private BreakoutUiOverlayView BuildTurnBasedServeOverlayView()
        {
            var isFreshServe = roundState == RoundState.ReadyToServe
                && turnBasedMultiplayerController != null
                && turnBasedMultiplayerController.LastSwitchReason == BreakoutTurnSwitchReason.None;
            var summaryLines = new List<string>
            {
                isFreshServe
                    ? turnBasedMultiplayerController?.BuildCurrentPlayerHudLabel() ?? "Player 01 is on deck."
                    : turnBasedMultiplayerController?.BuildSwitchSummaryLine() ?? "Next player is on deck.",
                BuildRunSummaryLabel(),
            };

            return new BreakoutUiOverlayView
            {
                Title = turnBasedMultiplayerController?.BuildSwitchTitle() ?? "Up Next",
                SummaryTitle = isFreshServe ? "First Serve" : "Next Serve",
                SummaryLines = summaryLines.ToArray(),
                LeaderboardTitle = "Scoreboard",
                LeaderboardEntries = BuildTurnBasedLeaderboardEntries(10),
                LeaderboardLines = BuildTurnBasedLeaderboardLines(10),
                FooterLines = new[]
                {
                    "Press Space to serve. Up/Down tunes speed.",
                },
                IsCompact = false,
            };
        }

        private static string GetOverlayActionLabel(OverlayAction action)
        {
            return action switch
            {
                OverlayAction.Resume => "Resume",
                OverlayAction.RestartRun => "Restart Run",
                OverlayAction.NextLevel => "Next Level",
                OverlayAction.ReturnToRunSetup => "Return To Setup",
                OverlayAction.ReturnToMainMenu => "Return To Main Menu",
                OverlayAction.QuitGame => "Quit Game",
                _ => action.ToString(),
            };
        }

        private BreakoutUiMenuView BuildMainMenuView()
        {
            var previewSettings = BuildRunSettingsFromPending(out var previewValidation);
            BreakoutRogueRunResultStore.TryLoad(out var lastRogueResult);
            var selectedPaddle = ResolveSelectedRoguePaddle();
            var marathonHeatBest = BreakoutSoloMarathonScoreStore.Load(selectedSoloMarathonDifficulty);
            var marathonOverallBest = BreakoutSoloMarathonScoreStore.LoadBestOverall();
            var context = new BreakoutMainMenuContext
            {
                SelectedActionIndex = selectedMainMenuActionIndex,
                PendingSeedDisplay = GetPendingSeedDisplay(),
                DifficultyPresetLabel = pendingRunSetup.DifficultyPreset.ToString(),
                BallsPerServe = pendingRunSetup.BallsPerServe,
                PreviewSettings = previewSettings,
                ScoreModeSummaryLabel = BuildScoreModeSummaryLabel(previewSettings),
                RetrySummaryLabel = BuildRetrySummaryLabel(previewSettings),
                DropSummaryLabel = BuildDropSummaryLabel(previewSettings),
                PreviewValidation = previewValidation,
                PendingValidationMessage = pendingValidationMessage,
                LastRogueResultSummary = BreakoutRogueRunResultStore.BuildSummary(lastRogueResult),
                AvailableRogueIntensity = BreakoutRogueIntensityProgressStore.GetAvailableIntensity(selectedPaddle.DisplayName),
                SoloMarathonDifficultyLabel = FormatSoloMarathonDifficultyLabel(selectedSoloMarathonDifficulty),
                SoloMarathonScoreMultiplierLabel = $"x{GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty):0.00}",
                SoloMarathonBestForHeatSummary = marathonHeatBest == null
                    ? "Selected Heat: no record yet."
                    : $"Selected Heat: {BreakoutSoloMarathonScoreStore.BuildSummary(marathonHeatBest)}",
                SoloMarathonBestOverallSummary = marathonOverallBest == null
                    ? "All-Time: no record yet."
                    : $"All-Time: {BreakoutSoloMarathonScoreStore.BuildSummary(marathonOverallBest)}",
            };

            return mainMenuService.BuildView(context);
        }

        private BreakoutUiProgressionView BuildProgressionPageView()
        {
            progressionPageService ??= new BreakoutProgressionPageService();
            return progressionPageService.BuildView(
                loadedPowerUpDefinitions,
                themeService,
                selectedProgressionSortMode,
                selectedProgressionRarityFilter,
                selectedProgressionTypeFilter,
                selectedProgressionLockFilter);
        }

        private BreakoutUiRunSetupView BuildRunSetupView()
        {
            var previewSettings = BuildRunSettingsFromPending(out var previewValidation);

            if (isTurnBasedSetupActive)
            {
                var hotSeatFields = GetHotSeatSetupFields();
                var hotSeatLimitLine = turnBasedMultiplayerController != null
                    && turnBasedMultiplayerController.SelectedMode == BreakoutHotSeatMode.Outlast
                        ? $"Lives Each: {turnBasedMultiplayerController.SelectedOutlastLives:00}"
                        : $"Turns Each: {turnBasedMultiplayerController?.SelectedTopScoreTurnLimit ?? BreakoutTurnBasedMultiplayerController.DefaultTopScoreTurnLimit:00}";
                return new BreakoutUiRunSetupView
                {
                    Title = "Hot Seat Setup",
                    Subtitle = "Pick the cabinet crew, win condition, and heat. The tape seed shuffles fresh when the match starts.",
                    FieldLines = new[]
                    {
                        $"Players: {turnBasedMultiplayerController?.SelectedPlayerCount ?? BreakoutTurnBasedMultiplayerController.MinimumPlayerCount:00}",
                        $"Mode: {turnBasedMultiplayerController?.GetSelectedModeLabel() ?? "Top Score"}",
                        hotSeatLimitLine,
                        $"Heat Level: {turnBasedMultiplayerController?.GetSelectedDifficultyLabel() ?? "Gnarly"}",
                    },
                    SelectedFieldIndex = Mathf.Max(0, Array.IndexOf(hotSeatFields, selectedRunSetupField)),
                    PreviewLine = $"Preview: {turnBasedMultiplayerController?.BuildSetupPreviewLine() ?? "02 players"} | Drops {BuildDropSummaryLabel(previewSettings)} | {previewSettings.LevelGlitchLabel} | Ball x{previewSettings.BallSpeedMultiplier:0.00} | Random Tape ID",
                    ValidationText = previewValidation,
                    HintText = "Up/Down selects. Left/Right adjusts. N resets. Esc backs out. Space launches.",
                };
            }

            var fieldLines = new List<string>
            {
                $"Tape ID: {GetPendingSeedDisplay()}",
                $"Difficulty: {pendingRunSetup.DifficultyPreset}",
                $"Score Mode: {previewSettings.ScoringModeLabel}",
                $"Balls Per Serve: {pendingRunSetup.BallsPerServe}",
                $"Paddle Width Bias: {FormatSignedStep(pendingRunSetup.PaddleWidthStep)}",
                $"Ball Speed Bias: {FormatSignedStep(pendingRunSetup.BallSpeedStep)}",
                $"Brick Durability Bias: {FormatSignedStep(pendingRunSetup.BrickDurabilityStep)}",
                $"Drop Pool: {previewSettings.DropPoolLabel}",
                $"Capsule Party: {(previewSettings.ForcePickupDropsOnBreak ? "On" : "Off")}",
                $"Glitches: {FormatLevelGlitchSelectionLabel(pendingRunSetup.SelectedLevelGlitch)}",
                $"Theme: {previewSettings.ThemeLabel}",
            };

            return new BreakoutUiRunSetupView
            {
                Title = "Run Setup",
                Subtitle = "Dial in the cabinet before launch. The same Tape ID preserves the run while score mode, modifiers, and palette reshape the pressure curve.",
                FieldLines = fieldLines.ToArray(),
                SelectedFieldIndex = (int)selectedRunSetupField,
                PreviewLine = $"Preview: {BuildScoreModeSummaryLabel(previewSettings)} | {BuildRetrySummaryLabel(previewSettings)} | Paddle x{previewSettings.PaddleWidthMultiplier:0.00} | Ball speed x{previewSettings.BallSpeedMultiplier:0.00} | Brick durability x{previewSettings.BrickDurabilityMultiplier:0.00} | Drops {BuildDropSummaryLabel(previewSettings)} | {previewSettings.LevelGlitchLabel}",
                ValidationText = previewValidation,
                HintText = "Up/Down selects. Left/Right adjusts. Digits edit Tape ID. Backspace deletes. T shuffles. N resets. Space launches.",
            };
        }

        private BreakoutUiRunSetupView BuildDeveloperMenuView()
        {
            developerLaunchState ??= new BreakoutDeveloperLaunchState();
            var encounter = developerLaunchState.ResolveEncounter();
            var selectedPaddle = developerLaunchState.ResolvePaddle();
            var currentUpgrade = developerLaunchState.ResolveCurrentUpgrade(loadedRunUpgradeDefinitions);
            var currentDrop = developerLaunchState.ResolveCurrentDropUnlock(loadedPowerUpDefinitions);
            var forcedDrop = developerLaunchState.ResolveForcedDrop(loadedPowerUpDefinitions);
            var upgradeSelected = currentUpgrade != null && developerLaunchState.IsUpgradeSelected(currentUpgrade);
            var dropSelected = currentDrop != null && developerLaunchState.IsDropUnlockSelected(currentDrop);
            var forcedDropEnabled = developerLaunchState.ForcedDropEnabled && forcedDrop != null;
            var forcedGlitch = developerLaunchState.ForcedLevelGlitchSelection;
            var forcedGlitchEnabled = developerLaunchState.ForcedLevelGlitchEnabled;

            return new BreakoutUiRunSetupView
            {
                Title = "Dev Mode",
                Subtitle = "Local Rogue jump bench. Pick an encounter, seed a build, and launch straight into the test target.",
                FieldLines = new[]
                {
                    $"Encounter: {FormatDeveloperEncounterLabel(encounter)}",
                    $"Lives: {developerLaunchState.LivesRemaining}",
                    $"Heat: {developerLaunchState.Intensity:00}/{BreakoutRunProgression.MaxRogueIntensity:00}",
                    $"Upgrade: {FormatDeveloperToggle(upgradeSelected)} {FormatDeveloperUpgradeLabel(currentUpgrade)}",
                    $"Drop Unlock: {FormatDeveloperToggle(dropSelected)} {FormatDeveloperDropLabel(currentDrop)}",
                    $"Forced Drop: {FormatDeveloperToggle(forcedDropEnabled)} {FormatDeveloperDropLabel(forcedDrop)}",
                    $"Forced Glitch: {FormatDeveloperForcedGlitchLabel(forcedGlitchEnabled, forcedGlitch)}",
                },
                SelectedFieldIndex = (int)selectedDeveloperLaunchField,
                PreviewLine = $"Preview: {FormatDeveloperEncounterLabel(encounter)} | Heat {developerLaunchState.Intensity:00} | Balls {developerLaunchState.LivesRemaining:00} | Paddle x{selectedPaddle.WidthMultiplier:0.00} speed x{selectedPaddle.SpeedMultiplier:0.00} | Build {developerLaunchState.SelectedUpgradeCount:00} upgrades, {developerLaunchState.SelectedDropUnlockCount:00} drops | Force {FormatDeveloperForcedDropPreview(forcedDropEnabled, forcedDrop)} | Glitch {FormatDeveloperForcedGlitchPreview(forcedGlitchEnabled, forcedGlitch)} | Theme {ResolvePendingThemeDefinition()?.DisplayName ?? "Fallback"}",
                ValidationText = "Encounter cycles through Stage 01-10. Dev runs do not update the saved Neon Ladder result.",
                HintText = "Up/Down selects. Left/Right changes. T toggles. N clears. Esc backs out. Space launches.",
            };
        }

        private BreakoutUiOverlayView BuildSoloMarathonSetupView()
        {
            var heatLabel = FormatSoloMarathonDifficultyLabel(selectedSoloMarathonDifficulty);
            var selectedBest = BreakoutSoloMarathonScoreStore.Load(selectedSoloMarathonDifficulty);
            var bestLine = selectedBest == null
                ? $"Top Score: no {heatLabel} record yet."
                : $"Top Score: {FormatScoreValue(selectedBest.Score)} | Stage {selectedBest.StageReached:00} | Tape ID {selectedBest.Seed}";

            return new BreakoutUiOverlayView
            {
                Title = "Neon Marathon",
                SummaryTitle = "Heat Select",
                SummaryLines = new[]
                {
                    $"Selected Heat: {heatLabel} | Score x{GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty):0.00}",
                    bestLine,
                    BuildSoloMarathonHeatRulesLine(selectedSoloMarathonDifficulty),
                    "Five balls. Random Tape ID. High Score mode.",
                },
                LeaderboardTitle = "Top Scores By Heat",
                LeaderboardEntries = BuildSoloMarathonLeaderboardEntries(),
                ActionLabels = BuildSoloMarathonSetupActionLabels(),
                SelectedActionIndex = Mathf.Clamp(selectedSoloMarathonSetupActionIndex, 0, SoloMarathonBackActionIndex),
                FooterLines = new[]
                {
                    "Up/Down selects. Left/Right changes heat. Space confirms. Esc backs out.",
                },
                IsCompact = false,
            };
        }

        private BreakoutUiOverlayView BuildLifetimeStatsOverlayView()
        {
            var lifetimeStats = runStatsService?.LifetimeStats ?? new BreakoutRunStatsSnapshot();
            return new BreakoutUiOverlayView
            {
                Title = "Stats",
                SummaryTitle = "Cabinet Total",
                StatsRows = BuildLifetimeStatsRows(lifetimeStats),
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(RoundState.LifetimeStats)),
                SelectedActionIndex = selectedOverlayActionIndex,
                FooterLines = new[]
                {
                    "Up/Down scrolls. Esc backs out.",
                },
                IsCompact = false,
                EmphasizeSummary = true,
            };
        }

        private static BreakoutUiStatsRowView[] BuildLifetimeStatsRows(BreakoutRunStatsSnapshot stats)
        {
            stats ??= new BreakoutRunStatsSnapshot();
            return new[]
            {
                BuildStatsRow("Runs Recorded", stats.RunsRecorded.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Balls Launched", stats.BallsLaunched.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Lives Lost", stats.LivesLost.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Levels Cleared", stats.LevelsCleared.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Walls Hit", stats.WallsHit.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Paddle Hits", stats.PaddleHits.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Paddle Distance Traveled", BreakoutRunStatsService.FormatDistance(stats.PaddleDistanceTraveled)),
                BuildStatsRow("Balls Distance Traveled", BreakoutRunStatsService.FormatDistance(stats.BallsDistanceTraveled)),
                BuildStatsRow("Bricks Hit", stats.BricksHit.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Bricks Destroyed", stats.BricksDestroyed.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Total Drops Picked Up", stats.TotalDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Total Drops Dropped", stats.TotalDropsDropped.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Helpful Drops Picked Up", stats.HelpfulDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Helpful Drops Dropped", stats.HelpfulDropsDropped.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Harmful Drops Picked Up", stats.HarmfulDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Harmful Drops Dropped", stats.HarmfulDropsDropped.ToString("00", CultureInfo.InvariantCulture)),
                BuildStatsRow("Time Played", BreakoutRunStatsService.FormatDuration(stats.TimePlayedSeconds)),
                BuildStatsRow("Glitches Encountered", stats.GlitchesEncountered.ToString("00", CultureInfo.InvariantCulture)),
            };
        }

        private static BreakoutUiStatsRowView BuildStatsRow(string label, string value)
        {
            return new BreakoutUiStatsRowView
            {
                Label = label,
                Value = value,
            };
        }

        private BreakoutUiHudView BuildHudView(Rect playfieldRect)
        {
            var bounceZoneLeftScreen = activeCamera != null
                ? activeCamera.WorldToScreenPoint(new Vector3(arenaLeft, 0f, 0f)).x
                : 0f;
            var speed = GetDisplayedBallSpeed();
            var speedBase = GetBallSpeedBase();
            var maximumSpeed = GetMaximumBallSpeed();
            var isRogueRun = activeRunSettings != null && activeRunSettings.IsRogueMode;
            var intensity = isRogueRun
                ? BreakoutRunProgression.ClampRogueIntensity(activeRunSettings.RogueIntensity)
                : BreakoutRunProgression.MinRogueIntensity;
            var intensityProgress = BreakoutRunProgression.GetRogueIntensityProgress(intensity);
            var playerHudLabel = BuildTurnBasedPlayerHudPrefix();
            var scoreText = FormatScoreValue(score);

            if (!string.IsNullOrWhiteSpace(playerHudLabel))
            {
                scoreText = $"{playerHudLabel.ToUpperInvariant()}   {scoreText}";
            }

            var ballStyle = themeService != null
                ? themeService.ResolveBallStyle()
                : new ThemeVisualStyle(ballColor, ballColor, ballSprite);
            var hasPaddleScreenTarget = TryGetPaddleHudTarget(out var paddleScreenTarget);

            return new BreakoutUiHudView
            {
                PlayfieldRect = playfieldRect,
                TopLine = scoreText,
                ScoreText = scoreText,
                ScoreValue = score,
                IsScoreLeakActive = IsScoreLeakDraining(),
                ScoreLeakIntensity = GetScoreLeakVisualIntensity(),
                LifeCount = Mathf.Max(0, GetLifeCounterValue()),
                LifeIcon = ballStyle.Sprite != null ? ballStyle.Sprite : ballSprite,
                LifeIconColor = ballStyle.PrimaryColor,
                MissileCount = Mathf.Max(0, availableMissiles),
                MissileIcon = missileSprite != null ? missileSprite : triangleSprite,
                MissileIconColor = ResolveMissileColor(),
                HasPaddleScreenTarget = hasPaddleScreenTarget,
                PaddleScreenTarget = paddleScreenTarget,
                BottomLine = BuildGameplayStatusLine().ToUpperInvariant(),
                HasBrickCounter = currentLevel != null,
                BricksRemaining = Mathf.Max(0, requiredBricksRemaining),
                ShowMenuButton = CanPauseRoundState(roundState) || roundState == RoundState.Paused,
                IsPaused = roundState == RoundState.Paused,
                IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
                SpeedMeter = new BreakoutUiSpeedMeterView
                {
                    BounceZoneLeftScreen = bounceZoneLeftScreen,
                    IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
                    Speed = speed,
                    SpeedRatio = Mathf.Clamp01(Mathf.InverseLerp(speedBase, maximumSpeed, speed)),
                },
                IntensityGauge = new BreakoutUiIntensityGaugeView
                {
                    IsVisible = isRogueRun,
                    Intensity = intensity,
                    MaxIntensity = BreakoutRunProgression.MaxRogueIntensity,
                    Progress = intensityProgress,
                    PulseRate = Mathf.Lerp(1.6f, 8.6f, intensityProgress),
                    Color = BreakoutRunProgression.GetRogueIntensityGaugeColor(intensity),
                },
                StageLadder = new BreakoutUiStageLadderView
                {
                    IsVisible = isRogueRun,
                    CurrentStage = Mathf.Clamp(currentLevelIndex + 1, 1, BreakoutRunProgression.TargetLevelCount),
                    TotalStages = BreakoutRunProgression.TargetLevelCount,
                    CompletedStages = roundState == RoundState.LevelComplete && !HasNextLevel()
                        ? BreakoutRunProgression.TargetLevelCount
                        : Mathf.Clamp(currentLevelIndex, 0, BreakoutRunProgression.TargetLevelCount),
                },
            };
        }

        private bool TryGetPaddleHudTarget(out Vector2 screenTarget)
        {
            screenTarget = Vector2.zero;

            if (activeCamera == null || paddle == null)
            {
                return false;
            }

            var worldTarget = paddleCollider != null
                ? new Vector3(paddleCollider.bounds.center.x, paddleCollider.bounds.max.y, 0f)
                : paddle.transform.position;
            var screenPoint = activeCamera.WorldToScreenPoint(worldTarget);

            if (screenPoint.z < 0f)
            {
                return false;
            }

            screenTarget = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            return true;
        }

        private BreakoutUiOverlayView BuildPauseOverlayView()
        {
            return new BreakoutUiOverlayView
            {
                Title = "Menu",
                SummaryLines = new[]
                {
                    BuildPauseSummaryLabel(),
                    BuildUpgradeSummaryLabel(3),
                    currentLevelVariationLabel,
                },
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(RoundState.Paused)),
                SelectedActionIndex = selectedOverlayActionIndex,
                FooterLines = new[]
                {
                    "A/D or Left/Right moves. Space launches/fires. M missiles. T nudges. Up/Down tunes speed.",
                    "Up/Down selects. Space confirms. Esc/P resumes. R setup.",
                },
                IsCompact = true,
            };
        }

        private BreakoutUiUpgradeDraftView BuildUpgradeDraftView()
        {
            var offers = activeRunState?.PendingDraftOffers;

            if (offers == null || offers.Count == 0)
            {
                return new BreakoutUiUpgradeDraftView
                {
                    Title = "Reward Draft",
                    Subtitle = "No rewards available.",
                    BuildLine = BuildUpgradeSummaryLabel(3),
                    HintText = "Press R to return to setup.",
                };
            }

            var optionViews = new BreakoutUiUpgradeDraftOptionView[offers.Count];

            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                var upgrade = offer?.UpgradeDefinition;
                var dropUnlock = offer?.DropUnlockDefinition;
                var currentStacks = activeRunState != null ? activeRunState.GetStackCount(upgrade) : 0;
                optionViews[index] = new BreakoutUiUpgradeDraftOptionView
                {
                    Title = BuildDraftOfferTitle(offer),
                    Description = BuildDraftOfferDescription(offer),
                    Detail = offer != null && offer.Kind == BreakoutRunDraftOfferKind.RunUpgrade && upgrade != null
                        ? $"Run Upgrade | Stacks {currentStacks}/{upgrade.MaxStacks} | {BuildUpgradeMechanicalSummary(upgrade)}"
                        : BuildDropUnlockDetail(dropUnlock),
                    Icon = ResolveDraftOfferIcon(offer),
                    IconLabel = ResolveDraftOfferIconLabel(offer),
                    Accent = ResolveDraftOfferAccentColor(offer),
                };
            }

            return new BreakoutUiUpgradeDraftView
            {
                Title = "Reward Draft",
                Subtitle = $"Level {currentLevelIndex + 1:00} cleared. Choose one reward for the rest of this run.",
                BuildLine = BuildUpgradeSummaryLabel(4),
                Options = optionViews,
                SelectedOptionIndex = Mathf.Clamp(selectedUpgradeDraftIndex, 0, optionViews.Length - 1),
                MissilePurchaseLabel = "BUY MISSILE",
                MissilePurchaseDetail = $"{FormatScoreValue(MissileRewardPurchaseScoreCost)} pts | +{GetRewardMissilePurchaseStockGrant()} stock | Stock {Mathf.Max(0, availableMissiles):00}",
                CanPurchaseMissile = CanPurchaseMissile(score),
                MissilePurchaseIcon = missileSprite != null ? missileSprite : triangleSprite,
                MissilePurchaseColor = ResolveMissileColor(),
                HintText = "Left/Right selects. Space confirms. Click chooses. M buys missile. R returns to setup.",
            };
        }

        private BreakoutUiOverlayView BuildEndStateOverlayView()
        {
            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                var isTurnGameOver = roundState == RoundState.GameOver;
                var winner = turnBasedMultiplayerController?.Winner;
                var summaryLines = new List<string>
                {
                    isTurnGameOver
                        ? winner != null
                            ? $"Winner: {winner.DisplayName} | Score {FormatScoreValue(winner.Score)} | Stage {winner.CurrentLevelIndex + 1:00}"
                            : $"Final score {FormatScoreValue(score)} | Reached {BuildLevelLabel()}"
                        : turnBasedMultiplayerController?.BuildSwitchSummaryLine() ?? "Stage clear. Next player is on deck.",
                    BuildRunSummaryLabel(),
                };

                if (isTurnGameOver)
                {
                    summaryLines.AddRange(BreakoutRunStatsService.BuildRunStatsLines(runStatsService?.CurrentRunStats, includeRunCount: false));
                }

                return new BreakoutUiOverlayView
                {
                    Title = isTurnGameOver
                        ? "Hot Seat Over"
                        : turnBasedMultiplayerController?.BuildSwitchTitle() ?? "Up Next",
                    SummaryTitle = isTurnGameOver ? "Final Round" : "Round Info",
                    SummaryLines = summaryLines.ToArray(),
                    LeaderboardTitle = "Scoreboard",
                    LeaderboardEntries = BuildTurnBasedLeaderboardEntries(10),
                    LeaderboardLines = BuildTurnBasedLeaderboardLines(10),
                    ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(roundState)),
                    SelectedActionIndex = selectedOverlayActionIndex,
                    FooterLines = new[]
                    {
                        isTurnGameOver
                            ? "Restart the hot seat tape, jump back to setup, or return to the main menu."
                            : "Leaderboard locked for the switch. Advance when the next player is ready.",
                    },
                    IsCompact = false,
                };
            }

            var isGameOver = roundState == RoundState.GameOver;
            var title = isGameOver
                ? activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                    ? activeSoloMarathonNewHighScore ? "Hall of Rad!" : "Neon Marathon Over"
                    : activeRunSettings != null && activeRunSettings.IsRogueMode ? "Ladder Wiped Out" : "Run Over"
                : HasNextLevel()
                    ? "Level Cleared"
                    : (activeRunSettings != null && activeRunSettings.IsRogueMode ? "Ladder Cleared" : "Final Level Cleared");
            var summary = isGameOver
                ? activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                    ? new[]
                    {
                        $"{GetScoreDisplayLabel()} {FormatScoreValue(score)}",
                        $"Best {FormatScoreValue(activeSoloMarathonRecord?.Score ?? score)}",
                        $"Reached {BuildLevelLabel()}",
                        $"Tape ID {activeRunSettings.Seed.ToString(CultureInfo.InvariantCulture)}",
                    }
                    : new[]
                    {
                        $"{GetScoreDisplayLabel()} {FormatScoreValue(score)}",
                        $"Reached {BuildLevelLabel()}",
                        $"Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}",
                    }
                : HasNextLevel()
                    ? new[]
                    {
                        $"{GetScoreDisplayLabel()} {FormatScoreValue(score)}",
                        $"{GetLifeCounterLabel()} {GetLifeCounterValue():00}",
                        $"Next Stage {currentLevelIndex + 2:00}",
                    }
                    : new[]
                    {
                        $"{GetScoreDisplayLabel()} {FormatScoreValue(score)}",
                        $"{GetLifeCounterLabel()} {GetLifeCounterValue():00}",
                        $"Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}",
                    };
            var endSummaryLines = new List<string>(summary);
            var useRunStatsTable = activeRunSettings != null
                && activeRunSettings.IsRogueMode
                && (isGameOver || !HasNextLevel());

            if (!useRunStatsTable && (isGameOver || !HasNextLevel()))
            {
                endSummaryLines.AddRange(BreakoutRunStatsService.BuildRunStatsLines(runStatsService?.CurrentRunStats, includeRunCount: false));
            }

            var footer = isGameOver
                ? activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                    ? (activeSoloMarathonNewHighScore
                        ? "New Neon Marathon best saved. Restart the chase or return to the main menu."
                        : "Restart the chase or return to the main menu.")
                    : activeRunSettings != null && activeRunSettings.IsRogueMode
                    ? "Restart the climb or return to the main menu."
                    : "Restart the run, jump back to setup, or return to the main menu."
                : HasNextLevel()
                    ? "Advance to the next stage, restart the run, or return to the menu."
                    : activeRunSettings != null && activeRunSettings.IsRogueMode
                        ? "The 10-stage ladder is complete. Restart or head back to the menu."
                        : "The 10-stage run is complete. Restart, tune a new setup, or head back to the menu.";
            return new BreakoutUiOverlayView
            {
                Title = title,
                SummaryLines = useRunStatsTable ? Array.Empty<string>() : endSummaryLines.ToArray(),
                StatsRows = useRunStatsTable
                    ? BuildRunEndStatsRows(summary, runStatsService?.CurrentRunStats)
                    : Array.Empty<BreakoutUiStatsRowView>(),
                LeaderboardTitle = isGameOver && activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                    ? "Top Scores By Heat"
                    : string.Empty,
                LeaderboardEntries = isGameOver && activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                    ? BuildSoloMarathonLeaderboardEntries()
                    : Array.Empty<BreakoutTurnLeaderboardEntry>(),
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(roundState)),
                SelectedActionIndex = selectedOverlayActionIndex,
                FooterLines = new[] { footer },
                IsCompact = false,
                EmphasizeSummary = true,
            };
        }

        private static BreakoutUiStatsRowView[] BuildRunEndStatsRows(string[] summaryLines, BreakoutRunStatsSnapshot stats)
        {
            stats ??= new BreakoutRunStatsSnapshot();
            var rows = new List<BreakoutUiStatsRowView>();

            if (summaryLines != null)
            {
                for (var index = 0; index < summaryLines.Length; index++)
                {
                    if (TrySplitResultSummaryLine(summaryLines[index], out var label, out var value))
                    {
                        rows.Add(BuildStatsRow(label, value));
                    }
                }
            }

            rows.Add(BuildStatsRow("Time Played", BreakoutRunStatsService.FormatDuration(stats.TimePlayedSeconds)));
            rows.Add(BuildStatsRow("Balls Launched", stats.BallsLaunched.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Lives Lost", stats.LivesLost.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Levels Cleared", stats.LevelsCleared.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Walls Hit", stats.WallsHit.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Paddle Hits", stats.PaddleHits.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Paddle Distance Traveled", BreakoutRunStatsService.FormatDistance(stats.PaddleDistanceTraveled)));
            rows.Add(BuildStatsRow("Balls Distance Traveled", BreakoutRunStatsService.FormatDistance(stats.BallsDistanceTraveled)));
            rows.Add(BuildStatsRow("Bricks Hit", stats.BricksHit.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Bricks Destroyed", stats.BricksDestroyed.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Total Drops Picked Up", stats.TotalDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Total Drops Dropped", stats.TotalDropsDropped.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Helpful Drops Picked Up", stats.HelpfulDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Helpful Drops Dropped", stats.HelpfulDropsDropped.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Harmful Drops Picked Up", stats.HarmfulDropsPickedUp.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Harmful Drops Dropped", stats.HarmfulDropsDropped.ToString("00", CultureInfo.InvariantCulture)));
            rows.Add(BuildStatsRow("Glitches Encountered", stats.GlitchesEncountered.ToString("00", CultureInfo.InvariantCulture)));
            return rows.ToArray();
        }

        private static bool TrySplitResultSummaryLine(string line, out string label, out string value)
        {
            label = string.Empty;
            value = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var trimmed = line.Trim();

            if (trimmed.StartsWith("Tape ID ", StringComparison.OrdinalIgnoreCase))
            {
                label = "Tape ID";
                value = trimmed.Substring("Tape ID ".Length).Trim();
                return !string.IsNullOrWhiteSpace(value);
            }

            if (trimmed.StartsWith("Reached ", StringComparison.OrdinalIgnoreCase))
            {
                label = "Reached";
                value = trimmed.Substring("Reached ".Length).Trim();
                return !string.IsNullOrWhiteSpace(value);
            }

            var splitIndex = trimmed.LastIndexOf(' ');

            if (splitIndex <= 0 || splitIndex >= trimmed.Length - 1)
            {
                label = "Result";
                value = trimmed;
                return true;
            }

            label = trimmed.Substring(0, splitIndex).Trim();
            value = trimmed.Substring(splitIndex + 1).Trim();
            return !string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(value);
        }

        private BreakoutUiModifierView[] BuildModifierViews()
        {
            if (powerUpService == null || powerUpService.ActiveTimedEffects.Count == 0)
            {
                return Array.Empty<BreakoutUiModifierView>();
            }

            var summaries = powerUpService.BuildTimedEffectStackSummaries();

            if (summaries.Count == 0)
            {
                return Array.Empty<BreakoutUiModifierView>();
            }

            var views = new List<BreakoutUiModifierView>();

            for (var index = 0; index < summaries.Count; index++)
            {
                var summary = summaries[index];

                var effectStyle = themeService != null
                    ? themeService.ResolvePowerUpStyle(summary.Definition)
                    : new ThemeVisualStyle(Color.white, Color.white, squareSprite);
                views.Add(new BreakoutUiModifierView
                {
                    Label = summary.DisplayMultiplier > 1.001f
                        ? $"{summary.Definition.HudLabel} x{BreakoutPowerUpService.FormatMultiplier(summary.DisplayMultiplier)}"
                        : summary.Definition.HudLabel,
                    RemainingDuration = summary.RemainingDuration,
                    DurationRatio = summary.DurationRatio,
                    Color = effectStyle.PrimaryColor,
                });
            }

            return views.ToArray();
        }

        private BreakoutUiRunUpgradePanelView BuildRunUpgradePanelView(Rect playfieldRect)
        {
            var chosenUpgrades = activeRunState?.ChosenUpgrades;

            if (activeRunState == null || !activeRunState.HasActiveBuild)
            {
                return new BreakoutUiRunUpgradePanelView
                {
                    PlayfieldRect = playfieldRect,
                    IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
                };
            }

            var items = new List<BreakoutUiRunUpgradePanelItemView>();
            var addedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var upgrade = chosenUpgrades[index];

                if (upgrade == null || !addedIds.Add(upgrade.UpgradeId))
                {
                    continue;
                }

                var stackCount = activeRunState.GetStackCount(upgrade);
                items.Add(new BreakoutUiRunUpgradePanelItemView
                {
                    Label = upgrade.HudLabel,
                    Title = upgrade.DisplayName,
                    Description = upgrade.Description,
                    Detail = $"{BuildUpgradeMechanicalSummary(upgrade)} | Stacks {stackCount}/{upgrade.MaxStacks}",
                    StackCount = stackCount,
                    Icon = ResolveRunUpgradeIcon(upgrade),
                    Accent = ResolveRunUpgradeAccentColor(upgrade),
                });
            }

            var chosenDropUnlocks = activeRunState.ChosenDropUnlocks;

            for (var index = 0; index < chosenDropUnlocks.Count; index++)
            {
                var dropUnlock = chosenDropUnlocks[index];

                if (dropUnlock == null || !addedIds.Add($"drop:{BreakoutPowerUpIdentity.GetStableId(dropUnlock)}"))
                {
                    continue;
                }

                items.Add(new BreakoutUiRunUpgradePanelItemView
                {
                    Label = dropUnlock.HudLabel,
                    Title = $"Drop Unlock: {dropUnlock.DisplayName}",
                    Description = $"Adds {dropUnlock.DisplayName} capsules to this Neon Ladder run's drop pool.",
                    Detail = BuildDropUnlockDetail(dropUnlock),
                    StackCount = 1,
                    Icon = ResolvePowerUpIcon(dropUnlock),
                    Accent = ResolvePowerUpAccentColor(dropUnlock),
                });
            }

            return new BreakoutUiRunUpgradePanelView
            {
                Items = items.ToArray(),
                PlayfieldRect = playfieldRect,
                IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
            };
        }

        private BreakoutUiDiagnosticsView BuildDiagnosticsView(Rect playfieldRect)
        {
            var lines = new List<string>
            {
                $"{currentLevelVariationLabel}   |   State: {BuildRoundStateLabel()}",
                BuildBallSpeedControlLabel(),
                $"{BuildActiveEffectsLabel()}   |   {BuildRunSummaryLabel()}",
                BuildUpgradeSummaryLabel(6),
            };

            if (!string.IsNullOrWhiteSpace(pendingValidationMessage))
            {
                lines.Add(pendingValidationMessage);
            }

            return new BreakoutUiDiagnosticsView
            {
                Lines = lines.ToArray(),
                PlayfieldRect = playfieldRect,
            };
        }

        private BreakoutUiBannerView BuildPickupBannerView()
        {
            return new BreakoutUiBannerView
            {
                Text = powerUpService?.PickupBannerText ?? string.Empty,
                Timer = powerUpService?.PickupBannerTimer ?? 0f,
                Color = powerUpService?.PickupBannerColor ?? Color.white,
            };
        }

        private BreakoutUiCapsuleMadnessView BuildCapsuleMadnessView(Rect playfieldRect)
        {
            return new BreakoutUiCapsuleMadnessView
            {
                Text = "Capsule Madness!!",
                PlayfieldRect = playfieldRect,
                Timer = powerUpService?.CapsuleMadnessTimer ?? 0f,
                Duration = BreakoutPowerUpService.CapsuleMadnessDurationSeconds,
                Color = ResolveCapsuleMadnessColor(),
            };
        }

        private BreakoutUiCapsuleMadnessView BuildAutoSaveBurstView(Rect playfieldRect)
        {
            return new BreakoutUiCapsuleMadnessView
            {
                Text = "Auto Save!!",
                PlayfieldRect = playfieldRect,
                Timer = autoSaveBurstTimer,
                Duration = AutoSaveBurstDurationSeconds,
                Color = ResolveAutoSaveColor(),
            };
        }

        private BreakoutUiCapsuleMadnessView BuildSpareFuseBurstView(Rect playfieldRect)
        {
            return new BreakoutUiCapsuleMadnessView
            {
                Text = "Spare Fuse!!",
                PlayfieldRect = playfieldRect,
                Timer = spareFuseBurstTimer,
                Duration = SpareFuseBurstDurationSeconds,
                Color = ResolveSpareFuseColor(),
            };
        }

        private string[] BuildOverlayActionLabels(OverlayAction[] actions)
        {
            var labels = new string[actions.Length];

            for (var index = 0; index < actions.Length; index++)
            {
                labels[index] = GetOverlayActionLabel(actions[index]);
            }

            return labels;
        }

        private void HandleOverlayActionClick(int actionIndex)
        {
            var actions = GetOverlayActionsForState(roundState);

            if (actionIndex < 0 || actionIndex >= actions.Length)
            {
                return;
            }

            selectedOverlayActionIndex = actionIndex;
            PerformOverlayAction(actions[actionIndex]);
        }

        private void HandleMainMenuActionClick(int actionIndex)
        {
            var actions = mainMenuService?.BuildActions() ?? Array.Empty<BreakoutMainMenuAction>();

            if (actionIndex < 0 || actionIndex >= actions.Length)
            {
                return;
            }

            selectedMainMenuActionIndex = actionIndex;
            PerformMainMenuAction(actions[actionIndex]);
        }

        private void HandleSoloMarathonSetupActionClick(int actionIndex)
        {
            if (actionIndex < 0 || actionIndex > SoloMarathonBackActionIndex)
            {
                return;
            }

            selectedSoloMarathonSetupActionIndex = actionIndex;

            if (actionIndex < SoloMarathonHeatLevels.Length)
            {
                selectedSoloMarathonDifficulty = SoloMarathonHeatLevels[actionIndex];
                pendingValidationMessage = BuildSoloMarathonHeatSelectionMessage();
                return;
            }

            if (actionIndex == SoloMarathonLaunchActionIndex)
            {
                StartSoloMarathonRun();
                return;
            }

            if (actionIndex == SoloMarathonBackActionIndex)
            {
                EnterMainMenu();
            }
        }

        private void ToggleDiagnosticsOverlay()
        {
            isDiagnosticsOverlayVisible = !isDiagnosticsOverlayVisible;
        }

        private string BuildLevelLabel()
        {
            return currentLevel == null
                ? "No levels loaded"
                : $"Level {currentLevelIndex + 1:00}";
        }

        private string GetScoreDisplayLabel()
        {
            return activeRunSettings != null && activeRunSettings.ScoringMode == RunScoringMode.HighScore
                ? "High Score"
                : "Score";
        }

        private string GetLifeCounterLabel()
        {
            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                return turnBasedMultiplayerController != null && turnBasedMultiplayerController.IsTopScoreMode
                    ? "Turns"
                    : "Lives";
            }

            return UsesHighScoreMode() && !UsesFiniteHighScoreLives() ? "Losses" : "Lives";
        }

        private int GetLifeCounterValue()
        {
            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                return turnBasedMultiplayerController != null && turnBasedMultiplayerController.IsTopScoreMode
                    ? turnBasedMultiplayerController.GetCurrentPlayerTurnsRemaining()
                    : turnBasedMultiplayerController?.GetCurrentPlayerLivesRemaining() ?? livesRemaining;
            }

            return UsesHighScoreMode() && !UsesFiniteHighScoreLives() ? lifeLossCount : livesRemaining;
        }

        private string BuildScoreModeSummaryLabel(RunSettings settings)
        {
            if (settings == null)
            {
                return "Score Mode offline";
            }

            return settings.UsesLifeLossScorePenalty
                ? $"{settings.ScoringModeLabel} | Life Loss -{settings.LifeLossScorePenalty:0000}"
                : settings.ScoringModeLabel;
        }

        private static string BuildRetrySummaryLabel(RunSettings settings)
        {
            if (settings == null)
            {
                return "Retries offline";
            }

            return settings.IsSoloMarathonMode
                ? $"Lives {settings.StartingLives}"
                : settings.ScoringMode == RunScoringMode.HighScore
                ? "Retries unlimited"
                : $"Lives {settings.StartingLives}";
        }

        private static string FormatScoreValue(int value)
        {
            if (value < 0)
            {
                var absoluteValue = -(long)value;
                return $"-{absoluteValue:0000}";
            }

            return value.ToString("0000", CultureInfo.InvariantCulture);
        }

        private string BuildGameplayStatusLine()
        {
            if (tiltAlarmState.IsAlarmActive)
            {
                return $"Tilt Alarm - rail locked {tiltAlarmState.AlarmTimer:0.0}s";
            }

            if (tiltAlarmState.IsWarningHot)
            {
                return $"Tilt heat {Mathf.RoundToInt(tiltAlarmState.HeatRatio * 100f):00}%";
            }

            return activeLevelGlitchPlan != null && activeLevelGlitchPlan.IsActive
                ? activeLevelGlitchPlan.HudLabel
                : string.Empty;
        }

        private string BuildReadyToServeMessage()
        {
            if (activeRunSettings == null || !activeRunSettings.IsTurnBasedMode)
            {
                return "Press Space to launch. T nudges during play. Up/Down tunes speed.";
            }

            return $"{turnBasedMultiplayerController?.BuildSwitchTitle() ?? "Up Next"}\nPress Space to launch. T nudges during play. Up/Down tunes speed.";
        }

        private string BuildLifeLostMessage()
        {
            if (serveBallRevealDelayTimer > 0f)
            {
                return BuildLifeLostWaitingMessage();
            }

            if (activeRunSettings != null && activeRunSettings.IsTurnBasedMode)
            {
                var lines = new List<string>
                {
                    turnBasedMultiplayerController?.BuildSwitchTitle() ?? "Up Next",
                    turnBasedMultiplayerController?.BuildSwitchSummaryLine() ?? "Ball lost. Next player is on deck.",
                };
                lines.AddRange(BuildTurnBasedLeaderboardLines(5));
                lines.Add("Press Space to serve.");
                return string.Join("\n", lines);
            }

            if (GetLifeLossScorePenalty() > 0)
            {
                if (lastLifeLossUsedAutoSave)
                {
                    return BuildAutoSaveLifeLossSummary();
                }

                if (lastLifeLossUsedSpareFuse)
                {
                    return BuildSpareFuseLifeLossSummary();
                }

                return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                    ? $"Ball lost. -{GetLifeLossScorePenalty():0000} score. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                    : $"Life lost. -{GetLifeLossScorePenalty():0000} score. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.";
            }

            if (lastLifeLossUsedAutoSave)
            {
                return BuildAutoSaveLifeLossSummary();
            }

            if (lastLifeLossUsedSpareFuse)
            {
                return BuildSpareFuseLifeLossSummary();
            }

            return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                ? $"Ball lost. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                : $"Life lost. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.";
        }

        private string BuildLifeLostWaitingMessage()
        {
            if (GetLifeLossScorePenalty() > 0)
            {
                return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                    ? $"Ball lost. -{GetLifeLossScorePenalty():0000} score. Losses {lifeLossCount:00}. Ball loading..."
                    : $"Life lost. -{GetLifeLossScorePenalty():0000} score. {livesRemaining} remaining. Ball loading...";
            }

            return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                ? $"Ball lost. Losses {lifeLossCount:00}. Ball loading..."
                : $"Life lost. {livesRemaining} remaining. Ball loading...";
        }

        private string BuildAutoSaveLifeLossSummary()
        {
            return $"AUTO SAVE! -{lastAutoSaveScoreCost:0000} points. Extra ball loaded. Press Space to serve again. Up/Down tunes speed.";
        }

        private static string BuildSpareFuseLifeLossSummary()
        {
            return "SPARE FUSE! The cabinet saved your last ball. Press Space to serve again. Up/Down tunes speed.";
        }

        private string BuildPauseSummaryLabel()
        {
            return $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Balls {Mathf.Max(0, activeBalls.Count):00} | {BuildLevelLabel()}";
        }

        private string BuildTurnBasedPlayerHudPrefix()
        {
            return activeRunSettings != null && activeRunSettings.IsTurnBasedMode
                ? turnBasedMultiplayerController?.BuildCurrentPlayerHudLabel() ?? string.Empty
                : string.Empty;
        }

        private string[] BuildTurnBasedLeaderboardLines(int maxLines)
        {
            return activeRunSettings != null && activeRunSettings.IsTurnBasedMode
                ? turnBasedMultiplayerController?.BuildLeaderboardLines(maxLines) ?? Array.Empty<string>()
                : Array.Empty<string>();
        }

        private BreakoutTurnLeaderboardEntry[] BuildTurnBasedLeaderboardEntries(int maxLines)
        {
            return activeRunSettings != null && activeRunSettings.IsTurnBasedMode
                ? turnBasedMultiplayerController?.BuildLeaderboardEntries(maxLines) ?? Array.Empty<BreakoutTurnLeaderboardEntry>()
                : Array.Empty<BreakoutTurnLeaderboardEntry>();
        }

        private BreakoutTurnLeaderboardEntry[] BuildSoloMarathonLeaderboardEntries()
        {
            var entries = new BreakoutTurnLeaderboardEntry[SoloMarathonHeatLevels.Length];

            for (var index = 0; index < SoloMarathonHeatLevels.Length; index++)
            {
                var heat = SoloMarathonHeatLevels[index];
                var record = BreakoutSoloMarathonScoreStore.Load(heat);
                entries[index] = new BreakoutTurnLeaderboardEntry(
                    index + 1,
                    heat == selectedSoloMarathonDifficulty,
                    $"Heat {FormatSoloMarathonDifficultyLabel(heat)}",
                    record?.Score ?? 0,
                    record?.StageReached ?? 1,
                    0,
                    0,
                    record == null ? "NO SCORE" : $"ST{record.StageReached:00}");
            }

            return entries;
        }

        private string BuildUpgradeSummaryLabel(int maxNames)
        {
            if (activeRunState == null || !activeRunState.HasActiveBuild)
            {
                return "Build: none";
            }

            var chosenUpgrades = activeRunState.ChosenUpgrades;
            var chosenDropUnlocks = activeRunState.ChosenDropUnlocks;
            var names = new List<string>();

            for (var index = 0; index < chosenUpgrades.Count && names.Count < Mathf.Max(1, maxNames); index++)
            {
                var upgrade = chosenUpgrades[index];

                if (upgrade != null)
                {
                    names.Add(upgrade.HudLabel);
                }
            }

            for (var index = 0; index < chosenDropUnlocks.Count && names.Count < Mathf.Max(1, maxNames); index++)
            {
                var definition = chosenDropUnlocks[index];

                if (definition != null)
                {
                    names.Add(definition.HudLabel);
                }
            }

            var chosenRewardCount = chosenUpgrades.Count + chosenDropUnlocks.Count;
            var remainder = Mathf.Max(0, chosenRewardCount - names.Count);
            var labels = string.Join(", ", names);

            if (remainder > 0)
            {
                labels = string.IsNullOrWhiteSpace(labels) ? $"+{remainder} more" : $"{labels}, +{remainder} more";
            }

            return $"Build {chosenRewardCount:00}: {labels}";
        }

        private string BuildBallSpeedControlLabel()
        {
            var baseSpeed = GetBallSpeedBase();
            var currentSpeed = GetDisplayedBallSpeed();
            var maxSpeed = GetMaximumBallSpeed();
            var speedScoreMultiplier = scoreService?.GetScoreMultiplierForSpeed(currentSpeed, ballSpeed) ?? 1f;
            var scoreMultiplier = speedScoreMultiplier * GetActiveScoreMultiplier();
            return
                $"Ball Speed {currentSpeed:0.00} | Score x{scoreMultiplier:0.00} | Base {baseSpeed:0.00} | Manual x{manualBallSpeedMultiplier:0.00} | Cap {maxSpeed:0.00}";
        }

        private string BuildRoundStateLabel()
        {
            return roundState switch
            {
                RoundState.ReadyToServe => "Ready to serve",
                RoundState.SoloMarathonSetup => "Neon Marathon setup",
                RoundState.LifetimeStats => "Lifetime stats",
                RoundState.Playing => "Ball in play",
                RoundState.LifeLost => "Recovering from a loss",
                RoundState.UpgradeDraft => "Choosing a run reward",
                RoundState.Paused => "Paused",
                RoundState.LevelComplete => "Level complete",
                RoundState.GameOver => "Game over",
                _ => roundState.ToString(),
            };
        }

        private string BuildRunSummaryLabel()
        {
            if (activeRunSettings == null)
            {
                return $"Tape ID: {GetPendingSeedDisplay()} | Theme: {ResolvePendingThemeDefinition()?.DisplayName ?? "Fallback"} | Preview only";
            }

            var heatSummary = activeRunSettings.IsRogueMode
                ? $" | Heat {activeRunSettings.RogueIntensity:00}"
                : activeRunSettings.IsSoloMarathonMode
                    ? $" | Heat Score x{GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty):0.00}"
                    : string.Empty;
            var summary =
                $"{activeRunSettings.GameModeLabel} | Tape ID: {activeRunSettings.Seed} | {activeRunSettings.DifficultyLabel} | {BuildScoreModeSummaryLabel(activeRunSettings)} | {BuildRetrySummaryLabel(activeRunSettings)} | Balls/Serve {GetEffectiveBallsPerServe()} | " +
                $"Theme: {activeRunSettings.ThemeLabel} | Drops: {BuildDropSummaryLabel(activeRunSettings)} | {activeRunSettings.LevelGlitchLabel}{heatSummary} | Paddle x{activeRunSettings.PaddleWidthMultiplier:0.00} speed x{activeRunSettings.PaddleSpeedMultiplier:0.00} | Ball x{activeRunSettings.BallSpeedMultiplier:0.00} | Build {GetChosenUpgradeCount():00}";
            return summary;
        }

        private static string BuildDropSummaryLabel(RunSettings settings)
        {
            if (settings == null)
            {
                return "Offline";
            }

            return settings.ForcePickupDropsOnBreak
                ? $"{settings.DropPoolLabel} | {settings.DropCadenceLabel}"
                : settings.DropPoolLabel;
        }

        private static string FormatLevelGlitchSelectionLabel(LevelGlitchSelection selection)
        {
            return selection switch
            {
                LevelGlitchSelection.Random => "Random",
                LevelGlitchSelection.WarpGates => "Warp Gates",
                LevelGlitchSelection.TurboRail => "Turbo Rail",
                LevelGlitchSelection.MirrorGrid => "Mirror Grid",
                LevelGlitchSelection.GravityPocket => "Gravity Pocket",
                LevelGlitchSelection.TokenStorm => "Token Storm",
                LevelGlitchSelection.StaticWall => "Static Wall",
                LevelGlitchSelection.RowRewrite => "Row Rewrite",
                LevelGlitchSelection.PrismLanes => "Prism Lanes",
                LevelGlitchSelection.SwitchbackRails => "Switchback Rails",
                LevelGlitchSelection.CapsuleRoulette => "Capsule Roulette",
                LevelGlitchSelection.DriftRows => "Drift Rows",
                LevelGlitchSelection.HotCorners => "Hot Corners",
                LevelGlitchSelection.FlickerBricks => "Flicker Bricks",
                LevelGlitchSelection.CassetteSkip => "Cassette Skip",
                LevelGlitchSelection.GhostRow => "Ghost Row",
                LevelGlitchSelection.SplitHorizon => "Split Horizon",
                LevelGlitchSelection.RogueGate => "Rogue Gate",
                LevelGlitchSelection.PickupPinball => "Pickup Pinball",
                LevelGlitchSelection.MagnetStorm => "Magnet Storm",
                LevelGlitchSelection.BrickConveyor => "Brick Conveyor",
                LevelGlitchSelection.BlacklightBricks => "Blacklight Bricks",
                LevelGlitchSelection.RewindWall => "Rewind Wall",
                LevelGlitchSelection.ScoreLeak => "Score Leak",
                LevelGlitchSelection.LaserRain => "Laser Rain",
                LevelGlitchSelection.ThinAir => "Thin Air",
                LevelGlitchSelection.PrismShuffle => "Prism Shuffle",
                LevelGlitchSelection.CloneStatic => "Clone Static",
                LevelGlitchSelection.DropTide => "Drop Tide",
                LevelGlitchSelection.BrickLock => "Brick Lock",
                LevelGlitchSelection.SpeedSteps => "Speed Steps",
                LevelGlitchSelection.MirrorServe => "Mirror Serve",
                LevelGlitchSelection.StaticJackpot => "Static Jackpot",
                LevelGlitchSelection.JammedRails => "Jammed Rails",
                LevelGlitchSelection.GravitySwap => "Gravity Swap",
                LevelGlitchSelection.VhsTear => "VHS Tear",
                LevelGlitchSelection.CapsuleBlackout => "Capsule Blackout",
                LevelGlitchSelection.Brickquake => "Brickquake",
                LevelGlitchSelection.CabinetTilt => "Cabinet Tilt",
                LevelGlitchSelection.TurboTax => "Turbo Tax",
                LevelGlitchSelection.WarpJam => "Warp Jam",
                LevelGlitchSelection.NeonFlood => "Neon Flood",
                LevelGlitchSelection.LockstepRows => "Lockstep Rows",
                LevelGlitchSelection.StaticServe => "Static Serve",
                LevelGlitchSelection.MeltdownCore => "Meltdown Core",
                _ => "Off",
            };
        }

        private static string FormatSoloMarathonDifficultyLabel(BreakoutHotSeatDifficulty difficulty)
        {
            return difficulty switch
            {
                BreakoutHotSeatDifficulty.Chill => "Chill",
                BreakoutHotSeatDifficulty.Rad => "Rad",
                BreakoutHotSeatDifficulty.Mondo => "Mondo",
                BreakoutHotSeatDifficulty.Bogus => "Bogus",
                _ => "Gnarly",
            };
        }

        internal static float GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty difficulty)
        {
            return difficulty switch
            {
                BreakoutHotSeatDifficulty.Rad => 1.15f,
                BreakoutHotSeatDifficulty.Gnarly => 1.35f,
                BreakoutHotSeatDifficulty.Mondo => 1.65f,
                BreakoutHotSeatDifficulty.Bogus => 2f,
                _ => 1f,
            };
        }

        private static int GetSoloMarathonHeatIndex(BreakoutHotSeatDifficulty difficulty)
        {
            for (var index = 0; index < SoloMarathonHeatLevels.Length; index++)
            {
                if (SoloMarathonHeatLevels[index] == difficulty)
                {
                    return index;
                }
            }

            return (int)BreakoutHotSeatDifficulty.Gnarly;
        }

        private string[] BuildSoloMarathonSetupActionLabels()
        {
            var labels = new string[SoloMarathonBackActionIndex + 1];

            for (var index = 0; index < SoloMarathonHeatLevels.Length; index++)
            {
                var heat = SoloMarathonHeatLevels[index];
                labels[index] = heat == selectedSoloMarathonDifficulty
                    ? $"{FormatSoloMarathonDifficultyLabel(heat)} [ON]"
                    : FormatSoloMarathonDifficultyLabel(heat);
            }

            labels[SoloMarathonLaunchActionIndex] = "Start Neon Marathon";
            labels[SoloMarathonBackActionIndex] = "Back To Mode Select";
            return labels;
        }

        private string BuildSoloMarathonHeatSelectionMessage()
        {
            return $"Heat {FormatSoloMarathonDifficultyLabel(selectedSoloMarathonDifficulty)} selected. Score x{GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty):0.00}.";
        }

        private static string BuildSoloMarathonHeatRulesLine(BreakoutHotSeatDifficulty difficulty)
        {
            return difficulty switch
            {
                BreakoutHotSeatDifficulty.Chill => "Chill: wider paddle, slower ball, helpful capsules.",
                BreakoutHotSeatDifficulty.Rad => "Rad: light help, clean speed, friendly capsules.",
                BreakoutHotSeatDifficulty.Mondo => "Mondo: faster ball, tougher bricks, glitched stages.",
                BreakoutHotSeatDifficulty.Bogus => "Bogus: harmful capsules, hot speed, max payout.",
                _ => "Gnarly: standard gear with glitched stages in the mix.",
            };
        }

        private void ToggleHudMenuOverlay()
        {
            if (roundState == RoundState.Paused)
            {
                ResumeGameplay();
                return;
            }

            if (CanPauseRoundState(roundState))
            {
                PauseGameplay();
            }
        }

        private void QuitGame()
        {
            Debug.Log("Quit requested from the cabinet menu.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private string GetPendingSeedDisplay()
        {
            var pendingSeed = pendingRunSetup?.PendingSeedText;
            return string.IsNullOrWhiteSpace(pendingSeed) ? "<random on start>" : pendingSeed;
        }

        private static string FormatSignedStep(int value)
        {
            return value == 0 ? "0" : value > 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatDeveloperToggle(bool selected)
        {
            return selected ? "[ON]" : "[--]";
        }

        private static string FormatDeveloperEncounterLabel(BreakoutDeveloperEncounter encounter)
        {
            return $"[STAGE] {encounter.DisplayName}";
        }

        private static string FormatDeveloperUpgradeLabel(RunUpgradeDefinition upgrade)
        {
            return upgrade != null ? upgrade.DisplayName : "No upgrades loaded";
        }

        private static string FormatDeveloperDropLabel(PowerUpDefinition drop)
        {
            if (drop == null)
            {
                return "No drops loaded";
            }

            return drop.IsBeneficial ? drop.DisplayName : $"{drop.DisplayName} (Hazard)";
        }

        private static string FormatDeveloperForcedDropPreview(bool enabled, PowerUpDefinition drop)
        {
            if (!enabled || drop == null)
            {
                return "Off";
            }

            return drop.DisplayName;
        }

        private static string FormatDeveloperForcedGlitchLabel(bool enabled, LevelGlitchSelection selection)
        {
            return $"{FormatDeveloperToggle(enabled)} {FormatLevelGlitchSelectionLabel(selection)}";
        }

        private static string FormatDeveloperForcedGlitchPreview(bool enabled, LevelGlitchSelection selection)
        {
            return !enabled
                ? "normal rolls"
                : FormatLevelGlitchSelectionLabel(selection);
        }

        private static string BuildDeveloperForcedGlitchMessage(bool enabled, LevelGlitchSelection selection)
        {
            return !enabled
                ? "Glitches use normal Rogue rules."
                : $"{FormatLevelGlitchSelectionLabel(selection)} forced for this stage.";
        }

        private void UpdateTimedEffects()
        {
            powerUpService?.UpdateTimedEffects(IsGameplaySimulationActive(), Time.deltaTime, ApplyActiveEffects);
        }

        private void UpdatePickupBanner()
        {
            powerUpService?.UpdatePickupBanner(Time.deltaTime);
        }

        private void UpdateAutoSaveBurst()
        {
            if (autoSaveBurstTimer <= 0f)
            {
                return;
            }

            autoSaveBurstTimer = Mathf.Max(0f, autoSaveBurstTimer - Time.unscaledDeltaTime);
        }

        private void UpdateSpareFuseBurst()
        {
            if (spareFuseBurstTimer <= 0f)
            {
                return;
            }

            spareFuseBurstTimer = Mathf.Max(0f, spareFuseBurstTimer - Time.unscaledDeltaTime);
        }

        private void TrySpawnPickup(Brick brick)
        {
            var spawnedPickup = powerUpService?.TrySpawnPickup(
                brick,
                activeRunSettings,
                activeRunState,
                GetEffectiveDropChanceMultiplier(),
                NextGameplayRandomFloat,
                pickupsRoot,
                arenaBottom,
                themeService,
                this,
                ResolveDeveloperForcedDrop(),
                loadedPowerUpDefinitions,
                GetEffectivePickupFallSpeedMultiplier(),
                IsCapsuleRouletteActive());

            if (spawnedPickup != null)
            {
                ApplyTokenStormFallSpeed(spawnedPickup);
                ApplyDropTideWaveDelay(spawnedPickup);
                ApplyPickupPinballMotion(spawnedPickup);
                ApplyCapsuleBlackout(spawnedPickup);
                runStatsService?.RegisterDropDropped(spawnedPickup.Definition);
                audioService?.PlayPickupDropped();
            }

            ApplyVisualEffectState();
        }

        private void TrySpawnTurboTaxHazardPickup(Brick brick, BallController scoringBall, BrickDestructionCause destructionCause)
        {
            if (brick == null
                || scoringBall == null
                || powerUpService == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TurboTax)
                || destructionCause != BrickDestructionCause.Impact)
            {
                return;
            }

            var turboTax = activeLevelGlitchPlan.TurboTax;

            if (!BreakoutTurboTaxCalculator.ShouldRollSlowHazardDrop(scoringBall.CurrentSpeed, ballSpeed, turboTax)
                || NextGameplayRandomFloat(0f, 1f) > turboTax.SlowHazardDropChance
                || !TryResolveTurboTaxHazardDrop(out var hazardDrop))
            {
                return;
            }

            var spawnedPickup = powerUpService.TrySpawnPickup(
                brick,
                activeRunSettings,
                activeRunState,
                1f,
                NextGameplayRandomFloat,
                pickupsRoot,
                arenaBottom,
                themeService,
                this,
                hazardDrop,
                loadedPowerUpDefinitions,
                GetEffectivePickupFallSpeedMultiplier(),
                IsCapsuleRouletteActive());

            if (spawnedPickup == null)
            {
                return;
            }

            ApplyTokenStormFallSpeed(spawnedPickup);
            ApplyDropTideWaveDelay(spawnedPickup);
            ApplyPickupPinballMotion(spawnedPickup);
            ApplyCapsuleBlackout(spawnedPickup);
            runStatsService?.RegisterDropDropped(spawnedPickup.Definition);
            audioService?.PlayPickupDropped();
            ApplyVisualEffectState();
        }

        private void TryTriggerNeonFlood(Brick brick, int slamChainCount)
        {
            if (brick == null
                || powerUpService == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.NeonFlood)
                || !BreakoutNeonFloodCalculator.ShouldTrigger(
                    slamChainCount,
                    Time.time,
                    lastNeonFloodTriggerTime,
                    activeLevelGlitchPlan.NeonFlood)
                || !TryResolveNeonFloodDrop(isHelpful: true, out var helpfulDrop)
                || !TryResolveNeonFloodDrop(isHelpful: false, out var harmfulDrop))
            {
                return;
            }

            var neonFlood = activeLevelGlitchPlan.NeonFlood;
            var origin = (Vector2)brick.transform.position;
            var helpfulFirst = NextGameplayRandomFloat(0f, 1f) < 0.5f;
            var helpfulOffset = new Vector2(helpfulFirst ? -neonFlood.PickupOffset : neonFlood.PickupOffset, 0f);
            var harmfulOffset = -helpfulOffset;
            var baseFallSpeedMultiplier = GetEffectivePickupFallSpeedMultiplier();

            var helpfulPickup = SpawnNeonFloodPickup(
                origin + helpfulOffset,
                helpfulDrop,
                baseFallSpeedMultiplier * neonFlood.HelpfulFallSpeedMultiplier);
            var harmfulPickup = SpawnNeonFloodPickup(
                origin + harmfulOffset,
                harmfulDrop,
                baseFallSpeedMultiplier * neonFlood.HarmfulFallSpeedMultiplier);

            if (helpfulPickup == null && harmfulPickup == null)
            {
                return;
            }

            lastNeonFloodTriggerTime = Time.time;
            powerUpService.ShowStatusBanner("NEON FLOOD!", new Color(0.03f, 0.93f, 0.98f, 1f), 1.35f);
            audioService?.PlayPickupDropped();
            ApplyVisualEffectState();
        }

        private PowerUpPickup SpawnNeonFloodPickup(Vector2 position, PowerUpDefinition definition, float pickupFallSpeedMultiplier)
        {
            var pickup = powerUpService.SpawnForcedPickup(
                position,
                definition,
                loadedPowerUpDefinitions,
                activeRunSettings,
                activeRunState,
                NextGameplayRandomFloat,
                pickupsRoot,
                arenaBottom,
                themeService,
                this,
                pickupFallSpeedMultiplier,
                IsCapsuleRouletteActive());

            if (pickup == null)
            {
                return null;
            }

            ApplyTokenStormFallSpeed(pickup);
            ApplyDropTideWaveDelay(pickup);
            ApplyPickupPinballMotion(pickup);
            ApplyCapsuleBlackout(pickup);
            runStatsService?.RegisterDropDropped(pickup.Definition);
            return pickup;
        }

        private bool TryResolveNeonFloodDrop(bool isHelpful, out PowerUpDefinition drop)
        {
            drop = null;

            if (TryResolveNeonFloodDrop(isHelpful, requireUnlocked: true, out drop))
            {
                return true;
            }

            return TryResolveNeonFloodDrop(isHelpful, requireUnlocked: false, out drop);
        }

        private bool TryResolveNeonFloodDrop(bool isHelpful, bool requireUnlocked, out PowerUpDefinition drop)
        {
            drop = null;

            if (loadedPowerUpDefinitions.Count == 0)
            {
                return false;
            }

            var totalWeight = 0f;

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (!IsNeonFloodDropCandidate(definition, isHelpful, requireUnlocked))
                {
                    continue;
                }

                totalWeight += BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);
            }

            if (totalWeight <= 0f)
            {
                return false;
            }

            var roll = NextGameplayRandomFloat(0f, totalWeight);

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (!IsNeonFloodDropCandidate(definition, isHelpful, requireUnlocked))
                {
                    continue;
                }

                roll -= BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);

                if (roll <= 0f)
                {
                    drop = definition;
                    return true;
                }
            }

            return false;
        }

        private bool IsNeonFloodDropCandidate(PowerUpDefinition definition, bool isHelpful, bool requireUnlocked)
        {
            if (definition == null
                || definition.IsBeneficial != isHelpful
                || definition.EffectType == PowerUpEffectType.RandomMixedDrop
                || definition.EffectType == PowerUpEffectType.RandomHarmfulDrop)
            {
                return false;
            }

            return !requireUnlocked
                || activeRunState == null
                || activeRunState.IsDropUnlocked(definition);
        }

        private bool TryResolveTurboTaxHazardDrop(out PowerUpDefinition hazardDrop)
        {
            hazardDrop = null;

            if (TryResolveTurboTaxHazardDrop(requireUnlocked: true, out hazardDrop))
            {
                return true;
            }

            return TryResolveTurboTaxHazardDrop(requireUnlocked: false, out hazardDrop);
        }

        private bool TryResolveTurboTaxHazardDrop(bool requireUnlocked, out PowerUpDefinition hazardDrop)
        {
            hazardDrop = null;

            if (loadedPowerUpDefinitions.Count == 0)
            {
                return false;
            }

            var totalWeight = 0f;

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (!IsTurboTaxHazardCandidate(definition, requireUnlocked))
                {
                    continue;
                }

                totalWeight += BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);
            }

            if (totalWeight <= 0f)
            {
                return false;
            }

            var roll = NextGameplayRandomFloat(0f, totalWeight);

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (!IsTurboTaxHazardCandidate(definition, requireUnlocked))
                {
                    continue;
                }

                roll -= BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);

                if (roll <= 0f)
                {
                    hazardDrop = definition;
                    return true;
                }
            }

            return false;
        }

        private bool IsTurboTaxHazardCandidate(PowerUpDefinition definition, bool requireUnlocked)
        {
            if (definition == null
                || definition.IsBeneficial
                || definition.EffectType == PowerUpEffectType.RandomMixedDrop)
            {
                return false;
            }

            return !requireUnlocked
                || activeRunState == null
                || activeRunState.IsDropUnlocked(definition);
        }

        private void UpdateCapsuleRoulettePickups()
        {
            powerUpService?.UpdateCapsuleRoulettePickups(
                IsGameplaySimulationActive() && IsCapsuleRouletteActive(),
                Time.deltaTime,
                NextGameplayRandomFloat,
                themeService);
        }

        private bool IsCapsuleRouletteActive()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CapsuleRoulette);
        }

        private bool IsPickupPinballActive()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.PickupPinball);
        }

        private bool IsDropTideActive()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.DropTide);
        }

        private bool IsCapsuleBlackoutActive()
        {
            return activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.CapsuleBlackout);
        }

        private PowerUpDefinition ResolveDeveloperForcedDrop()
        {
            if (!isDeveloperRunActive || developerLaunchState == null || !developerLaunchState.ForcedDropEnabled)
            {
                return null;
            }

            return developerLaunchState.ResolveForcedDrop(loadedPowerUpDefinitions);
        }

        private void ApplyPowerUp(PowerUpDefinition powerUpDefinition)
        {
            if (powerUpService == null)
            {
                return;
            }

            var applicationResult = powerUpService.ApplyPowerUp(
                powerUpDefinition,
                themeService,
                GetPersistentRunUpgradeModifiers().MaxActiveHazardTimedEffectStacks);

            if (applicationResult.ShieldWallChargesGranted > 0)
            {
                shieldWallCharges += applicationResult.ShieldWallChargesGranted;
                UpdateShieldWallVisual();
            }

            if (applicationResult.MissileChargesGranted > 0)
            {
                availableMissiles += applicationResult.MissileChargesGranted;
            }

            if (applicationResult.ShouldTriggerFuseBurst)
            {
                TryTriggerFuseBurst();
            }

            ApplyActiveEffects();

            if (applicationResult.ShouldSpawnMultiBall)
            {
                SpawnMultiBall(powerUpDefinition);
            }
        }

        private void QueueRiskRebateForPickup(PowerUpDefinition caughtDefinition)
        {
            if (powerUpService == null || caughtDefinition == null)
            {
                return;
            }

            var extensionSeconds = GetPersistentRunUpgradeModifiers().RiskRebateHelpfulEffectExtensionSeconds;

            if (extensionSeconds <= 0f || !CanTriggerRiskRebate(caughtDefinition))
            {
                return;
            }

            powerUpService.QueueHelpfulTimedEffectExtension(extensionSeconds);
            powerUpService.ShowStatusBanner("RISK REBATE!", new Color(1f, 0.87f, 0.36f, 1f), 1.15f);
        }

        private static bool CanTriggerRiskRebate(PowerUpDefinition caughtDefinition)
        {
            return caughtDefinition != null
                && (!caughtDefinition.IsBeneficial || caughtDefinition.EffectType == PowerUpEffectType.RandomMixedDrop);
        }

        private void AwardCapsuleMadnessPickupBonus(Vector2 pickupPosition)
        {
            var bonusPoints = BreakoutPowerUpService.CapsuleMadnessPickupBonusPoints;
            score += bonusPoints;
            scoreService?.CreateFloatingScorePopup(
                pickupPosition,
                bonusPoints,
                "CAPSULE MADNESS",
                ResolveCapsuleMadnessColor());
        }

        private int ResolveStaticJackpotBonusPoints(
            Brick brick,
            BallController scoringBall,
            BrickDestructionCause destructionCause,
            int awardedBasePoints)
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.StaticJackpot)
                || brick == null
                || awardedBasePoints <= 0)
            {
                return 0;
            }

            var staticJackpot = activeLevelGlitchPlan.StaticJackpot;
            var playfieldBounds = Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop);

            if (BreakoutStaticJackpotCalculator.TryFindZone(
                brick.transform.position,
                playfieldBounds,
                staticJackpot,
                out _))
            {
                return Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * (staticJackpot.BonusScoreMultiplier - 1f)));
            }

            if (destructionCause == BrickDestructionCause.Impact && scoringBall != null)
            {
                scoringBall.ApplySpeedBurst(
                    staticJackpot.MissSpeedBurstMultiplier,
                    staticJackpot.MissSpeedBurstDurationSeconds);
            }

            return 0;
        }

        private int ResolveTurboTaxBonusPoints(
            BallController scoringBall,
            BrickDestructionCause destructionCause,
            int awardedBasePoints)
        {
            if (activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TurboTax)
                || destructionCause != BrickDestructionCause.Impact
                || scoringBall == null)
            {
                return 0;
            }

            return BreakoutTurboTaxCalculator.CalculateHighSpeedBonusPoints(
                awardedBasePoints,
                scoringBall.CurrentSpeed,
                ballSpeed,
                activeLevelGlitchPlan.TurboTax);
        }

        private void AwardBankBonusIfAvailable(Vector2 worldPosition)
        {
            if (powerUpService == null || !powerUpService.TryConsumeBankBonus(out var bonusPoints))
            {
                return;
            }

            score += bonusPoints;
            audioService?.PlayBonusScore();
            scoreService?.CreateFloatingScorePopup(
                worldPosition,
                bonusPoints,
                "BANK BONUS",
                ResolveComboPopupColor());
        }

        private static string CombineBonusLabels(string firstLabel, string secondLabel)
        {
            if (string.IsNullOrWhiteSpace(firstLabel))
            {
                return secondLabel ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(secondLabel))
            {
                return firstLabel;
            }

            return $"{firstLabel} + {secondLabel}";
        }

        private bool TryApplyAutoSave()
        {
            if (!ShouldAutoSaveLastBall(activeRunSettings, score))
            {
                return false;
            }

            var autoSaveCost = GetAutoSaveScoreCost(activeRunSettings);
            score -= autoSaveCost;
            livesRemaining += 1;
            lastLifeLossUsedAutoSave = true;
            lastAutoSaveScoreCost = autoSaveCost;
            autoSaveBurstTimer = AutoSaveBurstDurationSeconds;
            powerUpService?.ShowStatusBanner($"AUTO SAVE -{autoSaveCost:0000}", ResolveAutoSaveColor(), AutoSaveBurstDurationSeconds);
            return true;
        }

        private bool TryApplySpareFuse()
        {
            if (GetRemainingSpareFuseSaves() <= 0)
            {
                return false;
            }

            spareFuseSavesUsed += 1;
            livesRemaining += 1;
            lastLifeLossUsedSpareFuse = true;
            spareFuseBurstTimer = SpareFuseBurstDurationSeconds;
            powerUpService?.ShowStatusBanner("SPARE FUSE!", ResolveSpareFuseColor(), SpareFuseBurstDurationSeconds);
            return true;
        }

        private void ApplyActiveEffects()
        {
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            activeEffectModifiers = powerUpService != null
                ? powerUpService.CalculateEffectModifiers(
                    (activeRunSettings?.PaddleWidthMultiplier ?? 1f) * persistentModifiers.PaddleWidthMultiplier,
                    persistentModifiers.WavyPaddleStrength)
                : new BreakoutEffectModifiers(
                    (activeRunSettings?.PaddleWidthMultiplier ?? 1f) * persistentModifiers.PaddleWidthMultiplier,
                    1f,
                    persistentModifiers.WavyPaddleStrength,
                    1f,
                    1f,
                    false,
                    false,
                    false,
                    false,
                    0f,
                    false,
                    0f,
                    0f,
                    1f,
                    0f,
                    0f,
                    0f,
                    1f,
                    1f,
                    false,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    false,
                    1f,
                    0f,
                    false);
            paddle.SetMoveSpeed(currentLevelPaddleSpeed
                * (activeRunSettings?.PaddleSpeedMultiplier ?? 1f)
                * activeEffectModifiers.PaddleSpeedMultiplier
                * (tiltAlarmState.IsAlarmActive ? 0f : 1f));
            var paddleHitMaximumWidth = paddle.SetWidthMultiplier(activeEffectModifiers.PaddleWidthMultiplier);

            if (paddleHitMaximumWidth && TryBreakWidePaddle())
            {
                ApplyActiveEffects();
                TriggerWidePaddleBreakFeedback();
                return;
            }

            UpdateJammedRailsPulse();

            if (activeEffectModifiers.BallSizeMultiplier > BallController.MaximumSizeMultiplier
                && TryPopMegaBall())
            {
                ApplyActiveEffects();
                TriggerMegaBallPopFeedback();
                return;
            }

            if (TryPopOverstackedMicroSpark())
            {
                ApplyActiveEffects();
                TriggerMicroSparkPopFeedback();
                return;
            }

            powerUpService?.RefreshActivePickupFallSpeedMultiplier(activeEffectModifiers.PickupFallSpeedMultiplier);

            paddle.SetWavyStrength(activeEffectModifiers.WavyPaddleStrength);
            paddle.SetControlsReversed(activeEffectModifiers.ReverseControlsEnabled);
            paddle.SetSplitGapWidthNormalized(activeEffectModifiers.SplitPaddleGapNormalized);
            paddle.SetLagSpikeStrength(activeEffectModifiers.LagSpikeStrength);
            paddle.SetHitTiltDegrees(activeEffectModifiers.PaddleHitTiltDegrees);
            paddle.SetWrapRailEnabled(activeEffectModifiers.PaddleWrapEnabled);
            paddle.SetClonePaddleEnabled(activeEffectModifiers.PaddleCloneEnabled);
            paddle.SetMirrorImagePaddleEnabled(activeEffectModifiers.MirrorImagePaddleEnabled);

            var currentBallSpeed = GetCurrentBallSpeed();
            var gravityWellCenter = new Vector2(0f, (arenaTop + arenaBottom) * 0.5f);
            var solarShotCharged = powerUpService != null && powerUpService.SolarShotCharges > 0;

            if (serveBall != null)
            {
                serveBall.SetMovementSpeed(currentBallSpeed);
                serveBall.SetPhaseThroughBricks(activeEffectModifiers.PhaseBallEnabled);
                serveBall.SetWeakBrickPierceThroughBricks(activeEffectModifiers.WeakBrickPierceEnabled);
                serveBall.SetSizeMultiplier(activeEffectModifiers.BallSizeMultiplier);
                serveBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
                ApplyGravityPocketToBall(serveBall);
                ApplySplitHorizonToBall(serveBall);
                ApplyCabinetTiltToBall(serveBall);
                serveBall.SetHotPotatoStrength(activeEffectModifiers.HotPotatoStrength);
                serveBall.SetExplosiveBallStrength(activeEffectModifiers.ExplosiveBallStrength);
                serveBall.SetSolarShotCharged(solarShotCharged);
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                activeBall.SetMovementSpeed(currentBallSpeed);
                activeBall.SetPhaseThroughBricks(activeEffectModifiers.PhaseBallEnabled);
                activeBall.SetWeakBrickPierceThroughBricks(activeEffectModifiers.WeakBrickPierceEnabled);
                activeBall.SetSizeMultiplier(activeEffectModifiers.BallSizeMultiplier);
                activeBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
                ApplyGravityPocketToBall(activeBall);
                ApplySplitHorizonToBall(activeBall);
                ApplyCabinetTiltToBall(activeBall);
                activeBall.SetHotPotatoStrength(activeEffectModifiers.HotPotatoStrength);
                activeBall.SetExplosiveBallStrength(activeEffectModifiers.ExplosiveBallStrength);
                activeBall.SetSolarShotCharged(solarShotCharged);
            }

            ApplyVisualEffectState();
            ApplyBrickJammerState();
            UpdateShieldWallVisual();
            UpdateVectorSightVisual();

            if (stickyCaughtBall != null && !stickyCaughtBallUsesCleanCatch && !activeEffectModifiers.StickyPaddleEnabled)
            {
                ReleaseStickyCaughtBall();
            }
        }

        private void UpdateVectorSightVisual()
        {
            if (vectorSightVisual == null)
            {
                return;
            }

            if (activeEffectModifiers.VectorSightStrength <= 0.001f
                || !IsGameplaySimulationActive()
                || paddle == null
                || paddleCollider == null)
            {
                vectorSightVisual.Hide();
                return;
            }

            if (!TryResolveVectorSightSegment(out var origin, out var target, out var proximity))
            {
                vectorSightVisual.Hide();
                return;
            }

            var strength = Mathf.Clamp01(activeEffectModifiers.VectorSightStrength);
            var alpha = Mathf.Lerp(0.42f, 1f, Mathf.Clamp01(proximity * strength));
            vectorSightVisual.Show(
                origin,
                target,
                ResolveVectorSightCoreColor(),
                ResolveVectorSightGlowColor(),
                alpha);
        }

        private bool TryResolveVectorSightSegment(out Vector2 origin, out Vector2 target, out float proximity)
        {
            origin = Vector2.zero;
            target = Vector2.zero;
            proximity = 0f;

            var ball = ResolveVectorSightBall();

            if (ball == null)
            {
                return false;
            }

            var paddleBounds = paddleCollider.bounds;
            var paddleTop = paddleBounds.max.y + Mathf.Max(0.03f, ballRadius * 0.45f);
            var ballPosition = (Vector2)ball.transform.position;
            var verticalDistance = Mathf.Max(0f, ballPosition.y - paddleTop);

            if (ball.HasLaunched)
            {
                var velocity = ball.CurrentVelocity;

                if (velocity.y >= -0.01f || verticalDistance > VectorSightPreviewRange)
                {
                    return false;
                }
            }

            var contactX = Mathf.Clamp(ballPosition.x, paddleBounds.min.x, paddleBounds.max.x);
            var bounceDirection = ball.ResolvePaddleBounceDirection(paddle, contactX);

            if (bounceDirection.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            origin = new Vector2(contactX, paddleTop);
            target = origin + (bounceDirection.normalized * VectorSightPreviewLength);
            proximity = ball.HasLaunched
                ? 1f - Mathf.Clamp01(verticalDistance / VectorSightPreviewRange)
                : 1f;
            return true;
        }

        private BallController ResolveVectorSightBall()
        {
            if (stickyCaughtBall != null)
            {
                return stickyCaughtBall;
            }

            if (serveBall != null && !serveBall.HasLaunched)
            {
                return serveBall;
            }

            BallController closestBall = null;
            var closestVerticalDistance = float.PositiveInfinity;
            var paddleTop = paddleCollider != null ? paddleCollider.bounds.max.y : arenaBottom + paddleFloorOffset;

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var ball = activeBalls[index];

                if (ball == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                if (!ball.HasLaunched || ball.CurrentVelocity.y >= -0.01f)
                {
                    continue;
                }

                var verticalDistance = ball.transform.position.y - paddleTop;

                if (verticalDistance < -0.2f
                    || verticalDistance > VectorSightPreviewRange
                    || verticalDistance >= closestVerticalDistance)
                {
                    continue;
                }

                closestVerticalDistance = verticalDistance;
                closestBall = ball;
            }

            return closestBall;
        }

        private Color ResolveVectorSightCoreColor()
        {
            return themeService != null
                ? Color.Lerp(
                    Color.white,
                    themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBeneficial, new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), powerUpSprite).PrimaryColor,
                    0.28f)
                : new Color(0.88f, 1f, 0.95f, 1f);
        }

        private Color ResolveVectorSightGlowColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBeneficial,
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    new Color(0.03f, 0.93f, 0.98f, 1f),
                    powerUpSprite).SecondaryColor
                : new Color(0.03f, 0.93f, 0.98f, 1f);
        }

        private bool TryBreakWidePaddle()
        {
            return powerUpService != null && powerUpService.RemoveBeneficialPaddleWidthEffects() > 0;
        }

        private bool TryPopMegaBall()
        {
            return powerUpService != null && powerUpService.RemoveBeneficialBallSizeEffects() > 0;
        }

        private bool TryPopOverstackedMicroSpark()
        {
            return powerUpService != null
                && powerUpService.RemoveMicroSparkEffectsAtStackThreshold(MicroSparkPopStackThreshold) > 0;
        }

        private void TriggerWidePaddleBreakFeedback()
        {
            paddle?.StartBreakWiggle();
            audioService?.PlayPowerDown();
            powerUpService?.ShowStatusBanner("RAIL BUSTED!", new Color(1f, 0.28f, 0.32f, 1f), 1.4f);
        }

        private void TriggerMegaBallPopFeedback()
        {
            audioService?.PlayMegaBallPop();
            powerUpService?.ShowStatusBanner("MEGA POP!", new Color(1f, 0.88f, 0.28f, 1f), 1.4f);
        }

        private void TriggerMicroSparkPopFeedback()
        {
            audioService?.PlayMegaBallPop();
            powerUpService?.ShowStatusBanner("MICRO POP!", new Color(1f, 0.87f, 0.36f, 1f), 1.4f);
        }

        private void SpawnMultiBall(PowerUpDefinition powerUpDefinition)
        {
            if (!IsGameplaySimulationActive() || activeBalls.Count == 0)
            {
                return;
            }

            var sourceBall = activeBalls[0];

            if (sourceBall == null)
            {
                return;
            }

            var sourceVelocity = sourceBall.GetComponent<Rigidbody2D>().linearVelocity;
            var sourceDirection = sourceVelocity.sqrMagnitude > 0.01f
                ? sourceVelocity.normalized
                : Vector2.up;
            var launchDirections = powerUpService != null
                ? powerUpService.BuildMultiBallDirections(sourceDirection, Mathf.Max(1, powerUpDefinition.ExtraBallCount))
                : new[] { sourceDirection };

            for (var index = 0; index < launchDirections.Length; index++)
            {
                var extraBall = CreateBall(false);
                extraBall.SetWorldPosition(sourceBall.transform.position);
                extraBall.Launch(launchDirections[index]);
                activeBalls.Add(extraBall);
            }
        }

        private void TryTriggerDoubleTap(BallController sourceBall, PaddleController hitPaddle, Vector2 contactPoint)
        {
            if (!IsGameplaySimulationActive()
                || sourceBall == null
                || hitPaddle == null
                || ballSpawnService == null
                || powerUpService == null
                || !powerUpService.TryConsumeDoubleTapCharge(out var doubleTapDefinition, out var copyBallCount))
            {
                return;
            }

            ApplyActiveEffects();
            var bounceDirection = sourceBall.ResolvePaddleBounceDirection(hitPaddle, contactPoint.x);
            SpawnDoubleTapCopyBalls(contactPoint, bounceDirection, copyBallCount);
            powerUpService.ShowStatusBanner("DOUBLE TAP!", ResolvePowerUpAccentColor(doubleTapDefinition), 1.35f);
        }

        private void SpawnDoubleTapCopyBalls(Vector2 launchCenter, Vector2 bounceDirection, int copyBallCount)
        {
            var resolvedCount = Mathf.Clamp(copyBallCount, 1, 4);
            var resolvedDirection = bounceDirection.sqrMagnitude > 0.01f ? bounceDirection.normalized : Vector2.up;
            var launchDirections = powerUpService != null
                ? powerUpService.BuildMultiBallDirections(resolvedDirection, resolvedCount)
                : BuildFallbackSplitDirections(resolvedDirection, resolvedCount);
            var launchOffset = ResolveDoubleTapLaunchOffset();

            for (var index = 0; index < launchDirections.Length; index++)
            {
                var direction = launchDirections[index].sqrMagnitude > 0.01f
                    ? launchDirections[index].normalized
                    : resolvedDirection;
                var copyBall = CreateBall(false);
                copyBall.SetBaseSizeMultiplier(DoubleTapCopyBallSizeMultiplier);
                copyBall.SetWorldPosition(launchCenter + (direction * launchOffset));
                copyBall.Launch(direction);
                activeBalls.Add(copyBall);
            }
        }

        private float ResolveDoubleTapLaunchOffset()
        {
            return Mathf.Max(0.04f, ballRadius * DoubleTapCopyBallSizeMultiplier * DoubleTapLaunchOffsetMultiplier);
        }

        private void TryTriggerPrismPop(BallController sourceBall, Vector2 splitCenter, BrickDestructionCause destructionCause)
        {
            if (destructionCause != BrickDestructionCause.Impact
                || sourceBall == null
                || ballSpawnService == null
                || powerUpService == null
                || !powerUpService.TryConsumePrismPopCharge(out var prismPopDefinition, out var effectMultiplier))
            {
                return;
            }

            SpawnPrismPopCopyBall(sourceBall, splitCenter, prismPopDefinition, effectMultiplier);
            ApplyActiveEffects();
        }

        private void SpawnPrismPopCopyBall(
            BallController sourceBall,
            Vector2 splitCenter,
            PowerUpDefinition prismPopDefinition,
            float effectMultiplier)
        {
            var sourceDirection = ResolveBallTravelDirection(sourceBall);
            var copyDirection = ResolvePrismPopCopyDirection(sourceDirection);
            var copyBall = CreateBall(false);
            copyBall.SetBaseSizeMultiplier(PrismPopCopyBallSizeMultiplier);
            copyBall.SetWorldPosition(splitCenter + (copyDirection * ResolvePrismPopLaunchOffset()));
            copyBall.Launch(copyDirection);
            activeBalls.Add(copyBall);
            temporaryBallLifetimes.Add(new TemporaryBallLifetime(
                copyBall,
                ResolvePrismPopCopyLifetime(prismPopDefinition, effectMultiplier)));
            powerUpService?.ShowStatusBanner("PRISM POP!", ResolvePrismPopColor(prismPopDefinition), 1.25f);
        }

        private Vector2 ResolveBallTravelDirection(BallController ball)
        {
            if (ball == null)
            {
                return Vector2.up;
            }

            var velocity = ball.CurrentVelocity;
            return velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector2.up;
        }

        private Vector2 ResolvePrismPopCopyDirection(Vector2 sourceDirection)
        {
            var resolvedDirection = sourceDirection.sqrMagnitude > 0.01f ? sourceDirection.normalized : Vector2.up;
            var mirroredDirection = new Vector2(-resolvedDirection.x, resolvedDirection.y);

            if (Mathf.Abs(mirroredDirection.x) < PrismPopMinimumHorizontalDirection)
            {
                var horizontalSign = NextGameplayRandomBool() ? -1f : 1f;
                mirroredDirection.x = PrismPopMinimumHorizontalDirection * horizontalSign;
            }

            return mirroredDirection.sqrMagnitude > 0.01f ? mirroredDirection.normalized : Vector2.up;
        }

        private float ResolvePrismPopCopyLifetime(PowerUpDefinition definition, float effectMultiplier)
        {
            var baseLifetime = definition != null && definition.Scalar > 0f
                ? definition.Scalar
                : PrismPopFallbackCopyLifetimeSeconds;
            return Mathf.Max(0.5f, baseLifetime * Mathf.Max(0.1f, effectMultiplier));
        }

        private float ResolvePrismPopLaunchOffset()
        {
            return Mathf.Max(0.04f, ballRadius * PrismPopCopyBallSizeMultiplier * PrismPopLaunchOffsetMultiplier);
        }

        private Color ResolvePrismPopColor(PowerUpDefinition definition)
        {
            if (themeService != null && definition != null)
            {
                return themeService.ResolvePowerUpStyle(definition).PrimaryColor;
            }

            return definition != null ? definition.PickupColor : new Color(0.01f, 0.93f, 0.98f, 1f);
        }

        private void SplitBallFromExplosiveBrick(
            BallController sourceBall,
            Vector2 splitCenter,
            float speedBurstMultiplier,
            float speedBurstDuration)
        {
            if (sourceBall == null || ballSpawnService == null)
            {
                return;
            }

            var sourceBody = sourceBall.GetComponent<Rigidbody2D>();
            var sourceVelocity = sourceBody != null ? sourceBody.linearVelocity : Vector2.zero;
            var sourceDirection = sourceVelocity.sqrMagnitude > 0.01f
                ? sourceVelocity.normalized
                : Vector2.up;
            var launchDirections = powerUpService != null
                ? powerUpService.BuildMultiBallDirections(sourceDirection, ExplosiveBrickSplitBallCount)
                : BuildFallbackSplitDirections(sourceDirection, ExplosiveBrickSplitBallCount);

            if (launchDirections.Length == 0)
            {
                return;
            }

            sourceBall.SetBaseSizeMultiplier(ExplosiveBrickSplitBallSizeMultiplier);
            sourceBall.SetWorldPosition(splitCenter + (launchDirections[0] * ResolveSplitBallLaunchOffset()));
            sourceBall.ApplyCollisionResponse(launchDirections[0]);
            sourceBall.ApplySpeedBurst(speedBurstMultiplier, speedBurstDuration);

            if (!activeBalls.Contains(sourceBall))
            {
                activeBalls.Add(sourceBall);
            }

            for (var index = 1; index < launchDirections.Length; index++)
            {
                var splitBall = CreateBall(false);
                splitBall.SetBaseSizeMultiplier(ExplosiveBrickSplitBallSizeMultiplier);
                splitBall.SetWorldPosition(splitCenter + (launchDirections[index] * ResolveSplitBallLaunchOffset()));
                splitBall.Launch(launchDirections[index]);
                splitBall.ApplySpeedBurst(speedBurstMultiplier, speedBurstDuration);
                activeBalls.Add(splitBall);
            }
        }

        private float ResolveSplitBallLaunchOffset()
        {
            return Mathf.Max(0.04f, ballRadius * ExplosiveBrickSplitBallSizeMultiplier * 0.9f);
        }

        private Vector2[] BuildFallbackSplitDirections(Vector2 sourceDirection, int ballCount)
        {
            var resolvedDirection = sourceDirection.sqrMagnitude > 0.01f ? sourceDirection.normalized : Vector2.up;
            var resolvedCount = Mathf.Max(1, ballCount);
            var directions = new Vector2[resolvedCount];

            for (var index = 0; index < resolvedCount; index++)
            {
                var angle = resolvedCount == 1
                    ? 0f
                    : Mathf.Lerp(-multiBallSpreadAngle, multiBallSpreadAngle, index / (resolvedCount - 1f));
                directions[index] = (Vector2)(Quaternion.Euler(0f, 0f, angle) * resolvedDirection);
            }

            return directions;
        }

        private void UpdateTemporaryBallLifetimes()
        {
            if (!IsGameplaySimulationActive() || temporaryBallLifetimes.Count == 0)
            {
                return;
            }

            for (var index = temporaryBallLifetimes.Count - 1; index >= 0; index--)
            {
                var lifetime = temporaryBallLifetimes[index];
                var ball = lifetime.Ball;

                if (ball == null)
                {
                    temporaryBallLifetimes.RemoveAt(index);
                    continue;
                }

                lifetime.RemainingSeconds = Mathf.Max(0f, lifetime.RemainingSeconds - Time.deltaTime);

                if (lifetime.RemainingSeconds > 0f)
                {
                    continue;
                }

                ExpireTemporaryBall(index, ball);
            }
        }

        private void ExpireTemporaryBall(int lifetimeIndex, BallController ball)
        {
            temporaryBallLifetimes.RemoveAt(lifetimeIndex);

            if (ball == null)
            {
                return;
            }

            if (CountActiveBallsExcluding(ball) <= 0)
            {
                HandleBallLost(ball);
                return;
            }

            if (stickyCaughtBall == ball)
            {
                stickyCaughtBall = null;
            }

            activeBalls.Remove(ball);
            ball.Stop();
            ball.gameObject.SetActive(false);
            DestroyRuntimeObject(ball.gameObject);
        }

        private bool TryExpireMirrorServeBall(BallController ball)
        {
            if (ball == null || !mirrorServeBalls.Remove(ball))
            {
                return false;
            }

            RemoveTemporaryBallLifetime(ball);

            if (CountActiveBallsExcluding(ball) <= 0)
            {
                HandleBallLost(ball);
                return true;
            }

            if (stickyCaughtBall == ball)
            {
                stickyCaughtBall = null;
                stickyCaughtBallUsesCleanCatch = false;
                cleanCatchReleaseOffsetNormalized = 0f;
                cleanCatchReleaseAimMultiplier = 1f;
            }

            activeBalls.Remove(ball);
            ball.Stop();
            ball.gameObject.SetActive(false);
            DestroyRuntimeObject(ball.gameObject);
            return true;
        }

        private int CountActiveBallsExcluding(BallController excludedBall)
        {
            var count = 0;

            for (var index = 0; index < activeBalls.Count; index++)
            {
                var activeBall = activeBalls[index];

                if (activeBall != null && activeBall != excludedBall)
                {
                    count++;
                }
            }

            return count;
        }

        private void RemoveTemporaryBallLifetime(BallController ball)
        {
            if (ball == null)
            {
                return;
            }

            for (var index = temporaryBallLifetimes.Count - 1; index >= 0; index--)
            {
                if (temporaryBallLifetimes[index].Ball == ball)
                {
                    temporaryBallLifetimes.RemoveAt(index);
                }
            }
        }

        private void ClearPickups()
        {
            powerUpService?.ClearPickups();
        }

        private void ApplySolarShotVisualState()
        {
            var solarShotCharged = powerUpService != null && powerUpService.SolarShotCharges > 0;

            if (serveBall != null)
            {
                serveBall.SetSolarShotCharged(solarShotCharged);
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                activeBall.SetSolarShotCharged(solarShotCharged);
            }
        }

        private void ClearMissiles()
        {
            for (var index = activeMissiles.Count - 1; index >= 0; index--)
            {
                var missile = activeMissiles[index];

                if (missile == null)
                {
                    continue;
                }

                missile.gameObject.SetActive(false);
                DestroyRuntimeObject(missile.gameObject);
            }

            activeMissiles.Clear();
        }

        private void ClearTimedEffects()
        {
            powerUpService?.ClearTimedEffects();
            ApplyActiveEffects();
        }

        private void EnsureServeBallExists()
        {
            if (serveBall == null)
            {
                serveBall = CreateBall(true);
            }

            serveBall.gameObject.SetActive(true);
            serveBall.SetMovementSpeed(GetCurrentBallSpeed());
            RefreshServeBallVisualStyle();
        }

        private void RefreshServeBallVisualStyle()
        {
            serveBall?.ApplyVisualStyle(ResolveBallVisualStyle());
        }

        private void UpdateServeBallRevealDelay()
        {
            if (serveBallRevealDelayTimer <= 0f)
            {
                return;
            }

            serveBallRevealDelayTimer = Mathf.Max(0f, serveBallRevealDelayTimer - Time.unscaledDeltaTime);

            if (serveBallRevealDelayTimer > 0f)
            {
                return;
            }

            RevealServeBallForServe();
        }

        private void RevealServeBallForServe()
        {
            if (serveBall == null)
            {
                return;
            }

            serveBallRevealDelayTimer = 0f;
            serveBall.gameObject.SetActive(true);
            serveBall.SetMovementSpeed(GetCurrentBallSpeed());
            RefreshServeBallVisualStyle();
            serveBall.ResetToPaddle();
        }

        private bool ShouldDelayServeBallReveal(RoundState nextState)
        {
            return nextState == RoundState.LifeLost
                && !lastLifeLossUsedAutoSave
                && !lastLifeLossUsedSpareFuse
                && !(UsesHighScoreMode() && !UsesFiniteHighScoreLives())
                && (activeRunSettings == null || !activeRunSettings.IsTurnBasedMode);
        }

        private bool IsServeBallReadyToLaunch()
        {
            return serveBall != null
                && serveBallRevealDelayTimer <= 0f
                && serveBall.gameObject.activeInHierarchy;
        }

        private void DestroyAdditionalBalls()
        {
            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null || activeBall == serveBall)
                {
                    continue;
                }

                RemoveTemporaryBallLifetime(activeBall);
                DestroyRuntimeObject(activeBall.gameObject);
            }
        }

        private void StopAllBalls()
        {
            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                if (activeBalls[index] == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                activeBalls[index].Stop();
            }
        }

        private float GetCurrentBallSpeed()
        {
            return GetBallSpeedBase() * Mathf.Clamp(manualBallSpeedMultiplier, 1f, Mathf.Max(1f, manualBallSpeedMaxMultiplier));
        }

        private float GetModeBallSpeedMultiplier()
        {
            return activeRunSettings != null && activeRunSettings.IsRogueMode
                ? rogueRunController?.GetStageBallSpeedMultiplier(currentLevelIndex) ?? BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(currentLevelIndex)
                : 1f;
        }

        private int GetLifeLossScorePenalty()
        {
            return activeRunSettings != null && activeRunSettings.UsesLifeLossScorePenalty
                ? activeRunSettings.LifeLossScorePenalty
                : 0;
        }

        private void CompleteAndAdvanceTurnBasedTurn(BreakoutTurnSwitchReason reason)
        {
            if (activeRunSettings == null || !activeRunSettings.IsTurnBasedMode)
            {
                return;
            }

            SaveCurrentTurnBasedBrickState(reason);

            var advanceResult = turnBasedMultiplayerController != null
                ? turnBasedMultiplayerController.CompleteTurnAndAdvance(score, reason)
                : new BreakoutTurnAdvanceResult(true, null);

            if (advanceResult.IsRunComplete)
            {
                score = advanceResult.Winner?.Score ?? score;
                livesRemaining = advanceResult.Winner?.LivesRemaining ?? 0;
                lifeLossCount = advanceResult.Winner?.BallsLost ?? lifeLossCount;
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                ClearPickups();
                StopAllBalls();
                stickyCaughtBall = null;
                FinalizeActiveRunStats(completed: true);
                return;
            }

            score = turnBasedMultiplayerController?.GetCurrentPlayerScore() ?? 0;
            livesRemaining = turnBasedMultiplayerController?.GetCurrentPlayerLivesRemaining() ?? activeRunSettings.StartingLives;
            lifeLossCount = turnBasedMultiplayerController?.CurrentPlayer?.BallsLost ?? 0;
            currentLevelIndex = turnBasedMultiplayerController?.GetCurrentPlayerLevelIndex() ?? currentLevelIndex;
            LoadLevel(currentLevelIndex, RoundState.LifeLost);
        }

        private void SaveCurrentTurnBasedBrickState(BreakoutTurnSwitchReason reason)
        {
            if (turnBasedMultiplayerController == null
                || currentLevel == null
                || reason == BreakoutTurnSwitchReason.LevelCleared)
            {
                return;
            }

            turnBasedMultiplayerController.SaveCurrentPlayerBrickState(
                currentLevelIndex,
                brickService?.CaptureBrickStates() ?? Array.Empty<BreakoutBrickState>());
        }

        private bool UsesHighScoreMode()
        {
            return activeRunSettings != null && activeRunSettings.ScoringMode == RunScoringMode.HighScore;
        }

        private bool UsesFiniteHighScoreLives()
        {
            return activeRunSettings != null && activeRunSettings.IsSoloMarathonMode;
        }

        private void RecordRogueRunResult(bool completed)
        {
            if (!ShouldRecordRogueRunResult(activeRunResultRecorded, isDeveloperRunActive, activeRunSettings))
            {
                return;
            }

            activeRunResultRecorded = true;
            var result = rogueRunController != null
                ? rogueRunController.BuildResult(activeRunSettings, completed, currentLevelIndex + 1, score)
                : BreakoutRogueRunResultStore.BuildResult(
                    activeRunSettings,
                    completed,
                    currentLevelIndex + 1,
                    DefaultRoguePaddleLabel,
                    score);
            BreakoutRogueRunResultStore.Save(result);
        }

        private void RecordSoloMarathonResult()
        {
            if (activeSoloMarathonResultRecorded
                || activeRunSettings == null
                || !activeRunSettings.IsSoloMarathonMode)
            {
                return;
            }

            activeSoloMarathonResultRecorded = true;
            activeSoloMarathonNewHighScore = BreakoutSoloMarathonScoreStore.TrySaveBest(
                selectedSoloMarathonDifficulty,
                activeRunSettings.DifficultyLabel,
                score,
                currentLevelIndex + 1,
                activeRunSettings.Seed,
                out activeSoloMarathonRecord);
        }

        internal static bool ShouldRecordRogueRunResult(bool resultAlreadyRecorded, bool developerRunActive, RunSettings settings)
        {
            return !resultAlreadyRecorded
                && !developerRunActive
                && settings != null
                && settings.IsRogueMode;
        }

        internal static bool ShouldRebuildRogueRunOnRestart(RunSettings settings)
        {
            return settings != null && settings.IsRogueMode;
        }

        internal static bool ShouldAutoSaveLastBall(RunSettings settings, int currentScore)
        {
            return settings != null
                && settings.IsRogueMode
                && settings.RogueIntensity <= AutoSaveMaximumRogueHeat
                && currentScore >= GetAutoSaveScoreCost(settings);
        }

        internal static int GetAutoSaveScoreCost(RunSettings settings)
        {
            var intensity = settings != null
                ? BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity)
                : BreakoutRunProgression.MinRogueIntensity;
            var pricedHeatCount = Mathf.Max(0, intensity - AutoSaveBaseCostMaximumRogueHeat);
            return AutoSaveBaseScoreCost + (pricedHeatCount * AutoSaveScoreCostIncreasePerHeat);
        }

        private void ApplyLifeLossScorePenalty()
        {
            var penalty = GetLifeLossScorePenalty();

            if (penalty <= 0)
            {
                return;
            }

            score -= penalty;
        }

        private BreakoutScoreContext BuildScoreContext()
        {
            return new BreakoutScoreContext(
                activeBalls.Count,
                GetDisplayedBallSpeed(),
                ballSpeed,
                Time.time,
                GetActiveScoreMultiplier());
        }

        private float GetActiveScoreMultiplier()
        {
            var glitchMultiplier = activeLevelGlitchPlan != null && activeLevelGlitchPlan.IsActive
                ? activeLevelGlitchPlan.ScoreMultiplier
                : 1f;
            var marathonHeatMultiplier = activeRunSettings != null && activeRunSettings.IsSoloMarathonMode
                ? GetSoloMarathonHeatScoreMultiplier(selectedSoloMarathonDifficulty)
                : 1f;
            return Mathf.Max(0.1f, activeEffectModifiers.ScoreMultiplier)
                * Mathf.Max(1f, glitchMultiplier)
                * Mathf.Max(1f, marathonHeatMultiplier);
        }

        private float GetDisplayedBallSpeed()
        {
            var highestActiveSpeed = 0f;

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                highestActiveSpeed = Mathf.Max(highestActiveSpeed, activeBall.CurrentSpeed);
            }

            return highestActiveSpeed > 0.01f ? highestActiveSpeed : GetCurrentBallSpeed();
        }

        private float GetBallSpeedBase()
        {
            return currentLevelBallSpeed * Mathf.Clamp(GetTimedBallSpeedMultiplier(), 0.6f, 1.75f);
        }

        private Color ResolveComboPopupColor()
        {
            return themeService != null
                ? themeService.ResolveBallStyle().PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private Color ResolveCapsuleMadnessColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBurst, ballColor, ballColor, squareSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private Color ResolveAutoSaveColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBeneficial,
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    squareSprite).PrimaryColor
                : new Color(0.45f, 0.95f, 0.72f, 1f);
        }

        private Color ResolveSpareFuseColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(1f, 0.87f, 0.36f, 1f),
                    squareSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private Color ResolveTiltWarningColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBeneficial,
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    squareSprite).PrimaryColor
                : new Color(0.45f, 0.95f, 0.72f, 1f);
        }

        private Color ResolveTiltNudgeColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(1f, 0.49f, 0.86f, 1f),
                    squareSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private Color ResolveTiltAlarmColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupHarmful,
                    new Color(0.99f, 0.27f, 0.31f, 1f),
                    new Color(1f, 0.49f, 0.86f, 1f),
                    squareSprite).PrimaryColor
                : new Color(0.99f, 0.27f, 0.31f, 1f);
        }

        private float GetMaximumBallSpeed()
        {
            return GetBallSpeedBase() * Mathf.Max(1f, manualBallSpeedMaxMultiplier);
        }

        private float GetTimedBallSpeedMultiplier()
        {
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            return powerUpService != null
                ? powerUpService.CalculateEffectModifiers(
                    (activeRunSettings?.PaddleWidthMultiplier ?? 1f) * persistentModifiers.PaddleWidthMultiplier,
                    persistentModifiers.WavyPaddleStrength).TimedBallSpeedMultiplier
                : 1f;
        }

        private bool TryOpenUpgradeDraft()
        {
            if (activeRunState == null || upgradeDraftService == null || activeRunSettings == null)
            {
                return false;
            }

            if (activeRunSettings.IsSoloMarathonMode)
            {
                return false;
            }

            activeRunState.RegisterLevelClear();
            if (activeRunSettings.IsRogueMode)
            {
                rogueRunController?.UnlockHazardsForClearedLevel(activeRunState, activeRunSettings);
            }

            var offers = activeRunSettings.IsRogueMode && rogueRunController != null
                ? rogueRunController.GenerateDraft(activeRunState, activeRunSettings, currentLevelIndex)
                : upgradeDraftService.GenerateDraft(activeRunState, activeRunSettings, activeRunSettings.Seed, currentLevelIndex, 3);

            if (offers.Length == 0)
            {
                activeRunState.ClearPendingDraftOffers();
                return false;
            }

            activeRunState.SetPendingDraftOffers(offers);
            roundState = RoundState.UpgradeDraft;
            selectedUpgradeDraftIndex = 0;
            rewardMissileBonusPurchasesUsedThisDraft = 0;
            return true;
        }

        private void HandleUpgradeDraftChoice(int optionIndex)
        {
            if (activeRunState == null)
            {
                return;
            }

            selectedUpgradeDraftIndex = optionIndex;

            if (!activeRunState.TryApplyPendingDraftOffer(optionIndex, out var appliedOffer))
            {
                return;
            }

            var appliedUpgrade = appliedOffer.UpgradeDefinition;

            if (!UsesHighScoreMode() && appliedUpgrade != null && appliedUpgrade.BonusLives > 0)
            {
                livesRemaining += appliedUpgrade.BonusLives;
            }

            ApplyActiveEffects();
            ShowRunDraftRewardBanner(appliedOffer);

            if (HasNextLevel())
            {
                LoadLevel(currentLevelIndex + 1, RoundState.ReadyToServe);
            }
            else
            {
                roundState = RoundState.LevelComplete;
                selectedOverlayActionIndex = 0;
                FinalizeActiveRunStats(completed: true);
            }
        }

        private void TryPurchaseRewardMissile()
        {
            if (!CanPurchaseMissile(score))
            {
                powerUpService?.ShowStatusBanner("MISSILE NEEDS 5000", ResolveMissileColor(), 1.6f);
                return;
            }

            score -= MissileRewardPurchaseScoreCost;
            var stockGranted = GetRewardMissilePurchaseStockGrant();
            availableMissiles += stockGranted;
            if (stockGranted > 1)
            {
                rewardMissileBonusPurchasesUsedThisDraft++;
            }

            powerUpService?.ShowStatusBanner(stockGranted > 1 ? $"+{stockGranted} MISSILES" : "+ MISSILE", ResolveMissileColor(), 1.6f);
            audioService?.PlayPickupCollected(null);
        }

        internal static bool CanPurchaseMissile(int currentScore)
        {
            return currentScore >= MissileRewardPurchaseScoreCost;
        }

        private BreakoutRunUpgradeModifiers GetPersistentRunUpgradeModifiers()
        {
            return activeRunState != null
                ? activeRunState.CalculateModifiers()
                : new BreakoutRunUpgradeModifiers(1f, 1f, 1f, 1f, 0f, 0f, 1f, 0, 0, 0, 0, 0, 0f, 0, 0f);
        }

        private int GetEffectiveBallsPerServe()
        {
            var baseBallsPerServe = activeRunSettings == null ? 1 : activeRunSettings.BallsPerServe;
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            return Mathf.Clamp(baseBallsPerServe + persistentModifiers.ExtraBallsPerServe, 1, 6);
        }

        private int GetEffectiveFreeMissileShotsPerLevel()
        {
            return Mathf.Max(0, GetPersistentRunUpgradeModifiers().FreeMissileShotsPerLevel);
        }

        private bool HasFreeMissileShotAvailable()
        {
            return freeMissilesFiredThisLevel < GetEffectiveFreeMissileShotsPerLevel();
        }

        private int GetRewardMissilePurchaseStockGrant()
        {
            var bonusStock = GetPersistentRunUpgradeModifiers().RewardMissilePurchaseBonusStock;
            return 1 + (rewardMissileBonusPurchasesUsedThisDraft <= 0 ? Mathf.Max(0, bonusStock) : 0);
        }

        private int GetEffectiveTiltWarningSavesPerLevel()
        {
            return Mathf.Clamp(GetPersistentRunUpgradeModifiers().TiltWarningSavesPerLevel, 0, 3);
        }

        private int GetRemainingSpareFuseSaves()
        {
            return Mathf.Clamp(GetPersistentRunUpgradeModifiers().SpareFuseSavesPerRun - spareFuseSavesUsed, 0, 3);
        }

        private float GetEffectiveDropChanceMultiplier()
        {
            var baseMultiplier = activeRunSettings?.DropChanceMultiplier ?? 1f;
            var tokenStormMultiplier = activeLevelGlitchPlan != null
                && activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TokenStorm)
                ? activeLevelGlitchPlan.TokenStorm.DropChanceMultiplier
                : 1f;
            return Mathf.Clamp(
                baseMultiplier * GetPersistentRunUpgradeModifiers().DropChanceMultiplier * tokenStormMultiplier,
                0f,
                3f);
        }

        private float GetEffectivePickupFallSpeedMultiplier()
        {
            var activePickupMultiplier = activeEffectModifiers.PickupFallSpeedMultiplier > 0f
                ? activeEffectModifiers.PickupFallSpeedMultiplier
                : 1f;
            return Mathf.Clamp(
                GetPersistentRunUpgradeModifiers().PickupFallSpeedMultiplier * activePickupMultiplier,
                0.35f,
                1.5f);
        }

        private void ApplyTokenStormFallSpeed(PowerUpPickup pickup)
        {
            if (pickup == null
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.TokenStorm))
            {
                return;
            }

            var tokenStorm = activeLevelGlitchPlan.TokenStorm;
            var fallSpeedMultiplier = NextGameplayRandomFloat(
                tokenStorm.MinimumFallSpeedMultiplier,
                tokenStorm.MaximumFallSpeedMultiplier);
            pickup.MultiplyFallSpeed(fallSpeedMultiplier);
        }

        private void ApplyDropTideWaveDelay(PowerUpPickup pickup)
        {
            if (pickup == null || !IsDropTideActive())
            {
                return;
            }

            var delaySeconds = BreakoutLevelGlitchPlanner.CalculateDropTideReleaseDelay(
                activeLevelElapsedSeconds,
                activeLevelGlitchPlan.DropTide);
            pickup.DelayFall(delaySeconds);
        }

        private void ApplyPickupPinballMotion(PowerUpPickup pickup)
        {
            if (pickup == null || !IsPickupPinballActive())
            {
                return;
            }

            var directionSign = NextGameplayRandomFloat(0f, 1f) < 0.5f ? -1f : 1f;
            pickup.EnablePinball(
                Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop),
                directionSign,
                activeLevelGlitchPlan.PickupPinball);
        }

        private void TriggerCapsuleBlackoutFromPickupCatch()
        {
            if (!IsCapsuleBlackoutActive())
            {
                return;
            }

            powerUpService?.TriggerCapsuleBlackout(activeLevelGlitchPlan.CapsuleBlackout);
            powerUpService?.ShowStatusBanner("DROP BLACKOUT!", new Color(1f, 0.87f, 0.36f, 1f), 1.15f);
        }

        private void ApplyCapsuleBlackout(PowerUpPickup pickup)
        {
            if (pickup == null || !IsCapsuleBlackoutActive())
            {
                return;
            }

            powerUpService?.ApplyCapsuleBlackoutToPickup(pickup);
        }

        private void UpdateActiveLevelElapsedSeconds()
        {
            if (IsGameplaySimulationActive())
            {
                activeLevelElapsedSeconds += Mathf.Max(0f, Time.deltaTime);
            }
        }

        private void UpdateJammedRailsPulse()
        {
            if (paddle == null || activeLevelGlitchPlan == null || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.JammedRails))
            {
                return;
            }

            var baseWidthMultiplier = Mathf.Max(0.1f, activeEffectModifiers.PaddleWidthMultiplier);
            var jammedWidthMultiplier = BreakoutJammedRailsCalculator.CalculateWidthMultiplier(
                activeLevelElapsedSeconds,
                activeLevelGlitchPlan.JammedRails);
            paddle.SetWidthMultiplier(baseWidthMultiplier * jammedWidthMultiplier);
        }

        private float GetEffectiveBrickMagnetStrength()
        {
            var brickAttractionStrength = Mathf.Max(
                activeEffectModifiers.BrickMagnetStrength,
                GetPersistentRunUpgradeModifiers().BrickMagnetStrength);

            return Mathf.Clamp(
                brickAttractionStrength - activeEffectModifiers.BrickRepulsionStrength,
                -1f,
                1f);
        }

        private float GetEffectiveSpecialBrickEffectMultiplier()
        {
            return Mathf.Clamp(GetPersistentRunUpgradeModifiers().SpecialBrickEffectMultiplier, 1f, 2f);
        }

        private int GetChosenUpgradeCount()
        {
            return activeRunState == null
                ? 0
                : activeRunState.ChosenUpgrades.Count + activeRunState.ChosenDropUnlocks.Count;
        }

        private string BuildDraftOfferTitle(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return "Missing Reward";
            }

            return offer.Kind == BreakoutRunDraftOfferKind.DropUnlock
                ? $"Drop Unlock: {offer.DisplayName}"
                : offer.DisplayName;
        }

        private string BuildDraftOfferDescription(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return "This draft slot failed to load.";
            }

            if (offer.Kind == BreakoutRunDraftOfferKind.RunUpgrade)
            {
                return offer.UpgradeDefinition != null ? offer.UpgradeDefinition.Description : "This draft slot failed to load.";
            }

            var dropUnlock = offer.DropUnlockDefinition;
            return dropUnlock != null
                ? $"Adds {dropUnlock.RarityLabel} {dropUnlock.DisplayName} capsules to this Neon Ladder run's drop pool."
                : "This drop unlock failed to load.";
        }

        private string BuildDropUnlockDetail(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return "Drop Unlock | Unavailable";
            }

            var polarity = definition.IsBeneficial ? "Helpful Drop" : "Hazard Drop";
            return $"Drop Unlock | {definition.RarityLabel} | {polarity} | {BuildPowerUpMechanicalSummary(definition)}";
        }

        private string BuildPowerUpMechanicalSummary(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return "Unavailable";
            }

            return definition.EffectType switch
            {
                PowerUpEffectType.PaddleWidthMultiplier => $"Paddle x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PaddleSpeedMultiplier => $"Paddle speed x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BallSpeedMultiplier => $"Ball x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BallSizeMultiplier => $"Ball size x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MultiBallBurst => $"+{Mathf.Max(1, definition.ExtraBallCount)} balls",
                PowerUpEffectType.WavyPaddle => $"Wave {definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PaddleHitTilt => $"Paddle tilts {definition.Scalar:0.#}deg per hit for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PaddleWrap => $"Paddle wraps sides for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.StickyPaddle => $"Catch ball for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.CleanCatch => $"Catch next paddle hit, aim x{definition.Scalar:0.00}",
                PowerUpEffectType.LaserPaddle => $"Laser paddle for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ShieldWall => "Shield save",
                PowerUpEffectType.RewindCatch => "Rewind next missed ball",
                PowerUpEffectType.PhaseBall => $"Phase ball for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ChainLightning => $"Chain hits for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ActiveDropMultiplier => $"Active effects x{definition.Scalar:0.00}",
                PowerUpEffectType.BrickMagnet => $"Brick pull for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MagnetFlip => $"Brick repel for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ScoreMultiplier => $"Score x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PaddleClone => $"Clone rail for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MirrorImagePaddle => $"Mirror rail for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BrickJammer => $"Brick jam for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.HotPotatoBall => $"Ball x{definition.Scalar:0.00}, score x{definition.Scalar:0.00}",
                PowerUpEffectType.JackpotJam => $"Score x{definition.Scalar:0.00}, ball x{BreakoutPowerUpService.JackpotJamBallSpeedMultiplier:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MicroSpark => $"Ball size x{definition.Scalar:0.00}, score x{definition.SecondaryScalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.OverdriveTape => $"Ball, paddle, and capsules x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.DoubleTap => $"Next paddle hit adds {Mathf.Max(1, definition.ExtraBallCount)} copy balls; paddle x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ExplosiveBall => $"Explodes bricks for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.FinalBreakthru => $"Weak pierce, explosions, score x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.VectorSight => $"Aim preview for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.CapsuleMagnet => $"Helpful capsules drift for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BankBonus => $"+{Mathf.Max(1, Mathf.RoundToInt(definition.Scalar))}/wall bank for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PrismPop => $"Next brick hit splits a {definition.Scalar:0.#}s copy ball",
                PowerUpEffectType.SolarShot => "Next weak brick burns through",
                PowerUpEffectType.FuseBurst => $"Clears damaged/weak brick; blackout for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MissileStock => $"+{Mathf.Max(1, definition.ExtraBallCount > 0 ? definition.ExtraBallCount : Mathf.RoundToInt(definition.Scalar))} missile stock",
                PowerUpEffectType.BogusBounce => $"Next {Mathf.Max(1, definition.ExtraBallCount > 0 ? definition.ExtraBallCount : BreakoutPowerUpService.BogusBounceDefaultCharges)} wall bounces go wild",
                PowerUpEffectType.CabinetJackpot => "Refresh all active timed effects",
                PowerUpEffectType.RandomHarmfulDrop => "Disguised random hazard",
                PowerUpEffectType.RandomMixedDrop => "Random helpful drop and hazard",
                _ => $"{definition.HudLabel} for {definition.DurationSeconds:0.#}s",
            };
        }

        private static bool IsSolarShotVulnerable(Brick brick)
        {
            return brick != null
                && brick.Definition != null
                && brick.Definition.IsBreakable
                && brick.HitPointsRemaining <= 1;
        }

        private Color ResolveSolarShotColor()
        {
            return new Color(1f, 0.84f, 0.28f, 1f);
        }

        private bool TryTriggerFuseBurst()
        {
            if (brickEffectResolver == null
                || !brickEffectResolver.TryResolveFuseBurstTarget(bricks, out var target)
                || target == null)
            {
                return false;
            }

            var burstPosition = (Vector2)target.transform.position;
            target.ApplyEffectHit(null, BrickDestructionCause.FuseBurst, target.HitPointsRemaining);
            SpawnExplosionVisual(burstPosition, FuseBurstPulseRadius);
            powerUpService?.ShowStatusBanner("FUSE BURST!", ResolveFuseBurstColor(), 1.45f);
            return true;
        }

        private Color ResolveFuseBurstColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(1f, 0.28f, 0.66f, 1f),
                    powerUpSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private static float ResolveExplosiveBallExplosionRadius(float strength)
        {
            return Mathf.Lerp(ExplosiveBallMinimumRadius, ExplosiveBallMaximumRadius, Mathf.Clamp01(strength / 2f));
        }

        private static float ResolveExplosiveBallSpeedBurstMultiplier(float strength)
        {
            return Mathf.Lerp(
                ExplosiveBallMinimumSpeedBurstMultiplier,
                ExplosiveBallMaximumSpeedBurstMultiplier,
                Mathf.Clamp01(strength / 2f));
        }

        private string BuildUpgradeMechanicalSummary(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return "Unavailable";
            }

            var parts = new List<string>();

            if (!Mathf.Approximately(upgrade.PaddleWidthMultiplier, 1f))
            {
                parts.Add($"Paddle x{upgrade.PaddleWidthMultiplier:0.00}");
            }

            if (!Mathf.Approximately(upgrade.BallSpeedMultiplier, 1f))
            {
                parts.Add($"Ball x{upgrade.BallSpeedMultiplier:0.00}");
            }

            if (!Mathf.Approximately(upgrade.DropChanceMultiplier, 1f))
            {
                parts.Add($"Drops x{upgrade.DropChanceMultiplier:0.00}");
            }

            if (!Mathf.Approximately(upgrade.PickupFallSpeedMultiplier, 1f))
            {
                parts.Add($"Pickup fall x{upgrade.PickupFallSpeedMultiplier:0.00}");
            }

            if (upgrade.ExtraBallsPerServe > 0)
            {
                parts.Add($"+{upgrade.ExtraBallsPerServe} ball/serve");
            }

            if (upgrade.BonusLives > 0)
            {
                parts.Add($"+{upgrade.BonusLives} life");
            }

            if (upgrade.SpareFuseSavesPerRun > 0)
            {
                parts.Add($"Last-life save x{upgrade.SpareFuseSavesPerRun}");
            }

            if (upgrade.FreeMissileShotsPerLevel > 0)
            {
                parts.Add($"Free missile x{upgrade.FreeMissileShotsPerLevel}/level");
            }

            if (upgrade.RewardMissilePurchaseBonusStock > 0)
            {
                parts.Add($"+{upgrade.RewardMissilePurchaseBonusStock} missile buy bonus");
            }

            if (upgrade.RiskRebateHelpfulEffectExtensionSeconds > 0f)
            {
                parts.Add($"+{upgrade.RiskRebateHelpfulEffectExtensionSeconds:0.#}s risk rebate");
            }

            if (upgrade.MaxActiveHazardTimedEffectStacks > 0)
            {
                parts.Add($"Hazard stacks max {upgrade.MaxActiveHazardTimedEffectStacks}");
            }

            if (upgrade.WarpHandleSpeedTrimRatio > 0.001f)
            {
                parts.Add($"Warp trim {upgrade.WarpHandleSpeedTrimRatio * 100f:0}%/level");
            }

            if (upgrade.WavyPaddleStrength > 0.001f)
            {
                parts.Add($"Wave {upgrade.WavyPaddleStrength:0.00}");
            }

            if (upgrade.BrickMagnetStrength > 0.001f)
            {
                parts.Add($"Brick pull {upgrade.BrickMagnetStrength:0.00}");
            }

            if (!Mathf.Approximately(upgrade.SpecialBrickEffectMultiplier, 1f))
            {
                parts.Add($"Brick FX x{upgrade.SpecialBrickEffectMultiplier:0.00}");
            }

            return parts.Count == 0 ? "Passive build mod" : string.Join(" | ", parts);
        }

        private static float ResolveSpecialBrickSpeedBurstMultiplier(float speedBurstMultiplier, float specialBrickEffectMultiplier)
        {
            return 1f + ((Mathf.Max(1f, speedBurstMultiplier) - 1f) * Mathf.Max(1f, specialBrickEffectMultiplier));
        }

        private Color ResolveRunUpgradeAccentColor(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return Color.white;
            }

            return themeService != null
                ? themeService.ResolveThemeStyle(upgrade.ThemeSlot, upgrade.AccentColor, upgrade.AccentColor, squareSprite).PrimaryColor
                : upgrade.AccentColor;
        }

        private Sprite ResolveRunUpgradeIcon(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return null;
            }

            var resourcePath = upgrade.ResolveIconSpriteResourcePath();

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            if (!runUpgradeSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            {
                cachedSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(resourcePath);
                runUpgradeSpriteCache[resourcePath] = cachedSprite;
            }

            return cachedSprite;
        }

        private Color ResolvePowerUpAccentColor(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return Color.white;
            }

            return themeService != null
                ? themeService.ResolvePowerUpStyle(definition).PrimaryColor
                : definition.PickupColor;
        }

        private Sprite ResolvePowerUpIcon(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return powerUpSprite != null ? powerUpSprite : squareSprite;
            }

            var resourcePath = definition.ResolvePickupSpriteResourcePath();

            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                if (!powerUpIconSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
                {
                    cachedSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(resourcePath);
                    powerUpIconSpriteCache[resourcePath] = cachedSprite;
                }

                if (cachedSprite != null)
                {
                    return cachedSprite;
                }
            }

            var styledSprite = themeService != null
                ? themeService.ResolvePowerUpStyle(definition).Sprite
                : null;

            if (styledSprite != null)
            {
                return styledSprite;
            }

            return powerUpSprite != null ? powerUpSprite : squareSprite;
        }

        private Color ResolveDraftOfferAccentColor(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return Color.white;
            }

            return offer.Kind == BreakoutRunDraftOfferKind.DropUnlock
                ? ResolvePowerUpAccentColor(offer.DropUnlockDefinition)
                : ResolveRunUpgradeAccentColor(offer.UpgradeDefinition);
        }

        private Sprite ResolveDraftOfferIcon(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return null;
            }

            var icon = offer.Kind == BreakoutRunDraftOfferKind.DropUnlock
                ? ResolvePowerUpIcon(offer.DropUnlockDefinition)
                : ResolveRunUpgradeIcon(offer.UpgradeDefinition);
            return icon == squareSprite ? null : icon;
        }

        private static string ResolveDraftOfferIconLabel(BreakoutRunDraftOffer offer)
        {
            if (offer == null)
            {
                return string.Empty;
            }

            var label = offer.Kind == BreakoutRunDraftOfferKind.DropUnlock
                ? offer.DropUnlockDefinition?.HudLabel
                : offer.UpgradeDefinition?.HudLabel;

            if (string.IsNullOrWhiteSpace(label))
            {
                label = offer.DisplayName;
            }

            return BuildIconFallbackLabel(label);
        }

        private static string BuildIconFallbackLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            var normalized = label.Trim().ToUpperInvariant();

            if (normalized.Length <= 5)
            {
                return normalized;
            }

            var builder = new System.Text.StringBuilder(5);
            var takeNext = true;

            for (var index = 0; index < normalized.Length && builder.Length < 5; index++)
            {
                var current = normalized[index];

                if (char.IsLetterOrDigit(current))
                {
                    if (takeNext)
                    {
                        builder.Append(current);
                    }

                    takeNext = false;
                    continue;
                }

                takeNext = true;
            }

            if (builder.Length > 0)
            {
                return builder.ToString();
            }

            return normalized.Substring(0, Mathf.Min(5, normalized.Length));
        }

        private void ShowRunDraftRewardBanner(BreakoutRunDraftOffer offer)
        {
            if (offer == null || powerUpService == null)
            {
                return;
            }

            if (offer.Kind == BreakoutRunDraftOfferKind.DropUnlock)
            {
                powerUpService.ShowStatusBanner($"+ DROP {offer.DisplayName}", ResolvePowerUpAccentColor(offer.DropUnlockDefinition), 2.2f);
                return;
            }

            powerUpService.ShowStatusBanner($"+ BUILD {offer.DisplayName}", ResolveRunUpgradeAccentColor(offer.UpgradeDefinition), 2.2f);
        }

        private bool ReleaseStickyCaughtBall()
        {
            if (stickyCaughtBall == null)
            {
                return false;
            }

            var releasedBall = stickyCaughtBall;
            var releaseUsesCleanCatch = stickyCaughtBallUsesCleanCatch;
            var releaseOffset = cleanCatchReleaseOffsetNormalized;
            var releaseAimMultiplier = cleanCatchReleaseAimMultiplier;
            stickyCaughtBall = null;
            stickyCaughtBallUsesCleanCatch = false;
            cleanCatchReleaseOffsetNormalized = 0f;
            cleanCatchReleaseAimMultiplier = 1f;

            if (releasedBall != null)
            {
                if (releaseUsesCleanCatch)
                {
                    releasedBall.LaunchFromPaddleAim(releaseOffset, releaseAimMultiplier);
                }
                else
                {
                    releasedBall.Launch();
                }
            }

            return true;
        }

        private static float ResolvePaddleHitOffset(PaddleController hitPaddle, float contactWorldX)
        {
            if (hitPaddle == null || hitPaddle.HalfWidthWorld <= 0.001f)
            {
                return 0f;
            }

            return Mathf.Clamp(
                (contactWorldX - hitPaddle.transform.position.x) / hitPaddle.HalfWidthWorld,
                -1f,
                1f);
        }

        private bool FireLaserVolley()
        {
            if (!IsGameplaySimulationActive()
                || paddle == null
                || !activeEffectModifiers.LaserPaddleEnabled
                || laserShotCooldownTimer > 0f)
            {
                return false;
            }

            if (!brickEffectResolver.TryResolveLaserTargets(
                    bricks,
                    paddle.transform.position,
                    paddle.HalfWidthWorld,
                    out var leftTarget,
                    out var rightTarget))
            {
                return false;
            }

            var scoringBall = ResolvePrimaryScoringBall();

            if (leftTarget != null)
            {
                SpawnLaserBeamVisual(leftTarget, true);
                leftTarget.ApplyEffectHit(scoringBall, BrickDestructionCause.Laser, 1);
            }

            if (rightTarget != null)
            {
                SpawnLaserBeamVisual(rightTarget, false);
                rightTarget.ApplyEffectHit(scoringBall, BrickDestructionCause.Laser, 1);
            }

            laserShotCooldownTimer = LaserShotCooldownSeconds;
            return true;
        }

        private void UpdateLaserRain()
        {
            if (!IsGameplaySimulationActive()
                || activeLevelGlitchPlan == null
                || !activeLevelGlitchPlan.HasGlitch(BreakoutLevelGlitchType.LaserRain))
            {
                ClearPendingLaserRainLanes();
                laserRainStrikeTimer = 0f;
                return;
            }

            var deltaTime = Time.deltaTime;

            for (var index = pendingLaserRainLanes.Count - 1; index >= 0; index--)
            {
                var lane = pendingLaserRainLanes[index];

                if (lane == null)
                {
                    pendingLaserRainLanes.RemoveAt(index);
                    continue;
                }

                lane.WarningTimer -= deltaTime;

                if (lane.WarningTimer > 0f)
                {
                    continue;
                }

                StrikeLaserRainLane(lane.WorldX);
                pendingLaserRainLanes.RemoveAt(index);
            }

            laserRainStrikeTimer = Mathf.Max(0f, laserRainStrikeTimer - deltaTime);

            if (laserRainStrikeTimer > 0f)
            {
                return;
            }

            QueueLaserRainVolley();
            laserRainStrikeTimer = ResolveLaserRainIntervalSeconds();
        }

        private void QueueLaserRainVolley()
        {
            var laneCount = ResolveLaserRainLaneCount();
            var queuedXs = new List<float>(laneCount);

            for (var index = 0; index < laneCount; index++)
            {
                var laneX = ResolveLaserRainLaneX(queuedXs);
                queuedXs.Add(laneX);
                pendingLaserRainLanes.Add(new PendingLaserRainLane(laneX, SpawnLaserRainLaneVisual(laneX)));
            }
        }

        private void StrikeLaserRainLane(float laneX)
        {
            var scoringBall = ResolvePrimaryScoringBall();
            var hitRadius = LaserRainLaneHalfWidth + LaserRainBrickHitPadding;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null
                    || brick.Definition == null
                    || !brick.Definition.IsBreakable
                    || brick.IsPendingRemoval)
                {
                    continue;
                }

                if (Mathf.Abs(brick.transform.position.x - laneX) > hitRadius)
                {
                    continue;
                }

                brick.ApplyEffectHit(scoringBall, BrickDestructionCause.Laser, 1);
            }

            BounceBallsCaughtInLaserRain(laneX);
        }

        private void BounceBallsCaughtInLaserRain(float laneX)
        {
            BounceBallCaughtInLaserRain(serveBall, laneX);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var ball = activeBalls[index];

                if (ball == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                BounceBallCaughtInLaserRain(ball, laneX);
            }
        }

        private void BounceBallCaughtInLaserRain(BallController ball, float laneX)
        {
            if (ball == null || !ball.HasLaunched)
            {
                return;
            }

            var ballPosition = (Vector2)ball.transform.position;

            if (Mathf.Abs(ballPosition.x - laneX) > LaserRainLaneHalfWidth + ballRadius + LaserRainBallHitPadding)
            {
                return;
            }

            var currentVelocity = ball.CurrentVelocity;
            var currentDirection = currentVelocity.sqrMagnitude > 0.001f ? currentVelocity.normalized : Vector2.up;
            var sideSign = Mathf.Sign(ballPosition.x - laneX);

            if (Mathf.Approximately(sideSign, 0f))
            {
                sideSign = Mathf.Sign(currentDirection.x);
            }

            if (Mathf.Approximately(sideSign, 0f))
            {
                sideSign = 1f;
            }

            var bounceDirection = new Vector2(
                currentDirection.x + (sideSign * LaserRainBallSideKick),
                Mathf.Abs(currentDirection.y) + LaserRainBallVerticalKick);
            ball.ApplyCollisionResponse(bounceDirection, LaserRainBallMinimumVertical);
        }

        private BreakoutLaserRainLaneVisual SpawnLaserRainLaneVisual(float laneX)
        {
            if (effectsRoot == null && runtimeRoot == null)
            {
                return null;
            }

            var laneObject = new GameObject("Laser Rain Lane");
            laneObject.transform.SetParent(effectsRoot != null ? effectsRoot : runtimeRoot, false);
            var visual = laneObject.AddComponent<BreakoutLaserRainLaneVisual>();
            visual.Configure(
                laneX,
                arenaBottom + 0.18f,
                arenaTop - 0.1f,
                LaserRainLaneHalfWidth * 2f,
                LaserRainWarningSeconds,
                LaserRainBeamLifetimeSeconds,
                additiveLineMaterial,
                ResolveLaserRainWarningColor(),
                ResolveLaserBeamCoreColor(),
                ResolveLaserRainGlowColor());
            return visual;
        }

        private void ClearPendingLaserRainLanes()
        {
            for (var index = pendingLaserRainLanes.Count - 1; index >= 0; index--)
            {
                var lane = pendingLaserRainLanes[index];

                if (lane?.Visual != null)
                {
                    DestroyRuntimeObject(lane.Visual.gameObject);
                }
            }

            pendingLaserRainLanes.Clear();
        }

        private float ResolveLaserRainLaneX(List<float> queuedXs)
        {
            var minimumX = arenaLeft + LaserRainLaneHorizontalPadding;
            var maximumX = arenaRight - LaserRainLaneHorizontalPadding;

            if (maximumX <= minimumX)
            {
                return (arenaLeft + arenaRight) * 0.5f;
            }

            for (var attempt = 0; attempt < 8; attempt++)
            {
                var candidate = NextGameplayRandomFloat(minimumX, maximumX);

                if (!IsLaserRainLaneTooClose(candidate, queuedXs))
                {
                    return candidate;
                }
            }

            var fallbackRatio = (queuedXs.Count + 1f) / (queuedXs.Count + 2f);
            return Mathf.Lerp(minimumX, maximumX, fallbackRatio);
        }

        private static bool IsLaserRainLaneTooClose(float candidateX, List<float> queuedXs)
        {
            if (queuedXs == null)
            {
                return false;
            }

            for (var index = 0; index < queuedXs.Count; index++)
            {
                if (Mathf.Abs(candidateX - queuedXs[index]) < LaserRainLaneHalfWidth * 3.2f)
                {
                    return true;
                }
            }

            return false;
        }

        private static int ResolveLaserRainLaneCount()
        {
            return Mathf.Clamp(2, 1, LaserRainMaximumLaneCount);
        }

        private static float ResolveLaserRainIntervalSeconds()
        {
            return Mathf.Max(LaserRainMinimumIntervalSeconds, LaserRainBaseIntervalSeconds);
        }

        private bool FireMissile()
        {
            if (!IsGameplaySimulationActive()
                || paddle == null
                || (availableMissiles <= 0 && !HasFreeMissileShotAvailable())
                || missileShotCooldownTimer > 0f)
            {
                return false;
            }

            var missileObject = new GameObject("Brick Missile");
            missileObject.transform.SetParent(effectsRoot != null ? effectsRoot : runtimeRoot, false);
            var launchPosition = paddleCollider != null
                ? new Vector2(paddleCollider.bounds.center.x, paddleCollider.bounds.max.y + 0.16f)
                : (Vector2)paddle.transform.position + (Vector2.up * 0.52f);
            missileObject.transform.position = launchPosition;
            missileObject.transform.localScale = new Vector3(0.12f, 0.2f, 1f);
            missileObject.AddComponent<SpriteRenderer>();
            missileObject.AddComponent<CircleCollider2D>();
            missileObject.AddComponent<Rigidbody2D>();

            var projectile = missileObject.AddComponent<BreakoutMissileProjectile>();
            projectile.Configure(
                this,
                missileSprite != null ? missileSprite : triangleSprite,
                additiveSpriteMaterial,
                ResolveMissileColor(),
                MissileSpeed,
                MissileRadius,
                arenaTop + 0.75f);
            activeMissiles.Add(projectile);
            if (HasFreeMissileShotAvailable())
            {
                freeMissilesFiredThisLevel++;
                powerUpService?.ShowStatusBanner("FREE TOKEN!", ResolveMissileColor(), 1.15f);
            }
            else
            {
                availableMissiles = Mathf.Max(0, availableMissiles - 1);
            }

            missileShotCooldownTimer = MissileShotCooldownSeconds;
            return true;
        }

        internal void HandleMissileHitBrick(BreakoutMissileProjectile projectile, Brick brick)
        {
            RemoveMissile(projectile);

            if (brick == null || brick.Definition == null || !brick.Definition.IsBreakable)
            {
                var explosionPosition = projectile != null
                    ? (Vector2)projectile.transform.position
                    : brick != null
                        ? (Vector2)brick.transform.position
                        : Vector2.zero;
                audioService?.PlayExplosion();
                SpawnExplosionVisual(explosionPosition, MissileExplosionRadius * 0.72f);
                DestroyMissile(projectile);
                return;
            }

            brick.DestroyByMissile();
            DestroyMissile(projectile);
        }

        internal void HandleMissileExpired(BreakoutMissileProjectile projectile)
        {
            RemoveMissile(projectile);
            DestroyMissile(projectile);
        }

        private void RemoveMissile(BreakoutMissileProjectile projectile)
        {
            if (projectile != null)
            {
                activeMissiles.Remove(projectile);
            }
        }

        private void DestroyMissile(BreakoutMissileProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            projectile.gameObject.SetActive(false);
            DestroyRuntimeObject(projectile.gameObject);
        }

        private Color ResolveMissileColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(1f, 0.28f, 0.66f, 1f),
                    missileSprite != null ? missileSprite : triangleSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private Color ResolveBrickBloomColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupBurst,
                    new Color(1f, 0.87f, 0.36f, 1f),
                    new Color(0.45f, 0.95f, 0.72f, 1f),
                    powerUpSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
        }

        private void SpawnExplosionVisual(Vector2 worldPosition, float radius)
        {
            if (effectsRoot == null)
            {
                return;
            }

            var explosionObject = new GameObject("Neon Explosion");
            explosionObject.transform.SetParent(effectsRoot, false);
            explosionObject.transform.position = worldPosition;
            var visual = explosionObject.AddComponent<BreakoutExplosionPulseVisual>();
            var coreColor = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBurst, new Color(1f, 0.87f, 0.36f, 1f), new Color(1f, 0.28f, 0.66f, 1f), circleSprite).PrimaryColor
                : new Color(1f, 0.87f, 0.36f, 1f);
            var ringColor = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.BrickPrimary, new Color(1f, 0.28f, 0.66f, 1f), new Color(0.01f, 0.93f, 0.98f, 1f), circleSprite).PrimaryColor
                : new Color(1f, 0.28f, 0.66f, 1f);
            visual.Configure(warpGateRingSprite != null ? warpGateRingSprite : circleSprite, circleSprite, additiveSpriteMaterial, coreColor, ringColor, radius);
        }

        private void SplitBallsCaughtInExplosion(
            Vector2 explosionCenter,
            float explosionRadius,
            BallController excludedBall,
            float speedBurstMultiplier,
            float speedBurstDuration)
        {
            if (explosionRadius <= 0.01f)
            {
                return;
            }

            var candidates = new List<BallController>();
            AddExplosionBallCandidate(candidates, serveBall, excludedBall, explosionCenter, explosionRadius);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                AddExplosionBallCandidate(candidates, activeBall, excludedBall, explosionCenter, explosionRadius);
            }

            for (var index = 0; index < candidates.Count; index++)
            {
                SplitBallFromExplosiveBrick(candidates[index], explosionCenter, speedBurstMultiplier, speedBurstDuration);
            }
        }

        private static void AddExplosionBallCandidate(
            List<BallController> candidates,
            BallController ball,
            BallController excludedBall,
            Vector2 explosionCenter,
            float explosionRadius)
        {
            if (ball == null || ball == excludedBall || !ball.HasLaunched || candidates.Contains(ball))
            {
                return;
            }

            if (((Vector2)ball.transform.position - explosionCenter).sqrMagnitude > explosionRadius * explosionRadius)
            {
                return;
            }

            candidates.Add(ball);
        }

        private void SpawnLaserBeamVisual(Brick target, bool leftEmitter)
        {
            if (target == null || paddle == null)
            {
                return;
            }

            var paddlePosition = (Vector2)paddle.transform.position;
            var emitterPosition = brickEffectResolver.ResolveLaserEmitterPosition(paddlePosition, paddle.HalfWidthWorld, leftEmitter);

            if (paddleCollider != null)
            {
                emitterPosition.y = paddleCollider.bounds.max.y + 0.04f;
            }

            var targetPosition = (Vector2)target.transform.position;
            var beamObject = new GameObject(leftEmitter ? "Left Laser Beam" : "Right Laser Beam");
            beamObject.transform.SetParent(effectsRoot != null ? effectsRoot : runtimeRoot, false);

            var beam = beamObject.AddComponent<BreakoutLaserBeamVisual>();
            var coreColor = ResolveLaserBeamCoreColor();
            var glowColor = ResolveLaserBeamGlowColor(leftEmitter);
            var muzzleColor = Color.Lerp(coreColor, glowColor, 0.45f);
            beam.Configure(
                emitterPosition,
                targetPosition,
                additiveLineMaterial,
                coreColor,
                glowColor,
                muzzleColor,
                LaserBeamLifetimeSeconds);
        }

        private Color ResolveLaserBeamCoreColor()
        {
            return themeService != null
                ? Color.Lerp(
                    Color.white,
                    themeService.ResolveThemeStyle(ThemeVisualSlot.Ball, ballColor, ballColor, ballSprite).PrimaryColor,
                    0.3f)
                : new Color(1f, 0.94f, 1f, 1f);
        }

        private Color ResolveLaserBeamGlowColor(bool leftEmitter)
        {
            if (themeService == null)
            {
                return leftEmitter
                    ? new Color(1f, 0.16f, 0.66f, 1f)
                    : new Color(0.01f, 0.93f, 0.98f, 1f);
            }

            return leftEmitter
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.BrickPrimary,
                    new Color(1f, 0.16f, 0.66f, 1f),
                    new Color(1f, 0.16f, 0.66f, 1f),
                    squareSprite).PrimaryColor
                : themeService.ResolveThemeStyle(
                    ThemeVisualSlot.BrickTertiary,
                    new Color(0.01f, 0.93f, 0.98f, 1f),
                    new Color(0.01f, 0.93f, 0.98f, 1f),
                    squareSprite).PrimaryColor;
        }

        private Color ResolveLaserRainWarningColor()
        {
            return themeService != null
                ? themeService.ResolveThemeStyle(
                    ThemeVisualSlot.PickupHarmful,
                    new Color(1f, 0.22f, 0.3f, 1f),
                    new Color(1f, 0.22f, 0.3f, 1f),
                    squareSprite).PrimaryColor
                : new Color(1f, 0.22f, 0.3f, 1f);
        }

        private Color ResolveLaserRainGlowColor()
        {
            if (themeService == null)
            {
                return new Color(1f, 0.16f, 0.66f, 1f);
            }

            return themeService.ResolveThemeStyle(
                ThemeVisualSlot.BrickPrimary,
                new Color(1f, 0.16f, 0.66f, 1f),
                new Color(1f, 0.16f, 0.66f, 1f),
                squareSprite).PrimaryColor;
        }

        private BallController ResolvePrimaryScoringBall()
        {
            for (var index = 0; index < activeBalls.Count; index++)
            {
                if (activeBalls[index] != null)
                {
                    return activeBalls[index];
                }
            }

            return serveBall;
        }

        private void TryTriggerChainLightning(Vector2 origin, Brick sourceBrick, BallController scoringBall, BrickDestructionCause destructionCause)
        {
            if (activeEffectModifiers.ChainLightningStrength <= 0.001f || destructionCause == BrickDestructionCause.ChainLightning)
            {
                return;
            }

            var chainTargets = brickEffectResolver.ResolveChainLightningTargets(
                bricks,
                origin,
                sourceBrick,
                activeEffectModifiers.ChainLightningStrength);

            for (var index = 0; index < chainTargets.Count; index++)
            {
                chainTargets[index]?.ApplyEffectHit(scoringBall, BrickDestructionCause.ChainLightning, 1);
            }
        }

        private void ApplyVisualEffectState()
        {
            var visibilityMultiplier = Mathf.Clamp01(activeEffectModifiers.FogVisibilityMultiplier);

            brickService?.ApplyVisibilityMultiplier(visibilityMultiplier);

            var activePickups = powerUpService?.ActivePickups;

            if (activePickups != null)
            {
                for (var index = activePickups.Count - 1; index >= 0; index--)
                {
                    var pickup = activePickups[index];

                    if (pickup == null)
                    {
                        activePickups.RemoveAt(index);
                        continue;
                    }

                    pickup.SetVisibilityMultiplier(visibilityMultiplier);
                }
            }

            backgroundPresenter?.ApplyVisibilityMultiplier(visibilityMultiplier);
        }

        private void ApplyBrickJammerState()
        {
            var jammerStrength = activeEffectModifiers.BrickJammerStrength;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.SetJammerStrength(jammerStrength);
            }
        }

        private void RefreshBrickMagnetTargets()
        {
            var brickMagnetStrength = GetEffectiveBrickMagnetStrength();

            if (Mathf.Abs(brickMagnetStrength) <= 0.001f)
            {
                ClearBrickMagnetTargets();
                return;
            }

            ApplyBrickMagnetTarget(serveBall, brickMagnetStrength);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                ApplyBrickMagnetTarget(activeBall, brickMagnetStrength);
            }
        }

        private void ClearBrickMagnetTargets()
        {
            serveBall?.SetBrickMagnetTarget(Vector2.zero, 0f);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                activeBall.SetBrickMagnetTarget(Vector2.zero, 0f);
            }
        }

        private void ApplyBrickMagnetTarget(BallController ball, float brickMagnetStrength)
        {
            if (ball == null)
            {
                return;
            }

            var hasTarget = brickMagnetStrength < 0f
                ? TryFindNearestBrickBlock(ball.transform.position, out var targetPosition)
                : TryFindNearestBreakableBrick(ball.transform.position, out targetPosition);

            if (hasTarget)
            {
                ball.SetBrickMagnetTarget(targetPosition, brickMagnetStrength);
                return;
            }

            ball.SetBrickMagnetTarget(Vector2.zero, 0f);
        }

        private bool TryFindNearestBreakableBrick(Vector2 sourcePosition, out Vector2 targetPosition)
        {
            Brick nearestBrick = null;
            var nearestDistanceSquared = float.PositiveInfinity;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (brick.Definition == null || !brick.Definition.IsBreakable)
                {
                    continue;
                }

                var distanceSquared = ((Vector2)brick.transform.position - sourcePosition).sqrMagnitude;

                if (distanceSquared >= nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                nearestBrick = brick;
            }

            if (nearestBrick == null)
            {
                targetPosition = Vector2.zero;
                return false;
            }

            targetPosition = nearestBrick.transform.position;
            return true;
        }

        private bool TryFindNearestBrickBlock(Vector2 sourcePosition, out Vector2 targetPosition)
        {
            Brick nearestBrick = null;
            var nearestDistanceSquared = float.PositiveInfinity;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                var distanceSquared = ((Vector2)brick.transform.position - sourcePosition).sqrMagnitude;

                if (distanceSquared >= nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                nearestBrick = brick;
            }

            if (nearestBrick == null)
            {
                targetPosition = Vector2.zero;
                return false;
            }

            targetPosition = nearestBrick.transform.position;
            return true;
        }

        private void RefreshCapsuleMagnetTargets()
        {
            powerUpService?.RefreshCapsuleMagnetTargets(activeEffectModifiers.CapsuleMagnetStrength, paddleCollider);
        }

        private void UpdateShieldWallVisual()
        {
            if (shieldWallRenderer == null)
            {
                return;
            }

            shieldWallRenderer.enabled = shieldWallCharges > 0;
            if (shieldWallCollider != null)
            {
                shieldWallCollider.enabled = shieldWallCharges > 0;
            }

            if (!shieldWallRenderer.enabled)
            {
                shieldWallVisual?.SetActive(false);
                return;
            }

            var shieldStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBeneficial, new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite);
            var shieldColor = shieldStyle.PrimaryColor;
            shieldColor.a = Mathf.Clamp(0.52f + ((shieldWallCharges - 1) * 0.08f), 0.52f, 0.82f);
            shieldWallRenderer.color = shieldColor;
            shieldWallRenderer.transform.position = new Vector2(0f, arenaBottom + ShieldWallYOffset);
            shieldWallVisual?.SetState(
                shieldWallCharges,
                shieldStyle,
                new Vector2(0f, arenaBottom + ShieldWallYOffset),
                (arenaRight - arenaLeft) - 0.3f,
                ShieldWallThickness);
        }

        private string BuildActiveEffectsLabel()
        {
            return powerUpService != null ? powerUpService.BuildActiveEffectsLabel() : "Active Effects: none";
        }
    }
}
