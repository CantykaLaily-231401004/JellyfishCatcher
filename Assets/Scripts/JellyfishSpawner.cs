using UnityEngine;

public class JellyfishSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject jellyfishPrefab;

    public float spawnRate = 1f;
    public float minSpawnRate = 0.3f;
    public float difficultyIncrease = 0.003f;

    public int maxJellyfish = 20;
    public int maxJellyfishLimit = 40;

    [Header("Spawn Area")]
    public float spawnYPosition = 300f;
    public float spawnXMin = -350f;
    public float spawnXMax = 350f;

    private float spawnTimer = 0f;
    private int currentJellyfishCount = 0;

    void Update()
    {

        spawnRate -= Time.deltaTime * difficultyIncrease;
        spawnRate = Mathf.Clamp(spawnRate, minSpawnRate, 5f);

        if (maxJellyfish < maxJellyfishLimit)
        {
            maxJellyfish += Mathf.FloorToInt(Time.deltaTime * 0.2f);
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate && currentJellyfishCount < maxJellyfish)
        {
            SpawnJellyfish();
            spawnTimer = 0f;
        }
    }

    void SpawnJellyfish()
    {
        float randomX = Random.Range(spawnXMin, spawnXMax);

        // Tambah variasi spawn agar terasa hidup
        float randomYOffset = Random.Range(-20f, 20f);
        Vector3 spawnPosition = new Vector3(randomX, spawnYPosition + randomYOffset, 0);

        GameObject jellyfish = Instantiate(jellyfishPrefab, spawnPosition, Quaternion.identity);

        currentJellyfishCount++;
    }

    public void OnJellyfishDestroyed()
    {
        currentJellyfishCount--;
        if (currentJellyfishCount < 0)
            currentJellyfishCount = 0;
    }
}