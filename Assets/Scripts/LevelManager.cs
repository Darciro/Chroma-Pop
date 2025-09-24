using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChromaPop
{
    /// <summary>
    /// Handles scene management and level transitions.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        /// <summary>
        /// Starts the game by loading the next scene in the build order.
        /// </summary>
        public void StartGame()
        {
            // Load the specified scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
