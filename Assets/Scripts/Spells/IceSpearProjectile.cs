using System.Collections.Generic;
using UnityEngine;

public class IceSpearProjectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed = 18f;
    private int damage = 32;
    private float slowFactor = 0.6f; // 40% slow
    private float slowDuration = 2.5f;
    private float lifeTime = 2.5f;

    private Rigidbody2D rb;
    private HashSet<int> hitEnemyIds = new HashSet<int>();

    private static Sprite cachedIceSprite;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Setup(Vector2 direction, float projSpeed, int dmg, float slow, float duration)
    {
        this.moveDirection = direction.normalized;
        this.speed = projSpeed;
        this.damage = dmg;
        this.slowFactor = slow;
        this.slowDuration = duration;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
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
        HandleHit(collider.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject target)
    {
        if (target.CompareTag("Player") || target.name == "Player") return;

        int id = target.GetInstanceID();
        if (hitEnemyIds.Contains(id)) return;
        hitEnemyIds.Add(id);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayIceSpearPierce();
        }

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        EnemyController enemyCtrl = target.GetComponent<EnemyController>();
        if (enemyCtrl != null)
        {
            enemyCtrl.ApplySlow(slowFactor, slowDuration);
        }
    }

    public static IceSpearProjectile Create(Vector3 pos)
    {
        GameObject go = new GameObject("IceSpear");
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (cachedIceSprite == null)
        {
#if UNITY_EDITOR
            cachedIceSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/ice_spear.png");
#endif
        }
        if (cachedIceSprite != null) sr.sprite = cachedIceSprite;
        else sr.color = Color.cyan;

        sr.sortingOrder = 7;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 0.4f);

        return go.AddComponent<IceSpearProjectile>();
    }
}
