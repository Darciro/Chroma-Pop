using TMPro;
using UnityEngine;

namespace ChromaPop
{
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
}
