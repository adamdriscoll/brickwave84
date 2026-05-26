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
        "brick_missile",
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
    public void ProgressionPageUsesSavedHeatForGlitchUnlocks()
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
        var switchbackRails = view.Cards.First(card => card.Title == "Switchback Rails");
        var capsuleRoulette = view.Cards.First(card => card.Title == "Capsule Roulette");
        var driftRows = view.Cards.First(card => card.Title == "Drift Rows");
        var hotCorners = view.Cards.First(card => card.Title == "Hot Corners");
        var flickerBricks = view.Cards.First(card => card.Title == "Flicker Bricks");
        var ghostRow = view.Cards.First(card => card.Title == "Ghost Row");
        var splitHorizon = view.Cards.First(card => card.Title == "Split Horizon");

        Assert.That(view.LadderLines, Has.Some.Contains("Heat 06"));
        Assert.That(view.LadderLines.Any(line => line.Contains("Paddle")), Is.False);
        Assert.That(view.NextSignal, Does.Not.Contain("Paddle"));
        Assert.That(view.Paddles, Is.Empty);
        Assert.That(rowRewrite.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.Unlocked));
        Assert.That(prismLanes.UnlockState, Is.EqualTo(BreakoutUiProgressionUnlockState.SeenLocked));
        Assert.That(rowRewrite.UnlockHint, Does.Contain("Heat 06"));
        Assert.That(rowRewrite.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(prismLanes.UnlockHint, Does.Contain("Heat 07"));
        Assert.That(switchbackRails.UnlockHint, Does.Contain("Heat 08"));
        Assert.That(capsuleRoulette.UnlockHint, Does.Contain("Heat 09"));
        Assert.That(driftRows.UnlockHint, Does.Contain("Heat 10"));
        Assert.That(hotCorners.UnlockHint, Does.Contain("Heat 11"));
        Assert.That(flickerBricks.UnlockHint, Does.Contain("Heat 12"));
        Assert.That(ghostRow.UnlockHint, Does.Contain("Heat 14"));
        Assert.That(splitHorizon.UnlockHint, Does.Contain("Heat 15"));
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
        var expectedDropHeats = Enumerable.Range(1, 46)
            .Concat(new[] { 48, 49, 50 })
            .ToArray();

        Assert.That(unlockHeats, Is.EqualTo(expectedDropHeats));

        var view = new BreakoutProgressionPageService().BuildView(drops);
        var turboRail = view.Cards.First(card => card.Title == "Turbo Rail");
        var mirrorGrid = view.Cards.First(card => card.Title == "Mirror Grid");
        var tokenStorm = view.Cards.First(card => card.Title == "Token Storm");
        var gravityPocket = view.Cards.First(card => card.Title == "Gravity Pocket");
        var rowRewrite = view.Cards.First(card => card.Title == "Row Rewrite");
        var prismLanes = view.Cards.First(card => card.Title == "Prism Lanes");
        var switchbackRails = view.Cards.First(card => card.Title == "Switchback Rails");
        var capsuleRoulette = view.Cards.First(card => card.Title == "Capsule Roulette");
        var driftRows = view.Cards.First(card => card.Title == "Drift Rows");
        var hotCorners = view.Cards.First(card => card.Title == "Hot Corners");
        var flickerBricks = view.Cards.First(card => card.Title == "Flicker Bricks");
        var cassetteSkip = view.Cards.First(card => card.Title == "Cassette Skip");
        var ghostRow = view.Cards.First(card => card.Title == "Ghost Row");
        var splitHorizon = view.Cards.First(card => card.Title == "Split Horizon");
        var solarShot = view.Cards.First(card => card.Title == "Solar Shot");
        var wrapRail = view.Cards.First(card => card.Title == "Wrap Rail");
        var staticShoes = view.Cards.First(card => card.Title == "Static Shoes");
        var jackpotJam = view.Cards.First(card => card.Title == "Jackpot Jam");
        var rewindCatch = view.Cards.First(card => card.Title == "Rewind Catch");
        var microSpark = view.Cards.First(card => card.Title == "Micro Spark");
        var brickBloom = view.Cards.First(card => card.Title == "Brick Bloom");
        var doubleTap = view.Cards.First(card => card.Title == "Double Tap");
        var fuseBurst = view.Cards.First(card => card.Title == "Fuse Burst");
        var overdriveTape = view.Cards.First(card => card.Title == "Overdrive Tape");
        var bogusBounce = view.Cards.First(card => card.Title == "Bogus Bounce");
        var cabinetJackpot = view.Cards.First(card => card.Title == "Cabinet Jackpot");
        var finalBreakthru = view.Cards.First(card => card.Title == "Final Breakthru");

        Assert.That(turboRail.UnlockHint, Does.Contain("Heat 01"));
        Assert.That(mirrorGrid.UnlockHint, Does.Contain("Heat 02"));
        Assert.That(mirrorGrid.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(gravityPocket.UnlockHint, Does.Contain("Heat 04"));
        Assert.That(gravityPocket.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(gravityPocket.Family, Does.Contain("Epic"));
        Assert.That(tokenStorm.UnlockHint, Does.Contain("Heat 03"));
        Assert.That(tokenStorm.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(tokenStorm.Family, Does.Contain("Epic"));
        Assert.That(rowRewrite.UnlockHint, Does.Contain("Heat 06"));
        Assert.That(rowRewrite.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(rowRewrite.Description, Does.Contain("rerolls"));
        Assert.That(rowRewrite.Family, Does.Contain("Rare"));
        Assert.That(prismLanes.UnlockHint, Does.Contain("Heat 07"));
        Assert.That(prismLanes.UnlockHint, Does.Not.Contain("preview"));
        Assert.That(prismLanes.Description, Does.Contain("refract"));
        Assert.That(prismLanes.Family, Does.Contain("Rare"));
        Assert.That(switchbackRails.UnlockHint, Does.Contain("Heat 08"));
        Assert.That(switchbackRails.Description, Does.Contain("swap rebound angles"));
        Assert.That(switchbackRails.Family, Does.Contain("Rare"));
        Assert.That(capsuleRoulette.UnlockHint, Does.Contain("Heat 09"));
        Assert.That(capsuleRoulette.Description, Does.Contain("rotate polarity"));
        Assert.That(capsuleRoulette.Family, Does.Contain("Rare"));
        Assert.That(driftRows.UnlockHint, Does.Contain("Heat 10"));
        Assert.That(driftRows.Description, Does.Contain("opposite directions"));
        Assert.That(driftRows.Family, Does.Contain("Rare"));
        Assert.That(hotCorners.UnlockHint, Does.Contain("Heat 11"));
        Assert.That(hotCorners.Description, Does.Contain("kick balls back toward center"));
        Assert.That(hotCorners.Family, Does.Contain("Rare"));
        Assert.That(flickerBricks.UnlockHint, Does.Contain("Heat 12"));
        Assert.That(flickerBricks.Description, Does.Contain("only collide while visible"));
        Assert.That(flickerBricks.Family, Does.Contain("Rare"));
        Assert.That(cassetteSkip.UnlockHint, Does.Contain("Heat 13"));
        Assert.That(cassetteSkip.Description, Does.Contain("skips forward"));
        Assert.That(cassetteSkip.Family, Does.Contain("Rare"));
        Assert.That(ghostRow.UnlockHint, Does.Contain("Heat 14"));
        Assert.That(ghostRow.Description, Does.Contain("phases out after hits"));
        Assert.That(ghostRow.Family, Does.Contain("Rare"));
        Assert.That(splitHorizon.UnlockHint, Does.Contain("Heat 15"));
        Assert.That(splitHorizon.Description, Does.Contain("midpoint bends"));
        Assert.That(splitHorizon.Family, Does.Contain("Rare"));
        Assert.That(solarShot.UnlockHint, Does.Contain("Heat 36"));
        Assert.That(solarShot.Description, Does.Contain("burns away"));
        Assert.That(wrapRail.UnlockHint, Does.Contain("Heat 37"));
        Assert.That(wrapRail.Description, Does.Contain("exits one side wall"));
        Assert.That(staticShoes.UnlockHint, Does.Contain("Heat 38"));
        Assert.That(staticShoes.Description, Does.Contain("Paddle movement x0.60"));
        Assert.That(jackpotJam.UnlockHint, Does.Contain("Heat 39"));
        Assert.That(jackpotJam.Description, Does.Contain("Score x3.00"));
        Assert.That(rewindCatch.UnlockHint, Does.Contain("Heat 40"));
        Assert.That(rewindCatch.Description, Does.Contain("rewinds to its last paddle hit"));
        Assert.That(microSpark.UnlockHint, Does.Contain("Heat 41"));
        Assert.That(microSpark.Description, Does.Contain("Ball size x0.55"));
        Assert.That(microSpark.Description, Does.Contain("score x1.75"));
        Assert.That(brickBloom.UnlockHint, Does.Contain("Heat 42"));
        Assert.That(brickBloom.Description, Does.Contain("spawns 2 tiny bonus bricks"));
        Assert.That(doubleTap.UnlockHint, Does.Contain("Heat 44"));
        Assert.That(doubleTap.Description, Does.Contain("2 angled copy balls"));
        Assert.That(fuseBurst.UnlockHint, Does.Contain("Heat 45"));
        Assert.That(fuseBurst.Description, Does.Contain("Clears one damaged brick"));
        Assert.That(overdriveTape.UnlockHint, Does.Contain("Heat 46"));
        Assert.That(overdriveTape.Description, Does.Contain("capsules all move x1.25"));
        Assert.That(bogusBounce.UnlockHint, Does.Contain("Heat 48"));
        Assert.That(bogusBounce.Description, Does.Contain("3 wall bounces"));
        Assert.That(cabinetJackpot.UnlockHint, Does.Contain("Heat 49"));
        Assert.That(cabinetJackpot.Description, Does.Contain("Refreshes every active timed effect"));
        Assert.That(finalBreakthru.UnlockHint, Does.Contain("Heat 50"));
        Assert.That(finalBreakthru.Description, Does.Contain("pierces weak bricks"));
    }

    [Test]
    public void ProgressionPageShowsJackpotJamAsLiveHeatThirtyNineDrop()
    {
        var jackpotJam = Resources.Load<PowerUpDefinition>("PowerUps/JackpotJam");
        Assert.That(jackpotJam, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { jackpotJam });
        var card = view.Cards.First(item => item.Title == "Jackpot Jam");

        Assert.That(card.UnlockHint, Does.Contain("Heat 39"));
        Assert.That(card.Description, Does.Contain("Score x3.00"));
        Assert.That(card.Description, Does.Contain("ball speed x1.35"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Jackpot Jam"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsStaticShoesAsLiveHeatThirtyEightHazardDrop()
    {
        var staticShoes = Resources.Load<PowerUpDefinition>("PowerUps/StaticShoes");
        Assert.That(staticShoes, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { staticShoes });
        var card = view.Cards.First(item => item.Title == "Static Shoes");

        Assert.That(card.UnlockHint, Does.Contain("Heat 38"));
        Assert.That(card.Description, Does.Contain("Paddle movement x0.60"));
        Assert.That(card.Family, Does.Contain("Epic Hazard"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Static Shoes"), Is.EqualTo(1));
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
    public void ProgressionPageShowsRewindCatchAsLiveHeatFortyDrop()
    {
        var rewindCatch = Resources.Load<PowerUpDefinition>("PowerUps/RewindCatch");
        Assert.That(rewindCatch, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { rewindCatch });
        var card = view.Cards.First(item => item.Title == "Rewind Catch");

        Assert.That(card.UnlockHint, Does.Contain("Heat 40"));
        Assert.That(card.Description, Does.Contain("rewinds to its last paddle hit"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Rewind Catch"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsMicroSparkAsLiveHeatFortyOneDrop()
    {
        var microSpark = Resources.Load<PowerUpDefinition>("PowerUps/MicroSpark");
        Assert.That(microSpark, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { microSpark });
        var card = view.Cards.First(item => item.Title == "Micro Spark");

        Assert.That(card.UnlockHint, Does.Contain("Heat 41"));
        Assert.That(card.Description, Does.Contain("Ball size x0.55"));
        Assert.That(card.Description, Does.Contain("score x1.75"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Micro Spark"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsBrickBloomAsLiveHeatFortyTwoDrop()
    {
        var brickBloom = Resources.Load<PowerUpDefinition>("PowerUps/BrickBloom");
        Assert.That(brickBloom, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { brickBloom });
        var card = view.Cards.First(item => item.Title == "Brick Bloom");

        Assert.That(card.UnlockHint, Does.Contain("Heat 42"));
        Assert.That(card.Description, Does.Contain("spawns 2 tiny bonus bricks"));
        Assert.That(card.Family, Does.Contain("Epic Mixed"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Brick Bloom"), Is.EqualTo(1));
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
    public void ProgressionPageShowsSolarShotAsLiveHeatThirtySixDrop()
    {
        var solarShot = Resources.Load<PowerUpDefinition>("PowerUps/SolarShot");
        Assert.That(solarShot, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { solarShot });
        var card = view.Cards.First(item => item.Title == "Solar Shot");

        Assert.That(card.UnlockHint, Does.Contain("Heat 36"));
        Assert.That(card.Description, Does.Contain("weak brick"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Solar Shot"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsWrapRailAsLiveHeatThirtySevenDrop()
    {
        var wrapRail = Resources.Load<PowerUpDefinition>("PowerUps/WrapRail");
        Assert.That(wrapRail, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { wrapRail });
        var card = view.Cards.First(item => item.Title == "Wrap Rail");

        Assert.That(card.UnlockHint, Does.Contain("Heat 37"));
        Assert.That(card.Description, Does.Contain("exits one side wall"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Wrap Rail"), Is.EqualTo(1));
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
    public void ProgressionPageShowsMagnetFlipAsLiveEpicHazardDrop()
    {
        var magnetFlip = Resources.Load<PowerUpDefinition>("PowerUps/MagnetFlip");
        Assert.That(magnetFlip, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { magnetFlip });
        var card = view.Cards.First(item => item.Title == "Magnet Flip");

        Assert.That(card.UnlockHint, Does.Contain("Heat 43"));
        Assert.That(card.Description, Does.Contain("pushed away from nearby bricks"));
        Assert.That(card.Family, Does.Contain("Epic Hazard"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
    }

    [Test]
    public void ProgressionPageShowsDoubleTapAsLiveHeatFortyFourDrop()
    {
        var doubleTap = Resources.Load<PowerUpDefinition>("PowerUps/DoubleTap");
        Assert.That(doubleTap, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { doubleTap });
        var card = view.Cards.First(item => item.Title == "Double Tap");

        Assert.That(card.UnlockHint, Does.Contain("Heat 44"));
        Assert.That(card.Description, Does.Contain("2 angled copy balls"));
        Assert.That(card.Description, Does.Contain("paddle width x0.72"));
        Assert.That(card.Family, Does.Contain("Epic Mixed"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Double Tap"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsFuseBurstAsLiveHeatFortyFiveDrop()
    {
        var fuseBurst = Resources.Load<PowerUpDefinition>("PowerUps/FuseBurst");
        Assert.That(fuseBurst, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { fuseBurst });
        var card = view.Cards.First(item => item.Title == "Fuse Burst");

        Assert.That(card.UnlockHint, Does.Contain("Heat 45"));
        Assert.That(card.Description, Does.Contain("Clears one damaged brick"));
        Assert.That(card.Description, Does.Contain("black"));
        Assert.That(card.Family, Does.Contain("Epic Mixed"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Fuse Burst"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsOverdriveTapeAsLiveHeatFortySixDrop()
    {
        var overdriveTape = Resources.Load<PowerUpDefinition>("PowerUps/OverdriveTape");
        Assert.That(overdriveTape, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { overdriveTape });
        var card = view.Cards.First(item => item.Title == "Overdrive Tape");

        Assert.That(card.UnlockHint, Does.Contain("Heat 46"));
        Assert.That(card.Description, Does.Contain("Ball, paddle, and capsules"));
        Assert.That(card.Description, Does.Contain("x1.25"));
        Assert.That(card.Family, Does.Contain("Epic Mixed"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Overdrive Tape"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsBogusBounceAsLiveHeatFortyEightDrop()
    {
        var bogusBounce = Resources.Load<PowerUpDefinition>("PowerUps/BogusBounce");
        Assert.That(bogusBounce, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { bogusBounce });
        var card = view.Cards.First(item => item.Title == "Bogus Bounce");

        Assert.That(card.UnlockHint, Does.Contain("Heat 48"));
        Assert.That(card.Description, Does.Contain("3 wall bounces"));
        Assert.That(card.Description, Does.Contain("wild angles"));
        Assert.That(card.Family, Does.Contain("Epic Hazard"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Bogus Bounce"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsCabinetJackpotAsLiveHeatFortyNineDrop()
    {
        var cabinetJackpot = Resources.Load<PowerUpDefinition>("PowerUps/CabinetJackpot");
        Assert.That(cabinetJackpot, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { cabinetJackpot });
        var card = view.Cards.First(item => item.Title == "Cabinet Jackpot");

        Assert.That(card.UnlockHint, Does.Contain("Heat 49"));
        Assert.That(card.Description, Does.Contain("Refreshes every active timed effect"));
        Assert.That(card.Family, Does.Contain("Epic Mixed"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Cabinet Jackpot"), Is.EqualTo(1));
    }

    [Test]
    public void ProgressionPageShowsFinalBreakthruAsLiveHeatFiftyDrop()
    {
        var finalBreakthru = Resources.Load<PowerUpDefinition>("PowerUps/FinalBreakthru");
        Assert.That(finalBreakthru, Is.Not.Null);

        var view = new BreakoutProgressionPageService().BuildView(new[] { finalBreakthru });
        var card = view.Cards.First(item => item.Title == "Final Breakthru");

        Assert.That(card.UnlockHint, Does.Contain("Heat 50"));
        Assert.That(card.Description, Does.Contain("pierces weak bricks"));
        Assert.That(card.Description, Does.Contain("x2.00"));
        Assert.That(card.Family, Does.Contain("Epic Helpful"));
        Assert.That(card.StateLabel, Is.EqualTo("Locked"));
        Assert.That(view.Cards.Count(item => item.Title == "Final Breakthru"), Is.EqualTo(1));
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
