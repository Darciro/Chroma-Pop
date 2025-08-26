using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    public GameObject balloonPrefab;
    public float spawnIntervalMin = 1f;
    public float spawnIntervalMax = 3f;
    private float nextSpawnInterval;
    public float xMin = -2.5f;
    public float xMax = 2.5f;
    public float ySpawn = -5f;
    public float yDestroy = 6f;

    private float timer;
    private readonly System.Collections.Generic.List<GameObject> spawnedBalloons = new System.Collections.Generic.List<GameObject>();

    void Update()
    {
        if (GameManager.Instance.gameStarted != true)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= nextSpawnInterval)
        {
            SpawnBalloon();
            timer = 0f;
            nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
        ValidateBalloons();
    }

    void Start()
    {
        nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void SpawnBalloon()
    {
        float xPos = Random.Range(xMin, xMax);
        Vector3 spawnPos = new Vector3(xPos, ySpawn, 0f);
        GameObject balloon = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);

        // Assign a random color to the balloon
        BalloonController balloonController = balloon.GetComponent<BalloonController>();
        if (balloonController != null)
        {
            // Get a random color from the enum
            BalloonColorEnum randomColor = (BalloonColorEnum)Random.Range(0, System.Enum.GetValues(typeof(BalloonColorEnum)).Length);
            balloonController.SetBalloonColor(randomColor);
        }

        spawnedBalloons.Add(balloon);
    }

    void ValidateBalloons()
    {
        for (int i = spawnedBalloons.Count - 1; i >= 0; i--)
        {
            GameObject balloon = spawnedBalloons[i];
            if (balloon == null)
            {
                spawnedBalloons.RemoveAt(i);
                continue;
            }
            if (balloon.transform.position.y > yDestroy)
            {
                Destroy(balloon);
                spawnedBalloons.RemoveAt(i);
            }
        }
    }
}
