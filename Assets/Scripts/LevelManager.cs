using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChromaPop
{
    public class LevelManager : MonoBehaviour
    {
        public void StartGame()
        {
            // Load the specified scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
