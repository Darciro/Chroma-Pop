using UnityEngine;

namespace ChromaPop
{
    public class GameTween : MonoBehaviour
    {
        public static GameTween Instance { get; private set; }

        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject sequenceContainer;

        // Cached state for camera shake
        private Transform _cameraShakeAnchor;
        private int _shakeTweenId = -1;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (gameOverScreen != null)
            {
                GameOverTransitions();
            }

            if (sequenceContainer != null)
            {
                InitSequenceTransitions();
            }
        }

        public void GameOverTransitions()
        {
            gameOverScreen.SetActive(false);
            LeanTween.alphaCanvas(gameOverScreen.GetComponent<CanvasGroup>(), 0, 0);
        }

        public void InitGameOverTransitions()
        {
            gameOverScreen.SetActive(true);
            LeanTween.alphaCanvas(gameOverScreen.GetComponent<CanvasGroup>(), 1f, .5f).setDelay(0.25f);
        }

        public void ShakeCamera(float intensity = 0.1f, float duration = 0.2f)
        {
            // Robust, timeScale-independent camera shake that also works on Android.
            // Uses an inserted parent "anchor" to avoid conflicts with scripts that move the camera each frame.

            Camera mainCamera = ResolveActiveCamera();
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found for shake effect!");
                return;
            }

            // Ensure we have an anchor parent we can move safely
            _cameraShakeAnchor = GetOrCreateShakeAnchor(mainCamera.transform);
            if (_cameraShakeAnchor == null)
            {
                Debug.LogWarning("Failed to prepare camera shake anchor.");
                return;
            }

            // Cancel any existing shake tweens on the anchor
            if (_shakeTweenId != -1)
            {
                LeanTween.cancel(_shakeTweenId);
                _shakeTweenId = -1;
            }
            LeanTween.cancel(_cameraShakeAnchor.gameObject);

            Vector3 originalLocalPos = _cameraShakeAnchor.localPosition; // typically zero

            // Drive shake via a single value tween with per-frame random offset and falloff
            var descr = LeanTween.value(_cameraShakeAnchor.gameObject, 0f, 1f, duration)
                .setEase(LeanTweenType.linear)
                .setIgnoreTimeScale(true) // works even if timeScale == 0
                .setOnUpdate((float t) =>
                {
                    // t goes from 0..1, reduce amplitude over time
                    float amp = intensity * (1f - t);
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-amp, amp),
                        Random.Range(-amp, amp),
                        0f);
                    _cameraShakeAnchor.localPosition = originalLocalPos + randomOffset;
                })
                .setOnComplete(() =>
                {
                    _cameraShakeAnchor.localPosition = originalLocalPos; // reset
                    _shakeTweenId = -1;
                });

            _shakeTweenId = descr.id;
        }

        // Inserts a parent "CameraShakeAnchor" above the camera if not already present
        private Transform GetOrCreateShakeAnchor(Transform camTransform)
        {
            if (camTransform == null) return null;

            // If already anchored, reuse it
            if (camTransform.parent != null && camTransform.parent.name == "CameraShakeAnchor")
            {
                return camTransform.parent;
            }

            // Create an anchor and insert it between the current parent and the camera
            Transform currentParent = camTransform.parent;
            GameObject anchorGO = new GameObject("CameraShakeAnchor");
            Transform anchor = anchorGO.transform;

            // Match world pose and maintain hierarchy
            if (currentParent != null)
            {
                anchor.SetParent(currentParent, worldPositionStays: false);
            }

            anchor.position = camTransform.position;
            anchor.rotation = camTransform.rotation;
            anchor.localScale = camTransform.localScale;

            camTransform.SetParent(anchor, worldPositionStays: false);

            // Ensure local position is zero so offsets apply as expected
            camTransform.localPosition = Vector3.zero;

            return anchor;
        }

        // Attempts to get the active camera even if no object has the "MainCamera" tag (common on mobile builds)
        private Camera ResolveActiveCamera()
        {
            // Try the tagged main camera first
            if (Camera.main != null)
                return Camera.main;

            // Fallback: any enabled camera
            var all = Camera.allCameras;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].enabled)
                    return all[i];
            }

            // Last resort: search the scene (Unity 6 API)
            var anyCam = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
            if (anyCam != null && anyCam.enabled)
                return anyCam;

            return null;
        }


        public void InitSequenceTransitions(bool fastTransition = false)
        {
            // Animate each colorTarget child with staggered delays
            if (sequenceContainer != null)
            {
                // Find the grid layout group container (first child)
                Transform gridContainer = sequenceContainer.transform.GetChild(0);

                if (gridContainer != null)
                {
                    int childCount = gridContainer.childCount;

                    // Choose timing based on context
                    float staggerDelay = fastTransition ? 0.03f : 0.25f;
                    float animationDuration = fastTransition ? 0.12f : 0.4f;

                    for (int i = 0; i < childCount; i++)
                    {
                        Transform colorTarget = gridContainer.GetChild(i);
                        float delay = 0 + (i * staggerDelay);

                        // Start from scale 0
                        LeanTween.scale(colorTarget.gameObject, Vector3.zero, 0f);

                        // Scale up animation with context-appropriate timing
                        LeanTween.scale(colorTarget.gameObject, new Vector3(.85f, .85f, 0), animationDuration)
                            .setDelay(delay)
                            .setEase(LeanTweenType.easeOutBack);
                    }
                }
            }
        }
    }
}
