using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutBackgroundPresenter
    {
        private readonly Camera activeCamera;
        private readonly float cameraHalfHeight;
        private readonly Color backgroundFallback;
        private readonly Color wallFallback;
        private readonly float imageTintStrength;
        private readonly float imageAlpha;
        private readonly float hazeAlpha;
        private readonly float scanlineAlpha;
        private readonly Sprite hazeSprite;
        private readonly Sprite scanlineSprite;

        private SpriteRenderer backgroundRenderer;
        private SpriteRenderer hazeRenderer;
        private SpriteRenderer scanlineRenderer;

        public BreakoutBackgroundPresenter(
            Camera activeCamera,
            float cameraHalfHeight,
            Color backgroundFallback,
            Color wallFallback,
            float imageTintStrength,
            float imageAlpha,
            float hazeAlpha,
            float scanlineAlpha,
            Sprite hazeSprite,
            Sprite scanlineSprite)
        {
            this.activeCamera = activeCamera;
            this.cameraHalfHeight = cameraHalfHeight;
            this.backgroundFallback = backgroundFallback;
            this.wallFallback = wallFallback;
            this.imageTintStrength = imageTintStrength;
            this.imageAlpha = imageAlpha;
            this.hazeAlpha = hazeAlpha;
            this.scanlineAlpha = scanlineAlpha;
            this.hazeSprite = hazeSprite;
            this.scanlineSprite = scanlineSprite;
        }

        public void Create(Transform backgroundRoot, Material spriteMaterial)
        {
            var backgroundObject = new GameObject("Backdrop");
            backgroundObject.transform.SetParent(backgroundRoot, false);
            backgroundObject.transform.position = Vector3.zero;

            backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingOrder = -100;
            backgroundRenderer.sharedMaterial = spriteMaterial;

            var hazeObject = new GameObject("Backdrop Haze");
            hazeObject.transform.SetParent(backgroundRoot, false);
            hazeRenderer = hazeObject.AddComponent<SpriteRenderer>();
            hazeRenderer.sprite = hazeSprite;
            hazeRenderer.sortingOrder = -99;
            hazeRenderer.sharedMaterial = spriteMaterial;

            var scanlineObject = new GameObject("Backdrop Scanlines");
            scanlineObject.transform.SetParent(backgroundRoot, false);
            scanlineRenderer = scanlineObject.AddComponent<SpriteRenderer>();
            scanlineRenderer.sprite = scanlineSprite;
            scanlineRenderer.sortingOrder = -98;
            scanlineRenderer.sharedMaterial = spriteMaterial;
        }

        public void UpdateVisuals(BreakoutThemeService themeService, Sprite backgroundSprite)
        {
            if (backgroundRenderer == null || activeCamera == null)
            {
                return;
            }

            var backgroundStyle = themeService != null
                ? themeService.ResolveBackgroundStyle(backgroundSprite)
                : new ThemeVisualStyle(backgroundFallback, backgroundFallback, backgroundSprite);

            var sprite = backgroundStyle.Sprite;
            backgroundRenderer.sprite = sprite;
            backgroundRenderer.enabled = sprite != null;

            var tint = Color.Lerp(Color.white, backgroundStyle.PrimaryColor, imageTintStrength);
            tint.a = imageAlpha;
            backgroundRenderer.color = tint;
            backgroundRenderer.transform.position = Vector3.zero;

            var overlayScale = CalculateFullscreenScale(sprite != null ? sprite : hazeSprite);
            backgroundRenderer.transform.localScale = sprite != null ? CalculateFullscreenScale(sprite) : overlayScale;

            if (hazeRenderer != null)
            {
                var hazeColor = Color.Lerp(backgroundStyle.PrimaryColor, wallFallback, 0.58f);
                hazeColor.a = hazeAlpha;
                hazeRenderer.enabled = hazeSprite != null;
                hazeRenderer.color = hazeColor;
                hazeRenderer.transform.position = Vector3.zero;
                hazeRenderer.transform.localScale = overlayScale;
            }

            if (scanlineRenderer != null)
            {
                var scanlineColor = Color.Lerp(Color.white, backgroundStyle.PrimaryColor, 0.3f);
                scanlineColor.a = scanlineAlpha;
                scanlineRenderer.enabled = scanlineSprite != null;
                scanlineRenderer.color = scanlineColor;
                scanlineRenderer.transform.position = Vector3.zero;
                scanlineRenderer.transform.localScale = overlayScale;
            }
        }

        public void ApplyVisibilityMultiplier(float visibilityMultiplier)
        {
            if (hazeRenderer == null)
            {
                return;
            }

            var hazeColor = hazeRenderer.color;
            hazeColor.a = Mathf.Clamp01(hazeAlpha + ((1f - visibilityMultiplier) * 0.32f));
            hazeRenderer.color = hazeColor;
        }

        private Vector3 CalculateFullscreenScale(Sprite sprite)
        {
            if (sprite == null || activeCamera == null)
            {
                return Vector3.one;
            }

            var spriteSize = sprite.bounds.size;

            if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f)
            {
                return Vector3.one;
            }

            var visibleHeight = cameraHalfHeight * 2f;
            var visibleWidth = visibleHeight * activeCamera.aspect;
            var scale = Mathf.Max(visibleWidth / spriteSize.x, visibleHeight / spriteSize.y);
            return new Vector3(scale, scale, 1f);
        }
    }
}
