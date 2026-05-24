using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutExplosionPulseVisual : MonoBehaviour
    {
        private const float LifetimeSeconds = 0.42f;
        private const int ShardCount = 8;

        private readonly SpriteRenderer[] shardRenderers = new SpriteRenderer[ShardCount];
        private readonly Vector3[] shardDirections = new Vector3[ShardCount];
        private SpriteRenderer ringRenderer;
        private SpriteRenderer coreRenderer;
        private float radius = 1f;
        private float timer;
        private Color coreColor = Color.white;
        private Color ringColor = Color.white;

        public void Configure(
            Sprite ringSprite,
            Sprite coreSprite,
            Material material,
            Color resolvedCoreColor,
            Color resolvedRingColor,
            float explosionRadius)
        {
            radius = Mathf.Max(0.1f, explosionRadius);
            coreColor = resolvedCoreColor;
            ringColor = resolvedRingColor;
            timer = LifetimeSeconds;

            ringRenderer = CreateRenderer("Ring", ringSprite, material, 21);
            coreRenderer = CreateRenderer("Core", coreSprite, material, 22);

            for (var index = 0; index < shardRenderers.Length; index++)
            {
                var angle = (Mathf.PI * 2f * index / shardRenderers.Length) + (Mathf.PI * 0.08f);
                shardDirections[index] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                shardRenderers[index] = CreateRenderer($"Shard {index + 1}", coreSprite, material, 23);
            }

            UpdateVisuals(0f);
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            var progress = Mathf.Clamp01(1f - (timer / LifetimeSeconds));

            if (timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            UpdateVisuals(progress);
        }

        private SpriteRenderer CreateRenderer(string objectName, Sprite sprite, Material material, int sortingOrder)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            BreakoutSpriteRendererUtility.NormalizeScale(renderer);
            return renderer;
        }

        private void UpdateVisuals(float progress)
        {
            var eased = 1f - Mathf.Pow(1f - progress, 2f);
            var fade = 1f - progress;

            if (ringRenderer != null)
            {
                ringRenderer.transform.localScale = Vector3.one * Mathf.Lerp(radius * 0.22f, radius * 2.05f, eased);
                ringRenderer.color = WithAlpha(ringColor, fade * 0.72f);
            }

            if (coreRenderer != null)
            {
                coreRenderer.transform.localScale = Vector3.one * Mathf.Lerp(radius * 0.72f, radius * 0.18f, progress);
                coreRenderer.color = WithAlpha(coreColor, fade * 0.82f);
            }

            for (var index = 0; index < shardRenderers.Length; index++)
            {
                var shard = shardRenderers[index];

                if (shard == null)
                {
                    continue;
                }

                shard.transform.localPosition = shardDirections[index] * Mathf.Lerp(radius * 0.14f, radius * 0.82f, eased);
                shard.transform.localScale = new Vector3(radius * 0.08f, radius * 0.22f, 1f) * Mathf.Lerp(1f, 0.25f, progress);
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(shardDirections[index].y, shardDirections[index].x) * Mathf.Rad2Deg);
                shard.color = WithAlpha(coreColor, fade * 0.62f);
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
