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
            int oldHealth = health;
            health = Mathf.Max(0, health + delta);
            Debug.Log($"[HealthManager] ChangeHealth: {oldHealth} -> {health} (delta: {delta})");
            UpdateUI();
        }

        public void SetHealth(int value)
        {
            int oldHealth = health;
            health = Mathf.Max(0, value);
            Debug.Log($"[HealthManager] SetHealth: {oldHealth} -> {health}");
            UpdateUI();
        }

        public int GetHealth() => health;

        public void ResetHealth(int startingHealth)
        {
            int oldHealth = health;
            health = startingHealth;
            Debug.Log($"[HealthManager] ResetHealth: {oldHealth} -> {health} (starting health: {startingHealth})");
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
