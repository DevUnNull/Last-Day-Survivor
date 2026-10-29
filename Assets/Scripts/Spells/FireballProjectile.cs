using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed = 12f;
    private int damage = 45;
    private float explosionRadius = 2.0f;
    private float lifeTime = 3f;

    private Rigidbody2D rb;
    private bool hasExploded = false;

    private static Sprite cachedFireSprite;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Setup(Vector2 direction, float projSpeed, int dmg, float radius)
    {
        this.moveDirection = direction.normalized;
        this.speed = projSpeed;
        this.damage = dmg;
        this.explosionRadius = radius;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (hasExploded) return;
        if (rb != null)
        {
            Vector2 targetPos = rb.position + moveDirection * (speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") || collider.name == "Player") return;
        if (collider.GetComponent<EnemyHealth>() != null)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player") return;
        if (collision.gameObject.GetComponent<EnemyHealth>() != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFireballExplode();
        }

        // Damage all enemies in explosion radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var col in hits)
        {
            if (col == null) continue;
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        SpawnExplosionVisual();
        Destroy(gameObject);
    }

    private void SpawnExplosionVisual()
    {
        GameObject expObj = new GameObject("FireballExplosion");
        expObj.transform.position = transform.position;
        expObj.transform.localScale = Vector3.one * (explosionRadius * 1.5f);

        SpriteRenderer sr = expObj.AddComponent<SpriteRenderer>();
        if (cachedFireSprite == null)
        {
#if UNITY_EDITOR
            cachedFireSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/fireball.png");
#endif
        }
        if (cachedFireSprite != null)
        {
            sr.sprite = cachedFireSprite;
        }
        sr.color = new Color(1f, 0.45f, 0.1f, 0.85f);
        sr.sortingOrder = 8;

        // Quick fade out
        expObj.AddComponent<ExplosionEffectHelper>();
    }

    public static FireballProjectile Create(Vector3 pos)
    {
        GameObject go = new GameObject("Fireball");
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (cachedFireSprite == null)
        {
#if UNITY_EDITOR
            cachedFireSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/fireball.png");
#endif
        }
        if (cachedFireSprite != null) sr.sprite = cachedFireSprite;
        else sr.color = Color.red;

        sr.sortingOrder = 7;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        return go.AddComponent<FireballProjectile>();
    }
}

public class ExplosionEffectHelper : MonoBehaviour
{
    private float timer = 0f;
    private const float Duration = 0.22f;
    private SpriteRenderer sr;
    private Vector3 initialScale;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / Duration;
        transform.localScale = initialScale * (1f + progress * 0.4f);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.85f, 0f, progress);
            sr.color = c;
        }

        if (timer >= Duration)
        {
            Destroy(gameObject);
        }
    }
}
