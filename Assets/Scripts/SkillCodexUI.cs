using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DhafinFawwaz.AnimationUILib;
using DhafinFawwaz.AnimationUILib.Demo;
using UnityEngine.InputSystem;

public class SkillCodexUI : MonoBehaviour
{
    public enum SkillCategory
    {
        All,
        ActiveSpell,
        PassiveUpgrade
    }

    [System.Serializable]
    public class SkillLevelInfo
    {
        public int level;
        public string title;
        [TextArea(2, 4)] public string description;
        public bool isUltimate;
    }

    [System.Serializable]
    public class SkillData
    {
        public string id;
        public string name;
        public SkillCategory category;
        public Sprite icon;
        public string tagText;
        public int maxLevel;
        [TextArea(2, 5)] public string overview;
        public List<SkillLevelInfo> levels = new List<SkillLevelInfo>();
    }

    [Header("Upgrade Menu 4 Cards")]
    public GameObject[] cardRoots = new GameObject[4]; // Upgrade1, Upgrade1 (1), Upgrade1 (2), Upgrade1 (3)

    [Header("Category Option Buttons")]
    public ButtonUI optionSpellsBtn;   // Option 1
    public ButtonUI optionPassivesBtn; // Option 2
    public ButtonUI optionAllBtn;      // Option 3
    public ButtonUI backBtn;           // Back

    [Header("Page Navigation")]
    public Button prevPageBtn;
    public Button nextPageBtn;
    public TMP_Text pageIndicatorText;

    [Header("Detail Progression Modal")]
    public GameObject detailModal;
    public Image detailIcon;
    public TMP_Text detailNameText;
    public TMP_Text detailTagText;
    public TMP_Text detailCategoryText;
    public TMP_Text detailMaxLevelText;
    public TMP_Text detailOverviewText;
    public Transform detailLevelsContainer;
    public Button detailCloseBtn;

    [Header("AnimationUI Transition")]
    public AnimationUI transitionUI;

    [Header("Skill Database")]
    public List<SkillData> skills = new List<SkillData>();

    private SkillCategory currentCategory = SkillCategory.All;
    private int currentPage = 0;
    private const int CARDS_PER_PAGE = 4;
    private List<SkillData> filteredSkills = new List<SkillData>();
    private List<GameObject> spawnedLevelCards = new List<GameObject>();

    private static readonly Vector2 ICONE_HOME = new Vector2(0f, 305.77f);
    private static readonly Vector2 START_HOME = new Vector2(3f, -43f);
    private static readonly Vector2 SKILL_HOME = new Vector2(3f, -239f);
    private static readonly Vector2 SETTING_HOME = new Vector2(-115.24f, -411f);
    private static readonly Vector2 QUIT_HOME = new Vector2(126.24f, -411f);

    private bool isTransitioning = false;

    private Font pixelFont;
    private TMP_FontAsset passionFont;

