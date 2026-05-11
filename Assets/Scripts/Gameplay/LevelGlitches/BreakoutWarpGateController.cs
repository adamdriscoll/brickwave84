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

        public int PortalCount => portals.Count;

        public void Configure(BreakoutGameController controller)
        {
            gameController = controller;
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
            if (ball == null || source == null || portals.Count < 2)
            {
                return false;
            }

            var ballId = ball.GetInstanceID();
            var now = Time.time;

            if (cooldownUntilByBallId.TryGetValue(ballId, out var cooldownUntil) && now < cooldownUntil)
            {
                return false;
            }

            var target = SelectTargetPortal(source);

            if (target == null)
            {
                return false;
            }

            cooldownUntilByBallId[ballId] = now + TeleportCooldownSeconds;
            ball.SetWorldPosition(target.ExitPosition);
            ball.ApplyCollisionResponse(BuildExitDirection(target, ball.CurrentVelocity), 0.12f);
            return true;
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

        private Vector2 BuildExitDirection(BreakoutWarpGatePortal target, Vector2 incomingVelocity)
        {
            var incomingDirection = incomingVelocity.sqrMagnitude > 0.001f
                ? incomingVelocity.normalized
                : Vector2.up;
            var tangentNoise = NextRandomFloat(-0.24f, 0.24f);

            return target.Wall switch
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
                : Random.Range(minInclusive, maxInclusive);
        }
    }
}
