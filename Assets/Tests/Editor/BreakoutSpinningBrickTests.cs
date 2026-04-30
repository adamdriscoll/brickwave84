using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutSpinningBrickTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private GameObject brickObject;

    [TearDown]
    public void TearDown()
    {
        if (brickObject != null)
        {
            Object.DestroyImmediate(brickObject);
        }
    }

    [Test]
    public void SpinningBrickInitializationCreatesAnchoredBody()
    {
        var brick = CreateBrick(spinsOnHit: true);

        var body = brick.GetComponent<Rigidbody2D>();
        var hinge = brick.GetComponent<HingeJoint2D>();

        Assert.That(body, Is.Not.Null);
        Assert.That(body.freezeRotation, Is.False);
        Assert.That(hinge, Is.Not.Null);
        Assert.That(hinge.connectedBody, Is.Null);
        Assert.That(hinge.connectedAnchor, Is.EqualTo((Vector2)brick.transform.position));
    }

    [Test]
    public void SpinningBrickImpactAddsAngularVelocityAndBiasesBounce()
    {
        var brick = CreateBrick(spinsOnHit: true);
        var body = brick.GetComponent<Rigidbody2D>();
        var impactPoint = new Vector2(0.52f, 0.2f);
        var incomingDirection = new Vector2(0.18f, -1f).normalized;

        InvokePrivateMethod(brick, "RegisterImpactSpin", impactPoint, incomingDirection, 8f);

        Assert.That(Mathf.Abs(body.angularVelocity), Is.GreaterThan(0.01f));

        var bounceDirection = InvokeSpinBounce(brick, impactPoint, incomingDirection, 8f);
        var surfaceNormal = (impactPoint - (Vector2)brick.transform.position).normalized;

        if (Vector2.Dot(surfaceNormal, incomingDirection) > -0.05f)
        {
            surfaceNormal = -surfaceNormal;
        }

        var reflectedDirection = Vector2.Reflect(incomingDirection, surfaceNormal).normalized;

        Assert.That(Vector2.Distance(bounceDirection, reflectedDirection), Is.GreaterThan(0.05f));
    }

    [Test]
    public void NonSpinningBrickDoesNotCreateSpinBody()
    {
        var brick = CreateBrick(spinsOnHit: false);

        Assert.That(brick.GetComponent<Rigidbody2D>(), Is.Null);
        Assert.That(brick.GetComponent<HingeJoint2D>(), Is.Null);
    }

    private Brick CreateBrick(bool spinsOnHit)
    {
        brickObject = new GameObject(spinsOnHit ? "Spinning Brick" : "Basic Brick");
        brickObject.transform.position = new Vector3(0f, 2f, 0f);
        brickObject.transform.localScale = new Vector3(1.15f, 0.58f, 1f);
        brickObject.AddComponent<BoxCollider2D>();

        var visualObject = new GameObject("Visual");
        visualObject.transform.SetParent(brickObject.transform, false);
        visualObject.AddComponent<SpriteRenderer>();

        var brick = brickObject.AddComponent<Brick>();
        brick.Initialize(
            null,
            CreateDefinition(spinsOnHit),
            effectiveHitPoints: spinsOnHit ? 2 : 1,
            new ThemeVisualStyle(Color.white, Color.gray, null),
            motionSpeed: 0f,
            motionDirection: Vector2.zero);
        return brick;
    }

    private static BrickDefinition CreateDefinition(bool spinsOnHit)
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        SetPrivateField(definition, "displayName", spinsOnHit ? "Rotor Brick" : "Basic Brick");
        SetPrivateField(definition, "hitPoints", spinsOnHit ? 2 : 1);
        SetPrivateField(definition, "spinsOnHit", spinsOnHit);
        SetPrivateField(definition, "spinTorqueImpulse", 160f);
        SetPrivateField(definition, "spinMaxAngularVelocity", 420f);
        SetPrivateField(definition, "spinAngularDamping", 2.2f);
        SetPrivateField(definition, "spinBounceStrength", 0.9f);
        return definition;
    }

    private static Vector2 InvokeSpinBounce(Brick brick, Vector2 impactPoint, Vector2 incomingDirection, float impactSpeed)
    {
        var method = typeof(Brick).GetMethod("TryGetSpinBounceDirection", InstanceFlags);
        Assert.That(method, Is.Not.Null);

        var args = new object[] { impactPoint, incomingDirection, impactSpeed, null };
        var handled = (bool)method.Invoke(brick, args);

        Assert.That(handled, Is.True);
        return (Vector2)args[3];
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
