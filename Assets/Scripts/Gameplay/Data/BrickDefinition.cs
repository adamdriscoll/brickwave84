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
        [SerializeField, Range(0f, 1f)] private float dropChance = 0.15f;
        [SerializeField] private BrickPowerUpDropEntry[] dropTable = Array.Empty<BrickPowerUpDropEntry>();
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color damagedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public int HitPoints => indestructible ? 0 : Mathf.Max(1, hitPoints);

        public int ScoreValue => Mathf.Max(0, scoreValue);

        public bool IsBreakable => !indestructible;

        public bool CountsTowardLevelCompletion => !indestructible && countsTowardLevelCompletion;

        public float DropChance => Mathf.Clamp01(dropChance);

        public BrickPowerUpDropEntry[] DropTable => dropTable ?? Array.Empty<BrickPowerUpDropEntry>();

        public Color BaseColor => baseColor;

        public Color DamagedColor => damagedColor;
    }
}
