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

        /// <summary>
        /// Shakes the main camera for a brief moment when a balloon pops
        /// </summary>
        /// <param name="intensity">Shake intensity (default: 0.1f)</param>
        /// <param name="duration">Shake duration (default: 0.2f)</param>
        public void ShakeCamera(float intensity = 0.1f, float duration = 0.2f)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found for shake effect!");
                return;
            }

            Vector3 originalPosition = mainCamera.transform.position;

            // Create a shake sequence
            LeanTween.moveX(mainCamera.gameObject, originalPosition.x + intensity, duration * 0.1f)
                .setEase(LeanTweenType.easeShake)
                .setLoopPingPong(Mathf.RoundToInt(duration * 20))
                .setOnComplete(() =>
                {
                    // Reset camera to original position
                    mainCamera.transform.position = originalPosition;
                });
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
