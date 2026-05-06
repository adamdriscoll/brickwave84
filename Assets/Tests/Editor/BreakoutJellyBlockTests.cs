using System.Collections.Generic;
using GetBricked.Gameplay;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutJellyBlockTests
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
    public void JellySlowImmediatelyReducesLaunchedBallSpeed()
    {
        var ball = CreateBall(10f);
        ball.Launch(Vector2.up);

        ball.ApplyJellySlow(0.84f, 1.25f);

        Assert.That(ball.CurrentSpeed, Is.EqualTo(8.4f).Within(0.001f));
    }

    private BallController CreateBall(float speed)
    {
        var ballObject = new GameObject("Jelly Test Ball");
        runtimeObjects.Add(ballObject);
        ballObject.AddComponent<CircleCollider2D>();
        var body = ballObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        var ball = ballObject.AddComponent<BallController>();
        ball.Configure(null, null, speed, 0.2f, -10f, 0.5f, false);
        return ball;
    }
}
