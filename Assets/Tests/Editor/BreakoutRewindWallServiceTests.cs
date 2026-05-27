using System.Collections.Generic;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRewindWallServiceTests
{
    private readonly List<Object> cleanupObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (var index = cleanupObjects.Count - 1; index >= 0; index--)
        {
            if (cleanupObjects[index] != null)
            {
                Object.DestroyImmediate(cleanupObjects[index]);
            }
        }

        cleanupObjects.Clear();
    }

    [Test]
    public void ArmMakesTargetRowOptionalAndRebuildsItOnceAfterClear()
    {
        var definition = CreateBrickDefinition();
        var bricks = new List<Brick>
        {
            CreateBrick(definition, row: 0, column: 0),
            CreateBrick(definition, row: 1, column: 0),
            CreateBrick(definition, row: 1, column: 1),
        };
        var service = new BreakoutRewindWallService(bricks);

        var removedObjectiveCount = service.Arm(new BreakoutRewindWallSpec(1f, 0.8f, 0.5f), currentLevelRowCount: 2);

        Assert.That(removedObjectiveCount, Is.EqualTo(2));
        Assert.That(bricks[0].CountsTowardLevelCompletion, Is.True);
        Assert.That(bricks[1].CountsTowardLevelCompletion, Is.False);
        Assert.That(bricks[2].CountsTowardLevelCompletion, Is.False);

        var firstTarget = bricks[1];
        bricks.Remove(firstTarget);
        Assert.That(service.TryRegisterDestroyedBrick(firstTarget), Is.False);

        var secondTarget = bricks[1];
        bricks.Remove(secondTarget);
        Assert.That(service.TryRegisterDestroyedBrick(secondTarget), Is.True);

        Assert.That(service.Update(0.35f, out var showWarning, out var rebuildStates), Is.False);
        Assert.That(showWarning, Is.True);
        Assert.That(rebuildStates, Is.Null);

        Assert.That(service.Update(0.5f, out showWarning, out rebuildStates), Is.True);
        Assert.That(showWarning, Is.False);
        Assert.That(rebuildStates, Has.Length.EqualTo(2));
        Assert.That(rebuildStates[0].CountsTowardLevelCompletion, Is.False);
        Assert.That(rebuildStates[1].CountsTowardLevelCompletion, Is.False);

        Assert.That(service.TryRegisterDestroyedBrick(secondTarget), Is.False);
    }

    private BrickDefinition CreateBrickDefinition()
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        cleanupObjects.Add(definition);
        return definition;
    }

    private Brick CreateBrick(BrickDefinition definition, int row, int column)
    {
        var brickObject = new GameObject($"Rewind Wall Test Brick {row}-{column}");
        cleanupObjects.Add(brickObject);
        var visualObject = new GameObject("Visual");
        visualObject.transform.SetParent(brickObject.transform, false);
        visualObject.AddComponent<SpriteRenderer>();
        brickObject.AddComponent<BoxCollider2D>();

        var brick = brickObject.AddComponent<Brick>();
        brick.Initialize(
            null,
            definition,
            1,
            new ThemeVisualStyle(Color.white, Color.gray, null),
            0f,
            Vector2.zero,
            row,
            column);
        return brick;
    }
}
