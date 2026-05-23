using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutSpriteRendererUtility
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static MaterialPropertyBlock propertyBlock;

        public static void ApplyTint(SpriteRenderer spriteRenderer, Color color)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = Color.white;
            propertyBlock ??= new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorPropertyId, color);
            propertyBlock.SetColor(BaseColorPropertyId, color);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        public static Color ResolveTint(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return Color.white;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(propertyBlock);
            var color = propertyBlock.GetColor(ColorPropertyId);
            return color.a > 0.0001f ? color : spriteRenderer.color;
        }

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