    private void Awake()
    {
        if (skills == null || skills.Count == 0)
        {
            InitializeDefaultSkills();
        }
#if UNITY_EDITOR
        AssignDefaultSprites();
#endif

        if (optionSpellsBtn != null)
        {
            optionSpellsBtn.onClick.RemoveAllListeners();
            optionSpellsBtn.onClick.AddListener(() => SetCategory(SkillCategory.ActiveSpell));
        }
        if (optionPassivesBtn != null)
        {
            optionPassivesBtn.onClick.RemoveAllListeners();
            optionPassivesBtn.onClick.AddListener(() => SetCategory(SkillCategory.PassiveUpgrade));
        }
        if (optionAllBtn != null)
        {
            optionAllBtn.onClick.RemoveAllListeners();
            optionAllBtn.onClick.AddListener(() => SetCategory(SkillCategory.All));
        }
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackClicked);
        }

        if (prevPageBtn != null)
        {
            prevPageBtn.onClick.RemoveAllListeners();
            prevPageBtn.onClick.AddListener(PrevPage);
        }
        if (nextPageBtn != null)
        {
            nextPageBtn.onClick.RemoveAllListeners();
            nextPageBtn.onClick.AddListener(NextPage);
        }
        if (detailCloseBtn != null)
        {
            detailCloseBtn.onClick.RemoveAllListeners();
            detailCloseBtn.onClick.AddListener(CloseDetailModal);
        }
    }

    private void OnEnable()
    {
        isTransitioning = false;
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = true;
        }

        if (skills == null || skills.Count == 0)
        {
            InitializeDefaultSkills();
        }
        if (detailModal != null) detailModal.SetActive(false);
        SetCategory(currentCategory);
    }

    private void OnDisable()
    {
        isTransitioning = false;
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = true;
        }
    }

    private void Start()
    {
        FilterSkills();
        RefreshCards();
        UpdateCategoryButtonsVisual();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            OnBackClicked();
        }
    }

    public void OpenPanel()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (transitionUI != null)
        {
            transitionUI.OnAnimationEnded -= HandleOpenEnded;
            transitionUI.OnAnimationEnded += HandleOpenEnded;
            transitionUI.Play();
        }
        else
        {
            gameObject.SetActive(true);
            isTransitioning = false;
        }

        if (detailModal != null) detailModal.SetActive(false);
        SetCategory(SkillCategory.All);
    }

    private void HandleOpenEnded()
    {
        if (transitionUI != null)
        {
            transitionUI.OnAnimationEnded -= HandleOpenEnded;
        }
        isTransitioning = false;
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = true;
        }
    }

    public void OnBackClicked()
    {
        if (detailModal != null && detailModal.activeSelf)
        {
            CloseDetailModal();
            return;
        }

        if (isTransitioning) return;
        isTransitioning = true;

        if (transitionUI != null)
        {
            transitionUI.OnAnimationEnded -= HandleTransitionBackEnded;
            transitionUI.OnAnimationEnded += HandleTransitionBackEnded;
            transitionUI.PlayReversed();
        }
        else
        {
            gameObject.SetActive(false);
            EnsureMainMenuRestored();
            isTransitioning = false;
        }
    }

    private void HandleTransitionBackEnded()
    {
        if (transitionUI != null)
        {
            transitionUI.OnAnimationEnded -= HandleTransitionBackEnded;
        }

        EnsureMainMenuRestored();
        isTransitioning = false;
    }

    public void EnsureMainMenuRestored()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            canvas.SetActive(true);
            Transform t;
            if ((t = canvas.transform.Find("Icone")) != null) t.GetComponent<RectTransform>().anchoredPosition = ICONE_HOME;
            if ((t = canvas.transform.Find("Start")) != null) t.GetComponent<RectTransform>().anchoredPosition = START_HOME;
            if ((t = canvas.transform.Find("Skill")) != null) t.GetComponent<RectTransform>().anchoredPosition = SKILL_HOME;
            if ((t = canvas.transform.Find("Setting")) != null) t.GetComponent<RectTransform>().anchoredPosition = SETTING_HOME;
            if ((t = canvas.transform.Find("Quit")) != null) t.GetComponent<RectTransform>().anchoredPosition = QUIT_HOME;
        }

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = true;
        }
    }

    public void SetCategory(SkillCategory category)
    {
        currentCategory = category;
        currentPage = 0;
        FilterSkills();
        RefreshCards();
        UpdateCategoryButtonsVisual();
    }

    private void FilterSkills()
    {
        filteredSkills.Clear();
        for (int i = 0; i < skills.Count; i++)
        {
            if (currentCategory == SkillCategory.All || skills[i].category == currentCategory)
            {
                filteredSkills.Add(skills[i]);
            }
        }
    }

    public int GetMaxPage()
    {
        if (filteredSkills.Count == 0) return 0;
        return Mathf.Max(0, Mathf.CeilToInt((float)filteredSkills.Count / CARDS_PER_PAGE) - 1);
    }

    public void NextPage()
    {
        int maxPage = GetMaxPage();
        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshCards();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshCards();
        }
    }

    public void RefreshCards()
    {
        int maxPage = GetMaxPage();
        if (currentPage > maxPage) currentPage = maxPage;
        if (currentPage < 0) currentPage = 0;

        int startIndex = currentPage * CARDS_PER_PAGE;

        for (int i = 0; i < CARDS_PER_PAGE; i++)
        {
            if (i >= cardRoots.Length || cardRoots[i] == null) continue;

            int skillIndex = startIndex + i;
            if (skillIndex < filteredSkills.Count)
            {
                BindCard(cardRoots[i], filteredSkills[skillIndex]);
            }
            else
            {
                cardRoots[i].SetActive(false);
            }
        }

        // Update Page Navigation Text
        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = $"TRANG {currentPage + 1} / {maxPage + 1}";
        }
        if (prevPageBtn != null) prevPageBtn.interactable = (currentPage > 0);
        if (nextPageBtn != null) nextPageBtn.interactable = (currentPage < maxPage);
    }

    private void BindCard(GameObject cardObj, SkillData skill)
    {
        if (cardObj == null) return;
        cardObj.SetActive(true);

        // Title on red ribbon
        Transform titleTr = cardObj.transform.Find("Title");
        if (titleTr != null)
        {
            var tmp = titleTr.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = skill.name.ToUpper();
        }

        // Subtitle / Tag above icon
        Transform upTr = cardObj.transform.Find("Upgrade");
        if (upTr != null)
        {
            var tmp = upTr.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = skill.tagText;
                tmp.color = (skill.category == SkillCategory.ActiveSpell) ? new Color(0.4f, 0.85f, 1f, 1f) : new Color(0.5f, 1f, 0.6f, 1f);
            }
        }

        // Cost text -> Max Level
        Transform costTr = cardObj.transform.Find("Cost");
        if (costTr != null)
        {
            var tmp = costTr.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = $"CẤP TỐI ĐA: {skill.maxLevel}";
        }

        // Description
        Transform descTr = cardObj.transform.Find("Description");
        if (descTr != null)
        {
            var tmp = descTr.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = skill.overview;
                tmp.fontSize = 20;
            }
        }

        // Icon Slot inside portrait box
        Transform iconSlot = cardObj.transform.Find("IconSlot");
        if (iconSlot != null)
        {
            var img = iconSlot.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = skill.icon;
                img.preserveAspect = true;
                img.color = Color.white;
            }
        }

        // Details Button (formerly BuyButton)
        Transform buyBtnTr = cardObj.transform.Find("BuyButton");
        if (buyBtnTr != null)
        {
            var btnTmp = buyBtnTr.GetComponentInChildren<TMP_Text>();
            if (btnTmp != null) btnTmp.text = "CHI TIẾT";

            var btn = buyBtnTr.GetComponent<Button>();
            if (btn == null) btn = buyBtnTr.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OpenDetailModal(skill));
        }

        // Click on entire card also opens detail modal
        var cardBtn = cardObj.GetComponent<Button>();
        if (cardBtn == null) cardBtn = cardObj.AddComponent<Button>();
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.onClick.AddListener(() => OpenDetailModal(skill));
    }

    private void UpdateCategoryButtonsVisual()
    {
        Color activeTextColor = new Color(1f, 0.88f, 0.25f, 1f); // Vibrant gold
        Color inactiveTextColor = Color.white;

        Color activeBgColor = new Color(0.18f, 0.35f, 0.65f, 1f);
        Color inactiveBgColor = new Color(0.294f, 0.318f, 0.404f, 1f);

        SetOptionButtonVisual(optionSpellsBtn, currentCategory == SkillCategory.ActiveSpell, activeTextColor, inactiveTextColor, activeBgColor, inactiveBgColor);
        SetOptionButtonVisual(optionPassivesBtn, currentCategory == SkillCategory.PassiveUpgrade, activeTextColor, inactiveTextColor, activeBgColor, inactiveBgColor);
        SetOptionButtonVisual(optionAllBtn, currentCategory == SkillCategory.All, activeTextColor, inactiveTextColor, activeBgColor, inactiveBgColor);
    }

    private void SetOptionButtonVisual(ButtonUI btn, bool isActive, Color activeText, Color inactiveText, Color activeBg, Color inactiveBg)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.color = isActive ? activeText : inactiveText;

        var img = btn.GetComponent<Image>();
        if (img != null) img.color = isActive ? activeBg : inactiveBg;
    }

    public void OpenDetailModal(SkillData skill)
    {
        if (detailModal == null || skill == null) return;
        detailModal.SetActive(true);

        if (detailIcon != null)
        {
            detailIcon.sprite = skill.icon;
            detailIcon.preserveAspect = true;
        }

        if (detailNameText != null) detailNameText.text = skill.name.ToUpper();
        if (detailTagText != null) detailTagText.text = skill.tagText;

        if (detailCategoryText != null)
        {
            detailCategoryText.text = (skill.category == SkillCategory.ActiveSpell) ? "[ CHIÊU KÍCH HOẠT ]" : "[ NỘI TẠI HỖ TRỢ ]";
            detailCategoryText.color = (skill.category == SkillCategory.ActiveSpell) ?
                new Color(0.4f, 0.85f, 1f, 1f) : new Color(0.5f, 1f, 0.6f, 1f);
        }

        if (detailMaxLevelText != null) detailMaxLevelText.text = $"CẤP TỐI ĐA: {skill.maxLevel}";
        if (detailOverviewText != null) detailOverviewText.text = skill.overview;

        // Render levels
        if (detailLevelsContainer != null)
        {
            foreach (var obj in spawnedLevelCards)
            {
                if (obj != null) SafeDestroy(obj);
            }
            spawnedLevelCards.Clear();

            for (int i = 0; i < skill.levels.Count; i++)
            {
                var lvl = skill.levels[i];
                GameObject lvlCard = CreateLevelItemUI(lvl);
                spawnedLevelCards.Add(lvlCard);
            }
        }
    }

    public void CloseDetailModal()
    {
        if (detailModal != null) detailModal.SetActive(false);
    }

    private GameObject CreateLevelItemUI(SkillLevelInfo lvl)
    {
        GameObject card = new GameObject($"Level_{lvl.level}");
        card.transform.SetParent(detailLevelsContainer, false);

        RectTransform rt = card.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 62f);

        Image bg = card.AddComponent<Image>();
        bg.color = lvl.isUltimate ? new Color(0.24f, 0.16f, 0.08f, 0.95f) : new Color(0.08f, 0.11f, 0.18f, 0.92f);

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = lvl.isUltimate ? new Color(1f, 0.75f, 0.15f, 0.95f) : new Color(0.20f, 0.28f, 0.42f, 0.7f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Badge Level Text (Left)
        GameObject badgeObj = new GameObject("Badge");
        badgeObj.transform.SetParent(card.transform, false);
        RectTransform badgeRt = badgeObj.AddComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0f, 0.5f);
        badgeRt.anchorMax = new Vector2(0f, 0.5f);
        badgeRt.pivot = new Vector2(0f, 0.5f);
        badgeRt.anchoredPosition = new Vector2(12f, 0f);
        badgeRt.sizeDelta = new Vector2(110f, 44f);

        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = lvl.isUltimate ? new Color(0.85f, 0.55f, 0.08f, 0.95f) : new Color(0.14f, 0.20f, 0.32f, 0.95f);

        GameObject badgeTextObj = new GameObject("BadgeText");
        badgeTextObj.transform.SetParent(badgeObj.transform, false);
        RectTransform btRt = badgeTextObj.AddComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.offsetMin = Vector2.zero;
        btRt.offsetMax = Vector2.zero;

        var badgeTmp = badgeTextObj.AddComponent<TextMeshProUGUI>();
        badgeTmp.fontSize = 20;
        badgeTmp.fontStyle = FontStyles.Bold;
        badgeTmp.alignment = TextAlignmentOptions.Center;
        badgeTmp.color = lvl.isUltimate ? Color.black : Color.white;
        badgeTmp.text = lvl.isUltimate ? "LV.5 ★" : $"LV. {lvl.level}";

        // Description Text (Right)
        GameObject descObj = new GameObject("Desc");
        descObj.transform.SetParent(card.transform, false);
        RectTransform descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 0f);
        descRt.anchorMax = new Vector2(1f, 1f);
        descRt.offsetMin = new Vector2(136f, 6f);
        descRt.offsetMax = new Vector2(-12f, -6f);

        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.fontSize = 18;
        descTmp.lineSpacing = 1.15f;
        descTmp.alignment = TextAlignmentOptions.MidlineLeft;
        descTmp.color = lvl.isUltimate ? new Color(1f, 0.92f, 0.5f, 1f) : new Color(0.85f, 0.90f, 0.98f, 1f);
        descTmp.text = lvl.description;

        return card;
    }

    private void SafeDestroy(GameObject obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    public void InitializeDefaultSkills()
    {
        skills.Clear();

        // 1. Magic Bolt
        var mb = new SkillData
        {
            id = "magic_bolt",
            name = "Tia Sét Ma Pháp",
            category = SkillCategory.ActiveSpell,
            tagText = "[ KÍCH HOẠT - TIA SÉT BÍ THUẬT ]",
            maxLevel = 5,
            overview = "Bắn các tia sét bí thuật tốc độ cao vào kẻ địch gần nhất. Đạn xuyên phá mục tiêu và đạt khả năng xuyên nhiều quái khi nâng cấp tối đa.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Mở khóa", description = "Bắn 1 Tia Sét Ma Pháp gây 20 sát thương bí thuật. Hồi chiêu: 0.8s.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cường hóa", description = "+10 Sát thương Tia Sét Ma Pháp và +15% tốc độ bay của đạn.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Niệm nhanh", description = "Giảm thời gian hồi chiêu còn 0.55s (tốc độ bắn nhanh hơn 30%).", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Bắn ba", description = "Bắn 3 Tia Sét Ma Pháp cùng lúc theo góc tỏa phía trước.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Quá tải bí thuật", description = "TỐI THƯỢNG: Đạn xuyên qua 2 kẻ địch và tăng thêm +13 sát thương!", isUltimate = true }
            }
        };
        skills.Add(mb);

        // 2. Fireball
        var fb = new SkillData
        {
            id = "fireball",
            name = "Quả Cầu Lửa",
            category = SkillCategory.ActiveSpell,
            tagText = "[ KÍCH HOẠT - SÁT THƯƠNG DIỆN RỘNG ]",
            maxLevel = 5,
            overview = "Phóng quả cầu lửa khổng lồ phát nổ khi va chạm, hủy diệt hàng loạt quái vật bằng sát thương diện rộng uy lực.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Mở khóa", description = "Phóng 1 quả cầu lửa phát nổ gây 45 sát thương diện rộng. Hồi chiêu: 1.8s.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Bán kính nổ", description = "+20 Sát thương nổ và +30% bán kính phát nổ.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Song hỏa", description = "Phóng 2 Cầu Lửa đồng thời vào các vị trí mục tiêu khác nhau.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Gia tốc", description = "Giảm thời gian hồi chiêu xuống 1.35s và +15 sát thương.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Hỏa ngục bùng nổ", description = "TỐI THƯỢNG: Phóng 3 Cầu Lửa, 110 sát thương cùng bán kính nổ rung chuyển màn hình!", isUltimate = true }
            }
        };
        skills.Add(fb);

        // 3. Ice Spear
        var is_ = new SkillData
        {
            id = "ice_spear",
            name = "Băng Thương",
            category = SkillCategory.ActiveSpell,
            tagText = "[ KÍCH HOẠT - XUYÊN THẦU & LÀM CHẬM ]",
            maxLevel = 5,
            overview = "Ngưng tụ những ngọn giáo băng sắc nhọn xuyên qua toàn bộ kẻ địch trên đường bay và làm chậm tốc độ chạy của chúng.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Mở khóa", description = "Bắn giáo băng xuyên qua mọi mục tiêu, gây 35 sát thương và làm chậm 40%.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Tê cóng", description = "+16 Sát thương và giảm thời gian hồi chiêu xuống 1.9s.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Song thương", description = "Bắn 2 Băng Thương theo góc tỏa đôi.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Băng vĩnh cửu", description = "Làm chậm tăng lên 60% với thời gian kéo dài 3.5s.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Gai băng cực đại", description = "TỐI THƯỢNG: Bắn 3 Băng Thương, đóng băng hoàn toàn kẻ địch khi trúng đòn!", isUltimate = true }
            }
        };
        skills.Add(is_);

        // 4. Dark Orb
        var _do = new SkillData
        {
            id = "dark_orb",
            name = "Cầu Bóng Tối",
            category = SkillCategory.ActiveSpell,
            tagText = "[ KÍCH HOẠT - VÒNG QUANH PHÒNG THỦ ]",
            maxLevel = 5,
            overview = "Triệu hồi các khối cầu hắc ám bay quanh Pháp Sư, tạo nên vòng bảo hộ liên tục nghiền nát mọi kẻ địch áp sát.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Mở khóa", description = "Triệu hồi 1 cầu bóng tối bay quanh bạn, liên tục gây 25 sát thương va chạm.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Động năng", description = "+1 Cầu Bóng Tối (tổng cộng 2 cầu) và +25% tốc độ xoay quanh.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Uy lực hư không", description = "+1 Cầu Bóng Tối (tổng cộng 3 cầu) và +10 sát thương mỗi cú chạm.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Mở rộng quỹ đạo", description = "+1 Cầu Bóng Tối (tổng cộng 4 cầu) và +20% bán kính xoay.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Vành đai hư không", description = "TỐI THƯỢNG: 5 Cầu Bóng Tối xoay vũ bão, gây 55 sát thương va chạm!", isUltimate = true }
            }
        };
        skills.Add(_do);

        // 5. Magic Shield
        var ms = new SkillData
        {
            id = "magic_shield",
            name = "Khiên Hộ Thể",
            category = SkillCategory.ActiveSpell,
            tagText = "[ KÍCH HOẠT - LỚP KHIÊN & SÓNG NỔ ]",
            maxLevel = 5,
            overview = "Hiện thực hóa lớp khiên bảo vệ giúp hấp thụ sát thương. Tự hồi phục đầy máu sau khi rời khỏi nguy hiểm vài giây.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Mở khóa", description = "Tạo lớp khiên hấp thụ tới 35 sát thương. Hồi lại sau 8s rời giao tranh.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Gia cố", description = "Độ bền khiên tăng lên 60 Máu và giảm thời gian hồi còn 6s.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Sóng xung kích", description = "Sóng Nổ Khiên: Khi vỡ, phát nổ đẩy lùi và gây sát thương quái xung quanh.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Thành lũy", description = "Độ bền khiên tăng lên 125 Máu và giảm thời gian hồi phục còn 5s.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Thần khiên Aegis", description = "TỐI THƯỢNG: Khiên 170 Máu và giảm vĩnh viễn 15% mọi sát thương nhận vào!", isUltimate = true }
            }
        };
        skills.Add(ms);

        // 6. Might (Arcane Tome)
        var might = new SkillData
        {
            id = "might",
            name = "Cổ Thư Bí Thuật",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ NỘI TẠI - SÁT THƯƠNG PHÉP ]",
            maxLevel = 5,
            overview = "Cổ thư ma pháp đánh thức sức mạnh tiềm ẩn, tăng phần trăm sát thương vĩnh viễn cho tất cả các chiêu thức.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Cấp 1", description = "+20% Sát thương phép cho toàn bộ kỹ năng đang dùng.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cấp 2", description = "+40% Sát thương phép (cộng dồn).", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Cấp 3", description = "+60% Sát thương phép (cộng dồn).", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Cấp 4", description = "+80% Sát thương phép (cộng dồn).", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Đại pháp sư", description = "CẤP TỐI ĐA: +100% Sát thương phép (nhân đôi toàn bộ hỏa lực ma pháp)!", isUltimate = true }
            }
        };
        skills.Add(might);

        // 7. Swiftness (Hermes Boots)
        var swift = new SkillData
        {
            id = "swiftness",
            name = "Giày Thần Tốc",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ NỘI TẠI - TỐC ĐỘ DI CHUYỂN ]",
            maxLevel = 5,
            overview = "Đôi giày thần thánh ban tặng bước chân thanh thoát, giúp Pháp Sư dễ dàng lướt qua bầy quái vật đông đúc.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Cấp 1", description = "+15% Tốc độ di chuyển cho nhân vật.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cấp 2", description = "+30% Tốc độ di chuyển.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Cấp 3", description = "+45% Tốc độ di chuyển.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Cấp 4", description = "+60% Tốc độ di chuyển.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Thần phong lướt gió", description = "CẤP TỐI ĐA: +75% Tốc độ di chuyển, bỏ xa mọi đàn quái trong nháy mắt!", isUltimate = true }
            }
        };
        skills.Add(swift);

        // 8. Haste (Spell Haste)
        var haste = new SkillData
        {
            id = "haste",
            name = "Gia Tốc Niệm",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ NỘI TẠI - GIẢM THỜI GIAN HỒI ]",
            maxLevel = 5,
            overview = "Khai thông kinh mạch bí thuật giúp hồi chiêu thần tốc, xả đạn phép liên hoàn như một cơn bão ma pháp không ngừng nghỉ.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Cấp 1", description = "-12% Thời gian hồi chiêu cho tất cả phép thuật.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cấp 2", description = "-24% Thời gian hồi chiêu cho tất cả phép thuật.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Cấp 3", description = "-36% Thời gian hồi chiêu cho tất cả phép thuật.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Cấp 4", description = "-48% Thời gian hồi chiêu cho tất cả phép thuật.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Cơn bão cuồng nộ", description = "CẤP TỐI ĐA: -60% Thời gian hồi chiêu, bắn phép liên tục không ngừng!", isUltimate = true }
            }
        };
        skills.Add(haste);

        // 9. Vitality (Vitality Heart)
        var vit = new SkillData
        {
            id = "vitality",
            name = "Trái Tim Sức Sống",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ NỘI TẠI - MÁU TỐI ĐA ]",
            maxLevel = 5,
            overview = "Truyền sinh mệnh thuần khiết vào Pháp Sư, gia tăng lượng máu tối đa và hồi phục vết thương trong trận chiến.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Cấp 1", description = "+25 Máu tối đa và hồi ngay 25 Máu.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cấp 2", description = "+50 Máu tối đa và hồi ngay 25 Máu.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Cấp 3", description = "+75 Máu tối đa và hồi ngay 25 Máu.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Cấp 4", description = "+100 Máu tối đa và hồi ngay 25 Máu.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Trái tim bất tử", description = "CẤP TỐI ĐA: +125 Máu tối đa (đạt 225 tổng Máu) tạo sức bền kinh ngạc!", isUltimate = true }
            }
        };
        skills.Add(vit);

        // 10. Magnet (Magnetic Pendant)
        var mag = new SkillData
        {
            id = "magnet",
            name = "Mặt Dây Nam Châm",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ NỘI TẠI - BÁN KÍNH HÚT KINH NGHIỆM ]",
            maxLevel = 5,
            overview = "Cổ vật từ tính phát ra từ trường hút toàn bộ ngọc kinh nghiệm rơi trên chiến trường về phía Pháp Sư.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Cấp 1", description = "+45% Bán kính hút ngọc kinh nghiệm.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Cấp 2", description = "+90% Bán kính hút ngọc kinh nghiệm.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Cấp 3", description = "+135% Bán kính hút ngọc kinh nghiệm.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Cấp 4", description = "+180% Bán kính hút ngọc kinh nghiệm.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Hút vũ trụ", description = "CẤP TỐI ĐA: +225% Bán kính hút, thu thập mọi viên ngọc trên chiến trường!", isUltimate = true }
            }
        };
        skills.Add(mag);

        // 11. Health Potion
        var pot = new SkillData
        {
            id = "health_potion",
            name = "Bình Dược Liệu",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ HỖ TRỢ - HỒI MÁU TỨC THÌ ]",
            maxLevel = 1,
            overview = "Bình thuốc hồi phục khẩn cấp xuất hiện khi bị thương. Dùng ngay để hồi máu và tiếp tục chiến đấu.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Hồi máu khẩn cấp", description = "Hồi phục ngay 35 Máu khi chọn.", isUltimate = false }
            }
        };
        skills.Add(pot);
#if UNITY_EDITOR
        AssignDefaultSprites();
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Assign Default Skill Sprites")]
    public void AssignDefaultSprites()
    {
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

        if (skills != null)
        {
            foreach (var s in skills)
            {
                if (s != null && map.ContainsKey(s.id))
                {
                    var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(map[s.id]);
                    if (sp != null) s.icon = sp;
                }
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
