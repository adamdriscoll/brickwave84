using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class BreakoutGlowRenderer : MonoBehaviour
    {
        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer haloRenderer;
        private float glowScale = 1.18f;
        private float glowAlpha = 0.3f;
        private float haloScale = 1.36f;
        private float haloAlpha = 0.12f;

        public void Configure(float glowScaleMultiplier, float glowOpacity, float haloScaleMultiplier, float haloOpacity)
        {
            glowScale = Mathf.Max(1.01f, glowScaleMultiplier);
            glowAlpha = Mathf.Clamp01(glowOpacity);
            haloScale = Mathf.Max(glowScale, haloScaleMultiplier);
            haloAlpha = Mathf.Clamp01(haloOpacity);
            EnsureRenderers();
            SyncFromCore();
        }

        public void ApplyStyle(ThemeVisualStyle visualStyle)
        {
            EnsureRenderers();
            SyncFromCore();
            ApplyColorAndSprite(visualStyle.PrimaryColor, visualStyle.Sprite);
        }

        public void ApplyColor(Color color)
        {
            EnsureRenderers();
            SyncFromCore();
            ApplyColorAndSprite(color, coreRenderer != null ? coreRenderer.sprite : null);
        }

        private void EnsureRenderers()
        {
            coreRenderer ??= GetComponent<SpriteRenderer>();

            if (glowRenderer == null)
            {
                glowRenderer = CreateLayerRenderer("Glow", -1);
            }

            if (haloRenderer == null)
            {
                haloRenderer = CreateLayerRenderer("Halo", -2);
            }
        }

        private SpriteRenderer CreateLayerRenderer(string layerName, int sortingOffset)
        {
            var child = new GameObject(layerName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.maskInteraction = SpriteMaskInteraction.None;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = (coreRenderer != null ? coreRenderer.sortingOrder : 0) + sortingOffset;
            return renderer;
        }

        private void SyncFromCore()
        {
            if (coreRenderer == null)
            {
                return;
            }

            SyncLayer(glowRenderer, glowScale, coreRenderer.sortingOrder - 1);
            SyncLayer(haloRenderer, haloScale, coreRenderer.sortingOrder - 2);
        }

        private void SyncLayer(SpriteRenderer layerRenderer, float scaleMultiplier, int sortingOrder)
        {
            if (layerRenderer == null || coreRenderer == null)
            {
                return;
            }

            layerRenderer.sortingLayerID = coreRenderer.sortingLayerID;
            layerRenderer.sortingLayerName = coreRenderer.sortingLayerName;
            layerRenderer.sortingOrder = sortingOrder;
            layerRenderer.drawMode = coreRenderer.drawMode;
            layerRenderer.size = coreRenderer.size;
            layerRenderer.flipX = coreRenderer.flipX;
            layerRenderer.flipY = coreRenderer.flipY;
            layerRenderer.transform.localScale = Vector3.one * scaleMultiplier;
        }

        private void ApplyColorAndSprite(Color color, Sprite sprite)
        {
            if (glowRenderer == null || haloRenderer == null)
            {
                return;
            }

            glowRenderer.sprite = sprite;
            haloRenderer.sprite = sprite;
            glowRenderer.color = new Color(color.r, color.g, color.b, glowAlpha);
            haloRenderer.color = new Color(color.r, color.g, color.b, haloAlpha);
        }
    }
}
