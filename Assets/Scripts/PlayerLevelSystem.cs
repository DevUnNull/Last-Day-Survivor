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

    public int CalculateRequiredExp(int level)
    {
        // Smooth exponential-linear roguelite scaling
        // Lv 1->2: 25, Lv 2->3: 45, Lv 3->4: 70, Lv 4->5: 100, etc.
        return 15 + level * 10 + Mathf.RoundToInt(Mathf.Pow(level, 1.35f) * 8f);
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
