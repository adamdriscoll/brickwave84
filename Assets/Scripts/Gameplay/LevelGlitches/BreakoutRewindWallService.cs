using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutRewindWallService
    {
        private readonly IList<Brick> bricks;
        private BreakoutRewindWallSpec activeSpec;
        private BreakoutBrickState[] rewindStates = System.Array.Empty<BreakoutBrickState>();
        private bool isArmed;
        private bool hasRebuilt;
        private bool isWaitingToRebuild;
        private bool hasShownWarning;
        private int targetRowIndex = -1;
        private float rebuildTimer;

        public BreakoutRewindWallService(IList<Brick> bricks)
        {
            this.bricks = bricks;
        }

        public int Arm(BreakoutRewindWallSpec spec, int currentLevelRowCount)
        {
            Clear();
            activeSpec = spec;

            if (!TryResolveTargetRow(spec, currentLevelRowCount, out targetRowIndex))
            {
                return 0;
            }

            var states = new List<BreakoutBrickState>();
            var removedObjectiveCount = 0;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (!IsTargetBreakableBrick(brick, targetRowIndex, includePendingRemoval: false))
                {
                    continue;
                }

                var state = brick.CaptureState();
                states.Add(new BreakoutBrickState(
                    state.Definition,
                    state.Position,
                    state.Row,
                    state.Column,
                    state.HitPointsRemaining,
                    state.MotionConfig,
                    countsTowardLevelCompletion: false));

                if (brick.CountsTowardLevelCompletion)
                {
                    removedObjectiveCount++;
                }

                brick.SetCountsTowardLevelCompletionOverride(false);
            }

            if (states.Count == 0)
            {
                targetRowIndex = -1;
                return 0;
            }

            rewindStates = states.ToArray();
            isArmed = true;
            return removedObjectiveCount;
        }

        public bool TryRegisterDestroyedBrick(Brick brick)
        {
            if (!isArmed
                || hasRebuilt
                || isWaitingToRebuild
                || !IsTargetBreakableBrick(brick, targetRowIndex, includePendingRemoval: true))
            {
                return false;
            }

            if (HasRemainingTargetBreakableBricks())
            {
                return false;
            }

            isWaitingToRebuild = true;
            hasShownWarning = false;
            rebuildTimer = activeSpec.RebuildDelaySeconds;
            return true;
        }

        public bool Update(float deltaTime, out bool showWarning, out BreakoutBrickState[] rebuildBrickStates)
        {
            showWarning = false;
            rebuildBrickStates = null;

            if (!isArmed || !isWaitingToRebuild || hasRebuilt)
            {
                return false;
            }

            rebuildTimer = Mathf.Max(0f, rebuildTimer - Mathf.Max(0f, deltaTime));

            if (!hasShownWarning && rebuildTimer <= activeSpec.WarningSeconds)
            {
                hasShownWarning = true;
                showWarning = true;
            }

            if (rebuildTimer > 0f)
            {
                return false;
            }

            hasRebuilt = true;
            isWaitingToRebuild = false;
            rebuildBrickStates = rewindStates;
            return rebuildBrickStates != null && rebuildBrickStates.Length > 0;
        }

        public void Clear()
        {
            isArmed = false;
            hasRebuilt = false;
            isWaitingToRebuild = false;
            hasShownWarning = false;
            targetRowIndex = -1;
            rebuildTimer = 0f;
            activeSpec = default;
            rewindStates = System.Array.Empty<BreakoutBrickState>();
        }

        private bool HasRemainingTargetBreakableBricks()
        {
            if (bricks == null || targetRowIndex < 0)
            {
                return false;
            }

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (IsTargetBreakableBrick(brick, targetRowIndex, includePendingRemoval: false))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveTargetRow(BreakoutRewindWallSpec spec, int currentLevelRowCount, out int rowIndex)
        {
            rowIndex = -1;

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

                if (brick.IsPendingRemoval || brick.Definition == null || !brick.Definition.IsBreakable)
                {
                    continue;
                }

                var row = brick.CaptureState().Row;
                maxRow = Mathf.Max(maxRow, row);

                if (!activeRows.Contains(row))
                {
                    activeRows.Add(row);
                }
            }

            if (activeRows.Count == 0)
            {
                return false;
            }

            var targetRow = Mathf.RoundToInt(spec.NormalizedRow * maxRow);
            rowIndex = activeRows[0];
            var bestDistance = Mathf.Abs(rowIndex - targetRow);

            for (var index = 1; index < activeRows.Count; index++)
            {
                var candidate = activeRows[index];
                var distance = Mathf.Abs(candidate - targetRow);

                if (distance < bestDistance)
                {
                    rowIndex = candidate;
                    bestDistance = distance;
                }
            }

            return rowIndex >= 0;
        }

        private static bool IsTargetBreakableBrick(Brick brick, int rowIndex, bool includePendingRemoval)
        {
            return brick != null
                && rowIndex >= 0
                && (includePendingRemoval || !brick.IsPendingRemoval)
                && brick.Definition != null
                && brick.Definition.IsBreakable
                && brick.CaptureState().Row == rowIndex;
        }
    }
}
