using UnityEngine;

namespace ChromaPop
{
    /// <summary>
    /// Manages the game's overall state and timing.
    /// </summary>
    [System.Serializable]
    public class GameState
    {
        public bool isGameActive;
        public float gameTime;

        /// <summary>
        /// Resets the game state to initial values.
        /// </summary>
        public void Reset()
        {
            isGameActive = false;
            gameTime = 0f;
        }
    }
}
