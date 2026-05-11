using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutTurboRailVisual : MonoBehaviour
    {
        private const float PulseRate = 4.8f;
        private const float ScanRate = 1.65f;
        private const float ImpactDuration = 0.42f;
        private const float BorderThickness = 0.055f;
        private const float BorderOverhang = 0.18f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer railEdgeRendererA;
        private SpriteRenderer railEdgeRendererB;
        private SpriteRenderer endCapRendererA;
        private SpriteRenderer endCapRendererB;
        private SpriteRenderer stripeRendererA;
        private SpriteRenderer stripeRendererB;
        private SpriteRenderer scanRenderer;
        private Transform flexRoot;
        private Transform glowTransform;
        private Transform scanTransform;
        private Vector3 inwardNormal = Vector3.right;
        private Vector3 baseGlowScale = Vector3.one;
        private Vector3 baseScanScale = Vector3.one;
        private Vector2 scanTravel = Vector2.right;
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

            var isTopRail = wall == BreakoutWarpGateWall.Top;
            var longAxis = isTopRail ? size.x : size.y;
            var borderSize = isTopRail
                ? new Vector2(size.x + BorderOverhang, BorderThickness)
                : new Vector2(BorderThickness, size.y + BorderOverhang);
            var borderOffset = isTopRail
                ? new Vector2(0f, (size.y * 0.5f) + (BorderThickness * 0.52f))
                : new Vector2((size.x * 0.5f) + (BorderThickness * 0.52f), 0f);
            var endCapSize = isTopRail
                ? new Vector2(BorderThickness, size.y + BorderOverhang)
                : new Vector2(size.x + BorderOverhang, BorderThickness);
            var endCapOffset = isTopRail
                ? new Vector2((size.x * 0.5f) + (BorderThickness * 0.52f), 0f)
                : new Vector2(0f, (size.y * 0.5f) + (BorderThickness * 0.52f));
            var endCapRotation = isTopRail ? 18f : -18f;

            railEdgeRendererA = CreateLayer(
                "Cyan Edge A",
                flexRoot,
                squareSprite,
                additiveMaterial,
                21,
                new Color(0.05f, 0.94f, 1f, 0.84f),
                borderSize,
                borderOffset,
                out _);
            railEdgeRendererB = CreateLayer(
                "Cyan Edge B",
                flexRoot,
                squareSprite,
                additiveMaterial,
                21,
                new Color(0.05f, 0.94f, 1f, 0.74f),
                borderSize,
                -borderOffset,
                out _);
            endCapRendererA = CreateLayer(
                "Slash Cap A",
                flexRoot,
                squareSprite,
                additiveMaterial,
                22,
                new Color(1f, 0.25f, 0.86f, 0.72f),
                endCapSize,
                endCapOffset,
                out _,
                endCapRotation);
            endCapRendererB = CreateLayer(
                "Slash Cap B",
                flexRoot,
                squareSprite,
                additiveMaterial,
                22,
                new Color(1f, 0.87f, 0.36f, 0.68f),
                endCapSize,
                -endCapOffset,
                out _,
                endCapRotation);

            var stripeSize = isTopRail
                ? new Vector2(0.1f, size.y * 1.36f)
                : new Vector2(size.x * 1.36f, 0.1f);
            var stripeOffset = isTopRail
                ? new Vector2(size.x * 0.29f, 0f)
                : new Vector2(0f, size.y * 0.29f);

            stripeRendererA = CreateLayer(
                "Magenta Strike A",
                flexRoot,
                squareSprite,
                additiveMaterial,
                23,
                new Color(1f, 0.22f, 0.84f, 0.82f),
                stripeSize,
                stripeOffset,
                out _);
            stripeRendererB = CreateLayer(
                "Gold Strike B",
                flexRoot,
                squareSprite,
                additiveMaterial,
                23,
                new Color(1f, 0.87f, 0.36f, 0.74f),
                stripeSize,
                -stripeOffset,
                out _);

            var scanSize = isTopRail
                ? new Vector2(Mathf.Min(0.18f, longAxis * 0.18f), size.y * 1.58f)
                : new Vector2(size.x * 1.58f, Mathf.Min(0.18f, longAxis * 0.18f));
            scanTravel = isTopRail
                ? new Vector2(Mathf.Max(0f, (size.x * 0.5f) - 0.16f), 0f)
                : new Vector2(0f, Mathf.Max(0f, (size.y * 0.5f) - 0.16f));
            scanRenderer = CreateLayer(
                "Scan Spark",
                flexRoot,
                squareSprite,
                additiveMaterial,
                24,
                new Color(0.98f, 1f, 1f, 0.88f),
                scanSize,
                -scanTravel,
                out scanTransform);

            baseGlowScale = glowTransform != null ? glowTransform.localScale : Vector3.one;
            baseScanScale = scanTransform != null ? scanTransform.localScale : Vector3.one;
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
            var hotPulse = Mathf.SmoothStep(0f, 1f, pulse);
            var impactRatio = impactTimer / ImpactDuration;
            var impactWave = Mathf.Sin((1f - impactRatio) * Mathf.PI * 4.4f) * impactRatio;
            var flexOffset = inwardNormal * impactWave * 0.12f;
            var scan = Mathf.Repeat((time * ScanRate) + (phase * 0.19f), 1f);

            if (flexRoot != null)
            {
                flexRoot.localPosition = flexOffset;
            }

            if (glowTransform != null)
            {
                var glowPulse = 1f + (pulse * 0.08f) + (impactRatio * 0.18f);
                glowTransform.localScale = baseGlowScale * glowPulse;
            }

            if (scanTransform != null)
            {
                scanTransform.localPosition = Vector2.Lerp(-scanTravel, scanTravel, scan);
                scanTransform.localScale = baseScanScale * Mathf.Lerp(0.72f, 1.08f, hotPulse);
            }

            SetAlpha(coreRenderer, Mathf.Lerp(0.78f, 1f, hotPulse));
            SetAlpha(glowRenderer, Mathf.Lerp(0.24f, 0.58f, hotPulse) + (impactRatio * 0.28f));
            SetAlpha(railEdgeRendererA, Mathf.Lerp(0.56f, 0.96f, hotPulse) + (impactRatio * 0.14f));
            SetAlpha(railEdgeRendererB, Mathf.Lerp(0.46f, 0.82f, 1f - hotPulse) + (impactRatio * 0.1f));
            SetAlpha(endCapRendererA, Mathf.Lerp(0.5f, 0.9f, hotPulse) + (impactRatio * 0.16f));
            SetAlpha(endCapRendererB, Mathf.Lerp(0.36f, 0.76f, 1f - hotPulse) + (impactRatio * 0.14f));
            SetAlpha(stripeRendererA, Mathf.Lerp(0.46f, 0.92f, pulse) + (impactRatio * 0.12f));
            SetAlpha(stripeRendererB, Mathf.Lerp(0.36f, 0.82f, 1f - pulse) + (impactRatio * 0.14f));
            SetAlpha(scanRenderer, Mathf.Lerp(0.16f, 0.7f, hotPulse) + (impactRatio * 0.2f));
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
            out Transform layerTransform,
            float localRotationDegrees = 0f)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(parent, false);
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
