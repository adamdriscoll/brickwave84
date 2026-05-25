using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutSwitchbackRailVisual : MonoBehaviour
    {
        private const float PulseRate = 4.2f;
        private const float ImpactDuration = 0.34f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer edgeRendererA;
        private SpriteRenderer edgeRendererB;
        private SpriteRenderer arrowRendererA;
        private SpriteRenderer arrowRendererB;
        private Transform glowTransform;
        private Vector3 baseGlowScale = Vector3.one;
        private Vector3 baseLocalScale = Vector3.one;
        private float phase;
        private float switchCycleSeconds = 2.6f;
        private float phaseOffsetSeconds;
        private float impactTimer;

        public void Configure(
            Sprite squareSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            BreakoutWarpGateWall wall,
            Vector2 size,
            int railIndex,
            float cycleSeconds,
            float phaseOffset)
        {
            phase = railIndex * 1.91f;
            switchCycleSeconds = Mathf.Max(1.2f, cycleSeconds);
            phaseOffsetSeconds = Mathf.Max(0f, phaseOffset);
            baseLocalScale = transform.localScale;
            var additive = additiveMaterial != null ? additiveMaterial : unlitMaterial;
            var inwardSign = wall == BreakoutWarpGateWall.Right ? -1f : 1f;

            coreRenderer = CreateLayer(
                "Switchback Core",
                squareSprite,
                unlitMaterial,
                18,
                new Color(0.09f, 0.018f, 0.14f, 0.88f),
                size,
                Vector2.zero,
                out _);
            glowRenderer = CreateLayer(
                "Switchback Glow",
                squareSprite,
                additive,
                19,
                new Color(0.03f, 0.93f, 0.98f, 0.26f),
                new Vector2(size.x + 0.14f, size.y + 0.2f),
                Vector2.zero,
                out glowTransform);
            edgeRendererA = CreateLayer(
                "Switchback Cyan Edge",
                squareSprite,
                additive,
                21,
                new Color(0.03f, 0.93f, 0.98f, 0.78f),
                new Vector2(size.x * 1.18f, 0.055f),
                new Vector2(0f, size.y * 0.5f),
                out _);
            edgeRendererB = CreateLayer(
                "Switchback Magenta Edge",
                squareSprite,
                additive,
                21,
                new Color(1f, 0.22f, 0.84f, 0.72f),
                new Vector2(size.x * 1.18f, 0.055f),
                new Vector2(0f, size.y * -0.5f),
                out _);
            arrowRendererA = CreateLayer(
                "Switchback Arrow Up",
                squareSprite,
                additive,
                23,
                new Color(1f, 0.87f, 0.36f, 0.86f),
                new Vector2(size.x * 1.34f, 0.12f),
                new Vector2(0f, size.y * 0.22f),
                out _,
                31f * inwardSign);
            arrowRendererB = CreateLayer(
                "Switchback Arrow Down",
                squareSprite,
                additive,
                23,
                new Color(0.98f, 1f, 1f, 0.76f),
                new Vector2(size.x * 1.34f, 0.12f),
                new Vector2(0f, size.y * -0.22f),
                out _,
                -31f * inwardSign);

            baseGlowScale = glowTransform != null ? glowTransform.localScale : Vector3.one;
            UpdateVisuals(0f);
        }

        public void PlayImpact()
        {
            impactTimer = ImpactDuration;
        }

        private void Update()
        {
            if (impactTimer > 0f)
            {
                impactTimer = Mathf.Max(0f, impactTimer - Time.deltaTime);
            }

            UpdateVisuals(Time.time);
        }

        private void UpdateVisuals(float time)
        {
            var pulse = (Mathf.Sin((time * PulseRate) + phase) + 1f) * 0.5f;
            var impactRatio = impactTimer / ImpactDuration;
            var upperActive = Mathf.Repeat(time + phaseOffsetSeconds, switchCycleSeconds * 2f) < switchCycleSeconds;

            if (glowTransform != null)
            {
                glowTransform.localScale = baseGlowScale * (1f + (pulse * 0.08f) + (impactRatio * 0.22f));
            }

            transform.localScale = baseLocalScale * (1f + (impactRatio * 0.035f));
            SetAlpha(coreRenderer, Mathf.Lerp(0.56f, 0.9f, pulse) + (impactRatio * 0.1f));
            SetAlpha(glowRenderer, Mathf.Lerp(0.16f, 0.4f, pulse) + (impactRatio * 0.22f));
            SetAlpha(edgeRendererA, Mathf.Lerp(0.38f, 0.86f, upperActive ? pulse : 1f - pulse) + (impactRatio * 0.12f));
            SetAlpha(edgeRendererB, Mathf.Lerp(0.34f, 0.82f, upperActive ? 1f - pulse : pulse) + (impactRatio * 0.12f));
            SetAlpha(arrowRendererA, upperActive ? 0.92f + (impactRatio * 0.08f) : 0.22f);
            SetAlpha(arrowRendererB, upperActive ? 0.22f : 0.88f + (impactRatio * 0.1f));
        }

        private SpriteRenderer CreateLayer(
            string layerName,
            Sprite sprite,
            Material material,
            int sortingOrder,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            out Transform layerTransform,
            float localRotationDegrees = 0f)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(transform, false);
            layerTransform.localPosition = localPosition;
            layerTransform.localRotation = Quaternion.Euler(0f, 0f, localRotationDegrees);
            layerTransform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            if (renderer == null)
            {
                return;
            }

            var color = renderer.color;
            color.a = Mathf.Clamp01(alpha);
            renderer.color = color;
        }
    }
}
