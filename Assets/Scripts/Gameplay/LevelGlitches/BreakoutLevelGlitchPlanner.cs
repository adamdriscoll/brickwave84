using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutLevelGlitchType
    {
        None = 0,
        WarpGates = 1,
        TurboRail = 2,
        MirrorGrid = 3,
        GravityPocket = 4,
        TokenStorm = 5,
        StaticWall = 6,
        RowRewrite = 7,
        PrismLanes = 8,
        SwitchbackRails = 9,
        CapsuleRoulette = 10,
        DriftRows = 11,
        HotCorners = 12,
        FlickerBricks = 13,
        CassetteSkip = 14,
        GhostRow = 15,
        SplitHorizon = 16,
        RogueGate = 17,
        PickupPinball = 18,
        MagnetStorm = 19,
        BrickConveyor = 20,
        BlacklightBricks = 21,
        RewindWall = 22,
        ScoreLeak = 23,
        LaserRain = 24,
        ThinAir = 25,
        PrismShuffle = 26,
        CloneStatic = 27,
        DropTide = 28,
        BrickLock = 29,
        SpeedSteps = 30,
        MirrorServe = 31,
        StaticJackpot = 32,
        JammedRails = 33,
        GravitySwap = 34,
        VhsTear = 35,
        CapsuleBlackout = 36,
        Brickquake = 37,
        TurboTax = 38,
        WarpJam = 39,
        NeonFlood = 40,
        LockstepRows = 41,
        StaticServe = 42,
        MeltdownCore = 44,
    }

    internal readonly struct BreakoutLevelGlitchDefinition
    {
        public BreakoutLevelGlitchDefinition(
            BreakoutLevelGlitchType glitchType,
            LevelGlitchSelection selection,
            BreakoutContentRarity rarity,
            int ladderUnlockIntensity)
        {
            GlitchType = glitchType;
            Selection = selection;
            Rarity = rarity;
            LadderUnlockIntensity = BreakoutRunProgression.ClampRogueIntensity(ladderUnlockIntensity);
        }

        public BreakoutLevelGlitchType GlitchType { get; }

        public LevelGlitchSelection Selection { get; }

        public BreakoutContentRarity Rarity { get; }

        public int LadderUnlockIntensity { get; }
    }

    internal enum BreakoutWarpGateWall
    {
        Left = 0,
        Right = 1,
        Top = 2,
    }

    internal readonly struct BreakoutWarpGateSpec
    {
        public BreakoutWarpGateSpec(BreakoutWarpGateWall wall, float normalizedPosition)
        {
            Wall = wall;
            NormalizedPosition = Mathf.Clamp01(normalizedPosition);
        }

        public BreakoutWarpGateWall Wall { get; }

        public float NormalizedPosition { get; }
    }

    internal readonly struct BreakoutWarpJamSpec
    {
        public BreakoutWarpJamSpec(float wrongExitChance)
        {
            WrongExitChance = Mathf.Clamp01(wrongExitChance);
        }

        public float WrongExitChance { get; }
    }

    internal readonly struct BreakoutTurboRailSpec
    {
        public BreakoutTurboRailSpec(BreakoutWarpGateWall wall, float normalizedPosition, float normalizedLength)
        {
            Wall = wall;
            NormalizedPosition = Mathf.Clamp01(normalizedPosition);
            NormalizedLength = Mathf.Clamp01(normalizedLength);
        }

        public BreakoutWarpGateWall Wall { get; }

        public float NormalizedPosition { get; }

        public float NormalizedLength { get; }
    }

    internal readonly struct BreakoutGravityPocketSpec
    {
        public BreakoutGravityPocketSpec(
            float normalizedX,
            float normalizedY,
            float radius,
            float strength,
            float driftSpeed = 0.24f,
            float driftPhase = 0f)
        {
            NormalizedX = Mathf.Clamp01(normalizedX);
            NormalizedY = Mathf.Clamp01(normalizedY);
            Radius = Mathf.Max(0.5f, radius);
            Strength = Mathf.Clamp01(strength);
            DriftSpeed = Mathf.Max(0.01f, driftSpeed);
            DriftPhase = driftPhase;
        }

        public float NormalizedX { get; }

        public float NormalizedY { get; }

        public float Radius { get; }

        public float Strength { get; }

        public float DriftSpeed { get; }

        public float DriftPhase { get; }
    }

    internal readonly struct BreakoutTokenStormSpec
    {
        public BreakoutTokenStormSpec(float dropChanceMultiplier, float minimumFallSpeedMultiplier, float maximumFallSpeedMultiplier)
        {
            DropChanceMultiplier = Mathf.Max(1f, dropChanceMultiplier);
            MinimumFallSpeedMultiplier = Mathf.Clamp(minimumFallSpeedMultiplier, 0.35f, 1.5f);
            MaximumFallSpeedMultiplier = Mathf.Clamp(maximumFallSpeedMultiplier, MinimumFallSpeedMultiplier, 1.5f);
        }

        public float DropChanceMultiplier { get; }

        public float MinimumFallSpeedMultiplier { get; }

        public float MaximumFallSpeedMultiplier { get; }
    }

    internal readonly struct BreakoutStaticWallSpec
    {
        public BreakoutStaticWallSpec(BreakoutWarpGateWall wall, float weakCycleSeconds, float weakDurationSeconds, float phaseOffsetSeconds)
        {
            Wall = wall == BreakoutWarpGateWall.Right
                ? BreakoutWarpGateWall.Right
                : BreakoutWarpGateWall.Left;
            WeakCycleSeconds = Mathf.Max(0.75f, weakCycleSeconds);
            WeakDurationSeconds = Mathf.Clamp(weakDurationSeconds, 0.18f, WeakCycleSeconds * 0.75f);
            PhaseOffsetSeconds = Mathf.Max(0f, phaseOffsetSeconds);
        }

        public BreakoutWarpGateWall Wall { get; }

        public float WeakCycleSeconds { get; }

        public float WeakDurationSeconds { get; }

        public float PhaseOffsetSeconds { get; }
    }

    internal readonly struct BreakoutThinAirSpec
    {
        public BreakoutThinAirSpec(
            BreakoutWarpGateWall wall,
            float openCycleSeconds,
            float openDurationSeconds,
            float warningSeconds,
            float phaseOffsetSeconds)
        {
            Wall = wall == BreakoutWarpGateWall.Right
                ? BreakoutWarpGateWall.Right
                : BreakoutWarpGateWall.Left;
            OpenCycleSeconds = Mathf.Max(1.2f, openCycleSeconds);
            OpenDurationSeconds = Mathf.Clamp(openDurationSeconds, 0.24f, OpenCycleSeconds * 0.65f);
            WarningSeconds = Mathf.Clamp(warningSeconds, 0.1f, OpenCycleSeconds - OpenDurationSeconds);
            PhaseOffsetSeconds = Mathf.Max(0f, phaseOffsetSeconds);
        }

        public BreakoutWarpGateWall Wall { get; }

        public float OpenCycleSeconds { get; }

        public float OpenDurationSeconds { get; }

        public float WarningSeconds { get; }

        public float PhaseOffsetSeconds { get; }
    }

    internal readonly struct BreakoutCloneStaticSpec
    {
        public BreakoutCloneStaticSpec(float delaySeconds, float verticalOffset, float widthMultiplier)
        {
            DelaySeconds = Mathf.Clamp(delaySeconds, 0.18f, 0.85f);
            VerticalOffset = Mathf.Clamp(verticalOffset, 0.45f, 1.35f);
            WidthMultiplier = Mathf.Clamp(widthMultiplier, 0.45f, 0.95f);
        }

        public float DelaySeconds { get; }

        public float VerticalOffset { get; }

        public float WidthMultiplier { get; }
    }

    internal readonly struct BreakoutRowRewriteSpec
    {
        public BreakoutRowRewriteSpec(
            float normalizedRow,
            float triggerSeconds,
            float warningSeconds,
            float fillChance,
            int patternSeed)
        {
            NormalizedRow = Mathf.Clamp01(normalizedRow);
            TriggerSeconds = Mathf.Max(3f, triggerSeconds);
            WarningSeconds = Mathf.Clamp(warningSeconds, 0.5f, TriggerSeconds - 0.25f);
            FillChance = Mathf.Clamp(fillChance, 0.35f, 0.9f);
            PatternSeed = patternSeed == int.MinValue ? int.MaxValue : Mathf.Abs(patternSeed);
        }

        public float NormalizedRow { get; }

        public float TriggerSeconds { get; }

        public float WarningSeconds { get; }

        public float FillChance { get; }

        public int PatternSeed { get; }
    }

    internal readonly struct BreakoutPrismLaneSpec
    {
        public BreakoutPrismLaneSpec(float normalizedX, float normalizedWidth, float refractionSign)
        {
            NormalizedX = Mathf.Clamp01(normalizedX);
            NormalizedWidth = Mathf.Clamp(normalizedWidth, 0.04f, 0.14f);
            RefractionSign = Mathf.Sign(Mathf.Approximately(refractionSign, 0f) ? 1f : refractionSign);
        }

        public float NormalizedX { get; }

        public float NormalizedWidth { get; }

        public float RefractionSign { get; }
    }

    internal readonly struct BreakoutSwitchbackRailSpec
    {
        public BreakoutSwitchbackRailSpec(float normalizedPosition, float normalizedLength, float switchCycleSeconds, float phaseOffsetSeconds)
        {
            NormalizedPosition = Mathf.Clamp01(normalizedPosition);
            NormalizedLength = Mathf.Clamp(normalizedLength, 0.22f, 0.56f);
            SwitchCycleSeconds = Mathf.Max(1.2f, switchCycleSeconds);
            PhaseOffsetSeconds = Mathf.Max(0f, phaseOffsetSeconds);
        }

        public float NormalizedPosition { get; }

        public float NormalizedLength { get; }

        public float SwitchCycleSeconds { get; }

        public float PhaseOffsetSeconds { get; }
    }

    internal readonly struct BreakoutDriftRowsSpec
    {
        public BreakoutDriftRowsSpec(float speed, float startingDirectionSign)
        {
            Speed = Mathf.Clamp(speed, 0.12f, 0.75f);
            StartingDirectionSign = Mathf.Sign(Mathf.Approximately(startingDirectionSign, 0f) ? 1f : startingDirectionSign);
        }

        public float Speed { get; }

        public float StartingDirectionSign { get; }
    }

    internal readonly struct BreakoutBrickConveyorSpec
    {
        public BreakoutBrickConveyorSpec(float speed, float startingDirectionSign, float wrapPadding)
        {
            Speed = Mathf.Clamp(speed, 0.18f, 0.85f);
            StartingDirectionSign = Mathf.Sign(Mathf.Approximately(startingDirectionSign, 0f) ? 1f : startingDirectionSign);
            WrapPadding = Mathf.Clamp(wrapPadding, 0.1f, 1.5f);
        }

        public float Speed { get; }

        public float StartingDirectionSign { get; }

        public float WrapPadding { get; }
    }

    internal readonly struct BreakoutLockstepRowsSpec
    {
        public BreakoutLockstepRowsSpec(float paddleDeltaMultiplier, float startingDirectionSign, float maximumStep)
        {
            PaddleDeltaMultiplier = Mathf.Clamp(paddleDeltaMultiplier, 0.1f, 1.4f);
            StartingDirectionSign = Mathf.Sign(Mathf.Approximately(startingDirectionSign, 0f) ? 1f : startingDirectionSign);
            MaximumStep = Mathf.Clamp(maximumStep, 0.02f, 0.35f);
        }

        public float PaddleDeltaMultiplier { get; }

        public float StartingDirectionSign { get; }

        public float MaximumStep { get; }
    }

    internal readonly struct BreakoutStaticServeSpec
    {
        public BreakoutStaticServeSpec(int ruleCount)
        {
            RuleCount = Mathf.Clamp(ruleCount, 1, 3);
        }

        public int RuleCount { get; }
    }

    internal readonly struct BreakoutMeltdownCoreSpec
    {
        public BreakoutMeltdownCoreSpec(
            float targetRow,
            float targetColumn,
            float speedBurstMultiplier,
            float speedBurstDurationSeconds,
            float bannerCooldownSeconds)
        {
            TargetRow = Mathf.Clamp01(targetRow);
            TargetColumn = Mathf.Clamp01(targetColumn);
            SpeedBurstMultiplier = Mathf.Clamp(speedBurstMultiplier, 1.02f, 1.45f);
            SpeedBurstDurationSeconds = Mathf.Clamp(speedBurstDurationSeconds, 0.35f, 4f);
            BannerCooldownSeconds = Mathf.Clamp(bannerCooldownSeconds, 0.25f, 3f);
        }

        public float TargetRow { get; }

        public float TargetColumn { get; }

        public float SpeedBurstMultiplier { get; }

        public float SpeedBurstDurationSeconds { get; }

        public float BannerCooldownSeconds { get; }
    }

    internal readonly struct BreakoutHotCornersSpec
    {
        public BreakoutHotCornersSpec(float bumperSize, float speedBurstMultiplier, float speedBurstDurationSeconds)
        {
            BumperSize = Mathf.Clamp(bumperSize, 0.5f, 1.15f);
            SpeedBurstMultiplier = Mathf.Clamp(speedBurstMultiplier, 1.05f, 1.65f);
            SpeedBurstDurationSeconds = Mathf.Clamp(speedBurstDurationSeconds, 0.75f, 5f);
        }

        public float BumperSize { get; }

        public float SpeedBurstMultiplier { get; }

        public float SpeedBurstDurationSeconds { get; }
    }

    internal readonly struct BreakoutFlickerBricksSpec
    {
        public BreakoutFlickerBricksSpec(
            float affectedBrickChance,
            float visibleSeconds,
            float hiddenSeconds,
            float hiddenAlpha,
            int patternSeed)
        {
            AffectedBrickChance = Mathf.Clamp(affectedBrickChance, 0.2f, 0.65f);
            VisibleSeconds = Mathf.Clamp(visibleSeconds, 0.8f, 2.2f);
            HiddenSeconds = Mathf.Clamp(hiddenSeconds, 0.35f, 1.2f);
            HiddenAlpha = Mathf.Clamp(hiddenAlpha, 0f, 0.18f);
            PatternSeed = patternSeed == int.MinValue ? int.MaxValue : Mathf.Abs(patternSeed);
        }

        public float AffectedBrickChance { get; }

        public float VisibleSeconds { get; }

        public float HiddenSeconds { get; }

        public float HiddenAlpha { get; }

        public int PatternSeed { get; }
    }

    internal readonly struct BreakoutCassetteSkipSpec
    {
        public BreakoutCassetteSkipSpec(int paddleHitsPerSkip, float skipDistance)
        {
            PaddleHitsPerSkip = Mathf.Clamp(paddleHitsPerSkip, 2, 6);
            SkipDistance = Mathf.Clamp(skipDistance, 0.5f, 2.35f);
        }

        public int PaddleHitsPerSkip { get; }

        public float SkipDistance { get; }
    }

    internal readonly struct BreakoutGhostRowSpec
    {
        public BreakoutGhostRowSpec(
            float normalizedRow,
            float phaseDurationSeconds,
            float cooldownSeconds,
            float hiddenAlpha)
        {
            NormalizedRow = Mathf.Clamp01(normalizedRow);
            PhaseDurationSeconds = Mathf.Clamp(phaseDurationSeconds, 1.2f, 5f);
            CooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.35f, 4f);
            HiddenAlpha = Mathf.Clamp(hiddenAlpha, 0.04f, 0.28f);
        }

        public float NormalizedRow { get; }

        public float PhaseDurationSeconds { get; }

        public float CooldownSeconds { get; }

        public float HiddenAlpha { get; }
    }

    internal readonly struct BreakoutRewindWallSpec
    {
        public BreakoutRewindWallSpec(
            float normalizedRow,
            float rebuildDelaySeconds,
            float warningSeconds)
        {
            NormalizedRow = Mathf.Clamp01(normalizedRow);
            RebuildDelaySeconds = Mathf.Clamp(rebuildDelaySeconds, 0.8f, 5f);
            WarningSeconds = Mathf.Clamp(warningSeconds, 0.25f, RebuildDelaySeconds);
        }

        public float NormalizedRow { get; }

        public float RebuildDelaySeconds { get; }

        public float WarningSeconds { get; }
    }

    internal readonly struct BreakoutScoreLeakSpec
    {
        public BreakoutScoreLeakSpec(float pointsPerSecond, float graceSeconds)
        {
            PointsPerSecond = Mathf.Clamp(pointsPerSecond, 4f, 30f);
            GraceSeconds = Mathf.Clamp(graceSeconds, 0.5f, 3f);
        }

        public float PointsPerSecond { get; }

        public float GraceSeconds { get; }
    }

    internal readonly struct BreakoutPrismShuffleSpec
    {
        public BreakoutPrismShuffleSpec(float affectedBrickChance, float rotationDegrees, float minimumHorizontal, int patternSeed)
        {
            AffectedBrickChance = Mathf.Clamp(affectedBrickChance, 0.2f, 0.65f);
            RotationDegrees = Mathf.Clamp(rotationDegrees, 6f, 24f);
            MinimumHorizontal = Mathf.Clamp(minimumHorizontal, 0.32f, 0.82f);
            PatternSeed = patternSeed == int.MinValue ? int.MaxValue : Mathf.Abs(patternSeed);
        }

        public float AffectedBrickChance { get; }

        public float RotationDegrees { get; }

        public float MinimumHorizontal { get; }

        public int PatternSeed { get; }
    }

    internal static class BreakoutScoreLeakCalculator
    {
        public static int CalculatePenalty(int currentScore, float pointsPerSecond, float deltaSeconds, ref float leakAccumulator)
        {
            if (currentScore <= 0 || pointsPerSecond <= 0f || deltaSeconds <= 0f)
            {
                leakAccumulator = 0f;
                return 0;
            }

            leakAccumulator = Mathf.Max(0f, leakAccumulator) + pointsPerSecond * deltaSeconds;
            var penalty = Mathf.Min(currentScore, Mathf.FloorToInt(leakAccumulator));

            if (penalty <= 0)
            {
                return 0;
            }

            leakAccumulator -= penalty;
            return penalty;
        }
    }

    internal readonly struct BreakoutSplitHorizonSpec
    {
        public BreakoutSplitHorizonSpec(float normalizedY, float bendDegrees, float cooldownSeconds)
        {
            NormalizedY = Mathf.Clamp01(normalizedY);
            BendDegrees = Mathf.Clamp(bendDegrees, 3f, 18f);
            CooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.04f, 0.35f);
        }

        public float NormalizedY { get; }

        public float BendDegrees { get; }

        public float CooldownSeconds { get; }
    }

    internal readonly struct BreakoutPickupPinballSpec
    {
        public BreakoutPickupPinballSpec(
            float lateralVelocityMultiplier,
            float upwardVelocityMultiplier,
            float gravityMultiplier,
            float bounceDamping)
        {
            LateralVelocityMultiplier = Mathf.Clamp(lateralVelocityMultiplier, 0.35f, 1.4f);
            UpwardVelocityMultiplier = Mathf.Clamp(upwardVelocityMultiplier, 0.1f, 1.1f);
            GravityMultiplier = Mathf.Clamp(gravityMultiplier, 0.6f, 2.2f);
            BounceDamping = Mathf.Clamp(bounceDamping, 0.65f, 1f);
        }

        public float LateralVelocityMultiplier { get; }

        public float UpwardVelocityMultiplier { get; }

        public float GravityMultiplier { get; }

        public float BounceDamping { get; }
    }

    internal readonly struct BreakoutDropTideSpec
    {
        public BreakoutDropTideSpec(float waveIntervalSeconds, float minimumHoldSeconds, float phaseOffsetSeconds)
        {
            WaveIntervalSeconds = Mathf.Clamp(waveIntervalSeconds, 0.65f, 2.4f);
            MinimumHoldSeconds = Mathf.Clamp(minimumHoldSeconds, 0.04f, WaveIntervalSeconds * 0.5f);
            PhaseOffsetSeconds = Mathf.Max(0f, phaseOffsetSeconds);
        }

        public float WaveIntervalSeconds { get; }

        public float MinimumHoldSeconds { get; }

        public float PhaseOffsetSeconds { get; }
    }

    internal readonly struct BreakoutCapsuleBlackoutSpec
    {
        public BreakoutCapsuleBlackoutSpec(float triggerWindowSeconds, float hiddenDurationSeconds, float visibilityMultiplier)
        {
            TriggerWindowSeconds = Mathf.Clamp(triggerWindowSeconds, 0.45f, 4f);
            HiddenDurationSeconds = Mathf.Clamp(hiddenDurationSeconds, 0.35f, 3f);
            VisibilityMultiplier = Mathf.Clamp(visibilityMultiplier, 0.01f, 0.55f);
        }

        public float TriggerWindowSeconds { get; }

        public float HiddenDurationSeconds { get; }

        public float VisibilityMultiplier { get; }
    }

    internal readonly struct BreakoutBrickLockSpec
    {
        public BreakoutBrickLockSpec(
            float lockedClusterRow,
            float lockedClusterColumn,
            float keyClusterRow,
            float keyClusterColumn,
            int clusterRadius)
        {
            LockedClusterRow = Mathf.Clamp01(lockedClusterRow);
            LockedClusterColumn = Mathf.Clamp01(lockedClusterColumn);
            KeyClusterRow = Mathf.Clamp01(keyClusterRow);
            KeyClusterColumn = Mathf.Clamp01(keyClusterColumn);
            ClusterRadius = Mathf.Clamp(clusterRadius, 1, 2);
        }

        public float LockedClusterRow { get; }

        public float LockedClusterColumn { get; }

        public float KeyClusterRow { get; }

        public float KeyClusterColumn { get; }

        public int ClusterRadius { get; }
    }

    internal readonly struct BreakoutBrickquakeSpec
    {
        public BreakoutBrickquakeSpec(
            float radius,
            float minimumOffset,
            float maximumOffset,
            int maxAffectedBricks,
            float heavySpeedThreshold,
            float cooldownSeconds)
        {
            Radius = Mathf.Clamp(radius, 0.75f, 3.2f);
            MinimumOffset = Mathf.Clamp(minimumOffset, 0.03f, 0.4f);
            MaximumOffset = Mathf.Clamp(maximumOffset, MinimumOffset, 0.52f);
            MaxAffectedBricks = Mathf.Clamp(maxAffectedBricks, 2, 12);
            HeavySpeedThreshold = Mathf.Clamp(heavySpeedThreshold, 7f, 18f);
            CooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.08f, 2.2f);
        }

        public float Radius { get; }

        public float MinimumOffset { get; }

        public float MaximumOffset { get; }

        public int MaxAffectedBricks { get; }

        public float HeavySpeedThreshold { get; }

        public float CooldownSeconds { get; }
    }

    internal readonly struct BreakoutTurboTaxSpec
    {
        public BreakoutTurboTaxSpec(
            float highSpeedThresholdMultiplier,
            float highSpeedScoreMultiplier,
            float slowSpeedThresholdMultiplier,
            float slowHazardDropChance)
        {
            HighSpeedThresholdMultiplier = Mathf.Clamp(highSpeedThresholdMultiplier, 1.02f, 1.85f);
            HighSpeedScoreMultiplier = Mathf.Clamp(highSpeedScoreMultiplier, 1.05f, 2.4f);
            SlowSpeedThresholdMultiplier = Mathf.Clamp(slowSpeedThresholdMultiplier, 0.45f, 1.15f);
            SlowHazardDropChance = Mathf.Clamp01(slowHazardDropChance);
        }

        public float HighSpeedThresholdMultiplier { get; }

        public float HighSpeedScoreMultiplier { get; }

        public float SlowSpeedThresholdMultiplier { get; }

        public float SlowHazardDropChance { get; }
    }

    internal readonly struct BreakoutNeonFloodSpec
    {
        public BreakoutNeonFloodSpec(
            int slamChainThreshold,
            float cooldownSeconds,
            float pickupOffset,
            float helpfulFallSpeedMultiplier,
            float harmfulFallSpeedMultiplier)
        {
            SlamChainThreshold = Mathf.Clamp(slamChainThreshold, 2, 12);
            CooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.1f, 8f);
            PickupOffset = Mathf.Clamp(pickupOffset, 0.12f, 1.2f);
            HelpfulFallSpeedMultiplier = Mathf.Clamp(helpfulFallSpeedMultiplier, 0.35f, 1.5f);
            HarmfulFallSpeedMultiplier = Mathf.Clamp(harmfulFallSpeedMultiplier, 0.35f, 1.5f);
        }

        public int SlamChainThreshold { get; }

        public float CooldownSeconds { get; }

        public float PickupOffset { get; }

        public float HelpfulFallSpeedMultiplier { get; }

        public float HarmfulFallSpeedMultiplier { get; }
    }

    internal static class BreakoutNeonFloodCalculator
    {
        public static bool ShouldTrigger(int slamChainCount, float currentTimeSeconds, float lastTriggerTimeSeconds, BreakoutNeonFloodSpec spec)
        {
            return slamChainCount >= spec.SlamChainThreshold
                && currentTimeSeconds - lastTriggerTimeSeconds >= spec.CooldownSeconds;
        }
    }

    internal static class BreakoutTurboTaxCalculator
    {
        public static int CalculateHighSpeedBonusPoints(int awardedBasePoints, float scoringSpeed, float baseBallSpeed, BreakoutTurboTaxSpec spec)
        {
            if (awardedBasePoints <= 0 || !IsHighSpeedBreak(scoringSpeed, baseBallSpeed, spec))
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * (spec.HighSpeedScoreMultiplier - 1f)));
        }

        public static bool ShouldRollSlowHazardDrop(float scoringSpeed, float baseBallSpeed, BreakoutTurboTaxSpec spec)
        {
            return scoringSpeed > 0.01f
                && baseBallSpeed > 0.01f
                && scoringSpeed <= baseBallSpeed * spec.SlowSpeedThresholdMultiplier
                && spec.SlowHazardDropChance > 0f;
        }

        public static bool IsHighSpeedBreak(float scoringSpeed, float baseBallSpeed, BreakoutTurboTaxSpec spec)
        {
            return scoringSpeed > 0.01f
                && baseBallSpeed > 0.01f
                && scoringSpeed >= baseBallSpeed * spec.HighSpeedThresholdMultiplier;
        }
    }

    internal readonly struct BreakoutSpeedStepsSpec
    {
        public BreakoutSpeedStepsSpec(float stepMultiplierIncrease, float maximumMultiplier)
        {
            StepMultiplierIncrease = Mathf.Clamp(stepMultiplierIncrease, 0.025f, 0.12f);
            MaximumMultiplier = Mathf.Clamp(maximumMultiplier, 1.15f, 2.25f);
        }

        public float StepMultiplierIncrease { get; }

        public float MaximumMultiplier { get; }
    }

    internal readonly struct BreakoutJammedRailsSpec
    {
        public BreakoutJammedRailsSpec(
            float minimumWidthMultiplier,
            float maximumWidthMultiplier,
            float pulseCycleSeconds,
            float phaseOffsetSeconds)
        {
            MinimumWidthMultiplier = Mathf.Clamp(minimumWidthMultiplier, 0.55f, 1f);
            MaximumWidthMultiplier = Mathf.Clamp(maximumWidthMultiplier, Mathf.Max(MinimumWidthMultiplier, 1f), 1.45f);
            PulseCycleSeconds = Mathf.Clamp(pulseCycleSeconds, 1.45f, 4.25f);
            PhaseOffsetSeconds = Mathf.Max(0f, phaseOffsetSeconds);
        }

        public float MinimumWidthMultiplier { get; }

        public float MaximumWidthMultiplier { get; }

        public float PulseCycleSeconds { get; }

        public float PhaseOffsetSeconds { get; }
    }

    internal static class BreakoutJammedRailsCalculator
    {
        public static float CalculateWidthMultiplier(float elapsedSeconds, BreakoutJammedRailsSpec spec)
        {
            if (spec.PulseCycleSeconds <= 0.001f)
            {
                return 1f;
            }

            var phase = Mathf.Repeat(Mathf.Max(0f, elapsedSeconds) + spec.PhaseOffsetSeconds, spec.PulseCycleSeconds)
                / spec.PulseCycleSeconds;
            var wave = (Mathf.Sin((phase * Mathf.PI * 2f) - (Mathf.PI * 0.5f)) + 1f) * 0.5f;
            var easedWave = Mathf.SmoothStep(0f, 1f, wave);
            return Mathf.Lerp(spec.MinimumWidthMultiplier, spec.MaximumWidthMultiplier, easedWave);
        }
    }

    internal readonly struct BreakoutStaticJackpotZoneSpec
    {
        public BreakoutStaticJackpotZoneSpec(float normalizedX, float normalizedY, float radius)
        {
            NormalizedX = Mathf.Clamp01(normalizedX);
            NormalizedY = Mathf.Clamp01(normalizedY);
            Radius = Mathf.Clamp(radius, 0.45f, 1.35f);
        }

        public float NormalizedX { get; }

        public float NormalizedY { get; }

        public float Radius { get; }
    }

    internal readonly struct BreakoutStaticJackpotSpec
    {
        public BreakoutStaticJackpotSpec(
            BreakoutStaticJackpotZoneSpec[] zones,
            float bonusScoreMultiplier,
            float missSpeedBurstMultiplier,
            float missSpeedBurstDurationSeconds)
        {
            Zones = zones ?? Array.Empty<BreakoutStaticJackpotZoneSpec>();
            BonusScoreMultiplier = Mathf.Clamp(bonusScoreMultiplier, 1.1f, 3f);
            MissSpeedBurstMultiplier = Mathf.Clamp(missSpeedBurstMultiplier, 1.02f, 1.5f);
            MissSpeedBurstDurationSeconds = Mathf.Clamp(missSpeedBurstDurationSeconds, 0.4f, 4f);
        }

        public BreakoutStaticJackpotZoneSpec[] Zones { get; }

        public float BonusScoreMultiplier { get; }

        public float MissSpeedBurstMultiplier { get; }

        public float MissSpeedBurstDurationSeconds { get; }
    }

    internal readonly struct BreakoutVhsTearSpec
    {
        public BreakoutVhsTearSpec(
            float normalizedY,
            float deflectionDegrees,
            float jitterStrength,
            float cooldownSeconds)
        {
            NormalizedY = Mathf.Clamp01(normalizedY);
            DeflectionDegrees = Mathf.Clamp(deflectionDegrees, 8f, 26f);
            JitterStrength = Mathf.Clamp(jitterStrength, 0.08f, 0.42f);
            CooldownSeconds = Mathf.Clamp(cooldownSeconds, 0.06f, 0.28f);
        }

        public float NormalizedY { get; }

        public float DeflectionDegrees { get; }

        public float JitterStrength { get; }

        public float CooldownSeconds { get; }
    }

    internal static class BreakoutStaticJackpotCalculator
    {
        public static bool TryFindZone(
            Vector2 worldPosition,
            Rect playfieldBounds,
            BreakoutStaticJackpotSpec spec,
            out int zoneIndex)
        {
            var zones = spec.Zones ?? Array.Empty<BreakoutStaticJackpotZoneSpec>();

            for (var index = 0; index < zones.Length; index++)
            {
                var center = ResolveZoneCenter(playfieldBounds, zones[index]);
                var radius = zones[index].Radius;

                if ((worldPosition - center).sqrMagnitude <= radius * radius)
                {
                    zoneIndex = index;
                    return true;
                }
            }

            zoneIndex = -1;
            return false;
        }

        public static Vector2 ResolveZoneCenter(Rect playfieldBounds, BreakoutStaticJackpotZoneSpec zone)
        {
            return new Vector2(
                Mathf.Lerp(playfieldBounds.xMin, playfieldBounds.xMax, zone.NormalizedX),
                Mathf.Lerp(playfieldBounds.yMin, playfieldBounds.yMax, zone.NormalizedY));
        }
    }

    internal sealed class BreakoutLevelGlitchPlan
    {
        public static readonly BreakoutLevelGlitchPlan None = new BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType.None,
            BreakoutContentRarity.Common,
            string.Empty,
            string.Empty,
            1f,
            Array.Empty<BreakoutWarpGateSpec>(),
            default,
            default,
            default);

        public BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType glitchType,
            BreakoutContentRarity rarity,
            string displayName,
            string hudLabel,
            float scoreMultiplier,
            BreakoutWarpGateSpec[] warpGates,
            BreakoutTurboRailSpec turboRail,
            BreakoutTokenStormSpec tokenStorm = default,
            BreakoutGravityPocketSpec gravityPocket = default,
            BreakoutStaticWallSpec staticWall = default,
            BreakoutThinAirSpec thinAir = default,
            BreakoutLevelGlitchType[] activeGlitchTypes = null,
            BreakoutRowRewriteSpec rowRewrite = default,
            BreakoutPrismLaneSpec[] prismLanes = null,
            BreakoutSwitchbackRailSpec switchbackRails = default,
            BreakoutDriftRowsSpec driftRows = default,
            BreakoutHotCornersSpec hotCorners = default,
            BreakoutFlickerBricksSpec flickerBricks = default,
            BreakoutCassetteSkipSpec cassetteSkip = default,
            BreakoutGhostRowSpec ghostRow = default,
            BreakoutSplitHorizonSpec splitHorizon = default,
            BreakoutPickupPinballSpec pickupPinball = default,
            BreakoutGravityPocketSpec[] magnetStormPockets = null,
            BreakoutBrickConveyorSpec brickConveyor = default,
            BreakoutRewindWallSpec rewindWall = default,
            BreakoutScoreLeakSpec scoreLeak = default,
            BreakoutPrismShuffleSpec prismShuffle = default,
            BreakoutCloneStaticSpec cloneStatic = default,
            BreakoutDropTideSpec dropTide = default,
            BreakoutBrickLockSpec brickLock = default,
            BreakoutSpeedStepsSpec speedSteps = default,
            BreakoutJammedRailsSpec jammedRails = default,
            BreakoutStaticJackpotSpec staticJackpot = default,
            BreakoutVhsTearSpec vhsTear = default,
            BreakoutCapsuleBlackoutSpec capsuleBlackout = default,
            BreakoutBrickquakeSpec brickquake = default,
            BreakoutTurboTaxSpec turboTax = default,
            BreakoutWarpJamSpec warpJam = default,
            BreakoutNeonFloodSpec neonFlood = default,
            BreakoutLockstepRowsSpec lockstepRows = default,
            BreakoutStaticServeSpec staticServe = default,
            BreakoutMeltdownCoreSpec meltdownCore = default)
        {
            GlitchType = glitchType;
            Rarity = BreakoutRarityRules.Clamp(rarity);
            DisplayName = displayName ?? string.Empty;
            HudLabel = hudLabel ?? string.Empty;
            ScoreMultiplier = Mathf.Max(1f, scoreMultiplier);
            WarpGates = warpGates ?? Array.Empty<BreakoutWarpGateSpec>();
            TurboRail = turboRail;
            TokenStorm = tokenStorm;
            GravityPocket = gravityPocket;
            StaticWall = staticWall;
            ThinAir = thinAir;
            RowRewrite = rowRewrite;
            PrismLanes = prismLanes ?? Array.Empty<BreakoutPrismLaneSpec>();
            SwitchbackRails = switchbackRails;
            DriftRows = driftRows;
            HotCorners = hotCorners;
            FlickerBricks = flickerBricks;
            CassetteSkip = cassetteSkip;
            GhostRow = ghostRow;
            SplitHorizon = splitHorizon;
            PickupPinball = pickupPinball;
            MagnetStormPockets = magnetStormPockets ?? Array.Empty<BreakoutGravityPocketSpec>();
            BrickConveyor = brickConveyor;
            RewindWall = rewindWall;
            ScoreLeak = scoreLeak;
            PrismShuffle = prismShuffle;
            CloneStatic = cloneStatic;
            DropTide = dropTide;
            BrickLock = brickLock;
            SpeedSteps = speedSteps;
            JammedRails = jammedRails;
            StaticJackpot = staticJackpot;
            VhsTear = vhsTear;
            CapsuleBlackout = capsuleBlackout;
            Brickquake = brickquake;
            TurboTax = turboTax;
            WarpJam = warpJam;
            NeonFlood = neonFlood;
            LockstepRows = lockstepRows;
            StaticServe = staticServe;
            MeltdownCore = meltdownCore;
            ActiveGlitchTypes = activeGlitchTypes != null && activeGlitchTypes.Length > 0
                ? activeGlitchTypes
                : glitchType != BreakoutLevelGlitchType.None
                    ? new[] { glitchType }
                    : Array.Empty<BreakoutLevelGlitchType>();
        }

        public BreakoutLevelGlitchType GlitchType { get; }

        public BreakoutContentRarity Rarity { get; }

        public string DisplayName { get; }

        public string HudLabel { get; }

        public float ScoreMultiplier { get; }

        public BreakoutWarpGateSpec[] WarpGates { get; }

        public BreakoutTurboRailSpec TurboRail { get; }

        public BreakoutTokenStormSpec TokenStorm { get; }

        public BreakoutGravityPocketSpec GravityPocket { get; }

        public BreakoutStaticWallSpec StaticWall { get; }

        public BreakoutThinAirSpec ThinAir { get; }

        public BreakoutRowRewriteSpec RowRewrite { get; }

        public BreakoutPrismLaneSpec[] PrismLanes { get; }

        public BreakoutSwitchbackRailSpec SwitchbackRails { get; }

        public BreakoutDriftRowsSpec DriftRows { get; }

        public BreakoutHotCornersSpec HotCorners { get; }

        public BreakoutFlickerBricksSpec FlickerBricks { get; }

        public BreakoutCassetteSkipSpec CassetteSkip { get; }

        public BreakoutGhostRowSpec GhostRow { get; }

        public BreakoutSplitHorizonSpec SplitHorizon { get; }

        public BreakoutPickupPinballSpec PickupPinball { get; }

        public BreakoutGravityPocketSpec[] MagnetStormPockets { get; }

        public BreakoutBrickConveyorSpec BrickConveyor { get; }

        public BreakoutRewindWallSpec RewindWall { get; }

        public BreakoutScoreLeakSpec ScoreLeak { get; }

        public BreakoutPrismShuffleSpec PrismShuffle { get; }

        public BreakoutCloneStaticSpec CloneStatic { get; }

        public BreakoutDropTideSpec DropTide { get; }

        public BreakoutBrickLockSpec BrickLock { get; }

        public BreakoutSpeedStepsSpec SpeedSteps { get; }

        public BreakoutJammedRailsSpec JammedRails { get; }

        public BreakoutStaticJackpotSpec StaticJackpot { get; }

        public BreakoutVhsTearSpec VhsTear { get; }

        public BreakoutCapsuleBlackoutSpec CapsuleBlackout { get; }

        public BreakoutBrickquakeSpec Brickquake { get; }

        public BreakoutTurboTaxSpec TurboTax { get; }

        public BreakoutWarpJamSpec WarpJam { get; }

        public BreakoutNeonFloodSpec NeonFlood { get; }

        public BreakoutLockstepRowsSpec LockstepRows { get; }

        public BreakoutStaticServeSpec StaticServe { get; }

        public BreakoutMeltdownCoreSpec MeltdownCore { get; }

        public BreakoutLevelGlitchType[] ActiveGlitchTypes { get; }

        public bool IsActive => ActiveGlitchTypes.Length > 0;

        public bool HasGlitch(BreakoutLevelGlitchType glitchType)
        {
            for (var index = 0; index < ActiveGlitchTypes.Length; index++)
            {
                if (ActiveGlitchTypes[index] == glitchType)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static class BreakoutLevelGlitchPlanner
    {
        public const int TurboRailLadderUnlockIntensity = 1;
        public const int MirrorGridLadderUnlockIntensity = 2;
        public const int TokenStormLadderUnlockIntensity = 3;
        public const int GravityPocketLadderUnlockIntensity = 4;
        public const int StaticWallLadderUnlockIntensity = 5;
        public const int RowRewriteLadderUnlockIntensity = 6;
        public const int PrismLanesLadderUnlockIntensity = 7;
        public const int SwitchbackRailsLadderUnlockIntensity = 8;
        public const int CapsuleRouletteLadderUnlockIntensity = 9;
        public const int DriftRowsLadderUnlockIntensity = 10;
        public const int HotCornersLadderUnlockIntensity = 11;
        public const int FlickerBricksLadderUnlockIntensity = 12;
        public const int CassetteSkipLadderUnlockIntensity = 13;
        public const int GhostRowLadderUnlockIntensity = 14;
        public const int SplitHorizonLadderUnlockIntensity = 15;
        public const int BrickConveyorLadderUnlockIntensity = 17;
        public const int RogueGateLadderUnlockIntensity = 18;
        public const int PickupPinballLadderUnlockIntensity = 19;
        public const int MagnetStormLadderUnlockIntensity = 20;
        public const int BlacklightBricksLadderUnlockIntensity = 21;
        public const int RewindWallLadderUnlockIntensity = 22;
        public const int ScoreLeakLadderUnlockIntensity = 23;
        public const int LaserRainLadderUnlockIntensity = 24;
        public const int ThinAirLadderUnlockIntensity = 25;
        public const int PrismShuffleLadderUnlockIntensity = 26;
        public const int CloneStaticLadderUnlockIntensity = 27;
        public const int DropTideLadderUnlockIntensity = 28;
        public const int BrickLockLadderUnlockIntensity = 29;
        public const int SpeedStepsLadderUnlockIntensity = 30;
        public const int MirrorServeLadderUnlockIntensity = 31;
        public const int StaticJackpotLadderUnlockIntensity = 32;
        public const int JammedRailsLadderUnlockIntensity = 33;
        public const int GravitySwapLadderUnlockIntensity = 34;
        public const int VhsTearLadderUnlockIntensity = 35;
        public const int CapsuleBlackoutLadderUnlockIntensity = 36;
        public const int BrickquakeLadderUnlockIntensity = 37;
        public const int TurboTaxLadderUnlockIntensity = 38;
        public const int WarpJamLadderUnlockIntensity = 39;
        public const int NeonFloodLadderUnlockIntensity = 40;
        public const int LockstepRowsLadderUnlockIntensity = 41;
        public const int StaticServeLadderUnlockIntensity = 42;
        public const int MeltdownCoreLadderUnlockIntensity = 44;

        private const float WarpGateScoreMultiplier = 1.35f;
        private const float TurboRailScoreMultiplier = 1.25f;
        private const float MirrorGridScoreMultiplier = 1.3f;
        private const float TokenStormScoreMultiplier = 1.32f;
        private const float GravityPocketScoreMultiplier = 1.38f;
        private const float StaticWallScoreMultiplier = 1.34f;
        private const float RowRewriteScoreMultiplier = 1.28f;
        private const float PrismLanesScoreMultiplier = 1.31f;
        private const float SwitchbackRailsScoreMultiplier = 1.29f;
        private const float CapsuleRouletteScoreMultiplier = 1.27f;
        private const float DriftRowsScoreMultiplier = 1.26f;
        private const float HotCornersScoreMultiplier = 1.28f;
        private const float FlickerBricksScoreMultiplier = 1.3f;
        private const float CassetteSkipScoreMultiplier = 1.27f;
        private const float GhostRowScoreMultiplier = 1.29f;
        private const float SplitHorizonScoreMultiplier = 1.3f;
        private const float BrickConveyorScoreMultiplier = 1.35f;
        private const float RogueGateScoreMultiplier = 1.36f;
        private const float PickupPinballScoreMultiplier = 1.33f;
        private const float MagnetStormScoreMultiplier = 1.4f;
        private const float BlacklightBricksScoreMultiplier = 1.36f;
        private const float RewindWallScoreMultiplier = 1.37f;
        private const float ScoreLeakScoreMultiplier = 1.42f;
        private const float LaserRainScoreMultiplier = 1.44f;
        private const float ThinAirScoreMultiplier = 1.45f;
        private const float PrismShuffleScoreMultiplier = 1.43f;
        private const float CloneStaticScoreMultiplier = 1.46f;
        private const float DropTideScoreMultiplier = 1.34f;
        private const float BrickLockScoreMultiplier = 1.47f;
        private const float SpeedStepsScoreMultiplier = 1.48f;
        private const float MirrorServeScoreMultiplier = 1.36f;
        private const float StaticJackpotScoreMultiplier = 1.5f;
        private const float JammedRailsScoreMultiplier = 1.49f;
        private const float GravitySwapScoreMultiplier = 1.51f;
        private const float VhsTearScoreMultiplier = 1.52f;
        private const float CapsuleBlackoutScoreMultiplier = 1.38f;
        private const float BrickquakeScoreMultiplier = 1.53f;
        private const float TurboTaxHighSpeedScoreMultiplier = 1.42f;
        private const float WarpJamScoreMultiplier = 1.54f;
        private const float WarpJamWrongExitChance = 0.36f;
        private const float NeonFloodScoreMultiplier = 1.41f;
        private const float LockstepRowsScoreMultiplier = 1.43f;
        private const float StaticServeScoreMultiplier = 1.5f;
        private const float MeltdownCoreScoreMultiplier = 1.55f;

        private static readonly BreakoutLevelGlitchDefinition[] GlitchDefinitions =
        {
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.WarpGates,
                LevelGlitchSelection.WarpGates,
                BreakoutContentRarity.Common,
                BreakoutRunProgression.MinRogueIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.TurboRail,
                LevelGlitchSelection.TurboRail,
                BreakoutContentRarity.Rare,
                TurboRailLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.MirrorGrid,
                LevelGlitchSelection.MirrorGrid,
                BreakoutContentRarity.Rare,
                MirrorGridLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.TokenStorm,
                LevelGlitchSelection.TokenStorm,
                BreakoutContentRarity.Epic,
                TokenStormLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.GravityPocket,
                LevelGlitchSelection.GravityPocket,
                BreakoutContentRarity.Epic,
                GravityPocketLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.StaticWall,
                LevelGlitchSelection.StaticWall,
                BreakoutContentRarity.Epic,
                StaticWallLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.RowRewrite,
                LevelGlitchSelection.RowRewrite,
                BreakoutContentRarity.Rare,
                RowRewriteLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.PrismLanes,
                LevelGlitchSelection.PrismLanes,
                BreakoutContentRarity.Rare,
                PrismLanesLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.SwitchbackRails,
                LevelGlitchSelection.SwitchbackRails,
                BreakoutContentRarity.Rare,
                SwitchbackRailsLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.CapsuleRoulette,
                LevelGlitchSelection.CapsuleRoulette,
                BreakoutContentRarity.Rare,
                CapsuleRouletteLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.DriftRows,
                LevelGlitchSelection.DriftRows,
                BreakoutContentRarity.Rare,
                DriftRowsLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.HotCorners,
                LevelGlitchSelection.HotCorners,
                BreakoutContentRarity.Rare,
                HotCornersLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.FlickerBricks,
                LevelGlitchSelection.FlickerBricks,
                BreakoutContentRarity.Rare,
                FlickerBricksLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.CassetteSkip,
                LevelGlitchSelection.CassetteSkip,
                BreakoutContentRarity.Rare,
                CassetteSkipLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.GhostRow,
                LevelGlitchSelection.GhostRow,
                BreakoutContentRarity.Rare,
                GhostRowLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.SplitHorizon,
                LevelGlitchSelection.SplitHorizon,
                BreakoutContentRarity.Rare,
                SplitHorizonLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.BrickConveyor,
                LevelGlitchSelection.BrickConveyor,
                BreakoutContentRarity.Epic,
                BrickConveyorLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.RogueGate,
                LevelGlitchSelection.RogueGate,
                BreakoutContentRarity.Epic,
                RogueGateLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.PickupPinball,
                LevelGlitchSelection.PickupPinball,
                BreakoutContentRarity.Epic,
                PickupPinballLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.MagnetStorm,
                LevelGlitchSelection.MagnetStorm,
                BreakoutContentRarity.Epic,
                MagnetStormLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.BlacklightBricks,
                LevelGlitchSelection.BlacklightBricks,
                BreakoutContentRarity.Epic,
                BlacklightBricksLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.RewindWall,
                LevelGlitchSelection.RewindWall,
                BreakoutContentRarity.Epic,
                RewindWallLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.ScoreLeak,
                LevelGlitchSelection.ScoreLeak,
                BreakoutContentRarity.Epic,
                ScoreLeakLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.LaserRain,
                LevelGlitchSelection.LaserRain,
                BreakoutContentRarity.Epic,
                LaserRainLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.ThinAir,
                LevelGlitchSelection.ThinAir,
                BreakoutContentRarity.Epic,
                ThinAirLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.PrismShuffle,
                LevelGlitchSelection.PrismShuffle,
                BreakoutContentRarity.Epic,
                PrismShuffleLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.CloneStatic,
                LevelGlitchSelection.CloneStatic,
                BreakoutContentRarity.Epic,
                CloneStaticLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.DropTide,
                LevelGlitchSelection.DropTide,
                BreakoutContentRarity.Epic,
                DropTideLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.BrickLock,
                LevelGlitchSelection.BrickLock,
                BreakoutContentRarity.Epic,
                BrickLockLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.SpeedSteps,
                LevelGlitchSelection.SpeedSteps,
                BreakoutContentRarity.Epic,
                SpeedStepsLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.MirrorServe,
                LevelGlitchSelection.MirrorServe,
                BreakoutContentRarity.Epic,
                MirrorServeLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.StaticJackpot,
                LevelGlitchSelection.StaticJackpot,
                BreakoutContentRarity.Epic,
                StaticJackpotLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.JammedRails,
                LevelGlitchSelection.JammedRails,
                BreakoutContentRarity.Epic,
                JammedRailsLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.GravitySwap,
                LevelGlitchSelection.GravitySwap,
                BreakoutContentRarity.Epic,
                GravitySwapLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.VhsTear,
                LevelGlitchSelection.VhsTear,
                BreakoutContentRarity.Epic,
                VhsTearLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.CapsuleBlackout,
                LevelGlitchSelection.CapsuleBlackout,
                BreakoutContentRarity.Epic,
                CapsuleBlackoutLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.Brickquake,
                LevelGlitchSelection.Brickquake,
                BreakoutContentRarity.Epic,
                BrickquakeLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.TurboTax,
                LevelGlitchSelection.TurboTax,
                BreakoutContentRarity.Epic,
                TurboTaxLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.WarpJam,
                LevelGlitchSelection.WarpJam,
                BreakoutContentRarity.Epic,
                WarpJamLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.NeonFlood,
                LevelGlitchSelection.NeonFlood,
                BreakoutContentRarity.Epic,
                NeonFloodLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.LockstepRows,
                LevelGlitchSelection.LockstepRows,
                BreakoutContentRarity.Epic,
                LockstepRowsLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.StaticServe,
                LevelGlitchSelection.StaticServe,
                BreakoutContentRarity.Epic,
                StaticServeLadderUnlockIntensity),
            new BreakoutLevelGlitchDefinition(
                BreakoutLevelGlitchType.MeltdownCore,
                LevelGlitchSelection.MeltdownCore,
                BreakoutContentRarity.Epic,
                MeltdownCoreLadderUnlockIntensity),
        };

        public static BreakoutLevelGlitchPlan BuildPlan(
            DeterministicRandomService random,
            RunSettings settings,
            int levelIndex)
        {
            if (settings == null || random == null)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            if (settings.ForceLevelGlitchRoll || IsForcedLevelGlitchSelection(settings.SelectedLevelGlitch))
            {
                return BuildSelectedGlitchPlan(random, settings, settings.SelectedLevelGlitch);
            }

            if (settings.IsRogueMode && BreakoutRunProgression.IsFinalStage(levelIndex))
            {
                return BuildRandomGlitchPlan(random, settings, levelIndex);
            }

            var chance = GetGlitchChance(settings, levelIndex);

            if (chance <= 0f || random.NextFloat() > chance)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            return BuildRandomGlitchPlan(random, settings, levelIndex);
        }

        private static BreakoutLevelGlitchPlan BuildSelectedGlitchPlan(
            DeterministicRandomService random,
            RunSettings settings,
            LevelGlitchSelection selection)
        {
            var definition = ResolveGlitchDefinition(random, settings, selection);

            if (definition.GlitchType == BreakoutLevelGlitchType.None)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            return BuildPlanFromDefinition(random, definition);
        }

        private static BreakoutLevelGlitchPlan BuildPlanFromDefinition(
            DeterministicRandomService random,
            BreakoutLevelGlitchDefinition definition)
        {
            if (definition.GlitchType == BreakoutLevelGlitchType.TurboRail)
            {
                return BuildTurboRailPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.MirrorGrid)
            {
                return BuildMirrorGridPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.GravityPocket)
            {
                return BuildGravityPocketPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.TokenStorm)
            {
                return BuildTokenStormPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.StaticWall)
            {
                return BuildStaticWallPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.RowRewrite)
            {
                return BuildRowRewritePlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.PrismLanes)
            {
                return BuildPrismLanesPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.SwitchbackRails)
            {
                return BuildSwitchbackRailsPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.CapsuleRoulette)
            {
                return BuildCapsuleRoulettePlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.DriftRows)
            {
                return BuildDriftRowsPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.HotCorners)
            {
                return BuildHotCornersPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.FlickerBricks)
            {
                return BuildFlickerBricksPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.CassetteSkip)
            {
                return BuildCassetteSkipPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.GhostRow)
            {
                return BuildGhostRowPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.SplitHorizon)
            {
                return BuildSplitHorizonPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.BrickConveyor)
            {
                return BuildBrickConveyorPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.RogueGate)
            {
                return BuildRogueGatePlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.PickupPinball)
            {
                return BuildPickupPinballPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.MagnetStorm)
            {
                return BuildMagnetStormPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.BlacklightBricks)
            {
                return BuildBlacklightBricksPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.RewindWall)
            {
                return BuildRewindWallPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.ScoreLeak)
            {
                return BuildScoreLeakPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.LaserRain)
            {
                return BuildLaserRainPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.ThinAir)
            {
                return BuildThinAirPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.PrismShuffle)
            {
                return BuildPrismShufflePlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.CloneStatic)
            {
                return BuildCloneStaticPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.DropTide)
            {
                return BuildDropTidePlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.BrickLock)
            {
                return BuildBrickLockPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.SpeedSteps)
            {
                return BuildSpeedStepsPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.MirrorServe)
            {
                return BuildMirrorServePlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.StaticJackpot)
            {
                return BuildStaticJackpotPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.JammedRails)
            {
                return BuildJammedRailsPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.GravitySwap)
            {
                return BuildGravitySwapPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.VhsTear)
            {
                return BuildVhsTearPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.CapsuleBlackout)
            {
                return BuildCapsuleBlackoutPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.Brickquake)
            {
                return BuildBrickquakePlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.TurboTax)
            {
                return BuildTurboTaxPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.WarpJam)
            {
                return BuildWarpJamPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.NeonFlood)
            {
                return BuildNeonFloodPlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.LockstepRows)
            {
                return BuildLockstepRowsPlan(random, definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.StaticServe)
            {
                return BuildStaticServePlan(definition.Rarity);
            }

            if (definition.GlitchType == BreakoutLevelGlitchType.MeltdownCore)
            {
                return BuildMeltdownCorePlan(random, definition.Rarity);
            }

            return BuildWarpGatePlan(random, definition.Rarity);
        }

        private static BreakoutLevelGlitchPlan BuildRandomGlitchPlan(
            DeterministicRandomService random,
            RunSettings settings,
            int levelIndex)
        {
            var targetCount = GetRandomGlitchTargetCount(random, settings, levelIndex);
            var definitions = new List<BreakoutLevelGlitchDefinition>();
            var selectedTypes = new HashSet<BreakoutLevelGlitchType>();

            for (var index = 0; index < targetCount; index++)
            {
                var definition = PickRandomGlitchDefinition(random, settings, selectedTypes);

                if (definition.GlitchType == BreakoutLevelGlitchType.None)
                {
                    break;
                }

                definitions.Add(definition);
                MarkGlitchSelectionExclusions(selectedTypes, definition.GlitchType);
            }

            if (definitions.Count == 0)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            if (definitions.Count == 1)
            {
                return BuildPlanFromDefinition(random, definitions[0]);
            }

            return BuildCompositePlan(random, definitions);
        }

        internal static int GetRandomGlitchTargetCount(DeterministicRandomService random, RunSettings settings, int levelIndex)
        {
            if (random == null || settings == null)
            {
                return 1;
            }

            var unlockIntensity = settings.IsRogueMode
                ? settings.LevelGlitchUnlockIntensity
                : BreakoutRunProgression.MaxRogueIntensity;
            var levelPressure = Mathf.Clamp01(levelIndex / 9f);
            var maxCount = unlockIntensity >= 35
                ? 3
                : unlockIntensity >= 18
                    ? 2
                    : 1;
            var count = 1;

            if (maxCount >= 2)
            {
                var secondChance = Mathf.Clamp01(Mathf.InverseLerp(12f, 35f, unlockIntensity) * 0.45f + levelPressure * 0.25f);

                if (BreakoutRunProgression.IsFinalStage(levelIndex))
                {
                    secondChance = Mathf.Clamp01(secondChance + 0.2f);
                }

                if (random.NextFloat() <= secondChance)
                {
                    count++;
                }
            }

            if (maxCount >= 3 && count >= 2)
            {
                var thirdChance = Mathf.Clamp01(Mathf.InverseLerp(35f, BreakoutRunProgression.MaxRogueIntensity, unlockIntensity) * 0.35f + levelPressure * 0.15f);

                if (BreakoutRunProgression.IsFinalStage(levelIndex))
                {
                    thirdChance = Mathf.Clamp01(thirdChance + 0.15f);
                }

                if (random.NextFloat() <= thirdChance)
                {
                    count++;
                }
            }

            return Mathf.Clamp(count, 1, maxCount);
        }

        private static BreakoutLevelGlitchPlan BuildCompositePlan(
            DeterministicRandomService random,
            List<BreakoutLevelGlitchDefinition> definitions)
        {
            var activeTypes = new BreakoutLevelGlitchType[definitions.Count];
            var displayName = string.Empty;
            var scoreMultiplier = 1f;
            var rarity = BreakoutContentRarity.Common;
            var warpGates = Array.Empty<BreakoutWarpGateSpec>();
            var turboRail = default(BreakoutTurboRailSpec);
            var tokenStorm = default(BreakoutTokenStormSpec);
            var gravityPocket = default(BreakoutGravityPocketSpec);
            var staticWall = default(BreakoutStaticWallSpec);
            var thinAir = default(BreakoutThinAirSpec);
            var rowRewrite = default(BreakoutRowRewriteSpec);
            var prismLanes = Array.Empty<BreakoutPrismLaneSpec>();
            var switchbackRails = default(BreakoutSwitchbackRailSpec);
            var driftRows = default(BreakoutDriftRowsSpec);
            var hotCorners = default(BreakoutHotCornersSpec);
            var flickerBricks = default(BreakoutFlickerBricksSpec);
            var cassetteSkip = default(BreakoutCassetteSkipSpec);
            var ghostRow = default(BreakoutGhostRowSpec);
            var splitHorizon = default(BreakoutSplitHorizonSpec);
            var pickupPinball = default(BreakoutPickupPinballSpec);
            var magnetStormPockets = Array.Empty<BreakoutGravityPocketSpec>();
            var brickConveyor = default(BreakoutBrickConveyorSpec);
            var rewindWall = default(BreakoutRewindWallSpec);
            var scoreLeak = default(BreakoutScoreLeakSpec);
            var prismShuffle = default(BreakoutPrismShuffleSpec);
            var cloneStatic = default(BreakoutCloneStaticSpec);
            var dropTide = default(BreakoutDropTideSpec);
            var brickLock = default(BreakoutBrickLockSpec);
            var speedSteps = default(BreakoutSpeedStepsSpec);
            var jammedRails = default(BreakoutJammedRailsSpec);
            var staticJackpot = default(BreakoutStaticJackpotSpec);
            var vhsTear = default(BreakoutVhsTearSpec);
            var capsuleBlackout = default(BreakoutCapsuleBlackoutSpec);
            var brickquake = default(BreakoutBrickquakeSpec);
            var turboTax = default(BreakoutTurboTaxSpec);
            var warpJam = default(BreakoutWarpJamSpec);
            var neonFlood = default(BreakoutNeonFloodSpec);
            var lockstepRows = default(BreakoutLockstepRowsSpec);
            var staticServe = default(BreakoutStaticServeSpec);
            var meltdownCore = default(BreakoutMeltdownCoreSpec);

            for (var index = 0; index < definitions.Count; index++)
            {
                var plan = BuildPlanFromDefinition(random, definitions[index]);
                activeTypes[index] = plan.GlitchType;
                displayName = string.IsNullOrWhiteSpace(displayName)
                    ? plan.DisplayName
                    : $"{displayName} + {plan.DisplayName}";
                scoreMultiplier *= Mathf.Max(1f, plan.ScoreMultiplier);
                rarity = (BreakoutContentRarity)Mathf.Max((int)rarity, (int)plan.Rarity);

                if (plan.HasGlitch(BreakoutLevelGlitchType.WarpGates))
                {
                    warpGates = plan.WarpGates;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.RogueGate))
                {
                    warpGates = plan.WarpGates;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.TurboRail))
                {
                    turboRail = plan.TurboRail;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.TokenStorm))
                {
                    tokenStorm = plan.TokenStorm;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.GravityPocket))
                {
                    gravityPocket = plan.GravityPocket;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.GravitySwap))
                {
                    gravityPocket = plan.GravityPocket;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.StaticWall))
                {
                    staticWall = plan.StaticWall;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.ThinAir))
                {
                    thinAir = plan.ThinAir;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.RowRewrite))
                {
                    rowRewrite = plan.RowRewrite;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.PrismLanes))
                {
                    prismLanes = plan.PrismLanes;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.SwitchbackRails))
                {
                    switchbackRails = plan.SwitchbackRails;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.DriftRows))
                {
                    driftRows = plan.DriftRows;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.HotCorners))
                {
                    hotCorners = plan.HotCorners;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.FlickerBricks))
                {
                    flickerBricks = plan.FlickerBricks;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.CassetteSkip))
                {
                    cassetteSkip = plan.CassetteSkip;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.GhostRow))
                {
                    ghostRow = plan.GhostRow;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.SplitHorizon))
                {
                    splitHorizon = plan.SplitHorizon;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.PickupPinball))
                {
                    pickupPinball = plan.PickupPinball;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.MagnetStorm))
                {
                    magnetStormPockets = plan.MagnetStormPockets;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.BrickConveyor))
                {
                    brickConveyor = plan.BrickConveyor;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.RewindWall))
                {
                    rewindWall = plan.RewindWall;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.ScoreLeak))
                {
                    scoreLeak = plan.ScoreLeak;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.PrismShuffle))
                {
                    prismShuffle = plan.PrismShuffle;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.CloneStatic))
                {
                    cloneStatic = plan.CloneStatic;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.DropTide))
                {
                    dropTide = plan.DropTide;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.BrickLock))
                {
                    brickLock = plan.BrickLock;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.SpeedSteps))
                {
                    speedSteps = plan.SpeedSteps;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.JammedRails))
                {
                    jammedRails = plan.JammedRails;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.StaticJackpot))
                {
                    staticJackpot = plan.StaticJackpot;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.VhsTear))
                {
                    vhsTear = plan.VhsTear;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.CapsuleBlackout))
                {
                    capsuleBlackout = plan.CapsuleBlackout;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.Brickquake))
                {
                    brickquake = plan.Brickquake;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.TurboTax))
                {
                    turboTax = plan.TurboTax;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.WarpJam))
                {
                    warpGates = plan.WarpGates;
                    warpJam = plan.WarpJam;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.NeonFlood))
                {
                    neonFlood = plan.NeonFlood;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.LockstepRows))
                {
                    lockstepRows = plan.LockstepRows;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.StaticServe))
                {
                    staticServe = plan.StaticServe;
                }

                if (plan.HasGlitch(BreakoutLevelGlitchType.MeltdownCore))
                {
                    meltdownCore = plan.MeltdownCore;
                }
            }

            return new BreakoutLevelGlitchPlan(
                activeTypes[0],
                rarity,
                displayName,
                $"Glitch Stack x{scoreMultiplier:0.00}",
                scoreMultiplier,
                warpGates,
                turboRail,
                tokenStorm,
                gravityPocket,
                staticWall,
                thinAir,
                activeTypes,
                rowRewrite,
                prismLanes,
                switchbackRails,
                driftRows,
                hotCorners,
                flickerBricks,
                cassetteSkip,
                ghostRow,
                splitHorizon,
                pickupPinball,
                magnetStormPockets,
                brickConveyor,
                rewindWall,
                scoreLeak,
                prismShuffle,
                cloneStatic,
                dropTide,
                brickLock,
                speedSteps,
                jammedRails,
                staticJackpot,
                vhsTear,
                capsuleBlackout,
                brickquake,
                turboTax,
                warpJam,
                neonFlood,
                lockstepRows,
                staticServe,
                meltdownCore);
        }

        private static void MarkGlitchSelectionExclusions(
            HashSet<BreakoutLevelGlitchType> selectedTypes,
            BreakoutLevelGlitchType selectedType)
        {
            if (selectedTypes == null)
            {
                return;
            }

            selectedTypes.Add(selectedType);

            if (selectedType == BreakoutLevelGlitchType.WarpGates)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.RogueGate);
                selectedTypes.Add(BreakoutLevelGlitchType.WarpJam);
            }
            else if (selectedType == BreakoutLevelGlitchType.RogueGate)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.WarpGates);
                selectedTypes.Add(BreakoutLevelGlitchType.WarpJam);
            }
            else if (selectedType == BreakoutLevelGlitchType.WarpJam)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.WarpGates);
                selectedTypes.Add(BreakoutLevelGlitchType.RogueGate);
            }
            else if (selectedType == BreakoutLevelGlitchType.RowRewrite)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.RewindWall);
            }
            else if (selectedType == BreakoutLevelGlitchType.RewindWall)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.RowRewrite);
            }
            else if (selectedType == BreakoutLevelGlitchType.StaticWall)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.ThinAir);
                selectedTypes.Add(BreakoutLevelGlitchType.StaticServe);
            }
            else if (selectedType == BreakoutLevelGlitchType.ThinAir)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.StaticWall);
                selectedTypes.Add(BreakoutLevelGlitchType.SwitchbackRails);
            }
            else if (selectedType == BreakoutLevelGlitchType.SwitchbackRails)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.ThinAir);
                selectedTypes.Add(BreakoutLevelGlitchType.StaticServe);
            }
            else if (selectedType == BreakoutLevelGlitchType.TurboRail)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.StaticServe);
            }
            else if (selectedType == BreakoutLevelGlitchType.GravityPocket)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.GravitySwap);
            }
            else if (selectedType == BreakoutLevelGlitchType.GravitySwap)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.GravityPocket);
            }
            else if (selectedType == BreakoutLevelGlitchType.DriftRows)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.BrickConveyor);
                selectedTypes.Add(BreakoutLevelGlitchType.LockstepRows);
            }
            else if (selectedType == BreakoutLevelGlitchType.BrickConveyor)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.DriftRows);
                selectedTypes.Add(BreakoutLevelGlitchType.LockstepRows);
            }
            else if (selectedType == BreakoutLevelGlitchType.LockstepRows)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.DriftRows);
                selectedTypes.Add(BreakoutLevelGlitchType.BrickConveyor);
            }
            else if (selectedType == BreakoutLevelGlitchType.StaticServe)
            {
                selectedTypes.Add(BreakoutLevelGlitchType.TurboRail);
                selectedTypes.Add(BreakoutLevelGlitchType.StaticWall);
                selectedTypes.Add(BreakoutLevelGlitchType.SwitchbackRails);
            }
        }

        private static BreakoutLevelGlitchPlan BuildWarpGatePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var gateCount = random.Range(2, 5);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.WarpGates,
                rarity,
                "Warp Gates",
                $"Warp Gates x{WarpGateScoreMultiplier:0.00}",
                WarpGateScoreMultiplier,
                BuildWarpGates(random, gateCount),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildRogueGatePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.RogueGate,
                rarity,
                "Rogue Gate",
                $"Rogue Gate x{RogueGateScoreMultiplier:0.00}",
                RogueGateScoreMultiplier,
                BuildRogueGate(random),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildWarpJamPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var gateCount = random.Range(3, 5);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.WarpJam,
                rarity,
                "Warp Jam",
                $"Warp Jam {WarpJamWrongExitChance * 100f:0}% x{WarpJamScoreMultiplier:0.00}",
                WarpJamScoreMultiplier,
                BuildWarpGates(random, gateCount),
                default,
                warpJam: new BreakoutWarpJamSpec(WarpJamWrongExitChance));
        }

        private static BreakoutLevelGlitchPlan BuildNeonFloodPlan(BreakoutContentRarity rarity)
        {
            var neonFlood = new BreakoutNeonFloodSpec(4, 1.65f, 0.42f, 0.88f, 1.16f);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.NeonFlood,
                rarity,
                "Neon Flood",
                $"Neon Flood {neonFlood.SlamChainThreshold}+ x{NeonFloodScoreMultiplier:0.00}",
                NeonFloodScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                neonFlood: neonFlood);
        }

        private static BreakoutLevelGlitchPlan BuildTurboRailPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.TurboRail,
                rarity,
                "Turbo Rail",
                $"Turbo Rail x{TurboRailScoreMultiplier:0.00}",
                TurboRailScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                BuildTurboRail(random));
        }

        private static BreakoutLevelGlitchPlan BuildMirrorGridPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.MirrorGrid,
                rarity,
                "Mirror Grid",
                $"Mirror Grid x{MirrorGridScoreMultiplier:0.00}",
                MirrorGridScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildGravityPocketPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.GravityPocket,
                rarity,
                "Gravity Pocket",
                $"Gravity Pocket x{GravityPocketScoreMultiplier:0.00}",
                GravityPocketScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                BuildGravityPocket(random));
        }

        private static BreakoutLevelGlitchPlan BuildGravitySwapPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.GravitySwap,
                rarity,
                "Gravity Swap",
                $"Gravity Swap x{GravitySwapScoreMultiplier:0.00}",
                GravitySwapScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                BuildGravitySwap(random));
        }

        private static BreakoutLevelGlitchPlan BuildTokenStormPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.TokenStorm,
                rarity,
                "Token Storm",
                $"Token Storm x{TokenStormScoreMultiplier:0.00}",
                TokenStormScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                new BreakoutTokenStormSpec(1.65f, 0.55f, 1.45f));
        }

        private static BreakoutLevelGlitchPlan BuildPickupPinballPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.PickupPinball,
                rarity,
                "Pickup Pinball",
                $"Pickup Pinball x{PickupPinballScoreMultiplier:0.00}",
                PickupPinballScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                pickupPinball: new BreakoutPickupPinballSpec(0.82f, 0.62f, 1.18f, 0.88f));
        }

        private static BreakoutLevelGlitchPlan BuildMagnetStormPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.MagnetStorm,
                rarity,
                "Magnet Storm",
                $"Magnet Storm x{MagnetStormScoreMultiplier:0.00}",
                MagnetStormScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                magnetStormPockets: BuildMagnetStormPockets(random));
        }

        private static BreakoutLevelGlitchPlan BuildDropTidePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var dropTide = BuildDropTide(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.DropTide,
                rarity,
                "Drop Tide",
                $"Drop Tide {dropTide.WaveIntervalSeconds:0.0}s x{DropTideScoreMultiplier:0.00}",
                DropTideScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                dropTide: dropTide);
        }

        private static BreakoutLevelGlitchPlan BuildBrickLockPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.BrickLock,
                rarity,
                "Brick Lock",
                $"Brick Lock x{BrickLockScoreMultiplier:0.00}",
                BrickLockScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                brickLock: BuildBrickLock(random));
        }

        private static BreakoutLevelGlitchPlan BuildSpeedStepsPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var speedSteps = BuildSpeedSteps(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.SpeedSteps,
                rarity,
                "Speed Steps",
                $"Speed Steps +{speedSteps.StepMultiplierIncrease:0.00} x{SpeedStepsScoreMultiplier:0.00}",
                SpeedStepsScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                speedSteps: speedSteps);
        }

        private static BreakoutLevelGlitchPlan BuildMirrorServePlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.MirrorServe,
                rarity,
                "Mirror Serve",
                $"Mirror Serve x{MirrorServeScoreMultiplier:0.00}",
                MirrorServeScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildStaticJackpotPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var staticJackpot = BuildStaticJackpot(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.StaticJackpot,
                rarity,
                "Static Jackpot",
                $"Static Jackpot x{StaticJackpotScoreMultiplier:0.00}",
                StaticJackpotScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                staticJackpot: staticJackpot);
        }

        private static BreakoutLevelGlitchPlan BuildJammedRailsPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var jammedRails = BuildJammedRails(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.JammedRails,
                rarity,
                "Jammed Rails",
                $"Jammed Rails {jammedRails.MinimumWidthMultiplier:0.00}-{jammedRails.MaximumWidthMultiplier:0.00}x x{JammedRailsScoreMultiplier:0.00}",
                JammedRailsScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                jammedRails: jammedRails);
        }

        private static BreakoutLevelGlitchPlan BuildVhsTearPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var vhsTear = BuildVhsTear(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.VhsTear,
                rarity,
                "VHS Tear",
                $"VHS Tear {vhsTear.DeflectionDegrees:0}deg x{VhsTearScoreMultiplier:0.00}",
                VhsTearScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                vhsTear: vhsTear);
        }

        private static BreakoutLevelGlitchPlan BuildCapsuleBlackoutPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.CapsuleBlackout,
                rarity,
                "Capsule Blackout",
                $"Capsule Blackout x{CapsuleBlackoutScoreMultiplier:0.00}",
                CapsuleBlackoutScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                capsuleBlackout: new BreakoutCapsuleBlackoutSpec(2.4f, 1.45f, 0.03f));
        }

        private static BreakoutLevelGlitchPlan BuildBrickquakePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var brickquake = BuildBrickquake(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.Brickquake,
                rarity,
                "Brickquake",
                $"Brickquake x{BrickquakeScoreMultiplier:0.00}",
                BrickquakeScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                brickquake: brickquake);
        }

        private static BreakoutLevelGlitchPlan BuildTurboTaxPlan(BreakoutContentRarity rarity)
        {
            var turboTax = new BreakoutTurboTaxSpec(1.18f, TurboTaxHighSpeedScoreMultiplier, 0.95f, 0.42f);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.TurboTax,
                rarity,
                "Turbo Tax",
                $"Turbo Tax >{turboTax.HighSpeedThresholdMultiplier:0.00}x pays x{turboTax.HighSpeedScoreMultiplier:0.00}",
                1f,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                turboTax: turboTax);
        }

        private static BreakoutLevelGlitchPlan BuildStaticWallPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.StaticWall,
                rarity,
                "Static Wall",
                $"Static Wall x{StaticWallScoreMultiplier:0.00}",
                StaticWallScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                default,
                BuildStaticWall(random));
        }

        private static BreakoutLevelGlitchPlan BuildRowRewritePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.RowRewrite,
                rarity,
                "Row Rewrite",
                $"Row Rewrite x{RowRewriteScoreMultiplier:0.00}",
                RowRewriteScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                default,
                default,
                rowRewrite: BuildRowRewrite(random));
        }

        private static BreakoutLevelGlitchPlan BuildPrismLanesPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.PrismLanes,
                rarity,
                "Prism Lanes",
                $"Prism Lanes x{PrismLanesScoreMultiplier:0.00}",
                PrismLanesScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                default,
                default,
                rowRewrite: default,
                prismLanes: BuildPrismLanes(random));
        }

        private static BreakoutLevelGlitchPlan BuildSwitchbackRailsPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.SwitchbackRails,
                rarity,
                "Switchback Rails",
                $"Switchback Rails x{SwitchbackRailsScoreMultiplier:0.00}",
                SwitchbackRailsScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                default,
                default,
                default,
                rowRewrite: default,
                prismLanes: null,
                switchbackRails: BuildSwitchbackRails(random));
        }

        private static BreakoutLevelGlitchPlan BuildCapsuleRoulettePlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.CapsuleRoulette,
                rarity,
                "Capsule Roulette",
                $"Capsule Roulette x{CapsuleRouletteScoreMultiplier:0.00}",
                CapsuleRouletteScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildDriftRowsPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.DriftRows,
                rarity,
                "Drift Rows",
                $"Drift Rows x{DriftRowsScoreMultiplier:0.00}",
                DriftRowsScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                driftRows: BuildDriftRows(random));
        }

        private static BreakoutLevelGlitchPlan BuildHotCornersPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.HotCorners,
                rarity,
                "Hot Corners",
                $"Hot Corners x{HotCornersScoreMultiplier:0.00}",
                HotCornersScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                hotCorners: BuildHotCorners(random));
        }

        private static BreakoutLevelGlitchPlan BuildFlickerBricksPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.FlickerBricks,
                rarity,
                "Flicker Bricks",
                $"Flicker Bricks x{FlickerBricksScoreMultiplier:0.00}",
                FlickerBricksScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                flickerBricks: BuildFlickerBricks(random));
        }

        private static BreakoutLevelGlitchPlan BuildCassetteSkipPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.CassetteSkip,
                rarity,
                "Cassette Skip",
                $"Cassette Skip x{CassetteSkipScoreMultiplier:0.00}",
                CassetteSkipScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                cassetteSkip: BuildCassetteSkip(random));
        }

        private static BreakoutLevelGlitchPlan BuildGhostRowPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.GhostRow,
                rarity,
                "Ghost Row",
                $"Ghost Row x{GhostRowScoreMultiplier:0.00}",
                GhostRowScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                ghostRow: BuildGhostRow(random));
        }

        private static BreakoutLevelGlitchPlan BuildSplitHorizonPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.SplitHorizon,
                rarity,
                "Split Horizon",
                $"Split Horizon x{SplitHorizonScoreMultiplier:0.00}",
                SplitHorizonScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                splitHorizon: BuildSplitHorizon(random));
        }

        private static BreakoutLevelGlitchPlan BuildBrickConveyorPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.BrickConveyor,
                rarity,
                "Brick Conveyor",
                $"Brick Conveyor x{BrickConveyorScoreMultiplier:0.00}",
                BrickConveyorScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                brickConveyor: BuildBrickConveyor(random));
        }

        private static BreakoutLevelGlitchPlan BuildLockstepRowsPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.LockstepRows,
                rarity,
                "Lockstep Rows",
                $"Lockstep Rows x{LockstepRowsScoreMultiplier:0.00}",
                LockstepRowsScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                lockstepRows: BuildLockstepRows(random));
        }

        private static BreakoutLevelGlitchPlan BuildStaticServePlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.StaticServe,
                rarity,
                "Static Serve",
                $"Static Serve x{StaticServeScoreMultiplier:0.00}",
                StaticServeScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                staticServe: new BreakoutStaticServeSpec(3));
        }

        private static BreakoutLevelGlitchPlan BuildMeltdownCorePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.MeltdownCore,
                rarity,
                "Meltdown Core",
                $"Meltdown Core x{MeltdownCoreScoreMultiplier:0.00}",
                MeltdownCoreScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                meltdownCore: BuildMeltdownCore(random));
        }

        private static BreakoutLevelGlitchPlan BuildBlacklightBricksPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.BlacklightBricks,
                rarity,
                "Blacklight Bricks",
                $"Blacklight Bricks x{BlacklightBricksScoreMultiplier:0.00}",
                BlacklightBricksScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildRewindWallPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.RewindWall,
                rarity,
                "Rewind Wall",
                $"Rewind Wall x{RewindWallScoreMultiplier:0.00}",
                RewindWallScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                rewindWall: BuildRewindWall(random));
        }

        private static BreakoutLevelGlitchPlan BuildScoreLeakPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            var scoreLeak = BuildScoreLeak(random);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.ScoreLeak,
                rarity,
                "Score Leak",
                $"Score Leak -{scoreLeak.PointsPerSecond:0}/s x{ScoreLeakScoreMultiplier:0.00}",
                ScoreLeakScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                scoreLeak: scoreLeak);
        }

        private static BreakoutLevelGlitchPlan BuildLaserRainPlan(BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.LaserRain,
                rarity,
                "Laser Rain",
                $"Laser Rain x{LaserRainScoreMultiplier:0.00}",
                LaserRainScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildThinAirPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.ThinAir,
                rarity,
                "Thin Air",
                $"Thin Air x{ThinAirScoreMultiplier:0.00}",
                ThinAirScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                thinAir: BuildThinAir(random));
        }

        private static BreakoutLevelGlitchPlan BuildPrismShufflePlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.PrismShuffle,
                rarity,
                "Prism Shuffle",
                $"Prism Shuffle x{PrismShuffleScoreMultiplier:0.00}",
                PrismShuffleScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                prismShuffle: BuildPrismShuffle(random));
        }

        private static BreakoutLevelGlitchPlan BuildCloneStaticPlan(DeterministicRandomService random, BreakoutContentRarity rarity)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.CloneStatic,
                rarity,
                "Clone Static",
                $"Clone Static x{CloneStaticScoreMultiplier:0.00}",
                CloneStaticScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                default,
                cloneStatic: BuildCloneStatic(random));
        }

        public static float GetGlitchChance(RunSettings settings, int levelIndex)
        {
            if (settings == null)
            {
                return 0f;
            }

            if (settings.IsRogueMode && !HasUnlockedRogueGlitch(settings))
            {
                return 0f;
            }

            var levelPressure = Mathf.Clamp01(levelIndex / 9f);
            float baseChance;

            if (settings.IsRogueMode)
            {
                var intensity = BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity);
                var completedUnlockIntensity = settings.LevelGlitchUnlockIntensity;

                if (BreakoutRunProgression.IsFinalStage(levelIndex))
                {
                    return 1f;
                }

                if (completedUnlockIntensity <= 0)
                {
                    return 0f;
                }

                baseChance = Mathf.Lerp(0.12f, 0.58f, Mathf.InverseLerp(1f, BreakoutRunProgression.MaxRogueIntensity, completedUnlockIntensity));
                baseChance += levelPressure * 0.1f;
            }
            else if (settings.IsTurnBasedMode)
            {
                if (!settings.LevelGlitchesEnabled)
                {
                    return 0f;
                }

                baseChance = 0.18f + (levelPressure * 0.12f);
            }
            else
            {
                if (!settings.LevelGlitchesEnabled)
                {
                    return 0f;
                }

                baseChance = settings.DifficultyPreset switch
                {
                    RunDifficultyPreset.Casual => 0.18f,
                    RunDifficultyPreset.Brutal => 0.42f,
                    _ => 0.28f,
                };
                baseChance += levelPressure * 0.12f;
            }

            return Mathf.Clamp01(baseChance * Mathf.Max(0f, settings.LevelGlitchChanceMultiplier));
        }

        private static BreakoutWarpGateSpec[] BuildWarpGates(DeterministicRandomService random, int gateCount)
        {
            gateCount = Mathf.Clamp(gateCount, 2, 4);
            var gates = new BreakoutWarpGateSpec[gateCount];

            for (var index = 0; index < gateCount; index++)
            {
                var wall = ResolveGateWall(random, index);
                var lane = (index + 1f) / (gateCount + 1f);
                var jitter = random.Range(-0.12f, 0.12f);
                gates[index] = new BreakoutWarpGateSpec(wall, Mathf.Clamp01(lane + jitter));
            }

            return gates;
        }

        private static BreakoutTurboRailSpec BuildTurboRail(DeterministicRandomService random)
        {
            var wall = (BreakoutWarpGateWall)random.Range(0, 3);
            return new BreakoutTurboRailSpec(
                wall,
                random.Range(0.18f, 0.82f),
                random.Range(0.18f, 0.32f));
        }

        private static BreakoutGravityPocketSpec BuildGravityPocket(DeterministicRandomService random)
        {
            return new BreakoutGravityPocketSpec(
                random.Range(0.24f, 0.76f),
                random.Range(0.34f, 0.78f),
                random.Range(1.9f, 2.45f),
                random.Range(0.58f, 0.78f),
                random.Range(0.18f, 0.28f),
                random.Range(0f, Mathf.PI * 2f));
        }

        private static BreakoutGravityPocketSpec BuildGravitySwap(DeterministicRandomService random)
        {
            return new BreakoutGravityPocketSpec(
                random.Range(0.24f, 0.76f),
                random.Range(0.32f, 0.8f),
                random.Range(2.05f, 2.55f),
                random.Range(0.62f, 0.82f),
                random.Range(0.2f, 0.3f),
                random.Range(0f, Mathf.PI * 2f));
        }

        private static BreakoutGravityPocketSpec[] BuildMagnetStormPockets(DeterministicRandomService random)
        {
            const int pocketCount = 3;
            var pockets = new BreakoutGravityPocketSpec[pocketCount];

            for (var index = 0; index < pocketCount; index++)
            {
                var lane = (index + 1f) / (pocketCount + 1f);
                var mirroredLane = index % 2 == 0 ? lane : 1f - lane;
                pockets[index] = new BreakoutGravityPocketSpec(
                    Mathf.Clamp01(mirroredLane + random.Range(-0.08f, 0.08f)),
                    random.Range(0.28f, 0.82f),
                    random.Range(1.45f, 1.85f),
                    random.Range(0.44f, 0.58f),
                    random.Range(0.22f, 0.36f),
                    random.Range(0f, Mathf.PI * 2f));
            }

            return pockets;
        }

        private static BreakoutStaticWallSpec BuildStaticWall(DeterministicRandomService random)
        {
            return new BreakoutStaticWallSpec(
                random.NextBool() ? BreakoutWarpGateWall.Right : BreakoutWarpGateWall.Left,
                random.Range(2.1f, 2.85f),
                random.Range(0.72f, 1.05f),
                random.Range(0f, 2.85f));
        }

        private static BreakoutRowRewriteSpec BuildRowRewrite(DeterministicRandomService random)
        {
            return new BreakoutRowRewriteSpec(
                random.Range(0.12f, 0.78f),
                random.Range(8.5f, 13.5f),
                random.Range(1.8f, 2.6f),
                random.Range(0.54f, 0.76f),
                random.Range(1, int.MaxValue));
        }

        private static BreakoutPrismLaneSpec[] BuildPrismLanes(DeterministicRandomService random)
        {
            var laneCount = random.Range(2, 4);
            var lanes = new BreakoutPrismLaneSpec[laneCount];
            var startingSign = random.NextBool() ? 1f : -1f;

            for (var index = 0; index < laneCount; index++)
            {
                var lane = (index + 1f) / (laneCount + 1f);
                var jitter = random.Range(-0.045f, 0.045f);
                var width = random.Range(0.055f, 0.085f);
                var sign = startingSign * (index % 2 == 0 ? 1f : -1f);
                lanes[index] = new BreakoutPrismLaneSpec(lane + jitter, width, sign);
            }

            return lanes;
        }

        private static BreakoutSwitchbackRailSpec BuildSwitchbackRails(DeterministicRandomService random)
        {
            return new BreakoutSwitchbackRailSpec(
                random.Range(0.42f, 0.72f),
                random.Range(0.34f, 0.48f),
                random.Range(2.35f, 3.15f),
                random.Range(0f, 3.15f));
        }

        private static BreakoutDriftRowsSpec BuildDriftRows(DeterministicRandomService random)
        {
            return new BreakoutDriftRowsSpec(
                random.Range(0.26f, 0.38f),
                random.NextBool() ? 1f : -1f);
        }

        private static BreakoutHotCornersSpec BuildHotCorners(DeterministicRandomService random)
        {
            return new BreakoutHotCornersSpec(
                random.Range(0.72f, 0.94f),
                random.Range(1.16f, 1.28f),
                random.Range(2.35f, 3.25f));
        }

        private static BreakoutFlickerBricksSpec BuildFlickerBricks(DeterministicRandomService random)
        {
            return new BreakoutFlickerBricksSpec(
                random.Range(0.36f, 0.48f),
                random.Range(1.25f, 1.65f),
                random.Range(0.58f, 0.88f),
                random.Range(0.035f, 0.075f),
                random.Range(1, int.MaxValue));
        }

        private static BreakoutCassetteSkipSpec BuildCassetteSkip(DeterministicRandomService random)
        {
            return new BreakoutCassetteSkipSpec(
                random.Range(2, 4),
                random.Range(1.45f, 2.05f));
        }

        private static BreakoutGhostRowSpec BuildGhostRow(DeterministicRandomService random)
        {
            return new BreakoutGhostRowSpec(
                random.Range(0.16f, 0.82f),
                random.Range(2.25f, 3.35f),
                random.Range(0.85f, 1.45f),
                random.Range(0.08f, 0.14f));
        }

        private static BreakoutRewindWallSpec BuildRewindWall(DeterministicRandomService random)
        {
            return new BreakoutRewindWallSpec(
                random.Range(0.18f, 0.84f),
                random.Range(1.65f, 2.65f),
                random.Range(0.55f, 0.95f));
        }

        private static BreakoutScoreLeakSpec BuildScoreLeak(DeterministicRandomService random)
        {
            return new BreakoutScoreLeakSpec(
                random.Range(10f, 16f),
                random.Range(1.15f, 1.75f));
        }

        private static BreakoutThinAirSpec BuildThinAir(DeterministicRandomService random)
        {
            return new BreakoutThinAirSpec(
                random.NextBool() ? BreakoutWarpGateWall.Right : BreakoutWarpGateWall.Left,
                random.Range(3.35f, 4.25f),
                random.Range(0.85f, 1.22f),
                random.Range(0.52f, 0.78f),
                random.Range(0f, 4.25f));
        }

        private static BreakoutPrismShuffleSpec BuildPrismShuffle(DeterministicRandomService random)
        {
            return new BreakoutPrismShuffleSpec(
                random.Range(0.36f, 0.48f),
                random.Range(11f, 16f),
                random.Range(0.54f, 0.68f),
                random.Range(1, int.MaxValue));
        }

        private static BreakoutCloneStaticSpec BuildCloneStatic(DeterministicRandomService random)
        {
            return new BreakoutCloneStaticSpec(
                random.Range(0.32f, 0.52f),
                random.Range(0.82f, 1.08f),
                random.Range(0.64f, 0.78f));
        }

        private static BreakoutDropTideSpec BuildDropTide(DeterministicRandomService random)
        {
            return new BreakoutDropTideSpec(
                random.Range(1.15f, 1.55f),
                random.Range(0.12f, 0.22f),
                random.Range(0f, 1.55f));
        }

        private static BreakoutBrickLockSpec BuildBrickLock(DeterministicRandomService random)
        {
            var lockedRow = random.Range(0.18f, 0.82f);
            var keyRow = Mathf.Repeat(lockedRow + random.Range(0.32f, 0.68f), 1f);
            return new BreakoutBrickLockSpec(
                lockedRow,
                random.Range(0.22f, 0.78f),
                keyRow,
                random.Range(0.22f, 0.78f),
                random.NextBool() ? 1 : 2);
        }

        private static BreakoutSpeedStepsSpec BuildSpeedSteps(DeterministicRandomService random)
        {
            return new BreakoutSpeedStepsSpec(
                random.Range(0.05f, 0.07f),
                random.Range(1.62f, 1.78f));
        }

        private static BreakoutStaticJackpotSpec BuildStaticJackpot(DeterministicRandomService random)
        {
            const int zoneCount = 3;
            var zones = new BreakoutStaticJackpotZoneSpec[zoneCount];
            var phase = random.Range(0f, Mathf.PI * 2f);

            for (var index = 0; index < zoneCount; index++)
            {
                var lane = (index + 1f) / (zoneCount + 1f);
                var wave = Mathf.Sin(phase + index * 2.1f);
                zones[index] = new BreakoutStaticJackpotZoneSpec(
                    Mathf.Clamp01(lane + random.Range(-0.08f, 0.08f)),
                    Mathf.Clamp01(0.42f + wave * 0.18f + random.Range(-0.05f, 0.05f)),
                    random.Range(0.74f, 0.94f));
            }

            return new BreakoutStaticJackpotSpec(
                zones,
                random.Range(1.65f, 1.85f),
                random.Range(1.08f, 1.14f),
                random.Range(1.25f, 1.75f));
        }

        private static BreakoutJammedRailsSpec BuildJammedRails(DeterministicRandomService random)
        {
            return new BreakoutJammedRailsSpec(
                random.Range(0.68f, 0.76f),
                random.Range(1.22f, 1.34f),
                random.Range(2.05f, 2.85f),
                random.Range(0f, 2.85f));
        }

        private static BreakoutVhsTearSpec BuildVhsTear(DeterministicRandomService random)
        {
            return new BreakoutVhsTearSpec(
                random.Range(0.36f, 0.68f),
                random.Range(14f, 22f),
                random.Range(0.18f, 0.34f),
                random.Range(0.1f, 0.18f));
        }

        internal static float CalculateDropTideReleaseDelay(float elapsedSeconds, BreakoutDropTideSpec spec)
        {
            if (elapsedSeconds < 0f || spec.WaveIntervalSeconds <= 0.001f)
            {
                return 0f;
            }

            var phase = Mathf.Repeat(elapsedSeconds + spec.PhaseOffsetSeconds, spec.WaveIntervalSeconds);
            var delay = spec.WaveIntervalSeconds - phase;

            if (delay < spec.MinimumHoldSeconds)
            {
                delay += spec.WaveIntervalSeconds;
            }

            return Mathf.Max(0f, delay);
        }

        private static BreakoutSplitHorizonSpec BuildSplitHorizon(DeterministicRandomService random)
        {
            return new BreakoutSplitHorizonSpec(
                random.Range(0.44f, 0.56f),
                random.Range(7.5f, 10.5f),
                random.Range(0.1f, 0.16f));
        }

        private static BreakoutBrickConveyorSpec BuildBrickConveyor(DeterministicRandomService random)
        {
            return new BreakoutBrickConveyorSpec(
                random.Range(0.34f, 0.48f),
                random.NextBool() ? 1f : -1f,
                random.Range(0.35f, 0.65f));
        }

        private static BreakoutLockstepRowsSpec BuildLockstepRows(DeterministicRandomService random)
        {
            return new BreakoutLockstepRowsSpec(
                random.Range(0.44f, 0.62f),
                random.NextBool() ? 1f : -1f,
                random.Range(0.12f, 0.2f));
        }

        private static BreakoutBrickquakeSpec BuildBrickquake(DeterministicRandomService random)
        {
            return new BreakoutBrickquakeSpec(
                random.Range(1.85f, 2.45f),
                random.Range(0.1f, 0.16f),
                random.Range(0.26f, 0.36f),
                random.Range(4, 8),
                random.Range(9.8f, 11.8f),
                random.Range(0.42f, 0.64f));
        }

        private static BreakoutMeltdownCoreSpec BuildMeltdownCore(DeterministicRandomService random)
        {
            return new BreakoutMeltdownCoreSpec(
                random.Range(0.2f, 0.72f),
                random.Range(0.18f, 0.82f),
                random.Range(1.12f, 1.2f),
                random.Range(1.4f, 2.1f),
                random.Range(0.7f, 1.05f));
        }

        private static BreakoutWarpGateSpec[] BuildRogueGate(DeterministicRandomService random)
        {
            return new[]
            {
                new BreakoutWarpGateSpec(
                    ResolveGateWall(random, random.Range(0, 3)),
                    random.Range(0.18f, 0.82f)),
            };
        }

        private static BreakoutLevelGlitchDefinition ResolveGlitchDefinition(
            DeterministicRandomService random,
            RunSettings settings,
            LevelGlitchSelection selection)
        {
            if (IsForcedLevelGlitchSelection(selection))
            {
                return TryFindGlitchDefinition(selection, settings, out var forcedDefinition)
                    ? forcedDefinition
                    : default;
            }

            return PickRandomGlitchDefinition(random, settings);
        }

        private static BreakoutLevelGlitchDefinition PickRandomGlitchDefinition(
            DeterministicRandomService random,
            RunSettings settings)
        {
            return PickRandomGlitchDefinition(random, settings, null);
        }

        private static BreakoutLevelGlitchDefinition PickRandomGlitchDefinition(
            DeterministicRandomService random,
            RunSettings settings,
            HashSet<BreakoutLevelGlitchType> excludedTypes)
        {
            var totalWeight = 0f;

            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                var definition = GlitchDefinitions[index];

                if (IsGlitchUnlockedForSettings(definition, settings)
                    && (excludedTypes == null || !excludedTypes.Contains(definition.GlitchType)))
                {
                    totalWeight += BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);
                }
            }

            if (totalWeight <= 0f)
            {
                return default;
            }

            var roll = random.Range(0f, totalWeight);

            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                var definition = GlitchDefinitions[index];

                if (!IsGlitchUnlockedForSettings(definition, settings)
                    || (excludedTypes != null && excludedTypes.Contains(definition.GlitchType)))
                {
                    continue;
                }

                roll -= BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);

                if (roll <= 0f)
                {
                    return definition;
                }
            }

            return GlitchDefinitions[0];
        }

        private static bool TryFindGlitchDefinition(
            LevelGlitchSelection selection,
            RunSettings settings,
            out BreakoutLevelGlitchDefinition definition)
        {
            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                var candidate = GlitchDefinitions[index];

                if (candidate.Selection == selection && IsGlitchUnlockedForSettings(candidate, settings))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = default;
            return false;
        }

        private static bool IsGlitchUnlockedForSettings(BreakoutLevelGlitchDefinition definition, RunSettings settings)
        {
            return definition.GlitchType != BreakoutLevelGlitchType.None
                && (settings == null
                    || !settings.IsRogueMode
                    || settings.IgnoreLevelGlitchUnlocks
                    || IsDefaultRogueGlitch(definition)
                    || settings.LevelGlitchUnlockIntensity >= definition.LadderUnlockIntensity);
        }

        private static bool HasUnlockedRogueGlitch(RunSettings settings)
        {
            var completedUnlockIntensity = settings != null
                ? settings.LevelGlitchUnlockIntensity
                : 0;

            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                var definition = GlitchDefinitions[index];

                if (IsDefaultRogueGlitch(definition) || completedUnlockIntensity >= definition.LadderUnlockIntensity)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefaultRogueGlitch(BreakoutLevelGlitchDefinition definition)
        {
            return definition.GlitchType == BreakoutLevelGlitchType.WarpGates;
        }

        private static bool IsForcedLevelGlitchSelection(LevelGlitchSelection selection)
        {
            return selection == LevelGlitchSelection.WarpGates
                || selection == LevelGlitchSelection.TurboRail
                || selection == LevelGlitchSelection.MirrorGrid
                || selection == LevelGlitchSelection.GravityPocket
                || selection == LevelGlitchSelection.TokenStorm
                || selection == LevelGlitchSelection.StaticWall
                || selection == LevelGlitchSelection.RowRewrite
                || selection == LevelGlitchSelection.PrismLanes
                || selection == LevelGlitchSelection.SwitchbackRails
                || selection == LevelGlitchSelection.CapsuleRoulette
                || selection == LevelGlitchSelection.DriftRows
                || selection == LevelGlitchSelection.HotCorners
                || selection == LevelGlitchSelection.FlickerBricks
                || selection == LevelGlitchSelection.CassetteSkip
                || selection == LevelGlitchSelection.GhostRow
                || selection == LevelGlitchSelection.SplitHorizon
                || selection == LevelGlitchSelection.BrickConveyor
                || selection == LevelGlitchSelection.RogueGate
                || selection == LevelGlitchSelection.PickupPinball
                || selection == LevelGlitchSelection.MagnetStorm
                || selection == LevelGlitchSelection.BlacklightBricks
                || selection == LevelGlitchSelection.RewindWall
                || selection == LevelGlitchSelection.ScoreLeak
                || selection == LevelGlitchSelection.LaserRain
                || selection == LevelGlitchSelection.ThinAir
                || selection == LevelGlitchSelection.PrismShuffle
                || selection == LevelGlitchSelection.CloneStatic
                || selection == LevelGlitchSelection.DropTide
                || selection == LevelGlitchSelection.BrickLock
                || selection == LevelGlitchSelection.SpeedSteps
                || selection == LevelGlitchSelection.MirrorServe
                || selection == LevelGlitchSelection.StaticJackpot
                || selection == LevelGlitchSelection.JammedRails
                || selection == LevelGlitchSelection.GravitySwap
                || selection == LevelGlitchSelection.VhsTear
                || selection == LevelGlitchSelection.CapsuleBlackout
                || selection == LevelGlitchSelection.Brickquake
                || selection == LevelGlitchSelection.TurboTax
                || selection == LevelGlitchSelection.WarpJam
                || selection == LevelGlitchSelection.NeonFlood
                || selection == LevelGlitchSelection.LockstepRows
                || selection == LevelGlitchSelection.StaticServe
                || selection == LevelGlitchSelection.MeltdownCore;
        }

        private static BreakoutWarpGateWall ResolveGateWall(DeterministicRandomService random, int index)
        {
            if (index == 0)
            {
                return BreakoutWarpGateWall.Left;
            }

            if (index == 1)
            {
                return BreakoutWarpGateWall.Right;
            }

            return (BreakoutWarpGateWall)random.Range(0, 3);
        }
    }
}
