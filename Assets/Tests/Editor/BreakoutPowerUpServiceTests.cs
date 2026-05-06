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
    public void BlackoutFogEffectCanMakeBricksFullyInvisible()
    {
        var service = CreateService();
        var blackout = CreatePowerUp("Blackout", PowerUpEffectType.FogOfWar, false, 8f, 0f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 0f));
        var brickRenderer = brick.GetComponentInChildren<SpriteRenderer>();

        service.ApplyPowerUp(blackout, null);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);
        brick.SetVisibilityMultiplier(modifiers.FogVisibilityMultiplier);

        Assert.That(modifiers.FogVisibilityMultiplier, Is.Zero);
        Assert.That(brickRenderer.color.a, Is.Zero);
    }

    [Test]
    public void CalculateEffectModifiersIncludesNewArcadeDrops()
    {
        var service = CreateService();
        var magnet = CreatePowerUp("Brick Magnet", PowerUpEffectType.BrickMagnet, true, 10f, 0.38f);
        var scoreSurge = CreatePowerUp("Score Surge", PowerUpEffectType.ScoreMultiplier, true, 10f, 2f);
        var clone = CreatePowerUp("Paddle Clone", PowerUpEffectType.PaddleClone, true, 10f, 1f);
        var jammer = CreatePowerUp("Brick Jammer", PowerUpEffectType.BrickJammer, false, 8f, 0.75f);
        var hotPotato = CreatePowerUp("Hot Potato Ball", PowerUpEffectType.HotPotatoBall, true, 9f, 1.28f);
        var boomBall = CreatePowerUp("Boom Ball", PowerUpEffectType.ExplosiveBall, true, 10f, 1f);
        var megaBall = CreatePowerUp("Mega Ball", PowerUpEffectType.BallSizeMultiplier, true, 10f, 5f);

        service.ApplyPowerUp(magnet, null);
        service.ApplyPowerUp(scoreSurge, null);
        service.ApplyPowerUp(clone, null);
        service.ApplyPowerUp(jammer, null);
        service.ApplyPowerUp(hotPotato, null);
        service.ApplyPowerUp(boomBall, null);
        service.ApplyPowerUp(megaBall, null);

        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(modifiers.BrickMagnetStrength, Is.EqualTo(0.38f).Within(0.0001f));
        Assert.That(modifiers.ScoreMultiplier, Is.EqualTo(2f * 1.28f).Within(0.0001f));
        Assert.That(modifiers.PaddleCloneEnabled, Is.True);
        Assert.That(modifiers.BrickJammerStrength, Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(1.28f).Within(0.0001f));
        Assert.That(modifiers.BallSizeMultiplier, Is.EqualTo(5f).Within(0.0001f));
        Assert.That(modifiers.HotPotatoStrength, Is.GreaterThan(0f));
        Assert.That(modifiers.ExplosiveBallStrength, Is.EqualTo(1f).Within(0.0001f));
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
    public void ApplyingStackedTimedEffectRefreshesTimerInsteadOfAddingDuration()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);

        service.ApplyPowerUp(wide, null);
        service.UpdateTimedEffects(isPlaying: true, deltaTime: 3f, modifiersChanged: null);
        service.ApplyPowerUp(wide, null);

        var summaries = service.BuildTimedEffectStackSummaries();

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(2));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(summaries[0].RemainingDuration, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(summaries[0].DurationRatio, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("WIDE PADDLE x2 10.0s"));
    }

    [Test]
    public void RemoveBeneficialPaddleWidthEffectsCancelsWideDropsOnly()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.72f);
        var slowBall = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.8f);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(narrow, null);
        service.ApplyPowerUp(slowBall, null);

        var removedCount = service.RemoveBeneficialPaddleWidthEffects();
        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(removedCount, Is.EqualTo(1));
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(2));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(0.72f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(0.8f).Within(0.0001f));
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

    [Test]
    public void CapsuleMadnessActivatesAtFivePickupsAndRearmsAfterCountDrops()
    {
        var service = CreateService();

        for (var index = 0; index < BreakoutPowerUpService.CapsuleMadnessPickupThreshold - 1; index++)
        {
            service.ActivePickups.Add(CreatePickup());
        }

        service.EvaluateCapsuleMadnessActivation();

        Assert.That(service.IsCapsuleMadnessActive, Is.False);

        var fifthPickup = CreatePickup();
        service.ActivePickups.Add(fifthPickup);
        service.EvaluateCapsuleMadnessActivation();

        Assert.That(service.IsCapsuleMadnessActive, Is.True);
        Assert.That(service.CapsuleMadnessTimer, Is.EqualTo(BreakoutPowerUpService.CapsuleMadnessDurationSeconds).Within(0.0001f));
        Assert.That(service.PickupBannerText, Is.EqualTo("CAPSULE MADNESS!!"));

        service.UpdateTimedEffects(isPlaying: false, deltaTime: 99f, modifiersChanged: null);

        Assert.That(service.CapsuleMadnessTimer, Is.EqualTo(BreakoutPowerUpService.CapsuleMadnessDurationSeconds).Within(0.0001f));

        service.UpdateTimedEffects(isPlaying: true, deltaTime: BreakoutPowerUpService.CapsuleMadnessDurationSeconds + 0.1f, modifiersChanged: null);
        service.EvaluateCapsuleMadnessActivation();

        Assert.That(service.IsCapsuleMadnessActive, Is.False);

        service.RemovePickup(fifthPickup);
        service.ActivePickups.Add(fifthPickup);
        service.EvaluateCapsuleMadnessActivation();

        Assert.That(service.IsCapsuleMadnessActive, Is.True);
    }

    [Test]
    public void TrySpawnPickupReturnsSpawnedPickupWhenDropIsCreated()
    {
        var service = CreateService();
        var powerUp = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, powerUp));
        var pickupsRoot = CreateRuntimeRoot("Pickups");

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => 0f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(service.ActivePickups, Does.Contain(pickup));
        Assert.That(pickup.Definition, Is.SameAs(powerUp));
        Assert.That(pickup.transform.parent, Is.SameAs(pickupsRoot));
    }

    [Test]
    public void TrySpawnPickupReturnsNullWhenDropDoesNotPassChance()
    {
        var service = CreateService();
        var powerUp = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 0.5f, powerUp));
        var pickupsRoot = CreateRuntimeRoot("Pickups");

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => 1f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Null);
        Assert.That(service.ActivePickups, Is.Empty);
    }

    [Test]
    public void RogueDropsOnlySpawnFromUnlockedRunPool()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var laser = CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 10f, 1f);
        SetPrivateField(wide, "powerUpId", "large_paddle");
        SetPrivateField(laser, "powerUpId", "laser_paddle");
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, laser));
        var runState = new BreakoutRunState();
        var pickupsRoot = CreateRuntimeRoot("Pickups");
        var rogueSettings = new RunSettings(
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
            RunGameMode.Rogue);

        runState.SetInitialDropUnlocks(new[] { wide });

        var pickup = service.TrySpawnPickup(
            brick,
            rogueSettings,
            runState,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, max) => max > 1f ? 1.5f : 0f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(wide));
    }

    private BreakoutPowerUpService CreateService(float multiBallSpreadAngle = 18f)
    {
        return new BreakoutPowerUpService(new Vector2(0.55f, 0.55f), 3.2f, multiBallSpreadAngle, null);
    }

    private Transform CreateRuntimeRoot(string name)
    {
        var rootObject = new GameObject(name);
        runtimeObjects.Add(rootObject);
        return rootObject.transform;
    }

    private Brick CreateBrick(BrickDefinition definition)
    {
        var brickObject = new GameObject("Brick");
        runtimeObjects.Add(brickObject);
        brickObject.AddComponent<BoxCollider2D>();

        var visualObject = new GameObject("Visual");
        runtimeObjects.Add(visualObject);
        visualObject.transform.SetParent(brickObject.transform, false);
        visualObject.AddComponent<SpriteRenderer>();

        var brick = brickObject.AddComponent<Brick>();
        brick.Initialize(
            null,
            definition,
            effectiveHitPoints: definition.HitPoints,
            new ThemeVisualStyle(Color.white, Color.gray, null),
            motionSpeed: 0f,
            motionDirection: Vector2.zero);
        return brick;
    }

    private PowerUpPickup CreatePickup()
    {
        var pickupObject = new GameObject("Pickup");
        runtimeObjects.Add(pickupObject);
        pickupObject.AddComponent<BoxCollider2D>();
        pickupObject.AddComponent<SpriteRenderer>();
        pickupObject.AddComponent<Rigidbody2D>();
        return pickupObject.AddComponent<PowerUpPickup>();
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

    private BrickDefinition CreateBrickDefinition(float dropChance, params PowerUpDefinition[] powerUps)
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(definition);
        SetPrivateField(definition, "displayName", "Drop Brick");
        SetPrivateField(definition, "hitPoints", 1);
        SetPrivateField(definition, "dropChance", dropChance);
        var dropEntries = new BrickPowerUpDropEntry[powerUps.Length];

        for (var index = 0; index < powerUps.Length; index++)
        {
            dropEntries[index] = CreateDropEntry(powerUps[index], 1f);
        }

        SetPrivateField(definition, "dropTable", dropEntries);
        return definition;
    }

    private static BrickPowerUpDropEntry CreateDropEntry(PowerUpDefinition powerUp, float weight)
    {
        object entry = new BrickPowerUpDropEntry();
        SetPrivateField(entry, "powerUpDefinition", powerUp);
        SetPrivateField(entry, "weight", weight);
        return (BrickPowerUpDropEntry)entry;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
