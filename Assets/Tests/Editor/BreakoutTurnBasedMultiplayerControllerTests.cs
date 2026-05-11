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

    [Test]
    public void TopScoreEndsAfterConfiguredTurnsEach()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();
        controller.AdjustSelectedTopScoreTurnLimit(-4);
        controller.StartRun(100, 0);

        var first = controller.CompleteTurnAndAdvance(100, BreakoutTurnSwitchReason.BallLost);
        Assert.That(first.IsRunComplete, Is.False);

        var final = controller.CompleteTurnAndAdvance(50, BreakoutTurnSwitchReason.BallLost);

        Assert.That(final.IsRunComplete, Is.True);
        Assert.That(controller.Winner.DisplayName, Is.EqualTo(controller.Players[0].DisplayName));
    }

    [Test]
    public void OutlastEndsWithLastAlive()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();
        controller.AdjustSelectedMode(1);
        controller.AdjustSelectedOutlastLives(-2);
        controller.AdjustSelectedPlayerCount(1);
        controller.StartRun(200, 0);

        controller.CompleteTurnAndAdvance(10, BreakoutTurnSwitchReason.BallLost);
        Assert.That(controller.CurrentPlayer.PlayerNumber, Is.EqualTo(2));

        var final = controller.CompleteTurnAndAdvance(20, BreakoutTurnSwitchReason.BallLost);

        Assert.That(final.IsRunComplete, Is.True);
        Assert.That(controller.Winner.PlayerNumber, Is.EqualTo(3));
        Assert.That(controller.Players[0].IsEliminated, Is.True);
        Assert.That(controller.Players[1].IsEliminated, Is.True);
    }

    [Test]
    public void LevelClearedAdvancesOnlyCurrentPlayersStage()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();
        controller.StartRun(300, 0);

        controller.CompleteTurnAndAdvance(500, BreakoutTurnSwitchReason.LevelCleared);

        Assert.That(controller.Players[0].CurrentLevelIndex, Is.EqualTo(1));
        Assert.That(controller.Players[1].CurrentLevelIndex, Is.EqualTo(0));
        Assert.That(controller.GetCurrentPlayerLevelIndex(), Is.EqualTo(0));
    }

    [Test]
    public void BrickStateIsStoredPerPlayerAndStage()
    {
        var controller = new BreakoutTurnBasedMultiplayerController();
        controller.StartRun(400, 0);
        var firstPlayerState = new[]
        {
            new BreakoutBrickState(null, UnityEngine.Vector2.zero, 0, 0, 1, default),
        };

        controller.SaveCurrentPlayerBrickState(0, firstPlayerState);
        controller.CompleteTurnAndAdvance(0, BreakoutTurnSwitchReason.BallLost);

        Assert.That(controller.TryGetCurrentPlayerBrickState(0, out _), Is.False);

        controller.SaveCurrentPlayerBrickState(0, System.Array.Empty<BreakoutBrickState>());
        controller.CompleteTurnAndAdvance(0, BreakoutTurnSwitchReason.BallLost);

        Assert.That(controller.TryGetCurrentPlayerBrickState(0, out var restored), Is.True);
        Assert.That(restored, Is.SameAs(firstPlayerState));
    }
}
