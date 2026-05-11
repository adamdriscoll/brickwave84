using GetBricked.Gameplay;
using NUnit.Framework;

public sealed class BreakoutTurnBasedMultiplayerControllerTests
{
    [Test]
    public void PlayerCountClampsBetweenTwoAndTen()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();

        controller.AdjustSelectedPlayerCount(-10);
        Assert.That(controller.SelectedPlayerCount, Is.EqualTo(2));

        controller.AdjustSelectedPlayerCount(20);
        Assert.That(controller.SelectedPlayerCount, Is.EqualTo(10));
    }

    [Test]
    public void StartRunGeneratesDeterministicNames()
    {
        var first = new BreakoutTurnBasedMultiplayerController();
        var second = new BreakoutTurnBasedMultiplayerController();

        first.AdjustSelectedPlayerCount(2);
        second.AdjustSelectedPlayerCount(2);
        first.StartRun(4242, 0);
        second.StartRun(4242, 0);

        Assert.That(first.Players.Count, Is.EqualTo(4));
        Assert.That(second.Players.Count, Is.EqualTo(4));
        Assert.That(first.Players[0].DisplayName, Is.EqualTo(second.Players[0].DisplayName));
        Assert.That(first.Players[3].DisplayName, Is.EqualTo(second.Players[3].DisplayName));
    }

    [Test]
    public void CompleteTurnAddsDeltaAndAdvancesPlayer()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();
        controller.StartRun(100, 0);

        var firstPlayerName = controller.CurrentPlayer.DisplayName;
        controller.CompleteTurnAndAdvance(1250, BreakoutTurnSwitchReason.BallLost);

        Assert.That(controller.CurrentPlayer.DisplayName, Is.Not.EqualTo(firstPlayerName));
        Assert.That(controller.Players[0].Score, Is.EqualTo(1250));
        Assert.That(controller.Players[0].BallsLost, Is.EqualTo(1));
        Assert.That(controller.Players[0].TurnsTaken, Is.EqualTo(1));
    }
}
