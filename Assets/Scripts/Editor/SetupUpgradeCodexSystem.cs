using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DhafinFawwaz.AnimationUILib;
using DhafinFawwaz.AnimationUILib.Demo;

public static class SetupUpgradeCodexSystem
{
    [MenuItem("Tools/Setup Animated Upgrade Codex Layout")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "1_1_MainMenu")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/1_1_MainMenu.unity");
            scene = EditorSceneManager.GetActiveScene();
        }

        // 1. Locate Canvas (Main Menu)
        Canvas mainCanvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name == "Canvas")
            {
                mainCanvas = c;
                break;
            }
        }

        if (mainCanvas == null)
        {
            Debug.LogError("Canvas not found in 1_1_MainMenu!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(mainCanvas.gameObject, "Setup Animated Upgrade Codex");

        // Remove old SkillCodexPanel if present
        Transform oldPanel = mainCanvas.transform.Find("SkillCodexPanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // 2. Ensure Upgrade & AnimationUIs exist in 1_1_MainMenu
        GameObject upgradeObj = null;
        GameObject animUIsObj = null;
        foreach (var r in scene.GetRootGameObjects())
        {
            if (r.name == "Upgrade") upgradeObj = r;
            if (r.name == "AnimationUIs") animUIsObj = r;
        }

        if (upgradeObj == null || animUIsObj == null)
        {
            var homeScene = EditorSceneManager.OpenScene("Assets/AnimationUI/Demo/Scene/Home.unity", OpenSceneMode.Additive);
            GameObject upSrc = null;
            GameObject animSrc = null;
            foreach (var r in homeScene.GetRootGameObjects())
            {
                if (r.name == "Upgrade") upSrc = r;
                if (r.name == "AnimationUIs") animSrc = r;
            }

            if (upgradeObj == null && upSrc != null)
            {
                upgradeObj = Object.Instantiate(upSrc);
                upgradeObj.name = "Upgrade";
                EditorSceneManager.MoveGameObjectToScene(upgradeObj, scene);
            }

            if (animUIsObj == null && animSrc != null)
            {
                animUIsObj = Object.Instantiate(animSrc);
                animUIsObj.name = "AnimationUIs";
                EditorSceneManager.MoveGameObjectToScene(animUIsObj, scene);
            }

            EditorSceneManager.CloseScene(homeScene, true);
        }

        // Clean up unused AnimationUIs children
        for (int i = animUIsObj.transform.childCount - 1; i >= 0; i--)
        {
            var child = animUIsObj.transform.GetChild(i);
            if (child.name != "Home -> Upgrade")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        GameObject h2uObj = animUIsObj.transform.Find("Home -> Upgrade")?.gameObject;
        if (h2uObj == null)
        {
            Debug.LogError("Home -> Upgrade not found in AnimationUIs!");
            return;
        }

        AnimationUI aui = h2uObj.GetComponent<AnimationUI>();

        // Load fonts & sprites
        TMP_FontAsset passionFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/AnimationUI/Demo/Font/PassionOne-Regular SDF.asset");
        Sprite parallelogram = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AnimationUI/Demo/UI/parallelogram.png");
        Sprite parallelogramFlipped = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AnimationUI/Demo/UI/parallelogramFlipped.png");

        // 3. Setup Upgrade Canvas background
        Transform bgColTr = upgradeObj.transform.Find("BGColor");
        GameObject bgColObj = (bgColTr != null) ? bgColTr.gameObject : new GameObject("BGColor");
        if (bgColTr == null)
        {
            bgColObj.transform.SetParent(upgradeObj.transform, false);
            bgColObj.transform.SetSiblingIndex(0);
            RectTransform bgRt = bgColObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            Image bgImg = bgColObj.AddComponent<Image>();
            bgImg.color = new Color(0.949f, 0.435f, 0.404f, 1f); // Same peach color as Home.unity
            bgImg.raycastTarget = false;
        }
        else
        {
            Image bgImg = bgColObj.GetComponent<Image>();
            if (bgImg != null) bgImg.raycastTarget = false;
        }

        Transform bgYellowTr = upgradeObj.transform.Find("BGYellow");
        if (bgYellowTr != null)
        {
            Image yImg = bgYellowTr.GetComponent<Image>();
            if (yImg != null) yImg.raycastTarget = false;
        }

        // 4. Setup Option Buttons (Option 1, 2, 3, Back)
        Transform opt1 = upgradeObj.transform.Find("Option 1");
        Transform opt2 = upgradeObj.transform.Find("Option 2");
        Transform opt3 = upgradeObj.transform.Find("Option 3");
        Transform back = upgradeObj.transform.Find("Back");

        if (opt1 != null) opt1.GetComponentInChildren<TMP_Text>().text = "SPELLS (5)";
        if (opt2 != null) opt2.GetComponentInChildren<TMP_Text>().text = "PASSIVES (6)";
        if (opt3 != null) opt3.GetComponentInChildren<TMP_Text>().text = "ALL (11)";
        if (back != null) back.GetComponentInChildren<TMP_Text>().text = "BACK";

        // 5. Setup the 4 Cards
        string[] cardNames = { "Upgrade1", "Upgrade1 (1)", "Upgrade1 (2)", "Upgrade1 (3)" };
        GameObject[] cardRoots = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            Transform cardTr = upgradeObj.transform.Find(cardNames[i]);
            if (cardTr == null) continue;
            cardRoots[i] = cardTr.gameObject;

            // Ensure IconSlot image exists over the cyan box (approx -135, -10, size 140x140)
            Transform iconTr = cardTr.Find("IconSlot");
            if (iconTr == null)
            {
                GameObject iconObj = new GameObject("IconSlot");
                iconObj.transform.SetParent(cardTr, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchoredPosition = new Vector2(-135f, -10f);
                irt.sizeDelta = new Vector2(136f, 136f);
                Image img = iconObj.AddComponent<Image>();
                img.preserveAspect = true;
            }

            // Style BuyButton
            Transform buyBtn = cardTr.Find("BuyButton");
            if (buyBtn != null)
            {
                var txt = buyBtn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "VIEW DETAILS";
                    txt.fontSize = 22;
                }
            }
        }

        // 6. Setup Page Navigation (below cards: centered between x=360 and x=1020 => x=690)
        Transform pageNavTr = upgradeObj.transform.Find("PageNavigation");
        GameObject pageNavObj = (pageNavTr != null) ? pageNavTr.gameObject : new GameObject("PageNavigation");
        if (pageNavTr == null)
        {
            pageNavObj.transform.SetParent(upgradeObj.transform, false);
            RectTransform pnRt = pageNavObj.AddComponent<RectTransform>();
            pnRt.anchorMin = new Vector2(0f, 0.5f);
            pnRt.anchorMax = new Vector2(0f, 0.5f);
            pnRt.pivot = new Vector2(0.5f, 0.5f);
            pnRt.anchoredPosition = new Vector2(690f, -450f);
            pnRt.sizeDelta = new Vector2(440f, 50f);
        }

        // Prev Button
        Transform prevBtnTr = pageNavObj.transform.Find("PrevButton");
        GameObject prevBtnObj = (prevBtnTr != null) ? prevBtnTr.gameObject : new GameObject("PrevButton");
        if (prevBtnTr == null)
        {
            prevBtnObj.transform.SetParent(pageNavObj.transform, false);
            RectTransform prt = prevBtnObj.AddComponent<RectTransform>();
            prt.anchoredPosition = new Vector2(-140f, 0f);
            prt.sizeDelta = new Vector2(130f, 44f);
            Image pImg = prevBtnObj.AddComponent<Image>();
            pImg.sprite = parallelogramFlipped;
            pImg.color = new Color(0.24f, 0.30f, 0.44f, 1f);
            prevBtnObj.AddComponent<Button>();

            GameObject ptxt = new GameObject("Text");
            ptxt.transform.SetParent(prevBtnObj.transform, false);
            RectTransform trt = ptxt.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var tmp = ptxt.AddComponent<TextMeshProUGUI>();
            tmp.text = "◀ PREV";
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = passionFont;
            tmp.color = Color.white;
        }

        // Next Button
        Transform nextBtnTr = pageNavObj.transform.Find("NextButton");
        GameObject nextBtnObj = (nextBtnTr != null) ? nextBtnTr.gameObject : new GameObject("NextButton");
        if (nextBtnTr == null)
        {
            nextBtnObj.transform.SetParent(pageNavObj.transform, false);
            RectTransform nrt = nextBtnObj.AddComponent<RectTransform>();
            nrt.anchoredPosition = new Vector2(140f, 0f);
            nrt.sizeDelta = new Vector2(130f, 44f);
            Image nImg = nextBtnObj.AddComponent<Image>();
            nImg.sprite = parallelogram;
            nImg.color = new Color(0.24f, 0.30f, 0.44f, 1f);
            nextBtnObj.AddComponent<Button>();

            GameObject ntxt = new GameObject("Text");
            ntxt.transform.SetParent(nextBtnObj.transform, false);
            RectTransform trt = ntxt.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var tmp = ntxt.AddComponent<TextMeshProUGUI>();
            tmp.text = "NEXT ▶";
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = passionFont;
            tmp.color = Color.white;
        }

        // Page Indicator Text
        Transform pageTextTr = pageNavObj.transform.Find("PageText");
        GameObject pageTextObj = (pageTextTr != null) ? pageTextTr.gameObject : new GameObject("PageText");
        if (pageTextTr == null)
        {
            pageTextObj.transform.SetParent(pageNavObj.transform, false);
            RectTransform ptRt = pageTextObj.AddComponent<RectTransform>();
            ptRt.anchoredPosition = Vector2.zero;
            ptRt.sizeDelta = new Vector2(140f, 44f);
            var tmp = pageTextObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "PAGE 1 / 2";
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = passionFont;
            tmp.color = new Color(1f, 0.9f, 0.3f, 1f);
        }

        // 7. Setup Detail Progression Modal inside Upgrade
        Transform modalTr = upgradeObj.transform.Find("DetailModal");
        GameObject modalObj = (modalTr != null) ? modalTr.gameObject : new GameObject("DetailModal");
        if (modalTr == null)
        {
            modalObj.transform.SetParent(upgradeObj.transform, false);
            RectTransform mrt = modalObj.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.offsetMin = Vector2.zero;
            mrt.offsetMax = Vector2.zero;

            // Dim Background
            Image dimImg = modalObj.AddComponent<Image>();
            dimImg.color = new Color(0.04f, 0.06f, 0.12f, 0.88f);

            // Container Box
            GameObject boxObj = new GameObject("Box");
            boxObj.transform.SetParent(modalObj.transform, false);
            RectTransform boxRt = boxObj.AddComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(980f, 720f);
            boxRt.anchoredPosition = Vector2.zero;

            Image boxImg = boxObj.AddComponent<Image>();
            boxImg.color = new Color(0.08f, 0.11f, 0.18f, 0.98f);
            Outline boxOutline = boxObj.AddComponent<Outline>();
            boxOutline.effectColor = new Color(0.9f, 0.72f, 0.2f, 1f);
            boxOutline.effectDistance = new Vector2(3f, -3f);

            // Header Banner
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(boxObj.transform, false);
            RectTransform hRt = headerObj.AddComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0f, 96f);
            hRt.anchoredPosition = Vector2.zero;
            Image hImg = headerObj.AddComponent<Image>();
            hImg.color = new Color(0.12f, 0.16f, 0.26f, 0.98f);

            // Icon
            GameObject iconObj = new GameObject("DetailIcon");
            iconObj.transform.SetParent(headerObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(20f, 0f);
            iconRt.sizeDelta = new Vector2(68f, 68f);
            Image dIconImg = iconObj.AddComponent<Image>();
            dIconImg.preserveAspect = true;

            // Name
            GameObject nameObj = new GameObject("DetailName");
            nameObj.transform.SetParent(headerObj.transform, false);
            RectTransform nRt = nameObj.AddComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0f, 0.5f);
            nRt.anchorMax = new Vector2(1f, 1f);
            nRt.pivot = new Vector2(0f, 1f);
            nRt.anchoredPosition = new Vector2(104f, -14f);
            nRt.sizeDelta = new Vector2(-280f, 38f);
            var dNameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            dNameTmp.fontSize = 28;
            dNameTmp.fontStyle = FontStyles.Bold;
            dNameTmp.color = new Color(1f, 0.9f, 0.35f, 1f);
            dNameTmp.font = passionFont;

            // Tag & Category
            GameObject tagObj = new GameObject("DetailTag");
            tagObj.transform.SetParent(headerObj.transform, false);
            RectTransform tRt = tagObj.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(1f, 0.5f);
            tRt.pivot = new Vector2(0f, 0f);
            tRt.anchoredPosition = new Vector2(104f, 14f);
            tRt.sizeDelta = new Vector2(-280f, 30f);
            var dTagTmp = tagObj.AddComponent<TextMeshProUGUI>();
            dTagTmp.fontSize = 18;
            dTagTmp.color = new Color(0.4f, 0.85f, 1f, 1f);
            dTagTmp.font = passionFont;

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform cRt = closeBtnObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1f, 0.5f);
            cRt.anchorMax = new Vector2(1f, 0.5f);
            cRt.pivot = new Vector2(1f, 0.5f);
            cRt.anchoredPosition = new Vector2(-16f, 0f);
            cRt.sizeDelta = new Vector2(120f, 44f);
            Image cImg = closeBtnObj.AddComponent<Image>();
            cImg.color = new Color(0.7f, 0.18f, 0.18f, 1f);
            closeBtnObj.AddComponent<Button>();

            GameObject cTxtObj = new GameObject("Text");
            cTxtObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform ctRt = cTxtObj.AddComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.sizeDelta = Vector2.zero;
            var cTmp = cTxtObj.AddComponent<TextMeshProUGUI>();
            cTmp.text = "✕ CLOSE";
            cTmp.fontSize = 18;
            cTmp.fontStyle = FontStyles.Bold;
            cTmp.alignment = TextAlignmentOptions.Center;
            cTmp.font = passionFont;
            cTmp.color = Color.white;

            // Overview Box
            GameObject ovObj = new GameObject("OverviewBox");
            ovObj.transform.SetParent(boxObj.transform, false);
            RectTransform ovRt = ovObj.AddComponent<RectTransform>();
            ovRt.anchorMin = new Vector2(0f, 1f);
            ovRt.anchorMax = new Vector2(1f, 1f);
            ovRt.pivot = new Vector2(0.5f, 1f);
            ovRt.anchoredPosition = new Vector2(0f, -112f);
            ovRt.sizeDelta = new Vector2(-40f, 76f);
            Image ovImg = ovObj.AddComponent<Image>();
            ovImg.color = new Color(0.05f, 0.08f, 0.13f, 0.95f);

            GameObject ovTxtObj = new GameObject("Text");
            ovTxtObj.transform.SetParent(ovObj.transform, false);
            RectTransform ovtRt = ovTxtObj.AddComponent<RectTransform>();
            ovtRt.anchorMin = Vector2.zero;
            ovtRt.anchorMax = Vector2.one;
            ovtRt.offsetMin = new Vector2(16f, 8f);
            ovtRt.offsetMax = new Vector2(-16f, -8f);
            var ovTmp = ovTxtObj.AddComponent<TextMeshProUGUI>();
            ovTmp.fontSize = 17;
            ovTmp.alignment = TextAlignmentOptions.MidlineLeft;
            ovTmp.color = new Color(0.78f, 0.88f, 0.98f, 1f);

            // Progression Container
            GameObject progObj = new GameObject("ProgressionContainer");
            progObj.transform.SetParent(boxObj.transform, false);
            RectTransform progRt = progObj.AddComponent<RectTransform>();
            progRt.anchorMin = Vector2.zero;
            progRt.anchorMax = Vector2.one;
            progRt.offsetMin = new Vector2(20f, 20f);
            progRt.offsetMax = new Vector2(-20f, -204f);
            VerticalLayoutGroup vlg = progObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.spacing = 8f;
        }

        modalObj.SetActive(false); // Inactive by default

        // 8. Bind SkillCodexUI component on Upgrade
        SkillCodexUI codex = upgradeObj.GetComponent<SkillCodexUI>();
        if (codex == null) codex = upgradeObj.AddComponent<SkillCodexUI>();

        codex.cardRoots = cardRoots;
        codex.optionSpellsBtn = opt1.GetComponent<ButtonUI>();
        codex.optionPassivesBtn = opt2.GetComponent<ButtonUI>();
        codex.optionAllBtn = opt3.GetComponent<ButtonUI>();
        codex.backBtn = back.GetComponent<ButtonUI>();

        codex.prevPageBtn = prevBtnObj.GetComponent<Button>();
        codex.nextPageBtn = nextBtnObj.GetComponent<Button>();
        codex.pageIndicatorText = pageTextObj.GetComponent<TMP_Text>();

        codex.detailModal = modalObj;
        codex.detailIcon = modalObj.transform.Find("Box/Header/DetailIcon")?.GetComponent<Image>();
        codex.detailNameText = modalObj.transform.Find("Box/Header/DetailName")?.GetComponent<TMP_Text>();
        codex.detailTagText = modalObj.transform.Find("Box/Header/DetailTag")?.GetComponent<TMP_Text>();
        codex.detailOverviewText = modalObj.transform.Find("Box/OverviewBox/Text")?.GetComponent<TMP_Text>();
        codex.detailLevelsContainer = modalObj.transform.Find("Box/ProgressionContainer");
        codex.detailCloseBtn = modalObj.transform.Find("Box/Header/CloseButton")?.GetComponent<Button>();
        codex.transitionUI = aui;

        codex.InitializeDefaultSkills();
        AssignSkillSprites(codex);

        // 9. Wire AnimationUI Sequences (Home -> Upgrade)
        Transform iconeTr = mainCanvas.transform.Find("Icone");
        Transform startTr = mainCanvas.transform.Find("Start");
        Transform skillTr = mainCanvas.transform.Find("Skill");
        Transform settingTr = mainCanvas.transform.Find("Setting");
        Transform quitTr = mainCanvas.transform.Find("Quit");

        bgYellowTr = upgradeObj.transform.Find("BGYellow");

        Sequence[] seqs = aui.AnimationSequence;
        if (seqs != null && seqs.Length >= 42)
        {
            // [3] Icone (slides up)
            seqs[3].TargetComp = iconeTr.GetComponent<RectTransform>();
            seqs[3].AnchoredPositionStart = new Vector3(0f, 305.77f, 0f);
            seqs[3].AnchoredPositionEnd = new Vector3(0f, 900f, 0f);

            // [4] Start (slides left)
            seqs[4].TargetComp = startTr.GetComponent<RectTransform>();
            seqs[4].AnchoredPositionStart = new Vector3(3f, -43f, 0f);
            seqs[4].AnchoredPositionEnd = new Vector3(-900f, -43f, 0f);

            // [7] Skill (slides left)
            seqs[7].TargetComp = skillTr.GetComponent<RectTransform>();
            seqs[7].AnchoredPositionStart = new Vector3(3f, -239f, 0f);
            seqs[7].AnchoredPositionEnd = new Vector3(-900f, -239f, 0f);

            // [10] Setting (slides left)
            seqs[10].TargetComp = settingTr.GetComponent<RectTransform>();
            seqs[10].AnchoredPositionStart = new Vector3(-115.24f, -411f, 0f);
            seqs[10].AnchoredPositionEnd = new Vector3(-900f, -411f, 0f);

            // [13] Quit (slides right)
            seqs[13].TargetComp = quitTr.GetComponent<RectTransform>();
            seqs[13].AnchoredPositionStart = new Vector3(126.24f, -411f, 0f);
            seqs[13].AnchoredPositionEnd = new Vector3(900f, -411f, 0f);

            // [16] SetActive Upgrade True
            seqs[16].Target = upgradeObj;
            seqs[16].IsActivating = true;

            // [17] SetActive Canvas False
            seqs[17].Target = mainCanvas.gameObject;
            seqs[17].IsActivating = false;

            // [19] BGYellow
            seqs[19].TargetComp = bgYellowTr.GetComponent<RectTransform>();

            // [22] Option 3
            seqs[22].TargetComp = opt3.GetComponent<RectTransform>();

            // [25] Option 2
            seqs[25].TargetComp = opt2.GetComponent<RectTransform>();

            // [28] Option 1
            seqs[28].TargetComp = opt1.GetComponent<RectTransform>();

            // [31] Upgrade1 (Card 0)
            seqs[31].TargetComp = cardRoots[0].GetComponent<RectTransform>();

            // [34] Upgrade1 (1) (Card 1)
            seqs[34].TargetComp = cardRoots[1].GetComponent<RectTransform>();

            // [37] Back
            seqs[37].TargetComp = back.GetComponent<RectTransform>();

            // [38] Upgrade1 (2) (Card 2)
            seqs[38].TargetComp = cardRoots[2].GetComponent<RectTransform>();

            // [41] Upgrade1 (3) (Card 3)
            seqs[41].TargetComp = cardRoots[3].GetComponent<RectTransform>();
        }

        // 10. Wire Canvas/Skill button to aui.Play()
        Button skillBtn = skillTr.GetComponent<Button>();
        if (skillBtn != null)
        {
            SerializedObject soBtn = new SerializedObject(skillBtn);
            SerializedProperty onClickProp = soBtn.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            onClickProp.ClearArray();
            onClickProp.InsertArrayElementAtIndex(0);
            SerializedProperty call = onClickProp.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = aui;
            call.FindPropertyRelative("m_MethodName").stringValue = "Play";
            call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
            call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly
            soBtn.ApplyModifiedProperties();
        }

        // 11. Clear persistent call on Back button so SkillCodexUI handles it cleanly without double-invocation
        ButtonUI backBui = back.GetComponent<ButtonUI>();
        if (backBui != null)
        {
            SerializedObject soBack = new SerializedObject(backBui);
            SerializedProperty onClickProp = soBack.FindProperty("_onClick.m_PersistentCalls.m_Calls");
            onClickProp.ClearArray();
            soBack.ApplyModifiedProperties();
        }

        // 12. Set Upgrade inactive by default so MainMenu is active initially
        upgradeObj.SetActive(false);
        mainCanvas.gameObject.SetActive(true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Successfully setup animated Upgrade Codex layout with AnimationUI!");
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
