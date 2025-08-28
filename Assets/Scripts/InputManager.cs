using ChromaPop.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromaPop.Input
{
    /// <summary>
    /// Handles player input for balloon interaction using the new Input System.
    /// Supports both mouse clicks (for editor/desktop) and touch input (for mobile).
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask balloonLayerMask = -1;

        private Camera mainCamera;
        private InputAction clickAction;
        private InputAction touchAction;

        private void Awake()
        {
            InitializeComponents();
            SetupInputActions();
        }

        private void OnEnable()
        {
            clickAction?.Enable();
            touchAction?.Enable();
        }

        private void OnDisable()
        {
            clickAction?.Disable();
            touchAction?.Disable();
        }

        private void OnDestroy()
        {
            clickAction?.Dispose();
            touchAction?.Dispose();
        }

        private void InitializeComponents()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("No main camera found! Input detection will not work.", this);
            }
        }

        private void SetupInputActions()
        {
            // Mouse input for desktop/editor
            clickAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton"
            );

            // Touch input for mobile - using press instead of tap
            touchAction = new InputAction(
                name: "Touch",
                type: InputActionType.Button,
                binding: "<Touchscreen>/primaryTouch/press"
            );

            clickAction.performed += OnClickPerformed;
            touchAction.performed += OnTouchPerformed;

            // Debug logging
            Debug.Log($"Input setup complete - Touch available: {Touchscreen.current != null}, " +
                     $"Mouse available: {Mouse.current != null}");
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (!CanProcessInput()) return;

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            ProcessClickAt(screenPosition);
        }

        private void OnTouchPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Touch input detected!");

            if (!CanProcessTouchInput()) return;

            // Get touch position from the primary touch
            Vector2 screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Debug.Log($"Touch position: {screenPosition}");
            ProcessClickAt(screenPosition);
        }

        private bool CanProcessInput()
        {
            return mainCamera != null &&
                   Mouse.current != null &&
                   GameManager.Instance != null;
        }

        private bool CanProcessTouchInput()
        {
            bool canProcess = mainCamera != null &&
                   Touchscreen.current != null &&
                   Touchscreen.current.primaryTouch.press.isPressed &&
                   GameManager.Instance != null;

            // Debug logging for touch input issues
            if (!canProcess && Touchscreen.current != null)
            {
                Debug.Log($"Touch input blocked - Camera: {mainCamera != null}, " +
                         $"Touch pressed: {Touchscreen.current.primaryTouch.press.isPressed}, " +
                         $"GameManager: {GameManager.Instance != null}");
            }

            return canProcess;
        }

        private void ProcessClickAt(Vector2 screenPosition)
        {
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, balloonLayerMask);

            if (hit.collider != null)
            {
                TryPopBalloon(hit.collider.gameObject);
            }
        }

        private void TryPopBalloon(GameObject hitObject)
        {
            BalloonController balloon = hitObject.GetComponent<BalloonController>();

            if (balloon != null)
            {
                balloon.Pop();
            }
        }

        // Legacy Update method for fallback input handling
        private void Update()
        {
            // Only use legacy input if new Input System actions are not working
            if ((clickAction == null || !clickAction.enabled) &&
                (touchAction == null || !touchAction.enabled))
            {
                HandleLegacyInput();
            }
        }

        private void HandleLegacyInput()
        {
            // Handle mouse input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (CanProcessInput())
                {
                    Vector2 screenPosition = Mouse.current.position.ReadValue();
                    ProcessClickAt(screenPosition);
                }
                return;
            }

            // Handle touch input - check for touch began
            if (Touchscreen.current != null)
            {
                var primaryTouch = Touchscreen.current.primaryTouch;
                if (primaryTouch.press.wasPressedThisFrame)
                {
                    if (CanProcessLegacyTouchInput())
                    {
                        Vector2 screenPosition = primaryTouch.position.ReadValue();
                        ProcessClickAt(screenPosition);
                    }
                }
            }
        }

        private bool CanProcessLegacyTouchInput()
        {
            return mainCamera != null &&
                   Touchscreen.current != null &&
                   GameManager.Instance != null;
        }
    }
}