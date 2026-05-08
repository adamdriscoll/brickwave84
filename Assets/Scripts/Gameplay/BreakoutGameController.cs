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
        private const string BossPaddleSpriteResourcePath = "Sprites/paddle-punk";
        private const string PowerUpSpriteResourcePath = "Sprites/powerup";
        private const float LaserShotCooldownSeconds = 0.3f;
        private const float LaserBeamLifetimeSeconds = 0.16f;
        private const float BossPaddleCollisionSpeedBurstSeconds = 1.8f;
        private const int PaddlePunkShieldLaneCount = 5;
        private const float PaddlePunkShieldFadeSeconds = 0.42f;
        private const float ShieldWallYOffset = 0.38f;
        private const float ShieldWallThickness = 0.16f;
        private const float ExplosiveBallMinimumRadius = 1.25f;
        private const float ExplosiveBallMaximumRadius = 2.05f;
        private const float ExplosiveBallMinimumSpeedBurstMultiplier = 1.1f;
        private const float ExplosiveBallMaximumSpeedBurstMultiplier = 1.22f;
        private const float ExplosiveBallSpeedBurstDuration = 1.45f;
        private const string DefaultRoguePaddleLabel = BreakoutRogueRunResultStore.DefaultPaddleLabel;

        private enum RoundState
        {
            MainMenu,
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
        private readonly List<Brick> paddlePunkShieldBricks = new List<Brick>();
        private readonly Dictionary<string, Sprite> runUpgradeSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private readonly BreakoutBrickEffectResolver brickEffectResolver = new BreakoutBrickEffectResolver();

        private Camera activeCamera;
        private Transform runtimeRoot;
        private Transform backgroundRoot;
        private Transform wallsRoot;
        private Transform ballsRoot;
        private Transform bricksRoot;
        private Transform pickupsRoot;
        private Transform effectsRoot;
        private Transform bossRoot;
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
        private Sprite bossPaddleSprite;
        private Sprite powerUpSprite;
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
        private BreakoutUiRenderer uiRenderer;
        private IBreakoutScoreService scoreService;
        private BreakoutBackgroundLibrary backgroundLibrary;
        private BreakoutBackgroundPresenter backgroundPresenter;
        private BreakoutRunState activeRunState;
        private BreakoutUpgradeDraftService upgradeDraftService;
        private BreakoutRogueRunController rogueRunController;
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
        private int selectedOverlayActionIndex;
        private string currentLevelVariationLabel = "Variation: not started";
        private string pendingValidationMessage = string.Empty;
        private float manualBallSpeedMultiplier = 1f;
        private int manualBallSpeedHoldDirection;
        private float manualBallSpeedHoldTimer;
        private bool isDiagnosticsOverlayVisible;
        private int selectedUpgradeDraftIndex;
        private bool activeRunResultRecorded;
        private BreakoutEffectModifiers activeEffectModifiers;
        private BallController stickyCaughtBall;
        private SpriteRenderer shieldWallRenderer;
        private Collider2D shieldWallCollider;
        private BreakoutShieldWallVisual shieldWallVisual;
        private int shieldWallCharges;
        private float laserShotCooldownTimer;
        private BreakoutBossGate? activeBossGate;
        private BreakoutBossGate? pendingBossGate;
        private BreakoutPaddlePunkBoss activePaddlePunkBoss;
        private BreakoutBrickosaurusWrecksBoss activeBrickosaurusBoss;
        private BreakoutMainframeManiacBoss activeMainframeManiacBoss;
        private int bossShieldSpawnIndex;
        private bool isDeveloperRunActive;

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
            uiRenderer = new BreakoutUiRenderer();
            scoreService = new BreakoutScoreService();
            activeRunState = new BreakoutRunState();
            developerLaunchState = new BreakoutDeveloperLaunchState();
            upgradeDraftService = new BreakoutUpgradeDraftService(loadedRunUpgradeDefinitions);
            rogueRunController = new BreakoutRogueRunController(loadedRunUpgradeDefinitions, loadedPowerUpDefinitions);
            CreateRuntimeRoots();
            CreateAudioService();
            CreateActorSpawnServices();
            CreateBrickService();
            CreateBackground();
            CreateBounds();
            CreateShieldWallVisual();
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
            UpdatePickupBanner();
            audioService?.Update(Time.unscaledDeltaTime);
            scoreService?.UpdateFloatingScorePopups(Time.unscaledDeltaTime);
            laserShotCooldownTimer = Mathf.Max(0f, laserShotCooldownTimer - Time.deltaTime);
            UpdateBossEncounter();

            var keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (roundState == RoundState.MainMenu
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
            if (roundState != RoundState.Playing || lostBall == null)
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

            if (UsesHighScoreMode())
            {
                lifeLossCount += 1;
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
            ApplyLifeLossScorePenalty();

            if (livesRemaining <= 0)
            {
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                ClearPickups();
                RecordRogueRunResult(completed: false);

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

        public bool TryRescueBallWithShield(BallController ball, bool requireImpactThreshold = true)
        {
            if (roundState != RoundState.Playing || ball == null || shieldWallCharges <= 0)
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

        internal bool TryHandleBossPaddleCollision(BallController ball, BreakoutPaddlePunkBoss bossPaddle, Collision2D collision)
        {
            if (roundState != RoundState.Playing
                || ball == null
                || bossPaddle == null
                || activeBossGate == null
                || !bossPaddle.TryBuildCollisionResponse(collision, out var bounceDirection, out var speedBurstMultiplier))
            {
                return false;
            }

            audioService?.PlayBallHitPaddle();
            ball.ApplyCollisionResponse(bounceDirection, 0.18f);

            if (speedBurstMultiplier > 1.001f)
            {
                ball.ApplySpeedBurst(speedBurstMultiplier, BossPaddleCollisionSpeedBurstSeconds);
            }

            return true;
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

        internal bool TryHandleBrickosaurusCollision(BallController ball, BreakoutBrickosaurusPart bossPart, Collision2D collision)
        {
            if (roundState != RoundState.Playing
                || ball == null
                || bossPart == null
                || activeBossGate == null
                || activeBrickosaurusBoss == null)
            {
                return false;
            }

            var result = activeBrickosaurusBoss.TryHandlePartHit(bossPart, ball, collision);

            if (!result.Handled)
            {
                return false;
            }

            ball.ApplyCollisionResponse(result.BounceDirection, 0.16f);

            if (result.SpeedBurstMultiplier > 1.001f)
            {
                ball.ApplySpeedBurst(result.SpeedBurstMultiplier, result.SpeedBurstDuration);
            }

            if (result.Damaged)
            {
                ball.RegisterBrickScore();
                audioService?.PlayBrickHit(null);
            }

            if (result.LayerDestroyed)
            {
                audioService?.PlayBrickDestroyed(null);
            }

            if (result.ScorePoints > 0)
            {
                score += result.ScorePoints;
                scoreService?.CreateFloatingScorePopup(
                    bossPart.transform.position,
                    result.ScorePoints,
                    "WRECKS",
                    result.CalloutColor);
            }

            if (result.LayerDestroyed && !string.IsNullOrWhiteSpace(result.Callout))
            {
                powerUpService?.ShowStatusBanner(result.Callout, result.CalloutColor, 1.05f);
            }

            if (result.Defeated)
            {
                EvaluateLevelCompletion();
            }

            return true;
        }

        internal bool TryHandleMainframeManiacCollision(BallController ball, BreakoutMainframeManiacNode bossNode, Collision2D collision)
        {
            if (roundState != RoundState.Playing
                || ball == null
                || bossNode == null
                || activeBossGate == null
                || activeMainframeManiacBoss == null)
            {
                return false;
            }

            var result = activeMainframeManiacBoss.TryHandleNodeHit(bossNode, ball, collision);

            if (!result.Handled)
            {
                return false;
            }

            ball.ApplyCollisionResponse(result.BounceDirection, 0.14f);

            if (result.SpeedBurstMultiplier > 1.001f)
            {
                ball.ApplySpeedBurst(result.SpeedBurstMultiplier, result.SpeedBurstDuration);
            }

            if (result.Damaged)
            {
                ball.RegisterBrickScore();
                audioService?.PlayBrickHit(null);
            }

            if (result.LayerDestroyed)
            {
                audioService?.PlayBrickDestroyed(null);
            }

            if (result.ScorePoints > 0)
            {
                score += result.ScorePoints;
                scoreService?.CreateFloatingScorePopup(
                    bossNode.transform.position,
                    result.ScorePoints,
                    "MAINFRAME",
                    result.CalloutColor);
            }

            if (!string.IsNullOrWhiteSpace(result.Callout))
            {
                powerUpService?.ShowStatusBanner(result.Callout, result.CalloutColor, result.LayerDestroyed ? 1.15f : 0.82f);
            }

            if (result.Defeated)
            {
                EvaluateLevelCompletion();
            }

            return true;
        }

        public void HandleBallHitPaddle()
        {
            audioService?.PlayBallHitPaddle();
        }

        public void HandleBallHitWall()
        {
            audioService?.PlayBallHitWall();
        }

        public void HandleBrickHit(Brick brick)
        {
            audioService?.PlayBrickHit(brick?.Definition);
        }

        public void HandlePickupCaught(PowerUpPickup pickup)
        {
            if (roundState != RoundState.Playing || pickup == null)
            {
                return;
            }

            var pickupPosition = (Vector2)pickup.transform.position;
            var awardCapsuleMadnessBonus = powerUpService != null && powerUpService.IsCapsuleMadnessActive;
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

        internal void HandleBrickosaurusPowerDownHit(BreakoutBrickosaurusPowerDownProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            if (roundState == RoundState.Playing)
            {
                var hazard = ResolveBrickosaurusPowerDownDefinition();

                if (hazard != null)
                {
                    audioService?.PlayPickupCollected(hazard);
                    ApplyPowerUp(hazard);
                }
                else
                {
                    powerUpService?.ShowStatusBanner("BOGUS SHOT!", new Color(1f, 0.18f, 0.32f, 1f), 1.2f);
                }
            }

            projectile.gameObject.SetActive(false);
            DestroyRuntimeObject(projectile.gameObject);
        }

        private void StartNewRun()
        {
            StartNewRun(null, -1, applyDeveloperSelections: false);
        }

        private void StartNewRun(BreakoutDeveloperEncounter? developerEncounter, int developerLivesRemaining, bool applyDeveloperSelections)
        {
            if (activeRunSettings == null)
            {
                activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
            }

            isDeveloperRunActive = applyDeveloperSelections;
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
            currentLevelIndex = developerEncounter?.LevelIndex ?? 0;
            currentLevelVariationLabel = "Variation: pending";
            pendingBossGate = null;
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
            ClearBossEncounter();
            UpdateShieldWallVisual();

            if (developerEncounter.HasValue && developerEncounter.Value.IsBossGate)
            {
                LoadBossGate(developerEncounter.Value.BossGate.Value);
            }
            else
            {
                LoadLevel(currentLevelIndex, RoundState.ReadyToServe);
            }

            Debug.Log(
                $"Starting {(isDeveloperRunActive ? "Developer " : string.Empty)}{activeRunSettings.GameModeLabel} run | seed {activeRunSettings.Seed} | preset {activeRunSettings.DifficultyLabel} | " +
                $"score mode {activeRunSettings.ScoringModeLabel} | life loss penalty {activeRunSettings.LifeLossScorePenalty} | " +
                $"balls/serve {activeRunSettings.BallsPerServe} | paddle {activeRunSettings.SelectedPaddleLabel} width x{activeRunSettings.PaddleWidthMultiplier:0.00} speed x{activeRunSettings.PaddleSpeedMultiplier:0.00} | " +
                $"ball speed x{activeRunSettings.BallSpeedMultiplier:0.00} | brick durability x{activeRunSettings.BrickDurabilityMultiplier:0.00} | " +
                $"drops {activeRunSettings.DropPoolLabel} | theme {activeRunSettings.ThemeLabel}");
        }

        private void EnterMainMenu()
        {
            roundState = RoundState.MainMenu;
            selectedMainMenuActionIndex = 0;
            selectedOverlayActionIndex = 0;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            isDiagnosticsOverlayVisible = false;
            isDeveloperRunActive = false;
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
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

        private void EnterRunSetup()
        {
            if (pendingRunSetup == null)
            {
                ResetPendingRunSetup(generateNewSeed: true);
            }

            roundState = RoundState.RunSetup;
            selectedRunSetupField = BreakoutRunSetupField.Seed;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            isDiagnosticsOverlayVisible = false;
            isDeveloperRunActive = false;
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void EnterDeveloperMenu()
        {
            developerLaunchState ??= new BreakoutDeveloperLaunchState();
            roundState = RoundState.DeveloperMenu;
            selectedDeveloperLaunchField = BreakoutDeveloperLaunchField.Encounter;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            isDiagnosticsOverlayVisible = false;
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
            audioService?.PlayMusic(BreakoutMusicTrack.Menu);
        }

        private void ResetRuntimeForMetaFlow()
        {
            SetSimulationPaused(false);
            manualBallSpeedMultiplier = 1f;
            brickService?.ClearBricks();
            ClearPickups();
            ClearTimedEffects();
            ClearBossEncounter();
            StopAllBalls();
            DestroyAdditionalBalls();
            activeBalls.Clear();
            pendingBossGate = null;
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
                selectedRunSetupField = (BreakoutRunSetupField)Mathf.Max(0, (int)selectedRunSetupField - 1);
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedRunSetupField = (BreakoutRunSetupField)Mathf.Min((int)BreakoutRunSetupField.Theme, (int)selectedRunSetupField + 1);
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
                selectedDeveloperLaunchField = (BreakoutDeveloperLaunchField)Mathf.Min((int)BreakoutDeveloperLaunchField.DropUnlock, (int)selectedDeveloperLaunchField + 1);
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

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                selectedMainMenuActionIndex = Mathf.Clamp(selectedMainMenuActionIndex, 0, actions.Length - 1);
                PerformMainMenuAction(actions[selectedMainMenuActionIndex]);
            }
        }

        private OverlayAction[] GetOverlayActionsForState(RoundState state)
        {
            var isRogueRun = activeRunSettings != null && activeRunSettings.IsRogueMode;

            return state switch
            {
                RoundState.Paused => isRogueRun
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
                    : isRogueRun
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
                RoundState.GameOver => isRogueRun
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
                    StartNewRun();
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

            if (action == BreakoutMainMenuAction.CustomGame)
            {
                EnterRunSetup();
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
            if (activeRunSettings != null && activeRunSettings.IsRogueMode)
            {
                EnterMainMenu();
                return;
            }

            EnterRunSetup();
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
                    selectedPaddle.SpeedMultiplier);
            pendingValidationMessage = $"Rogue tape loaded: {activeRunSettings.SelectedPaddleLabel}, Heat {activeRunSettings.RogueIntensity:00}/50, 10 stages, 3 balls, draft rewards, growing drop pool.";
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
                    selectedPaddle.SpeedMultiplier);

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

            pendingRunSetup.AdjustField(selectedRunSetupField, direction, GenerateSeed, ShiftThemeId);
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
            return pendingRunSetup.BuildRunSettings(startingLives, lifeLossScorePenalty, selectedTheme, GenerateSeed, out validationMessage, commitSeedText);
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
                Debug.LogWarning("No power-up definitions were found in Resources/PowerUps. Rogue drop unlocks will be skipped.");
            }
        }

        private void LoadLevel(int levelIndex, RoundState serveState)
        {
            ClearBossEncounter();
            brickService?.ClearBricks();
            ClearPickups();
            stickyCaughtBall = null;
            ClearTimedEffects();
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
            backgroundHazeSprite = BreakoutRuntimeVisualFactory.CreateBackgroundHazeSprite();
            backgroundScanlineSprite = BreakoutRuntimeVisualFactory.CreateBackgroundScanlineSprite();
            backgroundLibrary = new BreakoutBackgroundLibrary();
            backgroundLibrary.LoadFromResources();
            ballSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(BallSpriteResourcePath, circleSprite);
            brickSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(BrickSpriteResourcePath, squareSprite);
            paddleSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(PaddleSpriteResourcePath, squareSprite);
            bossPaddleSprite = BreakoutRuntimeVisualFactory.LoadSpriteResource(BossPaddleSpriteResourcePath, paddleSprite);
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

            bossRoot = new GameObject("Bosses").transform;
            bossRoot.SetParent(runtimeRoot, false);
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
            requiredBricksRemaining = brickService != null
                ? brickService.BuildBrickWall(layoutPlan, arenaTop)
                : 0;
        }

        private void LoadBossGate(BreakoutBossGate bossGate)
        {
            ClearBossEncounter();
            brickService?.ClearBricks();
            ClearPickups();
            stickyCaughtBall = null;
            ClearTimedEffects();
            scoreService?.ResetComboTracking(clearPopups: true);

            activeBossGate = bossGate;
            pendingBossGate = null;
            bossShieldSpawnIndex = 0;
            currentLevelIndex = bossGate.TriggerLevelIndex;
            currentLevel = ResolveLevelTemplate(currentLevelIndex);
            currentLevelDisplayName = bossGate.DisplayName;
            currentLevelVariationLabel = bossGate.BossType switch
            {
                BreakoutBossGateType.BrickosaurusWrecks => $"Boss Gate: {bossGate.HudLabel} | Strip the shield, scales, and core from the snake.",
                BreakoutBossGateType.MainframeManiac => $"Boss Gate: {bossGate.HudLabel} | Break data banks, then crash the core.",
                _ => $"Boss Gate: {bossGate.HudLabel} | Break the weak points behind the rail.",
            };

            UpdateBackgroundVisuals();
            audioService?.PlayBossMusic(bossGate.BossType);
            ApplyBossGateTuning(bossGate);
            if (bossGate.BossType == BreakoutBossGateType.BrickosaurusWrecks)
            {
                BuildBrickosaurusWrecksBossGate(bossGate);
            }
            else if (bossGate.BossType == BreakoutBossGateType.MainframeManiac)
            {
                BuildMainframeManiacBossGate(bossGate);
            }
            else
            {
                BuildPaddlePunkBossGate(bossGate);
            }

            PrepareServe(RoundState.ReadyToServe);

            powerUpService?.ShowStatusBanner(bossGate.HudLabel, new Color(1f, 0.18f, 0.23f, 1f), 2.4f);
        }

        private void ApplyBossGateTuning(BreakoutBossGate bossGate)
        {
            var persistentModifiers = GetPersistentRunUpgradeModifiers();
            currentLevelPaddleSpeed = paddleSpeed * Mathf.Lerp(1f, 1.08f, Mathf.Clamp01(bossGate.GateIndex / 2f));
            currentLevelBallSpeed = ballSpeed
                * (activeRunSettings?.BallSpeedMultiplier ?? 1f)
                * GetModeBallSpeedMultiplier()
                * BreakoutRunProgression.GetBossGateBallSpeedMultiplier(bossGate)
                * persistentModifiers.BallSpeedMultiplier;
            ApplyActiveEffects();
        }

        private void BuildPaddlePunkBossGate(BreakoutBossGate bossGate)
        {
            if (brickService == null)
            {
                requiredBricksRemaining = 0;
                return;
            }

            requiredBricksRemaining = 0;
            var weakPointDefinition = FindBrickDefinition("Tiny Brick") ?? FindBreakableBrickDefinition();
            var shieldDefinition = FindBrickDefinition("Steel Brick");
            var weakPointCount = Mathf.Clamp(4 + bossGate.GateIndex, 4, 6);
            var weakPointSpacing = Mathf.Min(1.18f, (arenaRight - arenaLeft - 1.5f) / Mathf.Max(1, weakPointCount - 1));
            var weakPointStartX = -weakPointSpacing * (weakPointCount - 1) * 0.5f;
            var weakPointY = arenaTop - 0.52f;

            for (var index = 0; index < weakPointCount; index++)
            {
                var position = new Vector2(weakPointStartX + (index * weakPointSpacing), weakPointY);
                var brick = brickService.CreateBrick(position, weakPointDefinition, 0, index, default);

                if (brick != null && brick.CountsTowardLevelCompletion)
                {
                    requiredBricksRemaining++;
                }
            }

            if (shieldDefinition != null)
            {
                var shieldY = arenaTop - 1.92f;
                var shieldOffset = 2.65f + (bossGate.GateIndex * 0.28f);
                RegisterPaddlePunkShieldBrick(brickService.CreateBrick(new Vector2(-shieldOffset, shieldY), shieldDefinition, 1, 0, default));
                RegisterPaddlePunkShieldBrick(brickService.CreateBrick(new Vector2(shieldOffset, shieldY), shieldDefinition, 1, 1, default));
            }

            CreatePaddlePunkBossActor(bossGate);
        }

        private void BuildBrickosaurusWrecksBossGate(BreakoutBossGate bossGate)
        {
            brickService?.ClearBricks();
            requiredBricksRemaining = 0;
            CreateBrickosaurusWrecksBossActor(bossGate);
        }

        private void BuildMainframeManiacBossGate(BreakoutBossGate bossGate)
        {
            brickService?.ClearBricks();
            requiredBricksRemaining = 0;
            CreateMainframeManiacBossActor(bossGate);
        }

        private void CreateBrickosaurusWrecksBossActor(BreakoutBossGate bossGate)
        {
            var bossObject = new GameObject("Brickosaurus Wrecks");
            bossObject.transform.SetParent(bossRoot != null ? bossRoot : runtimeRoot, false);
            bossObject.transform.position = Vector3.zero;

            activeBrickosaurusBoss = bossObject.AddComponent<BreakoutBrickosaurusWrecksBoss>();
            activeBrickosaurusBoss.Configure(
                this,
                bossRoot != null ? bossRoot : runtimeRoot,
                circleSprite,
                circleSprite,
                triangleSprite,
                powerUpSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                bounceMaterial,
                Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop),
                brickSize,
                bossGate.GateIndex,
                NextGameplayRandomFloat);
        }

        private void CreateMainframeManiacBossActor(BreakoutBossGate bossGate)
        {
            var bossObject = new GameObject("Mainframe Maniac");
            bossObject.transform.SetParent(bossRoot != null ? bossRoot : runtimeRoot, false);
            bossObject.transform.position = Vector3.zero;

            activeMainframeManiacBoss = bossObject.AddComponent<BreakoutMainframeManiacBoss>();
            activeMainframeManiacBoss.Configure(
                this,
                bossRoot != null ? bossRoot : runtimeRoot,
                squareSprite,
                circleSprite,
                powerUpSprite,
                spriteUnlitMaterial,
                additiveSpriteMaterial,
                bounceMaterial,
                Rect.MinMaxRect(arenaLeft, arenaBottom, arenaRight, arenaTop),
                brickSize,
                bossGate.GateIndex,
                NextGameplayRandomFloat);
        }

        private void CreatePaddlePunkBossActor(BreakoutBossGate bossGate)
        {
            var bossObject = new GameObject("The Paddle Punk");
            bossObject.transform.SetParent(bossRoot != null ? bossRoot : runtimeRoot, false);
            bossObject.transform.localScale = new Vector3(paddleSize.x * 1.12f, paddleSize.y * 0.82f, 1f);
            bossObject.transform.position = new Vector2(0f, arenaTop - 1.42f);

            var visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(bossObject.transform, false);

            var spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = bossPaddleSprite != null ? bossPaddleSprite : paddleSprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 11;
            spriteRenderer.sharedMaterial = spriteUnlitMaterial;
            BreakoutSpriteRendererUtility.NormalizeScale(spriteRenderer);

            var collider = bossObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            var body = bossObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            activePaddlePunkBoss = bossObject.AddComponent<BreakoutPaddlePunkBoss>();
            activePaddlePunkBoss.Configure(
                bossGate.GateIndex,
                arenaLeft,
                arenaRight,
                bossObject.transform.localScale.x * 0.5f,
                paddleSpeed * (0.72f + (bossGate.GateIndex * 0.08f)),
                ResolveBossTargetBallPosition,
                NextGameplayRandomFloat);
        }

        private void UpdateBossEncounter()
        {
            if (roundState != RoundState.Playing
                || activeBossGate == null
                || activePaddlePunkBoss == null
                || !activePaddlePunkBoss.TryConsumeShieldSpawnRequest())
            {
                return;
            }

            ShufflePaddlePunkShieldBrick(activeBossGate.Value);
        }

        private void ShufflePaddlePunkShieldBrick(BreakoutBossGate bossGate)
        {
            var shieldDefinition = FindBrickDefinition("Steel Brick");

            if (brickService == null || shieldDefinition == null)
            {
                return;
            }

            PrunePaddlePunkShieldBricks();

            var laneIndex = ResolveNextPaddlePunkShieldLane(bossGate);

            if (paddlePunkShieldBricks.Count > 0)
            {
                var departingBrick = paddlePunkShieldBricks[0];
                paddlePunkShieldBricks.RemoveAt(0);
                FadeOutAndRemovePaddlePunkShieldBrick(departingBrick);
            }

            bossShieldSpawnIndex++;
            var position = ResolvePaddlePunkShieldPosition(laneIndex, bossGate);
            var arrivingBrick = brickService.CreateBrick(position, shieldDefinition, 2, bossShieldSpawnIndex, default);
            RegisterPaddlePunkShieldBrick(arrivingBrick);
            FadeInPaddlePunkShieldBrick(arrivingBrick);
        }

        private void RegisterPaddlePunkShieldBrick(Brick brick)
        {
            if (brick == null)
            {
                return;
            }

            paddlePunkShieldBricks.Add(brick);
        }

        private void PrunePaddlePunkShieldBricks()
        {
            for (var index = paddlePunkShieldBricks.Count - 1; index >= 0; index--)
            {
                if (paddlePunkShieldBricks[index] == null)
                {
                    paddlePunkShieldBricks.RemoveAt(index);
                }
            }
        }

        private int ResolveNextPaddlePunkShieldLane(BreakoutBossGate bossGate)
        {
            for (var attempt = 0; attempt < PaddlePunkShieldLaneCount; attempt++)
            {
                var laneIndex = (bossShieldSpawnIndex + bossGate.GateIndex + attempt) % PaddlePunkShieldLaneCount;
                var lanePosition = ResolvePaddlePunkShieldPosition(laneIndex, bossGate);

                if (!HasPaddlePunkShieldNear(lanePosition))
                {
                    return laneIndex;
                }
            }

            return (bossShieldSpawnIndex + bossGate.GateIndex) % PaddlePunkShieldLaneCount;
        }

        private Vector2 ResolvePaddlePunkShieldPosition(int laneIndex, BreakoutBossGate bossGate)
        {
            var normalizedLane = PaddlePunkShieldLaneCount <= 1 ? 0.5f : laneIndex / (float)(PaddlePunkShieldLaneCount - 1);
            var x = Mathf.Lerp(arenaLeft + 1.1f, arenaRight - 1.1f, normalizedLane);
            var y = arenaTop - Mathf.Lerp(1.78f, 2.28f, bossGate.GateIndex / 2f);
            return new Vector2(x, y);
        }

        private bool HasPaddlePunkShieldNear(Vector2 position)
        {
            for (var index = 0; index < paddlePunkShieldBricks.Count; index++)
            {
                var brick = paddlePunkShieldBricks[index];

                if (brick != null && ((Vector2)brick.transform.position - position).sqrMagnitude < 0.16f)
                {
                    return true;
                }
            }

            return false;
        }

        private void FadeInPaddlePunkShieldBrick(Brick brick)
        {
            if (brick == null)
            {
                return;
            }

            var fadeAnimator = brick.GetComponent<BreakoutBrickFadeAnimator>() ?? brick.gameObject.AddComponent<BreakoutBrickFadeAnimator>();
            fadeAnimator.PlayFadeIn(PaddlePunkShieldFadeSeconds);
        }

        private void FadeOutAndRemovePaddlePunkShieldBrick(Brick brick)
        {
            if (brick == null)
            {
                return;
            }

            var fadeAnimator = brick.GetComponent<BreakoutBrickFadeAnimator>() ?? brick.gameObject.AddComponent<BreakoutBrickFadeAnimator>();
            fadeAnimator.PlayFadeOut(PaddlePunkShieldFadeSeconds, RemoveFadedPaddlePunkShieldBrick);
        }

        private void RemoveFadedPaddlePunkShieldBrick(Brick brick)
        {
            if (brick == null || brickService == null || !brickService.RemoveBrick(brick))
            {
                return;
            }

            brickService.DisableAndDestroyBrick(brick);
        }

        private Vector2? ResolveBossTargetBallPosition()
        {
            BallController bestBall = null;
            var bestY = float.NegativeInfinity;

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var candidate = activeBalls[index];

                if (candidate == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                if (candidate.transform.position.y > bestY)
                {
                    bestY = candidate.transform.position.y;
                    bestBall = candidate;
                }
            }

            if (bestBall != null)
            {
                return bestBall.transform.position;
            }

            return serveBall != null ? serveBall.transform.position : null;
        }

        private BrickDefinition FindBrickDefinition(string displayName)
        {
            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition != null && string.Equals(definition.DisplayName, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }

            return null;
        }

        private BrickDefinition FindBreakableBrickDefinition()
        {
            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition != null && definition.IsBreakable)
                {
                    return definition;
                }
            }

            return loadedBrickDefinitions.Count > 0 ? loadedBrickDefinitions[0] : null;
        }

        private void ClearBossEncounter()
        {
            activeBossGate = null;
            activePaddlePunkBoss = null;
            activeBrickosaurusBoss = null;
            activeMainframeManiacBoss = null;
            bossShieldSpawnIndex = 0;
            paddlePunkShieldBricks.Clear();

            if (bossRoot == null)
            {
                return;
            }

            for (var index = bossRoot.childCount - 1; index >= 0; index--)
            {
                DestroyRuntimeObject(bossRoot.GetChild(index).gameObject);
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

            if (activeBossGate.HasValue && activeBrickosaurusBoss != null)
            {
                levelCleared = activeBrickosaurusBoss.IsDefeated;
            }
            else if (activeBossGate.HasValue && activeMainframeManiacBoss != null)
            {
                levelCleared = activeMainframeManiacBoss.IsDefeated;
            }

            if (!levelCleared)
            {
                return;
            }

            if (activeBossGate.HasValue)
            {
                CompleteBossGate(activeBossGate.Value);
                return;
            }

            SetSimulationPaused(false);
            ClearPickups();
            StopAllBalls();
            stickyCaughtBall = null;
            audioService?.PlayLevelComplete();

            if (activeRunSettings != null
                && activeRunSettings.IsRogueMode
                && BreakoutRunProgression.TryGetBossGateAfterLevel(currentLevelIndex, out var bossGate))
            {
                pendingBossGate = bossGate;

                if (TryOpenUpgradeDraft())
                {
                    return;
                }

                LoadBossGate(bossGate);
                return;
            }

            if (HasNextLevel() && TryOpenUpgradeDraft())
            {
                return;
            }

            roundState = RoundState.LevelComplete;
            selectedOverlayActionIndex = 0;
            RecordRogueRunResult(completed: !HasNextLevel());
        }

        private void CompleteBossGate(BreakoutBossGate bossGate)
        {
            SetSimulationPaused(false);
            ClearPickups();
            StopAllBalls();
            stickyCaughtBall = null;
            audioService?.PlayLevelComplete();
            powerUpService?.ShowStatusBanner(
                bossGate.BossType switch
                {
                    BreakoutBossGateType.BrickosaurusWrecks => "WRECKED!",
                    BreakoutBossGateType.MainframeManiac => "FATAL ERROR!",
                    _ => "PUNK WIPED!",
                },
                new Color(1f, 0.18f, 0.23f, 1f),
                2.2f);
            ClearBossEncounter();

            if (bossGate.TriggerLevelIndex >= BreakoutRunProgression.TargetLevelCount - 1)
            {
                roundState = RoundState.LevelComplete;
                selectedOverlayActionIndex = 0;
                RecordRogueRunResult(completed: true);
                return;
            }

            LoadLevel(bossGate.TriggerLevelIndex + 1, RoundState.ReadyToServe);
        }

        private bool HasNextLevel()
        {
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

            var message = roundState switch
            {
                RoundState.ReadyToServe => "Press Space to launch the ball. Up/Down tunes speed.",
                RoundState.LifeLost => GetLifeLossScorePenalty() > 0
                    ? UsesHighScoreMode()
                        ? $"Ball lost. -{GetLifeLossScorePenalty():0000} score. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                        : $"Life lost. -{GetLifeLossScorePenalty():0000} score. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed."
                    : UsesHighScoreMode()
                        ? $"Ball lost. Losses {lifeLossCount:00}. Press Space to serve again. Up/Down tunes speed."
                        : $"Life lost. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.",
                _ => string.Empty,
            };
            uiRenderer.DrawMessageOverlay(message);
            uiRenderer.DrawPickupBanner(BuildPickupBannerView());
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
            var unlockedPaddles = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();
            var nextPaddleUnlock = BreakoutRoguePaddleCatalog.GetNextLockedPaddle();
            var nextPaddleUnlockRequirement = BreakoutRoguePaddleCatalog.GetUnlockRequirement(nextPaddleUnlock);
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
                SelectedRoguePaddleLabel = selectedPaddle.DisplayName,
                SelectedRoguePaddleIdentity = selectedPaddle.Identity,
                SelectedRoguePaddleStrength = selectedPaddle.Strength,
                SelectedRoguePaddleDrawback = selectedPaddle.Drawback,
                NextRoguePaddleUnlockLabel = string.IsNullOrWhiteSpace(nextPaddleUnlock.DisplayName)
                    ? string.Empty
                    : nextPaddleUnlock.DisplayName,
                NextRoguePaddleUnlockRequirementLabel = string.IsNullOrWhiteSpace(nextPaddleUnlockRequirement.DisplayName)
                    ? BreakoutRoguePaddleCatalog.DefaultPaddle.DisplayName
                    : nextPaddleUnlockRequirement.DisplayName,
                UnlockedRoguePaddleCount = unlockedPaddles.Length,
                TotalRoguePaddleCount = BreakoutRoguePaddleCatalog.AllPaddles.Count,
            };

            return mainMenuService.BuildView(context);
        }

        private BreakoutUiRunSetupView BuildRunSetupView()
        {
            var previewSettings = BuildRunSettingsFromPending(out var previewValidation);
            return new BreakoutUiRunSetupView
            {
                Title = "Run Setup",
                Subtitle = "Dial in the cabinet before launch. The same Tape ID preserves the run while score mode, modifiers, and palette reshape the pressure curve.",
                FieldLines = new[]
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
                    $"Theme: {previewSettings.ThemeLabel}",
                },
                SelectedFieldIndex = (int)selectedRunSetupField,
                PreviewLine = $"Preview: {BuildScoreModeSummaryLabel(previewSettings)} | {BuildRetrySummaryLabel(previewSettings)} | Paddle x{previewSettings.PaddleWidthMultiplier:0.00} | Ball speed x{previewSettings.BallSpeedMultiplier:0.00} | Brick durability x{previewSettings.BrickDurabilityMultiplier:0.00} | Drops {BuildDropSummaryLabel(previewSettings)}",
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
            var upgradeSelected = currentUpgrade != null && developerLaunchState.IsUpgradeSelected(currentUpgrade);
            var dropSelected = currentDrop != null && developerLaunchState.IsDropUnlockSelected(currentDrop);

            return new BreakoutUiRunSetupView
            {
                Title = "Dev Mode",
                Subtitle = "Local Rogue jump bench. Pick an encounter, seed a build, and launch straight into the test target.",
                FieldLines = new[]
                {
                    $"Encounter: {FormatDeveloperEncounterLabel(encounter)}",
                    $"Paddle: {selectedPaddle.DisplayName}",
                    $"Lives: {developerLaunchState.LivesRemaining}",
                    $"Heat: {developerLaunchState.Intensity:00}/{BreakoutRunProgression.MaxRogueIntensity:00}",
                    $"Upgrade: {FormatDeveloperToggle(upgradeSelected)} {FormatDeveloperUpgradeLabel(currentUpgrade)}",
                    $"Drop Unlock: {FormatDeveloperToggle(dropSelected)} {FormatDeveloperDropLabel(currentDrop)}",
                },
                SelectedFieldIndex = (int)selectedDeveloperLaunchField,
                PreviewLine = $"Preview: {FormatDeveloperEncounterLabel(encounter)} | {selectedPaddle.DisplayName} | Heat {developerLaunchState.Intensity:00} | Balls {developerLaunchState.LivesRemaining:00} | Paddle x{selectedPaddle.WidthMultiplier:0.00} speed x{selectedPaddle.SpeedMultiplier:0.00} | Build {developerLaunchState.SelectedUpgradeCount:00} upgrades, {developerLaunchState.SelectedDropUnlockCount:00} drops | Theme {ResolvePendingThemeDefinition()?.DisplayName ?? "Fallback"}",
                ValidationText = "Encounter cycles through Stage 01-10, then Boss Gate 1-3. Dev runs do not update the saved Rogue result.",
                HintText = "Up/Down selects. Left/Right changes. T toggles build picks. N clears build. Esc returns to menu. Space launches.",
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
            return new BreakoutUiHudView
            {
                TopLine = $"{GetScoreDisplayLabel().ToUpperInvariant()} {FormatScoreValue(score)}   {GetLifeCounterLabel().ToUpperInvariant()} {GetLifeCounterValue():00}   {BuildLevelLabel().ToUpperInvariant()}",
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
                Subtitle = pendingBossGate.HasValue
                    ? $"Level {currentLevelIndex + 1:00} cleared. Choose one reward before {pendingBossGate.Value.HudLabel}."
                    : $"Level {currentLevelIndex + 1:00} cleared. Choose one reward for the rest of this run.",
                BuildLine = BuildUpgradeSummaryLabel(4),
                Options = optionViews,
                SelectedOptionIndex = Mathf.Clamp(selectedUpgradeDraftIndex, 0, optionViews.Length - 1),
                HintText = "Left/Right selects. Space confirms. Clicking a card also takes it. R abandons the run and returns to setup.",
            };
        }

        private BreakoutUiOverlayView BuildEndStateOverlayView()
        {
            var isGameOver = roundState == RoundState.GameOver;
            var title = isGameOver
                ? (activeRunSettings != null && activeRunSettings.IsRogueMode ? "Rogue Wiped Out" : "Run Over")
                : HasNextLevel()
                    ? "Level Cleared"
                    : (activeRunSettings != null && activeRunSettings.IsRogueMode ? "Mixtape Cleared" : "Final Layout Cleared");
            var summary = isGameOver
                ? $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | Reached {BuildLevelLabel()} | Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}"
                : HasNextLevel()
                    ? $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Next up: level {currentLevelIndex + 2:00}"
                    : $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}";
            var footer = isGameOver
                ? activeRunSettings != null && activeRunSettings.IsRogueMode
                    ? "Restart the run or return to the main menu."
                    : "Restart the run, jump back to setup, or return to the main menu."
                : HasNextLevel()
                    ? "Advance to the next stage, restart the run, or return to the menu."
                    : activeRunSettings != null && activeRunSettings.IsRogueMode
                        ? "The 10-stage mixtape is complete. Restart or head back to the menu."
                        : "The 10-stage run is complete. Restart, tune a new setup, or head back to the menu.";
            return new BreakoutUiOverlayView
            {
                Title = title,
                SummaryLines = new[]
                {
                    summary,
                    BuildRunSummaryLabel(),
                },
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(roundState)),
                SelectedActionIndex = selectedOverlayActionIndex,
                FooterLines = new[] { footer },
                IsCompact = false,
            };
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
                    Description = $"Adds {dropUnlock.DisplayName} capsules to this Rogue run's drop pool.",
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

        private void ToggleDiagnosticsOverlay()
        {
            isDiagnosticsOverlayVisible = !isDiagnosticsOverlayVisible;
        }

        private string BuildLevelLabel()
        {
            if (activeBossGate.HasValue)
            {
                return activeBossGate.Value.DisplayName;
            }

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
            return UsesHighScoreMode() ? "Losses" : "Lives";
        }

        private int GetLifeCounterValue()
        {
            return UsesHighScoreMode() ? lifeLossCount : livesRemaining;
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

            return settings.ScoringMode == RunScoringMode.HighScore
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

            if (!activeBossGate.HasValue)
            {
                return $"Remaining Bricks {requiredBricksRemaining:00}";
            }

            if (activeBrickosaurusBoss != null)
            {
                var bodyCount = activeBrickosaurusBoss.ConnectedBodySegmentCount;
                var headLabel = activeBrickosaurusBoss.IsHeadVulnerable
                    ? $"HEAD {activeBrickosaurusBoss.HeadHitsRemaining:0}"
                    : $"BODY {bodyCount:00}";
                return $"{headLabel} | {activeBrickosaurusBoss.PhaseLabel}";
            }

            if (activeMainframeManiacBoss != null)
            {
                var coreLabel = activeMainframeManiacBoss.IsCoreVulnerable
                    ? $"CORE {activeMainframeManiacBoss.CoreHitsRemaining:0}"
                    : $"DATA {activeMainframeManiacBoss.DataBanksRemaining:00}";
                return $"{coreLabel} | {activeMainframeManiacBoss.PhaseLabel}";
            }

            var phaseLabel = activePaddlePunkBoss != null ? activePaddlePunkBoss.PhaseLabel : activeBossGate.Value.HudLabel;
            return $"Wall Core {requiredBricksRemaining:00} | {phaseLabel}";
        }

        private string BuildGameplayStatusLine()
        {
            var progressLabel = BuildRemainingBricksLabel();

            if (activeRunState == null || !activeRunState.HasActiveBuild)
            {
                return progressLabel;
            }

            return $"{progressLabel} | {BuildUpgradeSummaryLabel(2)}";
        }

        private string BuildPauseSummaryLabel()
        {
            return $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Balls {Mathf.Max(0, activeBalls.Count):00} | {BuildLevelLabel()}";
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
            var scoreMultiplier = speedScoreMultiplier * Mathf.Max(0.1f, activeEffectModifiers.ScoreMultiplier);
            return
                $"Ball Speed {currentSpeed:0.00} | Score x{scoreMultiplier:0.00} | Base {baseSpeed:0.00} | Manual x{manualBallSpeedMultiplier:0.00} | Cap {maxSpeed:0.00}";
        }

        private string BuildRoundStateLabel()
        {
            return roundState switch
            {
                RoundState.ReadyToServe => "Ready to serve",
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

            var heatSummary = activeRunSettings.IsRogueMode ? $" | Heat {activeRunSettings.RogueIntensity:00}" : string.Empty;
            var summary =
                $"{activeRunSettings.GameModeLabel} | Tape ID: {activeRunSettings.Seed} | {activeRunSettings.DifficultyLabel} | {BuildScoreModeSummaryLabel(activeRunSettings)} | {BuildRetrySummaryLabel(activeRunSettings)} | Balls/Serve {GetEffectiveBallsPerServe()} | " +
                $"Theme: {activeRunSettings.ThemeLabel} | Drops: {BuildDropSummaryLabel(activeRunSettings)}{heatSummary} | {activeRunSettings.SelectedPaddleLabel} x{activeRunSettings.PaddleWidthMultiplier:0.00} speed x{activeRunSettings.PaddleSpeedMultiplier:0.00} | Ball x{activeRunSettings.BallSpeedMultiplier:0.00} | Build {GetChosenUpgradeCount():00}";
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
            return encounter.IsBossGate
                ? $"[BOSS] {encounter.DisplayName}"
                : $"[STAGE] {encounter.DisplayName}";
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

        private void UpdateTimedEffects()
        {
            powerUpService?.UpdateTimedEffects(roundState == RoundState.Playing, Time.deltaTime, ApplyActiveEffects);
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
                this);

            if (spawnedPickup != null)
            {
                audioService?.PlayPickupDropped();
            }

            ApplyVisualEffectState();
        }

        private PowerUpDefinition ResolveBrickosaurusPowerDownDefinition()
        {
            var hazardCount = 0;

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (IsBrickosaurusPowerDownCandidate(definition))
                {
                    hazardCount++;
                }
            }

            if (hazardCount == 0)
            {
                return null;
            }

            var selectedHazardIndex = Mathf.FloorToInt(NextGameplayRandomFloat(0f, hazardCount));
            selectedHazardIndex = Mathf.Clamp(selectedHazardIndex, 0, hazardCount - 1);
            var seenHazards = 0;

            for (var index = 0; index < loadedPowerUpDefinitions.Count; index++)
            {
                var definition = loadedPowerUpDefinitions[index];

                if (!IsBrickosaurusPowerDownCandidate(definition))
                {
                    continue;
                }

                if (seenHazards == selectedHazardIndex)
                {
                    return definition;
                }

                seenHazards++;
            }

            return null;
        }

        private static bool IsBrickosaurusPowerDownCandidate(PowerUpDefinition definition)
        {
            if (definition == null || definition.IsBeneficial)
            {
                return false;
            }

            return definition.EffectType == PowerUpEffectType.BallSpeedMultiplier
                || definition.EffectType == PowerUpEffectType.WavyPaddle
                || definition.EffectType == PowerUpEffectType.ReverseControls
                || definition.EffectType == PowerUpEffectType.GravityWell
                || definition.EffectType == PowerUpEffectType.LagSpike;
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
                    0f);
            paddle.SetMoveSpeed(currentLevelPaddleSpeed * (activeRunSettings?.PaddleSpeedMultiplier ?? 1f));
            var paddleHitMaximumWidth = paddle.SetWidthMultiplier(activeEffectModifiers.PaddleWidthMultiplier);

            if (paddleHitMaximumWidth && TryBreakWidePaddle())
            {
                ApplyActiveEffects();
                TriggerWidePaddleBreakFeedback();
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

            if (stickyCaughtBall != null && !activeEffectModifiers.StickyPaddleEnabled)
            {
                ReleaseStickyCaughtBall();
            }
        }

        private bool TryBreakWidePaddle()
        {
            return powerUpService != null && powerUpService.RemoveBeneficialPaddleWidthEffects() > 0;
        }

        private void TriggerWidePaddleBreakFeedback()
        {
            paddle?.StartBreakWiggle();
            audioService?.PlayPowerDown();
            powerUpService?.ShowStatusBanner("RAIL BUSTED!", new Color(1f, 0.28f, 0.32f, 1f), 1.4f);
        }

        private void SpawnMultiBall(PowerUpDefinition powerUpDefinition)
        {
            if (roundState != RoundState.Playing || activeBalls.Count == 0)
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

        private bool UsesHighScoreMode()
        {
            return activeRunSettings != null && activeRunSettings.ScoringMode == RunScoringMode.HighScore;
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
                activeEffectModifiers.ScoreMultiplier);
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

            activeRunState.RegisterLevelClear();
            if (activeRunSettings.IsRogueMode)
            {
                rogueRunController?.UnlockHazardsForClearedLevel(activeRunState);
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

            if (pendingBossGate.HasValue)
            {
                LoadBossGate(pendingBossGate.Value);
                return;
            }

            if (HasNextLevel())
            {
                LoadLevel(currentLevelIndex + 1, RoundState.ReadyToServe);
            }
            else
            {
                roundState = RoundState.LevelComplete;
                selectedOverlayActionIndex = 0;
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
                ? $"Adds {dropUnlock.DisplayName} capsules to this Rogue run's drop pool."
                : "This drop unlock failed to load.";
        }

        private string BuildDropUnlockDetail(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return "Drop Unlock | Unavailable";
            }

            var polarity = definition.IsBeneficial ? "Helpful Drop" : "Hazard Drop";
            return $"Drop Unlock | {polarity} | {BuildPowerUpMechanicalSummary(definition)}";
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
            return themeService != null
                ? themeService.ResolvePowerUpStyle(definition).Sprite
                : powerUpSprite;
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
            if (roundState != RoundState.Playing
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
