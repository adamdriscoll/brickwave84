using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutWarpGateController : MonoBehaviour
    {
        private const float TeleportCooldownSeconds = 0.18f;

        private readonly List<BreakoutWarpGatePortal> portals = new List<BreakoutWarpGatePortal>();
        private readonly Dictionary<int, float> cooldownUntilByBallId = new Dictionary<int, float>();

        private BreakoutGameController gameController;
        private Func<BreakoutWarpGateSpec, Vector2> resolvePortalPosition;
        private Func<BreakoutWarpGateSpec, Vector2> resolveExitPosition;
        private BreakoutWarpJamSpec warpJam;
        private bool isRogueGate;

        public int PortalCount => portals.Count;

        public void Configure(BreakoutGameController controller, BreakoutWarpJamSpec warpJamSpec = default)
        {
            gameController = controller;
            warpJam = warpJamSpec;
            isRogueGate = false;
            resolvePortalPosition = null;
            resolveExitPosition = null;
        }

        public void ConfigureRogueGate(
            BreakoutGameController controller,
            Func<BreakoutWarpGateSpec, Vector2> portalPositionResolver,
            Func<BreakoutWarpGateSpec, Vector2> exitPositionResolver)
        {
            gameController = controller;
            warpJam = default;
            isRogueGate = true;
            resolvePortalPosition = portalPositionResolver;
            resolveExitPosition = exitPositionResolver;
        }

        public void RegisterPortal(BreakoutWarpGatePortal portal)
        {
            if (portal != null && !portals.Contains(portal))
            {
                portals.Add(portal);
            }
        }

        public bool TryTeleport(BallController ball, BreakoutWarpGatePortal source)
        {
            if (ball == null || source == null)
            {
                return false;
            }

            if (isRogueGate)
            {
                return TryTeleportRogueGate(ball, source);
            }

            if (portals.Count < 2)
            {
                return false;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return false;
            }

            var target = ApplyWarpJam(source, SelectTargetPortal(source));

            if (target == null)
            {
                return false;
            }

            cooldownUntilByBallId[ballId] = now + TeleportCooldownSeconds;
            ball.SetWorldPosition(target.ExitPosition);
            ball.ApplyCollisionResponse(BuildExitDirection(target, ball.CurrentVelocity), 0.12f);
            return true;
        }

        private bool TryTeleportRogueGate(BallController ball, BreakoutWarpGatePortal source)
        {
            if (resolvePortalPosition == null || resolveExitPosition == null)
            {
                return false;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return false;
            }

            var nextSpec = BuildRogueGateDestination(source);
            cooldownUntilByBallId[ballId] = now + TeleportCooldownSeconds;
            ball.SetWorldPosition(resolveExitPosition(nextSpec));
            ball.ApplyCollisionResponse(BuildExitDirection(nextSpec.Wall, ball.CurrentVelocity), 0.12f);
            RelocateRogueGate(source, nextSpec);
            return true;
        }

        private BreakoutWarpGateSpec BuildRogueGateDestination(BreakoutWarpGatePortal source)
        {
            var nextWall = (BreakoutWarpGateWall)Mathf.Clamp(
                Mathf.FloorToInt(NextRandomFloat(0f, 3f)),
                0,
                2);

            if (nextWall == source.Wall)
            {
                nextWall = (BreakoutWarpGateWall)(((int)nextWall + 1) % 3);
            }

            var normalizedPosition = NextRandomFloat(0.14f, 0.86f);

            if (nextWall == source.Wall && Mathf.Abs(normalizedPosition - source.NormalizedPosition) < 0.22f)
            {
                normalizedPosition = Mathf.Repeat(normalizedPosition + 0.38f, 0.72f) + 0.14f;
            }

            return new BreakoutWarpGateSpec(nextWall, normalizedPosition);
        }

        private void RelocateRogueGate(BreakoutWarpGatePortal portal, BreakoutWarpGateSpec spec)
        {
            portal.transform.position = resolvePortalPosition(spec);
            portal.Configure(this, portal.PortalIndex, spec, resolveExitPosition(spec));

            if (portal.TryGetComponent<BoxCollider2D>(out var collider))
            {
                collider.size = spec.Wall == BreakoutWarpGateWall.Top
                    ? new Vector2(0.78f, 0.54f)
                    : new Vector2(0.54f, 0.78f);
            }

            if (portal.TryGetComponent<BreakoutWarpGateVisual>(out var visual))
            {
                visual.SetWall(spec.Wall);
            }
        }

        private BreakoutWarpGatePortal SelectTargetPortal(BreakoutWarpGatePortal source)
        {
            var candidates = new List<BreakoutWarpGatePortal>(portals.Count);

            for (var index = 0; index < portals.Count; index++)
            {
                var portal = portals[index];

                if (portal != null && portal != source && portal.Wall != source.Wall)
                {
                    candidates.Add(portal);
                }
            }

            if (candidates.Count == 0)
            {
                for (var index = 0; index < portals.Count; index++)
                {
                    var portal = portals[index];

                    if (portal != null && portal != source)
                    {
                        candidates.Add(portal);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            var randomIndex = Mathf.Clamp(
                Mathf.FloorToInt(NextRandomFloat(0f, candidates.Count)),
                0,
                candidates.Count - 1);
            return candidates[randomIndex];
        }

        private BreakoutWarpGatePortal ApplyWarpJam(BreakoutWarpGatePortal source, BreakoutWarpGatePortal linkedTarget)
        {
            if (linkedTarget == null || warpJam.WrongExitChance <= 0f)
            {
                return linkedTarget;
            }

            var sourceIndex = portals.IndexOf(source);
            var linkedTargetIndex = portals.IndexOf(linkedTarget);
            var jammedIndex = ResolveWarpJamPortalIndex(
                sourceIndex,
                linkedTargetIndex,
                portals.Count,
                warpJam.WrongExitChance,
                NextRandomFloat(0f, 1f),
                NextRandomFloat(0f, 1f));

            return jammedIndex >= 0 && jammedIndex < portals.Count && portals[jammedIndex] != null
                ? portals[jammedIndex]
                : linkedTarget;
        }

        internal static int ResolveWarpJamPortalIndex(
            int sourceIndex,
            int linkedTargetIndex,
            int portalCount,
            float wrongExitChance,
            float chanceRoll,
            float candidateRoll)
        {
            if (portalCount < 3
                || sourceIndex < 0
                || sourceIndex >= portalCount
                || linkedTargetIndex < 0
                || linkedTargetIndex >= portalCount
                || Mathf.Clamp01(chanceRoll) > Mathf.Clamp01(wrongExitChance))
            {
                return linkedTargetIndex;
            }

            var candidateCount = portalCount - 2;
            var selectedCandidate = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(candidateRoll) * candidateCount),
                0,
                candidateCount - 1);
            var candidateIndex = 0;

            for (var index = 0; index < portalCount; index++)
            {
                if (index == sourceIndex || index == linkedTargetIndex)
                {
                    continue;
                }

                if (candidateIndex == selectedCandidate)
                {
                    return index;
                }

                candidateIndex++;
            }

            return linkedTargetIndex;
        }

        private Vector2 BuildExitDirection(BreakoutWarpGatePortal target, Vector2 incomingVelocity)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var tangentNoise = NextRandomFloat(-0.24f, 0.24f);

            return BuildExitDirection(target.Wall, incomingDirection, tangentNoise);
        }

        private Vector2 BuildExitDirection(BreakoutWarpGateWall wall, Vector2 incomingVelocity)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var tangentNoise = NextRandomFloat(-0.24f, 0.24f);
            return BuildExitDirection(wall, incomingDirection, tangentNoise);
        }

        private static Vector2 BuildExitDirection(BreakoutWarpGateWall wall, Vector2 incomingDirection, float tangentNoise)
        {
            return wall switch
            {
                BreakoutWarpGateWall.Left => new Vector2(1f, incomingDirection.y + tangentNoise).normalized,
                BreakoutWarpGateWall.Right => new Vector2(-1f, incomingDirection.y + tangentNoise).normalized,
                BreakoutWarpGateWall.Top => new Vector2(incomingDirection.x + tangentNoise, -1f).normalized,
                _ => incomingDirection,
            };
        }

        private float NextRandomFloat(float minInclusive, float maxInclusive)
        {
            return gameController != null
                ? gameController.NextGameplayRandomFloat(minInclusive, maxInclusive)
                : UnityEngine.Random.Range(minInclusive, maxInclusive);
        }
    }
}
