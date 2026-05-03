using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class BreakoutShieldWallVisual : MonoBehaviour
    {
        private const int WaveSegments = 32;
        private const int ShieldSortingOrder = 12;
        private const float WaveSpeed = 8.5f;
        private const float WaveFrequency = 2.75f;
        private const float HarmonicFrequency = 6.5f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer haloRenderer;
        private LineRenderer positiveWaveRenderer;
        private LineRenderer negativeWaveRenderer;
        private Color primaryColor = new Color(0.45f, 0.95f, 0.72f, 1f);
        private Color secondaryColor = new Color(0.03f, 0.93f, 0.98f, 1f);
        private float width = 8f;
        private float height = 0.16f;
        private float phase;
        private float impactFlash;
        private int charges;
        private bool isActive;

        public void Configure(SpriteRenderer renderer, Sprite sprite, Material additiveMaterial, float visualWidth, float visualHeight)
        {
            coreRenderer = renderer != null ? renderer : GetComponent<SpriteRenderer>();
            width = Mathf.Max(0.1f, visualWidth);
            height = Mathf.Max(0.03f, visualHeight);

            if (coreRenderer != null)
            {
                coreRenderer.sprite = sprite;
                coreRenderer.sharedMaterial = additiveMaterial;
                coreRenderer.sortingOrder = ShieldSortingOrder;
            }

            glowRenderer = CreateSpriteLayer("AC Glow", sprite, additiveMaterial, ShieldSortingOrder - 1, 1.05f, 2.9f);
            haloRenderer = CreateSpriteLayer("Wide Field", sprite, additiveMaterial, ShieldSortingOrder - 2, 1.02f, 5.4f);
            positiveWaveRenderer = CreateWaveLayer("Positive Current", transform, additiveMaterial, ShieldSortingOrder + 1);
            negativeWaveRenderer = CreateWaveLayer("Negative Current", transform, additiveMaterial, ShieldSortingOrder + 2);
            SetActive(false);
        }

        public void SetState(int shieldCharges, ThemeVisualStyle style, Vector2 center, float visualWidth, float visualHeight)
        {
            charges = Mathf.Max(0, shieldCharges);
            width = Mathf.Max(0.1f, visualWidth);
            height = Mathf.Max(0.03f, visualHeight);
            transform.position = center;
            transform.localScale = new Vector3(width, height, 1f);

            primaryColor = style.PrimaryColor;
            secondaryColor = style.SecondaryColor;

            if (secondaryColor.maxColorComponent <= 0.05f)
            {
                secondaryColor = new Color(0.03f, 0.93f, 0.98f, 1f);
            }

            SetActive(charges > 0);
            RefreshSpriteLayers(0f);
            RefreshWaves();
        }

        public void SetActive(bool active)
        {
            isActive = active;
            SetRendererEnabled(coreRenderer, active);
            SetRendererEnabled(glowRenderer, active);
            SetRendererEnabled(haloRenderer, active);
            SetRendererEnabled(positiveWaveRenderer, active);
            SetRendererEnabled(negativeWaveRenderer, active);
        }

        public void PlayImpactFlash()
        {
            impactFlash = 1f;
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            phase += Time.deltaTime * WaveSpeed;
            impactFlash = Mathf.Max(0f, impactFlash - (Time.deltaTime * 4.5f));

            var pulse = 0.5f + (Mathf.Sin(phase * 1.6f) * 0.5f);
            RefreshSpriteLayers(pulse);
            RefreshWaves();
        }

        private SpriteRenderer CreateSpriteLayer(
            string layerName,
            Sprite sprite,
            Material additiveMaterial,
            int sortingOrder,
            float widthScale,
            float heightScale)
        {
            var child = new GameObject(layerName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = new Vector3(widthScale, heightScale, 1f);

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = additiveMaterial;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
        }

        private static LineRenderer CreateWaveLayer(string layerName, Transform parent, Material additiveMaterial, int sortingOrder)
        {
            var child = new GameObject(layerName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            var renderer = child.AddComponent<LineRenderer>();
            renderer.positionCount = WaveSegments + 1;
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 4;
            renderer.numCornerVertices = 2;
            renderer.startWidth = 0.035f;
            renderer.endWidth = 0.02f;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = additiveMaterial;
            renderer.enabled = false;
            return renderer;
        }

        private void RefreshSpriteLayers(float pulse)
        {
            var chargeBoost = Mathf.Clamp01((charges - 1) * 0.16f);
            var flashBoost = impactFlash * 0.28f;
            var coreAlpha = Mathf.Clamp01(0.52f + chargeBoost + (pulse * 0.1f) + flashBoost);
            var glowAlpha = Mathf.Clamp01(0.18f + (pulse * 0.11f) + (impactFlash * 0.22f));
            var haloAlpha = Mathf.Clamp01(0.08f + (pulse * 0.045f) + (impactFlash * 0.14f));

            SetSpriteColor(coreRenderer, primaryColor, coreAlpha);
            SetSpriteColor(glowRenderer, Color.Lerp(primaryColor, secondaryColor, 0.28f), glowAlpha);
            SetSpriteColor(haloRenderer, secondaryColor, haloAlpha);
        }

        private void RefreshWaves()
        {
            RefreshWave(positiveWaveRenderer, phase, primaryColor, 0.78f);
            RefreshWave(negativeWaveRenderer, phase + Mathf.PI, secondaryColor, 0.58f);
        }

        private void RefreshWave(LineRenderer renderer, float wavePhase, Color color, float baseAlpha)
        {
            if (renderer == null)
            {
                return;
            }

            var halfWidth = width * 0.5f;
            var center = transform.position;
            var amplitude = height * (0.72f + (impactFlash * 0.38f));
            var harmonicAmplitude = amplitude * 0.32f;

            for (var index = 0; index <= WaveSegments; index++)
            {
                var normalized = index / (float)WaveSegments;
                var x = Mathf.Lerp(-halfWidth, halfWidth, normalized);
                var wave = Mathf.Sin((normalized * Mathf.PI * 2f * WaveFrequency) + wavePhase) * amplitude;
                var harmonic = Mathf.Sin((normalized * Mathf.PI * 2f * HarmonicFrequency) - (wavePhase * 1.45f)) * harmonicAmplitude;
                renderer.SetPosition(index, new Vector3(center.x + x, center.y + wave + harmonic, center.z));
            }

            var alpha = Mathf.Clamp01(baseAlpha + (impactFlash * 0.2f));
            renderer.startColor = new Color(color.r, color.g, color.b, alpha);
            renderer.endColor = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha * 0.32f));
            renderer.startWidth = 0.035f + (impactFlash * 0.035f);
            renderer.endWidth = 0.02f + (impactFlash * 0.02f);
        }

        private static void SetRendererEnabled(Renderer renderer, bool enabled)
        {
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        private static void SetSpriteColor(SpriteRenderer renderer, Color color, float alpha)
        {
            if (renderer != null)
            {
                renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
            }
        }

        private void OnDestroy()
        {
            DestroyWaveRenderer(positiveWaveRenderer);
            DestroyWaveRenderer(negativeWaveRenderer);
        }

        private static void DestroyWaveRenderer(LineRenderer renderer)
        {
            if (renderer != null)
            {
                Destroy(renderer.gameObject);
            }
        }
    }
}
