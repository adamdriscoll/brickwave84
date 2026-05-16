using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRunStateTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private readonly List<UnityEngine.Object> runtimeObjects = new List<UnityEngine.Object>();

    [TearDown]
    public void TearDown()
    {
        for (var index = runtimeObjects.Count - 1; index >= 0; index--)
        {
            if (runtimeObjects[index] != null)
            {
                UnityEngine.Object.DestroyImmediate(runtimeObjects[index]);
            }
        }

        runtimeObjects.Clear();
    }

    [Test]
    public void TryApplyPendingDraftOfferAppliesModifiersAndClearsOffers()
    {
        var runState = new BreakoutRunState();
        var upgrade = CreateUpgrade(
            "wide-loader",
            paddleWidthMultiplier: 1.15f,
            ballSpeedMultiplier: 1.08f,
            dropChanceMultiplier: 1.2f,
            pickupFallSpeedMultiplier: 0.84f,
            wavyPaddleStrength: 0.35f,
            brickMagnetStrength: 0.18f,
            specialBrickEffectMultiplier: 1.25f,
            extraBallsPerServe: 1,
            tiltWarningSavesPerLevel: 1);

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(upgrade) });

        var applied = runState.TryApplyPendingDraftOffer(0, out var appliedOffer);
        var modifiers = runState.CalculateModifiers();

        Assert.That(applied, Is.True);
        Assert.That(appliedOffer.UpgradeDefinition, Is.EqualTo(upgrade));
        Assert.That(runState.PendingDraftOffers, Is.Empty);
        Assert.That(runState.ChosenUpgrades, Has.Count.EqualTo(1));
        Assert.That(runState.GetStackCount(" wide-loader "), Is.EqualTo(1));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(1.15f).Within(0.0001f));
        Assert.That(modifiers.BallSpeedMultiplier, Is.EqualTo(1.08f).Within(0.0001f));
        Assert.That(modifiers.DropChanceMultiplier, Is.EqualTo(1.2f).Within(0.0001f));
        Assert.That(modifiers.PickupFallSpeedMultiplier, Is.EqualTo(0.84f).Within(0.0001f));
        Assert.That(modifiers.WavyPaddleStrength, Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(modifiers.BrickMagnetStrength, Is.EqualTo(0.18f).Within(0.0001f));
        Assert.That(modifiers.SpecialBrickEffectMultiplier, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(modifiers.ExtraBallsPerServe, Is.EqualTo(1));
        Assert.That(modifiers.TiltWarningSavesPerLevel, Is.EqualTo(1));
    }

    [Test]
    public void CanOfferStopsAtMaxStacks()
    {
        var runState = new BreakoutRunState();
        var upgrade = CreateUpgrade("afterburn", maxStacks: 2, ballSpeedMultiplier: 1.08f);

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(upgrade) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);
        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(upgrade) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        Assert.That(runState.GetStackCount(upgrade), Is.EqualTo(2));
        Assert.That(runState.CanOffer(upgrade), Is.False);
    }

    [Test]
    public void CanOfferRejectsConflictingUpgradeIdsCaseInsensitively()
    {
        var runState = new BreakoutRunState();
        var chosen = CreateUpgrade("wide-loader", excludedUpgradeIds: new[] { "flux-line" });
        var conflict = CreateUpgrade("FLUX-LINE");

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(chosen) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        Assert.That(runState.HasConflict(conflict), Is.True);
        Assert.That(runState.CanOffer(conflict), Is.False);
    }

    [Test]
    public void ResetClearsBuildDraftsStacksAndProgress()
    {
        var runState = new BreakoutRunState();
        var upgrade = CreateUpgrade("repair-stock", bonusLives: 1);

        runState.RegisterLevelClear();
        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(upgrade) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        runState.Reset();

        Assert.That(runState.ClearedLevelCount, Is.Zero);
        Assert.That(runState.ChosenUpgrades, Is.Empty);
        Assert.That(runState.PendingDraftOffers, Is.Empty);
        Assert.That(runState.GetStackCount(upgrade), Is.Zero);
        Assert.That(runState.HasActiveBuild, Is.False);
    }

    [Test]
    public void DropUnlockOffersAddRunLocalDropWithoutCountingStartingPoolAsBuild()
    {
        var runState = new BreakoutRunState();
        var startingDrop = CreatePowerUp("Wide Paddle", "large_paddle", beneficial: true);
        var draftedDrop = CreatePowerUp("Laser Paddle", "laser_paddle", beneficial: true);

        runState.SetInitialDropUnlocks(new[] { startingDrop });

        Assert.That(runState.IsDropUnlocked(startingDrop), Is.True);
        Assert.That(runState.HasActiveBuild, Is.False);
        Assert.That(runState.CanOffer(BreakoutRunDraftOffer.FromDropUnlock(startingDrop)), Is.False);
        Assert.That(runState.CanOffer(BreakoutRunDraftOffer.FromDropUnlock(draftedDrop)), Is.True);

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromDropUnlock(draftedDrop) });

        Assert.That(runState.TryApplyPendingDraftOffer(0, out var appliedOffer), Is.True);
        Assert.That(appliedOffer.Kind, Is.EqualTo(BreakoutRunDraftOfferKind.DropUnlock));
        Assert.That(runState.IsDropUnlocked(draftedDrop), Is.True);
        Assert.That(runState.ChosenDropUnlocks, Is.EqualTo(new[] { draftedDrop }));
        Assert.That(runState.HasActiveBuild, Is.True);
    }

    private RunUpgradeDefinition CreateUpgrade(
        string upgradeId,
        int maxStacks = 3,
        string[] excludedUpgradeIds = null,
        float paddleWidthMultiplier = 1f,
        float ballSpeedMultiplier = 1f,
        float dropChanceMultiplier = 1f,
        float pickupFallSpeedMultiplier = 1f,
        float wavyPaddleStrength = 0f,
        float brickMagnetStrength = 0f,
        float specialBrickEffectMultiplier = 1f,
        int extraBallsPerServe = 0,
        int bonusLives = 0,
        int tiltWarningSavesPerLevel = 0)
    {
        var upgrade = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        runtimeObjects.Add(upgrade);
        SetPrivateField(upgrade, "upgradeId", upgradeId);
        SetPrivateField(upgrade, "displayName", upgradeId);
        SetPrivateField(upgrade, "hudLabel", upgradeId.ToUpperInvariant());
        SetPrivateField(upgrade, "draftWeight", 1f);
        SetPrivateField(upgrade, "maxStacks", maxStacks);
        SetPrivateField(upgrade, "excludedUpgradeIds", excludedUpgradeIds ?? Array.Empty<string>());
        SetPrivateField(upgrade, "paddleWidthMultiplier", paddleWidthMultiplier);
        SetPrivateField(upgrade, "ballSpeedMultiplier", ballSpeedMultiplier);
        SetPrivateField(upgrade, "dropChanceMultiplier", dropChanceMultiplier);
        SetPrivateField(upgrade, "pickupFallSpeedMultiplier", pickupFallSpeedMultiplier);
        SetPrivateField(upgrade, "wavyPaddleStrength", wavyPaddleStrength);
        SetPrivateField(upgrade, "brickMagnetStrength", brickMagnetStrength);
        SetPrivateField(upgrade, "specialBrickEffectMultiplier", specialBrickEffectMultiplier);
        SetPrivateField(upgrade, "extraBallsPerServe", extraBallsPerServe);
        SetPrivateField(upgrade, "bonusLives", bonusLives);
        SetPrivateField(upgrade, "tiltWarningSavesPerLevel", tiltWarningSavesPerLevel);
        return upgrade;
    }

    private PowerUpDefinition CreatePowerUp(string displayName, string powerUpId, bool beneficial)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        runtimeObjects.Add(powerUp);
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "powerUpId", powerUpId);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "durationSeconds", 10f);
        SetPrivateField(powerUp, "scalar", 1f);
        return powerUp;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
