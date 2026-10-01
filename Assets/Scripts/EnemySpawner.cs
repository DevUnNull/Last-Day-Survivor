using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialSpawnInterval = 2.5f;
    [SerializeField] private float minimumSpawnInterval = 0.3f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.05f; // Decreases spawn interval over time

    [Header("Spawn Distance (Outside View)")]
    [SerializeField] private float spawnRadiusMin = 10f; // Outside camera view radius
    [SerializeField] private float spawnRadiusMax = 14f;

    private Transform playerTransform;
    private float currentSpawnInterval;
    private float spawnTimer;

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy) return;

        // Gradually decrease spawn interval over time to increase density
        if (currentSpawnInterval > minimumSpawnInterval)
        {
            currentSpawnInterval -= spawnIntervalDecreaseRate * Time.deltaTime;
            if (currentSpawnInterval < minimumSpawnInterval)
            {
                currentSpawnInterval = minimumSpawnInterval;
            }
        }

        // Spawn timer
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // Random position in a ring outside player view
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector3 spawnPosition = (Vector3)playerTransform.position + (Vector3)(randomDir * distance);
        spawnPosition.z = 0f;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
