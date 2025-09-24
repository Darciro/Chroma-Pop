using TMPro;
using UnityEngine;

namespace ChromaPop
{
    /// <summary>
    /// Handles all score-related functionality including display and persistence.
    /// </summary>
    [System.Serializable]
    public class ScoreManager
    {
        private int score = 0;
        private readonly TextMeshProUGUI scoreText;

        public ScoreManager(TextMeshProUGUI scoreText)
        {
            this.scoreText = scoreText;
        }

        /// <summary>
        /// Adds points to the current score.
        /// </summary>
        /// <param name="amount">Points to add</param>
        public void AddScore(int amount)
        {
            score += amount;
            UpdateUI();
        }

        /// <summary>
        /// Sets the score to a specific value.
        /// </summary>
        /// <param name="value">New score value</param>
        public void SetScore(int value)
        {
            score = value;
            UpdateUI();
        }

        /// <summary>
        /// Gets the current score.
        /// </summary>
        /// <returns>Current score value</returns>
        public int GetScore() => score;

        /// <summary>
        /// Resets the score to zero.
        /// </summary>
        public void ResetScore()
        {
            score = 0;
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI text with the current score.
        /// </summary>
        public void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
    }
}
