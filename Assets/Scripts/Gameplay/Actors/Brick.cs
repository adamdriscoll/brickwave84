using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    public enum BrickDestructionCause
    {
        Impact = 0,
        Explosion = 1,
        Laser = 2,
        ChainLightning = 3,
        Missile = 4,
        FuseBurst = 5,
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private BrickDefinition definition;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private SpriteRenderer prismShuffleRenderer;
        private BoxCollider2D brickCollider;
        private Rigidbody2D brickBody;
        private HingeJoint2D spinJoint;
        private ThemeVisualStyle themedStyle;
        private ThemeVisualStyle blacklightDisguiseStyle;
        private int maxHitPoints;
        private int hitPointsRemaining;
        private float movementSpeed;
        private Vector2 lastMovementDirection;
        private Rect movementBounds;
        private bool hasMotion;
        private bool hasMovementBounds;
        private bool usesConveyorWrap;
        private float conveyorWrapPadding;
        private bool isPendingRemoval;
        private bool canSpin;
        private Color themedBaseColor;
        private Color themedDamagedColor;
        private float visibilityMultiplier = 1f;
        private float transitionVisibilityMultiplier = 1f;
        private float flickerVisibilityMultiplier = 1f;
        private float ghostVisibilityMultiplier = 1f;
        private float jammerStrength;
        private bool flickerEnabled;
        private float flickerVisibleSeconds;
        private float flickerHiddenSeconds;
        private float flickerHiddenAlpha;
        private float flickerPhaseSeconds;
        private bool isFlickerColliderVisible = true;
        private bool isGhosted;
        private bool isBlacklightDisguised;
        private bool isPrismShuffleMarked;
        private float prismShuffleRotationSign = 1f;
        private float prismShuffleRotationDegrees;
        private float prismShuffleMinimumHorizontal;
        private bool hasCompletionOverride;
        private bool countsTowardLevelCompletionOverride;
        private Vector3 visualBaseScale = Vector3.one;
        private float jellyWobbleTimer;
        private float jellyWobbleDuration;
        private float jellyWobbleDirection = 1f;
        private int layoutRow;
        private int layoutColumn;

        public BrickDefinition Definition => definition;

        public int ScoreValue => definition == null ? 0 : definition.ScoreValue;

        public bool CountsTowardLevelCompletion => definition != null
            && definition.IsBreakable
            && (hasCompletionOverride ? countsTowardLevelCompletionOverride : definition.CountsTowardLevelCompletion);

        public bool IsExplosive => definition != null && definition.IsExplosive;

        public int HitPointsRemaining => hitPointsRemaining;

        public bool IsDamaged => definition != null
            && definition.IsBreakable
            && hitPointsRemaining > 0
            && hitPointsRemaining < maxHitPoints;

        public bool IsPendingRemoval => isPendingRemoval;

        public bool IsBlacklightDisguised => isBlacklightDisguised;

        public bool IsPrismShuffleMarked => isPrismShuffleMarked;

        public void Initialize(
            BreakoutGameController controller,
            BrickDefinition brickDefinition,
            int effectiveHitPoints,
            ThemeVisualStyle visualStyle,
            float motionSpeed,
            Vector2 motionDirection,
            int row = 0,
            int column = 0)
        {
            gameController = controller;
            definition = brickDefinition;
            brickCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            glowRenderer = GetComponentInChildren<BreakoutGlowRenderer>();
            layoutRow = Mathf.Max(0, row);
            layoutColumn = Mathf.Max(0, column);
            maxHitPoints = definition != null && definition.IsBreakable
                ? Mathf.Max(1, effectiveHitPoints)
                : 0;
            hitPointsRemaining = maxHitPoints;
            ApplyTheme(visualStyle);
            ConfigureMotion(motionSpeed, motionDirection);
        }

        internal BreakoutBrickState CaptureState()
        {
            var motionConfig = hasMotion
                ? new BreakoutBrickMotionConfig(movementSpeed, lastMovementDirection)
                : default;
            return new BreakoutBrickState(
                definition,
                transform.position,
                layoutRow,
                layoutColumn,
                hitPointsRemaining,
                motionConfig,
                CountsTowardLevelCompletion);
        }

        internal void RestoreHitPoints(int restoredHitPoints)
        {
            if (definition == null || !definition.IsBreakable)
            {
                hitPointsRemaining = 0;
                RefreshVisual();
                return;
            }

            hitPointsRemaining = Mathf.Clamp(restoredHitPoints, 1, maxHitPoints);
            RefreshVisual();
        }

        internal void MirrorHorizontally(float centerX)
        {
            var position = (Vector2)transform.position;
            var mirroredPosition = new Vector2(centerX - (position.x - centerX), position.y);
            transform.position = mirroredPosition;

            if (hasMotion && lastMovementDirection.sqrMagnitude > 0.0001f)
            {
                lastMovementDirection = new Vector2(-lastMovementDirection.x, lastMovementDirection.y).normalized;
            }

            if (brickBody == null)
            {
                return;
            }

            brickBody.position = mirroredPosition;

            if (hasMotion)
            {
                brickBody.linearVelocity = lastMovementDirection * movementSpeed;
            }
            else
            {
                var velocity = brickBody.linearVelocity;
                brickBody.linearVelocity = new Vector2(-velocity.x, velocity.y);
            }

            if (canSpin)
            {
                brickBody.angularVelocity = -brickBody.angularVelocity;
            }

            if (spinJoint != null)
            {
                spinJoint.connectedAnchor = mirroredPosition;
            }

            brickBody.WakeUp();
        }

        public void ApplyTheme(ThemeVisualStyle visualStyle)
        {
            themedStyle = visualStyle;
            ApplyVisualStyle(isBlacklightDisguised ? blacklightDisguiseStyle : visualStyle);
        }

        public void ApplyBlacklightDisguise(ThemeVisualStyle disguiseStyle)
        {
            if (definition == null || !definition.IsBreakable)
            {
                return;
            }

            blacklightDisguiseStyle = disguiseStyle;
            isBlacklightDisguised = true;
            ApplyVisualStyle(blacklightDisguiseStyle);
        }

        public void ClearBlacklightDisguise()
        {
            if (!isBlacklightDisguised)
            {
                return;
            }

            isBlacklightDisguised = false;
            ApplyVisualStyle(themedStyle);
        }

        public void SetPrismShuffleMarked(float rotationSign, float rotationDegrees, float minimumHorizontal)
        {
            if (definition == null || !definition.IsBreakable)
            {
                return;
            }

            isPrismShuffleMarked = true;
            prismShuffleRotationSign = Mathf.Sign(Mathf.Approximately(rotationSign, 0f) ? 1f : rotationSign);
            prismShuffleRotationDegrees = Mathf.Clamp(rotationDegrees, 6f, 24f);
            prismShuffleMinimumHorizontal = Mathf.Clamp(minimumHorizontal, 0.32f, 0.82f);
            EnsurePrismShuffleRenderer();
            RefreshPrismShuffleVisual();
        }

        public void ClearPrismShuffleMarked()
        {
            isPrismShuffleMarked = false;
            RefreshPrismShuffleVisual();
        }

        private void ApplyVisualStyle(ThemeVisualStyle visualStyle)
        {
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = visualStyle.Sprite;
            NormalizeSpriteRendererScale();
            visualBaseScale = spriteRenderer.transform.localScale;
            themedBaseColor = visualStyle.PrimaryColor;
            themedDamagedColor = visualStyle.SecondaryColor;
            glowRenderer?.ApplyStyle(visualStyle);
            RefreshVisual();
            RefreshPrismShuffleVisual();
        }

        public void SetVisibilityMultiplier(float multiplier)
        {
            visibilityMultiplier = Mathf.Clamp01(multiplier);
            RefreshVisual();
        }

        public void SetTransitionVisibilityMultiplier(float multiplier)
        {
            transitionVisibilityMultiplier = Mathf.Clamp01(multiplier);
            RefreshVisual();
        }

        public void SetFlicker(float visibleSeconds, float hiddenSeconds, float hiddenAlpha, float phaseSeconds)
        {
            flickerVisibleSeconds = Mathf.Max(0.1f, visibleSeconds);
            flickerHiddenSeconds = Mathf.Max(0.1f, hiddenSeconds);
            flickerHiddenAlpha = Mathf.Clamp01(hiddenAlpha);
            flickerPhaseSeconds = Mathf.Max(0f, phaseSeconds);
            flickerEnabled = definition != null && definition.IsBreakable;
            UpdateFlicker(forceRefresh: true);
        }

        public void ClearFlicker()
        {
            flickerEnabled = false;
            flickerVisibilityMultiplier = 1f;
            isFlickerColliderVisible = true;
            RefreshColliderState();
            RefreshVisual();
        }

        internal void SetGhosted(bool ghosted, float hiddenAlpha = 0.12f)
        {
            isGhosted = ghosted && definition != null && definition.IsBreakable;
            ghostVisibilityMultiplier = isGhosted ? Mathf.Clamp01(hiddenAlpha) : 1f;
            RefreshColliderState();
            RefreshVisual();
        }

        internal void SetCountsTowardLevelCompletionOverride(bool countsTowardLevelCompletion)
        {
            hasCompletionOverride = true;
            countsTowardLevelCompletionOverride = countsTowardLevelCompletion;
        }

        public void SetJammerStrength(float strength)
        {
            jammerStrength = Mathf.Clamp01(strength);
        }

        public void SetMovementBounds(Rect bounds)
        {
            if (bounds.width <= 0.01f || bounds.height <= 0.01f)
            {
                hasMovementBounds = false;
                return;
            }

            movementBounds = bounds;
            hasMovementBounds = true;
        }

        internal void SetGlitchMotion(float speed, Vector2 direction)
        {
            usesConveyorWrap = false;
            conveyorWrapPadding = 0f;
            movementSpeed = Mathf.Max(0f, speed);
            hasMotion = movementSpeed > 0.01f && direction.sqrMagnitude > 0.001f;

            if (hasMotion)
            {
                lastMovementDirection = direction.normalized;
            }

            if (!hasMotion && !canSpin)
            {
                if (brickBody != null)
                {
                    brickBody.linearVelocity = Vector2.zero;
                }

                return;
            }

            EnsureDynamicBody();

            if (brickBody == null)
            {
                return;
            }

            if (spinJoint != null && hasMotion)
            {
                spinJoint.enabled = false;
            }

            brickBody.linearVelocity = hasMotion
                ? lastMovementDirection * movementSpeed
                : Vector2.zero;
            brickBody.WakeUp();
        }

        internal void SetConveyorMotion(float speed, Vector2 direction, float wrapPadding)
        {
            usesConveyorWrap = true;
            conveyorWrapPadding = Mathf.Max(0f, wrapPadding);
            SetGlitchMotion(speed, direction);
            usesConveyorWrap = hasMotion;
            conveyorWrapPadding = Mathf.Max(0f, wrapPadding);
        }

        private void FixedUpdate()
        {
            if (!hasMotion || brickBody == null)
            {
                if (!canSpin || brickBody == null)
                {
                    return;
                }
            }

            if (jammerStrength > 0.001f)
            {
                brickBody.linearVelocity = Vector2.zero;
                brickBody.angularVelocity *= 1f - jammerStrength;
                return;
            }

            if (hasMotion)
            {
                var currentVelocity = brickBody.linearVelocity;

                if (currentVelocity.sqrMagnitude > 0.0001f)
                {
                    lastMovementDirection = currentVelocity.normalized;
                }
                else if (lastMovementDirection.sqrMagnitude <= 0.0001f)
                {
                    lastMovementDirection = Vector2.right;
                }

                if (usesConveyorWrap)
                {
                    WrapMovingBrickInsideBounds();
                }
                else
                {
                    KeepMovingBrickInsideBounds();
                }

                brickBody.linearVelocity = lastMovementDirection * movementSpeed;
            }

            if (canSpin && definition != null)
            {
                brickBody.angularVelocity = Mathf.Clamp(
                    brickBody.angularVelocity,
                    -definition.SpinMaxAngularVelocity,
                    definition.SpinMaxAngularVelocity);
            }
        }

        private void Update()
        {
            UpdateFlicker(forceRefresh: false);
            UpdateJellyWobble();
            UpdatePrismShufflePulse();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isPendingRemoval)
            {
                return;
            }

            UpdateMotionDirectionFromCollision(collision);

            if (definition == null || !collision.collider.TryGetComponent<BallController>(out var scoringBall))
            {
                return;
            }

            if (definition.SpinsOnHit)
            {
                ApplyImpactSpin(collision);
            }

            RevealBlacklightDisguise();

            if (definition.JellyOnHit)
            {
                ApplyJellyImpact(scoringBall, collision);
            }

            if (!definition.IsBreakable)
            {
                gameController?.HandleBrickHit(this, scoringBall);
                return;
            }

            if (gameController != null && gameController.TryHandleSolarShot(scoringBall, this))
            {
                return;
            }

            ApplyDamage(1, scoringBall, BrickDestructionCause.Impact);
        }

        public void DestroyByExplosion(BallController scoringBall)
        {
            if (isPendingRemoval || definition == null || !definition.IsBreakable)
            {
                return;
            }

            RevealBlacklightDisguise();
            isPendingRemoval = true;
            hitPointsRemaining = 0;
            gameController.HandleBrickDestroyed(this, scoringBall, BrickDestructionCause.Explosion);
        }

        public void DestroyByMissile()
        {
            if (isPendingRemoval || definition == null || !definition.IsBreakable)
            {
                return;
            }

            RevealBlacklightDisguise();
            isPendingRemoval = true;
            hitPointsRemaining = 0;
            gameController.HandleBrickDestroyed(this, null, BrickDestructionCause.Missile);
        }

        public void ApplyEffectHit(BallController scoringBall, BrickDestructionCause destructionCause, int damage = 1)
        {
            if (isPendingRemoval || definition == null || !definition.IsBreakable)
            {
                return;
            }

            RevealBlacklightDisguise();
            ApplyDamage(Mathf.Max(1, damage), scoringBall, destructionCause);
        }

        public bool TryApplyBallCollisionResponse(BallController ball, Collision2D collision)
        {
            if (ball == null
                || collision == null
                || definition == null
                || !definition.SpinsOnHit)
            {
                return false;
            }

            var contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)ball.transform.position;
            var relativeVelocity = collision.relativeVelocity;

            if (relativeVelocity.sqrMagnitude <= 0.0001f)
            {
                relativeVelocity = ball.CurrentVelocity;
            }

            var incomingDirection = relativeVelocity.sqrMagnitude > 0.0001f
                ? relativeVelocity.normalized
                : Vector2.down;

            if (!TryGetSpinBounceDirection(contactPoint, incomingDirection, relativeVelocity.magnitude, out var bounceDirection))
            {
                return false;
            }

            ball.ApplyCollisionResponse(bounceDirection, 0.08f);
            return true;
        }

        public bool TryApplyPrismShuffleCollisionResponse(BallController ball, Collision2D collision)
        {
            if (!isPrismShuffleMarked
                || ball == null
                || collision == null
                || definition == null
                || !definition.IsBreakable)
            {
                return false;
            }

            var surfaceNormal = collision.contactCount > 0
                ? collision.GetContact(0).normal
                : ((Vector2)ball.transform.position - (Vector2)transform.position).normalized;
            var relativeVelocity = collision.relativeVelocity;

            if (relativeVelocity.sqrMagnitude <= 0.0001f)
            {
                relativeVelocity = ball.CurrentVelocity;
            }

            var shuffleDirection = BuildPrismShuffleDirection(
                relativeVelocity,
                surfaceNormal,
                prismShuffleRotationSign,
                prismShuffleRotationDegrees,
                prismShuffleMinimumHorizontal);

            if (shuffleDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            ball.ApplyCollisionResponse(shuffleDirection, 0.08f);
            PulsePrismShuffleMark();
            return true;
        }

        internal static Vector2 BuildPrismShuffleDirection(
            Vector2 incomingVelocity,
            Vector2 surfaceNormal,
            float rotationSign,
            float rotationDegrees,
            float minimumHorizontal)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.down;
            var resolvedNormal = surfaceNormal.sqrMagnitude > 0.001f
                ? surfaceNormal.normalized
                : Vector2.up;

            if (Vector2.Dot(resolvedNormal, incomingDirection) > -0.05f)
            {
                resolvedNormal = -resolvedNormal;
            }

            var reflectedDirection = Vector2.Reflect(incomingDirection, resolvedNormal).normalized;
            var sign = Mathf.Sign(Mathf.Approximately(rotationSign, 0f) ? reflectedDirection.x : rotationSign);
            var angle = Mathf.Clamp(rotationDegrees, 0f, 28f) * Mathf.Deg2Rad * sign;
            var sin = Mathf.Sin(angle);
            var cos = Mathf.Cos(angle);
            var rotatedDirection = new Vector2(
                (reflectedDirection.x * cos) - (reflectedDirection.y * sin),
                (reflectedDirection.x * sin) + (reflectedDirection.y * cos));
            var horizontalSign = Mathf.Sign(rotatedDirection.x);

            if (Mathf.Approximately(horizontalSign, 0f))
            {
                horizontalSign = sign;
            }

            var verticalSign = Mathf.Sign(rotatedDirection.y);

            if (Mathf.Approximately(verticalSign, 0f))
            {
                verticalSign = Mathf.Sign(reflectedDirection.y);
            }

            if (Mathf.Approximately(verticalSign, 0f))
            {
                verticalSign = 1f;
            }

            var horizontalMagnitude = Mathf.Clamp(
                Mathf.Max(Mathf.Abs(rotatedDirection.x), minimumHorizontal),
                0.08f,
                0.92f);
            var verticalMagnitude = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (horizontalMagnitude * horizontalMagnitude)));

            return new Vector2(horizontalMagnitude * horizontalSign, verticalMagnitude * verticalSign).normalized;
        }

        internal bool TryGetSpinBounceDirection(
            Vector2 impactPoint,
            Vector2 incomingDirection,
            float relativeSpeed,
            out Vector2 bounceDirection)
        {
            bounceDirection = Vector2.zero;

            if (definition == null || !definition.SpinsOnHit)
            {
                return false;
            }

            var resolvedIncomingDirection = incomingDirection.sqrMagnitude > 0.0001f
                ? incomingDirection.normalized
                : Vector2.down;
            var contactOffset = impactPoint - (Vector2)transform.position;

            if (contactOffset.sqrMagnitude <= 0.0001f)
            {
                contactOffset = new Vector2(resolvedIncomingDirection.x, 0.5f);
            }

            var surfaceNormal = contactOffset.normalized;

            if (Vector2.Dot(surfaceNormal, resolvedIncomingDirection) > -0.05f)
            {
                surfaceNormal = -surfaceNormal;
            }

            var reflectedDirection = Vector2.Reflect(resolvedIncomingDirection, surfaceNormal).normalized;
            var tangentDirection = new Vector2(-surfaceNormal.y, surfaceNormal.x);
            var predictedAngularVelocity = Mathf.Clamp(
                (brickBody != null ? brickBody.angularVelocity : 0f) + EstimateAngularVelocityDelta(impactPoint, resolvedIncomingDirection, relativeSpeed),
                -definition.SpinMaxAngularVelocity,
                definition.SpinMaxAngularVelocity);
            var spinFactor = definition.SpinMaxAngularVelocity > 0.01f
                ? predictedAngularVelocity / definition.SpinMaxAngularVelocity
                : 0f;
            var motionInfluence = brickBody != null ? brickBody.linearVelocity * 0.045f : Vector2.zero;
            var biasedDirection = (
                reflectedDirection
                + (tangentDirection * spinFactor * definition.SpinBounceStrength)
                + motionInfluence).normalized;

            if (biasedDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            bounceDirection = biasedDirection;
            return true;
        }

        internal void RegisterImpactSpin(Vector2 impactPoint, Vector2 incomingDirection, float impactSpeed)
        {
            if (definition == null || !definition.SpinsOnHit)
            {
                return;
            }

            EnsureDynamicBody();

            if (brickBody == null)
            {
                return;
            }

            brickBody.angularVelocity += EstimateAngularVelocityDelta(impactPoint, incomingDirection, impactSpeed);
            brickBody.angularVelocity = Mathf.Clamp(
                brickBody.angularVelocity,
                -definition.SpinMaxAngularVelocity,
                definition.SpinMaxAngularVelocity);
            brickBody.WakeUp();
        }

        private void ConfigureMotion(float motionSpeed, Vector2 motionDirection)
        {
            canSpin = definition != null && definition.SpinsOnHit;
            movementSpeed = Mathf.Max(0f, motionSpeed);
            hasMotion = movementSpeed > 0.01f && motionDirection.sqrMagnitude > 0.001f;

            if (!hasMotion && !canSpin)
            {
                return;
            }

            if (hasMotion)
            {
                lastMovementDirection = motionDirection.normalized;
            }

            EnsureDynamicBody();

            if (brickBody == null)
            {
                return;
            }

            brickBody.linearVelocity = hasMotion
                ? lastMovementDirection * movementSpeed
                : Vector2.zero;
        }

        private void KeepMovingBrickInsideBounds()
        {
            if (!hasMovementBounds || brickBody == null || lastMovementDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var halfSize = new Vector2(
                Mathf.Abs(transform.lossyScale.x) * 0.5f,
                Mathf.Abs(transform.lossyScale.y) * 0.5f);
            var minX = movementBounds.xMin + halfSize.x;
            var maxX = movementBounds.xMax - halfSize.x;
            var minY = movementBounds.yMin + halfSize.y;
            var maxY = movementBounds.yMax - halfSize.y;

            if (minX > maxX || minY > maxY)
            {
                return;
            }

            var position = brickBody.position;
            var clampedPosition = new Vector2(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY));
            var projectedPosition = clampedPosition + (lastMovementDirection.normalized * movementSpeed * Time.fixedDeltaTime);
            var direction = lastMovementDirection;

            if ((projectedPosition.x <= minX && direction.x < 0f) || (projectedPosition.x >= maxX && direction.x > 0f))
            {
                direction.x = -direction.x;
            }

            if ((projectedPosition.y <= minY && direction.y < 0f) || (projectedPosition.y >= maxY && direction.y > 0f))
            {
                direction.y = -direction.y;
            }

            if ((clampedPosition - position).sqrMagnitude > 0.000001f)
            {
                brickBody.position = clampedPosition;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                lastMovementDirection = direction.normalized;
            }
        }

        private void UpdateMotionDirectionFromCollision(Collision2D collision)
        {
            if (!hasMotion || collision == null)
            {
                return;
            }

            if (brickBody != null && brickBody.linearVelocity.sqrMagnitude > 0.0001f)
            {
                lastMovementDirection = brickBody.linearVelocity.normalized;
                return;
            }

            if (collision.contactCount <= 0 || lastMovementDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            lastMovementDirection = Vector2.Reflect(lastMovementDirection, collision.GetContact(0).normal).normalized;
        }

        private void RefreshVisual()
        {
            if (definition == null || spriteRenderer == null)
            {
                return;
            }

            Color resolvedColor;

            if (!definition.IsBreakable || maxHitPoints <= 1)
            {
                resolvedColor = themedBaseColor;
            }
            else
            {
                var integrity = Mathf.InverseLerp(1f, maxHitPoints, hitPointsRemaining);
                resolvedColor = Color.Lerp(themedDamagedColor, themedBaseColor, integrity);
            }

            resolvedColor.a *= visibilityMultiplier * transitionVisibilityMultiplier * flickerVisibilityMultiplier * ghostVisibilityMultiplier;
            spriteRenderer.color = resolvedColor;
            glowRenderer?.ApplyColor(resolvedColor);
            RefreshPrismShuffleVisual();
        }

        private void EnsurePrismShuffleRenderer()
        {
            if (prismShuffleRenderer != null || spriteRenderer == null)
            {
                return;
            }

            var markObject = new GameObject("Prism Shuffle Mark");
            markObject.transform.SetParent(spriteRenderer.transform, false);
            markObject.transform.localPosition = Vector3.zero;
            markObject.transform.localRotation = Quaternion.identity;
            markObject.transform.localScale = Vector3.one * 1.08f;
            prismShuffleRenderer = markObject.AddComponent<SpriteRenderer>();
            prismShuffleRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
            prismShuffleRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            prismShuffleRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        private void RefreshPrismShuffleVisual()
        {
            if (!isPrismShuffleMarked || spriteRenderer == null)
            {
                if (prismShuffleRenderer != null)
                {
                    prismShuffleRenderer.enabled = false;
                }

                return;
            }

            EnsurePrismShuffleRenderer();

            if (prismShuffleRenderer == null)
            {
                return;
            }

            prismShuffleRenderer.enabled = true;
            prismShuffleRenderer.sprite = spriteRenderer.sprite;
            prismShuffleRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            prismShuffleRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            var tint = prismShuffleRotationSign > 0f
                ? new Color(0.03f, 0.93f, 0.98f, 0.44f)
                : new Color(1f, 0.22f, 0.84f, 0.44f);
            tint.a *= visibilityMultiplier * transitionVisibilityMultiplier * flickerVisibilityMultiplier * ghostVisibilityMultiplier;
            prismShuffleRenderer.color = tint;
        }

        private void PulsePrismShuffleMark()
        {
            if (prismShuffleRenderer == null)
            {
                return;
            }

            prismShuffleRenderer.transform.localScale = Vector3.one * 1.18f;
        }

        private void UpdatePrismShufflePulse()
        {
            if (prismShuffleRenderer == null || !prismShuffleRenderer.enabled)
            {
                return;
            }

            prismShuffleRenderer.transform.localScale = Vector3.Lerp(
                prismShuffleRenderer.transform.localScale,
                Vector3.one * 1.08f,
                Mathf.Clamp01(Time.deltaTime * 8f));
        }

        private void RevealBlacklightDisguise()
        {
            if (isBlacklightDisguised)
            {
                ClearBlacklightDisguise();
            }
        }

        private void UpdateFlicker(bool forceRefresh)
        {
            if (!flickerEnabled)
            {
                return;
            }

            brickCollider ??= GetComponent<BoxCollider2D>();

            var cycleSeconds = flickerVisibleSeconds + flickerHiddenSeconds;
            if (cycleSeconds <= 0.001f)
            {
                return;
            }

            var cyclePosition = Mathf.Repeat(Time.time + flickerPhaseSeconds, cycleSeconds);
            var isVisible = cyclePosition < flickerVisibleSeconds;
            var resolvedVisibility = isVisible ? 1f : flickerHiddenAlpha;

            if (!forceRefresh
                && Mathf.Approximately(resolvedVisibility, flickerVisibilityMultiplier)
                && isVisible == isFlickerColliderVisible)
            {
                return;
            }

            flickerVisibilityMultiplier = resolvedVisibility;
            isFlickerColliderVisible = isVisible;
            RefreshColliderState();
            RefreshVisual();
        }

        private void RefreshColliderState()
        {
            brickCollider ??= GetComponent<BoxCollider2D>();

            if (brickCollider != null)
            {
                brickCollider.enabled = !isGhosted && isFlickerColliderVisible;
            }
        }

        private void WrapMovingBrickInsideBounds()
        {
            if (!hasMovementBounds || brickBody == null || lastMovementDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var halfSize = new Vector2(
                Mathf.Abs(transform.lossyScale.x) * 0.5f,
                Mathf.Abs(transform.lossyScale.y) * 0.5f);
            var minX = movementBounds.xMin + halfSize.x;
            var maxX = movementBounds.xMax - halfSize.x;
            var minY = movementBounds.yMin + halfSize.y;
            var maxY = movementBounds.yMax - halfSize.y;

            if (minX > maxX || minY > maxY)
            {
                return;
            }

            var position = brickBody.position;
            var wrappedPosition = new Vector2(
                position.x,
                Mathf.Clamp(position.y, minY, maxY));
            var padding = conveyorWrapPadding;

            if (lastMovementDirection.x > 0f && position.x > maxX + padding)
            {
                wrappedPosition.x = minX - padding;
            }
            else if (lastMovementDirection.x < 0f && position.x < minX - padding)
            {
                wrappedPosition.x = maxX + padding;
            }

            if ((wrappedPosition - position).sqrMagnitude > 0.000001f)
            {
                brickBody.position = wrappedPosition;
            }
        }

        private void ApplyDamage(int damage, BallController scoringBall, BrickDestructionCause destructionCause)
        {
            if (definition == null || !definition.IsBreakable)
            {
                return;
            }

            hitPointsRemaining = Mathf.Max(0, hitPointsRemaining - Mathf.Max(1, damage));

            if (hitPointsRemaining <= 0)
            {
                isPendingRemoval = true;
                gameController.HandleBrickDestroyed(this, scoringBall, destructionCause);
                return;
            }

            gameController?.HandleBrickHit(this, scoringBall);
            RefreshVisual();
        }

        private void EnsureDynamicBody()
        {
            brickBody = GetComponent<Rigidbody2D>();

            if (brickBody == null)
            {
                brickBody = gameObject.AddComponent<Rigidbody2D>();
            }

            brickBody.bodyType = RigidbodyType2D.Dynamic;
            brickBody.gravityScale = 0f;
            brickBody.freezeRotation = !canSpin;
            brickBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            brickBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            brickBody.linearDamping = hasMotion ? 0f : 0.35f;
            brickBody.angularDamping = canSpin && definition != null ? definition.SpinAngularDamping : 0f;
            brickBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            brickBody.mass = canSpin ? 10f : 8f;

            if (canSpin)
            {
                ConfigureSpinJoint();
            }
        }

        private void ConfigureSpinJoint()
        {
            spinJoint = GetComponent<HingeJoint2D>();

            if (spinJoint == null)
            {
                spinJoint = gameObject.AddComponent<HingeJoint2D>();
            }

            spinJoint.autoConfigureConnectedAnchor = false;
            spinJoint.anchor = Vector2.zero;
            spinJoint.connectedBody = null;
            spinJoint.connectedAnchor = transform.position;
            spinJoint.useLimits = false;
            spinJoint.useMotor = false;
        }

        private void ApplyImpactSpin(Collision2D collision)
        {
            if (collision == null)
            {
                return;
            }

            var impactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;
            var impactVelocity = collision.relativeVelocity;

            if (impactVelocity.sqrMagnitude <= 0.0001f)
            {
                impactVelocity = collision.collider.attachedRigidbody != null
                    ? collision.collider.attachedRigidbody.linearVelocity
                    : Vector2.down;
            }

            RegisterImpactSpin(impactPoint, impactVelocity.normalized, impactVelocity.magnitude);
        }

        private void ApplyJellyImpact(BallController scoringBall, Collision2D collision)
        {
            if (definition == null || !definition.JellyOnHit)
            {
                return;
            }

            scoringBall?.ApplyJellySlow(definition.JellyBallSpeedMultiplier, definition.JellySlowDuration);

            var contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;
            jellyWobbleDirection = contactPoint.x >= transform.position.x ? -1f : 1f;
            jellyWobbleDuration = Mathf.Max(0.1f, definition.JellySlowDuration * 0.55f);
            jellyWobbleTimer = jellyWobbleDuration;
        }

        private void UpdateJellyWobble()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (jellyWobbleTimer <= 0f)
            {
                spriteRenderer.transform.localScale = visualBaseScale;
                spriteRenderer.transform.localRotation = Quaternion.identity;
                return;
            }

            jellyWobbleTimer = Mathf.Max(0f, jellyWobbleTimer - Time.deltaTime);
            var duration = Mathf.Max(0.1f, jellyWobbleDuration);
            var remainingRatio = Mathf.Clamp01(jellyWobbleTimer / duration);
            var phase = (1f - remainingRatio) * Mathf.PI * 6f;
            var wobble = Mathf.Sin(phase) * remainingRatio * (definition?.JellyWobbleStrength ?? 0f);
            spriteRenderer.transform.localScale = new Vector3(
                visualBaseScale.x * (1f + (wobble * 0.9f)),
                visualBaseScale.y * (1f - (wobble * 0.65f)),
                visualBaseScale.z);
            spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, jellyWobbleDirection * wobble * 8f);
        }

        private float EstimateAngularVelocityDelta(Vector2 impactPoint, Vector2 incomingDirection, float impactSpeed)
        {
            if (definition == null || !definition.SpinsOnHit)
            {
                return 0f;
            }

            var contactOffset = impactPoint - (Vector2)transform.position;

            if (contactOffset.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            var resolvedIncomingDirection = incomingDirection.sqrMagnitude > 0.0001f
                ? incomingDirection.normalized
                : Vector2.down;
            var tangentialDirection = new Vector2(-contactOffset.y, contactOffset.x).normalized;
            var signedSpin = Vector2.Dot(resolvedIncomingDirection, tangentialDirection);
            var spinDirection = Mathf.Abs(signedSpin) > 0.0001f
                ? Mathf.Sign(signedSpin)
                : Mathf.Sign(resolvedIncomingDirection.x);
            var speedFactor = Mathf.Clamp(impactSpeed / 7.5f, 0.55f, 1.45f);
            return definition.SpinTorqueImpulse * spinDirection * speedFactor;
        }

        private void NormalizeSpriteRendererScale()
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
    }
}
