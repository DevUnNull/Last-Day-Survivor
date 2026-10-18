using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupSkillCodex
{
    [MenuItem("Tools/Setup Skill Codex UI in MainMenu")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "1_1_MainMenu")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/1_1_MainMenu.unity");
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in 1_1_MainMenu!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Setup Skill Codex UI");

        Font pixelFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Baloo_2,Chewy,Press_Start_2P/Press_Start_2P/PressStart2P-Regular.ttf");
        if (pixelFont == null) pixelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Remove existing panel if any
        Transform existing = canvas.transform.Find("SkillCodexPanel");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        // 1. Root Modal Panel
        GameObject modalObj = new GameObject("SkillCodexPanel");
        modalObj.transform.SetParent(canvas.transform, false);
        RectTransform modalRt = modalObj.AddComponent<RectTransform>();
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.offsetMin = Vector2.zero;
        modalRt.offsetMax = Vector2.zero;

        // Dim background overlay (click to close)
        GameObject dimObj = new GameObject("DimOverlay");
        dimObj.transform.SetParent(modalObj.transform, false);
        RectTransform dimRt = dimObj.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        Image dimImg = dimObj.AddComponent<Image>();
        dimImg.color = new Color(0.02f, 0.03f, 0.06f, 0.90f);
        Button dimBtn = dimObj.AddComponent<Button>();

        // 2. Main Box Frame (1480 x 860)
        GameObject boxObj = new GameObject("MainBox");
        boxObj.transform.SetParent(modalObj.transform, false);
        RectTransform boxRt = boxObj.AddComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(1480f, 860f);

        Image boxImg = boxObj.AddComponent<Image>();
        boxImg.color = new Color(0.07f, 0.09f, 0.15f, 0.98f);

        Outline boxOutline = boxObj.AddComponent<Outline>();
        boxOutline.effectColor = new Color(0.85f, 0.65f, 0.18f, 0.95f);
        boxOutline.effectDistance = new Vector2(3f, -3f);

        // 3. Header Banner
        GameObject headerObj = new GameObject("HeaderBanner");
        headerObj.transform.SetParent(boxObj.transform, false);
        RectTransform headerRt = headerObj.AddComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = Vector2.zero;
        headerRt.sizeDelta = new Vector2(0f, 90f);

        Image headerImg = headerObj.AddComponent<Image>();
        headerImg.color = new Color(0.11f, 0.15f, 0.25f, 0.98f);

        Outline headerLine = headerObj.AddComponent<Outline>();
        headerLine.effectColor = new Color(0.24f, 0.35f, 0.55f, 0.8f);
        headerLine.effectDistance = new Vector2(0f, -2f);

        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0f, 12f);
        titleRt.sizeDelta = new Vector2(900f, 38f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pixelFont;
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.88f, 0.25f, 1f);
        titleText.text = "⚔ SPELL & UPGRADE CODEX ⚔";

        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.25f, 0.12f, 0f, 0.9f);
        titleOutline.effectDistance = new Vector2(2f, -2f);

        // Subtitle Text
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(headerObj.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.5f);
        subRt.anchorMax = new Vector2(0.5f, 0.5f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.anchoredPosition = new Vector2(0f, -22f);
        subRt.sizeDelta = new Vector2(900f, 24f);

        Text subText = subObj.AddComponent<Text>();
        subText.font = pixelFont;
        subText.fontSize = 10;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.68f, 0.78f, 0.95f, 1f);
        subText.text = "Encyclopedia of arcane spells and abilities for the Wizard";

        // Top Close Button [✕ CLOSE]
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(headerObj.transform, false);
        RectTransform closeRt = closeObj.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 0.5f);
        closeRt.anchorMax = new Vector2(1f, 0.5f);
        closeRt.pivot = new Vector2(1f, 0.5f);
        closeRt.anchoredPosition = new Vector2(-20f, 0f);
        closeRt.sizeDelta = new Vector2(130f, 44f);

        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.65f, 0.16f, 0.16f, 0.95f);

        Outline closeOutline = closeObj.AddComponent<Outline>();
        closeOutline.effectColor = new Color(0.9f, 0.4f, 0.4f, 0.8f);
        closeOutline.effectDistance = new Vector2(2f, -2f);

        Button closeBtn = closeObj.AddComponent<Button>();
        closeObj.AddComponent<UIButtonAnimation>();

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        RectTransform ctRt = closeTextObj.AddComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.offsetMin = Vector2.zero;
        ctRt.offsetMax = Vector2.zero;

        Text ctText = closeTextObj.AddComponent<Text>();
        ctText.font = pixelFont;
        ctText.fontSize = 11;
        ctText.fontStyle = FontStyle.Bold;
        ctText.alignment = TextAnchor.MiddleCenter;
        ctText.color = Color.white;
        ctText.text = "✕ CLOSE";

        // 4. Category Tabs Bar
        GameObject tabsObj = new GameObject("TabsBar");
        tabsObj.transform.SetParent(boxObj.transform, false);
        RectTransform tabsRt = tabsObj.AddComponent<RectTransform>();
        tabsRt.anchorMin = new Vector2(0f, 1f);
        tabsRt.anchorMax = new Vector2(0f, 1f);
        tabsRt.pivot = new Vector2(0f, 1f);
        tabsRt.anchoredPosition = new Vector2(20f, -98f);
        tabsRt.sizeDelta = new Vector2(460f, 38f);

        HorizontalLayoutGroup tabsHlg = tabsObj.AddComponent<HorizontalLayoutGroup>();
        tabsHlg.childForceExpandWidth = true;
        tabsHlg.childForceExpandHeight = true;
        tabsHlg.spacing = 6f;

        Button tabAllBtn = CreateTabButton(tabsObj.transform, "ALL (11)", pixelFont);
        Button tabSpellsBtn = CreateTabButton(tabsObj.transform, "SPELLS (5)", pixelFont);
        Button tabPassivesBtn = CreateTabButton(tabsObj.transform, "PASSIVES (6)", pixelFont);

        // 5. Left Column (Scrollable List of Skills)
        GameObject leftColObj = new GameObject("LeftColumn");
        leftColObj.transform.SetParent(boxObj.transform, false);
        RectTransform leftRt = leftColObj.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0f, 0f);
        leftRt.anchorMax = new Vector2(0f, 1f);
        leftRt.pivot = new Vector2(0f, 0.5f);
        leftRt.offsetMin = new Vector2(20f, 70f);
        leftRt.offsetMax = new Vector2(480f, -142f);

        Image leftBg = leftColObj.AddComponent<Image>();
        leftBg.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);

        Outline leftOutline = leftColObj.AddComponent<Outline>();
        leftOutline.effectColor = new Color(0.20f, 0.28f, 0.42f, 0.7f);
        leftOutline.effectDistance = new Vector2(2f, -2f);

        // ScrollRect
        ScrollRect scrollRect = leftColObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 35f;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(leftColObj.transform, false);
        RectTransform vpRt = viewportObj.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = new Vector2(6f, 6f);
        vpRt.offsetMax = new Vector2(-6f, -6f);

        Image vpImg = viewportObj.AddComponent<Image>();
        vpImg.color = Color.white;
        Mask vpMask = viewportObj.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup contentVlg = contentObj.AddComponent<VerticalLayoutGroup>();
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;
        contentVlg.childControlWidth = true;
        contentVlg.childControlHeight = false;
        contentVlg.spacing = 8f;
        contentVlg.padding = new RectOffset(4, 4, 4, 4);

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRt;
        scrollRect.content = contentRt;

        // 6. Right Column (Details)
        GameObject rightColObj = new GameObject("RightColumn");
        rightColObj.transform.SetParent(boxObj.transform, false);
        RectTransform rightRt = rightColObj.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0f, 0f);
        rightRt.anchorMax = new Vector2(1f, 1f);
        rightRt.pivot = new Vector2(1f, 0.5f);
        rightRt.offsetMin = new Vector2(500f, 70f);
        rightRt.offsetMax = new Vector2(-20f, -98f);

        Image rightBg = rightColObj.AddComponent<Image>();
        rightBg.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);

        Outline rightOutline = rightColObj.AddComponent<Outline>();
        rightOutline.effectColor = new Color(0.20f, 0.28f, 0.42f, 0.7f);
        rightOutline.effectDistance = new Vector2(2f, -2f);

        // Right Detail Header (Icon, Name, Tag, Max Level)
        GameObject detailHeaderObj = new GameObject("DetailHeader");
        detailHeaderObj.transform.SetParent(rightColObj.transform, false);
        RectTransform dhRt = detailHeaderObj.AddComponent<RectTransform>();
        dhRt.anchorMin = new Vector2(0f, 1f);
        dhRt.anchorMax = new Vector2(1f, 1f);
        dhRt.pivot = new Vector2(0.5f, 1f);
        dhRt.anchoredPosition = new Vector2(0f, -10f);
        dhRt.sizeDelta = new Vector2(-20f, 110f);

        Image dhBg = detailHeaderObj.AddComponent<Image>();
        dhBg.color = new Color(0.09f, 0.13f, 0.22f, 0.95f);

        Outline dhOutline = detailHeaderObj.AddComponent<Outline>();
        dhOutline.effectColor = new Color(0.24f, 0.35f, 0.55f, 0.7f);
        dhOutline.effectDistance = new Vector2(2f, -2f);

        // Big Icon Box
        GameObject bigIconBox = new GameObject("IconBox");
        bigIconBox.transform.SetParent(detailHeaderObj.transform, false);
        RectTransform bibRt = bigIconBox.AddComponent<RectTransform>();
        bibRt.anchorMin = new Vector2(0f, 0.5f);
        bibRt.anchorMax = new Vector2(0f, 0.5f);
        bibRt.pivot = new Vector2(0f, 0.5f);
        bibRt.anchoredPosition = new Vector2(16f, 0f);
        bibRt.sizeDelta = new Vector2(84f, 84f);

        Image bibBg = bigIconBox.AddComponent<Image>();
        bibBg.color = new Color(0.04f, 0.06f, 0.1f, 1f);

        Outline bibOutline = bigIconBox.AddComponent<Outline>();
        bibOutline.effectColor = new Color(0.85f, 0.65f, 0.18f, 0.9f);
        bibOutline.effectDistance = new Vector2(2f, -2f);

        GameObject bigIconObj = new GameObject("Icon");
        bigIconObj.transform.SetParent(bigIconBox.transform, false);
        RectTransform biRt = bigIconObj.AddComponent<RectTransform>();
        biRt.anchorMin = Vector2.zero;
        biRt.anchorMax = Vector2.one;
        biRt.offsetMin = new Vector2(6f, 6f);
        biRt.offsetMax = new Vector2(-6f, -6f);

        Image detailIconImg = bigIconObj.AddComponent<Image>();
        detailIconImg.preserveAspect = true;

        // Detail Name Text
        GameObject dNameObj = new GameObject("NameText");
        dNameObj.transform.SetParent(detailHeaderObj.transform, false);
        RectTransform dnRt = dNameObj.AddComponent<RectTransform>();
        dnRt.anchorMin = new Vector2(0f, 1f);
        dnRt.anchorMax = new Vector2(1f, 1f);
        dnRt.pivot = new Vector2(0f, 1f);
        dnRt.anchoredPosition = new Vector2(116f, -14f);
        dnRt.sizeDelta = new Vector2(-280f, 30f);

        Text detailNameText = dNameObj.AddComponent<Text>();
        detailNameText.font = pixelFont;
        detailNameText.fontSize = 17;
        detailNameText.fontStyle = FontStyle.Bold;
        detailNameText.alignment = TextAnchor.MiddleLeft;
        detailNameText.color = new Color(1f, 0.90f, 0.35f, 1f);

        // English Name Text
        GameObject dEnNameObj = new GameObject("EnglishNameText");
        dEnNameObj.transform.SetParent(detailHeaderObj.transform, false);
        RectTransform denRt = dEnNameObj.AddComponent<RectTransform>();
        denRt.anchorMin = new Vector2(0f, 1f);
        denRt.anchorMax = new Vector2(1f, 1f);
        denRt.pivot = new Vector2(0f, 1f);
        denRt.anchoredPosition = new Vector2(116f, -44f);
        denRt.sizeDelta = new Vector2(-280f, 22f);

        Text detailEnText = dEnNameObj.AddComponent<Text>();
        detailEnText.font = pixelFont;
        detailEnText.fontSize = 11;
        detailEnText.alignment = TextAnchor.MiddleLeft;
        detailEnText.color = new Color(0.7f, 0.82f, 0.98f, 1f);

        // Category Tag Text
        GameObject dTagObj = new GameObject("TagText");
        dTagObj.transform.SetParent(detailHeaderObj.transform, false);
        RectTransform dtagRt = dTagObj.AddComponent<RectTransform>();
        dtagRt.anchorMin = new Vector2(0f, 0f);
        dtagRt.anchorMax = new Vector2(1f, 0f);
        dtagRt.pivot = new Vector2(0f, 0f);
        dtagRt.anchoredPosition = new Vector2(116f, 14f);
        dtagRt.sizeDelta = new Vector2(-280f, 22f);

        Text detailTagText = dTagObj.AddComponent<Text>();
        detailTagText.font = pixelFont;
        detailTagText.fontSize = 10;
        detailTagText.alignment = TextAnchor.MiddleLeft;
        detailTagText.color = new Color(0.4f, 0.85f, 1f, 1f);

        // Max Level Badge (Top Right of Detail Header)
        GameObject dMaxLvlObj = new GameObject("MaxLevelBadge");
        dMaxLvlObj.transform.SetParent(detailHeaderObj.transform, false);
        RectTransform dmlRt = dMaxLvlObj.AddComponent<RectTransform>();
        dmlRt.anchorMin = new Vector2(1f, 0.5f);
        dmlRt.anchorMax = new Vector2(1f, 0.5f);
        dmlRt.pivot = new Vector2(1f, 0.5f);
        dmlRt.anchoredPosition = new Vector2(-16f, 0f);
        dmlRt.sizeDelta = new Vector2(160f, 44f);

        Image dmlImg = dMaxLvlObj.AddComponent<Image>();
        dmlImg.color = new Color(0.16f, 0.24f, 0.40f, 0.95f);

        Outline dmlOutline = dMaxLvlObj.AddComponent<Outline>();
        dmlOutline.effectColor = new Color(0.4f, 0.6f, 0.9f, 0.7f);
        dmlOutline.effectDistance = new Vector2(2f, -2f);

        GameObject dmlTextObj = new GameObject("Text");
        dmlTextObj.transform.SetParent(dMaxLvlObj.transform, false);
        RectTransform dmltRt = dmlTextObj.AddComponent<RectTransform>();
        dmltRt.anchorMin = Vector2.zero;
        dmltRt.anchorMax = Vector2.one;
        dmltRt.offsetMin = Vector2.zero;
        dmltRt.offsetMax = Vector2.zero;

        Text detailMaxLvlText = dmlTextObj.AddComponent<Text>();
        detailMaxLvlText.font = pixelFont;
        detailMaxLvlText.fontSize = 10;
        detailMaxLvlText.fontStyle = FontStyle.Bold;
        detailMaxLvlText.alignment = TextAnchor.MiddleCenter;
        detailMaxLvlText.color = new Color(1f, 0.88f, 0.35f, 1f);

        // Overview Box
        GameObject overviewBox = new GameObject("OverviewBox");
        overviewBox.transform.SetParent(rightColObj.transform, false);
        RectTransform obRt = overviewBox.AddComponent<RectTransform>();
        obRt.anchorMin = new Vector2(0f, 1f);
        obRt.anchorMax = new Vector2(1f, 1f);
        obRt.pivot = new Vector2(0.5f, 1f);
        obRt.anchoredPosition = new Vector2(0f, -128f);
        obRt.sizeDelta = new Vector2(-20f, 66f);

        Image obImg = overviewBox.AddComponent<Image>();
        obImg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);

        Outline obOutline = overviewBox.AddComponent<Outline>();
        obOutline.effectColor = new Color(0.18f, 0.24f, 0.36f, 0.6f);
        obOutline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject overviewTextObj = new GameObject("OverviewText");
        overviewTextObj.transform.SetParent(overviewBox.transform, false);
        RectTransform otRt = overviewTextObj.AddComponent<RectTransform>();
        otRt.anchorMin = Vector2.zero;
        otRt.anchorMax = Vector2.one;
        otRt.offsetMin = new Vector2(14f, 6f);
        otRt.offsetMax = new Vector2(-14f, -6f);

        Text detailOverviewText = overviewTextObj.AddComponent<Text>();
        detailOverviewText.font = pixelFont;
        detailOverviewText.fontSize = 10;
        detailOverviewText.lineSpacing = 1.3f;
        detailOverviewText.verticalOverflow = VerticalWrapMode.Overflow;
        detailOverviewText.alignment = TextAnchor.MiddleLeft;
        detailOverviewText.color = new Color(0.85f, 0.90f, 0.98f, 1f);

        // Progression Title Text
        GameObject progTitleObj = new GameObject("ProgressionTitle");
        progTitleObj.transform.SetParent(rightColObj.transform, false);
        RectTransform ptRt = progTitleObj.AddComponent<RectTransform>();
        ptRt.anchorMin = new Vector2(0f, 1f);
        ptRt.anchorMax = new Vector2(1f, 1f);
        ptRt.pivot = new Vector2(0f, 1f);
        ptRt.anchoredPosition = new Vector2(12f, -202f);
        ptRt.sizeDelta = new Vector2(-24f, 24f);

        Text progTitleText = progTitleObj.AddComponent<Text>();
        progTitleText.font = pixelFont;
        progTitleText.fontSize = 11;
        progTitleText.fontStyle = FontStyle.Bold;
        progTitleText.alignment = TextAnchor.MiddleLeft;
        progTitleText.color = new Color(1f, 0.85f, 0.3f, 1f);
        progTitleText.text = "UPGRADE PROGRESSION DETAILS (LEVEL 1 -> 5):";

        // Levels Scroll/Container
        GameObject levelsContObj = new GameObject("LevelsContainer");
        levelsContObj.transform.SetParent(rightColObj.transform, false);
        RectTransform lcRt = levelsContObj.AddComponent<RectTransform>();
        lcRt.anchorMin = new Vector2(0f, 0f);
        lcRt.anchorMax = new Vector2(1f, 1f);
        lcRt.offsetMin = new Vector2(10f, 10f);
        lcRt.offsetMax = new Vector2(-10f, -230f);

        VerticalLayoutGroup lcVlg = levelsContObj.AddComponent<VerticalLayoutGroup>();
        lcVlg.childForceExpandWidth = true;
        lcVlg.childForceExpandHeight = false;
        lcVlg.childControlWidth = true;
        lcVlg.childControlHeight = false;
        lcVlg.spacing = 8f;

        // 7. Bottom Return Button [BACK TO MAIN MENU]
        GameObject returnObj = new GameObject("ReturnButton");
        returnObj.transform.SetParent(boxObj.transform, false);
        RectTransform retRt = returnObj.AddComponent<RectTransform>();
        retRt.anchorMin = new Vector2(0.5f, 0f);
        retRt.anchorMax = new Vector2(0.5f, 0f);
        retRt.pivot = new Vector2(0.5f, 0f);
        retRt.anchoredPosition = new Vector2(0f, 14f);
        retRt.sizeDelta = new Vector2(280f, 44f);

        Image retImg = returnObj.AddComponent<Image>();
        retImg.color = new Color(0.16f, 0.36f, 0.72f, 0.95f);

        Outline retOutline = returnObj.AddComponent<Outline>();
        retOutline.effectColor = new Color(0.4f, 0.65f, 1f, 0.8f);
        retOutline.effectDistance = new Vector2(2f, -2f);

        Button retBtn = returnObj.AddComponent<Button>();
        returnObj.AddComponent<UIButtonAnimation>();

        GameObject retTextObj = new GameObject("Text");
        retTextObj.transform.SetParent(returnObj.transform, false);
        RectTransform rttRt = retTextObj.AddComponent<RectTransform>();
        rttRt.anchorMin = Vector2.zero;
        rttRt.anchorMax = Vector2.one;
        rttRt.offsetMin = Vector2.zero;
        rttRt.offsetMax = Vector2.zero;

        Text retText = retTextObj.AddComponent<Text>();
        retText.font = pixelFont;
        retText.fontSize = 11;
        retText.fontStyle = FontStyle.Bold;
        retText.alignment = TextAnchor.MiddleCenter;
        retText.color = Color.white;
        retText.text = "◀ BACK TO MENU";

        // Bind Dim Button to Close as well
        dimBtn.onClick.AddListener(() => retBtn.onClick.Invoke());

        // 8. Bind Component SkillCodexUI
        SkillCodexUI codexUI = modalObj.AddComponent<SkillCodexUI>();
        codexUI.InitializeDefaultSkills();

        // Assign Sprites to Default Skills
        AssignSkillSprites(codexUI);

        SerializedObject soCodex = new SerializedObject(codexUI);
        soCodex.FindProperty("modalPanel").objectReferenceValue = modalObj;
        soCodex.FindProperty("closeButton").objectReferenceValue = closeBtn;
        soCodex.FindProperty("returnButton").objectReferenceValue = retBtn;
        soCodex.FindProperty("tabAllButton").objectReferenceValue = tabAllBtn;
        soCodex.FindProperty("tabSpellsButton").objectReferenceValue = tabSpellsBtn;
        soCodex.FindProperty("tabPassivesButton").objectReferenceValue = tabPassivesBtn;
        soCodex.FindProperty("listContentContainer").objectReferenceValue = contentRt;
        soCodex.FindProperty("listScrollRect").objectReferenceValue = scrollRect;
        soCodex.FindProperty("detailIcon").objectReferenceValue = detailIconImg;
        soCodex.FindProperty("detailNameText").objectReferenceValue = detailNameText;
        soCodex.FindProperty("detailEnglishNameText").objectReferenceValue = detailEnText;
        soCodex.FindProperty("detailCategoryTagText").objectReferenceValue = detailTagText;
        soCodex.FindProperty("detailMaxLevelText").objectReferenceValue = detailMaxLvlText;
        soCodex.FindProperty("detailOverviewText").objectReferenceValue = detailOverviewText;
        soCodex.FindProperty("levelsContainer").objectReferenceValue = lcRt;
        soCodex.FindProperty("pixelFont").objectReferenceValue = pixelFont;
        soCodex.ApplyModifiedProperties();

        // 9. Wire Skill button on Canvas to OpenPanel
        GameObject skillBtnObj = GameObject.Find("Canvas/Skill");
        if (skillBtnObj != null)
        {
            Button sBtn = skillBtnObj.GetComponent<Button>();
            if (sBtn != null)
            {
                // Clear old persistent calls
                SerializedObject soBtn = new SerializedObject(sBtn);
                SerializedProperty callsProp = soBtn.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                callsProp.ClearArray();
                callsProp.arraySize = 1;
                SerializedProperty call = callsProp.GetArrayElementAtIndex(0);
                call.FindPropertyRelative("m_Target").objectReferenceValue = codexUI;
                call.FindPropertyRelative("m_MethodName").stringValue = "OpenPanel";
                call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
                call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly
                soBtn.ApplyModifiedProperties();
            }
        }

        // Set inactive by default
        modalObj.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Successfully created SkillCodexPanel and wired Skill button!");
    }

    private static Button CreateTabButton(Transform parent, string title, Font font)
    {
        GameObject tabObj = new GameObject($"Tab_{title}");
        tabObj.transform.SetParent(parent, false);

        RectTransform rt = tabObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140f, 36f);

        Image img = tabObj.AddComponent<Image>();
        img.color = new Color(0.10f, 0.14f, 0.22f, 0.9f);

        Outline outline = tabObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.24f, 0.32f, 0.48f, 0.7f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = tabObj.AddComponent<Button>();
        tabObj.AddComponent<UIButtonAnimation>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tabObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text t = textObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 10;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = title;

        return btn;
    }

    private static void AssignSkillSprites(SkillCodexUI codexUI)
    {
        SerializedObject so = new SerializedObject(codexUI);
        SerializedProperty skillsProp = so.FindProperty("skills");

        var map = new Dictionary<string, string>
        {
            { "magic_bolt", "Assets/Sprite/Skill/Magic Bolt.png" },
            { "fireball", "Assets/Sprite/Skill/Fireball.png" },
            { "ice_spear", "Assets/Sprite/Skill/Ice Spear.png" },
            { "dark_orb", "Assets/Sprite/Skill/Dark Orb.png" },
            { "magic_shield", "Assets/Sprite/Skill/Magic Shield.png" },
            { "might", "Assets/Sprite/Skill/Might.png" },
            { "swiftness", "Assets/Sprite/Skill/Swiftness.png" },
            { "haste", "Assets/Sprite/Skill/Spell Haste.png" },
            { "vitality", "Assets/Sprite/Skill/Vitality.png" },
            { "magnet", "Assets/Sprite/Skill/Magnet.png" },
            { "health_potion", "Assets/Sprite/Skill/Health Potion.png" }
        };

        for (int i = 0; i < skillsProp.arraySize; i++)
        {
            SerializedProperty skillProp = skillsProp.GetArrayElementAtIndex(i);
            string id = skillProp.FindPropertyRelative("id").stringValue;
            if (map.ContainsKey(id))
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(map[id]);
                if (sp != null)
                {
                    skillProp.FindPropertyRelative("icon").objectReferenceValue = sp;
                }
            }
        }
        so.ApplyModifiedProperties();
    }
}
