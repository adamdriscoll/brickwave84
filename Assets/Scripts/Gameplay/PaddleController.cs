using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PaddleController : MonoBehaviour
    {
        private Rigidbody2D paddleBody;
        private float moveSpeed;
        private float minX;
        private float maxX;
        private float startingY;
        private Vector2 startingPosition;
        private float horizontalInput;

        public float HalfWidthWorld { get; private set; }

        public void Configure(float speed, float minimumX, float maximumX, float startY, float halfWidthWorld)
        {
            moveSpeed = speed;
            minX = minimumX;
            maxX = maximumX;
            startingY = startY;
            startingPosition = new Vector2(0f, startY);
            HalfWidthWorld = halfWidthWorld;
            paddleBody = GetComponent<Rigidbody2D>();
            ResetToStart();
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
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
                minX,
                maxX);

            paddleBody.MovePosition(new Vector2(nextX, startingY));
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
