using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutBallSizeTests
{
    private GameObject ballObject;

    [TearDown]
    public void TearDown()
    {
        if (ballObject != null)
        {
            Object.DestroyImmediate(ballObject);
        }
    }

    [Test]
    public void SetSizeMultiplierScalesBallFromConfiguredBaseSize()
    {
        ballObject = new GameObject("Mega Ball Test");
        ballObject.transform.localScale = Vector3.one * 0.36f;
        ballObject.AddComponent<CircleCollider2D>();
        ballObject.AddComponent<Rigidbody2D>();
        var ball = ballObject.AddComponent<GetBricked.Gameplay.BallController>();

        ball.Configure(null, null, 7.5f, 0.35f, -5f, 0.23f, false);
        ball.SetSizeMultiplier(5f);

        Assert.That(ballObject.transform.localScale.x, Is.EqualTo(1.8f).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.y, Is.EqualTo(1.8f).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.z, Is.EqualTo(0.36f).Within(0.0001f));
    }
}
