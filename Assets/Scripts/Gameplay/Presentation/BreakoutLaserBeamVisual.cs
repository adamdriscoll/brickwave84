using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    internal sealed class BreakoutLaserBeamVisual : MonoBehaviour
    {
        private const float MinimumLifetimeSeconds = 0.03f;
        private const int LaserSortingOrder = 18;
        private const float CoreStartWidth = 0.055f;
        private const float CoreEndWidth = 0.022f;
        private const float GlowStartWidth = 0.19f;
        private const float GlowEndWidth = 0.06f;
        private const float MuzzleStartWidth = 0.26f;
        private const float MuzzleEndWidth = 0.08f;

        private LineRenderer coreRenderer;
        private LineRenderer glowRenderer;
        private LineRenderer muzzleRenderer;
        private Color coreColor;
        private Color glowColor;
        private Color muzzleColor;
        private float lifetimeSeconds = 0.14f;
        private float ageSeconds;

        public void Configure(
            Vector2 origin,
            Vector2 target,
            Material material,
            Color core,
            Color glow,
            Color muzzle,
            float lifetime)
        {
            lifetimeSeconds = Mathf.Max(MinimumLifetimeSeconds, lifetime);
            coreColor = core;
            glowColor = glow;
            muzzleColor = muzzle;

            coreRenderer = GetComponent<LineRenderer>();
            glowRenderer = CreateChildRenderer("Glow");
            muzzleRenderer = CreateChildRenderer("Muzzle Flash");

            ConfigureRenderer(glowRenderer, material, GlowStartWidth, GlowEndWidth, LaserSortingOrder - 1);
            ConfigureRenderer(coreRenderer, material, CoreStartWidth, CoreEndWidth, LaserSortingOrder);
            ConfigureRenderer(muzzleRenderer, material, MuzzleStartWidth, MuzzleEndWidth, LaserSortingOrder + 1);

            SetBeamPositions(glowRenderer, origin, target);
            SetBeamPositions(coreRenderer, origin, target);
            SetBeamPositions(muzzleRenderer, origin, Vector2.Lerp(origin, target, 0.08f));
            RefreshAlpha(1f);
        }

        private void Update()
        {
            ageSeconds += Time.deltaTime;
            var remaining = 1f - Mathf.Clamp01(ageSeconds / lifetimeSeconds);
            RefreshAlpha(remaining * remaining);

            if (ageSeconds >= lifetimeSeconds)
            {
                Destroy(gameObject);
            }
        }

        private LineRenderer CreateChildRenderer(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.AddComponent<LineRenderer>();
        }

        private static void ConfigureRenderer(
            LineRenderer renderer,
            Material material,
            float startWidth,
            float endWidth,
            int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.positionCount = 2;
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 5;
            renderer.numCornerVertices = 2;
            renderer.startWidth = startWidth;
            renderer.endWidth = endWidth;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        private static void SetBeamPositions(LineRenderer renderer, Vector2 origin, Vector2 target)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.SetPosition(0, origin);
            renderer.SetPosition(1, target);
        }

        private void RefreshAlpha(float normalizedAlpha)
        {
            SetRendererColor(glowRenderer, glowColor, 0.42f * normalizedAlpha, 0.06f * normalizedAlpha);
            SetRendererColor(coreRenderer, coreColor, 1f * normalizedAlpha, 0.78f * normalizedAlpha);
            SetRendererColor(muzzleRenderer, muzzleColor, 0.82f * normalizedAlpha, 0f);
        }

        private static void SetRendererColor(LineRenderer renderer, Color color, float startAlpha, float endAlpha)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.startColor = new Color(color.r, color.g, color.b, Mathf.Clamp01(startAlpha));
            renderer.endColor = new Color(color.r, color.g, color.b, Mathf.Clamp01(endAlpha));
        }
    }
}
