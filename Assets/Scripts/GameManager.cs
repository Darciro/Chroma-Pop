using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;
    public int startingHealth = 3;
    public RectTransform sequenceContainerGrid;
    public GameObject colorTargetPrefab;
    public GameObject gameOverScreen;
    public bool gameStarted = false;

    private int score = 0;
    private int health = 0;
    private int sequences = 3;
    private int currentSequenceIndex = 0;

    private readonly List<GameObject> sequenceList = new List<GameObject>();
    private readonly List<BalloonColorEnum> colorSequenceList = new List<BalloonColorEnum>();

    void Awake()
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
        StartGame();
    }

    public void StartGame()
    {
        health = startingHealth;
        gameOverScreen.SetActive(false);
        gameStarted = true;
        InitializeSequence();
        UpdateScoreUI();
        UpdateHealthUI();
    }

    public void ValidateScore(BalloonColorEnum color)
    {
        // Check if the popped balloon color matches the expected color in sequence
        if (colorSequenceList[currentSequenceIndex] == color)
        {
            // Mark the current sequence item as checked (visual feedback)
            MarkSequenceItemAsChecked(currentSequenceIndex);

            // Move to next item in sequence
            currentSequenceIndex++;

            // Add score for correct sequence
            AddScore(1);

            // Check if sequence is complete
            if (currentSequenceIndex >= colorSequenceList.Count)
            {
                OnSequenceCompleted();
            }
        }
        else
        {
            // Player loses health for incorrect sequence
            ChangeHealth(-1);

            // Check if player has no health left
            if (GetHealth() <= 0)
            {
                OnGameOver();
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void SetScore(int value)
    {
        score = value;
        UpdateScoreUI();
    }

    public int GetScore() => score;

    public void ChangeHealth(int delta)
    {
        health += delta;
        if (health < 0) health = 0;
        UpdateHealthUI();
    }

    public void SetHealth(int value)
    {
        health = value;
        UpdateHealthUI();
    }

    public int GetHealth() => health;

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = health.ToString();
    }

    private void InitializeSequence()
    {
        ClearSequence();
        currentSequenceIndex = 0; // Reset sequence validation progress

        for (int i = 0; i < sequences; i++)
        {
            GameObject colorTargetInstance = Instantiate(colorTargetPrefab, sequenceContainerGrid);
            sequenceList.Add(colorTargetInstance);

            // Pick a random value from the BalloonColorEnum enum
            BalloonColorEnum randomBalloonColor = (BalloonColorEnum)Random.Range(0, System.Enum.GetValues(typeof(BalloonColorEnum)).Length);
            colorSequenceList.Add(randomBalloonColor);

            // Use the helper method to get the color
            Color targetColor = GetColorFromEnum(randomBalloonColor);

            var childImage = colorTargetInstance.transform.Find("Image")?.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = targetColor;
            }
        }
    }

    public void ClearSequence()
    {
        foreach (Transform child in sequenceContainerGrid)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < sequenceList.Count; i++)
        {
            if (sequenceList[i] != null)
                Destroy(sequenceList[i]);
        }
        sequenceList.Clear();
        colorSequenceList.Clear();
        currentSequenceIndex = 0; // Reset sequence progress when clearing
    }

    private void MarkSequenceItemAsChecked(int index)
    {
        if (index >= 0 && index < sequenceList.Count && sequenceList[index] != null)
        {
            // Find the child Image component and hide it, show checked state
            var childImage = sequenceList[index].transform.Find("Image")?.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.gameObject.SetActive(false);
                sequenceList[index].transform.Find("Checked")?.gameObject.SetActive(true);
            }
            else
            {
                // Try parent Image as fallback
                var parentImage = sequenceList[index].GetComponent<Image>();
                if (parentImage != null)
                {
                    // Disable all child elements first
                    foreach (Transform child in parentImage.transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    // Find and enable the "Checked" child element
                    Transform checkedChild = parentImage.transform.Find("Checked");
                    if (checkedChild != null)
                    {
                        checkedChild.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void ResetSequenceProgress()
    {
        currentSequenceIndex = 0;

        // Reset all sequence items to their original colors
        for (int i = 0; i < sequenceList.Count; i++)
        {
            if (sequenceList[i] != null && i < colorSequenceList.Count)
            {
                var childImage = sequenceList[i].transform.Find("Image")?.GetComponent<Image>();
                if (childImage != null)
                {
                    // Restore original color based on the sequence
                    Color originalColor = GetColorFromEnum(colorSequenceList[i]);
                    childImage.color = originalColor;
                }
            }
        }
    }

    private Color GetColorFromEnum(BalloonColorEnum balloonColor)
    {
        switch (balloonColor)
        {
            case BalloonColorEnum.Blue:
                return Color.blue;
            case BalloonColorEnum.Green:
                return Color.green;
            case BalloonColorEnum.Orange:
                return new Color(1f, 0.5f, 0f); // Orange
            case BalloonColorEnum.Pink:
                return new Color(1f, 0.75f, 0.8f); // Pink
            case BalloonColorEnum.Purple:
                return new Color(0.5f, 0f, 0.5f); // Purple
            case BalloonColorEnum.Red:
                return Color.red;
            case BalloonColorEnum.Yellow:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    private void OnSequenceCompleted()
    {
        // Add bonus score for completing sequence
        AddScore(50);

        // Generate a new sequence
        InitializeSequence();
    }

    private void OnGameOver()
    {
        gameStarted = false;
        // Show game over UI
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // Reset game state
        SetScore(0);
        SetHealth(0);
    }
}
