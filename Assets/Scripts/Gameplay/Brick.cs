using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    public enum BrickDestructionCause
    {
        Impact = 0,
        Explosion = 1,
    }

    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private BrickDefinition definition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D brickBody;
        private int maxHitPoints;
        private int hitPointsRemaining;
        private float movementSpeed;
        private Vector2 lastMovementDirection;
        private bool hasMotion;
        private bool isPendingRemoval;
        private Color themedBaseColor;
        private Color themedDamagedColor;

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
            spriteRenderer = GetComponent<SpriteRenderer>();
            maxHitPoints = definition != null && definition.IsBreakable
                ? Mathf.Max(1, effectiveHitPoints)
                : 0;
            hitPointsRemaining = maxHitPoints;
            ApplyTheme(visualStyle);
            ConfigureMotion(motionSpeed, motionDirection);
        }

        public void ApplyTheme(ThemeVisualStyle visualStyle)
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = visualStyle.Sprite;
            themedBaseColor = visualStyle.PrimaryColor;
            themedDamagedColor = visualStyle.SecondaryColor;
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

            hitPointsRemaining = Mathf.Max(0, hitPointsRemaining - 1);

            if (hitPointsRemaining <= 0)
            {
                isPendingRemoval = true;
                gameController.HandleBrickDestroyed(this, scoringBall, BrickDestructionCause.Impact);
                return;
            }

            RefreshVisual();
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

            if (!definition.IsBreakable || maxHitPoints <= 1)
            {
                spriteRenderer.color = themedBaseColor;
                return;
            }

            var integrity = Mathf.InverseLerp(1f, maxHitPoints, hitPointsRemaining);
            spriteRenderer.color = Color.Lerp(themedDamagedColor, themedBaseColor, integrity);
        }
    }
}
