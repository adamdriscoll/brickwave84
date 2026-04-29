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
        private readonly Sprite squareSprite;
        private readonly Sprite circleSprite;

        public BreakoutThemeService(
            Color backgroundFallback,
            Color wallFallback,
            Color paddleFallback,
            Color ballFallback,
            Sprite squareSprite,
            Sprite circleSprite)
        {
            this.backgroundFallback = backgroundFallback;
            this.wallFallback = wallFallback;
            this.paddleFallback = paddleFallback;
            this.ballFallback = ballFallback;
            this.squareSprite = squareSprite;
            this.circleSprite = circleSprite;
        }

        public ThemeDefinition AppliedTheme { get; private set; }

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
            return ResolveThemeStyle(ThemeVisualSlot.Ball, ballFallback, ballFallback, circleSprite);
        }

        public ThemeVisualStyle ResolveBrickStyle(BrickDefinition definition)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.gray, squareSprite);
            }

            return ResolveThemeStyle(definition.ResolveThemeSlot(), definition.BaseColor, definition.DamagedColor, squareSprite);
        }

        public ThemeVisualStyle ResolvePowerUpStyle(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return new ThemeVisualStyle(Color.white, Color.white, squareSprite);
            }

            return ResolveThemeStyle(definition.ResolveThemeSlot(), definition.PickupColor, definition.PickupColor, squareSprite);
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

            activeCamera.backgroundColor = ResolveThemeStyle(ThemeVisualSlot.Background, backgroundFallback, backgroundFallback, null).PrimaryColor;
        }

        private void ApplyThemeToWalls(IList<SpriteRenderer> wallRenderers)
        {
            if (wallRenderers == null)
            {
                return;
            }

            var wallStyle = ResolveThemeStyle(ThemeVisualSlot.Wall, wallFallback, wallFallback, squareSprite);

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

            var paddleStyle = ResolveThemeStyle(ThemeVisualSlot.Paddle, paddleFallback, paddleFallback, squareSprite);
            paddleSpriteRenderer.sprite = paddleStyle.Sprite;
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
    }
}
