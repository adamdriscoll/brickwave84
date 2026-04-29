using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BallController : MonoBehaviour
    {
        [SerializeField] private float maxPaddleBounceAngle = 70f;

        private BreakoutGameController gameController;
        private PaddleController paddle;
        private Rigidbody2D ballBody;
        private float launchSpeed;
        private float minimumVerticalDirection;
        private float lossThresholdY;
        private float paddleFollowOffset;
        private bool hasLaunched;

        public void Configure(
            BreakoutGameController controller,
            PaddleController paddleController,
            float speed,
            float minimumVertical,
            float lossY,
            float followOffset)
        {
            gameController = controller;
            paddle = paddleController;
            launchSpeed = speed;
            minimumVerticalDirection = Mathf.Clamp(minimumVertical, 0.15f, 0.95f);
            lossThresholdY = lossY;
            paddleFollowOffset = followOffset;
            ballBody = GetComponent<Rigidbody2D>();
        }

        public void ResetToPaddle()
        {
            hasLaunched = false;

            if (ballBody == null)
            {
                ballBody = GetComponent<Rigidbody2D>();
            }

            ballBody.linearVelocity = Vector2.zero;

            if (paddle == null)
            {
                return;
            }

            var anchoredPosition = (Vector2)paddle.transform.position + (Vector2.up * paddleFollowOffset);
            transform.position = anchoredPosition;
            ballBody.position = anchoredPosition;
        }

        public void Launch()
        {
            if (hasLaunched || ballBody == null)
            {
                return;
            }

            hasLaunched = true;

            var launchDirection = new Vector2(Random.Range(-0.25f, 0.25f), 1f).normalized;
            ballBody.linearVelocity = launchDirection * launchSpeed;
        }

        public void Stop()
        {
            hasLaunched = false;

            if (ballBody != null)
            {
                ballBody.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (ballBody == null)
            {
                return;
            }

            if (!hasLaunched)
            {
                if (paddle == null)
                {
                    return;
                }

                var targetPosition = (Vector2)paddle.transform.position + (Vector2.up * paddleFollowOffset);
                ballBody.MovePosition(targetPosition);
                return;
            }

            if (transform.position.y < lossThresholdY)
            {
                Stop();
                gameController.HandleBallLost();
                return;
            }

            ClampBallVelocity();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!hasLaunched)
            {
                return;
            }

            if (collision.collider.TryGetComponent<PaddleController>(out var hitPaddle))
            {
                RedirectFromPaddle(hitPaddle, collision);
                return;
            }

            ClampBallVelocity();
        }

        private void RedirectFromPaddle(PaddleController hitPaddle, Collision2D collision)
        {
            if (ballBody == null)
            {
                return;
            }

            var contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            var normalizedOffset = Mathf.Clamp(
                (contactPoint.x - hitPaddle.transform.position.x) / hitPaddle.HalfWidthWorld,
                -1f,
                1f);

            var bounceAngleRadians = normalizedOffset * maxPaddleBounceAngle * Mathf.Deg2Rad;
            var bounceDirection = new Vector2(Mathf.Sin(bounceAngleRadians), Mathf.Cos(bounceAngleRadians)).normalized;

            if (Mathf.Abs(bounceDirection.y) < minimumVerticalDirection)
            {
                bounceDirection = new Vector2(
                    bounceDirection.x,
                    Mathf.Sign(Mathf.Approximately(bounceDirection.y, 0f) ? 1f : bounceDirection.y) * minimumVerticalDirection).normalized;
            }

            ballBody.linearVelocity = bounceDirection * launchSpeed;
        }

        private void ClampBallVelocity()
        {
            if (ballBody == null)
            {
                return;
            }

            var velocity = ballBody.linearVelocity;

            if (velocity.sqrMagnitude < 0.01f)
            {
                velocity = Vector2.up * launchSpeed;
            }

            velocity = velocity.normalized * launchSpeed;

            if (Mathf.Abs(velocity.y) < launchSpeed * minimumVerticalDirection)
            {
                var ySign = Mathf.Sign(Mathf.Approximately(velocity.y, 0f) ? 1f : velocity.y);
                var adjustedY = launchSpeed * minimumVerticalDirection * ySign;
                var adjustedX = Mathf.Sqrt(Mathf.Max(0.01f, (launchSpeed * launchSpeed) - (adjustedY * adjustedY)));
                adjustedX *= Mathf.Approximately(velocity.x, 0f)
                    ? (Random.value < 0.5f ? -1f : 1f)
                    : Mathf.Sign(velocity.x);
                velocity = new Vector2(adjustedX, adjustedY);
            }

            ballBody.linearVelocity = velocity;
        }
    }
}
