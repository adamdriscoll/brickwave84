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
        var staticShoes = CreatePowerUp("Static Shoes", PowerUpEffectType.PaddleSpeedMultiplier, false, 8f, 0.6f);
        var slowBall = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.8f);
        var wave = CreatePowerUp("Wave", PowerUpEffectType.WavyPaddle, false, 10f, 0.55f);
        var fog = CreatePowerUp("Fog", PowerUpEffectType.FogOfWar, false, 10f, 0.45f);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(staticShoes, null);
        service.ApplyPowerUp(slowBall, null);
        service.ApplyPowerUp(wave, null);
        service.ApplyPowerUp(fog, null);

        var modifiers = service.CalculateEffectModifiers(1.1f, 0.25f);

        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(1.1f * 1.2f * 1.2f).Within(0.0001f));
        Assert.That(modifiers.PaddleSpeedMultiplier, Is.EqualTo(0.6f).Within(0.0001f));
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
    public void FuseBurstStartsShortBlackoutAndRequestsControllerBurst()
    {
        var service = CreateService();
        var fuseBurst = CreatePowerUp("Fuse Burst", PowerUpEffectType.FuseBurst, true, 4f, 0f, BreakoutContentRarity.Epic);

        var result = service.ApplyPowerUp(fuseBurst, null);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(fuseBurst.IsTimed, Is.True);
        Assert.That(result.ShouldTriggerFuseBurst, Is.True);
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].Definition, Is.SameAs(fuseBurst));
        Assert.That(modifiers.FogVisibilityMultiplier, Is.Zero);
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("FUSE BURST 4.0s"));
    }

    [Test]
    public void FuseBurstStacksBlackoutDurationLikeTimedEffects()
    {
        var service = CreateService();
        var fuseBurst = CreatePowerUp("Fuse Burst", PowerUpEffectType.FuseBurst, true, 4f, 0f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(fuseBurst, null);
        service.UpdateTimedEffects(isPlaying: true, deltaTime: 2f, modifiersChanged: null);
        service.ApplyPowerUp(fuseBurst, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(2));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(4f).Within(0.0001f));
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
        var megaBall = CreatePowerUp("Mega Ball", PowerUpEffectType.BallSizeMultiplier, true, 10f, 1.8f);
        var vectorSight = CreatePowerUp("Vector Sight", PowerUpEffectType.VectorSight, true, 14f, 1f);
        var capsuleMagnet = CreatePowerUp("Capsule Magnet", PowerUpEffectType.CapsuleMagnet, true, 12f, 1f);
        var magnetFlip = CreatePowerUp("Magnet Flip", PowerUpEffectType.MagnetFlip, false, 8f, 0.2f, BreakoutContentRarity.Epic);
        var mirrorImage = CreatePowerUp("Mirror Image", PowerUpEffectType.MirrorImagePaddle, true, 12f, 1f);
        var cleanCatch = CreatePowerUp("Clean Catch", PowerUpEffectType.CleanCatch, true, 10f, 1.35f);
        var tiltRail = CreatePowerUp("Tilt Rail", PowerUpEffectType.PaddleHitTilt, false, 12f, 9f);
        var wrapRail = CreatePowerUp("Wrap Rail", PowerUpEffectType.PaddleWrap, true, 10f, 1f);
        var jackpotJam = CreatePowerUp("Jackpot Jam", PowerUpEffectType.JackpotJam, true, 8f, 3f, BreakoutContentRarity.Epic);
        var overdriveTape = CreatePowerUp("Overdrive Tape", PowerUpEffectType.OverdriveTape, true, 9f, 1.25f, BreakoutContentRarity.Epic);
        var finalBreakthru = CreatePowerUp("Final Breakthru", PowerUpEffectType.FinalBreakthru, true, 6f, 2f, BreakoutContentRarity.Epic, secondaryScalar: 1.15f);

        service.ApplyPowerUp(magnet, null);
        service.ApplyPowerUp(scoreSurge, null);
        service.ApplyPowerUp(clone, null);
        service.ApplyPowerUp(jammer, null);
        service.ApplyPowerUp(hotPotato, null);
        service.ApplyPowerUp(boomBall, null);
        service.ApplyPowerUp(megaBall, null);
        service.ApplyPowerUp(vectorSight, null);
        service.ApplyPowerUp(capsuleMagnet, null);
        service.ApplyPowerUp(magnetFlip, null);
        service.ApplyPowerUp(mirrorImage, null);
        service.ApplyPowerUp(cleanCatch, null);
        service.ApplyPowerUp(tiltRail, null);
        service.ApplyPowerUp(wrapRail, null);
        service.ApplyPowerUp(jackpotJam, null);
        service.ApplyPowerUp(overdriveTape, null);
        service.ApplyPowerUp(finalBreakthru, null);

        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(modifiers.PaddleSpeedMultiplier, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(modifiers.BrickMagnetStrength, Is.EqualTo(0.38f).Within(0.0001f));
        Assert.That(modifiers.ScoreMultiplier, Is.EqualTo(2f * 1.28f * 3f * 2f).Within(0.0001f));
        Assert.That(modifiers.PaddleCloneEnabled, Is.True);
        Assert.That(modifiers.BrickJammerStrength, Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(1.28f * BreakoutPowerUpService.JackpotJamBallSpeedMultiplier * 1.25f).Within(0.0001f));
        Assert.That(modifiers.PickupFallSpeedMultiplier, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(modifiers.BallSizeMultiplier, Is.EqualTo(1.8f).Within(0.0001f));
        Assert.That(modifiers.HotPotatoStrength, Is.GreaterThan(0f));
        Assert.That(modifiers.ExplosiveBallStrength, Is.EqualTo(1.15f).Within(0.0001f));
        Assert.That(modifiers.WeakBrickPierceEnabled, Is.True);
        Assert.That(modifiers.VectorSightStrength, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(modifiers.CapsuleMagnetStrength, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(modifiers.BrickRepulsionStrength, Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(modifiers.MirrorImagePaddleEnabled, Is.True);
        Assert.That(modifiers.CleanCatchAimMultiplier, Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(modifiers.PaddleHitTiltDegrees, Is.EqualTo(9f).Within(0.0001f));
        Assert.That(modifiers.PaddleWrapEnabled, Is.True);
    }

    [Test]
    public void MicroSparkShrinksBallAndBoostsScore()
    {
        var service = CreateService();
        var microSpark = CreatePowerUp(
            "Micro Spark",
            PowerUpEffectType.MicroSpark,
            true,
            10f,
            0.55f,
            BreakoutContentRarity.Epic,
            secondaryScalar: 1.75f);

        service.ApplyPowerUp(microSpark, null);

        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(modifiers.BallSizeMultiplier, Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(modifiers.ScoreMultiplier, Is.EqualTo(1.75f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("MICRO SPARK 10.0s"));
    }

    [Test]
    public void DoubleTapArmsNextPaddleHitAndStartsRailShrinkWhenConsumed()
    {
        var service = CreateService();
        var doubleTap = CreatePowerUp(
            "Double Tap",
            PowerUpEffectType.DoubleTap,
            true,
            3.5f,
            0.72f,
            BreakoutContentRarity.Epic,
            extraBallCount: 2);

        service.ApplyPowerUp(doubleTap, null);

        Assert.That(doubleTap.IsTimed, Is.False);
        Assert.That(service.DoubleTapCharges, Is.EqualTo(1));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("DOUBLE TAP"));

        Assert.That(service.TryConsumeDoubleTapCharge(out var consumedDefinition, out var copyBallCount), Is.True);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(consumedDefinition, Is.SameAs(doubleTap));
        Assert.That(copyBallCount, Is.EqualTo(2));
        Assert.That(service.DoubleTapCharges, Is.Zero);
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(modifiers.PaddleWidthMultiplier, Is.EqualTo(0.72f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("DOUBLE TAP 3.5s"));
    }

    [Test]
    public void RemoveMicroSparkEffectsAtStackThresholdCancelsOnlyOverstackedMicroSpark()
    {
        var service = CreateService();
        var microSpark = CreatePowerUp(
            "Micro Spark",
            PowerUpEffectType.MicroSpark,
            true,
            10f,
            0.55f,
            BreakoutContentRarity.Epic,
            secondaryScalar: 1.75f);
        var scoreSurge = CreatePowerUp("Score Surge", PowerUpEffectType.ScoreMultiplier, true, 10f, 2f);

        for (var index = 0; index < 4; index++)
        {
            service.ApplyPowerUp(microSpark, null);
        }

        service.ApplyPowerUp(scoreSurge, null);

        var removedCount = service.RemoveMicroSparkEffectsAtStackThreshold(4f);
        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(removedCount, Is.EqualTo(1));
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].Definition, Is.SameAs(scoreSurge));
        Assert.That(modifiers.BallSizeMultiplier, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(modifiers.ScoreMultiplier, Is.EqualTo(2f).Within(0.0001f));
    }

    [Test]
    public void RemoveBeneficialBallSizeEffectsCancelsMegaDropsOnly()
    {
        var service = CreateService();
        var megaBall = CreatePowerUp("Mega Ball", PowerUpEffectType.BallSizeMultiplier, true, 10f, 1.8f);
        var microBall = CreatePowerUp("Micro Ball", PowerUpEffectType.BallSizeMultiplier, false, 10f, 0.6f);
        var slowBall = CreatePowerUp("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.8f);

        service.ApplyPowerUp(megaBall, null);
        service.ApplyPowerUp(microBall, null);
        service.ApplyPowerUp(slowBall, null);

        var removedCount = service.RemoveBeneficialBallSizeEffects();
        var modifiers = service.CalculateEffectModifiers(1f, 0f);

        Assert.That(removedCount, Is.EqualTo(1));
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(2));
        Assert.That(modifiers.BallSizeMultiplier, Is.EqualTo(0.6f).Within(0.0001f));
        Assert.That(modifiers.TimedBallSpeedMultiplier, Is.EqualTo(0.8f).Within(0.0001f));
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
    public void BankBonusChargesFromWallBouncesUntilConsumed()
    {
        var service = CreateService();
        var bankBonus = CreatePowerUp("Bank Bonus", PowerUpEffectType.BankBonus, true, 12f, 50f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(bankBonus, null);

        Assert.That(service.ChargeBankBonusFromWallBounce(), Is.EqualTo(50));
        Assert.That(service.ChargeBankBonusFromWallBounce(), Is.EqualTo(50));
        Assert.That(service.BankBonusChargePoints, Is.EqualTo(100));
        Assert.That(service.TryConsumeBankBonus(out var bonusPoints), Is.True);
        Assert.That(bonusPoints, Is.EqualTo(100));
        Assert.That(service.BankBonusChargePoints, Is.Zero);
        Assert.That(service.TryConsumeBankBonus(out _), Is.False);
    }

    [Test]
    public void PrismPopConsumesOneArmedChargeAtATime()
    {
        var service = CreateService();
        var prismPop = CreatePowerUp("Prism Pop", PowerUpEffectType.PrismPop, true, 10f, 4f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(prismPop, null);
        service.ApplyPowerUp(prismPop, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(2));
        Assert.That(service.TryConsumePrismPopCharge(out var firstDefinition, out var firstMultiplier), Is.True);
        Assert.That(firstDefinition, Is.SameAs(prismPop));
        Assert.That(firstMultiplier, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(1));
        Assert.That(service.TryConsumePrismPopCharge(out var secondDefinition, out _), Is.True);
        Assert.That(secondDefinition, Is.SameAs(prismPop));
        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.TryConsumePrismPopCharge(out _, out _), Is.False);
    }

    [Test]
    public void SolarShotStacksAndConsumesOneChargeAtATime()
    {
        var service = CreateService();
        var solarShot = CreatePowerUp("Solar Shot", PowerUpEffectType.SolarShot, true, 0f, 1f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(solarShot, null);
        service.ApplyPowerUp(solarShot, null);

        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.SolarShotCharges, Is.EqualTo(2));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("SOLAR x2"));
        Assert.That(service.TryConsumeSolarShotCharge(), Is.True);
        Assert.That(service.SolarShotCharges, Is.EqualTo(1));
        Assert.That(service.TryConsumeSolarShotCharge(), Is.True);
        Assert.That(service.TryConsumeSolarShotCharge(), Is.False);
    }

    [Test]
    public void RewindCatchStacksAndConsumesOneChargeAtATime()
    {
        var service = CreateService();
        var rewindCatch = CreatePowerUp("Rewind Catch", PowerUpEffectType.RewindCatch, true, 0f, 1f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(rewindCatch, null);
        service.ApplyPowerUp(rewindCatch, null);

        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.RewindCatchCharges, Is.EqualTo(2));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("REWIND x2"));
        Assert.That(service.TryConsumeRewindCatchCharge(out var firstDefinition), Is.True);
        Assert.That(firstDefinition, Is.SameAs(rewindCatch));
        Assert.That(service.RewindCatchCharges, Is.EqualTo(1));
        Assert.That(service.TryConsumeRewindCatchCharge(out var secondDefinition), Is.True);
        Assert.That(secondDefinition, Is.SameAs(rewindCatch));
        Assert.That(service.TryConsumeRewindCatchCharge(out _), Is.False);
    }

    [Test]
    public void BogusBounceStacksThreeWallBounceCharges()
    {
        var service = CreateService();
        var bogusBounce = CreatePowerUp("Bogus Bounce", PowerUpEffectType.BogusBounce, false, 0f, 52f, BreakoutContentRarity.Epic, extraBallCount: 3);

        service.ApplyPowerUp(bogusBounce, null);
        service.ApplyPowerUp(bogusBounce, null);

        Assert.That(bogusBounce.IsTimed, Is.False);
        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.BogusBounceCharges, Is.EqualTo(6));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("BOGUS BOUNCE x6"));
        Assert.That(service.TryConsumeBogusBounceCharge(out var wildAngleDegrees), Is.True);
        Assert.That(wildAngleDegrees, Is.EqualTo(52f).Within(0.0001f));
        Assert.That(service.BogusBounceCharges, Is.EqualTo(5));
    }

    [Test]
    public void BogusBounceClearsStoredAngleAfterLastCharge()
    {
        var service = CreateService();
        var bogusBounce = CreatePowerUp("Bogus Bounce", PowerUpEffectType.BogusBounce, false, 0f, 52f, BreakoutContentRarity.Epic, extraBallCount: 1);

        service.ApplyPowerUp(bogusBounce, null);

        Assert.That(service.TryConsumeBogusBounceCharge(out var wildAngleDegrees), Is.True);
        Assert.That(wildAngleDegrees, Is.EqualTo(52f).Within(0.0001f));
        Assert.That(service.BogusBounceCharges, Is.Zero);
        Assert.That(service.TryConsumeBogusBounceCharge(out _), Is.False);
    }

    [Test]
    public void CabinetJackpotRefreshesEveryActiveTimedEffect()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.45f);
        var staticShoes = CreatePowerUp("Static Shoes", PowerUpEffectType.PaddleSpeedMultiplier, false, 8f, 0.6f);
        var jackpot = CreatePowerUp("Cabinet Jackpot", PowerUpEffectType.CabinetJackpot, true, 0f, 1f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(staticShoes, null);
        service.UpdateTimedEffects(isPlaying: true, deltaTime: 5f, modifiersChanged: null);
        service.ApplyPowerUp(jackpot, null);

        Assert.That(jackpot.IsTimed, Is.False);
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(2));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects[1].RemainingDuration, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("WIDE PADDLE 12.0s"));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("STATIC SHOES 8.0s"));
    }

    [Test]
    public void CabinetJackpotPreservesStacksAndEffectMultipliers()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.45f);
        var mondoMulti = CreatePowerUp("Mondo Multi", PowerUpEffectType.ActiveDropMultiplier, true, 0f, 2f);
        var jackpot = CreatePowerUp("Cabinet Jackpot", PowerUpEffectType.CabinetJackpot, true, 0f, 1f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(wide, null);
        service.ApplyPowerUp(mondoMulti, null);
        service.UpdateTimedEffects(isPlaying: true, deltaTime: 6f, modifiersChanged: null);
        service.ApplyPowerUp(jackpot, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(2));
        Assert.That(service.ActiveTimedEffects[0].EffectMultiplier, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(service.CalculateEffectModifiers(1f, 0f).PaddleWidthMultiplier, Is.EqualTo(Mathf.Pow(1.45f, 4f)).Within(0.0001f));
    }

    [Test]
    public void QueuedHelpfulExtensionExtendsNextHelpfulTimedEffect()
    {
        var service = CreateService();
        var staticShoes = CreatePowerUp("Static Shoes", PowerUpEffectType.PaddleSpeedMultiplier, false, 8f, 0.6f);
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.45f);

        service.QueueHelpfulTimedEffectExtension(4f);
        service.ApplyPowerUp(staticShoes, null);
        service.ApplyPowerUp(wide, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(2));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects[1].RemainingDuration, Is.EqualTo(16f).Within(0.0001f));
        Assert.That(service.PendingHelpfulTimedEffectExtensionSeconds, Is.Zero);
    }

    [Test]
    public void QueuedHelpfulExtensionStacksUntilHelpfulTimedEffect()
    {
        var service = CreateService();
        var multiBall = CreatePowerUp("Multi-Ball", PowerUpEffectType.MultiBallBurst, true, 0f, 1f, extraBallCount: 2);
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.45f);

        service.QueueHelpfulTimedEffectExtension(3f);
        service.QueueHelpfulTimedEffectExtension(2f);
        service.ApplyPowerUp(multiBall, null);
        service.ApplyPowerUp(wide, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].RemainingDuration, Is.EqualTo(17f).Within(0.0001f));
        Assert.That(service.PendingHelpfulTimedEffectExtensionSeconds, Is.Zero);
    }

    [Test]
    public void BrickBloomStacksAndConsumesOneChargeAtATime()
    {
        var service = CreateService();
        var brickBloom = CreatePowerUp("Brick Bloom", PowerUpEffectType.BrickBloom, true, 0f, 1f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(brickBloom, null);
        service.ApplyPowerUp(brickBloom, null);

        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.BrickBloomCharges, Is.EqualTo(2));
        Assert.That(service.BuildActiveEffectsLabel(), Does.Contain("BLOOM x2"));
        Assert.That(service.TryConsumeBrickBloomCharge(out var firstDefinition), Is.True);
        Assert.That(firstDefinition, Is.SameAs(brickBloom));
        Assert.That(service.BrickBloomCharges, Is.EqualTo(1));
        Assert.That(service.TryConsumeBrickBloomCharge(out var secondDefinition), Is.True);
        Assert.That(secondDefinition, Is.SameAs(brickBloom));
    }

    [Test]
    public void CleanCatchConsumesOneArmedChargeAtATime()
    {
        var service = CreateService();
        var cleanCatch = CreatePowerUp("Clean Catch", PowerUpEffectType.CleanCatch, true, 10f, 1.35f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(cleanCatch, null);
        service.ApplyPowerUp(cleanCatch, null);

        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.CalculateEffectModifiers(1f, 0f).CleanCatchAimMultiplier, Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(service.TryConsumeCleanCatchCharge(out var firstDefinition, out var firstAimMultiplier), Is.True);
        Assert.That(firstDefinition, Is.SameAs(cleanCatch));
        Assert.That(firstAimMultiplier, Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(service.ActiveTimedEffects, Has.Count.EqualTo(1));
        Assert.That(service.ActiveTimedEffects[0].StackCount, Is.EqualTo(1));
        Assert.That(service.TryConsumeCleanCatchCharge(out var secondDefinition, out _), Is.True);
        Assert.That(secondDefinition, Is.SameAs(cleanCatch));
        Assert.That(service.ActiveTimedEffects, Is.Empty);
        Assert.That(service.TryConsumeCleanCatchCharge(out _, out _), Is.False);
    }

    [Test]
    public void BankBonusChargeCapsToPreventWallFarming()
    {
        var service = CreateService();
        var bankBonus = CreatePowerUp("Bank Bonus", PowerUpEffectType.BankBonus, true, 12f, 50f, BreakoutContentRarity.Epic);

        service.ApplyPowerUp(bankBonus, null);

        for (var index = 0; index < 12; index++)
        {
            service.ChargeBankBonusFromWallBounce();
        }

        Assert.That(service.BankBonusChargePoints, Is.EqualTo(BreakoutPowerUpService.BankBonusMaximumChargePoints));
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
    public void CapsuleMagnetTargetsOnlyNearbyHelpfulPickups()
    {
        var service = CreateService();
        var helpfulDefinition = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var harmfulDefinition = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        var helpfulPickup = CreateConfiguredPickup(helpfulDefinition, new Vector2(0.4f, 0.6f));
        var harmfulPickup = CreateConfiguredPickup(harmfulDefinition, new Vector2(0.4f, 0.6f));
        var farHelpfulPickup = CreateConfiguredPickup(helpfulDefinition, new Vector2(8f, 0.6f));
        var paddleObject = new GameObject("Paddle");
        runtimeObjects.Add(paddleObject);
        paddleObject.transform.position = Vector2.zero;
        var paddleCollider = paddleObject.AddComponent<BoxCollider2D>();

        service.ActivePickups.Add(helpfulPickup);
        service.ActivePickups.Add(harmfulPickup);
        service.ActivePickups.Add(farHelpfulPickup);

        service.RefreshCapsuleMagnetTargets(1f, paddleCollider);

        Assert.That(GetPrivateField<float>(helpfulPickup, "capsuleMagnetStrength"), Is.GreaterThan(0f));
        Assert.That(GetPrivateField<float>(harmfulPickup, "capsuleMagnetStrength"), Is.Zero);
        Assert.That(GetPrivateField<float>(farHelpfulPickup, "capsuleMagnetStrength"), Is.Zero);
    }

    [Test]
    public void CapsuleMagnetDriftsPickupSidewaysWhileFalling()
    {
        var pickup = CreateConfiguredPickup(
            CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f),
            new Vector2(-1f, 2f));

        pickup.SetCapsuleMagnetTarget(new Vector2(1f, 0f), 1f);

        var movementStep = (Vector2)InvokePrivateMethod(pickup, "ResolveFixedMovementStep");

        Assert.That(movementStep.x, Is.GreaterThan(0f));
        Assert.That(movementStep.y, Is.LessThan(0f));
    }

    [Test]
    public void ActivePickupFallSpeedMultiplierAcceleratesExistingPickups()
    {
        var service = CreateService();
        var powerUp = CreatePowerUp("Overdrive Tape", PowerUpEffectType.OverdriveTape, true, 9f, 1.25f);
        var pickup = CreateConfiguredPickup(powerUp, Vector2.zero);
        service.ActivePickups.Add(pickup);

        service.RefreshActivePickupFallSpeedMultiplier(1.25f);

        var movementStep = (Vector2)InvokePrivateMethod(pickup, "ResolveFixedMovementStep");

        Assert.That(GetPrivateField<float>(pickup, "activeFallSpeedMultiplier"), Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(movementStep.y, Is.EqualTo(-3.2f * 1.25f * Time.fixedDeltaTime).Within(0.0001f));
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
    public void TrySpawnPickupAppliesPickupFallSpeedMultiplier()
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
            controller: null,
            pickupFallSpeedMultiplier: 0.5f);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(GetPrivateField<float>(pickup, "fallSpeed"), Is.EqualTo(1.6f).Within(0.0001f));
    }

    [Test]
    public void CapsuleRouletteRerollsSpawnedPickupToOppositePolarity()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        SetPrivateField(wide, "pickupColor", new Color(0.2f, 0.95f, 0.45f, 1f));
        SetPrivateField(narrow, "pickupColor", new Color(1f, 0.2f, 0.25f, 1f));
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, narrow));
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
            controller: null,
            enableCapsuleRoulette: true);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(wide));

        service.UpdateCapsuleRoulettePickups(
            isPlaying: true,
            deltaTime: BreakoutPowerUpService.CapsuleRouletteIntervalSeconds,
            nextGameplayRandomFloat: (_, _) => 0f,
            themeService: null);

        Assert.That(pickup.Definition, Is.SameAs(narrow));
        Assert.That(pickup.VisualDefinition, Is.SameAs(narrow));
        Assert.That(pickup.GetComponent<SpriteRenderer>().color, Is.EqualTo(narrow.PickupColor));
    }

    [Test]
    public void CapsuleRouletteDoesNotArmWithoutBothPolarityPools()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var laser = CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 10f, 1f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, laser));
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
            controller: null,
            enableCapsuleRoulette: true);

        service.UpdateCapsuleRoulettePickups(
            isPlaying: true,
            deltaTime: BreakoutPowerUpService.CapsuleRouletteIntervalSeconds,
            nextGameplayRandomFloat: (_, _) => 0f,
            themeService: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(wide));
    }

    [Test]
    public void CapsuleBlackoutHidesSpawnedPickupDuringArmedWindow()
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

        service.TriggerCapsuleBlackout(new BreakoutCapsuleBlackoutSpec(2.4f, 1.45f, 0.03f));
        service.ApplyCapsuleBlackoutToPickup(pickup);
        pickup.SetVisibilityMultiplier(0.5f);

        Assert.That(service.IsCapsuleBlackoutArmed, Is.True);
        Assert.That(pickup.GetComponent<SpriteRenderer>().color.a, Is.EqualTo(0.015f).Within(0.001f));

        service.UpdateTimedEffects(isPlaying: true, deltaTime: 2.5f, modifiersChanged: null);

        Assert.That(service.IsCapsuleBlackoutArmed, Is.False);
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
    public void ForcedDropSpawnsSelectedDropRegardlessOfChanceAndTable()
    {
        var service = CreateService();
        var forcedDrop = CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 10f, 1f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 0f));
        var pickupsRoot = CreateRuntimeRoot("Pickups");

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 0f,
            nextGameplayRandomFloat: (_, _) =>
            {
                Assert.Fail("Forced non-random drops should not roll random chance.");
                return 0f;
            },
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null,
            forcedDropDefinition: forcedDrop,
            forcedDropCandidatePool: new[] { forcedDrop });

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(forcedDrop));
        Assert.That(service.ActivePickups, Does.Contain(pickup));
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
            nextGameplayRandomFloat: (_, max) => max > 1f ? max * 0.5f : 0f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(wide));
    }

    [Test]
    public void BogusTapeSpawnsAsHelpfulDisguiseButCollectsAsRandomUnlockedHazard()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var bogusTape = CreatePowerUp("Bogus Tape", PowerUpEffectType.RandomHarmfulDrop, false, 0f, 1f);
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        SetPrivateField(wide, "powerUpId", "large_paddle");
        SetPrivateField(bogusTape, "powerUpId", "bogus_tape");
        SetPrivateField(narrow, "powerUpId", "small_paddle");
        SetPrivateField(wide, "pickupColor", new Color(0.2f, 0.95f, 0.45f, 1f));
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, bogusTape, narrow));
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

        runState.SetInitialDropUnlocks(new[] { wide, bogusTape, narrow });

        var pickup = service.TrySpawnPickup(
            brick,
            rogueSettings,
            runState,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, max) => Mathf.Approximately(max, 1f) ? 0f : 1.1f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(narrow));
        Assert.That(pickup.VisualDefinition, Is.SameAs(wide));
        Assert.That(pickup.UsesHelpfulVisualDisguise, Is.True);
        Assert.That(pickup.GetComponent<SpriteRenderer>().color, Is.EqualTo(wide.PickupColor));
    }

    [Test]
    public void BogusTapeDoesNotSpawnWithoutUnlockedHelpfulAndHarmfulTargets()
    {
        var service = CreateService();
        var bogusTape = CreatePowerUp("Bogus Tape", PowerUpEffectType.RandomHarmfulDrop, false, 0f, 1f);
        SetPrivateField(bogusTape, "powerUpId", "bogus_tape");
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, bogusTape));
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

        runState.SetInitialDropUnlocks(new[] { bogusTape });

        var pickup = service.TrySpawnPickup(
            brick,
            rogueSettings,
            runState,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => 0f,
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Null);
        Assert.That(service.ActivePickups, Is.Empty);
    }

    [Test]
    public void MysteryTapeSpawnsVisibleMysteryPickupWithHelpfulAndHarmfulPayloads()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var mysteryTape = CreatePowerUp("Mystery Tape", PowerUpEffectType.RandomMixedDrop, true, 0f, 1f);
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, mysteryTape, narrow));
        var pickupsRoot = CreateRuntimeRoot("Pickups");
        var rolls = new Queue<float>(new[] { 0f, 1.5f, 0f, 0f });

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => rolls.Dequeue(),
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(mysteryTape));
        Assert.That(pickup.VisualDefinition, Is.SameAs(mysteryTape));
        Assert.That(pickup.PrimaryPayloadDefinition, Is.SameAs(wide));
        Assert.That(pickup.SecondaryPayloadDefinition, Is.SameAs(narrow));
        Assert.That(pickup.UsesHelpfulVisualDisguise, Is.False);
    }

    [Test]
    public void MysteryTapeChoosesPayloadsFromBothPolarityPools()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var laser = CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 10f, 1f);
        var mysteryTape = CreatePowerUp("Mystery Tape", PowerUpEffectType.RandomMixedDrop, true, 0f, 1f);
        var narrow = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        var fog = CreatePowerUp("Fog", PowerUpEffectType.FogOfWar, false, 10f, 0.45f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, laser, mysteryTape, narrow, fog));
        var pickupsRoot = CreateRuntimeRoot("Pickups");
        var rolls = new Queue<float>(new[] { 0f, 2.5f, 1.5f, 1.5f });

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => rolls.Dequeue(),
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(mysteryTape));
        Assert.That(pickup.VisualDefinition, Is.SameAs(mysteryTape));
        Assert.That(pickup.PrimaryPayloadDefinition, Is.SameAs(laser));
        Assert.That(pickup.SecondaryPayloadDefinition, Is.SameAs(fog));
    }

    [Test]
    public void MysteryTapeDoesNotSpawnWithoutBothHelpfulAndHarmfulTargets()
    {
        var service = CreateService();
        var wide = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var mysteryTape = CreatePowerUp("Mystery Tape", PowerUpEffectType.RandomMixedDrop, true, 0f, 1f);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, wide, mysteryTape));
        var pickupsRoot = CreateRuntimeRoot("Pickups");
        var rolls = new Queue<float>(new[] { 0f, 1.5f });

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, _) => rolls.Dequeue(),
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(pickup, Is.Null);
        Assert.That(service.ActivePickups, Is.Empty);
    }

    [Test]
    public void TrySpawnPickupAppliesRarityWeightsToAuthoredDropTable()
    {
        var service = CreateService();
        var common = CreatePowerUp("Common Drop", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var rare = CreatePowerUp("Rare Drop", PowerUpEffectType.LaserPaddle, true, 10f, 1f, BreakoutContentRarity.Rare);
        var brick = CreateBrick(CreateBrickDefinition(dropChance: 1f, common, rare));
        var pickupsRoot = CreateRuntimeRoot("Pickups");
        var selectionTotalWeight = 0f;

        var pickup = service.TrySpawnPickup(
            brick,
            activeRunSettings: null,
            activeRunState: null,
            effectiveDropChanceMultiplier: 1f,
            nextGameplayRandomFloat: (_, max) =>
            {
                if (Mathf.Approximately(max, 1f))
                {
                    return 0f;
                }

                selectionTotalWeight = max;
                return 1.05f;
            },
            pickupsRoot,
            arenaBottom: -4f,
            themeService: null,
            controller: null);

        Assert.That(selectionTotalWeight, Is.EqualTo(1f + BreakoutRarityRules.GetDropWeightMultiplier(BreakoutContentRarity.Rare)).Within(0.0001f));
        Assert.That(pickup, Is.Not.Null);
        Assert.That(pickup.Definition, Is.SameAs(rare));
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

    private PowerUpPickup CreateConfiguredPickup(PowerUpDefinition definition, Vector2 position)
    {
        var pickup = CreatePickup();
        pickup.transform.position = position;
        pickup.Configure(
            null,
            definition,
            speed: 3.2f,
            missY: -10f,
            startingRotationDegrees: 45f,
            spinDegreesPerSecond: 0f,
            new ThemeVisualStyle(Color.white, Color.white, null));
        return pickup;
    }

    private PowerUpDefinition CreatePowerUp(
        string displayName,
        PowerUpEffectType effectType,
        bool beneficial,
        float durationSeconds,
        float scalar,
        BreakoutContentRarity rarity = BreakoutContentRarity.Common,
        float secondaryScalar = 1f,
        int extraBallCount = 0)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        runtimeObjects.Add(powerUp);
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "effectType", effectType);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "rarity", rarity);
        SetPrivateField(powerUp, "durationSeconds", durationSeconds);
        SetPrivateField(powerUp, "scalar", scalar);
        SetPrivateField(powerUp, "secondaryScalar", secondaryScalar);
        SetPrivateField(powerUp, "extraBallCount", extraBallCount);
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

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        return (T)field.GetValue(instance);
    }

    private static object InvokePrivateMethod(object instance, string methodName)
    {
        var method = instance.GetType().GetMethod(methodName, InstanceFlags);
        Assert.That(method, Is.Not.Null, $"Missing method '{methodName}' on {instance.GetType().Name}.");
        return method.Invoke(instance, Array.Empty<object>());
    }
}
