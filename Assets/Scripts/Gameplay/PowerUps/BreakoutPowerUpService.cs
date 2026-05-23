using System.Collections.Generic;
using System.Text;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal sealed class BreakoutActiveTimedEffect
    {
        public BreakoutActiveTimedEffect(PowerUpDefinition definition, float remainingDuration)
        {
            Definition = definition;
            RemainingDuration = remainingDuration;
            StackCount = 1;
            EffectMultiplier = 1f;
        }

        public PowerUpDefinition Definition { get; }

        public float RemainingDuration { get; set; }

        public int StackCount { get; private set; }

        public float EffectMultiplier { get; private set; }

        public float EffectStrength => Mathf.Max(1, StackCount) * Mathf.Max(0.1f, EffectMultiplier);

        public void AddStack(float durationSeconds)
        {
            StackCount += 1;
            RemainingDuration = Mathf.Max(0f, durationSeconds);
        }

        public bool ConsumeStack()
        {
            if (StackCount <= 1)
            {
                return false;
            }

            StackCount -= 1;
            return true;
        }

        public void MultiplyEffect(float multiplier)
        {
            EffectMultiplier *= Mathf.Max(0.1f, multiplier);
        }
    }

    internal sealed class BreakoutTimedEffectStackSummary
    {
        public BreakoutTimedEffectStackSummary(
            PowerUpDefinition definition,
            int stackCount,
            float effectMultiplier,
            float remainingDuration,
            float durationRatio)
        {
            Definition = definition;
            StackCount = Mathf.Max(1, stackCount);
            EffectMultiplier = Mathf.Max(0.1f, effectMultiplier);
            RemainingDuration = remainingDuration;
            DurationRatio = durationRatio;
        }

        public PowerUpDefinition Definition { get; }

        public int StackCount { get; }

        public float EffectMultiplier { get; }

        public float DisplayMultiplier => StackCount * EffectMultiplier;

        public float RemainingDuration { get; private set; }

        public float DurationRatio { get; private set; }
    }

    internal readonly struct BreakoutEffectModifiers
    {
        public BreakoutEffectModifiers(
            float paddleWidthMultiplier,
            float wavyPaddleStrength,
            float timedBallSpeedMultiplier,
            float ballSizeMultiplier,
            bool stickyPaddleEnabled,
            bool laserPaddleEnabled,
            bool phaseBallEnabled,
            float chainLightningStrength,
            bool reverseControlsEnabled,
            float splitPaddleGapNormalized,
            float gravityWellStrength,
            float fogVisibilityMultiplier,
            float lagSpikeStrength,
            float brickMagnetStrength,
            float scoreMultiplier,
            bool paddleCloneEnabled,
            float brickJammerStrength,
            float hotPotatoStrength,
            float explosiveBallStrength,
            float vectorSightStrength,
            float capsuleMagnetStrength,
            bool mirrorImagePaddleEnabled,
            float cleanCatchAimMultiplier,
            float paddleHitTiltDegrees)
        {
            PaddleWidthMultiplier = paddleWidthMultiplier;
            WavyPaddleStrength = wavyPaddleStrength;
            TimedBallSpeedMultiplier = timedBallSpeedMultiplier;
            BallSizeMultiplier = ballSizeMultiplier;
            StickyPaddleEnabled = stickyPaddleEnabled;
            LaserPaddleEnabled = laserPaddleEnabled;
            PhaseBallEnabled = phaseBallEnabled;
            ChainLightningStrength = chainLightningStrength;
            ReverseControlsEnabled = reverseControlsEnabled;
            SplitPaddleGapNormalized = splitPaddleGapNormalized;
            GravityWellStrength = gravityWellStrength;
            FogVisibilityMultiplier = fogVisibilityMultiplier;
            LagSpikeStrength = lagSpikeStrength;
            BrickMagnetStrength = brickMagnetStrength;
            ScoreMultiplier = scoreMultiplier;
            PaddleCloneEnabled = paddleCloneEnabled;
            BrickJammerStrength = brickJammerStrength;
            HotPotatoStrength = hotPotatoStrength;
            ExplosiveBallStrength = explosiveBallStrength;
            VectorSightStrength = vectorSightStrength;
            CapsuleMagnetStrength = capsuleMagnetStrength;
            MirrorImagePaddleEnabled = mirrorImagePaddleEnabled;
            CleanCatchAimMultiplier = cleanCatchAimMultiplier;
            PaddleHitTiltDegrees = paddleHitTiltDegrees;
        }

        public float PaddleWidthMultiplier { get; }

        public float WavyPaddleStrength { get; }

        public float TimedBallSpeedMultiplier { get; }

        public float BallSizeMultiplier { get; }

        public bool StickyPaddleEnabled { get; }

        public bool LaserPaddleEnabled { get; }

        public bool PhaseBallEnabled { get; }

        public float ChainLightningStrength { get; }

        public bool ReverseControlsEnabled { get; }

        public float SplitPaddleGapNormalized { get; }

        public float GravityWellStrength { get; }

        public float FogVisibilityMultiplier { get; }

        public float LagSpikeStrength { get; }

        public float BrickMagnetStrength { get; }

        public float ScoreMultiplier { get; }

        public bool PaddleCloneEnabled { get; }

        public float BrickJammerStrength { get; }

        public float HotPotatoStrength { get; }

        public float ExplosiveBallStrength { get; }

        public float VectorSightStrength { get; }

        public float CapsuleMagnetStrength { get; }

        public bool MirrorImagePaddleEnabled { get; }

        public float CleanCatchAimMultiplier { get; }

        public float PaddleHitTiltDegrees { get; }
    }

    internal readonly struct BreakoutPowerUpApplicationResult
    {
        public BreakoutPowerUpApplicationResult(bool shouldSpawnMultiBall, int shieldWallChargesGranted)
        {
            ShouldSpawnMultiBall = shouldSpawnMultiBall;
            ShieldWallChargesGranted = Mathf.Max(0, shieldWallChargesGranted);
        }

        public bool ShouldSpawnMultiBall { get; }

        public int ShieldWallChargesGranted { get; }
    }

    internal struct BreakoutEffectModifierAccumulator
    {
        private float paddleWidthMultiplier;
        private float wavyPaddleStrength;
        private float timedBallSpeedMultiplier;
        private float ballSizeMultiplier;
        private bool stickyPaddleEnabled;
        private bool laserPaddleEnabled;
        private bool phaseBallEnabled;
        private float chainLightningStrength;
        private bool reverseControlsEnabled;
        private float splitPaddleGapNormalized;
        private float gravityWellStrength;
        private float fogVisibilityMultiplier;
        private float lagSpikeStrength;
        private float brickMagnetStrength;
        private float scoreMultiplier;
        private bool paddleCloneEnabled;
        private float brickJammerStrength;
        private float hotPotatoStrength;
        private float explosiveBallStrength;
        private float vectorSightStrength;
        private float capsuleMagnetStrength;
        private bool mirrorImagePaddleEnabled;
        private float cleanCatchAimMultiplier;
        private float paddleHitTiltDegrees;

        public BreakoutEffectModifierAccumulator(float basePaddleWidthMultiplier, float baseWavyPaddleStrength)
        {
            paddleWidthMultiplier = Mathf.Max(0.1f, basePaddleWidthMultiplier);
            wavyPaddleStrength = Mathf.Clamp01(baseWavyPaddleStrength);
            timedBallSpeedMultiplier = 1f;
            ballSizeMultiplier = 1f;
            stickyPaddleEnabled = false;
            laserPaddleEnabled = false;
            phaseBallEnabled = false;
            chainLightningStrength = 0f;
            reverseControlsEnabled = false;
            splitPaddleGapNormalized = 0f;
            gravityWellStrength = 0f;
            fogVisibilityMultiplier = 1f;
            lagSpikeStrength = 0f;
            brickMagnetStrength = 0f;
            scoreMultiplier = 1f;
            paddleCloneEnabled = false;
            brickJammerStrength = 0f;
            hotPotatoStrength = 0f;
            explosiveBallStrength = 0f;
            vectorSightStrength = 0f;
            capsuleMagnetStrength = 0f;
            mirrorImagePaddleEnabled = false;
            cleanCatchAimMultiplier = 1f;
            paddleHitTiltDegrees = 0f;
        }

        public void Apply(BreakoutActiveTimedEffect activeEffect)
        {
            var powerUpDefinition = activeEffect?.Definition;

            if (powerUpDefinition == null)
            {
                return;
            }

            var effectStrength = activeEffect.EffectStrength;

            switch (powerUpDefinition.EffectType)
            {
                case PowerUpEffectType.PaddleWidthMultiplier:
                    paddleWidthMultiplier *= Mathf.Pow(powerUpDefinition.Scalar, effectStrength);
                    break;
                case PowerUpEffectType.BallSpeedMultiplier:
                    timedBallSpeedMultiplier *= Mathf.Pow(powerUpDefinition.Scalar, effectStrength);
                    break;
                case PowerUpEffectType.BallSizeMultiplier:
                    ballSizeMultiplier *= Mathf.Pow(powerUpDefinition.Scalar, effectStrength);
                    break;
                case PowerUpEffectType.WavyPaddle:
                    wavyPaddleStrength = Mathf.Max(wavyPaddleStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.StickyPaddle:
                    stickyPaddleEnabled = true;
                    break;
                case PowerUpEffectType.LaserPaddle:
                    laserPaddleEnabled = true;
                    break;
                case PowerUpEffectType.PhaseBall:
                    phaseBallEnabled = true;
                    break;
                case PowerUpEffectType.ChainLightning:
                    chainLightningStrength = Mathf.Max(chainLightningStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.ReverseControls:
                    reverseControlsEnabled = true;
                    break;
                case PowerUpEffectType.SplitPaddle:
                    splitPaddleGapNormalized = Mathf.Max(splitPaddleGapNormalized, Mathf.Clamp(powerUpDefinition.Scalar * effectStrength * 0.34f, 0.18f, 0.42f));
                    break;
                case PowerUpEffectType.GravityWell:
                    gravityWellStrength = Mathf.Max(gravityWellStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.FogOfWar:
                    fogVisibilityMultiplier = Mathf.Min(fogVisibilityMultiplier, Mathf.Clamp01(Mathf.Pow(powerUpDefinition.VisibilityScalar, effectStrength)));
                    break;
                case PowerUpEffectType.LagSpike:
                    lagSpikeStrength = Mathf.Max(lagSpikeStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.BrickMagnet:
                    brickMagnetStrength = Mathf.Max(brickMagnetStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.ScoreMultiplier:
                    scoreMultiplier *= Mathf.Pow(powerUpDefinition.Scalar, effectStrength);
                    break;
                case PowerUpEffectType.PaddleClone:
                    paddleCloneEnabled = true;
                    break;
                case PowerUpEffectType.BrickJammer:
                    brickJammerStrength = Mathf.Max(brickJammerStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.HotPotatoBall:
                    timedBallSpeedMultiplier *= Mathf.Pow(powerUpDefinition.Scalar, effectStrength);
                    scoreMultiplier *= Mathf.Pow(Mathf.Max(1f, powerUpDefinition.Scalar), effectStrength);
                    hotPotatoStrength = Mathf.Max(hotPotatoStrength, Mathf.Clamp01((powerUpDefinition.Scalar - 1f) * effectStrength));
                    break;
                case PowerUpEffectType.ExplosiveBall:
                    explosiveBallStrength = Mathf.Max(explosiveBallStrength, Mathf.Max(0.1f, powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.VectorSight:
                    vectorSightStrength = Mathf.Max(vectorSightStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.CapsuleMagnet:
                    capsuleMagnetStrength = Mathf.Max(capsuleMagnetStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
                case PowerUpEffectType.MirrorImagePaddle:
                    mirrorImagePaddleEnabled = true;
                    break;
                case PowerUpEffectType.CleanCatch:
                    cleanCatchAimMultiplier = Mathf.Max(
                        cleanCatchAimMultiplier,
                        Mathf.Max(1f, powerUpDefinition.Scalar * activeEffect.EffectMultiplier));
                    break;
                case PowerUpEffectType.PaddleHitTilt:
                    paddleHitTiltDegrees = Mathf.Max(
                        paddleHitTiltDegrees,
                        Mathf.Clamp(powerUpDefinition.Scalar * effectStrength, 0f, 18f));
                    break;
            }
        }

        public BreakoutEffectModifiers ToModifiers()
        {
            return new BreakoutEffectModifiers(
                paddleWidthMultiplier,
                wavyPaddleStrength,
                timedBallSpeedMultiplier,
                Mathf.Max(0.1f, ballSizeMultiplier),
                stickyPaddleEnabled,
                laserPaddleEnabled,
                phaseBallEnabled,
                chainLightningStrength,
                reverseControlsEnabled,
                splitPaddleGapNormalized,
                gravityWellStrength,
                fogVisibilityMultiplier,
                lagSpikeStrength,
                brickMagnetStrength,
                Mathf.Max(0.1f, scoreMultiplier),
                paddleCloneEnabled,
                brickJammerStrength,
                hotPotatoStrength,
                explosiveBallStrength,
                vectorSightStrength,
                capsuleMagnetStrength,
                mirrorImagePaddleEnabled,
                Mathf.Max(1f, cleanCatchAimMultiplier),
                Mathf.Max(0f, paddleHitTiltDegrees));
        }
    }

    internal readonly struct BreakoutPowerUpDropCandidate
    {
        public BreakoutPowerUpDropCandidate(PowerUpDefinition definition, float weight)
        {
            Definition = definition;
            Weight = Mathf.Max(0f, weight);
        }

        public PowerUpDefinition Definition { get; }

        public float Weight { get; }
    }

    internal sealed class BreakoutPowerUpService
    {
        public const int CapsuleMadnessPickupThreshold = 5;
        public const float CapsuleMadnessDurationSeconds = 2.6f;
        public const int CapsuleMadnessPickupBonusPoints = 250;
        public const float CapsuleMagnetRange = 4.25f;
        public const int BankBonusMaximumChargePoints = 300;

        private readonly Vector2 pickupSize;
        private readonly float pickupFallSpeed;
        private readonly float multiBallSpreadAngle;
        private readonly Material pickupMaterial;
        private bool capsuleMadnessThresholdArmed = true;

        public BreakoutPowerUpService(Vector2 pickupSize, float pickupFallSpeed, float multiBallSpreadAngle, Material pickupMaterial)
        {
            this.pickupSize = pickupSize;
            this.pickupFallSpeed = pickupFallSpeed;
            this.multiBallSpreadAngle = multiBallSpreadAngle;
            this.pickupMaterial = pickupMaterial;
        }

        public List<PowerUpPickup> ActivePickups { get; } = new List<PowerUpPickup>();

        public List<BreakoutActiveTimedEffect> ActiveTimedEffects { get; } = new List<BreakoutActiveTimedEffect>();

        public string PickupBannerText { get; private set; } = string.Empty;

        public float PickupBannerTimer { get; private set; }

        public Color PickupBannerColor { get; private set; } = Color.white;

        public float CapsuleMadnessTimer { get; private set; }

        public bool IsCapsuleMadnessActive => CapsuleMadnessTimer > 0f;

        public int BankBonusChargePoints { get; private set; }

        public void UpdateTimedEffects(bool isPlaying, float deltaTime, System.Action modifiersChanged)
        {
            if (!isPlaying)
            {
                return;
            }

            if (CapsuleMadnessTimer > 0f)
            {
                CapsuleMadnessTimer = Mathf.Max(0f, CapsuleMadnessTimer - deltaTime);
            }

            if (ActiveTimedEffects.Count == 0)
            {
                return;
            }

            var hasChanges = false;

            for (var index = ActiveTimedEffects.Count - 1; index >= 0; index--)
            {
                var activeEffect = ActiveTimedEffects[index];

                if (activeEffect.Definition == null)
                {
                    ActiveTimedEffects.RemoveAt(index);
                    hasChanges = true;
                    continue;
                }

                activeEffect.RemainingDuration = Mathf.Max(0f, activeEffect.RemainingDuration - deltaTime);

                if (activeEffect.RemainingDuration > 0f)
                {
                    continue;
                }

                ActiveTimedEffects.RemoveAt(index);
                hasChanges = true;
            }

            if (hasChanges)
            {
                modifiersChanged?.Invoke();
            }
        }

        public void UpdatePickupBanner(float deltaTime)
        {
            if (PickupBannerTimer <= 0f)
            {
                return;
            }

            PickupBannerTimer = Mathf.Max(0f, PickupBannerTimer - deltaTime);
        }

        public PowerUpPickup TrySpawnPickup(
            Brick brick,
            RunSettings activeRunSettings,
            BreakoutRunState activeRunState,
            float effectiveDropChanceMultiplier,
            System.Func<float, float, float> nextGameplayRandomFloat,
            Transform pickupsRoot,
            float arenaBottom,
            BreakoutThemeService themeService,
            BreakoutGameController controller,
            PowerUpDefinition forcedDropDefinition = null,
            IReadOnlyList<PowerUpDefinition> forcedDropCandidatePool = null,
            float pickupFallSpeedMultiplier = 1f)
        {
            if (brick == null || brick.Definition == null)
            {
                return null;
            }

            var brickDefinition = brick.Definition;
            var dropTable = BuildDropCandidates(brickDefinition, activeRunSettings, activeRunState);
            var forceSpecificDrop = forcedDropDefinition != null;
            var forcePickupDrops = activeRunSettings?.ForcePickupDropsOnBreak == true;
            var effectiveDropChance = forcePickupDrops
                ? 1f
                : Mathf.Clamp01(brickDefinition.DropChance * Mathf.Max(0f, effectiveDropChanceMultiplier));

            if (!forceSpecificDrop
                && (dropTable.Length == 0
                || effectiveDropChance <= 0f
                || activeRunSettings?.DropPoolMode == DropPoolMode.Disabled
                || nextGameplayRandomFloat == null
                || (!forcePickupDrops && nextGameplayRandomFloat(0f, 1f) > effectiveDropChance)))
            {
                return null;
            }

            PowerUpDefinition selectedPowerUp;

            if (forceSpecificDrop)
            {
                selectedPowerUp = forcedDropDefinition;
                dropTable = BuildForcedDropCandidateTable(forcedDropDefinition, forcedDropCandidatePool, activeRunSettings);
            }
            else
            {
                selectedPowerUp = SelectDropFromTable(dropTable, activeRunSettings, activeRunState, nextGameplayRandomFloat);
            }

            if (selectedPowerUp == null)
            {
                return null;
            }

            var visualPowerUp = selectedPowerUp;
            var usesHelpfulVisualDisguise = false;
            PowerUpDefinition primaryPayloadDefinition = null;
            PowerUpDefinition secondaryPayloadDefinition = null;

            if (selectedPowerUp.EffectType == PowerUpEffectType.RandomHarmfulDrop
                && !TryResolveRandomHarmfulDrop(
                    selectedPowerUp,
                    dropTable,
                    activeRunSettings,
                    activeRunState,
                    nextGameplayRandomFloat,
                    forceSpecificDrop,
                    out selectedPowerUp,
                    out visualPowerUp,
                    out usesHelpfulVisualDisguise))
            {
                return null;
            }

            if (selectedPowerUp.EffectType == PowerUpEffectType.RandomMixedDrop
                && !TryResolveRandomMixedDrop(
                    selectedPowerUp,
                    dropTable,
                    activeRunSettings,
                    activeRunState,
                    nextGameplayRandomFloat,
                    forceSpecificDrop,
                    out primaryPayloadDefinition,
                    out secondaryPayloadDefinition))
            {
                return null;
            }

            return CreatePickup(
                (Vector2)brick.transform.position,
                selectedPowerUp,
                visualPowerUp,
                usesHelpfulVisualDisguise,
                primaryPayloadDefinition,
                secondaryPayloadDefinition,
                pickupsRoot,
                arenaBottom,
                themeService,
                controller,
                pickupFallSpeedMultiplier);
        }

        private static PowerUpDefinition SelectDropFromTable(
            BreakoutPowerUpDropCandidate[] dropTable,
            RunSettings activeRunSettings,
            BreakoutRunState activeRunState,
            System.Func<float, float, float> nextGameplayRandomFloat)
        {
            var totalWeight = 0f;

            for (var index = 0; index < dropTable.Length; index++)
            {
                if (!IsDropAllowed(activeRunSettings, activeRunState, dropTable[index].Definition))
                {
                    continue;
                }

                totalWeight += dropTable[index].Weight;
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            var roll = nextGameplayRandomFloat(0f, totalWeight);

            for (var index = 0; index < dropTable.Length; index++)
            {
                var entry = dropTable[index];

                if (!IsDropAllowed(activeRunSettings, activeRunState, entry.Definition))
                {
                    continue;
                }

                roll -= entry.Weight;

                if (roll > 0f)
                {
                    continue;
                }

                return entry.Definition;
            }

            return null;
        }

        public BreakoutPowerUpApplicationResult ApplyPowerUp(PowerUpDefinition powerUpDefinition, BreakoutThemeService themeService)
        {
            if (powerUpDefinition == null)
            {
                return default;
            }

            if (powerUpDefinition.IsTimed)
            {
                var stackCount = AddTimedEffect(powerUpDefinition);
                ShowPickupBanner(powerUpDefinition, themeService, stackCount);
                return default;
            }

            if (powerUpDefinition.EffectType == PowerUpEffectType.ActiveDropMultiplier)
            {
                MultiplyActiveTimedEffects(powerUpDefinition.Scalar);
                ShowPickupBanner(powerUpDefinition, themeService, 1);

                return default;
            }

            ShowPickupBanner(powerUpDefinition, themeService, 1);

            return powerUpDefinition.EffectType switch
            {
                PowerUpEffectType.MultiBallBurst => new BreakoutPowerUpApplicationResult(true, 0),
                PowerUpEffectType.ShieldWall => new BreakoutPowerUpApplicationResult(false, Mathf.Max(1, powerUpDefinition.ExtraBallCount > 0 ? powerUpDefinition.ExtraBallCount : Mathf.RoundToInt(powerUpDefinition.Scalar))),
                _ => default,
            };
        }

        public BreakoutEffectModifiers CalculateEffectModifiers(RunSettings activeRunSettings)
        {
            return CalculateEffectModifiers(activeRunSettings?.PaddleWidthMultiplier ?? 1f, baseWavyPaddleStrength: 0f);
        }

        public BreakoutEffectModifiers CalculateEffectModifiers(float basePaddleWidthMultiplier, float baseWavyPaddleStrength)
        {
            var accumulator = new BreakoutEffectModifierAccumulator(basePaddleWidthMultiplier, baseWavyPaddleStrength);

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                accumulator.Apply(ActiveTimedEffects[index]);
            }

            return accumulator.ToModifiers();
        }

        public Vector2[] BuildMultiBallDirections(Vector2 sourceDirection, int extraBallCount)
        {
            var resolvedDirection = sourceDirection.sqrMagnitude > 0.01f ? sourceDirection.normalized : Vector2.up;
            var resolvedCount = Mathf.Max(1, extraBallCount);
            var directions = new Vector2[resolvedCount];

            for (var index = 0; index < resolvedCount; index++)
            {
                var angle = resolvedCount == 1
                    ? 0f
                    : Mathf.Lerp(-multiBallSpreadAngle, multiBallSpreadAngle, index / (resolvedCount - 1f));
                directions[index] = (Vector2)(Quaternion.Euler(0f, 0f, angle) * resolvedDirection);
            }

            return directions;
        }

        public void RemovePickup(PowerUpPickup pickup)
        {
            if (pickup != null)
            {
                ActivePickups.Remove(pickup);
            }

            UpdateCapsuleMadnessThresholdArming();
        }

        public void RefreshCapsuleMagnetTargets(float strength, Collider2D paddleCollider)
        {
            var clampedStrength = Mathf.Clamp01(strength);
            var hasActiveMagnet = clampedStrength > 0.001f && paddleCollider != null;
            var targetPosition = hasActiveMagnet
                ? (Vector2)paddleCollider.bounds.center
                : Vector2.zero;

            for (var index = ActivePickups.Count - 1; index >= 0; index--)
            {
                var pickup = ActivePickups[index];

                if (pickup == null)
                {
                    ActivePickups.RemoveAt(index);
                    continue;
                }

                if (!hasActiveMagnet || pickup.Definition == null || !pickup.Definition.IsBeneficial)
                {
                    pickup.SetCapsuleMagnetTarget(Vector2.zero, 0f);
                    continue;
                }

                var distance = Vector2.Distance(pickup.transform.position, targetPosition);

                if (distance > CapsuleMagnetRange)
                {
                    pickup.SetCapsuleMagnetTarget(Vector2.zero, 0f);
                    continue;
                }

                var proximity = 1f - Mathf.Clamp01(distance / CapsuleMagnetRange);
                pickup.SetCapsuleMagnetTarget(targetPosition, clampedStrength * Mathf.Lerp(0.35f, 1f, proximity));
            }
        }

        public void ClearPickups()
        {
            for (var index = ActivePickups.Count - 1; index >= 0; index--)
            {
                if (ActivePickups[index] == null)
                {
                    continue;
                }

                ActivePickups[index].gameObject.SetActive(false);
                Object.Destroy(ActivePickups[index].gameObject);
            }

            ActivePickups.Clear();
            CapsuleMadnessTimer = 0f;
            capsuleMadnessThresholdArmed = true;
        }

        public void ClearTimedEffects()
        {
            ActiveTimedEffects.Clear();
            ClearBankBonusCharge();
        }

        public int ChargeBankBonusFromWallBounce()
        {
            var chargePoints = ResolveBankBonusChargePerWallBounce();

            if (chargePoints <= 0)
            {
                return 0;
            }

            var previousCharge = BankBonusChargePoints;
            BankBonusChargePoints = Mathf.Min(BankBonusMaximumChargePoints, BankBonusChargePoints + chargePoints);
            return BankBonusChargePoints - previousCharge;
        }

        public bool TryConsumeBankBonus(out int bonusPoints)
        {
            bonusPoints = BankBonusChargePoints;

            if (bonusPoints <= 0)
            {
                return false;
            }

            BankBonusChargePoints = 0;
            return true;
        }

        public bool TryConsumePrismPopCharge(out PowerUpDefinition definition, out float effectMultiplier)
        {
            definition = null;
            effectMultiplier = 1f;

            for (var index = ActiveTimedEffects.Count - 1; index >= 0; index--)
            {
                var activeEffect = ActiveTimedEffects[index];
                var activeDefinition = activeEffect?.Definition;

                if (activeDefinition == null || activeDefinition.EffectType != PowerUpEffectType.PrismPop)
                {
                    continue;
                }

                definition = activeDefinition;
                effectMultiplier = activeEffect.EffectMultiplier;

                if (!activeEffect.ConsumeStack())
                {
                    ActiveTimedEffects.RemoveAt(index);
                }

                return true;
            }

            return false;
        }

        public bool TryConsumeCleanCatchCharge(out PowerUpDefinition definition, out float aimMultiplier)
        {
            definition = null;
            aimMultiplier = 1f;

            for (var index = ActiveTimedEffects.Count - 1; index >= 0; index--)
            {
                var activeEffect = ActiveTimedEffects[index];
                var activeDefinition = activeEffect?.Definition;

                if (activeDefinition == null || activeDefinition.EffectType != PowerUpEffectType.CleanCatch)
                {
                    continue;
                }

                definition = activeDefinition;
                aimMultiplier = Mathf.Max(1f, activeDefinition.Scalar * activeEffect.EffectMultiplier);

                if (!activeEffect.ConsumeStack())
                {
                    ActiveTimedEffects.RemoveAt(index);
                }

                return true;
            }

            return false;
        }

        public void ClearBankBonusCharge()
        {
            BankBonusChargePoints = 0;
        }

        public int RemoveBeneficialPaddleWidthEffects()
        {
            var removedCount = 0;

            for (var index = ActiveTimedEffects.Count - 1; index >= 0; index--)
            {
                var definition = ActiveTimedEffects[index]?.Definition;

                if (definition == null
                    || definition.EffectType != PowerUpEffectType.PaddleWidthMultiplier
                    || !definition.IsBeneficial
                    || definition.Scalar <= 1f)
                {
                    continue;
                }

                ActiveTimedEffects.RemoveAt(index);
                removedCount++;
            }

            return removedCount;
        }

        public int RemoveBeneficialBallSizeEffects()
        {
            var removedCount = 0;

            for (var index = ActiveTimedEffects.Count - 1; index >= 0; index--)
            {
                var definition = ActiveTimedEffects[index]?.Definition;

                if (definition == null
                    || definition.EffectType != PowerUpEffectType.BallSizeMultiplier
                    || !definition.IsBeneficial
                    || definition.Scalar <= 1f)
                {
                    continue;
                }

                ActiveTimedEffects.RemoveAt(index);
                removedCount++;
            }

            return removedCount;
        }

        public void ShowStatusBanner(string text, Color color, float durationSeconds = 1.8f)
        {
            PickupBannerText = text ?? string.Empty;
            PickupBannerColor = color;
            PickupBannerTimer = Mathf.Max(0f, durationSeconds);
        }

        public string BuildActiveEffectsLabel()
        {
            var summaries = BuildTimedEffectStackSummaries();

            if (summaries.Count == 0)
            {
                return "Active Effects: none";
            }

            var builder = new StringBuilder("Active Effects: ");

            for (var index = 0; index < summaries.Count; index++)
            {
                var summary = summaries[index];

                if (builder.Length > 16)
                {
                    builder.Append(" | ");
                }

                builder.Append(summary.Definition.HudLabel);

                var displayMultiplier = summary.DisplayMultiplier;

                if (Mathf.Abs(displayMultiplier - 1f) > 0.001f)
                {
                    builder.Append(" x");
                    builder.Append(FormatMultiplier(displayMultiplier));
                }

                builder.Append(' ');
                builder.Append(summary.RemainingDuration.ToString("0.0"));
                builder.Append('s');
            }

            return builder.ToString();
        }

        public List<BreakoutTimedEffectStackSummary> BuildTimedEffectStackSummaries()
        {
            var summaries = new List<BreakoutTimedEffectStackSummary>();

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];
                var definition = activeEffect.Definition;

                if (definition == null)
                {
                    continue;
                }

                var totalDuration = definition.DurationSeconds;
                var durationRatio = totalDuration > 0f
                    ? Mathf.Clamp01(activeEffect.RemainingDuration / totalDuration)
                    : 1f;
                summaries.Add(new BreakoutTimedEffectStackSummary(
                    definition,
                    activeEffect.StackCount,
                    activeEffect.EffectMultiplier,
                    activeEffect.RemainingDuration,
                    durationRatio));
            }

            return summaries;
        }

        public void EvaluateCapsuleMadnessActivation()
        {
            if (ActivePickups.Count < CapsuleMadnessPickupThreshold)
            {
                capsuleMadnessThresholdArmed = true;
                return;
            }

            if (!capsuleMadnessThresholdArmed || IsCapsuleMadnessActive)
            {
                return;
            }

            CapsuleMadnessTimer = CapsuleMadnessDurationSeconds;
            capsuleMadnessThresholdArmed = false;
            ShowStatusBanner("CAPSULE MADNESS!!", new Color(1f, 0.87f, 0.36f, 1f), CapsuleMadnessDurationSeconds);
        }

        private static bool IsDropAllowed(RunSettings activeRunSettings, BreakoutRunState activeRunState, PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
            {
                return false;
            }

            if (activeRunSettings != null
                && activeRunSettings.IsRogueMode
                && (activeRunState == null || !activeRunState.IsDropUnlocked(powerUpDefinition)))
            {
                return false;
            }

            var dropPoolMode = activeRunSettings?.DropPoolMode ?? DropPoolMode.Mixed;

            return dropPoolMode switch
            {
                DropPoolMode.HelpfulOnly => powerUpDefinition.IsBeneficial,
                DropPoolMode.HarmfulOnly => !powerUpDefinition.IsBeneficial,
                DropPoolMode.Disabled => false,
                _ => true,
            };
        }

        private static BreakoutPowerUpDropCandidate[] BuildDropCandidates(
            BrickDefinition brickDefinition,
            RunSettings activeRunSettings,
            BreakoutRunState activeRunState)
        {
            if (activeRunSettings != null && activeRunSettings.IsRogueMode && activeRunState != null)
            {
                var unlockedDrops = activeRunState.UnlockedDropDefinitions;
                var candidates = new List<BreakoutPowerUpDropCandidate>();

                for (var index = 0; index < unlockedDrops.Count; index++)
                {
                    var definition = unlockedDrops[index];

                    if (definition == null)
                    {
                        continue;
                    }

                    candidates.Add(new BreakoutPowerUpDropCandidate(definition, ResolveRogueDropWeight(definition, activeRunSettings)));
                }

                return candidates.ToArray();
            }

            var dropTable = brickDefinition.DropTable;
            var authoredCandidates = new BreakoutPowerUpDropCandidate[dropTable.Length];

            for (var index = 0; index < dropTable.Length; index++)
            {
                var definition = dropTable[index].PowerUpDefinition;
                var rarityMultiplier = definition != null
                    ? BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity)
                    : 0f;
                authoredCandidates[index] = new BreakoutPowerUpDropCandidate(definition, dropTable[index].Weight * rarityMultiplier);
            }

            return authoredCandidates;
        }

        private static float ResolveRogueDropWeight(PowerUpDefinition definition, RunSettings activeRunSettings)
        {
            if (definition == null)
            {
                return 0f;
            }

            var heatProgress = activeRunSettings != null
                ? BreakoutRunProgression.GetRogueIntensityProgress(activeRunSettings.RogueIntensity)
                : 0f;

            var polarityWeight = definition.IsBeneficial
                ? Mathf.Lerp(1.08f, 0.86f, heatProgress)
                : Mathf.Lerp(0.38f, 1.35f, heatProgress);

            return polarityWeight * BreakoutRarityRules.GetDropWeightMultiplier(definition.Rarity);
        }

        private PowerUpPickup CreatePickup(
            Vector2 position,
            PowerUpDefinition powerUpDefinition,
            PowerUpDefinition visualPowerUpDefinition,
            bool usesHelpfulVisualDisguise,
            PowerUpDefinition primaryPayloadDefinition,
            PowerUpDefinition secondaryPayloadDefinition,
            Transform pickupsRoot,
            float arenaBottom,
            BreakoutThemeService themeService,
            BreakoutGameController controller,
            float pickupFallSpeedMultiplier)
        {
            var pickupObject = new GameObject(powerUpDefinition.DisplayName);
            pickupObject.transform.SetParent(pickupsRoot, false);
            pickupObject.transform.position = position;
            pickupObject.transform.localScale = new Vector3(pickupSize.x, pickupSize.y, 1f);
            pickupObject.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 14;
            spriteRenderer.sharedMaterial = pickupMaterial;

            pickupObject.AddComponent<BoxCollider2D>();
            pickupObject.AddComponent<Rigidbody2D>();

            var pickup = pickupObject.AddComponent<PowerUpPickup>();
            var resolvedVisualPowerUp = visualPowerUpDefinition != null ? visualPowerUpDefinition : powerUpDefinition;
            var pickupStyle = ResolvePickupVisualStyle(resolvedVisualPowerUp, usesHelpfulVisualDisguise, themeService);
            pickup.Configure(
                controller,
                powerUpDefinition,
                pickupFallSpeed * Mathf.Clamp(pickupFallSpeedMultiplier, 0.35f, 1.5f),
                arenaBottom - 0.9f,
                ResolvePickupStartingRotation(powerUpDefinition),
                ResolvePickupSpinDegreesPerSecond(powerUpDefinition),
                pickupStyle,
                resolvedVisualPowerUp,
                usesHelpfulVisualDisguise,
                primaryPayloadDefinition,
                secondaryPayloadDefinition);
            ActivePickups.Add(pickup);
            EvaluateCapsuleMadnessActivation();
            return pickup;
        }

        private static bool TryResolveRandomHarmfulDrop(
            PowerUpDefinition bogusTapeDefinition,
            BreakoutPowerUpDropCandidate[] dropTable,
            RunSettings activeRunSettings,
            BreakoutRunState activeRunState,
            System.Func<float, float, float> nextGameplayRandomFloat,
            bool ignoreDropAllowed,
            out PowerUpDefinition harmfulDefinition,
            out PowerUpDefinition visualDefinition,
            out bool usesHelpfulVisualDisguise)
        {
            harmfulDefinition = null;
            visualDefinition = null;
            usesHelpfulVisualDisguise = false;

            if (nextGameplayRandomFloat == null)
            {
                return false;
            }

            var harmfulCandidates = new List<BreakoutPowerUpDropCandidate>();
            var helpfulCandidates = new List<BreakoutPowerUpDropCandidate>();

            for (var index = 0; index < dropTable.Length; index++)
            {
                var candidate = dropTable[index];
                var definition = candidate.Definition;

                if (!ignoreDropAllowed && !IsDropAllowed(activeRunSettings, activeRunState, definition))
                {
                    continue;
                }

                if (definition.IsBeneficial)
                {
                    helpfulCandidates.Add(candidate);
                    continue;
                }

                if (definition != bogusTapeDefinition
                    && definition.EffectType != PowerUpEffectType.RandomHarmfulDrop)
                {
                    harmfulCandidates.Add(candidate);
                }
            }

            if (harmfulCandidates.Count == 0 || helpfulCandidates.Count == 0)
            {
                return false;
            }

            harmfulDefinition = PickWeightedDefinition(harmfulCandidates, nextGameplayRandomFloat);
            visualDefinition = PickWeightedDefinition(helpfulCandidates, nextGameplayRandomFloat);
            usesHelpfulVisualDisguise = harmfulDefinition != null && visualDefinition != null;
            return usesHelpfulVisualDisguise;
        }

        private static bool TryResolveRandomMixedDrop(
            PowerUpDefinition mysteryDropDefinition,
            BreakoutPowerUpDropCandidate[] dropTable,
            RunSettings activeRunSettings,
            BreakoutRunState activeRunState,
            System.Func<float, float, float> nextGameplayRandomFloat,
            bool ignoreDropAllowed,
            out PowerUpDefinition helpfulDefinition,
            out PowerUpDefinition harmfulDefinition)
        {
            helpfulDefinition = null;
            harmfulDefinition = null;

            if (nextGameplayRandomFloat == null)
            {
                return false;
            }

            var helpfulCandidates = new List<BreakoutPowerUpDropCandidate>();
            var harmfulCandidates = new List<BreakoutPowerUpDropCandidate>();

            for (var index = 0; index < dropTable.Length; index++)
            {
                var candidate = dropTable[index];
                var definition = candidate.Definition;

                if (!ignoreDropAllowed && !IsDropAllowed(activeRunSettings, activeRunState, definition))
                {
                    continue;
                }

                if (definition == mysteryDropDefinition
                    || definition == null
                    || definition.EffectType == PowerUpEffectType.RandomMixedDrop
                    || definition.EffectType == PowerUpEffectType.RandomHarmfulDrop)
                {
                    continue;
                }

                if (definition.IsBeneficial)
                {
                    helpfulCandidates.Add(candidate);
                }
                else
                {
                    harmfulCandidates.Add(candidate);
                }
            }

            if (helpfulCandidates.Count == 0 || harmfulCandidates.Count == 0)
            {
                return false;
            }

            helpfulDefinition = PickWeightedDefinition(helpfulCandidates, nextGameplayRandomFloat);
            harmfulDefinition = PickWeightedDefinition(harmfulCandidates, nextGameplayRandomFloat);
            return helpfulDefinition != null && harmfulDefinition != null;
        }

        private static BreakoutPowerUpDropCandidate[] BuildForcedDropCandidateTable(
            PowerUpDefinition forcedDropDefinition,
            IReadOnlyList<PowerUpDefinition> forcedDropCandidatePool,
            RunSettings activeRunSettings)
        {
            var candidates = new List<BreakoutPowerUpDropCandidate>();

            if (forcedDropCandidatePool != null)
            {
                for (var index = 0; index < forcedDropCandidatePool.Count; index++)
                {
                    var definition = forcedDropCandidatePool[index];

                    if (definition != null)
                    {
                        candidates.Add(new BreakoutPowerUpDropCandidate(definition, ResolveRogueDropWeight(definition, activeRunSettings)));
                    }
                }
            }

            var forcedDropId = BreakoutPowerUpIdentity.GetStableId(forcedDropDefinition);
            var containsForcedDrop = false;

            for (var index = 0; index < candidates.Count; index++)
            {
                if (string.Equals(
                    BreakoutPowerUpIdentity.GetStableId(candidates[index].Definition),
                    forcedDropId,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    containsForcedDrop = true;
                    break;
                }
            }

            if (!containsForcedDrop && forcedDropDefinition != null)
            {
                candidates.Add(new BreakoutPowerUpDropCandidate(forcedDropDefinition, ResolveRogueDropWeight(forcedDropDefinition, activeRunSettings)));
            }

            return candidates.ToArray();
        }

        private static PowerUpDefinition PickWeightedDefinition(
            List<BreakoutPowerUpDropCandidate> candidates,
            System.Func<float, float, float> nextGameplayRandomFloat)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            if (candidates.Count == 1)
            {
                return candidates[0].Definition;
            }

            var totalWeight = 0f;

            for (var index = 0; index < candidates.Count; index++)
            {
                totalWeight += Mathf.Max(0f, candidates[index].Weight);
            }

            if (totalWeight <= 0f)
            {
                return candidates[candidates.Count - 1].Definition;
            }

            var roll = nextGameplayRandomFloat(0f, totalWeight);

            for (var index = 0; index < candidates.Count; index++)
            {
                roll -= Mathf.Max(0f, candidates[index].Weight);

                if (roll <= 0f)
                {
                    return candidates[index].Definition;
                }
            }

            return candidates[candidates.Count - 1].Definition;
        }

        private static ThemeVisualStyle ResolvePickupVisualStyle(
            PowerUpDefinition visualPowerUpDefinition,
            bool usesHelpfulVisualDisguise,
            BreakoutThemeService themeService)
        {
            if (themeService != null)
            {
                return usesHelpfulVisualDisguise
                    ? themeService.ResolveHelpfulPickupDisguiseStyle(visualPowerUpDefinition)
                    : themeService.ResolvePowerUpStyle(visualPowerUpDefinition);
            }

            return visualPowerUpDefinition != null
                ? new ThemeVisualStyle(visualPowerUpDefinition.PickupColor, visualPowerUpDefinition.PickupColor, null)
                : new ThemeVisualStyle(Color.white, Color.white, null);
        }

        private void UpdateCapsuleMadnessThresholdArming()
        {
            if (ActivePickups.Count < CapsuleMadnessPickupThreshold)
            {
                capsuleMadnessThresholdArmed = true;
            }
        }

        private int AddTimedEffect(PowerUpDefinition powerUpDefinition)
        {
            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];

                if (activeEffect.Definition != powerUpDefinition)
                {
                    continue;
                }

                activeEffect.AddStack(powerUpDefinition.DurationSeconds);
                return activeEffect.StackCount;
            }

            ActiveTimedEffects.Add(new BreakoutActiveTimedEffect(powerUpDefinition, powerUpDefinition.DurationSeconds));
            return 1;
        }

        private int MultiplyActiveTimedEffects(float effectMultiplier)
        {
            if (ActiveTimedEffects.Count == 0)
            {
                return 0;
            }

            var multipliedCount = 0;

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];

                if (activeEffect?.Definition == null)
                {
                    continue;
                }

                activeEffect.MultiplyEffect(effectMultiplier);
                multipliedCount++;
            }

            return multipliedCount;
        }

        private int ResolveBankBonusChargePerWallBounce()
        {
            var totalCharge = 0f;

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];
                var definition = activeEffect?.Definition;

                if (definition == null || definition.EffectType != PowerUpEffectType.BankBonus)
                {
                    continue;
                }

                totalCharge += Mathf.Max(1f, definition.Scalar) * activeEffect.EffectStrength;
            }

            return Mathf.Max(0, Mathf.RoundToInt(totalCharge));
        }

        private void ShowPickupBanner(PowerUpDefinition powerUpDefinition, BreakoutThemeService themeService, int stackCount)
        {
            PickupBannerText = powerUpDefinition.IsBeneficial
                ? $"+ {powerUpDefinition.DisplayName}"
                : $"- {powerUpDefinition.DisplayName}";

            if (stackCount > 1)
            {
                PickupBannerText = $"{PickupBannerText} x{stackCount}";
            }

            PickupBannerColor = themeService != null
                ? themeService.ResolvePowerUpStyle(powerUpDefinition).PrimaryColor
                : Color.white;
            PickupBannerTimer = 1.6f;
        }

        internal static string FormatMultiplier(float multiplier)
        {
            var clampedMultiplier = Mathf.Max(0.1f, multiplier);

            if (Mathf.Abs(clampedMultiplier - Mathf.Round(clampedMultiplier)) < 0.001f)
            {
                return Mathf.RoundToInt(clampedMultiplier).ToString();
            }

            return clampedMultiplier.ToString("0.##");
        }

        private static float ResolvePickupStartingRotation(PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
            {
                return 45f;
            }

            return 45f + ((((int)powerUpDefinition.EffectType) % 4) * 12f);
        }

        private static float ResolvePickupSpinDegreesPerSecond(PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
            {
                return 150f;
            }

            var baseSpeed = 135f + (((int)powerUpDefinition.EffectType) * 9f);

            if (powerUpDefinition.EffectType == PowerUpEffectType.MultiBallBurst)
            {
                baseSpeed += 24f;
            }

            return powerUpDefinition.IsBeneficial ? baseSpeed : -baseSpeed;
        }
    }
}
