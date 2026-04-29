using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private PowerUpDefinition definition;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private float fallSpeed;
        private float missThresholdY;

        public PowerUpDefinition Definition => definition;

        public void Configure(BreakoutGameController controller, PowerUpDefinition powerUpDefinition, float speed, float missY, ThemeVisualStyle visualStyle)
        {
            gameController = controller;
            definition = powerUpDefinition;
            fallSpeed = Mathf.Max(0.1f, speed);
            missThresholdY = missY;
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();

            if (TryGetComponent<Collider2D>(out var pickupCollider))
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

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            if (transform.position.y < missThresholdY)
            {
                gameController.HandlePickupMissed(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (definition == null || !other.TryGetComponent<PaddleController>(out _))
            {
                return;
            }

            gameController.HandlePickupCaught(this);
        }

    }
}
