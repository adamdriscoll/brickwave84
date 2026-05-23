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
        MirrorGrid = 3,
        GravityPocket = 4,
        TokenStorm = 5,
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
            BreakoutGravityPocketSpec gravityPocket = default)
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

        public bool IsActive => GlitchType != BreakoutLevelGlitchType.None;
    }

    internal static class BreakoutLevelGlitchPlanner
    {
        public const int TurboRailLadderUnlockIntensity = 31;
        public const int MirrorGridLadderUnlockIntensity = 37;
        public const int TokenStormLadderUnlockIntensity = 40;
        public const int GravityPocketLadderUnlockIntensity = 41;

        private const float WarpGateScoreMultiplier = 1.35f;
        private const float TurboRailScoreMultiplier = 1.25f;
        private const float MirrorGridScoreMultiplier = 1.3f;
        private const float TokenStormScoreMultiplier = 1.32f;
        private const float GravityPocketScoreMultiplier = 1.38f;

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

            if (settings.IsRogueMode && BreakoutRunProgression.IsFinalStage(levelIndex))
            {
                return BuildSelectedGlitchPlan(random, settings, settings.SelectedLevelGlitch);
            }

            if (settings.ForceLevelGlitchRoll || IsForcedLevelGlitchSelection(settings.SelectedLevelGlitch))
            {
                return BuildSelectedGlitchPlan(random, settings, settings.SelectedLevelGlitch);
            }

            var chance = GetGlitchChance(settings, levelIndex);

            if (chance <= 0f || random.NextFloat() > chance)
            {
                return BreakoutLevelGlitchPlan.None;
            }

            return BuildSelectedGlitchPlan(random, settings, settings.SelectedLevelGlitch);
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

            return BuildWarpGatePlan(random, definition.Rarity);
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

        public static float GetGlitchChance(RunSettings settings, int levelIndex)
        {
            if (settings == null)
            {
                return 0f;
            }

            if (settings.IsRogueMode && !HasUnlockedRogueGlitch(settings.RogueIntensity))
            {
                return 0f;
            }

            var levelPressure = Mathf.Clamp01(levelIndex / 9f);
            float baseChance;

            if (settings.IsRogueMode)
            {
                var intensity = BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity);

                if (BreakoutRunProgression.IsFinalStage(levelIndex))
                {
                    return 1f;
                }

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
            var totalWeight = 0f;

            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                if (IsGlitchUnlockedForSettings(GlitchDefinitions[index], settings))
                {
                    totalWeight += BreakoutRarityRules.GetDropWeightMultiplier(GlitchDefinitions[index].Rarity);
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

                if (!IsGlitchUnlockedForSettings(definition, settings))
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
                    || BreakoutRunProgression.ClampRogueIntensity(settings.RogueIntensity) >= definition.LadderUnlockIntensity);
        }

        private static bool HasUnlockedRogueGlitch(int intensity)
        {
            var clampedIntensity = BreakoutRunProgression.ClampRogueIntensity(intensity);

            for (var index = 0; index < GlitchDefinitions.Length; index++)
            {
                if (clampedIntensity >= GlitchDefinitions[index].LadderUnlockIntensity)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsForcedLevelGlitchSelection(LevelGlitchSelection selection)
        {
            return selection == LevelGlitchSelection.WarpGates
                || selection == LevelGlitchSelection.TurboRail
                || selection == LevelGlitchSelection.MirrorGrid
                || selection == LevelGlitchSelection.GravityPocket
                || selection == LevelGlitchSelection.TokenStorm;
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
