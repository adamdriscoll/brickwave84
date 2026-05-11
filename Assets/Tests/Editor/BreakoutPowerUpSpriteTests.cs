using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutPowerUpSpriteTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [Test]
    public void PowerUpDefinitionBuildsSpriteResourcePathFromStableId()
    {
        var powerUp = CreatePowerUp("Sticky Paddle");
        SetPrivateField(powerUp, "powerUpId", "sticky_paddle");

        Assert.That(powerUp.ResolvePickupSpriteResourcePath(), Is.EqualTo("Sprites/sticky-paddle"));
    }

    [Test]
    public void ThemeServiceUsesDefinitionSpecificPickupSpriteWhenConfigured()
    {
        var fallbackTexture = new Texture2D(8, 8);
        var customTexture = new Texture2D(8, 8);
        var fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        var customSprite = Sprite.Create(customTexture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);

        try
        {
            var powerUp = CreatePowerUp("Sticky Paddle");
            SetPrivateField(powerUp, "powerUpId", "sticky_paddle");

            var serviceType = GetGameplayType("GetBricked.Gameplay.BreakoutThemeService");
            var themeService = Activator.CreateInstance(
                serviceType,
                Color.black,
                Color.gray,
                Color.cyan,
                Color.yellow,
                null,
                fallbackSprite,
                fallbackSprite,
                fallbackSprite,
                fallbackSprite,
                fallbackSprite,
                new Dictionary<string, Sprite>
                {
                    ["Sprites/sticky-paddle"] = customSprite,
                });

            var style = (ThemeVisualStyle)serviceType
                .GetMethod("ResolvePowerUpStyle", InstanceFlags)
                .Invoke(themeService, new object[] { powerUp });

            Assert.That(style.Sprite, Is.SameAs(customSprite));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(fallbackSprite);
            UnityEngine.Object.DestroyImmediate(customSprite);
            UnityEngine.Object.DestroyImmediate(fallbackTexture);
            UnityEngine.Object.DestroyImmediate(customTexture);
        }
    }

    [Test]
    public void PowerUpIconResolverUsesPickupSpriteResourceWhenThemeServiceIsUnavailable()
    {
        var powerUp = Resources.Load<PowerUpDefinition>("PowerUps/FastBall");
        var expectedSprite = Resources.Load<Sprite>("Sprites/fast-ball");
        Assert.That(powerUp, Is.Not.Null);
        Assert.That(expectedSprite, Is.Not.Null);

        var controllerObject = new GameObject("Controller");
        controllerObject.SetActive(false);
        var fallbackTexture = new Texture2D(8, 8);
        var fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);

        try
        {
            var controller = controllerObject.AddComponent<BreakoutGameController>();
            SetPrivateField(controller, "powerUpSprite", fallbackSprite);
            SetPrivateField(controller, "squareSprite", fallbackSprite);
            SetPrivateField(controller, "themeService", null);

            var resolvedSprite = (Sprite)typeof(BreakoutGameController)
                .GetMethod("ResolvePowerUpIcon", InstanceFlags)
                .Invoke(controller, new object[] { powerUp });

            Assert.That(resolvedSprite, Is.SameAs(expectedSprite));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(fallbackSprite);
            UnityEngine.Object.DestroyImmediate(fallbackTexture);
        }
    }

    [Test]
    public void UiIconResolverRasterizesPickupSpriteForImgui()
    {
        var sprite = Resources.Load<Sprite>("Sprites/fast-ball");
        Assert.That(sprite, Is.Not.Null);

        var rendererType = GetGameplayType("GetBricked.Gameplay.BreakoutUiRenderer");
        var renderer = Activator.CreateInstance(rendererType, nonPublic: true);
        var arguments = new object[] { sprite, default(Rect) };

        var texture = (Texture)rendererType
            .GetMethod("ResolveIconTexture", InstanceFlags)
            .Invoke(renderer, arguments);

        Assert.That(texture, Is.Not.Null);
        Assert.That(texture, Is.Not.SameAs(sprite.texture));
    }

    private static PowerUpDefinition CreatePowerUp(string displayName)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        SetPrivateField(powerUp, "effectType", PowerUpEffectType.WavyPaddle);
        SetPrivateField(powerUp, "beneficial", true);
        SetPrivateField(powerUp, "durationSeconds", 15f);
        SetPrivateField(powerUp, "scalar", 1f);
        return powerUp;
    }

    private static Type GetGameplayType(string fullName)
    {
        var assembly = typeof(BreakoutGameController).Assembly;
        var resolvedType = assembly.GetType(fullName, throwOnError: false);
        Assert.That(resolvedType, Is.Not.Null, $"Could not resolve gameplay type '{fullName}'.");
        return resolvedType;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
