using System.Collections.Generic;
using GetBricked.Gameplay;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutBallSpeedBurstTests
{
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
    public void SpeedBurstKeepsHighestMultiplierWithoutStacking()
    {
        var ball = CreateBall(10f);
        ball.Launch(Vector2.up);

        ball.ApplySpeedBurst(1.35f, 4f);
        ball.ApplySpeedBurst(1.35f, 4f);

        Assert.That(ball.CurrentSpeed, Is.EqualTo(13.5f).Within(0.001f));
    }

    [Test]
    public void StackingSpeedBurstAddsMultiplierWhileActive()
    {
        var ball = CreateBall(10f);
        ball.Launch(Vector2.up);

        ball.ApplyStackingSpeedBurst(1.35f, 4f, 0.12f, 1.85f, 1.25f, 7.5f);
        ball.ApplyStackingSpeedBurst(1.35f, 4f, 0.12f, 1.85f, 1.25f, 7.5f);

        Assert.That(ball.CurrentSpeed, Is.EqualTo(14.7f).Within(0.001f));
    }

    [Test]
    public void StackingSpeedBurstCapsMultiplier()
    {
        var ball = CreateBall(10f);
        ball.Launch(Vector2.up);

        for (var hitIndex = 0; hitIndex < 8; hitIndex++)
        {
            ball.ApplyStackingSpeedBurst(1.35f, 4f, 0.12f, 1.85f, 1.25f, 7.5f);
        }

        Assert.That(ball.CurrentSpeed, Is.EqualTo(18.5f).Within(0.001f));
    }

    [Test]
    public void SpeedStepsClimbUntilReset()
    {
        var ball = CreateBall(10f);
        ball.Launch(Vector2.up);

        ball.ApplySpeedStep(0.06f, 1.18f);
        ball.ApplySpeedStep(0.06f, 1.18f);
        ball.ApplySpeedStep(0.06f, 1.18f);
        ball.ApplySpeedStep(0.06f, 1.18f);

        Assert.That(ball.CurrentSpeed, Is.EqualTo(11.8f).Within(0.001f));

        ball.ResetSpeedSteps();

        Assert.That(ball.CurrentSpeed, Is.EqualTo(10f).Within(0.001f));
    }

    private BallController CreateBall(float speed)
    {
        var ballObject = new GameObject("Speed Burst Test Ball");
        runtimeObjects.Add(ballObject);
        ballObject.AddComponent<CircleCollider2D>();
        var body = ballObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        var ball = ballObject.AddComponent<BallController>();
        ball.Configure(null, null, speed, 0.2f, -10f, 0.5f, false);
        return ball;
    }
}
