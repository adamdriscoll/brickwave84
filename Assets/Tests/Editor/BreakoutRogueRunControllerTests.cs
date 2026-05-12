using GetBricked.Gameplay;
using NUnit.Framework;

public sealed class BreakoutRogueRunControllerTests
{
    [Test]
    public void HazardUnlocksScaleWithHeatInsteadOfOpeningEveryRunHot()
    {
        Assert.That(BreakoutRogueRunController.GetAutoHazardUnlockCount(1, 1), Is.Zero);
        Assert.That(BreakoutRogueRunController.GetAutoHazardUnlockCount(3, 1), Is.EqualTo(1));
        Assert.That(BreakoutRogueRunController.GetAutoHazardUnlockCount(1, 50), Is.EqualTo(3));
    }
}
