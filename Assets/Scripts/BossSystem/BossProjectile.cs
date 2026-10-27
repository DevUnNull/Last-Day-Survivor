using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed = 8f;
    private int damage = 20;
    private float lifetime = 5f;
    private Rigidbody2D rb;

    public void Setup(Vector2 dir, float projSpeed, int dmg, float life)
    {
        this.direction = dir.normalized;
        this.speed = projSpeed;
        this.damage = dmg;
        this.lifetime = life;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Hit(collider.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.gameObject);
    }

    private void Hit(GameObject target)
    {
        if (target.CompareTag("Player") || target.name == "Player")
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
