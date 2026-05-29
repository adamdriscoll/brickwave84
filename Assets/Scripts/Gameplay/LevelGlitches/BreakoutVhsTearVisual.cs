using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutVhsTearVisual : MonoBehaviour
    {
        private const float PulseRate = 5.6f;
        private const float TearRate = 9.2f;
        private const float ImpactDuration = 0.32f;

        private SpriteRenderer glowRenderer;
        private SpriteRenderer coreRenderer;
        private SpriteRenderer magentaRenderer;
        private SpriteRenderer cyanRenderer;
        private SpriteRenderer whiteNoiseRenderer;
        private Transform glowTransform;
        private Transform coreTransform;
        private Transform magentaTransform;
        private Transform cyanTransform;
        private Transform whiteNoiseTransform;
        private float baseWidth;
        private float baseThickness;
        private float jitterStrength;
        private float impactTimer;
        private float impactLocalX;

        public void Configure(
            Sprite squareSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            float width,
            float thickness,
            float tearJitterStrength)
        {
            var additive = additiveMaterial != null ? additiveMaterial : unlitMaterial;
            baseWidth = Mathf.Max(1f, width);
            baseThickness = Mathf.Max(0.03f, thickness);
            jitterStrength = Mathf.Clamp(tearJitterStrength, 0.08f, 0.42f);

            glowRenderer = CreateLayer(
                "VHS Tear Glow",
                squareSprite,
                additive,
                14,
                new Color(1f, 0.22f, 0.84f, 0.18f),
                new Vector2(baseWidth, baseThickness * 5.8f),
                Vector2.zero,
                out glowTransform);
            coreRenderer = CreateLayer(
                "VHS Tear Core",
                squareSprite,
                unlitMaterial,
                18,
                new Color(0.98f, 1f, 1f, 0.62f),
                new Vector2(baseWidth, baseThickness),
                Vector2.zero,
                out coreTransform);
            magentaRenderer = CreateLayer(
                "VHS Tear Magenta Offset",
                squareSprite,
                additive,
                20,
                new Color(1f, 0.22f, 0.84f, 0.72f),
                new Vector2(baseWidth * 0.94f, baseThickness * 0.68f),
                new Vector2(0.16f, baseThickness * 1.4f),
                out magentaTransform);
            cyanRenderer = CreateLayer(
                "VHS Tear Cyan Offset",
                squareSprite,
                additive,
                20,
                new Color(0.03f, 0.93f, 0.98f, 0.64f),
                new Vector2(baseWidth * 0.9f, baseThickness * 0.62f),
                new Vector2(-0.2f, -baseThickness * 1.35f),
                out cyanTransform);
            whiteNoiseRenderer = CreateLayer(
                "VHS Tear Static Dash",
                squareSprite,
                additive,
                23,
                new Color(0.98f, 1f, 1f, 0.78f),
                new Vector2(baseWidth * 0.16f, baseThickness * 0.82f),
                Vector2.zero,
                out whiteNoiseTransform);

            UpdateVisuals(0f);
        }

        public void PlayImpact(float worldX)
        {
            impactTimer = ImpactDuration;
            impactLocalX = Mathf.Clamp(worldX - transform.position.x, baseWidth * -0.5f, baseWidth * 0.5f);
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
            var pulse = (Mathf.Sin(time * PulseRate) + 1f) * 0.5f;
            var tear = Mathf.Sin(time * TearRate) * jitterStrength;
            var impactRatio = impactTimer / ImpactDuration;
            var impactWidth = Mathf.Lerp(baseWidth * 0.16f, baseWidth * 0.36f, impactRatio);

            if (glowTransform != null)
            {
                glowTransform.localScale = new Vector3(baseWidth, baseThickness * Mathf.Lerp(3.8f, 6.6f, pulse + impactRatio), 1f);
                glowTransform.localPosition = new Vector2(tear * 0.18f, 0f);
            }

            if (coreTransform != null)
            {
                coreTransform.localPosition = new Vector2(tear * 0.08f, 0f);
            }

            if (magentaTransform != null)
            {
                magentaTransform.localPosition = new Vector2(0.18f + tear, baseThickness * Mathf.Lerp(1.1f, 1.8f, pulse));
            }

            if (cyanTransform != null)
            {
                cyanTransform.localPosition = new Vector2(-0.18f - tear * 0.7f, -baseThickness * Mathf.Lerp(1f, 1.7f, 1f - pulse));
            }

            if (whiteNoiseTransform != null)
            {
                var noiseTravel = Mathf.PingPong(time * 4.4f, 1f);
                whiteNoiseTransform.localPosition = new Vector2(
                    Mathf.Lerp(baseWidth * -0.42f, baseWidth * 0.42f, noiseTravel) + tear,
                    baseThickness * Mathf.Lerp(-2.2f, 2.2f, pulse));
                whiteNoiseTransform.localScale = new Vector3(
                    Mathf.Max(baseWidth * 0.12f, impactWidth),
                    baseThickness * Mathf.Lerp(0.6f, 1.25f, impactRatio),
                    1f);

                if (impactRatio > 0f)
                {
                    whiteNoiseTransform.localPosition = new Vector2(impactLocalX, whiteNoiseTransform.localPosition.y);
                }
            }

            SetAlpha(glowRenderer, Mathf.Lerp(0.08f, 0.24f, pulse) + impactRatio * 0.22f);
            SetAlpha(coreRenderer, Mathf.Lerp(0.38f, 0.72f, pulse) + impactRatio * 0.2f);
            SetAlpha(magentaRenderer, Mathf.Lerp(0.28f, 0.82f, pulse) + impactRatio * 0.16f);
            SetAlpha(cyanRenderer, Mathf.Lerp(0.26f, 0.76f, 1f - pulse) + impactRatio * 0.16f);
            SetAlpha(whiteNoiseRenderer, Mathf.Lerp(0.18f, 0.7f, pulse) + impactRatio * 0.28f);
        }

        private SpriteRenderer CreateLayer(
            string layerName,
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
            layerTransform.SetParent(transform, false);
            layerTransform.localPosition = localPosition;
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
