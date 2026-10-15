using UnityEngine;
using UnityEngine.UI;

public class ExpBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image expFillImage;
    [SerializeField] private Text levelText;
    [SerializeField] private Text expProgressText;

    [Header("Animation Settings")]
    [SerializeField] private float fillLerpSpeed = 10f;

    private float targetFillRatio = 0f;
    private float currentFillRatio = 0f;

    private void Start()
    {
        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.OnExpChanged += HandleExpChanged;
            HandleExpChanged(PlayerLevelSystem.Instance.CurrentExp, PlayerLevelSystem.Instance.ExpToNextLevel, PlayerLevelSystem.Instance.CurrentLevel);
        }
        else
        {
            // Try to find in scene
            PlayerLevelSystem pls = FindFirstObjectByType<PlayerLevelSystem>();
            if (pls != null)
            {
                pls.OnExpChanged += HandleExpChanged;
                HandleExpChanged(pls.CurrentExp, pls.ExpToNextLevel, pls.CurrentLevel);
            }
        }
    }

    private void OnDestroy()
    {
        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.OnExpChanged -= HandleExpChanged;
        }
    }

    private void HandleExpChanged(int currentExp, int maxExp, int level)
    {
        if (maxExp > 0)
        {
            targetFillRatio = Mathf.Clamp01((float)currentExp / maxExp);
        }
        else
        {
            targetFillRatio = 0f;
        }

        if (levelText != null)
        {
            levelText.text = $"LV. {level}";
        }

        if (expProgressText != null)
        {
            int percent = Mathf.RoundToInt(targetFillRatio * 100f);
            expProgressText.text = $"{currentExp} / {maxExp} EXP ({percent}%)";
        }
    }

    private void Update()
    {
        currentFillRatio = Mathf.Lerp(currentFillRatio, targetFillRatio, Time.unscaledDeltaTime * fillLerpSpeed);

        if (expFillImage != null)
        {
            expFillImage.fillAmount = currentFillRatio;

            // Dynamically adjust anchors so it fills correctly even without a 9-sliced sprite
            RectTransform fillRect = expFillImage.rectTransform;
            if (fillRect != null)
            {
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(currentFillRatio, 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }
        }
    }
}
