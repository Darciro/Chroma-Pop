using TMPro;
using UnityEngine;

namespace ChromaPop
{
    /// <summary>
    /// Handles all health-related functionality including display and validation.
    /// </summary>
    [System.Serializable]
    public class HealthManager
    {
        private int health = 0;
        private readonly TextMeshProUGUI healthText;

        public HealthManager(TextMeshProUGUI healthText, int startingHealth)
        {
            this.healthText = healthText;
            this.health = startingHealth;
        }

        /// <summary>
        /// Changes the player's health by the specified amount.
        /// Health cannot go below zero.
        /// </summary>
        /// <param name="delta">Health change (positive or negative)</param>
        public void ChangeHealth(int delta)
        {
            health = Mathf.Max(0, health + delta);
            UpdateUI();
        }

        /// <summary>
        /// Sets the player's health to a specific value.
        /// Health cannot go below zero.
        /// </summary>
        /// <param name="value">New health value</param>
        public void SetHealth(int value)
        {
            health = Mathf.Max(0, value);
            UpdateUI();
        }

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        /// <returns>Current health value</returns>
        public int GetHealth() => health;

        /// <summary>
        /// Resets health to the specified starting value.
        /// </summary>
        /// <param name="startingHealth">Initial health value</param>
        public void ResetHealth(int startingHealth)
        {
            health = startingHealth;
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI text with the current health.
        /// </summary>
        public void UpdateUI()
        {
            if (healthText != null)
            {
                healthText.text = health.ToString();
            }
        }
    }
}
