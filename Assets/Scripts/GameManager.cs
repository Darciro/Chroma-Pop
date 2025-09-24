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
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
        }

        /// <summary>
        /// Starts a new game session with fresh state.
        /// </summary>
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

        /// <summary>
        /// Resets all game state to initial values.
        /// </summary>
        private void ResetGameState()
        {
            if (GameTween.Instance != null)
            {
                GameTween.Instance.GameOverTransitions();
            }

            gameState.Reset();
            scoreManager.ResetScore();
            healthManager.ResetHealth(startingHealth);
            sequenceManager.ClearSequence();

            countdownActive = false;
            currentCountdownTime = countdownTime;
            isProcessingSequenceChange = false;

            if (balloonSpawner != null)
            {
                balloonSpawner.StopSpawning();
                balloonSpawner.ClearAllBalloons();
                balloonSpawner.ResetSpawningState();
            }
        }

        /// <summary>
        /// Validates the color of a popped balloon against the current sequence.
        /// Handles scoring, health changes, and sequence progression.
        /// </summary>
        /// <param name="color">The color of the popped balloon</param>
        public void ValidateScore(BalloonColorEnum color)
        {
            // Ensure the game is in a valid state to process input
            if (!gameStarted || sequenceManager == null)
            {
                return;
            }

            // Prevent validation during sequence changes to avoid race conditions
            if (isProcessingSequenceChange)
            {
                return;
            }

            bool isCorrect = sequenceManager.ValidateNextColor(color);

            if (isCorrect)
            {
                OnCorrectSequence();
            }
            else
            {
                OnIncorrectSequence();
            }
        }

        /// <summary>
        /// Handles correct balloon sequence input.
        /// </summary>
        private void OnCorrectSequence()
        {
            scoreManager.AddScore(1);

            if (sequenceManager.IsSequenceComplete())
            {
                OnSequenceCompleted();
            }
        }

        /// <summary>
        /// Handles incorrect balloon sequence input.
        /// </summary>
        private void OnIncorrectSequence()
        {
            healthManager.ChangeHealth(-1);

            if (healthManager.GetHealth() <= 0)
            {
                OnGameOver();
            }
        }

        /// <summary>
        /// Handles sequence completion with bonus scoring and new sequence generation.
        /// </summary>
        private void OnSequenceCompleted()
        {
            if (isProcessingSequenceChange) return;

            isProcessingSequenceChange = true;

            // Award bonus points for completing the sequence
            scoreManager.AddScore(sequenceCompletionBonus);

            // Generate new sequence and restart countdown
            sequenceManager.GenerateNewSequence();
            StartCountdown();

            isProcessingSequenceChange = false;
        }

        /// <summary>
        /// Starts or restarts the countdown timer for the current sequence.
        /// </summary>
        private void StartCountdown()
        {
            currentCountdownTime = countdownTime;
            countdownActive = true;
            UpdateCountdownSlider();
        }

        /// <summary>
        /// Updates the countdown timer and handles expiration.
        /// </summary>
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

        /// <summary>
        /// Updates the countdown slider UI to reflect current timer state.
        /// </summary>
        private void UpdateCountdownSlider()
        {
            if (countdownSlider != null)
            {
                float fillAmount = currentCountdownTime / countdownTime;
                countdownSlider.fillAmount = fillAmount;
            }
        }

        /// <summary>
        /// Handles countdown timer expiration by reducing health and generating new sequence.
        /// </summary>
        private void OnCountdownExpired()
        {
            if (isProcessingSequenceChange) return;

            isProcessingSequenceChange = true;
            countdownActive = false;

            // Reduce health when countdown expires
            healthManager.ChangeHealth(-1);

            // Check for game over
            if (healthManager.GetHealth() <= 0)
            {
                isProcessingSequenceChange = false;
                OnGameOver();
                return;
            }

            // Generate new sequence and restart countdown
            sequenceManager.GenerateNewSequence();
            StartCountdown();

            isProcessingSequenceChange = false;
        }

        /// <summary>
        /// Handles game over state by stopping spawning, updating UI, and triggering transitions.
        /// </summary>
        private void OnGameOver()
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = GetScore().ToString();
            }

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

            if (GameTween.Instance != null)
            {
                GameTween.Instance.InitGameOverTransitions();
            }
        }

        /// <summary>
        /// Called by BalloonSpawner when it begins spawning balloons.
        /// Starts the countdown timer for the first game session.
        /// </summary>
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

        /// <summary>
        /// Updates all UI elements with current game state.
        /// </summary>
        private void UpdateUI()
        {
            scoreManager.UpdateUI();
            healthManager.UpdateUI();
        }

        /// <summary>
        /// Generates a new color sequence for the player to match.
        /// </summary>
        public void GenerateNewSequence()
        {
            sequenceManager.GenerateNewSequence();
        }

        // Public API for external access
        /// <summary>
        /// Adds points to the current score.
        /// </summary>
        /// <param name="amount">Points to add</param>
        public void AddScore(int amount) => scoreManager.AddScore(amount);

        /// <summary>
        /// Sets the score to a specific value.
        /// </summary>
        /// <param name="value">New score value</param>
        public void SetScore(int value) => scoreManager.SetScore(value);

        /// <summary>
        /// Gets the current score.
        /// </summary>
        /// <returns>Current score value</returns>
        public int GetScore() => scoreManager.GetScore();

        /// <summary>
        /// Changes the player's health by the specified amount.
        /// </summary>
        /// <param name="delta">Health change (positive or negative)</param>
        public void ChangeHealth(int delta) => healthManager.ChangeHealth(delta);

        /// <summary>
        /// Sets the player's health to a specific value.
        /// </summary>
        /// <param name="value">New health value</param>
        public void SetHealth(int value) => healthManager.SetHealth(value);

        /// <summary>
        /// Gets the current health.
        /// </summary>
        /// <returns>Current health value</returns>
        public int GetHealth() => healthManager.GetHealth();
    }
}
