using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    internal sealed class BreakoutMissileProjectile : MonoBehaviour
    {
        private BreakoutGameController controller;
        private Rigidbody2D missileBody;
        private SpriteRenderer spriteRenderer;
        private float topBoundsY;
        private bool consumed;

        public void Configure(
            BreakoutGameController gameController,
            Sprite sprite,
            Material material,
            Color color,
            float speed,
            float radius,
            float maxY)
        {
            controller = gameController;
            topBoundsY = maxY;
            missileBody = GetComponent<Rigidbody2D>();
            missileBody.gravityScale = 0f;
            missileBody.bodyType = RigidbodyType2D.Dynamic;
            missileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            missileBody.linearDamping = 0f;
            missileBody.angularDamping = 0f;
            missileBody.freezeRotation = true;
            missileBody.linearVelocity = Vector2.up * Mathf.Max(0.1f, speed);

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = Mathf.Max(0.02f, radius);

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.sharedMaterial = material;
            spriteRenderer.sortingOrder = 24;
            BreakoutSpriteRendererUtility.NormalizeScale(spriteRenderer);
            BreakoutSpriteRendererUtility.ApplyTint(spriteRenderer, color);
        }

        private void Update()
        {
            if (consumed)
            {
                return;
            }

            if (transform.position.y > topBoundsY)
            {
                consumed = true;
                controller?.HandleMissileExpired(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || other == null)
            {
                return;
            }

            if (!other.TryGetComponent<Brick>(out var brick))
            {
                return;
            }

            consumed = true;
            controller?.HandleMissileHitBrick(this, brick);
        }
    }
}
