using System;
using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutLevelPlannerTests
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
    public void BrutalOpeningStageExposesAllBrickAndDropTypes()
    {
        var level = CreateLevelDefinition();
        var dropA = CreatePowerUpDefinition("Drop A");
        var dropB = CreatePowerUpDefinition("Drop B");
        var dropC = CreatePowerUpDefinition("Drop C");
        var dropD = CreatePowerUpDefinition("Drop D");
        var dropE = CreatePowerUpDefinition("Drop E");

        var bricks = new List<BrickDefinition>
        {
            CreateBrickDefinition("Basic Brick", hitPoints: 1, isBreakable: true, dropA),
            CreateBrickDefinition("Reinforced Brick", hitPoints: 2, isBreakable: true, dropB),
            CreateBrickDefinition("Fortified Brick", hitPoints: 3, isBreakable: true, dropC),
            CreateBrickDefinition("Spinner Brick", hitPoints: 2, isBreakable: true, dropD, spinsOnHit: true),
            CreateBrickDefinition("Explosive Brick", hitPoints: 1, isBreakable: true, dropE, isExplosive: true),
            CreateBrickDefinition("Steel Brick", hitPoints: 1, isBreakable: false, null),
        };

        var planner = CreateLevelPlanner(new List<LevelDefinition> { level }, bricks);
        var buildPlan = planner.GetType().GetMethod("BuildPlan", InstanceFlags);
        Assert.That(buildPlan, Is.Not.Null);

        var plan = buildPlan.Invoke(
            planner,
            new object[]
            {
                level,
                0,
                new DeterministicRandomService(4242),
                new RunSettings(4242, RunDifficultyPreset.Brutal, 2, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, true, null),
            });

        Assert.That(GetFieldValue<int>(plan, "UniqueBrickTypeCount"), Is.EqualTo(bricks.Count));
        Assert.That(GetFieldValue<int>(plan, "AvailableDropTypeCount"), Is.EqualTo(5));
    }

    private object CreateLevelPlanner(List<LevelDefinition> levels, List<BrickDefinition> bricks)
    {
        var plannerType = typeof(BreakoutGameController).Assembly.GetType("GetBricked.Gameplay.BreakoutLevelPlanner", throwOnError: false);
        Assert.That(plannerType, Is.Not.Null, "Could not resolve BreakoutLevelPlanner.");
        return Activator.CreateInstance(plannerType, levels, bricks, new Func<int>(() => 4242));
    }

    private LevelDefinition CreateLevelDefinition()
    {
        var level = ScriptableObject.CreateInstance<LevelDefinition>();
        runtimeObjects.Add(level);
        SetPrivateField(level, "displayName", "Opening Volley");
        SetPrivateField(level, "layoutRows", new[] { "........", "........", "........" });
        SetPrivateField(level, "topInset", 1.5f);
        SetPrivateField(level, "ballSpeedMultiplier", 1f);
        SetPrivateField(level, "paddleSpeedMultiplier", 1f);
        return level;
    }

    private BrickDefinition CreateBrickDefinition(
        string displayName,
        int hitPoints,
        bool isBreakable,
        PowerUpDefinition powerUpDefinition,
        bool spinsOnHit = false,
        bool isExplosive = false)
    {
        var brick = ScriptableObject.CreateInstance<BrickDefinition>();
        runtimeObjects.Add(brick);
        SetPrivateField(brick, "displayName", displayName);
        SetPrivateField(brick, "hitPoints", hitPoints);
        SetPrivateField(brick, "scoreValue", 100);
        SetPrivateField(brick, "indestructible", !isBreakable);
        SetPrivateField(brick, "spinsOnHit", spinsOnHit);
        SetPrivateField(brick, "explosive", isExplosive);
        SetPrivateField(brick, "dropChance", powerUpDefinition != null ? 1f : 0f);
        SetPrivateField(
            brick,
            "dropTable",
            powerUpDefinition != null
                ? new[]
                {
                    new BrickPowerUpDropEntry(),
                }
                : Array.Empty<BrickPowerUpDropEntry>());

        if (powerUpDefinition != null)
        {
            var dropTable = (BrickPowerUpDropEntry[])typeof(BrickDefinition).GetProperty("DropTable", InstanceFlags)?.GetValue(brick);
            Assert.That(dropTable, Is.Not.Null);
            object entry = dropTable[0];
            SetPrivateField(entry, "powerUpDefinition", powerUpDefinition);
            SetPrivateField(entry, "weight", 1f);
            dropTable[0] = (BrickPowerUpDropEntry)entry;
            SetPrivateField(brick, "dropTable", dropTable);
        }

        return brick;
    }

    private PowerUpDefinition CreatePowerUpDefinition(string displayName)
    {
        var powerUp = ScriptableObject.CreateInstance<PowerUpDefinition>();
        runtimeObjects.Add(powerUp);
        SetPrivateField(powerUp, "displayName", displayName);
        SetPrivateField(powerUp, "hudLabel", displayName.ToUpperInvariant());
        return powerUp;
    }

    private static T GetFieldValue<T>(object instance, string fieldName)
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
