using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutThinAirWallSection : MonoBehaviour
    {
        private static readonly Color WarningColor = new Color(1f, 0.22f, 0.84f, 0.92f);
        private static readonly Color OpenColor = new Color(0.03f, 0.93f, 0.98f, 0.18f);

        private BreakoutWarpGateWall wall;
        private SpriteRenderer wallRenderer;
        private Collider2D wallCollider;
        private Vector3 baseScale = Vector3.one;
        private Color baseColor = Color.white;
        private float openCycleSeconds = 3.8f;
        private float openDurationSeconds = 1f;
        private float warningSeconds = 0.6f;
        private float phaseOffsetSeconds;

        public BreakoutWarpGateWall Wall => wall;

        public bool IsOpenPhase => PhaseTime < openDurationSeconds;

        private float PhaseTime => Mathf.Repeat(Time.time + phaseOffsetSeconds, openCycleSeconds);

        public void Configure(
            BreakoutWarpGateWall thinAirWall,
            SpriteRenderer renderer,
            Collider2D collider,
            float cycleSeconds,
            float durationSeconds,
            float warningDurationSeconds,
            float phaseOffset)
        {
            wall = thinAirWall == BreakoutWarpGateWall.Right
                ? BreakoutWarpGateWall.Right
                : BreakoutWarpGateWall.Left;
            wallRenderer = renderer;
            wallCollider = collider;
            baseScale = transform.localScale;
            baseColor = wallRenderer != null ? wallRenderer.color : Color.white;
            openCycleSeconds = Mathf.Max(1.2f, cycleSeconds);
            openDurationSeconds = Mathf.Clamp(durationSeconds, 0.24f, openCycleSeconds * 0.65f);
            warningSeconds = Mathf.Clamp(warningDurationSeconds, 0.1f, openCycleSeconds - openDurationSeconds);
            phaseOffsetSeconds = Mathf.Max(0f, phaseOffset);
            RefreshVisual();
        }

        public void Restore()
        {
            if (wallCollider != null)
            {
                wallCollider.enabled = true;
            }

            if (wallRenderer != null)
            {
                wallRenderer.color = baseColor;
            }

            transform.localScale = baseScale;
        }

        private void Update()
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            var open = IsOpenPhase;

            if (wallCollider != null)
            {
                wallCollider.enabled = !open;
            }

            if (wallRenderer == null)
            {
                return;
            }

            var phase = PhaseTime;
            var warningStart = openCycleSeconds - warningSeconds;
            var warningRatio = phase >= warningStart
                ? Mathf.InverseLerp(warningStart, openCycleSeconds, phase)
                : 0f;
            var pulse = 0.5f + (Mathf.Sin(Time.time * (open ? 12f : 8f)) * 0.5f);
            var targetColor = open
                ? Color.Lerp(OpenColor, Color.white, pulse * 0.18f)
                : Color.Lerp(baseColor, WarningColor, warningRatio * (0.6f + pulse * 0.4f));
            wallRenderer.color = targetColor;

            var widthScale = open
                ? Mathf.Lerp(0.12f, 0.32f, pulse)
                : 1f + (warningRatio * pulse * 0.08f);
            transform.localScale = new Vector3(baseScale.x * widthScale, baseScale.y, baseScale.z);
        }
    }
}
