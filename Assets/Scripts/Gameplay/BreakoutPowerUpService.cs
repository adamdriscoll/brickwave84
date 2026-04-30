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
        }

        public PowerUpDefinition Definition { get; }

        public float RemainingDuration { get; set; }
    }

    internal readonly struct BreakoutEffectModifiers
    {
        public BreakoutEffectModifiers(float paddleWidthMultiplier, float wavyPaddleStrength, float timedBallSpeedMultiplier)
        {
            PaddleWidthMultiplier = paddleWidthMultiplier;
            WavyPaddleStrength = wavyPaddleStrength;
            TimedBallSpeedMultiplier = timedBallSpeedMultiplier;
        }

        public float PaddleWidthMultiplier { get; }

        public float WavyPaddleStrength { get; }

        public float TimedBallSpeedMultiplier { get; }
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
            var effectiveDropChance = Mathf.Clamp01(brickDefinition.DropChance * Mathf.Max(0f, effectiveDropChanceMultiplier));

            if (dropTable.Length == 0
                || effectiveDropChance <= 0f
                || activeRunSettings?.DropPoolMode == DropPoolMode.Disabled
                || nextGameplayRandomFloat == null
                || nextGameplayRandomFloat(0f, 1f) > effectiveDropChance)
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

        public bool ApplyPowerUp(PowerUpDefinition powerUpDefinition, BreakoutThemeService themeService)
        {
            if (powerUpDefinition == null)
            {
                return false;
            }

            ShowPickupBanner(powerUpDefinition, themeService);

            if (powerUpDefinition.IsTimed)
            {
                AddOrExtendTimedEffect(powerUpDefinition);
                return false;
            }

            return powerUpDefinition.EffectType == PowerUpEffectType.MultiBallBurst;
        }

        public BreakoutEffectModifiers CalculateEffectModifiers(RunSettings activeRunSettings)
        {
            var paddleWidthMultiplier = activeRunSettings?.PaddleWidthMultiplier ?? 1f;
            var wavyPaddleStrength = 0f;
            var timedBallSpeedMultiplier = 1f;

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var powerUpDefinition = ActiveTimedEffects[index].Definition;

                if (powerUpDefinition == null)
                {
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.PaddleWidthMultiplier)
                {
                    paddleWidthMultiplier *= powerUpDefinition.Scalar;
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.BallSpeedMultiplier)
                {
                    timedBallSpeedMultiplier *= powerUpDefinition.Scalar;
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.WavyPaddle)
                {
                    wavyPaddleStrength = Mathf.Max(wavyPaddleStrength, powerUpDefinition.Scalar);
                }
            }

            return new BreakoutEffectModifiers(paddleWidthMultiplier, wavyPaddleStrength, timedBallSpeedMultiplier);
        }

        public BreakoutEffectModifiers CalculateEffectModifiers(float basePaddleWidthMultiplier, float baseWavyPaddleStrength)
        {
            var paddleWidthMultiplier = Mathf.Max(0.1f, basePaddleWidthMultiplier);
            var wavyPaddleStrength = Mathf.Clamp01(baseWavyPaddleStrength);
            var timedBallSpeedMultiplier = 1f;

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var powerUpDefinition = ActiveTimedEffects[index].Definition;

                if (powerUpDefinition == null)
                {
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.PaddleWidthMultiplier)
                {
                    paddleWidthMultiplier *= powerUpDefinition.Scalar;
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.BallSpeedMultiplier)
                {
                    timedBallSpeedMultiplier *= powerUpDefinition.Scalar;
                    continue;
                }

                if (powerUpDefinition.EffectType == PowerUpEffectType.WavyPaddle)
                {
                    wavyPaddleStrength = Mathf.Max(wavyPaddleStrength, powerUpDefinition.Scalar);
                }
            }

            return new BreakoutEffectModifiers(paddleWidthMultiplier, wavyPaddleStrength, timedBallSpeedMultiplier);
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
            if (ActiveTimedEffects.Count == 0)
            {
                return "Active Effects: none";
            }

            var builder = new StringBuilder("Active Effects: ");

            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];

                if (activeEffect.Definition == null)
                {
                    continue;
                }

                if (builder.Length > 16)
                {
                    builder.Append(" | ");
                }

                builder.Append(activeEffect.Definition.HudLabel);
                builder.Append(' ');
                builder.Append(activeEffect.RemainingDuration.ToString("0.0"));
                builder.Append('s');
            }

            return builder.ToString();
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
            pickup.Configure(controller, powerUpDefinition, pickupFallSpeed, arenaBottom - 0.9f, pickupStyle);
            ActivePickups.Add(pickup);
        }

        private void AddOrExtendTimedEffect(PowerUpDefinition powerUpDefinition)
        {
            for (var index = 0; index < ActiveTimedEffects.Count; index++)
            {
                var activeEffect = ActiveTimedEffects[index];

                if (activeEffect.Definition != powerUpDefinition)
                {
                    continue;
                }

                activeEffect.RemainingDuration += powerUpDefinition.DurationSeconds;
                return;
            }

            ActiveTimedEffects.Add(new BreakoutActiveTimedEffect(powerUpDefinition, powerUpDefinition.DurationSeconds));
        }

        private void ShowPickupBanner(PowerUpDefinition powerUpDefinition, BreakoutThemeService themeService)
        {
            PickupBannerText = powerUpDefinition.IsBeneficial
                ? $"+ {powerUpDefinition.DisplayName}"
                : $"- {powerUpDefinition.DisplayName}";
            PickupBannerColor = themeService != null
                ? themeService.ResolvePowerUpStyle(powerUpDefinition).PrimaryColor
                : Color.white;
            PickupBannerTimer = 1.6f;
        }
    }
}
