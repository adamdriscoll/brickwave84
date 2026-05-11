using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutTurboRailVisual : MonoBehaviour
    {
        private const float PulseRate = 5.4f;
        private const float ImpactDuration = 0.42f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer stripeRendererA;
        private SpriteRenderer stripeRendererB;
        private Transform flexRoot;
        private Transform glowTransform;
        private Vector3 inwardNormal = Vector3.right;
        private Vector3 baseGlowScale = Vector3.one;
        private float phase;
        private float impactTimer;

        public void Configure(
            Sprite squareSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            BreakoutWarpGateWall wall,
            Vector2 size,
            int railIndex)
        {
            phase = railIndex * 1.37f;
            inwardNormal = ResolveInwardNormal(wall);

            flexRoot = new GameObject("Flex Root").transform;
            flexRoot.SetParent(transform, false);
            flexRoot.localPosition = Vector3.zero;
            flexRoot.localRotation = Quaternion.identity;

            coreRenderer = CreateLayer(
                "Smoked Rail",
                flexRoot,
                squareSprite,
                unlitMaterial,
                18,
                new Color(0.08f, 0.015f, 0.12f, 0.96f),
                size,
                Vector2.zero,
                out _);
            glowRenderer = CreateLayer(
                "Turbo Glow",
                flexRoot,
                squareSprite,
                additiveMaterial,
                19,
                new Color(0.03f, 0.93f, 0.98f, 0.42f),
                new Vector2(size.x + 0.16f, size.y + 0.16f),
                Vector2.zero,
                out glowTransform);

            var stripeSize = wall == BreakoutWarpGateWall.Top
                ? new Vector2(0.12f, size.y * 1.18f)
                : new Vector2(size.x * 1.18f, 0.12f);
            var stripeOffset = wall == BreakoutWarpGateWall.Top
                ? new Vector2(size.x * 0.32f, 0f)
                : new Vector2(0f, size.y * 0.32f);

            stripeRendererA = CreateLayer(
                "Hot Stripe A",
                flexRoot,
                squareSprite,
                additiveMaterial,
                20,
                new Color(1f, 0.22f, 0.84f, 0.82f),
                stripeSize,
                stripeOffset,
                out _);
            stripeRendererB = CreateLayer(
                "Hot Stripe B",
                flexRoot,
                squareSprite,
                additiveMaterial,
                20,
                new Color(1f, 0.87f, 0.36f, 0.74f),
                stripeSize,
                -stripeOffset,
                out _);

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
            var impactWave = Mathf.Sin((1f - impactRatio) * Mathf.PI * 4.4f) * impactRatio;
            var flexOffset = inwardNormal * impactWave * 0.12f;

            if (flexRoot != null)
            {
                flexRoot.localPosition = flexOffset;
            }

            if (glowTransform != null)
            {
                var glowPulse = 1f + (pulse * 0.08f) + (impactRatio * 0.18f);
                glowTransform.localScale = baseGlowScale * glowPulse;
            }

            SetAlpha(coreRenderer, Mathf.Lerp(0.82f, 1f, pulse));
            SetAlpha(glowRenderer, Mathf.Lerp(0.22f, 0.5f, pulse) + (impactRatio * 0.24f));
            SetAlpha(stripeRendererA, Mathf.Lerp(0.5f, 0.9f, pulse) + (impactRatio * 0.1f));
            SetAlpha(stripeRendererB, Mathf.Lerp(0.42f, 0.78f, 1f - pulse) + (impactRatio * 0.12f));
        }

        private static SpriteRenderer CreateLayer(
            string layerName,
            Transform parent,
            Sprite sprite,
            Material material,
            int sortingOrder,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            out Transform layerTransform)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(parent, false);
            layerTransform.localPosition = localPosition;
            layerTransform.localRotation = Quaternion.identity;
            layerTransform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private static Vector3 ResolveInwardNormal(BreakoutWarpGateWall wall)
        {
            return wall switch
            {
                BreakoutWarpGateWall.Right => Vector3.left,
                BreakoutWarpGateWall.Top => Vector3.down,
                _ => Vector3.right,
            };
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
