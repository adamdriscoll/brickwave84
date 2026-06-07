using GetBricked.Gameplay;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutBallEscapeBoundsTests
{
    [Test]
    public void BallBeyondLeftArenaMarginCountsAsEscaped()
    {
        var arenaBounds = Rect.MinMaxRect(-8f, -4f, 8f, 4f);

        Assert.That(BreakoutGameController.HasBallEscapedPlayableArena(new Vector2(-9.2f, 0f), arenaBounds, 1.1f), Is.True);
    }

    [Test]
    public void BallBeyondRightArenaMarginCountsAsEscaped()
    {
        var arenaBounds = Rect.MinMaxRect(-8f, -4f, 8f, 4f);

        Assert.That(BreakoutGameController.HasBallEscapedPlayableArena(new Vector2(9.2f, 0f), arenaBounds, 1.1f), Is.True);
    }

    [Test]
    public void BallBeyondTopArenaMarginCountsAsEscaped()
    {
        var arenaBounds = Rect.MinMaxRect(-8f, -4f, 8f, 4f);

        Assert.That(BreakoutGameController.HasBallEscapedPlayableArena(new Vector2(0f, 5.2f), arenaBounds, 1.1f), Is.True);
    }

    [Test]
    public void BallInsideArenaMarginDoesNotCountAsEscaped()
    {
        var arenaBounds = Rect.MinMaxRect(-8f, -4f, 8f, 4f);

        Assert.That(BreakoutGameController.HasBallEscapedPlayableArena(new Vector2(8.9f, 4.9f), arenaBounds, 1.1f), Is.False);
    }

    [Test]
    public void BallBelowArenaDoesNotBypassBottomLossHandling()
    {
        var arenaBounds = Rect.MinMaxRect(-8f, -4f, 8f, 4f);

        Assert.That(BreakoutGameController.HasBallEscapedPlayableArena(new Vector2(0f, -5.2f), arenaBounds, 1.1f), Is.False);
    }
}
