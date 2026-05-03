using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutGameControllerPowerUpTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    private GameObject controllerObject;
    private GameObject paddleObject;
    private readonly List<UnityEngine.Object> runtimeObjects = new List<UnityEngine.Object>();

    [TearDown]
    public void TearDown()
    {
        if (controllerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
        }

        if (paddleObject != null)
        {
            UnityEngine.Object.DestroyImmediate(paddleObject);
        }

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
    public void ApplyingWavyPowerUpUpdatesPaddleImmediately()
    {
        var controller = CreateControllerHarness(out var paddle);
        var wavyPowerUp = CreatePowerUp(
            "Wavy Paddle",
            PowerUpEffectType.WavyPaddle,
            beneficial: false,
            durationSeconds: 12f,
            scalar: 0.8f);

        InvokePrivateMethod(controller, "ApplyPowerUp", wavyPowerUp);

        Assert.That(GetPrivateField<float>(paddle, "wavyStrength"), Is.EqualTo(0.8f).Within(0.0001f));
    }

    [Test]
    public void ApplyingWidthPowerUpUpdatesPaddleScaleImmediately()
    {
        var controller = CreateControllerHarness(out var paddle);
        var widePowerUp = CreatePowerUp(
            "Wide Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: true,
            durationSeconds: 12f,
            scalar: 1.45f);

        InvokePrivateMethod(controller, "ApplyPowerUp", widePowerUp);

        Assert.That(paddle.transform.localScale.x, Is.EqualTo(2.1f * 1.45f).Within(0.0001f));
    }

    [Test]
    public void BrickosaurusPowerDownPoolKeepsPaddleReadable()
    {
        Assert.That(IsBrickosaurusPowerDownCandidate(CreatePowerUp(
            "Reverse Controls",
            PowerUpEffectType.ReverseControls,
            beneficial: false,
            durationSeconds: 8f,
            scalar: 1f)), Is.True);
        Assert.That(IsBrickosaurusPowerDownCandidate(CreatePowerUp(
            "Lag Spike",
            PowerUpEffectType.LagSpike,
            beneficial: false,
            durationSeconds: 8f,
            scalar: 0.5f)), Is.True);
        Assert.That(IsBrickosaurusPowerDownCandidate(CreatePowerUp(
            "Narrow Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: false,
            durationSeconds: 10f,
            scalar: 0.72f)), Is.False);
        Assert.That(IsBrickosaurusPowerDownCandidate(CreatePowerUp(
            "Split Paddle",
            PowerUpEffectType.SplitPaddle,
            beneficial: false,
            durationSeconds: 12f,
            scalar: 1f)), Is.False);
        Assert.That(IsBrickosaurusPowerDownCandidate(CreatePowerUp(
            "Blackout",
            PowerUpEffectType.FogOfWar,
            beneficial: false,
            durationSeconds: 8f,
            scalar: 0.42f)), Is.False);
    }

    [Test]
    public void BrickServiceUsesDefinitionSizeMultiplier()
    {
        var controller = CreateControllerHarness(out _);
        var bricksRootObject = new GameObject("Bricks Root");
        runtimeObjects.Add(bricksRootObject);
        SetPrivateField(controller, "bricksRoot", bricksRootObject.transform);
        SetPrivateField(controller, "squareSprite", CreateSquareSprite());

        var tinyDefinition = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(tinyDefinition);
        SetPrivateField(tinyDefinition, "displayName", "Tiny Brick");
        SetPrivateField(tinyDefinition, "hitPoints", 1);
        SetPrivateField(tinyDefinition, "scoreValue", 325);
        SetPrivateField(tinyDefinition, "sizeMultiplier", 0.5f);
        SetPrivateField(tinyDefinition, "indestructible", false);
        SetPrivateField(tinyDefinition, "countsTowardLevelCompletion", true);
        SetPrivateField(tinyDefinition, "dropChance", 0f);
        SetPrivateField(tinyDefinition, "dropTable", Array.Empty<BrickPowerUpDropEntry>());
        var motionConfig = Activator.CreateInstance(GetGameplayType("GetBricked.Gameplay.BreakoutBrickMotionConfig"));
        InvokePrivateMethod(controller, "CreateBrickService");
        var brickService = GetPrivateField<object>(controller, "brickService");
        var createBrick = brickService.GetType().GetMethod("CreateBrick", InstanceFlags);
        Assert.That(createBrick, Is.Not.Null);

        createBrick.Invoke(brickService, new object[] { new Vector2(1f, 2f), tinyDefinition, 0, 0, motionConfig });

        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");

        Assert.That(bricks, Has.Count.EqualTo(1));
        Assert.That(bricks[0].transform.localScale.x, Is.EqualTo(1.15f * 0.5f).Within(0.0001f));
        Assert.That(bricks[0].transform.localScale.y, Is.EqualTo(0.58f * 0.5f).Within(0.0001f));
    }

    [Test]
    public void MovingBricksReflectAtBottomArenaBound()
    {
        var controller = CreateControllerHarness(out _);
        var definition = CreateBrickDefinition("Moving Brick", 1, 100);
        var motionConfig = Activator.CreateInstance(
            GetGameplayType("GetBricked.Gameplay.BreakoutBrickMotionConfig"),
            2f,
            Vector2.down);
        var brickService = GetPrivateField<object>(controller, "brickService");
        var createBrick = brickService.GetType().GetMethod("CreateBrick", InstanceFlags);
        Assert.That(createBrick, Is.Not.Null);

        var brick = (Brick)createBrick.Invoke(
            brickService,
            new object[] { new Vector2(0f, -4.95f), definition, 0, 0, motionConfig });
        var brickBody = brick.GetComponent<Rigidbody2D>();

        InvokePrivateMethod(brick, "FixedUpdate");

        Assert.That(brickBody.position.y, Is.GreaterThanOrEqualTo(-4.711f));
        Assert.That(brickBody.linearVelocity.y, Is.GreaterThan(0f));
    }

    [Test]
    public void ApplyingSameShrinkPowerUpTwiceStacksPaddleScale()
    {
        var controller = CreateControllerHarness(out var paddle);
        var shrinkPowerUp = CreatePowerUp(
            "Narrow Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: false,
            durationSeconds: 10f,
            scalar: 0.72f);

        InvokePrivateMethod(controller, "ApplyPowerUp", shrinkPowerUp);
        InvokePrivateMethod(controller, "ApplyPowerUp", shrinkPowerUp);

        Assert.That(paddle.transform.localScale.x, Is.EqualTo(2.1f * 0.72f * 0.72f).Within(0.0001f));
    }

    [Test]
    public void ApplyingSameWidePowerUpTwiceStacksPaddleScaleBelowWidthCap()
    {
        var controller = CreateControllerHarness(out var paddle);
        var widePowerUp = CreatePowerUp(
            "Wide Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: true,
            durationSeconds: 12f,
            scalar: 1.45f);

        InvokePrivateMethod(controller, "ApplyPowerUp", widePowerUp);
        InvokePrivateMethod(controller, "ApplyPowerUp", widePowerUp);

        Assert.That(paddle.transform.localScale.x, Is.EqualTo(2.1f * 1.45f * 1.45f).Within(0.0001f));
    }

    [Test]
    public void ApplyingStackedWidePowerUpsBreaksPaddleAtPlayableArenaWidth()
    {
        var controller = CreateControllerHarness(out var paddle);
        var widePowerUp = CreatePowerUp(
            "Wide Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: true,
            durationSeconds: 12f,
            scalar: 1.45f);

        for (var index = 0; index < 6; index++)
        {
            InvokePrivateMethod(controller, "ApplyPowerUp", widePowerUp);
        }

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");
        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        var pickupBannerView = InvokePrivateMethodWithResult(controller, "BuildPickupBannerView");

        Assert.That(activeTimedEffects.Count, Is.Zero);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "PaddleWidthMultiplier"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(paddle.transform.localScale.x, Is.EqualTo(2.1f).Within(0.0001f));
        Assert.That(paddle.HalfWidthWorld, Is.EqualTo(1.05f).Within(0.0001f));
        Assert.That(paddle.IsBreakWiggleActive, Is.True);
        Assert.That(GetFieldValue<string>(pickupBannerView, "Text"), Is.EqualTo("RAIL BUSTED!"));
    }

    [Test]
    public void ApplyingSameTimedPowerUpTwiceShowsStackCountInUiLabels()
    {
        var controller = CreateControllerHarness(out _);
        var shrinkPowerUp = CreatePowerUp(
            "Narrow Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: false,
            durationSeconds: 10f,
            scalar: 0.72f);

        InvokePrivateMethod(controller, "ApplyPowerUp", shrinkPowerUp);
        InvokePrivateMethod(controller, "ApplyPowerUp", shrinkPowerUp);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");
        var activeEffectsLabel = (string)InvokePrivateMethodWithResult(controller, "BuildActiveEffectsLabel");
        var pickupBannerView = InvokePrivateMethodWithResult(controller, "BuildPickupBannerView");
        var modifierViews = (Array)InvokePrivateMethodWithResult(controller, "BuildModifierViews");

        Assert.That(activeTimedEffects.Count, Is.EqualTo(1));
        Assert.That(GetPropertyValue<int>(activeTimedEffects[0], "StackCount"), Is.EqualTo(2));
        Assert.That(GetPropertyValue<float>(activeTimedEffects[0], "RemainingDuration"), Is.EqualTo(10f).Within(0.0001f));
        Assert.That(activeEffectsLabel, Does.Contain("x2"));
        Assert.That(activeEffectsLabel, Does.Contain("10.0s"));
        Assert.That(GetFieldValue<string>(pickupBannerView, "Text"), Does.Contain("x2"));
        Assert.That(modifierViews.Length, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(modifierViews.GetValue(0), "Label"), Does.Contain("x2"));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "RemainingDuration"), Is.EqualTo(10f).Within(0.0001f));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "DurationRatio"), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void CaughtPickupDuringCapsuleMadnessAwardsBonusPoints()
    {
        var controller = CreateControllerHarness(out _);
        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var pickupObject = new GameObject("Capsule");
        runtimeObjects.Add(pickupObject);
        pickupObject.AddComponent<BoxCollider2D>();
        pickupObject.AddComponent<SpriteRenderer>();
        pickupObject.AddComponent<Rigidbody2D>();
        var pickup = pickupObject.AddComponent<PowerUpPickup>();
        var powerUp = CreatePowerUp(
            "Score Capsule",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: true,
            durationSeconds: 0f,
            scalar: 1f);
        SetPrivateField(pickup, "definition", powerUp);
        var activePickups = GetPropertyValue<System.Collections.IList>(powerUpService, "ActivePickups");
        activePickups.Add(pickup);

        for (var index = 1; index < BreakoutPowerUpService.CapsuleMadnessPickupThreshold; index++)
        {
            var extraPickupObject = new GameObject($"Capsule {index}");
            runtimeObjects.Add(extraPickupObject);
            extraPickupObject.AddComponent<BoxCollider2D>();
            extraPickupObject.AddComponent<SpriteRenderer>();
            extraPickupObject.AddComponent<Rigidbody2D>();
            activePickups.Add(extraPickupObject.AddComponent<PowerUpPickup>());
        }

        InvokePrivateMethod(powerUpService, "EvaluateCapsuleMadnessActivation");
        SetPrivateField(controller, "score", 100);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandlePickupCaught(pickup);

        var popups = GetFloatingScorePopups(controller);

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(100 + BreakoutPowerUpService.CapsuleMadnessPickupBonusPoints));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo($"+{BreakoutPowerUpService.CapsuleMadnessPickupBonusPoints}"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Is.EqualTo("COMBO BONUS: CAPSULE MADNESS!"));
    }

    [Test]
    public void ApplyingActiveDropMultiplierDoublesEffectsWithoutExtendingTimers()
    {
        var controller = CreateControllerHarness(out var paddle);
        var widePowerUp = CreatePowerUp(
            "Wide Paddle",
            PowerUpEffectType.PaddleWidthMultiplier,
            beneficial: true,
            durationSeconds: 12f,
            scalar: 1.45f);
        var multiplierPowerUp = CreatePowerUp(
            "Mondo Multi",
            PowerUpEffectType.ActiveDropMultiplier,
            beneficial: true,
            durationSeconds: 0f,
            scalar: 2f);

        InvokePrivateMethod(controller, "ApplyPowerUp", widePowerUp);
        InvokePrivateMethod(controller, "ApplyPowerUp", multiplierPowerUp);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");
        var activeEffectsLabel = (string)InvokePrivateMethodWithResult(controller, "BuildActiveEffectsLabel");
        var modifierViews = (Array)InvokePrivateMethodWithResult(controller, "BuildModifierViews");

        Assert.That(activeTimedEffects.Count, Is.EqualTo(1));
        Assert.That(GetPropertyValue<int>(activeTimedEffects[0], "StackCount"), Is.EqualTo(1));
        Assert.That(GetPropertyValue<float>(activeTimedEffects[0], "EffectMultiplier"), Is.EqualTo(2f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeTimedEffects[0], "RemainingDuration"), Is.EqualTo(12f).Within(0.0001f));
        Assert.That(paddle.transform.localScale.x, Is.EqualTo(2.1f * 1.45f * 1.45f).Within(0.0001f));
        Assert.That(activeEffectsLabel, Does.Contain("x2"));
        Assert.That(activeEffectsLabel, Does.Contain("12.0s"));
        Assert.That(modifierViews.Length, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(modifierViews.GetValue(0), "Label"), Does.Contain("x2"));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "RemainingDuration"), Is.EqualTo(12f).Within(0.0001f));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "DurationRatio"), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void ApplyingSameTimedPowerUpTwiceRefreshesTimerAcrossTimedDropTypes()
    {
        var timedPowerUps = new[]
        {
            CreateTimedPowerUpCase("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.72f),
            CreateTimedPowerUpCase("Slow Ball", PowerUpEffectType.BallSpeedMultiplier, true, 10f, 0.78f),
            CreateTimedPowerUpCase("Wavy Paddle", PowerUpEffectType.WavyPaddle, false, 12f, 1f),
            CreateTimedPowerUpCase("Sticky Paddle", PowerUpEffectType.StickyPaddle, true, 15f, 1f),
            CreateTimedPowerUpCase("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 15f, 1f),
            CreateTimedPowerUpCase("Phase Ball", PowerUpEffectType.PhaseBall, true, 12f, 1f),
            CreateTimedPowerUpCase("Chain Lightning", PowerUpEffectType.ChainLightning, true, 14f, 0.35f),
            CreateTimedPowerUpCase("Reverse Controls", PowerUpEffectType.ReverseControls, false, 8f, 1f),
            CreateTimedPowerUpCase("Split Paddle", PowerUpEffectType.SplitPaddle, false, 12f, 1f),
            CreateTimedPowerUpCase("Gravity Well", PowerUpEffectType.GravityWell, false, 12f, 0.35f),
            CreateTimedPowerUpCase("Fog of War", PowerUpEffectType.FogOfWar, false, 10f, 0.55f),
            CreateTimedPowerUpCase("Lag Spike", PowerUpEffectType.LagSpike, false, 8f, 0.5f),
            CreateTimedPowerUpCase("Boom Ball", PowerUpEffectType.ExplosiveBall, true, 10f, 1f),
        };

        for (var index = 0; index < timedPowerUps.Length; index++)
        {
            var controller = CreateControllerHarness(out _);
            var powerUpCase = timedPowerUps[index];
            var powerUp = CreatePowerUp(
                powerUpCase.DisplayName,
                powerUpCase.EffectType,
                powerUpCase.Beneficial,
                powerUpCase.DurationSeconds,
                powerUpCase.Scalar);

            InvokePrivateMethod(controller, "ApplyPowerUp", powerUp);
            InvokePrivateMethod(controller, "ApplyPowerUp", powerUp);

            var powerUpService = GetPrivateField<object>(controller, "powerUpService");
            var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");

            Assert.That(activeTimedEffects.Count, Is.EqualTo(1), $"Expected a single stacked timed effect entry for {powerUpCase.DisplayName}.");
            Assert.That(GetPropertyValue<int>(activeTimedEffects[0], "StackCount"), Is.EqualTo(2), $"Expected stack count 2 for {powerUpCase.DisplayName}.");
            Assert.That(
                GetPropertyValue<float>(activeTimedEffects[0], "RemainingDuration"),
                Is.EqualTo(powerUpCase.DurationSeconds).Within(0.0001f),
                $"Expected refreshed duration for {powerUpCase.DisplayName}.");

            UnityEngine.Object.DestroyImmediate(controller.gameObject);
            controllerObject = null;
        }
    }

    [Test]
    public void ApplyingAdvancedTimedPowerUpsUpdatesControllerAndBallState()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(controller, "serveBall", serveBall);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);

        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Sticky Paddle", PowerUpEffectType.StickyPaddle, true, 15f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 15f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Phase Ball", PowerUpEffectType.PhaseBall, true, 12f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Chain Lightning", PowerUpEffectType.ChainLightning, true, 14f, 0.35f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Reverse Controls", PowerUpEffectType.ReverseControls, false, 8f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Split Paddle", PowerUpEffectType.SplitPaddle, false, 12f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Gravity Well", PowerUpEffectType.GravityWell, false, 12f, 0.35f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Fog of War", PowerUpEffectType.FogOfWar, false, 10f, 0.55f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Lag Spike", PowerUpEffectType.LagSpike, false, 8f, 0.5f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Boom Ball", PowerUpEffectType.ExplosiveBall, true, 10f, 1f));

        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        var ballRenderer = serveBall.GetComponent<SpriteRenderer>();

        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "StickyPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "LaserPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "PhaseBallEnabled"), Is.True);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ChainLightningStrength"), Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "FogVisibilityMultiplier"), Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ExplosiveBallStrength"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(paddle, "controlsReversed"), Is.True);
        Assert.That(GetPrivateField<float>(paddle, "splitGapWidthNormalized"), Is.GreaterThan(0.2f));
        Assert.That(GetPrivateField<float>(paddle, "lagSpikeStrength"), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(serveBall, "phaseThroughBricks"), Is.True);
        Assert.That(GetPrivateField<float>(serveBall, "gravityWellStrength"), Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(serveBall.IsExplosiveBall, Is.True);
        Assert.That(ballRenderer.color.r, Is.GreaterThan(ballRenderer.color.g));
    }

    [Test]
    public void FireLaserVolleyCreatesBeamVisualsForResolvedTargets()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        var leftBrick = CreateBrickHarness(controller, "Left Target", 100, new Vector2(-1.2f, 1.6f));
        var rightBrick = CreateBrickHarness(controller, "Right Target", 100, new Vector2(1.2f, 1.6f));
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var effectsRoot = GetPrivateField<Transform>(controller, "effectsRoot");

        SetPrivateField(leftBrick, "hitPointsRemaining", 2);
        SetPrivateField(rightBrick, "hitPointsRemaining", 2);
        SetPrivateField(controller, "serveBall", serveBall);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        bricks.Add(leftBrick);
        bricks.Add(rightBrick);

        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Laser Paddle", PowerUpEffectType.LaserPaddle, true, 15f, 1f));
        SetPrivateEnumField(controller, "roundState", "Playing");

        var fired = (bool)InvokePrivateMethodWithResult(controller, "FireLaserVolley");

        Assert.That(fired, Is.True);
        Assert.That(effectsRoot.childCount, Is.EqualTo(2));
        Assert.That(effectsRoot.GetComponentsInChildren<LineRenderer>().Length, Is.EqualTo(6));
    }

    [Test]
    public void ExplosiveBallImpactDestroysNearbyBricksAndSpeedsBall()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var ballBody = scoringBall.GetComponent<Rigidbody2D>();
        var sourceBrick = CreateBrickHarness(controller, "Source", 100, Vector2.zero);
        var nearBrick = CreateBrickHarness(controller, "Near", 100, new Vector2(1.15f, 0f));
        var farBrick = CreateBrickHarness(controller, "Far", 100, new Vector2(3.5f, 0f));
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        bricks.Add(sourceBrick);
        bricks.Add(nearBrick);
        bricks.Add(farBrick);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(scoringBall);

        SetPrivateField(scoringBall, "hasLaunched", true);
        ballBody.linearVelocity = Vector2.up * 8f;
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Boom Ball", PowerUpEffectType.ExplosiveBall, true, 10f, 1f));

        controller.HandleBrickDestroyed(sourceBrick, scoringBall, BrickDestructionCause.Impact);

        Assert.That(bricks.Count, Is.EqualTo(1));
        Assert.That(bricks[0], Is.SameAs(farBrick));
        Assert.That(ballBody.linearVelocity.magnitude, Is.GreaterThan(8f));
    }

    [Test]
    public void ApplyingShieldWallPowerUpGrantsAndConsumesRescueCharge()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateEnumField(controller, "roundState", "Playing");

        InvokePrivateMethod(
            controller,
            "ApplyPowerUp",
            CreatePowerUp("Shield Wall", PowerUpEffectType.ShieldWall, true, 0f, 1f));

        Assert.That(GetPrivateField<int>(controller, "shieldWallCharges"), Is.EqualTo(1));
        Assert.That(controller.TryRescueBallWithShield(serveBall), Is.True);
        Assert.That(GetPrivateField<int>(controller, "shieldWallCharges"), Is.EqualTo(0));
        Assert.That(serveBall.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(0f));
    }

    [Test]
    public void LosingALifeInHighScoreModeSubtractsPenaltyFromScoreAndKeepsRunAlive()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.HighScore, 3, 650, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null));
        SetPrivateField(controller, "serveBall", serveBall);
        SetPrivateField(controller, "livesRemaining", 3);
        SetPrivateField(controller, "score", 125);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandleBallLost(serveBall);

        Assert.That(GetPrivateField<int>(controller, "livesRemaining"), Is.EqualTo(3));
        Assert.That(GetPrivateField<int>(controller, "lifeLossCount"), Is.EqualTo(1));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(-525));
        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("LifeLost"));
        Assert.That(GetPrivateField<List<BallController>>(controller, "activeBalls").Count, Is.EqualTo(1));
    }

    [Test]
    public void RapidBrickBreaksAwardSlamChainBonusAndCreatePopup()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(scoringBall);
        var firstBrick = CreateBrickHarness(controller, "Brick A", 100, new Vector2(-1f, 1f));
        var secondBrick = CreateBrickHarness(controller, "Brick B", 100, new Vector2(1f, 1f));
        bricks.Add(firstBrick);
        bricks.Add(secondBrick);

        controller.HandleBrickDestroyed(firstBrick, scoringBall, BrickDestructionCause.Impact);
        controller.HandleBrickDestroyed(secondBrick, scoringBall, BrickDestructionCause.Impact);

        var popups = GetFloatingScorePopups(controller);

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(235));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+21"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Is.EqualTo("COMBO BONUS: SLAM CHAIN!"));
    }

    [Test]
    public void RicochetKillsAwardBankShotBonusAndResetBallChain()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var brick = CreateBrickHarness(controller, "Ricochet Brick", 100, new Vector2(0f, 2f));
        SetPrivateField(scoringBall, "ricochetCountSinceLastBrick", 3);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(scoringBall);
        GetPrivateField<List<Brick>>(controller, "bricks").Add(brick);

        controller.HandleBrickDestroyed(brick, scoringBall, BrickDestructionCause.Impact);

        var popups = GetFloatingScorePopups(controller);

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(152));
        Assert.That(GetPrivateField<int>(scoringBall, "ricochetCountSinceLastBrick"), Is.EqualTo(0));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+45"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Is.EqualTo("COMBO BONUS: BANK SHOT!"));
    }

    [Test]
    public void DifferentBallsScoringBackToBackAwardPartySplitBonus()
    {
        var controller = CreateControllerHarness(out var paddle);
        var firstBall = CreateBallHarness(controller, paddle);
        var secondBall = CreateBallHarness(controller, paddle);
        var firstBrick = CreateBrickHarness(controller, "First Brick", 100, new Vector2(-1f, 1f));
        var secondBrick = CreateBrickHarness(controller, "Second Brick", 100, new Vector2(1f, 1f));
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var balls = GetPrivateField<List<BallController>>(controller, "activeBalls");
        balls.Add(firstBall);
        balls.Add(secondBall);
        bricks.Add(firstBrick);
        bricks.Add(secondBrick);

        controller.HandleBrickDestroyed(firstBrick, firstBall, BrickDestructionCause.Impact);
        controller.HandleBrickDestroyed(secondBrick, secondBall, BrickDestructionCause.Impact);

        var popups = GetFloatingScorePopups(controller);

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(262));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+48"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Is.EqualTo("COMBO BONUS: SLAM CHAIN + PARTY SPLIT!"));
    }

    private BreakoutGameController CreateControllerHarness(out PaddleController paddle)
    {
        controllerObject = new GameObject("BreakoutGameController Test");
        var controller = controllerObject.AddComponent<BreakoutGameController>();

        paddleObject = new GameObject("Paddle");
        paddleObject.transform.localScale = new Vector3(2.1f, 0.74f, 1f);
        var paddleCollider = paddleObject.AddComponent<BoxCollider2D>();
        paddleObject.AddComponent<Rigidbody2D>();
        paddle = paddleObject.AddComponent<PaddleController>();
        paddle.Configure(controller, 12f, -10f, 10f, -3.5f);

        var effectsRoot = new GameObject("Effects").transform;
        runtimeObjects.Add(effectsRoot.gameObject);

        SetPrivateField(controller, "paddle", paddle);
        SetPrivateField(controller, "paddleCollider", paddleCollider);
        SetPrivateField(controller, "effectsRoot", effectsRoot);
        SetPrivateField(controller, "powerUpService", CreatePowerUpService());
        SetPrivateField(controller, "scoreService", new BreakoutScoreService());
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null));
        SetPrivateField(controller, "currentLevelPaddleSpeed", 12f);
        SetPrivateField(controller, "currentLevelBallSpeed", 8f);
        SetPrivateField(controller, "arenaLeft", -8f);
        SetPrivateField(controller, "arenaRight", 8f);
        SetPrivateField(controller, "arenaTop", 5f);
        SetPrivateField(controller, "arenaBottom", -5f);
        InvokePrivateMethod(controller, "CreateBrickService");
        return controller;
    }

    private BallController CreateBallHarness(BreakoutGameController controller, PaddleController paddle)
    {
        var ballObject = new GameObject("Ball");
        runtimeObjects.Add(ballObject);
        ballObject.AddComponent<SpriteRenderer>();
        ballObject.AddComponent<CircleCollider2D>();
        ballObject.AddComponent<Rigidbody2D>();
        var ball = ballObject.AddComponent<BallController>();
        ball.Configure(controller, paddle, 8f, 0.35f, -6f, 0.5f, false);
        ball.ApplyVisualStyle(new ThemeVisualStyle(Color.yellow, Color.yellow, null));
        return ball;
    }

    private Brick CreateBrickHarness(BreakoutGameController controller, string displayName, int scoreValue, Vector2 worldPosition)
    {
        var brickObject = new GameObject(displayName);
        runtimeObjects.Add(brickObject);
        brickObject.transform.position = worldPosition;
        brickObject.AddComponent<BoxCollider2D>();
        var brick = brickObject.AddComponent<Brick>();
        var definition = CreateBrickDefinition(displayName, 1, scoreValue);
        SetPrivateField(brick, "gameController", controller);
        SetPrivateField(brick, "definition", definition);
        return brick;
    }

    private BrickDefinition CreateBrickDefinition(string displayName, int hitPoints, int scoreValue)
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(definition);
        SetPrivateField(definition, "displayName", displayName);
        SetPrivateField(definition, "hitPoints", hitPoints);
        SetPrivateField(definition, "scoreValue", scoreValue);
        SetPrivateField(definition, "indestructible", false);
        SetPrivateField(definition, "countsTowardLevelCompletion", true);
        SetPrivateField(definition, "dropChance", 0f);
        SetPrivateField(definition, "dropTable", Array.Empty<BrickPowerUpDropEntry>());
        return definition;
    }

    private static object CreatePowerUpService()
    {
        var serviceType = GetGameplayType("GetBricked.Gameplay.BreakoutPowerUpService");
        return Activator.CreateInstance(serviceType, new Vector2(0.55f, 0.55f), 3.2f, 22f, null);
    }

    private Sprite CreateSquareSprite()
    {
        var texture = new Texture2D(8, 8);
        runtimeObjects.Add(texture);
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        runtimeObjects.Add(sprite);
        return sprite;
    }

    private static PowerUpDefinition CreatePowerUp(
        string displayName,
        PowerUpEffectType effectType,
        bool beneficial,
        float durationSeconds,
        float scalar)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "effectType", effectType);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "durationSeconds", durationSeconds);
        SetPrivateField(powerUp, "scalar", scalar);
        SetPrivateField(powerUp, "extraBallCount", 0);
        return powerUp;
    }

    private static TimedPowerUpCase CreateTimedPowerUpCase(
        string displayName,
        PowerUpEffectType effectType,
        bool beneficial,
        float durationSeconds,
        float scalar)
    {
        return new TimedPowerUpCase(displayName, effectType, beneficial, durationSeconds, scalar);
    }

    private static Type GetGameplayType(string fullName)
    {
        var assembly = typeof(BreakoutGameController).Assembly;
        var resolvedType = assembly.GetType(fullName, throwOnError: false);
        Assert.That(resolvedType, Is.Not.Null, $"Could not resolve gameplay type '{fullName}'.");
        return resolvedType;
    }

    private static void InvokePrivateMethod(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, InstanceFlags);
        Assert.That(method, Is.Not.Null, $"Missing method '{methodName}' on {instance.GetType().Name}.");
        method.Invoke(instance, args);
    }

    private static object InvokePrivateMethodWithResult(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, InstanceFlags);
        Assert.That(method, Is.Not.Null, $"Missing method '{methodName}' on {instance.GetType().Name}.");
        return method.Invoke(instance, args);
    }

    private static bool IsBrickosaurusPowerDownCandidate(PowerUpDefinition definition)
    {
        var method = typeof(BreakoutGameController).GetMethod("IsBrickosaurusPowerDownCandidate", StaticFlags);
        Assert.That(method, Is.Not.Null, "Missing Brickosaurus power-down candidate resolver.");
        return (bool)method.Invoke(null, new object[] { definition });
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        return (T)field.GetValue(instance);
    }

    private static T GetFieldValue<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        return (T)field.GetValue(instance);
    }

    private static System.Collections.IList GetFloatingScorePopups(BreakoutGameController controller)
    {
        var scoreService = GetPrivateField<object>(controller, "scoreService");
        return GetPrivateField<System.Collections.IList>(scoreService, "floatingScorePopups");
    }

    private static T GetPropertyValue<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, InstanceFlags);
        Assert.That(property, Is.Not.Null, $"Missing property '{propertyName}' on {instance.GetType().Name}.");
        return (T)property.GetValue(instance);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }

    private static void SetPrivateEnumField(object instance, string fieldName, string enumValue)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        var resolvedEnumValue = Enum.Parse(field.FieldType, enumValue);
        field.SetValue(instance, resolvedEnumValue);
    }

    private readonly struct TimedPowerUpCase
    {
        public TimedPowerUpCase(string displayName, PowerUpEffectType effectType, bool beneficial, float durationSeconds, float scalar)
        {
            DisplayName = displayName;
            EffectType = effectType;
            Beneficial = beneficial;
            DurationSeconds = durationSeconds;
            Scalar = scalar;
        }

        public string DisplayName { get; }

        public PowerUpEffectType EffectType { get; }

        public bool Beneficial { get; }

        public float DurationSeconds { get; }

        public float Scalar { get; }
    }
}
