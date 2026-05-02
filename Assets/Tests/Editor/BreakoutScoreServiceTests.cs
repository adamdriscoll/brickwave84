using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutScoreServiceTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private GameObject brickObject;
    private BrickDefinition brickDefinition;

    [TearDown]
    public void TearDown()
    {
        if (brickObject != null)
        {
            Object.DestroyImmediate(brickObject);
        }

        if (brickDefinition != null)
        {
            Object.DestroyImmediate(brickDefinition);
        }
    }

    [Test]
    public void BuildBrickScoreAwardScalesBasePointsByBallSpeed()
    {
        var service = new BreakoutScoreService();
        var brick = CreateBrick(scoreValue: 120);
        var context = new BreakoutScoreContext(activeBallCount: 1, displayedBallSpeed: 12f, baseBallSpeed: 8f, currentTimeSeconds: 10f);

        var award = service.BuildBrickScoreAward(brick, null, BrickDestructionCause.Impact, context);

        Assert.That(award.BasePoints, Is.EqualTo(180));
        Assert.That(award.BonusPoints, Is.Zero);
        Assert.That(award.TotalPoints, Is.EqualTo(180));
    }

    [Test]
    public void BuildBrickScoreAwardAddsSlamChainBonusInsideWindow()
    {
        var service = new BreakoutScoreService();
        var brick = CreateBrick(scoreValue: 100);

        service.RegisterBrickScoreEvent(null, awardedPoints: true, currentTimeSeconds: 10f);
        var award = service.BuildBrickScoreAward(
            brick,
            null,
            BrickDestructionCause.Impact,
            new BreakoutScoreContext(activeBallCount: 1, displayedBallSpeed: 8f, baseBallSpeed: 8f, currentTimeSeconds: 10.6f));

        Assert.That(award.BasePoints, Is.EqualTo(100));
        Assert.That(award.BonusPoints, Is.EqualTo(20));
        Assert.That(award.BonusLabel, Is.EqualTo("SLAM CHAIN"));
    }

    [Test]
    public void BuildBrickScoreAwardAddsScoreMultiplierBonus()
    {
        var service = new BreakoutScoreService();
        var brick = CreateBrick(scoreValue: 100);
        var context = new BreakoutScoreContext(
            activeBallCount: 1,
            displayedBallSpeed: 8f,
            baseBallSpeed: 8f,
            currentTimeSeconds: 10f,
            scoreMultiplier: 2f);

        var award = service.BuildBrickScoreAward(brick, null, BrickDestructionCause.Impact, context);

        Assert.That(award.BasePoints, Is.EqualTo(100));
        Assert.That(award.BonusPoints, Is.EqualTo(100));
        Assert.That(award.TotalPoints, Is.EqualTo(200));
        Assert.That(award.BonusLabel, Is.EqualTo("SCORE SURGE"));
    }

    private Brick CreateBrick(int scoreValue)
    {
        brickObject = new GameObject("Scoring Brick");
        brickObject.AddComponent<BoxCollider2D>();
        var brick = brickObject.AddComponent<Brick>();
        brickDefinition = ScriptableObject.CreateInstance<BrickDefinition>();
        SetPrivateField(brickDefinition, "displayName", "Scoring Brick");
        SetPrivateField(brickDefinition, "hitPoints", 1);
        SetPrivateField(brickDefinition, "scoreValue", scoreValue);
        SetPrivateField(brickDefinition, "countsTowardLevelCompletion", true);
        brick.Initialize(null, brickDefinition, 1, new ThemeVisualStyle(Color.white, Color.gray, null), 0f, Vector2.zero);
        return brick;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
