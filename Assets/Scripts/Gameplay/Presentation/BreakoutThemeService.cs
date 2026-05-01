using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutThemeService
    {
        private readonly Color backgroundFallback;
        private readonly Color wallFallback;
        private readonly Color paddleFallback;
        private readonly Color ballFallback;
        private readonly Sprite backgroundFallbackSprite;
        private readonly Sprite wallFallbackSprite;
        private readonly Sprite paddleFallbackSprite;
        private readonly Sprite ballFallbackSprite;
        private readonly Sprite brickFallbackSprite;
        private readonly Sprite powerUpFallbackSprite;
        private readonly IDictionary<string, Sprite> powerUpSpriteOverrides;
        private readonly Dictionary<string, Sprite> powerUpSpriteCache;
        private readonly Dictionary<string, Sprite> brickSpriteCache;

        public BreakoutThemeService(
            Color backgroundFallback,
            Color wallFallback,
            Color paddleFallback,
            Color ballFallback,
            Sprite backgroundFallbackSprite,
            Sprite wallFallbackSprite,
            Sprite paddleFallbackSprite,
            Sprite ballFallbackSprite,
            Sprite brickFallbackSprite,
            Sprite powerUpFallbackSprite,
            IDictionary<string, Sprite> powerUpSpriteOverrides = null)
        {
            this.backgroundFallback = backgroundFallback;
            this.wallFallback = wallFallback;
            this.paddleFallback = paddleFallback;
            this.ballFallback = ballFallback;
            this.backgroundFallbackSprite = backgroundFallbackSprite;
            this.wallFallbackSprite = wallFallbackSprite;
            this.paddleFallbackSprite = paddleFallbackSprite;
            this.ballFallbackSprite = ballFallbackSprite;
            this.brickFallbackSprite = brickFallbackSprite;
            this.powerUpFallbackSprite = powerUpFallbackSprite;
            this.powerUpSpriteOverrides = powerUpSpriteOverrides;
            powerUpSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            brickSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        }

        public ThemeDefinition AppliedTheme { get; private set; }

        public ThemeVisualStyle ResolveBackgroundStyle(Sprite fallbackSprite = null)
        {
            return ResolveThemeStyle(
                ThemeVisualSlot.Background,
                backgroundFallback,
                backgroundFallback,
                fallbackSprite != null ? fallbackSprite : backgroundFallbackSprite);
        }

        public void ApplyTheme(
            ThemeDefinition theme,
            Camera activeCamera,
            IList<SpriteRenderer> wallRenderers,
            SpriteRenderer paddleSpriteRenderer,
            BallController serveBall,
            IList<BallController> activeBalls,
            IList<Brick> bricks,
            IList<PowerUpPickup> activePickups)
        {
            AppliedTheme = theme;
            ApplyThemeToCamera(activeCamera);
            ApplyThemeToWalls(wallRenderers);
            ApplyThemeToPaddle(paddleSpriteRenderer);
            ApplyThemeToBall(serveBall);
            ApplyThemeToBalls(activeBalls);
            ApplyThemeToBricks(bricks);
            ApplyThemeToPickups(activePickups);
        }

        public ThemeVisualStyle ResolveBallStyle()
        {
            return ResolveThemeStyle(ThemeVisualSlot.Ball, ballFallback, ballFallback, ballFallbackSprite);
        }

        public ThemeVisualStyle ResolveBrickStyle(BrickDefinition definition)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.gray, brickFallbackSprite);
            }

            return ResolveThemeStyle(
                definition.ResolveThemeSlot(),
                definition.BaseColor,
                definition.DamagedColor,
                ResolveBrickSprite(definition));
        }

        public ThemeVisualStyle ResolvePowerUpStyle(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.white, powerUpFallbackSprite);
            }

            return ResolveThemeStyle(
                definition.ResolveThemeSlot(),
                definition.PickupColor,
                definition.PickupColor,
                ResolvePowerUpSprite(definition));
        }

        public ThemeVisualStyle ResolveThemeStyle(ThemeVisualSlot slot, Color fallbackPrimary, Color fallbackSecondary, Sprite fallbackSprite)
        {
            var fallbackStyle = new ThemeVisualStyle(fallbackPrimary, fallbackSecondary, fallbackSprite);
            return AppliedTheme != null
                ? AppliedTheme.ResolveStyle(slot, fallbackStyle)
                : fallbackStyle;
        }

        private void ApplyThemeToCamera(Camera activeCamera)
        {
            if (activeCamera == null)
            {
                return;
            }

            activeCamera.backgroundColor = ResolveBackgroundStyle().PrimaryColor;
        }

        private void ApplyThemeToWalls(IList<SpriteRenderer> wallRenderers)
        {
            if (wallRenderers == null)
            {
                return;
            }

            var wallStyle = ResolveThemeStyle(ThemeVisualSlot.Wall, wallFallback, wallFallback, wallFallbackSprite);

            for (var index = wallRenderers.Count - 1; index >= 0; index--)
            {
                var wallRenderer = wallRenderers[index];

                if (wallRenderer == null)
                {
                    wallRenderers.RemoveAt(index);
                    continue;
                }

                wallRenderer.sprite = wallStyle.Sprite;
                wallRenderer.color = wallStyle.PrimaryColor;
            }
        }

        private void ApplyThemeToPaddle(SpriteRenderer paddleSpriteRenderer)
        {
            if (paddleSpriteRenderer == null)
            {
                return;
            }

            var paddleStyle = ResolveThemeStyle(ThemeVisualSlot.Paddle, paddleFallback, paddleFallback, paddleFallbackSprite);
            paddleSpriteRenderer.sprite = paddleStyle.Sprite;
            NormalizeSpriteRendererScale(paddleSpriteRenderer);
            paddleSpriteRenderer.color = paddleStyle.PrimaryColor;

            if (paddleSpriteRenderer.TryGetComponent<BreakoutGlowRenderer>(out var glowRenderer))
            {
                glowRenderer.ApplyStyle(paddleStyle);
            }
        }

        private void ApplyThemeToBalls(IList<BallController> activeBalls)
        {
            if (activeBalls == null)
            {
                return;
            }

            for (var index = activeBalls.Count - 1; index >= 0; index--)
            {
                var ball = activeBalls[index];

                if (ball == null)
                {
                    activeBalls.RemoveAt(index);
                    continue;
                }

                ApplyThemeToBall(ball);
            }
        }

        private void ApplyThemeToBall(BallController ball)
        {
            if (ball == null || !ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                return;
            }

            var ballStyle = ResolveBallStyle();
            spriteRenderer.sprite = ballStyle.Sprite;
            spriteRenderer.color = ballStyle.PrimaryColor;

            if (spriteRenderer.TryGetComponent<BreakoutGlowRenderer>(out var glowRenderer))
            {
                glowRenderer.ApplyStyle(ballStyle);
            }
        }

        private void ApplyThemeToBricks(IList<Brick> bricks)
        {
            if (bricks == null)
            {
                return;
            }

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.ApplyTheme(ResolveBrickStyle(brick.Definition));
            }
        }

        private void ApplyThemeToPickups(IList<PowerUpPickup> activePickups)
        {
            if (activePickups == null)
            {
                return;
            }

            for (var index = activePickups.Count - 1; index >= 0; index--)
            {
                var pickup = activePickups[index];

                if (pickup == null)
                {
                    activePickups.RemoveAt(index);
                    continue;
                }

                pickup.ApplyTheme(ResolvePowerUpStyle(pickup.Definition));
            }
        }

        private Sprite ResolvePowerUpSprite(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return powerUpFallbackSprite;
            }

            var resourcePath = definition.ResolvePickupSpriteResourcePath();

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return powerUpFallbackSprite;
            }

            if (powerUpSpriteOverrides != null
                && powerUpSpriteOverrides.TryGetValue(resourcePath, out var overrideSprite)
                && overrideSprite != null)
            {
                return overrideSprite;
            }

            if (!powerUpSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            {
                cachedSprite = Resources.Load<Sprite>(resourcePath);
                powerUpSpriteCache[resourcePath] = cachedSprite;
            }

            return cachedSprite != null ? cachedSprite : powerUpFallbackSprite;
        }

        private Sprite ResolveBrickSprite(BrickDefinition definition)
        {
            if (definition == null)
            {
                return brickFallbackSprite;
            }

            var resourcePath = definition.ResolveSpriteResourcePath();

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return brickFallbackSprite;
            }

            if (!brickSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            {
                cachedSprite = Resources.Load<Sprite>(resourcePath);
                brickSpriteCache[resourcePath] = cachedSprite;
            }

            return cachedSprite != null ? cachedSprite : brickFallbackSprite;
        }

        private static void NormalizeSpriteRendererScale(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var sprite = spriteRenderer.sprite;

            if (sprite == null)
            {
                spriteRenderer.transform.localScale = Vector3.one;
                return;
            }

            var spriteSize = sprite.bounds.size;
            var scaleX = spriteSize.x > 0.0001f ? 1f / spriteSize.x : 1f;
            var scaleY = spriteSize.y > 0.0001f ? 1f / spriteSize.y : 1f;
            spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
