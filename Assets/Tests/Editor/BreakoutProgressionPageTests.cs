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
        Assert.That(actions[actions.Length - 1], Is.EqualTo(BreakoutMainMenuAction.QuitGame));
    }

    [Test]
    public void MainMenuShowsPowerDownQuitAction()
    {
        var view = new BreakoutMainMenuService().BuildView(new BreakoutMainMenuContext());
        var powerDownIndex = Array.IndexOf(view.ActionLabels, "Power Down");

        Assert.That(view.ActionLabels, Has.Some.EqualTo("Power Down"));
        Assert.That(view.ActionGroupLabels[powerDownIndex], Is.EqualTo("Cabinet"));
        Assert.That(view.ActionTones[powerDownIndex], Is.EqualTo(BreakoutUiMenuActionTone.Danger));
        Assert.That(view.ActionIcons[powerDownIndex], Is.EqualTo(BreakoutUiMenuActionIcon.Power));
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
    public void ProgressionPageUsesSavedHeatForGlitchPlaceholderUnlockPreview()
    {
        BreakoutRogueRunResultStore.Save(new BreakoutRogueRunResult
        {
            Completed = true,
            CurrentIntensity = 6,
            SelectedPaddle = BreakoutRogueRunResultStore.DefaultPaddleLabel,
            StageReached = BreakoutRunProgression.TargetLevelCount,
            Score = 1000,
            Seed = 1234,
        });

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>());
        var rowRewrite = view.Cards.First(card => card.Title == "Row Rewrite");
        var prismLanes = view.Cards.First(card => card.Title == "Prism Lanes");

        Assert.That(view.LadderLines, Has.Some.Contains("Heat 06"));
        Assert.That(view.LadderLines.Any(line => line.Contains("Paddle")), Is.False);
        Assert.That(view.NextSignal, Does.Not.Contain("Paddle"));
        Assert.That(view.Paddles, Is.Empty);
        Assert.That(rowRewrite.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.Unlocked));
        Assert.That(prismLanes.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(rowRewrite.UnlockHint, Does.Contain("preview"));
        Assert.That(prismLanes.UnlockHint, Does.Contain("Heat 07"));
    }

    [Test]
    public void AuthoredLiveUnlocksUseParallelDropAndGlitchHeatTracks()
    {
        var drops = Resources.LoadAll<PowerUpDefinition>("PowerUps");
        var unlockHeats = drops
            .Where(definition => !IsDefaultDrop(definition))
            .Select(definition => definition.LadderUnlockIntensity)
            .OrderBy(heat => heat)
            .ToArray();
        var expectedDropHeats = Enumerable.Range(1, 35).ToArray();

        Assert.That(unlockHeats, Is.EqualTo(expectedDropHeats));

        var view = new BreakoutProgressionPageService().BuildView(Array.Empty<PowerUpDefinition>());
        var turboRail = view.Cards.First(card => card.Title == "Turbo Rail");
        var mirrorGrid = view.Cards.First(card => card.Title == "Mirror Grid");
        var tokenStorm = view.Cards.First(card => card.Title == "Token Storm");
        var gravityPocket = view.Cards.First(card => card.Title == "Gravity Pocket");
        var solarShot = view.Cards.First(card => card.Title == "Solar Shot");

        Assert.That(turboRail.UnlockHint, Does.Contain("Heat 01"));
        Assert.That(mirrorGrid.UnlockHint, Does.Contain("Heat 02"));
        Assert.That(mirrorGrid.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(gravityPocket.UnlockHint, Does.Contain("Heat 04"));
        Assert.That(gravityPocket.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(gravityPocket.Family, Does.Contain("Epic"));
        Assert.That(tokenStorm.UnlockHint, Does.Contain("Heat 03"));
        Assert.That(tokenStorm.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(tokenStorm.Family, Does.Contain("Epic"));
        Assert.That(solarShot.UnlockHint, Does.Contain("Heat 36"));
    }

    [Test]
    public void ProgressionPageShowsTiltRailAsLiveHeatThirtyFiveHazardDrop()
    {
        var tiltRail = Resources.Load<PowerUpDefinition>("PowerUps/TiltRail");
        Assert.That(tiltRail, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { tiltRail });
        var card = view.Cards.First(item => item.Title == "Tilt Rail");

        Assert.That(card.UnlockHint, Does.Contain("Heat 35"));
        Assert.That(card.Description, Does.Contain("tilts the rail"));
        Assert.That(card.Family, Does.Contain("Epic Hazard"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsBankBonusAsLiveHeatThirtyTwoDrop()
    {
        var bankBonus = Resources.Load<PowerUpDefinition>("PowerUps/BankBonus");
        Assert.That(bankBonus, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { bankBonus });
        var card = view.Cards.First(item => item.Title == "Bank Bonus");

        Assert.That(card.UnlockHint, Does.Contain("Heat 32"));
        Assert.That(card.Description, Does.Contain("Wall bounces bank +50 points"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsPrismPopAsLiveHeatThirtyThreeDrop()
    {
        var prismPop = Resources.Load<PowerUpDefinition>("PowerUps/PrismPop");
        Assert.That(prismPop, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { prismPop });
        var card = view.Cards.First(item => item.Title == "Prism Pop");

        Assert.That(card.UnlockHint, Does.Contain("Heat 33"));
        Assert.That(card.Description, Does.Contain("short-lived copy ball"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Prism Pop"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsCleanCatchAsLiveHeatThirtyOneDrop()
    {
        var cleanCatch = Resources.Load<PowerUpDefinition>("PowerUps/CleanCatch");
        Assert.That(cleanCatch, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { cleanCatch });
        var card = view.Cards.First(item => item.Title == "Clean Catch");

        Assert.That(card.UnlockHint, Does.Contain("Heat 31"));
        Assert.That(card.Description, Does.Contain("releases with aim"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Clean Catch"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsMysteryTapeAsLiveHeatThirtyFourDrop()
    {
        var mysteryTape = Resources.Load<PowerUpDefinition>("PowerUps/MysteryTape");
        Assert.That(mysteryTape, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { mysteryTape });
        var card = view.Cards.First(item => item.Title == "Mystery Tape");

        Assert.That(card.UnlockHint, Does.Contain("Heat 34"));
        Assert.That(card.Description, Does.Contain("helpful drop and"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Mystery Tape"), Is.EqualTo(1));
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
