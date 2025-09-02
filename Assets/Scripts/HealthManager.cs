using TMPro;
using UnityEngine;

namespace ChromaPop
{
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
}
