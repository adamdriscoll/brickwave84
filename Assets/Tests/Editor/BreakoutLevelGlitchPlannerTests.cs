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
    public void RogueGlitchesStartAtHigherHeat()
    {
        var earlySettings = CreateRogueSettings(rogueIntensity: 7);
        var harderSettings = CreateRogueSettings(rogueIntensity: 30);

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
    public void RogueGlitchRarityControlsWhenTurboRailCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(rogueIntensity: 8, levelGlitchSelection: LevelGlitchSelection.TurboRail);
        var unlockedSettings = CreateRogueSettings(rogueIntensity: 18, levelGlitchSelection: LevelGlitchSelection.TurboRail);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.TurboRail));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
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
    public void RandomSelectionStillUsesGlitchChance()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.Random);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.False);
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
