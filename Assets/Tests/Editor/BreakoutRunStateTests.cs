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
            wavyPaddleStrength: 0.35f,
            extraBallsPerServe: 1);

        runState.SetPendingDraftOffers(new[] { upgrade });

        var applied = runState.TryApplyPendingDraftOffer(0, out var appliedUpgrade);
        var modifiers = runState.CalculateModifiers();

        Assert.That(applied, Is.True);
        Assert.That(appliedUpgrade, Is.EqualTo(upgrade));
        Assert.That(runState.PendingDraftOffers, Is.Empty);
        Assert.That(runState.ChosenUpgrades, Has.Count.EqualTo(1));
        Assert.That(runState.GetStackCount(" wide-loader "), Is.EqualTo(1));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(1.15f).Within(0.0001f));
        Assert.That(modifiers.BallSpeedMultiplier, Is.EqualTo(1.08f).Within(0.0001f));
        Assert.That(modifiers.DropChanceMultiplier, Is.EqualTo(1.2f).Within(0.0001f));
        Assert.That(modifiers.WavyPaddleStrength, Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(modifiers.ExtraBallsPerServe, Is.EqualTo(1));
    }

    [Test]
    public void CanOfferStopsAtMaxStacks()
    {
        var runState = new BreakoutRunState();
        var upgrade = CreateUpgrade("afterburn", maxStacks: 2, ballSpeedMultiplier: 1.08f);

        runState.SetPendingDraftOffers(new[] { upgrade });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);
        runState.SetPendingDraftOffers(new[] { upgrade });
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

        runState.SetPendingDraftOffers(new[] { chosen });
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
        runState.SetPendingDraftOffers(new[] { upgrade });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        runState.Reset();

        Assert.That(runState.ClearedLevelCount, Is.Zero);
        Assert.That(runState.ChosenUpgrades, Is.Empty);
        Assert.That(runState.PendingDraftOffers, Is.Empty);
        Assert.That(runState.GetStackCount(upgrade), Is.Zero);
        Assert.That(runState.HasActiveBuild, Is.False);
    }

    private RunUpgradeDefinition CreateUpgrade(
        string upgradeId,
        int maxStacks = 3,
        string[] excludedUpgradeIds = null,
        float paddleWidthMultiplier = 1f,
        float ballSpeedMultiplier = 1f,
        float dropChanceMultiplier = 1f,
        float wavyPaddleStrength = 0f,
        int extraBallsPerServe = 0,
        int bonusLives = 0)
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
        SetPrivateField(upgrade, "wavyPaddleStrength", wavyPaddleStrength);
        SetPrivateField(upgrade, "extraBallsPerServe", extraBallsPerServe);
        SetPrivateField(upgrade, "bonusLives", bonusLives);
        return upgrade;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
