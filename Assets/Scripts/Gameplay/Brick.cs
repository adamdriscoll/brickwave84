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
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private BrickDefinition definition;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D brickBody;
        private HingeJoint2D spinJoint;
        private int maxHitPoints;
        private int hitPointsRemaining;
        private float movementSpeed;
        private Vector2 lastMovementDirection;
        private bool hasMotion;
        private bool isPendingRemoval;
        private bool canSpin;
        private Color themedBaseColor;
        private Color themedDamagedColor;
        private float visibilityMultiplier = 1f;

        public BrickDefinition Definition => definition;

        public int ScoreValue => definition == null ? 0 : definition.ScoreValue;

        public bool CountsTowardLevelCompletion => definition != null && definition.CountsTowardLevelCompletion;

        public bool IsExplosive => definition != null && definition.IsExplosive;

        public void Initialize(
            BreakoutGameController controller,
            BrickDefinition brickDefinition,
            int effectiveHitPoints,
            ThemeVisualStyle visualStyle,
            float motionSpeed,
            Vector2 motionDirection)
        {
            gameController = controller;
            definition = brickDefinition;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            glowRenderer = GetComponentInChildren<BreakoutGlowRenderer>();
            maxHitPoints = definition != null && definition.IsBreakable
                ? Mathf.Max(1, effectiveHitPoints)
                : 0;
            hitPointsRemaining = maxHitPoints;
            ApplyTheme(visualStyle);
            ConfigureMotion(motionSpeed, motionDirection);
        }

        public void ApplyTheme(ThemeVisualStyle visualStyle)
        {
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = visualStyle.Sprite;
            NormalizeSpriteRendererScale();
            themedBaseColor = visualStyle.PrimaryColor;
            themedDamagedColor = visualStyle.SecondaryColor;
            glowRenderer?.ApplyStyle(visualStyle);
            RefreshVisual();
        }

        public void SetVisibilityMultiplier(float multiplier)
        {
            visibilityMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);
            RefreshVisual();
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

            if (!definition.IsBreakable)
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

            isPendingRemoval = true;
            hitPointsRemaining = 0;
            gameController.HandleBrickDestroyed(this, scoringBall, BrickDestructionCause.Explosion);
        }

        public void ApplyEffectHit(BallController scoringBall, BrickDestructionCause destructionCause, int damage = 1)
        {
            if (isPendingRemoval || definition == null || !definition.IsBreakable)
            {
                return;
            }

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

            resolvedColor.a *= visibilityMultiplier;
            spriteRenderer.color = resolvedColor;
            glowRenderer?.ApplyColor(resolvedColor);
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
