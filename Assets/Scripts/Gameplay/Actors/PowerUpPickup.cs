using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private PowerUpDefinition definition;
        private PowerUpDefinition visualDefinition;
        private PowerUpDefinition primaryPayloadDefinition;
        private PowerUpDefinition secondaryPayloadDefinition;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D pickupBody;
        private Collider2D pickupCollider;
        private float fallSpeed;
        private float activeFallSpeedMultiplier = 1f;
        private float missThresholdY;
        private float rotationDegreesPerSecond;
        private float currentRotationDegrees;
        private Rect pinballBounds;
        private Vector2 pinballVelocity;
        private float pinballGravityMultiplier = 1f;
        private float pinballBounceDamping = 0.88f;
        private float pinballBrickCooldownTimer;
        private Brick lastPinballBrick;
        private Vector2 capsuleMagnetTarget;
        private float capsuleMagnetStrength;
        private Vector3 targetVisualScale = Vector3.one;
        private float visibilityMultiplier = 1f;
        private bool pinballEnabled;
        private bool isResolved;

        public PowerUpDefinition Definition => definition;

        public PowerUpDefinition VisualDefinition => visualDefinition != null ? visualDefinition : definition;

        public PowerUpDefinition PrimaryPayloadDefinition => primaryPayloadDefinition;

        public PowerUpDefinition SecondaryPayloadDefinition => secondaryPayloadDefinition;

        public bool UsesHelpfulVisualDisguise { get; private set; }

        public void MultiplyFallSpeed(float multiplier)
        {
            fallSpeed = Mathf.Max(0.1f, fallSpeed * Mathf.Clamp(multiplier, 0.35f, 1.5f));
        }

        public void SetActiveFallSpeedMultiplier(float multiplier)
        {
            activeFallSpeedMultiplier = Mathf.Clamp(multiplier, 0.35f, 1.5f);
        }

        public void SetCapsuleMagnetTarget(Vector2 targetPosition, float strength)
        {
            capsuleMagnetTarget = targetPosition;
            capsuleMagnetStrength = Mathf.Clamp01(strength);
        }

        internal void EnablePinball(Rect bounds, float directionSign, BreakoutPickupPinballSpec spec)
        {
            var resolvedSign = Mathf.Sign(Mathf.Approximately(directionSign, 0f) ? 1f : directionSign);
            pinballBounds = bounds;
            pinballGravityMultiplier = spec.GravityMultiplier;
            pinballBounceDamping = spec.BounceDamping;
            pinballVelocity = new Vector2(
                resolvedSign * fallSpeed * spec.LateralVelocityMultiplier,
                fallSpeed * spec.UpwardVelocityMultiplier);
            pinballBrickCooldownTimer = 0f;
            lastPinballBrick = null;
            pinballEnabled = true;
        }

        public void Configure(
            BreakoutGameController controller,
            PowerUpDefinition powerUpDefinition,
            float speed,
            float missY,
            float startingRotationDegrees,
            float spinDegreesPerSecond,
            ThemeVisualStyle visualStyle,
            PowerUpDefinition pickupVisualDefinition = null,
            bool usesHelpfulVisualDisguise = false,
            PowerUpDefinition pickupPrimaryPayloadDefinition = null,
            PowerUpDefinition pickupSecondaryPayloadDefinition = null)
        {
            gameController = controller;
            definition = powerUpDefinition;
            visualDefinition = pickupVisualDefinition != null ? pickupVisualDefinition : powerUpDefinition;
            primaryPayloadDefinition = pickupPrimaryPayloadDefinition;
            secondaryPayloadDefinition = pickupSecondaryPayloadDefinition;
            UsesHelpfulVisualDisguise = usesHelpfulVisualDisguise;
            fallSpeed = Mathf.Max(0.1f, speed);
            missThresholdY = missY;
            currentRotationDegrees = startingRotationDegrees;
            rotationDegreesPerSecond = spinDegreesPerSecond;
            targetVisualScale = transform.localScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();
            pickupBody = GetComponent<Rigidbody2D>();
            pickupCollider = GetComponent<Collider2D>();

            if (pickupBody != null)
            {
                pickupBody.bodyType = RigidbodyType2D.Kinematic;
                pickupBody.gravityScale = 0f;
                pickupBody.freezeRotation = false;
                pickupBody.interpolation = RigidbodyInterpolation2D.Interpolate;
                pickupBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                pickupBody.linearVelocity = Vector2.zero;
                pickupBody.rotation = currentRotationDegrees;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, currentRotationDegrees);
            }

            if (pickupCollider != null)
            {
                pickupCollider.isTrigger = true;
            }

            pinballEnabled = false;
            pinballVelocity = Vector2.zero;
            pinballBrickCooldownTimer = 0f;
            lastPinballBrick = null;
            ApplyTheme(visualStyle);
        }

        public void ApplyTheme(ThemeVisualStyle visualStyle)
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = visualStyle.Sprite;
            NormalizeVisualScale();
            var resolvedColor = visualStyle.PrimaryColor;
            resolvedColor.a *= visibilityMultiplier;
            spriteRenderer.color = resolvedColor;
            glowRenderer?.ApplyStyle(visualStyle);
        }

        public void ApplyRoulettePayload(
            PowerUpDefinition powerUpDefinition,
            ThemeVisualStyle visualStyle,
            PowerUpDefinition pickupVisualDefinition = null,
            bool usesHelpfulVisualDisguise = false,
            PowerUpDefinition pickupPrimaryPayloadDefinition = null,
            PowerUpDefinition pickupSecondaryPayloadDefinition = null,
            float spinDegreesPerSecond = 0f)
        {
            if (powerUpDefinition == null)
            {
                return;
            }

            definition = powerUpDefinition;
            visualDefinition = pickupVisualDefinition != null ? pickupVisualDefinition : powerUpDefinition;
            primaryPayloadDefinition = pickupPrimaryPayloadDefinition;
            secondaryPayloadDefinition = pickupSecondaryPayloadDefinition;
            UsesHelpfulVisualDisguise = usesHelpfulVisualDisguise;
            rotationDegreesPerSecond = spinDegreesPerSecond;
            gameObject.name = powerUpDefinition.DisplayName;
            ApplyTheme(visualStyle);
        }

        public void SetVisibilityMultiplier(float multiplier)
        {
            visibilityMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);

            if (spriteRenderer == null)
            {
                return;
            }

            var color = spriteRenderer.color;
            color.a = visibilityMultiplier;
            spriteRenderer.color = color;
        }

        private void FixedUpdate()
        {
            if (isResolved)
            {
                return;
            }

            var nextPosition = ResolveNextFixedPosition();

            if (pickupBody != null)
            {
                pickupBody.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }

            if (Mathf.Abs(rotationDegreesPerSecond) > 0.01f)
            {
                currentRotationDegrees = Mathf.Repeat(currentRotationDegrees + (rotationDegreesPerSecond * Time.fixedDeltaTime), 360f);

                if (pickupBody != null)
                {
                    pickupBody.MoveRotation(currentRotationDegrees);
                    transform.rotation = Quaternion.Euler(0f, 0f, currentRotationDegrees);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, currentRotationDegrees);
                }
            }

            if (IsOverlappingPaddle())
            {
                isResolved = true;
                gameController.HandlePickupCaught(this);
                return;
            }

            if (transform.position.y < missThresholdY)
            {
                isResolved = true;
                gameController.HandlePickupMissed(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCatch(other);
            TryHandlePinballBounce(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCatch(other);
            TryHandlePinballBounce(other);
        }

        private void TryCatch(Collider2D other)
        {
            if (isResolved || definition == null || other == null || !other.TryGetComponent<PaddleController>(out _))
            {
                return;
            }

            isResolved = true;
            gameController.HandlePickupCaught(this);
        }

        private void TryHandlePinballBounce(Collider2D other)
        {
            if (!pinballEnabled || isResolved || other == null || !other.TryGetComponent<Brick>(out var brick))
            {
                return;
            }

            if (brick == null || brick.Definition == null || brick.IsPendingRemoval)
            {
                return;
            }

            if (pinballBrickCooldownTimer > 0f && brick == lastPinballBrick)
            {
                return;
            }

            var normal = ResolvePinballBounceNormal((Vector2)transform.position, other.bounds);
            pinballVelocity = BuildPinballBounceVelocity(pinballVelocity, normal, fallSpeed, pinballBounceDamping);
            pinballBrickCooldownTimer = 0.08f;
            lastPinballBrick = brick;
        }

        private bool IsOverlappingPaddle()
        {
            if (isResolved || definition == null || pickupCollider == null || gameController == null)
            {
                return false;
            }

            var paddleCollider = gameController.PaddleCollider;
            return paddleCollider != null && pickupCollider.bounds.Intersects(paddleCollider.bounds);
        }

        private Vector2 ResolveFixedMovementStep()
        {
            var effectiveFallSpeed = fallSpeed * activeFallSpeedMultiplier;
            var movementStep = Vector2.down * (effectiveFallSpeed * Time.fixedDeltaTime);

            if (capsuleMagnetStrength <= 0.001f)
            {
                return movementStep;
            }

            var pullX = capsuleMagnetTarget.x - transform.position.x;

            if (Mathf.Abs(pullX) <= 0.01f)
            {
                return movementStep;
            }

            var lateralSpeed = effectiveFallSpeed * Mathf.Lerp(0.45f, 1.35f, capsuleMagnetStrength);
            var maxStep = lateralSpeed * Time.fixedDeltaTime;
            movementStep.x = Mathf.Clamp(pullX * capsuleMagnetStrength * 1.45f * Time.fixedDeltaTime, -maxStep, maxStep);
            return movementStep;
        }

        private Vector2 ResolveNextFixedPosition()
        {
            if (!pinballEnabled)
            {
                return (Vector2)transform.position + ResolveFixedMovementStep();
            }

            pinballBrickCooldownTimer = Mathf.Max(0f, pinballBrickCooldownTimer - Time.fixedDeltaTime);
            var effectiveFallSpeed = fallSpeed * activeFallSpeedMultiplier;
            var gravity = effectiveFallSpeed * pinballGravityMultiplier;
            pinballVelocity.y = Mathf.Max(
                -effectiveFallSpeed * 1.55f,
                pinballVelocity.y - (gravity * Time.fixedDeltaTime));

            if (capsuleMagnetStrength > 0.001f)
            {
                var pullX = capsuleMagnetTarget.x - transform.position.x;
                pinballVelocity.x += Mathf.Clamp(
                    pullX * capsuleMagnetStrength * 1.35f * Time.fixedDeltaTime,
                    -effectiveFallSpeed * 0.5f,
                    effectiveFallSpeed * 0.5f);
            }

            var nextPosition = (Vector2)transform.position + (pinballVelocity * Time.fixedDeltaTime);
            return ResolvePinballArenaBounce(nextPosition, effectiveFallSpeed);
        }

        private Vector2 ResolvePinballArenaBounce(Vector2 nextPosition, float effectiveFallSpeed)
        {
            var halfExtents = pickupCollider != null
                ? (Vector2)pickupCollider.bounds.extents
                : Vector2.zero;
            var minX = pinballBounds.xMin + halfExtents.x;
            var maxX = pinballBounds.xMax - halfExtents.x;
            var maxY = pinballBounds.yMax - halfExtents.y;

            if (nextPosition.x < minX)
            {
                nextPosition.x = minX;
                pinballVelocity = BuildPinballBounceVelocity(pinballVelocity, Vector2.right, effectiveFallSpeed, pinballBounceDamping);
            }
            else if (nextPosition.x > maxX)
            {
                nextPosition.x = maxX;
                pinballVelocity = BuildPinballBounceVelocity(pinballVelocity, Vector2.left, effectiveFallSpeed, pinballBounceDamping);
            }

            if (nextPosition.y > maxY)
            {
                nextPosition.y = maxY;
                pinballVelocity = BuildPinballBounceVelocity(pinballVelocity, Vector2.down, effectiveFallSpeed, pinballBounceDamping);
            }

            return nextPosition;
        }

        internal static Vector2 BuildPinballBounceVelocity(
            Vector2 incomingVelocity,
            Vector2 normal,
            float baseFallSpeed,
            float damping)
        {
            var resolvedNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector2.up;
            var reflected = Vector2.Reflect(incomingVelocity, resolvedNormal) * Mathf.Clamp(damping, 0.65f, 1f);
            var minimumHorizontalSpeed = Mathf.Max(0.12f, baseFallSpeed * 0.24f);

            if (Mathf.Abs(reflected.x) < minimumHorizontalSpeed)
            {
                var sign = Mathf.Sign(Mathf.Approximately(reflected.x, 0f) ? incomingVelocity.x : reflected.x);
                reflected.x = sign * minimumHorizontalSpeed;
            }

            return Vector2.ClampMagnitude(reflected, Mathf.Max(1f, baseFallSpeed * 1.65f));
        }

        internal static Vector2 ResolvePinballBounceNormal(Vector2 pickupPosition, Bounds obstacleBounds)
        {
            if (!obstacleBounds.Contains(pickupPosition))
            {
                var closestPoint = (Vector2)obstacleBounds.ClosestPoint(pickupPosition);
                var outward = pickupPosition - closestPoint;
                return outward.sqrMagnitude > 0.0001f ? outward.normalized : Vector2.up;
            }

            var leftDistance = Mathf.Abs(pickupPosition.x - obstacleBounds.min.x);
            var rightDistance = Mathf.Abs(obstacleBounds.max.x - pickupPosition.x);
            var bottomDistance = Mathf.Abs(pickupPosition.y - obstacleBounds.min.y);
            var topDistance = Mathf.Abs(obstacleBounds.max.y - pickupPosition.y);
            var nearest = Mathf.Min(leftDistance, rightDistance, bottomDistance, topDistance);

            if (Mathf.Approximately(nearest, leftDistance))
            {
                return Vector2.left;
            }

            if (Mathf.Approximately(nearest, rightDistance))
            {
                return Vector2.right;
            }

            return Mathf.Approximately(nearest, bottomDistance) ? Vector2.down : Vector2.up;
        }

        private void NormalizeVisualScale()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var sprite = spriteRenderer.sprite;

            if (sprite == null)
            {
                transform.localScale = targetVisualScale;
                return;
            }

            var spriteSize = sprite.bounds.size;
            var scaleX = spriteSize.x > 0.0001f ? targetVisualScale.x / spriteSize.x : targetVisualScale.x;
            var scaleY = spriteSize.y > 0.0001f ? targetVisualScale.y / spriteSize.y : targetVisualScale.y;
            transform.localScale = new Vector3(scaleX, scaleY, targetVisualScale.z);
        }

    }
}
