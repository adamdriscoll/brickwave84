using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutArenaBounds
    {
        public BreakoutArenaBounds(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public float Left { get; }

        public float Right { get; }

        public float Top { get; }

        public float Bottom { get; }

        public float Width => Mathf.Max(0f, Right - Left);

        public float Height => Mathf.Max(0f, Top - Bottom);
    }

    internal static class BreakoutPlayfieldGeometry
    {
        public const float DefaultMaximumGameplayAspectRatio = 16f / 9f;

        public static BreakoutArenaBounds CalculateArenaBounds(
            float cameraHalfHeight,
            float cameraAspect,
            float playfieldPadding,
            float maximumGameplayAspectRatio)
        {
            var halfHeight = Mathf.Max(0.01f, cameraHalfHeight);
            var aspect = Mathf.Max(0.01f, cameraAspect);
            var cappedAspect = maximumGameplayAspectRatio > 0f
                ? Mathf.Min(aspect, maximumGameplayAspectRatio)
                : aspect;
            var visibleHalfWidth = halfHeight * cappedAspect;
            var padding = Mathf.Max(0f, playfieldPadding);
            var horizontalPadding = Mathf.Min(padding, Mathf.Max(0f, visibleHalfWidth - 0.01f));
            var verticalPadding = Mathf.Min(padding, Mathf.Max(0f, halfHeight - 0.01f));

            return new BreakoutArenaBounds(
                -visibleHalfWidth + horizontalPadding,
                visibleHalfWidth - horizontalPadding,
                halfHeight - verticalPadding,
                -halfHeight + verticalPadding);
        }
    }
}
