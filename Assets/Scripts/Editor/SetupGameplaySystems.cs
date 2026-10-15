using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class SetupGameplaySystems
{
    [MenuItem("Tools/Setup Survivor Gameplay Systems")]
    public static void Setup()
    {
        // 1. Setup Player
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found in the active scene!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(player, "Setup Survivor Systems on Player");

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) stats = player.AddComponent<PlayerStats>();

        PlayerLevelSystem levelSys = player.GetComponent<PlayerLevelSystem>();
        if (levelSys == null) levelSys = player.AddComponent<PlayerLevelSystem>();

        FireballSpell fireball = player.GetComponent<FireballSpell>();
        if (fireball == null) fireball = player.AddComponent<FireballSpell>();

        IceSpearSpell iceSpear = player.GetComponent<IceSpearSpell>();
        if (iceSpear == null) iceSpear = player.AddComponent<IceSpearSpell>();

        DarkOrbSpell darkOrb = player.GetComponent<DarkOrbSpell>();
        if (darkOrb == null) darkOrb = player.AddComponent<DarkOrbSpell>();

        MagicShield shield = player.GetComponent<MagicShield>();
        if (shield == null) shield = player.AddComponent<MagicShield>();

        // 2. Setup Manager
        GameObject manager = GameObject.Find("GameMangaer");
        if (manager == null) manager = GameObject.Find("GameManager");
        if (manager == null) manager = new GameObject("GameManager");

        Undo.RegisterFullObjectHierarchyUndo(manager, "Setup UpgradeManager");
        UpgradeManager upgradeMgr = manager.GetComponent<UpgradeManager>();
        if (upgradeMgr == null) upgradeMgr = manager.AddComponent<UpgradeManager>();

        // 3. Setup UI in Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Setup Survivor HUD & Level Up UI");

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            Text sampleText = canvas.GetComponentInChildren<Text>(true);
            if (sampleText != null) defaultFont = sampleText.font;
        }

        // Build EXP Bar in HUD
        Transform existingExpBar = canvas.transform.Find("ExpBarPanel");
        if (existingExpBar != null) Object.DestroyImmediate(existingExpBar.gameObject);

        GameObject expBarObj = CreateExpBar(canvas.transform, defaultFont);

        // Build Level Up Panel
        Transform existingLevelUp = canvas.transform.Find("LevelUpPanel");
        if (existingLevelUp != null) Object.DestroyImmediate(existingLevelUp.gameObject);

        GameObject levelUpPanelObj = CreateLevelUpModal(canvas.transform, defaultFont, upgradeMgr);

        // Link UpgradeManager with LevelUpUI and Icons
        LevelUpUI levelUpUI = levelUpPanelObj.GetComponent<LevelUpUI>();
        SerializedObject soUpgrade = new SerializedObject(upgradeMgr);
        soUpgrade.FindProperty("levelUpUI").objectReferenceValue = levelUpUI;
        soUpgrade.FindProperty("iconBolt").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_magic_bolt.png");
        soUpgrade.FindProperty("iconFire").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_fireball.png");
        soUpgrade.FindProperty("iconIce").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_ice_spear.png");
        soUpgrade.FindProperty("iconOrb").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_dark_orb.png");
        soUpgrade.FindProperty("iconShield").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_magic_shield.png");
        soUpgrade.FindProperty("iconTome").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_arcane_tome.png");
        soUpgrade.FindProperty("iconBoots").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_hermes_boots.png");
        soUpgrade.FindProperty("iconHaste").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_spell_haste.png");
        soUpgrade.FindProperty("iconHeart").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_vitality_heart.png");
        soUpgrade.FindProperty("iconMagnet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_magnetic_pendant.png");
        soUpgrade.FindProperty("iconPotion").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Generated/icon_heal_potion.png");
        soUpgrade.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Survivor Gameplay Systems (EXP, Spells, Passives, Level Up UI) successfully setup in scene!");
    }

    private static GameObject CreateExpBar(Transform parent, Font font)
    {
        GameObject panel = new GameObject("ExpBarPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.22f, 1f);
        rt.anchorMax = new Vector2(0.78f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -16f);
        rt.sizeDelta = new Vector2(0f, 24f);

        // Background
        Image bgImg = panel.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.11f, 0.18f, 0.92f);

        // Fill Image Container
        GameObject fillContainer = new GameObject("FillContainer");
        fillContainer.transform.SetParent(panel.transform, false);
        RectTransform fillContainerRt = fillContainer.AddComponent<RectTransform>();
        fillContainerRt.anchorMin = new Vector2(0f, 0f);
        fillContainerRt.anchorMax = new Vector2(1f, 1f);
        fillContainerRt.offsetMin = new Vector2(3f, 3f);
        fillContainerRt.offsetMax = new Vector2(-3f, -3f);

        GameObject fillObj = new GameObject("ExpFill");
        fillObj.transform.SetParent(fillContainer.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.0f, 0.85f, 1.0f, 1f);

        // Level Badge on the left
        GameObject levelBadgeObj = new GameObject("LevelBadge");
        levelBadgeObj.transform.SetParent(panel.transform, false);
        RectTransform badgeRt = levelBadgeObj.AddComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0f, 0.5f);
        badgeRt.anchorMax = new Vector2(0f, 0.5f);
        badgeRt.pivot = new Vector2(1f, 0.5f);
        badgeRt.anchoredPosition = new Vector2(-10f, 0f);
        badgeRt.sizeDelta = new Vector2(90f, 28f);

        Image badgeBg = levelBadgeObj.AddComponent<Image>();
        badgeBg.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(levelBadgeObj.transform, false);
        RectTransform levelTextRt = levelTextObj.AddComponent<RectTransform>();
        levelTextRt.anchorMin = Vector2.zero;
        levelTextRt.anchorMax = Vector2.one;
        levelTextRt.offsetMin = Vector2.zero;
        levelTextRt.offsetMax = Vector2.zero;

        Text levelText = levelTextObj.AddComponent<Text>();
        levelText.font = font;
        levelText.fontSize = 16;
        levelText.fontStyle = FontStyle.Bold;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = new Color(1f, 0.85f, 0.2f, 1f);
        levelText.text = "LV. 1";

        // Progress Text in middle
        GameObject expTextObj = new GameObject("ExpText");
        expTextObj.transform.SetParent(panel.transform, false);
        RectTransform expTextRt = expTextObj.AddComponent<RectTransform>();
        expTextRt.anchorMin = Vector2.zero;
        expTextRt.anchorMax = Vector2.one;
        expTextRt.offsetMin = Vector2.zero;
        expTextRt.offsetMax = Vector2.zero;

        Text expText = expTextObj.AddComponent<Text>();
        expText.font = font;
        expText.fontSize = 13;
        expText.fontStyle = FontStyle.Bold;
        expText.alignment = TextAnchor.MiddleCenter;
        expText.color = Color.white;
        expText.text = "0 / 25 EXP (0%)";

        // Attach ExpBarUI
        ExpBarUI expBarUI = panel.AddComponent<ExpBarUI>();
        SerializedObject so = new SerializedObject(expBarUI);
        so.FindProperty("expFillImage").objectReferenceValue = fillImg;
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("expProgressText").objectReferenceValue = expText;
        so.ApplyModifiedProperties();

        return panel;
    }

    private static GameObject CreateLevelUpModal(Transform parent, Font font, UpgradeManager upgradeMgr)
    {
        GameObject modal = new GameObject("LevelUpPanel");
        modal.transform.SetParent(parent, false);

        RectTransform modalRt = modal.AddComponent<RectTransform>();
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.offsetMin = Vector2.zero;
        modalRt.offsetMax = Vector2.zero;

        // Dark dimming overlay
        Image overlay = modal.AddComponent<Image>();
        overlay.color = new Color(0.04f, 0.05f, 0.09f, 0.90f);

        // Header Banner: LEVEL UP!
        GameObject headerObj = new GameObject("HeaderTitle");
        headerObj.transform.SetParent(modal.transform, false);
        RectTransform headerRt = headerObj.AddComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 0.5f);
        headerRt.anchorMax = new Vector2(0.5f, 0.5f);
        headerRt.pivot = new Vector2(0.5f, 0.5f);
        headerRt.anchoredPosition = new Vector2(0f, 260f);
        headerRt.sizeDelta = new Vector2(600f, 70f);

        Text headerText = headerObj.AddComponent<Text>();
        headerText.font = font;
        headerText.fontSize = 46;
        headerText.fontStyle = FontStyle.Bold;
        headerText.alignment = TextAnchor.MiddleCenter;
        headerText.color = new Color(1f, 0.85f, 0.2f, 1f);
        headerText.text = "LEVEL UP!";

        // Header Outline effect
        Outline outline = headerObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.1f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(modal.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.5f);
        subRt.anchorMax = new Vector2(0.5f, 0.5f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.anchoredPosition = new Vector2(0f, 210f);
        subRt.sizeDelta = new Vector2(600f, 35f);

        Text subText = subObj.AddComponent<Text>();
        subText.font = font;
        subText.fontSize = 18;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.7f, 0.8f, 0.95f, 1f);
        subText.text = "Choose an upgrade to empower your Wizard";

        // Cards Container
        GameObject containerObj = new GameObject("CardsContainer");
        containerObj.transform.SetParent(modal.transform, false);
        RectTransform containerRt = containerObj.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0f, -30f);
        containerRt.sizeDelta = new Vector2(900f, 380f);

        UpgradeCardUI[] cardArray = new UpgradeCardUI[3];
        float[] xPositions = new float[] { -290f, 0f, 290f };

        for (int i = 0; i < 3; i++)
        {
            cardArray[i] = CreateUpgradeCard(containerObj.transform, font, xPositions[i], i);
        }

        LevelUpUI levelUpUI = modal.AddComponent<LevelUpUI>();
        SerializedObject soModal = new SerializedObject(levelUpUI);
        soModal.FindProperty("modalPanel").objectReferenceValue = modal;
        soModal.FindProperty("headerTitleText").objectReferenceValue = headerText;
        soModal.FindProperty("subtitleText").objectReferenceValue = subText;

        SerializedProperty cardsProp = soModal.FindProperty("cards");
        cardsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cardArray[i];
        }
        soModal.ApplyModifiedProperties();

        // Inactive by default
        modal.SetActive(false);

        return modal;
    }

    private static UpgradeCardUI CreateUpgradeCard(Transform parent, Font font, float posX, int index)
    {
        GameObject cardObj = new GameObject($"UpgradeCard_{index}");
        cardObj.transform.SetParent(parent, false);

        RectTransform cardRt = cardObj.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = new Vector2(posX, 0f);
        cardRt.sizeDelta = new Vector2(260f, 360f);

        // Card Frame Image
        Image bgImg = cardObj.AddComponent<Image>();
        bgImg.color = new Color(0.10f, 0.13f, 0.20f, 0.98f);

        // Add Button and Animation on entire card
        Button cardBtn = cardObj.AddComponent<Button>();
        cardBtn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = cardBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.25f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.95f, 1f);
        cardBtn.colors = colors;

        cardObj.AddComponent<UIButtonAnimation>();

        // Border outline
        Outline cardBorder = cardObj.AddComponent<Outline>();
        cardBorder.effectColor = new Color(0.25f, 0.35f, 0.52f, 0.8f);
        cardBorder.effectDistance = new Vector2(2f, 2f);

        // 1. Tag / Category Banner
        GameObject tagObj = new GameObject("TagText");
        tagObj.transform.SetParent(cardObj.transform, false);
        RectTransform tagRt = tagObj.AddComponent<RectTransform>();
        tagRt.anchorMin = new Vector2(0f, 1f);
        tagRt.anchorMax = new Vector2(1f, 1f);
        tagRt.pivot = new Vector2(0.5f, 1f);
        tagRt.anchoredPosition = new Vector2(0f, -16f);
        tagRt.sizeDelta = new Vector2(-20f, 24f);

        Text tagText = tagObj.AddComponent<Text>();
        tagText.font = font;
        tagText.fontSize = 14;
        tagText.fontStyle = FontStyle.Bold;
        tagText.alignment = TextAnchor.MiddleCenter;
        tagText.color = new Color(1f, 0.85f, 0.2f, 1f);
        tagText.text = "[ NEW SPELL ]";

        // 2. Icon Container
        GameObject iconFrameObj = new GameObject("IconFrame");
        iconFrameObj.transform.SetParent(cardObj.transform, false);
        RectTransform iconFrameRt = iconFrameObj.AddComponent<RectTransform>();
        iconFrameRt.anchorMin = new Vector2(0.5f, 1f);
        iconFrameRt.anchorMax = new Vector2(0.5f, 1f);
        iconFrameRt.pivot = new Vector2(0.5f, 1f);
        iconFrameRt.anchoredPosition = new Vector2(0f, -50f);
        iconFrameRt.sizeDelta = new Vector2(72f, 72f);

        Image iconFrameImg = iconFrameObj.AddComponent<Image>();
        iconFrameImg.color = new Color(0.06f, 0.08f, 0.12f, 1f);

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(iconFrameObj.transform, false);
        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.offsetMin = new Vector2(4f, 4f);
        iconRt.offsetMax = new Vector2(-4f, -4f);

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.preserveAspect = true;

        // 3. Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -135f);
        titleRt.sizeDelta = new Vector2(-20f, 30f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.text = "Spell Name";

        // 4. Level Badge Text
        GameObject levelObj = new GameObject("LevelBadgeText");
        levelObj.transform.SetParent(cardObj.transform, false);
        RectTransform levelRt = levelObj.AddComponent<RectTransform>();
        levelRt.anchorMin = new Vector2(0f, 1f);
        levelRt.anchorMax = new Vector2(1f, 1f);
        levelRt.pivot = new Vector2(0.5f, 1f);
        levelRt.anchoredPosition = new Vector2(0f, -165f);
        levelRt.sizeDelta = new Vector2(-20f, 22f);

        Text levelText = levelObj.AddComponent<Text>();
        levelText.font = font;
        levelText.fontSize = 14;
        levelText.fontStyle = FontStyle.Bold;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = new Color(0.4f, 0.85f, 1f, 1f);
        levelText.text = "Lv. 1 → Lv. 2";

        // 5. Description Text
        GameObject descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(cardObj.transform, false);
        RectTransform descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 1f);
        descRt.anchorMax = new Vector2(1f, 1f);
        descRt.pivot = new Vector2(0.5f, 1f);
        descRt.anchoredPosition = new Vector2(0f, -195f);
        descRt.sizeDelta = new Vector2(-24f, 95f);

        Text descText = descObj.AddComponent<Text>();
        descText.font = font;
        descText.fontSize = 14;
        descText.lineSpacing = 1.15f;
        descText.alignment = TextAnchor.UpperCenter;
        descText.color = new Color(0.8f, 0.88f, 0.98f, 1f);
        descText.text = "Upgrade description explaining details of the upgrade.";

        // 6. Action Button at bottom
        GameObject btnObj = new GameObject("SelectButton");
        btnObj.transform.SetParent(cardObj.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 18f);
        btnRt.sizeDelta = new Vector2(180f, 40f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.42f, 0.85f, 1f);

        Button bottomBtn = btnObj.AddComponent<Button>();

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = font;
        btnText.fontSize = 15;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.text = "CHOOSE";

        // Bind CardUI
        UpgradeCardUI cardUI = cardObj.AddComponent<UpgradeCardUI>();
        SerializedObject soCard = new SerializedObject(cardUI);
        soCard.FindProperty("tagText").objectReferenceValue = tagText;
        soCard.FindProperty("iconImage").objectReferenceValue = iconImg;
        soCard.FindProperty("titleText").objectReferenceValue = titleText;
        soCard.FindProperty("levelBadgeText").objectReferenceValue = levelText;
        soCard.FindProperty("descriptionText").objectReferenceValue = descText;
        soCard.FindProperty("selectButton").objectReferenceValue = cardBtn;
        soCard.FindProperty("cardBackgroundImage").objectReferenceValue = bgImg;
        soCard.ApplyModifiedProperties();

        // Also redirect bottom button to card button
        bottomBtn.onClick.AddListener(() => cardBtn.onClick.Invoke());

        return cardUI;
    }
}
