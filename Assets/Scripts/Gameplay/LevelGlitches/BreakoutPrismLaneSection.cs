using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutPrismLaneSection : MonoBehaviour
    {
        private const float ContactCooldownSeconds = 0.18f;
        private const float HorizontalKick = 0.34f;
        private const float MinimumHorizontal = 0.42f;
        private const float MaximumHorizontal = 0.86f;
        private const float MinimumVertical = 0.14f;

        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutPrismLaneVisual visual;
        private float refractionSign = 1f;

        public void Configure(float laneRefractionSign, BreakoutPrismLaneVisual laneVisual)
        {
            refractionSign = Mathf.Sign(Mathf.Approximately(laneRefractionSign, 0f) ? 1f : laneRefractionSign);
            visual = laneVisual;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || !other.TryGetComponent<BallController>(out var ball))
            {
                return;
            }

            TryRefract(ball);
        }

        private void TryRefract(BallController ball)
        {
            if (ball == null || !ball.HasLaunched)
            {
                return;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return;
            }

            cooldownUntilByBallId[ballId] = now + ContactCooldownSeconds;
            ball.ApplyCollisionResponse(BuildRefractedDirection(ball.CurrentVelocity, refractionSign), MinimumVertical);
            visual?.PlayImpact();
        }

        internal static Vector2 BuildRefractedDirection(Vector2 incomingVelocity, float laneRefractionSign)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var verticalSign = Mathf.Sign(Mathf.Approximately(incomingDirection.y, 0f) ? 1f : incomingDirection.y);
            var horizontalSign = Mathf.Sign(Mathf.Approximately(laneRefractionSign, 0f) ? 1f : laneRefractionSign);
            var horizontalMagnitude = Mathf.Clamp(
                Mathf.Abs(incomingDirection.x) + HorizontalKick,
                MinimumHorizontal,
                MaximumHorizontal);
            var verticalMagnitude = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (horizontalMagnitude * horizontalMagnitude)));

            return new Vector2(horizontalMagnitude * horizontalSign, verticalMagnitude * verticalSign).normalized;
        }
    }
}
