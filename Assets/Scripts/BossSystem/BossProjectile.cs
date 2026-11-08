using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed = 8f;
    private int damage = 20;
    private float lifetime = 5f;
    private BossThemeType bossTheme = BossThemeType.SkyDragon;
    private Rigidbody2D rb;
    private float fxSpawnTimer = 0f;
    private Vector3 baseScale = Vector3.one * 1.15f;

    public void Setup(Vector2 dir, float projSpeed, int dmg, float life, BossThemeType theme = BossThemeType.SkyDragon)
    {
        this.direction = dir.normalized;
        this.speed = projSpeed;
        this.damage = dmg;
        this.lifetime = life;
        this.bossTheme = theme;
        this.baseScale = transform.localScale;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.AttachBossProjectileTrail(gameObject, bossTheme);
        }

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector2 targetPos = rb.position + direction * (speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }

        // Dynamic rotation & pulse so projectile looks energized
        transform.Rotate(0f, 0f, 260f * Time.fixedDeltaTime);
        float pulse = 1f + Mathf.Sin(Time.time * 16f) * 0.12f;
        transform.localScale = baseScale * pulse;

        fxSpawnTimer += Time.fixedDeltaTime;
        if (fxSpawnTimer >= 0.045f)
        {
            fxSpawnTimer = 0f;
            if (SpellFXManager.Instance != null)
            {
                SpellFXManager.Instance.SpawnBossProjectileFlightFX(transform.position, direction, bossTheme);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Hit(collider.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        Hit(collider.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.gameObject);
    }

    private void Hit(GameObject target)
    {
        if (target == null) return;

        // Ignore boss, other boss projectiles, and friendly enemies
        if (target.GetComponent<BossController>() != null || target.GetComponentInParent<BossController>() != null) return;
        if (target.GetComponent<BossProjectile>() != null) return;
        if (target.GetComponent<EnemyController>() != null || target.GetComponentInParent<EnemyController>() != null) return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = target.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyAttack();
            }

            if (SpellFXManager.Instance != null)
            {
                SpellFXManager.Instance.SpawnBossProjectileHitFX(transform.position, direction, bossTheme);
            }

            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
