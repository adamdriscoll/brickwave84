using System;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;

public sealed class BreakoutRunSetupStateTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [Test]
    public void BuildRunSettingsCarriesCapsulePartyFlag()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("Restore", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                4242,
                "4242",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                false,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 4242),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.ForcePickupDropsOnBreak, Is.True);
        Assert.That(runSettings.DropCadenceLabel, Is.EqualTo("Capsule Party"));
    }

    [Test]
    public void BuildRunSettingsDefaultsCapsulePartyOn()
    {
        var state = CreateRunSetupState();
        var buildRunSettings = state.GetType().GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 7777),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.ForcePickupDropsOnBreak, Is.True);
        Assert.That(runSettings.DropCadenceLabel, Is.EqualTo("Capsule Party"));
    }

    [Test]
    public void RestoreCanStillLeaveCapsulePartyOff()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("Restore", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1111,
                "1111",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                false,
                false,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1111),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.ForcePickupDropsOnBreak, Is.False);
        Assert.That(runSettings.DropCadenceLabel, Is.EqualTo("Standard"));
    }

    [Test]
    public void BuildRunSettingsCarriesHighScoreModeAndLifeLossPenalty()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("Restore", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                2024,
                "2024",
                RunDifficultyPreset.Standard,
                RunScoringMode.HighScore,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                false,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            600,
            null,
            new Func<int>(() => 2024),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.ScoringMode, Is.EqualTo(RunScoringMode.HighScore));
        Assert.That(runSettings.LifeLossScorePenalty, Is.EqualTo(600));
        Assert.That(runSettings.UsesLifeLossScorePenalty, Is.True);
        Assert.That(runSettings.ScoringModeLabel, Is.EqualTo("High Score"));
    }

    [Test]
    public void BuildRunSettingsCarriesLevelGlitchToggle()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("Restore", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                true,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.WarpGates));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Warp Gates Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedTurboRailGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.TurboRail,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.TurboRail));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Turbo Rail Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedMirrorGridGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.MirrorGrid,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.MirrorGrid));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Mirror Grid Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedGravityPocketGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.GravityPocket,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.GravityPocket));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Gravity Pocket Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedTokenStormGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.TokenStorm,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.TokenStorm));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Token Storm Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedStaticWallGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.StaticWall,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.StaticWall));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Static Wall Armed"));
    }

    [Test]
    public void BuildRunSettingsCarriesSelectedRowRewriteGlitch()
    {
        var state = CreateRunSetupState();
        var stateType = state.GetType();

        stateType.GetMethod("RestoreWithLevelGlitchSelection", InstanceFlags)?.Invoke(
            state,
            new object[]
            {
                1984,
                "1984",
                RunDifficultyPreset.Standard,
                RunScoringMode.Classic,
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                LevelGlitchSelection.RowRewrite,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            500,
            null,
            new Func<int>(() => 1984),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.LevelGlitchesEnabled, Is.True);
        Assert.That(runSettings.SelectedLevelGlitch, Is.EqualTo(LevelGlitchSelection.RowRewrite));
        Assert.That(runSettings.LevelGlitchLabel, Is.EqualTo("Row Rewrite Armed"));
    }

    private static object CreateRunSetupState()
    {
        var stateType = typeof(BreakoutGameController).Assembly.GetType("GetBricked.Gameplay.BreakoutRunSetupState", throwOnError: false);
        Assert.That(stateType, Is.Not.Null, "Could not resolve BreakoutRunSetupState.");
        return Activator.CreateInstance(stateType, string.Empty, new Func<int>(() => 1234));
    }
}
