using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BallController : MonoBehaviour
    {
        [SerializeField] private float maxPaddleBounceAngle = 70f;

        private BreakoutGameController gameController;
        private PaddleController paddle;
        private Rigidbody2D ballBody;
        private float launchSpeed;
        private float minimumVerticalDirection;
        private float lossThresholdY;
        private float paddleFollowOffset;
        private float speedBurstMultiplier = 1f;
        private float speedBurstTimeRemaining;
        private bool attachedToPaddle;
        private bool followsPaddleWhenIdle;
        private bool hasLaunched;
        private bool phaseThroughBricks;
        private float gravityWellStrength;
        private Vector2 gravityWellPoint;
        private Vector2 lastTravelDirection = Vector2.up;
        private int ricochetCountSinceLastBrick;

        public float CurrentSpeed => ballBody != null ? ballBody.linearVelocity.magnitude : 0f;

        public Vector2 CurrentVelocity => ballBody != null ? ballBody.linearVelocity : Vector2.zero;

        public bool IsAttachedToPaddle => attachedToPaddle;

        public int RicochetCountSinceLastBrick => ricochetCountSinceLastBrick;

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
            ballBody = GetComponent<Rigidbody2D>();
        }

        public void ResetToPaddle()
        {
            hasLaunched = false;
            attachedToPaddle = false;
            ClearSpeedBurst();
            ricochetCountSinceLastBrick = 0;

            if (ballBody == null)
            {
                ballBody = GetComponent<Rigidbody2D>();
            }

            ballBody.linearVelocity = Vector2.zero;

            if (paddle == null)
            {
                return;
            }

            SetWorldPosition((Vector2)paddle.transform.position + (Vector2.up * paddleFollowOffset));
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
        }

        public void SetMovementSpeed(float speed)
        {
            launchSpeed = Mathf.Max(0.1f, speed);

            if (ballBody != null && hasLaunched && ballBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                ballBody.linearVelocity = ballBody.linearVelocity.normalized * GetTargetSpeed();
            }
        }

        public void SetPhaseThroughBricks(bool enabled)
        {
            phaseThroughBricks = enabled;
        }

        public void SetGravityWell(Vector2 centerPoint, float strength)
        {
            gravityWellPoint = centerPoint;
            gravityWellStrength = Mathf.Clamp01(strength);
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
            ricochetCountSinceLastBrick = 0;

            if (ballBody != null)
            {
                ballBody.linearVelocity = Vector2.zero;
            }

            if (paddle != null)
            {
                SetWorldPosition((Vector2)paddle.transform.position + (Vector2.up * paddleFollowOffset));
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
            ricochetCountSinceLastBrick = 0;

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

                var targetPosition = (Vector2)paddle.transform.position + (Vector2.up * paddleFollowOffset);
                ballBody.MovePosition(targetPosition);
                return;
            }

            var currentVelocity = ballBody.linearVelocity;

            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                lastTravelDirection = currentVelocity.normalized;
            }

            if (transform.position.y < lossThresholdY)
            {
                if (gameController != null && gameController.TryRescueBallWithShield(this))
                {
                    return;
                }

                Stop();
                gameController.HandleBallLost(this);
                return;
            }

            UpdateSpeedBurstTimer();
            ApplyGravityWell();
            ClampBallVelocity();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!hasLaunched)
            {
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

            var normalizedOffset = Mathf.Clamp(
                (contactPoint.x - hitPaddle.transform.position.x) / hitPaddle.HalfWidthWorld,
                -1f,
                1f);

            var bounceAngleRadians = normalizedOffset * maxPaddleBounceAngle * Mathf.Deg2Rad;
            var bounceDirection = new Vector2(Mathf.Sin(bounceAngleRadians), Mathf.Cos(bounceAngleRadians)).normalized;

            if (Mathf.Abs(bounceDirection.y) < minimumVerticalDirection)
            {
                bounceDirection = new Vector2(
                    bounceDirection.x,
                    Mathf.Sign(Mathf.Approximately(bounceDirection.y, 0f) ? 1f : bounceDirection.y) * minimumVerticalDirection).normalized;
            }

            ApplyCollisionResponse(bounceDirection);
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
            return launchSpeed * Mathf.Max(1f, speedBurstMultiplier);
        }

        private void ContinueThroughBrickImpact()
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

        private void ClearSpeedBurst()
        {
            speedBurstMultiplier = 1f;
            speedBurstTimeRemaining = 0f;
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
