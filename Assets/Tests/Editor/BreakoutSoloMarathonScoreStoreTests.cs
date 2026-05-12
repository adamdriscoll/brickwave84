using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutSoloMarathonScoreStoreTests
{
    [SetUp]
    public void SetUp()
    {
        ClearRecords();
    }

    [TearDown]
    public void TearDown()
    {
        ClearRecords();
    }

    [Test]
    public void RunSettingsCanIdentifySoloMarathonMode()
    {
        var settings = new RunSettings(
            1984,
            RunDifficultyPreset.Standard,
            RunScoringMode.HighScore,
            5,
            0,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null,
            RunGameMode.SoloMarathon,
            difficultyLabel: "Gnarly");

        Assert.That(settings.IsSoloMarathonMode, Is.True);
        Assert.That(settings.GameModeLabel, Is.EqualTo("Neon Marathon"));
        Assert.That(settings.StartingLives, Is.EqualTo(5));
        Assert.That(settings.ScoringMode, Is.EqualTo(RunScoringMode.HighScore));
        Assert.That(settings.DifficultyLabel, Is.EqualTo("Gnarly"));
    }

    [Test]
    public void ScoreStoreOnlyReplacesBestScoreForSelectedHeat()
    {
        Assert.That(
            BreakoutSoloMarathonScoreStore.TrySaveBest(
                BreakoutHotSeatDifficulty.Gnarly,
                "Gnarly",
                1200,
                4,
                1111,
                out var firstRecord),
            Is.True);

        Assert.That(firstRecord.Score, Is.EqualTo(1200));
        Assert.That(
            BreakoutSoloMarathonScoreStore.TrySaveBest(
                BreakoutHotSeatDifficulty.Gnarly,
                "Gnarly",
                900,
                8,
                2222,
                out var unchangedRecord),
            Is.False);

        Assert.That(unchangedRecord.Score, Is.EqualTo(1200));
        Assert.That(unchangedRecord.StageReached, Is.EqualTo(4));

        Assert.That(
            BreakoutSoloMarathonScoreStore.TrySaveBest(
                BreakoutHotSeatDifficulty.Gnarly,
                "Gnarly",
                1300,
                5,
                3333,
                out var improvedRecord),
            Is.True);

        Assert.That(improvedRecord.Score, Is.EqualTo(1300));
        Assert.That(BreakoutSoloMarathonScoreStore.Load(BreakoutHotSeatDifficulty.Gnarly).Seed, Is.EqualTo(3333));
    }

    [Test]
    public void ScoreStoreTracksBestOverallAcrossHeatLevels()
    {
        BreakoutSoloMarathonScoreStore.TrySaveBest(BreakoutHotSeatDifficulty.Chill, "Chill", 900, 3, 1111, out _);
        BreakoutSoloMarathonScoreStore.TrySaveBest(BreakoutHotSeatDifficulty.Bogus, "Bogus", 1800, 2, 2222, out _);

        var best = BreakoutSoloMarathonScoreStore.LoadBestOverall();

        Assert.That(best, Is.Not.Null);
        Assert.That(best.Score, Is.EqualTo(1800));
        Assert.That(best.DifficultyLabel, Is.EqualTo("Bogus"));
        Assert.That(BreakoutSoloMarathonScoreStore.BuildSummary(best), Does.Contain("Best Score: 1800"));
    }

    [Test]
    public void HeatScoreMultiplierPaysMoreAtHigherHeat()
    {
        Assert.That(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Chill), Is.EqualTo(1f));
        Assert.That(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Rad), Is.GreaterThan(1f));
        Assert.That(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Gnarly), Is.GreaterThan(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Rad)));
        Assert.That(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Mondo), Is.GreaterThan(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Gnarly)));
        Assert.That(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Bogus), Is.GreaterThan(BreakoutGameController.GetSoloMarathonHeatScoreMultiplier(BreakoutHotSeatDifficulty.Mondo)));
    }

    private static void ClearRecords()
    {
        for (var value = (int)BreakoutHotSeatDifficulty.Chill; value <= (int)BreakoutHotSeatDifficulty.Bogus; value++)
        {
            PlayerPrefs.DeleteKey(BreakoutSoloMarathonScoreStore.BuildKey((BreakoutHotSeatDifficulty)value));
        }
    }
}
