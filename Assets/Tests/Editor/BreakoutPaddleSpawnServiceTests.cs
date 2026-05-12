using NUnit.Framework;
using UnityEngine;

namespace GetBricked.Gameplay.Tests
{
    public sealed class BreakoutPaddleSpawnServiceTests
    {
        [Test]
        public void PaddleRendersAboveGameplayActors()
        {
            var root = new GameObject("Paddle Test Root").transform;
            var sprite = CreateSprite();
            var service = new BreakoutPaddleSpawnService(
                null,
                root,
                new Vector2(2.1f, 0.74f),
                Color.white,
                sprite,
                null,
                null);

            var spawn = service.CreatePaddle(12f, -5f, 5f, -4f);

            Assert.That(spawn.SpriteRenderer.sortingOrder, Is.GreaterThan(14));
            Assert.That(spawn.SpriteRenderer.enabled, Is.True);
            Assert.That(spawn.SpriteRenderer.sprite, Is.Not.Null);
            Assert.That(spawn.SpriteRenderer.color.a, Is.GreaterThan(0.9f));
            Assert.That(spawn.SpriteRenderer.bounds.size.x, Is.GreaterThan(1f));
            Assert.That(spawn.SpriteRenderer.bounds.size.y, Is.GreaterThan(0.25f));

            Object.DestroyImmediate(root.gameObject);
            Object.DestroyImmediate(sprite);
        }

        private static Sprite CreateSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }
    }
}
