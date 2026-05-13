using System;
using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRunStatsServiceTests
{
    private const BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private string playerPrefsPrefix;

    [SetUp]
    public void SetUp()
    {
        playerPrefsPrefix = $"Brickwave84.Tests.Stats.{Guid.NewGuid():N}.";
    }

    [TearDown]
    public void TearDown()
    {
        new BreakoutRunStatsService(playerPrefsPrefix).ClearLifetimeStats();
    }

    [Test]
    public void FinalizeRunAggregatesLifetimeStatsOnce()
    {
        var service = new BreakoutRunStatsService(playerPrefsPrefix);
        var helpfulDrop = CreatePowerUp(beneficial: true);
        var harmfulDrop = CreatePowerUp(beneficial: false);

        service.BeginRun(CreateRunSettings());
        service.RegisterBallLaunched();
        service.RegisterLifeLost();
        service.RegisterLevelCleared();
        service.RegisterWallHit();
        service.RegisterPaddleHit();
        service.RegisterBrickHit();
        service.RegisterBrickDestroyed();
        service.RegisterDropDropped(helpfulDrop);
        service.RegisterDropDropped(harmfulDrop);
        service.RegisterDropPickedUp(helpfulDrop);
        service.RegisterGlitchEncountered();
        service.TrackFrame(65.5f, null, null, trackTime: true, trackDistances: false);

        service.FinalizeRun(completed: true, persistLifetime: true);
        service.FinalizeRun(completed: true, persistLifetime: true);

        var lifetime = service.LifetimeStats;
        Assert.That(lifetime.RunsRecorded, Is.EqualTo(1));
        Assert.That(lifetime.BallsLaunched, Is.EqualTo(1));
        Assert.That(lifetime.LivesLost, Is.EqualTo(1));
        Assert.That(lifetime.LevelsCleared, Is.EqualTo(1));
        Assert.That(lifetime.WallsHit, Is.EqualTo(1));
        Assert.That(lifetime.PaddleHits, Is.EqualTo(1));
        Assert.That(lifetime.BricksHit, Is.EqualTo(2));
        Assert.That(lifetime.BricksDestroyed, Is.EqualTo(1));
        Assert.That(lifetime.TotalDropsDropped, Is.EqualTo(2));
        Assert.That(lifetime.TotalDropsPickedUp, Is.EqualTo(1));
        Assert.That(lifetime.HelpfulDropsDropped, Is.EqualTo(1));
        Assert.That(lifetime.HarmfulDropsDropped, Is.EqualTo(1));
        Assert.That(lifetime.HelpfulDropsPickedUp, Is.EqualTo(1));
        Assert.That(lifetime.HarmfulDropsPickedUp, Is.EqualTo(0));
        Assert.That(lifetime.GlitchesEncountered, Is.EqualTo(1));
        Assert.That(lifetime.TimePlayedSeconds, Is.EqualTo(65.5f).Within(0.001f));

        var reloadedLifetime = new BreakoutRunStatsService(playerPrefsPrefix).LifetimeStats;
        Assert.That(reloadedLifetime.RunsRecorded, Is.EqualTo(1));
        Assert.That(reloadedLifetime.BricksDestroyed, Is.EqualTo(1));
    }

    [Test]
    public void TrackFrameAccumulatesPaddleAndLaunchedBallDistance()
    {
        var service = new BreakoutRunStatsService(playerPrefsPrefix);
        var paddleObject = new GameObject("Stats Test Paddle");
        var ballObject = new GameObject("Stats Test Ball");

        try
        {
            var paddle = paddleObject.AddComponent<PaddleController>();
            var ball = ballObject.AddComponent<BallController>();
            ball.Configure(null, null, 1f, 0.35f, -10f, 0f, false);
            ball.Launch(Vector2.right);

            service.BeginRun(CreateRunSettings());
            service.TrackFrame(0f, paddle, new[] { ball }, trackTime: false, trackDistances: true);

            paddleObject.transform.position = new Vector3(3f, 4f, 0f);
            ballObject.transform.position = new Vector3(0f, 6f, 0f);
            service.TrackFrame(0f, paddle, new[] { ball }, trackTime: false, trackDistances: true);

            var current = service.CurrentRunStats;
            Assert.That(current.PaddleDistanceTraveled, Is.EqualTo(5f).Within(0.001f));
            Assert.That(current.BallsDistanceTraveled, Is.EqualTo(6f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(paddleObject);
            UnityEngine.Object.DestroyImmediate(ballObject);
        }
    }

    private static RunSettings CreateRunSettings()
    {
        return new RunSettings(
            12345,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            0,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null);
    }

    private static PowerUpDefinition CreatePowerUp(bool beneficial)
    {
        var definition = ScriptableObject.CreateInstance<PowerUpDefinition>();
        typeof(PowerUpDefinition)
            .GetField("beneficial", PrivateInstanceFlags)
            ?.SetValue(definition, beneficial);
        return definition;
    }
}
