using TMPro;
using UnityEngine;

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
        [SerializeField] private GameObject gameOverScreen;

        [Header("Sequence UI")]
        [SerializeField] private RectTransform sequenceContainerGrid;
        [SerializeField] private GameObject colorTargetPrefab;

        [Header("Game Settings")]
        [SerializeField] private int startingHealth = 3;
        [SerializeField] private int sequenceLength = 3;
        [SerializeField] private int sequenceCompletionBonus = 10;

        // Game State
        private GameState gameState;
        private ScoreManager scoreManager;
        private HealthManager healthManager;
        private SequenceManager sequenceManager;

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

        public void StartGame()
        {
            ResetGameState();
            gameStarted = true;

            sequenceManager.GenerateNewSequence();
            UpdateUI();
        }

        private void ResetGameState()
        {
            GameTween.Instance.GameOverTransitions();
            gameState.Reset();
            scoreManager.ResetScore();
            healthManager.ResetHealth(startingHealth);
            sequenceManager.ClearSequence();
        }

        public void ValidateScore(BalloonColorEnum color)
        {
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
            scoreManager.AddScore(sequenceCompletionBonus);
            sequenceManager.GenerateNewSequence();
        }

        private void OnGameOver()
        {
            finalScoreText.text = GetScore().ToString();
            gameStarted = false;
            GameTween.Instance.InitGameOverTransitions();
            ResetGameState();
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
