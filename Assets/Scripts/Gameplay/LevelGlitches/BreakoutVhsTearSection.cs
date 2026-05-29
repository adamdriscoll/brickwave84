using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutVhsTearSection : MonoBehaviour
    {
        private const float MinimumVertical = 0.12f;
        private const float MinimumHorizontal = 0.34f;
        private const float MaximumHorizontal = 0.88f;

        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutVhsTearVisual visual;
        private float deflectionDegrees = 16f;
        private float jitterStrength = 0.2f;
        private float contactCooldownSeconds = 0.14f;

        public void Configure(
            float tearDeflectionDegrees,
            float tearJitterStrength,
            float cooldownSeconds,
            BreakoutVhsTearVisual tearVisual)
        {
            deflectionDegrees = Mathf.Clamp(tearDeflectionDegrees, 8f, 26f);
            jitterStrength = Mathf.Clamp(tearJitterStrength, 0.08f, 0.42f);
            contactCooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.06f, 0.28f);
            visual = tearVisual;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDeflect(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDeflect(other);
        }

        private void TryDeflect(Collider2D other)
        {
            if (other == null || !other.TryGetComponent<BallController>(out var ball) || !ball.HasLaunched)
            {
                return;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return;
            }

            cooldownUntilByBallId[ballId] = now + contactCooldownSeconds;
            var tearPhase = now + transform.position.y;
            ball.ApplyCollisionResponse(
                BuildDeflectedDirection(ball.CurrentVelocity, ball.transform.position.x, tearPhase, deflectionDegrees, jitterStrength),
                MinimumVertical);
            visual?.PlayImpact(ball.transform.position.x);
        }

        internal static Vector2 BuildDeflectedDirection(
            Vector2 incomingVelocity,
            float worldX,
            float tearPhase,
            float tearDeflectionDegrees,
            float tearJitterStrength)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var verticalSign = Mathf.Sign(Mathf.Approximately(incomingDirection.y, 0f) ? 1f : incomingDirection.y);
            var tearWave = Mathf.Sin((worldX * 2.37f) + (tearPhase * 5.1f));
            var horizontalSign = Mathf.Sign(tearWave);

            if (Mathf.Approximately(horizontalSign, 0f))
            {
                horizontalSign = Mathf.Sign(incomingDirection.x);
            }

            if (Mathf.Approximately(horizontalSign, 0f))
            {
                horizontalSign = 1f;
            }

            var deflection = Mathf.Sin(Mathf.Clamp(tearDeflectionDegrees, 8f, 26f) * Mathf.Deg2Rad);
            var jitter = Mathf.Abs(tearWave) * Mathf.Clamp(tearJitterStrength, 0.08f, 0.42f) * 0.45f;
            var horizontalMagnitude = Mathf.Clamp(
                Mathf.Abs(incomingDirection.x) + deflection + jitter,
                MinimumHorizontal,
                MaximumHorizontal);
            var verticalMagnitude = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (horizontalMagnitude * horizontalMagnitude)));

            return new Vector2(horizontalMagnitude * horizontalSign, verticalMagnitude * verticalSign).normalized;
        }
    }
}
