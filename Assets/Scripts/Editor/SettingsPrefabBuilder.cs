using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SettingsPrefabBuilder
{
    private const string PREFAB_PATH = "Assets/Prefabs/SettingsPanel.prefab";

    [MenuItem("Tools/Build Settings Panel Prefab")]
    public static GameObject BuildPrefab()
    {
        // 1. Configure sprites first
        SettingsSpritePostprocessor.ConfigureSprites();

        // 2. Load Sprites & Fonts
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/panel_frame_9slice.png");
        Sprite iconVol = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/icon_volume.png");
        Sprite iconBgm = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/icon_bgm.png");
        Sprite iconSfx = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/icon_sfx.png");
        Sprite iconVfx = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/icon_vfx.png");
        Sprite iconClose = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/icon_close.png");
        Sprite knobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/slider_knob.png");
        Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/slider_track.png");
        Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/slider_fill.png");
        Sprite toggleOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/toggle_on.png");
        Sprite toggleOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Settings/toggle_off.png");

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/PressStart2P-Regular SDF.asset");
        if (fontAsset == null)
        {
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/AnimationUI/Demo/Font/PassionOne-Regular SDF.asset");
        }

        // 3. Create Root GameObject (Stretch overlay)
        GameObject rootGo = new GameObject("SettingsPanel");
        RectTransform rootRect = rootGo.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Dim background blocker button
        GameObject bgGo = new GameObject("BackgroundBlocker");
        bgGo.transform.SetParent(rootGo.transform, false);
        RectTransform bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.72f);
        Button bgButton = bgGo.AddComponent<Button>();

        // Center Panel Window
        GameObject panelWin = new GameObject("PanelWindow");
        panelWin.transform.SetParent(rootGo.transform, false);
        RectTransform winRect = panelWin.AddComponent<RectTransform>();
        winRect.anchorMin = new Vector2(0.5f, 0.5f);
        winRect.anchorMax = new Vector2(0.5f, 0.5f);
        winRect.pivot = new Vector2(0.5f, 0.5f);
        winRect.sizeDelta = new Vector2(560, 480);
        winRect.anchoredPosition = Vector2.zero;

        Image winImage = panelWin.AddComponent<Image>();
        if (frameSprite != null)
        {
            winImage.sprite = frameSprite;
            winImage.type = Image.Type.Sliced;
        }
        else
        {
            winImage.color = new Color(0.12f, 0.1f, 0.18f, 0.95f);
        }

        // Title Text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelWin.transform, false);
        RectTransform titleRect = titleGo.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -28);
        titleRect.sizeDelta = new Vector2(400, 45);

        TextMeshProUGUI titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) titleTMP.font = fontAsset;
        titleTMP.text = "CÀI ĐẶT";
        titleTMP.fontSize = 24;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.25f, 1f);

        // Close Button
        GameObject closeGo = new GameObject("CloseButton");
        closeGo.transform.SetParent(panelWin.transform, false);
        RectTransform closeRect = closeGo.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        closeRect.sizeDelta = new Vector2(36, 36);

        Image closeImg = closeGo.AddComponent<Image>();
        if (iconClose != null) closeImg.sprite = iconClose;
        Button closeBtn = closeGo.AddComponent<Button>();

        // Content Container (Vertical layout)
        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panelWin.transform, false);
        RectTransform contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0, -25);
        contentRect.sizeDelta = new Vector2(480, 340);

        VerticalLayoutGroup vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Helper to build Slider Row
        Slider masterSlider = CreateSliderRow(contentGo, "Row_MasterVol", "TỔNG ÂM LƯỢNG", iconVol, knobSprite, trackSprite, fillSprite, fontAsset);
        Slider bgmSlider = CreateSliderRow(contentGo, "Row_BGMVol", "NHẠC NỀN (BGM)", iconBgm, knobSprite, trackSprite, fillSprite, fontAsset);
        Slider sfxSlider = CreateSliderRow(contentGo, "Row_SFXVol", "HIỆU ỨNG (SFX)", iconSfx, knobSprite, trackSprite, fillSprite, fontAsset);

        // Toggle Row for VFX
        var (vfxBtn, vfxImg, vfxStatusText) = CreateToggleRow(contentGo, "Row_VFX", "HIỆU ỨNG VFX", iconVfx, toggleOn, fontAsset);

        // 4. Attach SettingPanelUI Component
        SettingPanelUI panelScript = rootGo.AddComponent<SettingPanelUI>();

        // Wire references using SerializedObject
        SerializedObject so = new SerializedObject(panelScript);
        so.FindProperty("panelRoot").objectReferenceValue = rootGo;
        so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        so.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
        so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        so.FindProperty("vfxToggleBtn").objectReferenceValue = vfxBtn;
        so.FindProperty("vfxToggleImage").objectReferenceValue = vfxImg;
        so.FindProperty("vfxStatusText").objectReferenceValue = vfxStatusText;
        so.FindProperty("closeBtn").objectReferenceValue = closeBtn;
        so.FindProperty("backgroundBlockerBtn").objectReferenceValue = bgButton;
        so.FindProperty("toggleOnSprite").objectReferenceValue = toggleOn;
        so.FindProperty("toggleOffSprite").objectReferenceValue = toggleOff;
        so.ApplyModifiedProperties();

        // Save as Prefab
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PREFAB_PATH);
        Debug.Log("[Settings] Prefab created successfully at: " + PREFAB_PATH);

        // Destroy temporary scene instance
        Object.DestroyImmediate(rootGo);

        return savedPrefab;
    }

    private static Slider CreateSliderRow(GameObject parent, string rowName, string label, Sprite icon, Sprite knob, Sprite track, Sprite fill, TMP_FontAsset font)
    {
        GameObject row = new GameObject(rowName);
        row.transform.SetParent(parent.transform, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(480, 54);

        // Icon
        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(row.transform, false);
        RectTransform iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(10, 0);
        iconRect.sizeDelta = new Vector2(32, 32);

        Image iconImg = iconGo.AddComponent<Image>();
        if (icon != null) iconImg.sprite = icon;

        // Label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(52, 0);
        labelRect.sizeDelta = new Vector2(180, 32);

        TextMeshProUGUI labelTMP = labelGo.AddComponent<TextMeshProUGUI>();
        if (font != null) labelTMP.font = font;
        labelTMP.text = label;
        labelTMP.fontSize = 12;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        labelTMP.color = Color.white;

        // Slider Object
        GameObject sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(row.transform, false);
        RectTransform sliderRect = sliderGo.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(1f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(1f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(-10, 0);
        sliderRect.sizeDelta = new Vector2(210, 24);

        Slider slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Background / Track
        GameObject trackGo = new GameObject("Background");
        trackGo.transform.SetParent(sliderGo.transform, false);
        RectTransform trackRect = trackGo.AddComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0f, 0.25f);
        trackRect.anchorMax = new Vector2(1f, 0.75f);
        trackRect.offsetMin = Vector2.zero;
        trackRect.offsetMax = Vector2.zero;

        Image trackImg = trackGo.AddComponent<Image>();
        if (track != null)
        {
            trackImg.sprite = track;
            trackImg.type = Image.Type.Sliced;
        }
        else
        {
            trackImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        }

        // Fill Area
        GameObject fillAreaGo = new GameObject("Fill Area");
        fillAreaGo.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        RectTransform fillRect = fillGo.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;

        Image fillImg = fillGo.AddComponent<Image>();
        if (fill != null)
        {
            fillImg.sprite = fill;
            fillImg.type = Image.Type.Sliced;
        }
        else
        {
            fillImg.color = new Color(0.95f, 0.75f, 0.2f, 1f);
        }

        // Handle Slide Area
        GameObject handleAreaGo = new GameObject("Handle Slide Area");
        handleAreaGo.transform.SetParent(sliderGo.transform, false);
        RectTransform handleAreaRect = handleAreaGo.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        GameObject handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleAreaGo.transform, false);
        RectTransform handleRect = handleGo.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24, 24);

        Image handleImg = handleGo.AddComponent<Image>();
        if (knob != null) handleImg.sprite = knob;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        return slider;
    }

    private static (Button, Image, TextMeshProUGUI) CreateToggleRow(GameObject parent, string rowName, string label, Sprite icon, Sprite toggleSprite, TMP_FontAsset font)
    {
        GameObject row = new GameObject(rowName);
        row.transform.SetParent(parent.transform, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(480, 54);

        // Icon
        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(row.transform, false);
        RectTransform iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(10, 0);
        iconRect.sizeDelta = new Vector2(32, 32);

        Image iconImg = iconGo.AddComponent<Image>();
        if (icon != null) iconImg.sprite = icon;

        // Label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(52, 0);
        labelRect.sizeDelta = new Vector2(180, 32);

        TextMeshProUGUI labelTMP = labelGo.AddComponent<TextMeshProUGUI>();
        if (font != null) labelTMP.font = font;
        labelTMP.text = label;
        labelTMP.fontSize = 12;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        labelTMP.color = Color.white;

        // Button Container
        GameObject btnGo = new GameObject("ToggleBtn");
        btnGo.transform.SetParent(row.transform, false);
        RectTransform btnRect = btnGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 0.5f);
        btnRect.anchorMax = new Vector2(1f, 0.5f);
        btnRect.pivot = new Vector2(1f, 0.5f);
        btnRect.anchoredPosition = new Vector2(-10, 0);
        btnRect.sizeDelta = new Vector2(110, 36);

        Image btnImg = btnGo.AddComponent<Image>();
        if (toggleSprite != null) btnImg.sprite = toggleSprite;
        Button btn = btnGo.AddComponent<Button>();

        // Status text next to or inside toggle
        GameObject statusGo = new GameObject("StatusText");
        statusGo.transform.SetParent(btnGo.transform, false);
        RectTransform statusRect = statusGo.AddComponent<RectTransform>();
        statusRect.anchorMin = Vector2.zero;
        statusRect.anchorMax = Vector2.one;
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;

        TextMeshProUGUI statusTMP = statusGo.AddComponent<TextMeshProUGUI>();
        if (font != null) statusTMP.font = font;
        statusTMP.text = "BẬT";
        statusTMP.fontSize = 11;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusTMP.color = Color.white;

        return (btn, btnImg, statusTMP);
    }
}

