using System;
using UnityEngine;

namespace GetBricked.Gameplay
{
    [RequireComponent(typeof(Brick))]
    internal sealed class BreakoutBrickFadeAnimator : MonoBehaviour
    {
        private Brick brick;
        private Collider2D brickCollider;
        private Action<Brick> completionCallback;
        private float durationSeconds = 0.35f;
        private float elapsedSeconds;
        private bool fadeIn = true;
        private bool isAnimating;

        public void PlayFadeIn(float duration, Action<Brick> onComplete = null)
        {
            Configure(duration, fadeInAnimation: true, onComplete);
            SetColliderEnabled(false);
            brick.SetTransitionVisibilityMultiplier(0f);
        }

        public void PlayFadeOut(float duration, Action<Brick> onComplete = null)
        {
            Configure(duration, fadeInAnimation: false, onComplete);
            SetColliderEnabled(false);
            brick.SetTransitionVisibilityMultiplier(1f);
        }

        private void Awake()
        {
            brick = GetComponent<Brick>();
            brickCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (!isAnimating || brick == null)
            {
                return;
            }

            elapsedSeconds = Mathf.Min(durationSeconds, elapsedSeconds + Time.deltaTime);
            var progress = durationSeconds > 0.0001f ? elapsedSeconds / durationSeconds : 1f;
            var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            brick.SetTransitionVisibilityMultiplier(fadeIn ? easedProgress : 1f - easedProgress);

            if (progress < 1f)
            {
                return;
            }

            isAnimating = false;

            if (fadeIn)
            {
                brick.SetTransitionVisibilityMultiplier(1f);
                SetColliderEnabled(true);
            }
            else
            {
                brick.SetTransitionVisibilityMultiplier(0f);
            }

            var callback = completionCallback;
            completionCallback = null;
            callback?.Invoke(brick);
            Destroy(this);
        }

        private void Configure(float duration, bool fadeInAnimation, Action<Brick> onComplete)
        {
            brick ??= GetComponent<Brick>();
            brickCollider ??= GetComponent<Collider2D>();
            durationSeconds = Mathf.Max(0.01f, duration);
            elapsedSeconds = 0f;
            fadeIn = fadeInAnimation;
            completionCallback = onComplete;
            isAnimating = brick != null;
        }

        private void SetColliderEnabled(bool enabled)
        {
            if (brickCollider != null)
            {
                brickCollider.enabled = enabled;
            }
        }
    }
}
