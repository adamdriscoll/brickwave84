using GetBricked.Gameplay;
using NUnit.Framework;
using UnityEngine;

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

    [Test]
    public void GameplayHudFrameStaysNearCappedPlayfieldOnUltraWideScreens()
    {
        var playfield = new Rect(1022f, 80f, 1796f, 920f);

        var frame = BreakoutUiRenderer.CalculateGameplayHudFrame(playfield, 3840f, 1080f);

        Assert.That(frame.x, Is.GreaterThan(700f));
        Assert.That(frame.xMax, Is.LessThan(3140f));
        Assert.That(frame.x, Is.LessThanOrEqualTo(playfield.x));
        Assert.That(frame.xMax, Is.GreaterThanOrEqualTo(playfield.xMax));
    }

    [Test]
    public void GameplayHudFrameUsesScreenMarginsWhenPlayfieldAlreadyFillsStandardWidth()
    {
        var playfield = new Rect(62f, 80f, 1796f, 920f);

        var frame = BreakoutUiRenderer.CalculateGameplayHudFrame(playfield, 1920f, 1080f);

        Assert.That(frame.x, Is.EqualTo(18f).Within(0.001f));
        Assert.That(frame.xMax, Is.EqualTo(1902f).Within(0.001f));
    }

    [Test]
    public void GameplaySidePanelUsesUltraWideShelfWhenThereIsRoom()
    {
        var playfield = new Rect(1022f, 80f, 1796f, 920f);

        var panel = BreakoutUiRenderer.CalculateGameplaySidePanelRect(
            playfield,
            3840f,
            1080f,
            206f,
            88f,
            22f,
            preferRight: true);
        var frame = BreakoutUiRenderer.CalculateGameplayHudFrame(playfield, 3840f, 1080f);

        Assert.That(panel.x, Is.GreaterThan(playfield.xMax));
        Assert.That(panel.xMax, Is.LessThanOrEqualTo(frame.xMax));
    }
}
