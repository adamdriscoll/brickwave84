using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutUpgradeDraftServiceTests
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
    public void GenerateDraftIsDeterministicForSameRunStateAndSeed()
    {
        var upgrades = new List<RunUpgradeDefinition>
        {
            CreateUpgrade("wide-loader", draftWeight: 1f),
            CreateUpgrade("afterburn-coil", draftWeight: 3f),
            CreateUpgrade("lucky-circuit", draftWeight: 2f),
            CreateUpgrade("split-serve", draftWeight: 1.5f),
        };
        var service = new BreakoutUpgradeDraftService(upgrades);
        var runSettings = CreateRunSettings(RunScoringMode.Classic);
        var firstRunState = new BreakoutRunState();
        var secondRunState = new BreakoutRunState();
        firstRunState.RegisterLevelClear();
        secondRunState.RegisterLevelClear();

        var firstDraft = service.GenerateDraft(firstRunState, runSettings, runSeed: 8675309, levelIndex: 2, offerCount: 3);
        var secondDraft = service.GenerateDraft(secondRunState, runSettings, runSeed: 8675309, levelIndex: 2, offerCount: 3);

        Assert.That(secondDraft, Is.EqualTo(firstDraft));
    }

    [Test]
    public void GenerateDraftFiltersHighScoreLivesMaxStacksAndConflicts()
    {
        var maxed = CreateUpgrade("maxed", maxStacks: 1);
        var lifeBonus = CreateUpgrade("repair-stock", bonusLives: 1);
        var blocked = CreateUpgrade("blocked");
        var keeper = CreateUpgrade("afterburn-coil", ballSpeedMultiplier: 1.08f);
        var chosenBlocker = CreateUpgrade("chosen-blocker", excludedUpgradeIds: new[] { "blocked" });
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(new List<RunUpgradeDefinition>
        {
            maxed,
            lifeBonus,
            blocked,
            keeper,
        });

        runState.SetPendingDraftOffers(new[] { maxed });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);
        runState.SetPendingDraftOffers(new[] { chosenBlocker });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        var draft = service.GenerateDraft(runState, CreateRunSettings(RunScoringMode.HighScore), runSeed: 4242, levelIndex: 0, offerCount: 3);

        Assert.That(draft, Is.EqualTo(new[] { keeper }));
    }

    [Test]
    public void GenerateDraftReturnsEmptyWhenNoOffersCanBeMade()
    {
        var maxed = CreateUpgrade("maxed", maxStacks: 1);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(new List<RunUpgradeDefinition> { maxed });

        runState.SetPendingDraftOffers(new[] { maxed });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        var draft = service.GenerateDraft(runState, CreateRunSettings(RunScoringMode.Classic), runSeed: 11, levelIndex: 0, offerCount: 3);

        Assert.That(draft, Is.Empty);
    }

    private static RunSettings CreateRunSettings(RunScoringMode scoringMode)
    {
        return new RunSettings(
            1234,
            RunDifficultyPreset.Standard,
            scoringMode,
            3,
            500,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null);
    }

    private RunUpgradeDefinition CreateUpgrade(
        string upgradeId,
        int maxStacks = 3,
        float draftWeight = 1f,
        string[] excludedUpgradeIds = null,
        float ballSpeedMultiplier = 1f,
        int bonusLives = 0)
    {
        var upgrade = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        runtimeObjects.Add(upgrade);
        SetPrivateField(upgrade, "upgradeId", upgradeId);
        SetPrivateField(upgrade, "displayName", upgradeId);
        SetPrivateField(upgrade, "hudLabel", upgradeId.ToUpperInvariant());
        SetPrivateField(upgrade, "draftWeight", draftWeight);
        SetPrivateField(upgrade, "maxStacks", maxStacks);
        SetPrivateField(upgrade, "excludedUpgradeIds", excludedUpgradeIds ?? Array.Empty<string>());
        SetPrivateField(upgrade, "paddleWidthMultiplier", 1f);
        SetPrivateField(upgrade, "ballSpeedMultiplier", ballSpeedMultiplier);
        SetPrivateField(upgrade, "dropChanceMultiplier", 1f);
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
