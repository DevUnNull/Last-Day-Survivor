using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings (Inspector Configurable)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float baseFireRate = 0.35f; // Seconds between shots
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int baseBulletDamage = 25;
    [SerializeField] private float targetingRange = 10f;

    [Header("Spell Progression")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private int pierceCount = 0;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => 5;

    private float nextFireTime;

    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Transform targetEnemy = FindNearestEnemy();
            if (targetEnemy != null)
            {
                Shoot(targetEnemy);
                float cooldownMod = PlayerStats.Instance != null ? PlayerStats.Instance.CooldownMultiplier : 1f;
                nextFireTime = Time.time + (baseFireRate * cooldownMod);
            }
        }
    }

    public void UpgradeMagicBolt()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;

        switch (currentLevel)
        {
            case 2:
                projectileCount = 2;
                break;
            case 3:
                baseBulletDamage = 32;
                baseFireRate = 0.28f;
                break;
            case 4:
                projectileCount = 3;
                break;
            case 5:
                pierceCount = 2;
                baseBulletDamage = 45;
                baseFireRate = 0.22f;
                break;
        }
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

    private void Shoot(Transform target)
    {
        if (bulletPrefab == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBoltCast();
        }

        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float damageMod = PlayerStats.Instance != null ? PlayerStats.Instance.MagicDamageMultiplier : 1f;
        int finalDamage = Mathf.RoundToInt(baseBulletDamage * damageMod);

        if (projectileCount == 1)
        {
            FireSingleBullet(baseDir, finalDamage);
        }
        else if (projectileCount == 2)
        {
            // Fire 2 bullets spread by -8 and +8 degrees
            FireSingleBullet(RotateVector(baseDir, -8f), finalDamage);
            FireSingleBullet(RotateVector(baseDir, 8f), finalDamage);
        }
        else if (projectileCount >= 3)
        {
            // Fire 3 bullets spread: -15, 0, +15 degrees
            FireSingleBullet(RotateVector(baseDir, -15f), finalDamage);
            FireSingleBullet(baseDir, finalDamage);
            FireSingleBullet(RotateVector(baseDir, 15f), finalDamage);
        }
    }

    private void FireSingleBullet(Vector2 dir, int damage)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(dir, bulletSpeed, damage, pierceCount);
        }
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
