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
        private int maxHitPoints;
        private int hitPointsRemaining;
        private Color themedBaseColor;
        private Color themedDamagedColor;

        public BrickDefinition Definition => definition;

        public int ScoreValue => definition == null ? 0 : definition.ScoreValue;

        public bool CountsTowardLevelCompletion => definition != null && definition.CountsTowardLevelCompletion;

        public void Initialize(BreakoutGameController controller, BrickDefinition brickDefinition, int effectiveHitPoints, ThemeVisualStyle visualStyle)
        {
            gameController = controller;
            definition = brickDefinition;
            spriteRenderer = GetComponent<SpriteRenderer>();
            maxHitPoints = definition != null && definition.IsBreakable
                ? Mathf.Max(1, effectiveHitPoints)
                : 0;
            hitPointsRemaining = maxHitPoints;
            ApplyTheme(visualStyle);
        }

        public void ApplyTheme(ThemeVisualStyle visualStyle)
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = visualStyle.Sprite;
            themedBaseColor = visualStyle.PrimaryColor;
            themedDamagedColor = visualStyle.SecondaryColor;
            RefreshVisual();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null || !collision.collider.TryGetComponent<BallController>(out var scoringBall))
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
                gameController.HandleBrickDestroyed(this, scoringBall);
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

            if (!definition.IsBreakable || maxHitPoints <= 1)
            {
                spriteRenderer.color = themedBaseColor;
                return;
            }

            var integrity = Mathf.InverseLerp(1f, maxHitPoints, hitPointsRemaining);
            spriteRenderer.color = Color.Lerp(themedDamagedColor, themedBaseColor, integrity);
        }
    }
}
