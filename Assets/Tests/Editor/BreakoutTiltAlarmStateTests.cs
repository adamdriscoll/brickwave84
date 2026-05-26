using GetBricked.Gameplay;
using NUnit.Framework;

public sealed class BreakoutTiltAlarmStateTests
{
    [Test]
    public void RapidNudgesWarnThenTripAlarmAndLockUntilTimerClears()
    {
        var state = new BreakoutTiltAlarmState();

        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Nudge));
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Warning));
        Assert.That(state.IsWarningHot, Is.True);
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Alarm));
        Assert.That(state.IsAlarmActive, Is.True);
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Locked));

        Assert.That(state.Update(1f), Is.False);
        Assert.That(state.IsAlarmActive, Is.True);
        Assert.That(state.Update(2f), Is.True);
        Assert.That(state.IsAlarmActive, Is.False);
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Nudge));
    }

    [Test]
    public void HeatDecaysBeforeAlarmThreshold()
    {
        var state = new BreakoutTiltAlarmState();

        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Nudge));
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Warning));

        state.Update(4f);

        Assert.That(state.IsWarningHot, Is.False);
        Assert.That(state.RegisterNudge(), Is.EqualTo(BreakoutTiltAlarmTriggerResult.Nudge));
    }
}
