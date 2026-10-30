using System;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }

    [Header("Progression Data")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 15;
    [SerializeField] private int pendingLevelUps = 0;

    public int CurrentLevel => currentLevel;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => expToNextLevel;
    public int PendingLevelUps => pendingLevelUps;

    public event Action<int, int, int> OnExpChanged; // (currentExp, maxExp, level)
    public event Action<int> OnLevelUp;             // (newLevel)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        expToNextLevel = CalculateRequiredExp(currentLevel);
        OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);
    }

    public static int CalculateRequiredExp(int level)
    {
        // Smooth early-game progression for fast leveling:
        // Lv 1: 15 EXP (1 early-game kill = instant Level 2 upgrade)
        // Lv 2: 35 EXP (~2 kills -> Level 3)
        // Lv 3: 65 EXP (~3-4 kills -> Level 4)
        // Lv 4: 105 EXP (~5-6 kills -> Level 5)
        // Lv 5: 155 EXP (~7-8 kills -> Level 6)
        if (level <= 1) return 15;
        if (level == 2) return 35;
        if (level == 3) return 65;
        if (level == 4) return 105;
        if (level == 5) return 155;

        // Level 6 and beyond: smooth progressive scaling
        float baseRequirement = 15f;
        float linearGrowth = level * 25f;
        float exponentialGrowth = Mathf.Pow(level, 1.6f) * 6f;

        // Tier jumps after Level 10 and Level 20
        float tierBonus = 0f;
        if (level > 20)
        {
            tierBonus = (level - 20) * 80f + 10 * 30f;
        }
        else if (level > 10)
        {
            tierBonus = (level - 10) * 30f;
        }

        return Mathf.RoundToInt(baseRequirement + linearGrowth + exponentialGrowth + tierBonus);
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;

        bool leveledUp = false;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel = CalculateRequiredExp(currentLevel);
            pendingLevelUps++;
            leveledUp = true;
            OnLevelUp?.Invoke(currentLevel);
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);

        if (leveledUp)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUpJingle();
            }

            if (UpgradeManager.Instance != null && !UpgradeManager.Instance.IsActive)
            {
                UpgradeManager.Instance.ShowLevelUpSelection();
            }
        }
    }

    public void ConsumePendingLevelUp()
    {
        if (pendingLevelUps > 0)
        {
            pendingLevelUps--;
        }
    }
}
