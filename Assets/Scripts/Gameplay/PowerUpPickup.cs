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
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D pickupBody;
        private Collider2D pickupCollider;
        private float fallSpeed;
        private float missThresholdY;
        private bool isResolved;

        public PowerUpDefinition Definition => definition;

        public void Configure(BreakoutGameController controller, PowerUpDefinition powerUpDefinition, float speed, float missY, ThemeVisualStyle visualStyle)
        {
            gameController = controller;
            definition = powerUpDefinition;
            fallSpeed = Mathf.Max(0.1f, speed);
            missThresholdY = missY;
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();
            pickupBody = GetComponent<Rigidbody2D>();
            pickupCollider = GetComponent<Collider2D>();

            if (pickupBody != null)
            {
                pickupBody.bodyType = RigidbodyType2D.Kinematic;
                pickupBody.gravityScale = 0f;
                pickupBody.freezeRotation = true;
                pickupBody.interpolation = RigidbodyInterpolation2D.Interpolate;
                pickupBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                pickupBody.linearVelocity = Vector2.zero;
            }

            if (pickupCollider != null)
            {
                pickupCollider.isTrigger = true;
            }

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
            spriteRenderer.color = visualStyle.PrimaryColor;
            glowRenderer?.ApplyStyle(visualStyle);
        }

        private void FixedUpdate()
        {
            if (isResolved)
            {
                return;
            }

            var nextPosition = (Vector2)transform.position + (Vector2.down * (fallSpeed * Time.fixedDeltaTime));

            if (pickupBody != null)
            {
                pickupBody.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
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
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCatch(other);
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

        private bool IsOverlappingPaddle()
        {
            if (isResolved || definition == null || pickupCollider == null || gameController == null)
            {
                return false;
            }

            var paddleCollider = gameController.PaddleCollider;
            return paddleCollider != null && pickupCollider.bounds.Intersects(paddleCollider.bounds);
        }

    }
}
