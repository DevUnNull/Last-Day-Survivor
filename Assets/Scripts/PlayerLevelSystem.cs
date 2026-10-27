using System;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }

    [Header("Progression Data")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 25;
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
        // Roguelite steep progression curve:
        // Early levels are accessible, but mid-to-high levels become exponentially harder.
        // Lv 1: 40 EXP (needs 4 gems)
        // Lv 2: 110 EXP (needs 11 gems)
        // Lv 3: 200 EXP (needs 20 gems)
        // Lv 5: 450 EXP
        // Lv 10: ~1,500 EXP
        // Lv 15: ~3,400 EXP
        // Lv 20: ~6,200 EXP
        if (level <= 1) return 40;

        float baseRequirement = 40f;
        float linearGrowth = level * 25f;
        float exponentialGrowth = Mathf.Pow(level, 1.85f) * 18f;

        // Tier jumps after Level 10 and Level 20
        float tierBonus = 0f;
        if (level > 20)
        {
            tierBonus = (level - 20) * 120f + 10 * 50f;
        }
        else if (level > 10)
        {
            tierBonus = (level - 10) * 50f;
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

        if (leveledUp && UpgradeManager.Instance != null && !UpgradeManager.Instance.IsActive)
        {
            UpgradeManager.Instance.ShowLevelUpSelection();
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
