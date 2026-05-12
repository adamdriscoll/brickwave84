using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRogueRunTests
{
    private const string LastRogueResultKey = "GetBricked.Rogue.LastResult";
    private const string RogueProgressKey = "GetBricked.Rogue.IntensityProgress";
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
        PlayerPrefs.DeleteKey(RogueProgressKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
        PlayerPrefs.DeleteKey(RogueProgressKey);
    }

    [Test]
    public void RunSettingsCanIdentifyRogueMode()
    {
        var settings = new RunSettings(
            4242,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
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
            RunGameMode.Rogue);

        Assert.That(settings.IsRogueMode, Is.True);
        Assert.That(settings.GameModeLabel, Is.EqualTo("Neon Ladder"));
        Assert.That(settings.StartingLives, Is.EqualTo(3));
        Assert.That(settings.ScoringMode, Is.EqualTo(RunScoringMode.Classic));
    }

    [Test]
    public void RogueStageBallSpeedMultiplierRampsAcrossTenStages()
    {
        var firstStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(0);
        var middleStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(4);
        var finalStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(9);

        Assert.That(firstStage, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(middleStage, Is.GreaterThan(firstStage));
        Assert.That(finalStage, Is.EqualTo(1.1f).Within(0.0001f));
    }

    [Test]
    public void RogueIntensityBallSpeedMultiplierRampsAcrossFiftyHeatLevels()
    {
        var firstHeat = BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(1);
        var middleHeat = BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(25);
        var finalHeat = BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(50);

        Assert.That(firstHeat, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(middleHeat, Is.GreaterThan(firstHeat));
        Assert.That(finalHeat, Is.EqualTo(1.18f).Within(0.0001f));
    }

    [Test]
    public void RogueIntensityGaugeColorMovesFromGreenToYellowToRed()
    {
        var low = BreakoutRunProgression.GetRogueIntensityGaugeColor(1);
        var mid = BreakoutRunProgression.GetRogueIntensityGaugeColor(25);
        var high = BreakoutRunProgression.GetRogueIntensityGaugeColor(50);

        Assert.That(low.g, Is.GreaterThan(low.r));
        Assert.That(mid.r, Is.GreaterThan(0.9f));
        Assert.That(mid.g, Is.GreaterThan(0.7f));
        Assert.That(high.r, Is.GreaterThan(high.g));
    }

    [Test]
    public void DeveloperLaunchStateCanTargetStagesAndToggleBuildPicks()
    {
        var state = new BreakoutDeveloperLaunchState();
        var upgrade = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        var drop = ScriptableObject.CreateInstance<PowerUpDefinition>();
        drop.name = "Debug Drop";

        try
        {
            state.AdjustField(BreakoutDeveloperLaunchField.Encounter, 10, null, null);
            var encounter = state.ResolveEncounter();

            Assert.That(encounter.LevelIndex, Is.EqualTo(0));
            Assert.That(encounter.DisplayName, Is.EqualTo("Stage 01"));

            state.AdjustField(BreakoutDeveloperLaunchField.Lives, 20, null, null);
            Assert.That(state.LivesRemaining, Is.EqualTo(9));

            state.AdjustField(BreakoutDeveloperLaunchField.Heat, 60, null, null);
            Assert.That(state.Intensity, Is.EqualTo(BreakoutRunProgression.MaxRogueIntensity));

            state.AdjustField(BreakoutDeveloperLaunchField.Paddle, 1, null, null);
            Assert.That(state.ResolvePaddle().DisplayName, Is.EqualTo("Comet Paddle"));

            state.ToggleCurrentUpgrade(new[] { upgrade });
            state.ToggleCurrentDropUnlock(new[] { drop });

            Assert.That(state.SelectedUpgradeCount, Is.EqualTo(1));
            Assert.That(state.SelectedDropUnlockCount, Is.EqualTo(1));
            Assert.That(state.IsUpgradeSelected(upgrade), Is.True);
            Assert.That(state.IsDropUnlockSelected(drop), Is.True);

            state.ClearBuild();

            Assert.That(state.SelectedUpgradeCount, Is.EqualTo(0));
            Assert.That(state.SelectedDropUnlockCount, Is.EqualTo(0));
            Assert.That(state.Intensity, Is.EqualTo(BreakoutRunProgression.MaxRogueIntensity));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(upgrade);
            UnityEngine.Object.DestroyImmediate(drop);
        }
    }

    [Test]
    public void RoguePaddleCatalogUnlocksAlternatesThroughCompletionChain()
    {
        var initialPaddles = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();

        Assert.That(initialPaddles, Has.Length.EqualTo(1));
        Assert.That(initialPaddles[0].DisplayName, Is.EqualTo("Classic Paddle"));

        var classicSettings = CreateRogueSettings(1010, intensity: 1);
        var classicClear = BreakoutRogueRunResultStore.BuildResult(classicSettings, completed: true, stageReached: 10, "Classic Paddle", 5000);

        BreakoutRogueRunResultStore.Save(classicClear);

        var cometUnlocked = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();

        Assert.That(cometUnlocked, Has.Length.EqualTo(2));
        Assert.That(cometUnlocked[1].DisplayName, Is.EqualTo("Comet Paddle"));
        Assert.That(BreakoutRoguePaddleCatalog.GetNextLockedPaddle().DisplayName, Is.EqualTo("Cruiser Paddle"));

        var cometClear = BreakoutRogueRunResultStore.BuildResult(classicSettings, completed: true, stageReached: 10, "Comet Paddle", 6200);

        BreakoutRogueRunResultStore.Save(cometClear);

        var cruiserUnlocked = BreakoutRoguePaddleCatalog.BuildUnlockedPaddles();

        Assert.That(cruiserUnlocked, Has.Length.EqualTo(3));
        Assert.That(cruiserUnlocked[2].DisplayName, Is.EqualTo("Cruiser Paddle"));
    }

    [Test]
    public void RogueRunControllerAppliesSelectedPaddleTuning()
    {
        var controller = new BreakoutRogueRunController(new List<RunUpgradeDefinition>(), new List<PowerUpDefinition>());

        var cometSettings = controller.BuildRunSettings(5050, 500, null, "Comet Paddle");
        var cruiserSettings = controller.BuildRunSettings(6060, 500, null, "Cruiser Paddle");

        Assert.That(cometSettings.SelectedPaddleLabel, Is.EqualTo("Comet Paddle"));
        Assert.That(cometSettings.PaddleWidthMultiplier, Is.EqualTo(0.82f).Within(0.0001f));
        Assert.That(cometSettings.PaddleSpeedMultiplier, Is.EqualTo(1.22f).Within(0.0001f));

        Assert.That(cruiserSettings.SelectedPaddleLabel, Is.EqualTo("Cruiser Paddle"));
        Assert.That(cruiserSettings.PaddleWidthMultiplier, Is.EqualTo(1.22f).Within(0.0001f));
        Assert.That(cruiserSettings.PaddleSpeedMultiplier, Is.EqualTo(0.82f).Within(0.0001f));
    }

    [Test]
    public void RogueRunControllerAutomaticallyUnlocksHazardsAsStagesAdvance()
    {
        var helpful = CreatePowerUp("Laser Grid", "laser_grid", beneficial: true);
        var blackout = CreatePowerUp("Blackout", "blackout", beneficial: false);
        var jammer = CreatePowerUp("Brick Jammer", "brick_jammer", beneficial: false);
        var drift = CreatePowerUp("Signal Drift", "signal_drift", beneficial: false);

        try
        {
            var controller = new BreakoutRogueRunController(
                new List<RunUpgradeDefinition>(),
                new List<PowerUpDefinition> { helpful, drift, jammer, blackout });
            var runState = new BreakoutRunState();

            controller.InitializeRunState(runState);
            Assert.That(runState.IsDropUnlocked(blackout), Is.False);

            var lowHeatSettings = CreateRogueSettings(1010, intensity: 1);
            var highHeatSettings = CreateRogueSettings(1010, intensity: 50);

            runState.RegisterLevelClear();
            Assert.That(controller.UnlockHazardsForClearedLevel(runState, lowHeatSettings), Is.Zero);
            Assert.That(runState.IsDropUnlocked(blackout), Is.False);

            runState.RegisterLevelClear();
            Assert.That(controller.UnlockHazardsForClearedLevel(runState, lowHeatSettings), Is.Zero);
            Assert.That(runState.IsDropUnlocked(blackout), Is.False);

            runState.RegisterLevelClear();
            Assert.That(controller.UnlockHazardsForClearedLevel(runState, lowHeatSettings), Is.EqualTo(1));
            Assert.That(runState.IsDropUnlocked(blackout), Is.True);
            Assert.That(runState.IsDropUnlocked(jammer), Is.False);

            runState.RegisterLevelClear();
            Assert.That(controller.UnlockHazardsForClearedLevel(runState, lowHeatSettings), Is.Zero);

            Assert.That(controller.UnlockHazardsForClearedLevel(runState, highHeatSettings), Is.EqualTo(2));
            Assert.That(runState.IsDropUnlocked(jammer), Is.True);
            Assert.That(runState.IsDropUnlocked(drift), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(helpful);
            Object.DestroyImmediate(blackout);
            Object.DestroyImmediate(jammer);
            Object.DestroyImmediate(drift);
        }
    }

    [Test]
    public void RogueRunControllerOnlyAutoUnlocksHazardsInCurrentRarityBand()
    {
        var commonHazard = CreatePowerUp("Common Hazard", "common_hazard", beneficial: false);
        var rareHazard = CreatePowerUp("Rare Hazard", "rare_hazard", beneficial: false, BreakoutContentRarity.Rare);

        try
        {
            var controller = new BreakoutRogueRunController(
                new List<RunUpgradeDefinition>(),
                new List<PowerUpDefinition> { rareHazard, commonHazard });
            var runState = new BreakoutRunState();
            var lowHeatSettings = CreateRogueSettings(1010, intensity: 8);
            var rareHeatSettings = CreateRogueSettings(1010, intensity: 18);

            for (var index = 0; index < 6; index++)
            {
                runState.RegisterLevelClear();
            }

            Assert.That(controller.UnlockHazardsForClearedLevel(runState, lowHeatSettings), Is.EqualTo(1));
            Assert.That(runState.IsDropUnlocked(commonHazard), Is.True);
            Assert.That(runState.IsDropUnlocked(rareHazard), Is.False);

            Assert.That(controller.UnlockHazardsForClearedLevel(runState, rareHeatSettings), Is.EqualTo(1));
            Assert.That(runState.IsDropUnlocked(rareHazard), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(commonHazard);
            Object.DestroyImmediate(rareHazard);
        }
    }

    [Test]
    public void DeveloperRunsDoNotRecordRogueResults()
    {
        var settings = new RunSettings(
            1212,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
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
            RunGameMode.Rogue);

        Assert.That(BreakoutGameController.ShouldRecordRogueRunResult(false, false, settings), Is.True);
        Assert.That(BreakoutGameController.ShouldRecordRogueRunResult(false, true, settings), Is.False);
    }

    [Test]
    public void RogueResultStorePersistsLastRunSummary()
    {
        var settings = new RunSettings(
            9876,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
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
            RunGameMode.Rogue,
            rogueIntensity: 7);
        var result = BreakoutRogueRunResultStore.BuildResult(settings, completed: false, stageReached: 6, "Classic Paddle", 1234);

        BreakoutRogueRunResultStore.Save(result);

        Assert.That(BreakoutRogueRunResultStore.TryLoad(out var loaded), Is.True);
        Assert.That(loaded.Completed, Is.False);
        Assert.That(loaded.StageReached, Is.EqualTo(6));
        Assert.That(loaded.SelectedPaddle, Is.EqualTo("Classic Paddle"));
        Assert.That(loaded.CurrentIntensity, Is.EqualTo(7));
        Assert.That(loaded.Seed, Is.EqualTo(9876));
        Assert.That(loaded.Score, Is.EqualTo(1234));
        Assert.That(BreakoutRogueRunResultStore.BuildSummary(loaded), Does.Contain("Wiped Out | Stage 06/10"));
        Assert.That(BreakoutRogueRunResultStore.BuildSummary(loaded), Does.Contain("Heat 07"));
    }

    [Test]
    public void RogueIntensityProgressUnlocksNextHeatOnlyAfterCompletion()
    {
        var failedSettings = CreateRogueSettings(1010, intensity: 1);
        var failedResult = BreakoutRogueRunResultStore.BuildResult(failedSettings, completed: false, stageReached: 8, "Classic Paddle", 4000);

        BreakoutRogueRunResultStore.Save(failedResult);

        Assert.That(BreakoutRogueIntensityProgressStore.GetAvailableIntensity("Classic Paddle"), Is.EqualTo(1));
        Assert.That(BreakoutRogueIntensityProgressStore.GetBestStageReached("Classic Paddle", 1), Is.EqualTo(8));

        var completedResult = BreakoutRogueRunResultStore.BuildResult(failedSettings, completed: true, stageReached: 10, "Classic Paddle", 6500);

        BreakoutRogueRunResultStore.Save(completedResult);

        Assert.That(BreakoutRogueIntensityProgressStore.GetHighestCompletedIntensity("Classic Paddle"), Is.EqualTo(1));
        Assert.That(BreakoutRogueIntensityProgressStore.GetAvailableIntensity("Classic Paddle"), Is.EqualTo(2));
        Assert.That(BreakoutRogueIntensityProgressStore.GetBestStageReached("Classic Paddle", 1), Is.EqualTo(10));
    }

    [Test]
    public void RogueRunControllerBuildsRunAtAvailableIntensity()
    {
        var controller = new BreakoutRogueRunController(new List<RunUpgradeDefinition>(), new List<PowerUpDefinition>());
        var completedSettings = CreateRogueSettings(2020, intensity: 1);
        var completedResult = BreakoutRogueRunResultStore.BuildResult(completedSettings, completed: true, stageReached: 10, "Classic Paddle", 5000);

        BreakoutRogueRunResultStore.Save(completedResult);

        var settings = controller.BuildRunSettings(3030, 500, null);

        Assert.That(settings.RogueIntensity, Is.EqualTo(2));
        Assert.That(settings.SelectedPaddleLabel, Is.EqualTo("Classic Paddle"));
        Assert.That(settings.BallSpeedMultiplier, Is.EqualTo(BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(2)).Within(0.0001f));
    }

    [Test]
    public void RogueRunControllerCanBuildDeveloperRunAtRequestedIntensity()
    {
        var controller = new BreakoutRogueRunController(new List<RunUpgradeDefinition>(), new List<PowerUpDefinition>());

        var settings = controller.BuildRunSettings(4040, 500, null, intensityOverride: 42);

        Assert.That(settings.RogueIntensity, Is.EqualTo(42));
        Assert.That(settings.BallSpeedMultiplier, Is.EqualTo(BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(42)).Within(0.0001f));
    }

    private static RunSettings CreateRogueSettings(int seed, int intensity)
    {
        return new RunSettings(
            seed,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            500,
            1,
            1f,
            BreakoutRunProgression.GetRogueIntensityBallSpeedMultiplier(intensity),
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null,
            RunGameMode.Rogue,
            intensity);
    }

    private static PowerUpDefinition CreatePowerUp(
        string displayName,
        string powerUpId,
        bool beneficial,
        BreakoutContentRarity rarity = BreakoutContentRarity.Common)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
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
