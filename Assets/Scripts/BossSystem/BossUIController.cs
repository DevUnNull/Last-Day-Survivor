using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIController : MonoBehaviour
{
    public static BossUIController Instance { get; private set; }

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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (bossUIRoot != null) bossUIRoot.SetActive(false);
    }

    public void ShowBossUI(BossController boss)
    {
        this.currentBoss = boss;
        if (currentBoss == null || currentBoss.BossConfig == null) return;

        if (bossUIRoot != null) bossUIRoot.SetActive(true);

        var config = currentBoss.BossConfig;
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

        UpdatePhase(currentBoss.CurrentPhaseIndex);
        UpdateHealth(config.maxHealth, config.maxHealth);

        // Subscribe to health events
        if (currentBoss.Health != null)
        {
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
            phaseText.text = $"PHASE {phaseIndex + 1}";
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
            // Also adjust anchors to guarantee fill works with or without sliced sprite
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

    private void Update()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * 10f);
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
