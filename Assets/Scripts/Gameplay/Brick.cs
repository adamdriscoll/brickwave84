using UnityEngine;

namespace GetBricked.Gameplay
{
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;

        public void Initialize(BreakoutGameController controller)
        {
            gameController = controller;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<BallController>(out _))
            {
                return;
            }

            gameController.HandleBrickDestroyed(this);
        }
    }
}
