using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutGravityPocketVisual : MonoBehaviour
    {
        private const float PulseRate = 3.7f;
        private const float RingRotationRate = 58f;

        private SpriteRenderer coreRenderer;
        private SpriteRenderer pullFieldRenderer;
        private SpriteRenderer innerRingRenderer;
        private SpriteRenderer outerRingRenderer;
        private SpriteRenderer sweepRenderer;
        private Transform coreTransform;
        private Transform pullFieldTransform;
        private Transform innerRingTransform;
        private Transform outerRingTransform;
        private Transform sweepTransform;
        private Rect movementBounds;
        private Vector2 homePosition;
        private Vector2 driftAmplitude;
        private float radius = 1f;
        private float phase;
        private float driftSpeed = 0.24f;
        private float pullDirectionSign = 1f;

        public void Configure(
            Sprite circleSprite,
            Sprite squareSprite,
            Material unlitMaterial,
            Material additiveMaterial,
            float pocketRadius,
            Rect driftBounds,
            float pocketDriftSpeed,
            float pocketDriftPhase)
        {
            radius = Mathf.Max(0.5f, pocketRadius);
            movementBounds = driftBounds;
            homePosition = ClampToBounds(transform.position, movementBounds);
            driftAmplitude = ResolveDriftAmplitude(homePosition, movementBounds, radius);
            driftSpeed = Mathf.Max(0.01f, pocketDriftSpeed);
            phase = pocketDriftPhase;
            transform.position = homePosition;

            pullFieldRenderer = CreateLayer(
                "Pocket Field",
                circleSprite,
                additiveMaterial,
                16,
                new Color(0.03f, 0.93f, 0.98f, 0.18f),
                Vector2.one * radius * 2f,
                Vector2.zero,
                out pullFieldTransform);
            outerRingRenderer = CreateLayer(
                "Outer Ring",
                circleSprite,
                additiveMaterial,
                20,
                new Color(1f, 0.22f, 0.84f, 0.64f),
                Vector2.one * radius * 0.9f,
                Vector2.zero,
                out outerRingTransform);
            innerRingRenderer = CreateLayer(
                "Inner Ring",
                circleSprite,
                additiveMaterial,
                22,
                new Color(0.98f, 1f, 1f, 0.76f),
                Vector2.one * radius * 0.46f,
                Vector2.zero,
                out innerRingTransform);
            coreRenderer = CreateLayer(
                "Dark Pocket",
                circleSprite,
                unlitMaterial,
                21,
                new Color(0.018f, 0.006f, 0.032f, 0.94f),
                Vector2.one * radius * 0.34f,
                Vector2.zero,
                out coreTransform);
            sweepRenderer = CreateLayer(
                "Orbit Sweep",
                squareSprite,
                additiveMaterial,
                23,
                new Color(0.03f, 0.93f, 0.98f, 0.58f),
                new Vector2(radius * 0.1f, radius * 1.05f),
                Vector2.zero,
                out sweepTransform);

            UpdateVisuals(0f);
        }

        public void SetPullDirectionSign(float directionSign)
        {
            pullDirectionSign = Mathf.Sign(Mathf.Approximately(directionSign, 0f) ? 1f : directionSign);
            UpdateVisuals(Time.time);
        }

        private void Update()
        {
            Tick(Time.time);
        }

        public void Tick(float time)
        {
            UpdateDrift(time);
            UpdateVisuals(time);
        }

        private void UpdateDrift(float time)
        {
            if (movementBounds.width <= 0.001f || movementBounds.height <= 0.001f)
            {
                return;
            }

            var driftTime = time * driftSpeed;
            var offset = new Vector2(
                Mathf.Sin(driftTime + phase) * driftAmplitude.x,
                Mathf.Sin((driftTime * 1.37f) + phase + 1.41f) * driftAmplitude.y);
            transform.position = ClampToBounds(homePosition + offset, movementBounds);
        }

        private void UpdateVisuals(float time)
        {
            var pulse = (Mathf.Sin((time * PulseRate) + phase) + 1f) * 0.5f;
            var smoothPulse = Mathf.SmoothStep(0f, 1f, pulse);

            ApplyScale(pullFieldTransform, Vector2.one * radius * 2f, Mathf.Lerp(0.92f, 1.05f, smoothPulse));
            ApplyScale(outerRingTransform, Vector2.one * radius * 0.9f, Mathf.Lerp(1.04f, 0.94f, smoothPulse));
            ApplyScale(innerRingTransform, Vector2.one * radius * 0.46f, Mathf.Lerp(0.86f, 1.12f, smoothPulse));
            ApplyScale(coreTransform, Vector2.one * radius * 0.34f, Mathf.Lerp(0.92f, 1.04f, smoothPulse));

            if (outerRingTransform != null)
            {
                outerRingTransform.localRotation = Quaternion.Euler(0f, 0f, -time * RingRotationRate * pullDirectionSign);
            }

            if (innerRingTransform != null)
            {
                innerRingTransform.localRotation = Quaternion.Euler(0f, 0f, time * RingRotationRate * 1.6f * pullDirectionSign);
            }

            if (sweepTransform != null)
            {
                sweepTransform.localRotation = Quaternion.Euler(0f, 0f, time * RingRotationRate * 2.2f * pullDirectionSign);
                sweepTransform.localScale = new Vector3(radius * Mathf.Lerp(0.07f, 0.13f, smoothPulse), radius * 1.05f, 1f);
            }

            var repelling = pullDirectionSign < 0f;
            SetTint(pullFieldRenderer, repelling ? new Color(1f, 0.22f, 0.84f, 0.18f) : new Color(0.03f, 0.93f, 0.98f, 0.18f));
            SetTint(outerRingRenderer, repelling ? new Color(0.03f, 0.93f, 0.98f, 0.64f) : new Color(1f, 0.22f, 0.84f, 0.64f));
            SetTint(sweepRenderer, repelling ? new Color(1f, 0.87f, 0.36f, 0.58f) : new Color(0.03f, 0.93f, 0.98f, 0.58f));

            SetAlpha(pullFieldRenderer, Mathf.Lerp(0.1f, 0.26f, smoothPulse));
            SetAlpha(outerRingRenderer, Mathf.Lerp(0.34f, 0.72f, 1f - smoothPulse));
            SetAlpha(innerRingRenderer, Mathf.Lerp(0.48f, 0.9f, smoothPulse));
            SetAlpha(coreRenderer, Mathf.Lerp(0.82f, 0.98f, smoothPulse));
            SetAlpha(sweepRenderer, Mathf.Lerp(0.24f, 0.68f, smoothPulse));
        }

        private SpriteRenderer CreateLayer(
            string layerName,
            Sprite sprite,
            Material material,
            int sortingOrder,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            out Transform layerTransform)
        {
            var layer = new GameObject(layerName);
            layerTransform = layer.transform;
            layerTransform.SetParent(transform, false);
            layerTransform.localPosition = localPosition;
            layerTransform.localRotation = Quaternion.identity;
            layerTransform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private static void ApplyScale(Transform target, Vector2 baseScale, float multiplier)
        {
            if (target != null)
            {
                target.localScale = new Vector3(baseScale.x * multiplier, baseScale.y * multiplier, 1f);
            }
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

        private static void SetTint(SpriteRenderer renderer, Color tint)
        {
            if (renderer == null)
            {
                return;
            }

            var color = renderer.color;
            tint.a = color.a;
            renderer.color = tint;
        }

        private static Vector2 ResolveDriftAmplitude(Vector2 origin, Rect bounds, float pocketRadius)
        {
            var edgePadding = Mathf.Max(0.45f, pocketRadius * 0.32f);
            var leftRoom = Mathf.Max(0f, origin.x - (bounds.xMin + edgePadding));
            var rightRoom = Mathf.Max(0f, (bounds.xMax - edgePadding) - origin.x);
            var bottomRoom = Mathf.Max(0f, origin.y - (bounds.yMin + edgePadding));
            var topRoom = Mathf.Max(0f, (bounds.yMax - edgePadding) - origin.y);

            return new Vector2(
                Mathf.Min(1.35f, Mathf.Min(leftRoom, rightRoom)),
                Mathf.Min(1.05f, Mathf.Min(bottomRoom, topRoom)));
        }

        private static Vector2 ClampToBounds(Vector2 position, Rect bounds)
        {
            if (bounds.width <= 0.001f || bounds.height <= 0.001f)
            {
                return position;
            }

            return new Vector2(
                Mathf.Clamp(position.x, bounds.xMin, bounds.xMax),
                Mathf.Clamp(position.y, bounds.yMin, bounds.yMax));
        }
    }
}
