using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeCategory
{
    NewSpell,
    SpellUpgrade,
    PassiveStat,
    Consumable
}

[System.Serializable]
public class UpgradeOption
{
    public string id;
    public string title;
    public string subtitle;
    public string description;
    public UpgradeCategory category;
    public int currentLevel;
    public int nextLevel;
    public int maxLevel = 5;
    public Sprite icon;
    public Action onSelected;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public bool IsActive => levelUpUI != null && levelUpUI.IsShowing;

    [Header("UI Reference")]
    [SerializeField] private LevelUpUI levelUpUI;

    // References to player components
    private PlayerShooter playerShooter;
    private FireballSpell fireballSpell;
    private IceSpearSpell iceSpearSpell;
    private DarkOrbSpell darkOrbSpell;
    private MagicShield magicShield;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;

    // Passive ranks
    private int arcaneTomeRank = 0;
    private int hermesBootsRank = 0;
    private int spellHasteRank = 0;
    private int vitalityHeartRank = 0;
    private int magneticPendantRank = 0;

    [Header("Upgrade Icons")]
    [SerializeField] private Sprite iconBolt;
    [SerializeField] private Sprite iconFire;
    [SerializeField] private Sprite iconIce;
    [SerializeField] private Sprite iconOrb;
    [SerializeField] private Sprite iconShield;
    [SerializeField] private Sprite iconTome;
    [SerializeField] private Sprite iconBoots;
    [SerializeField] private Sprite iconHaste;
    [SerializeField] private Sprite iconHeart;
    [SerializeField] private Sprite iconMagnet;
    [SerializeField] private Sprite iconPotion;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadIcons();
    }

    private void Start()
    {
        FindPlayerComponents();
        if (levelUpUI == null)
        {
            levelUpUI = FindFirstObjectByType<LevelUpUI>();
        }
    }

    private void FindPlayerComponents()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            playerShooter = player.GetComponent<PlayerShooter>();
            playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = player.AddComponent<PlayerStats>();

            playerHealth = player.GetComponent<PlayerHealth>();

            // Spells can be on player
            fireballSpell = player.GetComponent<FireballSpell>();
            if (fireballSpell == null) fireballSpell = player.AddComponent<FireballSpell>();

            iceSpearSpell = player.GetComponent<IceSpearSpell>();
            if (iceSpearSpell == null) iceSpearSpell = player.AddComponent<IceSpearSpell>();

            darkOrbSpell = player.GetComponent<DarkOrbSpell>();
            if (darkOrbSpell == null) darkOrbSpell = player.AddComponent<DarkOrbSpell>();

            magicShield = player.GetComponent<MagicShield>();
            if (magicShield == null) magicShield = player.AddComponent<MagicShield>();
        }
    }

    private void LoadIcons()
    {
#if UNITY_EDITOR
        if (iconBolt == null) iconBolt = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magic Bolt.png");
        if (iconFire == null) iconFire = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Fireball.png");
        if (iconIce == null) iconIce = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Ice Spear.png");
        if (iconOrb == null) iconOrb = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Dark Orb.png");
        if (iconShield == null) iconShield = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magic Shield.png");
        if (iconTome == null) iconTome = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Might.png");
        if (iconBoots == null) iconBoots = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Swiftness.png");
        if (iconHaste == null) iconHaste = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Spell Haste.png");
        if (iconHeart == null) iconHeart = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Vitality.png");
        if (iconMagnet == null) iconMagnet = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magnet.png");
        if (iconPotion == null) iconPotion = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Health Potion.png");
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Assign New Skill Icons")]
    public void AssignNewIcons()
    {
        iconBolt = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magic Bolt.png");
        iconFire = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Fireball.png");
        iconIce = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Ice Spear.png");
        iconOrb = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Dark Orb.png");
        iconShield = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magic Shield.png");
        iconTome = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Might.png");
        iconBoots = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Swiftness.png");
        iconHaste = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Spell Haste.png");
        iconHeart = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Vitality.png");
        iconMagnet = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Magnet.png");
        iconPotion = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Skill/Health Potion.png");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public void ShowLevelUpSelection()
    {
        FindPlayerComponents();
        List<UpgradeOption> available = GetAvailableUpgrades();

        // Pick 3 random distinct upgrades
        List<UpgradeOption> choices = new List<UpgradeOption>();
        while (choices.Count < 3 && available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            choices.Add(available[idx]);
            available.RemoveAt(idx);
        }

        // Fallback consumables if pool exhausted
        while (choices.Count < 3)
        {
            choices.Add(GetFallbackHealOption());
        }

        if (levelUpUI != null)
        {
            levelUpUI.Show(choices);
        }
    }

    public void OnUpgradeChosen(UpgradeOption option)
    {
        option.onSelected?.Invoke();

        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.ConsumePendingLevelUp();
            if (PlayerLevelSystem.Instance.PendingLevelUps > 0)
            {
                // Another level up queued
                ShowLevelUpSelection();
                return;
            }
        }

        if (levelUpUI != null)
        {
            levelUpUI.Hide();
        }
    }

    private List<UpgradeOption> GetAvailableUpgrades()
    {
        List<UpgradeOption> list = new List<UpgradeOption>();

        // 1. Magic Bolt
        if (playerShooter != null && playerShooter.CurrentLevel < playerShooter.MaxLevel)
        {
            int next = playerShooter.CurrentLevel + 1;
            list.Add(new UpgradeOption
            {
                id = "magic_bolt",
                title = "Tia Sét Ma Pháp",
                subtitle = "NÂNG CẤP CHIÊU",
                category = UpgradeCategory.SpellUpgrade,
                currentLevel = playerShooter.CurrentLevel,
                nextLevel = next,
                icon = iconBolt,
                description = GetMagicBoltDesc(next),
                onSelected = () => playerShooter.UpgradeMagicBolt()
            });
        }

        // 2. Fireball
        if (fireballSpell != null && fireballSpell.CurrentLevel < fireballSpell.MaxLevel)
        {
            int next = fireballSpell.CurrentLevel + 1;
            bool isNew = fireballSpell.CurrentLevel == 0;
            list.Add(new UpgradeOption
            {
                id = "fireball",
                title = "Quả Cầu Lửa",
                subtitle = isNew ? "CHIÊU MỚI" : "NÂNG CẤP CHIÊU",
                category = isNew ? UpgradeCategory.NewSpell : UpgradeCategory.SpellUpgrade,
                currentLevel = fireballSpell.CurrentLevel,
                nextLevel = next,
                icon = iconFire,
                description = GetFireballDesc(next),
                onSelected = () => fireballSpell.UnlockOrUpgrade()
            });
        }

        // 3. Ice Spear
        if (iceSpearSpell != null && iceSpearSpell.CurrentLevel < iceSpearSpell.MaxLevel)
        {
            int next = iceSpearSpell.CurrentLevel + 1;
            bool isNew = iceSpearSpell.CurrentLevel == 0;
            list.Add(new UpgradeOption
            {
                id = "ice_spear",
                title = "Băng Thương",
                subtitle = isNew ? "CHIÊU MỚI" : "NÂNG CẤP CHIÊU",
                category = isNew ? UpgradeCategory.NewSpell : UpgradeCategory.SpellUpgrade,
                currentLevel = iceSpearSpell.CurrentLevel,
                nextLevel = next,
                icon = iconIce,
                description = GetIceSpearDesc(next),
                onSelected = () => iceSpearSpell.UnlockOrUpgrade()
            });
        }

        // 4. Dark Orb
        if (darkOrbSpell != null && darkOrbSpell.CurrentLevel < darkOrbSpell.MaxLevel)
        {
            int next = darkOrbSpell.CurrentLevel + 1;
            bool isNew = darkOrbSpell.CurrentLevel == 0;
            list.Add(new UpgradeOption
            {
                id = "dark_orb",
                title = "Cầu Bóng Tối",
                subtitle = isNew ? "CHIÊU MỚI" : "NÂNG CẤP CHIÊU",
                category = isNew ? UpgradeCategory.NewSpell : UpgradeCategory.SpellUpgrade,
                currentLevel = darkOrbSpell.CurrentLevel,
                nextLevel = next,
                icon = iconOrb,
                description = GetDarkOrbDesc(next),
                onSelected = () => darkOrbSpell.UnlockOrUpgrade()
            });
        }

        // 5. Magic Shield
        if (magicShield != null && magicShield.CurrentLevel < magicShield.MaxLevel)
        {
            int next = magicShield.CurrentLevel + 1;
            bool isNew = magicShield.CurrentLevel == 0;
            list.Add(new UpgradeOption
            {
                id = "magic_shield",
                title = "Khiên Hộ Thể",
                subtitle = isNew ? "CHIÊU MỚI" : "NÂNG CẤP CHIÊU",
                category = isNew ? UpgradeCategory.NewSpell : UpgradeCategory.SpellUpgrade,
                currentLevel = magicShield.CurrentLevel,
                nextLevel = next,
                icon = iconShield,
                description = GetMagicShieldDesc(next),
                onSelected = () => magicShield.UnlockOrUpgrade()
            });
        }

        // 6. Arcane Tome (Passive)
        if (arcaneTomeRank < 5)
        {
            int next = arcaneTomeRank + 1;
            list.Add(new UpgradeOption
            {
                id = "arcane_tome",
                title = "Cổ Thư Bí Thuật",
                subtitle = "NỘI TẠI",
                category = UpgradeCategory.PassiveStat,
                currentLevel = arcaneTomeRank,
                nextLevel = next,
                icon = iconTome,
                description = "+20% Sát thương ma pháp cho tất cả kỹ năng đang dùng.",
                onSelected = () =>
                {
                    arcaneTomeRank++;
                    if (playerStats != null) playerStats.AddDamageMultiplier(0.20f);
                }
            });
        }

        // 7. Hermes Boots (Passive)
        if (hermesBootsRank < 5)
        {
            int next = hermesBootsRank + 1;
            list.Add(new UpgradeOption
            {
                id = "hermes_boots",
                title = "Giày Thần Tốc",
                subtitle = "NỘI TẠI",
                category = UpgradeCategory.PassiveStat,
                currentLevel = hermesBootsRank,
                nextLevel = next,
                icon = iconBoots,
                description = "+15% Tốc độ di chuyển giúp luồn lách qua bầy quái.",
                onSelected = () =>
                {
                    hermesBootsRank++;
                    if (playerStats != null) playerStats.AddMoveSpeedMultiplier(0.15f);
                }
            });
        }

        // 8. Spell Haste (Passive)
        if (spellHasteRank < 5)
        {
            int next = spellHasteRank + 1;
            list.Add(new UpgradeOption
            {
                id = "spell_haste",
                title = "Gia Tốc Niệm",
                subtitle = "NỘI TẠI",
                category = UpgradeCategory.PassiveStat,
                currentLevel = spellHasteRank,
                nextLevel = next,
                icon = iconHaste,
                description = "+12% Giảm thời gian hồi chiêu cho toàn bộ phép thuật.",
                onSelected = () =>
                {
                    spellHasteRank++;
                    if (playerStats != null) playerStats.ReduceCooldown(0.12f);
                }
            });
        }

        // 9. Vitality Heart (Passive)
        if (vitalityHeartRank < 5)
        {
            int next = vitalityHeartRank + 1;
            list.Add(new UpgradeOption
            {
                id = "vitality_heart",
                title = "Trái Tim Sức Sống",
                subtitle = "NỘI TẠI",
                category = UpgradeCategory.PassiveStat,
                currentLevel = vitalityHeartRank,
                nextLevel = next,
                icon = iconHeart,
                description = "+25 Máu tối đa và hồi phục ngay 25 Máu.",
                onSelected = () =>
                {
                    vitalityHeartRank++;
                    if (playerHealth != null) playerHealth.IncreaseMaxHealth(25, true);
                }
            });
        }

        // 10. Magnetic Pendant (Passive)
        if (magneticPendantRank < 5)
        {
            int next = magneticPendantRank + 1;
            list.Add(new UpgradeOption
            {
                id = "magnetic_pendant",
                title = "Mặt Dây Nam Châm",
                subtitle = "NỘI TẠI",
                category = UpgradeCategory.PassiveStat,
                currentLevel = magneticPendantRank,
                nextLevel = next,
                icon = iconMagnet,
                description = "+45% Bán kính hút ngọc kinh nghiệm từ xa.",
                onSelected = () =>
                {
                    magneticPendantRank++;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayMagnetPickup();
                    if (playerStats != null) playerStats.AddPickupRange(1.6f);
                }
            });
        }

        return list;
    }

    private UpgradeOption GetFallbackHealOption()
    {
        return new UpgradeOption
        {
            id = "heal_potion",
            title = "Bình Dược Liệu",
            subtitle = "HỒI MÁU",
            category = UpgradeCategory.Consumable,
            currentLevel = 0,
            nextLevel = 0,
            icon = iconPotion,
            description = "Hồi phục 35 Máu ngay lập tức.",
            onSelected = () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPotionPickup();
                if (playerHealth != null) playerHealth.Heal(35);
            }
        };
    }

    private string GetMagicBoltDesc(int rank)
    {
        switch (rank)
        {
            case 2: return "Bắn 2 Tia Sét Ma Pháp cùng một lúc.";
            case 3: return "+25% Tốc độ bắn và +7 Sát thương ma pháp.";
            case 4: return "Bắn 3 Tia Sét Ma Pháp tỏa ra phía trước.";
            case 5: return "Quá Tải Bí Thuật: Đạn xuyên qua 2 quái và +13 Sát thương!";
            default: return "Gia tăng sức mạnh Tia Sét Ma Pháp.";
        }
    }

    private string GetFireballDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Phóng cầu lửa phát nổ gây 45 sát thương diện rộng.";
            case 2: return "+20 Sát thương nổ và +30% Bán kính vụ nổ.";
            case 3: return "Phóng 2 Cầu Lửa đồng thời vào các mục tiêu khác nhau.";
            case 4: return "Giảm hồi chiêu xuống 1.35 giây và +15 Sát thương.";
            case 5: return "Hỏa Ngục Bùng Nổ: 3 Cầu Lửa, 110 Sát thương, bán kính nổ cực lớn!";
            default: return "Cường hóa phép Quả Cầu Lửa.";
        }
    }

    private string GetIceSpearDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Bắn ngọn giáo băng xuyên qua mọi kẻ địch và làm chậm 40%.";
            case 2: return "+16 Sát thương và giảm thời gian hồi chiêu còn 1.9 giây.";
            case 3: return "Bắn 2 Băng Thương theo góc tỏa đôi.";
            case 4: return "Làm chậm tăng lên 60% với thời gian kéo dài 3.5 giây.";
            case 5: return "Gai Băng Vĩnh Cửu: 3 Băng Thương, đóng băng hoàn toàn quái trúng đòn!";
            default: return "Cường hóa phép Băng Thương.";
        }
    }

    private string GetDarkOrbDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Triệu hồi cầu hắc ám bay quanh bản thân, gây sát thương liên tục.";
            case 2: return "+1 Cầu Bóng Tối (tổng 2 cầu) và +25% tốc độ xoay.";
            case 3: return "+1 Cầu Bóng Tối (tổng 3 cầu) và +10 sát thương mỗi đòn.";
            case 4: return "+1 Cầu Bóng Tối (tổng 4 cầu) và +20% bán kính quỹ đạo.";
            case 5: return "Vành Đai Hư Không: 5 Cầu xoay vũ bão, gây 55 sát thương va chạm!";
            default: return "Cường hóa phép Cầu Bóng Tối.";
        }
    }

    private string GetMagicShieldDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Tạo lớp khiên hấp thụ tối đa 35 sát thương. Tự hồi phục khi rời giao tranh.";
            case 2: return "Độ bền khiên tăng lên 60 Máu và thời gian hồi phục giảm còn 6 giây.";
            case 3: return "Sóng Xung Kích: Khi vỡ, phát nổ đẩy lùi và gây sát thương quái xung quanh.";
            case 4: return "Độ bền khiên tăng lên 125 Máu và thời gian hồi phục giảm còn 5 giây.";
            case 5: return "Thần Khiên Aegis: 170 Máu khiên và giảm vĩnh viễn 15% sát thương gánh chịu!";
            default: return "Cường hóa phép Khiên Hộ Thể.";
        }
    }
}
