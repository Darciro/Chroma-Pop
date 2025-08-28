using System.Collections.Generic;
using ChromaPop.Core;
using ChromaPop.Gameplay;
using UnityEngine;
using BalloonColorEnum = ChromaPop.Core.BalloonColorEnum;

namespace ChromaPop.Gameplay
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

        [Header("Spawn Area")]
        [SerializeField] private float xMin = -2.5f;
        [SerializeField] private float xMax = 2.5f;
        [SerializeField] private float ySpawn = -5f;
        [SerializeField] private float yDestroy = 6f;

        private float timer;
        private float nextSpawnInterval;
        private readonly List<GameObject> spawnedBalloons = new List<GameObject>();

        private void Start()
        {
            SetRandomSpawnInterval();
        }

        private void Update()
        {
            if (!ShouldSpawnBalloons()) return;

            UpdateSpawnTimer();
            CleanupBalloons();
        }

        private bool ShouldSpawnBalloons()
        {
            return GameManager.Instance != null && GameManager.Instance.gameStarted;
        }

        private void UpdateSpawnTimer()
        {
            timer += Time.deltaTime;

            if (timer >= nextSpawnInterval)
            {
                SpawnBalloon();
                ResetSpawnTimer();
            }
        }

        private void ResetSpawnTimer()
        {
            timer = 0f;
            SetRandomSpawnInterval();
        }

        private void SetRandomSpawnInterval()
        {
            nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
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

            BalloonColorEnum randomColor = GetRandomBalloonColor();
            balloonController.SetBalloonColor(randomColor);
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
            enabled = false;
        }

        public void StartSpawning()
        {
            enabled = true;
            ResetSpawnTimer();
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
    }
}
