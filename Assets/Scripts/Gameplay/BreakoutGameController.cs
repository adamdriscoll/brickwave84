using System;
using System.Collections.Generic;
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

        private enum RoundState
        {
            ReadyToServe,
            Playing,
            LifeLost,
            LevelComplete,
            GameOver,
        }

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
        private LevelDefinition currentLevel;
        private int currentLevelIndex;
        private int levelScore;
        private float currentLevelBallSpeed;
        private float currentLevelPaddleSpeed;
        private int ballInstanceCounter;
        private string pickupBannerText;
        private float pickupBannerTimer;
        private Color pickupBannerColor = Color.white;

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
            StartNewRun();
        }

        private void OnDestroy()
        {
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

            if (keyboard.rKey.wasPressedThisFrame)
            {
                StartNewRun();
                return;
            }

            if ((keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) == false)
            {
                return;
            }

            if (roundState == RoundState.ReadyToServe || roundState == RoundState.LifeLost)
            {
                roundState = RoundState.Playing;
                serveBall.Launch();
                return;
            }

            if (roundState == RoundState.LevelComplete || roundState == RoundState.GameOver)
            {
                if (roundState == RoundState.LevelComplete && HasNextLevel())
                {
                    LoadLevel(currentLevelIndex + 1, RoundState.ReadyToServe);
                    return;
                }

                StartNewRun();
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
            livesRemaining = Mathf.Max(1, startingLives);
            score = 0;
            currentLevelIndex = 0;
            ClearTimedEffects();
            ClearPickups();
            LoadLevel(currentLevelIndex, RoundState.ReadyToServe);
        }

        private void PrepareServe(RoundState nextState)
        {
            roundState = nextState;
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
                StopAllBalls();
                return;
            }

            currentLevelIndex = levelIndex;
            currentLevel = loadedLevels[currentLevelIndex];
            levelScore = 0;

            ApplyLevelTuning(currentLevel);
            BuildBrickWall(currentLevel);
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
            currentLevelBallSpeed = ballSpeed * level.BallSpeedMultiplier;
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

        private void BuildBrickWall(LevelDefinition level)
        {
            requiredBricksRemaining = 0;
            if (level == null)
            {
                return;
            }

            var legend = BuildLegendLookup(level);
            var layoutRows = level.LayoutRows;
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

                    CreateBrick(position, definition, row, column);
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

        private void CreateBrick(Vector2 position, BrickDefinition definition, int row, int column)
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
            brick.Initialize(this, definition);
            bricks.Add(brick);

            if (brick.CountsTowardLevelCompletion)
            {
                requiredBricksRemaining++;
            }
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

            var levelLabel = currentLevel == null
                ? "No levels loaded"
                : $"Level: {currentLevelIndex + 1:00}/{loadedLevels.Count:00} - {currentLevel.DisplayName}";

            var objectiveLabel = currentLevel == null
                ? "Objective unavailable."
                : currentLevel.CompletionRule == LevelCompletionRule.ClearRequiredBricks
                    ? $"Objective: Clear remaining breakable bricks ({requiredBricksRemaining:00} left)."
                    : $"Objective: Score {currentLevel.TargetScore:0000} points this level ({levelScore:0000}/{currentLevel.TargetScore:0000}).";

            GUI.Label(new Rect(16f, 16f, 900f, 30f), $"Score: {score:0000}   Lives: {livesRemaining:00}   {levelLabel}", hudStyle);
            GUI.Label(new Rect(16f, 48f, 1100f, 28f), objectiveLabel, hudStyle);
            GUI.Label(new Rect(16f, 80f, 1100f, 28f), "Move with A/D or Left/Right. Launch or continue with Space. Press R to restart the run.", hudStyle);
            GUI.Label(new Rect(16f, 112f, 1200f, 28f), BuildActiveEffectsLabel(), hudStyle);

            if (roundState == RoundState.Playing)
            {
                DrawPickupBanner();
                return;
            }

            var message = roundState switch
            {
                RoundState.ReadyToServe => "Press Space to launch the ball.",
                RoundState.LifeLost => $"Life lost. {livesRemaining} remaining. Press Space to serve again.",
                RoundState.LevelComplete => HasNextLevel()
                    ? "Level cleared. Press Space to load the next layout."
                    : "Final level cleared. Press Space to start a new run.",
                RoundState.GameOver => "Game over. Press Space to restart.",
                _ => string.Empty,
            };

            var boxRect = new Rect((Screen.width * 0.5f) - 230f, (Screen.height * 0.5f) - 32f, 460f, 64f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.Label(boxRect, message, messageStyle);
            DrawPickupBanner();
        }

        private void EnsureGuiStyles()
        {
            if (hudStyle != null && messageStyle != null && pickupStyle != null)
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

            if (dropTable.Length == 0 || brickDefinition.DropChance <= 0f || UnityEngine.Random.value > brickDefinition.DropChance)
            {
                return;
            }

            var totalWeight = 0f;

            for (var index = 0; index < dropTable.Length; index++)
            {
                totalWeight += dropTable[index].Weight;
            }

            if (totalWeight <= 0f)
            {
                return;
            }

            var roll = UnityEngine.Random.value * totalWeight;
            PowerUpDefinition selectedPowerUp = null;

            for (var index = 0; index < dropTable.Length; index++)
            {
                var entry = dropTable[index];
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
            var paddleWidthMultiplier = 1f;
            var ballSpeedMultiplier = 1f;

            // Matching effects extend duration; opposing effects multiply together and naturally cancel each other out.
            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var powerUpDefinition = activeTimedEffects[index].Definition;

                if (powerUpDefinition == null)
                {
                    continue;
                }

                switch (powerUpDefinition.EffectType)
                {
                    case PowerUpEffectType.PaddleWidthMultiplier:
                        paddleWidthMultiplier *= powerUpDefinition.Scalar;
                        break;
                    case PowerUpEffectType.BallSpeedMultiplier:
                        ballSpeedMultiplier *= powerUpDefinition.Scalar;
                        break;
                }
            }

            paddle.SetMoveSpeed(currentLevelPaddleSpeed);
            paddle.SetWidthMultiplier(Mathf.Clamp(paddleWidthMultiplier, 0.6f, 1.8f));

            var currentBallSpeed = currentLevelBallSpeed * Mathf.Clamp(ballSpeedMultiplier, 0.6f, 1.75f);

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
            var ballSpeedMultiplier = 1f;

            for (var index = 0; index < activeTimedEffects.Count; index++)
            {
                var powerUpDefinition = activeTimedEffects[index].Definition;

                if (powerUpDefinition == null || powerUpDefinition.EffectType != PowerUpEffectType.BallSpeedMultiplier)
                {
                    continue;
                }

                ballSpeedMultiplier *= powerUpDefinition.Scalar;
            }

            return currentLevelBallSpeed * Mathf.Clamp(ballSpeedMultiplier, 0.6f, 1.75f);
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

            var rect = new Rect((Screen.width * 0.5f) - 170f, 18f, 340f, 36f);
            var previousGuiColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = pickupBannerColor;
            GUI.Label(rect, pickupBannerText, pickupStyle);
            GUI.color = previousGuiColor;
        }
    }
}
