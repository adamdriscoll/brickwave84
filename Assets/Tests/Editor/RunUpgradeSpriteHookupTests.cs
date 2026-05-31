using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class RunUpgradeSpriteHookupTests
{
    [TestCase("Upgrades/Afterburn", "Sprites/afterburn")]
    [TestCase("Upgrades/BrickMagnet", "Sprites/brick-magnet")]
    [TestCase("Upgrades/CrowdControl", "Sprites/signal-drift")]
    [TestCase("Upgrades/FluxLine", "Sprites/flux-line")]
    [TestCase("Upgrades/FreeToken", "Sprites/brick-missile-drop")]
    [TestCase("Upgrades/HotShrapnel", "Sprites/hot-shrapnel")]
    [TestCase("Upgrades/LuckyCircuit", "Sprites/lucky-circuit")]
    [TestCase("Upgrades/NeonInsurance", "Sprites/neon-insurance")]
    [TestCase("Upgrades/RepairStock", "Sprites/repair-stock")]
    [TestCase("Upgrades/SplitServe", "Sprites/split-serve")]
    [TestCase("Upgrades/SpareFuse", "Sprites/spare-fuse")]
    [TestCase("Upgrades/TiltWarning", "Sprites/tilt-warning")]
    [TestCase("Upgrades/WarpHandle", "Sprites/warp-handle")]
    [TestCase("Upgrades/WideLoader", "Sprites/wide-loader")]
    public void UpgradeAssetResolvesImportedSprite(string upgradeAssetPath, string spriteResourcePath)
    {
        var definition = Resources.Load<RunUpgradeDefinition>(upgradeAssetPath);
        Assert.That(definition, Is.Not.Null, $"Missing upgrade definition at Resources/{upgradeAssetPath}.");

        Assert.That(definition.ResolveIconSpriteResourcePath(), Is.EqualTo(spriteResourcePath));

        var sprite = Resources.Load<Sprite>(spriteResourcePath);
        Assert.That(sprite, Is.Not.Null, $"Missing upgrade sprite at Resources/{spriteResourcePath}.");
    }
}
