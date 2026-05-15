using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class RunUpgradeSpriteHookupTests
{
    [TestCase("Upgrades/Afterburn", "Sprites/afterburn")]
    [TestCase("Upgrades/FluxLine", "Sprites/flux-line")]
    [TestCase("Upgrades/LuckyCircuit", "Sprites/lucky-circuit")]
    [TestCase("Upgrades/NeonInsurance", "Sprites/neon-insurance")]
    [TestCase("Upgrades/RepairStock", "Sprites/repair-stock")]
    [TestCase("Upgrades/SplitServe", "Sprites/split-serve")]
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
