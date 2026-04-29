using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Brick : MonoBehaviour
    {
        private BreakoutGameController gameController;
        private BrickDefinition definition;
        private SpriteRenderer spriteRenderer;
        private int hitPointsRemaining;

        public BrickDefinition Definition => definition;

        public int ScoreValue => definition == null ? 0 : definition.ScoreValue;

        public bool CountsTowardLevelCompletion => definition != null && definition.CountsTowardLevelCompletion;

        public void Initialize(BreakoutGameController controller, BrickDefinition brickDefinition)
        {
            gameController = controller;
            definition = brickDefinition;
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitPointsRemaining = definition != null && definition.IsBreakable ? definition.HitPoints : 0;
            RefreshVisual();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null || !collision.collider.TryGetComponent<BallController>(out _))
            {
                return;
            }

            if (!definition.IsBreakable)
            {
                return;
            }

            hitPointsRemaining = Mathf.Max(0, hitPointsRemaining - 1);

            if (hitPointsRemaining <= 0)
            {
                gameController.HandleBrickDestroyed(this);
                return;
            }

            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (definition == null || spriteRenderer == null)
            {
                return;
            }

            if (!definition.IsBreakable || definition.HitPoints <= 1)
            {
                spriteRenderer.color = definition.BaseColor;
                return;
            }

            var integrity = Mathf.InverseLerp(1f, definition.HitPoints, hitPointsRemaining);
            spriteRenderer.color = Color.Lerp(definition.DamagedColor, definition.BaseColor, integrity);
        }
    }
}
