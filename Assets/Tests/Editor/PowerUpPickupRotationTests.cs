using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class PowerUpPickupRotationTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private GameObject pickupObject;

    [TearDown]
    public void TearDown()
    {
        if (pickupObject != null)
        {
            Object.DestroyImmediate(pickupObject);
        }
    }

    [Test]
    public void FallingPickupAdvancesRotationDuringFixedUpdate()
    {
        pickupObject = new GameObject("Pickup");
        pickupObject.AddComponent<SpriteRenderer>();
        pickupObject.AddComponent<BoxCollider2D>();
        pickupObject.AddComponent<Rigidbody2D>();

        var pickup = pickupObject.AddComponent<PowerUpPickup>();
        pickup.Configure(
            null,
            CreatePowerUpDefinition(),
            speed: 3f,
            missY: -10f,
            startingRotationDegrees: 45f,
            spinDegreesPerSecond: 180f,
            new ThemeVisualStyle(Color.white, Color.white, null));

        var initialRotation = pickup.transform.eulerAngles.z;
        InvokePrivateMethod(pickup, "FixedUpdate");

        Assert.That(pickup.transform.eulerAngles.z, Is.Not.EqualTo(initialRotation).Within(0.0001f));
    }

    [Test]
    public void DelayedPickupHoldsPositionUntilWaveRelease()
    {
        pickupObject = new GameObject("Pickup");
        pickupObject.AddComponent<SpriteRenderer>();
        pickupObject.AddComponent<BoxCollider2D>();
        pickupObject.AddComponent<Rigidbody2D>();

        var pickup = pickupObject.AddComponent<PowerUpPickup>();
        pickup.Configure(
            null,
            CreatePowerUpDefinition(),
            speed: 3f,
            missY: -10f,
            startingRotationDegrees: 45f,
            spinDegreesPerSecond: 0f,
            new ThemeVisualStyle(Color.white, Color.white, null));
        pickup.DelayFall(Time.fixedDeltaTime * 2f);

        var initialY = pickup.transform.position.y;
        InvokePrivateMethod(pickup, "FixedUpdate");

        Assert.That(pickup.transform.position.y, Is.EqualTo(initialY).Within(0.0001f));
        Assert.That(pickup.FallDelaySeconds, Is.GreaterThan(0f));
    }

    [Test]
    public void ApplyingThemeNormalizesPickupSpriteToTargetWorldSize()
    {
        pickupObject = new GameObject("Pickup");
        pickupObject.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
        pickupObject.AddComponent<BoxCollider2D>();
        pickupObject.AddComponent<Rigidbody2D>();

        var pickup = pickupObject.AddComponent<PowerUpPickup>();
        pickup.Configure(
            null,
            CreatePowerUpDefinition(),
            speed: 3f,
            missY: -10f,
            startingRotationDegrees: 45f,
            spinDegreesPerSecond: 180f,
            new ThemeVisualStyle(Color.white, Color.white, CreateTestSprite(400, 200)));

        Assert.That(spriteRenderer.bounds.size.x, Is.EqualTo(0.55f).Within(0.02f));
        Assert.That(spriteRenderer.bounds.size.y, Is.EqualTo(0.55f).Within(0.02f));
    }

    private static PowerUpDefinition CreatePowerUpDefinition()
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        SetPrivateField(powerUp, "displayName", "Spin Test");
        SetPrivateField(powerUp, "hudLabel", "SPIN");
        SetPrivateField(powerUp, "effectType", PowerUpEffectType.PaddleWidthMultiplier);
        SetPrivateField(powerUp, "beneficial", true);
        SetPrivateField(powerUp, "durationSeconds", 10f);
        SetPrivateField(powerUp, "scalar", 1.2f);
        return powerUp;
    }

    private static Sprite CreateTestSprite(int width, int height)
    {
        var texture = new Texture2D(width, height);
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static void InvokePrivateMethod(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, InstanceFlags);
        Assert.That(method, Is.Not.Null, $"Missing method '{methodName}' on {instance.GetType().Name}.");
        method.Invoke(instance, args);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
