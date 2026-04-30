using System;
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
            new RunSettings(1234, RunDifficultyPreset.Standard, 3, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, null));
        SetPrivateField(controller, "currentLevelPaddleSpeed", 12f);
        SetPrivateField(controller, "currentLevelBallSpeed", 8f);
        return controller;
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

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        return (T)field.GetValue(instance);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
