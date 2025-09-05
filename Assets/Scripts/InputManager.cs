using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromaPop
{
    /// <summary>
    /// Unified pointer input (mouse + touch) using the new Input System.
    /// - Single InputAction map for press + position (covers editor, desktop, and mobile).
    /// - Ignores presses over UI.
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
            pressAction = new InputAction(
                name: "PointerPress",
                type: InputActionType.Button,
                binding: "<Pointer>/press",
                interactions: "press"
            );

            positionAction = new InputAction(
                name: "PointerPosition",
                type: InputActionType.Value,
                binding: "<Pointer>/position"
            );
        }

        private void OnPressPerformed(InputAction.CallbackContext ctx)
        {
            if (!CanProcessNow()) return;

            Vector2 screenPos = Vector2.zero;

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                screenPos = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else
            {
                screenPos = positionAction.ReadValue<Vector2>();
            }

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
            return true;
        }

        private void ProcessPointerAt(Vector2 screenPosition)
        {
            Vector3 worldPoint;

            if (mainCamera.orthographic)
            {
                var sp = new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane);
                worldPoint = mainCamera.ScreenToWorldPoint(sp);
            }
            else
            {
                var sp = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
                worldPoint = mainCamera.ScreenToWorldPoint(sp);
            }

            float hitRadius = 0.1f;
            Collider2D hit = Physics2D.OverlapCircle(worldPoint, hitRadius, balloonLayerMask);

            if (hit != null)
            {
                var balloon = hit.GetComponent<BalloonController>();
                if (balloon != null)
                {
                    balloon.Pop();
                }
            }
        }
        private void Update()
        {
            if (hasPendingInput)
            {
                hasPendingInput = false;
                ProcessPointerAt(pendingInputPosition);
                return;
            }

            if (pressAction != null && pressAction.enabled && positionAction != null && positionAction.enabled)
                return;

            if (!CanProcessNow()) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();

                if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
                    mousePos.y >= 0 && mousePos.y <= Screen.height)
                {
                    ProcessPointerAt(mousePos);
                }
                return;
            }

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
