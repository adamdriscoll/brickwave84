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
                new RunSettings(4242, RunDifficultyPreset.Brutal, RunScoringMode.Classic, 2, 500, 1, 1f, 1f, 1f, 1f, DropPoolMode.Mixed, true, null),
            });

        Assert.That(GetFieldValue<int>(plan, "UniqueBrickTypeCount"), Is.EqualTo(bricks.Count));
        Assert.That(GetFieldValue<int>(plan, "AvailableDropTypeCount"), Is.EqualTo(5));
    }

    [Test]
    public void TinyBricksRemainRarerThanFullSizeBasicBricksWhenAvailable()
    {
        var tinyBrick = CreateBrickDefinition("Tiny Brick", hitPoints: 1, isBreakable: true, null);
        var basicBrick = CreateBrickDefinition("Basic Brick", hitPoints: 1, isBreakable: true, null);
        SetPrivateField(tinyBrick, "sizeMultiplier", 0.5f);

        var weightMethod = GetGameplayType("GetBricked.Gameplay.BreakoutLevelPlanner")
            .GetMethod("GetProceduralBrickWeight", BindingFlags.Static | InstanceFlags);
        Assert.That(weightMethod, Is.Not.Null);

        var tinyWeight = (float)weightMethod.Invoke(null, new object[] { tinyBrick, 1, 4, 5, 8, 2, false });
        var basicWeight = (float)weightMethod.Invoke(null, new object[] { basicBrick, 1, 4, 5, 8, 2, false });

        Assert.That(tinyWeight, Is.GreaterThan(0f));
        Assert.That(tinyWeight, Is.LessThan(basicWeight));
    }

    [Test]
    public void RunProgressionStopsAfterTenthLevel()
    {
        var level = CreateLevelDefinition();
        var progressionType = GetGameplayType("GetBricked.Gameplay.BreakoutRunProgression");
        var hasNextLevel = progressionType.GetMethod("HasNextLevel", BindingFlags.Static | InstanceFlags);
        Assert.That(hasNextLevel, Is.Not.Null);

        Assert.That((bool)hasNextLevel.Invoke(null, new object[] { level, 8, 4 }), Is.True);
        Assert.That((bool)hasNextLevel.Invoke(null, new object[] { level, 9, 4 }), Is.False);
    }

    [Test]
    public void ExplosiveBricksWaitUntilMidRunInStandardDifficulty()
    {
        var explosiveBrick = CreateBrickDefinition(
            "Explosive Brick",
            hitPoints: 1,
            isBreakable: true,
            null,
            isExplosive: true);
        var weightMethod = GetGameplayType("GetBricked.Gameplay.BreakoutLevelPlanner")
            .GetMethod("GetProceduralBrickWeight", BindingFlags.Static | InstanceFlags);
        Assert.That(weightMethod, Is.Not.Null);

        var stageFiveWeight = (float)weightMethod.Invoke(
            null,
            new object[] { explosiveBrick, 1, 4, 5, 8, 4, false });
        var stageSixWeight = (float)weightMethod.Invoke(
            null,
            new object[] { explosiveBrick, 1, 4, 5, 8, 5, false });

        Assert.That(stageFiveWeight, Is.EqualTo(0f));
        Assert.That(stageSixWeight, Is.GreaterThan(0f));
    }

    [Test]
    public void SplitBricksJoinProceduralPoolAfterOpeningStages()
    {
        var tinyBrick = CreateBrickDefinition("Tiny Brick", hitPoints: 1, isBreakable: true, null);
        var splitBrick = CreateBrickDefinition("Split Brick", hitPoints: 2, isBreakable: true, null);
        SetPrivateField(tinyBrick, "sizeMultiplier", 0.5f);
        SetPrivateField(splitBrick, "splitsOnBreak", true);
        SetPrivateField(splitBrick, "splitBrickDefinition", tinyBrick);

        var plannerType = GetGameplayType("GetBricked.Gameplay.BreakoutLevelPlanner");
        var weightMethod = plannerType.GetMethod("GetProceduralBrickWeight", BindingFlags.Static | InstanceFlags);
        var symbolMethod = plannerType.GetMethod("BuildProceduralBrickSymbol", BindingFlags.Static | InstanceFlags);
        Assert.That(weightMethod, Is.Not.Null);
        Assert.That(symbolMethod, Is.Not.Null);

        var stageTwoWeight = (float)weightMethod.Invoke(
            null,
            new object[] { splitBrick, 1, 4, 5, 8, 1, false });
        var stageThreeWeight = (float)weightMethod.Invoke(
            null,
            new object[] { splitBrick, 1, 4, 5, 8, 2, false });

        Assert.That(stageTwoWeight, Is.EqualTo(0f));
        Assert.That(stageThreeWeight, Is.GreaterThan(0f));
        Assert.That((char)symbolMethod.Invoke(null, new object[] { splitBrick }), Is.EqualTo('X'));
    }

    [Test]
    public void LaterLoopsCanAddMovementToOpeningTemplate()
    {
        var basicBrick = CreateBrickDefinition("Basic Brick", hitPoints: 1, isBreakable: true, null);
        var motionMethod = GetGameplayType("GetBricked.Gameplay.BreakoutLevelPlanner")
            .GetMethod("ResolveProceduralBrickMotion", BindingFlags.Static | InstanceFlags);
        Assert.That(motionMethod, Is.Not.Null);

        var motion = motionMethod.Invoke(
            null,
            new object[]
            {
                new DeterministicRandomService(14),
                basicBrick,
                0,
                2,
                1,
                4,
                5,
                9,
            });

        Assert.That(GetPropertyValue<bool>(motion, "IsEnabled"), Is.True);
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

    private static Type GetGameplayType(string fullName)
    {
        var assembly = typeof(BreakoutGameController).Assembly;
        var resolvedType = assembly.GetType(fullName, throwOnError: false);
        Assert.That(resolvedType, Is.Not.Null, $"Could not resolve gameplay type '{fullName}'.");
        return resolvedType;
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
}
