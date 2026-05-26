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
            BreakoutLevelGlitchType[] activeGlitchTypes = null,
            BreakoutRowRewriteSpec rowRewrite = default,
            BreakoutPrismLaneSpec[] prismLanes = null,
            BreakoutSwitchbackRailSpec switchbackRails = default,
            BreakoutDriftRowsSpec driftRows = default,
            BreakoutHotCornersSpec hotCorners = default,
            BreakoutFlickerBricksSpec flickerBricks = default)
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
            RowRewrite = rowRewrite;
            PrismLanes = prismLanes ?? Array.Empty<BreakoutPrismLaneSpec>();
            SwitchbackRails = switchbackRails;
            DriftRows = driftRows;
            HotCorners = hotCorners;
            FlickerBricks = flickerBricks;
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

        public BreakoutRowRewriteSpec RowRewrite { get; }

        public BreakoutPrismLaneSpec[] PrismLanes { get; }

        public BreakoutSwitchbackRailSpec SwitchbackRails { get; }

        public BreakoutDriftRowsSpec DriftRows { get; }

        public BreakoutHotCornersSpec HotCorners { get; }

        public BreakoutFlickerBricksSpec FlickerBricks { get; }

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
                selectedTypes.Add(definition.GlitchType);
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
            var rowRewrite = default(BreakoutRowRewriteSpec);
            var prismLanes = Array.Empty<BreakoutPrismLaneSpec>();
            var switchbackRails = default(BreakoutSwitchbackRailSpec);
            var driftRows = default(BreakoutDriftRowsSpec);
            var hotCorners = default(BreakoutHotCornersSpec);
            var flickerBricks = default(BreakoutFlickerBricksSpec);

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

                if (plan.HasGlitch(BreakoutLevelGlitchType.StaticWall))
                {
                    staticWall = plan.StaticWall;
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
                activeTypes,
                rowRewrite,
                prismLanes,
                switchbackRails,
                driftRows,
                hotCorners,
                flickerBricks);
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
                || selection == LevelGlitchSelection.FlickerBricks;
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
