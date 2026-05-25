using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    public sealed class BreakoutBrickEffectResolver
    {
        private const float LaserEmitterOffsetMultiplier = 0.55f;
        private const float LaserMinimumTargetHeightOffset = 0.2f;
        private const float LaserVerticalBias = 0.08f;
        private const float MinimumChainLightningStrength = 0.001f;
        private const float MinimumChainLightningRadius = 1.8f;
        private const float MaximumChainLightningRadius = 3.25f;
        private const float ChainLightningTargetScale = 4f;
        private const int MaximumChainLightningTargets = 3;

        public bool TryResolveLaserTargets(
            IReadOnlyList<Brick> bricks,
            Vector2 paddlePosition,
            float paddleHalfWidthWorld,
            out Brick leftTarget,
            out Brick rightTarget)
        {
            leftTarget = null;
            rightTarget = null;

            if (bricks == null)
            {
                return false;
            }

            var leftEmitterX = ResolveLaserEmitterPosition(paddlePosition, paddleHalfWidthWorld, true).x;
            var rightEmitterX = ResolveLaserEmitterPosition(paddlePosition, paddleHalfWidthWorld, false).x;
            leftTarget = FindBestLaserTarget(bricks, leftEmitterX, paddlePosition.y, excluded: null);
            rightTarget = FindBestLaserTarget(bricks, rightEmitterX, paddlePosition.y, leftTarget);
            return leftTarget != null || rightTarget != null;
        }

        public Vector2 ResolveLaserEmitterPosition(Vector2 paddlePosition, float paddleHalfWidthWorld, bool leftEmitter)
        {
            var offsetDirection = leftEmitter ? -1f : 1f;
            return new Vector2(
                paddlePosition.x + (paddleHalfWidthWorld * LaserEmitterOffsetMultiplier * offsetDirection),
                paddlePosition.y);
        }

        public IReadOnlyList<Brick> ResolveChainLightningTargets(
            IReadOnlyList<Brick> bricks,
            Vector2 origin,
            Brick sourceBrick,
            float chainLightningStrength)
        {
            if (bricks == null || chainLightningStrength <= MinimumChainLightningStrength)
            {
                return Array.Empty<Brick>();
            }

            var chainRadius = Mathf.Lerp(
                MinimumChainLightningRadius,
                MaximumChainLightningRadius,
                Mathf.Clamp01(chainLightningStrength));
            var chainRadiusSquared = chainRadius * chainRadius;
            var maxTargets = Mathf.Clamp(1 + Mathf.RoundToInt(chainLightningStrength * ChainLightningTargetScale), 1, MaximumChainLightningTargets);
            var candidates = new List<Brick>();

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (!IsBreakable(candidate) || candidate == sourceBrick)
                {
                    continue;
                }

                var offset = (Vector2)candidate.transform.position - origin;

                if (offset.sqrMagnitude > chainRadiusSquared)
                {
                    continue;
                }

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
            {
                return Array.Empty<Brick>();
            }

            candidates.Sort((left, right) =>
            {
                var leftDistance = ((Vector2)left.transform.position - origin).sqrMagnitude;
                var rightDistance = ((Vector2)right.transform.position - origin).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });

            if (candidates.Count > maxTargets)
            {
                candidates.RemoveRange(maxTargets, candidates.Count - maxTargets);
            }

            return candidates;
        }

        public bool TryResolveFuseBurstTarget(IReadOnlyList<Brick> bricks, out Brick target)
        {
            target = null;

            if (bricks == null)
            {
                return false;
            }

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (!IsFuseBurstCandidate(candidate))
                {
                    continue;
                }

                if (target == null || CompareFuseBurstPriority(candidate, target) < 0)
                {
                    target = candidate;
                }
            }

            return target != null;
        }

        private static Brick FindBestLaserTarget(IReadOnlyList<Brick> bricks, float beamOriginX, float paddleY, Brick excluded)
        {
            Brick bestCandidate = null;
            var bestScore = float.MaxValue;

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (!IsBreakable(candidate)
                    || candidate == excluded
                    || candidate.transform.position.y <= paddleY + LaserMinimumTargetHeightOffset)
                {
                    continue;
                }

                var score = Mathf.Abs(candidate.transform.position.x - beamOriginX)
                    + (Mathf.Abs(candidate.transform.position.y - paddleY) * LaserVerticalBias);

                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestCandidate = candidate;
            }

            return bestCandidate;
        }

        private static bool IsBreakable(Brick candidate)
        {
            return candidate != null
                && candidate.Definition != null
                && candidate.Definition.IsBreakable;
        }

        private static bool IsFuseBurstCandidate(Brick candidate)
        {
            return IsBreakable(candidate)
                && !candidate.IsPendingRemoval;
        }

        private static int CompareFuseBurstPriority(Brick left, Brick right)
        {
            if (left.IsDamaged != right.IsDamaged)
            {
                return left.IsDamaged ? -1 : 1;
            }

            var hitPointComparison = left.HitPointsRemaining.CompareTo(right.HitPointsRemaining);

            if (hitPointComparison != 0)
            {
                return hitPointComparison;
            }

            var heightComparison = right.transform.position.y.CompareTo(left.transform.position.y);

            if (heightComparison != 0)
            {
                return heightComparison;
            }

            return left.transform.position.x.CompareTo(right.transform.position.x);
        }
    }
}
