using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutTurboRailSection : MonoBehaviour
    {
        private const float ContactCooldownSeconds = 0.12f;

        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutGameController gameController;
        private BreakoutTurboRailVisual visual;
        private BreakoutWarpGateWall wall;
        private float speedBurstMultiplier = 1.25f;
        private float speedBurstDuration = 4f;
        private float speedBurstStackMultiplier = 0.12f;
        private float speedBurstMaximumMultiplier = 1.85f;
        private float speedBurstStackDuration = 1.25f;
        private float speedBurstMaximumDuration = 7.5f;

        public void Configure(
            BreakoutGameController controller,
            BreakoutWarpGateWall railWall,
            float burstMultiplier,
            float burstDurationSeconds,
            float burstStackMultiplier,
            float burstMaximumMultiplier,
            float burstStackDurationSeconds,
            float burstMaximumDurationSeconds,
            BreakoutTurboRailVisual railVisual)
        {
            gameController = controller;
            wall = railWall;
            speedBurstMultiplier = Mathf.Max(1f, burstMultiplier);
            speedBurstDuration = Mathf.Max(0.1f, burstDurationSeconds);
            speedBurstStackMultiplier = Mathf.Max(0f, burstStackMultiplier);
            speedBurstMaximumMultiplier = Mathf.Max(speedBurstMultiplier, burstMaximumMultiplier);
            speedBurstStackDuration = Mathf.Max(0f, burstStackDurationSeconds);
            speedBurstMaximumDuration = Mathf.Max(speedBurstDuration, burstMaximumDurationSeconds);
            visual = railVisual;
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
            var bounceDirection = BuildBounceDirection(ball.CurrentVelocity);
            ball.ApplyStackingSpeedBurst(
                speedBurstMultiplier,
                speedBurstDuration,
                speedBurstStackMultiplier,
                speedBurstMaximumMultiplier,
                speedBurstStackDuration,
                speedBurstMaximumDuration);
            ball.ApplyCollisionResponse(bounceDirection, wall == BreakoutWarpGateWall.Top ? 0.08f : 0.04f);
            visual?.PlayImpact();
            gameController?.HandleBallHitWall();
            gameController?.TryApplyWarpHandle(ball);
            return true;
        }

        private Vector2 BuildBounceDirection(Vector2 incomingVelocity)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var tangentNoise = gameController != null
                ? gameController.NextGameplayRandomFloat(-0.18f, 0.18f)
                : Random.Range(-0.18f, 0.18f);

            return wall switch
            {
                BreakoutWarpGateWall.Left => new Vector2(1f, incomingDirection.y + tangentNoise).normalized,
                BreakoutWarpGateWall.Right => new Vector2(-1f, incomingDirection.y + tangentNoise).normalized,
                BreakoutWarpGateWall.Top => new Vector2(incomingDirection.x + tangentNoise, -1f).normalized,
                _ => incomingDirection,
            };
        }
    }
}
