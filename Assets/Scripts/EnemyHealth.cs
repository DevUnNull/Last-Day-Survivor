using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 50;
    [SerializeField] protected int currentHealth;

    [Header("EXP Drop")]
    [SerializeField] protected GameObject expGemPrefab;
    [SerializeField] protected int expDropValue = 10;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int, int> onHealthChanged;
    public System.Action onDied;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetHealthStats(int newMaxHealth, int expReward = -1)
    {
        this.maxHealth = newMaxHealth;
        this.currentHealth = newMaxHealth;
        if (expReward > 0)
        {
            this.expDropValue = expReward;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        onDied?.Invoke();

        if (expGemPrefab != null)
        {
            Instantiate(expGemPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            ExpGem.Spawn(transform.position, expDropValue);
        }

        Destroy(gameObject);
    }
}
