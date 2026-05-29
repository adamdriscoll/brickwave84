using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutScoreContext
    {
        public BreakoutScoreContext(int activeBallCount, float displayedBallSpeed, float baseBallSpeed, float currentTimeSeconds, float scoreMultiplier = 1f)
        {
            ActiveBallCount = Mathf.Max(0, activeBallCount);
            DisplayedBallSpeed = displayedBallSpeed;
            BaseBallSpeed = baseBallSpeed;
            CurrentTimeSeconds = currentTimeSeconds;
            ScoreMultiplier = Mathf.Max(0.1f, scoreMultiplier);
        }

        public int ActiveBallCount { get; }

        public float DisplayedBallSpeed { get; }

        public float BaseBallSpeed { get; }

        public float CurrentTimeSeconds { get; }

        public float ScoreMultiplier { get; }
    }

    internal readonly struct BrickScoreAward
    {
        public BrickScoreAward(int basePoints, int bonusPoints, string bonusLabel, int slamChainCount = 0)
        {
            BasePoints = basePoints;
            BonusPoints = bonusPoints;
            BonusLabel = bonusLabel ?? string.Empty;
            SlamChainCount = Mathf.Max(0, slamChainCount);
        }

        public int BasePoints { get; }

        public int BonusPoints { get; }

        public int TotalPoints => BasePoints + BonusPoints;

        public string BonusLabel { get; }

        public int SlamChainCount { get; }
    }

    internal sealed class BreakoutScoreService : IBreakoutScoreService
    {
        private const float SlamChainWindowSeconds = 1.1f;
        private const float PartySplitWindowSeconds = 0.75f;
        private const float FloatingScoreLifetimeSeconds = 0.9f;
        private const float FloatingScoreTravelDistance = 0.55f;

        private readonly List<FloatingScorePopup> floatingScorePopups = new List<FloatingScorePopup>();
        private float lastScoredBrickTime = float.NegativeInfinity;
        private int slamChainCount;
        private BallController lastScoringBall;

        public BrickScoreAward BuildBrickScoreAward(Brick brick, BallController scoringBall, BrickDestructionCause destructionCause, BreakoutScoreContext context)
        {
            if (brick == null)
            {
                return default;
            }

            var baseScore = brick.ScoreValue;

            if (baseScore <= 0)
            {
                return default;
            }

            var scoringSpeed = scoringBall != null && scoringBall.CurrentSpeed > 0.01f
                ? scoringBall.CurrentSpeed
                : context.DisplayedBallSpeed;
            var awardedBasePoints = Mathf.Max(1, Mathf.RoundToInt(baseScore * GetScoreMultiplierForSpeed(scoringSpeed, context.BaseBallSpeed)));
            var projectedSlamChainCount = GetProjectedSlamChainCount(context.CurrentTimeSeconds);
            var comboBonuses = new List<ScoreComboBonus>(3);
            TryAddScoreMultiplierBonus(awardedBasePoints, context.ScoreMultiplier, comboBonuses);
            TryAddSlamChainBonus(awardedBasePoints, projectedSlamChainCount, comboBonuses);
            TryAddBankShotBonus(awardedBasePoints, scoringBall, destructionCause, comboBonuses);
            TryAddPartySplitBonus(awardedBasePoints, scoringBall, context, comboBonuses);

            var bonusPoints = 0;
            var bonusLabels = string.Empty;

            for (var index = 0; index < comboBonuses.Count; index++)
            {
                bonusPoints += comboBonuses[index].Points;
                bonusLabels = string.IsNullOrEmpty(bonusLabels)
                    ? comboBonuses[index].Label
                    : $"{bonusLabels} + {comboBonuses[index].Label}";
            }

            return new BrickScoreAward(awardedBasePoints, bonusPoints, bonusLabels, projectedSlamChainCount);
        }

        public float GetScoreMultiplierForSpeed(float speed, float baseBallSpeed)
        {
            return Mathf.Max(0.1f, speed / Mathf.Max(0.1f, baseBallSpeed));
        }

        public void RegisterBrickScoreEvent(BallController scoringBall, bool awardedPoints, float currentTimeSeconds)
        {
            if (!awardedPoints)
            {
                return;
            }

            slamChainCount = GetProjectedSlamChainCount(currentTimeSeconds);
            lastScoredBrickTime = currentTimeSeconds;
            lastScoringBall = scoringBall;
            scoringBall?.RegisterBrickScore();
        }

        public void ResetComboTracking(bool clearPopups)
        {
            lastScoredBrickTime = float.NegativeInfinity;
            slamChainCount = 0;
            lastScoringBall = null;

            if (clearPopups)
            {
                floatingScorePopups.Clear();
            }
        }

        public void CreateFloatingScorePopup(Vector2 worldPosition, int bonusPoints, string bonusLabel, Color popupColor)
        {
            floatingScorePopups.Add(new FloatingScorePopup
            {
                WorldPosition = worldPosition,
                PrimaryText = $"+{bonusPoints}",
                SecondaryText = BuildComboPopupText(bonusLabel),
                Color = popupColor,
                Age = 0f,
                Lifetime = FloatingScoreLifetimeSeconds,
            });
        }

        public void UpdateFloatingScorePopups(float deltaTimeSeconds)
        {
            if (floatingScorePopups.Count == 0)
            {
                return;
            }

            for (var index = floatingScorePopups.Count - 1; index >= 0; index--)
            {
                floatingScorePopups[index].Age += Mathf.Max(0f, deltaTimeSeconds);

                if (floatingScorePopups[index].Age >= floatingScorePopups[index].Lifetime)
                {
                    floatingScorePopups.RemoveAt(index);
                }
            }
        }

        public BreakoutUiFloatingScoreView[] BuildFloatingScoreViews(Camera camera, int screenHeight)
        {
            if (camera == null || floatingScorePopups.Count == 0)
            {
                return Array.Empty<BreakoutUiFloatingScoreView>();
            }

            var views = new List<BreakoutUiFloatingScoreView>(floatingScorePopups.Count);

            for (var index = 0; index < floatingScorePopups.Count; index++)
            {
                var popup = floatingScorePopups[index];
                var lifeRatio = popup.Lifetime > 0.001f ? Mathf.Clamp01(popup.Age / popup.Lifetime) : 1f;
                var alpha = lifeRatio < 0.22f
                    ? Mathf.InverseLerp(0f, 0.22f, lifeRatio)
                    : 1f - Mathf.InverseLerp(0.22f, 1f, lifeRatio);
                var worldPosition = popup.WorldPosition + (Vector2.up * (FloatingScoreTravelDistance * lifeRatio));
                var screenPosition = camera.WorldToScreenPoint(worldPosition);

                if (screenPosition.z < 0f)
                {
                    continue;
                }

                views.Add(new BreakoutUiFloatingScoreView
                {
                    ScreenPosition = new Vector2(screenPosition.x, screenHeight - screenPosition.y),
                    PrimaryText = popup.PrimaryText,
                    SecondaryText = popup.SecondaryText,
                    Color = popup.Color,
                    Alpha = Mathf.Clamp01(alpha),
                });
            }

            return views.ToArray();
        }

        private static string BuildComboPopupText(string bonusLabel)
        {
            return string.IsNullOrWhiteSpace(bonusLabel)
                ? "COMBO BONUS!"
                : $"COMBO BONUS: {bonusLabel}!";
        }

        private static void TryAddScoreMultiplierBonus(int awardedBasePoints, float scoreMultiplier, List<ScoreComboBonus> comboBonuses)
        {
            if (scoreMultiplier <= 1.001f)
            {
                return;
            }

            var bonusPoints = Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * (scoreMultiplier - 1f)));
            comboBonuses.Add(new ScoreComboBonus("SCORE SURGE", bonusPoints));
        }

        private static void TryAddSlamChainBonus(int awardedBasePoints, int projectedChainCount, List<ScoreComboBonus> comboBonuses)
        {
            if (projectedChainCount < 2)
            {
                return;
            }

            var bonusMultiplier = Mathf.Min(0.2f * (projectedChainCount - 1), 0.8f);
            var bonusPoints = Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * bonusMultiplier));
            comboBonuses.Add(new ScoreComboBonus("SLAM CHAIN", bonusPoints));
        }

        private static void TryAddBankShotBonus(
            int awardedBasePoints,
            BallController scoringBall,
            BrickDestructionCause destructionCause,
            List<ScoreComboBonus> comboBonuses)
        {
            if (destructionCause != BrickDestructionCause.Impact
                || scoringBall == null
                || scoringBall.RicochetCountSinceLastBrick < 2)
            {
                return;
            }

            var bonusMultiplier = Mathf.Min(0.14f * scoringBall.RicochetCountSinceLastBrick, 0.56f);
            var bonusPoints = Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * bonusMultiplier));
            comboBonuses.Add(new ScoreComboBonus("BANK SHOT", bonusPoints));
        }

        private void TryAddPartySplitBonus(int awardedBasePoints, BallController scoringBall, BreakoutScoreContext context, List<ScoreComboBonus> comboBonuses)
        {
            if (scoringBall == null
                || context.ActiveBallCount <= 1
                || lastScoringBall == null
                || lastScoringBall == scoringBall
                || context.CurrentTimeSeconds - lastScoredBrickTime > PartySplitWindowSeconds)
            {
                return;
            }

            var bonusPoints = Mathf.Max(1, Mathf.RoundToInt(awardedBasePoints * 0.25f));
            comboBonuses.Add(new ScoreComboBonus("PARTY SPLIT", bonusPoints));
        }

        private int GetProjectedSlamChainCount(float currentTimeSeconds)
        {
            return currentTimeSeconds - lastScoredBrickTime <= SlamChainWindowSeconds
                ? slamChainCount + 1
                : 1;
        }

        private readonly struct ScoreComboBonus
        {
            public ScoreComboBonus(string label, int points)
            {
                Label = label;
                Points = points;
            }

            public string Label { get; }

            public int Points { get; }
        }

        private sealed class FloatingScorePopup
        {
            public Vector2 WorldPosition;
            public string PrimaryText = string.Empty;
            public string SecondaryText = string.Empty;
            public Color Color = Color.white;
            public float Age;
            public float Lifetime = FloatingScoreLifetimeSeconds;
        }
    }
}
