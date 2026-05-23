using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutPaddleSpawnResult
    {
        public BreakoutPaddleSpawnResult(PaddleController paddle, Collider2D collider, SpriteRenderer spriteRenderer)
        {
            Paddle = paddle;
            Collider = collider;
            SpriteRenderer = spriteRenderer;
        }

        public PaddleController Paddle { get; }

        public Collider2D Collider { get; }

        public SpriteRenderer SpriteRenderer { get; }
    }

    internal sealed class BreakoutPaddleSpawnService
    {
        public const int PaddleSortingOrder = 40;

        private readonly BreakoutGameController controller;
        private readonly Transform root;
        private readonly Vector2 paddleSize;
        private readonly Color paddleColor;
        private readonly Sprite paddleSprite;
        private readonly Material spriteMaterial;
        private readonly PhysicsMaterial2D physicsMaterial;

        public BreakoutPaddleSpawnService(
            BreakoutGameController controller,
            Transform root,
            Vector2 paddleSize,
            Color paddleColor,
            Sprite paddleSprite,
            Material spriteMaterial,
            PhysicsMaterial2D physicsMaterial)
        {
            this.controller = controller;
            this.root = root;
            this.paddleSize = paddleSize;
            this.paddleColor = paddleColor;
            this.paddleSprite = paddleSprite;
            this.spriteMaterial = spriteMaterial;
            this.physicsMaterial = physicsMaterial;
        }

        public BreakoutPaddleSpawnResult CreatePaddle(float speed, float arenaLeft, float arenaRight, float startY)
        {
            var paddleObject = new GameObject("Paddle");
            paddleObject.transform.SetParent(root, false);
            paddleObject.transform.localScale = new Vector3(paddleSize.x, paddleSize.y, 1f);

            var paddleVisual = new GameObject("Visual");
            paddleVisual.transform.SetParent(paddleObject.transform, false);

            var spriteRenderer = paddleVisual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = paddleSprite;
            spriteRenderer.sortingOrder = PaddleSortingOrder;
            spriteRenderer.enabled = true;
            spriteRenderer.sharedMaterial = spriteMaterial;
            BreakoutSpriteRendererUtility.ApplyTint(spriteRenderer, paddleColor);
            BreakoutSpriteRendererUtility.NormalizeScale(spriteRenderer);

            var collider = paddleObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = physicsMaterial;

            var body = paddleObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var paddle = paddleObject.AddComponent<PaddleController>();
            paddle.Configure(controller, speed, arenaLeft, arenaRight, startY);

            return new BreakoutPaddleSpawnResult(paddle, collider, spriteRenderer);
        }
    }
}
