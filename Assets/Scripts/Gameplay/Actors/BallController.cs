using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BallController : MonoBehaviour
    {
        public const float MaximumSizeMultiplier = 1.8f;
        private static readonly Color SolarShotColor = new Color(1f, 0.72f, 0.08f, 1f);

        [SerializeField] private float maxPaddleBounceAngle = 70f;

        private BreakoutGameController gameController;
        private PaddleController paddle;
        private Rigidbody2D ballBody;
        private float launchSpeed;
        private float minimumVerticalDirection;
        private float lossThresholdY;
        private float paddleFollowOffset;
        private Vector3 configuredBaseScale = Vector3.one;
        private Vector3 baseScale = Vector3.one;
        private float sizeMultiplier = 1f;
        private float speedBurstMultiplier = 1f;
        private float speedBurstTimeRemaining;
        private float jellySlowMultiplier = 1f;
        private float jellySlowTimeRemaining;
        private bool attachedToPaddle;
        private bool followsPaddleWhenIdle;
        private bool hasLaunched;
        private bool phaseThroughBricks;
        private bool weakBrickPierceThroughBricks;
        private float gravityWellStrength;
        private Vector2 gravityWellPoint;
        private float gravityPocketStrength;
        private float gravityPocketRadius;
        private Vector2 gravityPocketPoint;
        private bool splitHorizonEnabled;
        private float splitHorizonY;
        private float splitHorizonBendDegrees;
        private float splitHorizonCooldownSeconds;
        private float splitHorizonCooldownTimer;
        private float splitHorizonPreviousOffset;
        private float brickMagnetStrength;
        private Vector2 brickMagnetPoint;
        private float hotPotatoStrength;
        private float hotPotatoSpeedMultiplier = 1f;
        private float explosiveBallStrength;
        private bool solarShotCharged;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private ThemeVisualStyle baseVisualStyle = new ThemeVisualStyle(Color.white, Color.white, null);
        private Vector2 lastTravelDirection = Vector2.up;
        private Vector2 lastPaddleHitRewindPosition;
        private Vector2 lastPaddleHitRewindDirection = Vector2.up;
        private int ricochetCountSinceLastBrick;
        private bool hasLastPaddleHitRewindAnchor;

        public float CurrentSpeed => ballBody != null ? ballBody.linearVelocity.magnitude : 0f;

        public Vector2 CurrentVelocity => ballBody != null ? ballBody.linearVelocity : Vector2.zero;

        public bool IsAttachedToPaddle => attachedToPaddle;

        public bool HasLaunched => hasLaunched;

        public int RicochetCountSinceLastBrick => ricochetCountSinceLastBrick;

        public bool IsExplosiveBall => explosiveBallStrength > 0.001f;

        public float ExplosiveBallStrength => explosiveBallStrength;

        public void Configure(
            BreakoutGameController controller,
            PaddleController paddleController,
            float speed,
            float minimumVertical,
            float lossY,
            float followOffset,
            bool followPaddleWhenIdle)
        {
            gameController = controller;
            paddle = paddleController;
            launchSpeed = speed;
            minimumVerticalDirection = Mathf.Clamp(minimumVertical, 0.15f, 0.95f);
            lossThresholdY = lossY;
            paddleFollowOffset = followOffset;
            followsPaddleWhenIdle = followPaddleWhenIdle;
            configuredBaseScale = transform.localScale;
            baseScale = configuredBaseScale;
            ballBody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();
            CaptureCurrentVisualStyleAsFallback();
        }

        public void ResetToPaddle()
        {
            hasLaunched = false;
            attachedToPaddle = false;
            ClearSpeedBurst();
            ClearJellySlow();
            ResetHotPotato();
            ricochetCountSinceLastBrick = 0;
            ClearPaddleHitRewindAnchor();
            SetBaseSizeMultiplier(1f);

            if (ballBody == null)
            {
                ballBody = GetComponent<Rigidbody2D>();
            }

            ballBody.linearVelocity = Vector2.zero;

            if (paddle == null)
            {
                return;
            }

            SetWorldPosition((Vector2)paddle.transform.position + (Vector2.up * ResolvePaddleFollowOffset()));
        }

        public void Launch()
        {
            var horizontalLaunch = gameController != null
                ? gameController.NextGameplayRandomFloat(-0.25f, 0.25f)
                : Random.Range(-0.25f, 0.25f);
            Launch(new Vector2(horizontalLaunch, 1f));
        }

        public void Launch(Vector2 direction)
        {
            if (hasLaunched || ballBody == null)
            {
                return;
            }

            attachedToPaddle = false;
            hasLaunched = true;
            ResetHotPotato();
            ricochetCountSinceLastBrick = 0;

            var launchDirection = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.up;

            if (Mathf.Abs(launchDirection.y) < minimumVerticalDirection)
            {
                launchDirection = new Vector2(
                    launchDirection.x,
                    Mathf.Sign(Mathf.Approximately(launchDirection.y, 0f) ? 1f : launchDirection.y) * minimumVerticalDirection).normalized;
            }

            lastTravelDirection = launchDirection;
            ballBody.linearVelocity = launchDirection * GetTargetSpeed();
            gameController?.HandleBallLaunched();
        }

        public void SetMovementSpeed(float speed)
        {
            launchSpeed = Mathf.Max(0.1f, speed);

            if (ballBody != null && hasLaunched && ballBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
            }
        }

        public void SetBaseSizeMultiplier(float multiplier)
        {
            var clampedMultiplier = Mathf.Clamp(multiplier, 0.1f, MaximumSizeMultiplier);
            baseScale = configuredBaseScale * clampedMultiplier;
            SetSizeMultiplier(sizeMultiplier);
        }

        public void SetPhaseThroughBricks(bool enabled)
        {
            phaseThroughBricks = enabled;
        }

        public void SetWeakBrickPierceThroughBricks(bool enabled)
        {
            weakBrickPierceThroughBricks = enabled;
        }

        public void SetSizeMultiplier(float multiplier)
        {
            sizeMultiplier = multiplier > 0.001f ? Mathf.Clamp(multiplier, 0.1f, MaximumSizeMultiplier) : 1f;
            transform.localScale = new Vector3(
                baseScale.x * sizeMultiplier,
                baseScale.y * sizeMultiplier,
                baseScale.z);

            if (!hasLaunched && paddle != null && (attachedToPaddle || followsPaddleWhenIdle))
            {
                SetWorldPosition((Vector2)paddle.transform.position + (Vector2.up * ResolvePaddleFollowOffset()));
            }
        }

        public void ApplyVisualStyle(ThemeVisualStyle visualStyle)
        {
            var visualSprite = visualStyle.Sprite != null || spriteRenderer == null
                ? visualStyle.Sprite
                : spriteRenderer.sprite;
            baseVisualStyle = new ThemeVisualStyle(visualStyle.PrimaryColor, visualStyle.SecondaryColor, visualSprite);
            RefreshVisualStyle();
        }

        public void SetExplosiveBallStrength(float strength)
        {
            explosiveBallStrength = Mathf.Max(0f, strength);
            RefreshVisualStyle();
        }

        public void SetSolarShotCharged(bool charged)
        {
            if (solarShotCharged == charged)
            {
                return;
            }

            solarShotCharged = charged;
            RefreshVisualStyle();
        }

        public void SetGravityWell(Vector2 centerPoint, float strength)
        {
            gravityWellPoint = centerPoint;
            gravityWellStrength = Mathf.Clamp01(strength);
        }

        public void SetGravityPocket(Vector2 centerPoint, float radius, float strength)
        {
            gravityPocketPoint = centerPoint;
            gravityPocketRadius = Mathf.Max(0f, radius);
            gravityPocketStrength = gravityPocketRadius > 0.001f ? Mathf.Clamp01(strength) : 0f;
        }

        public void SetSplitHorizon(float horizonY, float bendDegrees, float cooldownSeconds)
        {
            splitHorizonEnabled = bendDegrees > 0.001f;
            splitHorizonY = horizonY;
            splitHorizonBendDegrees = Mathf.Clamp(bendDegrees, 0f, 28f);
            splitHorizonCooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.02f, 0.6f);
            splitHorizonCooldownTimer = 0f;
            splitHorizonPreviousOffset = transform.position.y - splitHorizonY;
        }

        public void SetBrickMagnetTarget(Vector2 targetPoint, float strength)
        {
            brickMagnetPoint = targetPoint;
            brickMagnetStrength = Mathf.Clamp(strength, -1f, 1f);
        }

        public void SetHotPotatoStrength(float strength)
        {
            hotPotatoStrength = Mathf.Clamp01(strength);

            if (hotPotatoStrength <= 0.001f)
            {
                hotPotatoSpeedMultiplier = 1f;
            }
        }

        public void AttachToPaddle()
        {
            if (ballBody == null)
            {
                ballBody = GetComponent<Rigidbody2D>();
            }

            attachedToPaddle = true;
            hasLaunched = false;
            ClearSpeedBurst();
            ClearJellySlow();
            ResetHotPotato();
            ricochetCountSinceLastBrick = 0;

            if (ballBody != null)
            {
                ballBody.linearVelocity = Vector2.zero;
            }

            if (paddle != null)
            {
                SetWorldPosition((Vector2)paddle.transform.position + (Vector2.up * ResolvePaddleFollowOffset()));
            }
        }

        public void PassThroughPaddle(float horizontalDirection)
        {
            if (ballBody == null)
            {
                return;
            }

            attachedToPaddle = false;
            hasLaunched = true;
            var xDirection = Mathf.Abs(horizontalDirection) < 0.15f
                ? ((gameController != null && gameController.NextGameplayRandomBool()) ? -0.35f : 0.35f)
                : Mathf.Sign(horizontalDirection) * Mathf.Max(0.2f, Mathf.Abs(horizontalDirection));
            var downwardDirection = new Vector2(xDirection, -1f).normalized;
            lastTravelDirection = downwardDirection;
            ballBody.position += downwardDirection * 0.08f;
            ballBody.linearVelocity = downwardDirection * GetTargetSpeed();
        }

        public void BounceFromShield(float yPosition)
        {
            if (ballBody == null)
            {
                return;
            }

            var currentVelocity = ballBody.linearVelocity;
            var rescuedDirection = currentVelocity.sqrMagnitude > 0.01f
                ? new Vector2(currentVelocity.x, Mathf.Abs(currentVelocity.y))
                : new Vector2(
                    gameController != null ? gameController.NextGameplayRandomFloat(-0.45f, 0.45f) : Random.Range(-0.45f, 0.45f),
                    1f);

            if (Mathf.Abs(rescuedDirection.y) < minimumVerticalDirection)
            {
                rescuedDirection = new Vector2(
                    rescuedDirection.x,
                    minimumVerticalDirection).normalized;
            }

            attachedToPaddle = false;
            hasLaunched = true;
            var rescuedPosition = new Vector2(transform.position.x, yPosition);
            SetWorldPosition(rescuedPosition);
            lastTravelDirection = rescuedDirection.normalized;
            ballBody.linearVelocity = lastTravelDirection * GetTargetSpeed();
        }

        public void RecordPaddleHitRewindAnchor(PaddleController hitPaddle, float contactWorldX)
        {
            if (hitPaddle == null)
            {
                return;
            }

            lastPaddleHitRewindPosition = transform.position;
            lastPaddleHitRewindDirection = ResolvePaddleBounceDirection(hitPaddle, contactWorldX);
            hasLastPaddleHitRewindAnchor = true;
        }

        public bool TryRewindToLastPaddleHit()
        {
            if (!hasLastPaddleHitRewindAnchor || ballBody == null)
            {
                return false;
            }

            var rewindDirection = lastPaddleHitRewindDirection.sqrMagnitude > 0.001f
                ? lastPaddleHitRewindDirection.normalized
                : Vector2.up;

            attachedToPaddle = false;
            hasLaunched = true;
            SetWorldPosition(lastPaddleHitRewindPosition);
            lastTravelDirection = rewindDirection;
            ballBody.linearVelocity = rewindDirection * GetTargetSpeed();
            return true;
        }

        public void ApplySpeedBurst(float multiplier, float durationSeconds)
        {
            if (ballBody == null || !hasLaunched)
            {
                return;
            }

            speedBurstMultiplier = Mathf.Max(speedBurstMultiplier, Mathf.Max(1f, multiplier));
            speedBurstTimeRemaining = Mathf.Max(speedBurstTimeRemaining, Mathf.Max(0.1f, durationSeconds));
            ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
        }

        public void ApplyStackingSpeedBurst(
            float multiplier,
            float durationSeconds,
            float stackMultiplierIncrease,
            float maximumMultiplier,
            float stackDurationIncrease,
            float maximumDurationSeconds)
        {
            if (ballBody == null || !hasLaunched)
            {
                return;
            }

            var baseMultiplier = Mathf.Max(1f, multiplier);
            var multiplierCap = Mathf.Max(baseMultiplier, maximumMultiplier);
            var baseDuration = Mathf.Max(0.1f, durationSeconds);
            var durationCap = Mathf.Max(baseDuration, maximumDurationSeconds);

            if (speedBurstTimeRemaining > 0f && speedBurstMultiplier > 1.001f)
            {
                speedBurstMultiplier = Mathf.Min(
                    multiplierCap,
                    Mathf.Max(baseMultiplier, speedBurstMultiplier) + Mathf.Max(0f, stackMultiplierIncrease));
                speedBurstTimeRemaining = Mathf.Min(
                    durationCap,
                    Mathf.Max(baseDuration, speedBurstTimeRemaining) + Mathf.Max(0f, stackDurationIncrease));
            }
            else
            {
                speedBurstMultiplier = baseMultiplier;
                speedBurstTimeRemaining = baseDuration;
            }

            ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
        }

        public void ApplyJellySlow(float multiplier, float durationSeconds)
        {
            if (ballBody == null || !hasLaunched)
            {
                return;
            }

            jellySlowMultiplier = Mathf.Min(
                Mathf.Clamp(jellySlowMultiplier, 0.2f, 1f),
                Mathf.Clamp(multiplier, 0.2f, 1f));
            jellySlowTimeRemaining = Mathf.Max(jellySlowTimeRemaining, Mathf.Max(0.1f, durationSeconds));

            if (ballBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
            }
        }

        public void SetWorldPosition(Vector2 worldPosition)
        {
            transform.position = worldPosition;

            if (ballBody != null)
            {
                ballBody.position = worldPosition;
            }
        }

        public void Stop()
        {
            hasLaunched = false;
            attachedToPaddle = false;
            ClearSpeedBurst();
            ClearJellySlow();
            ResetHotPotato();
            ricochetCountSinceLastBrick = 0;
            ClearPaddleHitRewindAnchor();

            if (ballBody != null)
            {
                ballBody.linearVelocity = Vector2.zero;
            }
        }

        public void RegisterBrickScore()
        {
            ricochetCountSinceLastBrick = 0;
        }

        public void ApplyCollisionResponse(Vector2 direction, float minimumVerticalFraction = -1f)
        {
            if (ballBody == null)
            {
                return;
            }

            var resolvedMinimumVertical = minimumVerticalFraction < 0f
                ? minimumVerticalDirection
                : Mathf.Clamp(minimumVerticalFraction, 0f, 0.95f);
            var resolvedDirection = NormalizeDirection(direction, resolvedMinimumVertical);
            lastTravelDirection = resolvedDirection;
            ballBody.linearVelocity = resolvedDirection * GetTargetSpeed();
        }

        public Vector2 ResolvePaddleBounceDirection(PaddleController hitPaddle, float contactWorldX)
        {
            if (hitPaddle == null || hitPaddle.HalfWidthWorld <= 0.001f)
            {
                return Vector2.up;
            }

            var normalizedOffset = Mathf.Clamp(
                (contactWorldX - hitPaddle.transform.position.x) / hitPaddle.HalfWidthWorld,
                -1f,
                1f);

            return ResolvePaddleBounceDirection(normalizedOffset);
        }

        public void LaunchFromPaddleAim(float normalizedPaddleOffset, float horizontalAimMultiplier)
        {
            Launch(ResolvePaddleBounceDirection(Mathf.Clamp(normalizedPaddleOffset, -1f, 1f) * Mathf.Max(1f, horizontalAimMultiplier)));
        }

        private void FixedUpdate()
        {
            if (ballBody == null)
            {
                return;
            }

            if (!hasLaunched)
            {
                if ((!followsPaddleWhenIdle && !attachedToPaddle) || paddle == null)
                {
                    return;
                }

                var targetPosition = (Vector2)paddle.transform.position + (Vector2.up * ResolvePaddleFollowOffset());
                ballBody.MovePosition(targetPosition);
                return;
            }

            var currentVelocity = ballBody.linearVelocity;

            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                lastTravelDirection = currentVelocity.normalized;
            }

            if (currentVelocity.y < 0f && gameController != null && gameController.TryRescueBallWithShield(this))
            {
                return;
            }

            if (transform.position.y < lossThresholdY)
            {
                if (gameController != null && gameController.TryRescueBallWithTiltWarning(this))
                {
                    return;
                }

                if (gameController != null && gameController.TryRescueBallWithShield(this))
                {
                    return;
                }

                if (gameController != null && gameController.TryRescueBallWithRewindCatch(this))
                {
                    return;
                }

                Stop();
                gameController.HandleBallLost(this);
                return;
            }

            UpdateSpeedBurstTimer();
            UpdateJellySlowTimer();
            ApplyGravityWell();
            ApplyGravityPocket();
            ApplySplitHorizon();
            ApplyBrickMagnet();
            ClampBallVelocity();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!hasLaunched)
            {
                return;
            }

            RegisterHotPotatoHit();

            if (collision.collider.TryGetComponent<BreakoutShieldWallVisual>(out _)
                && gameController != null
                && gameController.TryRescueBallWithShield(this, false))
            {
                return;
            }

            if (collision.collider.TryGetComponent<BreakoutTurboRailSection>(out var turboRail))
            {
                RegisterRicochet();

                if (turboRail.TryHandleBallCollision(this))
                {
                    return;
                }
            }

            if (collision.collider.TryGetComponent<BreakoutSwitchbackRailSection>(out var switchbackRail))
            {
                RegisterRicochet();

                if (switchbackRail.TryHandleBallCollision(this))
                {
                    return;
                }
            }

            if (collision.collider.TryGetComponent<BreakoutHotCornerBumper>(out var hotCorner))
            {
                RegisterRicochet();

                if (hotCorner.TryHandleBallCollision(this))
                {
                    return;
                }
            }

            if (collision.collider.TryGetComponent<BreakoutStaticWallSection>(out var staticWall))
            {
                RegisterRicochet();

                if (staticWall.TryHandleBallCollision(this))
                {
                    return;
                }

                gameController?.HandleBallHitWall();
                gameController?.TryApplyBogusBounce(this);
                ClampBallVelocity();
                return;
            }

            if (collision.collider.TryGetComponent<PaddleController>(out var hitPaddle))
            {
                gameController?.HandleBallHitPaddle();

                if (gameController != null && gameController.TryHandleBallPaddleCollision(this, hitPaddle, collision))
                {
                    return;
                }

                RegisterRicochet();
                RedirectFromPaddle(hitPaddle, collision);
                return;
            }

            if (collision.collider.TryGetComponent<Brick>(out var solarShotBrick)
                && gameController != null
                && gameController.TryHandleSolarShot(this, solarShotBrick))
            {
                return;
            }

            if (weakBrickPierceThroughBricks
                && collision.collider.TryGetComponent<Brick>(out var weakBrick)
                && weakBrick.Definition != null
                && weakBrick.Definition.IsBreakable
                && weakBrick.HitPointsRemaining <= 1)
            {
                ContinueThroughBrickImpact();
                return;
            }

            if (phaseThroughBricks
                && collision.collider.TryGetComponent<Brick>(out var hitBrick)
                && hitBrick.Definition != null
                && hitBrick.Definition.IsBreakable)
            {
                ContinueThroughBrickImpact();
                return;
            }

            if (collision.collider.TryGetComponent<Brick>(out var collidedBrick))
            {
                if (collidedBrick.Definition == null || !collidedBrick.Definition.IsBreakable)
                {
                    RegisterRicochet();
                }
            }
            else
            {
                RegisterRicochet();
                gameController?.HandleBallHitWall();
                gameController?.TryApplyBogusBounce(this);
            }

            if (collision.collider.TryGetComponent<Brick>(out var spinningBrick)
                && spinningBrick.TryApplyBallCollisionResponse(this, collision))
            {
                return;
            }

            ClampBallVelocity();
        }

        private void RedirectFromPaddle(PaddleController hitPaddle, Collision2D collision)
        {
            if (ballBody == null)
            {
                return;
            }

            var contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            gameController?.ApplyPaddleHitTilt(hitPaddle, contactPoint.x, ballBody.linearVelocity);
            var bounceDirection = ResolvePaddleBounceDirection(hitPaddle, contactPoint.x);
            lastPaddleHitRewindPosition = transform.position;
            lastPaddleHitRewindDirection = bounceDirection;
            hasLastPaddleHitRewindAnchor = true;
            ApplyCollisionResponse(bounceDirection);
            gameController?.TryApplyCassetteSkip(this);
        }

        private Vector2 ResolvePaddleBounceDirection(float normalizedOffset)
        {
            var bounceAngleRadians = Mathf.Clamp(normalizedOffset, -1f, 1f) * maxPaddleBounceAngle * Mathf.Deg2Rad;
            var bounceDirection = new Vector2(Mathf.Sin(bounceAngleRadians), Mathf.Cos(bounceAngleRadians)).normalized;

            if (Mathf.Abs(bounceDirection.y) < minimumVerticalDirection)
            {
                bounceDirection = new Vector2(
                    bounceDirection.x,
                    Mathf.Sign(Mathf.Approximately(bounceDirection.y, 0f) ? 1f : bounceDirection.y) * minimumVerticalDirection).normalized;
            }

            return bounceDirection;
        }

        private void ClampBallVelocity(float minimumVerticalFraction = -1f)
        {
            if (ballBody == null)
            {
                return;
            }

            var velocity = ballBody.linearVelocity;
            var targetSpeed = GetTargetSpeed();

            if (velocity.sqrMagnitude < 0.01f)
            {
                velocity = Vector2.up * targetSpeed;
            }

            var resolvedMinimumVertical = minimumVerticalFraction < 0f
                ? minimumVerticalDirection
                : Mathf.Clamp(minimumVerticalFraction, 0f, 0.95f);
            var direction = NormalizeDirection(velocity, resolvedMinimumVertical);
            lastTravelDirection = direction;
            ballBody.linearVelocity = direction * targetSpeed;
        }

        private void UpdateSpeedBurstTimer()
        {
            if (speedBurstTimeRemaining <= 0f)
            {
                return;
            }

            speedBurstTimeRemaining = Mathf.Max(0f, speedBurstTimeRemaining - Time.fixedDeltaTime);

            if (speedBurstTimeRemaining > 0f)
            {
                return;
            }

            ClearSpeedBurst();
        }

        private float GetTargetSpeed()
        {
            return launchSpeed
                * Mathf.Max(1f, speedBurstMultiplier)
                * Mathf.Max(1f, hotPotatoSpeedMultiplier)
                * Mathf.Clamp(jellySlowMultiplier, 0.2f, 1f);
        }

        private float ResolvePaddleFollowOffset()
        {
            var baseRadius = Mathf.Max(baseScale.x, baseScale.y) * 0.5f;
            return paddleFollowOffset + (baseRadius * (sizeMultiplier - 1f));
        }

        public void ContinueThroughBrickImpact()
        {
            if (ballBody == null)
            {
                return;
            }

            var direction = lastTravelDirection.sqrMagnitude > 0.001f
                ? lastTravelDirection.normalized
                : Vector2.up;
            ballBody.position += direction * 0.04f;
            ballBody.linearVelocity = direction * GetTargetSpeed();
        }

        private void ApplyGravityWell()
        {
            if (ballBody == null || gravityWellStrength <= 0.001f)
            {
                return;
            }

            var pullVector = gravityWellPoint - ballBody.position;

            if (pullVector.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var currentDirection = ballBody.linearVelocity.sqrMagnitude > 0.01f
                ? ballBody.linearVelocity.normalized
                : lastTravelDirection.normalized;
            var bendFactor = gravityWellStrength * Time.fixedDeltaTime * 3.25f;
            var curvedDirection = (currentDirection + (pullVector.normalized * bendFactor)).normalized;

            if (curvedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            lastTravelDirection = curvedDirection;
            ballBody.linearVelocity = curvedDirection * GetTargetSpeed();
        }

        private void ApplyGravityPocket()
        {
            if (ballBody == null || gravityPocketStrength <= 0.001f || gravityPocketRadius <= 0.001f)
            {
                return;
            }

            var pullVector = gravityPocketPoint - ballBody.position;
            var distance = pullVector.magnitude;

            if (distance <= 0.0001f || distance > gravityPocketRadius)
            {
                return;
            }

            var currentDirection = ballBody.linearVelocity.sqrMagnitude > 0.01f
                ? ballBody.linearVelocity.normalized
                : lastTravelDirection.normalized;
            var falloff = 1f - Mathf.Clamp01(distance / gravityPocketRadius);
            var bendFactor = gravityPocketStrength * Mathf.Lerp(0.35f, 1f, falloff) * Time.fixedDeltaTime * 4.9f;
            var curvedDirection = (currentDirection + (pullVector.normalized * bendFactor)).normalized;

            if (curvedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            lastTravelDirection = curvedDirection;
            ballBody.linearVelocity = curvedDirection * GetTargetSpeed();
        }

        private void ApplySplitHorizon()
        {
            if (ballBody == null || !splitHorizonEnabled)
            {
                return;
            }

            if (splitHorizonCooldownTimer > 0f)
            {
                splitHorizonCooldownTimer = Mathf.Max(0f, splitHorizonCooldownTimer - Time.fixedDeltaTime);
            }

            var currentOffset = ballBody.position.y - splitHorizonY;
            var crossedHorizon = splitHorizonPreviousOffset < 0f && currentOffset >= 0f
                || splitHorizonPreviousOffset > 0f && currentOffset <= 0f;

            if (crossedHorizon && splitHorizonCooldownTimer <= 0f)
            {
                var crossingDirectionY = currentOffset - splitHorizonPreviousOffset;
                var refractedDirection = BuildSplitHorizonDirection(
                    ballBody.linearVelocity,
                    ballBody.position.x,
                    crossingDirectionY,
                    splitHorizonBendDegrees,
                    minimumVerticalDirection);
                lastTravelDirection = refractedDirection;
                ballBody.linearVelocity = refractedDirection * GetTargetSpeed();
                splitHorizonCooldownTimer = splitHorizonCooldownSeconds;
            }

            splitHorizonPreviousOffset = currentOffset;
        }

        private void ApplyBrickMagnet()
        {
            if (ballBody == null || Mathf.Abs(brickMagnetStrength) <= 0.001f)
            {
                return;
            }

            var pullVector = brickMagnetPoint - ballBody.position;

            if (pullVector.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var currentDirection = ballBody.linearVelocity.sqrMagnitude > 0.01f
                ? ballBody.linearVelocity.normalized
                : lastTravelDirection.normalized;
            var magnetDirection = pullVector.normalized;
            var curvedDirection = brickMagnetStrength > 0f
                ? ApplyBrickAttraction(currentDirection, magnetDirection)
                : ApplyBrickRepulsion(currentDirection, magnetDirection);

            if (curvedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            lastTravelDirection = curvedDirection;
            ballBody.linearVelocity = curvedDirection * GetTargetSpeed();
        }

        private Vector2 ApplyBrickAttraction(Vector2 currentDirection, Vector2 magnetDirection)
        {
            var bendFactor = brickMagnetStrength * Time.fixedDeltaTime * 4.4f;
            return (currentDirection + (magnetDirection * bendFactor)).normalized;
        }

        private Vector2 ApplyBrickRepulsion(Vector2 currentDirection, Vector2 magnetDirection)
        {
            var awayDirection = -magnetDirection;

            if (Vector2.Dot(currentDirection, awayDirection) < -0.92f)
            {
                var sideSign = Mathf.Sign(ballBody.position.x - brickMagnetPoint.x);

                if (Mathf.Approximately(sideSign, 0f))
                {
                    sideSign = Mathf.Sign(lastTravelDirection.x);
                }

                if (Mathf.Approximately(sideSign, 0f))
                {
                    sideSign = 1f;
                }

                awayDirection = (awayDirection + (Vector2.right * sideSign * 0.85f)).normalized;
            }

            if (currentDirection.y > 0f && awayDirection.y < 0f)
            {
                var sideSign = Mathf.Sign(awayDirection.x);

                if (Mathf.Approximately(sideSign, 0f))
                {
                    sideSign = Mathf.Sign(ballBody.position.x - brickMagnetPoint.x);
                }

                if (Mathf.Approximately(sideSign, 0f))
                {
                    sideSign = 1f;
                }

                awayDirection = new Vector2(
                    Mathf.Max(Mathf.Abs(awayDirection.x), 0.75f) * sideSign,
                    Mathf.Max(0.32f, currentDirection.y * 0.55f)).normalized;
            }

            var bendFactor = Mathf.Clamp01(Mathf.Abs(brickMagnetStrength) * Time.fixedDeltaTime * 10f);
            return Vector2.Lerp(currentDirection, awayDirection, bendFactor).normalized;
        }

        internal static Vector2 BuildSplitHorizonDirection(
            Vector2 incomingVelocity,
            float worldX,
            float crossingDirectionY,
            float bendDegrees,
            float minimumVerticalFraction)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var horizontalSign = Mathf.Sign(worldX);

            if (Mathf.Approximately(horizontalSign, 0f))
            {
                horizontalSign = Mathf.Sign(incomingDirection.x);
            }

            if (Mathf.Approximately(horizontalSign, 0f))
            {
                horizontalSign = 1f;
            }

            var verticalSign = Mathf.Sign(incomingDirection.y);

            if (Mathf.Approximately(verticalSign, 0f))
            {
                verticalSign = Mathf.Sign(crossingDirectionY);
            }

            if (Mathf.Approximately(verticalSign, 0f))
            {
                verticalSign = 1f;
            }

            var horizontalKick = Mathf.Sin(Mathf.Clamp(bendDegrees, 0f, 28f) * Mathf.Deg2Rad);
            var resolvedMinimumVertical = Mathf.Clamp(minimumVerticalFraction, 0.05f, 0.95f);
            var maximumHorizontal = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (resolvedMinimumVertical * resolvedMinimumVertical)));
            var horizontal = Mathf.Clamp(
                incomingDirection.x + (horizontalKick * horizontalSign),
                -maximumHorizontal,
                maximumHorizontal);
            var vertical = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (horizontal * horizontal))) * verticalSign;

            return new Vector2(horizontal, vertical).normalized;
        }

        private void ClearSpeedBurst()
        {
            speedBurstMultiplier = 1f;
            speedBurstTimeRemaining = 0f;
        }

        private void UpdateJellySlowTimer()
        {
            if (jellySlowTimeRemaining <= 0f)
            {
                return;
            }

            jellySlowTimeRemaining = Mathf.Max(0f, jellySlowTimeRemaining - Time.fixedDeltaTime);

            if (jellySlowTimeRemaining > 0f)
            {
                return;
            }

            ClearJellySlow();
        }

        private void ClearJellySlow()
        {
            jellySlowMultiplier = 1f;
            jellySlowTimeRemaining = 0f;
        }

        private void ResetHotPotato()
        {
            hotPotatoSpeedMultiplier = 1f;
        }

        private void ClearPaddleHitRewindAnchor()
        {
            hasLastPaddleHitRewindAnchor = false;
            lastPaddleHitRewindPosition = Vector2.zero;
            lastPaddleHitRewindDirection = Vector2.up;
        }

        private void RefreshVisualStyle()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            glowRenderer ??= GetComponent<BreakoutGlowRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            var resolvedSprite = baseVisualStyle.Sprite != null
                ? baseVisualStyle.Sprite
                : spriteRenderer.sprite;
            spriteRenderer.sprite = resolvedSprite;
            var resolvedColor = baseVisualStyle.PrimaryColor;

            if (IsExplosiveBall)
            {
                resolvedColor = Color.Lerp(resolvedColor, new Color(1f, 0.34f, 0.1f, 1f), 0.78f);
            }

            if (solarShotCharged)
            {
                resolvedColor = Color.Lerp(resolvedColor, SolarShotColor, 0.86f);
            }

            BreakoutSpriteRendererUtility.ApplyTint(spriteRenderer, resolvedColor);
            glowRenderer?.ApplyColor(resolvedColor);
        }

        private void CaptureCurrentVisualStyleAsFallback()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var currentColor = BreakoutSpriteRendererUtility.ResolveTint(spriteRenderer);
            baseVisualStyle = new ThemeVisualStyle(currentColor, currentColor, spriteRenderer.sprite);
        }

        private void RegisterHotPotatoHit()
        {
            if (ballBody == null || hotPotatoStrength <= 0.001f)
            {
                return;
            }

            var rampMultiplier = 1.065f + (hotPotatoStrength * 0.1f);
            hotPotatoSpeedMultiplier = Mathf.Min(2.25f, Mathf.Max(1f, hotPotatoSpeedMultiplier) * rampMultiplier);

            if (ballBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
            }
        }

        private void RegisterRicochet()
        {
            ricochetCountSinceLastBrick = Mathf.Max(0, ricochetCountSinceLastBrick + 1);
        }

        private Vector2 NormalizeDirection(Vector2 direction, float minimumVerticalFraction)
        {
            var resolvedDirection = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : (lastTravelDirection.sqrMagnitude > 0.001f ? lastTravelDirection.normalized : Vector2.up);

            if (minimumVerticalFraction <= 0f || Mathf.Abs(resolvedDirection.y) >= minimumVerticalFraction)
            {
                return resolvedDirection;
            }

            var ySign = Mathf.Sign(Mathf.Approximately(resolvedDirection.y, 0f) ? 1f : resolvedDirection.y);
            var adjustedY = minimumVerticalFraction * ySign;
            var adjustedX = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (adjustedY * adjustedY)));
            adjustedX *= Mathf.Approximately(resolvedDirection.x, 0f)
                ? ((gameController != null ? gameController.NextGameplayRandomBool() : Random.value < 0.5f) ? -1f : 1f)
                : Mathf.Sign(resolvedDirection.x);
            return new Vector2(adjustedX, adjustedY).normalized;
        }
    }
}
