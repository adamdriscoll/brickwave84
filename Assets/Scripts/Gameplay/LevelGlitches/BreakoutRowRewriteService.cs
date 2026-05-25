using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutRowRewriteService
    {
        private readonly IList<Brick> bricks;
        private readonly IReadOnlyList<BrickDefinition> brickDefinitions;

        public BreakoutRowRewriteService(IList<Brick> bricks, IReadOnlyList<BrickDefinition> brickDefinitions)
        {
            this.bricks = bricks;
            this.brickDefinitions = brickDefinitions;
        }

        public bool TryBuildRewrite(
            BreakoutRowRewriteSpec spec,
            int currentLevelRowCount,
            int currentLevelColumnCount,
            out int rowIndex,
            out BreakoutProceduralBrickCell[] rowCells)
        {
            rowIndex = 0;
            rowCells = null;

            if (!TryResolveTargetRow(spec, currentLevelRowCount, out rowIndex))
            {
                return false;
            }

            rowCells = BuildCells(spec, rowIndex, ResolveActiveColumnCount(currentLevelColumnCount));
            return rowCells.Length > 0;
        }

        private bool TryResolveTargetRow(BreakoutRowRewriteSpec spec, int currentLevelRowCount, out int rowIndex)
        {
            rowIndex = 0;

            if (bricks == null || bricks.Count == 0)
            {
                return false;
            }

            var activeRows = new List<int>();
            var maxRow = Mathf.Max(0, currentLevelRowCount - 1);

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (brick.IsPendingRemoval || brick.Definition == null)
                {
                    continue;
                }

                var state = brick.CaptureState();
                maxRow = Mathf.Max(maxRow, state.Row);

                if (!activeRows.Contains(state.Row))
                {
                    activeRows.Add(state.Row);
                }
            }

            if (activeRows.Count == 0)
            {
                return false;
            }

            var targetRow = Mathf.RoundToInt(Mathf.Clamp01(spec.NormalizedRow) * maxRow);
            var bestRow = activeRows[0];
            var bestDistance = Mathf.Abs(bestRow - targetRow);

            for (var index = 1; index < activeRows.Count; index++)
            {
                var candidate = activeRows[index];
                var distance = Mathf.Abs(candidate - targetRow);

                if (distance < bestDistance)
                {
                    bestRow = candidate;
                    bestDistance = distance;
                }
            }

            rowIndex = bestRow;
            return true;
        }

        private int ResolveActiveColumnCount(int currentLevelColumnCount)
        {
            var columnCount = Mathf.Max(1, currentLevelColumnCount);

            if (bricks == null)
            {
                return columnCount;
            }

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (brick.Definition == null)
                {
                    continue;
                }

                columnCount = Mathf.Max(columnCount, brick.CaptureState().Column + 1);
            }

            return Mathf.Clamp(columnCount, 1, 12);
        }

        private BreakoutProceduralBrickCell[] BuildCells(BreakoutRowRewriteSpec spec, int rowIndex, int columnCount)
        {
            columnCount = Mathf.Clamp(columnCount, 1, 12);
            var cells = new BreakoutProceduralBrickCell[columnCount];
            var random = new DeterministicRandomService(DeterministicRandomService.CombineSeed(spec.PatternSeed, rowIndex + 1));
            var requiredCount = 0;

            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var checkerBias = ((rowIndex + columnIndex) & 1) == 0 ? 0.08f : -0.06f;

                if (random.NextFloat() > Mathf.Clamp01(spec.FillChance + checkerBias))
                {
                    continue;
                }

                var definition = PickBrickDefinition(random, rowIndex, columnIndex, columnCount);

                if (definition == null)
                {
                    continue;
                }

                cells[columnIndex] = new BreakoutProceduralBrickCell(definition, default);

                if (definition.CountsTowardLevelCompletion)
                {
                    requiredCount++;
                }
            }

            EnsureRequiredBricks(cells, random, columnCount, requiredCount);
            return cells;
        }

        private void EnsureRequiredBricks(
            BreakoutProceduralBrickCell[] cells,
            DeterministicRandomService random,
            int columnCount,
            int requiredCount)
        {
            var minimumRequired = Mathf.Clamp(columnCount / 3, 2, 4);

            for (var attempts = 0; requiredCount < minimumRequired && attempts < columnCount * 2; attempts++)
            {
                var columnIndex = attempts < columnCount
                    ? (columnCount / 2 + attempts) % columnCount
                    : random.Range(0, columnCount);

                if (cells[columnIndex] != null)
                {
                    continue;
                }

                var definition = PickFallbackBrickDefinition();

                if (definition == null)
                {
                    return;
                }

                cells[columnIndex] = new BreakoutProceduralBrickCell(definition, default);
                requiredCount++;
            }
        }

        private BrickDefinition PickBrickDefinition(
            DeterministicRandomService random,
            int rowIndex,
            int columnIndex,
            int columnCount)
        {
            if (brickDefinitions == null || brickDefinitions.Count == 0)
            {
                return null;
            }

            var totalWeight = 0f;

            for (var index = 0; index < brickDefinitions.Count; index++)
            {
                totalWeight += GetBrickWeight(brickDefinitions[index], rowIndex, columnIndex, columnCount);
            }

            if (totalWeight <= 0f)
            {
                return PickFallbackBrickDefinition();
            }

            var roll = random.Range(0f, totalWeight);

            for (var index = 0; index < brickDefinitions.Count; index++)
            {
                var definition = brickDefinitions[index];
                roll -= GetBrickWeight(definition, rowIndex, columnIndex, columnCount);

                if (roll <= 0f)
                {
                    return definition;
                }
            }

            return PickFallbackBrickDefinition();
        }

        private static float GetBrickWeight(BrickDefinition definition, int rowIndex, int columnIndex, int columnCount)
        {
            if (definition == null)
            {
                return 0f;
            }

            var centerBias = columnCount <= 1
                ? 1f
                : 1f - Mathf.Abs((((float)columnIndex / (columnCount - 1f)) * 2f) - 1f);

            if (!definition.IsBreakable)
            {
                return 0.12f + (centerBias * 0.08f);
            }

            if (definition.IsExplosive || definition.SplitsOnBreak)
            {
                return 0.28f + (centerBias * 0.16f);
            }

            if (definition.SpinsOnHit || definition.JellyOnHit)
            {
                return 0.36f + (centerBias * 0.12f);
            }

            if (definition.HitPoints >= 3)
            {
                return 0.45f + ((rowIndex % 2) * 0.08f);
            }

            if (definition.HitPoints == 2)
            {
                return 0.95f + (centerBias * 0.18f);
            }

            return definition.SizeMultiplier <= 0.55f ? 0.34f : 3.4f;
        }

        private BrickDefinition PickFallbackBrickDefinition()
        {
            if (brickDefinitions == null)
            {
                return null;
            }

            for (var index = 0; index < brickDefinitions.Count; index++)
            {
                var definition = brickDefinitions[index];

                if (definition != null
                    && definition.CountsTowardLevelCompletion
                    && definition.HitPoints <= 1
                    && definition.SizeMultiplier >= 0.95f)
                {
                    return definition;
                }
            }

            for (var index = 0; index < brickDefinitions.Count; index++)
            {
                var definition = brickDefinitions[index];

                if (definition != null && definition.CountsTowardLevelCompletion)
                {
                    return definition;
                }
            }

            return brickDefinitions.Count > 0 ? brickDefinitions[0] : null;
        }
    }
}
