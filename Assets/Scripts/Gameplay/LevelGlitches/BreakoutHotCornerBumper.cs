using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutHotCornerBumper : MonoBehaviour
    {
        private static readonly Color CoreColor = new Color(1f, 0.38f, 0.13f, 0.88f);
        private static readonly Color GlowColor = new Color(1f, 0.87f, 0.36f, 0.5f);

        private const float ContactCooldownSeconds = 0.14f;
        private const float MinimumVertical = 0.08f;

        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutGameController gameController;
        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private Vector2 targetPoint;
        private float speedBurstMultiplier = 1.18f;
        private float speedBurstDurationSeconds = 2.6f;
        private float impactFlashTimer;

        public void Configure(
            BreakoutGameController controller,
            Sprite sprite,
            Material coreMaterial,
            Material glowMaterial,
            Vector2 size,
            Vector2 centerTarget,
            float burstMultiplier,
            float burstDurationSeconds)
        {
            gameController = controller;
            targetPoint = centerTarget;
            speedBurstMultiplier = Mathf.Max(1f, burstMultiplier);
            speedBurstDurationSeconds = Mathf.Max(0.1f, burstDurationSeconds);

            coreRenderer = CreateLayer("Hot Corner Core", sprite, coreMaterial, CoreColor, size, 12);
            glowRenderer = CreateLayer("Hot Corner Glow", sprite, glowMaterial != null ? glowMaterial : coreMaterial, GlowColor, size * 1.42f, 11);
        }

        public bool TryHandleBallCollision(BallController ball)
        {
            if (ball == null)
            {
                return false;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return true;
            }

            cooldownUntilByBallId[ballId] = now + ContactCooldownSeconds;
            impactFlashTimer = 0.22f;
            ball.ApplySpeedBurst(speedBurstMultiplier, speedBurstDurationSeconds);
            ball.ApplyCollisionResponse(BuildKickDirection(ball.transform.position, targetPoint, ball.CurrentVelocity), MinimumVertical);
            gameController?.HandleBallHitWall();
            RefreshVisual();
            return true;
        }

        internal static Vector2 BuildKickDirection(Vector2 ballPosition, Vector2 centerTarget, Vector2 incomingVelocity)
        {
            var centerDirection = centerTarget - ballPosition;

            if (centerDirection.sqrMagnitude <= 0.001f)
            {
                centerDirection = incomingVelocity.sqrMagnitude > 0.001f
                    ? -incomingVelocity.normalized
                    : Vector2.down;
            }

            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : centerDirection.normalized;
            var blendedDirection = (centerDirection.normalized * 1.35f) + (incomingDirection * 0.25f);

            return blendedDirection.sqrMagnitude > 0.001f
                ? blendedDirection.normalized
                : centerDirection.normalized;
        }

        private void Update()
        {
            if (impactFlashTimer > 0f)
            {
                impactFlashTimer = Mathf.Max(0f, impactFlashTimer - Time.deltaTime);
            }

            RefreshVisual();
        }

        private SpriteRenderer CreateLayer(
            string layerName,
            Sprite sprite,
            Material material,
            Color color,
            Vector2 size,
            int sortingOrder)
        {
            var layerObject = new GameObject(layerName);
            layerObject.transform.SetParent(transform, false);
            layerObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private void RefreshVisual()
        {
            var flash = impactFlashTimer > 0f ? Mathf.Clamp01(impactFlashTimer / 0.22f) : 0f;
            var pulse = 0.74f + (Mathf.PingPong(Time.time * 5.2f, 1f) * 0.26f);

            if (coreRenderer != null)
            {
                coreRenderer.color = Color.Lerp(CoreColor, Color.white, flash * 0.5f);
                coreRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 28f);
            }

            if (glowRenderer != null)
            {
                glowRenderer.color = Color.Lerp(GlowColor * pulse, Color.white, flash * 0.35f);
                glowRenderer.transform.localScale = new Vector3(1f + (flash * 0.18f), 1f + (flash * 0.18f), 1f);
            }
        }
    }
}
