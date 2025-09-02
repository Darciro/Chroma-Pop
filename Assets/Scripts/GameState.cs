using UnityEngine;

namespace ChromaPop
{
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
}
