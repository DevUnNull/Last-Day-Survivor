using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings (Inspector Configurable)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.3f; // Seconds between shots
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int bulletDamage = 25;
    [SerializeField] private float targetingRange = 10f;

    private float nextFireTime;

    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Transform targetEnemy = FindNearestEnemy();
            if (targetEnemy != null)
            {
                Shoot(targetEnemy);
                nextFireTime = Time.time + fireRate;
            }
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

        Vector2 direction = (target.position - transform.position).normalized;
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(direction, bulletSpeed, bulletDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize targeting range in Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
