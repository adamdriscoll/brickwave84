using System;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutLevelGlitchType
    {
        None = 0,
        WarpGates = 1,
        TurboRail = 2,
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

    internal sealed class BreakoutLevelGlitchPlan
    {
        public static readonly BreakoutLevelGlitchPlan None = new BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType.None,
            string.Empty,
            string.Empty,
            1f,
            Array.Empty<BreakoutWarpGateSpec>(),
            default);

        public BreakoutLevelGlitchPlan(
            BreakoutLevelGlitchType glitchType,
            string displayName,
            string hudLabel,
            float scoreMultiplier,
            BreakoutWarpGateSpec[] warpGates,
            BreakoutTurboRailSpec turboRail)
        {
            GlitchType = glitchType;
            DisplayName = displayName ?? string.Empty;
            HudLabel = hudLabel ?? string.Empty;
            ScoreMultiplier = Mathf.Max(1f, scoreMultiplier);
            WarpGates = warpGates ?? Array.Empty<BreakoutWarpGateSpec>();
            TurboRail = turboRail;
        }

        public BreakoutLevelGlitchType GlitchType { get; }

        public string DisplayName { get; }

        public string HudLabel { get; }

        public float ScoreMultiplier { get; }

        public BreakoutWarpGateSpec[] WarpGates { get; }

        public BreakoutTurboRailSpec TurboRail { get; }

        public bool IsActive => GlitchType != BreakoutLevelGlitchType.None;
    }

    internal static class BreakoutLevelGlitchPlanner
    {
        private const float WarpGateScoreMultiplier = 1.35f;
        private const float TurboRailScoreMultiplier = 1.25f;

        public static BreakoutLevelGlitchPlan BuildPlan(
            DeterministicRandomService random,
            RunSettings settings,
            int levelIndex)
        {
            if (settings == null || random == null)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            if (IsForcedLevelGlitchSelection(settings.SelectedLevelGlitch))
            {
                return BuildSelectedGlitchPlan(random, settings.SelectedLevelGlitch);
            }

            var chance = GetGlitchChance(settings, levelIndex);

            if (chance <= 0f || random.NextFloat() > chance)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            return BuildSelectedGlitchPlan(random, settings.SelectedLevelGlitch);
        }

        private static BreakoutLevelGlitchPlan BuildSelectedGlitchPlan(
            DeterministicRandomService random,
            LevelGlitchSelection selection)
        {
            if (ShouldBuildTurboRail(random, selection))
            {
                return BuildTurboRailPlan(random);
            }

            return BuildWarpGatePlan(random);
        }

        private static BreakoutLevelGlitchPlan BuildWarpGatePlan(DeterministicRandomService random)
        {
            var gateCount = random.Range(2, 5);
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.WarpGates,
                "Warp Gates",
                $"Warp Gates x{WarpGateScoreMultiplier:0.00}",
                WarpGateScoreMultiplier,
                BuildWarpGates(random, gateCount),
                default);
        }

        private static BreakoutLevelGlitchPlan BuildTurboRailPlan(DeterministicRandomService random)
        {
            return new BreakoutLevelGlitchPlan(
                BreakoutLevelGlitchType.TurboRail,
                "Turbo Rail",
                $"Turbo Rail x{TurboRailScoreMultiplier:0.00}",
                TurboRailScoreMultiplier,
                Array.Empty<BreakoutWarpGateSpec>(),
                BuildTurboRail(random));
        }

        public static float GetGlitchChance(RunSettings settings, int levelIndex)
        {
            if (settings == null)
            {
                return 0f;
            }

            var levelPressure = Mathf.Clamp01(levelIndex / 9f);
            float baseChance;

            if (settings.IsRogueMode)
            {
                var intensity = BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity);

                if (intensity < 8)
                {
                    return 0f;
                }

                baseChance = Mathf.Lerp(0.12f, 0.58f, Mathf.InverseLerp(8f, BreakoutRunProgression.MaxRogueIntensity, intensity));
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

        private static bool ShouldBuildTurboRail(DeterministicRandomService random, LevelGlitchSelection selection)
        {
            return selection switch
            {
                LevelGlitchSelection.TurboRail => true,
                LevelGlitchSelection.WarpGates => false,
                _ => random.Range(0, 2) == 1,
            };
        }

        private static bool IsForcedLevelGlitchSelection(LevelGlitchSelection selection)
        {
            return selection == LevelGlitchSelection.WarpGates
                || selection == LevelGlitchSelection.TurboRail;
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
