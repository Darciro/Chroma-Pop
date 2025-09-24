using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ChromaPop
{
    /// <summary>
    /// Unified pointer input (mouse + touch) using the new Input System.
    /// - Single InputAction map for press + position (covers editor, desktop, and mobile).
    /// - Blocks balloon interactions when the pointer is over UI elements.
    /// - Works with both orthographic and perspective 2D cameras.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("Which layers contain balloons that can be popped.")]
        [SerializeField] private LayerMask balloonLayerMask = ~0;

        [Header("Behaviour")]
        [Tooltip("If true, input is ignored when the game is paused (Time.timeScale == 0).")]
        [SerializeField] private bool ignoreWhenPaused = true;

        private Camera mainCamera;

        // One map with two actions: press and position
        private InputAction pressAction;
        private InputAction positionAction;

        // Input processing state
        private bool hasPendingInput = false;
        private Vector2 pendingInputPosition;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found! Input detection will not work.", this);
                enabled = false;
                return;
            }

            SetupInputActions();
        }

        private void OnEnable()
        {
            pressAction?.Enable();
            positionAction?.Enable();
            if (pressAction != null) pressAction.performed += OnPressPerformed;
        }

        private void OnDisable()
        {
            if (pressAction != null) pressAction.performed -= OnPressPerformed;
            pressAction?.Disable();
            positionAction?.Disable();
        }

        private void OnDestroy()
        {
            pressAction?.Dispose();
            positionAction?.Dispose();
        }

        private void SetupInputActions()
        {
            pressAction = new InputAction("Press", binding: "<Mouse>/leftButton");
            pressAction.AddBinding("<Touchscreen>/primaryTouch/press");

            positionAction = new InputAction("Position", binding: "<Mouse>/position");
            positionAction.AddBinding("<Touchscreen>/primaryTouch/position");
        }

        private void OnPressPerformed(InputAction.CallbackContext ctx)
        {
            if (!CanProcessNow()) return;

            // Defer processing to Update()
            Vector2 screenPos = positionAction.ReadValue<Vector2>();

            if (screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height)
            {
                return;
            }

            hasPendingInput = true;
            pendingInputPosition = screenPos;
        }

        private bool CanProcessNow()
        {
            if (mainCamera == null) return false;
            if (ignoreWhenPaused && Time.timeScale == 0f) return false;
            if (GameManager.Instance == null) return false;

            // Additional check: don't process input if game is not started
            if (!GameManager.Instance.gameStarted) return false;

            return true;
        }

        /// <summary>
        /// Checks if the pointer is over a UI element
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject(Pointer.current?.deviceId ?? -1);
        }

        /// <summary>
        /// Processes pointer input at the specified screen position and attempts to pop a balloon.
        /// Validates tag, collider, and ensures the balloon is not behind UI elements.
        /// </summary>
        /// <param name="screenPosition">The screen position of the input</param>
        private void ProcessPointerAt(Vector2 screenPosition)
        {
            Vector3 worldPoint = GetWorldPointFromScreenPosition(screenPosition);

            float hitRadius = 0.1f;
            Collider2D hit = Physics2D.OverlapCircle(worldPoint, hitRadius, balloonLayerMask);

            if (hit != null && IsValidBalloonTarget(hit))
            {
                var balloon = hit.GetComponent<BalloonController>();
                if (balloon != null)
                {
                    balloon.Pop();
                }
            }
        }

        /// <summary>
        /// Converts screen position to world position based on camera type.
        /// </summary>
        /// <param name="screenPosition">Screen position to convert</param>
        /// <returns>World position</returns>
        private Vector3 GetWorldPointFromScreenPosition(Vector2 screenPosition)
        {
            if (mainCamera.orthographic)
            {
                var sp = new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane);
                return mainCamera.ScreenToWorldPoint(sp);
            }
            else
            {
                var sp = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
                return mainCamera.ScreenToWorldPoint(sp);
            }
        }

        /// <summary>
        /// Validates if the hit collider represents a valid balloon target.
        /// Checks for correct tag and ensures the balloon is not already popped.
        /// </summary>
        /// <param name="hit">The collider that was hit</param>
        /// <returns>True if the target is a valid balloon</returns>
        private bool IsValidBalloonTarget(Collider2D hit)
        {
            // Check if the object has the correct "Balloon" tag
            if (!hit.CompareTag("Balloon"))
            {
                return false;
            }

            // Ensure the collider is active and enabled
            if (!hit.enabled || !hit.gameObject.activeInHierarchy)
            {
                return false;
            }

            // Check if balloon controller exists and balloon is not already popped
            var balloonController = hit.GetComponent<BalloonController>();
            if (balloonController == null || balloonController.IsPopped())
            {
                return false;
            }

            return true;
        }

        private void Update()
        {
            if (!CanProcessNow())
                return;

            // Process deferred input from input system
            if (hasPendingInput)
            {
                hasPendingInput = false;

                if (!IsPointerOverUI())
                {
                    ProcessPointerAt(pendingInputPosition);
                }

                return;
            }

            // Process fallback input (for mouse or touch input not triggering InputAction)
            if (IsPointerOverUI()) return;

            // Mouse input fallback
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();

                if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
                    mousePos.y >= 0 && mousePos.y <= Screen.height)
                {
                    ProcessPointerAt(mousePos);
                    return;
                }
            }

            // Touch input fallback
            if (Touchscreen.current != null)
            {
                var t = Touchscreen.current.primaryTouch;
                if (t.press.wasPressedThisFrame)
                {
                    Vector2 touchPos = t.position.ReadValue();

                    if (touchPos.x >= 0 && touchPos.x <= Screen.width &&
                        touchPos.y >= 0 && touchPos.y <= Screen.height)
                    {
                        ProcessPointerAt(touchPos);
                    }
                }
            }
        }

    }
}
