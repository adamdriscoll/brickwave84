using System;
using System.Linq;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutProgressionPageTests
{
    private const string RogueProgressKey = "GetBricked.Rogue.IntensityProgress";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(RogueProgressKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(RogueProgressKey);
    }

    [Test]
    public void ProgressionPageLaunchesFromNeonLadderInsteadOfTopLevelAction()
    {
        var actions = new BreakoutMainMenuService().BuildActions();

        Assert.That(actions[0], Is.EqualTo(BreakoutMainMenuAction.Rogue));
        Assert.That(Array.IndexOf(actions, BreakoutMainMenuAction.Progression), Is.EqualTo(-1));
        Assert.That(actions[1], Is.EqualTo(BreakoutMainMenuAction.SoloMarathon));
    }

    [Test]
    public void ProgressionPageUsesSavedHeatForPlaceholderUnlockPreview()
    {
        BreakoutRogueRunResultStore.Save(new BreakoutRogueRunResult
        {
            Completed = true,
            CurrentIntensity = 8,
            SelectedPaddle = BreakoutRogueRunResultStore.DefaultPaddleLabel,
            StageReached = BreakoutRunProgression.TargetLevelCount,
            Score = 1000,
            Seed = 1234,
        });

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>());
        var chromeRail = view.Cards.First(card => card.Title == "Chrome Rail");
        var solarShot = view.Cards.First(card => card.Title == "Solar Shot");

        Assert.That(view.LadderLines, Has.Some.Contains("Heat 08"));
        Assert.That(view.LadderLines.Any(line => line.Contains("Paddle")), Is.False);
        Assert.That(view.NextSignal, Does.Not.Contain("Paddle"));
        Assert.That(view.Paddles, Is.Empty);
        Assert.That(chromeRail.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.Unlocked));
        Assert.That(solarShot.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(chromeRail.UnlockHint, Does.Contain("preview"));
        Assert.That(solarShot.UnlockHint, Does.Contain("Heat 12"));
    }

    [Test]
    public void ProgressionPageShowsBogusTapeAsLockedLadderHazard()
    {
        var bogusTape = Resources.Load<PowerUpDefinition>("PowerUps/BogusTape");
        Assert.That(bogusTape, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { bogusTape });
        var card = view.Cards.First(item => item.Title == "Bogus Tape");

        Assert.That(card.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(card.Family, Does.Contain("Hazard"));
        Assert.That(card.Description, Does.Contain("random hazard"));
        Assert.That(card.UnlockHint, Does.Contain("Heat 08"));
        Assert.That(view.MeterLines, Has.Some.Contains("Drops: 00 Default"));
    }

    [Test]
    public void ProgressionPageShowsVectorSightAsLiveHeatSixDrop()
    {
        var vectorSight = Resources.Load<PowerUpDefinition>("PowerUps/VectorSight");
        Assert.That(vectorSight, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { vectorSight });
        var card = view.Cards.First(item => item.Title == "Vector Sight");

        Assert.That(card.UnlockHint, Does.Contain("Heat 06"));
        Assert.That(card.Description, Does.Contain("aim preview"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsCapsuleMagnetAsLiveRareDrop()
    {
        var capsuleMagnet = Resources.Load<PowerUpDefinition>("PowerUps/CapsuleMagnet");
        Assert.That(capsuleMagnet, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { capsuleMagnet });
        var card = view.Cards.First(item => item.Title == "Capsule Magnet");

        Assert.That(card.UnlockHint, Does.Contain("Heat 18"));
        Assert.That(card.Description, Does.Contain("helpful capsules drift"));
        Assert.That(card.Family, Does.Contain("Rare Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }
}
