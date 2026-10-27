using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class SetupSurvivorArchitecture
{
    private static TMP_FontAsset GetPixelFont()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/PressStart2P-Regular SDF.asset");
    }

    [MenuItem("Tools/1. Build Boss Prefab")]
    public static GameObject BuildBossPrefab()
    {
        if (!Directory.Exists("Assets/Prefabs/Bosses"))
        {
            Directory.CreateDirectory("Assets/Prefabs/Bosses");
        }

        string prefabPath = "Assets/Prefabs/Bosses/BossPrefab.prefab";

        GameObject go = new GameObject("BossPrefab");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 1.6f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        go.AddComponent<BossHealth>();
        go.AddComponent<BossAnimationController>();
        go.AddComponent<BossTelegraphController>();
        go.AddComponent<BossController>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        Debug.Log("[SetupSurvivorArchitecture] BossPrefab saved at " + prefabPath);
        return prefab;
    }

    [MenuItem("Tools/2. Setup Main Menu Map Selection")]
    public static void SetupMainMenu()
    {
        SetupMainMenuScene("Assets/Scenes/1_1_MainMenu.unity", false);
    }

    public static void SetupMainMenuScene(string scenePath, bool panelActiveByDefault)
    {
        if (!string.IsNullOrEmpty(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        TMP_FontAsset font = GetPixelFont();
        Sprite upgradeBaseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AnimationUI/Demo/UI/UpgradeBase.png");

        // Load 3 MapData assets
        MapData map1 = AssetDatabase.LoadAssetAtPath<MapData>("Assets/Settings/Maps/Map_1_SkyRuins.asset");
        MapData map2 = AssetDatabase.LoadAssetAtPath<MapData>("Assets/Settings/Maps/Map_2_DarkStorm.asset");
        MapData map3 = AssetDatabase.LoadAssetAtPath<MapData>("Assets/Settings/Maps/Map_3_AbyssalEclipse.asset");

        // Clean up previous MapSelectionPanel if present
        Transform oldPanel = canvas.transform.Find("MapSelectionPanel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

        // 1. Root Panel
        GameObject panelObj = new GameObject("MapSelectionPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // Dark dim backdrop
        Image bgImg = panelObj.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

        // 2. Title Header
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -40f);
        titleRt.sizeDelta = new Vector2(900f, 60f);

        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTmp.font = font;
        titleTmp.text = "CHOOSE BATTLEGROUND";
        titleTmp.fontSize = 32f;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(1f, 0.85f, 0.3f, 1f);

        // Subtitle
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(panelObj.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 1f);
        subRt.anchorMax = new Vector2(0.5f, 1f);
        subRt.pivot = new Vector2(0.5f, 1f);
        subRt.anchoredPosition = new Vector2(0f, -100f);
        subRt.sizeDelta = new Vector2(900f, 35f);

        TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
        if (font != null) subTmp.font = font;
        subTmp.text = "SURVIVE WAVES • SLAY THE COLOSSAL BOSS";
        subTmp.fontSize = 14f;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.7f, 0.8f, 0.9f, 0.85f);

        // 3. Cards Container
        GameObject containerObj = new GameObject("CardsContainer");
        containerObj.transform.SetParent(panelObj.transform, false);
        RectTransform containerRt = containerObj.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0f, 15f);
        containerRt.sizeDelta = new Vector2(1360f, 580f);

        HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 35f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Build 3 Cards
        MapData[] maps = new MapData[] { map1, map2, map3 };
        MapCardUI[] createdCards = new MapCardUI[3];

        for (int i = 0; i < 3; i++)
        {
            createdCards[i] = CreateMapCard(containerObj.transform, i + 1, maps[i], font, upgradeBaseSprite);
        }

        // 4. Bottom Controls Bar
        GameObject bottomBar = new GameObject("BottomBar");
        bottomBar.transform.SetParent(panelObj.transform, false);
        RectTransform bbRt = bottomBar.AddComponent<RectTransform>();
        bbRt.anchorMin = new Vector2(0.5f, 0f);
        bbRt.anchorMax = new Vector2(0.5f, 0f);
        bbRt.pivot = new Vector2(0.5f, 0f);
        bbRt.anchoredPosition = new Vector2(0f, 35f);
        bbRt.sizeDelta = new Vector2(700f, 75f);

        // Start Button
        GameObject startBtnObj = new GameObject("StartGameButton");
        startBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform sbRt = startBtnObj.AddComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1f, 0.5f);
        sbRt.anchorMax = new Vector2(1f, 0.5f);
        sbRt.pivot = new Vector2(1f, 0.5f);
        sbRt.anchoredPosition = new Vector2(0f, 0f);
        sbRt.sizeDelta = new Vector2(280f, 65f);

        Image sbImg = startBtnObj.AddComponent<Image>();
        sbImg.color = new Color(0.18f, 0.65f, 0.32f, 1f);
        Button startBtn = startBtnObj.AddComponent<Button>();

        GameObject sbTextObj = new GameObject("Text");
        sbTextObj.transform.SetParent(startBtnObj.transform, false);
        RectTransform sbtRt = sbTextObj.AddComponent<RectTransform>();
        sbtRt.anchorMin = Vector2.zero;
        sbtRt.anchorMax = Vector2.one;
        sbtRt.offsetMin = Vector2.zero;
        sbtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI sbTmp = sbTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) sbTmp.font = font;
        sbTmp.text = "START MAP";
        sbTmp.fontSize = 18f;
        sbTmp.alignment = TextAlignmentOptions.Center;
        sbTmp.color = Color.white;

        // Back Button
        GameObject backBtnObj = new GameObject("BackButton");
        backBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform bbBtnRt = backBtnObj.AddComponent<RectTransform>();
        bbBtnRt.anchorMin = new Vector2(0f, 0.5f);
        bbBtnRt.anchorMax = new Vector2(0f, 0.5f);
        bbBtnRt.pivot = new Vector2(0f, 0.5f);
        bbBtnRt.anchoredPosition = new Vector2(0f, 0f);
        bbBtnRt.sizeDelta = new Vector2(220f, 65f);

        Image bbImg = backBtnObj.AddComponent<Image>();
        bbImg.color = new Color(0.4f, 0.15f, 0.18f, 1f);
        Button backBtn = backBtnObj.AddComponent<Button>();

        GameObject bbTextObj = new GameObject("Text");
        bbTextObj.transform.SetParent(backBtnObj.transform, false);
        RectTransform bbtRt = bbTextObj.AddComponent<RectTransform>();
        bbtRt.anchorMin = Vector2.zero;
        bbtRt.anchorMax = Vector2.one;
        bbtRt.offsetMin = Vector2.zero;
        bbtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI bbTmp = bbTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) bbTmp.font = font;
        bbTmp.text = "BACK";
        bbTmp.fontSize = 18f;
        bbTmp.alignment = TextAlignmentOptions.Center;
        bbTmp.color = Color.white;

        // 5. Attach MapSelectionController
        MapSelectionController controller = panelObj.AddComponent<MapSelectionController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("mapSelectionPanel").objectReferenceValue = panelObj;
        so.FindProperty("startGameButton").objectReferenceValue = startBtn;
        so.FindProperty("backButton").objectReferenceValue = backBtn;

        SerializedProperty mapsProp = so.FindProperty("availableMaps");
        mapsProp.ClearArray();
        for (int i = 0; i < 3; i++)
        {
            mapsProp.InsertArrayElementAtIndex(i);
            mapsProp.GetArrayElementAtIndex(i).objectReferenceValue = maps[i];
        }

        SerializedProperty cardsProp = so.FindProperty("mapCards");
        cardsProp.ClearArray();
        for (int i = 0; i < 3; i++)
        {
            cardsProp.InsertArrayElementAtIndex(i);
            cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdCards[i];
        }

        so.ApplyModifiedProperties();

        // 6. Hook Main Menu's Start Button
        Transform startBtnTransform = canvas.transform.Find("Start");
        if (startBtnTransform != null)
        {
            Button menuStartButton = startBtnTransform.GetComponent<Button>();
            if (menuStartButton != null)
            {
                // Remove old persistent events
                SerializedObject soMenuBtn = new SerializedObject(menuStartButton);
                SerializedProperty callsProp = soMenuBtn.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                callsProp.ClearArray();

                // Add call to controller.OpenMapSelection()
                callsProp.InsertArrayElementAtIndex(0);
                SerializedProperty call = callsProp.GetArrayElementAtIndex(0);
                call.FindPropertyRelative("m_Target").objectReferenceValue = controller;
                call.FindPropertyRelative("m_MethodName").stringValue = "OpenMapSelection";
                call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
                call.FindPropertyRelative("m_CallState").enumValueIndex = 1; // EditorAndRuntime

                soMenuBtn.ApplyModifiedProperties();
                Debug.Log("[SetupSurvivorArchitecture] Main Menu 'Start' button successfully redirected to OpenMapSelection()!");
            }
        }

        panelObj.SetActive(panelActiveByDefault);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"[SetupSurvivorArchitecture] {EditorSceneManager.GetActiveScene().name} setup complete!");
    }

    private static MapCardUI CreateMapCard(Transform parent, int index, MapData data, TMP_FontAsset font, Sprite baseSprite)
    {
        GameObject cardObj = new GameObject($"MapCard_{index}");
        cardObj.transform.SetParent(parent, false);

        RectTransform cardRt = cardObj.AddComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(410f, 550f);

        Image bg = cardObj.AddComponent<Image>();
        if (baseSprite != null)
        {
            bg.sprite = baseSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.9f, 0.9f, 0.95f, 1f);
        }
        else
        {
            bg.color = new Color(0.12f, 0.14f, 0.2f, 1f);
        }

        Button btn = cardObj.AddComponent<Button>();

        // 1. Preview Banner Image
        GameObject previewObj = new GameObject("MapPreview");
        previewObj.transform.SetParent(cardObj.transform, false);
        RectTransform prevRt = previewObj.AddComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0.5f, 1f);
        prevRt.anchorMax = new Vector2(0.5f, 1f);
        prevRt.pivot = new Vector2(0.5f, 1f);
        prevRt.anchoredPosition = new Vector2(0f, -20f);
        prevRt.sizeDelta = new Vector2(370f, 180f);

        Image prevImg = previewObj.AddComponent<Image>();
        if (data != null && data.previewSprite != null)
        {
            prevImg.sprite = data.previewSprite;
        }
        else
        {
            prevImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);
        }

        // 2. Map Name
        GameObject nameObj = new GameObject("MapName");
        nameObj.transform.SetParent(cardObj.transform, false);
        RectTransform nameRt = nameObj.AddComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0.5f, 1f);
        nameRt.anchorMax = new Vector2(0.5f, 1f);
        nameRt.pivot = new Vector2(0.5f, 1f);
        nameRt.anchoredPosition = new Vector2(0f, -215f);
        nameRt.sizeDelta = new Vector2(370f, 32f);

        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        if (font != null) nameTmp.font = font;
        nameTmp.text = data != null ? data.mapName.ToUpper() : $"MAP {index}";
        nameTmp.fontSize = 18f;
        nameTmp.alignment = TextAlignmentOptions.Center;
        nameTmp.color = new Color(1f, 0.88f, 0.3f, 1f);

        // 3. Difficulty
        GameObject diffObj = new GameObject("Difficulty");
        diffObj.transform.SetParent(cardObj.transform, false);
        RectTransform diffRt = diffObj.AddComponent<RectTransform>();
        diffRt.anchorMin = new Vector2(0.5f, 1f);
        diffRt.anchorMax = new Vector2(0.5f, 1f);
        diffRt.pivot = new Vector2(0.5f, 1f);
        diffRt.anchoredPosition = new Vector2(0f, -250f);
        diffRt.sizeDelta = new Vector2(370f, 25f);

        TextMeshProUGUI diffTmp = diffObj.AddComponent<TextMeshProUGUI>();
        if (font != null) diffTmp.font = font;
        diffTmp.text = data != null ? data.difficulty : "NORMAL";
        diffTmp.fontSize = 13f;
        diffTmp.alignment = TextAlignmentOptions.Center;
        diffTmp.color = index == 1 ? Color.cyan : (index == 2 ? Color.yellow : new Color(1f, 0.4f, 0.4f, 1f));

        // 4. Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(cardObj.transform, false);
        RectTransform descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 1f);
        descRt.anchorMax = new Vector2(0.5f, 1f);
        descRt.pivot = new Vector2(0.5f, 1f);
        descRt.anchoredPosition = new Vector2(0f, -280f);
        descRt.sizeDelta = new Vector2(360f, 85f);

        TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
        if (font != null) descTmp.font = font;
        descTmp.text = data != null ? data.description : "";
        descTmp.fontSize = 10f;
        descTmp.alignment = TextAlignmentOptions.TopLeft;
        descTmp.color = new Color(0.2f, 0.22f, 0.3f, 1f); // Dark contrast for UpgradeBase background
        descTmp.enableWordWrapping = true;

        // 5. Boss Preview Box
        GameObject bossBoxObj = new GameObject("BossPreviewBox");
        bossBoxObj.transform.SetParent(cardObj.transform, false);
        RectTransform bboxRt = bossBoxObj.AddComponent<RectTransform>();
        bboxRt.anchorMin = new Vector2(0.5f, 0f);
        bboxRt.anchorMax = new Vector2(0.5f, 0f);
        bboxRt.pivot = new Vector2(0.5f, 0f);
        bboxRt.anchoredPosition = new Vector2(0f, 25f);
        bboxRt.sizeDelta = new Vector2(370f, 110f);

        Image bboxImg = bossBoxObj.AddComponent<Image>();
        bboxImg.color = new Color(0.08f, 0.1f, 0.15f, 0.95f);

        // Boss Icon
        GameObject biconObj = new GameObject("BossIcon");
        biconObj.transform.SetParent(bossBoxObj.transform, false);
        RectTransform biconRt = biconObj.AddComponent<RectTransform>();
        biconRt.anchorMin = new Vector2(0f, 0.5f);
        biconRt.anchorMax = new Vector2(0f, 0.5f);
        biconRt.pivot = new Vector2(0f, 0.5f);
        biconRt.anchoredPosition = new Vector2(12f, 0f);
        biconRt.sizeDelta = new Vector2(85f, 85f);

        Image biconImg = biconObj.AddComponent<Image>();
        biconImg.preserveAspect = true;
        if (data != null && data.bossData != null && data.bossData.bossIcon != null)
        {
            biconImg.sprite = data.bossData.bossIcon;
        }

        // Boss Text
        GameObject bnameObj = new GameObject("BossName");
        bnameObj.transform.SetParent(bossBoxObj.transform, false);
        RectTransform bnameRt = bnameObj.AddComponent<RectTransform>();
        bnameRt.anchorMin = new Vector2(0f, 0f);
        bnameRt.anchorMax = new Vector2(1f, 1f);
        bnameRt.offsetMin = new Vector2(110f, 10f);
        bnameRt.offsetMax = new Vector2(-10f, -10f);

        TextMeshProUGUI bnameTmp = bnameObj.AddComponent<TextMeshProUGUI>();
        if (font != null) bnameTmp.font = font;
        bnameTmp.text = data != null && data.bossData != null ? $"FINAL BOSS:\n<color=#FFD700>{data.bossData.bossName}</color>\n<size=9><color=#AAAAAA>{data.bossData.bossTitle}</color></size>" : "BOSS: UNKNOWN";
        bnameTmp.fontSize = 11f;
        bnameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        bnameTmp.color = Color.white;

        // 6. Selection Highlight Border
        GameObject highlightObj = new GameObject("SelectionHighlight");
        highlightObj.transform.SetParent(cardObj.transform, false);
        RectTransform hlRt = highlightObj.AddComponent<RectTransform>();
        hlRt.anchorMin = Vector2.zero;
        hlRt.anchorMax = Vector2.one;
        hlRt.offsetMin = new Vector2(-6f, -6f);
        hlRt.offsetMax = new Vector2(6f, 6f);

        Image hlImg = highlightObj.AddComponent<Image>();
        hlImg.color = new Color(1f, 0.85f, 0.2f, 0.8f);
        hlImg.raycastTarget = false;
        highlightObj.SetActive(false);

        // 7. Lock Overlay
        GameObject lockObj = new GameObject("LockOverlay");
        lockObj.transform.SetParent(cardObj.transform, false);
        RectTransform lockRt = lockObj.AddComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.offsetMin = Vector2.zero;
        lockRt.offsetMax = Vector2.zero;

        Image lockImg = lockObj.AddComponent<Image>();
        lockImg.color = new Color(0.02f, 0.02f, 0.04f, 0.88f);

        GameObject lockTextObj = new GameObject("LockText");
        lockTextObj.transform.SetParent(lockObj.transform, false);
        RectTransform ltRt = lockTextObj.AddComponent<RectTransform>();
        ltRt.anchorMin = Vector2.zero;
        ltRt.anchorMax = Vector2.one;
        ltRt.offsetMin = Vector2.zero;
        ltRt.offsetMax = Vector2.zero;

        TextMeshProUGUI ltTmp = lockTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ltTmp.font = font;
        ltTmp.text = "LOCKED\n\n<size=11><color=#FF6666>Defeat Previous Boss\nTo Unlock</color></size>";
        ltTmp.fontSize = 16f;
        ltTmp.alignment = TextAlignmentOptions.Center;
        ltTmp.color = Color.white;
        lockObj.SetActive(false);

        // Attach MapCardUI
        MapCardUI cardUI = cardObj.AddComponent<MapCardUI>();
        SerializedObject soCard = new SerializedObject(cardUI);
        soCard.FindProperty("mapPreviewImage").objectReferenceValue = prevImg;
        soCard.FindProperty("mapNameText").objectReferenceValue = nameTmp;
        soCard.FindProperty("mapDescText").objectReferenceValue = descTmp;
        soCard.FindProperty("difficultyText").objectReferenceValue = diffTmp;
        soCard.FindProperty("bossIconImage").objectReferenceValue = biconImg;
        soCard.FindProperty("bossNameText").objectReferenceValue = bnameTmp;
        soCard.FindProperty("lockOverlay").objectReferenceValue = lockObj;
        soCard.FindProperty("lockReasonText").objectReferenceValue = ltTmp;
        soCard.FindProperty("highlightBorder").objectReferenceValue = highlightObj;
        soCard.FindProperty("selectButton").objectReferenceValue = btn;
        soCard.ApplyModifiedProperties();

        return cardUI;
    }

    [MenuItem("Tools/3. Setup InGame Gameplay and Boss UI")]
    public static void SetupInGame()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/2_InGame.unity");

        GameObject gm = GameObject.Find("GameMangaer");
        if (gm == null) gm = GameObject.Find("GameManager");
        if (gm == null) gm = new GameObject("GameManager");

        GameplaySessionManager sessionMgr = gm.GetComponent<GameplaySessionManager>();
        if (sessionMgr == null) sessionMgr = gm.AddComponent<GameplaySessionManager>();

        MapData defaultMap = AssetDatabase.LoadAssetAtPath<MapData>("Assets/Settings/Maps/Map_1_SkyRuins.asset");
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bosses/BossPrefab.prefab");
        TMP_FontAsset font = GetPixelFont();
        Sprite winSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AnimationUI/Demo/UI/Win.png");
        Sprite loseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AnimationUI/Demo/UI/Lose.png");

        Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();

        // 1. Setup Background References
        Sprite map1Sprite = SetupMapsAndBosses.LoadSubSprite("Assets/AnimationUI/Demo/UI/map1.png");
        Sprite map2Sprite = SetupMapsAndBosses.LoadSubSprite("Assets/AnimationUI/Demo/UI/map2.png");
        Sprite map3Sprite = SetupMapsAndBosses.LoadSubSprite("Assets/AnimationUI/Demo/UI/map3.png");

        List<Image> bgList = new List<Image>();
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            foreach (Transform t in c.transform)
            {
                if (t.name.StartsWith("BackGround"))
                {
                    Image img = t.GetComponent<Image>();
                    if (img != null) bgList.Add(img);
                }
            }
        }

        if (bgList.Count > 0 && map1Sprite != null) bgList[0].sprite = map1Sprite;
        if (bgList.Count > 1 && map2Sprite != null) bgList[1].sprite = map2Sprite;
        if (bgList.Count > 2 && map3Sprite != null) bgList[2].sprite = map3Sprite;

        // 2. HUD Timer & Map Indicator
        Transform existingHud = mainCanvas.transform.Find("SurvivorHUD");
        if (existingHud != null) Object.DestroyImmediate(existingHud.gameObject);

        GameObject hudObj = new GameObject("SurvivorHUD");
        hudObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform hudRt = hudObj.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0f, 1f);
        hudRt.anchorMax = new Vector2(1f, 1f);
        hudRt.pivot = new Vector2(0.5f, 1f);
        hudRt.anchoredPosition = Vector2.zero;
        hudRt.sizeDelta = new Vector2(0f, 60f);

        // Timer Text
        GameObject timerObj = new GameObject("SurvivalTimerText");
        timerObj.transform.SetParent(hudObj.transform, false);
        RectTransform timerRt = timerObj.AddComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0.5f, 1f);
        timerRt.anchorMax = new Vector2(0.5f, 1f);
        timerRt.pivot = new Vector2(0.5f, 1f);
        timerRt.anchoredPosition = new Vector2(0f, -12f);
        timerRt.sizeDelta = new Vector2(400f, 40f);

        TextMeshProUGUI timerTmp = timerObj.AddComponent<TextMeshProUGUI>();
        if (font != null) timerTmp.font = font;
        timerTmp.text = "00:00 / 05:00";
        timerTmp.fontSize = 20f;
        timerTmp.alignment = TextAlignmentOptions.Center;
        timerTmp.color = Color.white;

        // Map Name
        GameObject mapNameObj = new GameObject("MapNameHUDText");
        mapNameObj.transform.SetParent(hudObj.transform, false);
        RectTransform mnRt = mapNameObj.AddComponent<RectTransform>();
        mnRt.anchorMin = new Vector2(0f, 1f);
        mnRt.anchorMax = new Vector2(0f, 1f);
        mnRt.pivot = new Vector2(0f, 1f);
        mnRt.anchoredPosition = new Vector2(25f, -15f);
        mnRt.sizeDelta = new Vector2(300f, 35f);

        TextMeshProUGUI mnTmp = mapNameObj.AddComponent<TextMeshProUGUI>();
        if (font != null) mnTmp.font = font;
        mnTmp.text = "SKY RUINS";
        mnTmp.fontSize = 14f;
        mnTmp.alignment = TextAlignmentOptions.Left;
        mnTmp.color = new Color(1f, 0.85f, 0.3f, 1f);

        // 3. Colossal Boss UI at top
        Transform existingBossUI = mainCanvas.transform.Find("BossUIPanel");
        if (existingBossUI != null) Object.DestroyImmediate(existingBossUI.gameObject);

        GameObject bossUIObj = new GameObject("BossUIPanel");
        bossUIObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform buiRt = bossUIObj.AddComponent<RectTransform>();
        buiRt.anchorMin = new Vector2(0.5f, 1f);
        buiRt.anchorMax = new Vector2(0.5f, 1f);
        buiRt.pivot = new Vector2(0.5f, 1f);
        buiRt.anchoredPosition = new Vector2(0f, -60f);
        buiRt.sizeDelta = new Vector2(820f, 95f);

        // Boss Box Frame
        Image buiBg = bossUIObj.AddComponent<Image>();
        buiBg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        // Boss Icon
        GameObject biconObj = new GameObject("BossIcon");
        biconObj.transform.SetParent(bossUIObj.transform, false);
        RectTransform biconRt = biconObj.AddComponent<RectTransform>();
        biconRt.anchorMin = new Vector2(0f, 0.5f);
        biconRt.anchorMax = new Vector2(0f, 0.5f);
        biconRt.pivot = new Vector2(0f, 0.5f);
        biconRt.anchoredPosition = new Vector2(10f, 0f);
        biconRt.sizeDelta = new Vector2(75f, 75f);
        Image biconImg = biconObj.AddComponent<Image>();
        biconImg.preserveAspect = true;

        // Boss Name & Title
        GameObject bnameObj = new GameObject("BossName");
        bnameObj.transform.SetParent(bossUIObj.transform, false);
        RectTransform bnRt = bnameObj.AddComponent<RectTransform>();
        bnRt.anchorMin = new Vector2(0f, 1f);
        bnRt.anchorMax = new Vector2(1f, 1f);
        bnRt.pivot = new Vector2(0f, 1f);
        bnRt.anchoredPosition = new Vector2(95f, -10f);
        bnRt.sizeDelta = new Vector2(-105f, 25f);

        TextMeshProUGUI bnTmp = bnameObj.AddComponent<TextMeshProUGUI>();
        if (font != null) bnTmp.font = font;
        bnTmp.text = "SKY DRAGON";
        bnTmp.fontSize = 16f;
        bnTmp.color = new Color(1f, 0.85f, 0.2f, 1f);

        // Boss Title Subtitle
        GameObject btitleObj = new GameObject("BossTitle");
        btitleObj.transform.SetParent(bossUIObj.transform, false);
        RectTransform btRt = btitleObj.AddComponent<RectTransform>();
        btRt.anchorMin = new Vector2(0f, 1f);
        btRt.anchorMax = new Vector2(1f, 1f);
        btRt.pivot = new Vector2(0f, 1f);
        btRt.anchoredPosition = new Vector2(95f, -32f);
        btRt.sizeDelta = new Vector2(-105f, 18f);

        TextMeshProUGUI btTmp = btitleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) btTmp.font = font;
        btTmp.text = "Scourge of the Ancient Skies";
        btTmp.fontSize = 10f;
        btTmp.color = new Color(0.7f, 0.75f, 0.85f, 1f);

        // Phase Text
        GameObject phaseObj = new GameObject("PhaseText");
        phaseObj.transform.SetParent(bossUIObj.transform, false);
        RectTransform ptRt = phaseObj.AddComponent<RectTransform>();
        ptRt.anchorMin = new Vector2(1f, 1f);
        ptRt.anchorMax = new Vector2(1f, 1f);
        ptRt.pivot = new Vector2(1f, 1f);
        ptRt.anchoredPosition = new Vector2(-15f, -12f);
        ptRt.sizeDelta = new Vector2(140f, 25f);

        TextMeshProUGUI ptTmp = phaseObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ptTmp.font = font;
        ptTmp.text = "PHASE 1";
        ptTmp.fontSize = 12f;
        ptTmp.alignment = TextAlignmentOptions.Right;
        ptTmp.color = new Color(1f, 0.4f, 0.4f, 1f);

        // Health Bar Background
        GameObject hpBgObj = new GameObject("HealthBarBG");
        hpBgObj.transform.SetParent(bossUIObj.transform, false);
        RectTransform hpBgRt = hpBgObj.AddComponent<RectTransform>();
        hpBgRt.anchorMin = new Vector2(0f, 0f);
        hpBgRt.anchorMax = new Vector2(1f, 0f);
        hpBgRt.pivot = new Vector2(0f, 0f);
        hpBgRt.anchoredPosition = new Vector2(95f, 12f);
        hpBgRt.sizeDelta = new Vector2(-110f, 24f);

        Image hpBgImg = hpBgObj.AddComponent<Image>();
        hpBgImg.color = new Color(0.2f, 0.05f, 0.08f, 1f);

        // Health Fill
        GameObject hpFillObj = new GameObject("HealthFill");
        hpFillObj.transform.SetParent(hpBgObj.transform, false);
        RectTransform hpFillRt = hpFillObj.AddComponent<RectTransform>();
        hpFillRt.anchorMin = Vector2.zero;
        hpFillRt.anchorMax = Vector2.one;
        hpFillRt.offsetMin = Vector2.zero;
        hpFillRt.offsetMax = Vector2.zero;

        Image hpFillImg = hpFillObj.AddComponent<Image>();
        hpFillImg.color = new Color(0.95f, 0.2f, 0.2f, 1f);

        // Health Numbers
        GameObject hpNumObj = new GameObject("HealthNumbers");
        hpNumObj.transform.SetParent(hpBgObj.transform, false);
        RectTransform hpNumRt = hpNumObj.AddComponent<RectTransform>();
        hpNumRt.anchorMin = Vector2.zero;
        hpNumRt.anchorMax = Vector2.one;
        hpNumRt.offsetMin = Vector2.zero;
        hpNumRt.offsetMax = Vector2.zero;

        TextMeshProUGUI hpNumTmp = hpNumObj.AddComponent<TextMeshProUGUI>();
        if (font != null) hpNumTmp.font = font;
        hpNumTmp.text = "2,500 / 2,500";
        hpNumTmp.fontSize = 11f;
        hpNumTmp.alignment = TextAlignmentOptions.Center;
        hpNumTmp.color = Color.white;

        // Wire BossUIController
        BossUIController buiController = bossUIObj.AddComponent<BossUIController>();
        SerializedObject soBui = new SerializedObject(buiController);
        soBui.FindProperty("bossUIRoot").objectReferenceValue = bossUIObj;
        soBui.FindProperty("bossNameText").objectReferenceValue = bnTmp;
        soBui.FindProperty("bossTitleText").objectReferenceValue = btTmp;
        soBui.FindProperty("bossIconImage").objectReferenceValue = biconImg;
        soBui.FindProperty("phaseText").objectReferenceValue = ptTmp;
        soBui.FindProperty("healthFillImage").objectReferenceValue = hpFillImg;
        soBui.FindProperty("healthNumbersText").objectReferenceValue = hpNumTmp;
        soBui.ApplyModifiedProperties();
        bossUIObj.SetActive(false);

        // 4. Boss Warning Banner
        Transform existingWarn = mainCanvas.transform.Find("BossWarningBanner");
        if (existingWarn != null) Object.DestroyImmediate(existingWarn.gameObject);

        GameObject warnObj = new GameObject("BossWarningBanner");
        warnObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform warnRt = warnObj.AddComponent<RectTransform>();
        warnRt.anchorMin = new Vector2(0f, 0.5f);
        warnRt.anchorMax = new Vector2(1f, 0.5f);
        warnRt.pivot = new Vector2(0.5f, 0.5f);
        warnRt.anchoredPosition = new Vector2(0f, 80f);
        warnRt.sizeDelta = new Vector2(0f, 100f);

        Image warnBg = warnObj.AddComponent<Image>();
        warnBg.color = new Color(0.85f, 0.1f, 0.1f, 0.85f);

        GameObject warnTextObj = new GameObject("Text");
        warnTextObj.transform.SetParent(warnObj.transform, false);
        RectTransform wtRt = warnTextObj.AddComponent<RectTransform>();
        wtRt.anchorMin = Vector2.zero;
        wtRt.anchorMax = Vector2.one;
        wtRt.offsetMin = Vector2.zero;
        wtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI wtTmp = warnTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) wtTmp.font = font;
        wtTmp.text = "WARNING!\nCOLOSSAL BOSS APPROACHING";
        wtTmp.fontSize = 26f;
        wtTmp.alignment = TextAlignmentOptions.Center;
        wtTmp.color = Color.white;
        warnObj.SetActive(false);

        // 5. Setup BossSpawnManager
        BossSpawnManager spawnMgr = gm.GetComponent<BossSpawnManager>();
        if (spawnMgr == null) spawnMgr = gm.AddComponent<BossSpawnManager>();

        SerializedObject soSpawn = new SerializedObject(spawnMgr);
        soSpawn.FindProperty("defaultBossPrefab").objectReferenceValue = bossPrefab;
        soSpawn.FindProperty("bossWarningBanner").objectReferenceValue = warnObj;
        soSpawn.ApplyModifiedProperties();

        // 6. Setup Victory Panel
        Transform existingVic = mainCanvas.transform.Find("VictoryPanel");
        if (existingVic != null) Object.DestroyImmediate(existingVic.gameObject);

        GameObject vicObj = new GameObject("VictoryPanel");
        vicObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform vicRt = vicObj.AddComponent<RectTransform>();
        vicRt.anchorMin = Vector2.zero;
        vicRt.anchorMax = Vector2.one;
        vicRt.offsetMin = Vector2.zero;
        vicRt.offsetMax = Vector2.zero;

        Image vicBg = vicObj.AddComponent<Image>();
        vicBg.color = new Color(0.03f, 0.05f, 0.08f, 0.95f);

        // Win Banner
        GameObject winBannerObj = new GameObject("WinBanner");
        winBannerObj.transform.SetParent(vicObj.transform, false);
        RectTransform wbRt = winBannerObj.AddComponent<RectTransform>();
        wbRt.anchorMin = new Vector2(0.5f, 1f);
        wbRt.anchorMax = new Vector2(0.5f, 1f);
        wbRt.pivot = new Vector2(0.5f, 1f);
        wbRt.anchoredPosition = new Vector2(0f, -40f);
        wbRt.sizeDelta = new Vector2(400f, 250f);

        Image wbImg = winBannerObj.AddComponent<Image>();
        wbImg.sprite = winSprite;
        wbImg.preserveAspect = true;

        // Victory Title
        GameObject vicTitleObj = new GameObject("VictoryTitle");
        vicTitleObj.transform.SetParent(vicObj.transform, false);
        RectTransform vtRt = vicTitleObj.AddComponent<RectTransform>();
        vtRt.anchorMin = new Vector2(0.5f, 0.5f);
        vtRt.anchorMax = new Vector2(0.5f, 0.5f);
        vtRt.pivot = new Vector2(0.5f, 0.5f);
        vtRt.anchoredPosition = new Vector2(0f, 60f);
        vtRt.sizeDelta = new Vector2(700f, 45f);

        TextMeshProUGUI vtTmp = vicTitleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vtTmp.font = font;
        vtTmp.text = "MAP CONQUERED!";
        vtTmp.fontSize = 24f;
        vtTmp.alignment = TextAlignmentOptions.Center;
        vtTmp.color = new Color(1f, 0.85f, 0.2f, 1f);

        // Stats Container
        GameObject vicTimeObj = new GameObject("TimeText");
        vicTimeObj.transform.SetParent(vicObj.transform, false);
        RectTransform vtimeRt = vicTimeObj.AddComponent<RectTransform>();
        vtimeRt.anchorMin = new Vector2(0.5f, 0.5f);
        vtimeRt.anchorMax = new Vector2(0.5f, 0.5f);
        vtimeRt.pivot = new Vector2(0.5f, 0.5f);
        vtimeRt.anchoredPosition = new Vector2(0f, 10f);
        vtimeRt.sizeDelta = new Vector2(500f, 30f);

        TextMeshProUGUI vtimeTmp = vicTimeObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vtimeTmp.font = font;
        vtimeTmp.text = "Time: 05:00";
        vtimeTmp.fontSize = 16f;
        vtimeTmp.alignment = TextAlignmentOptions.Center;
        vtimeTmp.color = Color.white;

        GameObject vicKillsObj = new GameObject("KillsText");
        vicKillsObj.transform.SetParent(vicObj.transform, false);
        RectTransform vkRt = vicKillsObj.AddComponent<RectTransform>();
        vkRt.anchorMin = new Vector2(0.5f, 0.5f);
        vkRt.anchorMax = new Vector2(0.5f, 0.5f);
        vkRt.pivot = new Vector2(0.5f, 0.5f);
        vkRt.anchoredPosition = new Vector2(0f, -30f);
        vkRt.sizeDelta = new Vector2(500f, 30f);

        TextMeshProUGUI vkTmp = vicKillsObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vkTmp.font = font;
        vkTmp.text = "Enemies Slain: 120";
        vkTmp.fontSize = 16f;
        vkTmp.alignment = TextAlignmentOptions.Center;
        vkTmp.color = Color.white;

        GameObject vicRewardsObj = new GameObject("RewardsText");
        vicRewardsObj.transform.SetParent(vicObj.transform, false);
        RectTransform vrRt = vicRewardsObj.AddComponent<RectTransform>();
        vrRt.anchorMin = new Vector2(0.5f, 0.5f);
        vrRt.anchorMax = new Vector2(0.5f, 0.5f);
        vrRt.pivot = new Vector2(0.5f, 0.5f);
        vrRt.anchoredPosition = new Vector2(0f, -70f);
        vrRt.sizeDelta = new Vector2(500f, 30f);

        TextMeshProUGUI vrTmp = vicRewardsObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vrTmp.font = font;
        vrTmp.text = "Gold Earned: +1,000";
        vrTmp.fontSize = 18f;
        vrTmp.alignment = TextAlignmentOptions.Center;
        vrTmp.color = new Color(1f, 0.9f, 0.3f, 1f);

        // Buttons
        GameObject vicReplayBtnObj = new GameObject("ReplayButton");
        vicReplayBtnObj.transform.SetParent(vicObj.transform, false);
        RectTransform vrbRt = vicReplayBtnObj.AddComponent<RectTransform>();
        vrbRt.anchorMin = new Vector2(0.5f, 0f);
        vrbRt.anchorMax = new Vector2(0.5f, 0f);
        vrbRt.pivot = new Vector2(0.5f, 0f);
        vrbRt.anchoredPosition = new Vector2(130f, 60f);
        vrbRt.sizeDelta = new Vector2(220f, 60f);

        Image vrbImg = vicReplayBtnObj.AddComponent<Image>();
        vrbImg.color = new Color(0.2f, 0.65f, 0.3f, 1f);
        Button vicReplayBtn = vicReplayBtnObj.AddComponent<Button>();

        GameObject vrbTextObj = new GameObject("Text");
        vrbTextObj.transform.SetParent(vicReplayBtnObj.transform, false);
        RectTransform vrbtRt = vrbTextObj.AddComponent<RectTransform>();
        vrbtRt.anchorMin = Vector2.zero;
        vrbtRt.anchorMax = Vector2.one;
        vrbtRt.offsetMin = Vector2.zero;
        vrbtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI vrbtTmp = vrbTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vrbtTmp.font = font;
        vrbtTmp.text = "REPLAY";
        vrbtTmp.fontSize = 16f;
        vrbtTmp.alignment = TextAlignmentOptions.Center;
        vrbtTmp.color = Color.white;

        GameObject vicMenuBtnObj = new GameObject("MenuButton");
        vicMenuBtnObj.transform.SetParent(vicObj.transform, false);
        RectTransform vmbRt = vicMenuBtnObj.AddComponent<RectTransform>();
        vmbRt.anchorMin = new Vector2(0.5f, 0f);
        vmbRt.anchorMax = new Vector2(0.5f, 0f);
        vmbRt.pivot = new Vector2(0.5f, 0f);
        vmbRt.anchoredPosition = new Vector2(-130f, 60f);
        vmbRt.sizeDelta = new Vector2(220f, 60f);

        Image vmbImg = vicMenuBtnObj.AddComponent<Image>();
        vmbImg.color = new Color(0.35f, 0.4f, 0.5f, 1f);
        Button vicMenuBtn = vicMenuBtnObj.AddComponent<Button>();

        GameObject vmbTextObj = new GameObject("Text");
        vmbTextObj.transform.SetParent(vicMenuBtnObj.transform, false);
        RectTransform vmbtRt = vmbTextObj.AddComponent<RectTransform>();
        vmbtRt.anchorMin = Vector2.zero;
        vmbtRt.anchorMax = Vector2.one;
        vmbtRt.offsetMin = Vector2.zero;
        vmbtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI vmbtTmp = vmbTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) vmbtTmp.font = font;
        vmbtTmp.text = "MAIN MENU";
        vmbtTmp.fontSize = 16f;
        vmbtTmp.alignment = TextAlignmentOptions.Center;
        vmbtTmp.color = Color.white;

        vicObj.SetActive(false);

        // 7. Wire GameResultController
        GameResultController resultCtrl = gm.GetComponent<GameResultController>();
        if (resultCtrl == null) resultCtrl = gm.AddComponent<GameResultController>();

        SerializedObject soResult = new SerializedObject(resultCtrl);
        soResult.FindProperty("victoryPanel").objectReferenceValue = vicObj;
        soResult.FindProperty("victoryBannerImage").objectReferenceValue = wbImg;
        soResult.FindProperty("victoryTitleText").objectReferenceValue = vtTmp;
        soResult.FindProperty("victoryTimeText").objectReferenceValue = vtimeTmp;
        soResult.FindProperty("victoryKillsText").objectReferenceValue = vkTmp;
        soResult.FindProperty("victoryRewardsText").objectReferenceValue = vrTmp;
        soResult.FindProperty("victoryRestartButton").objectReferenceValue = vicReplayBtn;
        soResult.FindProperty("victoryMenuButton").objectReferenceValue = vicMenuBtn;

        // Hook GameOverPanel as defeat panel if present
        GameObject gop = GameObject.Find("GameOverPanel");
        if (gop != null)
        {
            soResult.FindProperty("defeatPanel").objectReferenceValue = gop;
        }

        soResult.ApplyModifiedProperties();

        // 8. Wire GameplaySessionManager serialized references
        SerializedObject soSession = new SerializedObject(sessionMgr);
        soSession.FindProperty("fallbackDefaultMap").objectReferenceValue = defaultMap;
        soSession.FindProperty("survivalTimerText").objectReferenceValue = timerTmp;
        soSession.FindProperty("mapNameHudText").objectReferenceValue = mnTmp;

        SerializedProperty bgProp = soSession.FindProperty("backgroundImages");
        bgProp.ClearArray();
        for (int i = 0; i < bgList.Count; i++)
        {
            bgProp.InsertArrayElementAtIndex(i);
            bgProp.GetArrayElementAtIndex(i).objectReferenceValue = bgList[i];
        }

        soSession.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[SetupSurvivorArchitecture] 2_InGame setup complete!");
    }

    [MenuItem("Tools/4. Setup Map_Selection Scene")]
    public static void SetupMapSelectionScene()
    {
        EditorSceneManager.OpenScene("Assets/AnimationUI/Demo/Scene/Map_Selection.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        SetupMainMenuScene(null, true);
        Debug.Log("[SetupSurvivorArchitecture] Map_Selection standalone scene setup complete!");
    }
}
