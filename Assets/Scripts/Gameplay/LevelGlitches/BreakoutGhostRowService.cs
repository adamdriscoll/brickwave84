using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutGhostRowService
    {
        private readonly IList<Brick> bricks;
        private BreakoutGhostRowSpec activeSpec;
        private bool isArmed;
        private bool isPhased;
        private int targetRowIndex = -1;
        private float phaseTimer;
        private float cooldownTimer;

        public BreakoutGhostRowService(IList<Brick> bricks)
        {
            this.bricks = bricks;
        }

        public void Arm(BreakoutGhostRowSpec spec, int currentLevelRowCount)
        {
            activeSpec = spec;
            targetRowIndex = ResolveTargetRow(spec, currentLevelRowCount);
            phaseTimer = 0f;
            cooldownTimer = 0f;
            isPhased = false;
            isArmed = targetRowIndex >= 0;
        }

        public bool TryTriggerFromHit(Brick brick)
        {
            if (!isArmed
                || isPhased
                || cooldownTimer > 0f
                || brick == null
                || brick.Definition == null)
            {
                return false;
            }

            if (brick.CaptureState().Row != targetRowIndex)
            {
                return false;
            }

            return PhaseTargetRow();
        }

        public void Update(float deltaTime)
        {
            if (!isArmed)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer = Mathf.Max(0f, cooldownTimer - Mathf.Max(0f, deltaTime));
            }

            if (!isPhased)
            {
                return;
            }

            phaseTimer = Mathf.Max(0f, phaseTimer - Mathf.Max(0f, deltaTime));

            if (phaseTimer > 0f)
            {
                return;
            }

            SetTargetRowGhosted(false);
            isPhased = false;
            cooldownTimer = activeSpec.CooldownSeconds;
        }

        public void Clear()
        {
            SetTargetRowGhosted(false);
            isArmed = false;
            isPhased = false;
            targetRowIndex = -1;
            phaseTimer = 0f;
            cooldownTimer = 0f;
        }

        private bool PhaseTargetRow()
        {
            var affectedCount = SetTargetRowGhosted(true);

            if (affectedCount <= 0)
            {
                return false;
            }

            isPhased = true;
            phaseTimer = activeSpec.PhaseDurationSeconds;
            return true;
        }

        private int SetTargetRowGhosted(bool ghosted)
        {
            if (bricks == null || targetRowIndex < 0)
            {
                return 0;
            }

            var affectedCount = 0;

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

                if (brick.CaptureState().Row != targetRowIndex)
                {
                    continue;
                }

                brick.SetGhosted(ghosted, activeSpec.HiddenAlpha);
                affectedCount++;
            }

            return affectedCount;
        }

        private int ResolveTargetRow(BreakoutGhostRowSpec spec, int currentLevelRowCount)
        {
            if (bricks == null || bricks.Count == 0)
            {
                return currentLevelRowCount > 0
                    ? Mathf.RoundToInt(spec.NormalizedRow * Mathf.Max(0, currentLevelRowCount - 1))
                    : -1;
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

                var row = brick.CaptureState().Row;
                maxRow = Mathf.Max(maxRow, row);

                if (!activeRows.Contains(row))
                {
                    activeRows.Add(row);
                }
            }

            if (activeRows.Count == 0)
            {
                return -1;
            }

            var targetRow = Mathf.RoundToInt(spec.NormalizedRow * maxRow);
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

            return bestRow;
        }
    }
}
