using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRogueRunTests
{
    private const string LastRogueResultKey = "GetBricked.Rogue.LastResult";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
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
        Assert.That(settings.GameModeLabel, Is.EqualTo("Rogue"));
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
            RunGameMode.Rogue);
        var result = BreakoutRogueRunResultStore.BuildResult(settings, completed: false, stageReached: 6, "Classic Paddle", 1234);

        BreakoutRogueRunResultStore.Save(result);

        Assert.That(BreakoutRogueRunResultStore.TryLoad(out var loaded), Is.True);
        Assert.That(loaded.Completed, Is.False);
        Assert.That(loaded.StageReached, Is.EqualTo(6));
        Assert.That(loaded.SelectedPaddle, Is.EqualTo("Classic Paddle"));
        Assert.That(loaded.Seed, Is.EqualTo(9876));
        Assert.That(loaded.Score, Is.EqualTo(1234));
        Assert.That(BreakoutRogueRunResultStore.BuildSummary(loaded), Does.Contain("Wiped Out | Stage 06/10"));
    }
}
