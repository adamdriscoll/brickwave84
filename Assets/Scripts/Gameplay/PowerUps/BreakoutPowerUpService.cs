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
            RemainingDuration += Mathf.Max(0f, durationSeconds);
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
            bool stickyPaddleEnabled,
            bool laserPaddleEnabled,
            bool phaseBallEnabled,
            float chainLightningStrength,
            bool reverseControlsEnabled,
            float splitPaddleGapNormalized,
            float gravityWellStrength,
            float fogVisibilityMultiplier,
            float lagSpikeStrength)
        {
            PaddleWidthMultiplier = paddleWidthMultiplier;
            WavyPaddleStrength = wavyPaddleStrength;
            TimedBallSpeedMultiplier = timedBallSpeedMultiplier;
            StickyPaddleEnabled = stickyPaddleEnabled;
            LaserPaddleEnabled = laserPaddleEnabled;
            PhaseBallEnabled = phaseBallEnabled;
            ChainLightningStrength = chainLightningStrength;
            ReverseControlsEnabled = reverseControlsEnabled;
            SplitPaddleGapNormalized = splitPaddleGapNormalized;
            GravityWellStrength = gravityWellStrength;
            FogVisibilityMultiplier = fogVisibilityMultiplier;
            LagSpikeStrength = lagSpikeStrength;
        }

        public float PaddleWidthMultiplier { get; }

        public float WavyPaddleStrength { get; }

        public float TimedBallSpeedMultiplier { get; }

        public bool StickyPaddleEnabled { get; }

        public bool LaserPaddleEnabled { get; }

        public bool PhaseBallEnabled { get; }

        public float ChainLightningStrength { get; }

        public bool ReverseControlsEnabled { get; }

        public float SplitPaddleGapNormalized { get; }

        public float GravityWellStrength { get; }

        public float FogVisibilityMultiplier { get; }

        public float LagSpikeStrength { get; }
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
        private bool stickyPaddleEnabled;
        private bool laserPaddleEnabled;
        private bool phaseBallEnabled;
        private float chainLightningStrength;
        private bool reverseControlsEnabled;
        private float splitPaddleGapNormalized;
        private float gravityWellStrength;
        private float fogVisibilityMultiplier;
        private float lagSpikeStrength;

        public BreakoutEffectModifierAccumulator(float basePaddleWidthMultiplier, float baseWavyPaddleStrength)
        {
            paddleWidthMultiplier = Mathf.Max(0.1f, basePaddleWidthMultiplier);
            wavyPaddleStrength = Mathf.Clamp01(baseWavyPaddleStrength);
            timedBallSpeedMultiplier = 1f;
            stickyPaddleEnabled = false;
            laserPaddleEnabled = false;
            phaseBallEnabled = false;
            chainLightningStrength = 0f;
            reverseControlsEnabled = false;
            splitPaddleGapNormalized = 0f;
            gravityWellStrength = 0f;
            fogVisibilityMultiplier = 1f;
            lagSpikeStrength = 0f;
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
                    fogVisibilityMultiplier = Mathf.Min(fogVisibilityMultiplier, Mathf.Clamp(Mathf.Pow(powerUpDefinition.Scalar, effectStrength), 0.2f, 1f));
                    break;
                case PowerUpEffectType.LagSpike:
                    lagSpikeStrength = Mathf.Max(lagSpikeStrength, Mathf.Clamp01(powerUpDefinition.Scalar * effectStrength));
                    break;
            }
        }

        public BreakoutEffectModifiers ToModifiers()
        {
            return new BreakoutEffectModifiers(
                paddleWidthMultiplier,
                wavyPaddleStrength,
                timedBallSpeedMultiplier,
                stickyPaddleEnabled,
                laserPaddleEnabled,
                phaseBallEnabled,
                chainLightningStrength,
                reverseControlsEnabled,
                splitPaddleGapNormalized,
                gravityWellStrength,
                fogVisibilityMultiplier,
                lagSpikeStrength);
        }
    }

    internal sealed class BreakoutPowerUpService
    {
        private readonly Vector2 pickupSize;
        private readonly float pickupFallSpeed;
        private readonly float multiBallSpreadAngle;
        private readonly Material pickupMaterial;

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

        public void UpdateTimedEffects(bool isPlaying, float deltaTime, System.Action modifiersChanged)
        {
            if (!isPlaying || ActiveTimedEffects.Count == 0)
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

        public void TrySpawnPickup(
            Brick brick,
            RunSettings activeRunSettings,
            float effectiveDropChanceMultiplier,
            System.Func<float, float, float> nextGameplayRandomFloat,
            Transform pickupsRoot,
            float arenaBottom,
            BreakoutThemeService themeService,
            BreakoutGameController controller)
        {
            if (brick == null || brick.Definition == null)
            {
                return;
            }

            var brickDefinition = brick.Definition;
            var dropTable = brickDefinition.DropTable;
            var forcePickupDrops = activeRunSettings?.ForcePickupDropsOnBreak == true;
            var effectiveDropChance = forcePickupDrops
                ? 1f
                : Mathf.Clamp01(brickDefinition.DropChance * Mathf.Max(0f, effectiveDropChanceMultiplier));

            if (dropTable.Length == 0
                || effectiveDropChance <= 0f
                || activeRunSettings?.DropPoolMode == DropPoolMode.Disabled
                || nextGameplayRandomFloat == null
                || (!forcePickupDrops && nextGameplayRandomFloat(0f, 1f) > effectiveDropChance))
            {
                return;
            }

            var totalWeight = 0f;

            for (var index = 0; index < dropTable.Length; index++)
            {
                if (!IsDropAllowed(activeRunSettings, dropTable[index].PowerUpDefinition))
                {
                    continue;
                }

                totalWeight += dropTable[index].Weight;
            }

            if (totalWeight <= 0f)
            {
                return;
            }

            var roll = nextGameplayRandomFloat(0f, totalWeight);
            PowerUpDefinition selectedPowerUp = null;

            for (var index = 0; index < dropTable.Length; index++)
            {
                var entry = dropTable[index];

                if (!IsDropAllowed(activeRunSettings, entry.PowerUpDefinition))
                {
                    continue;
                }

                roll -= entry.Weight;

                if (roll > 0f)
                {
                    continue;
                }

                selectedPowerUp = entry.PowerUpDefinition;
                break;
            }

            if (selectedPowerUp == null)
            {
                return;
            }

            CreatePickup((Vector2)brick.transform.position, selectedPowerUp, pickupsRoot, arenaBottom, themeService, controller);
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
        }

        public void ClearTimedEffects()
        {
            ActiveTimedEffects.Clear();
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

                var totalDuration = definition.DurationSeconds * Mathf.Max(1, activeEffect.StackCount);
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

        private static bool IsDropAllowed(RunSettings activeRunSettings, PowerUpDefinition powerUpDefinition)
        {
            if (powerUpDefinition == null)
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

        private void CreatePickup(
            Vector2 position,
            PowerUpDefinition powerUpDefinition,
            Transform pickupsRoot,
            float arenaBottom,
            BreakoutThemeService themeService,
            BreakoutGameController controller)
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
            var pickupStyle = themeService != null
                ? themeService.ResolvePowerUpStyle(powerUpDefinition)
                : new ThemeVisualStyle(powerUpDefinition.PickupColor, powerUpDefinition.PickupColor, null);
            pickup.Configure(
                controller,
                powerUpDefinition,
                pickupFallSpeed,
                arenaBottom - 0.9f,
                ResolvePickupStartingRotation(powerUpDefinition),
                ResolvePickupSpinDegreesPerSecond(powerUpDefinition),
                pickupStyle);
            ActivePickups.Add(pickup);
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
