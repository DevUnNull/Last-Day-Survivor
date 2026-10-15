using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onPlayerDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void IncreaseMaxHealth(int amount, bool heal = true)
    {
        maxHealth += amount;
        if (heal)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        // Absorb damage if MagicShield is attached to Player
        MagicShield shield = GetComponent<MagicShield>();
        if (shield != null && shield.CurrentShield > 0)
        {
            damage = shield.AbsorbDamage(damage);
            if (damage <= 0) return;
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onPlayerDied?.Invoke();
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
        gameObject.SetActive(false);
    }
}
