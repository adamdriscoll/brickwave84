using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutStaticWallSection : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(0.99f, 0.27f, 0.31f, 0.42f);
        private static readonly Color WeakColor = new Color(1f, 0.87f, 0.36f, 0.82f);

        private BreakoutGameController gameController;
        private BreakoutWarpGateWall wall;
        private SpriteRenderer spriteRenderer;
        private Vector2 sectionSize;
        private float weakCycleSeconds = 2.4f;
        private float weakDurationSeconds = 0.85f;
        private float phaseOffsetSeconds;
        private float impactFlashTimer;

        public bool IsWeakPhase
        {
            get
            {
                var cycle = Mathf.Max(0.75f, weakCycleSeconds);
                return Mathf.Repeat(Time.time + phaseOffsetSeconds, cycle) <= weakDurationSeconds;
            }
        }

        public void Configure(
            BreakoutGameController controller,
            BreakoutWarpGateWall staticWall,
            SpriteRenderer renderer,
            Vector2 size,
            float weakCycle,
            float weakDuration,
            float phaseOffset)
        {
            gameController = controller;
            wall = staticWall == BreakoutWarpGateWall.Right
                ? BreakoutWarpGateWall.Right
                : BreakoutWarpGateWall.Left;
            spriteRenderer = renderer;
            sectionSize = size;
            weakCycleSeconds = Mathf.Max(0.75f, weakCycle);
            weakDurationSeconds = Mathf.Clamp(weakDuration, 0.18f, weakCycleSeconds * 0.75f);
            phaseOffsetSeconds = Mathf.Max(0f, phaseOffset);
            RefreshVisual();
        }

        public bool TryHandleBallCollision(BallController ball)
        {
            if (ball == null || !IsWeakPhase)
            {
                impactFlashTimer = 0.08f;
                RefreshVisual();
                return false;
            }

            impactFlashTimer = 0.2f;
            gameController?.HandleBallHitWall();

            var velocity = ball.CurrentVelocity;
            var travelDirection = velocity.sqrMagnitude > 0.01f
                ? velocity.normalized
                : Vector2.down;
            var inwardX = wall == BreakoutWarpGateWall.Right ? -0.24f : 0.24f;
            var downwardY = Mathf.Clamp(travelDirection.y * 0.45f, -0.82f, 0.3f);

            if (downwardY > -0.22f)
            {
                downwardY = -0.42f;
            }

            ball.ApplyCollisionResponse(new Vector2(inwardX, downwardY), 0.05f);
            RefreshVisual();
            return true;
        }

        private void Update()
        {
            if (impactFlashTimer > 0f)
            {
                impactFlashTimer = Mathf.Max(0f, impactFlashTimer - Time.deltaTime);
            }

            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var weakPhase = IsWeakPhase;
            var pulse = weakPhase
                ? 0.55f + (Mathf.PingPong(Time.time * 8.5f, 1f) * 0.45f)
                : 0.18f + (Mathf.PingPong(Time.time * 2.8f, 1f) * 0.16f);
            var flash = impactFlashTimer > 0f ? Mathf.Clamp01(impactFlashTimer / 0.2f) : 0f;
            var color = Color.Lerp(NormalColor, WeakColor, weakPhase ? pulse : 0f);
            color = Color.Lerp(color, Color.white, flash * 0.55f);
            spriteRenderer.color = color;
            spriteRenderer.transform.localScale = new Vector3(
                sectionSize.x * (1f + (flash * 0.12f)),
                sectionSize.y,
                1f);
        }
    }
}
