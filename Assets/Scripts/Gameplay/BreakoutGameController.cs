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
        private const float ShieldWallYOffset = 0.38f;

        private enum RoundState
        {
            MainMenu,
            RunSetup,
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
            StartRun,
            OpenRunSetup,
            ResetSetupDefaults,
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
        private readonly List<BallController> activeBalls = new List<BallController>();
        private readonly List<Sprite> loadedBackgroundSprites = new List<Sprite>();
        private readonly List<Texture2D> loadedBackgroundTextures = new List<Texture2D>();
        private readonly List<Sprite> runtimeGeneratedBackgroundSprites = new List<Sprite>();
        private readonly List<SpriteRenderer> wallRenderers = new List<SpriteRenderer>();

        private Camera activeCamera;
        private Transform runtimeRoot;
        private Transform backgroundRoot;
        private Transform wallsRoot;
        private Transform ballsRoot;
        private Transform bricksRoot;
        private Transform pickupsRoot;
        private SpriteRenderer backgroundSpriteRenderer;
        private SpriteRenderer backgroundHazeRenderer;
        private SpriteRenderer backgroundScanlineRenderer;
        private PaddleController paddle;
        private Collider2D paddleCollider;
        private SpriteRenderer paddleSpriteRenderer;
        private BallController serveBall;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Sprite backgroundHazeSprite;
        private Sprite backgroundScanlineSprite;
        private Sprite ballSprite;
        private Sprite brickSprite;
        private Sprite paddleSprite;
        private Sprite powerUpSprite;
        private PhysicsMaterial2D bounceMaterial;
        private Material spriteUnlitMaterial;
        private Material additiveSpriteMaterial;
        private VolumeProfile runtimeVolumeProfile;
        private BreakoutThemeService themeService;
        private BreakoutPowerUpService powerUpService;
        private BreakoutUiRenderer uiRenderer;
        private BreakoutRunState activeRunState;
        private BreakoutUpgradeDraftService upgradeDraftService;
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
        private LevelCompletionRule currentLevelCompletionRule = LevelCompletionRule.ClearRequiredBricks;
        private int currentLevelTargetScore;
        private int levelScore;
        private float currentLevelBallSpeed;
        private float currentLevelPaddleSpeed;
        private int ballInstanceCounter;
        private BreakoutLevelPlanner levelPlanner;
        private BreakoutRunSetupState pendingRunSetup;
        private RunSettings activeRunSettings;
        private DeterministicRandomService gameplayRandom;
        private BreakoutRunSetupField selectedRunSetupField;
        private RoundState pausedFromState;
        private int selectedOverlayActionIndex;
        private string currentLevelVariationLabel = "Variation: not started";
        private string pendingValidationMessage = string.Empty;
        private float manualBallSpeedMultiplier = 1f;
        private int manualBallSpeedHoldDirection;
        private float manualBallSpeedHoldTimer;
        private bool isDiagnosticsOverlayVisible;
        private int selectedUpgradeDraftIndex;
        private BreakoutEffectModifiers activeEffectModifiers;
        private BallController stickyCaughtBall;
        private SpriteRenderer shieldWallRenderer;
        private int shieldWallCharges;
        private float laserShotCooldownTimer;

        public Collider2D PaddleCollider => paddleCollider;

        private void Awake()
        {
            LoadBrickDefinitions();
            LoadLevelDefinitions();
            LoadThemeDefinitions();
            LoadRunUpgradeDefinitions();
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
            uiRenderer = new BreakoutUiRenderer();
            activeRunState = new BreakoutRunState();
            upgradeDraftService = new BreakoutUpgradeDraftService(loadedRunUpgradeDefinitions);
            CreateRuntimeRoots();
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

            if (backgroundHazeSprite != null)
            {
                Destroy(backgroundHazeSprite);
            }

            if (backgroundScanlineSprite != null)
            {
                Destroy(backgroundScanlineSprite);
            }

            for (var index = runtimeGeneratedBackgroundSprites.Count - 1; index >= 0; index--)
            {
                if (runtimeGeneratedBackgroundSprites[index] != null)
                {
                    Destroy(runtimeGeneratedBackgroundSprites[index]);
                }
            }

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

            if (runtimeVolumeProfile != null)
            {
                Destroy(runtimeVolumeProfile);
            }
        }

        private void Update()
        {
            UpdateTimedEffects();
            UpdatePickupBanner();
            laserShotCooldownTimer = Mathf.Max(0f, laserShotCooldownTimer - Time.deltaTime);

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
                EnterRunSetup();
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
            var explosionCenter = (Vector2)brick.transform.position;

            if (!bricks.Remove(brick))
            {
                return;
            }

            if (shouldExplode && destructionCause == BrickDestructionCause.Impact && scoringBall != null)
            {
                scoringBall.ApplySpeedBurst(brickDefinition.ExplosionSpeedMultiplier, brickDefinition.ExplosionSpeedDuration);
            }

            var awardedScore = CalculateBrickScore(brick, scoringBall);
            score += awardedScore;
            levelScore += awardedScore;
            TrySpawnPickup(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining - 1);
            }

            brick.gameObject.SetActive(false);
            Destroy(brick.gameObject);

            if (shouldExplode)
            {
                DestroyBricksInExplosionRadius(explosionCenter, brickDefinition.ExplosionRadius, brick, scoringBall);
            }

            TryTriggerChainLightning(explosionCenter, brick, scoringBall, destructionCause);

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
                if (isServeBall)
                {
                    lostBall.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(lostBall.gameObject);
                }

                return;
            }

            if (UsesHighScoreMode())
            {
                lifeLossCount += 1;
                ApplyLifeLossScorePenalty();

                if (!isServeBall)
                {
                    Destroy(lostBall.gameObject);
                }

                PrepareServe(RoundState.LifeLost);
                return;
            }

            livesRemaining = Mathf.Max(0, livesRemaining - 1);
            ApplyLifeLossScorePenalty();

            if (livesRemaining <= 0)
            {
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                ClearPickups();
                if (!isServeBall)
                {
                    Destroy(lostBall.gameObject);
                }

                return;
            }

            if (!isServeBall)
            {
                Destroy(lostBall.gameObject);
            }

            PrepareServe(RoundState.LifeLost);
        }

        public bool TryRescueBallWithShield(BallController ball)
        {
            if (roundState != RoundState.Playing || ball == null || shieldWallCharges <= 0)
            {
                return false;
            }

            shieldWallCharges = Mathf.Max(0, shieldWallCharges - 1);
            ball.BounceFromShield(arenaBottom + ShieldWallYOffset + 0.12f);
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

        public void HandlePickupCaught(PowerUpPickup pickup)
        {
            if (roundState != RoundState.Playing || pickup == null)
            {
                return;
            }

            powerUpService.RemovePickup(pickup);
            ApplyPowerUp(pickup.Definition);
            pickup.gameObject.SetActive(false);
            Destroy(pickup.gameObject);
        }

        public void HandlePickupMissed(PowerUpPickup pickup)
        {
            if (pickup == null)
            {
                return;
            }

            powerUpService.RemovePickup(pickup);
            pickup.gameObject.SetActive(false);
            Destroy(pickup.gameObject);
        }

        private void StartNewRun()
        {
            if (activeRunSettings == null)
            {
                activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
            }

            ApplyTheme(activeRunSettings.ThemeDefinition);
            isDiagnosticsOverlayVisible = false;
            manualBallSpeedMultiplier = 1f;
            SetSimulationPaused(false);
            selectedOverlayActionIndex = 0;
            gameplayRandom = new DeterministicRandomService(activeRunSettings.Seed);
            livesRemaining = activeRunSettings.StartingLives;
            lifeLossCount = 0;
            score = 0;
            currentLevelIndex = 0;
            currentLevelVariationLabel = "Variation: pending";
            shieldWallCharges = 0;
            laserShotCooldownTimer = 0f;
            stickyCaughtBall = null;
            activeRunState?.Reset();
            ClearTimedEffects();
            ClearPickups();
            UpdateShieldWallVisual();
            LoadLevel(currentLevelIndex, RoundState.ReadyToServe);

            Debug.Log(
                $"Starting run | seed {activeRunSettings.Seed} | preset {activeRunSettings.DifficultyLabel} | " +
                $"score mode {activeRunSettings.ScoringModeLabel} | life loss penalty {activeRunSettings.LifeLossScorePenalty} | " +
                $"balls/serve {activeRunSettings.BallsPerServe} | paddle x{activeRunSettings.PaddleWidthMultiplier:0.00} | " +
                $"ball speed x{activeRunSettings.BallSpeedMultiplier:0.00} | brick durability x{activeRunSettings.BrickDurabilityMultiplier:0.00} | " +
                $"drops {activeRunSettings.DropPoolLabel} | theme {activeRunSettings.ThemeLabel}");
        }

        private void EnterMainMenu()
        {
            roundState = RoundState.MainMenu;
            selectedOverlayActionIndex = 0;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            isDiagnosticsOverlayVisible = false;
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
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
            activeRunState?.Reset();
            ResetRuntimeForMetaFlow();
            ApplyPendingThemePreview();
        }

        private void ResetRuntimeForMetaFlow()
        {
            SetSimulationPaused(false);
            manualBallSpeedMultiplier = 1f;
            ClearBricks();
            ClearPickups();
            ClearTimedEffects();
            StopAllBalls();
            DestroyAdditionalBalls();
            activeBalls.Clear();
            activeRunState?.ClearPendingDraftOffers();
            selectedUpgradeDraftIndex = 0;
            stickyCaughtBall = null;
            shieldWallCharges = 0;
            laserShotCooldownTimer = 0f;
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
                EnterRunSetup();
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

        private OverlayAction[] GetOverlayActionsForState(RoundState state)
        {
            return state switch
            {
                RoundState.MainMenu => new[]
                {
                    OverlayAction.StartRun,
                    OverlayAction.OpenRunSetup,
                    OverlayAction.ResetSetupDefaults,
                },
                RoundState.Paused => new[]
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
                    : new[]
                    {
                        OverlayAction.RestartRun,
                        OverlayAction.ReturnToRunSetup,
                        OverlayAction.ReturnToMainMenu,
                    },
                RoundState.GameOver => new[]
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
                case OverlayAction.StartRun:
                    activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
                    SavePersistedRunSetup();
                    StartNewRun();
                    break;
                case OverlayAction.OpenRunSetup:
                case OverlayAction.ReturnToRunSetup:
                    EnterRunSetup();
                    break;
                case OverlayAction.ResetSetupDefaults:
                    ResetPendingRunSetup(generateNewSeed: true);
                    ApplyPendingThemePreview();
                    SavePersistedRunSetup();
                    pendingValidationMessage = "Run setup reset to defaults.";
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

        private void AdjustSelectedSetupField(int direction)
        {
            if (pendingRunSetup == null)
            {
                return;
            }

            pendingRunSetup.AdjustField(selectedRunSetupField, direction, GenerateSeed, ShiftThemeId);
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

        private void LoadLevel(int levelIndex, RoundState serveState)
        {
            ClearBricks();
            ClearPickups();
            stickyCaughtBall = null;
            ClearTimedEffects();

            if (loadedLevels.Count == 0 || levelIndex < 0)
            {
                currentLevel = null;
                currentLevelIndex = 0;
                currentLevelDisplayName = "No levels loaded";
                currentLevelCompletionRule = LevelCompletionRule.ClearRequiredBricks;
                currentLevelTargetScore = 0;
                levelScore = 0;
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
                currentLevelCompletionRule = LevelCompletionRule.ClearRequiredBricks;
                currentLevelTargetScore = 0;
                levelScore = 0;
                requiredBricksRemaining = 0;
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                StopAllBalls();
                UpdateBackgroundVisuals();
                return;
            }

            UpdateBackgroundVisuals();
            levelScore = 0;
            var levelPlan = BuildLevelLayoutPlan(currentLevel);
            currentLevelDisplayName = string.IsNullOrWhiteSpace(levelPlan.DisplayName)
                ? currentLevel.DisplayName
                : levelPlan.DisplayName;
            currentLevelCompletionRule = levelPlan.CompletionRule;
            currentLevelTargetScore = levelPlan.TargetScore;

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
                currentLevelBallSpeed = ballSpeed * persistentModifiers.BallSpeedMultiplier;
                ApplyActiveEffects();
                return;
            }

            currentLevelPaddleSpeed = paddleSpeed * Mathf.Max(0.5f, levelPlan.PaddleSpeedMultiplier);
            currentLevelBallSpeed = ballSpeed
                * Mathf.Max(0.5f, levelPlan.BallSpeedMultiplier)
                * (activeRunSettings?.BallSpeedMultiplier ?? 1f)
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
            squareSprite = CreateSquareSprite();
            circleSprite = CreateCircleSprite();
            backgroundHazeSprite = CreateBackgroundHazeSprite();
            backgroundScanlineSprite = CreateBackgroundScanlineSprite();
            LoadBackgroundLibrary();
            ballSprite = LoadSpriteResource(BallSpriteResourcePath, circleSprite);
            brickSprite = LoadSpriteResource(BrickSpriteResourcePath, squareSprite);
            paddleSprite = LoadSpriteResource(PaddleSpriteResourcePath, squareSprite);
            powerUpSprite = LoadSpriteResource(PowerUpSpriteResourcePath, squareSprite);
            spriteUnlitMaterial = CreateSpriteUnlitMaterial();
            additiveSpriteMaterial = CreateAdditiveSpriteMaterial();
            bounceMaterial = new PhysicsMaterial2D("PrototypeBounce")
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
            runtimeRoot = new GameObject("Runtime Prototype").transform;
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
        }

        private void CreateBackground()
        {
            var backgroundObject = new GameObject("Backdrop");
            backgroundObject.transform.SetParent(backgroundRoot, false);
            backgroundObject.transform.position = new Vector3(0f, 0f, 0f);

            backgroundSpriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundSpriteRenderer.sortingOrder = -100;
            backgroundSpriteRenderer.sharedMaterial = spriteUnlitMaterial;

            var hazeObject = new GameObject("Backdrop Haze");
            hazeObject.transform.SetParent(backgroundRoot, false);
            backgroundHazeRenderer = hazeObject.AddComponent<SpriteRenderer>();
            backgroundHazeRenderer.sprite = backgroundHazeSprite;
            backgroundHazeRenderer.sortingOrder = -99;
            backgroundHazeRenderer.sharedMaterial = spriteUnlitMaterial;

            var scanlineObject = new GameObject("Backdrop Scanlines");
            scanlineObject.transform.SetParent(backgroundRoot, false);
            backgroundScanlineRenderer = scanlineObject.AddComponent<SpriteRenderer>();
            backgroundScanlineRenderer.sprite = backgroundScanlineSprite;
            backgroundScanlineRenderer.sortingOrder = -98;
            backgroundScanlineRenderer.sharedMaterial = spriteUnlitMaterial;

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
            shieldObject.transform.localScale = new Vector3((arenaRight - arenaLeft) - 0.3f, 0.16f, 1f);

            shieldWallRenderer = shieldObject.AddComponent<SpriteRenderer>();
            shieldWallRenderer.sprite = squareSprite;
            shieldWallRenderer.sharedMaterial = additiveSpriteMaterial;
            shieldWallRenderer.sortingOrder = 12;
            shieldWallRenderer.enabled = false;
        }

        private void CreatePaddle()
        {
            var paddleObject = new GameObject("Paddle");
            paddleObject.transform.SetParent(runtimeRoot, false);
            paddleObject.transform.localScale = new Vector3(paddleSize.x, paddleSize.y, 1f);

            var paddleVisual = new GameObject("Visual");
            paddleVisual.transform.SetParent(paddleObject.transform, false);

            paddleSpriteRenderer = paddleVisual.AddComponent<SpriteRenderer>();
            paddleSpriteRenderer.sprite = paddleSprite;
            paddleSpriteRenderer.color = paddleColor;
            paddleSpriteRenderer.sortingOrder = 10;
            paddleSpriteRenderer.sharedMaterial = spriteUnlitMaterial;
            NormalizeSpriteRendererScale(paddleSpriteRenderer);

            paddleCollider = paddleObject.AddComponent<BoxCollider2D>();
            paddleCollider.sharedMaterial = bounceMaterial;

            var rigidbody = paddleObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            paddle = paddleObject.AddComponent<PaddleController>();
            paddle.Configure(
                this,
                paddleSpeed,
                arenaLeft,
                arenaRight,
                arenaBottom + paddleFloorOffset);
        }

        private BallController CreateBall(bool followsPaddleWhenIdle)
        {
            ballInstanceCounter++;

            var ballName = followsPaddleWhenIdle ? "Ball" : $"Ball {ballInstanceCounter}";
            var ballObject = new GameObject(ballName);
            ballObject.transform.SetParent(ballsRoot, false);
            ballObject.transform.localScale = Vector3.one * (ballRadius * 2f);

            var ballStyle = themeService != null
                ? themeService.ResolveBallStyle()
                : new ThemeVisualStyle(ballColor, ballColor, circleSprite);
            var spriteRenderer = ballObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ballStyle.Sprite;
            spriteRenderer.color = ballStyle.PrimaryColor;
            spriteRenderer.sortingOrder = 20;
            spriteRenderer.sharedMaterial = additiveSpriteMaterial;

            var collider = ballObject.AddComponent<CircleCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            var rigidbody = ballObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.linearDamping = 0f;

            var ball = ballObject.AddComponent<BallController>();
            ball.Configure(
                this,
                paddle,
                GetCurrentBallSpeed(),
                minimumVerticalDirection,
                arenaBottom - 1f,
                ballRadius + (paddleSize.y * 0.5f) + 0.05f,
                followsPaddleWhenIdle);
            ball.SetPhaseThroughBricks(activeEffectModifiers.PhaseBallEnabled);
            ball.SetGravityWell(new Vector2(0f, (arenaTop + arenaBottom) * 0.5f), activeEffectModifiers.GravityWellStrength);

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
            requiredBricksRemaining = 0;

            if (layoutPlan == null || layoutPlan.BrickRows.Length == 0)
            {
                return;
            }

            var layoutRows = layoutPlan.BrickRows;
            var startY = arenaTop - layoutPlan.TopInset;

            for (var row = 0; row < layoutRows.Length; row++)
            {
                var rowCells = layoutRows[row] ?? Array.Empty<BreakoutProceduralBrickCell>();
                var totalWidth = (rowCells.Length * brickSize.x) + (Mathf.Max(0, rowCells.Length - 1) * brickSpacing.x);
                var startX = (-totalWidth * 0.5f) + (brickSize.x * 0.5f);

                for (var column = 0; column < rowCells.Length; column++)
                {
                    var cell = rowCells[column];

                    if (cell == null || cell.Definition == null)
                    {
                        continue;
                    }

                    var position = new Vector2(
                        startX + (column * (brickSize.x + brickSpacing.x)),
                        startY - (row * (brickSize.y + brickSpacing.y)));

                    CreateBrick(position, cell.Definition, row, column, GetEffectiveBrickHitPoints(cell.Definition), cell.MotionConfig);
                }
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

        private void CreateBrick(
            Vector2 position,
            BrickDefinition definition,
            int row,
            int column,
            int effectiveHitPoints,
            BreakoutBrickMotionConfig motionConfig)
        {
            var brickObject = new GameObject($"{definition.DisplayName} {row + 1}-{column + 1}");
            brickObject.transform.SetParent(bricksRoot, false);
            brickObject.transform.position = position;
            brickObject.transform.localScale = new Vector3(brickSize.x, brickSize.y, 1f);

            var visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(brickObject.transform, false);

            var spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.sortingOrder = 5;
            spriteRenderer.sharedMaterial = spriteUnlitMaterial;
            NormalizeSpriteRendererScale(spriteRenderer);

            var collider = brickObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            var brick = brickObject.AddComponent<Brick>();
            brick.Initialize(
                this,
                definition,
                effectiveHitPoints,
                themeService != null ? themeService.ResolveBrickStyle(definition) : new ThemeVisualStyle(definition.BaseColor, definition.DamagedColor, squareSprite),
                motionConfig.Speed,
                motionConfig.InitialDirection);
            bricks.Add(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining++;
            }
        }

        private static void NormalizeSpriteRendererScale(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var sprite = spriteRenderer.sprite;

            if (sprite == null)
            {
                spriteRenderer.transform.localScale = Vector3.one;
                return;
            }

            var spriteSize = sprite.bounds.size;
            var scaleX = spriteSize.x > 0.0001f ? 1f / spriteSize.x : 1f;
            var scaleY = spriteSize.y > 0.0001f ? 1f / spriteSize.y : 1f;
            spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private int GetEffectiveBrickHitPoints(BrickDefinition definition)
        {
            if (definition == null || !definition.IsBreakable)
            {
                return 0;
            }

            var durabilityMultiplier = activeRunSettings?.BrickDurabilityMultiplier ?? 1f;
            return Mathf.Max(1, Mathf.RoundToInt(definition.HitPoints * durabilityMultiplier));
        }

        private void ClearBricks()
        {
            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                if (bricks[index] == null)
                {
                    continue;
                }

                bricks[index].gameObject.SetActive(false);
                Destroy(bricks[index].gameObject);
            }

            bricks.Clear();
        }

        private void DestroyBricksInExplosionRadius(
            Vector2 explosionCenter,
            float explosionRadius,
            Brick sourceBrick,
            BallController scoringBall)
        {
            if (explosionRadius <= 0.01f || bricks.Count == 0)
            {
                return;
            }

            var impactedBricks = new List<Brick>();
            var explosionRadiusSquared = explosionRadius * explosionRadius;

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (candidate == null
                    || candidate == sourceBrick
                    || candidate.Definition == null
                    || !candidate.Definition.IsBreakable)
                {
                    continue;
                }

                var offset = (Vector2)candidate.transform.position - explosionCenter;

                if (offset.sqrMagnitude > explosionRadiusSquared)
                {
                    continue;
                }

                impactedBricks.Add(candidate);
            }

            for (var index = 0; index < impactedBricks.Count; index++)
            {
                var impactedBrick = impactedBricks[index];

                if (impactedBrick == null)
                {
                    continue;
                }

                impactedBrick.DestroyByExplosion(scoringBall);
            }
        }

        private void EvaluateLevelCompletion()
        {
            if (currentLevel == null)
            {
                return;
            }

            var levelCleared = !HasBreakableBricksRemaining() || currentLevelCompletionRule switch
            {
                LevelCompletionRule.ClearRequiredBricks => requiredBricksRemaining <= 0,
                LevelCompletionRule.ReachTargetScore => levelScore >= currentLevelTargetScore,
                _ => false,
            };

            if (!levelCleared)
            {
                return;
            }

            SetSimulationPaused(false);
            ClearPickups();
            StopAllBalls();
            stickyCaughtBall = null;

            if (TryOpenUpgradeDraft())
            {
                return;
            }

            roundState = RoundState.LevelComplete;
            selectedOverlayActionIndex = 0;
        }

        private bool HasBreakableBricksRemaining()
        {
            for (var index = 0; index < bricks.Count; index++)
            {
                var brick = bricks[index];

                if (brick == null || brick.Definition == null)
                {
                    continue;
                }

                if (brick.Definition.IsBreakable)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasNextLevel()
        {
            return currentLevel != null && loadedLevels.Count > 0;
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

        private Sprite LoadSpriteResource(string resourcePath, Sprite fallbackSprite)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return fallbackSprite;
            }

            var loadedSprite = Resources.Load<Sprite>(resourcePath);
            return loadedSprite != null ? loadedSprite : fallbackSprite;
        }

        private void LoadBackgroundLibrary()
        {
            loadedBackgroundSprites.Clear();
            loadedBackgroundTextures.Clear();

            var spriteAssets = Resources.LoadAll<Sprite>("Backgrounds");

            for (var index = 0; index < spriteAssets.Length; index++)
            {
                if (spriteAssets[index] != null)
                {
                    loadedBackgroundSprites.Add(spriteAssets[index]);
                }
            }

            loadedBackgroundSprites.Sort(CompareNamedObjects);

            var textureAssets = Resources.LoadAll<Texture2D>("Backgrounds");

            for (var index = 0; index < textureAssets.Length; index++)
            {
                var texture = textureAssets[index];

                if (texture == null)
                {
                    continue;
                }

                if (ContainsNamedObject(loadedBackgroundSprites, texture.name))
                {
                    continue;
                }

                loadedBackgroundTextures.Add(texture);
            }

            loadedBackgroundTextures.Sort(CompareNamedObjects);
        }

        private Material CreateSpriteUnlitMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

            if (shader == null)
            {
                Debug.LogWarning("Could not find the URP sprite unlit shader. Runtime sprites will use Unity's default material.");
                return null;
            }

            return new Material(shader)
            {
                name = "RuntimeSpriteUnlit",
                hideFlags = HideFlags.DontSave,
            };
        }

        private Material CreateAdditiveSpriteMaterial()
        {
            var shader = Shader.Find("Get Bricked/Sprite Additive");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }

            if (shader == null)
            {
                Debug.LogWarning("Could not find an additive sprite shader. Falling back to a duplicate unlit material.");
                return CreateSpriteUnlitMaterial();
            }

            var material = new Material(shader)
            {
                name = "RuntimeSpriteAdditive",
                hideFlags = HideFlags.DontSave,
                renderQueue = 3000,
            };

            if (shader.name == "Universal Render Pipeline/Particles/Unlit")
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 2f);
                material.SetFloat("_SrcBlend", 5f);
                material.SetFloat("_DstBlend", 1f);
                material.SetFloat("_SrcBlendAlpha", 1f);
                material.SetFloat("_DstBlendAlpha", 1f);
                material.SetFloat("_ZWrite", 0f);
                material.SetColor("_BaseColor", Color.white);
                material.SetColor("_EmissionColor", Color.white * 2f);
            }

            return material;
        }

        private void UpdateBackgroundVisuals()
        {
            if (backgroundSpriteRenderer == null || activeCamera == null)
            {
                return;
            }

            var backgroundStyle = themeService != null
                ? themeService.ResolveBackgroundStyle(ResolveBackgroundSpriteForCurrentLevel())
                : new ThemeVisualStyle(backgroundColor, backgroundColor, ResolveBackgroundSpriteForCurrentLevel());

            var sprite = backgroundStyle.Sprite;
            backgroundSpriteRenderer.sprite = sprite;
            backgroundSpriteRenderer.enabled = sprite != null;

            var tint = Color.Lerp(Color.white, backgroundStyle.PrimaryColor, backgroundImageTintStrength);
            tint.a = backgroundImageAlpha;
            backgroundSpriteRenderer.color = tint;
            backgroundSpriteRenderer.transform.position = Vector3.zero;
            var overlayScale = CalculateFullscreenScale(sprite != null ? sprite : backgroundHazeSprite);
            backgroundSpriteRenderer.transform.localScale = sprite != null ? CalculateFullscreenScale(sprite) : overlayScale;

            if (backgroundHazeRenderer != null)
            {
                var hazeColor = Color.Lerp(backgroundStyle.PrimaryColor, wallColor, 0.58f);
                hazeColor.a = backgroundHazeAlpha;
                backgroundHazeRenderer.enabled = backgroundHazeSprite != null;
                backgroundHazeRenderer.color = hazeColor;
                backgroundHazeRenderer.transform.position = Vector3.zero;
                backgroundHazeRenderer.transform.localScale = overlayScale;
            }

            if (backgroundScanlineRenderer != null)
            {
                var scanlineColor = Color.Lerp(Color.white, backgroundStyle.PrimaryColor, 0.3f);
                scanlineColor.a = scanlineAlpha;
                backgroundScanlineRenderer.enabled = backgroundScanlineSprite != null;
                backgroundScanlineRenderer.color = scanlineColor;
                backgroundScanlineRenderer.transform.position = Vector3.zero;
                backgroundScanlineRenderer.transform.localScale = overlayScale;
            }
        }

        private Vector3 CalculateFullscreenScale(Sprite sprite)
        {
            if (sprite == null || activeCamera == null)
            {
                return Vector3.one;
            }

            var spriteSize = sprite.bounds.size;

            if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f)
            {
                return Vector3.one;
            }

            var visibleHeight = cameraHalfHeight * 2f;
            var visibleWidth = visibleHeight * activeCamera.aspect;
            var scale = Mathf.Max(visibleWidth / spriteSize.x, visibleHeight / spriteSize.y);
            return new Vector3(scale, scale, 1f);
        }

        private Sprite ResolveBackgroundSpriteForCurrentLevel()
        {
            if (loadedBackgroundSprites.Count > 0)
            {
                var spriteIndex = Mathf.Abs(currentLevelIndex) % loadedBackgroundSprites.Count;
                return loadedBackgroundSprites[spriteIndex];
            }

            if (loadedBackgroundTextures.Count == 0)
            {
                return null;
            }

            var textureIndex = Mathf.Abs(currentLevelIndex) % loadedBackgroundTextures.Count;
            return CreateRuntimeBackgroundSprite(loadedBackgroundTextures[textureIndex]);
        }

        private Sprite CreateRuntimeBackgroundSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            for (var index = 0; index < runtimeGeneratedBackgroundSprites.Count; index++)
            {
                var existingSprite = runtimeGeneratedBackgroundSprites[index];

                if (existingSprite != null && string.Equals(existingSprite.name, texture.name, StringComparison.Ordinal))
                {
                    return existingSprite;
                }
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = texture.name;
            runtimeGeneratedBackgroundSprites.Add(sprite);
            return sprite;
        }

        private static bool ContainsNamedObject<T>(List<T> objects, string candidateName)
            where T : UnityEngine.Object
        {
            for (var index = 0; index < objects.Count; index++)
            {
                if (objects[index] != null && string.Equals(objects[index].name, candidateName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareNamedObjects<T>(T left, T right)
            where T : UnityEngine.Object
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

            return string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase);
        }

        private Sprite CreateSquareSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        private Sprite CreateBackgroundHazeSprite()
        {
            const int textureSize = 256;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeBackgroundHazeTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            var radius = textureSize * 0.72f;

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var radial = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), center) / radius);
                    var vertical = Mathf.InverseLerp(0f, textureSize - 1f, y);
                    var alpha = Mathf.Lerp(0.04f, 0.92f, Mathf.Pow(radial, 1.45f));
                    alpha = Mathf.Max(alpha, Mathf.Lerp(0.08f, 0.42f, vertical * 0.85f));
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        private Sprite CreateBackgroundScanlineSprite()
        {
            const int textureWidth = 8;
            const int textureHeight = 1024;
            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeBackgroundScanlineTexture",
            };

            var pixels = new Color[textureWidth * textureHeight];

            for (var y = 0; y < textureHeight; y++)
            {
                var stripeCycle = y % 4;
                var alpha = stripeCycle == 0
                    ? 0.92f
                    : stripeCycle == 2
                        ? 0.28f
                        : 0.05f;

                for (var x = 0; x < textureWidth; x++)
                {
                    pixels[x + (y * textureWidth)] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        private Sprite CreateCircleSprite()
        {
            const int textureSize = 64;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeCircleTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var radius = textureSize * 0.5f;
            var center = new Vector2(radius - 0.5f, radius - 0.5f);

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var normalizedDistance = distance / radius;

                    if (normalizedDistance >= 1f)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var alpha = normalizedDistance <= 0.68f
                        ? 1f
                        : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.68f, 1f, normalizedDistance));
                    pixels[index] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
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
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Get Bricked", "Synthwave Cabinet Prototype", true));
                uiRenderer.DrawMainMenu(BuildMainMenuView(), HandleOverlayActionClick);
                return;
            }

            if (roundState == RoundState.RunSetup)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Run Setup", ResolvePendingThemeDefinition()?.DisplayName ?? "Theme Preview", true));
                uiRenderer.DrawRunSetup(BuildRunSetupView());
                return;
            }

            if (roundState == RoundState.UpgradeDraft)
            {
                uiRenderer.DrawCabinetBackdrop(BuildChromeView("Upgrade Draft", "Choose one permanent mod", true));
                uiRenderer.DrawUpgradeDraft(BuildUpgradeDraftView(), HandleUpgradeDraftChoice);
                uiRenderer.DrawPickupBanner(BuildPickupBannerView());
                return;
            }

            uiRenderer.DrawCabinetBackdrop(BuildChromeView(string.Empty, string.Empty, false));
            uiRenderer.DrawGameplayHud(BuildHudView(), ToggleDiagnosticsOverlay, ToggleHudMenuOverlay);
            uiRenderer.DrawModifierIndicator(BuildModifierViews(), isDiagnosticsOverlayVisible);

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
                OverlayAction.StartRun => "Start Run",
                OverlayAction.OpenRunSetup => "Open Run Setup",
                OverlayAction.ResetSetupDefaults => "Reset Setup Defaults",
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
            return new BreakoutUiMenuView
            {
                Title = "Get Bricked",
                Subtitle = "Neon cabinet online. Quick-start the last tuned run or open the control panel and retune the seed, score mode, modifiers, and palette.",
                SectionTitle = "Control Panel",
                ActionLabels = BuildOverlayActionLabels(GetOverlayActionsForState(RoundState.MainMenu)),
                SelectedActionIndex = selectedOverlayActionIndex,
                PreviewTitle = "Saved Run Loadout",
                PreviewLines = new[]
                {
                    $"Tape ID: {GetPendingSeedDisplay()}",
                    $"Difficulty: {pendingRunSetup.DifficultyPreset} | Score Mode: {previewSettings.ScoringModeLabel}",
                    $"Balls/Serve: {pendingRunSetup.BallsPerServe} | {BuildRetrySummaryLabel(previewSettings)}",
                    $"Theme: {previewSettings.ThemeLabel}",
                    $"Paddle x{previewSettings.PaddleWidthMultiplier:0.00} | Ball x{previewSettings.BallSpeedMultiplier:0.00}",
                    $"Brick durability x{previewSettings.BrickDurabilityMultiplier:0.00} | {BuildScoreModeSummaryLabel(previewSettings)}",
                    $"Drops: {BuildDropSummaryLabel(previewSettings)}",
                },
                ValidationText = previewValidation,
                FooterText = "Run setup selections persist automatically, so quick start reuses the last cabinet tuning across sessions.",
                HintText = "Up/Down selects. Space confirms. Open Run Setup for Tape ID editing, score mode tuning, modifier tweaks, and theme cycling.",
            };
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

        private BreakoutUiHudView BuildHudView()
        {
            var bounceZoneLeftScreen = activeCamera != null
                ? activeCamera.WorldToScreenPoint(new Vector3(arenaLeft, 0f, 0f)).x
                : 0f;
            var speed = GetDisplayedBallSpeed();
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
                    Title = "Upgrade Draft",
                    Subtitle = "No upgrades available.",
                    BuildLine = BuildUpgradeSummaryLabel(3),
                    HintText = "Press R to return to setup.",
                };
            }

            var optionViews = new BreakoutUiUpgradeDraftOptionView[offers.Count];

            for (var index = 0; index < offers.Count; index++)
            {
                var upgrade = offers[index];
                var currentStacks = activeRunState != null ? activeRunState.GetStackCount(upgrade) : 0;
                optionViews[index] = new BreakoutUiUpgradeDraftOptionView
                {
                    Title = upgrade != null ? upgrade.DisplayName : "Missing Upgrade",
                    Description = upgrade != null ? upgrade.Description : "This draft slot failed to load.",
                    Detail = upgrade != null
                        ? $"Stacks {currentStacks}/{upgrade.MaxStacks}   |   {BuildUpgradeMechanicalSummary(upgrade)}"
                        : "Unavailable",
                    Accent = ResolveRunUpgradeAccentColor(upgrade),
                };
            }

            return new BreakoutUiUpgradeDraftView
            {
                Title = "Upgrade Draft",
                Subtitle = $"Level {currentLevelIndex + 1:00} cleared. Choose one cabinet mod to lock in for the rest of this run.",
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
                ? "Run Over"
                : HasNextLevel()
                    ? "Level Cleared"
                    : "Final Layout Cleared";
            var summary = isGameOver
                ? $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | Reached {BuildLevelLabel()} | Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}"
                : HasNextLevel()
                    ? $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Next up: level {currentLevelIndex + 2:00}"
                    : $"{GetScoreDisplayLabel()} {FormatScoreValue(score)} | {GetLifeCounterLabel()} {GetLifeCounterValue():00} | Tape ID {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}";
            var footer = isGameOver
                ? "Restart the run, jump back to setup, or return to the main menu."
                : HasNextLevel()
                    ? "Advance to the next authored layout, restart the run, or return to the menu."
                    : "The authored run is complete. Restart, tune a new setup, or head back to the menu.";
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
                    Label = summary.StackCount > 1
                        ? $"{summary.Definition.HudLabel} x{summary.StackCount}"
                        : summary.Definition.HudLabel,
                    RemainingDuration = summary.RemainingDuration,
                    DurationRatio = summary.DurationRatio,
                    Color = effectStyle.PrimaryColor,
                });
            }

            return views.ToArray();
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

            if (roundState == RoundState.MainMenu)
            {
                actions = GetOverlayActionsForState(RoundState.MainMenu);
            }

            if (actionIndex < 0 || actionIndex >= actions.Length)
            {
                return;
            }

            selectedOverlayActionIndex = actionIndex;
            PerformOverlayAction(actions[actionIndex]);
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

            if (currentLevelCompletionRule == LevelCompletionRule.ReachTargetScore)
            {
                return $"Target Score {currentLevelTargetScore:0000} | Level Score {levelScore:0000}";
            }

            return $"Remaining Bricks {requiredBricksRemaining:00}";
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
            var names = new List<string>();

            for (var index = 0; index < chosenUpgrades.Count && names.Count < Mathf.Max(1, maxNames); index++)
            {
                var upgrade = chosenUpgrades[index];

                if (upgrade != null)
                {
                    names.Add(upgrade.HudLabel);
                }
            }

            var remainder = Mathf.Max(0, chosenUpgrades.Count - names.Count);
            var labels = string.Join(", ", names);

            if (remainder > 0)
            {
                labels = string.IsNullOrWhiteSpace(labels) ? $"+{remainder} more" : $"{labels}, +{remainder} more";
            }

            return $"Build {chosenUpgrades.Count:00}: {labels}";
        }

        private string BuildBallSpeedControlLabel()
        {
            var baseSpeed = GetBallSpeedBase();
            var currentSpeed = GetDisplayedBallSpeed();
            var maxSpeed = GetMaximumBallSpeed();
            return
                $"Ball Speed {currentSpeed:0.00} | Score x{GetScoreMultiplierForSpeed(currentSpeed):0.00} | Base {baseSpeed:0.00} | Manual x{manualBallSpeedMultiplier:0.00} | Cap {maxSpeed:0.00}";
        }

        private string BuildRoundStateLabel()
        {
            return roundState switch
            {
                RoundState.ReadyToServe => "Ready to serve",
                RoundState.Playing => "Ball in play",
                RoundState.LifeLost => "Recovering from a loss",
                RoundState.UpgradeDraft => "Choosing a permanent upgrade",
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

            var summary =
                $"Tape ID: {activeRunSettings.Seed} | {activeRunSettings.DifficultyLabel} | {BuildScoreModeSummaryLabel(activeRunSettings)} | {BuildRetrySummaryLabel(activeRunSettings)} | Balls/Serve {GetEffectiveBallsPerServe()} | " +
                $"Theme: {activeRunSettings.ThemeLabel} | Drops: {BuildDropSummaryLabel(activeRunSettings)} | Paddle x{activeRunSettings.PaddleWidthMultiplier:0.00} | Ball x{activeRunSettings.BallSpeedMultiplier:0.00} | Build {GetChosenUpgradeCount():00}";
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
            powerUpService?.TrySpawnPickup(
                brick,
                activeRunSettings,
                GetEffectiveDropChanceMultiplier(),
                NextGameplayRandomFloat,
                pickupsRoot,
                arenaBottom,
                themeService,
                this);
            ApplyVisualEffectState();
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
                    false,
                    false,
                    false,
                    0f,
                    false,
                    0f,
                    0f,
                    1f,
                    0f);
            paddle.SetMoveSpeed(currentLevelPaddleSpeed);
            paddle.SetWidthMultiplier(activeEffectModifiers.PaddleWidthMultiplier);
            paddle.SetWavyStrength(activeEffectModifiers.WavyPaddleStrength);
            paddle.SetControlsReversed(activeEffectModifiers.ReverseControlsEnabled);
            paddle.SetSplitGapWidthNormalized(activeEffectModifiers.SplitPaddleGapNormalized);
            paddle.SetLagSpikeStrength(activeEffectModifiers.LagSpikeStrength);

            var currentBallSpeed = GetCurrentBallSpeed();
            var gravityWellCenter = new Vector2(0f, (arenaTop + arenaBottom) * 0.5f);

            if (serveBall != null)
            {
                serveBall.SetMovementSpeed(currentBallSpeed);
                serveBall.SetPhaseThroughBricks(activeEffectModifiers.PhaseBallEnabled);
                serveBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
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
                activeBall.SetGravityWell(gravityWellCenter, activeEffectModifiers.GravityWellStrength);
            }

            ApplyVisualEffectState();
            UpdateShieldWallVisual();

            if (stickyCaughtBall != null && !activeEffectModifiers.StickyPaddleEnabled)
            {
                ReleaseStickyCaughtBall();
            }
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

                Destroy(activeBall.gameObject);
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

        private void ApplyLifeLossScorePenalty()
        {
            var penalty = GetLifeLossScorePenalty();

            if (penalty <= 0)
            {
                return;
            }

            score -= penalty;
        }

        private int CalculateBrickScore(Brick brick, BallController scoringBall)
        {
            if (brick == null)
            {
                return 0;
            }

            var baseScore = brick.ScoreValue;

            if (baseScore <= 0)
            {
                return 0;
            }

            var scoringSpeed = scoringBall != null && scoringBall.CurrentSpeed > 0.01f
                ? scoringBall.CurrentSpeed
                : GetDisplayedBallSpeed();
            return Mathf.Max(1, Mathf.RoundToInt(baseScore * GetScoreMultiplierForSpeed(scoringSpeed)));
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

        private float GetScoreMultiplierForSpeed(float speed)
        {
            return Mathf.Max(0.1f, speed / Mathf.Max(0.1f, ballSpeed));
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
            var offers = upgradeDraftService.GenerateDraft(activeRunState, activeRunSettings, activeRunSettings.Seed, currentLevelIndex, 3);

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

            if (!activeRunState.TryApplyPendingDraftOffer(optionIndex, out var appliedUpgrade))
            {
                return;
            }

            if (!UsesHighScoreMode() && appliedUpgrade != null && appliedUpgrade.BonusLives > 0)
            {
                livesRemaining += appliedUpgrade.BonusLives;
            }

            ApplyActiveEffects();
            ShowRunUpgradeBanner(appliedUpgrade);

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
            return activeRunState?.ChosenUpgrades.Count ?? 0;
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

        private void ShowRunUpgradeBanner(RunUpgradeDefinition upgrade)
        {
            if (upgrade == null || powerUpService == null)
            {
                return;
            }

            powerUpService.ShowStatusBanner($"+ BUILD {upgrade.DisplayName}", ResolveRunUpgradeAccentColor(upgrade), 2.2f);
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

            var targets = new List<Brick>(2);
            var excluded = new HashSet<Brick>();
            var leftEmitterX = paddle.transform.position.x - (paddle.HalfWidthWorld * 0.55f);
            var rightEmitterX = paddle.transform.position.x + (paddle.HalfWidthWorld * 0.55f);
            var leftTarget = FindLaserTarget(leftEmitterX, excluded);

            if (leftTarget != null)
            {
                targets.Add(leftTarget);
                excluded.Add(leftTarget);
            }

            var rightTarget = FindLaserTarget(rightEmitterX, excluded);

            if (rightTarget != null)
            {
                targets.Add(rightTarget);
            }

            if (targets.Count == 0)
            {
                return false;
            }

            var scoringBall = ResolvePrimaryScoringBall();

            for (var index = 0; index < targets.Count; index++)
            {
                targets[index]?.ApplyEffectHit(scoringBall, BrickDestructionCause.Laser, 1);
            }

            laserShotCooldownTimer = LaserShotCooldownSeconds;
            return true;
        }

        private Brick FindLaserTarget(float beamOriginX, ISet<Brick> excluded)
        {
            Brick bestCandidate = null;
            var bestScore = float.MaxValue;

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (candidate == null
                    || (excluded != null && excluded.Contains(candidate))
                    || candidate.Definition == null
                    || !candidate.Definition.IsBreakable
                    || candidate.transform.position.y <= paddle.transform.position.y + 0.2f)
                {
                    continue;
                }

                var score = Mathf.Abs(candidate.transform.position.x - beamOriginX)
                    + Mathf.Abs(candidate.transform.position.y - paddle.transform.position.y) * 0.08f;

                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestCandidate = candidate;
            }

            return bestCandidate;
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

            var chainRadius = Mathf.Lerp(1.8f, 3.25f, Mathf.Clamp01(activeEffectModifiers.ChainLightningStrength));
            var chainRadiusSquared = chainRadius * chainRadius;
            var maxTargets = Mathf.Clamp(1 + Mathf.RoundToInt(activeEffectModifiers.ChainLightningStrength * 4f), 1, 3);
            var candidates = new List<Brick>();

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (candidate == null
                    || candidate == sourceBrick
                    || candidate.Definition == null
                    || !candidate.Definition.IsBreakable)
                {
                    continue;
                }

                var offset = (Vector2)candidate.transform.position - origin;

                if (offset.sqrMagnitude > chainRadiusSquared)
                {
                    continue;
                }

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
            {
                return;
            }

            candidates.Sort((left, right) =>
            {
                var leftDistance = ((Vector2)left.transform.position - origin).sqrMagnitude;
                var rightDistance = ((Vector2)right.transform.position - origin).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });

            for (var index = 0; index < candidates.Count && index < maxTargets; index++)
            {
                candidates[index]?.ApplyEffectHit(scoringBall, BrickDestructionCause.ChainLightning, 1);
            }
        }

        private void ApplyVisualEffectState()
        {
            var visibilityMultiplier = activeEffectModifiers.FogVisibilityMultiplier > 0.001f
                ? activeEffectModifiers.FogVisibilityMultiplier
                : 1f;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.SetVisibilityMultiplier(visibilityMultiplier);
            }

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

            if (backgroundHazeRenderer != null)
            {
                var hazeColor = backgroundHazeRenderer.color;
                hazeColor.a = Mathf.Clamp01(backgroundHazeAlpha + ((1f - visibilityMultiplier) * 0.32f));
                backgroundHazeRenderer.color = hazeColor;
            }
        }

        private void UpdateShieldWallVisual()
        {
            if (shieldWallRenderer == null)
            {
                return;
            }

            shieldWallRenderer.enabled = shieldWallCharges > 0;

            if (!shieldWallRenderer.enabled)
            {
                return;
            }

            var shieldStyle = themeService != null
                ? themeService.ResolveThemeStyle(ThemeVisualSlot.PickupBeneficial, new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite)
                : new ThemeVisualStyle(new Color(0.45f, 0.95f, 0.72f, 1f), new Color(0.45f, 0.95f, 0.72f, 1f), squareSprite);
            var shieldColor = shieldStyle.PrimaryColor;
            shieldColor.a = Mathf.Clamp(0.52f + ((shieldWallCharges - 1) * 0.08f), 0.52f, 0.82f);
            shieldWallRenderer.color = shieldColor;
            shieldWallRenderer.transform.position = new Vector2(0f, arenaBottom + ShieldWallYOffset);
        }

        private string BuildActiveEffectsLabel()
        {
            return powerUpService != null ? powerUpService.BuildActiveEffectsLabel() : "Active Effects: none";
        }
    }
}
