using System;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    internal sealed class BreakoutPaddlePunkBoss : MonoBehaviour
    {
        private const float MaxDeflectionAngleDegrees = 66f;
        private const float MinDeflectionAngleDegrees = 10f;
        private const float ShieldSpawnBaseSeconds = 7.5f;
        private const float WiggleBaseIntervalSeconds = 5.6f;
        private const float WiggleDurationSeconds = 1.15f;
        private const float WiggleMaxVerticalOffset = 0.18f;
        private const float WiggleMaxRotationDegrees = 13f;
        private const float PauseMinIntervalSeconds = 3.2f;
        private const float PauseMaxIntervalSeconds = 6.1f;
        private const float PauseMinDurationSeconds = 0.58f;
        private const float PauseMaxDurationSeconds = 0.92f;
        private const int FirstInjuredSpinTriggerHitCount = 5;
        private const int InjuredSpinBaseHitInterval = 5;
        private const float InjuredSpinDurationSeconds = 1.65f;
        private const float InjuredSpinDegreesPerSecond = 720f;

        private Rigidbody2D bossBody;
        private Func<Vector2?> targetPositionResolver;
        private Func<float, float, float> randomRangeResolver;
        private float leftBoundaryX;
        private float rightBoundaryX;
        private float halfWidthWorld;
        private float moveSpeed;
        private float homeY;
        private float shieldSpawnTimer;
        private float pauseIntervalTimer;
        private float pauseTimer;
        private float injuredSpinTimer;
        private float injuredSpinRotation;
        private int gateIndex;
        private int bossHitCount;
        private int shieldSpawnCount;
        private int nextInjuredSpinHitCount;
        private float wiggleIntervalTimer;
        private float wiggleTimer;
        private float wigglePhase;
        private float currentWiggleYOffset;
        private float currentWiggleRotation;
        private bool hasPendingShieldSpawn;

        public int BossHitCount => bossHitCount;

        internal int NextInjuredSpinHitCount => nextInjuredSpinHitCount;

        internal bool IsWiggleActive => wiggleTimer > 0f;

        internal bool IsPausedForOpening => pauseTimer > 0f;

        internal bool IsInjuredSpinActive => injuredSpinTimer > 0f;

        internal void StartOpeningPause()
        {
            pauseTimer = BuildPauseDurationSeconds();
            StartWiggleBurst();
        }

        public string PhaseLabel
        {
            get
            {
                if (IsInjuredSpinActive)
                {
                    return "Injured";
                }

                if (IsPausedForOpening)
                {
                    return "Open Lane";
                }

                if (bossHitCount >= 6)
                {
                    return "No Mercy Rally";
                }

                return bossHitCount >= 3 ? "Turbo Mode" : "Warm-Up";
            }
        }

        public void Configure(
            int bossGateIndex,
            float minimumBoundaryX,
            float maximumBoundaryX,
            float halfWidth,
            float speed,
            Func<Vector2?> targetResolver,
            Func<float, float, float> randomRange = null)
        {
            gateIndex = Mathf.Max(0, bossGateIndex);
            leftBoundaryX = minimumBoundaryX;
            rightBoundaryX = maximumBoundaryX;
            halfWidthWorld = Mathf.Max(0.1f, halfWidth);
            moveSpeed = Mathf.Max(0f, speed);
            targetPositionResolver = targetResolver;
            randomRangeResolver = randomRange;
            homeY = transform.position.y;
            shieldSpawnTimer = Mathf.Max(3.5f, ShieldSpawnBaseSeconds - gateIndex);
            pauseIntervalTimer = BuildPauseIntervalSeconds();
            pauseTimer = 0f;
            injuredSpinTimer = 0f;
            injuredSpinRotation = 0f;
            nextInjuredSpinHitCount = FirstInjuredSpinTriggerHitCount;
            wiggleIntervalTimer = BuildWiggleIntervalSeconds();
            wiggleTimer = 0f;
            currentWiggleYOffset = 0f;
            currentWiggleRotation = 0f;
            bossBody = GetComponent<Rigidbody2D>();
        }

        public bool TryBuildCollisionResponse(Collision2D collision, out Vector2 bounceDirection, out float speedBurstMultiplier)
        {
            bossHitCount++;

            if (bossHitCount % 3 == 0)
            {
                StartWiggleBurst();
            }

            if (bossHitCount >= nextInjuredSpinHitCount)
            {
                StartInjuredSpin();
            }

            var contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;
            var normalizedOffset = Mathf.Clamp((contactPoint.x - transform.position.x) / halfWidthWorld, -1f, 1f);
            var absOffset = Mathf.Abs(normalizedOffset);
            var angle = Mathf.Lerp(MinDeflectionAngleDegrees, MaxDeflectionAngleDegrees, absOffset) * Mathf.Deg2Rad;
            var horizontalSign = Mathf.Approximately(normalizedOffset, 0f)
                ? (bossHitCount % 2 == 0 ? -1f : 1f)
                : Mathf.Sign(normalizedOffset);

            bounceDirection = new Vector2(Mathf.Sin(angle) * horizontalSign, -Mathf.Cos(angle)).normalized;
            speedBurstMultiplier = 1f + Mathf.Min(0.18f, bossHitCount * 0.018f);
            return true;
        }

        public bool TryConsumeShieldSpawnRequest()
        {
            if (!hasPendingShieldSpawn)
            {
                return false;
            }

            hasPendingShieldSpawn = false;
            return true;
        }

        private void FixedUpdate()
        {
            if (bossBody == null)
            {
                bossBody = GetComponent<Rigidbody2D>();
            }

            var targetX = ResolveTargetX();
            var phaseSpeedMultiplier = bossHitCount >= 6 ? 1.34f : bossHitCount >= 3 ? 1.16f : 1f;
            UpdatePauseTimer();
            var nextX = IsPausedForOpening || IsInjuredSpinActive
                ? bossBody.position.x
                : Mathf.MoveTowards(
                    bossBody.position.x,
                    targetX,
                    moveSpeed * phaseSpeedMultiplier * Time.fixedDeltaTime);
            nextX = Mathf.Clamp(nextX, leftBoundaryX + halfWidthWorld, rightBoundaryX - halfWidthWorld);
            UpdateWiggle();
            UpdateInjuredSpin();
            bossBody.MovePosition(new Vector2(nextX, homeY + currentWiggleYOffset));
            bossBody.MoveRotation(currentWiggleRotation + injuredSpinRotation);
            UpdateShieldSpawnTimer();
        }

        private float ResolveTargetX()
        {
            var targetPosition = targetPositionResolver?.Invoke();

            if (targetPosition.HasValue)
            {
                return targetPosition.Value.x;
            }

            var sweep = Mathf.Sin(Time.time * (0.85f + (gateIndex * 0.18f)));
            return Mathf.Lerp(leftBoundaryX + halfWidthWorld, rightBoundaryX - halfWidthWorld, (sweep + 1f) * 0.5f);
        }

        private void UpdateShieldSpawnTimer()
        {
            if (shieldSpawnCount >= 2 + gateIndex)
            {
                return;
            }

            shieldSpawnTimer = Mathf.Max(0f, shieldSpawnTimer - Time.fixedDeltaTime);

            if (shieldSpawnTimer > 0f)
            {
                return;
            }

            shieldSpawnCount++;
            hasPendingShieldSpawn = true;
            shieldSpawnTimer = Mathf.Max(4.5f, ShieldSpawnBaseSeconds - gateIndex - (bossHitCount * 0.22f));
        }

        private void UpdateWiggle()
        {
            if (wiggleTimer <= 0f)
            {
                wiggleIntervalTimer = Mathf.Max(0f, wiggleIntervalTimer - Time.fixedDeltaTime);

                if (wiggleIntervalTimer <= 0f)
                {
                    StartWiggleBurst();
                }
            }

            if (wiggleTimer <= 0f)
            {
                currentWiggleYOffset = Mathf.MoveTowards(currentWiggleYOffset, 0f, WiggleMaxVerticalOffset * 6f * Time.fixedDeltaTime);
                currentWiggleRotation = Mathf.MoveTowards(currentWiggleRotation, 0f, WiggleMaxRotationDegrees * 6f * Time.fixedDeltaTime);
                return;
            }

            wiggleTimer = Mathf.Max(0f, wiggleTimer - Time.fixedDeltaTime);
            var phaseSpeed = 15f + (gateIndex * 1.8f) + Mathf.Min(5f, bossHitCount * 0.35f);
            wigglePhase += Time.fixedDeltaTime * phaseSpeed;
            var envelope = Mathf.Sin(Mathf.Clamp01(wiggleTimer / WiggleDurationSeconds) * Mathf.PI);
            var intensity = 0.65f + (Mathf.Clamp(gateIndex, 0, 2) * 0.14f);
            currentWiggleYOffset = Mathf.Sin(wigglePhase) * WiggleMaxVerticalOffset * intensity * envelope;
            currentWiggleRotation = Mathf.Sin((wigglePhase * 1.47f) + 0.8f) * WiggleMaxRotationDegrees * intensity * envelope;

            if (wiggleTimer <= 0f)
            {
                wiggleIntervalTimer = BuildWiggleIntervalSeconds();
            }
        }

        private void StartWiggleBurst()
        {
            wiggleTimer = WiggleDurationSeconds;
            wiggleIntervalTimer = BuildWiggleIntervalSeconds();
            wigglePhase += 1.7f + (bossHitCount * 0.21f);
        }

        private float BuildWiggleIntervalSeconds()
        {
            return Mathf.Max(2.6f, WiggleBaseIntervalSeconds - (gateIndex * 0.55f) - (Mathf.Min(8, bossHitCount) * 0.18f));
        }

        private void UpdatePauseTimer()
        {
            if (IsInjuredSpinActive)
            {
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer = Mathf.Max(0f, pauseTimer - Time.fixedDeltaTime);

                if (pauseTimer <= 0f)
                {
                    pauseIntervalTimer = BuildPauseIntervalSeconds();
                }

                return;
            }

            pauseIntervalTimer = Mathf.Max(0f, pauseIntervalTimer - Time.fixedDeltaTime);

            if (pauseIntervalTimer > 0f)
            {
                return;
            }

            StartOpeningPause();
        }

        private void StartInjuredSpin()
        {
            injuredSpinTimer = InjuredSpinDurationSeconds;
            injuredSpinRotation = 0f;
            pauseTimer = Mathf.Max(pauseTimer, InjuredSpinDurationSeconds);
            pauseIntervalTimer = BuildPauseIntervalSeconds();
            nextInjuredSpinHitCount = BuildNextInjuredSpinHitCount(bossHitCount);
            StartWiggleBurst();
        }

        private void UpdateInjuredSpin()
        {
            if (injuredSpinTimer <= 0f)
            {
                injuredSpinRotation = Mathf.MoveTowardsAngle(
                    injuredSpinRotation,
                    0f,
                    InjuredSpinDegreesPerSecond * Time.fixedDeltaTime);
                return;
            }

            injuredSpinTimer = Mathf.Max(0f, injuredSpinTimer - Time.fixedDeltaTime);
            injuredSpinRotation = Mathf.Repeat(injuredSpinRotation + (InjuredSpinDegreesPerSecond * Time.fixedDeltaTime), 360f);

            if (injuredSpinTimer <= 0f)
            {
                pauseTimer = 0f;
                pauseIntervalTimer = BuildPauseIntervalSeconds();
            }
        }

        private float BuildPauseIntervalSeconds()
        {
            var pressureReduction = Mathf.Min(1.4f, bossHitCount * 0.08f) + (gateIndex * 0.18f);
            return Mathf.Max(2.35f, RandomRange(PauseMinIntervalSeconds, PauseMaxIntervalSeconds) - pressureReduction);
        }

        private float BuildPauseDurationSeconds()
        {
            return RandomRange(PauseMinDurationSeconds, PauseMaxDurationSeconds);
        }

        private int BuildNextInjuredSpinHitCount(int lastTriggerHitCount)
        {
            var jitterRoll = RandomRange(0f, 1f);
            var jitter = jitterRoll < 0.34f ? -1 : jitterRoll < 0.67f ? 0 : 1;
            return Mathf.Max(lastTriggerHitCount + 3, lastTriggerHitCount + InjuredSpinBaseHitInterval + jitter);
        }

        private float RandomRange(float minimumInclusive, float maximumInclusive)
        {
            return randomRangeResolver != null
                ? randomRangeResolver(minimumInclusive, maximumInclusive)
                : UnityEngine.Random.Range(minimumInclusive, maximumInclusive);
        }
    }
}
