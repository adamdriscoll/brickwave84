using NUnit.Framework;
using GetBricked.Gameplay.Data;
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
        ball.SetSizeMultiplier(1.5f);

        Assert.That(ballObject.transform.localScale.x, Is.EqualTo(0.54f).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.y, Is.EqualTo(0.54f).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.z, Is.EqualTo(0.36f).Within(0.0001f));
    }

    [Test]
    public void SetSizeMultiplierCapsOversizedMegaBall()
    {
        ballObject = new GameObject("Mega Ball Cap Test");
        ballObject.transform.localScale = Vector3.one * 0.36f;
        ballObject.AddComponent<CircleCollider2D>();
        ballObject.AddComponent<Rigidbody2D>();
        var ball = ballObject.AddComponent<GetBricked.Gameplay.BallController>();

        ball.Configure(null, null, 7.5f, 0.35f, -5f, 0.23f, false);
        ball.SetSizeMultiplier(5f);

        var expectedScale = 0.36f * GetBricked.Gameplay.BallController.MaximumSizeMultiplier;
        Assert.That(ballObject.transform.localScale.x, Is.EqualTo(expectedScale).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.y, Is.EqualTo(expectedScale).Within(0.0001f));
        Assert.That(ballObject.transform.localScale.z, Is.EqualTo(0.36f).Within(0.0001f));
    }

    [Test]
    public void ApplyVisualStyleKeepsExistingSpriteWhenStyleHasNoSprite()
    {
        var sprite = CreateSprite();
        ballObject = CreateConfiguredBallObject(sprite);
        var ball = ballObject.GetComponent<GetBricked.Gameplay.BallController>();
        var renderer = ballObject.GetComponent<SpriteRenderer>();

        ball.ApplyVisualStyle(new ThemeVisualStyle(Color.yellow, Color.yellow, null));

        Assert.That(renderer.sprite, Is.SameAs(sprite));
        Object.DestroyImmediate(sprite);
    }

    [Test]
    public void EffectVisualRefreshKeepsConfiguredSpriteBeforeThemeStylePass()
    {
        var sprite = CreateSprite();
        ballObject = CreateConfiguredBallObject(sprite);
        var ball = ballObject.GetComponent<GetBricked.Gameplay.BallController>();
        var renderer = ballObject.GetComponent<SpriteRenderer>();

        ball.SetExplosiveBallStrength(0.5f);

        Assert.That(renderer.sprite, Is.SameAs(sprite));
        Object.DestroyImmediate(sprite);
    }

    private static GameObject CreateConfiguredBallObject(Sprite sprite)
    {
        var configuredBallObject = new GameObject("Ball Visual Test");
        configuredBallObject.transform.localScale = Vector3.one * 0.36f;
        var renderer = configuredBallObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        configuredBallObject.AddComponent<CircleCollider2D>();
        configuredBallObject.AddComponent<Rigidbody2D>();
        var ball = configuredBallObject.AddComponent<GetBricked.Gameplay.BallController>();
        ball.Configure(null, null, 7.5f, 0.35f, -5f, 0.23f, false);
        return configuredBallObject;
    }

    private static Sprite CreateSprite()
    {
        var texture = Texture2D.whiteTexture;
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            texture.width);
    }
}
