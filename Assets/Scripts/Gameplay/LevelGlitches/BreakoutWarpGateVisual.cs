using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutWarpGateVisual : MonoBehaviour
    {
        private const float BasePulseRate = 3.15f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer innerVortexRenderer;
        private SpriteRenderer outerVortexRenderer;
        private SpriteRenderer ringRenderer;
        private SpriteRenderer haloRenderer;
        private Transform coreTransform;
        private Transform innerVortexTransform;
        private Transform outerVortexTransform;
        private Transform ringTransform;
        private Transform haloTransform;
        private Vector3 baseScale = Vector3.one;
        private Vector3 coreScale = Vector3.one;
        private Vector3 innerVortexScale = Vector3.one;
        private Vector3 outerVortexScale = Vector3.one;
        private Vector3 ringScale = Vector3.one;
        private Vector3 haloScale = Vector3.one;
        private float phase;

        public void Configure(
            Sprite coreSprite,
            Sprite vortexSprite,
            Sprite ringSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            BreakoutWarpGateWall wall,
            int portalIndex)
        {
            phase = portalIndex * 1.73f;
            baseScale = wall == BreakoutWarpGateWall.Top
                ? new Vector3(1.2f, 0.78f, 1f)
                : new Vector3(0.78f, 1.2f, 1f);

            coreRenderer = CreateLayer("Dark Core", coreSprite, unlitMaterial, 19, new Color(0.015f, 0.004f, 0.024f, 0.98f), out coreTransform);
            innerVortexRenderer = CreateLayer("Inner Vortex", vortexSprite, additiveMaterial, 20, new Color(0.03f, 0.93f, 0.98f, 0.82f), out innerVortexTransform);
            outerVortexRenderer = CreateLayer("Outer Vortex", vortexSprite, additiveMaterial, 18, new Color(1f, 0.25f, 0.86f, 0.48f), out outerVortexTransform);
            ringRenderer = CreateLayer("Event Ring", ringSprite, additiveMaterial, 21, new Color(0.98f, 1f, 1f, 0.86f), out ringTransform);
            haloRenderer = CreateLayer("Gravity Halo", ringSprite, additiveMaterial, 17, new Color(0.45f, 0.08f, 1f, 0.35f), out haloTransform);

            coreScale = Multiply(baseScale, 0.56f);
            innerVortexScale = Multiply(baseScale, 0.94f);
            outerVortexScale = Multiply(baseScale, 1.15f);
            ringScale = Multiply(baseScale, 1.03f);
            haloScale = Multiply(baseScale, 1.55f);
            UpdateVisuals(0f);
        }

        private void Update()
        {
            UpdateVisuals(Time.time);
        }

        private void UpdateVisuals(float time)
        {
            var pulse = (Mathf.Sin((time * BasePulseRate) + phase) + 1f) * 0.5f;
            var sharpPulse = Mathf.SmoothStep(0f, 1f, pulse);
            ApplyScale(coreTransform, coreScale, Mathf.Lerp(0.92f, 1.04f, sharpPulse));
            ApplyScale(innerVortexTransform, innerVortexScale, Mathf.Lerp(0.88f, 1.1f, sharpPulse));
            ApplyScale(outerVortexTransform, outerVortexScale, Mathf.Lerp(1.08f, 0.94f, sharpPulse));
            ApplyScale(ringTransform, ringScale, Mathf.Lerp(0.96f, 1.08f, sharpPulse));
            ApplyScale(haloTransform, haloScale, Mathf.Lerp(0.92f, 1.18f, sharpPulse));

            Rotate(innerVortexTransform, time * 165f);
            Rotate(outerVortexTransform, -time * 82f);
            Rotate(ringTransform, time * 34f);

            SetAlpha(innerVortexRenderer, Mathf.Lerp(0.5f, 0.9f, sharpPulse));
            SetAlpha(outerVortexRenderer, Mathf.Lerp(0.28f, 0.58f, 1f - sharpPulse));
            SetAlpha(ringRenderer, Mathf.Lerp(0.56f, 0.94f, sharpPulse));
            SetAlpha(haloRenderer, Mathf.Lerp(0.16f, 0.38f, sharpPulse));
        }

        private SpriteRenderer CreateLayer(
            string layerName,
            Sprite sprite,
            Material material,
            int sortingOrder,
            Color color,
            out Transform layerTransform)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(transform, false);
            layerTransform.localPosition = Vector3.zero;
            layerTransform.localRotation = Quaternion.identity;

            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private static Vector3 Multiply(Vector3 value, float multiplier)
        {
            return new Vector3(value.x * multiplier, value.y * multiplier, value.z);
        }

        private static void ApplyScale(Transform target, Vector3 scale, float pulse)
        {
            if (target != null)
            {
                target.localScale = Multiply(scale, pulse);
            }
        }

        private static void Rotate(Transform target, float degrees)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(0f, 0f, degrees);
            }
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
