using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromaPop
{
    /// <summary>
    /// Detects swipe gestures on the sequence container and triggers animations
    /// </summary>
    public class SwipeDetector : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float minSwipeTime = 0.1f;
        [SerializeField] private float maxSwipeTime = 1.0f;

        // Events
        public static event Action<Vector2> OnSwipeDetected;

        // Private fields
        private Vector2 startPos;
        private Vector2 endPos;
        private float startTime;
        private bool swipeDetected = false;
        private bool isPressed = false;

        void Update()
        {
            // Check for touch input first
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                if (!isPressed)
                {
                    // Touch just started
                    swipeDetected = false;
                    isPressed = true;
                    startPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    startTime = Time.time;
                }
            }
            else if (Touchscreen.current != null && isPressed)
            {
                // Touch just ended
                endPos = Touchscreen.current.primaryTouch.position.ReadValue();
                isPressed = false;
                DetectSwipe();
            }
            // Check for mouse input if no touch
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                if (!isPressed)
                {
                    // Mouse just pressed
                    swipeDetected = false;
                    isPressed = true;
                    startPos = Mouse.current.position.ReadValue();
                    startTime = Time.time;
                }
            }
            else if (Mouse.current != null && isPressed)
            {
                // Mouse just released
                endPos = Mouse.current.position.ReadValue();
                isPressed = false;
                DetectSwipe();
            }
        }

        private void DetectSwipe()
        {
            if (swipeDetected)
                return;

            Vector2 swipe = endPos - startPos;
            float swipeDistance = swipe.magnitude;
            float swipeTime = Time.time - startTime;

            // Check if swipe meets all requirements
            if (swipeDistance < minSwipeDistance)
                return;

            if (swipeTime < minSwipeTime || swipeTime > maxSwipeTime)
                return;

            swipeDetected = true;

            // Normalize the swipe direction and invert it for natural movement
            // When user swipes right, elements should move left (opposite direction)
            Vector2 swipeDirection = (-swipe).normalized;

            // Trigger the swipe event
            OnSwipeDetected?.Invoke(swipeDirection);
        }
    }
}
