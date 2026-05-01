using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutPowerUpServiceTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private readonly List<UnityEngine.Object> runtimeObjects = new List<UnityEngine.Object>();

    [TearDown]
    public void TearDown()
    {
        for (var index = runtimeObjects.Count - 1; index >= 0; index--)
        {
            if (runtimeObjects[index] != null)
            {
                UnityEngine.Object.DestroyImmediate(runtimeObjects[index]);
            }
        }

        runtimeObjects.Clear();
    }

    [Test]
    public void CalculateEffectModifiersCombinesBaseAndStackedTimedEffects()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var slowBall = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.8f);
        var wave = CreatePowerUp("Wave", PowerUpEffectType.WavyPaddle, false, 10f, 0.55f);
        var fog = CreatePowerUp("Fog", PowerUpEffectType.FogOfWar, false, 10f, 0.45f);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(slowBall, null);
        service.ApplyPowerUp(wave, null);
        service.ApplyPowerUp(fog, null);

        var modifiers = service.CalculateEffectModifiers(1.1f, 0.25f);

        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(1.1f * 1.2f * 1.2f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(modifiers.WavyPaddleStrength, Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(modifiers.FogVisibilityMultiplier, Is.EqualTo(0.45f).Within(0.0001f));
    }

    [Test]
    public void CalculateEffectModifiersRunSettingsOverloadUsesSameBaseMultiplierPath()
    {
        var service = CreateService();
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.75f);
        var runSettings = new RunSettings(
            1234,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            500,
            1,
            1.3f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null);

        service.ApplyPowerUp(narrow, null);

        var fromRunSettings = service.CalculateEffectModifiers(runSettings);
        var fromExplicitBase = service.CalculateEffectModifiers(1.3f, 0f);

        Assert.That(fromRunSettings.PaddleWidthMultiplier, Is.EqualTo(fromExplicitBase.PaddleWidthMultiplier).Within(0.0001f));
        Assert.That(fromRunSettings.TimedBallSpeedMultiplier, Is.EqualTo(fromExplicitBase.TimedBallSpeedMultiplier).Within(0.0001f));
    }

    [Test]
    public void UpdateTimedEffectsOnlyExpiresWhilePlayingAndNotifiesOnce()
    {
        var service = CreateService();
        var effect = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 4f, 0.8f);
        var changeNotifications = 0;

        service.ApplyPowerUp(effect, null);
        service.UpdateTimedEffects(isPlaying: false, deltaTime: 99f, () => changeNotifications++);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(changeNotifications, Is.Zero);

        service.UpdateTimedEffects(isPlaying: true, deltaTime: 3f, () => changeNotifications++);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(changeNotifications, Is.Zero);

        service.UpdateTimedEffects(isPlaying: true, deltaTime: 1.1f, () => changeNotifications++);

        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(changeNotifications, Is.EqualTo(1));
    }

    [Test]
    public void ApplyPowerUpReturnsImmediateResultsWithoutTimedEffectEntries()
    {
        var service = CreateService();
        var multiBall = CreatePowerUp("Multi-Ball", PowerUpEffectType.MultiBallBurst, true, 0f, 1f);
        var shield = CreatePowerUp("Shield Wall", PowerUpEffectType.ShieldWall, true, 0f, 2.6f);
        var multiplier = CreatePowerUp("Mondo Multi", PowerUpEffectType.ActiveDropMultiplier, true, 0f, 2f);
        var divider = CreatePowerUp("Bogus Multi", PowerUpEffectType.ActiveDropMultiplier, false, 0f, 0.5f);

        var multiBallResult = service.ApplyPowerUp(multiBall, null);
        var shieldResult = service.ApplyPowerUp(shield, null);
        service.ApplyPowerUp(multiplier, null);
        service.ApplyPowerUp(divider, null);

        Assert.That(multiBallResult.ShouldSpawnMultiBall, Is.True);
        Assert.That(multiBallResult.ShieldWallChargesGranted, Is.Zero);
        Assert.That(shieldResult.ShouldSpawnMultiBall, Is.False);
        Assert.That(shieldResult.ShieldWallChargesGranted, Is.EqualTo(3));
        Assert.That(service.ActiveTimedEffects, Is.Empty);
    }

    [Test]
    public void ActiveDropMultiplierDoublesActiveEffectsWithoutExtendingDurations()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.2f);
        var slowBall = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.8f);
        var fog = CreatePowerUp("Fog", PowerUpEffectType.FogOfWar, false, 10f, 0.45f);
        var multiplier = CreatePowerUp("Mondo Multi", PowerUpEffectType.ActiveDropMultiplier, true, 0f, 2f);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(slowBall, null);
        service.ApplyPowerUp(fog, null);

        service.ApplyPowerUp(multiplier, null);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);
        var summaries = service.BuildTimedEffectStackSummaries();

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(3));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].EffectMultiplier, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(1.2f * 1.2f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(0.8f * 0.8f).Within(0.0001f));
        Assert.That(modifiers.FogVisibilityMultiplier, Is.EqualTo(0.45f * 0.45f).Within(0.0001f));
        Assert.That(summaries[0].DisplayMultiplier, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(summaries[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(summaries[0].DurationRatio, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("WIDE PADDLE x2 12.0s"));
    }

    [Test]
    public void ActiveDropMultiplierCanHalveActiveEffectsWithoutExtendingDurations()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.2f);
        var divider = CreatePowerUp("Bogus Multi", PowerUpEffectType.ActiveDropMultiplier, false, 0f, 0.5f);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(divider, null);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);
        var summaries = service.BuildTimedEffectStackSummaries();

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].EffectMultiplier, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(Mathf.Pow(1.2f, 0.5f)).Within(0.0001f));
        Assert.That(summaries[0].DisplayMultiplier, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(summaries[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(summaries[0].DurationRatio, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("WIDE PADDLE x0.5 12.0s"));
    }

    [Test]
    public void BuildMultiBallDirectionsCreatesNormalizedSymmetricSpread()
    {
        var service = CreateService(multiBallSpreadAngle: 20f);

        var directions = service.BuildMultiBallDirections(Vector2.up, 3);

        Assert.That(directions, Has.Length.EqualTo(3));
        Assert.That(directions[0].magnitude, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(directions[1].x, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(directions[1].y, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(directions[0].x, Is.EqualTo(-directions[2].x).Within(0.0001f));
        Assert.That(directions[0].y, Is.EqualTo(directions[2].y).Within(0.0001f));
    }

    private BreakoutPowerUpService CreateService(float multiBallSpreadAngle = 18f)
    {
        return new BreakoutPowerUpService(new Vector2(0.55f, 0.55f), 3.2f, multiBallSpreadAngle, null);
    }

    private PowerUpDefinition CreatePowerUp(
        string displayName,
        PowerUpEffectType effectType,
        bool beneficial,
        float durationSeconds,
        float scalar)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        runtimeObjects.Add(powerUp);
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "effectType", effectType);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "durationSeconds", durationSeconds);
        SetPrivateField(powerUp, "scalar", scalar);
        SetPrivateField(powerUp, "extraBallCount", 0);
        return powerUp;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
