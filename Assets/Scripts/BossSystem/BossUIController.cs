using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIController : MonoBehaviour
{
    private static BossUIController _instance;
    public static BossUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BossUIController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("UI Containers")]
    [SerializeField] private GameObject bossUIRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text & Visuals")]
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI bossTitleText;
    [SerializeField] private Image bossIconImage;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Health Bar Elements")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthNumbersText;

    private BossController currentBoss;
    private float targetFill = 1f;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (bossUIRoot != null) bossUIRoot.SetActive(false);
    }

    public void ShowBossUI(BossController boss)
    {
        if (_instance == null) _instance = this;
        this.currentBoss = boss;
        if (currentBoss == null) return;

        if (gameObject != null && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (bossUIRoot != null)
        {
            bossUIRoot.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        var config = currentBoss.BossConfig;
        if (config != null)
        {
            if (bossNameText != null) bossNameText.text = config.bossName;
            if (bossTitleText != null) bossTitleText.text = config.bossTitle;

            if (bossIconImage != null)
            {
                if (config.bossIcon != null)
                {
                    bossIconImage.gameObject.SetActive(true);
                    bossIconImage.sprite = config.bossIcon;
                }
                else
                {
                    bossIconImage.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (bossNameText != null) bossNameText.text = "TRÙM CUỐI";
            if (bossTitleText != null) bossTitleText.text = "Kẻ Thống Trị Vực Thẳm";
        }

        UpdatePhase(currentBoss.CurrentPhaseIndex);

        int curHp = currentBoss.Health != null ? currentBoss.Health.CurrentHealth : (config != null ? config.maxHealth : 2000);
        int maxHp = currentBoss.Health != null ? currentBoss.Health.MaxHealth : (config != null ? config.maxHealth : 2000);
        UpdateHealth(curHp, maxHp);

        // Subscribe to health events
        if (currentBoss.Health != null)
        {
            currentBoss.Health.onHealthChanged -= UpdateHealth;
            currentBoss.Health.onHealthChanged += UpdateHealth;
        }
    }

    public void HideBossUI()
    {
        if (currentBoss != null && currentBoss.Health != null)
        {
            currentBoss.Health.onHealthChanged -= UpdateHealth;
        }

        if (bossUIRoot != null) bossUIRoot.SetActive(false);
        currentBoss = null;
    }

    public void UpdatePhase(int phaseIndex)
    {
        if (phaseText != null)
        {
            phaseText.text = $"GIAI ĐOẠN {phaseIndex + 1}";
        }
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        targetFill = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;

        if (healthNumbersText != null)
        {
            healthNumbersText.text = $"{currentHp:N0} / {maxHp:N0}";
        }

        if (healthFillImage != null)
        {
            if (healthFillImage.type != Image.Type.Filled)
            {
                RectTransform fillRt = healthFillImage.rectTransform;
                if (fillRt != null)
                {
                    fillRt.anchorMin = Vector2.zero;
                    fillRt.anchorMax = new Vector2(targetFill, 1f);
                    fillRt.offsetMin = Vector2.zero;
                    fillRt.offsetMax = Vector2.zero;
                }
            }
        }
    }

    private void Update()
    {
        if (healthFillImage != null)
        {
            if (healthFillImage.type == Image.Type.Filled)
            {
                healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * 10f);
            }
            else
            {
                RectTransform fillRt = healthFillImage.rectTransform;
                if (fillRt != null)
                {
                    Vector2 curMax = fillRt.anchorMax;
                    curMax.x = Mathf.Lerp(curMax.x, targetFill, Time.deltaTime * 10f);
                    fillRt.anchorMax = curMax;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (currentBoss != null && currentBoss.Health != null)
        {
            currentBoss.Health.onHealthChanged -= UpdateHealth;
        }
    }
}
