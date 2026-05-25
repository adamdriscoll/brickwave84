using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutPrismLaneVisual : MonoBehaviour
    {
        private const float PulseRate = 3.9f;
        private const float ScanRate = 1.45f;
        private const float ImpactDuration = 0.36f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer fieldRenderer;
        private SpriteRenderer edgeRendererA;
        private SpriteRenderer edgeRendererB;
        private SpriteRenderer slashRendererA;
        private SpriteRenderer slashRendererB;
        private SpriteRenderer scanRenderer;
        private Transform fieldTransform;
        private Transform scanTransform;
        private Vector2 baseSize;
        private Vector2 scanTravel;
        private float phase;
        private float impactTimer;

        public void Configure(
            Sprite squareSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            Vector2 size,
            int laneIndex,
            float refractionSign)
        {
            baseSize = size;
            phase = laneIndex * 1.71f;
            var additive = additiveMaterial != null ? additiveMaterial : unlitMaterial;

            fieldRenderer = CreateLayer(
                "Prism Field",
                squareSprite,
                additive,
                15,
                new Color(1f, 0.87f, 0.36f, 0.12f),
                new Vector2(size.x * 2.35f, size.y),
                Vector2.zero,
                out fieldTransform);
            coreRenderer = CreateLayer(
                "Prism Core",
                squareSprite,
                unlitMaterial,
                16,
                new Color(0.09f, 0.018f, 0.14f, 0.58f),
                size,
                Vector2.zero,
                out _);
            edgeRendererA = CreateLayer(
                "Cyan Prism Edge",
                squareSprite,
                additive,
                20,
                new Color(0.03f, 0.93f, 0.98f, 0.72f),
                new Vector2(0.035f, size.y),
                new Vector2(size.x * 0.52f, 0f),
                out _);
            edgeRendererB = CreateLayer(
                "Magenta Prism Edge",
                squareSprite,
                additive,
                20,
                new Color(1f, 0.22f, 0.84f, 0.72f),
                new Vector2(0.035f, size.y),
                new Vector2(size.x * -0.52f, 0f),
                out _);

            var slashRotation = Mathf.Sign(Mathf.Approximately(refractionSign, 0f) ? 1f : refractionSign) * 18f;
            slashRendererA = CreateLayer(
                "Gold Refraction Slash",
                squareSprite,
                additive,
                22,
                new Color(1f, 0.87f, 0.36f, 0.62f),
                new Vector2(size.x * 0.22f, size.y * 0.34f),
                new Vector2(0f, size.y * 0.19f),
                out _,
                slashRotation);
            slashRendererB = CreateLayer(
                "White Refraction Slash",
                squareSprite,
                additive,
                23,
                new Color(0.98f, 1f, 1f, 0.58f),
                new Vector2(size.x * 0.18f, size.y * 0.28f),
                new Vector2(0f, size.y * -0.22f),
                out _,
                slashRotation);

            scanTravel = new Vector2(0f, Mathf.Max(0f, (size.y * 0.5f) - 0.2f));
            scanRenderer = CreateLayer(
                "Prism Scan",
                squareSprite,
                additive,
                24,
                new Color(0.98f, 1f, 1f, 0.76f),
                new Vector2(size.x * 2.8f, 0.09f),
                -scanTravel,
                out scanTransform,
                slashRotation);

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
            var scan = Mathf.Repeat((time * ScanRate) + (phase * 0.23f), 1f);

            if (fieldTransform != null)
            {
                var fieldPulse = 1f + (pulse * 0.08f) + (impactRatio * 0.16f);
                fieldTransform.localScale = new Vector3(baseSize.x * 2.35f * fieldPulse, baseSize.y, 1f);
            }

            if (scanTransform != null)
            {
                scanTransform.localPosition = Vector2.Lerp(-scanTravel, scanTravel, scan);
            }

            SetAlpha(fieldRenderer, Mathf.Lerp(0.06f, 0.2f, pulse) + (impactRatio * 0.2f));
            SetAlpha(coreRenderer, Mathf.Lerp(0.34f, 0.66f, pulse) + (impactRatio * 0.12f));
            SetAlpha(edgeRendererA, Mathf.Lerp(0.36f, 0.9f, pulse) + (impactRatio * 0.16f));
            SetAlpha(edgeRendererB, Mathf.Lerp(0.34f, 0.86f, 1f - pulse) + (impactRatio * 0.14f));
            SetAlpha(slashRendererA, Mathf.Lerp(0.26f, 0.72f, pulse) + (impactRatio * 0.22f));
            SetAlpha(slashRendererB, Mathf.Lerp(0.22f, 0.66f, 1f - pulse) + (impactRatio * 0.22f));
            SetAlpha(scanRenderer, Mathf.Lerp(0.16f, 0.72f, pulse) + (impactRatio * 0.2f));
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
