using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;

public sealed class BreakoutLevelGlitchPlannerTests
{
    [Test]
    public void CustomRunsOnlyRollGlitchesWhenEnabled()
    {
        var cleanSettings = CreateSettings(levelGlitchesEnabled: false);
        var glitchedSettings = CreateSettings(levelGlitchesEnabled: true);

        Assert.That(BreakoutLevelGlitchPlanner.GetGlitchChance(cleanSettings, levelIndex: 5), Is.Zero);
        Assert.That(BreakoutLevelGlitchPlanner.GetGlitchChance(glitchedSettings, levelIndex: 5), Is.GreaterThan(0f));
    }

    [Test]
    public void RogueGlitchesStartAfterFirstHeatClear()
    {
        var earlySettings = CreateRogueSettings(rogueIntensity: 1);
        var harderSettings = CreateRogueSettings(rogueIntensity: 2);

        Assert.That(BreakoutLevelGlitchPlanner.GetGlitchChance(earlySettings, levelIndex: 5), Is.Zero);
        Assert.That(BreakoutLevelGlitchPlanner.GetGlitchChance(harderSettings, levelIndex: 5), Is.GreaterThan(0f));
    }

    [Test]
    public void RogueFinalStageAlwaysRollsGlitch()
    {
        var settings = CreateRogueSettings(rogueIntensity: 1);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), settings, levelIndex: 9);

        Assert.That(BreakoutLevelGlitchPlanner.GetGlitchChance(settings, levelIndex: 9), Is.EqualTo(1f));
        Assert.That(plan.IsActive, Is.True);
    }

    [Test]
    public void RogueGlitchHeatControlsWhenTurboRailCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.TurboRailLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.TurboRail);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.TurboRailLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.TurboRail);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TurboRail));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenMirrorGridCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.MirrorGridLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.MirrorGrid);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.MirrorGridLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.MirrorGrid);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.MirrorGrid));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Mirror Grid"));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.3f).Within(0.0001f));
        Assert.That(unlockedPlan.WarpGates, Is.Empty);
    }

    [Test]
    public void RogueGlitchHeatControlsWhenGravityPocketCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.GravityPocketLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.GravityPocket);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.GravityPocketLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.GravityPocket);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.GravityPocket));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Gravity Pocket"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.38f).Within(0.0001f));
        Assert.That(unlockedPlan.GravityPocket.Radius, Is.InRange(1.9f, 2.45f));
        Assert.That(unlockedPlan.GravityPocket.Strength, Is.InRange(0.58f, 0.78f));
        Assert.That(unlockedPlan.GravityPocket.DriftSpeed, Is.InRange(0.18f, 0.28f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenTokenStormCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.TokenStormLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.TokenStorm);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.TokenStormLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.TokenStorm);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TokenStorm));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Token Storm"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.32f).Within(0.0001f));
        Assert.That(unlockedPlan.TokenStorm.DropChanceMultiplier, Is.EqualTo(1.65f).Within(0.0001f));
        Assert.That(unlockedPlan.TokenStorm.MinimumFallSpeedMultiplier, Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(unlockedPlan.TokenStorm.MaximumFallSpeedMultiplier, Is.EqualTo(1.45f).Within(0.0001f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenStaticWallCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.StaticWallLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.StaticWall);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.StaticWallLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.StaticWall);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.StaticWall));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Static Wall"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.34f).Within(0.0001f));
        Assert.That(unlockedPlan.StaticWall.Wall, Is.EqualTo(BreakoutWarpGateWall.Left).Or.EqualTo(BreakoutWarpGateWall.Right));
        Assert.That(unlockedPlan.StaticWall.WeakCycleSeconds, Is.InRange(2.1f, 2.85f));
        Assert.That(unlockedPlan.StaticWall.WeakDurationSeconds, Is.InRange(0.72f, 1.05f));
    }


    [Test]
    public void ForcedWarpGatePlanBuildsSmallPortalSetAndScoreBonus()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 3f,
            levelGlitchSelection: LevelGlitchSelection.WarpGates);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(2), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.WarpGates));
        Assert.That(plan.WarpGates.Length, Is.InRange(2, 4));
        Assert.That(plan.ScoreMultiplier, Is.GreaterThan(1f));
    }

    [Test]
    public void ForcedTurboRailPlanBuildsWallSectionAndScoreBonus()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 3f,
            levelGlitchSelection: LevelGlitchSelection.TurboRail);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TurboRail));
        Assert.That(plan.DisplayName, Is.EqualTo("Turbo Rail"));
        Assert.That(plan.TurboRail.NormalizedPosition, Is.InRange(0.18f, 0.82f));
        Assert.That(plan.TurboRail.NormalizedLength, Is.InRange(0.18f, 0.32f));
        Assert.That(plan.ScoreMultiplier, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(plan.WarpGates, Is.Empty);
    }

    [Test]
    public void SelectedTurboRailAlwaysBuildsTurboRailEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.TurboRail);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(2), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TurboRail));
    }

    [Test]
    public void SelectedWarpGatesAlwaysBuildsWarpGatesEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.WarpGates);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.WarpGates));
    }

    [Test]
    public void SelectedMirrorGridAlwaysBuildsMirrorGridEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.MirrorGrid);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.MirrorGrid));
    }

    [Test]
    public void SelectedGravityPocketAlwaysBuildsGravityPocketEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.GravityPocket);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.GravityPocket));
    }

    [Test]
    public void SelectedTokenStormAlwaysBuildsTokenStormEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.TokenStorm);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TokenStorm));
    }

    [Test]
    public void SelectedStaticWallAlwaysBuildsStaticWallEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.StaticWall);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.StaticWall));
    }


    [Test]
    public void ForcedDeveloperGlitchBypassesRogueHeatUnlock()
    {
        var settings = new RunSettings(
            1234,
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
            rogueIntensity: 1,
            levelGlitchesEnabled: true,
            levelGlitchChanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.MirrorGrid,
            forceLevelGlitchRoll: true,
            ignoreLevelGlitchUnlocks: true);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 0);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.MirrorGrid));
    }

    [Test]
    public void DeveloperHeatUnlockOverrideAllowsGlitchesForSelectedHeat()
    {
        var normalHeatFiveSettings = new RunSettings(
            1234,
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
            rogueIntensity: 5,
            levelGlitchesEnabled: true,
            levelGlitchChanceMultiplier: 1f,
            levelGlitchSelection: LevelGlitchSelection.StaticWall,
            forceLevelGlitchRoll: true);
        var developerHeatFiveSettings = new RunSettings(
            1234,
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
            rogueIntensity: 5,
            levelGlitchesEnabled: true,
            levelGlitchChanceMultiplier: 1f,
            levelGlitchSelection: LevelGlitchSelection.StaticWall,
            forceLevelGlitchRoll: true,
            levelGlitchUnlockIntensityOverride: 5);

        var normalPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), normalHeatFiveSettings, levelIndex: 0);
        var developerPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), developerHeatFiveSettings, levelIndex: 0);

        Assert.That(normalPlan.IsActive, Is.False);
        Assert.That(developerPlan.IsActive, Is.True);
        Assert.That(developerPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.StaticWall));
    }

    [Test]
    public void RandomSelectionStillUsesGlitchChance()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.Random);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.False);
    }

    [Test]
    public void RogueGlitchChanceRisesWithHeatAndLevelPressure()
    {
        var lowHeatEarlyStage = CreateRogueSettings(rogueIntensity: 2);
        var highHeatEarlyStage = CreateRogueSettings(rogueIntensity: 50);
        var highHeatLateStage = CreateRogueSettings(rogueIntensity: 50);

        var lowChance = BreakoutLevelGlitchPlanner.GetGlitchChance(lowHeatEarlyStage, levelIndex: 1);
        var highEarlyChance = BreakoutLevelGlitchPlanner.GetGlitchChance(highHeatEarlyStage, levelIndex: 1);
        var highLateChance = BreakoutLevelGlitchPlanner.GetGlitchChance(highHeatLateStage, levelIndex: 8);

        Assert.That(highEarlyChance, Is.GreaterThan(lowChance));
        Assert.That(highLateChance, Is.GreaterThan(highEarlyChance));
    }

    [Test]
    public void HighHeatRandomGlitchesCanStackDistinctGlitches()
    {
        var settings = new RunSettings(
            1234,
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
            rogueIntensity: 50,
            levelGlitchesEnabled: true,
            levelGlitchSelection: LevelGlitchSelection.Random,
            levelGlitchUnlockIntensityOverride: 50);
        BreakoutLevelGlitchPlan stackedPlan = null;

        for (var seed = 1; seed <= 100; seed++)
        {
            var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(seed), settings, levelIndex: 9);

            if (plan.ActiveGlitchTypes.Length > 1)
            {
                stackedPlan = plan;
                break;
            }
        }

        Assert.That(stackedPlan, Is.Not.Null);
        Assert.That(stackedPlan.ActiveGlitchTypes.Length, Is.GreaterThanOrEqualTo(2));
        Assert.That(stackedPlan.ScoreMultiplier, Is.GreaterThan(1.4f));
        Assert.That(stackedPlan.HudLabel, Does.Contain("Glitch Stack"));
    }

    [Test]
    public void ForcedGlitchesStaySingleEvenAtHighHeat()
    {
        var settings = CreateRogueSettings(
            rogueIntensity: 50,
            levelGlitchSelection: LevelGlitchSelection.StaticWall);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.ActiveGlitchTypes, Is.EqualTo(new[] { BreakoutLevelGlitchType.StaticWall }));
    }

    private static RunSettings CreateSettings(
        bool levelGlitchesEnabled,
        float chanceMultiplier = 1f,
        LevelGlitchSelection levelGlitchSelection = LevelGlitchSelection.Random)
    {
        return new RunSettings(
            1234,
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
            levelGlitchesEnabled: levelGlitchesEnabled,
            levelGlitchChanceMultiplier: chanceMultiplier,
            levelGlitchSelection: levelGlitchSelection);
    }

    private static RunSettings CreateRogueSettings(
        int rogueIntensity,
        LevelGlitchSelection levelGlitchSelection = LevelGlitchSelection.Random)
    {
        return new RunSettings(
            1234,
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
            rogueIntensity,
            levelGlitchesEnabled: true,
            levelGlitchSelection: levelGlitchSelection);
    }
}
