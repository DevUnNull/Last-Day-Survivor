using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class SettingPanelUI : MonoBehaviour
{
    public static SettingPanelUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button vfxToggleBtn;
    [SerializeField] private Image vfxToggleImage;
    [SerializeField] private TextMeshProUGUI vfxStatusText;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button backgroundBlockerBtn;

    [Header("In-Game Navigation Buttons")]
    [SerializeField] private Button returnToMenuBtn;
    [SerializeField] private Button quitGameBtn;

    [Header("Sprite Icons")]
    [SerializeField] private Sprite toggleOnSprite;
    [SerializeField] private Sprite toggleOffSprite;

    private bool isVfxEnabled = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (panelRoot == null)
        {
            panelRoot = transform.GetChild(0).gameObject;
        }

        // Register button listeners
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(Close);
        }

        if (backgroundBlockerBtn != null)
        {
            backgroundBlockerBtn.onClick.AddListener(Close);
        }

        if (vfxToggleBtn != null)
        {
            vfxToggleBtn.onClick.AddListener(ToggleVFX);
        }

        // Register slider listeners
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        // Register in-game nav buttons if assigned
        if (returnToMenuBtn != null)
        {
            returnToMenuBtn.onClick.AddListener(ReturnToMainMenu);
        }

        if (quitGameBtn != null)
        {
            quitGameBtn.onClick.AddListener(QuitGame);
        }

        // Default closed
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Start()
    {
        CreateInGameNavButtonsIfNeeded();
    }

    private void CreateInGameNavButtonsIfNeeded()
    {
        // If buttons not already serialized, construct them at the bottom of the panel
        if (returnToMenuBtn != null && quitGameBtn != null) return;

        Transform panelWin = panelRoot != null ? panelRoot.transform.Find("PanelWindow") : null;
        if (panelWin == null) return;

        // Container row for Quit & Menu buttons
        Transform existingNav = panelWin.Find("InGameNavRow");
        if (existingNav != null) return;

        GameObject navRow = new GameObject("InGameNavRow");
        navRow.transform.SetParent(panelWin, false);
        RectTransform navRt = navRow.AddComponent<RectTransform>();
        navRt.anchorMin = new Vector2(0.5f, 0f);
        navRt.anchorMax = new Vector2(0.5f, 0f);
        navRt.pivot = new Vector2(0.5f, 0f);
        navRt.anchoredPosition = new Vector2(0f, 20f);
        navRt.sizeDelta = new Vector2(480f, 44f);

        HorizontalLayoutGroup hlg = navRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // Button 1: Return to Menu
        GameObject menuBtnObj = new GameObject("ReturnToMenuBtn");
        menuBtnObj.transform.SetParent(navRow.transform, false);
        Image menuImg = menuBtnObj.AddComponent<Image>();
        menuImg.color = new Color(0.18f, 0.42f, 0.72f, 1f); // Classic blue
        returnToMenuBtn = menuBtnObj.AddComponent<Button>();
        returnToMenuBtn.onClick.AddListener(ReturnToMainMenu);

        GameObject menuTextObj = new GameObject("Text");
        menuTextObj.transform.SetParent(menuBtnObj.transform, false);
        TextMeshProUGUI menuTmp = menuTextObj.AddComponent<TextMeshProUGUI>();
        menuTmp.text = "MENU CHÍNH";
        menuTmp.fontSize = 15;
        menuTmp.fontStyle = FontStyles.Bold;
        menuTmp.alignment = TextAlignmentOptions.Center;
        menuTmp.color = Color.white;
        RectTransform mtRt = menuTextObj.GetComponent<RectTransform>();
        mtRt.anchorMin = Vector2.zero;
        mtRt.anchorMax = Vector2.one;
        mtRt.offsetMin = Vector2.zero;
        mtRt.offsetMax = Vector2.zero;

        // Button 2: Quit Game
        GameObject quitBtnObj = new GameObject("QuitGameBtn");
        quitBtnObj.transform.SetParent(navRow.transform, false);
        Image quitImg = quitBtnObj.AddComponent<Image>();
        quitImg.color = new Color(0.72f, 0.2f, 0.2f, 1f); // Crimson red
        quitGameBtn = quitBtnObj.AddComponent<Button>();
        quitGameBtn.onClick.AddListener(QuitGame);

        GameObject quitTextObj = new GameObject("Text");
        quitTextObj.transform.SetParent(quitBtnObj.transform, false);
        TextMeshProUGUI quitTmp = quitTextObj.AddComponent<TextMeshProUGUI>();
        quitTmp.text = "THOÁT GAME";
        quitTmp.fontSize = 15;
        quitTmp.fontStyle = FontStyles.Bold;
        quitTmp.alignment = TextAlignmentOptions.Center;
        quitTmp.color = Color.white;
        RectTransform qtRt = quitTextObj.GetComponent<RectTransform>();
        qtRt.anchorMin = Vector2.zero;
        qtRt.anchorMax = Vector2.one;
        qtRt.offsetMin = Vector2.zero;
        qtRt.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        // Press Escape to toggle settings (New Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    public bool IsOpen()
    {
        return panelRoot != null && panelRoot.activeSelf;
    }

    public void Open()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);

            // Pause if in a gameplay scene
            if (SceneManager.GetActiveScene().name != "1_1_MainMenu")
            {
                Time.timeScale = 0f;
            }

            // Refresh slider positions from AudioManager
            if (AudioManager.Instance != null)
            {
                if (masterSlider != null) masterSlider.value = AudioManager.Instance.MasterVolume;
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
            }

            isVfxEnabled = AudioManager.IsVFXEnabled();
            UpdateVFXVisuals();

            // Play open UI SFX
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCardAppear();
            }
        }
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);

            // Resume game timescale
            Time.timeScale = 1f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
        SceneManager.LoadScene("1_1_MainMenu");
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
        Debug.Log("[SettingPanelUI] Thoát trò chơi...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnMasterSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(val);
        }
    }

    private void OnBGMSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(val);
        }
    }

    private void OnSFXSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(val);
        }
    }

    private void ToggleVFX()
    {
        isVfxEnabled = !isVfxEnabled;
        AudioManager.SetVFXEnabled(isVfxEnabled);
        UpdateVFXVisuals();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
    }

    private void UpdateVFXVisuals()
    {
        if (vfxToggleImage != null)
        {
            if (toggleOnSprite != null && toggleOffSprite != null)
            {
                vfxToggleImage.sprite = isVfxEnabled ? toggleOnSprite : toggleOffSprite;
            }
            vfxToggleImage.color = isVfxEnabled ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        if (vfxStatusText != null)
        {
            vfxStatusText.text = isVfxEnabled ? "<color=#50E3C2>BẬT</color>" : "<color=#FF6B6B>TẮT</color>";
        }
    }
}
