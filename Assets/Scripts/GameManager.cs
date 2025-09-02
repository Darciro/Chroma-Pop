using System.Collections.Generic;
using ChromaPop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BalloonColorEnum = ChromaPop.BalloonColorEnum;

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

    /// <summary>
    /// Manages the game's overall state.
    /// </summary>
    [System.Serializable]
    public class GameState
    {
        public bool isGameActive;
        public float gameTime;

        public void Reset()
        {
            isGameActive = false;
            gameTime = 0f;
        }
    }

    /// <summary>
    /// Handles all score-related functionality.
    /// </summary>
    [System.Serializable]
    public class ScoreManager
    {
        private int score = 0;
        private TextMeshProUGUI scoreText;

        public ScoreManager(TextMeshProUGUI scoreText)
        {
            this.scoreText = scoreText;
        }

        public void AddScore(int amount)
        {
            score += amount;
            UpdateUI();
        }

        public void SetScore(int value)
        {
            score = value;
            UpdateUI();
        }

        public int GetScore() => score;

        public void ResetScore()
        {
            score = 0;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
    }

    /// <summary>
    /// Handles all health-related functionality.
    /// </summary>
    [System.Serializable]
    public class HealthManager
    {
        private int health = 0;
        private TextMeshProUGUI healthText;

        public HealthManager(TextMeshProUGUI healthText, int startingHealth)
        {
            this.healthText = healthText;
            this.health = startingHealth;
        }

        public void ChangeHealth(int delta)
        {
            health = Mathf.Max(0, health + delta);
            UpdateUI();
        }

        public void SetHealth(int value)
        {
            health = Mathf.Max(0, value);
            UpdateUI();
        }

        public int GetHealth() => health;

        public void ResetHealth(int startingHealth)
        {
            health = startingHealth;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (healthText != null)
            {
                healthText.text = health.ToString();
            }
        }
    }

    /// <summary>
    /// Handles color sequence generation and validation.
    /// </summary>
    [System.Serializable]
    public class SequenceManager
    {
        private readonly RectTransform sequenceContainer;
        private readonly GameObject colorTargetPrefab;
        private readonly int sequenceLength;

        private readonly List<GameObject> sequenceObjects = new List<GameObject>();
        private readonly List<BalloonColorEnum> colorSequence = new List<BalloonColorEnum>();
        private int currentSequenceIndex = 0;

        public SequenceManager(RectTransform container, GameObject prefab, int length)
        {
            sequenceContainer = container;
            colorTargetPrefab = prefab;
            sequenceLength = length;
        }

        public void GenerateNewSequence()
        {
            ClearSequence();
            CreateSequenceObjects();
            ResetProgress();
        }

        private void CreateSequenceObjects()
        {
            for (int i = 0; i < sequenceLength; i++)
            {
                CreateSequenceItem();
            }
        }

        private void CreateSequenceItem()
        {
            if (colorTargetPrefab == null || sequenceContainer == null)
            {
                Debug.LogError("Sequence UI components not properly configured!");
                return;
            }

            GameObject sequenceItem = Object.Instantiate(colorTargetPrefab, sequenceContainer);
            sequenceObjects.Add(sequenceItem);

            BalloonColorEnum randomColor = GetRandomColor();
            colorSequence.Add(randomColor);

            SetSequenceItemColor(sequenceItem, randomColor);
            GameTween.Instance.InitSequenceTransitions();
        }

        private BalloonColorEnum GetRandomColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(Random.Range(0, colorValues.Length));
        }

        private void SetSequenceItemColor(GameObject item, BalloonColorEnum color)
        {
            Color targetColor = ColorUtility.GetColorFromEnum(color);

            var childImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = targetColor;
            }
        }

        public bool ValidateNextColor(BalloonColorEnum color)
        {
            if (currentSequenceIndex >= colorSequence.Count)
                return false;

            bool isCorrect = colorSequence[currentSequenceIndex] == color;

            if (isCorrect)
            {
                MarkSequenceItemAsCompleted(currentSequenceIndex);
                currentSequenceIndex++;
            }

            return isCorrect;
        }

        private void MarkSequenceItemAsCompleted(int index)
        {
            if (index < 0 || index >= sequenceObjects.Count || sequenceObjects[index] == null)
                return;

            GameObject item = sequenceObjects[index];

            // Hide the color image
            var childImage = item.transform.Find("Image");
            if (childImage != null)
            {
                childImage.gameObject.SetActive(false);
            }

            // Show the checked indicator
            var checkedIndicator = item.transform.Find("Checked");
            if (checkedIndicator != null)
            {
                checkedIndicator.gameObject.SetActive(true);
            }
        }

        public bool IsSequenceComplete()
        {
            return currentSequenceIndex >= colorSequence.Count;
        }

        public void ClearSequence()
        {

            // Clear any existing UI objects from the container
            for (int i = sequenceContainer.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(sequenceContainer.GetChild(i).gameObject);
            }

            foreach (GameObject obj in sequenceObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }

            sequenceObjects.Clear();
            colorSequence.Clear();
            currentSequenceIndex = 0;
        }

        private void ResetProgress()
        {
            currentSequenceIndex = 0;
        }
    }

    /// <summary>
    /// Utility class for color conversions.
    /// </summary>
    public static class ColorUtility
    {
        public static Color GetColorFromEnum(BalloonColorEnum balloonColor)
        {
            return balloonColor switch
            {
                BalloonColorEnum.Blue => new Color(.3f, 0.8f, 1f),
                BalloonColorEnum.Green => new Color(.68f, 0.85f, 0),
                BalloonColorEnum.Orange => new Color(1f, 0.5f, 0f),
                BalloonColorEnum.Pink => new Color(1f, 0.75f, 0.8f),
                BalloonColorEnum.Purple => new Color(0.5f, 0f, 0.5f),
                BalloonColorEnum.Red => new Color(0.83f, 0, 0),
                BalloonColorEnum.Yellow => new Color(1f, .83f, .16f),
                _ => Color.white
            };
        }
    }
}
