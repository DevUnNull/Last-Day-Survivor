using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float lifeTime = 3f;

    private Vector2 moveDirection;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private int pierceCount = 0;
    private int hitCount = 0;

    public void Setup(Vector2 direction, float bulletSpeed, int bulletDamage, int pierce = 0)
    {
        this.moveDirection = direction.normalized;
        this.speed = bulletSpeed;
        this.damage = bulletDamage;
        this.pierceCount = pierce;
        this.hitCount = 0;

        // Rotate bullet sprite to face movement direction
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector2 targetPos = rb.position + moveDirection * (speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        HitTarget(collider.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HitTarget(collision.gameObject);
    }

    private void HitTarget(GameObject target)
    {
        if (target.CompareTag("Player") || target.name == "Player") return;

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBoltImpact();
            }

            enemyHealth.TakeDamage(damage);
            hitCount++;
            if (hitCount > pierceCount)
            {
                Destroy(gameObject);
            }
        }
    }
}
