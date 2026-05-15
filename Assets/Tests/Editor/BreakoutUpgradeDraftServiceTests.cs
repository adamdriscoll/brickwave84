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

        Assert.That(secondDraft, Has.Length.EqualTo(firstDraft.Length));

        for (var index = 0; index < firstDraft.Length; index++)
        {
            Assert.That(secondDraft[index].Kind, Is.EqualTo(firstDraft[index].Kind));
            Assert.That(secondDraft[index].OfferId, Is.EqualTo(firstDraft[index].OfferId));
        }
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

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(maxed) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);
        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(chosenBlocker) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        var draft = service.GenerateDraft(runState, CreateRunSettings(RunScoringMode.HighScore), runSeed: 4242, levelIndex: 0, offerCount: 3);

        Assert.That(draft, Has.Length.EqualTo(1));
        Assert.That(draft[0].Kind, Is.EqualTo(BreakoutRunDraftOfferKind.RunUpgrade));
        Assert.That(draft[0].UpgradeDefinition, Is.EqualTo(keeper));
    }

    [Test]
    public void GenerateDraftReturnsEmptyWhenNoOffersCanBeMade()
    {
        var maxed = CreateUpgrade("maxed", maxStacks: 1);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(new List<RunUpgradeDefinition> { maxed });

        runState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(maxed) });
        Assert.That(runState.TryApplyPendingDraftOffer(0, out _), Is.True);

        var draft = service.GenerateDraft(runState, CreateRunSettings(RunScoringMode.Classic), runSeed: 11, levelIndex: 0, offerCount: 3);

        Assert.That(draft, Is.Empty);
    }

    [Test]
    public void RogueDraftOffersHelpfulDropUnlocksAndSkipsAlreadyUnlockedDrops()
    {
        var wide = CreatePowerUp("Wide Paddle", "large_paddle", beneficial: true);
        var laser = CreatePowerUp("Laser Paddle", "laser_paddle", beneficial: true);
        var hazard = CreatePowerUp("Reverse Controls", "reverse_controls", beneficial: false);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(
            new List<RunUpgradeDefinition>(),
            new List<PowerUpDefinition> { wide, laser, hazard });

        runState.SetInitialDropUnlocks(new[] { wide });

        var draft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic, RunGameMode.Rogue),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);

        Assert.That(draft, Has.Length.EqualTo(1));
        Assert.That(draft[0].Kind, Is.EqualTo(BreakoutRunDraftOfferKind.DropUnlock));
        Assert.That(draft[0].DropUnlockDefinition, Is.EqualTo(laser));
    }

    [Test]
    public void RogueDraftOnlyOffersDropsUnlockedForCurrentHeat()
    {
        var common = CreatePowerUp("Common Drop", "common_drop", beneficial: true);
        var rare = CreatePowerUp("Rare Drop", "rare_drop", beneficial: true, BreakoutContentRarity.Rare);
        SetPrivateField(rare, "ladderUnlockIntensityOverride", 9);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(
            new List<RunUpgradeDefinition>(),
            new List<PowerUpDefinition> { common, rare });

        runState.SetInitialDropUnlocks(new[] { common });

        var earlyDraft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic, RunGameMode.Rogue, rogueIntensity: 8),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);

        Assert.That(earlyDraft, Is.Empty);

        var laterDraft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic, RunGameMode.Rogue, rogueIntensity: 9),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);

        Assert.That(laterDraft, Has.Length.EqualTo(1));
        Assert.That(laterDraft[0].DropUnlockDefinition, Is.EqualTo(rare));
    }

    [Test]
    public void RogueDraftHonorsExplicitDropUnlockIntensity()
    {
        var vectorSight = CreatePowerUp("Vector Sight", "vector_sight", beneficial: true);
        SetPrivateField(vectorSight, "ladderUnlockIntensityOverride", 6);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(
            new List<RunUpgradeDefinition>(),
            new List<PowerUpDefinition> { vectorSight });

        var lockedDraft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic, RunGameMode.Rogue, rogueIntensity: 5),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);
        var unlockedDraft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic, RunGameMode.Rogue, rogueIntensity: 6),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);

        Assert.That(lockedDraft, Is.Empty);
        Assert.That(unlockedDraft, Has.Length.EqualTo(1));
        Assert.That(unlockedDraft[0].DropUnlockDefinition, Is.EqualTo(vectorSight));
        Assert.That(vectorSight.LadderUnlockIntensity, Is.EqualTo(6));
    }

    [Test]
    public void CustomGameDraftDoesNotOfferDropUnlocks()
    {
        var laser = CreatePowerUp("Laser Paddle", "laser_paddle", beneficial: true);
        var runState = new BreakoutRunState();
        var service = new BreakoutUpgradeDraftService(
            new List<RunUpgradeDefinition>(),
            new List<PowerUpDefinition> { laser });

        var draft = service.GenerateDraft(
            runState,
            CreateRunSettings(RunScoringMode.Classic),
            runSeed: 123,
            levelIndex: 1,
            offerCount: 3);

        Assert.That(draft, Is.Empty);
    }

    private static RunSettings CreateRunSettings(
        RunScoringMode scoringMode,
        RunGameMode gameMode = RunGameMode.CustomGame,
        int rogueIntensity = 1)
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
            null,
            gameMode,
            rogueIntensity);
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
        SetPrivateField(upgrade, "pickupFallSpeedMultiplier", 1f);
        SetPrivateField(upgrade, "bonusLives", bonusLives);
        return upgrade;
    }

    private PowerUpDefinition CreatePowerUp(
        string displayName,
        string powerUpId,
        bool beneficial,
        BreakoutContentRarity rarity = BreakoutContentRarity.Common)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        runtimeObjects.Add(powerUp);
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "powerUpId", powerUpId);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "rarity", rarity);
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
