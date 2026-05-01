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
    public void ApplyingSameWidePowerUpTwiceStacksPaddleScaleWithoutClamp()
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
        Assert.That(GetPropertyValue<float>(activeTimedEffects[0], "RemainingDuration"), Is.EqualTo(20f).Within(0.0001f));
        Assert.That(activeEffectsLabel, Does.Contain("x2"));
        Assert.That(activeEffectsLabel, Does.Contain("20.0s"));
        Assert.That(GetFieldValue<string>(pickupBannerView, "Text"), Does.Contain("x2"));
        Assert.That(modifierViews.Length, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(modifierViews.GetValue(0), "Label"), Does.Contain("x2"));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "RemainingDuration"), Is.EqualTo(20f).Within(0.0001f));
        Assert.That(GetFieldValue<float>(modifierViews.GetValue(0), "DurationRatio"), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void ApplyingSameTimedPowerUpTwiceExtendsTimerAcrossTimedDropTypes()
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
                Is.EqualTo(powerUpCase.DurationSeconds * 2f).Within(0.0001f),
                $"Expected doubled duration for {powerUpCase.DisplayName}.");

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

        var activeEffectModifiers = GetPrivateField<object>(controller, "activeEffectModifiers");

        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "StickyPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "LaserPaddleEnabled"), Is.True);
        Assert.That(GetPropertyValue<bool>(activeEffectModifiers, "PhaseBallEnabled"), Is.True);
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "ChainLightningStrength"), Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(GetPropertyValue<float>(activeEffectModifiers, "FogVisibilityMultiplier"), Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(paddle, "controlsReversed"), Is.True);
        Assert.That(GetPrivateField<float>(paddle, "splitGapWidthNormalized"), Is.GreaterThan(0.2f));
        Assert.That(GetPrivateField<float>(paddle, "lagSpikeStrength"), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(GetPrivateField<bool>(serveBall, "phaseThroughBricks"), Is.True);
        Assert.That(GetPrivateField<float>(serveBall, "gravityWellStrength"), Is.EqualTo(0.35f).Within(0.0001f));
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

        var popups = GetPrivateField<System.Collections.IList>(controller, "floatingScorePopups");

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(220));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+20"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Does.Contain("SLAM CHAIN"));
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

        var popups = GetPrivateField<System.Collections.IList>(controller, "floatingScorePopups");

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(142));
        Assert.That(GetPrivateField<int>(scoringBall, "ricochetCountSinceLastBrick"), Is.EqualTo(0));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Does.Contain("BANK SHOT"));
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

        var popups = GetPrivateField<System.Collections.IList>(controller, "floatingScorePopups");

        Assert.That(GetPrivateField<int>(controller, "score"), Is.EqualTo(245));
        Assert.That(popups.Count, Is.EqualTo(1));
        Assert.That(GetFieldValue<string>(popups[0], "PrimaryText"), Is.EqualTo("+45"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Does.Contain("SLAM CHAIN"));
        Assert.That(GetFieldValue<string>(popups[0], "SecondaryText"), Does.Contain("PARTY SPLIT"));
    }

    private BreakoutGameController CreateControllerHarness(out PaddleController paddle)
    {
        controllerObject = new GameObject("BreakoutGameController Test");
        var controller = controllerObject.AddComponent<BreakoutGameController>();

        paddleObject = new GameObject("Paddle");
        paddleObject.transform.localScale = new Vector3(2.1f, 0.74f, 1f);
        paddleObject.AddComponent<BoxCollider2D>();
        paddleObject.AddComponent<Rigidbody2D>();
        paddle = paddleObject.AddComponent<PaddleController>();
        paddle.Configure(controller, 12f, -10f, 10f, -3.5f);

        SetPrivateField(controller, "paddle", paddle);
        SetPrivateField(controller, "powerUpService", CreatePowerUpService());
        SetPrivateField(
            controller,
            "activeRunSettings",
            new RunSettings(1234, RunDifficultyPreset.Standard, RunScoringMode.Classic, 3, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, false, null));
        SetPrivateField(controller, "currentLevelPaddleSpeed", 12f);
        SetPrivateField(controller, "currentLevelBallSpeed", 8f);
        SetPrivateField(controller, "arenaTop", 5f);
        SetPrivateField(controller, "arenaBottom", -5f);
        return controller;
    }

    private BallController CreateBallHarness(BreakoutGameController controller, PaddleController paddle)
    {
        var ballObject = new GameObject("Ball");
        runtimeObjects.Add(ballObject);
        ballObject.AddComponent<CircleCollider2D>();
        ballObject.AddComponent<Rigidbody2D>();
        var ball = ballObject.AddComponent<BallController>();
        ball.Configure(controller, paddle, 8f, 0.35f, -6f, 0.5f, false);
        return ball;
    }

    private Brick CreateBrickHarness(BreakoutGameController controller, string displayName, int scoreValue, Vector2 worldPosition)
    {
        var brickObject = new GameObject(displayName);
        runtimeObjects.Add(brickObject);
        brickObject.transform.position = worldPosition;
        brickObject.AddComponent<BoxCollider2D>();
        var brick = brickObject.AddComponent<Brick>();
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(definition);
        SetPrivateField(definition, "displayName", displayName);
        SetPrivateField(definition, "scoreValue", scoreValue);
        SetPrivateField(definition, "indestructible", false);
        SetPrivateField(definition, "countsTowardLevelCompletion", true);
        SetPrivateField(definition, "dropChance", 0f);
        SetPrivateField(definition, "dropTable", Array.Empty<BrickPowerUpDropEntry>());
        SetPrivateField(brick, "gameController", controller);
        SetPrivateField(brick, "definition", definition);
        return brick;
    }

    private static object CreatePowerUpService()
    {
        var serviceType = GetGameplayType("GetBricked.Gameplay.BreakoutPowerUpService");
        return Activator.CreateInstance(serviceType, new Vector2(0.55f, 0.55f), 3.2f, 22f, null);
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
