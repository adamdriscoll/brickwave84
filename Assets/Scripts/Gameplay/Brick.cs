using UnityEngine;

namespace GetBricked.Gameplay
{
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private bool breaksOnBallHit;

        public int ScoreValue { get; private set; }

        public bool CountsTowardLevelCompletion { get; private set; }

        public void Initialize(
            BreakoutGameController controller,
            int scoreValue,
            bool countsTowardLevelCompletion,
            bool destroyOnBallHit)
        {
            gameController = controller;
            ScoreValue = Mathf.Max(0, scoreValue);
            CountsTowardLevelCompletion = countsTowardLevelCompletion;
            breaksOnBallHit = destroyOnBallHit;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!breaksOnBallHit || !collision.collider.TryGetComponent<BallController>(out _))
            {
                return;
            }

            gameController.HandleBrickDestroyed(this);
        }
    }
}
