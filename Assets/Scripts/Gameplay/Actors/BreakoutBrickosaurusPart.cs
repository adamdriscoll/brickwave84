using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutBrickosaurusLayer
    {
        Shield = 0,
        Scale = 1,
        Core = 2,
        Head = 3,
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class BreakoutBrickosaurusPart : MonoBehaviour
    {
        private const int SpikeCount = 3;

        private BreakoutBrickosaurusWrecksBoss boss;
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D partBody;
        private Collider2D partCollider;
        private SpriteRenderer[] spikeRenderers;
        private Sprite bodySprite;
        private Sprite headSprite;
        private Sprite spikeSprite;
        private Material spriteMaterial;
        private Vector2 targetWorldSize;
        private Color baseColor;
        private Color damagedColor;
        private int maxHits;
        private int hitsRemaining;
        private bool isDestroyed;

        public int SegmentIndex { get; private set; }

        public bool IsHead { get; private set; }

        public BreakoutBrickosaurusLayer Layer { get; private set; }

        public bool IsDestroyed => isDestroyed;

        public int HitsRemaining => hitsRemaining;

        public void Configure(
            BreakoutBrickosaurusWrecksBoss owner,
            int segmentIndex,
            bool isHeadPart,
            BreakoutBrickosaurusLayer layer,
            Sprite bodyPartSprite,
            Sprite headPartSprite,
            Sprite scaleSpikeSprite,
            Material material,
            PhysicsMaterial2D physicsMaterial,
            Vector2 worldSize)
        {
            boss = owner;
            SegmentIndex = segmentIndex;
            IsHead = isHeadPart;
            bodySprite = bodyPartSprite;
            headSprite = headPartSprite != null ? headPartSprite : bodyPartSprite;
            spikeSprite = scaleSpikeSprite;
            spriteMaterial = material;
            targetWorldSize = worldSize;
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();
            partBody = GetComponent<Rigidbody2D>();

            spriteRenderer.sharedMaterial = material;
            spriteRenderer.sortingOrder = IsHead ? 14 : 9;

            partCollider = gameObject.AddComponent<CircleCollider2D>();
            partCollider.sharedMaterial = physicsMaterial;

            partBody.bodyType = RigidbodyType2D.Kinematic;
            partBody.gravityScale = 0f;
            partBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            partBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            partBody.useFullKinematicContacts = true;

            EnsureScaleSpikes();
            RevealLayer(layer);
        }

        public void RevealLayer(BreakoutBrickosaurusLayer layer)
        {
            Layer = IsHead ? BreakoutBrickosaurusLayer.Head : layer;
            maxHits = ResolveLayerHitPoints(Layer);
            hitsRemaining = maxHits;
            isDestroyed = false;
            gameObject.SetActive(true);
            ResolveLayerColors(Layer, out baseColor, out damagedColor);
            RefreshVisual();
        }

        public bool ApplyHit()
        {
            if (isDestroyed || hitsRemaining <= 0)
            {
                return false;
            }

            hitsRemaining = Mathf.Max(0, hitsRemaining - 1);
            RefreshVisual();
            return hitsRemaining <= 0;
        }

        public void MarkDestroyed()
        {
            isDestroyed = true;
            hitsRemaining = 0;
            gameObject.SetActive(false);
        }

        public void SetWorldPose(Vector2 worldPosition, float rotationDegrees)
        {
            transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(0f, 0f, rotationDegrees));

            if (partBody == null)
            {
                return;
            }

            partBody.MovePosition(worldPosition);
            partBody.MoveRotation(rotationDegrees);
        }

        public void SetVisualFlipX(bool flipped)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.flipX = flipped;
            glowRenderer?.ApplyColor(spriteRenderer.color);
        }

        private void Update()
        {
            if (isDestroyed || spriteRenderer == null || Layer != BreakoutBrickosaurusLayer.Shield)
            {
                return;
            }

            var pulse = 0.5f + (Mathf.Sin((Time.time * 7.5f) + (SegmentIndex * 0.67f)) * 0.5f);
            var pulseColor = Color.Lerp(baseColor, Color.white, 0.18f + (pulse * 0.16f));
            spriteRenderer.color = pulseColor;
            transform.localScale = BuildSpriteScale(spriteRenderer.sprite, targetWorldSize * (1f + (pulse * 0.045f)));
            glowRenderer?.ApplyColor(pulseColor);
        }

        private void RefreshVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = IsHead ? headSprite : bodySprite;
            spriteRenderer.color = BuildCurrentColor();
            transform.localScale = BuildSpriteScale(spriteRenderer.sprite, targetWorldSize);
            SyncColliderToSpriteBounds();
            SetScaleSpikesVisible(Layer == BreakoutBrickosaurusLayer.Scale);
            glowRenderer?.Configure(
                Layer == BreakoutBrickosaurusLayer.Shield ? 1.24f : 1.13f,
                Layer == BreakoutBrickosaurusLayer.Shield ? 0.24f : 0.12f,
                Layer == BreakoutBrickosaurusLayer.Shield ? 1.44f : 1.26f,
                Layer == BreakoutBrickosaurusLayer.Shield ? 0.1f : 0.04f);
            glowRenderer?.ApplyColor(spriteRenderer.color);
        }

        private void SyncColliderToSpriteBounds()
        {
            if (partCollider == null)
            {
                return;
            }

            var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;

            if (partCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = sprite != null
                    ? sprite.bounds.size
                    : Vector2.one;
                boxCollider.offset = Vector2.zero;
                return;
            }

            if (partCollider is CircleCollider2D circleCollider)
            {
                var spriteSize = sprite != null ? (Vector2)sprite.bounds.size : Vector2.one;
                circleCollider.radius = Mathf.Min(spriteSize.x, spriteSize.y) * 0.5f;
                circleCollider.offset = Vector2.zero;
            }
        }

        private Color BuildCurrentColor()
        {
            if (maxHits <= 1)
            {
                return baseColor;
            }

            var damageProgress = 1f - Mathf.Clamp01((hitsRemaining - 1f) / (maxHits - 1f));
            var damageColor = Color.Lerp(baseColor, damagedColor, damageProgress * 0.82f);

            if (Layer == BreakoutBrickosaurusLayer.Shield)
            {
                var warningTint = new Color(1f, 0.32f, 0.9f, baseColor.a);
                return Color.Lerp(damageColor, warningTint, damageProgress * 0.34f);
            }

            if (Layer == BreakoutBrickosaurusLayer.Scale)
            {
                var exposedTint = new Color(1f, 0.72f, 0.24f, 1f);
                return Color.Lerp(damageColor, exposedTint, damageProgress * 0.28f);
            }

            return damageColor;
        }

        private void EnsureScaleSpikes()
        {
            if (spikeRenderers != null)
            {
                return;
            }

            spikeRenderers = new SpriteRenderer[SpikeCount];

            for (var index = 0; index < spikeRenderers.Length; index++)
            {
                var spikeObject = new GameObject($"Scale Spike {index + 1}");
                spikeObject.transform.SetParent(transform, false);
                var spike = spikeObject.AddComponent<SpriteRenderer>();
                spike.sprite = spikeSprite;
                spike.sharedMaterial = spriteMaterial;
                spike.sortingOrder = 10;
                spike.color = new Color(1f, 0.47f, 0.86f, 0.92f);
                spikeRenderers[index] = spike;
            }
        }

        private void SetScaleSpikesVisible(bool visible)
        {
            if (spikeRenderers == null)
            {
                return;
            }

            for (var index = 0; index < spikeRenderers.Length; index++)
            {
                var spike = spikeRenderers[index];

                if (spike == null)
                {
                    continue;
                }

                spike.enabled = visible;
                var offsetX = Mathf.Lerp(-0.32f, 0.32f, spikeRenderers.Length <= 1 ? 0.5f : index / (float)(spikeRenderers.Length - 1));
                spike.transform.localPosition = new Vector3(offsetX, 0.52f, 0f);
                spike.transform.localRotation = Quaternion.identity;
                spike.transform.localScale = new Vector3(0.19f, 0.24f, 1f);
            }
        }

        private static int ResolveLayerHitPoints(BreakoutBrickosaurusLayer layer)
        {
            return layer switch
            {
                BreakoutBrickosaurusLayer.Shield => 1,
                BreakoutBrickosaurusLayer.Scale => 1,
                BreakoutBrickosaurusLayer.Head => 3,
                _ => 1,
            };
        }

        private static void ResolveLayerColors(BreakoutBrickosaurusLayer layer, out Color primary, out Color damaged)
        {
            switch (layer)
            {
                case BreakoutBrickosaurusLayer.Shield:
                    primary = new Color(0.1f, 0.9f, 1f, 0.94f);
                    damaged = new Color(1f, 0.3f, 0.88f, 0.94f);
                    break;
                case BreakoutBrickosaurusLayer.Scale:
                    primary = new Color(0.44f, 0.2f, 0.68f, 1f);
                    damaged = new Color(0.97f, 0.5f, 0.28f, 1f);
                    break;
                case BreakoutBrickosaurusLayer.Head:
                    primary = new Color(1f, 0.14f, 0.19f, 1f);
                    damaged = new Color(1f, 0.44f, 0.28f, 1f);
                    break;
                default:
                    primary = new Color(0.28f, 1f, 0.42f, 1f);
                    damaged = new Color(0.14f, 0.54f, 0.22f, 1f);
                    break;
            }
        }

        private static Vector3 BuildSpriteScale(Sprite sprite, Vector2 worldSize)
        {
            if (sprite == null || sprite.bounds.size.x <= 0.0001f || sprite.bounds.size.y <= 0.0001f)
            {
                return new Vector3(worldSize.x, worldSize.y, 1f);
            }

            return new Vector3(worldSize.x / sprite.bounds.size.x, worldSize.y / sprite.bounds.size.y, 1f);
        }
    }
}
