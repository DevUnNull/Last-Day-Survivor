using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [Header("EXP Properties")]
    [SerializeField] private int expValue = 15;
    [SerializeField] private float initialFlySpeed = 2f;
    [SerializeField] private float flyAcceleration = 18f;

    private Transform playerTransform;
    private bool isFlyingToPlayer;
    private float currentSpeed;
    private float initialY;
    private float randomOffset;

    private static Sprite cachedGemSprite;

    public void Setup(int exp)
    {
        this.expValue = exp;
    }

    private void Start()
    {
        initialY = transform.position.y;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
        currentSpeed = initialFlySpeed;

        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        float pickupRange = PlayerStats.Instance != null ? PlayerStats.Instance.PickupRange : 3.5f;

        if (!isFlyingToPlayer)
        {
            // Bobbing animation while idling on the ground
            float bobOffset = Mathf.Sin((Time.time * 4f) + randomOffset) * 0.08f;
            transform.position = new Vector3(transform.position.x, initialY + bobOffset, transform.position.z);

            if (distance <= pickupRange)
            {
                isFlyingToPlayer = true;
            }
        }
        else
        {
            // Fly accelerating towards player
            currentSpeed += flyAcceleration * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

            if (distance < 0.45f)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGemPickup();
        }

        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.AddExp(expValue);
        }

        Destroy(gameObject);
    }

    public static ExpGem Spawn(Vector3 position, int expAmount = 15)
    {
        GameObject gemObj = new GameObject("ExpGem");
        gemObj.transform.position = position;

        SpriteRenderer sr = gemObj.AddComponent<SpriteRenderer>();
        if (cachedGemSprite == null)
        {
            cachedGemSprite = Resources.Load<Sprite>("exp_gem");
            if (cachedGemSprite == null)
            {
#if UNITY_EDITOR
                cachedGemSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/exp_gem.png");
#endif
            }
        }

        if (cachedGemSprite != null)
        {
            sr.sprite = cachedGemSprite;
        }
        else
        {
            // Fallback square if sprite not found
            sr.color = new Color(0.2f, 0.9f, 1f, 1f);
        }

        sr.sortingOrder = 5;

        CircleCollider2D col = gemObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        ExpGem gem = gemObj.AddComponent<ExpGem>();
        gem.Setup(expAmount);

        return gem;
    }
}
