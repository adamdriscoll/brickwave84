using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    internal sealed class BreakoutLaserRainLaneVisual : MonoBehaviour
    {
        private const int WarningSortingOrder = 17;
        private const int BeamSortingOrder = 19;
        private const float MinimumDurationSeconds = 0.03f;

        private LineRenderer warningRenderer;
        private LineRenderer beamRenderer;
        private LineRenderer glowRenderer;
        private Vector2 laneBottom;
        private Vector2 laneTop;
        private Color warningColor = Color.white;
        private Color coreColor = Color.white;
        private Color glowColor = Color.white;
        private float laneWidth = 0.36f;
        private float warningDurationSeconds = 0.46f;
        private float beamDurationSeconds = 0.2f;
        private float ageSeconds;

        public void Configure(
            float laneWorldX,
            float bottomY,
            float topY,
            float visualWidth,
            float warningDuration,
            float beamDuration,
            Material material,
            Color resolvedWarningColor,
            Color resolvedCoreColor,
            Color resolvedGlowColor)
        {
            laneBottom = new Vector2(laneWorldX, bottomY);
            laneTop = new Vector2(laneWorldX, topY);
            laneWidth = Mathf.Max(0.06f, visualWidth);
            warningDurationSeconds = Mathf.Max(MinimumDurationSeconds, warningDuration);
            beamDurationSeconds = Mathf.Max(MinimumDurationSeconds, beamDuration);
            warningColor = resolvedWarningColor;
            coreColor = resolvedCoreColor;
            glowColor = resolvedGlowColor;
            warningRenderer = GetComponent<LineRenderer>();
            glowRenderer = CreateChildRenderer("Glow");
            beamRenderer = CreateChildRenderer("Beam");

            ConfigureRenderer(warningRenderer, material, laneWidth, WarningSortingOrder);
            ConfigureRenderer(glowRenderer, material, laneWidth * 1.95f, BeamSortingOrder - 1);
            ConfigureRenderer(beamRenderer, material, laneWidth * 0.42f, BeamSortingOrder);
            SetLanePositions(warningRenderer);
            SetLanePositions(glowRenderer);
            SetLanePositions(beamRenderer);
            RefreshVisuals();
        }

        private void Update()
        {
            ageSeconds += Time.deltaTime;

            if (ageSeconds >= warningDurationSeconds + beamDurationSeconds)
            {
                Destroy(gameObject);
                return;
            }

            RefreshVisuals();
        }

        private LineRenderer CreateChildRenderer(string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.AddComponent<LineRenderer>();
        }

        private static void ConfigureRenderer(LineRenderer renderer, Material material, float width, int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.positionCount = 2;
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 3;
            renderer.numCornerVertices = 1;
            renderer.startWidth = width;
            renderer.endWidth = width;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        private void SetLanePositions(LineRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.SetPosition(0, laneBottom);
            renderer.SetPosition(1, laneTop);
        }

        private void RefreshVisuals()
        {
            var warningProgress = Mathf.Clamp01(ageSeconds / warningDurationSeconds);
            var beamProgress = Mathf.Clamp01((ageSeconds - warningDurationSeconds) / beamDurationSeconds);
            var isBeamActive = ageSeconds >= warningDurationSeconds;
            var warningPulse = 0.45f + (Mathf.Sin(ageSeconds * 34f) * 0.16f);

            SetRendererColor(
                warningRenderer,
                warningColor,
                isBeamActive ? 0f : Mathf.Lerp(0.18f, 0.62f, warningProgress) + warningPulse);
            SetRendererColor(glowRenderer, glowColor, isBeamActive ? (1f - beamProgress) * 0.44f : 0f);
            SetRendererColor(beamRenderer, coreColor, isBeamActive ? (1f - beamProgress) * 0.95f : 0f);
        }

        private static void SetRendererColor(LineRenderer renderer, Color color, float alpha)
        {
            if (renderer == null)
            {
                return;
            }

            var resolvedAlpha = Mathf.Clamp01(alpha);
            color.a = resolvedAlpha;
            renderer.startColor = color;
            renderer.endColor = color;
        }
    }
}
