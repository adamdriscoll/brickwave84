using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutBrickosaurusHitResult
    {
        public BreakoutBrickosaurusHitResult(
            bool handled,
            bool damaged,
            bool layerDestroyed,
            bool defeated,
            Vector2 bounceDirection,
            float speedBurstMultiplier,
            float speedBurstDuration,
            int scorePoints,
            string callout,
            Color calloutColor)
        {
            Handled = handled;
            Damaged = damaged;
            LayerDestroyed = layerDestroyed;
            Defeated = defeated;
            BounceDirection = bounceDirection;
            SpeedBurstMultiplier = Mathf.Max(1f, speedBurstMultiplier);
            SpeedBurstDuration = Mathf.Max(0f, speedBurstDuration);
            ScorePoints = Mathf.Max(0, scorePoints);
            Callout = callout ?? string.Empty;
            CalloutColor = calloutColor;
        }

        public bool Handled { get; }

        public bool Damaged { get; }

        public bool LayerDestroyed { get; }

        public bool Defeated { get; }

        public Vector2 BounceDirection { get; }

        public float SpeedBurstMultiplier { get; }

        public float SpeedBurstDuration { get; }

        public int ScorePoints { get; }

        public string Callout { get; }

        public Color CalloutColor { get; }
    }

    internal sealed class BreakoutBrickosaurusWrecksBoss : MonoBehaviour
    {
        private const float ShieldSpeedBurstMultiplier = 1.24f;
        private const float ShieldSpeedBurstSeconds = 1.65f;
        private const float SegmentSpacingMultiplier = 0.58f;
        private const float BodyBounceJitterDegrees = 12f;
        private const float HeadSpinDegreesPerSecond = 900f;
        private const float SwoopWarningSeconds = 0.42f;
        private const float SwoopDiveSeconds = 0.78f;
        private const float SwoopRecoverSeconds = 0.56f;
        private const float SwoopTargetBottomClearance = 3.2f;
        private const float BodyBottomClearance = 2.65f;

        private readonly List<BreakoutBrickosaurusPart> bodyParts = new List<BreakoutBrickosaurusPart>();

        private BreakoutGameController controller;
        private Transform projectileRoot;
        private Sprite bodySprite;
        private Sprite headSprite;
        private Sprite spikeSprite;
        private Sprite projectileSprite;
        private Material spriteMaterial;
        private Material projectileMaterial;
        private PhysicsMaterial2D physicsMaterial;
        private Rect arenaBounds;
        private Vector2 segmentSize;
        private Vector2 headSize;
        private Func<float, float, float> randomRangeResolver;
        private BreakoutBrickosaurusPart headPart;
        private float homeY;
        private float sweepPhase;
        private float slitherPhase;
        private float previousHeadX;
        private float movementDirection = 1f;
        private float powerDownTimer;
        private float swoopTimer;
        private float swoopIntervalTimer;
        private Vector2 swoopStartPosition;
        private Vector2 swoopTargetPosition;
        private Vector2 currentHeadPosition;
        private float headSpinRotation;
        private int gateIndex;
        private int initialBodySegmentCount;
        private BreakoutBrickosaurusSwoopState swoopState;
        private bool isDefeated;

        public int ConnectedBodySegmentCount => CountConnectedBodySegments();

        public float BodyCompletionRatio => initialBodySegmentCount <= 0
            ? 0f
            : ConnectedBodySegmentCount / (float)initialBodySegmentCount;

        public bool IsHeadVulnerable => !isDefeated && ConnectedBodySegmentCount == 0;

        public int HeadHitsRemaining => headPart != null ? headPart.HitsRemaining : 0;

        public bool IsDefeated => isDefeated;

        internal bool IsSwooping => swoopState != BreakoutBrickosaurusSwoopState.Idle;

        internal float PowerDownTimer => powerDownTimer;

        public string PhaseLabel
        {
            get
            {
                if (isDefeated)
                {
                    return "Wrecked";
                }

                if (IsHeadVulnerable)
                {
                    return "Head Spin";
                }

                if (IsSwooping)
                {
                    return "Swoop";
                }

                var ratio = BodyCompletionRatio;
                if (ratio <= 0.5f)
                {
                    return "Turbo Slither";
                }

                return ratio <= 0.75f ? "Bogus Spit" : "Slither";
            }
        }

        public void Configure(
            BreakoutGameController gameController,
            Transform powerDownRoot,
            Sprite bodyPartSprite,
            Sprite headPartSprite,
            Sprite scaleSpikeSprite,
            Sprite powerDownSprite,
            Material bodyMaterial,
            Material powerDownMaterial,
            PhysicsMaterial2D sharedPhysicsMaterial,
            Rect playfieldBounds,
            Vector2 baseBrickSize,
            int bossGateIndex,
            Func<float, float, float> randomRange)
        {
            controller = gameController;
            projectileRoot = powerDownRoot != null ? powerDownRoot : transform.parent;
            bodySprite = bodyPartSprite;
            headSprite = headPartSprite != null ? headPartSprite : bodyPartSprite;
            spikeSprite = scaleSpikeSprite != null ? scaleSpikeSprite : bodyPartSprite;
            projectileSprite = powerDownSprite;
            spriteMaterial = bodyMaterial;
            projectileMaterial = powerDownMaterial;
            physicsMaterial = sharedPhysicsMaterial;
            arenaBounds = playfieldBounds;
            gateIndex = Mathf.Max(0, bossGateIndex);
            randomRangeResolver = randomRange;
            var segmentDiameter = baseBrickSize.x * 0.56f;
            segmentSize = new Vector2(segmentDiameter, segmentDiameter);
            var headDiameter = baseBrickSize.x * 1.18f;
            headSize = new Vector2(headDiameter, headDiameter);
            homeY = Mathf.Lerp(0.75f, arenaBounds.yMax - 1.05f, 0.72f);
            initialBodySegmentCount = Mathf.Clamp(12 + (gateIndex * 2), 12, 16);
            BuildBossParts();
            powerDownTimer = BuildNextPowerDownSeconds(initialDelay: true);
            swoopIntervalTimer = BuildNextSwoopIntervalSeconds(initialDelay: true);
        }

        public BreakoutBrickosaurusHitResult TryHandlePartHit(
            BreakoutBrickosaurusPart part,
            BallController ball,
            Collision2D collision)
        {
            if (isDefeated || part == null || ball == null)
            {
                return default;
            }

            var bounceDirection = BuildBounceDirection(part, ball, collision);

            if (part.IsHead && !IsHeadVulnerable)
            {
                return new BreakoutBrickosaurusHitResult(
                    true,
                    false,
                    false,
                    false,
                    bounceDirection,
                    1f,
                    0f,
                    0,
                    string.Empty,
                    Color.white);
            }

            var layer = part.Layer;
            if (!part.IsHead)
            {
                bounceDirection = ApplyBodyBounceJitter(bounceDirection);
            }

            var destroyedLayer = part.ApplyHit();
            var scorePoints = destroyedLayer ? ResolveLayerScore(layer) : 0;
            var defeated = false;
            var callout = string.Empty;
            var calloutColor = Color.white;

            if (layer == BreakoutBrickosaurusLayer.Shield)
            {
                callout = "SHIELD SURGE!";
                calloutColor = new Color(0.1f, 0.9f, 1f, 1f);
            }
            else if (layer == BreakoutBrickosaurusLayer.Scale)
            {
                bounceDirection = BuildScaleRicochetDirection();
                callout = "RIOT!";
                calloutColor = new Color(1f, 0.47f, 0.86f, 1f);
            }

            if (destroyedLayer)
            {
                ResolveDestroyedLayer(part, layer, out defeated);
            }

            return new BreakoutBrickosaurusHitResult(
                true,
                true,
                destroyedLayer,
                defeated,
                bounceDirection,
                layer == BreakoutBrickosaurusLayer.Shield ? ShieldSpeedBurstMultiplier : 1f,
                layer == BreakoutBrickosaurusLayer.Shield ? ShieldSpeedBurstSeconds : 0f,
                scorePoints,
                callout,
                calloutColor);
        }

        private void FixedUpdate()
        {
            if (isDefeated || headPart == null)
            {
                return;
            }

            var speed = ResolveSlitherSpeed();
            sweepPhase += Time.fixedDeltaTime * speed;
            slitherPhase += Time.fixedDeltaTime * (4.2f + (speed * 1.1f));

            var previousX = previousHeadX;
            var sweep = Mathf.Sin(sweepPhase);
            var headX = Mathf.Lerp(arenaBounds.xMin + 0.9f, arenaBounds.xMax - 0.9f, (sweep + 1f) * 0.5f);

            if (Mathf.Abs(headX - previousX) > 0.001f)
            {
                movementDirection = Mathf.Sign(headX - previousX);
            }

            previousHeadX = headX;
            var homeHeadPosition = new Vector2(
                headX,
                Mathf.Clamp(homeY + (Mathf.Sin(slitherPhase * 0.54f) * 0.16f), 0.55f, arenaBounds.yMax - 0.48f));
            var resolvedHeadPosition = UpdateSwoop(homeHeadPosition);
            var headRotation = IsHeadVulnerable
                ? UpdateHeadSpinRotation()
                : Mathf.Sin(slitherPhase) * 4.5f + ResolveSwoopRotationOffset();
            headPart.SetVisualFlipX(movementDirection < 0f && !IsHeadVulnerable);
            headPart.SetWorldPose(resolvedHeadPosition, headRotation);

            var connectedCount = ConnectedBodySegmentCount;
            var spacing = segmentSize.x * SegmentSpacingMultiplier;

            for (var index = 0; index < bodyParts.Count; index++)
            {
                var part = bodyParts[index];

                if (part == null || part.IsDestroyed || index >= connectedCount)
                {
                    continue;
                }

                var wave = slitherPhase - (index * 0.72f);
                var x = resolvedHeadPosition.x - (movementDirection * spacing * (index + 1)) + (Mathf.Sin(wave) * 0.19f);
                var y = ClampBodyY(resolvedHeadPosition.y - 0.16f + (Mathf.Sin(wave * 1.08f) * 0.3f));
                var rotation = Mathf.Sin(wave * 1.18f) * 13f;
                part.SetWorldPose(new Vector2(Mathf.Clamp(x, arenaBounds.xMin + 0.42f, arenaBounds.xMax - 0.42f), y), rotation);
            }

            UpdatePowerDownTimer();
        }

        private float UpdateHeadSpinRotation()
        {
            headSpinRotation = Mathf.Repeat(headSpinRotation + (HeadSpinDegreesPerSecond * Time.fixedDeltaTime), 360f);
            return headSpinRotation;
        }

        private void UpdatePowerDownTimer()
        {
            powerDownTimer = Mathf.Max(0f, powerDownTimer - Time.fixedDeltaTime);

            if (powerDownTimer > 0f)
            {
                return;
            }

            SpawnPowerDown();
            powerDownTimer = BuildNextPowerDownSeconds(initialDelay: false);
        }

        private Vector2 UpdateSwoop(Vector2 homeHeadPosition)
        {
            if (IsHeadVulnerable)
            {
                swoopState = BreakoutBrickosaurusSwoopState.Idle;
                swoopTimer = 0f;
                currentHeadPosition = homeHeadPosition;
                return homeHeadPosition;
            }

            if (swoopState == BreakoutBrickosaurusSwoopState.Idle)
            {
                currentHeadPosition = homeHeadPosition;
                swoopIntervalTimer = Mathf.Max(0f, swoopIntervalTimer - Time.fixedDeltaTime);

                if (swoopIntervalTimer <= 0f)
                {
                    StartSwoop(homeHeadPosition);
                }

                return currentHeadPosition;
            }

            swoopTimer = Mathf.Max(0f, swoopTimer - Time.fixedDeltaTime);

            switch (swoopState)
            {
                case BreakoutBrickosaurusSwoopState.Warning:
                    currentHeadPosition = Vector2.Lerp(homeHeadPosition, swoopStartPosition + (Vector2.up * 0.18f), 0.65f);

                    if (swoopTimer <= 0f)
                    {
                        swoopState = BreakoutBrickosaurusSwoopState.Diving;
                        swoopTimer = SwoopDiveSeconds;
                    }

                    break;
                case BreakoutBrickosaurusSwoopState.Diving:
                    var diveProgress = 1f - Mathf.Clamp01(swoopTimer / SwoopDiveSeconds);
                    currentHeadPosition = Vector2.Lerp(swoopStartPosition, swoopTargetPosition, SmoothStep(diveProgress));

                    if (swoopTimer <= 0f)
                    {
                        swoopState = BreakoutBrickosaurusSwoopState.Recovering;
                        swoopTimer = SwoopRecoverSeconds;
                    }

                    break;
                case BreakoutBrickosaurusSwoopState.Recovering:
                    var recoverProgress = 1f - Mathf.Clamp01(swoopTimer / SwoopRecoverSeconds);
                    currentHeadPosition = Vector2.Lerp(swoopTargetPosition, homeHeadPosition, SmoothStep(recoverProgress));

                    if (swoopTimer <= 0f)
                    {
                        swoopState = BreakoutBrickosaurusSwoopState.Idle;
                        swoopIntervalTimer = BuildNextSwoopIntervalSeconds(initialDelay: false);
                        currentHeadPosition = homeHeadPosition;
                    }

                    break;
            }

            return currentHeadPosition;
        }

        private void StartSwoop(Vector2 homeHeadPosition)
        {
            swoopState = BreakoutBrickosaurusSwoopState.Warning;
            swoopTimer = SwoopWarningSeconds;
            swoopStartPosition = homeHeadPosition;
            var targetX = Mathf.Clamp(
                homeHeadPosition.x + RandomRange(-1.65f, 1.65f),
                arenaBounds.xMin + 0.95f,
                arenaBounds.xMax - 0.95f);
            var targetY = arenaBounds.yMin + SwoopTargetBottomClearance;
            swoopTargetPosition = new Vector2(targetX, Mathf.Clamp(targetY, arenaBounds.yMin + 2.9f, arenaBounds.yMax - 1.35f));
        }

        private float ResolveSwoopRotationOffset()
        {
            return swoopState switch
            {
                BreakoutBrickosaurusSwoopState.Warning => -8f,
                BreakoutBrickosaurusSwoopState.Diving => -18f,
                BreakoutBrickosaurusSwoopState.Recovering => 10f,
                _ => 0f,
            };
        }

        private void SpawnPowerDown()
        {
            if (controller == null || projectileSprite == null || projectileRoot == null || headPart == null)
            {
                return;
            }

            var mouthOffset = new Vector2(movementDirection * headSize.x * 0.48f, -headSize.y * 0.04f);
            var projectileObject = new GameObject("Brickosaurus Bogus Shot");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = (Vector2)headPart.transform.position + mouthOffset;

            var direction = new Vector2(RandomRange(-0.44f, 0.44f), -1f).normalized;
            var projectile = projectileObject.AddComponent<BreakoutBrickosaurusPowerDownProjectile>();
            projectile.Configure(
                controller,
                projectileSprite,
                projectileMaterial,
                physicsMaterial,
                direction,
                2.9f + (gateIndex * 0.22f),
                arenaBounds.yMin - 0.8f);
        }

        private void BuildBossParts()
        {
            bodyParts.Clear();
            headPart = CreatePart("Brickosaurus Head", -1, true, BreakoutBrickosaurusLayer.Head, headSize);

            for (var index = 0; index < initialBodySegmentCount; index++)
            {
                bodyParts.Add(CreatePart($"Brickosaurus Body {index + 1:00}", index, false, BreakoutBrickosaurusLayer.Shield, segmentSize));
            }

            SyncPartPoses();
        }

        private BreakoutBrickosaurusPart CreatePart(
            string objectName,
            int segmentIndex,
            bool isHead,
            BreakoutBrickosaurusLayer layer,
            Vector2 worldSize)
        {
            var partObject = new GameObject(objectName);
            partObject.transform.SetParent(transform, false);

            if (!isHead)
            {
                partObject.AddComponent<BreakoutGlowRenderer>();
            }

            var part = partObject.AddComponent<BreakoutBrickosaurusPart>();
            part.Configure(
                this,
                segmentIndex,
                isHead,
                layer,
                bodySprite,
                headSprite,
                spikeSprite,
                spriteMaterial,
                physicsMaterial,
                worldSize);
            return part;
        }

        private void SyncPartPoses()
        {
            if (headPart == null)
            {
                return;
            }

            var sweep = Mathf.Sin(sweepPhase);
            var headX = Mathf.Lerp(arenaBounds.xMin + 0.9f, arenaBounds.xMax - 0.9f, (sweep + 1f) * 0.5f);
            previousHeadX = headX;
            var headY = Mathf.Clamp(homeY + (Mathf.Sin(slitherPhase * 0.54f) * 0.16f), 0.55f, arenaBounds.yMax - 0.48f);
            var headRotation = 0f;
            headPart.SetVisualFlipX(movementDirection < 0f);
            headPart.SetWorldPose(new Vector2(headX, headY), headRotation);

            var spacing = segmentSize.x * SegmentSpacingMultiplier;

            for (var index = 0; index < bodyParts.Count; index++)
            {
                var part = bodyParts[index];

                if (part == null || part.IsDestroyed)
                {
                    continue;
                }

                var wave = slitherPhase - (index * 0.72f);
                var x = headX - (movementDirection * spacing * (index + 1)) + (Mathf.Sin(wave) * 0.19f);
                var y = ClampBodyY(homeY - 0.16f + (Mathf.Sin(wave * 1.08f) * 0.3f));
                var rotation = Mathf.Sin(wave * 1.18f) * 13f;
                part.SetWorldPose(new Vector2(Mathf.Clamp(x, arenaBounds.xMin + 0.42f, arenaBounds.xMax - 0.42f), y), rotation);
            }
        }

        private void ResolveDestroyedLayer(BreakoutBrickosaurusPart part, BreakoutBrickosaurusLayer destroyedLayer, out bool defeated)
        {
            defeated = false;

            if (part.IsHead)
            {
                part.MarkDestroyed();
                isDefeated = true;
                defeated = true;
                return;
            }

            switch (destroyedLayer)
            {
                case BreakoutBrickosaurusLayer.Shield:
                    part.RevealLayer(BreakoutBrickosaurusLayer.Scale);
                    break;
                case BreakoutBrickosaurusLayer.Scale:
                    part.RevealLayer(BreakoutBrickosaurusLayer.Core);
                    break;
                default:
                    RemoveSegmentAndDisconnectedTail(part.SegmentIndex);
                    break;
            }
        }

        private void RemoveSegmentAndDisconnectedTail(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= bodyParts.Count)
            {
                return;
            }

            for (var index = segmentIndex; index < bodyParts.Count; index++)
            {
                bodyParts[index]?.MarkDestroyed();
            }
        }

        private int CountConnectedBodySegments()
        {
            var count = 0;

            for (var index = 0; index < bodyParts.Count; index++)
            {
                var part = bodyParts[index];

                if (part == null || part.IsDestroyed)
                {
                    break;
                }

                count++;
            }

            return count;
        }

        private float ResolveSlitherSpeed()
        {
            var baseSpeed = 0.82f + (gateIndex * 0.08f);

            if (IsHeadVulnerable)
            {
                return baseSpeed * 1.18f;
            }

            return BodyCompletionRatio <= 0.5f ? baseSpeed * 1.55f : baseSpeed;
        }

        private Vector2 BuildBounceDirection(BreakoutBrickosaurusPart part, BallController ball, Collision2D collision)
        {
            var contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)ball.transform.position;
            var direction = contactPoint - (Vector2)part.transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = -ball.CurrentVelocity;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.down;
            }

            return direction.normalized;
        }

        private Vector2 BuildScaleRicochetDirection()
        {
            var x = RandomRange(-0.92f, 0.92f);
            var y = RandomRange(-1f, 0.42f);

            if (Mathf.Abs(y) < 0.24f)
            {
                y = y < 0f ? -0.38f : 0.38f;
            }

            return new Vector2(x, y).normalized;
        }

        private Vector2 ApplyBodyBounceJitter(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.down;
            }

            var angleRadians = RandomRange(-BodyBounceJitterDegrees, BodyBounceJitterDegrees) * Mathf.Deg2Rad;
            var sin = Mathf.Sin(angleRadians);
            var cos = Mathf.Cos(angleRadians);
            var normalized = direction.normalized;
            return new Vector2(
                (normalized.x * cos) - (normalized.y * sin),
                (normalized.x * sin) + (normalized.y * cos)).normalized;
        }

        private float BuildNextPowerDownSeconds(bool initialDelay)
        {
            var minimum = initialDelay ? 0.75f : 2.1f;
            var maximum = initialDelay ? 1.35f : 3.55f;

            if (BodyCompletionRatio <= 0.5f || IsHeadVulnerable)
            {
                minimum -= 0.35f;
                maximum -= 0.55f;
            }

            return RandomRange(minimum, maximum);
        }

        private float BuildNextSwoopIntervalSeconds(bool initialDelay)
        {
            var minimum = initialDelay ? 2.4f : 4.8f;
            var maximum = initialDelay ? 3.8f : 7.2f;

            if (BodyCompletionRatio <= 0.5f)
            {
                minimum -= 0.55f;
                maximum -= 0.75f;
            }

            return RandomRange(minimum, maximum);
        }

        private float RandomRange(float minimumInclusive, float maximumInclusive)
        {
            return randomRangeResolver != null
                ? randomRangeResolver(minimumInclusive, maximumInclusive)
                : UnityEngine.Random.Range(minimumInclusive, maximumInclusive);
        }

        private float ClampBodyY(float y)
        {
            return Mathf.Clamp(y, arenaBounds.yMin + BodyBottomClearance, arenaBounds.yMax - 0.55f);
        }

        private static int ResolveLayerScore(BreakoutBrickosaurusLayer layer)
        {
            return layer switch
            {
                BreakoutBrickosaurusLayer.Shield => 120,
                BreakoutBrickosaurusLayer.Scale => 160,
                BreakoutBrickosaurusLayer.Core => 260,
                BreakoutBrickosaurusLayer.Head => 1500,
                _ => 100,
            };
        }

        private static float SmoothStep(float value)
        {
            var t = Mathf.Clamp01(value);
            return t * t * (3f - (2f * t));
        }
    }

    internal enum BreakoutBrickosaurusSwoopState
    {
        Idle = 0,
        Warning = 1,
        Diving = 2,
        Recovering = 3,
    }
}
