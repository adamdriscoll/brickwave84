using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PaddleController : MonoBehaviour
    {
        private Rigidbody2D paddleBody;
        private Vector3 baseScale;
        private float moveSpeed;
        private float leftBoundaryX;
        private float rightBoundaryX;
        private float startingY;
        private Vector2 startingPosition;
        private float horizontalInput;

        public float HalfWidthWorld { get; private set; }

        public void Configure(float speed, float minimumBoundaryX, float maximumBoundaryX, float startY)
        {
            moveSpeed = speed;
            leftBoundaryX = minimumBoundaryX;
            rightBoundaryX = maximumBoundaryX;
            startingY = startY;
            startingPosition = new Vector2(0f, startY);
            baseScale = transform.localScale;
            paddleBody = GetComponent<Rigidbody2D>();
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

        public void ResetToStart()
        {
            var resetPosition = startingPosition;
            transform.position = resetPosition;

            if (paddleBody == null)
            {
                paddleBody = GetComponent<Rigidbody2D>();
            }

            paddleBody.position = resetPosition;
            paddleBody.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            horizontalInput = ReadHorizontalInput();
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

            paddleBody.MovePosition(new Vector2(nextX, startingY));
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

            var clampedPosition = new Vector2(clampedX, startingY);
            transform.position = clampedPosition;
            paddleBody.position = clampedPosition;
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
