using System.Collections;
using UnityEngine;

public class BossSpawnManager : MonoBehaviour
{
    public static BossSpawnManager Instance { get; private set; }

    [Header("Boss Prefabs")]
    [SerializeField] private GameObject defaultBossPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceFromPlayer = 8f;
    [SerializeField] private GameObject bossWarningBanner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (bossWarningBanner != null) bossWarningBanner.SetActive(false);
    }

    public void SpawnBoss(BossData bossData, System.Action<BossController> onSpawned = null)
    {
        StartCoroutine(SpawnBossRoutine(bossData, onSpawned));
    }

    private IEnumerator SpawnBossRoutine(BossData bossData, System.Action<BossController> onSpawned)
    {
        if (bossData == null)
        {
            Debug.LogError("[BossSpawnManager] Cannot spawn boss: BossData is null!");
            yield break;
        }

        // Show warning announcement
        if (bossWarningBanner != null)
        {
            bossWarningBanner.SetActive(true);
        }

        Debug.Log($"[BossSpawnManager] Warning! {bossData.bossName} is appearing!");

        yield return new WaitForSeconds(1.8f);

        if (bossWarningBanner != null)
        {
            bossWarningBanner.SetActive(false);
        }

        // Determine spawn position relative to player
        Vector3 spawnPos = Vector3.zero;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");

        if (playerObj != null)
        {
            spawnPos = playerObj.transform.position + Vector3.up * spawnDistanceFromPlayer;
        }
        else
        {
            spawnPos = Vector3.up * 5f;
        }

        // Instantiate Boss
        GameObject bossObj;
        if (defaultBossPrefab != null)
        {
            bossObj = Instantiate(defaultBossPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bossObj = CreateDynamicBossGameObject(bossData, spawnPos);
        }

        BossController controller = bossObj.GetComponent<BossController>();
        if (controller == null) controller = bossObj.AddComponent<BossController>();

        controller.Initialize(bossData);

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.ShowBossUI(controller);
        }

        onSpawned?.Invoke(controller);
    }

    private GameObject CreateDynamicBossGameObject(BossData data, Vector3 position)
    {
        GameObject go = new GameObject(data.bossName);
        go.transform.position = position;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (data.defaultSprite != null) sr.sprite = data.defaultSprite;
        sr.sortingOrder = 10;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 1.5f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        go.AddComponent<BossHealth>();
        go.AddComponent<BossAnimationController>();
        go.AddComponent<BossTelegraphController>();
        go.AddComponent<BossController>();

        return go;
    }
}
