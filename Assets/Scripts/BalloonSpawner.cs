using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChromaPop
{
    /// <summary>
    /// Handles the spawning and management of balloons in the game.
    /// </summary>
    public class BalloonSpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        [SerializeField] private GameObject balloonPrefab;
        [SerializeField] private float spawnIntervalMin = 1f;
        [SerializeField] private float spawnIntervalMax = 3f;
        [SerializeField] private float initialDelayBeforeFirstSpawn = 1f;

        [Header("Spawn Area")]
        [SerializeField] private float xMin = -2.5f;
        [SerializeField] private float xMax = 2.5f;
        [SerializeField] private float ySpawn = -5f;
        [SerializeField] private float yDestroy = 6f;

        private readonly List<GameObject> spawnedBalloons = new List<GameObject>();
        private Coroutine spawningCoroutine;
        private bool hasStartedSpawning = false;

        private void Start()
        {
            spawningCoroutine = StartCoroutine(WaitAndStartSpawning());
        }

        /// <summary>
        /// Waits for game start conditions and then begins balloon spawning loop.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator WaitAndStartSpawning()
        {
            // Wait until the game allows spawning
            yield return new WaitUntil(() => ShouldSpawnBalloons());

            // Wait for the initial delay
            yield return new WaitForSeconds(initialDelayBeforeFirstSpawn);

            // Notify GameManager that spawning is about to begin
            if (GameManager.Instance != null && !hasStartedSpawning)
            {
                hasStartedSpawning = true;
                GameManager.Instance.OnBalloonsStartSpawning();
            }

            // Start the spawning loop
            while (true)
            {
                // Wait for the next spawn interval
                float spawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
                yield return new WaitForSeconds(spawnInterval);

                // Check if we should still spawn balloons
                if (ShouldSpawnBalloons())
                {
                    SpawnBalloon();
                }
            }
        }

        /// <summary>
        /// Checks if balloons should currently be spawned.
        /// </summary>
        /// <returns>True if conditions allow balloon spawning</returns>
        private bool ShouldSpawnBalloons()
        {
            return GameManager.Instance != null && GameManager.Instance.gameStarted;
        }

        private void Update()
        {
            CleanupBalloons();
        }

        /// <summary>
        /// Spawns a new balloon at a random position with random color and speed.
        /// </summary>
        private void SpawnBalloon()
        {
            if (balloonPrefab == null)
            {
                Debug.LogError("Balloon prefab is not assigned!", this);
                return;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);

            ConfigureBalloon(balloon);
            spawnedBalloons.Add(balloon);
        }

        /// <summary>
        /// Gets a random spawn position within the configured boundaries.
        /// </summary>
        /// <returns>Random spawn position</returns>
        private Vector3 GetRandomSpawnPosition()
        {
            float xPos = Random.Range(xMin, xMax);
            return new Vector3(xPos, ySpawn, 0f);
        }

        /// <summary>
        /// Configures a spawned balloon with random color and speed.
        /// </summary>
        /// <param name="balloon">The balloon GameObject to configure</param>
        private void ConfigureBalloon(GameObject balloon)
        {
            BalloonController balloonController = balloon.GetComponent<BalloonController>();

            if (balloonController == null)
            {
                Debug.LogError("Balloon prefab doesn't have a BalloonController component!", this);
                return;
            }

            // Set random color
            BalloonColorEnum randomColor = GetRandomBalloonColor();
            balloonController.SetBalloonColor(randomColor);

            // Set random speed within the balloon's configured range
            balloonController.SetRandomFloatSpeed();
        }

        /// <summary>
        /// Gets a random balloon color from all available colors.
        /// </summary>
        /// <returns>Random balloon color enum value</returns>
        private BalloonColorEnum GetRandomBalloonColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(Random.Range(0, colorValues.Length));
        }

        /// <summary>
        /// Cleans up destroyed or out-of-bounds balloons from the spawned balloons list.
        /// </summary>
        private void CleanupBalloons()
        {
            for (int i = spawnedBalloons.Count - 1; i >= 0; i--)
            {
                GameObject balloon = spawnedBalloons[i];

                if (ShouldRemoveBalloon(balloon))
                {
                    RemoveBalloonFromList(i);
                }
            }
        }

        /// <summary>
        /// Checks if a balloon should be removed from the spawned list.
        /// </summary>
        /// <param name="balloon">The balloon to check</param>
        /// <returns>True if the balloon should be removed</returns>
        private bool ShouldRemoveBalloon(GameObject balloon)
        {
            if (balloon == null) return true;

            if (balloon.transform.position.y > yDestroy)
            {
                Destroy(balloon);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a balloon from the spawned balloons list at the specified index.
        /// </summary>
        /// <param name="index">Index of the balloon to remove</param>
        private void RemoveBalloonFromList(int index)
        {
            spawnedBalloons.RemoveAt(index);
        }

        /// <summary>
        /// Stops the current spawning coroutine.
        /// </summary>
        public void StopSpawning()
        {
            if (spawningCoroutine != null)
            {
                StopCoroutine(spawningCoroutine);
                spawningCoroutine = null;
            }
        }

        /// <summary>
        /// Starts the balloon spawning process.
        /// </summary>
        public void StartSpawning()
        {
            StopSpawning(); // Stop any existing coroutine first
            hasStartedSpawning = false; // Reset the flag when restarting
            spawningCoroutine = StartCoroutine(WaitAndStartSpawning());
        }

        /// <summary>
        /// Destroys all currently spawned balloons and clears the list.
        /// </summary>
        public void ClearAllBalloons()
        {
            foreach (GameObject balloon in spawnedBalloons)
            {
                if (balloon != null)
                {
                    Destroy(balloon);
                }
            }
            spawnedBalloons.Clear();
        }

        /// <summary>
        /// Resets the spawning state flags.
        /// </summary>
        public void ResetSpawningState()
        {
            hasStartedSpawning = false;
        }
    }
}
