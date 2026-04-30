using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class PowerUpSpriteHookupTests
{
    [TestCase("PowerUps/FastBall", "Sprites/fast-ball")]
    [TestCase("PowerUps/SlowBall", "Sprites/slow-ball")]
    [TestCase("PowerUps/MultiBall", "Sprites/multiball")]
    [TestCase("PowerUps/LargePaddle", "Sprites/wide-paddle")]
    [TestCase("PowerUps/SmallPaddle", "Sprites/small-paddle")]
    [TestCase("PowerUps/WavyPaddle", "Sprites/wavy-paddle")]
    public void PickupAssetResolvesImportedSprite(string powerUpAssetPath, string spriteResourcePath)
    {
        var definition = Resources.Load<PowerUpDefinition>(powerUpAssetPath);
        Assert.That(definition, Is.Not.Null, $"Missing pickup definition at Resources/{powerUpAssetPath}.");

        Assert.That(definition.ResolvePickupSpriteResourcePath(), Is.EqualTo(spriteResourcePath));

        var sprite = Resources.Load<Sprite>(spriteResourcePath);
        Assert.That(sprite, Is.Not.Null, $"Missing pickup sprite at Resources/{spriteResourcePath}.");
    }
}
