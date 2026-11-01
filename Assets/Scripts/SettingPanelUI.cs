using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    private void Start()
    {
        // Load initial values from AudioManager
        if (AudioManager.Instance != null)
        {
            if (masterSlider != null) masterSlider.value = AudioManager.Instance.MasterVolume;
            if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
            if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
        }

        isVfxEnabled = AudioManager.IsVFXEnabled();
        UpdateVFXVisuals();

        // Default closed
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        // Press Escape to close/toggle settings
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOpen())
            {
                Close();
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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
        }
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

