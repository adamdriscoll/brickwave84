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
        private float fallSpeed;
        private float missThresholdY;

        public PowerUpDefinition Definition => definition;

        public void Configure(BreakoutGameController controller, PowerUpDefinition powerUpDefinition, float speed, float missY)
        {
            gameController = controller;
            definition = powerUpDefinition;
            fallSpeed = Mathf.Max(0.1f, speed);
            missThresholdY = missY;
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (TryGetComponent<Collider2D>(out var pickupCollider))
            {
                pickupCollider.isTrigger = true;
            }

            RefreshVisual();
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

        private void RefreshVisual()
        {
            if (spriteRenderer == null || definition == null)
            {
                return;
            }

            spriteRenderer.color = definition.PickupColor;
        }
    }
}
