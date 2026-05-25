using System.Collections.Generic;
using System.Reflection;
using GetBricked.Gameplay;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutBallBrickMagnetTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private readonly List<Object> runtimeObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (var index = runtimeObjects.Count - 1; index >= 0; index--)
        {
            if (runtimeObjects[index] != null)
            {
                Object.DestroyImmediate(runtimeObjects[index]);
            }
        }

        runtimeObjects.Clear();
    }

    [Test]
    public void BrickMagnetPullsBallTowardTarget()
    {
        var ball = CreateLaunchedBall();

        ball.SetBrickMagnetTarget(new Vector2(5f, 0f), 0.6f);
        InvokeFixedUpdate(ball);

        Assert.That(ball.CurrentVelocity.x, Is.GreaterThan(0f));
    }

    [Test]
    public void NegativeBrickMagnetStrengthRepelsBallFromTarget()
    {
        var ball = CreateLaunchedBall();

        ball.SetBrickMagnetTarget(new Vector2(5f, 0f), -0.6f);
        InvokeFixedUpdate(ball);

        Assert.That(ball.CurrentVelocity.x, Is.LessThan(0f));
    }

    [Test]
    public void NegativeBrickMagnetStrengthDeflectsHeadOnApproach()
    {
        var ball = CreateLaunchedBall();

        ball.SetBrickMagnetTarget(new Vector2(0f, 5f), -0.9f);
        InvokeFixedUpdate(ball);

        Assert.That(Mathf.Abs(ball.CurrentVelocity.x), Is.GreaterThan(0.2f));
        Assert.That(ball.CurrentVelocity.y, Is.GreaterThan(9f));
        Assert.That(ball.CurrentVelocity.y, Is.LessThan(10f));
    }

    private BallController CreateLaunchedBall()
    {
        var ballObject = new GameObject("Brick Magnet Test Ball");
        runtimeObjects.Add(ballObject);
        ballObject.AddComponent<CircleCollider2D>();
        var body = ballObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        var ball = ballObject.AddComponent<BallController>();
        ball.Configure(null, null, 10f, 0.2f, -10f, 0.5f, false);
        ball.Launch(Vector2.up);
        return ball;
    }

    private static void InvokeFixedUpdate(BallController ball)
    {
        typeof(BallController)
            .GetMethod("FixedUpdate", InstanceFlags)
            .Invoke(ball, null);
    }
}
