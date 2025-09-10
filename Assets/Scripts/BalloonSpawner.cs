using System.Collections;
using System.Collections.Generic;
using ChromaPop;
using UnityEngine;
using BalloonColorEnum = ChromaPop.BalloonColorEnum;

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

        private IEnumerator WaitAndStartSpawning()
        {
            // Wait until the game allows spawning
            yield return new WaitUntil(() => ShouldSpawnBalloons());

            // Wait for the initial delay
            yield return new WaitForSeconds(initialDelayBeforeFirstSpawn);

            // Notify GameManager that spawning is about to begin
            if (GameManager.Instance != null && !hasStartedSpawning)
            {
                Debug.Log("[BalloonSpawner] Notifying GameManager that balloons are starting to spawn");
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

        private void Update()
        {
            // Only cleanup balloons in Update
            CleanupBalloons();
        }

        private bool ShouldSpawnBalloons()
        {
            return GameManager.Instance != null && GameManager.Instance.gameStarted;
        }

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

        private Vector3 GetRandomSpawnPosition()
        {
            float xPos = Random.Range(xMin, xMax);
            return new Vector3(xPos, ySpawn, 0f);
        }

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

        private BalloonColorEnum GetRandomBalloonColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(Random.Range(0, colorValues.Length));
        }

        private void CleanupBalloons()
        {
            for (int i = spawnedBalloons.Count - 1; i >= 0; i--)
            {
                GameObject balloon = spawnedBalloons[i];

                if (ShouldRemoveBalloon(balloon, i))
                {
                    RemoveBalloonFromList(i);
                }
            }
        }

        private bool ShouldRemoveBalloon(GameObject balloon, int index)
        {
            if (balloon == null) return true;

            if (balloon.transform.position.y > yDestroy)
            {
                Destroy(balloon);
                return true;
            }

            return false;
        }

        private void RemoveBalloonFromList(int index)
        {
            spawnedBalloons.RemoveAt(index);
        }

        public void StopSpawning()
        {
            if (spawningCoroutine != null)
            {
                StopCoroutine(spawningCoroutine);
                spawningCoroutine = null;
            }
        }

        public void StartSpawning()
        {
            Debug.Log("[BalloonSpawner] StartSpawning called");
            StopSpawning(); // Stop any existing coroutine first
            hasStartedSpawning = false; // Reset the flag when restarting
            spawningCoroutine = StartCoroutine(WaitAndStartSpawning());
        }

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

        public void ResetSpawningState()
        {
            Debug.Log("[BalloonSpawner] Resetting spawning state");
            hasStartedSpawning = false;
        }
    }
}
