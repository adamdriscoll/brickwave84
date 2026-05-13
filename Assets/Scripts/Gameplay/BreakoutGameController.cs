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
        private const float LaserShotCooldownSeconds = 0.3f;
        private const float LaserBeamLifetimeSeconds = 0.16f;
        private const float ShieldWallYOffset = 0.38f;
        private const float ShieldWallThickness = 0.16f;
        private const float VectorSightPreviewRange = 2.45f;
        private const float VectorSightPreviewLength = 1.85f;
        private const float ExplosiveBallMinimumRadius = 1.25f;
        private const float ExplosiveBallMaximumRadius = 2.05f;
        private const float ExplosiveBallMinimumSpeedBurstMultiplier = 1.1f;
        private const float ExplosiveBallMaximumSpeedBurstMultiplier = 1.22f;
        private const float ExplosiveBallSpeedBurstDuration = 1.45f;
        private const float TurboRailSpeedBurstMultiplier = 1.35f;
        private const float TurboRailSpeedBurstDuration = 4f;
        private const float TurboRailSpeedBurstStackMultiplier = 0.12f;
        private const float TurboRailSpeedBurstMaximumMultiplier = 1.85f;
        private const float TurboRailSpeedBurstStackDuration = 1.25f;
        private const float TurboRailSpeedBurstMaximumDuration = 7.5f;
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
        private RoundState roundState;
        private int livesRemaining;
        private int lifeLossCount;
        private int score;
        private int requiredBricksRemaining;
        private float arenaLeft;
        private float arenaRight;
        private float arenaTop;
        private float arenaBottom;
        private LevelDefinition currentLevel;
        private int currentLevelIndex;
        private string currentLevelDisplayName = "No level loaded";
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
        private BreakoutHotSeatDifficulty selectedSoloMarathonDifficulty = BreakoutHotSeatDifficulty.Gnarly;
        private int selectedSoloMarathonSetupActionIndex = (int)BreakoutHotSeatDifficulty.Gnarly;
        private int selectedOverlayActionIndex;
        private string currentLevelVariationLabel = "Variation: not started";
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
        private SpriteRenderer shieldWallRenderer;
        private Collider2D shieldWallCollider;
        private BreakoutShieldWallVisual shieldWallVisual;
        private BreakoutVectorSightVisual vectorSightVisual;
        private int shieldWallCharges;
        private float laserShotCooldownTimer;
        private BreakoutLevelGlitchPlan activeLevelGlitchPlan = BreakoutLevelGlitchPlan.None;
        private BreakoutWarpGateController activeWarpGateController;
        private BreakoutTurboRailSection activeTurboRailSection;
        private bool isDeveloperRunActive;
        private bool isMenuAttractModeActive;
        private float menuAttractRestartTimer;
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
        }

        private void Update()
        {
            UpdateTimedEffects();
            RefreshBrickMagnetTargets();
            RefreshCapsuleMagnetTargets();
            UpdateVectorSightVisual();
            UpdatePickupBanner();
            TrackRunStatsFrame();
            audioService?.Update(Time.unscaledDeltaTime);
            scoreService?.UpdateFloatingScorePopups(Time.unscaledDeltaTime);
            laserShotCooldownTimer = Mathf.Max(0f, laserShotCooldownTimer - Time.deltaTime);
            UpdateMenuAttractMode();

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

            var actionPressed = keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;

            if (!actionPressed)
            {
                return;
            }

            if (roundState == RoundState.Playing)
            {
                if (ReleaseStickyCaughtBall())
                {
                    return;
                }

                if (FireLaserVolley())
                {
                    return;
                }
            }

            if (roundState == RoundState.ReadyToServe || roundState == RoundState.LifeLost)
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
            var shouldTriggerExplosiveBall = scoringBall != null
                && scoringBall.IsExplosiveBall
                && destructionCause == BrickDestructionCause.Impact;
            var explosionCenter = (Vector2)brick.transform.position;

            if (brickService == null || !brickService.RemoveBrick(brick))
            {
                return;
            }

            runStatsService?.RegisterBrickDestroyed();
            audioService?.PlayBrickDestroyed(brickDefinition);

            if (shouldExplode && destructionCause == BrickDestructionCause.Impact && scoringBall != null)
            {
                scoringBall.ApplySpeedBurst(brickDefinition.ExplosionSpeedMultiplier, brickDefinition.ExplosionSpeedDuration);
            }

            var scoreAward = scoreService != null
                ? scoreService.BuildBrickScoreAward(brick, scoringBall, destructionCause, BuildScoreContext())
                : default;
            score += scoreAward.TotalPoints;

            if (scoreAward.BonusPoints > 0)
            {
                audioService?.PlayBonusScore();
                scoreService?.CreateFloatingScorePopup(
                    explosionCenter,
                    scoreAward.BonusPoints,
                    scoreAward.BonusLabel,
                    ResolveComboPopupColor());
            }

            scoreService?.RegisterBrickScoreEvent(scoringBall, scoreAward.BasePoints > 0, Time.time);
            TrySpawnPickup(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining - 1);
            }

            brickService.DisableAndDestroyBrick(brick);

            var explosionHitCount = 0;

            if (shouldExplode)
            {
                explosionHitCount += brickService.DestroyBricksInExplosionRadius(explosionCenter, brickDefinition.ExplosionRadius, brick, scoringBall);
            }

            if (shouldTriggerExplosiveBall)
            {
                explosionHitCount += brickService.DestroyBricksInExplosionRadius(
                    explosionCenter,
                    ResolveExplosiveBallExplosionRadius(scoringBall.ExplosiveBallStrength),
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
            requiredBricksRemaining += brickService.SpawnSplitBricks(brickDefinition, explosionCenter);

            EvaluateLevelCompletion();
        }

        public void HandleBallLost(BallController lostBall)
        {
            if (!IsGameplaySimulationActive() || lostBall == null)
            {
                return;
            }

            var isServeBall = lostBall == serveBall;
            if (lostBall == stickyCaughtBall)
            {
                stickyCaughtBall = null;
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

            livesRemaining = Mathf.Max(0, livesRemaining - 1);
            runStatsService?.RegisterLifeLost();
            ApplyLifeLossScorePenalty();

            if (livesRemaining <= 0)
            {
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

            if (activeEffectModifiers.StickyPaddleEnabled && stickyCaughtBall == null)
            {
                stickyCaughtBall = ball;
                ball.AttachToPaddle();
                return true;
            }

            return false;
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

        public void HandleBallHitPaddle()
        {
            runStatsService?.RegisterPaddleHit();
            audioService?.PlayBallHitPaddle();
        }

        public void HandleBallHitWall()
        {
            runStatsService?.RegisterWallHit();
            audioService?.PlayBallHitWall();
        }

        public void HandleBallLaunched()
        {
            runStatsService?.RegisterBallLaunched();
        }

        public void HandleBrickHit(Brick brick)
        {
            runStatsService?.RegisterBrickHit();
            audioService?.PlayBrickHit(brick?.Definition);
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

            ApplyPowerUp(pickup.Definition);
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
            currentLevelVariationLabel = "Variation: pending";
            shieldWallCharges = 0;
            laserShotCooldownTimer = 0f;
            stickyCaughtBall = null;
            scoreService?.ResetComboTracking(clearPopups: true);
            activeRunState?.Reset();
            if (activeRunSettings.IsRogueMode)
            {
                rogueRunController?.InitializeRunState(activeRunState);
            }

            if (applyDeveloperSelections)
            {
                ApplyDeveloperSelectionsToRunState();
            }

            ClearTimedEffects();
            ClearPickups();
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
            currentLevelVariationLabel = "Variation: pending";
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
            currentLevelVariationLabel = "Variation: pending";
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
            currentLevelVariationLabel = "Variation: pending";
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
            currentLevelVariationLabel = "Variation: pending";
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
            SetSimulationPaused(false);
            manualBallSpeedMultiplier = 1f;
            brickService?.ClearBricks();
            ClearPickups();
            ClearTimedEffects();
            ClearLevelGlitches();
            StopAllBalls();
            DestroyAdditionalBalls();
            activeBalls.Clear();
            activeRunState?.ClearPendingDraftOffers();
            selectedUpgradeDraftIndex = 0;
            stickyCaughtBall = null;
            shieldWallCharges = 0;
            laserShotCooldownTimer = 0f;
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

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (definition != null && definition.IsUnlockedForLadderIntensity(intensity))
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

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                EnterMainMenu();
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                StartRogueRun();
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
                selectedDeveloperLaunchField = (BreakoutDeveloperLaunchField)Mathf.Min((int)BreakoutDeveloperLaunchField.ForcedDrop, (int)selectedDeveloperLaunchField + 1);
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
                EnterProgressionPage();
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
            activeRunSettings = rogueRunController != null
                ? rogueRunController.BuildRunSettings(GenerateSeed(), lifeLossScorePenalty, ResolvePendingThemeDefinition(), selectedPaddle.DisplayName, intensity)
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

            var encounter = developerLaunchState.ResolveEncounter();
            pendingValidationMessage = $"Dev jump loaded: {encounter.DisplayName} with {activeRunSettings.SelectedPaddleLabel} at Heat {activeRunSettings.RogueIntensity:00}.";
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
            roundState = RoundState.Playing;
            SetSimulationPaused(false);
            serveBall.Launch();
            SpawnConfiguredServeBalls();
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
            activeBalls.Clear();
            serveBall.SetMovementSpeed(GetCurrentBallSpeed());
            serveBall.ResetToPaddle();
            activeBalls.Add(serveBall);
            scoreService?.ResetComboTracking(clearPopups: false);
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

            if (loadedLevels.Count == 0 || levelIndex < 0)
            {
                currentLevel = null;
                currentLevelIndex = 0;
                currentLevelDisplayName = "No levels loaded";
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
                currentLevelDisplayName = "No levels loaded";
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
            currentLevelDisplayName = string.IsNullOrWhiteSpace(levelPlan.DisplayName)
                ? currentLevel.DisplayName
                : levelPlan.DisplayName;

            ApplyLevelTuning(levelPlan);
            BuildBrickWall(levelPlan);
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

            var visibleHalfWidth = cameraHalfHeight * activeCamera.aspect;
            arenaLeft = -visibleHalfWidth + playfieldPadding;
            arenaRight = visibleHalfWidth - playfieldPadding;
            arenaTop = cameraHalfHeight - playfieldPadding;
            arenaBottom = -cameraHalfHeight + playfieldPadding;
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
            bloom.highQualityFiltering.Override(true);
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
        }

        private Rect ResolveBrickMovementBounds()
        {
            return Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop);
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
            CreateBoundary(
                "Left Wall",
                new Vector2(arenaLeft - (wallThickness * 0.5f), 0f),
                new Vector2(wallThickness, cameraHalfHeight * 2f),
                wallColor);

            CreateBoundary(
                "Right Wall",
                new Vector2(arenaRight + (wallThickness * 0.5f), 0f),
                new Vector2(wallThickness, cameraHalfHeight * 2f),
                wallColor);

            CreateBoundary(
                "Top Wall",
                new Vector2(0f, arenaTop + (wallThickness * 0.5f)),
                new Vector2((arenaRight - arenaLeft) + (wallThickness * 2f), wallThickness),
                wallColor);
        }

        private void CreateBoundary(string wallName, Vector2 position, Vector2 size, Color color)
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
            return ballSpawnService.CreateBall(followsPaddleWhenIdle, arenaBottom - 1f);
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
            if (activeRunSettings != null
                && activeRunSettings.IsTurnBasedMode
                && turnBasedMultiplayerController != null
                && turnBasedMultiplayerController.TryGetCurrentPlayerBrickState(currentLevelIndex, out var savedBrickStates))
            {
                requiredBricksRemaining = brickService != null
                    ? brickService.BuildBrickWall(savedBrickStates)
                    : 0;
                return;
            }

            requiredBricksRemaining = brickService != null
                ? brickService.BuildBrickWall(layoutPlan, arenaTop)
                : 0;
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

            if (activeLevelGlitchPlan.GlitchType == BreakoutLevelGlitchType.WarpGates)
            {
                CreateWarpGates(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("WARP GATES!", new Color(0.03f, 0.93f, 0.98f, 1f), 2.2f);
            }
            else if (activeLevelGlitchPlan.GlitchType == BreakoutLevelGlitchType.TurboRail)
            {
                CreateTurboRail(activeLevelGlitchPlan);
                powerUpService?.ShowStatusBanner("TURBO RAIL!", new Color(1f, 0.22f, 0.84f, 1f), 2.2f);
            }
        }

        private void ClearLevelGlitches()
        {
            activeLevelGlitchPlan = BreakoutLevelGlitchPlan.None;

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
            activeWarpGateController.Configure(this);

            for (var index = 0; index < glitchPlan.WarpGates.Length; index++)
            {
                CreateWarpGatePortal(gateRoot.transform, index, glitchPlan.WarpGates[index]);
            }
        }

        private void CreateWarpGatePortal(Transform gateRoot, int portalIndex, BreakoutWarpGateSpec spec)
        {
            var portalObject = new GameObject($"Warp Gate {portalIndex + 1:00}");
            portalObject.transform.SetParent(gateRoot, false);
            portalObject.transform.position = ResolveWarpGatePosition(spec);

            var portal = portalObject.AddComponent<BreakoutWarpGatePortal>();
            portal.Configure(activeWarpGateController, portalIndex, spec.Wall, ResolveWarpGateExitPosition(spec));
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
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Progression", "Neon Ladder Service", true));
                uiRenderer.DrawProgressionPage(BuildProgressionPageView(), StartRogueRun, EnterMainMenu);
                return;
            }

            if (roundState == RoundState.LifetimeStats)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Lifetime Stats", "Cabinet Totals", true));
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
                uiRenderer.DrawUpgradeDraft(BuildUpgradeDraftView(), HandleUpgradeDraftChoice);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            var gameplayChromeView = BuildChromeView(string.Empty, string.Empty, false);
            uiRenderer.DrawCabinetBackdrop(gameplayChromeView);
            uiRenderer.DrawGameplayHud(BuildHudView(), ToggleDiagnosticsOverlay, ToggleHudMenuOverlay);
            uiRenderer.DrawModifierIndicator(BuildModifierViews(), isDiagnosticsOverlayVisible);
            uiRenderer.DrawRunUpgradePanel(BuildRunUpgradePanelView());
            uiRenderer.DrawCapsuleMadness(BuildCapsuleMadnessView(gameplayChromeView.PlayfieldRect));
            uiRenderer.DrawFloatingScorePopups(scoreService?.BuildFloatingScoreViews(activeCamera, Screen.height) ?? Array.Empty<BreakoutUiFloatingScoreView>());

            if (isDiagnosticsOverlayVisible)
            {
                uiRenderer.DrawDiagnosticsOverlay(BuildDiagnosticsView());
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
            return progressionPageService.BuildView(loadedPowerUpDefinitions, themeService);
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
                    HintText = "Up/Down selects. Left/Right adjusts. N resets defaults. Esc returns to menu. Space launches.",
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
                HintText = "Up/Down selects. Left/Right adjusts. Type digits for the Tape ID. Backspace edits. T randomizes. N resets defaults. Esc returns to menu. Space launches.",
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
                },
                SelectedFieldIndex = (int)selectedDeveloperLaunchField,
                PreviewLine = $"Preview: {FormatDeveloperEncounterLabel(encounter)} | Heat {developerLaunchState.Intensity:00} | Balls {developerLaunchState.LivesRemaining:00} | Paddle x{selectedPaddle.WidthMultiplier:0.00} speed x{selectedPaddle.SpeedMultiplier:0.00} | Build {developerLaunchState.SelectedUpgradeCount:00} upgrades, {developerLaunchState.SelectedDropUnlockCount:00} drops | Force {FormatDeveloperForcedDropPreview(forcedDropEnabled, forcedDrop)} | Theme {ResolvePendingThemeDefinition()?.DisplayName ?? "Fallback"}",
                ValidationText = "Encounter cycles through Stage 01-10. Dev runs do not update the saved Neon Ladder result.",
                HintText = "Up/Down selects. Left/Right changes. T toggles build/force. N clears build. Esc returns to menu. Space launches.",
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
                    "Pick a heat, then start the chase. Higher heat pays bigger score multipliers.",
                    "Up/Down selects. Left/Right changes heat. Space confirms. Esc returns to mode select.",
                },
                IsCompact = false,
            };
        }

        private BreakoutUiOverlayView BuildLifetimeStatsOverlayView()
        {
            var lifetimeStats = runStatsService?.LifetimeStats ?? new BreakoutRunStatsSnapshot();
            return new BreakoutUiOverlayView
            {
                Title = "Lifetime Stats",
                SummaryTitle = "Cabinet Total",
                StatsRows = BuildLifetimeStatsRows(lifetimeStats),
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(RoundState.LifetimeStats)),
                SelectedActionIndex = selectedOverlayActionIndex,
                FooterLines = new[]
                {
                    "Totals aggregate Neon Ladder, Neon Marathon, Custom Game, and Hot Seat runs. Up/Down scrolls.",
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

        private BreakoutUiHudView BuildHudView()
        {
            var bounceZoneLeftScreen = activeCamera != null
                ? activeCamera.WorldToScreenPoint(new Vector3(arenaLeft, 0f, 0f)).x
                : 0f;
            var speed = GetDisplayedBallSpeed();
            var isRogueRun = activeRunSettings != null && activeRunSettings.IsRogueMode;
            var intensity = isRogueRun
                ? BreakoutRunProgression.ClampRogueIntensity(activeRunSettings.RogueIntensity)
                : BreakoutRunProgression.MinRogueIntensity;
            var intensityProgress = BreakoutRunProgression.GetRogueIntensityProgress(intensity);
            var playerHudLabel = BuildTurnBasedPlayerHudPrefix();
            var topLine = $"{GetScoreDisplayLabel().ToUpperInvariant()} {FormatScoreValue(score)}   {GetLifeCounterLabel().ToUpperInvariant()} {GetLifeCounterValue():00}   {BuildLevelLabel().ToUpperInvariant()}";

            if (!string.IsNullOrWhiteSpace(playerHudLabel))
            {
                topLine = $"{playerHudLabel.ToUpperInvariant()}   {topLine}";
            }

            return new BreakoutUiHudView
            {
                TopLine = topLine,
                BottomLine = BuildGameplayStatusLine().ToUpperInvariant(),
                ShowMenuButton = CanPauseRoundState(roundState) || roundState == RoundState.Paused,
                IsPaused = roundState == RoundState.Paused,
                IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
                SpeedMeter = new BreakoutUiSpeedMeterView
                {
                    BounceZoneLeftScreen = bounceZoneLeftScreen,
                    IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
                    Speed = speed,
                    SpeedRatio = Mathf.Clamp01(speed / Mathf.Max(0.1f, GetMaximumBallSpeed())),
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
                    "Controls: A/D or Left/Right moves, Space launches/advances, Up/Down adjusts manual ball speed, Esc/P opens or closes this menu, R returns to run setup.",
                    "Up/Down selects. Space confirms. Esc or P resumes immediately.",
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
                HintText = "Left/Right selects. Space confirms. Clicking a card also takes it. R abandons the run and returns to setup.",
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
                    : (activeRunSettings != null && activeRunSettings.IsRogueMode ? "Ladder Cleared" : "Final Layout Cleared");
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
            var useRunStatsTable = isGameOver
                && activeRunSettings != null
                && activeRunSettings.IsRogueMode;

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

        private BreakoutUiRunUpgradePanelView BuildRunUpgradePanelView()
        {
            var chosenUpgrades = activeRunState?.ChosenUpgrades;

            if (activeRunState == null || !activeRunState.HasActiveBuild)
            {
                return new BreakoutUiRunUpgradePanelView
                {
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
                IsDiagnosticsVisible = isDiagnosticsOverlayVisible,
            };
        }

        private BreakoutUiDiagnosticsView BuildDiagnosticsView()
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
                : $"Level {currentLevelIndex + 1:00} - {currentLevelDisplayName}";
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

        private string BuildRemainingBricksLabel()
        {
            if (currentLevel == null)
            {
                return "Remaining Bricks --";
            }

            return $"Remaining Bricks {requiredBricksRemaining:00}";
        }

        private string BuildGameplayStatusLine()
        {
            var progressLabel = BuildRemainingBricksLabel();
            var glitchLabel = activeLevelGlitchPlan != null && activeLevelGlitchPlan.IsActive
                ? $" | {activeLevelGlitchPlan.HudLabel}"
                : string.Empty;

            if (activeRunState == null || !activeRunState.HasActiveBuild)
            {
                return $"{progressLabel}{glitchLabel}";
            }

            return $"{progressLabel}{glitchLabel} | {BuildUpgradeSummaryLabel(2)}";
        }

        private string BuildReadyToServeMessage()
        {
            if (activeRunSettings == null || !activeRunSettings.IsTurnBasedMode)
            {
                return "Press Space to launch the ball. Up/Down tunes speed.";
            }

            return $"{turnBasedMultiplayerController?.BuildSwitchTitle() ?? "Up Next"}\nPress Space to launch. Up/Down tunes speed.";
        }

        private string BuildLifeLostMessage()
        {
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
                return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                    ? $"Ball lost. -{GetLifeLossScorePenalty():0000} score. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                    : $"Life lost. -{GetLifeLossScorePenalty():0000} score. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.";
            }

            return UsesHighScoreMode() && !UsesFiniteHighScoreLives()
                ? $"Ball lost. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                : $"Life lost. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.";
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
            Debug.Log("Quit requested from the in-game menu.");

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

        private void UpdateTimedEffects()
        {
            powerUpService?.UpdateTimedEffects(IsGameplaySimulationActive(), Time.deltaTime, ApplyActiveEffects);
        }

        private void UpdatePickupBanner()
        {
            powerUpService?.UpdatePickupBanner(Time.deltaTime);
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
                loadedPowerUpDefinitions);

            if (spawnedPickup != null)
            {
                runStatsService?.RegisterDropDropped(spawnedPickup.Definition);
                audioService?.PlayPickupDropped();
            }

            ApplyVisualEffectState();
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

            var applicationResult = powerUpService.ApplyPowerUp(powerUpDefinition, themeService);

            if (applicationResult.ShieldWallChargesGranted > 0)
            {
                shieldWallCharges += applicationResult.ShieldWallChargesGranted;
                UpdateShieldWallVisual();
            }

            ApplyActiveEffects();

            if (applicationResult.ShouldSpawnMultiBall)
            {
                SpawnMultiBall(powerUpDefinition);
            }
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

        private void ApplyActiveEffects()
        {
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            activeEffectModifiers = powerUpService != null
                ? powerUpService.CalculateEffectModifiers(
                    (activeRunSettings?.PaddleWidthMultiplier ?? 1f) * persistentModifiers.PaddleWidthMultiplier,
                    persistentModifiers.WavyPaddleStrength)
                : new BreakoutEffectModifiers(
                    (activeRunSettings?.PaddleWidthMultiplier ?? 1f) * persistentModifiers.PaddleWidthMultiplier,
                    persistentModifiers.WavyPaddleStrength,
                    1f,
                    1f,
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
                    1f,
                    false,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f);
            paddle.SetMoveSpeed(currentLevelPaddleSpeed * (activeRunSettings?.PaddleSpeedMultiplier ?? 1f));
            var paddleHitMaximumWidth = paddle.SetWidthMultiplier(activeEffectModifiers.PaddleWidthMultiplier);

            if (paddleHitMaximumWidth && TryBreakWidePaddle())
            {
                ApplyActiveEffects();
                TriggerWidePaddleBreakFeedback();
                return;
            }

            if (activeEffectModifiers.BallSizeMultiplier > BallController.MaximumSizeMultiplier
                && TryPopMegaBall())
            {
                ApplyActiveEffects();
                TriggerMegaBallPopFeedback();
                return;
            }

            paddle.SetWavyStrength(activeEffectModifiers.WavyPaddleStrength);
            paddle.SetControlsReversed(activeEffectModifiers.ReverseControlsEnabled);
            paddle.SetSplitGapWidthNormalized(activeEffectModifiers.SplitPaddleGapNormalized);
            paddle.SetLagSpikeStrength(activeEffectModifiers.LagSpikeStrength);
            paddle.SetClonePaddleEnabled(activeEffectModifiers.PaddleCloneEnabled);

            var currentBallSpeed = GetCurrentBallSpeed();
            var gravityWellCenter = new Vector2(0f, (arenaTop + arenaBottom) * 0.5f);

            if (serveBall != null)
            {
                serveBall.SetMovementSpeed(currentBallSpeed);
                serveBall.SetPhaseThroughBricks(activeEffectModifiers.PhaseBallEnabled);
                serveBall.SetSizeMultiplier(activeEffectModifiers.BallSizeMultiplier);
                serveBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
                serveBall.SetHotPotatoStrength(activeEffectModifiers.HotPotatoStrength);
                serveBall.SetExplosiveBallStrength(activeEffectModifiers.ExplosiveBallStrength);
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
                activeBall.SetSizeMultiplier(activeEffectModifiers.BallSizeMultiplier);
                activeBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
                activeBall.SetHotPotatoStrength(activeEffectModifiers.HotPotatoStrength);
                activeBall.SetExplosiveBallStrength(activeEffectModifiers.ExplosiveBallStrength);
            }

            ApplyVisualEffectState();
            ApplyBrickJammerState();
            UpdateShieldWallVisual();
            UpdateVectorSightVisual();

            if (stickyCaughtBall != null && !activeEffectModifiers.StickyPaddleEnabled)
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

        private void ClearPickups()
        {
            powerUpService?.ClearPickups();
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

        private BreakoutRunUpgradeModifiers GetPersistentRunUpgradeModifiers()
        {
            return activeRunState != null
                ? activeRunState.CalculateModifiers()
                : new BreakoutRunUpgradeModifiers(1f, 1f, 1f, 0f, 0);
        }

        private int GetEffectiveBallsPerServe()
        {
            var baseBallsPerServe = activeRunSettings == null ? 1 : activeRunSettings.BallsPerServe;
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            return Mathf.Clamp(baseBallsPerServe + persistentModifiers.ExtraBallsPerServe, 1, 6);
        }

        private float GetEffectiveDropChanceMultiplier()
        {
            var baseMultiplier = activeRunSettings?.DropChanceMultiplier ?? 1f;
            return Mathf.Clamp(baseMultiplier * GetPersistentRunUpgradeModifiers().DropChanceMultiplier, 0f, 3f);
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
                PowerUpEffectType.BallSpeedMultiplier => $"Ball x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BallSizeMultiplier => $"Ball size x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.MultiBallBurst => $"+{Mathf.Max(1, definition.ExtraBallCount)} balls",
                PowerUpEffectType.WavyPaddle => $"Wave {definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.StickyPaddle => $"Catch ball for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.LaserPaddle => $"Laser paddle for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ShieldWall => "Shield save",
                PowerUpEffectType.PhaseBall => $"Phase ball for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ChainLightning => $"Chain hits for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ActiveDropMultiplier => $"Active effects x{definition.Scalar:0.00}",
                PowerUpEffectType.BrickMagnet => $"Brick pull for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.ScoreMultiplier => $"Score x{definition.Scalar:0.00} for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.PaddleClone => $"Clone rail for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.BrickJammer => $"Brick jam for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.HotPotatoBall => $"Ball x{definition.Scalar:0.00}, score x{definition.Scalar:0.00}",
                PowerUpEffectType.ExplosiveBall => $"Explodes bricks for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.VectorSight => $"Aim preview for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.CapsuleMagnet => $"Helpful capsules drift for {definition.DurationSeconds:0.#}s",
                PowerUpEffectType.RandomHarmfulDrop => "Disguised random hazard",
                _ => $"{definition.HudLabel} for {definition.DurationSeconds:0.#}s",
            };
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

            if (upgrade.ExtraBallsPerServe > 0)
            {
                parts.Add($"+{upgrade.ExtraBallsPerServe} ball/serve");
            }

            if (upgrade.BonusLives > 0)
            {
                parts.Add($"+{upgrade.BonusLives} life");
            }

            if (upgrade.WavyPaddleStrength > 0.001f)
            {
                parts.Add($"Wave {upgrade.WavyPaddleStrength:0.00}");
            }

            return parts.Count == 0 ? "Passive build mod" : string.Join(" | ", parts);
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
                return squareSprite;
            }

            var resourcePath = upgrade.ResolveIconSpriteResourcePath();

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return squareSprite;
            }

            if (!runUpgradeSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            {
                cachedSprite = Resources.Load<Sprite>(resourcePath);
                runUpgradeSpriteCache[resourcePath] = cachedSprite;
            }

            return cachedSprite != null ? cachedSprite : squareSprite;
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
                    cachedSprite = Resources.Load<Sprite>(resourcePath);
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
                return squareSprite;
            }

            return offer.Kind == BreakoutRunDraftOfferKind.DropUnlock
                ? ResolvePowerUpIcon(offer.DropUnlockDefinition)
                : ResolveRunUpgradeIcon(offer.UpgradeDefinition);
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
            stickyCaughtBall = null;

            if (releasedBall != null)
            {
                releasedBall.Launch();
            }

            return true;
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
            var visibilityMultiplier = activeEffectModifiers.FogVisibilityMultiplier > 0.001f
                ? activeEffectModifiers.FogVisibilityMultiplier
                : 1f;

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
            if (activeEffectModifiers.BrickMagnetStrength <= 0.001f)
            {
                ClearBrickMagnetTargets();
                return;
            }

            ApplyBrickMagnetTarget(serveBall);

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var activeBall = activeBalls[index];

                if (activeBall == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                ApplyBrickMagnetTarget(activeBall);
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

        private void ApplyBrickMagnetTarget(BallController ball)
        {
            if (ball == null)
            {
                return;
            }

            if (TryFindNearestBreakableBrick(ball.transform.position, out var targetPosition))
            {
                ball.SetBrickMagnetTarget(targetPosition, activeEffectModifiers.BrickMagnetStrength);
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
