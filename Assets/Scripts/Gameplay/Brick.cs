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
        private int maxHitPoints;
        private int hitPointsRemaining;
        private float movementSpeed;
        private Vector2 lastMovementDirection;
        private bool hasMotion;
        private bool isPendingRemoval;
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
                return;
            }

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

        private void ConfigureMotion(float motionSpeed, Vector2 motionDirection)
        {
            movementSpeed = Mathf.Max(0f, motionSpeed);
            hasMotion = movementSpeed > 0.01f && motionDirection.sqrMagnitude > 0.001f;

            if (!hasMotion)
            {
                return;
            }

            lastMovementDirection = motionDirection.normalized;
            brickBody = GetComponent<Rigidbody2D>();

            if (brickBody == null)
            {
                brickBody = gameObject.AddComponent<Rigidbody2D>();
            }

            brickBody.bodyType = RigidbodyType2D.Dynamic;
            brickBody.gravityScale = 0f;
            brickBody.freezeRotation = true;
            brickBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            brickBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            brickBody.linearDamping = 0f;
            brickBody.angularDamping = 0f;
            brickBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            brickBody.mass = 8f;
            brickBody.linearVelocity = lastMovementDirection * movementSpeed;
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
