using UnityEngine;

namespace ChromaPop
{
    public class GameTween : MonoBehaviour
    {
        public static GameTween Instance { get; private set; }

        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject sequenceContainer;

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
            // Game Over Screen
            LeanTween.alphaCanvas(gameOverScreen.GetComponent<CanvasGroup>(), 0, 0);
        }

        public void InitGameOverTransitions()
        {
            // Game Over Screen
            LeanTween.alphaCanvas(gameOverScreen.GetComponent<CanvasGroup>(), 1f, .5f).setDelay(0.25f);
        }

        public void ShakeCamera(float intensity = 0.1f, float duration = 0.2f)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found for shake effect!");
                return;
            }

            Transform camTransform = mainCamera.transform;
            Vector3 originalPosition = camTransform.localPosition;

            int shakeCount = Mathf.CeilToInt(duration / 0.02f); // 50 FPS shake steps
            float shakeDuration = duration / shakeCount;

            // Cancel any existing tweens on the camera to prevent overlap
            LeanTween.cancel(mainCamera.gameObject);

            void ShakeStep(int count)
            {
                if (count <= 0)
                {
                    camTransform.localPosition = originalPosition; // Reset
                    return;
                }

                Vector3 randomOffset = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    0f
                );

                LeanTween.moveLocal(mainCamera.gameObject, originalPosition + randomOffset, shakeDuration)
                    .setEase(LeanTweenType.easeShake)
                    .setOnComplete(() => ShakeStep(count - 1));
            }

            ShakeStep(shakeCount);
        }


        public void InitSequenceTransitions()
        {
            // Animate each colorTarget child with staggered delays
            if (sequenceContainer != null)
            {
                // Find the grid layout group container (first child)
                Transform gridContainer = sequenceContainer.transform.GetChild(0);

                if (gridContainer != null)
                {
                    int childCount = gridContainer.childCount;

                    for (int i = 0; i < childCount; i++)
                    {
                        Transform colorTarget = gridContainer.GetChild(i);
                        float delay = 0 + (i * 0.25f); // Base delay + staggered delay

                        // Start from scale 0
                        LeanTween.scale(colorTarget.gameObject, Vector3.zero, 0f);

                        // Scale up animation
                        LeanTween.scale(colorTarget.gameObject, Vector3.one, 0.4f)
                            .setDelay(delay)
                            .setEase(LeanTweenType.easeOutBack);
                    }
                }
            }
        }
    }
}
