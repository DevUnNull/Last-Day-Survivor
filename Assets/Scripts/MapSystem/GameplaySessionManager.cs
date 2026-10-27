using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplaySessionManager : MonoBehaviour
{
    public static GameplaySessionManager Instance { get; private set; }

    [Header("Active Map Session")]
    public static MapData SelectedMap { get; set; }
    [SerializeField] private MapData fallbackDefaultMap;

    [Header("Environment References")]
    [SerializeField] private Image[] backgroundImages; // Background variants in scene
    [SerializeField] private SpriteRenderer[] environmentRenderers;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI survivalTimerText;
    [SerializeField] private TextMeshProUGUI mapNameHudText;

    [Header("Runtime State")]
    private float currentSurvivalTime = 0f;
    private float targetSurvivalTime = 600f;
    private bool isBossSpawned = false;
    private bool isGameEnded = false;
    private int enemiesKilled = 0;

    public float CurrentSurvivalTime => currentSurvivalTime;
    public float TargetSurvivalTime => targetSurvivalTime;
    public int EnemiesKilled => enemiesKilled;
    public bool IsBossPhase => isBossSpawned;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Fallback if launched directly into 2_InGame
        if (SelectedMap == null && fallbackDefaultMap != null)
        {
            SelectedMap = fallbackDefaultMap;
        }
    }

    private void Start()
    {
        InitializeSession();
    }

    private void InitializeSession()
    {
        if (SelectedMap != null)
        {
            targetSurvivalTime = SelectedMap.survivalDuration;

            if (mapNameHudText != null)
            {
                mapNameHudText.text = SelectedMap.mapName.ToUpper();
            }

            ApplyEnvironmentVisuals(SelectedMap);
            ConfigureEnemySpawner(SelectedMap);
        }
        else
        {
            targetSurvivalTime = 300f;
        }

        // Hook player death
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.onPlayerDied.AddListener(OnPlayerDied);
            }
        }
    }

    private void ApplyEnvironmentVisuals(MapData map)
    {
        // 1. If backgroundImages provided in Canvas, activate the matching one or set sprite
        if (backgroundImages != null && backgroundImages.Length > 0)
        {
            // Activate background according to map rating (1, 2, or 3)
            int index = Mathf.Clamp(map.difficultyRating - 1, 0, backgroundImages.Length - 1);
            for (int i = 0; i < backgroundImages.Length; i++)
            {
                if (backgroundImages[i] != null)
                {
                    backgroundImages[i].gameObject.SetActive(i == index);
                    if (i == index && map.backgroundSprite != null)
                    {
                        backgroundImages[i].sprite = map.backgroundSprite;
                        backgroundImages[i].color = map.backgroundColorTint;
                    }
                }
            }
        }

        // 2. Adjust Camera or 2D Light color
        UnityEngine.Rendering.Universal.Light2D light2d = FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>();
        if (light2d != null)
        {
            light2d.color = map.ambientColor;
        }
    }

    private void ConfigureEnemySpawner(MapData map)
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            // We can scale spawner settings via reflection or serialized object if needed
        }
    }

    private void Update()
    {
        if (isGameEnded) return;

        // Debug hotkey for testing: press B to skip immediately to Boss!
        if (Input.GetKeyDown(KeyCode.B))
        {
            SkipToBoss();
        }

        if (!isBossSpawned)
        {
            currentSurvivalTime += Time.deltaTime;
            UpdateTimerUI();

            if (currentSurvivalTime >= targetSurvivalTime)
            {
                TriggerBossEncounter();
            }
        }
        else
        {
            currentSurvivalTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (survivalTimerText != null)
        {
            int curMinutes = Mathf.FloorToInt(currentSurvivalTime / 60f);
            int curSeconds = Mathf.FloorToInt(currentSurvivalTime % 60f);

            if (!isBossSpawned)
            {
                int maxMinutes = Mathf.FloorToInt(targetSurvivalTime / 60f);
                int maxSeconds = Mathf.FloorToInt(targetSurvivalTime % 60f);
                survivalTimerText.text = $"{curMinutes:00}:{curSeconds:00} / {maxMinutes:00}:{maxSeconds:00}";
            }
            else
            {
                survivalTimerText.text = $"BOSS FIGHT • {curMinutes:00}:{curSeconds:00}";
            }
        }
    }

    public void SkipToBoss()
    {
        if (isBossSpawned || isGameEnded) return;
        Debug.Log("[GameplaySessionManager] Debug Skip: Triggering Boss Spawn immediately!");
        currentSurvivalTime = targetSurvivalTime;
        TriggerBossEncounter();
    }

    private void TriggerBossEncounter()
    {
        if (isBossSpawned) return;
        isBossSpawned = true;

        // Pause standard enemy spawning
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.enabled = false;
        }

        BossData bossToSpawn = SelectedMap != null && SelectedMap.bossData != null ? SelectedMap.bossData : null;
        if (bossToSpawn != null)
        {
            if (BossSpawnManager.Instance != null)
            {
                BossSpawnManager.Instance.SpawnBoss(bossToSpawn);
            }
        }
        else
        {
            Debug.LogWarning("[GameplaySessionManager] No BossData assigned to active MapData!");
        }
    }

    public void RecordEnemyKill()
    {
        enemiesKilled++;
    }

    public void OnBossDefeated(BossData boss)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.HideBossUI();
        }

        if (SelectedMap != null)
        {
            MapProgressionManager.CompleteMap(SelectedMap.mapId, currentSurvivalTime);
        }

        int goldEarned = (boss != null ? boss.goldReward : 1000) + (enemiesKilled * 5);

        if (GameResultController.Instance != null)
        {
            GameResultController.Instance.ShowVictory(SelectedMap, currentSurvivalTime, enemiesKilled, goldEarned);
        }
        else
        {
            Debug.Log($"[GameplaySessionManager] VICTORY! Boss defeated in {currentSurvivalTime:F1}s!");
        }
    }

    private void OnPlayerDied()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.HideBossUI();
        }

        if (GameResultController.Instance != null)
        {
            GameResultController.Instance.ShowDefeat(SelectedMap, currentSurvivalTime, enemiesKilled);
        }
    }
}
