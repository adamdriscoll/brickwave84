using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutSwitchbackRailSection : MonoBehaviour
    {
        private const float ContactCooldownSeconds = 0.14f;
        private const float HorizontalMagnitude = 0.74f;
        private const float VerticalMagnitude = 0.68f;
        private const float MinimumVertical = 0.1f;

        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutGameController gameController;
        private BreakoutWarpGateWall wall;
        private BreakoutSwitchbackRailVisual visual;
        private float switchCycleSeconds = 2.6f;
        private float phaseOffsetSeconds;

        private float ActiveSwitchSign => Mathf.Repeat(Time.time + phaseOffsetSeconds, switchCycleSeconds * 2f) < switchCycleSeconds
            ? 1f
            : -1f;

        public void Configure(
            BreakoutGameController controller,
            BreakoutWarpGateWall railWall,
            float cycleSeconds,
            float phaseOffset,
            BreakoutSwitchbackRailVisual railVisual)
        {
            gameController = controller;
            wall = railWall == BreakoutWarpGateWall.Right
                ? BreakoutWarpGateWall.Right
                : BreakoutWarpGateWall.Left;
            switchCycleSeconds = Mathf.Max(1.2f, cycleSeconds);
            phaseOffsetSeconds = Mathf.Max(0f, phaseOffset);
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
            ball.ApplyCollisionResponse(BuildSwitchbackDirection(ball.CurrentVelocity, wall, ActiveSwitchSign), MinimumVertical);
            visual?.PlayImpact();
            gameController?.HandleBallHitWall();
            gameController?.TryApplyWarpHandle(ball);
            return true;
        }

        internal static Vector2 BuildSwitchbackDirection(Vector2 incomingVelocity, BreakoutWarpGateWall railWall, float switchSign)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var inwardSign = railWall == BreakoutWarpGateWall.Right ? -1f : 1f;
            var verticalSign = Mathf.Sign(Mathf.Approximately(switchSign, 0f) ? 1f : switchSign);
            var verticalBoost = Mathf.Max(0.36f, Mathf.Abs(incomingDirection.y) * 0.72f);
            var vertical = Mathf.Lerp(VerticalMagnitude, verticalBoost, 0.32f) * verticalSign;

            return new Vector2(HorizontalMagnitude * inwardSign, vertical).normalized;
        }
    }
}
