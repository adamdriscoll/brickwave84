using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PaddleController : MonoBehaviour
    {
        private struct DelayedPaddleSample
        {
            public DelayedPaddleSample(float time, Vector2 position, float rotation, Vector3 scale)
            {
                Time = time;
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }

            public float Time { get; }

            public Vector2 Position { get; }

            public float Rotation { get; }

            public Vector3 Scale { get; }
        }

        private const float WavyRetargetMinSeconds = 0.09f;
        private const float WavyRetargetMaxSeconds = 0.22f;
        private const float WavyBlendSpeed = 11f;
        private const float WavyReturnSpeed = 7f;
        private const float WavyMaxVerticalAmplitude = 0.32f;
        private const float WavyMaxRotationDegrees = 16f;
        private const float WavyStrengthEpsilon = 0.001f;
        private const float BreakWiggleDurationSeconds = 0.55f;
        private const float BreakWigglePhaseSpeed = 34f;
        private const float BreakWiggleMaxVerticalAmplitude = 0.18f;
        private const float BreakWiggleMaxRotationDegrees = 7f;
        private const float LagSpikeCycleSeconds = 0.72f;
        private const float LagSpikeMaxPauseSeconds = 0.14f;
        private const float HitTiltCenterDeadZone = 0.08f;
        private const float HitTiltVelocityDeadZone = 0.05f;
        private const float HitTiltMinimumAngleMultiplier = 0.65f;
        private const float HitTiltMaximumAngleMultiplier = 1.35f;
        private const float HitTiltReturnDegreesPerSecond = 220f;
        private const float MaxArenaWidthCoverage = 0.9f;
        private const float ClonePaddleYOffset = 0.74f;
        private const float ClonePaddleWidthMultiplier = 0.62f;
        private const int ClonePaddleSortingOrder = 19;
        private const int CloneStaticPaddleSortingOrder = 18;
        private const float CloneStaticDefaultDelaySeconds = 0.42f;
        private const float CloneStaticDefaultYOffset = 0.9f;
        private const float CloneStaticDefaultWidthMultiplier = 0.72f;
        private const float CloneStaticTintAlpha = 0.58f;
        private const float CloneStaticHistoryPaddingSeconds = 0.25f;
        private const float MirrorImagePaddleYOffset = 1.16f;
        private const int MirrorImagePaddleSortingOrder = 39;
        private const float AutopilotDeadZone = 0.16f;
        private const float AutopilotSlowRadius = 1.15f;
        private const float AutopilotInputBlendSpeed = 5.5f;
        private const float AutopilotMaximumInput = 0.85f;
        private const float WrapRailBoundaryAlpha = 0.5f;
        private const float WrapRailAlphaEpsilon = 0.01f;

        private BreakoutGameController gameController;
        private Rigidbody2D paddleBody;
        private Vector3 baseScale;
        private float moveSpeed;
        private float leftBoundaryX;
        private float rightBoundaryX;
        private float startingY;
        private Vector2 startingPosition;
        private float horizontalInput;
        private float currentWidthMultiplier = 1f;
        private float wavyStrength;
        private float currentWavyYOffset;
        private float currentWavyRotation;
        private float targetWavyYOffset;
        private float targetWavyRotation;
        private float wavyRetargetTimer;
        private float breakWiggleTimer;
        private float breakWigglePhase;
        private float currentBreakWiggleYOffset;
        private float currentBreakWiggleRotation;
        private bool controlsReversed;
        private float splitGapWidthNormalized;
        private float lagSpikeStrength;
        private float hitTiltDegrees;
        private float currentHitTiltRotation;
        private float lastHitTiltDirection = 1f;
        private bool isHitTiltReturning;
        private bool wrapRailEnabled;
        private GameObject wrapRailEchoObject;
        private SpriteRenderer wrapRailEchoRenderer;
        private SpriteRenderer sourcePaddleRenderer;
        private Color sourcePaddleTint = Color.white;
        private bool hasSourcePaddleTint;
        private bool wrapRailSourceTintOverridden;
        private bool isAutopilotEnabled;
        private float autopilotTargetX;
        private float autopilotInput;
        private GameObject clonePaddleObject;
        private BoxCollider2D clonePaddleCollider;
        private SpriteRenderer clonePaddleRenderer;
        private GameObject cloneStaticPaddleObject;
        private Rigidbody2D cloneStaticPaddleBody;
        private SpriteRenderer cloneStaticPaddleRenderer;
        private readonly System.Collections.Generic.List<DelayedPaddleSample> cloneStaticSamples = new System.Collections.Generic.List<DelayedPaddleSample>();
        private bool cloneStaticPaddleEnabled;
        private float cloneStaticDelaySeconds = CloneStaticDefaultDelaySeconds;
        private float cloneStaticYOffset = CloneStaticDefaultYOffset;
        private float cloneStaticWidthMultiplier = CloneStaticDefaultWidthMultiplier;
        private bool isAuxiliaryPaddle;
        private float inputDirectionMultiplier = 1f;
        private GameObject mirrorImagePaddleObject;
        private PaddleController mirrorImagePaddle;

        public float HalfWidthWorld { get; private set; }

        public bool IsBreakWiggleActive => breakWiggleTimer > 0f;

        public void Configure(BreakoutGameController controller, float speed, float minimumBoundaryX, float maximumBoundaryX, float startY)
        {
            gameController = controller;
            moveSpeed = speed;
            leftBoundaryX = minimumBoundaryX;
            rightBoundaryX = maximumBoundaryX;
            startingY = startY;
            startingPosition = new Vector2(0f, startY);
            baseScale = transform.localScale;
            paddleBody = GetComponent<Rigidbody2D>();
            ResetWavyMotion();
            SetWidthMultiplier(1f);
            ResetToStart();
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
            mirrorImagePaddle?.SetMoveSpeed(moveSpeed);
        }

        public void SetAutopilotTarget(float targetX)
        {
            isAutopilotEnabled = true;
            autopilotTargetX = targetX;
        }

        public void ClearAutopilotTarget()
        {
            isAutopilotEnabled = false;
            autopilotTargetX = 0f;
            autopilotInput = 0f;
            horizontalInput = 0f;
        }

        public bool SetWidthMultiplier(float multiplier)
        {
            currentWidthMultiplier = Mathf.Max(0.1f, multiplier);
            var arenaWidth = Mathf.Max(0f, rightBoundaryX - leftBoundaryX);
            var maxWidth = arenaWidth > 0f ? arenaWidth * MaxArenaWidthCoverage : float.PositiveInfinity;
            var requestedWidth = baseScale.x * currentWidthMultiplier;
            var width = Mathf.Min(requestedWidth, maxWidth);
            transform.localScale = new Vector3(width, baseScale.y, baseScale.z);
            HalfWidthWorld = width * 0.5f;
            SyncWrapRailEchoScale();
            ClampToBounds();
            mirrorImagePaddle?.SetWidthMultiplier(currentWidthMultiplier);
            return maxWidth < float.PositiveInfinity && requestedWidth >= maxWidth - 0.0001f;
        }

        public void StartBreakWiggle()
        {
            breakWiggleTimer = BreakWiggleDurationSeconds;
            breakWigglePhase += 1.31f;
        }

        public void SetWavyStrength(float strength)
        {
            wavyStrength = Mathf.Clamp01(strength);

            if (wavyStrength <= WavyStrengthEpsilon)
            {
                ResetWavyMotion();
                ClampToBounds();
                mirrorImagePaddle?.SetWavyStrength(wavyStrength);
                return;
            }

            RetargetWavyMotion();
            mirrorImagePaddle?.SetWavyStrength(wavyStrength);
        }

        public void SetControlsReversed(bool reversed)
        {
            controlsReversed = reversed;
            mirrorImagePaddle?.SetControlsReversed(controlsReversed);
        }

        public void SetSplitGapWidthNormalized(float normalizedWidth)
        {
            splitGapWidthNormalized = Mathf.Clamp(normalizedWidth, 0f, 0.45f);
            mirrorImagePaddle?.SetSplitGapWidthNormalized(splitGapWidthNormalized);
        }

        public void SetLagSpikeStrength(float strength)
        {
            lagSpikeStrength = Mathf.Clamp01(strength);
            mirrorImagePaddle?.SetLagSpikeStrength(lagSpikeStrength);
        }

        public void SetHitTiltDegrees(float degrees)
        {
            var wasHitTiltActive = hitTiltDegrees > WavyStrengthEpsilon;
            hitTiltDegrees = Mathf.Clamp(degrees, 0f, 18f);

            if (hitTiltDegrees > WavyStrengthEpsilon)
            {
                isHitTiltReturning = false;
            }
            else if (wasHitTiltActive || Mathf.Abs(currentHitTiltRotation) > WavyStrengthEpsilon)
            {
                currentHitTiltRotation = Mathf.DeltaAngle(0f, currentHitTiltRotation);
                isHitTiltReturning = Mathf.Abs(currentHitTiltRotation) > WavyStrengthEpsilon;

                if (!isHitTiltReturning)
                {
                    currentHitTiltRotation = 0f;
                    lastHitTiltDirection = 1f;
                    ClampToBounds();
                }
            }

            mirrorImagePaddle?.SetHitTiltDegrees(hitTiltDegrees);
        }

        public void SetWrapRailEnabled(bool enabled)
        {
            wrapRailEnabled = enabled;

            if (!wrapRailEnabled)
            {
                HideWrapRailEcho();
                RestoreWrapRailSourceTint();
            }

            mirrorImagePaddle?.SetWrapRailEnabled(wrapRailEnabled);
        }

        public void ApplyHitTilt(float contactWorldX, Vector2 incomingVelocity)
        {
            if (hitTiltDegrees <= WavyStrengthEpsilon)
            {
                return;
            }

            var normalizedOffset = HalfWidthWorld > 0.001f
                ? Mathf.Clamp((contactWorldX - transform.position.x) / HalfWidthWorld, -1f, 1f)
                : 0f;
            var hasIncomingAngle = incomingVelocity.sqrMagnitude > 0.0001f;
            var incomingDirection = hasIncomingAngle ? incomingVelocity.normalized : Vector2.zero;
            var tiltDirection = hasIncomingAngle && Mathf.Abs(incomingDirection.x) > HitTiltVelocityDeadZone
                ? Mathf.Sign(incomingDirection.x)
                : Mathf.Abs(normalizedOffset) > HitTiltCenterDeadZone
                ? Mathf.Sign(normalizedOffset)
                : lastHitTiltDirection;
            var angleMultiplier = hasIncomingAngle
                ? Mathf.Lerp(
                    HitTiltMinimumAngleMultiplier,
                    HitTiltMaximumAngleMultiplier,
                    Mathf.InverseLerp(HitTiltVelocityDeadZone, 0.92f, Mathf.Abs(incomingDirection.x)))
                : 1f;

            lastHitTiltDirection = Mathf.Approximately(tiltDirection, 0f) ? lastHitTiltDirection : tiltDirection;
            currentHitTiltRotation += hitTiltDegrees * angleMultiplier * lastHitTiltDirection;
        }

        public void SetClonePaddleEnabled(bool enabled)
        {
            EnsureClonePaddle();

            if (clonePaddleObject != null)
            {
                clonePaddleObject.SetActive(enabled);
            }
        }

        internal void SetCloneStaticPaddleEnabled(bool enabled, BreakoutCloneStaticSpec spec = default)
        {
            if (isAuxiliaryPaddle)
            {
                return;
            }

            cloneStaticPaddleEnabled = enabled;

            if (!enabled)
            {
                cloneStaticSamples.Clear();

                if (cloneStaticPaddleObject != null)
                {
                    cloneStaticPaddleObject.SetActive(false);
                }

                return;
            }

            cloneStaticDelaySeconds = spec.DelaySeconds > 0f ? spec.DelaySeconds : CloneStaticDefaultDelaySeconds;
            cloneStaticYOffset = spec.VerticalOffset > 0f ? spec.VerticalOffset : CloneStaticDefaultYOffset;
            cloneStaticWidthMultiplier = spec.WidthMultiplier > 0f ? spec.WidthMultiplier : CloneStaticDefaultWidthMultiplier;
            EnsureCloneStaticPaddle();
            cloneStaticPaddleObject.SetActive(true);
            ResetCloneStaticSamples();
            UpdateCloneStaticPaddle(Time.fixedTime);
        }

        public void SetMirrorImagePaddleEnabled(bool enabled)
        {
            if (isAuxiliaryPaddle)
            {
                return;
            }

            if (enabled)
            {
                EnsureMirrorImagePaddle();
            }

            if (mirrorImagePaddleObject != null)
            {
                mirrorImagePaddleObject.SetActive(enabled);
            }
        }

        public bool IsPointInsideSplitGap(float worldX)
        {
            if (splitGapWidthNormalized <= WavyStrengthEpsilon)
            {
                return false;
            }

            var localOffset = Mathf.Abs(worldX - transform.position.x);
            return localOffset <= HalfWidthWorld * splitGapWidthNormalized * 0.5f;
        }

        public void ResetToStart()
        {
            var resetPosition = startingPosition;

            if (paddleBody == null)
            {
                paddleBody = GetComponent<Rigidbody2D>();
            }

            ResetWavyPoseForCurrentState();
            ResetBreakWiggle();
            currentHitTiltRotation = 0f;
            lastHitTiltDirection = 1f;
            isHitTiltReturning = false;
            transform.SetPositionAndRotation(resetPosition, Quaternion.identity);
            paddleBody.position = resetPosition;
            paddleBody.rotation = 0f;
            paddleBody.linearVelocity = Vector2.zero;
            HideWrapRailEcho();
            RestoreWrapRailSourceTint();
            ResetCloneStaticSamples();
            UpdateCloneStaticPaddle(Time.fixedTime);
            mirrorImagePaddle?.ResetToStart();
        }

        private void OnDestroy()
        {
            if (!isAuxiliaryPaddle && mirrorImagePaddleObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(mirrorImagePaddleObject);
                }
                else
                {
                    DestroyImmediate(mirrorImagePaddleObject);
                }
            }

            if (wrapRailEchoObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(wrapRailEchoObject);
                }
                else
                {
                    DestroyImmediate(wrapRailEchoObject);
                }
            }

            if (!isAuxiliaryPaddle && cloneStaticPaddleObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(cloneStaticPaddleObject);
                }
                else
                {
                    DestroyImmediate(cloneStaticPaddleObject);
                }
            }
        }

        private void Update()
        {
            if (isAutopilotEnabled)
            {
                horizontalInput = UpdateAutopilotInput(Time.deltaTime);
            }
            else
            {
                autopilotInput = 0f;
                horizontalInput = ReadHorizontalInput();
            }

            UpdateWavyMotion(Time.deltaTime);
            UpdateBreakWiggle(Time.deltaTime);
            UpdateHitTiltReturn(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (paddleBody == null)
            {
                return;
            }

            var requestedX = paddleBody.position.x + (horizontalInput * moveSpeed * Time.fixedDeltaTime);
            var nextX = ResolveNextHorizontalPosition(requestedX);

            if (IsLagPauseWindow())
            {
                nextX = paddleBody.position.x;
            }

            var currentRotation = GetCurrentRotation();
            var nextPosition = new Vector2(nextX, startingY + GetCurrentVerticalOffset());

            if (DidWrapRailPosition(requestedX, nextX))
            {
                transform.SetPositionAndRotation(nextPosition, Quaternion.Euler(0f, 0f, currentRotation));
                paddleBody.position = nextPosition;
                paddleBody.rotation = currentRotation;
            }
            else
            {
                paddleBody.MovePosition(nextPosition);
                paddleBody.MoveRotation(currentRotation);
            }

            UpdateWrapRailEcho(nextPosition.x, nextPosition.y, currentRotation);
            RecordCloneStaticSample(Time.fixedTime, nextPosition, currentRotation);
            UpdateCloneStaticPaddle(Time.fixedTime);
        }

        private float ResolveNextHorizontalPosition(float requestedX)
        {
            var minX = leftBoundaryX + HalfWidthWorld;
            var maxX = rightBoundaryX - HalfWidthWorld;

            if (minX > maxX)
            {
                return (leftBoundaryX + rightBoundaryX) * 0.5f;
            }

            if (!wrapRailEnabled)
            {
                return Mathf.Clamp(requestedX, minX, maxX);
            }

            var arenaWidth = rightBoundaryX - leftBoundaryX;

            if (arenaWidth <= 0.001f)
            {
                return Mathf.Clamp(requestedX, minX, maxX);
            }

            if (requestedX > rightBoundaryX)
            {
                return requestedX - arenaWidth;
            }

            if (requestedX < leftBoundaryX)
            {
                return requestedX + arenaWidth;
            }

            return requestedX;
        }

        private bool DidWrapRailPosition(float requestedX, float resolvedX)
        {
            if (!wrapRailEnabled)
            {
                return false;
            }

            var arenaWidth = rightBoundaryX - leftBoundaryX;
            return arenaWidth > 0.001f && Mathf.Abs(requestedX - resolvedX) > arenaWidth * 0.5f;
        }

        private void ClampToBounds()
        {
            if (paddleBody == null)
            {
                return;
            }

            var clampedX = Mathf.Clamp(
                paddleBody.position.x,
                leftBoundaryX + HalfWidthWorld,
                rightBoundaryX - HalfWidthWorld);

            var clampedPosition = new Vector2(clampedX, startingY + GetCurrentVerticalOffset());
            var currentRotation = GetCurrentRotation();
            transform.SetPositionAndRotation(clampedPosition, Quaternion.Euler(0f, 0f, currentRotation));
            paddleBody.position = clampedPosition;
            paddleBody.rotation = currentRotation;
            UpdateWrapRailEcho(clampedPosition.x, clampedPosition.y, currentRotation);
        }

        private void UpdateWrapRailEcho(float currentX, float currentY, float currentRotation)
        {
            if (!wrapRailEnabled || HalfWidthWorld <= 0.001f)
            {
                HideWrapRailEcho();
                RestoreWrapRailSourceTint();
                return;
            }

            var minX = leftBoundaryX + HalfWidthWorld;
            var maxX = rightBoundaryX - HalfWidthWorld;
            var arenaWidth = rightBoundaryX - leftBoundaryX;

            if (minX > maxX || arenaWidth <= 0.001f)
            {
                HideWrapRailEcho();
                RestoreWrapRailSourceTint();
                return;
            }

            if (currentX > maxX)
            {
                var ratio = Mathf.InverseLerp(maxX, rightBoundaryX, currentX);
                ApplyWrapRailVisuals(
                    currentX,
                    currentY,
                    currentRotation,
                    currentX - arenaWidth,
                    Mathf.Lerp(1f, WrapRailBoundaryAlpha, ratio),
                    Mathf.Lerp(0f, WrapRailBoundaryAlpha, ratio));
                return;
            }

            if (currentX < minX)
            {
                var ratio = Mathf.InverseLerp(leftBoundaryX, minX, currentX);
                ApplyWrapRailVisuals(
                    currentX,
                    currentY,
                    currentRotation,
                    currentX + arenaWidth,
                    Mathf.Lerp(WrapRailBoundaryAlpha, 1f, ratio),
                    Mathf.Lerp(WrapRailBoundaryAlpha, 0f, ratio));
                return;
            }

            HideWrapRailEcho();
            RestoreWrapRailSourceTint();
        }

        private void ApplyWrapRailVisuals(
            float currentX,
            float currentY,
            float currentRotation,
            float echoX,
            float sourceAlpha,
            float echoAlpha)
        {
            ApplyWrapRailSourceAlpha(sourceAlpha);

            if (echoAlpha <= WrapRailAlphaEpsilon || !EnsureWrapRailEcho())
            {
                HideWrapRailEcho();
                return;
            }

            wrapRailEchoObject.SetActive(true);
            wrapRailEchoObject.transform.SetPositionAndRotation(
                new Vector3(echoX, currentY, transform.position.z),
                Quaternion.Euler(0f, 0f, currentRotation));
            SyncWrapRailEchoScale();
            ApplyTintAlpha(wrapRailEchoRenderer, sourcePaddleTint, echoAlpha);
        }

        private bool EnsureWrapRailEcho()
        {
            if (wrapRailEchoObject != null)
            {
                return wrapRailEchoRenderer != null;
            }

            CacheSourcePaddleRenderer();

            if (sourcePaddleRenderer == null)
            {
                return false;
            }

            wrapRailEchoObject = new GameObject("Wrap Rail Echo");
            wrapRailEchoObject.transform.SetParent(transform.parent, false);
            SyncWrapRailEchoScale();

            var echoVisual = new GameObject("Visual");
            echoVisual.transform.SetParent(wrapRailEchoObject.transform, false);

            wrapRailEchoRenderer = echoVisual.AddComponent<SpriteRenderer>();
            wrapRailEchoRenderer.sprite = sourcePaddleRenderer.sprite;
            wrapRailEchoRenderer.sharedMaterial = sourcePaddleRenderer.sharedMaterial;
            wrapRailEchoRenderer.sortingOrder = sourcePaddleRenderer.sortingOrder;
            BreakoutSpriteRendererUtility.NormalizeScale(wrapRailEchoRenderer);
            wrapRailEchoObject.SetActive(false);
            return true;
        }

        private void SyncWrapRailEchoScale()
        {
            if (wrapRailEchoObject != null)
            {
                wrapRailEchoObject.transform.localScale = transform.localScale;
            }
        }

        private void ApplyWrapRailSourceAlpha(float alpha)
        {
            CacheSourcePaddleRenderer();

            if (sourcePaddleRenderer == null)
            {
                return;
            }

            ApplyTintAlpha(sourcePaddleRenderer, sourcePaddleTint, alpha);
            wrapRailSourceTintOverridden = true;
        }

        private void RestoreWrapRailSourceTint()
        {
            if (!wrapRailSourceTintOverridden || sourcePaddleRenderer == null || !hasSourcePaddleTint)
            {
                return;
            }

            BreakoutSpriteRendererUtility.ApplyTint(sourcePaddleRenderer, sourcePaddleTint);
            wrapRailSourceTintOverridden = false;
        }

        private void HideWrapRailEcho()
        {
            if (wrapRailEchoObject != null)
            {
                wrapRailEchoObject.SetActive(false);
            }
        }

        private void CacheSourcePaddleRenderer()
        {
            if (sourcePaddleRenderer == null)
            {
                sourcePaddleRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (sourcePaddleRenderer == null || wrapRailSourceTintOverridden)
            {
                return;
            }

            sourcePaddleTint = BreakoutSpriteRendererUtility.ResolveTint(sourcePaddleRenderer);
            hasSourcePaddleTint = true;
        }

        private static void ApplyTintAlpha(SpriteRenderer spriteRenderer, Color baseTint, float alpha)
        {
            var tint = baseTint;
            tint.a *= Mathf.Clamp01(alpha);
            BreakoutSpriteRendererUtility.ApplyTint(spriteRenderer, tint);
        }

        private void UpdateWavyMotion(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (wavyStrength <= WavyStrengthEpsilon)
            {
                currentWavyYOffset = Mathf.MoveTowards(currentWavyYOffset, 0f, WavyMaxVerticalAmplitude * WavyReturnSpeed * deltaTime);
                currentWavyRotation = Mathf.MoveTowards(currentWavyRotation, 0f, WavyMaxRotationDegrees * WavyReturnSpeed * deltaTime);
                return;
            }

            wavyRetargetTimer = Mathf.Max(0f, wavyRetargetTimer - deltaTime);

            if (wavyRetargetTimer <= WavyStrengthEpsilon)
            {
                RetargetWavyMotion();
            }

            var blendFactor = 1f - Mathf.Exp(-WavyBlendSpeed * deltaTime);
            currentWavyYOffset = Mathf.Lerp(currentWavyYOffset, targetWavyYOffset, blendFactor);
            currentWavyRotation = Mathf.Lerp(currentWavyRotation, targetWavyRotation, blendFactor);
        }

        private void ResetWavyPoseForCurrentState()
        {
            currentWavyYOffset = 0f;
            currentWavyRotation = 0f;
            targetWavyYOffset = 0f;
            targetWavyRotation = 0f;
            wavyRetargetTimer = 0f;

            if (wavyStrength > WavyStrengthEpsilon)
            {
                RetargetWavyMotion();
            }
        }

        private void ResetWavyMotion()
        {
            currentWavyYOffset = 0f;
            currentWavyRotation = 0f;
            targetWavyYOffset = 0f;
            targetWavyRotation = 0f;
            wavyRetargetTimer = 0f;
        }

        private void UpdateBreakWiggle(float deltaTime)
        {
            if (deltaTime <= 0f || breakWiggleTimer <= 0f)
            {
                return;
            }

            breakWiggleTimer = Mathf.Max(0f, breakWiggleTimer - deltaTime);
            breakWigglePhase += BreakWigglePhaseSpeed * deltaTime;

            var normalizedTime = BreakWiggleDurationSeconds > 0f
                ? breakWiggleTimer / BreakWiggleDurationSeconds
                : 0f;
            var envelope = Mathf.Sin(Mathf.Clamp01(normalizedTime) * Mathf.PI);
            currentBreakWiggleYOffset = Mathf.Sin(breakWigglePhase) * BreakWiggleMaxVerticalAmplitude * envelope;
            currentBreakWiggleRotation = Mathf.Sin((breakWigglePhase * 1.63f) + 0.45f) * BreakWiggleMaxRotationDegrees * envelope;

            if (breakWiggleTimer <= 0f)
            {
                ResetBreakWiggle();
            }
        }

        private void ResetBreakWiggle()
        {
            breakWiggleTimer = 0f;
            currentBreakWiggleYOffset = 0f;
            currentBreakWiggleRotation = 0f;
        }

        private void UpdateHitTiltReturn(float deltaTime)
        {
            if (!isHitTiltReturning || deltaTime <= 0f)
            {
                return;
            }

            currentHitTiltRotation = Mathf.MoveTowards(
                currentHitTiltRotation,
                0f,
                HitTiltReturnDegreesPerSecond * deltaTime);

            if (Mathf.Abs(currentHitTiltRotation) <= WavyStrengthEpsilon)
            {
                currentHitTiltRotation = 0f;
                lastHitTiltDirection = 1f;
                isHitTiltReturning = false;
            }
        }

        private float GetCurrentVerticalOffset()
        {
            return currentWavyYOffset + currentBreakWiggleYOffset;
        }

        private float GetCurrentRotation()
        {
            return currentWavyRotation + currentBreakWiggleRotation + currentHitTiltRotation;
        }

        private void RetargetWavyMotion()
        {
            targetWavyYOffset = GetRandomFloat(-WavyMaxVerticalAmplitude, WavyMaxVerticalAmplitude) * wavyStrength;
            targetWavyRotation = GetRandomFloat(-WavyMaxRotationDegrees, WavyMaxRotationDegrees) * wavyStrength;
            wavyRetargetTimer = GetRandomFloat(WavyRetargetMinSeconds, WavyRetargetMaxSeconds);
        }

        private float GetRandomFloat(float minInclusive, float maxInclusive)
        {
            return gameController != null
                ? gameController.NextGameplayRandomFloat(minInclusive, maxInclusive)
                : Random.Range(minInclusive, maxInclusive);
        }

        private bool IsLagPauseWindow()
        {
            if (lagSpikeStrength <= WavyStrengthEpsilon)
            {
                return false;
            }

            var cycleTime = Mathf.Repeat(Time.time, LagSpikeCycleSeconds);
            return cycleTime < LagSpikeMaxPauseSeconds * lagSpikeStrength;
        }

        private void EnsureClonePaddle()
        {
            if (clonePaddleObject != null)
            {
                return;
            }

            var sourceRenderer = GetComponentInChildren<SpriteRenderer>();
            clonePaddleObject = new GameObject("Clone Paddle");
            clonePaddleObject.transform.SetParent(transform, false);
            clonePaddleObject.transform.localPosition = new Vector3(0f, ClonePaddleYOffset, 0f);
            clonePaddleObject.transform.localScale = new Vector3(ClonePaddleWidthMultiplier, 0.76f, 1f);

            clonePaddleCollider = clonePaddleObject.AddComponent<BoxCollider2D>();

            var sourceCollider = GetComponent<BoxCollider2D>();

            if (sourceCollider != null)
            {
                clonePaddleCollider.sharedMaterial = sourceCollider.sharedMaterial;
            }

            clonePaddleRenderer = clonePaddleObject.AddComponent<SpriteRenderer>();

            if (sourceRenderer != null)
            {
                clonePaddleRenderer.sprite = sourceRenderer.sprite;
                clonePaddleRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                BreakoutSpriteRendererUtility.ApplyTint(clonePaddleRenderer, BreakoutSpriteRendererUtility.ResolveTint(sourceRenderer));
            }

            clonePaddleRenderer.sortingOrder = ClonePaddleSortingOrder;
            clonePaddleObject.SetActive(false);
        }

        private void EnsureCloneStaticPaddle()
        {
            if (cloneStaticPaddleObject != null)
            {
                return;
            }

            var sourceRenderer = GetComponentInChildren<SpriteRenderer>();
            var sourceCollider = GetComponent<BoxCollider2D>();
            cloneStaticPaddleObject = new GameObject("Clone Static Paddle");
            cloneStaticPaddleObject.transform.SetParent(transform.parent, false);

            var ghostVisual = new GameObject("Visual");
            ghostVisual.transform.SetParent(cloneStaticPaddleObject.transform, false);

            cloneStaticPaddleRenderer = ghostVisual.AddComponent<SpriteRenderer>();

            if (sourceRenderer != null)
            {
                cloneStaticPaddleRenderer.sprite = sourceRenderer.sprite;
                cloneStaticPaddleRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                ApplyTintAlpha(cloneStaticPaddleRenderer, BreakoutSpriteRendererUtility.ResolveTint(sourceRenderer), CloneStaticTintAlpha);
                BreakoutSpriteRendererUtility.NormalizeScale(cloneStaticPaddleRenderer);
            }

            cloneStaticPaddleRenderer.sortingOrder = CloneStaticPaddleSortingOrder;

            var ghostCollider = cloneStaticPaddleObject.AddComponent<BoxCollider2D>();

            if (sourceCollider != null)
            {
                ghostCollider.sharedMaterial = sourceCollider.sharedMaterial;
            }

            cloneStaticPaddleBody = cloneStaticPaddleObject.AddComponent<Rigidbody2D>();
            cloneStaticPaddleBody.bodyType = RigidbodyType2D.Kinematic;
            cloneStaticPaddleBody.gravityScale = 0f;
            cloneStaticPaddleBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            cloneStaticPaddleBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            cloneStaticPaddleObject.SetActive(false);
        }

        private void ResetCloneStaticSamples()
        {
            cloneStaticSamples.Clear();

            if (!cloneStaticPaddleEnabled || paddleBody == null)
            {
                return;
            }

            var currentPosition = paddleBody.position;
            var currentRotation = GetCurrentRotation();
            RecordCloneStaticSample(Time.fixedTime, currentPosition, currentRotation);
        }

        private void RecordCloneStaticSample(float time, Vector2 position, float rotation)
        {
            if (!cloneStaticPaddleEnabled)
            {
                return;
            }

            cloneStaticSamples.Add(new DelayedPaddleSample(time, position, rotation, transform.localScale));
            TrimCloneStaticSamples(time);
        }

        private void TrimCloneStaticSamples(float currentTime)
        {
            var oldestUsefulTime = currentTime - cloneStaticDelaySeconds - CloneStaticHistoryPaddingSeconds;

            while (cloneStaticSamples.Count > 2 && cloneStaticSamples[1].Time < oldestUsefulTime)
            {
                cloneStaticSamples.RemoveAt(0);
            }
        }

        private void UpdateCloneStaticPaddle(float currentTime)
        {
            if (!cloneStaticPaddleEnabled || cloneStaticPaddleObject == null || cloneStaticSamples.Count == 0)
            {
                return;
            }

            var sample = ResolveCloneStaticSample(currentTime - cloneStaticDelaySeconds);
            var delayedPosition = new Vector2(sample.Position.x, startingY + cloneStaticYOffset);
            var delayedScale = sample.Scale;
            delayedScale.x *= cloneStaticWidthMultiplier;
            cloneStaticPaddleObject.transform.localScale = delayedScale;
            cloneStaticPaddleObject.transform.SetPositionAndRotation(
                delayedPosition,
                Quaternion.Euler(0f, 0f, sample.Rotation));

            if (cloneStaticPaddleBody != null)
            {
                cloneStaticPaddleBody.position = delayedPosition;
                cloneStaticPaddleBody.rotation = sample.Rotation;
            }
        }

        private DelayedPaddleSample ResolveCloneStaticSample(float targetTime)
        {
            if (cloneStaticSamples.Count == 1 || targetTime <= cloneStaticSamples[0].Time)
            {
                return cloneStaticSamples[0];
            }

            for (var index = 1; index < cloneStaticSamples.Count; index++)
            {
                var current = cloneStaticSamples[index];

                if (targetTime > current.Time)
                {
                    continue;
                }

                var previous = cloneStaticSamples[index - 1];
                var ratio = Mathf.InverseLerp(previous.Time, current.Time, targetTime);
                return new DelayedPaddleSample(
                    targetTime,
                    Vector2.Lerp(previous.Position, current.Position, ratio),
                    Mathf.LerpAngle(previous.Rotation, current.Rotation, ratio),
                    Vector3.Lerp(previous.Scale, current.Scale, ratio));
            }

            return cloneStaticSamples[cloneStaticSamples.Count - 1];
        }

        private void EnsureMirrorImagePaddle()
        {
            if (mirrorImagePaddle != null)
            {
                return;
            }

            var sourceRenderer = GetComponentInChildren<SpriteRenderer>();
            var sourceCollider = GetComponent<BoxCollider2D>();
            mirrorImagePaddleObject = new GameObject("Mirror Image Paddle");
            mirrorImagePaddleObject.transform.SetParent(transform.parent, false);
            mirrorImagePaddleObject.transform.localScale = baseScale;

            var mirrorVisual = new GameObject("Visual");
            mirrorVisual.transform.SetParent(mirrorImagePaddleObject.transform, false);

            var mirrorRenderer = mirrorVisual.AddComponent<SpriteRenderer>();

            if (sourceRenderer != null)
            {
                mirrorRenderer.sprite = sourceRenderer.sprite;
                mirrorRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                BreakoutSpriteRendererUtility.ApplyTint(mirrorRenderer, BreakoutSpriteRendererUtility.ResolveTint(sourceRenderer));
                BreakoutSpriteRendererUtility.NormalizeScale(mirrorRenderer);
            }

            mirrorRenderer.sortingOrder = MirrorImagePaddleSortingOrder;

            var mirrorCollider = mirrorImagePaddleObject.AddComponent<BoxCollider2D>();

            if (sourceCollider != null)
            {
                mirrorCollider.sharedMaterial = sourceCollider.sharedMaterial;
            }

            var mirrorBody = mirrorImagePaddleObject.AddComponent<Rigidbody2D>();
            mirrorBody.bodyType = RigidbodyType2D.Kinematic;
            mirrorBody.gravityScale = 0f;
            mirrorBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            mirrorBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            mirrorImagePaddle = mirrorImagePaddleObject.AddComponent<PaddleController>();
            mirrorImagePaddle.isAuxiliaryPaddle = true;
            mirrorImagePaddle.inputDirectionMultiplier = -1f;
            mirrorImagePaddle.Configure(gameController, moveSpeed, leftBoundaryX, rightBoundaryX, startingY + MirrorImagePaddleYOffset);
            mirrorImagePaddle.SetWidthMultiplier(currentWidthMultiplier);
            mirrorImagePaddle.SetWavyStrength(wavyStrength);
            mirrorImagePaddle.SetControlsReversed(controlsReversed);
            mirrorImagePaddle.SetSplitGapWidthNormalized(splitGapWidthNormalized);
            mirrorImagePaddle.SetLagSpikeStrength(lagSpikeStrength);
            mirrorImagePaddle.SetHitTiltDegrees(hitTiltDegrees);
            mirrorImagePaddle.SetWrapRailEnabled(wrapRailEnabled);
            mirrorImagePaddleObject.SetActive(false);
        }

        private float ReadHorizontalInput()
        {
            var keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return 0f;
            }

            var direction = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                direction -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                direction += 1f;
            }

            var resolvedDirection = controlsReversed ? -direction : direction;
            return resolvedDirection * inputDirectionMultiplier;
        }

        private float UpdateAutopilotInput(float deltaTime)
        {
            var currentX = paddleBody != null ? paddleBody.position.x : transform.position.x;
            var delta = autopilotTargetX - currentX;
            var desiredInput = 0f;

            if (Mathf.Abs(delta) > AutopilotDeadZone)
            {
                var distanceRatio = Mathf.Clamp01((Mathf.Abs(delta) - AutopilotDeadZone) / AutopilotSlowRadius);
                desiredInput = Mathf.Sign(delta) * Mathf.SmoothStep(0f, AutopilotMaximumInput, distanceRatio);
            }

            autopilotInput = Mathf.MoveTowards(
                autopilotInput,
                desiredInput,
                AutopilotInputBlendSpeed * Mathf.Max(0f, deltaTime));
            return autopilotInput;
        }
    }
}
