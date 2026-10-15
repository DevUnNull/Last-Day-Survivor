using UnityEngine;

public class MagicShield : MonoBehaviour
{
    [Header("Shield Properties")]
    [SerializeField] private int currentLevel = 0; // 0 = not unlocked
    [SerializeField] private int maxShield = 35;
    [SerializeField] private int currentShield = 0;
    [SerializeField] private float regenDelay = 7f;
    [SerializeField] private float damageReductionPercent = 0f;

    public int CurrentShield => currentShield;
    public int MaxShield => maxShield;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => 5;

    private float lastDamageTime;
    private GameObject shieldVisual;
    private SpriteRenderer shieldSr;

    private static Sprite cachedShieldSprite;

    public void UnlockOrUpgrade()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;

        switch (currentLevel)
        {
            case 1:
                maxShield = 35;
                regenDelay = 7f;
                damageReductionPercent = 0f;
                break;
            case 2:
                maxShield = 60;
                regenDelay = 6f;
                break;
            case 3:
                maxShield = 90;
                break;
            case 4:
                maxShield = 125;
                regenDelay = 5f;
                break;
            case 5:
                maxShield = 170;
                regenDelay = 4f;
                damageReductionPercent = 0.15f;
                break;
        }

        currentShield = maxShield;
        UpdateVisual();
    }

    private void Start()
    {
        CreateVisual();
        UpdateVisual();
    }

    private void Update()
    {
        if (currentLevel <= 0) return;

        // Regenerate shield if cooldown elapsed
        if (currentShield < maxShield && Time.time >= lastDamageTime + regenDelay)
        {
            currentShield = maxShield;
            UpdateVisual();
        }

        // Gentle pulse on visual
        if (shieldVisual != null && shieldVisual.activeSelf)
        {
            float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
            shieldVisual.transform.localScale = Vector3.one * (1.8f * scale);
        }
    }

    public int AbsorbDamage(int incomingDamage)
    {
        lastDamageTime = Time.time;

        if (damageReductionPercent > 0f)
        {
            incomingDamage = Mathf.RoundToInt(incomingDamage * (1f - damageReductionPercent));
        }

        if (currentShield >= incomingDamage)
        {
            currentShield -= incomingDamage;
            UpdateVisual();
            return 0; // Completely absorbed!
        }
        else
        {
            int remainingDamage = incomingDamage - currentShield;
            currentShield = 0;
            UpdateVisual();

            if (currentLevel >= 3)
            {
                TriggerShieldBreakBlast();
            }

            return remainingDamage;
        }
    }

    private void TriggerShieldBreakBlast()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 4.0f);
        foreach (var col in enemies)
        {
            if (col == null || col.CompareTag("Player") || col.name == "Player") continue;

            EnemyHealth eh = col.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(40);

            Rigidbody2D erb = col.GetComponent<Rigidbody2D>();
            if (erb != null)
            {
                Vector2 pushDir = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                erb.AddForce(pushDir * 10f, ForceMode2D.Impulse);
            }
        }
    }

    private void CreateVisual()
    {
        if (shieldVisual != null) return;

        shieldVisual = new GameObject("ShieldBarrierVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.transform.localScale = Vector3.one * 1.8f;

        shieldSr = shieldVisual.AddComponent<SpriteRenderer>();
        if (cachedShieldSprite == null)
        {
#if UNITY_EDITOR
            cachedShieldSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/magic_shield.png");
#endif
        }
        if (cachedShieldSprite != null) shieldSr.sprite = cachedShieldSprite;
        else shieldSr.color = new Color(0.2f, 0.7f, 1f, 0.4f);

        shieldSr.sortingOrder = 6;
    }

    private void UpdateVisual()
    {
        if (shieldVisual == null) CreateVisual();
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(currentLevel > 0 && currentShield > 0);
        }
    }
}
