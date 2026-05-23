using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PaddleController : MonoBehaviour
    {
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
        private const float MirrorImagePaddleYOffset = 1.16f;
        private const int MirrorImagePaddleSortingOrder = 39;
        private const float AutopilotDeadZone = 0.16f;
        private const float AutopilotSlowRadius = 1.15f;
        private const float AutopilotInputBlendSpeed = 5.5f;
        private const float AutopilotMaximumInput = 0.85f;

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
        private bool isAutopilotEnabled;
        private float autopilotTargetX;
        private float autopilotInput;
        private GameObject clonePaddleObject;
        private BoxCollider2D clonePaddleCollider;
        private SpriteRenderer clonePaddleRenderer;
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
            mirrorImagePaddle?.ResetToStart();
        }

        private void OnDestroy()
        {
            if (isAuxiliaryPaddle || mirrorImagePaddleObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(mirrorImagePaddleObject);
            }
            else
            {
                DestroyImmediate(mirrorImagePaddleObject);
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

            var nextX = Mathf.Clamp(
                paddleBody.position.x + (horizontalInput * moveSpeed * Time.fixedDeltaTime),
                leftBoundaryX + HalfWidthWorld,
                rightBoundaryX - HalfWidthWorld);

            if (IsLagPauseWindow())
            {
                nextX = paddleBody.position.x;
            }

            paddleBody.MovePosition(new Vector2(nextX, startingY + GetCurrentVerticalOffset()));
            paddleBody.MoveRotation(GetCurrentRotation());
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
