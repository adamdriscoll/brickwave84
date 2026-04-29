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

        private BreakoutGameController gameController;
        private Rigidbody2D paddleBody;
        private Vector3 baseScale;
        private float moveSpeed;
        private float leftBoundaryX;
        private float rightBoundaryX;
        private float startingY;
        private Vector2 startingPosition;
        private float horizontalInput;
        private float wavyStrength;
        private float currentWavyYOffset;
        private float currentWavyRotation;
        private float targetWavyYOffset;
        private float targetWavyRotation;
        private float wavyRetargetTimer;

        public float HalfWidthWorld { get; private set; }

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
        }

        public void SetWidthMultiplier(float multiplier)
        {
            var clampedMultiplier = Mathf.Clamp(multiplier, 0.55f, 1.85f);
            var width = baseScale.x * clampedMultiplier;
            transform.localScale = new Vector3(width, baseScale.y, baseScale.z);
            HalfWidthWorld = width * 0.5f;
            ClampToBounds();
        }

        public void SetWavyStrength(float strength)
        {
            wavyStrength = Mathf.Clamp01(strength);

            if (wavyStrength <= WavyStrengthEpsilon)
            {
                ResetWavyMotion();
                ClampToBounds();
                return;
            }

            RetargetWavyMotion();
        }

        public void ResetToStart()
        {
            var resetPosition = startingPosition;

            if (paddleBody == null)
            {
                paddleBody = GetComponent<Rigidbody2D>();
            }

            ResetWavyPoseForCurrentState();
            transform.SetPositionAndRotation(resetPosition, Quaternion.identity);
            paddleBody.position = resetPosition;
            paddleBody.rotation = 0f;
            paddleBody.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            horizontalInput = ReadHorizontalInput();
            UpdateWavyMotion(Time.deltaTime);
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

            paddleBody.MovePosition(new Vector2(nextX, startingY + currentWavyYOffset));
            paddleBody.MoveRotation(currentWavyRotation);
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

            var clampedPosition = new Vector2(clampedX, startingY + currentWavyYOffset);
            transform.SetPositionAndRotation(clampedPosition, Quaternion.Euler(0f, 0f, currentWavyRotation));
            paddleBody.position = clampedPosition;
            paddleBody.rotation = currentWavyRotation;
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

        private static float ReadHorizontalInput()
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

            return direction;
        }
    }
}
