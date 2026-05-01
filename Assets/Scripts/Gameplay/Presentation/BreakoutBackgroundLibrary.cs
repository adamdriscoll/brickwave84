using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutBackgroundLibrary
    {
        private readonly List<Sprite> sprites = new List<Sprite>();
        private readonly List<Texture2D> textures = new List<Texture2D>();
        private readonly List<Sprite> runtimeGeneratedSprites = new List<Sprite>();

        public void LoadFromResources()
        {
            sprites.Clear();
            textures.Clear();

            var spriteAssets = Resources.LoadAll<Sprite>("Backgrounds");

            for (var index = 0; index < spriteAssets.Length; index++)
            {
                if (spriteAssets[index] != null)
                {
                    sprites.Add(spriteAssets[index]);
                }
            }

            sprites.Sort(CompareNamedObjects);

            var textureAssets = Resources.LoadAll<Texture2D>("Backgrounds");

            for (var index = 0; index < textureAssets.Length; index++)
            {
                var texture = textureAssets[index];

                if (texture == null || ContainsNamedObject(sprites, texture.name))
                {
                    continue;
                }

                textures.Add(texture);
            }

            textures.Sort(CompareNamedObjects);
        }

        public Sprite ResolveSpriteForLevel(int levelIndex)
        {
            if (sprites.Count > 0)
            {
                var spriteIndex = Mathf.Abs(levelIndex) % sprites.Count;
                return sprites[spriteIndex];
            }

            if (textures.Count == 0)
            {
                return null;
            }

            var textureIndex = Mathf.Abs(levelIndex) % textures.Count;
            return CreateRuntimeSprite(textures[textureIndex]);
        }

        public void DestroyRuntimeSprites()
        {
            for (var index = runtimeGeneratedSprites.Count - 1; index >= 0; index--)
            {
                if (runtimeGeneratedSprites[index] != null)
                {
                    UnityEngine.Object.Destroy(runtimeGeneratedSprites[index]);
                }
            }

            runtimeGeneratedSprites.Clear();
        }

        private Sprite CreateRuntimeSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            for (var index = 0; index < runtimeGeneratedSprites.Count; index++)
            {
                var existingSprite = runtimeGeneratedSprites[index];

                if (existingSprite != null && string.Equals(existingSprite.name, texture.name, StringComparison.Ordinal))
                {
                    return existingSprite;
                }
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = texture.name;
            runtimeGeneratedSprites.Add(sprite);
            return sprite;
        }

        private static bool ContainsNamedObject<T>(List<T> objects, string candidateName)
            where T : UnityEngine.Object
        {
            for (var index = 0; index < objects.Count; index++)
            {
                if (objects[index] != null && string.Equals(objects[index].name, candidateName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareNamedObjects<T>(T left, T right)
            where T : UnityEngine.Object
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
