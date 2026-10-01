using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private bool invertFlipX = true; // Set true if default sprite faces Left

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float lastDamageTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
        }
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy) return;

        // Move towards Player
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 targetPos = rb.position + direction * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPos);

        // Flip sprite left/right based on target direction
        if (spriteRenderer != null)
        {
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = invertFlipX; // Right movement
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = !invertFlipX; // Left movement
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        TryDamagePlayer(collider.gameObject);
    }

    private void TryDamagePlayer(GameObject target)
    {
        if (Time.time < lastDamageTime + damageInterval) return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }
}
