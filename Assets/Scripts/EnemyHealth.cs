using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;

    [Header("EXP Drop")]
    [SerializeField] private GameObject expGemPrefab;
    [SerializeField] private int expDropValue = 10;

    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
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
