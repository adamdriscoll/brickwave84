using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutLevelLayoutPlan
    {
        public string DisplayName = string.Empty;
        public string[] LayoutRows = Array.Empty<string>();
        public BreakoutProceduralBrickCell[][] BrickRows = Array.Empty<BreakoutProceduralBrickCell[]>();
        public LevelCompletionRule CompletionRule = LevelCompletionRule.ClearRequiredBricks;
        public int TargetScore;
        public float BallSpeedMultiplier = 1f;
        public float PaddleSpeedMultiplier = 1f;
        public float TopInset = 1.5f;
        public bool MirrorLayout;
        public int[] RowShifts = Array.Empty<int>();
        public string PatternLabel = "Procedural";
        public int UniqueBrickTypeCount;
        public int AvailableDropTypeCount;
        public int MovingBrickCount;
        public BreakoutLevelGlitchPlan GlitchPlan = BreakoutLevelGlitchPlan.None;
        public string VariationSummary = "Variation: unavailable";
    }

    internal sealed class BreakoutProceduralBrickCell
    {
        public BreakoutProceduralBrickCell(BrickDefinition definition, BreakoutBrickMotionConfig motionConfig)
        {
            Definition = definition;
            MotionConfig = motionConfig;
        }

        public BrickDefinition Definition { get; }

        public BreakoutBrickMotionConfig MotionConfig { get; }
    }

    internal readonly struct BreakoutBrickMotionConfig
    {
        public BreakoutBrickMotionConfig(float speed, Vector2 initialDirection)
        {
            Speed = Mathf.Max(0f, speed);
            InitialDirection = initialDirection.sqrMagnitude > 0.001f
                ? initialDirection.normalized
                : Vector2.zero;
        }

        public float Speed { get; }

        public Vector2 InitialDirection { get; }

        public bool IsEnabled => Speed > 0.01f && InitialDirection.sqrMagnitude > 0.001f;
    }

    internal sealed class BreakoutLevelPlanner
    {
        private enum ProceduralPatternType
        {
            Bands = 0,
            Diamond = 1,
            Steps = 2,
            Lattice = 3,
            Core = 4,
            Columns = 5,
        }

        private readonly IReadOnlyList<LevelDefinition> loadedLevels;
        private readonly IReadOnlyList<BrickDefinition> loadedBrickDefinitions;
        private readonly Func<int> seedGenerator;

        public BreakoutLevelPlanner(
            IReadOnlyList<LevelDefinition> loadedLevels,
            IReadOnlyList<BrickDefinition> loadedBrickDefinitions,
            Func<int> seedGenerator)
        {
            this.loadedLevels = loadedLevels ?? Array.Empty<LevelDefinition>();
            this.loadedBrickDefinitions = loadedBrickDefinitions ?? Array.Empty<BrickDefinition>();
            this.seedGenerator = seedGenerator ?? throw new ArgumentNullException(nameof(seedGenerator));
        }

        public BreakoutLevelLayoutPlan BuildPlan(
            LevelDefinition level,
            int levelIndex,
            DeterministicRandomService gameplayRandom,
            RunSettings activeRunSettings)
        {
            var plan = new BreakoutLevelLayoutPlan();

            if (level == null)
            {
                return plan;
            }

            var templateCount = Mathf.Max(1, loadedLevels.Count);
            var profileIndex = Mathf.Abs(levelIndex % templateCount);
            var cycleIndex = levelIndex / templateCount;
            var levelProgress = BreakoutRunProgression.GetLevelProgress(levelIndex);
            var isBrutalRun = activeRunSettings != null && activeRunSettings.DifficultyPreset == RunDifficultyPreset.Brutal;
            var planner = gameplayRandom != null
                ? gameplayRandom.Fork((levelIndex + 1) * 7919)
                : new DeterministicRandomService(seedGenerator());
            var pattern = (ProceduralPatternType)planner.Range(0, Enum.GetValues(typeof(ProceduralPatternType)).Length);
            var minimumRowsForProgress = 3 + Mathf.FloorToInt(levelProgress * 4.01f);
            var minimumColumnsForProgress = 8 + Mathf.FloorToInt(levelProgress * 3.01f);
            var rowCount = Mathf.Clamp(
                Mathf.Max(minimumRowsForProgress, Mathf.Max(3, level.LayoutRows.Length) + (profileIndex >= 2 ? 1 : 0)),
                3,
                7);
            var columnCount = Mathf.Clamp(
                Mathf.Max(minimumColumnsForProgress, GetTemplateColumnCount(level) + (profileIndex >= templateCount - 1 ? 1 : 0)),
                8,
                11);

            plan.DisplayName = BuildProceduralLevelDisplayName(level, pattern, cycleIndex);
            plan.LayoutRows = new string[rowCount];
            plan.BrickRows = new BreakoutProceduralBrickCell[rowCount][];
            plan.RowShifts = new int[rowCount];
            plan.PatternLabel = GetProceduralPatternLabel(pattern);
            plan.MirrorLayout = planner.NextFloat() < 0.82f;
            plan.TopInset = Mathf.Max(1f, level.TopInset - (levelProgress * 0.22f));
            plan.PaddleSpeedMultiplier = Mathf.Lerp(1f, 1.12f, levelProgress)
                + (Mathf.Max(0f, level.PaddleSpeedMultiplier - 1f) * 0.2f);
            plan.BallSpeedMultiplier = Mathf.Lerp(1f, 1.26f, levelProgress)
                + (Mathf.Max(0f, level.BallSpeedMultiplier - 1f) * 0.1f);

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var cells = new BreakoutProceduralBrickCell[columnCount];
                var symbols = new char[columnCount];

                for (var fillIndex = 0; fillIndex < symbols.Length; fillIndex++)
                {
                    symbols[fillIndex] = '.';
                }

                var leftSideLength = (columnCount + 1) / 2;
                var generatedColumns = plan.MirrorLayout ? leftSideLength : columnCount;

                for (var columnIndex = 0; columnIndex < generatedColumns; columnIndex++)
                {
                    if (!ShouldPlaceProceduralBrick(planner, pattern, rowIndex, columnIndex, rowCount, columnCount, levelIndex))
                    {
                        continue;
                    }

                    var definition = SelectProceduralBrickDefinition(planner, rowIndex, columnIndex, rowCount, columnCount, levelIndex, isBrutalRun);

                    if (definition == null)
                    {
                        continue;
                    }

                    var motionConfig = ResolveProceduralBrickMotion(
                        planner,
                        definition,
                        profileIndex,
                        cycleIndex,
                        rowIndex,
                        columnIndex,
                        rowCount,
                        columnCount);
                    FillProceduralCell(cells, symbols, columnIndex, definition, motionConfig);

                    if (plan.MirrorLayout)
                    {
                        var mirroredColumn = (columnCount - 1) - columnIndex;
                        FillProceduralCell(cells, symbols, mirroredColumn, definition, motionConfig);
                    }
                }

                EnsureProceduralRowHasBricks(cells, symbols, planner, rowIndex, rowCount, columnCount, levelIndex, profileIndex, cycleIndex, isBrutalRun);
                var rowShift = BuildLevelPlanRowShift(planner, rowIndex, levelIndex, columnCount, CountOccupiedCells(symbols));
                plan.RowShifts[rowIndex] = rowShift;
                cells = RotateCells(cells, rowShift);
                symbols = RotateCharacters(symbols, rowShift);
                plan.BrickRows[rowIndex] = cells;
                plan.LayoutRows[rowIndex] = new string(symbols);
            }

            if (isBrutalRun && levelIndex == 0)
            {
                EnsureOpeningBrutalVariety(plan, planner, profileIndex, cycleIndex, rowCount, columnCount);
            }

            RecalculatePlanStats(plan);
            plan.GlitchPlan = BreakoutLevelGlitchPlanner.BuildPlan(planner.Fork(15485863), activeRunSettings, levelIndex);
            plan.CompletionRule = LevelCompletionRule.ClearRequiredBricks;
            plan.TargetScore = 0;
            plan.VariationSummary = BuildVariationSummary(plan);

            Debug.Log(
                $"Level variation | seed {activeRunSettings?.Seed ?? 0} | level {levelIndex + 1} | " +
                $"{plan.DisplayName} | {plan.VariationSummary}");
            return plan;
        }

        private int GetTemplateColumnCount(LevelDefinition level)
        {
            if (level == null)
            {
                return 8;
            }

            var layoutRows = level.LayoutRows;
            var maxColumns = 8;

            for (var index = 0; index < layoutRows.Length; index++)
            {
                maxColumns = Mathf.Max(maxColumns, string.IsNullOrEmpty(layoutRows[index]) ? 0 : layoutRows[index].Length);
            }

            return Mathf.Clamp(maxColumns, 8, 10);
        }

        private static string BuildProceduralLevelDisplayName(LevelDefinition level, ProceduralPatternType pattern, int cycleIndex)
        {
            var baseName = level == null ? "Procedural Layout" : level.DisplayName;
            var patternLabel = GetProceduralPatternLabel(pattern);
            return cycleIndex <= 0
                ? $"{baseName} [{patternLabel}]"
                : $"{baseName} [{patternLabel}] Loop {cycleIndex + 1}";
        }

        private static string GetProceduralPatternLabel(ProceduralPatternType pattern)
        {
            return pattern switch
            {
                ProceduralPatternType.Bands => "Bands",
                ProceduralPatternType.Diamond => "Diamond",
                ProceduralPatternType.Steps => "Steps",
                ProceduralPatternType.Lattice => "Lattice",
                ProceduralPatternType.Core => "Core",
                ProceduralPatternType.Columns => "Columns",
                _ => "Procedural",
            };
        }

        private static bool ShouldPlaceProceduralBrick(
            DeterministicRandomService planner,
            ProceduralPatternType pattern,
            int row,
            int column,
            int totalRows,
            int totalColumns,
            int levelIndex)
        {
            var topBias = totalRows <= 1 ? 1f : 1f - ((float)row / (totalRows - 1f));
            var normalizedColumn = totalColumns <= 1 ? 0.5f : (float)column / (totalColumns - 1f);
            var centerBias = 1f - Mathf.Abs((normalizedColumn * 2f) - 1f);
            var noise = planner.Range(-0.18f, 0.18f);
            var threshold = 0.48f - Mathf.Min(0.1f, levelIndex * 0.015f);
            var score = BuildProceduralPatternScore(pattern, row, column, totalRows, totalColumns, topBias, centerBias, levelIndex);
            return score + noise >= threshold;
        }

        private static float BuildProceduralPatternScore(
            ProceduralPatternType pattern,
            int row,
            int column,
            int totalRows,
            int totalColumns,
            float topBias,
            float centerBias,
            int levelIndex)
        {
            var centeredRow = totalRows <= 1 ? 0f : Mathf.Abs((((float)row / (totalRows - 1f)) * 2f) - 1f);
            var stepBias = column <= Mathf.Max(1, totalColumns - 2 - row) ? 0.2f : -0.12f;

            return pattern switch
            {
                ProceduralPatternType.Bands => 0.32f + (topBias * 0.48f) + (centerBias * 0.14f) + (((row + levelIndex) & 1) == 0 ? 0.08f : -0.05f),
                ProceduralPatternType.Diamond => 0.22f + (centerBias * 0.42f) + (topBias * 0.33f) - (centeredRow * 0.22f),
                ProceduralPatternType.Steps => 0.26f + (topBias * 0.4f) + stepBias + (centerBias * 0.08f),
                ProceduralPatternType.Lattice => 0.28f + (topBias * 0.3f) + ((((row + column) & 1) == 0) ? 0.2f : -0.18f) + (centerBias * 0.1f),
                ProceduralPatternType.Core => 0.2f + (centerBias * 0.38f) + (topBias * 0.24f) + (row == 0 || row == totalRows - 1 ? 0.12f : 0f),
                ProceduralPatternType.Columns => 0.24f + (topBias * 0.36f) + (((column + levelIndex) % 3) == 0 ? 0.24f : -0.05f) + (centerBias * 0.1f),
                _ => 0.4f,
            };
        }

        private BrickDefinition SelectProceduralBrickDefinition(
            DeterministicRandomService planner,
            int row,
            int column,
            int totalRows,
            int totalColumns,
            int levelIndex,
            bool isBrutalRun)
        {
            if (loadedBrickDefinitions.Count == 0)
            {
                return null;
            }

            var totalWeight = 0f;

            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                totalWeight += GetProceduralBrickWeight(loadedBrickDefinitions[index], row, column, totalRows, totalColumns, levelIndex, isBrutalRun);
            }

            if (totalWeight <= 0f)
            {
                return FindFallbackBasicBrick();
            }

            var roll = planner.Range(0f, totalWeight);

            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];
                roll -= GetProceduralBrickWeight(definition, row, column, totalRows, totalColumns, levelIndex, isBrutalRun);

                if (roll <= 0f)
                {
                    return definition;
                }
            }

            return FindFallbackBasicBrick();
        }

        private static float GetProceduralBrickWeight(
            BrickDefinition definition,
            int row,
            int column,
            int totalRows,
            int totalColumns,
            int levelIndex,
            bool isBrutalRun)
        {
            if (definition == null)
            {
                return 0f;
            }

            var topBias = totalRows <= 1 ? 1f : 1f - ((float)row / (totalRows - 1f));
            var normalizedColumn = totalColumns <= 1 ? 0.5f : (float)column / (totalColumns - 1f);
            var centerBias = 1f - Mathf.Abs((normalizedColumn * 2f) - 1f);
            var hotspot = topBias > 0.45f && centerBias > 0.4f;
            var isTinyBrick = definition.SizeMultiplier <= 0.55f;

            if (!definition.IsBreakable)
            {
                if (!isBrutalRun && levelIndex < 3)
                {
                    return 0f;
                }

                return hotspot
                    ? 0.2f + (Mathf.Min(6, levelIndex) * 0.025f)
                    : 0.08f + (Mathf.Min(6, levelIndex) * 0.018f);
            }

            float weight;

            if (definition.IsExplosive)
            {
                if (!isBrutalRun && levelIndex < 5)
                {
                    return 0f;
                }

                weight = 0.2f + (topBias * 0.16f) + (hotspot ? 0.14f : 0f) + ((levelIndex - 5) * 0.055f);
            }
            else if (definition.SplitsOnBreak)
            {
                if (!isBrutalRun && levelIndex < 2)
                {
                    return 0f;
                }

                weight = 0.42f + (topBias * 0.2f) + (centerBias * 0.18f) + (hotspot ? 0.12f : 0f) + ((levelIndex - 2) * 0.04f);
            }
            else if (definition.SpinsOnHit)
            {
                if (!isBrutalRun && levelIndex < 3)
                {
                    return 0f;
                }

                weight = 0.36f + (topBias * 0.26f) + (centerBias * 0.14f) + (hotspot ? 0.16f : 0f) + ((levelIndex - 3) * 0.05f);
            }
            else if (definition.JellyOnHit)
            {
                if (!isBrutalRun && levelIndex < 3)
                {
                    return 0f;
                }

                weight = 0.52f + (topBias * 0.22f) + (centerBias * 0.16f) + (hotspot ? 0.12f : 0f) + ((levelIndex - 3) * 0.045f);
            }
            else if (definition.HitPoints >= 3)
            {
                if (!isBrutalRun && levelIndex < 4)
                {
                    return 0f;
                }

                weight = 0.48f + (topBias * 0.38f) + (hotspot ? 0.18f : 0f) + ((levelIndex - 4) * 0.045f);
            }
            else if (definition.HitPoints == 2)
            {
                if (!isBrutalRun && levelIndex < 1)
                {
                    return 0f;
                }

                weight = 1.15f + (topBias * 0.5f) + (centerBias * 0.14f) + ((levelIndex - 1) * 0.055f);
            }
            else
            {
                weight = 4f + ((1f - topBias) * 0.75f);
            }

            if (!isTinyBrick)
            {
                return Mathf.Max(0f, weight);
            }

            if (!isBrutalRun && levelIndex < 2)
            {
                return 0f;
            }

            weight = (weight * 0.18f)
                + (centerBias * 0.22f)
                + (hotspot ? 0.16f : 0f)
                + Mathf.Min(0.14f, levelIndex * 0.022f);
            return Mathf.Max(0f, weight);
        }

        private BrickDefinition FindFallbackBasicBrick()
        {
            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition != null
                    && definition.IsBreakable
                    && !definition.IsExplosive
                    && definition.HitPoints <= 1
                    && definition.SizeMultiplier >= 0.95f)
                {
                    return definition;
                }
            }

            for (var index = 0; index < loadedBrickDefinitions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition != null && definition.IsBreakable)
                {
                    return definition;
                }
            }

            return loadedBrickDefinitions.Count > 0 ? loadedBrickDefinitions[0] : null;
        }

        private static BreakoutBrickMotionConfig ResolveProceduralBrickMotion(
            DeterministicRandomService planner,
            BrickDefinition definition,
            int profileIndex,
            int cycleIndex,
            int row,
            int column,
            int totalRows,
            int totalColumns)
        {
            if (definition == null || !definition.IsBreakable || definition.SpinsOnHit)
            {
                return default;
            }

            var motionProfileIndex = Mathf.Max(profileIndex, Mathf.Min(3, cycleIndex));

            if (motionProfileIndex <= 0)
            {
                return default;
            }

            var motionChance = motionProfileIndex switch
            {
                1 => definition.HitPoints >= 2 ? 0.14f : 0.05f,
                2 => definition.HitPoints >= 2 || definition.IsExplosive ? 0.24f : 0.1f,
                _ => definition.HitPoints >= 2 || definition.IsExplosive ? 0.3f : 0.16f,
            };

            motionChance += cycleIndex * 0.035f;

            if (planner.NextFloat() > motionChance)
            {
                return default;
            }

            var speed = 1.55f + (motionProfileIndex * 0.22f) + (cycleIndex * 0.14f);
            Vector2 direction;

            switch (motionProfileIndex)
            {
                case 1:
                    direction = (row & 1) == 0 ? Vector2.right : Vector2.left;
                    break;
                case 2:
                    direction = ((row + column) & 1) == 0
                        ? ResolveBaseBrickMovementDirection(BrickMovementDirection.Down)
                        : ResolveBaseBrickMovementDirection(BrickMovementDirection.Right);
                    break;
                default:
                    direction = ResolveCenterRelativeMovementDirection(
                        row,
                        column,
                        totalColumns,
                        totalRows,
                        clockwise: ((row + column + cycleIndex) & 1) == 0,
                        inward: false,
                        tangential: true);
                    break;
            }

            return new BreakoutBrickMotionConfig(speed, direction);
        }

        private void EnsureProceduralRowHasBricks(
            BreakoutProceduralBrickCell[] cells,
            char[] symbols,
            DeterministicRandomService planner,
            int row,
            int totalRows,
            int totalColumns,
            int levelIndex,
            int profileIndex,
            int cycleIndex,
            bool isBrutalRun)
        {
            var occupiedCells = CountOccupiedCells(symbols);
            var minimumBricks = row == totalRows - 1 ? 2 : Mathf.Clamp(totalColumns / 3, 3, 4);

            if (occupiedCells >= minimumBricks)
            {
                return;
            }

            var centerLeft = Mathf.Max(0, (totalColumns / 2) - 1);
            var centerRight = Mathf.Min(totalColumns - 1, totalColumns / 2);
            var preferredColumns = new[]
            {
                centerLeft,
                centerRight,
                Mathf.Max(0, centerLeft - 1),
                Mathf.Min(totalColumns - 1, centerRight + 1),
                0,
                totalColumns - 1,
            };

            for (var index = 0; index < preferredColumns.Length && occupiedCells < minimumBricks; index++)
            {
                var column = preferredColumns[index];

                if (column < 0 || column >= totalColumns || cells[column] != null)
                {
                    continue;
                }

                var definition = SelectProceduralBrickDefinition(planner, row, column, totalRows, totalColumns, levelIndex, isBrutalRun) ?? FindFallbackBasicBrick();
                var motionConfig = ResolveProceduralBrickMotion(planner, definition, profileIndex, cycleIndex, row, column, totalRows, totalColumns);
                FillProceduralCell(cells, symbols, column, definition, motionConfig);
                occupiedCells++;
            }
        }

        private void EnsureOpeningBrutalVariety(
            BreakoutLevelLayoutPlan plan,
            DeterministicRandomService planner,
            int profileIndex,
            int cycleIndex,
            int totalRows,
            int totalColumns)
        {
            if (plan == null || plan.BrickRows == null || plan.BrickRows.Length == 0 || loadedBrickDefinitions.Count == 0)
            {
                return;
            }

            var candidatePositions = new List<Vector2Int>(totalRows * totalColumns);

            for (var rowIndex = 0; rowIndex < plan.BrickRows.Length; rowIndex++)
            {
                var row = plan.BrickRows[rowIndex];

                if (row == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    if (row[columnIndex] != null)
                    {
                        candidatePositions.Add(new Vector2Int(columnIndex, rowIndex));
                    }
                }
            }

            if (candidatePositions.Count == 0)
            {
                for (var rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < totalColumns; columnIndex++)
                    {
                        candidatePositions.Add(new Vector2Int(columnIndex, rowIndex));
                    }
                }
            }

            plan.MirrorLayout = false;

            for (var index = 0; index < loadedBrickDefinitions.Count && index < candidatePositions.Count; index++)
            {
                var definition = loadedBrickDefinitions[index];

                if (definition == null)
                {
                    continue;
                }

                var position = candidatePositions[index];
                var motionConfig = ResolveProceduralBrickMotion(
                    planner,
                    definition,
                    profileIndex,
                    cycleIndex,
                    position.y,
                    position.x,
                    totalRows,
                    totalColumns);
                SetPlanCell(plan, position.y, position.x, definition, motionConfig);
            }
        }

        private static void SetPlanCell(
            BreakoutLevelLayoutPlan plan,
            int rowIndex,
            int columnIndex,
            BrickDefinition definition,
            BreakoutBrickMotionConfig motionConfig)
        {
            if (plan == null
                || rowIndex < 0
                || rowIndex >= plan.BrickRows.Length
                || rowIndex >= plan.LayoutRows.Length
                || definition == null)
            {
                return;
            }

            var row = plan.BrickRows[rowIndex];

            if (row == null || columnIndex < 0 || columnIndex >= row.Length)
            {
                return;
            }

            row[columnIndex] = new BreakoutProceduralBrickCell(definition, motionConfig);

            var symbols = (plan.LayoutRows[rowIndex] ?? string.Empty).PadRight(row.Length, '.').ToCharArray();
            symbols[columnIndex] = motionConfig.IsEnabled
                ? char.ToLowerInvariant(BuildProceduralBrickSymbol(definition))
                : BuildProceduralBrickSymbol(definition);
            plan.LayoutRows[rowIndex] = new string(symbols);
        }

        private static int RecalculatePlanStats(BreakoutLevelLayoutPlan plan)
        {
            if (plan == null || plan.BrickRows == null)
            {
                return 0;
            }

            var totalBreakableScore = 0;
            var usedDefinitions = new HashSet<BrickDefinition>();
            var usedDrops = new HashSet<PowerUpDefinition>();

            plan.MovingBrickCount = 0;

            for (var rowIndex = 0; rowIndex < plan.BrickRows.Length; rowIndex++)
            {
                var row = plan.BrickRows[rowIndex];

                if (row == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    var cell = row[columnIndex];

                    if (cell == null || cell.Definition == null)
                    {
                        continue;
                    }

                    usedDefinitions.Add(cell.Definition);

                    if (cell.MotionConfig.IsEnabled)
                    {
                        plan.MovingBrickCount++;
                    }

                    if (!cell.Definition.IsBreakable)
                    {
                        continue;
                    }

                    totalBreakableScore += Mathf.Max(0, cell.Definition.ScoreValue);
                    var dropTable = cell.Definition.DropTable;

                    for (var dropIndex = 0; dropIndex < dropTable.Length; dropIndex++)
                    {
                        if (dropTable[dropIndex].PowerUpDefinition != null)
                        {
                            usedDrops.Add(dropTable[dropIndex].PowerUpDefinition);
                        }
                    }
                }
            }

            plan.UniqueBrickTypeCount = usedDefinitions.Count;
            plan.AvailableDropTypeCount = usedDrops.Count;
            return totalBreakableScore;
        }

        private static void FillProceduralCell(
            BreakoutProceduralBrickCell[] cells,
            char[] symbols,
            int column,
            BrickDefinition definition,
            BreakoutBrickMotionConfig motionConfig)
        {
            if (cells == null || symbols == null || definition == null || column < 0 || column >= cells.Length || column >= symbols.Length)
            {
                return;
            }

            cells[column] = new BreakoutProceduralBrickCell(definition, motionConfig);
            var symbol = BuildProceduralBrickSymbol(definition);
            symbols[column] = motionConfig.IsEnabled ? char.ToLowerInvariant(symbol) : symbol;
        }

        private static BreakoutProceduralBrickCell[] RotateCells(BreakoutProceduralBrickCell[] rowCells, int shift)
        {
            if (rowCells == null || rowCells.Length == 0 || shift == 0)
            {
                return rowCells ?? Array.Empty<BreakoutProceduralBrickCell>();
            }

            var length = rowCells.Length;
            var wrappedShift = ((shift % length) + length) % length;

            if (wrappedShift == 0)
            {
                return rowCells;
            }

            var rotated = new BreakoutProceduralBrickCell[length];

            for (var index = 0; index < length; index++)
            {
                rotated[(index + wrappedShift) % length] = rowCells[index];
            }

            return rotated;
        }

        private static char[] RotateCharacters(char[] rowCharacters, int shift)
        {
            if (rowCharacters == null || rowCharacters.Length == 0 || shift == 0)
            {
                return rowCharacters ?? Array.Empty<char>();
            }

            return RotateRow(new string(rowCharacters), shift).ToCharArray();
        }

        private static int BuildLevelPlanRowShift(DeterministicRandomService planner, int rowIndex, int levelIndex, int totalColumns, int occupiedCells)
        {
            var maxShift = occupiedCells >= 3 ? Mathf.Min(2, Mathf.Max(1, totalColumns / 5)) : 0;

            if (maxShift <= 0)
            {
                return 0;
            }

            var baseShift = planner.Range(-maxShift, maxShift + 1);

            if (levelIndex <= 0)
            {
                return Mathf.Clamp(baseShift, -1, 1);
            }

            return rowIndex == 0 ? 0 : baseShift;
        }

        private static int CountOccupiedCells(char[] rowCharacters)
        {
            if (rowCharacters == null || rowCharacters.Length == 0)
            {
                return 0;
            }

            var occupiedCells = 0;

            for (var index = 0; index < rowCharacters.Length; index++)
            {
                if (rowCharacters[index] != '.' && !char.IsWhiteSpace(rowCharacters[index]))
                {
                    occupiedCells++;
                }
            }

            return occupiedCells;
        }

        private static char BuildProceduralBrickSymbol(BrickDefinition definition)
        {
            if (definition == null)
            {
                return '.';
            }

            if (!definition.IsBreakable)
            {
                return 'S';
            }

            if (definition.IsExplosive)
            {
                return 'E';
            }

            if (definition.SplitsOnBreak)
            {
                return 'X';
            }

            if (definition.SpinsOnHit)
            {
                return 'R';
            }

            if (definition.JellyOnHit)
            {
                return 'J';
            }

            return definition.HitPoints switch
            {
                <= 1 => 'A',
                2 => 'B',
                _ => 'C',
            };
        }

        private static Vector2 ResolveCenterRelativeMovementDirection(
            int row,
            int column,
            int rowLength,
            int totalRows,
            bool clockwise,
            bool inward,
            bool tangential = false)
        {
            var center = new Vector2((Mathf.Max(1, rowLength) - 1f) * 0.5f, (Mathf.Max(1, totalRows) - 1f) * 0.5f);
            var offset = new Vector2(column - center.x, center.y - row);

            if (offset.sqrMagnitude <= 0.0001f)
            {
                return clockwise ? Vector2.right : Vector2.up;
            }

            if (tangential)
            {
                return new Vector2(-offset.y, offset.x).normalized;
            }

            if (clockwise)
            {
                return new Vector2(offset.y, -offset.x).normalized;
            }

            return inward ? -offset.normalized : offset.normalized;
        }

        private static Vector2 ResolveBaseBrickMovementDirection(BrickMovementDirection direction)
        {
            return direction switch
            {
                BrickMovementDirection.Left => Vector2.left,
                BrickMovementDirection.Right => Vector2.right,
                BrickMovementDirection.Up => Vector2.up,
                BrickMovementDirection.Down => Vector2.down,
                BrickMovementDirection.UpLeft => new Vector2(-1f, 1f).normalized,
                BrickMovementDirection.UpRight => new Vector2(1f, 1f).normalized,
                BrickMovementDirection.DownLeft => new Vector2(-1f, -1f).normalized,
                BrickMovementDirection.DownRight => new Vector2(1f, -1f).normalized,
                _ => Vector2.right,
            };
        }

        private static string RotateRow(string rowLayout, int shift)
        {
            if (string.IsNullOrEmpty(rowLayout) || shift == 0)
            {
                return rowLayout ?? string.Empty;
            }

            var length = rowLayout.Length;
            var wrappedShift = ((shift % length) + length) % length;

            if (wrappedShift == 0)
            {
                return rowLayout;
            }

            return rowLayout.Substring(length - wrappedShift, wrappedShift) + rowLayout.Substring(0, length - wrappedShift);
        }

        private static string BuildVariationSummary(BreakoutLevelLayoutPlan plan)
        {
            if (plan == null || plan.LayoutRows.Length == 0)
            {
                return "Variation: unavailable";
            }

            var shiftedRows = 0;

            for (var index = 0; index < plan.RowShifts.Length; index++)
            {
                if (plan.RowShifts[index] != 0)
                {
                    shiftedRows++;
                }
            }

            var orientationLabel = plan.MirrorLayout ? "mirrored" : "asymmetric";
            var glitchLabel = plan.GlitchPlan != null && plan.GlitchPlan.IsActive
                ? $", glitch {plan.GlitchPlan.DisplayName}"
                : string.Empty;
            return
                $"Variation: {plan.PatternLabel}, {orientationLabel}, {shiftedRows} shifted rows, " +
                $"{plan.UniqueBrickTypeCount} brick types, {plan.AvailableDropTypeCount} drops, {plan.MovingBrickCount} movers{glitchLabel}";
        }
    }
}
