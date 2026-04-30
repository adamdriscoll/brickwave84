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
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                true,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
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
                1,
                0,
                0,
                0,
                DropPoolMode.Mixed,
                false,
                string.Empty,
            });

        var buildRunSettings = stateType.GetMethod("BuildRunSettings", InstanceFlags);
        Assert.That(buildRunSettings, Is.Not.Null);

        var args = new object[]
        {
            3,
            null,
            new Func<int>(() => 1111),
            null,
            true,
        };

        var runSettings = (RunSettings)buildRunSettings.Invoke(state, args);

        Assert.That(runSettings.ForcePickupDropsOnBreak, Is.False);
        Assert.That(runSettings.DropCadenceLabel, Is.EqualTo("Standard"));
    }

    private static object CreateRunSetupState()
    {
        var stateType = typeof(BreakoutGameController).Assembly.GetType("GetBricked.Gameplay.BreakoutRunSetupState", throwOnError: false);
        Assert.That(stateType, Is.Not.Null, "Could not resolve BreakoutRunSetupState.");
        return Activator.CreateInstance(stateType, string.Empty, new Func<int>(() => 1234));
    }
}
