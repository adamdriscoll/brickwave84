using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutBrickLockService
    {
        private readonly IList<Brick> bricks;
        private readonly List<Brick> shieldedBricks = new List<Brick>();
        private readonly List<Brick> keyBricks = new List<Brick>();
        private bool isArmed;

        public BreakoutBrickLockService(IList<Brick> bricks)
        {
            this.bricks = bricks;
        }

        internal int ShieldedCount => shieldedBricks.Count;

        internal int KeyCount => keyBricks.Count;

        public int Arm(BreakoutBrickLockSpec spec, int currentLevelRowCount, int currentLevelColumnCount)
        {
            Clear();

            if (bricks == null || bricks.Count == 0)
            {
                return 0;
            }

            var activeRows = BuildActiveRows(currentLevelRowCount);

            if (activeRows.Count == 0)
            {
                return 0;
            }

            var lockedRow = ResolveNearestRow(activeRows, spec.LockedClusterRow, currentLevelRowCount);
            var keyRow = ResolveNearestRow(activeRows, spec.KeyClusterRow, currentLevelRowCount);

            if (activeRows.Count > 1 && keyRow == lockedRow)
            {
                keyRow = ResolveDifferentNearestRow(activeRows, spec.KeyClusterRow, currentLevelRowCount, lockedRow);
            }

            AddCluster(shieldedBricks, lockedRow, spec.LockedClusterColumn, spec.ClusterRadius, currentLevelColumnCount, null);
            AddCluster(keyBricks, keyRow, spec.KeyClusterColumn, spec.ClusterRadius, currentLevelColumnCount, shieldedBricks);

            if (shieldedBricks.Count == 0 || keyBricks.Count == 0)
            {
                Clear();
                return 0;
            }

            for (var index = 0; index < shieldedBricks.Count; index++)
            {
                shieldedBricks[index]?.SetBrickLockShielded(true);
            }

            isArmed = true;
            return shieldedBricks.Count;
        }

        public bool TryRegisterDestroyedBrick(Brick brick)
        {
            if (!isArmed || brick == null || !ContainsBrick(keyBricks, brick))
            {
                return false;
            }

            RemoveNullAndPending(keyBricks);

            if (keyBricks.Count > 0)
            {
                return false;
            }

            UnlockShieldedBricks();
            return true;
        }

        public void Clear()
        {
            UnlockShieldedBricks();
            keyBricks.Clear();
            isArmed = false;
        }

        private void UnlockShieldedBricks()
        {
            for (var index = shieldedBricks.Count - 1; index >= 0; index--)
            {
                var brick = shieldedBricks[index];

                if (brick != null)
                {
                    brick.SetBrickLockShielded(false);
                }
            }

            shieldedBricks.Clear();
        }

        private List<int> BuildActiveRows(int currentLevelRowCount)
        {
            var activeRows = new List<int>();

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (!IsEligibleBreakableBrick(brick))
                {
                    continue;
                }

                var row = brick.CaptureState().Row;

                if (!activeRows.Contains(row))
                {
                    activeRows.Add(row);
                }
            }

            if (activeRows.Count == 0 && currentLevelRowCount > 0)
            {
                activeRows.Add(Mathf.RoundToInt(Mathf.Max(0, currentLevelRowCount - 1) * 0.5f));
            }

            return activeRows;
        }

        private void AddCluster(
            List<Brick> target,
            int row,
            float normalizedColumn,
            int clusterRadius,
            int currentLevelColumnCount,
            List<Brick> excludedBricks)
        {
            var maxColumn = Mathf.Max(0, currentLevelColumnCount - 1);
            var centerColumn = Mathf.RoundToInt(Mathf.Clamp01(normalizedColumn) * maxColumn);
            var candidates = new List<Brick>();

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (!IsEligibleBreakableBrick(brick)
                    || ContainsBrick(excludedBricks, brick)
                    || brick.CaptureState().Row != row)
                {
                    continue;
                }

                candidates.Add(brick);
            }

            candidates.Sort((left, right) =>
            {
                var leftDistance = Mathf.Abs(left.CaptureState().Column - centerColumn);
                var rightDistance = Mathf.Abs(right.CaptureState().Column - centerColumn);
                return leftDistance.CompareTo(rightDistance);
            });

            var targetCount = Mathf.Clamp((clusterRadius * 2) + 1, 2, 5);

            for (var index = 0; index < candidates.Count && target.Count < targetCount; index++)
            {
                target.Add(candidates[index]);
            }
        }

        private static int ResolveNearestRow(List<int> activeRows, float normalizedRow, int currentLevelRowCount)
        {
            return ResolveDifferentNearestRow(activeRows, normalizedRow, currentLevelRowCount, excludedRow: -1);
        }

        private static int ResolveDifferentNearestRow(
            List<int> activeRows,
            float normalizedRow,
            int currentLevelRowCount,
            int excludedRow)
        {
            var maxRow = Mathf.Max(0, currentLevelRowCount - 1);

            for (var index = 0; index < activeRows.Count; index++)
            {
                maxRow = Mathf.Max(maxRow, activeRows[index]);
            }

            var targetRow = Mathf.RoundToInt(Mathf.Clamp01(normalizedRow) * maxRow);
            var bestRow = -1;
            var bestDistance = int.MaxValue;

            for (var index = 0; index < activeRows.Count; index++)
            {
                var candidate = activeRows[index];

                if (candidate == excludedRow)
                {
                    continue;
                }

                var distance = Mathf.Abs(candidate - targetRow);

                if (distance < bestDistance)
                {
                    bestRow = candidate;
                    bestDistance = distance;
                }
            }

            return bestRow >= 0 ? bestRow : activeRows[0];
        }

        private static bool IsEligibleBreakableBrick(Brick brick)
        {
            return brick != null
                && !brick.IsPendingRemoval
                && brick.Definition != null
                && brick.Definition.IsBreakable;
        }

        private static bool ContainsBrick(List<Brick> source, Brick brick)
        {
            if (source == null || brick == null)
            {
                return false;
            }

            for (var index = 0; index < source.Count; index++)
            {
                if (source[index] == brick)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveNullAndPending(List<Brick> source)
        {
            if (source == null)
            {
                return;
            }

            for (var index = source.Count - 1; index >= 0; index--)
            {
                var brick = source[index];

                if (brick == null || brick.IsPendingRemoval)
                {
                    source.RemoveAt(index);
                }
            }
        }
    }
}
