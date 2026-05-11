using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutWarpGatePortal : MonoBehaviour
    {
        private BreakoutWarpGateController controller;

        public int PortalIndex { get; private set; }

        public BreakoutWarpGateWall Wall { get; private set; }

        public Vector2 ExitPosition { get; private set; }

        public void Configure(
            BreakoutWarpGateController warpGateController,
            int portalIndex,
            BreakoutWarpGateWall wall,
            Vector2 exitPosition)
        {
            controller = warpGateController;
            PortalIndex = Mathf.Max(0, portalIndex);
            Wall = wall;
            ExitPosition = exitPosition;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (controller == null || other == null)
            {
                return;
            }

            if (other.TryGetComponent<BallController>(out var ball))
            {
                controller.TryTeleport(ball, this);
            }
        }
    }
}
