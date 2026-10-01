using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Text healthText;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
            UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void UpdateHealthUI(int currentHp, int maxHp)
    {
        float fillRatio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        fillRatio = Mathf.Clamp01(fillRatio);

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = fillRatio;

            // Dynamically scale RectTransform anchors to guarantee fill ratio works with or without Sprite
            RectTransform fillRect = healthFillImage.rectTransform;
            if (fillRect != null)
            {
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(fillRatio, 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHp} / {maxHp}";
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
        }
    }
}
