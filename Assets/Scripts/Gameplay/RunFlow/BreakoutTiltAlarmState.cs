using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutTiltAlarmTriggerResult
    {
        Nudge,
        Warning,
        Alarm,
        Locked,
    }

    internal sealed class BreakoutTiltAlarmState
    {
        private const float HeatPerNudge = 1f;
        private const float WarningHeatThreshold = 2f;
        private const float AlarmHeatThreshold = 3f;
        private const float HeatDecayPerSecond = 0.55f;
        private const float AlarmDurationSeconds = 2.35f;

        private float heat;

        public bool IsAlarmActive => AlarmTimer > 0f;

        public bool IsWarningHot => heat >= WarningHeatThreshold;

        public float AlarmTimer { get; private set; }

        public float HeatRatio => Mathf.Clamp01(heat / AlarmHeatThreshold);

        public BreakoutTiltAlarmTriggerResult RegisterNudge()
        {
            if (IsAlarmActive)
            {
                return BreakoutTiltAlarmTriggerResult.Locked;
            }

            heat = Mathf.Min(AlarmHeatThreshold, heat + HeatPerNudge);

            if (heat >= AlarmHeatThreshold)
            {
                heat = 0f;
                AlarmTimer = AlarmDurationSeconds;
                return BreakoutTiltAlarmTriggerResult.Alarm;
            }

            return heat >= WarningHeatThreshold
                ? BreakoutTiltAlarmTriggerResult.Warning
                : BreakoutTiltAlarmTriggerResult.Nudge;
        }

        public bool Update(float deltaTimeSeconds)
        {
            var deltaTime = Mathf.Max(0f, deltaTimeSeconds);

            if (AlarmTimer > 0f)
            {
                AlarmTimer = Mathf.Max(0f, AlarmTimer - deltaTime);
                return AlarmTimer <= 0f;
            }

            heat = Mathf.Max(0f, heat - (HeatDecayPerSecond * deltaTime));
            return false;
        }

        public void Reset()
        {
            heat = 0f;
            AlarmTimer = 0f;
        }
    }
}
