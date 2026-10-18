using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DhafinFawwaz.AnimationUILib;
using DhafinFawwaz.AnimationUILib.Demo;

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
        if (Input.GetKeyDown(KeyCode.Escape))
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
            pageIndicatorText.text = $"PAGE {currentPage + 1} / {maxPage + 1}";
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
            if (tmp != null) tmp.text = $"MAX LV. {skill.maxLevel}";
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
            if (btnTmp != null) btnTmp.text = "DETAILS";

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
            detailCategoryText.text = (skill.category == SkillCategory.ActiveSpell) ? "[ ACTIVE SPELL ]" : "[ PASSIVE ABILITY ]";
            detailCategoryText.color = (skill.category == SkillCategory.ActiveSpell) ?
                new Color(0.4f, 0.85f, 1f, 1f) : new Color(0.5f, 1f, 0.6f, 1f);
        }

        if (detailMaxLevelText != null) detailMaxLevelText.text = $"MAX LEVEL: {skill.maxLevel}";
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
            name = "Magic Bolt",
            category = SkillCategory.ActiveSpell,
            tagText = "[ ACTIVE - ARCANE BOLTS ]",
            maxLevel = 5,
            overview = "Fires rapid piercing arcane bolts at the nearest enemy. High velocity projectile that shreds single targets and gains multi-enemy piercing upon reaching ultimate evolution.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Unlock", description = "Fires 1 Magic Bolt dealing 20 arcane damage. Cooldown: 0.8s.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Empower", description = "+10 Magic Bolt Damage and +15% projectile velocity.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Quickcast", description = "Reduces cooldown to 0.55s (30% faster fire rate).", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Triple Spread", description = "Fires 3 Magic Bolts simultaneously in a forward spread angle.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Arcane Overload", description = "ULTIMATE: Bolts pierce through 2 enemies and gain +13 bonus Damage!", isUltimate = true }
            }
        };
        skills.Add(mb);

        // 2. Fireball
        var fb = new SkillData
        {
            id = "fireball",
            name = "Fireball",
            category = SkillCategory.ActiveSpell,
            tagText = "[ ACTIVE - AREA OF EFFECT ]",
            maxLevel = 5,
            overview = "Hurls massive explosive fireballs that detonate on impact, decimating clusters of enemies with devastating area-of-effect damage.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Unlock", description = "Launches an explosive fireball dealing 45 AoE damage. Cooldown: 1.8s.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Blast Radius", description = "+20 Explosion Damage and +30% detonation radius.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Twin Flames", description = "Fires 2 Fireballs simultaneously at different target locations.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Accelerate", description = "Reduces cooldown to 1.35s and adds +15 Damage.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Inferno Blast", description = "ULTIMATE: Launches 3 Fireballs, 110 Damage, with a massive screen-shattering blast radius!", isUltimate = true }
            }
        };
        skills.Add(fb);

        // 3. Ice Spear
        var is_ = new SkillData
        {
            id = "ice_spear",
            name = "Ice Spear",
            category = SkillCategory.ActiveSpell,
            tagText = "[ ACTIVE - PIERCE & SLOW ]",
            maxLevel = 5,
            overview = "Condenses sharp glacial spikes that pierce through all enemies in their trajectory, heavily slowing down their movement speed.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Unlock", description = "Fires an ice spear that pierces all foes, dealing 35 damage and slowing by 40%.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Frostbite", description = "+16 Damage and reduces cooldown to 1.9s.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Twin Shards", description = "Fires 2 Ice Spears in twin spread angles.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Permafrost", description = "Slow increased to 60% with extended 3.5s freeze duration.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Glacial Spike", description = "ULTIMATE: Fires 3 Ice Spears, Freezes enemies completely solid on contact!", isUltimate = true }
            }
        };
        skills.Add(is_);

        // 4. Dark Orb
        var _do = new SkillData
        {
            id = "dark_orb",
            name = "Dark Orb",
            category = SkillCategory.ActiveSpell,
            tagText = "[ ACTIVE - ORBITAL DEFENSE ]",
            maxLevel = 5,
            overview = "Summons shadowy void spheres orbiting around the Wizard, creating a lethal perimeter that continuously shreds approaching enemies.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Unlock", description = "Summons 1 dark orb orbiting you, dealing 25 contact damage repeatedly.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Momentum", description = "+1 Dark Orb (2 total) and +25% orbit rotation speed.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Void Power", description = "+1 Dark Orb (3 total) and +10 damage per impact.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Expanding Orbit", description = "+1 Dark Orb (4 total) and +20% orbit radius.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Void Ring", description = "ULTIMATE: 5 Dark Orbs spinning furiously, dealing 55 contact damage!", isUltimate = true }
            }
        };
        skills.Add(_do);

        // 5. Magic Shield
        var ms = new SkillData
        {
            id = "magic_shield",
            name = "Magic Shield",
            category = SkillCategory.ActiveSpell,
            tagText = "[ ACTIVE - BARRIER & SHOCKWAVE ]",
            maxLevel = 5,
            overview = "Manifests a protective barrier that absorbs incoming damage. Regenerates back to full health after staying out of danger for several seconds.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Unlock", description = "Creates a barrier absorbing up to 35 damage. Recharges after 8s out of combat.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Reinforce", description = "Shield durability increased to 60 HP and recharge delay reduced to 6s.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Shockwave", description = "Shield Shockwave: Emits an explosive blast that knocks back and damages nearby foes when broken.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Bulwark", description = "Shield durability increased to 125 HP and recharge delay reduced to 5s.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Aegis Barrier", description = "ULTIMATE: 170 HP Shield and permanently grants 15% passive damage reduction!", isUltimate = true }
            }
        };
        skills.Add(ms);

        // 6. Might (Arcane Tome)
        var might = new SkillData
        {
            id = "might",
            name = "Might",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ PASSIVE - MAGIC DAMAGE ]",
            maxLevel = 5,
            overview = "An ancient arcane tome that awakens latent magical power, granting a permanent percentage bonus to all spell damage.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Rank 1", description = "+20% Magic Damage for all active spells.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Rank 2", description = "+40% Magic Damage (cumulative).", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Rank 3", description = "+60% Magic Damage (cumulative).", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Rank 4", description = "+80% Magic Damage (cumulative).", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Archmage Mastery", description = "MAX LEVEL: +100% Magic Damage (doubles total spell power output)!", isUltimate = true }
            }
        };
        skills.Add(might);

        // 7. Swiftness (Hermes Boots)
        var swift = new SkillData
        {
            id = "swiftness",
            name = "Swiftness",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ PASSIVE - MOVEMENT SPEED ]",
            maxLevel = 5,
            overview = "Mythical winged boots that bestow light, swift strides, allowing the Wizard to outmaneuver swarming enemy hordes.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Rank 1", description = "+15% Movement Speed for the character.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Rank 2", description = "+30% Movement Speed.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Rank 3", description = "+45% Movement Speed.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Rank 4", description = "+60% Movement Speed.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Zephyr Agility", description = "MAX LEVEL: +75% Movement Speed to outrun any swarm in the blink of an eye!", isUltimate = true }
            }
        };
        skills.Add(swift);

        // 8. Haste (Spell Haste)
        var haste = new SkillData
        {
            id = "haste",
            name = "Spell Haste",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ PASSIVE - COOLDOWN REDUCTION ]",
            maxLevel = 5,
            overview = "Unblocks arcane meridians to reduce spell recovery times, unleashing spells like a relentless magical tempest.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Rank 1", description = "-12% Cooldown across all spells.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Rank 2", description = "-24% Cooldown across all spells.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Rank 3", description = "-36% Cooldown across all spells.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Rank 4", description = "-48% Cooldown across all spells.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Rapid Tempest", description = "MAX LEVEL: -60% Cooldown Reduction for non-stop barrage of magic!", isUltimate = true }
            }
        };
        skills.Add(haste);

        // 9. Vitality (Vitality Heart)
        var vit = new SkillData
        {
            id = "vitality",
            name = "Vitality",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ PASSIVE - MAXIMUM HEALTH ]",
            maxLevel = 5,
            overview = "Infuses the Wizard with sacred life essence, boosting maximum health and instantly healing wounds during combat.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Rank 1", description = "+25 Max HP and immediately heals 25 HP.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Rank 2", description = "+50 Max HP and immediately heals 25 HP.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Rank 3", description = "+75 Max HP and immediately heals 25 HP.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Rank 4", description = "+100 Max HP and immediately heals 25 HP.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Immortal Core", description = "MAX LEVEL: +125 Max HP (reaching 225 total HP) for incredible endurance!", isUltimate = true }
            }
        };
        skills.Add(vit);

        // 10. Magnet (Magnetic Pendant)
        var mag = new SkillData
        {
            id = "magnet",
            name = "Magnet",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ PASSIVE - EXP PICKUP RANGE ]",
            maxLevel = 5,
            overview = "An enchanted magnetic relic that radiates an attracting field, pulling distant fallen experience gems straight to the Wizard.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Rank 1", description = "+45% EXP Gem pickup radius.", isUltimate = false },
                new SkillLevelInfo { level = 2, title = "Rank 2", description = "+90% EXP Gem pickup radius.", isUltimate = false },
                new SkillLevelInfo { level = 3, title = "Rank 3", description = "+135% EXP Gem pickup radius.", isUltimate = false },
                new SkillLevelInfo { level = 4, title = "Rank 4", description = "+180% EXP Gem pickup radius.", isUltimate = false },
                new SkillLevelInfo { level = 5, title = "Cosmic Pull", description = "MAX LEVEL: +225% Pickup Range to vacuum every single gem across the battlefield!", isUltimate = true }
            }
        };
        skills.Add(mag);

        // 11. Health Potion
        var pot = new SkillData
        {
            id = "health_potion",
            name = "Health Potion",
            category = SkillCategory.PassiveUpgrade,
            tagText = "[ SUPPORT - INSTANT HEAL ]",
            maxLevel = 1,
            overview = "An emergency restorative potion that appears when wounded. Drink immediately to recover health and keep fighting.",
            levels = new List<SkillLevelInfo>
            {
                new SkillLevelInfo { level = 1, title = "Emergency Heal", description = "Restores 35 HP immediately upon selection.", isUltimate = false }
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
