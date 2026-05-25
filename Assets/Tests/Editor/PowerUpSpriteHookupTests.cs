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
    [TestCase("PowerUps/MondoMulti", "Sprites/mondo-multi")]
    [TestCase("PowerUps/BogusMulti", "Sprites/bogus-multi")]
    [TestCase("PowerUps/LaserGrid", "Sprites/laser-grid")]
    [TestCase("PowerUps/BrickMagnet", "Sprites/brick-magnet")]
    [TestCase("PowerUps/CapsuleMagnet", "Sprites/capsule-magnet")]
    [TestCase("PowerUps/GhostBall", "Sprites/ghost-ball")]
    [TestCase("PowerUps/ScoreSurge", "Sprites/score-surge")]
    [TestCase("PowerUps/PaddleClone", "Sprites/paddle-clone")]
    [TestCase("PowerUps/BrickJammer", "Sprites/brick-jammer")]
    [TestCase("PowerUps/SignalDrift", "Sprites/signal-drift")]
    [TestCase("PowerUps/Blackout", "Sprites/blackout")]
    [TestCase("PowerUps/HotPotatoBall", "Sprites/hot-potato-ball")]
    [TestCase("PowerUps/BoomBall", "Sprites/boom-ball")]
    [TestCase("PowerUps/MegaBall", "Sprites/mega-ball")]
    [TestCase("PowerUps/NeonShield", "Sprites/neon-shield")]
    [TestCase("PowerUps/BogusTape", "Sprites/bogus-multi")]
    [TestCase("PowerUps/VectorSight", "Sprites/vector-sight")]
    [TestCase("PowerUps/MirrorImage", "Sprites/mirror-image")]
    [TestCase("PowerUps/BankBonus", "Sprites/bank-bonus")]
    [TestCase("PowerUps/CleanCatch", "Sprites/clean-catch")]
    [TestCase("PowerUps/MysteryTape", "Sprites/mystery-tape")]
    [TestCase("PowerUps/TiltRail", "Sprites/tilt-rail")]
    [TestCase("PowerUps/BrickMissile", "Sprites/brick-missile-drop")]
    [TestCase("PowerUps/SolarShot", "Sprites/solar-shot")]
    [TestCase("PowerUps/WrapRail", "Sprites/wrap-rail")]
    [TestCase("PowerUps/StaticShoes", "Sprites/static-shoes")]
    [TestCase("PowerUps/JackpotJam", "Sprites/jackpot-jam")]
    [TestCase("PowerUps/MicroSpark", "Sprites/micro-spark")]
    [TestCase("PowerUps/MagnetFlip", "Sprites/magnet-flip")]
    [TestCase("PowerUps/DoubleTap", "Sprites/double-tap")]
    [TestCase("PowerUps/FuseBurst", "Sprites/fuse-burst")]
    [TestCase("PowerUps/OverdriveTape", "Sprites/overdrive-tape")]
    [TestCase("PowerUps/BogusBounce", "Sprites/bogus-multi")]
    [TestCase("PowerUps/CabinetJackpot", "Sprites/cabinet-jackpot")]
    public void PickupAssetResolvesImportedSprite(string powerUpAssetPath, string spriteResourcePath)
    {
        var definition = Resources.Load<PowerUpDefinition>(powerUpAssetPath);
        Assert.That(definition, Is.Not.Null, $"Missing pickup definition at Resources/{powerUpAssetPath}.");

        Assert.That(definition.ResolvePickupSpriteResourcePath(), Is.EqualTo(spriteResourcePath));

        var sprite = Resources.Load<Sprite>(spriteResourcePath);
        Assert.That(sprite, Is.Not.Null, $"Missing pickup sprite at Resources/{spriteResourcePath}.");
    }
}
