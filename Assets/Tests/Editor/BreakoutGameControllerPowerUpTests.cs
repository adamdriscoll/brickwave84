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
    public void ApplyingStaticShoesSlowsPaddleMovementImmediately()
    {
        var controller = CreateControllerHarness(out var paddle);
        var staticShoes = CreatePowerUp(
            "Static Shoes",
            PowerUpEffectType.PaddleSpeedMultiplier,
            beneficial: false,
            durationSeconds: 8f,
            scalar: 0.6f);

        InvokePrivateMethod(controller, "ApplyPowerUp", staticShoes);

        Assert.That(GetPrivateField<float>(paddle, "moveSpeed"), Is.EqualTo(12f * 0.6f).Within(0.0001f));
    }

    [Test]
    public void ApplyingWrapRailMovesPaddleAcrossSideBounds()
    {
        var controller = CreateControllerHarness(out var paddle);
        var wrapPowerUp = CreatePowerUp(
            "Wrap Rail",
            PowerUpEffectType.PaddleWrap,
            beneficial: true,
            durationSeconds: 10f,
            scalar: 1f);

        InvokePrivateMethod(controller, "ApplyPowerUp", wrapPowerUp);

        var minX = -10f + paddle.HalfWidthWorld;
        var maxX = 10f - paddle.HalfWidthWorld;

        Assert.That(GetPrivateField<bool>(paddle, "wrapRailEnabled"), Is.True);
        Assert.That((float)InvokePrivateMethodWithResult(paddle, "ResolveNextHorizontalPosition", maxX + 0.01f), Is.EqualTo(maxX + 0.01f).Within(0.0001f));
        Assert.That((float)InvokePrivateMethodWithResult(paddle, "ResolveNextHorizontalPosition", 10.01f), Is.EqualTo(-9.99f).Within(0.0001f));
        Assert.That((bool)InvokePrivateMethodWithResult(paddle, "DidWrapRailPosition", 10.01f, -9.99f), Is.True);
        Assert.That((float)InvokePrivateMethodWithResult(paddle, "ResolveNextHorizontalPosition", minX - 0.01f), Is.EqualTo(minX - 0.01f).Within(0.0001f));
        Assert.That((float)InvokePrivateMethodWithResult(paddle, "ResolveNextHorizontalPosition", -10.01f), Is.EqualTo(9.99f).Within(0.0001f));
        Assert.That((bool)InvokePrivateMethodWithResult(paddle, "DidWrapRailPosition", -10.01f, 9.99f), Is.True);

        paddle.SetWrapRailEnabled(false);

        Assert.That((float)InvokePrivateMethodWithResult(paddle, "ResolveNextHorizontalPosition", maxX + 0.01f), Is.EqualTo(maxX).Within(0.0001f));
        Assert.That((bool)InvokePrivateMethodWithResult(paddle, "DidWrapRailPosition", 10.01f, -9.99f), Is.False);
    }

    [Test]
    public void WrapRailShowsOppositeSideEchoWhilePaddleEntersWall()
    {
        var controller = CreateControllerHarness(out var paddle);
        var visualObject = new GameObject("Visual");
        visualObject.transform.SetParent(paddle.transform, false);
        var sourceRenderer = visualObject.AddComponent<SpriteRenderer>();
        sourceRenderer.sortingOrder = 40;
        BreakoutSpriteRendererUtility.ApplyTint(sourceRenderer, Color.white);
        var wrapPowerUp = CreatePowerUp(
            "Wrap Rail",
            PowerUpEffectType.PaddleWrap,
            beneficial: true,
            durationSeconds: 10f,
            scalar: 1f);

        InvokePrivateMethod(controller, "ApplyPowerUp", wrapPowerUp);
        var rightCrossingX = Mathf.Lerp(10f - paddle.HalfWidthWorld, 10f, 0.5f);
        var leftCrossingX = Mathf.Lerp(-10f, -10f + paddle.HalfWidthWorld, 0.5f);

        InvokePrivateMethod(paddle, "UpdateWrapRailEcho", rightCrossingX, -3.5f, 0f);

        var echoObject = GetPrivateField<GameObject>(paddle, "wrapRailEchoObject");
        var sourceTint = BreakoutSpriteRendererUtility.ResolveTint(sourceRenderer);

        Assert.That(echoObject, Is.Not.Null);
        Assert.That(echoObject.activeSelf, Is.True);
        Assert.That(echoObject.transform.position.x, Is.EqualTo(rightCrossingX - 20f).Within(0.0001f));
        Assert.That(sourceTint.a, Is.EqualTo(0.75f).Within(0.0001f));

        InvokePrivateMethod(paddle, "UpdateWrapRailEcho", leftCrossingX, -3.5f, 0f);

        Assert.That(echoObject.activeSelf, Is.True);
        Assert.That(echoObject.transform.position.x, Is.EqualTo(leftCrossingX + 20f).Within(0.0001f));

        paddle.SetWrapRailEnabled(false);

        Assert.That(echoObject.activeSelf, Is.False);
        Assert.That(BreakoutSpriteRendererUtility.ResolveTint(sourceRenderer).a, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void ApplyingTiltRailRotatesPaddleOnBallHitsAndAnimatesBackAfterExpiry()
    {
        var controller = CreateControllerHarness(out var paddle);
        var tiltPowerUp = CreatePowerUp(
            "Tilt Rail",
            PowerUpEffectType.PaddleHitTilt,
            beneficial: false,
            durationSeconds: 12f,
            scalar: 9f);

        InvokePrivateMethod(controller, "ApplyPowerUp", tiltPowerUp);

        for (var index = 0; index < 41; index++)
        {
            controller.ApplyPaddleHitTilt(paddle, paddle.transform.position.x + paddle.HalfWidthWorld);
        }

        Assert.That(GetPrivateField<float>(paddle, "hitTiltDegrees"), Is.EqualTo(9f).Within(0.0001f));
        Assert.That(GetPrivateField<float>(paddle, "currentHitTiltRotation"), Is.GreaterThan(360f));

        paddle.SetHitTiltDegrees(0f);

        var returningRotation = GetPrivateField<float>(paddle, "currentHitTiltRotation");
        Assert.That(GetPrivateField<bool>(paddle, "isHitTiltReturning"), Is.True);
        Assert.That(returningRotation, Is.Not.Zero);
        Assert.That(Mathf.Abs(returningRotation), Is.LessThan(180f));

        InvokePrivateMethod(paddle, "UpdateHitTiltReturn", 0.02f);

        Assert.That(
            Mathf.Abs(GetPrivateField<float>(paddle, "currentHitTiltRotation")),
            Is.LessThan(Mathf.Abs(returningRotation)));

        InvokePrivateMethod(paddle, "UpdateHitTiltReturn", 1f);

        Assert.That(GetPrivateField<float>(paddle, "currentHitTiltRotation"), Is.Zero);
        Assert.That(GetPrivateField<bool>(paddle, "isHitTiltReturning"), Is.False);
    }

    [Test]
    public void TiltRailUsesIncomingBallAngleForDirectionAndStrength()
    {
        var controller = CreateControllerHarness(out var paddle);
        var tiltPowerUp = CreatePowerUp(
            "Tilt Rail",
            PowerUpEffectType.PaddleHitTilt,
            beneficial: false,
            durationSeconds: 12f,
            scalar: 9f);

        InvokePrivateMethod(controller, "ApplyPowerUp", tiltPowerUp);
        controller.ApplyPaddleHitTilt(
            paddle,
            paddle.transform.position.x - paddle.HalfWidthWorld,
            new Vector2(7f, -7f));

        var diagonalTilt = GetPrivateField<float>(paddle, "currentHitTiltRotation");
        Assert.That(diagonalTilt, Is.GreaterThan(9f));

        paddle.SetHitTiltDegrees(0f);
        InvokePrivateMethod(paddle, "UpdateHitTiltReturn", 1f);
        paddle.SetHitTiltDegrees(9f);
        controller.ApplyPaddleHitTilt(
            paddle,
            paddle.transform.position.x + paddle.HalfWidthWorld,
            new Vector2(-0.2f, -9f));

        var verticalTilt = GetPrivateField<float>(paddle, "currentHitTiltRotation");
        Assert.That(verticalTilt, Is.GreaterThan(0f));
        Assert.That(verticalTilt, Is.LessThan(9f));
        Assert.That(diagonalTilt, Is.GreaterThan(verticalTilt));
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
    public void ApplyingStackedMegaBallPowerUpsPopsBallBackToRegularSize()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(controller, "serveBall", serveBall);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        var megaBallPowerUp = CreatePowerUp(
            "Mega Ball",
            PowerUpEffectType.BallSizeMultiplier,
            beneficial: true,
            durationSeconds: 10f,
            scalar: 1.8f);

        InvokePrivateMethod(controller, "ApplyPowerUp", megaBallPowerUp);

        Assert.That(serveBall.transform.localScale.x, Is.EqualTo(1.8f).Within(0.0001f));

        InvokePrivateMethod(controller, "ApplyPowerUp", megaBallPowerUp);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");
        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        var pickupBannerView = InvokePrivateMethodWithResult(controller, "BuildPickupBannerView");

        Assert.That(activeTimedEffects.Count, Is.Zero);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "BallSizeMultiplier"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(serveBall.transform.localScale.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(serveBall.transform.localScale.y, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetFieldValue<string>(pickupBannerView, "Text"), Is.EqualTo("MEGA POP!"));
    }

    [Test]
    public void ApplyingFourMicroSparkPowerUpsPopsBallBackToRegularSize()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(controller, "serveBall", serveBall);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        var microSparkPowerUp = CreatePowerUp(
            "Micro Spark",
            PowerUpEffectType.MicroSpark,
            beneficial: true,
            durationSeconds: 10f,
            scalar: 0.55f,
            secondaryScalar: 1.75f);

        for (var index = 0; index < 3; index++)
        {
            InvokePrivateMethod(controller, "ApplyPowerUp", microSparkPowerUp);
        }

        Assert.That(serveBall.transform.localScale.x, Is.LessThan(0.2f));

        InvokePrivateMethod(controller, "ApplyPowerUp", microSparkPowerUp);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activeTimedEffects = GetPropertyValue<System.Collections.IList>(powerUpService, "ActiveTimedEffects");
        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        var pickupBannerView = InvokePrivateMethodWithResult(controller, "BuildPickupBannerView");

        Assert.That(activeTimedEffects.Count, Is.Zero);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "BallSizeMultiplier"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ScoreMultiplier"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(serveBall.transform.localScale.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(serveBall.transform.localScale.y, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetFieldValue<string>(pickupBannerView, "Text"), Is.EqualTo("MICRO POP!"));
    }

    [Test]
    public void ApplyingMissileDropAddsOneMissile()
    {
        var controller = CreateControllerHarness(out _);
        SetPrivateField(controller, "availableMissiles", 3);
        var missileDrop = CreatePowerUp(
            "Brick Missile",
            PowerUpEffectType.MissileStock,
            beneficial: true,
            durationSeconds: 0f,
            scalar: 1f);

        InvokePrivateMethod(controller, "ApplyPowerUp", missileDrop);

        Assert.That(GetPrivateField<int>(controller, "availableMissiles"), Is.EqualTo(4));
    }

    [Test]
    public void SolarShotBurnsThroughWeakBrickAndConsumesOneCharge()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var brick = CreateBrickHarness(controller, "Solar Brick", 100, new Vector2(0f, 2f));
        var solarShot = CreatePowerUp(
            "Solar Shot",
            PowerUpEffectType.SolarShot,
            beneficial: true,
            durationSeconds: 0f,
            scalar: 1f);

        SetPrivateField(brick, "maxHitPoints", 1);
        SetPrivateField(brick, "hitPointsRemaining", 1);
        GetPrivateField<List<Brick>>(controller, "bricks").Add(brick);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(scoringBall);
        InvokePrivateMethod(controller, "ApplyPowerUp", solarShot);

        var chargedColor = ResolveSpriteTint(scoringBall.GetComponent<SpriteRenderer>());
        Assert.That(chargedColor.g, Is.LessThan(0.85f));
        Assert.That(chargedColor.b, Is.GreaterThan(0.04f));
        Assert.That(controller.TryHandleSolarShot(scoringBall, brick), Is.True);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var dischargedColor = ResolveSpriteTint(scoringBall.GetComponent<SpriteRenderer>());
        Assert.That(GetPropertyValue<int>(powerUpService, "SolarShotCharges"), Is.Zero);
        Assert.That(dischargedColor.g, Is.GreaterThan(0.9f));
        Assert.That(dischargedColor.b, Is.LessThan(0.02f));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.GreaterThan(0));
        Assert.That(GetPrivateField<List<Brick>>(controller, "bricks"), Is.Empty);
    }

    [Test]
    public void SolarShotIgnoresBricksThatStillNeedMultipleHits()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var brick = CreateBrickHarness(controller, "Fortified Solar Brick", 100, new Vector2(0f, 2f));
        var solarShot = CreatePowerUp(
            "Solar Shot",
            PowerUpEffectType.SolarShot,
            beneficial: true,
            durationSeconds: 0f,
            scalar: 1f);

        SetPrivateField(brick, "maxHitPoints", 2);
        SetPrivateField(brick, "hitPointsRemaining", 2);
        InvokePrivateMethod(controller, "ApplyPowerUp", solarShot);

        Assert.That(controller.TryHandleSolarShot(scoringBall, brick), Is.False);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        Assert.That(GetPropertyValue<int>(powerUpService, "SolarShotCharges"), Is.EqualTo(1));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.Zero);
    }

    [Test]
    public void FuseBurstClearsMostDamagedBrickAndStartsBlackout()
    {
        var controller = CreateControllerHarness(out _);
        var undamaged = CreateBrickHarness(controller, "Undamaged Brick", 100, new Vector2(-1f, 2f));
        var target = CreateBrickHarness(controller, "Fuse Target", 100, new Vector2(0f, 3f));
        var lessDamaged = CreateBrickHarness(controller, "Less Damaged Brick", 100, new Vector2(1f, 4f));
        var undamagedRenderer = AttachBrickRenderer(undamaged);
        var lessDamagedRenderer = AttachBrickRenderer(lessDamaged);
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var fuseBurst = CreatePowerUp(
            "Fuse Burst",
            PowerUpEffectType.FuseBurst,
            beneficial: true,
            durationSeconds: 4f,
            scalar: 0f);

        SetPrivateField(undamaged, "maxHitPoints", 3);
        SetPrivateField(undamaged, "hitPointsRemaining", 3);
        SetPrivateField(target, "maxHitPoints", 4);
        SetPrivateField(target, "hitPointsRemaining", 1);
        SetPrivateField(lessDamaged, "maxHitPoints", 4);
        SetPrivateField(lessDamaged, "hitPointsRemaining", 2);
        bricks.Add(undamaged);
        bricks.Add(target);
        bricks.Add(lessDamaged);
        SetPrivateField(controller, "requiredBricksRemaining", 3);

        InvokePrivateMethod(controller, "ApplyPowerUp", fuseBurst);

        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        Assert.That(bricks, Has.No.Member(target));
        Assert.That(bricks, Has.Member(undamaged));
        Assert.That(bricks, Has.Member(lessDamaged));
        Assert.That(GetPrivateField<int>(controller, "requiredBricksRemaining"), Is.EqualTo(2));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.GreaterThan(0));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "FogVisibilityMultiplier"), Is.Zero);
        Assert.That(undamagedRenderer.color.a, Is.Zero);
        Assert.That(lessDamagedRenderer.color.a, Is.Zero);
    }

    [Test]
    public void FuseBurstClearsWeakBrickWhenNoneAreDamaged()
    {
        var controller = CreateControllerHarness(out _);
        var lowerBrick = CreateBrickHarness(controller, "Lower Brick", 100, new Vector2(1f, 1f));
        var higherBrick = CreateBrickHarness(controller, "Higher Brick", 100, new Vector2(-1f, 3f));
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var fuseBurst = CreatePowerUp(
            "Fuse Burst",
            PowerUpEffectType.FuseBurst,
            beneficial: true,
            durationSeconds: 4f,
            scalar: 0f);

        SetPrivateField(lowerBrick, "maxHitPoints", 1);
        SetPrivateField(lowerBrick, "hitPointsRemaining", 1);
        SetPrivateField(higherBrick, "maxHitPoints", 1);
        SetPrivateField(higherBrick, "hitPointsRemaining", 1);
        bricks.Add(lowerBrick);
        bricks.Add(higherBrick);
        SetPrivateField(controller, "requiredBricksRemaining", 2);

        InvokePrivateMethod(controller, "ApplyPowerUp", fuseBurst);

        Assert.That(bricks, Has.Member(lowerBrick));
        Assert.That(bricks, Has.No.Member(higherBrick));
        Assert.That(GetPrivateField<int>(controller, "requiredBricksRemaining"), Is.EqualTo(1));
    }

    [Test]
    public void RewardMissilePurchaseRequiresFiveThousandPoints()
    {
        Assert.That(BreakoutGameController.CanPurchaseMissile(4999), Is.False);
        Assert.That(BreakoutGameController.CanPurchaseMissile(5000), Is.True);
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
            CreateTimedPowerUpCase("Static Shoes", PowerUpEffectType.PaddleSpeedMultiplier, false, 8f, 0.6f),
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
            CreateTimedPowerUpCase("Final Breakthru", PowerUpEffectType.FinalBreakthru, true, 6f, 2f),
            CreateTimedPowerUpCase("Vector Sight", PowerUpEffectType.VectorSight, true, 14f, 1f),
            CreateTimedPowerUpCase("Mirror Image", PowerUpEffectType.MirrorImagePaddle, true, 12f, 1f),
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
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Final Breakthru", PowerUpEffectType.FinalBreakthru, true, 6f, 2f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Vector Sight", PowerUpEffectType.VectorSight, true, 14f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Mirror Image", PowerUpEffectType.MirrorImagePaddle, true, 12f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Wrap Rail", PowerUpEffectType.PaddleWrap, true, 10f, 1f));

        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");
        var ballRenderer = serveBall.GetComponent<SpriteRenderer>();

        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "StickyPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "LaserPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "PhaseBallEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "WeakBrickPierceEnabled"), Is.True);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ChainLightningStrength"), Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "FogVisibilityMultiplier"), Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ExplosiveBallStrength"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "VectorSightStrength"), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "MirrorImagePaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "PaddleWrapEnabled"), Is.True);
        Assert.That(GetPrivateField<bool>(paddle, "controlsReversed"), Is.True);
        Assert.That(GetPrivateField<bool>(paddle, "wrapRailEnabled"), Is.True);
        Assert.That(GetPrivateField<float>(paddle, "splitGapWidthNormalized"), Is.GreaterThan(0.2f));
        Assert.That(GetPrivateField<float>(paddle, "lagSpikeStrength"), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(serveBall, "phaseThroughBricks"), Is.True);
        Assert.That(GetPrivateField<bool>(serveBall, "weakBrickPierceThroughBricks"), Is.True);
        Assert.That(GetPrivateField<float>(serveBall, "gravityWellStrength"), Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(serveBall.IsExplosiveBall, Is.True);
        var ballTintBlock = new MaterialPropertyBlock();
        ballRenderer.GetPropertyBlock(ballTintBlock);
        var ballTint = ballTintBlock.GetColor(Shader.PropertyToID("_Color"));
        Assert.That(ballTint.r, Is.GreaterThan(ballTint.g));
    }

    [Test]
    public void ApplyingMirrorImageCreatesOppositeRailWithCurrentPaddleModifiers()
    {
        var controller = CreateControllerHarness(out var paddle);

        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Mirror Image", PowerUpEffectType.MirrorImagePaddle, true, 12f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 12f, 1.45f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Reverse Controls", PowerUpEffectType.ReverseControls, false, 8f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Split Paddle", PowerUpEffectType.SplitPaddle, false, 12f, 1f));
        InvokePrivateMethod(controller, "ApplyPowerUp", CreatePowerUp("Lag Spike", PowerUpEffectType.LagSpike, false, 8f, 0.5f));

        var mirrorObject = GetPrivateField<GameObject>(paddle, "mirrorImagePaddleObject");
        var mirrorPaddle = GetPrivateField<PaddleController>(paddle, "mirrorImagePaddle");

        Assert.That(mirrorObject, Is.Not.Null);
        Assert.That(mirrorObject.activeSelf, Is.True);
        Assert.That(mirrorPaddle.transform.position.y, Is.GreaterThan(paddle.transform.position.y + 1f));
        Assert.That(GetPrivateField<float>(mirrorPaddle, "inputDirectionMultiplier"), Is.EqualTo(-1f).Within(0.0001f));
        Assert.That(mirrorPaddle.transform.localScale.x, Is.EqualTo(paddle.transform.localScale.x).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(mirrorPaddle, "controlsReversed"), Is.True);
        Assert.That(GetPrivateField<float>(mirrorPaddle, "splitGapWidthNormalized"), Is.EqualTo(GetPrivateField<float>(paddle, "splitGapWidthNormalized")).Within(0.0001f));
        Assert.That(GetPrivateField<float>(mirrorPaddle, "lagSpikeStrength"), Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void CloneStaticPaddleFollowsDelayedPaddleSamples()
    {
        CreateControllerHarness(out var paddle);
        var spec = new BreakoutCloneStaticSpec(0.4f, 0.9f, 0.7f);

        paddle.SetCloneStaticPaddleEnabled(true, spec);
        InvokePrivateMethod(paddle, "RecordCloneStaticSample", 0f, new Vector2(-2f, -3.5f), 0f);
        InvokePrivateMethod(paddle, "RecordCloneStaticSample", 0.4f, new Vector2(2f, -3.5f), 0f);
        InvokePrivateMethod(paddle, "RecordCloneStaticSample", 0.8f, new Vector2(6f, -3.5f), 0f);
        InvokePrivateMethod(paddle, "UpdateCloneStaticPaddle", 0.8f);

        var cloneStaticObject = GetPrivateField<GameObject>(paddle, "cloneStaticPaddleObject");

        Assert.That(cloneStaticObject, Is.Not.Null);
        Assert.That(cloneStaticObject.activeSelf, Is.True);
        Assert.That(cloneStaticObject.transform.position.x, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(cloneStaticObject.transform.position.y, Is.EqualTo(-2.6f).Within(0.0001f));
        Assert.That(cloneStaticObject.transform.localScale.x, Is.EqualTo(paddle.transform.localScale.x * 0.7f).Within(0.0001f));

        paddle.SetCloneStaticPaddleEnabled(false);

        Assert.That(cloneStaticObject.activeSelf, Is.False);
    }

    [Test]
    public void CleanCatchRelaunchesWithAmplifiedPaddleAim()
    {
        var controller = CreateControllerHarness(out var paddle);
        var ball = CreateBallHarness(controller, paddle);

        var regularDirection = ball.ResolvePaddleBounceDirection(paddle, paddle.transform.position.x + (paddle.HalfWidthWorld * 0.45f));
        ball.AttachToPaddle();
        ball.LaunchFromPaddleAim(0.45f, 1.35f);

        Assert.That(ball.HasLaunched, Is.True);
        Assert.That(ball.CurrentVelocity.normalized.x, Is.GreaterThan(regularDirection.x));
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
    public void BrickBloomSpawnsTwoTinyBonusBricksOnNextBrokenBrick()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var sourceBrick = CreateBrickHarness(controller, "Source", 100, Vector2.zero);
        var tinyDefinition = CreateBrickDefinition("Tiny Brick", 1, 325);
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var loadedBrickDefinitions = GetPrivateField<List<BrickDefinition>>(controller, "loadedBrickDefinitions");
        var powerUpService = GetPrivateField<object>(controller, "powerUpService");

        SetPrivateField(tinyDefinition, "sizeMultiplier", 0.5f);
        loadedBrickDefinitions.Add(tinyDefinition);
        bricks.Add(sourceBrick);
        SetPrivateField(controller, "requiredBricksRemaining", 1);

        InvokePrivateMethod(
            controller,
            "ApplyPowerUp",
            CreatePowerUp("Brick Bloom", PowerUpEffectType.BrickBloom, true, 0f, 1f));

        controller.HandleBrickDestroyed(sourceBrick, scoringBall, BrickDestructionCause.Impact);

        Assert.That(GetPropertyValue<int>(powerUpService, "BrickBloomCharges"), Is.Zero);
        Assert.That(GetPrivateField<int>(controller, "requiredBricksRemaining"), Is.EqualTo(2));
        Assert.That(bricks, Has.Count.EqualTo(2));
        Assert.That(bricks, Has.All.Matches<Brick>(brick => brick.Definition == tinyDefinition));
        Assert.That(bricks.Exists(brick => brick.transform.position.x < 0f), Is.True);
        Assert.That(bricks.Exists(brick => brick.transform.position.x > 0f), Is.True);
    }

    [Test]
    public void ExplosiveBrickImpactSplitsScoringBallIntoThreeSmallBalls()
    {
        var controller = CreateControllerHarness(out _);
        var scoringBall = (BallController)InvokePrivateMethodWithResult(controller, "CreateBall", false);
        var ballBody = scoringBall.GetComponent<Rigidbody2D>();
        var sourceBrick = CreateBrickHarness(controller, "Explosive Brick", 250, Vector2.zero);
        var brickDefinition = sourceBrick.Definition;
        var activeBalls = GetPrivateField<List<BallController>>(controller, "activeBalls");
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");

        SetPrivateField(brickDefinition, "explosive", true);
        SetPrivateField(brickDefinition, "explosionSpeedMultiplier", 1.6f);
        SetPrivateField(brickDefinition, "explosionSpeedDuration", 2f);
        SetPrivateField(scoringBall, "hasLaunched", true);
        scoringBall.SetWorldPosition(new Vector2(0f, -0.3f));
        ballBody.linearVelocity = Vector2.up * 8f;
        activeBalls.Add(scoringBall);
        bricks.Add(sourceBrick);

        controller.HandleBrickDestroyed(sourceBrick, scoringBall, BrickDestructionCause.Impact);

        Assert.That(activeBalls.Count, Is.EqualTo(3));
        Assert.That(activeBalls, Has.All.Matches<BallController>(ball => ball != null && ball.HasLaunched));
        Assert.That(activeBalls, Has.All.Matches<BallController>(ball => ball.transform.localScale.x < 0.3f));
        Assert.That(activeBalls.Exists(ball => ball.CurrentVelocity.x < -0.1f), Is.True);
        Assert.That(activeBalls.Exists(ball => Mathf.Abs(ball.CurrentVelocity.x) <= 0.01f), Is.True);
        Assert.That(activeBalls.Exists(ball => ball.CurrentVelocity.x > 0.1f), Is.True);
        Assert.That(activeBalls[1].CurrentVelocity.magnitude, Is.GreaterThan(8f));
    }

    [Test]
    public void HotShrapnelUpgradeExtendsExplosiveBrickBlastRadius()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var sourceBrick = CreateBrickHarness(controller, "Explosive Brick", 250, Vector2.zero);
        var nearBrick = CreateBrickHarness(controller, "Near", 100, new Vector2(1.7f, 0f));
        var farBrick = CreateBrickHarness(controller, "Far", 100, new Vector2(3.5f, 0f));
        var brickDefinition = sourceBrick.Definition;
        var activeRunState = new BreakoutRunState();
        var hotShrapnel = CreateRunUpgrade("hot-shrapnel", specialBrickEffectMultiplier: 1.25f);
        var activeBalls = GetPrivateField<List<BallController>>(controller, "activeBalls");
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");

        SetPrivateField(brickDefinition, "explosive", true);
        SetPrivateField(brickDefinition, "explosionRadius", 1.5f);
        activeRunState.SetPendingDraftOffers(new[] { BreakoutRunDraftOffer.FromRunUpgrade(hotShrapnel) });
        Assert.That(activeRunState.TryApplyPendingDraftOffer(0, out _), Is.True);
        SetPrivateField(controller, "activeRunState", activeRunState);
        activeBalls.Add(scoringBall);
        bricks.Add(sourceBrick);
        bricks.Add(nearBrick);
        bricks.Add(farBrick);

        controller.HandleBrickDestroyed(sourceBrick, scoringBall, BrickDestructionCause.Impact);

        Assert.That(bricks, Is.EqualTo(new[] { farBrick }));
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

        var ballBody = serveBall.GetComponent<Rigidbody2D>();
        serveBall.SetWorldPosition(new Vector2(0f, -4.35f));
        ballBody.linearVelocity = Vector2.down * 8f;

        Assert.That(GetPrivateField<int>(controller, "shieldWallCharges"), Is.EqualTo(1));
        Assert.That(controller.TryRescueBallWithShield(serveBall), Is.True);
        Assert.That(GetPrivateField<int>(controller, "shieldWallCharges"), Is.EqualTo(0));
        Assert.That(ballBody.linearVelocity.y, Is.GreaterThan(0f));
        Assert.That(ballBody.position.y, Is.GreaterThan(-4.5f));
    }

    [Test]
    public void ShieldWallDoesNotConsumeChargeBeforeBallReachesWall()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateEnumField(controller, "roundState", "Playing");

        InvokePrivateMethod(
            controller,
            "ApplyPowerUp",
            CreatePowerUp("Shield Wall", PowerUpEffectType.ShieldWall, true, 0f, 1f));

        serveBall.SetWorldPosition(new Vector2(0f, -3f));
        serveBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * 8f;

        Assert.That(controller.TryRescueBallWithShield(serveBall), Is.False);
        Assert.That(GetPrivateField<int>(controller, "shieldWallCharges"), Is.EqualTo(1));
    }

    [Test]
    public void TiltWarningRescuesNearMissAndConsumesLevelCharge()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateEnumField(controller, "roundState", "Playing");
        SetPrivateField(controller, "tiltWarningSavesRemaining", 1);
        serveBall.SetWorldPosition(new Vector2(paddle.transform.position.x, -6.1f));
        serveBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * 8f;

        Assert.That(controller.TryRescueBallWithTiltWarning(serveBall), Is.True);
        Assert.That(GetPrivateField<int>(controller, "tiltWarningSavesRemaining"), Is.Zero);
        Assert.That(serveBall.CurrentVelocity.y, Is.GreaterThan(0f));
    }

    [Test]
    public void TiltWarningIgnoresWideMiss()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateEnumField(controller, "roundState", "Playing");
        SetPrivateField(controller, "tiltWarningSavesRemaining", 1);
        serveBall.SetWorldPosition(new Vector2(paddle.transform.position.x + 4f, -6.1f));
        serveBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * 8f;

        Assert.That(controller.TryRescueBallWithTiltWarning(serveBall), Is.False);
        Assert.That(GetPrivateField<int>(controller, "tiltWarningSavesRemaining"), Is.EqualTo(1));
        Assert.That(serveBall.CurrentVelocity.y, Is.LessThan(0f));
    }

    [Test]
    public void CabinetNudgeBendsLaunchedBallAndPreservesSpeed()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        serveBall.Launch(Vector2.up);
        var speedBeforeNudge = serveBall.CurrentSpeed;

        Assert.That(serveBall.ApplyCabinetNudge(0.24f), Is.True);

        Assert.That(serveBall.CurrentVelocity.x, Is.GreaterThan(0f));
        Assert.That(serveBall.CurrentSpeed, Is.EqualTo(speedBeforeNudge).Within(0.0001f));
    }

    [Test]
    public void TiltAlarmLocksPaddleUntilTimerClears()
    {
        var controller = CreateControllerHarness(out var paddle);
        var state = GetPrivateField<BreakoutTiltAlarmState>(controller, "tiltAlarmState");
        state.RegisterNudge();
        state.RegisterNudge();
        state.RegisterNudge();

        InvokePrivateMethod(controller, "ApplyActiveEffects");

        Assert.That(GetPrivateField<float>(paddle, "moveSpeed"), Is.Zero);

        state.Update(3f);
        InvokePrivateMethod(controller, "ApplyActiveEffects");

        Assert.That(GetPrivateField<float>(paddle, "moveSpeed"), Is.EqualTo(12f).Within(0.0001f));
    }

    [Test]
    public void RewindCatchReturnsMissedBallToLastPaddleHitAndConsumesCharge()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        var hitX = paddle.transform.position.x + (paddle.HalfWidthWorld * 0.45f);
        var hitPosition = new Vector2(hitX, paddle.transform.position.y + 0.7f);
        SetPrivateEnumField(controller, "roundState", "Playing");
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        serveBall.SetWorldPosition(hitPosition);
        serveBall.RecordPaddleHitRewindAnchor(paddle, hitX);
        SetPrivateField(serveBall, "hasLaunched", true);
        serveBall.SetWorldPosition(new Vector2(hitX, -6.25f));
        serveBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * 8f;

        InvokePrivateMethod(
            controller,
            "ApplyPowerUp",
            CreatePowerUp("Rewind Catch", PowerUpEffectType.RewindCatch, true, 0f, 1f));

        Assert.That(controller.TryRescueBallWithRewindCatch(serveBall), Is.True);

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        Assert.That(GetPropertyValue<int>(powerUpService, "RewindCatchCharges"), Is.Zero);
        Assert.That(serveBall.transform.position.x, Is.EqualTo(hitPosition.x).Within(0.0001f));
        Assert.That(serveBall.transform.position.y, Is.EqualTo(hitPosition.y).Within(0.0001f));
        Assert.That(serveBall.CurrentVelocity.y, Is.GreaterThan(0f));
        Assert.That(serveBall.HasLaunched, Is.True);
        Assert.That(GetPrivateField<List<BallController>>(controller, "activeBalls"), Does.Contain(serveBall));
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
    public void LastBallAutoSaveOnEarlyRogueHeatSpendsPointsAndKeepsRunAlive()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(
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
                rogueIntensity: 10));
        SetPrivateField(controller, "serveBall", serveBall);
        SetPrivateField(controller, "livesRemaining", 1);
        SetPrivateField(controller, "score", 10000);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandleBallLost(serveBall);

        Assert.That(GetPrivateField<int>(controller, "livesRemaining"), Is.EqualTo(1));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(0));
        Assert.That(GetPrivateField<bool>(controller, "lastLifeLossUsedAutoSave"), Is.True);
        Assert.That(GetPrivateField<float>(controller, "autoSaveBurstTimer"), Is.GreaterThan(0f));
        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("LifeLost"));
        Assert.That(GetPrivateField<List<BallController>>(controller, "activeBalls").Count, Is.EqualTo(1));
    }

    [Test]
    public void LosingFiniteLifeHidesServeBallUntilRevealCompletes()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null));
        SetPrivateField(controller, "serveBall", serveBall);
        SetPrivateField(controller, "livesRemaining", 2);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandleBallLost(serveBall);

        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("LifeLost"));
        Assert.That(serveBall.gameObject.activeSelf, Is.False);
        Assert.That(GetPrivateField<float>(controller, "serveBallRevealDelayTimer"), Is.GreaterThan(0f));

        InvokePrivateMethod(controller, "LaunchServe");

        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("LifeLost"));
        Assert.That(serveBall.HasLaunched, Is.False);

        InvokePrivateMethod(controller, "RevealServeBallForServe");
        InvokePrivateMethod(controller, "LaunchServe");

        Assert.That(serveBall.gameObject.activeSelf, Is.True);
        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("Playing"));
        Assert.That(serveBall.HasLaunched, Is.True);
    }

    [Test]
    public void LastBallAutoSaveOnHeatFortySpendsScaledPointsAndKeepsRunAlive()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(
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
                rogueIntensity: 40));
        SetPrivateField(controller, "serveBall", serveBall);
        SetPrivateField(controller, "livesRemaining", 1);
        SetPrivateField(controller, "score", 40000);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandleBallLost(serveBall);

        Assert.That(GetPrivateField<int>(controller, "livesRemaining"), Is.EqualTo(1));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(0));
        Assert.That(GetPrivateField<bool>(controller, "lastLifeLossUsedAutoSave"), Is.True);
        Assert.That(GetPrivateField<float>(controller, "autoSaveBurstTimer"), Is.GreaterThan(0f));
        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("LifeLost"));
        Assert.That(GetPrivateField<List<BallController>>(controller, "activeBalls").Count, Is.EqualTo(1));
    }

    [Test]
    public void LastBallAutoSaveRequiresEnoughPoints()
    {
        var controller = CreateControllerHarness(out var paddle);
        var serveBall = CreateBallHarness(controller, paddle);
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(
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
                rogueIntensity: 10));
        SetPrivateField(controller, "isDeveloperRunActive", true);
        SetPrivateField(controller, "serveBall", serveBall);
        SetPrivateField(controller, "livesRemaining", 1);
        SetPrivateField(controller, "score", 9999);
        GetPrivateField<List<BallController>>(controller, "activeBalls").Add(serveBall);
        SetPrivateEnumField(controller, "roundState", "Playing");

        controller.HandleBallLost(serveBall);

        Assert.That(GetPrivateField<int>(controller, "livesRemaining"), Is.EqualTo(0));
        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(9999));
        Assert.That(GetPrivateField<bool>(controller, "lastLifeLossUsedAutoSave"), Is.False);
        Assert.That(GetPrivateField<object>(controller, "roundState").ToString(), Is.EqualTo("GameOver"));
        Assert.That(GetPrivateField<List<BallController>>(controller, "activeBalls").Count, Is.EqualTo(0));
    }

    [Test]
    public void AutoSaveEligibilityExtendsThroughHeatFortyAndScalesCost()
    {
        var earlyHeat = new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null, RunGameMode.Rogue, rogueIntensity: 10);
        var extensionHeat = new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null, RunGameMode.Rogue, rogueIntensity: 11);
        var finalAutoSaveHeat = new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null, RunGameMode.Rogue, rogueIntensity: 40);
        var tooLateHeat = new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null, RunGameMode.Rogue, rogueIntensity: 41);
        var customGame = new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null);

        Assert.That(BreakoutGameController.GetAutoSaveScoreCost(earlyHeat), Is.EqualTo(10000));
        Assert.That(BreakoutGameController.GetAutoSaveScoreCost(extensionHeat), Is.EqualTo(11000));
        Assert.That(BreakoutGameController.GetAutoSaveScoreCost(finalAutoSaveHeat), Is.EqualTo(40000));
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(earlyHeat, 10000), Is.True);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(earlyHeat, 9999), Is.False);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(extensionHeat, 10999), Is.False);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(extensionHeat, 11000), Is.True);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(finalAutoSaveHeat, 39999), Is.False);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(finalAutoSaveHeat, 40000), Is.True);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(tooLateHeat, 41000), Is.False);
        Assert.That(BreakoutGameController.ShouldAutoSaveLastBall(customGame, 10000), Is.False);
    }

    [Test]
    public void CompletedRogueRunBuildsEndStateStatsTable()
    {
        var controller = CreateControllerHarness(out _);
        var finalLevel = CreateLevelDefinition("Final Breakthru");
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(
                2468,
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
                rogueIntensity: 12));
        SetPrivateField(controller, "currentLevel", finalLevel);
        SetPrivateField(controller, "currentLevelIndex", BreakoutRunProgression.TargetLevelCount - 1);
        SetPrivateField(controller, "score", 12345);
        SetPrivateField(controller, "livesRemaining", 2);
        GetPrivateField<List<LevelDefinition>>(controller, "loadedLevels").Add(finalLevel);
        SetPrivateEnumField(controller, "roundState", "LevelComplete");

        var overlay = (BreakoutUiOverlayView)InvokePrivateMethodWithResult(controller, "BuildEndStateOverlayView");

        Assert.That(overlay.Title, Is.EqualTo("Ladder Cleared"));
        Assert.That(overlay.SummaryLines, Is.Empty);
        Assert.That(overlay.StatsRows, Is.Not.Empty);
        Assert.That(overlay.StatsRows, Has.Some.Matches<BreakoutUiStatsRowView>(row => row.Label == "Tape ID" && row.Value == "2468"));
        Assert.That(overlay.StatsRows, Has.Some.Matches<BreakoutUiStatsRowView>(row => row.Label == "Levels Cleared"));
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
    public void NeonFloodSpawnsHelpfulAndHarmfulCapsulesAfterSlamChainSpike()
    {
        var controller = CreateControllerHarness(out var paddle);
        var scoringBall = CreateBallHarness(controller, paddle);
        var bricks = GetPrivateField<List<Brick>>(controller, "bricks");
        var balls = GetPrivateField<List<BallController>>(controller, "activeBalls");
        var loadedPowerUps = GetPrivateField<List<PowerUpDefinition>>(controller, "loadedPowerUpDefinitions");
        var pickupsRoot = new GameObject("Neon Flood Pickups").transform;
        runtimeObjects.Add(pickupsRoot.gameObject);
        var helpfulDrop = CreatePowerUp("Wide Paddle", PowerUpEffectType.PaddleWidthMultiplier, true, 10f, 1.2f);
        var harmfulDrop = CreatePowerUp("Narrow Paddle", PowerUpEffectType.PaddleWidthMultiplier, false, 10f, 0.7f);
        var plan = new BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType.NeonFlood,
            BreakoutContentRarity.Epic,
            "Neon Flood",
            "Neon Flood 4+ x1.41",
            1.41f,
            Array.Empty<BreakoutWarpGateSpec>(),
            default,
            neonFlood: new BreakoutNeonFloodSpec(4, 1.65f, 0.42f, 0.88f, 1.16f));

        balls.Add(scoringBall);
        loadedPowerUps.Add(helpfulDrop);
        loadedPowerUps.Add(harmfulDrop);
        SetPrivateField(controller, "pickupsRoot", pickupsRoot);
        SetPrivateField(controller, "gameplayRandom", new DeterministicRandomService(4));
        SetPrivateField(controller, "activeLevelGlitchPlan", plan);

        for (var index = 0; index < 4; index++)
        {
            var brick = CreateBrickHarness(controller, $"Flood Brick {index}", 100, new Vector2(index, 1f));
            bricks.Add(brick);
            controller.HandleBrickDestroyed(brick, scoringBall, BrickDestructionCause.Impact);
        }

        var powerUpService = GetPrivateField<object>(controller, "powerUpService");
        var activePickups = GetPropertyValue<List<PowerUpPickup>>(powerUpService, "ActivePickups");

        Assert.That(activePickups.Count, Is.EqualTo(2));
        Assert.That(activePickups.Exists(pickup => pickup.Definition == helpfulDrop), Is.True);
        Assert.That(activePickups.Exists(pickup => pickup.Definition == harmfulDrop), Is.True);
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
    public void BankBonusWallChargePaysOutOnNextBrickHit()
    {
        var controller = CreateControllerHarness(out _);
        var brick = CreateBrickHarness(controller, "Bank Brick", 100, new Vector2(0f, 2f));
        var bankBonus = CreatePowerUp("Bank Bonus", PowerUpEffectType.BankBonus, true, 12f, 50f);

        InvokePrivateMethod(controller, "ApplyPowerUp", bankBonus);
        controller.HandleBallHitWall();
        controller.HandleBallHitWall();
        controller.HandleBrickHit(brick);

        var popups = GetFloatingScorePopups(controller);
        var powerUpService = GetPrivateField<object>(controller, "powerUpService");

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(100));
        Assert.That(GetPropertyValue<int>(powerUpService, "BankBonusChargePoints"), Is.Zero);
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+100"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Is.EqualTo("COMBO BONUS: BANK BONUS!"));
    }

    [Test]
    public void BogusBounceConsumesChargeAndRedirectsBall()
    {
        var controller = CreateControllerHarness(out var paddle);
        var ball = CreateBallHarness(controller, paddle);
        var bogusBounce = CreatePowerUp("Bogus Bounce", PowerUpEffectType.BogusBounce, false, 0f, 52f);

        ball.Launch(Vector2.up);
        var originalDirection = ball.CurrentVelocity.normalized;

        InvokePrivateMethod(controller, "ApplyPowerUp", bogusBounce);
        var applied = controller.TryApplyBogusBounce(ball);
        var powerUpService = GetPrivateField<object>(controller, "powerUpService");

        Assert.That(applied, Is.True);
        Assert.That(Vector2.Angle(originalDirection, ball.CurrentVelocity.normalized), Is.GreaterThan(10f));
        Assert.That(GetPropertyValue<int>(powerUpService, "BogusBounceCharges"), Is.EqualTo(2));
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

    [Test]
    public void ScoreLeakResetsGraceWhenBrickBreaks()
    {
        var controller = CreateControllerHarness(out _);
        var scoreLeak = new BreakoutScoreLeakSpec(12f, 1.5f);
        var plan = new BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType.ScoreLeak,
            BreakoutContentRarity.Epic,
            "Score Leak",
            "Score Leak -12/s x1.42",
            1.42f,
            Array.Empty<BreakoutWarpGateSpec>(),
            default,
            scoreLeak: scoreLeak);

        SetPrivateField(controller, "activeLevelGlitchPlan", plan);
        SetPrivateField(controller, "scoreLeakGraceTimer", 0f);
        SetPrivateField(controller, "scoreLeakAccumulator", 2f);

        InvokePrivateMethod(controller, "ResetScoreLeakOnBrickBreak");

        Assert.That(GetPrivateField<float>(controller, "scoreLeakGraceTimer"), Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(GetPrivateField<float>(controller, "scoreLeakAccumulator"), Is.Zero);
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
        var ballsRoot = new GameObject("Balls").transform;
        runtimeObjects.Add(ballsRoot.gameObject);

        SetPrivateField(controller, "paddle", paddle);
        SetPrivateField(controller, "paddleCollider", paddleCollider);
        SetPrivateField(controller, "effectsRoot", effectsRoot);
        SetPrivateField(controller, "ballsRoot", ballsRoot);
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
        InvokePrivateMethod(controller, "CreateActorSpawnServices");
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

    private SpriteRenderer AttachBrickRenderer(Brick brick)
    {
        var visualObject = new GameObject($"{brick.name} Visual");
        runtimeObjects.Add(visualObject);
        visualObject.transform.SetParent(brick.transform, false);
        var spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
        SetPrivateField(brick, "spriteRenderer", spriteRenderer);
        SetPrivateField(brick, "themedBaseColor", Color.white);
        SetPrivateField(brick, "themedDamagedColor", Color.white);
        return spriteRenderer;
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
        float scalar,
        float secondaryScalar = 1f)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "effectType", effectType);
        SetPrivateField(powerUp, "beneficial", beneficial);
        SetPrivateField(powerUp, "durationSeconds", durationSeconds);
        SetPrivateField(powerUp, "scalar", scalar);
        SetPrivateField(powerUp, "secondaryScalar", secondaryScalar);
        SetPrivateField(powerUp, "extraBallCount", 0);
        return powerUp;
    }

    private RunUpgradeDefinition CreateRunUpgrade(
        string upgradeId,
        float specialBrickEffectMultiplier,
        int tiltWarningSavesPerLevel = 0)
    {
        var upgrade = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        runtimeObjects.Add(upgrade);
        SetPrivateField(upgrade, "upgradeId", upgradeId);
        SetPrivateField(upgrade, "displayName", upgradeId);
        SetPrivateField(upgrade, "draftWeight", 1f);
        SetPrivateField(upgrade, "maxStacks", 1);
        SetPrivateField(upgrade, "specialBrickEffectMultiplier", specialBrickEffectMultiplier);
        SetPrivateField(upgrade, "tiltWarningSavesPerLevel", tiltWarningSavesPerLevel);
        return upgrade;
    }

    private LevelDefinition CreateLevelDefinition(string displayName)
    {
        var level = ScriptableObject.CreateInstance<LevelDefinition>();
        runtimeObjects.Add(level);
        SetPrivateField(level, "displayName", displayName);
        return level;
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

    private static Color ResolveSpriteTint(SpriteRenderer spriteRenderer)
    {
        var propertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(propertyBlock);
        var color = propertyBlock.GetColor(Shader.PropertyToID("_Color"));
        return color.a > 0.0001f ? color : spriteRenderer.color;
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
