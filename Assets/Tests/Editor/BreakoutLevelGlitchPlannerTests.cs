using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

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
    public void RogueGlitchHeatControlsWhenRowRewriteCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RowRewriteLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.RowRewrite);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RowRewriteLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.RowRewrite);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RowRewrite));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Row Rewrite"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.28f).Within(0.0001f));
        Assert.That(unlockedPlan.RowRewrite.NormalizedRow, Is.InRange(0.12f, 0.78f));
        Assert.That(unlockedPlan.RowRewrite.TriggerSeconds, Is.InRange(8.5f, 13.5f));
        Assert.That(unlockedPlan.RowRewrite.FillChance, Is.InRange(0.54f, 0.76f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenPrismLanesCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.PrismLanesLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.PrismLanes);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.PrismLanesLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.PrismLanes);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.PrismLanes));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Prism Lanes"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.31f).Within(0.0001f));
        Assert.That(unlockedPlan.PrismLanes.Length, Is.InRange(2, 3));
        Assert.That(unlockedPlan.PrismLanes[0].NormalizedX, Is.InRange(0.1f, 0.9f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenSwitchbackRailsCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.SwitchbackRailsLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.SwitchbackRails);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.SwitchbackRailsLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.SwitchbackRails);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.SwitchbackRails));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Switchback Rails"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.29f).Within(0.0001f));
        Assert.That(unlockedPlan.SwitchbackRails.NormalizedPosition, Is.InRange(0.42f, 0.72f));
        Assert.That(unlockedPlan.SwitchbackRails.NormalizedLength, Is.InRange(0.34f, 0.48f));
        Assert.That(unlockedPlan.SwitchbackRails.SwitchCycleSeconds, Is.InRange(2.35f, 3.15f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenCapsuleRouletteCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.CapsuleRouletteLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.CapsuleRoulette);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.CapsuleRouletteLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.CapsuleRoulette);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.CapsuleRoulette));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Capsule Roulette"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.27f).Within(0.0001f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenDriftRowsCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.DriftRowsLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.DriftRows);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.DriftRowsLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.DriftRows);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.DriftRows));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Drift Rows"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.26f).Within(0.0001f));
        Assert.That(unlockedPlan.DriftRows.Speed, Is.InRange(0.26f, 0.38f));
        Assert.That(Mathf.Abs(unlockedPlan.DriftRows.StartingDirectionSign), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenHotCornersCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.HotCornersLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.HotCorners);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.HotCornersLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.HotCorners);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.HotCorners));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Hot Corners"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.28f).Within(0.0001f));
        Assert.That(unlockedPlan.HotCorners.BumperSize, Is.InRange(0.72f, 0.94f));
        Assert.That(unlockedPlan.HotCorners.SpeedBurstMultiplier, Is.InRange(1.16f, 1.28f));
        Assert.That(unlockedPlan.HotCorners.SpeedBurstDurationSeconds, Is.InRange(2.35f, 3.25f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenFlickerBricksCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.FlickerBricksLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.FlickerBricks);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.FlickerBricksLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.FlickerBricks);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.FlickerBricks));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Flicker Bricks"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.3f).Within(0.0001f));
        Assert.That(unlockedPlan.FlickerBricks.AffectedBrickChance, Is.InRange(0.36f, 0.48f));
        Assert.That(unlockedPlan.FlickerBricks.VisibleSeconds, Is.InRange(1.25f, 1.65f));
        Assert.That(unlockedPlan.FlickerBricks.HiddenSeconds, Is.InRange(0.58f, 0.88f));
        Assert.That(unlockedPlan.FlickerBricks.HiddenAlpha, Is.InRange(0.035f, 0.075f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenCassetteSkipCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.CassetteSkipLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.CassetteSkip);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.CassetteSkipLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.CassetteSkip);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.CassetteSkip));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Cassette Skip"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.27f).Within(0.0001f));
        Assert.That(unlockedPlan.CassetteSkip.PaddleHitsPerSkip, Is.InRange(2, 3));
        Assert.That(unlockedPlan.CassetteSkip.SkipDistance, Is.InRange(1.45f, 2.05f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenGhostRowCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.GhostRowLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.GhostRow);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.GhostRowLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.GhostRow);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.GhostRow));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Ghost Row"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.29f).Within(0.0001f));
        Assert.That(unlockedPlan.GhostRow.NormalizedRow, Is.InRange(0.16f, 0.82f));
        Assert.That(unlockedPlan.GhostRow.PhaseDurationSeconds, Is.InRange(2.25f, 3.35f));
        Assert.That(unlockedPlan.GhostRow.CooldownSeconds, Is.InRange(0.85f, 1.45f));
        Assert.That(unlockedPlan.GhostRow.HiddenAlpha, Is.InRange(0.08f, 0.14f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenSplitHorizonCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.SplitHorizonLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.SplitHorizon);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.SplitHorizonLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.SplitHorizon);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.SplitHorizon));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Split Horizon"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Rare));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.3f).Within(0.0001f));
        Assert.That(unlockedPlan.SplitHorizon.NormalizedY, Is.InRange(0.44f, 0.56f));
        Assert.That(unlockedPlan.SplitHorizon.BendDegrees, Is.InRange(7.5f, 10.5f));
        Assert.That(unlockedPlan.SplitHorizon.CooldownSeconds, Is.InRange(0.1f, 0.16f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenRogueGateCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RogueGateLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.RogueGate);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RogueGateLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.RogueGate);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RogueGate));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Rogue Gate"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.36f).Within(0.0001f));
        Assert.That(unlockedPlan.WarpGates, Has.Length.EqualTo(1));
        Assert.That(unlockedPlan.WarpGates[0].NormalizedPosition, Is.InRange(0.18f, 0.82f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenBrickConveyorCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.BrickConveyorLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.BrickConveyor);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.BrickConveyorLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.BrickConveyor);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.BrickConveyor));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Brick Conveyor"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(unlockedPlan.BrickConveyor.Speed, Is.InRange(0.34f, 0.48f));
        Assert.That(Mathf.Abs(unlockedPlan.BrickConveyor.StartingDirectionSign), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(unlockedPlan.BrickConveyor.WrapPadding, Is.InRange(0.35f, 0.65f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenPickupPinballCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.PickupPinballLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.PickupPinball);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.PickupPinballLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.PickupPinball);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.PickupPinball));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Pickup Pinball"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.33f).Within(0.0001f));
        Assert.That(unlockedPlan.PickupPinball.LateralVelocityMultiplier, Is.EqualTo(0.82f).Within(0.0001f));
        Assert.That(unlockedPlan.PickupPinball.UpwardVelocityMultiplier, Is.EqualTo(0.62f).Within(0.0001f));
        Assert.That(unlockedPlan.PickupPinball.GravityMultiplier, Is.EqualTo(1.18f).Within(0.0001f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenMagnetStormCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.MagnetStormLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.MagnetStorm);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.MagnetStormLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.MagnetStorm);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.MagnetStorm));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Magnet Storm"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.4f).Within(0.0001f));
        Assert.That(unlockedPlan.MagnetStormPockets, Has.Length.EqualTo(3));
        Assert.That(unlockedPlan.MagnetStormPockets[0].Radius, Is.InRange(1.45f, 1.85f));
        Assert.That(unlockedPlan.MagnetStormPockets[0].Strength, Is.InRange(0.44f, 0.58f));
        Assert.That(unlockedPlan.MagnetStormPockets[0].DriftSpeed, Is.InRange(0.22f, 0.36f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenBlacklightBricksCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.BlacklightBricksLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.BlacklightBricks);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.BlacklightBricksLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.BlacklightBricks);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.BlacklightBricks));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Blacklight Bricks"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.36f).Within(0.0001f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenRewindWallCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RewindWallLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.RewindWall);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.RewindWallLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.RewindWall);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RewindWall));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Rewind Wall"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.37f).Within(0.0001f));
        Assert.That(unlockedPlan.RewindWall.NormalizedRow, Is.InRange(0.18f, 0.84f));
        Assert.That(unlockedPlan.RewindWall.RebuildDelaySeconds, Is.InRange(1.65f, 2.65f));
        Assert.That(unlockedPlan.RewindWall.WarningSeconds, Is.InRange(0.55f, 0.95f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenScoreLeakCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.ScoreLeakLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.ScoreLeak);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.ScoreLeakLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.ScoreLeak);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.ScoreLeak));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Score Leak"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.42f).Within(0.0001f));
        Assert.That(unlockedPlan.ScoreLeak.PointsPerSecond, Is.InRange(10f, 16f));
        Assert.That(unlockedPlan.ScoreLeak.GraceSeconds, Is.InRange(1.15f, 1.75f));
    }

    [Test]
    public void RogueGlitchHeatControlsWhenLaserRainCanUnlock()
    {
        var lockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.LaserRainLadderUnlockIntensity,
            levelGlitchSelection: LevelGlitchSelection.LaserRain);
        var unlockedSettings = CreateRogueSettings(
            rogueIntensity: BreakoutLevelGlitchPlanner.LaserRainLadderUnlockIntensity + 1,
            levelGlitchSelection: LevelGlitchSelection.LaserRain);

        var lockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), lockedSettings, levelIndex: 9);
        var unlockedPlan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(7), unlockedSettings, levelIndex: 9);

        Assert.That(lockedPlan.IsActive, Is.False);
        Assert.That(unlockedPlan.IsActive, Is.True);
        Assert.That(unlockedPlan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.LaserRain));
        Assert.That(unlockedPlan.DisplayName, Is.EqualTo("Laser Rain"));
        Assert.That(unlockedPlan.Rarity, Is.EqualTo(BreakoutContentRarity.Epic));
        Assert.That(unlockedPlan.ScoreMultiplier, Is.EqualTo(1.44f).Within(0.0001f));
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
    public void SelectedRowRewriteAlwaysBuildsRowRewriteEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.RowRewrite);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RowRewrite));
    }

    [Test]
    public void SelectedPrismLanesAlwaysBuildsPrismLanesEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.PrismLanes);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.PrismLanes));
        Assert.That(plan.PrismLanes, Is.Not.Empty);
    }

    [Test]
    public void SelectedSwitchbackRailsAlwaysBuildsSwitchbackRailsEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.SwitchbackRails);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.SwitchbackRails));
        Assert.That(plan.SwitchbackRails.SwitchCycleSeconds, Is.GreaterThan(0f));
    }

    [Test]
    public void SelectedCapsuleRouletteAlwaysBuildsCapsuleRouletteEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.CapsuleRoulette);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.CapsuleRoulette));
        Assert.That(plan.HudLabel, Does.Contain("Capsule Roulette"));
    }

    [Test]
    public void SelectedDriftRowsAlwaysBuildsDriftRowsEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.DriftRows);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.DriftRows));
        Assert.That(plan.HudLabel, Does.Contain("Drift Rows"));
    }

    [Test]
    public void SelectedHotCornersAlwaysBuildsHotCornersEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.HotCorners);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.HotCorners));
        Assert.That(plan.HudLabel, Does.Contain("Hot Corners"));
    }

    [Test]
    public void SelectedFlickerBricksAlwaysBuildsFlickerBricksEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.FlickerBricks);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.FlickerBricks));
        Assert.That(plan.HudLabel, Does.Contain("Flicker Bricks"));
    }

    [Test]
    public void SelectedCassetteSkipAlwaysBuildsCassetteSkipEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.CassetteSkip);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.CassetteSkip));
        Assert.That(plan.HudLabel, Does.Contain("Cassette Skip"));
    }

    [Test]
    public void SelectedGhostRowAlwaysBuildsGhostRowEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.GhostRow);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.GhostRow));
        Assert.That(plan.HudLabel, Does.Contain("Ghost Row"));
    }

    [Test]
    public void SelectedSplitHorizonAlwaysBuildsSplitHorizonEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.SplitHorizon);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.SplitHorizon));
        Assert.That(plan.HudLabel, Does.Contain("Split Horizon"));
    }

    [Test]
    public void SelectedRogueGateAlwaysBuildsSingleMovingGateEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.RogueGate);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RogueGate));
        Assert.That(plan.HudLabel, Does.Contain("Rogue Gate"));
        Assert.That(plan.WarpGates, Has.Length.EqualTo(1));
    }

    [Test]
    public void SelectedBrickConveyorAlwaysBuildsBrickConveyorEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.BrickConveyor);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.BrickConveyor));
        Assert.That(plan.HudLabel, Does.Contain("Brick Conveyor"));
        Assert.That(plan.BrickConveyor.Speed, Is.GreaterThan(0f));
    }

    [Test]
    public void SelectedPickupPinballAlwaysBuildsPickupPinballEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.PickupPinball);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.PickupPinball));
        Assert.That(plan.HudLabel, Does.Contain("Pickup Pinball"));
    }

    [Test]
    public void SelectedMagnetStormAlwaysBuildsMagnetStormEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.MagnetStorm);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.MagnetStorm));
        Assert.That(plan.HudLabel, Does.Contain("Magnet Storm"));
        Assert.That(plan.MagnetStormPockets, Has.Length.EqualTo(3));
    }

    [Test]
    public void SelectedBlacklightBricksAlwaysBuildsBlacklightBricksEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.BlacklightBricks);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.BlacklightBricks));
        Assert.That(plan.HudLabel, Does.Contain("Blacklight Bricks"));
    }

    [Test]
    public void SelectedRewindWallAlwaysBuildsRewindWallEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.RewindWall);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.RewindWall));
        Assert.That(plan.HudLabel, Does.Contain("Rewind Wall"));
    }

    [Test]
    public void SelectedScoreLeakAlwaysBuildsScoreLeakEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.ScoreLeak);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.ScoreLeak));
        Assert.That(plan.HudLabel, Does.Contain("Score Leak"));
    }

    [Test]
    public void SelectedLaserRainAlwaysBuildsLaserRainEvenWhenChanceIsDisabled()
    {
        var settings = CreateSettings(
            levelGlitchesEnabled: true,
            chanceMultiplier: 0f,
            levelGlitchSelection: LevelGlitchSelection.LaserRain);

        var plan = BreakoutLevelGlitchPlanner.BuildPlan(new DeterministicRandomService(3), settings, levelIndex: 9);

        Assert.That(plan.IsActive, Is.True);
        Assert.That(plan.GlitchType, Is.EqualTo(BreakoutLevelGlitchType.LaserRain));
        Assert.That(plan.HudLabel, Does.Contain("Laser Rain"));
    }

    [Test]
    public void ScoreLeakPenaltyTricklesFromAccumulatorWithoutDroppingBelowZero()
    {
        var accumulator = 0f;

        var firstPenalty = BreakoutScoreLeakCalculator.CalculatePenalty(10, 12f, 0.08f, ref accumulator);
        var secondPenalty = BreakoutScoreLeakCalculator.CalculatePenalty(10, 12f, 0.08f, ref accumulator);
        var cappedPenalty = BreakoutScoreLeakCalculator.CalculatePenalty(1, 12f, 1f, ref accumulator);

        Assert.That(firstPenalty, Is.Zero);
        Assert.That(secondPenalty, Is.EqualTo(1));
        Assert.That(cappedPenalty, Is.EqualTo(1));
        Assert.That(accumulator, Is.GreaterThanOrEqualTo(0f));
    }

    [Test]
    public void PrismLaneRefractionBuildsSharperHorizontalDirection()
    {
        var refracted = BreakoutPrismLaneSection.BuildRefractedDirection(new UnityEngine.Vector2(0.12f, 1f), -1f);

        Assert.That(refracted.x, Is.LessThan(-0.4f));
        Assert.That(refracted.y, Is.GreaterThan(0f));
        Assert.That(refracted.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void SplitHorizonBendsBallAwayFromCenterWithoutFlippingVerticalTravel()
    {
        var refracted = BallController.BuildSplitHorizonDirection(
            new UnityEngine.Vector2(0.05f, 1f),
            worldX: 2f,
            crossingDirectionY: 1f,
            bendDegrees: 9f,
            minimumVerticalFraction: 0.35f);

        Assert.That(refracted.x, Is.GreaterThan(0.15f));
        Assert.That(refracted.y, Is.GreaterThan(0f));
        Assert.That(refracted.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void SwitchbackRailDirectionsAlternateVerticalAngle()
    {
        var upward = BreakoutSwitchbackRailSection.BuildSwitchbackDirection(new UnityEngine.Vector2(-0.3f, 0.2f), BreakoutWarpGateWall.Left, 1f);
        var downward = BreakoutSwitchbackRailSection.BuildSwitchbackDirection(new UnityEngine.Vector2(-0.3f, 0.2f), BreakoutWarpGateWall.Left, -1f);

        Assert.That(upward.x, Is.GreaterThan(0f));
        Assert.That(upward.y, Is.GreaterThan(0f));
        Assert.That(downward.x, Is.GreaterThan(0f));
        Assert.That(downward.y, Is.LessThan(0f));
        Assert.That(upward.magnitude, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(downward.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void DriftRowsAlternateHorizontalDirectionByRow()
    {
        var firstRow = BreakoutBrickService.ResolveDriftDirectionForRow(0, 1f);
        var secondRow = BreakoutBrickService.ResolveDriftDirectionForRow(1, 1f);
        var thirdRow = BreakoutBrickService.ResolveDriftDirectionForRow(2, -1f);

        Assert.That(firstRow.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(secondRow.x, Is.EqualTo(-1f).Within(0.0001f));
        Assert.That(thirdRow.x, Is.EqualTo(-1f).Within(0.0001f));
        Assert.That(firstRow.y, Is.Zero);
    }

    [Test]
    public void BrickConveyorAlternatesDirectionByTwoRowBands()
    {
        var firstRow = BreakoutBrickService.ResolveConveyorDirectionForRow(0, 1f);
        var secondRow = BreakoutBrickService.ResolveConveyorDirectionForRow(1, 1f);
        var thirdRow = BreakoutBrickService.ResolveConveyorDirectionForRow(2, 1f);
        var fourthRow = BreakoutBrickService.ResolveConveyorDirectionForRow(3, -1f);

        Assert.That(firstRow.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(secondRow.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(thirdRow.x, Is.EqualTo(-1f).Within(0.0001f));
        Assert.That(fourthRow.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(firstRow.y, Is.Zero);
    }

    [Test]
    public void HotCornerBumperKicksTowardCenter()
    {
        var direction = BreakoutHotCornerBumper.BuildKickDirection(
            new UnityEngine.Vector2(-5f, 5f),
            new UnityEngine.Vector2(0f, 1.2f),
            new UnityEngine.Vector2(-0.4f, 0.6f));

        Assert.That(direction.x, Is.GreaterThan(0f));
        Assert.That(direction.y, Is.LessThan(0f));
        Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void PickupPinballReflectsCapsulesOffBrickFaces()
    {
        var velocity = PowerUpPickup.BuildPinballBounceVelocity(
            new UnityEngine.Vector2(0.25f, -3.2f),
            UnityEngine.Vector2.up,
            3.2f,
            0.88f);

        Assert.That(velocity.y, Is.GreaterThan(0f));
        Assert.That(Mathf.Abs(velocity.x), Is.GreaterThan(0.5f));
        Assert.That(velocity.magnitude, Is.LessThanOrEqualTo(3.2f * 1.65f + 0.0001f));
    }

    [Test]
    public void PickupPinballNormalResolvesNearestBrickFace()
    {
        var bounds = new Bounds(UnityEngine.Vector3.zero, new UnityEngine.Vector3(2f, 1f, 1f));

        var topNormal = PowerUpPickup.ResolvePinballBounceNormal(new UnityEngine.Vector2(0.1f, 0.45f), bounds);
        var leftNormal = PowerUpPickup.ResolvePinballBounceNormal(new UnityEngine.Vector2(-0.95f, 0f), bounds);

        Assert.That(topNormal, Is.EqualTo(UnityEngine.Vector2.up));
        Assert.That(leftNormal, Is.EqualTo(UnityEngine.Vector2.left));
    }

    [Test]
    public void MagnetStormPullStepTugsCapsulesTowardPocket()
    {
        var pullStep = PowerUpPickup.BuildMagnetStormPullStep(
            new UnityEngine.Vector2(0f, 0f),
            new UnityEngine.Vector2(1f, 1f),
            2f,
            0.55f,
            3.2f,
            0.02f);

        Assert.That(pullStep.x, Is.GreaterThan(0f));
        Assert.That(pullStep.y, Is.GreaterThan(0f));
        Assert.That(pullStep.magnitude, Is.GreaterThan(0f));
        Assert.That(pullStep.magnitude, Is.LessThanOrEqualTo(3.2f * 1.15f * 0.02f + 0.0001f));
    }

    [Test]
    public void BrickFlickerDisablesColliderWhenHidden()
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        var brickObject = new GameObject("Flicker Brick Test");
        var visualObject = new GameObject("Visual");

        try
        {
            var collider = brickObject.AddComponent<BoxCollider2D>();
            visualObject.transform.SetParent(brickObject.transform, false);
            var renderer = visualObject.AddComponent<SpriteRenderer>();
            var brick = brickObject.AddComponent<Brick>();
            brick.Initialize(
                null,
                definition,
                1,
                new ThemeVisualStyle(Color.white, Color.gray, null),
                0f,
                Vector2.zero);

            var visibleSeconds = 0.1f;
            var hiddenSeconds = 10f;
            var cycleSeconds = visibleSeconds + hiddenSeconds;
            var hiddenPhase = Mathf.Repeat((visibleSeconds + 0.1f) - Time.time, cycleSeconds);

            brick.SetFlicker(visibleSeconds, hiddenSeconds, 0.05f, hiddenPhase);

            Assert.That(collider.enabled, Is.False);
            Assert.That(renderer.color.a, Is.EqualTo(0.05f).Within(0.001f));

            brick.ClearFlicker();

            Assert.That(collider.enabled, Is.True);
            Assert.That(renderer.color.a, Is.EqualTo(1f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(brickObject);
            Object.DestroyImmediate(definition);
        }
    }

    [Test]
    public void BlacklightBrickSpriteResourceIsAvailable()
    {
        var sprite = Resources.Load<Sprite>("Sprites/blacklight-brick");

        Assert.That(sprite, Is.Not.Null);
    }

    [Test]
    public void BrickBlacklightDisguiseRevealsOnEffectContact()
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        var brickObject = new GameObject("Blacklight Brick Test");
        var visualObject = new GameObject("Visual");

        try
        {
            visualObject.transform.SetParent(brickObject.transform, false);
            var renderer = visualObject.AddComponent<SpriteRenderer>();
            var brick = brickObject.AddComponent<Brick>();
            brickObject.AddComponent<BoxCollider2D>();
            brick.Initialize(
                null,
                definition,
                2,
                new ThemeVisualStyle(Color.red, Color.gray, null),
                0f,
                Vector2.zero);

            brick.ApplyBlacklightDisguise(new ThemeVisualStyle(Color.blue, Color.blue, null));

            Assert.That(brick.IsBlacklightDisguised, Is.True);
            Assert.That(renderer.color.r, Is.EqualTo(Color.blue.r).Within(0.001f));
            Assert.That(renderer.color.b, Is.EqualTo(Color.blue.b).Within(0.001f));

            brick.ApplyEffectHit(null, BrickDestructionCause.Laser, 1);

            Assert.That(brick.IsBlacklightDisguised, Is.False);
            Assert.That(renderer.color.r, Is.EqualTo(Color.gray.r).Within(0.001f));
            Assert.That(renderer.color.g, Is.EqualTo(Color.gray.g).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(brickObject);
            Object.DestroyImmediate(definition);
        }
    }

    [Test]
    public void BrickGhostingDisablesColliderAndRestoresThroughFlickerState()
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        var brickObject = new GameObject("Ghost Row Brick Test");
        var visualObject = new GameObject("Visual");

        try
        {
            var collider = brickObject.AddComponent<BoxCollider2D>();
            visualObject.transform.SetParent(brickObject.transform, false);
            var renderer = visualObject.AddComponent<SpriteRenderer>();
            var brick = brickObject.AddComponent<Brick>();
            brick.Initialize(
                null,
                definition,
                1,
                new ThemeVisualStyle(Color.white, Color.gray, null),
                0f,
                Vector2.zero);

            brick.SetGhosted(true, 0.12f);

            Assert.That(collider.enabled, Is.False);
            Assert.That(renderer.color.a, Is.EqualTo(0.12f).Within(0.001f));

            brick.SetFlicker(10f, 0.1f, 0.05f, 0f);
            Assert.That(collider.enabled, Is.False);

            brick.SetGhosted(false);
            Assert.That(collider.enabled, Is.True);
            Assert.That(renderer.color.a, Is.EqualTo(1f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(brickObject);
            Object.DestroyImmediate(definition);
        }
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
