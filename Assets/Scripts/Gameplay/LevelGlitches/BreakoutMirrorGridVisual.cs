using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutMirrorGridVisual : MonoBehaviour
    {
        private const float Duration = 0.92f;
        private const float BeamWidth = 0.08f;
        private const int GridLineCount = 7;

        private SpriteRenderer centerBeamRenderer;
        private SpriteRenderer flashRenderer;
        private SpriteRenderer leftScanRenderer;
        private SpriteRenderer rightScanRenderer;
        private SpriteRenderer[] gridLineRenderers;
        private Transform centerBeamTransform;
        private Transform flashTransform;
        private Transform leftScanTransform;
        private Transform rightScanTransform;
        private Transform[] gridLineTransforms;
        private Vector2 playfieldSize;
        private float elapsed;

        public void Configure(Sprite squareSprite, Material additiveMaterial, Rect playfieldBounds)
        {
            playfieldSize = new Vector2(
                Mathf.Max(1f, playfieldBounds.width),
                Mathf.Max(1f, playfieldBounds.height));
            transform.position = playfieldBounds.center;

            flashRenderer = CreateLayer(
                "Mirror Flash",
                squareSprite,
                additiveMaterial,
                transform,
                26,
                new Color(0.72f, 0.62f, 1f, 0.28f),
                playfieldSize,
                Vector2.zero,
                out flashTransform);
            centerBeamRenderer = CreateLayer(
                "Mirror Beam",
                squareSprite,
                additiveMaterial,
                transform,
                31,
                new Color(0.98f, 1f, 1f, 0.92f),
                new Vector2(BeamWidth, playfieldSize.y),
                Vector2.zero,
                out centerBeamTransform);
            leftScanRenderer = CreateLayer(
                "Left Scan",
                squareSprite,
                additiveMaterial,
                transform,
                30,
                new Color(0.03f, 0.93f, 0.98f, 0.82f),
                new Vector2(BeamWidth * 1.4f, playfieldSize.y),
                Vector2.zero,
                out leftScanTransform);
            rightScanRenderer = CreateLayer(
                "Right Scan",
                squareSprite,
                additiveMaterial,
                transform,
                30,
                new Color(1f, 0.22f, 0.84f, 0.78f),
                new Vector2(BeamWidth * 1.4f, playfieldSize.y),
                Vector2.zero,
                out rightScanTransform);

            gridLineRenderers = new SpriteRenderer[GridLineCount];
            gridLineTransforms = new Transform[GridLineCount];

            for (var index = 0; index < GridLineCount; index++)
            {
                var ratio = (index + 1f) / (GridLineCount + 1f);
                var y = Mathf.Lerp(-playfieldSize.y * 0.5f, playfieldSize.y * 0.5f, ratio);
                gridLineRenderers[index] = CreateLayer(
                    $"Grid Trace {index + 1:00}",
                    squareSprite,
                    additiveMaterial,
                    transform,
                    27,
                    index % 2 == 0
                        ? new Color(0.03f, 0.93f, 0.98f, 0.46f)
                        : new Color(1f, 0.22f, 0.84f, 0.42f),
                    new Vector2(playfieldSize.x, 0.035f),
                    new Vector2(0f, y),
                    out gridLineTransforms[index]);
            }

            UpdateVisual(0f);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / Duration);
            UpdateVisual(progress);

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateVisual(float progress)
        {
            var scanProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.72f));
            var flashProgress = Mathf.Clamp01(progress / 0.34f);
            var fade = 1f - Mathf.SmoothStep(0.58f, 1f, progress);
            var pulse = Mathf.Sin(progress * Mathf.PI * 4.5f) * (1f - progress);
            var scanTravel = Mathf.Max(0f, (playfieldSize.x * 0.5f) - BeamWidth);

            if (flashTransform != null)
            {
                var flashScale = Mathf.Lerp(0.14f, 1.04f, Mathf.SmoothStep(0f, 1f, flashProgress));
                flashTransform.localScale = new Vector3(playfieldSize.x * flashScale, playfieldSize.y, 1f);
            }

            if (centerBeamTransform != null)
            {
                var beamPulse = 1f + (Mathf.Abs(pulse) * 1.8f);
                centerBeamTransform.localScale = new Vector3(BeamWidth * beamPulse, playfieldSize.y, 1f);
            }

            if (leftScanTransform != null)
            {
                leftScanTransform.localPosition = new Vector2(-scanTravel * scanProgress, 0f);
            }

            if (rightScanTransform != null)
            {
                rightScanTransform.localPosition = new Vector2(scanTravel * scanProgress, 0f);
            }

            for (var index = 0; index < gridLineTransforms.Length; index++)
            {
                var lineTransform = gridLineTransforms[index];

                if (lineTransform == null)
                {
                    continue;
                }

                var stagger = index / (float)Mathf.Max(1, gridLineTransforms.Length - 1);
                var lineGrow = Mathf.Clamp01((progress * 1.55f) - (stagger * 0.18f));
                var lineFade = 1f - Mathf.SmoothStep(0.46f + (stagger * 0.12f), 1f, progress);
                lineTransform.localScale = new Vector3(
                    playfieldSize.x * Mathf.SmoothStep(0f, 1f, lineGrow),
                    0.035f,
                    1f);
                SetAlpha(gridLineRenderers[index], Mathf.Lerp(0.12f, 0.58f, lineGrow) * lineFade);
            }

            SetAlpha(flashRenderer, Mathf.Lerp(0.3f, 0f, flashProgress) * fade);
            SetAlpha(centerBeamRenderer, Mathf.Lerp(0.95f, 0.08f, progress) + (Mathf.Abs(pulse) * 0.22f));
            SetAlpha(leftScanRenderer, Mathf.Lerp(0.84f, 0.05f, scanProgress) * fade);
            SetAlpha(rightScanRenderer, Mathf.Lerp(0.8f, 0.05f, scanProgress) * fade);
        }

        private static SpriteRenderer CreateLayer(
            string layerName,
            Sprite sprite,
            Material material,
            Transform parent,
            int sortingOrder,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            out Transform layerTransform)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(parent, false);
            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            layerTransform.localPosition = localPosition;
            layerTransform.localRotation = Quaternion.identity;
            layerTransform.localScale = new Vector3(scale.x, scale.y, 1f);
            return renderer;
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            if (renderer == null)
            {
                return;
            }

            var color = renderer.color;
            color.a = Mathf.Clamp01(alpha);
            renderer.color = color;
        }
    }
}
