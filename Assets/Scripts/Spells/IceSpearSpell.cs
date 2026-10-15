using UnityEngine;

public class IceSpearSpell : MonoBehaviour
{
    [Header("Spell Properties")]
    [SerializeField] private int currentLevel = 0; // 0 = not unlocked
    [SerializeField] private float baseCooldown = 2.4f;
    [SerializeField] private int baseDamage = 32;
    [SerializeField] private float slowFactor = 0.6f; // 40% slow
    [SerializeField] private float slowDuration = 2.5f;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float targetingRange = 12f;
    [SerializeField] private int spearCount = 1;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => 5;

    private float nextCastTime;

    public void UnlockOrUpgrade()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;

        switch (currentLevel)
        {
            case 1:
                baseCooldown = 2.4f;
                baseDamage = 32;
                slowFactor = 0.6f;
                slowDuration = 2.5f;
                spearCount = 1;
                break;
            case 2:
                baseDamage = 48;
                baseCooldown = 1.9f;
                break;
            case 3:
                spearCount = 2;
                break;
            case 4:
                slowFactor = 0.4f; // 60% slow
                slowDuration = 3.5f;
                baseDamage = 65;
                break;
            case 5:
                spearCount = 3;
                baseDamage = 90;
                slowFactor = 0.1f; // 90% freeze
                slowDuration = 1.2f;
                baseCooldown = 1.4f;
                break;
        }
    }

    private void Update()
    {
        if (currentLevel <= 0) return;

        if (Time.time >= nextCastTime)
        {
            CastIceSpear();
            float cdMod = PlayerStats.Instance != null ? PlayerStats.Instance.CooldownMultiplier : 1f;
            nextCastTime = Time.time + (baseCooldown * cdMod);
        }
    }

    private void CastIceSpear()
    {
        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float dmgMod = PlayerStats.Instance != null ? PlayerStats.Instance.MagicDamageMultiplier : 1f;
        int finalDamage = Mathf.RoundToInt(baseDamage * dmgMod);

        if (spearCount == 1)
        {
            SpawnSpear(baseDir, finalDamage);
        }
        else if (spearCount == 2)
        {
            SpawnSpear(RotateVector(baseDir, -10f), finalDamage);
            SpawnSpear(RotateVector(baseDir, 10f), finalDamage);
        }
        else if (spearCount >= 3)
        {
            SpawnSpear(RotateVector(baseDir, -16f), finalDamage);
            SpawnSpear(baseDir, finalDamage);
            SpawnSpear(RotateVector(baseDir, 16f), finalDamage);
        }
    }

    private void SpawnSpear(Vector2 dir, int dmg)
    {
        IceSpearProjectile p = IceSpearProjectile.Create(transform.position);
        p.Setup(dir, projectileSpeed, dmg, slowFactor, slowDuration);
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y).normalized;
    }

    private Transform FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        Transform nearest = null;
        float shortestDistance = targetingRange;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= shortestDistance)
            {
                shortestDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}
