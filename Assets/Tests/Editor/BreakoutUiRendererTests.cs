using GetBricked.Gameplay;
using NUnit.Framework;

public sealed class BreakoutUiRendererTests
{
    [Test]
    public void ScoreReadoutReservesRoomForPopAnimation()
    {
        const float measuredFiveDigitScoreWidth = 54f;

        var scoreWidth = BreakoutUiRenderer.CalculateHudScoreReadoutWidth(
            measuredFiveDigitScoreWidth,
            availableWidth: 220f);

        Assert.That(scoreWidth, Is.GreaterThan(measuredFiveDigitScoreWidth));
        Assert.That(scoreWidth, Is.EqualTo(72f).Within(0.001f));
    }

    [Test]
    public void ScoreReadoutStillCapsToAvailableHudWidth()
    {
        var scoreWidth = BreakoutUiRenderer.CalculateHudScoreReadoutWidth(
            measuredTextWidth: 160f,
            availableWidth: 128f);

        Assert.That(scoreWidth, Is.EqualTo(128f).Within(0.001f));
    }
}
