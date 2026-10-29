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
        CheckLowHealthState();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHeal();
        }

        CheckLowHealthState();
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurt();
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            CheckLowHealthState();
        }
    }

    private void CheckLowHealthState()
    {
        if (AudioManager.Instance == null) return;

        bool isLowHealth = currentHealth > 0 && currentHealth <= (maxHealth * 0.3f);
        AudioManager.Instance.SetLowHealthLoop(isLowHealth);
    }

    private void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDie();
            AudioManager.Instance.SetLowHealthLoop(false);
        }

        onPlayerDied?.Invoke();
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetLowHealthLoop(false);
        }
    }
}
