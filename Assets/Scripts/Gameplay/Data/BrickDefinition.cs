using System;
using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    [Serializable]
    public struct BrickPowerUpDropEntry
    {
        [SerializeField] private PowerUpDefinition powerUpDefinition;
        [SerializeField, Min(0f)] private float weight;

        public PowerUpDefinition PowerUpDefinition => powerUpDefinition;

        public float Weight => Mathf.Max(0f, weight);
    }

    [CreateAssetMenu(menuName = "Get Bricked/Brick Definition", fileName = "BrickDefinition")]
    public sealed class BrickDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Brick";
        [SerializeField, Min(1)] private int hitPoints = 1;
        [SerializeField, Min(0)] private int scoreValue = 100;
        [SerializeField] private bool indestructible;
        [SerializeField] private bool countsTowardLevelCompletion = true;
        [SerializeField] private bool explosive;
        [SerializeField, Min(0.5f)] private float explosionRadius = 1.5f;
        [SerializeField, Min(1f)] private float explosionSpeedMultiplier = 2.2f;
        [SerializeField, Min(0.1f)] private float explosionSpeedDuration = 2.75f;
        [SerializeField] private bool spinsOnHit;
        [SerializeField, Min(0f)] private float spinTorqueImpulse = 140f;
        [SerializeField, Min(0f)] private float spinMaxAngularVelocity = 360f;
        [SerializeField, Min(0f)] private float spinAngularDamping = 2.4f;
        [SerializeField, Range(0f, 2f)] private float spinBounceStrength = 0.75f;
        [SerializeField, Range(0f, 1f)] private float dropChance = 0.15f;
        [SerializeField] private BrickPowerUpDropEntry[] dropTable = Array.Empty<BrickPowerUpDropEntry>();
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color damagedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private string spriteResourcePath = string.Empty;
        [SerializeField] private ThemeVisualSlot themeSlot = ThemeVisualSlot.Auto;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public int HitPoints => indestructible ? 0 : Mathf.Max(1, hitPoints);

        public int ScoreValue => Mathf.Max(0, scoreValue);

        public bool IsBreakable => !indestructible;

        public bool CountsTowardLevelCompletion => !indestructible && countsTowardLevelCompletion;

        public bool IsExplosive => IsBreakable && explosive;

        public float ExplosionRadius => IsExplosive ? Mathf.Max(0.5f, explosionRadius) : 0f;

        public float ExplosionSpeedMultiplier => IsExplosive ? Mathf.Max(1f, explosionSpeedMultiplier) : 1f;

        public float ExplosionSpeedDuration => IsExplosive ? Mathf.Max(0.1f, explosionSpeedDuration) : 0f;

        public bool SpinsOnHit => spinsOnHit;

        public float SpinTorqueImpulse => SpinsOnHit ? Mathf.Max(0f, spinTorqueImpulse) : 0f;

        public float SpinMaxAngularVelocity => SpinsOnHit ? Mathf.Max(1f, spinMaxAngularVelocity) : 0f;

        public float SpinAngularDamping => SpinsOnHit ? Mathf.Max(0f, spinAngularDamping) : 0f;

        public float SpinBounceStrength => SpinsOnHit ? Mathf.Clamp(spinBounceStrength, 0f, 2f) : 0f;

        public float DropChance => Mathf.Clamp01(dropChance);

        public BrickPowerUpDropEntry[] DropTable => dropTable ?? Array.Empty<BrickPowerUpDropEntry>();

        public Color BaseColor => baseColor;

        public Color DamagedColor => damagedColor;

        public string ResolveSpriteResourcePath()
        {
            return string.IsNullOrWhiteSpace(spriteResourcePath)
                ? string.Empty
                : spriteResourcePath.Trim();
        }

        public ThemeVisualSlot ResolveThemeSlot()
        {
            if (themeSlot != ThemeVisualSlot.Auto)
            {
                return themeSlot;
            }

            if (!IsBreakable)
            {
                return ThemeVisualSlot.BrickObstacle;
            }

            return HitPoints switch
            {
                1 => ThemeVisualSlot.BrickPrimary,
                2 => ThemeVisualSlot.BrickSecondary,
                _ => ThemeVisualSlot.BrickTertiary,
            };
        }
    }
}
