using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text tagText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text levelBadgeText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image cardBackgroundImage;

    private UpgradeOption currentOption;
    private LevelUpUI parentUI;

    public void Setup(UpgradeOption option, LevelUpUI ui)
    {
        this.currentOption = option;
        this.parentUI = ui;

        if (titleText != null)
        {
            titleText.text = option.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = option.description;
        }

        if (iconImage != null)
        {
            if (option.icon != null)
            {
                iconImage.sprite = option.icon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.color = Color.clear;
            }
        }

        if (tagText != null)
        {
            tagText.text = option.subtitle;
            switch (option.category)
            {
                case UpgradeCategory.NewSpell:
                    tagText.color = new Color(1f, 0.85f, 0.2f, 1f); // Vibrant gold
                    break;
                case UpgradeCategory.SpellUpgrade:
                    tagText.color = new Color(0.3f, 0.85f, 1f, 1f); // Electric cyan
                    break;
                case UpgradeCategory.PassiveStat:
                    tagText.color = new Color(0.4f, 0.95f, 0.4f, 1f); // Emerald green
                    break;
                case UpgradeCategory.Consumable:
                    tagText.color = new Color(1f, 0.45f, 0.5f, 1f); // Coral pink
                    break;
            }
        }

        if (levelBadgeText != null)
        {
            if (option.currentLevel == 0 && option.category == UpgradeCategory.NewSpell)
            {
                levelBadgeText.text = "NEW!";
                levelBadgeText.color = new Color(1f, 0.9f, 0.3f, 1f);
            }
            else if (option.nextLevel > 0)
            {
                levelBadgeText.text = $"Lv. {option.currentLevel} → Lv. {option.nextLevel}";
                levelBadgeText.color = new Color(0.85f, 0.9f, 1f, 1f);
            }
            else
            {
                levelBadgeText.text = "";
            }
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                if (parentUI != null && currentOption != null)
                {
                    parentUI.OnCardClicked(currentOption);
                }
            });
        }
    }
}
