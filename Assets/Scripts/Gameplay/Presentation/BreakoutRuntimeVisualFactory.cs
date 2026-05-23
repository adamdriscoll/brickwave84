using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal static class BreakoutRuntimeVisualFactory
    {
        private const string SpriteUnlitMaterialResourcePath = "Materials/RuntimeSpriteUnlit";
        private const string AdditiveSpriteMaterialResourcePath = "Materials/RuntimeSpriteAdditive";
        private const string AdditiveLineMaterialResourcePath = "Materials/RuntimeLineAdditive";
        private static readonly Dictionary<string, Sprite> spriteResourceCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> spriteNameCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> indexedSpriteDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static Sprite LoadSpriteResource(string resourcePath, Sprite fallbackSprite)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return fallbackSprite;
            }

            var loadedSprite = LoadSpriteResource(resourcePath);
            return loadedSprite != null ? loadedSprite : fallbackSprite;
        }

        public static Sprite LoadSpriteResource(string resourcePath)
        {
            var normalizedPath = NormalizeResourcePath(resourcePath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return null;
            }

            if (spriteResourceCache.TryGetValue(normalizedPath, out var cachedSprite))
            {
                return cachedSprite;
            }

            var loadedSprite = Resources.Load<Sprite>(normalizedPath);

            if (loadedSprite == null)
            {
                loadedSprite = ResolveSpriteFromDirectoryIndex(normalizedPath);
            }

            spriteResourceCache[normalizedPath] = loadedSprite;
            return loadedSprite;
        }

        public static Material CreateSpriteUnlitMaterial()
        {
            var includedMaterial = CreateMaterialFromResource(SpriteUnlitMaterialResourcePath, "RuntimeSpriteUnlit");

            if (includedMaterial != null)
            {
                return includedMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

            if (shader == null)
            {
                Debug.LogWarning("Could not find the URP sprite unlit shader. Runtime sprites will use Unity's default material.");
                return null;
            }

            return new Material(shader)
            {
                name = "RuntimeSpriteUnlit",
                hideFlags = HideFlags.DontSave,
            };
        }

        public static Material CreateAdditiveSpriteMaterial()
        {
            var includedMaterial = CreateMaterialFromResource(AdditiveSpriteMaterialResourcePath, "RuntimeSpriteAdditive");

            if (includedMaterial != null)
            {
                includedMaterial.renderQueue = 3000;
                return includedMaterial;
            }

            var shader = Shader.Find("Get Bricked/Sprite Additive");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }

            if (shader == null)
            {
                Debug.LogWarning("Could not find an additive sprite shader. Falling back to a duplicate unlit material.");
                return CreateSpriteUnlitMaterial();
            }

            var material = new Material(shader)
            {
                name = "RuntimeSpriteAdditive",
                hideFlags = HideFlags.DontSave,
                renderQueue = 3000,
            };

            if (shader.name == "Universal Render Pipeline/Particles/Unlit")
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 2f);
                material.SetFloat("_SrcBlend", 5f);
                material.SetFloat("_DstBlend", 1f);
                material.SetFloat("_SrcBlendAlpha", 1f);
                material.SetFloat("_DstBlendAlpha", 1f);
                material.SetFloat("_ZWrite", 0f);
                material.SetColor("_BaseColor", Color.white);
                material.SetColor("_EmissionColor", Color.white * 2f);
            }

            return material;
        }

        public static Material CreateAdditiveLineMaterial()
        {
            var includedMaterial = CreateMaterialFromResource(AdditiveLineMaterialResourcePath, "RuntimeLineAdditive");

            if (includedMaterial != null)
            {
                includedMaterial.renderQueue = 3000;
                return includedMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
            {
                Debug.LogWarning("Could not find the URP particle unlit shader. Laser beams will use the sprite additive material fallback.");
                return CreateAdditiveSpriteMaterial();
            }

            var material = new Material(shader)
            {
                name = "RuntimeLineAdditive",
                hideFlags = HideFlags.DontSave,
                renderQueue = 3000,
            };

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", 1f);
            material.SetFloat("_SrcBlendAlpha", 1f);
            material.SetFloat("_DstBlendAlpha", 1f);
            material.SetFloat("_ZWrite", 0f);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_EmissionColor", Color.white * 2f);
            return material;
        }

        public static Material CreateMaterialFromResource(string resourcePath, string materialName)
        {
            var materialAsset = Resources.Load<Material>(resourcePath);

            if (materialAsset == null)
            {
                return null;
            }

            return new Material(materialAsset)
            {
                name = materialName,
                hideFlags = HideFlags.DontSave,
            };
        }

        private static Sprite ResolveSpriteFromDirectoryIndex(string normalizedPath)
        {
            var slashIndex = normalizedPath.LastIndexOf('/');
            var directory = slashIndex >= 0 ? normalizedPath.Substring(0, slashIndex) : string.Empty;
            var spriteName = slashIndex >= 0 ? normalizedPath.Substring(slashIndex + 1) : normalizedPath;

            IndexSpriteDirectory(directory);

            if (spriteNameCache.TryGetValue(normalizedPath, out var pathMatchedSprite))
            {
                return pathMatchedSprite;
            }

            return spriteNameCache.TryGetValue(spriteName, out var nameMatchedSprite)
                ? nameMatchedSprite
                : null;
        }

        private static void IndexSpriteDirectory(string directory)
        {
            var normalizedDirectory = NormalizeResourcePath(directory);

            if (!indexedSpriteDirectories.Add(normalizedDirectory))
            {
                return;
            }

            var sprites = Resources.LoadAll<Sprite>(normalizedDirectory);

            for (var index = 0; index < sprites.Length; index++)
            {
                var sprite = sprites[index];

                if (sprite == null || string.IsNullOrWhiteSpace(sprite.name))
                {
                    continue;
                }

                var pathKey = string.IsNullOrWhiteSpace(normalizedDirectory)
                    ? sprite.name
                    : $"{normalizedDirectory}/{sprite.name}";
                spriteNameCache[pathKey] = sprite;
                spriteNameCache[sprite.name] = sprite;
            }
        }

        private static string NormalizeResourcePath(string resourcePath)
        {
            var normalizedPath = resourcePath.Trim().Replace('\\', '/');

            if (normalizedPath.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring("Assets/Resources/".Length);
            }

            var extensionIndex = normalizedPath.LastIndexOf('.');

            if (extensionIndex > normalizedPath.LastIndexOf('/'))
            {
                normalizedPath = normalizedPath.Substring(0, extensionIndex);
            }

            return normalizedPath.Trim('/');
        }

        public static Sprite CreateSquareSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateBackgroundHazeSprite()
        {
            const int textureSize = 256;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeBackgroundHazeTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            var radius = textureSize * 0.72f;

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var radial = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), center) / radius);
                    var vertical = Mathf.InverseLerp(0f, textureSize - 1f, y);
                    var alpha = Mathf.Lerp(0.04f, 0.92f, Mathf.Pow(radial, 1.45f));
                    alpha = Mathf.Max(alpha, Mathf.Lerp(0.08f, 0.42f, vertical * 0.85f));
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateBackgroundScanlineSprite()
        {
            const int textureWidth = 8;
            const int textureHeight = 1024;
            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeBackgroundScanlineTexture",
            };

            var pixels = new Color[textureWidth * textureHeight];

            for (var y = 0; y < textureHeight; y++)
            {
                var stripeCycle = y % 4;
                var alpha = stripeCycle == 0
                    ? 0.92f
                    : stripeCycle == 2
                        ? 0.28f
                        : 0.05f;

                for (var x = 0; x < textureWidth; x++)
                {
                    pixels[x + (y * textureWidth)] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateCircleSprite()
        {
            const int textureSize = 64;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeCircleTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var radius = textureSize * 0.5f;
            var center = new Vector2(radius - 0.5f, radius - 0.5f);

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var normalizedDistance = distance / radius;

                    if (normalizedDistance >= 1f)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var alpha = normalizedDistance <= 0.68f
                        ? 1f
                        : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.68f, 1f, normalizedDistance));
                    pixels[index] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateRingSprite()
        {
            const int textureSize = 128;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeRingTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            var radius = textureSize * 0.5f;

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var normalizedDistance = Vector2.Distance(new Vector2(x, y), center) / radius;
                    var ring = 1f - Mathf.Abs(normalizedDistance - 0.68f) / 0.12f;
                    var rim = 1f - Mathf.Abs(normalizedDistance - 0.92f) / 0.055f;
                    var alpha = Mathf.Clamp01(Mathf.Max(ring * 0.9f, rim * 0.42f));
                    pixels[index] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateVortexSprite()
        {
            const int textureSize = 128;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeVortexTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            var radius = textureSize * 0.5f;

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var offset = new Vector2(x, y) - center;
                    var normalizedDistance = offset.magnitude / radius;
                    var index = x + (y * textureSize);

                    if (normalizedDistance >= 1f || normalizedDistance <= 0.14f)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var angle = Mathf.Atan2(offset.y, offset.x);
                    var spiral = Mathf.Sin((angle * 3f) + (normalizedDistance * 15.5f));
                    var spiralAlpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.48f, 1f, spiral));
                    var radialAlpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.42f, normalizedDistance))
                        * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, normalizedDistance)));
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Clamp01(spiralAlpha * radialAlpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        public static Sprite CreateTriangleSprite()
        {
            const int textureSize = 64;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "RuntimeTriangleTexture",
            };

            var pixels = new Color[textureSize * textureSize];
            var halfWidth = (textureSize - 1) * 0.5f;
            var bottomY = textureSize * 0.82f;
            var topY = textureSize * 0.16f;

            for (var y = 0; y < textureSize; y++)
            {
                var verticalProgress = Mathf.InverseLerp(bottomY, topY, y);
                var allowedHalfWidth = Mathf.Lerp(halfWidth * 0.72f, 0f, verticalProgress);

                for (var x = 0; x < textureSize; x++)
                {
                    var index = x + (y * textureSize);
                    var insideVertical = y >= topY && y <= bottomY;
                    var insideHorizontal = Mathf.Abs(x - halfWidth) <= allowedHalfWidth;
                    pixels[index] = insideVertical && insideHorizontal ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }
    }
}
