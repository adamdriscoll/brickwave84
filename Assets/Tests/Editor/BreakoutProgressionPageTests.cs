using System;
using System.Linq;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutProgressionPageTests
{
    private const string RogueLastResultKey = "GetBricked.Rogue.LastResult";
    private const string RogueProgressKey = "GetBricked.Rogue.IntensityProgress";

    private static readonly string[] DefaultDropIds =
    {
        "large_paddle",
        "multi_ball",
        "slow_ball",
        "shield_wall",
    };

    [SetUp]
    public void SetUp()
    {
        ClearRogueProgress();
    }

    [TearDown]
    public void TearDown()
    {
        ClearRogueProgress();
    }

    private static void ClearRogueProgress()
    {
        PlayerPrefs.DeleteKey(RogueLastResultKey);
        PlayerPrefs.DeleteKey(RogueProgressKey);
        PlayerPrefs.Save();
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
    public void MainMenuHintOnlyListsControls()
    {
        var view = new BreakoutMainMenuService().BuildView(new BreakoutMainMenuContext());

        Assert.That(view.HintText, Is.EqualTo("Up/Down selects. Left/Right tunes. Space confirms."));
        Assert.That(view.HintText, Does.Not.Contain("Neon Ladder"));
        Assert.That(view.HintText, Does.Not.Contain("opens"));
    }

    [Test]
    public void ProgressionPageUsesSavedHeatForPlaceholderUnlockPreview()
    {
        BreakoutRogueRunResultStore.Save(new BreakoutRogueRunResult
        {
            Completed = true,
            CurrentIntensity = 32,
            SelectedPaddle = BreakoutRogueRunResultStore.DefaultPaddleLabel,
            StageReached = BreakoutRunProgression.TargetLevelCount,
            Score = 1000,
            Seed = 1234,
        });

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>());
        var chromeRail = view.Cards.First(card => card.Title == "Chrome Rail");
        var solarShot = view.Cards.First(card => card.Title == "Solar Shot");

        Assert.That(view.LadderLines, Has.Some.Contains("Heat 32"));
        Assert.That(view.LadderLines.Any(line => line.Contains("Paddle")), Is.False);
        Assert.That(view.NextSignal, Does.Not.Contain("Paddle"));
        Assert.That(view.Paddles, Is.Empty);
        Assert.That(chromeRail.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.Unlocked));
        Assert.That(solarShot.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(chromeRail.UnlockHint, Does.Contain("preview"));
        Assert.That(solarShot.UnlockHint, Does.Contain("Heat 35"));
    }

    [Test]
    public void AuthoredLiveUnlocksUseOneSignalPerHeatBeforeBacklog()
    {
        var drops = Resources.LoadAll<PowerUpDefinition>("PowerUps");
        var unlockHeats = drops
            .Where(definition => !IsDefaultDrop(definition))
            .Select(definition => definition.LadderUnlockIntensity)
            .OrderBy(heat => heat)
            .ToArray();
        var expectedDropHeats = Enumerable.Range(1, 30).Append(34).Append(36).ToArray();

        Assert.That(unlockHeats, Is.EqualTo(expectedDropHeats));

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>());
        var turboRail = view.Cards.First(card => card.Title == "Turbo Rail");
        var chromeRail = view.Cards.First(card => card.Title == "Chrome Rail");

        Assert.That(turboRail.UnlockHint, Does.Contain("Heat 31"));
        Assert.That(chromeRail.UnlockHint, Does.Contain("Heat 32"));
    }

    [Test]
    public void ProgressionPageShowsBankBonusAsLiveHeatThirtyFourDrop()
    {
        var bankBonus = Resources.Load<PowerUpDefinition>("PowerUps/BankBonus");
        Assert.That(bankBonus, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { bankBonus });
        var card = view.Cards.First(item => item.Title == "Bank Bonus");

        Assert.That(card.UnlockHint, Does.Contain("Heat 34"));
        Assert.That(card.Description, Does.Contain("Wall bounces bank +50 points"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsPrismPopAsLiveHeatThirtySixDrop()
    {
        var prismPop = Resources.Load<PowerUpDefinition>("PowerUps/PrismPop");
        Assert.That(prismPop, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { prismPop });
        var card = view.Cards.First(item => item.Title == "Prism Pop");

        Assert.That(card.UnlockHint, Does.Contain("Heat 36"));
        Assert.That(card.Description, Does.Contain("short-lived copy ball"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Prism Pop"), Is.EqualTo(1));
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
        Assert.That(card.UnlockHint, Does.Contain("Heat 05"));
        Assert.That(view.MeterLines, Has.Some.Contains("Drops: 00 Default"));
    }

    [Test]
    public void ProgressionPageShowsVectorSightAsLiveHeatFourDrop()
    {
        var vectorSight = Resources.Load<PowerUpDefinition>("PowerUps/VectorSight");
        Assert.That(vectorSight, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { vectorSight });
        var card = view.Cards.First(item => item.Title == "Vector Sight");

        Assert.That(card.UnlockHint, Does.Contain("Heat 04"));
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

        Assert.That(card.UnlockHint, Does.Contain("Heat 19"));
        Assert.That(card.Description, Does.Contain("helpful capsules drift"));
        Assert.That(card.Family, Does.Contain("Rare Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsMirrorImageAsLiveHeatFourteenHelpfulDrop()
    {
        var mirrorImage = Resources.Load<PowerUpDefinition>("PowerUps/MirrorImage");
        Assert.That(mirrorImage, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { mirrorImage });
        var card = view.Cards.First(item => item.Title == "Mirror Image");

        Assert.That(card.UnlockHint, Does.Contain("Heat 14"));
        Assert.That(card.Description, Does.Contain("opposite-moving mirror paddle"));
        Assert.That(card.Family, Does.Contain("Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    private static bool IsDefaultDrop(PowerUpDefinition definition)
    {
        var candidateId = BreakoutPowerUpIdentity.GetStableId(definition);

        return DefaultDropIds.Any(defaultId => string.Equals(
            candidateId,
            defaultId,
            StringComparison.OrdinalIgnoreCase));
    }
}
