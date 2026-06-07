using GetBricked.Gameplay;
using NUnit.Framework;

public sealed class BreakoutPlayfieldGeometryTests
{
    [Test]
    public void CalculateArenaBoundsCapsUltraWideGameplayWidth()
    {
        var standard = BreakoutPlayfieldGeometry.CalculateArenaBounds(
            5.2f,
            16f / 9f,
            0.6f,
            BreakoutPlayfieldGeometry.DefaultMaximumGameplayAspectRatio);
        var ultraWide = BreakoutPlayfieldGeometry.CalculateArenaBounds(
            5.2f,
            32f / 9f,
            0.6f,
            BreakoutPlayfieldGeometry.DefaultMaximumGameplayAspectRatio);

        Assert.That(ultraWide.Left, Is.EqualTo(standard.Left).Within(0.0001f));
        Assert.That(ultraWide.Right, Is.EqualTo(standard.Right).Within(0.0001f));
        Assert.That(ultraWide.Width, Is.EqualTo(standard.Width).Within(0.0001f));
    }

    [Test]
    public void CalculateArenaBoundsKeepsNarrowScreensNarrow()
    {
        var standard = BreakoutPlayfieldGeometry.CalculateArenaBounds(
            5.2f,
            16f / 9f,
            0.6f,
            BreakoutPlayfieldGeometry.DefaultMaximumGameplayAspectRatio);
        var narrow = BreakoutPlayfieldGeometry.CalculateArenaBounds(
            5.2f,
            4f / 3f,
            0.6f,
            BreakoutPlayfieldGeometry.DefaultMaximumGameplayAspectRatio);

        Assert.That(narrow.Width, Is.LessThan(standard.Width));
    }
}
