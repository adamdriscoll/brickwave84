using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    internal sealed class BreakoutVectorSightVisual : MonoBehaviour
    {
        private const int SegmentCount = 6;
        private const int SightSortingOrder = 17;
        private const float CoreWidth = 0.035f;
        private const float GlowWidth = 0.13f;
        private const float TickWidth = 0.024f;

        private LineRenderer coreRenderer;
        private LineRenderer glowRenderer;
        private LineRenderer tickRenderer;
        private float pulsePhase;

        public void Configure(Material additiveMaterial)
        {
            coreRenderer = GetComponent<LineRenderer>();
            glowRenderer = CreateChildRenderer("Vector Glow");
            tickRenderer = CreateChildRenderer("Vector Tick");

            ConfigureRenderer(glowRenderer, additiveMaterial, GlowWidth, SightSortingOrder - 1);
            ConfigureRenderer(coreRenderer, additiveMaterial, CoreWidth, SightSortingOrder);
            ConfigureRenderer(tickRenderer, additiveMaterial, TickWidth, SightSortingOrder + 1);
            Hide();
        }

        public void Show(Vector2 origin, Vector2 target, Color coreColor, Color glowColor, float alpha)
        {
            EnsureConfigured();

            var clampedAlpha = Mathf.Clamp01(alpha);
            var direction = target - origin;

            if (direction.sqrMagnitude <= 0.001f)
            {
                Hide();
                return;
            }

            SetVisible(true);
            SetSightPositions(glowRenderer, origin, target, SegmentCount);
            SetSightPositions(coreRenderer, origin, target, SegmentCount);
            SetTickPositions(tickRenderer, target, direction.normalized);
            SetRendererColor(glowRenderer, glowColor, 0.22f * clampedAlpha, 0.02f * clampedAlpha);
            SetRendererColor(coreRenderer, coreColor, 0.78f * clampedAlpha, 0.16f * clampedAlpha);
            SetRendererColor(tickRenderer, coreColor, 0.64f * clampedAlpha, 0.08f * clampedAlpha);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void Update()
        {
            pulsePhase += Time.deltaTime * 5.8f;
        }

        private void EnsureConfigured()
        {
            if (coreRenderer == null)
            {
                Configure(null);
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

        private static void ConfigureRenderer(LineRenderer renderer, Material material, float width, int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.positionCount = SegmentCount + 1;
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 3;
            renderer.numCornerVertices = 2;
            renderer.startWidth = width;
            renderer.endWidth = width * 0.55f;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        private void SetSightPositions(LineRenderer renderer, Vector2 origin, Vector2 target, int segmentCount)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.positionCount = segmentCount + 1;
            var direction = target - origin;
            var normal = new Vector2(-direction.y, direction.x).normalized;
            var shimmer = Mathf.Sin(pulsePhase) * 0.025f;

            for (var index = 0; index <= segmentCount; index++)
            {
                var t = index / (float)segmentCount;
                var offset = normal * shimmer * Mathf.Sin(t * Mathf.PI);
                renderer.SetPosition(index, origin + (direction * t) + offset);
            }
        }

        private static void SetTickPositions(LineRenderer renderer, Vector2 target, Vector2 direction)
        {
            if (renderer == null)
            {
                return;
            }

            var normal = new Vector2(-direction.y, direction.x) * 0.11f;
            renderer.positionCount = 2;
            renderer.SetPosition(0, target - normal);
            renderer.SetPosition(1, target + normal);
        }

        private void SetVisible(bool visible)
        {
            SetRendererEnabled(coreRenderer, visible);
            SetRendererEnabled(glowRenderer, visible);
            SetRendererEnabled(tickRenderer, visible);
        }

        private static void SetRendererEnabled(Renderer renderer, bool visible)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
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
