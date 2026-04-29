using UnityEngine;

namespace GetBricked.Gameplay.Data
{
    [CreateAssetMenu(menuName = "Get Bricked/Brick Definition", fileName = "BrickDefinition")]
    public sealed class BrickDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Brick";
        [SerializeField, Min(1)] private int hitPoints = 1;
        [SerializeField, Min(0)] private int scoreValue = 100;
        [SerializeField] private bool indestructible;
        [SerializeField] private bool countsTowardLevelCompletion = true;
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color damagedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        public int HitPoints => indestructible ? 0 : Mathf.Max(1, hitPoints);

        public int ScoreValue => Mathf.Max(0, scoreValue);

        public bool IsBreakable => !indestructible;

        public bool CountsTowardLevelCompletion => !indestructible && countsTowardLevelCompletion;

        public Color BaseColor => baseColor;

        public Color DamagedColor => damagedColor;
    }
}
