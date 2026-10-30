using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialSpawnInterval = 2.0f;
    [SerializeField] private float minimumSpawnInterval = 0.25f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.04f;

    [Header("Spawn Distance (Outside View)")]
    [SerializeField] private float spawnRadiusMin = 10f;
    [SerializeField] private float spawnRadiusMax = 14f;

    [Header("Base Monster Attributes")]
    [SerializeField] private int baseEnemyHealth = 50;
    [SerializeField] private int baseEnemyDamage = 10;

    [Header("Time-Based Progression Scaling")]
    [Tooltip("Health increase percentage per minute (e.g. 0.8 = +80% HP per minute)")]
    [SerializeField] private float healthScalePerMinute = 0.85f;
    [Tooltip("Damage increase percentage per minute (e.g. 0.45 = +45% Damage per minute)")]
    [SerializeField] private float damageScalePerMinute = 0.45f;
    [Tooltip("Base chance to spawn an Elite monster (increases over time)")]
    [SerializeField] private float baseEliteChance = 0.05f;

    public static EnemySpawner Instance { get; private set; }
    public GameObject EnemyPrefab => enemyPrefab;

    private Transform playerTransform;
    private float currentSpawnInterval;
    private float spawnTimer;
    private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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

        elapsedTime += Time.deltaTime;

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

    public GameObject SpawnCustomEnemy(Vector3 position, int health, int damage, float speedMult = 1f, Color? tint = null, float scaleMult = 1f, int expReward = 15)
    {
        if (enemyPrefab == null) return null;
        GameObject spawned = Instantiate(enemyPrefab, position, Quaternion.identity);

        if (Mathf.Abs(scaleMult - 1f) > 0.01f)
        {
            spawned.transform.localScale = spawned.transform.localScale * scaleMult;
        }

        EnemyController ctrl = spawned.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            ctrl.SetCombatStats(damage, speedMult, tint);
        }

        EnemyHealth eh = spawned.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.SetHealthStats(health, expReward);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemySpawn();
        }

        return spawned;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null) return;

        // Random position in a ring outside player view
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector3 spawnPosition = (Vector3)playerTransform.position + (Vector3)(randomDir * distance);
        spawnPosition.z = 0f;

        // Calculate time scaling (minutes elapsed)
        float minutesElapsed = elapsedTime / 60f;
        float hpMult = 1f + (minutesElapsed * healthScalePerMinute);
        float dmgMult = 1f + (minutesElapsed * damageScalePerMinute);

        // Elite roll (5% at start -> up to 30% at 5 minutes)
        float eliteChance = Mathf.Clamp(baseEliteChance + (minutesElapsed * 0.05f), 0.05f, 0.30f);
        bool isElite = Random.value < eliteChance;

        int finalHealth = Mathf.RoundToInt(baseEnemyHealth * hpMult);
        int finalDamage = Mathf.RoundToInt(baseEnemyDamage * dmgMult);

        // Base EXP: 15 EXP per normal monster, boosted to 20 EXP during the first 2 minutes (early game fast leveling)
        int expReward = minutesElapsed < 2.0f ? 20 : 15;
        float scaleMult = 1f;
        float speedMult = 1.0f;
        Color? tint = null;

        if (isElite)
        {
            finalHealth = Mathf.RoundToInt(finalHealth * 2.8f);
            finalDamage = Mathf.RoundToInt(finalDamage * 1.6f);
            expReward = minutesElapsed < 2.0f ? 60 : 50;
            scaleMult = 1.45f;
            speedMult = 1.15f;
            tint = new Color(1f, 0.45f, 0.2f, 1f); // fiery golden/orange tint
        }

        SpawnCustomEnemy(spawnPosition, finalHealth, finalDamage, speedMult, tint, scaleMult, expReward);
    }
}
