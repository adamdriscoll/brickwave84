using System.Collections.Generic;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutRogueRunTests
{
    private const string LastRogueResultKey = "GetBricked.Rogue.LastResult";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(LastRogueResultKey);
    }

    [Test]
    public void RunSettingsCanIdentifyRogueMode()
    {
        var settings = new RunSettings(
            4242,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            500,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null,
            RunGameMode.Rogue);

        Assert.That(settings.IsRogueMode, Is.True);
        Assert.That(settings.GameModeLabel, Is.EqualTo("Rogue"));
        Assert.That(settings.StartingLives, Is.EqualTo(3));
        Assert.That(settings.ScoringMode, Is.EqualTo(RunScoringMode.Classic));
    }

    [Test]
    public void RogueStageBallSpeedMultiplierRampsAcrossTenStages()
    {
        var firstStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(0);
        var middleStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(4);
        var finalStage = BreakoutRunProgression.GetRogueStageBallSpeedMultiplier(9);

        Assert.That(firstStage, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(middleStage, Is.GreaterThan(firstStage));
        Assert.That(finalStage, Is.EqualTo(1.1f).Within(0.0001f));
    }

    [Test]
    public void RogueBossGatesTriggerAfterStagesThreeSixAndTen()
    {
        Assert.That(BreakoutRunProgression.TryGetBossGateAfterLevel(1, out _), Is.False);

        Assert.That(BreakoutRunProgression.TryGetBossGateAfterLevel(2, out var firstGate), Is.True);
        Assert.That(firstGate.GateIndex, Is.EqualTo(0));
        Assert.That(firstGate.TriggerLevelIndex, Is.EqualTo(2));
        Assert.That(firstGate.BossType, Is.EqualTo(BreakoutBossGateType.PaddlePunk));
        Assert.That(firstGate.DisplayName, Is.EqualTo("Boss Gate 1 - The Paddle Punk"));

        Assert.That(BreakoutRunProgression.TryGetBossGateAfterLevel(5, out var secondGate), Is.True);
        Assert.That(secondGate.GateIndex, Is.EqualTo(1));

        Assert.That(BreakoutRunProgression.TryGetBossGateAfterLevel(9, out var finalGate), Is.True);
        Assert.That(finalGate.GateIndex, Is.EqualTo(2));
    }

    [Test]
    public void RogueBossGateBallSpeedRampsButStaysReadable()
    {
        BreakoutRunProgression.TryGetBossGateAfterLevel(2, out var firstGate);
        BreakoutRunProgression.TryGetBossGateAfterLevel(9, out var finalGate);

        var firstGateSpeed = BreakoutRunProgression.GetBossGateBallSpeedMultiplier(firstGate);
        var finalGateSpeed = BreakoutRunProgression.GetBossGateBallSpeedMultiplier(finalGate);

        Assert.That(firstGateSpeed, Is.EqualTo(0.96f).Within(0.0001f));
        Assert.That(finalGateSpeed, Is.GreaterThan(firstGateSpeed));
        Assert.That(finalGateSpeed, Is.EqualTo(1.08f).Within(0.0001f));
    }

    [Test]
    public void PaddlePunkStartsWiggleBurstAfterRepeatedRallies()
    {
        var bossObject = new GameObject("Paddle Punk Test");

        try
        {
            var boss = bossObject.AddComponent<BreakoutPaddlePunkBoss>();
            boss.Configure(0, -4f, 4f, 1f, 4f, () => null);

            Assert.That(boss.IsWiggleActive, Is.False);

            for (var index = 0; index < 3; index++)
            {
                Assert.That(boss.TryBuildCollisionResponse(null, out _, out _), Is.True);
            }

            Assert.That(boss.IsWiggleActive, Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(bossObject);
        }
    }

    [Test]
    public void PaddlePunkOpeningPauseCreatesScoringWindow()
    {
        var bossObject = new GameObject("Paddle Punk Pause Test");

        try
        {
            var boss = bossObject.AddComponent<BreakoutPaddlePunkBoss>();
            boss.Configure(0, -4f, 4f, 1f, 4f, () => null, (min, _) => min);

            boss.StartOpeningPause();

            Assert.That(boss.IsPausedForOpening, Is.True);
            Assert.That(boss.IsWiggleActive, Is.True);
            Assert.That(boss.PhaseLabel, Is.EqualTo("Open Lane"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(bossObject);
        }
    }

    [Test]
    public void PaddlePunkInjuredSpinRecursWithSeededJitter()
    {
        var bossObject = new GameObject("Paddle Punk Injured Test");

        try
        {
            var boss = bossObject.AddComponent<BreakoutPaddlePunkBoss>();
            var randomRolls = new Queue<float>(new[] { 0.5f, 0.1f, 0.1f });
            boss.Configure(0, -4f, 4f, 1f, 4f, () => null, (min, max) =>
            {
                if (Mathf.Approximately(min, 0f) && Mathf.Approximately(max, 1f) && randomRolls.Count > 0)
                {
                    return randomRolls.Dequeue();
                }

                return min;
            });

            for (var index = 0; index < 5; index++)
            {
                Assert.That(boss.TryBuildCollisionResponse(null, out _, out _), Is.True);
            }

            Assert.That(boss.BossHitCount, Is.EqualTo(5));
            Assert.That(boss.IsInjuredSpinActive, Is.True);
            Assert.That(boss.IsPausedForOpening, Is.True);
            Assert.That(boss.PhaseLabel, Is.EqualTo("Injured"));
            Assert.That(boss.NextInjuredSpinHitCount, Is.EqualTo(10));

            for (var index = 5; index < 10; index++)
            {
                Assert.That(boss.TryBuildCollisionResponse(null, out _, out _), Is.True);
            }

            Assert.That(boss.BossHitCount, Is.EqualTo(10));
            Assert.That(boss.NextInjuredSpinHitCount, Is.EqualTo(14));

            for (var index = 10; index < 14; index++)
            {
                Assert.That(boss.TryBuildCollisionResponse(null, out _, out _), Is.True);
            }

            Assert.That(boss.BossHitCount, Is.EqualTo(14));
            Assert.That(boss.NextInjuredSpinHitCount, Is.EqualTo(18));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(bossObject);
        }
    }

    [Test]
    public void DeveloperLaunchStateCanTargetBossesAndToggleBuildPicks()
    {
        var state = new BreakoutDeveloperLaunchState();
        var upgrade = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        var drop = ScriptableObject.CreateInstance<PowerUpDefinition>();
        drop.name = "Debug Drop";

        try
        {
            state.AdjustField(BreakoutDeveloperLaunchField.Encounter, 10, null, null);
            var encounter = state.ResolveEncounter();

            Assert.That(encounter.IsBossGate, Is.True);
            Assert.That(encounter.BossGate.Value.GateIndex, Is.EqualTo(0));
            Assert.That(encounter.LevelIndex, Is.EqualTo(2));

            state.AdjustField(BreakoutDeveloperLaunchField.Lives, 20, null, null);
            Assert.That(state.LivesRemaining, Is.EqualTo(9));

            state.ToggleCurrentUpgrade(new[] { upgrade });
            state.ToggleCurrentDropUnlock(new[] { drop });

            Assert.That(state.SelectedUpgradeCount, Is.EqualTo(1));
            Assert.That(state.SelectedDropUnlockCount, Is.EqualTo(1));
            Assert.That(state.IsUpgradeSelected(upgrade), Is.True);
            Assert.That(state.IsDropUnlockSelected(drop), Is.True);

            state.ClearBuild();

            Assert.That(state.SelectedUpgradeCount, Is.EqualTo(0));
            Assert.That(state.SelectedDropUnlockCount, Is.EqualTo(0));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(upgrade);
            UnityEngine.Object.DestroyImmediate(drop);
        }
    }

    [Test]
    public void DeveloperLaunchStateCanSelectEveryBossGate()
    {
        var state = new BreakoutDeveloperLaunchState();

        for (var bossIndex = 0; bossIndex < BreakoutDeveloperLaunchState.BossEncounterCount; bossIndex++)
        {
            state.Reset();
            state.AdjustField(
                BreakoutDeveloperLaunchField.Encounter,
                BreakoutRunProgression.TargetLevelCount + bossIndex,
                null,
                null);

            var encounter = state.ResolveEncounter();

            Assert.That(encounter.IsBossGate, Is.True);
            Assert.That(encounter.BossGate.Value.GateIndex, Is.EqualTo(bossIndex));
            Assert.That(encounter.DisplayName, Does.Contain("The Paddle Punk"));
        }
    }

    [Test]
    public void DeveloperRunsDoNotRecordRogueResults()
    {
        var settings = new RunSettings(
            1212,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            500,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null,
            RunGameMode.Rogue);

        Assert.That(BreakoutGameController.ShouldRecordRogueRunResult(false, false, settings), Is.True);
        Assert.That(BreakoutGameController.ShouldRecordRogueRunResult(false, true, settings), Is.False);
    }

    [Test]
    public void RogueResultStorePersistsLastRunSummary()
    {
        var settings = new RunSettings(
            9876,
            RunDifficultyPreset.Standard,
            RunScoringMode.Classic,
            3,
            500,
            1,
            1f,
            1f,
            1f,
            1f,
            DropPoolMode.Mixed,
            false,
            null,
            RunGameMode.Rogue);
        var result = BreakoutRogueRunResultStore.BuildResult(settings, completed: false, stageReached: 6, "Classic Paddle", 1234);

        BreakoutRogueRunResultStore.Save(result);

        Assert.That(BreakoutRogueRunResultStore.TryLoad(out var loaded), Is.True);
        Assert.That(loaded.Completed, Is.False);
        Assert.That(loaded.StageReached, Is.EqualTo(6));
        Assert.That(loaded.SelectedPaddle, Is.EqualTo("Classic Paddle"));
        Assert.That(loaded.Seed, Is.EqualTo(9876));
        Assert.That(loaded.Score, Is.EqualTo(1234));
        Assert.That(BreakoutRogueRunResultStore.BuildSummary(loaded), Does.Contain("Wiped Out | Stage 06/10"));
    }
}
