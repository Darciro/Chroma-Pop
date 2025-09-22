using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChromaPop
{
    /// <summary>
    /// Main game manager that handles game state, scoring, and sequence management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        // [SerializeField] private GameObject gameOverScreen;

        [Header("Sequence UI")]
        [SerializeField] private RectTransform sequenceContainerGrid;
        [SerializeField] private GameObject colorTargetPrefab;
        [SerializeField] private Image countdownSlider;

        [Header("Game Settings")]
        [SerializeField] private int startingHealth = 3;
        [SerializeField] private int sequenceLength = 3;
        [SerializeField] private int sequenceCompletionBonus = 10;
        [SerializeField] private float countdownTime = 10f;

        // Game State
        private GameState gameState;
        private ScoreManager scoreManager;
        private HealthManager healthManager;
        private SequenceManager sequenceManager;

        [Header("References")]
        [SerializeField] private BalloonSpawner balloonSpawner;

        // Countdown variables
        private float currentCountdownTime;
        private bool countdownActive = false;
        private bool isProcessingSequenceChange = false; // Prevent race conditions
        private bool isFirstGameSession = true; // Track if this is the first game session

        public bool gameStarted { get; private set; } = false;

        private void Awake()
        {
            InitializeSingleton();
            InitializeManagers();
        }

        private void Start()
        {
            StartGame();
        }

        private void Update()
        {
            if (gameStarted && countdownActive)
            {
                UpdateCountdown();
            }
        }

        private void InitializeSingleton()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void InitializeManagers()
        {
            gameState = new GameState();
            scoreManager = new ScoreManager(scoreText);
            healthManager = new HealthManager(healthText, startingHealth);
            sequenceManager = new SequenceManager(sequenceContainerGrid, colorTargetPrefab, sequenceLength);

            // Initialize swipe handling
            InitializeSwipeHandling();
        }

        private void InitializeSwipeHandling()
        {
            // Subscribe to swipe detector events
            SwipeDetector.OnSwipeDetected += HandleSwipeDetected;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            SwipeDetector.OnSwipeDetected -= HandleSwipeDetected;
        }

        private void HandleSwipeDetected(Vector2 swipeDirection)
        {
            // Only handle swipes when the game is active and not during other animations
            if (gameStarted && !sequenceManager.IsAnimatingSwipe())
            {
                sequenceManager.HandleSwipe(swipeDirection);
                // Note: Countdown continues uninterrupted during swipes
            }
        }

        public void StartGame()
        {
            ResetGameState();
            gameStarted = true;

            sequenceManager.GenerateNewSequence();
            UpdateUI();

            if (balloonSpawner != null)
            {
                balloonSpawner.StartSpawning();
            }

            // Only start countdown immediately if this is NOT the first game session
            // For first session, wait for balloons to start spawning
            if (!isFirstGameSession)
            {
                StartCountdown();
            }
        }

        /// <summary>
        /// Restarts the game completely. Can be called from UI buttons or other restart triggers.
        /// </summary>
        public void RestartGame()
        {
            StartGame();
        }

        private void ResetGameState()
        {
            GameTween.Instance.GameOverTransitions();
            gameState.Reset();
            scoreManager.ResetScore();
            healthManager.ResetHealth(startingHealth);
            sequenceManager.ClearSequence();

            countdownActive = false;
            currentCountdownTime = countdownTime;
            isProcessingSequenceChange = false; // Reset the flag

            if (balloonSpawner != null)
            {
                balloonSpawner.StopSpawning();
                balloonSpawner.ClearAllBalloons();
                balloonSpawner.ResetSpawningState();
            }
        }

        public void ValidateScore(BalloonColorEnum color)
        {
            // Ensure the game is in a valid state to process input
            if (!gameStarted || sequenceManager == null) return;

            // Prevent validation during sequence changes to avoid race conditions
            if (isProcessingSequenceChange) return;

            if (sequenceManager.ValidateNextColor(color))
            {
                OnCorrectSequence();
            }
            else
            {
                OnIncorrectSequence();
            }
        }

        private void OnCorrectSequence()
        {
            scoreManager.AddScore(1);

            if (sequenceManager.IsSequenceComplete())
            {
                OnSequenceCompleted();
            }
            // Don't restart countdown for individual correct balloons - only when full sequence is complete
        }

        private void OnIncorrectSequence()
        {
            healthManager.ChangeHealth(-1);

            if (healthManager.GetHealth() <= 0)
            {
                OnGameOver();
            }
        }

        private void OnSequenceCompleted()
        {
            if (isProcessingSequenceChange)
            {
                return;
            }

            isProcessingSequenceChange = true;
            int currentHealth = healthManager.GetHealth();

            scoreManager.AddScore(sequenceCompletionBonus);
            sequenceManager.GenerateNewSequence();
            StartCountdown(); // Restart countdown when sequence is completed

            int healthAfter = healthManager.GetHealth();
            isProcessingSequenceChange = false;
        }

        private void StartCountdown()
        {
            currentCountdownTime = countdownTime;
            countdownActive = true;
            UpdateCountdownSlider();
        }
        private void UpdateCountdown()
        {
            if (currentCountdownTime > 0)
            {
                currentCountdownTime -= Time.deltaTime;
                UpdateCountdownSlider();
            }
            else
            {
                OnCountdownExpired();
            }
        }

        private void UpdateCountdownSlider()
        {
            if (countdownSlider != null)
            {
                float fillAmount = currentCountdownTime / countdownTime;
                countdownSlider.fillAmount = fillAmount;
            }
        }

        private void OnCountdownExpired()
        {
            if (isProcessingSequenceChange)
            {
                return;
            }

            isProcessingSequenceChange = true;
            countdownActive = false;
            // Restart the sequence when countdown reaches zero
            sequenceManager.GenerateNewSequence();
            StartCountdown();
            isProcessingSequenceChange = false;
        }

        private void OnGameOver()
        {
            finalScoreText.text = GetScore().ToString();
            gameStarted = false;
            countdownActive = false;

            if (balloonSpawner != null)
            {
                balloonSpawner.StopSpawning();
                balloonSpawner.ClearAllBalloons();
            }

            sequenceManager.ClearSequence();

            currentCountdownTime = 0f;
            UpdateCountdownSlider();

            GameTween.Instance.InitGameOverTransitions();
        }

        public void OnBalloonsStartSpawning()
        {
            // Start the countdown when balloons actually begin spawning
            if (gameStarted && !countdownActive)
            {
                StartCountdown();
            }

            // Mark that we've completed the first game session
            if (isFirstGameSession)
            {
                isFirstGameSession = false;
            }
        }
        private void UpdateUI()
        {
            scoreManager.UpdateUI();
            healthManager.UpdateUI();
        }

        // Public API for external access
        public void AddScore(int amount) => scoreManager.AddScore(amount);
        public void SetScore(int value) => scoreManager.SetScore(value);
        public int GetScore() => scoreManager.GetScore();
        public void ChangeHealth(int delta) => healthManager.ChangeHealth(delta);
        public void SetHealth(int value) => healthManager.SetHealth(value);
        public int GetHealth() => healthManager.GetHealth();
    }
}
