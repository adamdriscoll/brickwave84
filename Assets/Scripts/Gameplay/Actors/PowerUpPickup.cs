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
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D pickupBody;
        private Collider2D pickupCollider;
        private float fallSpeed;
        private float missThresholdY;
        private float rotationDegreesPerSecond;
        private float currentRotationDegrees;
        private Vector3 targetVisualScale = Vector3.one;
        private float visibilityMultiplier = 1f;
        private bool isResolved;

        public PowerUpDefinition Definition => definition;

        public PowerUpDefinition VisualDefinition => visualDefinition != null ? visualDefinition : definition;

        public bool UsesHelpfulVisualDisguise { get; private set; }

        public void Configure(
            BreakoutGameController controller,
            PowerUpDefinition powerUpDefinition,
            float speed,
            float missY,
            float startingRotationDegrees,
            float spinDegreesPerSecond,
            ThemeVisualStyle visualStyle,
            PowerUpDefinition pickupVisualDefinition = null,
            bool usesHelpfulVisualDisguise = false)
        {
            gameController = controller;
            definition = powerUpDefinition;
            visualDefinition = pickupVisualDefinition != null ? pickupVisualDefinition : powerUpDefinition;
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

            var nextPosition = (Vector2)transform.position + (Vector2.down * (fallSpeed * Time.fixedDeltaTime));

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
