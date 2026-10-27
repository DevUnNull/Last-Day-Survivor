using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapCardUI : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI mapDescText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Boss Preview")]
    [SerializeField] private Image bossIconImage;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Lock Status Overlay")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TextMeshProUGUI lockReasonText;

    [Header("Selection Highlight")]
    [SerializeField] private GameObject highlightBorder;
    [SerializeField] private Image selectionGlow;

    [Header("Button Component")]
    [SerializeField] private Button selectButton;

    private MapData mapData;
    private bool isUnlocked = true;
    private System.Action<MapCardUI> onCardClicked;

    public MapData Data => mapData;
    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (selectButton == null) selectButton = GetComponent<Button>();
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => onCardClicked?.Invoke(this));
        }
    }

    public void Setup(MapData data, bool unlocked, System.Action<MapCardUI> onClickCallback)
    {
        this.mapData = data;
        this.isUnlocked = unlocked;
        this.onCardClicked = onClickCallback;

        if (mapData == null) return;

        if (mapNameText != null) mapNameText.text = mapData.mapName.ToUpper();
        if (mapDescText != null) mapDescText.text = mapData.description;
        if (difficultyText != null) difficultyText.text = mapData.difficulty;

        if (mapPreviewImage != null && mapData.previewSprite != null)
        {
            mapPreviewImage.sprite = mapData.previewSprite;
        }

        // Boss preview
        if (mapData.bossData != null)
        {
            if (bossNameText != null) bossNameText.text = $"BOSS: {mapData.bossData.bossName}";
            if (bossIconImage != null && mapData.bossData.bossIcon != null)
            {
                bossIconImage.sprite = mapData.bossData.bossIcon;
                bossIconImage.gameObject.SetActive(true);
            }
        }

        // Lock status
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked);
            if (!isUnlocked && lockReasonText != null)
            {
                lockReasonText.text = "LOCKED\nDefeat previous boss";
            }
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (highlightBorder != null) highlightBorder.SetActive(isSelected);
        if (selectionGlow != null) selectionGlow.enabled = isSelected;

        // Subtle scale feedback
        transform.localScale = isSelected ? Vector3.one * 1.04f : Vector3.one;
    }
}
