using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapSelectionController : MonoBehaviour
{
    [Header("Map Configurations (Data-Driven)")]
    [SerializeField] private List<MapData> availableMaps = new List<MapData>();

    [Header("UI References")]
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private MapCardUI[] mapCards;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    [Header("Active Selection Display")]
    [SerializeField] private TextMeshProUGUI selectedMapTitleText;
    [SerializeField] private TextMeshProUGUI selectedMapDescriptionText;

    private MapData currentSelectedMap;

    private void Awake()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void Start()
    {
        RefreshMapSelection();
    }

    public void OpenMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(true);
        }
        RefreshMapSelection();
    }

    public void CloseMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }
    }

    public void RefreshMapSelection()
    {
        if (availableMaps == null || availableMaps.Count == 0) return;

        MapData defaultSelect = null;

        for (int i = 0; i < mapCards.Length; i++)
        {
            if (i < availableMaps.Count)
            {
                MapData data = availableMaps[i];
                mapCards[i].gameObject.SetActive(true);

                bool unlocked = MapProgressionManager.IsMapUnlocked(data);
                mapCards[i].Setup(data, unlocked, OnCardSelected);

                if (unlocked && defaultSelect == null)
                {
                    defaultSelect = data;
                }
            }
            else
            {
                mapCards[i].gameObject.SetActive(false);
            }
        }

        // If nothing selected or previous selection locked, pick first unlocked
        if (currentSelectedMap == null || !MapProgressionManager.IsMapUnlocked(currentSelectedMap))
        {
            SelectMap(defaultSelect ?? availableMaps[0]);
        }
        else
        {
            SelectMap(currentSelectedMap);
        }
    }

    private void OnCardSelected(MapCardUI card)
    {
        if (card != null && card.Data != null)
        {
            SelectMap(card.Data);
        }
    }

    public void SelectMap(MapData map)
    {
        currentSelectedMap = map;

        bool isUnlocked = MapProgressionManager.IsMapUnlocked(map);

        // Highlight the selected card and unhighlight others
        foreach (var card in mapCards)
        {
            if (card != null && card.gameObject.activeSelf)
            {
                card.SetSelected(card.Data == map);
            }
        }

        // Update details display
        if (selectedMapTitleText != null)
        {
            selectedMapTitleText.text = map.mapName.ToUpper();
        }

        if (selectedMapDescriptionText != null)
        {
            selectedMapDescriptionText.text = map.description;
        }

        // Start button interactability
        if (startGameButton != null)
        {
            startGameButton.interactable = isUnlocked;
        }
    }

    private void OnStartGameClicked()
    {
        if (currentSelectedMap == null) return;
        if (!MapProgressionManager.IsMapUnlocked(currentSelectedMap))
        {
            Debug.LogWarning("[MapSelection] Selected map is currently locked!");
            return;
        }

        // Store selected map in session
        GameplaySessionManager.SelectedMap = currentSelectedMap;

        // Load scene
        string targetScene = !string.IsNullOrEmpty(currentSelectedMap.sceneName) ? currentSelectedMap.sceneName : "2_InGame";
        Debug.Log($"[MapSelection] Launching map '{currentSelectedMap.mapName}' -> loading scene '{targetScene}'");
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene);
    }

    private void OnBackClicked()
    {
        CloseMapSelection();
    }
}
