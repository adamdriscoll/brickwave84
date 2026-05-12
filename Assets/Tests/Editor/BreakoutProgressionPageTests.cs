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
    public void ProgressionPageShowsTopLevelMenuAction()
    {
        var actions = new BreakoutMainMenuService().BuildActions();

        Assert.That(actions[0], Is.EqualTo(BreakoutMainMenuAction.Rogue));
        Assert.That(actions[1], Is.EqualTo(BreakoutMainMenuAction.Progression));
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

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>(), BreakoutRogueRunResultStore.DefaultPaddleLabel);
        var chromeRail = view.Cards.First(card => card.Title == "Chrome Rail");
        var solarShot = view.Cards.First(card => card.Title == "Solar Shot");

        Assert.That(view.LadderLines[1], Does.Contain("Heat 08"));
        Assert.That(chromeRail.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.Unlocked));
        Assert.That(solarShot.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(chromeRail.UnlockHint, Does.Contain("preview"));
        Assert.That(solarShot.UnlockHint, Does.Contain("Heat 12"));
    }
}
