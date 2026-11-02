using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossFloatingHealthBar : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.6f, 0f);

    [Header("UI Elements")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI hpNumbersText;

    private BossHealth bossHealth;
    private BossController bossController;
    private float targetFill = 1f;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
        bossController = GetComponent<BossController>();

        if (canvas == null)
        {
            CreateWorldSpaceBar();
        }
    }

    private void Start()
    {
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged += OnHealthChanged;
            UpdateHealthDisplay(bossHealth.CurrentHealth, bossHealth.MaxHealth);
        }

        if (bossController != null && bossController.BossConfig != null && bossNameText != null)
        {
            bossNameText.text = bossController.BossConfig.bossName;
        }
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetFill, Time.deltaTime * 12f);
        }

        // Keep canvas facing camera without rotating with boss flips
        if (canvas != null)
        {
            canvas.transform.rotation = Quaternion.identity;
            float pScale = transform.lossyScale.y != 0 ? Mathf.Abs(transform.lossyScale.y) : 1f;
            canvas.transform.localPosition = new Vector3(0f, 1.55f / pScale, 0f);
            canvas.transform.localScale = Vector3.one * (0.015f / pScale);
        }
    }

    public void OnHealthChanged(int currentHp, int maxHp)
    {
        UpdateHealthDisplay(currentHp, maxHp);
    }

    public void UpdateHealthDisplay(int currentHp, int maxHp)
    {
        targetFill = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;

        if (hpNumbersText != null)
        {
            hpNumbersText.text = $"{currentHp:N0} / {maxHp:N0}";
        }

        if (currentHp <= 0 && canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    private void CreateWorldSpaceBar()
    {
        // 1. World Canvas
        GameObject canvasObj = new GameObject("BossWorldHealthCanvas");
        canvasObj.transform.SetParent(transform);
        float pScale = transform.lossyScale.y != 0 ? Mathf.Abs(transform.lossyScale.y) : 1f;
        canvasObj.transform.localPosition = new Vector3(0f, 1.55f / pScale, 0f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * (0.015f / pScale);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(160f, 40f);

        // 2. Boss Name (Top of bar)
        GameObject nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(canvasObj.transform, false);
        bossNameText = nameObj.AddComponent<TextMeshProUGUI>();
        bossNameText.fontSize = 14;
        bossNameText.fontStyle = FontStyles.Bold;
        bossNameText.alignment = TextAlignmentOptions.Center;
        bossNameText.color = new Color(1f, 0.85f, 0.3f, 1f); // Golden yellow
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(1f, 1f);
        nameRt.offsetMin = new Vector2(0f, 0f);
        nameRt.offsetMax = new Vector2(0f, 10f);

        string defaultName = "TRÙM";
        if (bossController != null && bossController.BossConfig != null)
        {
            defaultName = bossController.BossConfig.bossName;
        }
        bossNameText.text = defaultName;

        // 3. Slider Container
        GameObject sliderObj = new GameObject("HpSlider");
        sliderObj.transform.SetParent(canvasObj.transform, false);
        healthSlider = sliderObj.AddComponent<Slider>();
        healthSlider.interactable = false;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;

        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 0f);
        sliderRt.anchorMax = new Vector2(1f, 0.55f);
        sliderRt.offsetMin = new Vector2(4f, 2f);
        sliderRt.offsetMax = new Vector2(-4f, -2f);

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = Vector2.zero;
        fillAreaRt.offsetMax = Vector2.zero;

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.92f, 0.2f, 0.2f, 1f); // Vibrant crimson
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        healthSlider.targetGraphic = fillImage;
        healthSlider.fillRect = fillRt;

        // 4. HP Text on Bar
        GameObject textObj = new GameObject("HpNumbersText");
        textObj.transform.SetParent(sliderObj.transform, false);
        hpNumbersText = textObj.AddComponent<TextMeshProUGUI>();
        hpNumbersText.fontSize = 11;
        hpNumbersText.fontStyle = FontStyles.Bold;
        hpNumbersText.alignment = TextAlignmentOptions.Center;
        hpNumbersText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        if (bossHealth != null)
        {
            hpNumbersText.text = $"{bossHealth.CurrentHealth:N0} / {bossHealth.MaxHealth:N0}";
        }
    }
}

