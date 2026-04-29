using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GetBricked.Gameplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    public sealed class BreakoutGameController : MonoBehaviour
    {
        private sealed class ActiveTimedEffect
        {
            public ActiveTimedEffect(PowerUpDefinition definition, float remainingDuration)
            {
                Definition = definition;
                RemainingDuration = remainingDuration;
            }

            public PowerUpDefinition Definition { get; }

            public float RemainingDuration { get; set; }
        }

        private sealed class PendingRunSetup
        {
            public int Seed;
            public RunDifficultyPreset DifficultyPreset = RunDifficultyPreset.Standard;
            public int BallsPerServe = 1;
            public int PaddleWidthStep;
            public int BallSpeedStep;
            public int BrickDurabilityStep;
            public DropPoolMode DropPoolMode = DropPoolMode.Mixed;
        }

        private sealed class LevelLayoutPlan
        {
            public string[] LayoutRows = Array.Empty<string>();
            public bool MirrorLayout;
            public int[] RowShifts = Array.Empty<int>();
        }

        [Serializable]
        private sealed class PersistedRunSetup
        {
            public int Version = PersistedRunSetupVersion;
            public int Seed;
            public string PendingSeedText = string.Empty;
            public int DifficultyPreset = (int)RunDifficultyPreset.Standard;
            public int BallsPerServe = 1;
            public int PaddleWidthStep;
            public int BallSpeedStep;
            public int BrickDurabilityStep;
            public int DropPoolModeValue = (int)Gameplay.Data.DropPoolMode.Mixed;
        }

        private enum RunSetupField
        {
            Seed = 0,
            Difficulty = 1,
            BallsPerServe = 2,
            PaddleWidth = 3,
            BallSpeed = 4,
            BrickDurability = 5,
            DropPool = 6,
        }

        private enum RoundState
        {
            MainMenu,
            RunSetup,
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
        }

        private const string PersistedRunSetupKey = "GetBricked.RunSetup";
        private const int PersistedRunSetupVersion = 1;

        [Header("Camera")]
        [SerializeField] private float cameraHalfHeight = 5.2f;
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);

        [Header("Playfield")]
        [SerializeField] private float playfieldPadding = 0.6f;
        [SerializeField] private float wallThickness = 0.45f;
        [SerializeField] private Color wallColor = new Color(0.15f, 0.2f, 0.28f, 1f);

        [Header("Paddle")]
        [SerializeField] private Vector2 paddleSize = new Vector2(2.4f, 0.4f);
        [SerializeField] private float paddleSpeed = 12f;
        [SerializeField] private float paddleFloorOffset = 0.8f;
        [SerializeField] private Color paddleColor = new Color(0.94f, 0.96f, 1f, 1f);

        [Header("Ball")]
        [SerializeField] private float ballRadius = 0.18f;
        [SerializeField] private float ballSpeed = 7.5f;
        [SerializeField, Range(0.15f, 0.95f)] private float minimumVerticalDirection = 0.35f;
        [SerializeField] private Color ballColor = new Color(0.98f, 0.75f, 0.29f, 1f);

        [Header("Ball Speed Control")]
        [SerializeField, Range(0.02f, 0.25f)] private float manualBallSpeedStep = 0.08f;
        [SerializeField, Range(1f, 2.5f)] private float manualBallSpeedMaxMultiplier = 1.75f;

        [Header("Run Rules")]
        [SerializeField, Min(1)] private int startingLives = 3;

        [Header("Brick Wall")]
        [SerializeField] private Vector2 brickSize = new Vector2(1.15f, 0.45f);
        [SerializeField] private Vector2 brickSpacing = new Vector2(0.15f, 0.15f);

        [Header("Power-Ups")]
        [SerializeField] private Vector2 pickupSize = new Vector2(0.55f, 0.55f);
        [SerializeField] private float pickupFallSpeed = 3.2f;
        [SerializeField, Range(5f, 35f)] private float multiBallSpreadAngle = 18f;

        private readonly List<Brick> bricks = new List<Brick>();
        private readonly List<LevelDefinition> loadedLevels = new List<LevelDefinition>();
        private readonly List<BallController> activeBalls = new List<BallController>();
        private readonly List<PowerUpPickup> activePickups = new List<PowerUpPickup>();
        private readonly List<ActiveTimedEffect> activeTimedEffects = new List<ActiveTimedEffect>();

        private Camera activeCamera;
        private Transform runtimeRoot;
        private Transform wallsRoot;
        private Transform ballsRoot;
        private Transform bricksRoot;
        private Transform pickupsRoot;
        private PaddleController paddle;
        private BallController serveBall;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private PhysicsMaterial2D bounceMaterial;
        private RoundState roundState;
        private int livesRemaining;
        private int score;
        private int requiredBricksRemaining;
        private float arenaLeft;
        private float arenaRight;
        private float arenaTop;
        private float arenaBottom;
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
        private LevelDefinition currentLevel;
        private int currentLevelIndex;
        private int levelScore;
        private float currentLevelBallSpeed;
        private float currentLevelPaddleSpeed;
        private int ballInstanceCounter;
        private string pickupBannerText;
        private float pickupBannerTimer;
        private Color pickupBannerColor = Color.white;
        private PendingRunSetup pendingRunSetup;
        private RunSettings activeRunSettings;
        private DeterministicRandomService gameplayRandom;
        private RunSetupField selectedRunSetupField;
        private RoundState pausedFromState;
        private int selectedOverlayActionIndex;
        private string pendingSeedText = string.Empty;
        private string currentLevelVariationLabel = "Variation: not started";
        private string pendingValidationMessage = string.Empty;
        private float manualBallSpeedMultiplier = 1f;

        private void Awake()
        {
            LoadLevelDefinitions();
            ConfigureCamera();
            CreateRuntimeAssets();
            CreateRuntimeRoots();
            CreateBounds();
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

            if (bounceMaterial != null)
            {
                Destroy(bounceMaterial);
            }
        }

        private void Update()
        {
            UpdateTimedEffects();
            UpdatePickupBanner();

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
                HandleOverlayMenuInput(keyboard);
                return;
            }

            if (roundState == RoundState.RunSetup)
            {
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

            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                AdjustManualBallSpeed(1);
            }

            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                AdjustManualBallSpeed(-1);
            }

            if ((keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) == false)
            {
                return;
            }

            if (roundState == RoundState.ReadyToServe || roundState == RoundState.LifeLost)
            {
                LaunchServe();
            }
        }

        public void HandleBrickDestroyed(Brick brick)
        {
            if (!bricks.Remove(brick))
            {
                return;
            }

            score += brick.ScoreValue;
            levelScore += brick.ScoreValue;
            TrySpawnPickup(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining = Mathf.Max(0, requiredBricksRemaining - 1);
            }

            brick.gameObject.SetActive(false);
            Destroy(brick.gameObject);

            EvaluateLevelCompletion();
        }

        public void HandleBallLost(BallController lostBall)
        {
            if (roundState != RoundState.Playing || lostBall == null)
            {
                return;
            }

            var isServeBall = lostBall == serveBall;
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

            livesRemaining = Mathf.Max(0, livesRemaining - 1);

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

        public void HandlePickupCaught(PowerUpPickup pickup)
        {
            if (roundState != RoundState.Playing || pickup == null || !activePickups.Remove(pickup))
            {
                return;
            }

            ApplyPowerUp(pickup.Definition);
            pickup.gameObject.SetActive(false);
            Destroy(pickup.gameObject);
        }

        public void HandlePickupMissed(PowerUpPickup pickup)
        {
            if (pickup == null || !activePickups.Remove(pickup))
            {
                return;
            }

            pickup.gameObject.SetActive(false);
            Destroy(pickup.gameObject);
        }

        private void StartNewRun()
        {
            if (activeRunSettings == null)
            {
                activeRunSettings = BuildRunSettingsFromPending(out pendingValidationMessage, commitSeedText: true);
            }

            manualBallSpeedMultiplier = 1f;
            SetSimulationPaused(false);
            selectedOverlayActionIndex = 0;
            gameplayRandom = new DeterministicRandomService(activeRunSettings.Seed);
            livesRemaining = activeRunSettings.StartingLives;
            score = 0;
            currentLevelIndex = 0;
            currentLevelVariationLabel = "Variation: pending";
            ClearTimedEffects();
            ClearPickups();
            LoadLevel(currentLevelIndex, RoundState.ReadyToServe);

            Debug.Log(
                $"Starting run | seed {activeRunSettings.Seed} | preset {activeRunSettings.DifficultyLabel} | " +
                $"balls/serve {activeRunSettings.BallsPerServe} | paddle x{activeRunSettings.PaddleWidthMultiplier:0.00} | " +
                $"ball speed x{activeRunSettings.BallSpeedMultiplier:0.00} | brick durability x{activeRunSettings.BrickDurabilityMultiplier:0.00} | " +
                $"drops {activeRunSettings.DropPoolLabel}");
        }

        private void EnterMainMenu()
        {
            roundState = RoundState.MainMenu;
            selectedOverlayActionIndex = 0;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            ResetRuntimeForMetaFlow();
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
            selectedRunSetupField = RunSetupField.Seed;
            pendingValidationMessage = string.Empty;
            currentLevelVariationLabel = "Variation: pending";
            ResetRuntimeForMetaFlow();
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

            if (serveBall != null)
            {
                serveBall.Stop();
                serveBall.gameObject.SetActive(false);
            }

            paddle.ResetToStart();
        }

        private void ResetPendingRunSetup(bool generateNewSeed)
        {
            pendingRunSetup ??= new PendingRunSetup();
            pendingRunSetup.DifficultyPreset = RunDifficultyPreset.Standard;
            pendingRunSetup.BallsPerServe = 1;
            pendingRunSetup.PaddleWidthStep = 0;
            pendingRunSetup.BallSpeedStep = 0;
            pendingRunSetup.BrickDurabilityStep = 0;
            pendingRunSetup.DropPoolMode = DropPoolMode.Mixed;
            pendingRunSetup.Seed = generateNewSeed ? GenerateSeed() : pendingRunSetup.Seed;
            pendingSeedText = pendingRunSetup.Seed.ToString(CultureInfo.InvariantCulture);
        }

        private void LoadPersistedRunSetup()
        {
            if (!PlayerPrefs.HasKey(PersistedRunSetupKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(PersistedRunSetupKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                var persistedRunSetup = JsonUtility.FromJson<PersistedRunSetup>(json);

                if (persistedRunSetup == null || persistedRunSetup.Version != PersistedRunSetupVersion)
                {
                    return;
                }

                pendingRunSetup ??= new PendingRunSetup();
                pendingRunSetup.Seed = persistedRunSetup.Seed > 0 ? persistedRunSetup.Seed : GenerateSeed();
                pendingRunSetup.DifficultyPreset = (RunDifficultyPreset)Mathf.Clamp(
                    persistedRunSetup.DifficultyPreset,
                    (int)RunDifficultyPreset.Casual,
                    (int)RunDifficultyPreset.Brutal);
                pendingRunSetup.BallsPerServe = Mathf.Clamp(persistedRunSetup.BallsPerServe, 1, 4);
                pendingRunSetup.PaddleWidthStep = Mathf.Clamp(persistedRunSetup.PaddleWidthStep, -2, 2);
                pendingRunSetup.BallSpeedStep = Mathf.Clamp(persistedRunSetup.BallSpeedStep, -2, 2);
                pendingRunSetup.BrickDurabilityStep = Mathf.Clamp(persistedRunSetup.BrickDurabilityStep, -2, 2);
                pendingRunSetup.DropPoolMode = (DropPoolMode)Mathf.Clamp(
                    persistedRunSetup.DropPoolModeValue,
                    (int)DropPoolMode.Mixed,
                    (int)DropPoolMode.Disabled);
                pendingSeedText = persistedRunSetup.PendingSeedText ?? string.Empty;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load persisted run setup. Using defaults instead. {exception.Message}");
            }
        }

        private void SavePersistedRunSetup()
        {
            if (pendingRunSetup == null)
            {
                return;
            }

            var persistedRunSetup = new PersistedRunSetup
            {
                Seed = pendingRunSetup.Seed > 0 ? pendingRunSetup.Seed : GenerateSeed(),
                PendingSeedText = pendingSeedText ?? string.Empty,
                DifficultyPreset = (int)pendingRunSetup.DifficultyPreset,
                BallsPerServe = pendingRunSetup.BallsPerServe,
                PaddleWidthStep = pendingRunSetup.PaddleWidthStep,
                BallSpeedStep = pendingRunSetup.BallSpeedStep,
                BrickDurabilityStep = pendingRunSetup.BrickDurabilityStep,
                DropPoolModeValue = (int)pendingRunSetup.DropPoolMode,
            };

            PlayerPrefs.SetString(PersistedRunSetupKey, JsonUtility.ToJson(persistedRunSetup));
            PlayerPrefs.Save();
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
                selectedRunSetupField = (RunSetupField)Mathf.Max(0, (int)selectedRunSetupField - 1);
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                selectedRunSetupField = (RunSetupField)Mathf.Min((int)RunSetupField.DropPool, (int)selectedRunSetupField + 1);
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
                pendingRunSetup.Seed = GenerateSeed();
                pendingSeedText = pendingRunSetup.Seed.ToString(CultureInfo.InvariantCulture);
                setupChanged = true;
            }

            if (keyboard.backspaceKey.wasPressedThisFrame && selectedRunSetupField == RunSetupField.Seed && pendingSeedText.Length > 0)
            {
                pendingSeedText = pendingSeedText.Substring(0, pendingSeedText.Length - 1);
                setupChanged = true;
            }

            if (keyboard.deleteKey.wasPressedThisFrame && selectedRunSetupField == RunSetupField.Seed)
            {
                pendingSeedText = string.Empty;
                setupChanged = true;
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                ResetPendingRunSetup(generateNewSeed: true);
                setupChanged = true;
            }

            if (selectedRunSetupField == RunSetupField.Seed)
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
                SavePersistedRunSetup();
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

            switch (selectedRunSetupField)
            {
                case RunSetupField.Seed:
                    pendingRunSetup.Seed = ParsePendingSeed(commitSeedText: false);
                    pendingRunSetup.Seed = Mathf.Max(0, pendingRunSetup.Seed + direction);
                    pendingSeedText = pendingRunSetup.Seed.ToString(CultureInfo.InvariantCulture);
                    break;
                case RunSetupField.Difficulty:
                    pendingRunSetup.DifficultyPreset = (RunDifficultyPreset)Mathf.Clamp(
                        (int)pendingRunSetup.DifficultyPreset + direction,
                        (int)RunDifficultyPreset.Casual,
                        (int)RunDifficultyPreset.Brutal);
                    break;
                case RunSetupField.BallsPerServe:
                    pendingRunSetup.BallsPerServe = Mathf.Clamp(pendingRunSetup.BallsPerServe + direction, 1, 4);
                    break;
                case RunSetupField.PaddleWidth:
                    pendingRunSetup.PaddleWidthStep = Mathf.Clamp(pendingRunSetup.PaddleWidthStep + direction, -2, 2);
                    break;
                case RunSetupField.BallSpeed:
                    pendingRunSetup.BallSpeedStep = Mathf.Clamp(pendingRunSetup.BallSpeedStep + direction, -2, 2);
                    break;
                case RunSetupField.BrickDurability:
                    pendingRunSetup.BrickDurabilityStep = Mathf.Clamp(pendingRunSetup.BrickDurabilityStep + direction, -2, 2);
                    break;
                case RunSetupField.DropPool:
                    pendingRunSetup.DropPoolMode = (DropPoolMode)Mathf.Clamp(
                        (int)pendingRunSetup.DropPoolMode + direction,
                        (int)DropPoolMode.Mixed,
                        (int)DropPoolMode.Disabled);
                    break;
            }
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

        private bool AppendPressedSeedDigit(Keyboard keyboard)
        {
            if (pendingSeedText.Length >= 9)
            {
                return false;
            }

            if (TryGetPressedDigit(keyboard, out var digit))
            {
                pendingSeedText += digit;
                return true;
            }

            return false;
        }

        private static bool TryGetPressedDigit(Keyboard keyboard, out char digit)
        {
            if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame)
            {
                digit = '0';
                return true;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                digit = '1';
                return true;
            }

            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                digit = '2';
                return true;
            }

            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                digit = '3';
                return true;
            }

            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            {
                digit = '4';
                return true;
            }

            if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            {
                digit = '5';
                return true;
            }

            if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
            {
                digit = '6';
                return true;
            }

            if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame)
            {
                digit = '7';
                return true;
            }

            if (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame)
            {
                digit = '8';
                return true;
            }

            if (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame)
            {
                digit = '9';
                return true;
            }

            digit = default;
            return false;
        }

        private RunSettings BuildRunSettingsFromPending(out string validationMessage, bool commitSeedText = false)
        {
            pendingRunSetup ??= new PendingRunSetup();

            var seed = ParsePendingSeed(commitSeedText);
            pendingRunSetup.Seed = seed;

            var lives = startingLives;
            var paddleWidthMultiplier = 1f;
            var ballSpeedMultiplier = 1f;
            var brickDurabilityMultiplier = 1f;
            var dropChanceMultiplier = 1f;

            switch (pendingRunSetup.DifficultyPreset)
            {
                case RunDifficultyPreset.Casual:
                    lives += 1;
                    paddleWidthMultiplier *= 1.15f;
                    ballSpeedMultiplier *= 0.92f;
                    brickDurabilityMultiplier *= 0.9f;
                    dropChanceMultiplier *= 1.15f;
                    break;
                case RunDifficultyPreset.Brutal:
                    lives = Mathf.Max(1, lives - 1);
                    paddleWidthMultiplier *= 0.9f;
                    ballSpeedMultiplier *= 1.12f;
                    brickDurabilityMultiplier *= 1.2f;
                    dropChanceMultiplier *= 0.9f;
                    break;
            }

            paddleWidthMultiplier *= 1f + (pendingRunSetup.PaddleWidthStep * 0.12f);
            ballSpeedMultiplier *= 1f + (pendingRunSetup.BallSpeedStep * 0.08f);
            brickDurabilityMultiplier *= 1f + (pendingRunSetup.BrickDurabilityStep * 0.16f);

            paddleWidthMultiplier = Mathf.Clamp(paddleWidthMultiplier, 0.7f, 1.55f);
            ballSpeedMultiplier = Mathf.Clamp(ballSpeedMultiplier, 0.78f, 1.45f);
            brickDurabilityMultiplier = Mathf.Clamp(brickDurabilityMultiplier, 0.8f, 1.9f);

            var challengeIndex = (ballSpeedMultiplier * brickDurabilityMultiplier) / paddleWidthMultiplier;
            var warnings = new List<string>();

            if (challengeIndex > 1.75f)
            {
                var adjustedBallSpeed = Mathf.Clamp((1.75f * paddleWidthMultiplier) / brickDurabilityMultiplier, 0.78f, ballSpeedMultiplier);

                if (adjustedBallSpeed < ballSpeedMultiplier)
                {
                    ballSpeedMultiplier = adjustedBallSpeed;
                    warnings.Add("Ball speed was capped to keep the preset fair.");
                }
            }

            if (pendingRunSetup.DropPoolMode == DropPoolMode.Disabled)
            {
                warnings.Add("Drops disabled for this run.");
            }

            validationMessage = warnings.Count > 0
                ? string.Join(" ", warnings)
                : "Run validated. Same seed will replay the same level transforms, launch rolls, and drop rolls.";

            return new RunSettings(
                seed,
                pendingRunSetup.DifficultyPreset,
                lives,
                pendingRunSetup.BallsPerServe,
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                brickDurabilityMultiplier,
                dropChanceMultiplier,
                pendingRunSetup.DropPoolMode);
        }

        private int ParsePendingSeed(bool commitSeedText)
        {
            int parsedSeed;

            if (string.IsNullOrWhiteSpace(pendingSeedText))
            {
                parsedSeed = commitSeedText
                    ? GenerateSeed()
                    : pendingRunSetup != null && pendingRunSetup.Seed > 0
                        ? pendingRunSetup.Seed
                        : GenerateSeed();
            }
            else if (!int.TryParse(pendingSeedText, NumberStyles.None, CultureInfo.InvariantCulture, out parsedSeed))
            {
                parsedSeed = pendingRunSetup != null && pendingRunSetup.Seed > 0
                    ? pendingRunSetup.Seed
                    : GenerateSeed();
            }

            if (commitSeedText)
            {
                pendingSeedText = parsedSeed.ToString(CultureInfo.InvariantCulture);
            }

            return parsedSeed;
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
            var ballsPerServe = activeRunSettings == null ? 1 : activeRunSettings.BallsPerServe;

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

        private void LoadLevel(int levelIndex, RoundState serveState)
        {
            ClearBricks();
            ClearPickups();
            ClearTimedEffects();

            if (loadedLevels.Count == 0 || levelIndex < 0 || levelIndex >= loadedLevels.Count)
            {
                currentLevel = null;
                currentLevelIndex = 0;
                levelScore = 0;
                requiredBricksRemaining = 0;
                roundState = RoundState.GameOver;
                selectedOverlayActionIndex = 0;
                SetSimulationPaused(false);
                StopAllBalls();
                return;
            }

            currentLevelIndex = levelIndex;
            currentLevel = loadedLevels[currentLevelIndex];
            levelScore = 0;

            ApplyLevelTuning(currentLevel);
            BuildBrickWall(currentLevel, BuildLevelLayoutPlan(currentLevel));
            PrepareServe(serveState);
            EvaluateLevelCompletion();
        }

        private void ApplyLevelTuning(LevelDefinition level)
        {
            if (level == null)
            {
                currentLevelPaddleSpeed = paddleSpeed;
                currentLevelBallSpeed = ballSpeed;
                ApplyActiveEffects();
                return;
            }

            currentLevelPaddleSpeed = paddleSpeed * level.PaddleSpeedMultiplier;
            currentLevelBallSpeed = ballSpeed * level.BallSpeedMultiplier * (activeRunSettings?.BallSpeedMultiplier ?? 1f);
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
            bounceMaterial = new PhysicsMaterial2D("PrototypeBounce")
            {
                bounciness = 1f,
                friction = 0f,
            };
        }

        private void CreateRuntimeRoots()
        {
            runtimeRoot = new GameObject("Runtime Prototype").transform;
            runtimeRoot.SetParent(transform, false);

            wallsRoot = new GameObject("Bounds").transform;
            wallsRoot.SetParent(runtimeRoot, false);

            ballsRoot = new GameObject("Balls").transform;
            ballsRoot.SetParent(runtimeRoot, false);

            bricksRoot = new GameObject("Bricks").transform;
            bricksRoot.SetParent(runtimeRoot, false);

            pickupsRoot = new GameObject("Pickups").transform;
            pickupsRoot.SetParent(runtimeRoot, false);
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

            var collider = wallObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;
        }

        private void CreatePaddle()
        {
            var paddleObject = new GameObject("Paddle");
            paddleObject.transform.SetParent(runtimeRoot, false);
            paddleObject.transform.localScale = new Vector3(paddleSize.x, paddleSize.y, 1f);

            var spriteRenderer = paddleObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.color = paddleColor;
            spriteRenderer.sortingOrder = 10;

            var collider = paddleObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = bounceMaterial;

            var rigidbody = paddleObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            paddle = paddleObject.AddComponent<PaddleController>();
            paddle.Configure(
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

            var spriteRenderer = ballObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = circleSprite;
            spriteRenderer.color = ballColor;
            spriteRenderer.sortingOrder = 20;

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

            return ball;
        }

        private LevelLayoutPlan BuildLevelLayoutPlan(LevelDefinition level)
        {
            var layoutRows = level == null ? Array.Empty<string>() : level.LayoutRows;
            var plan = new LevelLayoutPlan
            {
                LayoutRows = new string[layoutRows.Length],
                RowShifts = new int[layoutRows.Length],
            };

            if (level == null)
            {
                currentLevelVariationLabel = "Variation: unavailable";
                return plan;
            }

            var planner = gameplayRandom != null
                ? gameplayRandom.Fork((currentLevelIndex + 1) * 7919)
                : new DeterministicRandomService(GenerateSeed());
            plan.MirrorLayout = planner.NextBool();

            for (var rowIndex = 0; rowIndex < layoutRows.Length; rowIndex++)
            {
                var transformedRow = layoutRows[rowIndex] ?? string.Empty;

                if (plan.MirrorLayout)
                {
                    transformedRow = ReverseRow(transformedRow);
                }

                var occupiedTiles = CountOccupiedTiles(transformedRow);
                var maxShift = occupiedTiles >= 2 ? Mathf.Min(2, Mathf.Max(0, transformedRow.Length / 4)) : 0;
                var rowShift = maxShift <= 0 ? 0 : planner.Range(-maxShift, maxShift + 1);

                if (occupiedTiles <= 2)
                {
                    rowShift = Mathf.Clamp(rowShift, -1, 1);
                }

                transformedRow = RotateRow(transformedRow, rowShift);
                plan.LayoutRows[rowIndex] = transformedRow;
                plan.RowShifts[rowIndex] = rowShift;
            }

            currentLevelVariationLabel = BuildVariationSummary(plan);
            Debug.Log($"Level variation | seed {activeRunSettings?.Seed ?? 0} | {currentLevel.DisplayName} | {currentLevelVariationLabel}");
            return plan;
        }

        private void BuildBrickWall(LevelDefinition level, LevelLayoutPlan layoutPlan)
        {
            requiredBricksRemaining = 0;
            if (level == null)
            {
                return;
            }

            var legend = BuildLegendLookup(level);
            var layoutRows = layoutPlan?.LayoutRows ?? level.LayoutRows;
            var startY = arenaTop - level.TopInset;

            for (var row = 0; row < layoutRows.Length; row++)
            {
                var rowLayout = layoutRows[row] ?? string.Empty;
                var totalWidth = (rowLayout.Length * brickSize.x) + (Mathf.Max(0, rowLayout.Length - 1) * brickSpacing.x);
                var startX = (-totalWidth * 0.5f) + (brickSize.x * 0.5f);

                for (var column = 0; column < rowLayout.Length; column++)
                {
                    var symbol = rowLayout[column];

                    if (symbol == '.' || char.IsWhiteSpace(symbol))
                    {
                        continue;
                    }

                    if (!legend.TryGetValue(symbol, out var definition) || definition == null)
                    {
                        Debug.LogWarning($"Level '{level.DisplayName}' is missing a brick definition for symbol '{symbol}'.");
                        continue;
                    }

                    var position = new Vector2(
                        startX + (column * (brickSize.x + brickSpacing.x)),
                        startY - (row * (brickSize.y + brickSpacing.y)));

                    CreateBrick(position, definition, row, column, GetEffectiveBrickHitPoints(definition));
                }
            }
        }

        private Dictionary<char, BrickDefinition> BuildLegendLookup(LevelDefinition level)
        {
            var legendLookup = new Dictionary<char, BrickDefinition>();
            var legendEntries = level.Legend;

            for (var index = 0; index < legendEntries.Length; index++)
            {
                var entry = legendEntries[index];
                var symbol = entry.Symbol;

                if (symbol == '\0' || char.IsWhiteSpace(symbol) || symbol == '.')
                {
                    continue;
                }

                legendLookup[symbol] = entry.BrickDefinition;
            }

            return legendLookup;
        }

        private static int CountOccupiedTiles(string rowLayout)
        {
            if (string.IsNullOrEmpty(rowLayout))
            {
                return 0;
            }

            var occupiedTiles = 0;

            for (var index = 0; index < rowLayout.Length; index++)
            {
                if (rowLayout[index] != '.' && !char.IsWhiteSpace(rowLayout[index]))
                {
                    occupiedTiles++;
                }
            }

            return occupiedTiles;
        }

        private static string ReverseRow(string rowLayout)
        {
            if (string.IsNullOrEmpty(rowLayout))
            {
                return string.Empty;
            }

            var characters = rowLayout.ToCharArray();
            Array.Reverse(characters);
            return new string(characters);
        }

        private static string RotateRow(string rowLayout, int shift)
        {
            if (string.IsNullOrEmpty(rowLayout) || shift == 0)
            {
                return rowLayout ?? string.Empty;
            }

            var length = rowLayout.Length;
            var wrappedShift = ((shift % length) + length) % length;

            if (wrappedShift == 0)
            {
                return rowLayout;
            }

            return rowLayout.Substring(length - wrappedShift, wrappedShift) + rowLayout.Substring(0, length - wrappedShift);
        }

        private static string BuildVariationSummary(LevelLayoutPlan plan)
        {
            if (plan == null || plan.LayoutRows.Length == 0)
            {
                return "Variation: authored";
            }

            var shiftedRows = 0;

            for (var index = 0; index < plan.RowShifts.Length; index++)
            {
                if (plan.RowShifts[index] != 0)
                {
                    shiftedRows++;
                }
            }

            return plan.MirrorLayout
                ? $"Variation: mirrored, {shiftedRows} shifted rows"
                : shiftedRows > 0
                    ? $"Variation: authored orientation, {shiftedRows} shifted rows"
                    : "Variation: authored orientation";
        }

        private void CreateBrick(Vector2 position, BrickDefinition definition, int row, int column, int effectiveHitPoints)
        {
            var brickObject = new GameObject($"{definition.DisplayName} {row + 1}-{column + 1}");
            brickObject.transform.SetParent(bricksRoot, false);
            brickObject.transform.position = position;
            brickObject.transform.localScale = new Vector3(brickSize.x, brickSize.y, 1f);

            var spriteRenderer = brickObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.sortingOrder = 5;

            brickObject.AddComponent<BoxCollider2D>();

            var brick = brickObject.AddComponent<Brick>();
            brick.Initialize(this, definition, effectiveHitPoints);
            bricks.Add(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining++;
            }
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

        private void EvaluateLevelCompletion()
        {
            if (currentLevel == null)
            {
                return;
            }

            var levelCleared = currentLevel.CompletionRule switch
            {
                LevelCompletionRule.ClearRequiredBricks => requiredBricksRemaining <= 0,
                LevelCompletionRule.ReachTargetScore => levelScore >= currentLevel.TargetScore,
                _ => false,
            };

            if (!levelCleared)
            {
                return;
            }

            roundState = RoundState.LevelComplete;
            selectedOverlayActionIndex = 0;
            SetSimulationPaused(false);
            ClearPickups();
            StopAllBalls();
        }

        private bool HasNextLevel()
        {
            return currentLevel != null && currentLevelIndex < loadedLevels.Count - 1;
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

        private Sprite CreateSquareSprite()
        {
            var texture = Texture2D.whiteTexture;
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
                    pixels[index] = distance <= radius ? Color.white : Color.clear;
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
            EnsureGuiStyles();

            if (roundState == RoundState.MainMenu)
            {
                DrawMainMenuUi();
                return;
            }

            if (roundState == RoundState.RunSetup)
            {
                DrawRunSetupUi();
                return;
            }

            DrawGameplayHud();

            if (roundState == RoundState.Playing)
            {
                DrawPickupBanner();
                return;
            }

            if (roundState == RoundState.Paused)
            {
                DrawPauseUi();
                DrawPickupBanner();
                return;
            }

            if (roundState == RoundState.LevelComplete || roundState == RoundState.GameOver)
            {
                DrawEndStateUi();
                DrawPickupBanner();
                return;
            }

            var message = roundState switch
            {
                RoundState.ReadyToServe => "Press Space to launch the ball. Up/Down tunes speed.",
                RoundState.LifeLost => $"Life lost. {livesRemaining} remaining. Press Space to serve again. Up/Down tunes speed.",
                _ => string.Empty,
            };

            var boxRect = new Rect((Screen.width * 0.5f) - 230f, (Screen.height * 0.5f) - 32f, 460f, 64f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(boxRect, message, messageStyle);
            DrawPickupBanner();
        }

        private void EnsureGuiStyles()
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
                && overlaySelectedActionStyle != null)
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
        }

        private void DrawMainMenuUi()
        {
            var previewSettings = BuildRunSettingsFromPending(out var previewValidation);
            var boxRect = new Rect((Screen.width * 0.5f) - 410f, (Screen.height * 0.5f) - 220f, 820f, 440f);
            var actions = GetOverlayActionsForState(RoundState.MainMenu);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 18f, boxRect.width - 48f, 36f), "Get Bricked", overlayTitleStyle);
            GUI.Label(new Rect(boxRect.x + 32f, boxRect.y + 58f, boxRect.width - 64f, 24f), "Chunk 07 meta flow: start a run, tune the setup, and move cleanly between play states.", setupHintStyle);

            GUI.Label(new Rect(boxRect.x + 42f, boxRect.y + 112f, 280f, 28f), "Main Menu", setupSelectedStyle);
            DrawOverlayActionList(actions, boxRect.x + 42f, boxRect.y + 150f, 270f, 36f);

            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 112f, 410f, 28f), "Saved Setup Preview", setupSelectedStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 150f, 410f, 26f), $"Seed: {GetPendingSeedDisplay()}", hudStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 180f, 410f, 26f), $"Difficulty: {pendingRunSetup.DifficultyPreset} | Balls/Serve: {pendingRunSetup.BallsPerServe}", hudStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 210f, 410f, 26f), $"Paddle x{previewSettings.PaddleWidthMultiplier:0.00} | Ball x{previewSettings.BallSpeedMultiplier:0.00}", hudStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 240f, 410f, 26f), $"Brick durability x{previewSettings.BrickDurabilityMultiplier:0.00} | Drops: {previewSettings.DropPoolLabel}", hudStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 278f, 410f, 74f), previewValidation, setupHintStyle);
            GUI.Label(new Rect(boxRect.x + 360f, boxRect.y + 352f, 410f, 42f), "Run setup selections persist automatically, so the quick-start option will reuse the last tuned configuration.", setupHintStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 395f, boxRect.width - 56f, 24f), "Up/Down selects. Space confirms. Open Run Setup for detailed seed and modifier edits.", setupHintStyle);
        }

        private void DrawRunSetupUi()
        {
            var previewSettings = BuildRunSettingsFromPending(out var previewValidation);
            var boxRect = new Rect((Screen.width * 0.5f) - 360f, (Screen.height * 0.5f) - 215f, 720f, 430f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 18f, boxRect.width - 48f, 34f), "Run Setup", setupTitleStyle);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 56f, boxRect.width - 56f, 22f), "Author levels stay intact, then the seed mirrors and shifts them deterministically per run.", setupHintStyle);

            var fieldX = boxRect.x + 36f;
            var fieldWidth = boxRect.width - 72f;
            var lineHeight = 30f;
            var startY = boxRect.y + 96f;

            DrawRunSetupField(fieldX, startY + (lineHeight * 0f), fieldWidth, RunSetupField.Seed, $"Seed: {GetPendingSeedDisplay()}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 1f), fieldWidth, RunSetupField.Difficulty, $"Difficulty: {pendingRunSetup.DifficultyPreset}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 2f), fieldWidth, RunSetupField.BallsPerServe, $"Balls Per Serve: {pendingRunSetup.BallsPerServe}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 3f), fieldWidth, RunSetupField.PaddleWidth, $"Paddle Width Bias: {FormatSignedStep(pendingRunSetup.PaddleWidthStep)}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 4f), fieldWidth, RunSetupField.BallSpeed, $"Ball Speed Bias: {FormatSignedStep(pendingRunSetup.BallSpeedStep)}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 5f), fieldWidth, RunSetupField.BrickDurability, $"Brick Durability Bias: {FormatSignedStep(pendingRunSetup.BrickDurabilityStep)}");
            DrawRunSetupField(fieldX, startY + (lineHeight * 6f), fieldWidth, RunSetupField.DropPool, $"Drop Pool: {previewSettings.DropPoolLabel}");

            GUI.Label(
                new Rect(boxRect.x + 28f, boxRect.y + 315f, boxRect.width - 56f, 24f),
                $"Preview: Lives {previewSettings.StartingLives} | Paddle x{previewSettings.PaddleWidthMultiplier:0.00} | Ball speed x{previewSettings.BallSpeedMultiplier:0.00} | Brick durability x{previewSettings.BrickDurabilityMultiplier:0.00}",
                hudStyle);
            GUI.Label(
                new Rect(boxRect.x + 28f, boxRect.y + 345f, boxRect.width - 56f, 40f),
                previewValidation,
                setupHintStyle);
            GUI.Label(
                new Rect(boxRect.x + 28f, boxRect.y + 384f, boxRect.width - 56f, 28f),
                "Up/Down selects. Left/Right adjusts. Type digits for the seed. Backspace edits. T randomizes. N resets defaults. Esc returns to menu. Space starts.",
                setupHintStyle);
        }

        private void DrawRunSetupField(float x, float y, float width, RunSetupField field, string value)
        {
            var isSelected = selectedRunSetupField == field;
            GUI.Label(new Rect(x, y, width, 26f), $"{(isSelected ? "> " : "  ")}{value}", isSelected ? setupSelectedStyle : hudStyle);
        }

        private void DrawGameplayHud()
        {
            var statsBoxRect = new Rect(16f, 16f, Screen.width - 32f, 98f);
            var detailBoxRect = new Rect(16f, 122f, Screen.width - 32f, string.IsNullOrWhiteSpace(pendingValidationMessage) ? 96f : 122f);

            GUI.Box(statsBoxRect, GUIContent.none);
            GUI.Box(detailBoxRect, GUIContent.none);

            GUI.Label(
                new Rect(statsBoxRect.x + 16f, statsBoxRect.y + 12f, statsBoxRect.width - 32f, 24f),
                $"Score {score:0000}   Lives {livesRemaining:00}   Balls {Mathf.Max(0, activeBalls.Count):00}   {BuildLevelLabel()}",
                hudStyle);
            GUI.Label(
                new Rect(statsBoxRect.x + 16f, statsBoxRect.y + 40f, statsBoxRect.width - 32f, 24f),
                BuildObjectiveLabel(),
                hudStyle);
            GUI.Label(
                new Rect(statsBoxRect.x + 16f, statsBoxRect.y + 68f, statsBoxRect.width - 32f, 24f),
                BuildRunSummaryLabel(),
                hudStyle);

            GUI.Label(
                new Rect(detailBoxRect.x + 16f, detailBoxRect.y + 10f, detailBoxRect.width - 32f, 24f),
                $"{currentLevelVariationLabel}   |   State: {BuildRoundStateLabel()}",
                hudStyle);
            GUI.Label(
                new Rect(detailBoxRect.x + 16f, detailBoxRect.y + 36f, detailBoxRect.width - 32f, 24f),
                BuildBallSpeedControlLabel(),
                hudStyle);
            GUI.Label(
                new Rect(detailBoxRect.x + 16f, detailBoxRect.y + 62f, detailBoxRect.width - 32f, 24f),
                BuildActiveEffectsLabel(),
                hudStyle);

            if (!string.IsNullOrWhiteSpace(pendingValidationMessage))
            {
                GUI.Label(
                    new Rect(detailBoxRect.x + 16f, detailBoxRect.y + 88f, detailBoxRect.width - 32f, 24f),
                    pendingValidationMessage,
                    hudStyle);
            }

            GUI.Label(
                new Rect(24f, detailBoxRect.yMax + 8f, Screen.width - 48f, 24f),
                "Move with A/D or Left/Right. Up/Down tunes ball speed. Space launches or advances. Esc/P pauses. R reopens run setup.",
                hudStyle);
        }

        private void DrawPauseUi()
        {
            var boxRect = new Rect((Screen.width * 0.5f) - 280f, (Screen.height * 0.5f) - 150f, 560f, 300f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 20f, boxRect.width - 48f, 34f), "Paused", overlayTitleStyle);
            GUI.Label(new Rect(boxRect.x + 32f, boxRect.y + 64f, boxRect.width - 64f, 24f), BuildPauseSummaryLabel(), overlayBodyStyle);
            GUI.Label(new Rect(boxRect.x + 32f, boxRect.y + 92f, boxRect.width - 64f, 24f), currentLevelVariationLabel, overlayBodyStyle);
            DrawOverlayActionList(GetOverlayActionsForState(RoundState.Paused), boxRect.x + 90f, boxRect.y + 136f, boxRect.width - 180f, 34f);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 258f, boxRect.width - 56f, 24f), "Up/Down selects. Space confirms. Esc or P resumes immediately.", setupHintStyle);
        }

        private void DrawEndStateUi()
        {
            var isGameOver = roundState == RoundState.GameOver;
            var title = isGameOver
                ? "Run Over"
                : HasNextLevel()
                    ? "Level Cleared"
                    : "Final Layout Cleared";
            var summary = isGameOver
                ? $"Score {score:0000} | Reached {BuildLevelLabel()} | Seed {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}"
                : HasNextLevel()
                    ? $"Score {score:0000} | Lives {livesRemaining:00} | Next up: level {currentLevelIndex + 2:00}"
                    : $"Score {score:0000} | Lives {livesRemaining:00} | Seed {activeRunSettings?.Seed.ToString(CultureInfo.InvariantCulture) ?? GetPendingSeedDisplay()}";
            var footer = isGameOver
                ? "Restart the run, jump back to setup, or return to the main menu."
                : HasNextLevel()
                    ? "Advance to the next authored layout, restart the run, or return to the menu."
                    : "The authored run is complete. Restart, tune a new setup, or head back to the menu.";
            var boxRect = new Rect((Screen.width * 0.5f) - 320f, (Screen.height * 0.5f) - 162f, 640f, 324f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 20f, boxRect.width - 48f, 36f), title, overlayTitleStyle);
            GUI.Label(new Rect(boxRect.x + 36f, boxRect.y + 68f, boxRect.width - 72f, 24f), summary, overlayBodyStyle);
            GUI.Label(new Rect(boxRect.x + 36f, boxRect.y + 96f, boxRect.width - 72f, 24f), BuildRunSummaryLabel(), overlayBodyStyle);
            DrawOverlayActionList(GetOverlayActionsForState(roundState), boxRect.x + 94f, boxRect.y + 146f, boxRect.width - 188f, 36f);
            GUI.Label(new Rect(boxRect.x + 28f, boxRect.y + 274f, boxRect.width - 56f, 32f), footer, setupHintStyle);
        }

        private void DrawOverlayActionList(OverlayAction[] actions, float x, float y, float width, float lineHeight)
        {
            for (var index = 0; index < actions.Length; index++)
            {
                var isSelected = index == Mathf.Clamp(selectedOverlayActionIndex, 0, actions.Length - 1);
                var label = $"{(isSelected ? "> " : "  ")}{GetOverlayActionLabel(actions[index])}";
                GUI.Label(new Rect(x, y + (lineHeight * index), width, lineHeight), label, isSelected ? overlaySelectedActionStyle : overlayActionStyle);
            }
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
                _ => action.ToString(),
            };
        }

        private string BuildLevelLabel()
        {
            return currentLevel == null
                ? "No levels loaded"
                : $"Level {currentLevelIndex + 1:00}/{loadedLevels.Count:00} - {currentLevel.DisplayName}";
        }

        private string BuildObjectiveLabel()
        {
            if (currentLevel == null)
            {
                return "Objective unavailable.";
            }

            return currentLevel.CompletionRule == LevelCompletionRule.ClearRequiredBricks
                ? $"Objective: Clear remaining breakable bricks ({requiredBricksRemaining:00} left)."
                : $"Objective: Score {currentLevel.TargetScore:0000} points this level ({levelScore:0000}/{currentLevel.TargetScore:0000}).";
        }

        private string BuildPauseSummaryLabel()
        {
            return $"Score {score:0000} | Lives {livesRemaining:00} | Balls {Mathf.Max(0, activeBalls.Count):00} | {BuildLevelLabel()}";
        }

        private string BuildBallSpeedControlLabel()
        {
            var baseSpeed = GetBallSpeedBase();
            var currentSpeed = GetCurrentBallSpeed();
            var maxSpeed = GetMaximumBallSpeed();
            return
                $"Ball Speed {currentSpeed:0.00} | Base {baseSpeed:0.00} | Manual x{manualBallSpeedMultiplier:0.00} | Cap {maxSpeed:0.00}";
        }

        private string BuildRoundStateLabel()
        {
            return roundState switch
            {
                RoundState.ReadyToServe => "Ready to serve",
                RoundState.Playing => "Ball in play",
                RoundState.LifeLost => "Recovering from a loss",
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
                return $"Run Seed: {GetPendingSeedDisplay()} | Preview only";
            }

            return
                $"Run Seed: {activeRunSettings.Seed} | {activeRunSettings.DifficultyLabel} | Balls/Serve {activeRunSettings.BallsPerServe} | " +
                $"Drops: {activeRunSettings.DropPoolLabel} | Paddle x{activeRunSettings.PaddleWidthMultiplier:0.00} | Ball x{activeRunSettings.BallSpeedMultiplier:0.00}";
        }

        private string GetPendingSeedDisplay()
        {
            return string.IsNullOrWhiteSpace(pendingSeedText) ? "<random on start>" : pendingSeedText;
        }

        private static string FormatSignedStep(int value)
        {
            return value == 0 ? "0" : value > 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateTimedEffects()
        {
            if (roundState != RoundState.Playing || activeTimedEffects.Count == 0)
            {
                return;
            }

            var modifiersChanged = false;

            for (var index = activeTimedEffects.Count - 1; index >= 0; index--)
            {
                var activeEffect = activeTimedEffects[index];

                if (activeEffect.Definition == null)
                {
                    activeTimedEffects.RemoveAt(index);
                    modifiersChanged = true;
                    continue;
                }

                activeEffect.RemainingDuration = Mathf.Max(0f, activeEffect.RemainingDuration - Time.deltaTime);

                if (activeEffect.RemainingDuration > 0f)
                {
                    continue;
                }

                activeTimedEffects.RemoveAt(index);
                modifiersChanged = true;
            }

            if (modifiersChanged)
            {
                ApplyActiveEffects();
            }
        }

        private void UpdatePickupBanner()
        {
            if (pickupBannerTimer <= 0f)
            {
                return;
            }

            pickupBannerTimer = Mathf.Max(0f, pickupBannerTimer - Time.deltaTime);
        }

        private void TrySpawnPickup(Brick brick)
        {
            if (brick == null || brick.Definition == null)
            {
                return;
            }

            var brickDefinition = brick.Definition;
            var dropTable = brickDefinition.DropTable;
            var effectiveDropChance = Mathf.Clamp01(brickDefinition.DropChance * (activeRunSettings?.DropChanceMultiplier ?? 1f));

            if (dropTable.Length == 0
                || effectiveDropChance <= 0f
                || activeRunSettings?.DropPoolMode == DropPoolMode.Disabled
                || NextGameplayRandomFloat(0f, 1f) > effectiveDropChance)
            {
                return;
            }

            var totalWeight = 0f;

            for (var index = 0; index < dropTable.Length; index++)
            {
                if (!IsDropAllowed(dropTable[index].PowerUpDefinition))
                {
                    continue;
                }

                totalWeight += dropTable[index].Weight;
            }

            if (totalWeight <= 0f)
            {
                return;
            }

            var roll = NextGameplayRandomFloat(0f, totalWeight);
            PowerUpDefinition selectedPowerUp = null;

            for (var index = 0; index < dropTable.Length; index++)
            {
                var entry = dropTable[index];

                if (!IsDropAllowed(entry.PowerUpDefinition))
                {
                    continue;
                }

                roll -= entry.Weight;

                if (roll > 0f)
                {
                    continue;
                }

                selectedPowerUp = entry.PowerUpDefinition;
                break;
            }

            if (selectedPowerUp == null)
            {
                return;
            }

            CreatePickup((Vector2)brick.transform.position, selectedPowerUp);
        }

        private bool IsDropAllowed(PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
            {
                return false;
            }

            var dropPoolMode = activeRunSettings?.DropPoolMode ?? DropPoolMode.Mixed;

            return dropPoolMode switch
            {
                DropPoolMode.HelpfulOnly => powerUpDefinition.IsBeneficial,
                DropPoolMode.HarmfulOnly => !powerUpDefinition.IsBeneficial,
                DropPoolMode.Disabled => false,
                _ => true,
            };
        }

        private void CreatePickup(Vector2 position, PowerUpDefinition powerUpDefinition)
        {
            var pickupObject = new GameObject(powerUpDefinition.DisplayName);
            pickupObject.transform.SetParent(pickupsRoot, false);
            pickupObject.transform.position = position;
            pickupObject.transform.localScale = new Vector3(pickupSize.x, pickupSize.y, 1f);
            pickupObject.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.sortingOrder = 14;

            pickupObject.AddComponent<BoxCollider2D>();

            var pickup = pickupObject.AddComponent<PowerUpPickup>();
            pickup.Configure(this, powerUpDefinition, pickupFallSpeed, arenaBottom - 0.9f);
            activePickups.Add(pickup);
        }

        private void ApplyPowerUp(PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
            {
                return;
            }

            ShowPickupBanner(powerUpDefinition);

            if (powerUpDefinition.IsTimed)
            {
                AddOrExtendTimedEffect(powerUpDefinition);
                return;
            }

            if (powerUpDefinition.EffectType == PowerUpEffectType.MultiBallBurst)
            {
                SpawnMultiBall(powerUpDefinition);
            }
        }

        private void AddOrExtendTimedEffect(PowerUpDefinition powerUpDefinition)
        {
            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var activeEffect = activeTimedEffects[index];

                if (activeEffect.Definition != powerUpDefinition)
                {
                    continue;
                }

                activeEffect.RemainingDuration += powerUpDefinition.DurationSeconds;
                ApplyActiveEffects();
                return;
            }

            activeTimedEffects.Add(new ActiveTimedEffect(powerUpDefinition, powerUpDefinition.DurationSeconds));
            ApplyActiveEffects();
        }

        private void ApplyActiveEffects()
        {
            var paddleWidthMultiplier = activeRunSettings?.PaddleWidthMultiplier ?? 1f;

            // Matching effects extend duration; opposing effects multiply together and naturally cancel each other out.
            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var powerUpDefinition = activeTimedEffects[index].Definition;

                if (powerUpDefinition == null || powerUpDefinition.EffectType != PowerUpEffectType.PaddleWidthMultiplier)
                {
                    continue;
                }

                paddleWidthMultiplier *= powerUpDefinition.Scalar;
            }

            paddle.SetMoveSpeed(currentLevelPaddleSpeed);
            paddle.SetWidthMultiplier(Mathf.Clamp(paddleWidthMultiplier, 0.6f, 1.8f));

            var currentBallSpeed = GetCurrentBallSpeed();

            if (serveBall != null)
            {
                serveBall.SetMovementSpeed(currentBallSpeed);
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
            var extraBallCount = Mathf.Max(1, powerUpDefinition.ExtraBallCount);

            for (var index = 0; index < extraBallCount; index++)
            {
                var angle = extraBallCount == 1
                    ? 0f
                    : Mathf.Lerp(-multiBallSpreadAngle, multiBallSpreadAngle, index / (extraBallCount - 1f));
                var direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * sourceDirection);

                var extraBall = CreateBall(false);
                extraBall.SetWorldPosition(sourceBall.transform.position);
                extraBall.Launch(direction);
                activeBalls.Add(extraBall);
            }
        }

        private void ClearPickups()
        {
            for (var index = activePickups.Count - 1; index >= 0; index--)
            {
                if (activePickups[index] == null)
                {
                    continue;
                }

                activePickups[index].gameObject.SetActive(false);
                Destroy(activePickups[index].gameObject);
            }

            activePickups.Clear();
        }

        private void ClearTimedEffects()
        {
            activeTimedEffects.Clear();
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

        private float GetBallSpeedBase()
        {
            return currentLevelBallSpeed * Mathf.Clamp(GetTimedBallSpeedMultiplier(), 0.6f, 1.75f);
        }

        private float GetMaximumBallSpeed()
        {
            return GetBallSpeedBase() * Mathf.Max(1f, manualBallSpeedMaxMultiplier);
        }

        private float GetTimedBallSpeedMultiplier()
        {
            var timedBallSpeedMultiplier = 1f;

            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var powerUpDefinition = activeTimedEffects[index].Definition;

                if (powerUpDefinition == null || powerUpDefinition.EffectType != PowerUpEffectType.BallSpeedMultiplier)
                {
                    continue;
                }

                timedBallSpeedMultiplier *= powerUpDefinition.Scalar;
            }

            return timedBallSpeedMultiplier;
        }

        private string BuildActiveEffectsLabel()
        {
            if (activeTimedEffects.Count == 0)
            {
                return "Active Effects: none";
            }

            var builder = new StringBuilder("Active Effects: ");

            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var activeEffect = activeTimedEffects[index];

                if (activeEffect.Definition == null)
                {
                    continue;
                }

                if (builder.Length > 16)
                {
                    builder.Append(" | ");
                }

                builder.Append(activeEffect.Definition.HudLabel);
                builder.Append(' ');
                builder.Append(activeEffect.RemainingDuration.ToString("0.0"));
                builder.Append('s');
            }

            return builder.ToString();
        }

        private void ShowPickupBanner(PowerUpDefinition powerUpDefinition)
        {
            pickupBannerText = powerUpDefinition.IsBeneficial
                ? $"+ {powerUpDefinition.DisplayName}"
                : $"- {powerUpDefinition.DisplayName}";
            pickupBannerColor = powerUpDefinition.PickupColor;
            pickupBannerTimer = 1.6f;
        }

        private void DrawPickupBanner()
        {
            if (pickupBannerTimer <= 0f || string.IsNullOrWhiteSpace(pickupBannerText))
            {
                return;
            }

            var rect = new Rect((Screen.width * 0.5f) - 170f, 226f, 340f, 36f);
            var previousGuiColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = pickupBannerColor;
            GUI.Label(rect, pickupBannerText, pickupStyle);
            GUI.color = previousGuiColor;
        }
    }
}
