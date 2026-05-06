using System;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutBallSpawnService
    {
        private const int BallSortingOrder = 20;

        private readonly BreakoutGameController controller;
        private readonly Transform ballsRoot;
        private readonly float ballRadius;
        private readonly Vector2 paddleSize;
        private readonly float minimumVerticalDirection;
        private readonly Material spriteMaterial;
        private readonly PhysicsMaterial2D physicsMaterial;
        private readonly Func<PaddleController> paddleResolver;
        private readonly Func<float> speedResolver;
        private readonly Func<ThemeVisualStyle> styleResolver;
        private readonly Func<Vector2> gravityWellCenterResolver;
        private readonly Func<BreakoutEffectModifiers> effectResolver;
        private int ballInstanceCounter;

        public BreakoutBallSpawnService(
            BreakoutGameController controller,
            Transform ballsRoot,
            float ballRadius,
            Vector2 paddleSize,
            float minimumVerticalDirection,
            Material spriteMaterial,
            PhysicsMaterial2D physicsMaterial,
            Func<PaddleController> paddleResolver,
            Func<float> speedResolver,
            Func<ThemeVisualStyle> styleResolver,
            Func<Vector2> gravityWellCenterResolver,
            Func<BreakoutEffectModifiers> effectResolver)
        {
            this.controller = controller;
            this.ballsRoot = ballsRoot;
            this.ballRadius = ballRadius;
            this.paddleSize = paddleSize;
            this.minimumVerticalDirection = minimumVerticalDirection;
            this.spriteMaterial = spriteMaterial;
            this.physicsMaterial = physicsMaterial;
            this.paddleResolver = paddleResolver ?? throw new ArgumentNullException(nameof(paddleResolver));
            this.speedResolver = speedResolver ?? throw new ArgumentNullException(nameof(speedResolver));
            this.styleResolver = styleResolver ?? throw new ArgumentNullException(nameof(styleResolver));
            this.gravityWellCenterResolver = gravityWellCenterResolver ?? throw new ArgumentNullException(nameof(gravityWellCenterResolver));
            this.effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        }

        public BallController CreateBall(bool followsPaddleWhenIdle, float lossThresholdY)
        {
            ballInstanceCounter++;

            var ballName = followsPaddleWhenIdle ? "Ball" : $"Ball {ballInstanceCounter}";
            var ballObject = new GameObject(ballName);
            ballObject.transform.SetParent(ballsRoot, false);
            ballObject.transform.localScale = Vector3.one * (ballRadius * 2f);

            var ballStyle = styleResolver();
            var spriteRenderer = ballObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ballStyle.Sprite;
            spriteRenderer.color = ballStyle.PrimaryColor;
            spriteRenderer.sortingOrder = BallSortingOrder;
            spriteRenderer.sharedMaterial = spriteMaterial;

            var collider = ballObject.AddComponent<CircleCollider2D>();
            collider.sharedMaterial = physicsMaterial;

            var body = ballObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.linearDamping = 0f;

            var ball = ballObject.AddComponent<BallController>();
            ball.Configure(
                controller,
                paddleResolver(),
                speedResolver(),
                minimumVerticalDirection,
                lossThresholdY,
                ballRadius + (paddleSize.y * 0.5f) + 0.05f,
                followsPaddleWhenIdle);
            ball.ApplyVisualStyle(ballStyle);

            var activeEffects = effectResolver();
            ball.SetPhaseThroughBricks(activeEffects.PhaseBallEnabled);
            ball.SetSizeMultiplier(activeEffects.BallSizeMultiplier);
            ball.SetGravityWell(gravityWellCenterResolver(), activeEffects.GravityWellStrength);
            ball.SetHotPotatoStrength(activeEffects.HotPotatoStrength);
            ball.SetExplosiveBallStrength(activeEffects.ExplosiveBallStrength);

            return ball;
        }
    }
}
