using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutBrickEffectResolverTests
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
    public void ResolveLaserTargetsSkipsDuplicateLowAndUnbreakableCandidates()
    {
        var resolver = new BreakoutBrickEffectResolver();
        var bricks = new List<Brick>
        {
            CreateBrick("Center", new Vector2(0f, 2f), breakable: true),
            CreateBrick("Low", new Vector2(-1.1f, 0.1f), breakable: true),
            CreateBrick("Shielded", new Vector2(1.1f, 2.2f), breakable: false),
        };

        var resolved = resolver.TryResolveLaserTargets(bricks, Vector2.zero, 2f, out var leftTarget, out var rightTarget);

        Assert.That(resolved, Is.True);
        Assert.That(leftTarget, Is.EqualTo(bricks[0]));
        Assert.That(rightTarget, Is.Null);
    }

    [Test]
    public void ResolveChainLightningTargetsPrioritizesNearestBreakableBricksAndCapsCount()
    {
        var resolver = new BreakoutBrickEffectResolver();
        var sourceBrick = CreateBrick("Source", new Vector2(0f, 2f), breakable: true);
        var nearest = CreateBrick("Nearest", new Vector2(0.35f, 2f), breakable: true);
        var second = CreateBrick("Second", new Vector2(1.1f, 2f), breakable: true);
        var third = CreateBrick("Third", new Vector2(1.7f, 2.1f), breakable: true);
        var fourth = CreateBrick("Fourth", new Vector2(2.5f, 2.05f), breakable: true);
        var unbreakable = CreateBrick("Unbreakable", new Vector2(0.1f, 2.05f), breakable: false);
        var outOfRange = CreateBrick("Far", new Vector2(4.2f, 2f), breakable: true);
        var bricks = new List<Brick> { sourceBrick, nearest, second, third, fourth, unbreakable, outOfRange };

        var targets = resolver.ResolveChainLightningTargets(bricks, sourceBrick.transform.position, sourceBrick, 1f);

        Assert.That(targets.Count, Is.EqualTo(3));
        Assert.That(targets[0], Is.EqualTo(nearest));
        Assert.That(targets[1], Is.EqualTo(second));
        Assert.That(targets[2], Is.EqualTo(third));
        Assert.That(targets, Has.No.Member(sourceBrick));
        Assert.That(targets, Has.No.Member(unbreakable));
        Assert.That(targets, Has.No.Member(outOfRange));
    }

    private Brick CreateBrick(string name, Vector2 position, bool breakable)
    {
        var brickObject = new GameObject(name);
        runtimeObjects.Add(brickObject);
        brickObject.transform.position = position;
        brickObject.transform.localScale = new Vector3(1.15f, 0.58f, 1f);
        brickObject.AddComponent<BoxCollider2D>();

        var visualObject = new GameObject(name + " Visual");
        runtimeObjects.Add(visualObject);
        visualObject.transform.SetParent(brickObject.transform, false);
        visualObject.AddComponent<SpriteRenderer>();

        var brick = brickObject.AddComponent<Brick>();
        brick.Initialize(
            null,
            CreateDefinition(name, breakable),
            effectiveHitPoints: breakable ? 1 : 0,
            new ThemeVisualStyle(Color.white, Color.gray, null),
            motionSpeed: 0f,
            motionDirection: Vector2.zero);
        return brick;
    }

    private BrickDefinition CreateDefinition(string displayName, bool breakable)
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(definition);
        SetPrivateField(definition, "displayName", displayName);
        SetPrivateField(definition, "hitPoints", 1);
        SetPrivateField(definition, "indestructible", !breakable);
        SetPrivateField(definition, "countsTowardLevelCompletion", breakable);
        SetPrivateField(definition, "dropChance", 0f);
        SetPrivateField(definition, "dropTable", Array.Empty<BrickPowerUpDropEntry>());
        return definition;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
