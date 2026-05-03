using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class BreakoutBrickosaurusPowerDownProjectile : MonoBehaviour
    {
        private BreakoutGameController controller;
        private Rigidbody2D projectileBody;
        private Vector2 velocity;
        private float missY;
        private bool isResolved;

        public void Configure(
            BreakoutGameController gameController,
            Sprite sprite,
            Material material,
            PhysicsMaterial2D physicsMaterial,
            Vector2 direction,
            float speed,
            float missThresholdY)
        {
            controller = gameController;
            missY = missThresholdY;
            velocity = (direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.down) * Mathf.Max(0.1f, speed);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color(1f, 0.18f, 0.32f, 1f);
            spriteRenderer.sortingOrder = 13;
            spriteRenderer.sharedMaterial = material;

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.sharedMaterial = physicsMaterial;

            projectileBody = GetComponent<Rigidbody2D>();
            projectileBody.bodyType = RigidbodyType2D.Kinematic;
            projectileBody.gravityScale = 0f;
            projectileBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            transform.localScale = Vector3.one * 0.34f;
        }

        private void FixedUpdate()
        {
            if (isResolved)
            {
                return;
            }

            var nextPosition = (Vector2)transform.position + (velocity * Time.fixedDeltaTime);
            projectileBody.MovePosition(nextPosition);
            projectileBody.MoveRotation(Mathf.Repeat(projectileBody.rotation + (520f * Time.fixedDeltaTime), 360f));

            if (nextPosition.y < missY)
            {
                ResolveWithoutHit();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isResolved || other == null || !other.TryGetComponent<PaddleController>(out _))
            {
                return;
            }

            isResolved = true;
            controller?.HandleBrickosaurusPowerDownHit(this);
        }

        public void ResolveWithoutHit()
        {
            if (isResolved)
            {
                return;
            }

            isResolved = true;
            Destroy(gameObject);
        }
    }
}
