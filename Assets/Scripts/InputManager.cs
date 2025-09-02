using UnityEngine;
using UnityEngine.EventSystems; // for UI blocking
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
            // Use the abstract <Pointer> device so the same binding covers mouse *and* touch.
            // Press interaction set to fire on "press" (you can switch to release by using Press(behavior=1))
            pressAction = new InputAction(
                name: "PointerPress",
                type: InputActionType.Button,
                binding: "*/press", // matches <Pointer>/press, works for Mouse and Touch primary
                interactions: "press" // change to "press(behavior=1)" if you want on-release instead
            );

            positionAction = new InputAction(
                name: "PointerPosition",
                type: InputActionType.Value,
                binding: "*/position" // matches <Pointer>/position (mouse or primary touch)
            );
        }

        private void OnPressPerformed(InputAction.CallbackContext ctx)
        {
            if (!CanProcessNow()) return;

            // If pointer is over UI, ignore (prevents popping through buttons, etc.)
            if (IsPointerOverUI()) return;

            // Read the pointer position (mouse or primary touch)
            Vector2 screenPos = positionAction.ReadValue<Vector2>();
            ProcessPointerAt(screenPos);
        }

        private bool CanProcessNow()
        {
            if (mainCamera == null) return false;
            if (ignoreWhenPaused && Time.timeScale == 0f) return false;
            if (GameManager.Instance == null) return false;
            return true;
        }

        private bool IsPointerOverUI()
        {
            // Works for mouse and touch (primary). For multi-touch UI checks,
            // you could pass a fingerId-specific PointerEventData if needed.
            if (EventSystem.current == null) return false;

            // For touch, try to map to fingerId so UI can disambiguate.
            // If Touchscreen exists and has an active primary touch, use its touchId.
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                return EventSystem.current.IsPointerOverGameObject(touchId);
            }

            // Fallback for mouse/other pointers
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void ProcessPointerAt(Vector2 screenPosition)
        {
            // Convert screen to world; ensure we pass a sensible z so ScreenToWorldPoint works in any camera mode
            var sp = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(sp);

            // OverlapPoint is ideal for a tap/click on 2D colliders
            Collider2D hit = Physics2D.OverlapPoint(worldPoint, balloonLayerMask);
            if (hit != null)
            {
                var balloon = hit.GetComponent<BalloonController>();
                if (balloon != null)
                {
                    balloon.Pop();
                }
            }
        }

        // Optional: legacy fallback if new Input System objects aren't available/enabled
        // (kept minimal; you can remove this if you don't need it)
        private void Update()
        {
            if (pressAction is { enabled: true } && positionAction is { enabled: true }) return;
            if (!CanProcessNow()) return;

            // Mouse fallback
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!IsPointerOverUI())
                {
                    ProcessPointerAt(Mouse.current.position.ReadValue());
                }
                return;
            }

            // Touch fallback (primary)
            if (Touchscreen.current != null)
            {
                var t = Touchscreen.current.primaryTouch;
                if (t.press.wasPressedThisFrame)
                {
                    if (!IsPointerOverUI())
                    {
                        ProcessPointerAt(t.position.ReadValue());
                    }
                }
            }
        }
    }
}
