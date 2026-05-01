using UnityEngine;

namespace GetBricked.Gameplay
{
    internal interface IBreakoutScoreService
    {
        BrickScoreAward BuildBrickScoreAward(Brick brick, BallController scoringBall, BrickDestructionCause destructionCause, BreakoutScoreContext context);

        float GetScoreMultiplierForSpeed(float speed, float baseBallSpeed);

        void RegisterBrickScoreEvent(BallController scoringBall, bool awardedPoints, float currentTimeSeconds);

        void ResetComboTracking(bool clearPopups);

        void CreateFloatingScorePopup(Vector2 worldPosition, int bonusPoints, string bonusLabel, Color popupColor);

        void UpdateFloatingScorePopups(float deltaTimeSeconds);

        BreakoutUiFloatingScoreView[] BuildFloatingScoreViews(Camera camera, int screenHeight);
    }
}
