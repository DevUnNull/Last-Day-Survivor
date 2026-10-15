using System.Collections.Generic;
using UnityEngine;

public class DarkOrbSpell : MonoBehaviour
{
    [Header("Spell Properties")]
    [SerializeField] private int currentLevel = 0; // 0 = not unlocked
    [SerializeField] private int orbCount = 1;
    [SerializeField] private int baseDamage = 25;
    [SerializeField] private float orbitRadius = 1.8f;
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second

    public int CurrentLevel => currentLevel;
    public int MaxLevel => 5;

    private List<GameObject> activeOrbs = new List<GameObject>();
    private float currentAngle = 0f;

    private static Sprite cachedOrbSprite;

    public void UnlockOrUpgrade()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;

        switch (currentLevel)
        {
            case 1:
                orbCount = 1;
                baseDamage = 25;
                orbitRadius = 1.8f;
                rotationSpeed = 180f;
                break;
            case 2:
                orbCount = 2;
                rotationSpeed = 220f;
                break;
            case 3:
                orbCount = 3;
                baseDamage = 35;
                break;
            case 4:
                orbCount = 4;
                orbitRadius = 2.2f;
                break;
            case 5:
                orbCount = 5;
                baseDamage = 55;
                rotationSpeed = 300f;
                break;
        }

        RebuildOrbs();
    }

    private void Update()
    {
        if (currentLevel <= 0) return;

        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        PositionOrbs();
    }

    private void RebuildOrbs()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        activeOrbs.Clear();

        if (currentLevel <= 0) return;

        if (cachedOrbSprite == null)
        {
#if UNITY_EDITOR
            cachedOrbSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/dark_orb.png");
#endif
        }

        for (int i = 0; i < orbCount; i++)
        {
            GameObject orbObj = new GameObject($"DarkOrb_{i}");
            orbObj.transform.SetParent(transform);

            SpriteRenderer sr = orbObj.AddComponent<SpriteRenderer>();
            if (cachedOrbSprite != null) sr.sprite = cachedOrbSprite;
            else sr.color = new Color(0.8f, 0.2f, 1f, 1f);

            sr.sortingOrder = 8;

            CircleCollider2D col = orbObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            DarkOrbContact contact = orbObj.AddComponent<DarkOrbContact>();
            contact.Setup(this);

            activeOrbs.Add(orbObj);
        }

        PositionOrbs();
    }

    private void PositionOrbs()
    {
        if (activeOrbs.Count == 0) return;

        float angleStep = 360f / activeOrbs.Count;
        for (int i = 0; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i] == null) continue;

            float angle = (currentAngle + i * angleStep) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
            activeOrbs[i].transform.position = transform.position + offset;
        }
    }

    public int GetCurrentDamage()
    {
        float mod = PlayerStats.Instance != null ? PlayerStats.Instance.MagicDamageMultiplier : 1f;
        return Mathf.RoundToInt(baseDamage * mod);
    }
}

public class DarkOrbContact : MonoBehaviour
{
    private DarkOrbSpell parentSpell;
    private Dictionary<int, float> lastHitTime = new Dictionary<int, float>();
    private const float HitCooldown = 0.4f;

    public void Setup(DarkOrbSpell spell)
    {
        this.parentSpell = spell;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        HandleContact(collider.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleContact(collision.gameObject);
    }

    private void HandleContact(GameObject target)
    {
        if (target.CompareTag("Player") || target.name == "Player") return;

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            int id = target.GetInstanceID();
            if (!lastHitTime.ContainsKey(id) || Time.time >= lastHitTime[id] + HitCooldown)
            {
                lastHitTime[id] = Time.time;
                if (parentSpell != null)
                {
                    enemy.TakeDamage(parentSpell.GetCurrentDamage());
                }
            }
        }
    }
}
