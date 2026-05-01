using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutSpriteRendererUtility
    {
        public static void NormalizeScale(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var sprite = spriteRenderer.sprite;

            if (sprite == null)
            {
                spriteRenderer.transform.localScale = Vector3.one;
                return;
            }

            var spriteSize = sprite.bounds.size;
            var scaleX = spriteSize.x > 0.0001f ? 1f / spriteSize.x : 1f;
            var scaleY = spriteSize.y > 0.0001f ? 1f / spriteSize.y : 1f;
            spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
