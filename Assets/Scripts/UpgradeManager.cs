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
                title = "Magic Bolt",
                subtitle = "SPELL UPGRADE",
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
                title = "Fireball",
                subtitle = isNew ? "NEW SPELL" : "SPELL UPGRADE",
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
                title = "Ice Spear",
                subtitle = isNew ? "NEW SPELL" : "SPELL UPGRADE",
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
                title = "Dark Orb",
                subtitle = isNew ? "NEW SPELL" : "SPELL UPGRADE",
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
                title = "Magic Shield",
                subtitle = isNew ? "NEW SPELL" : "SPELL UPGRADE",
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
                title = "Arcane Tome",
                subtitle = "PASSIVE STAT",
                category = UpgradeCategory.PassiveStat,
                currentLevel = arcaneTomeRank,
                nextLevel = next,
                icon = iconTome,
                description = "+20% Magic Damage for all active spells.",
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
                title = "Hermes Boots",
                subtitle = "PASSIVE STAT",
                category = UpgradeCategory.PassiveStat,
                currentLevel = hermesBootsRank,
                nextLevel = next,
                icon = iconBoots,
                description = "+15% Movement Speed to outmaneuver hordes.",
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
                title = "Spell Haste",
                subtitle = "PASSIVE STAT",
                category = UpgradeCategory.PassiveStat,
                currentLevel = spellHasteRank,
                nextLevel = next,
                icon = iconHaste,
                description = "+12% Cooldown Reduction across all spells.",
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
                title = "Vitality Heart",
                subtitle = "PASSIVE STAT",
                category = UpgradeCategory.PassiveStat,
                currentLevel = vitalityHeartRank,
                nextLevel = next,
                icon = iconHeart,
                description = "+25 Max HP and immediately heals 25 HP.",
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
                title = "Magnetic Pendant",
                subtitle = "PASSIVE STAT",
                category = UpgradeCategory.PassiveStat,
                currentLevel = magneticPendantRank,
                nextLevel = next,
                icon = iconMagnet,
                description = "+45% EXP Pickup Range to easily gather gems.",
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
            title = "Field Potion",
            subtitle = "RESTORE HEALTH",
            category = UpgradeCategory.Consumable,
            currentLevel = 0,
            nextLevel = 0,
            icon = iconPotion,
            description = "Restores 35 HP immediately.",
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
            case 2: return "Fires 2 Magic Bolts simultaneously.";
            case 3: return "+25% Fire Rate and +7 Magic Damage.";
            case 4: return "Fires 3 Magic Bolts in a forward spread.";
            case 5: return "Arcane Overload: Bolts pierce 2 enemies & +13 Damage!";
            default: return "Enhances Magic Bolt power.";
        }
    }

    private string GetFireballDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Launches explosive fireballs dealing 45 AoE damage.";
            case 2: return "+20 Explosion Damage and +30% Blast Radius.";
            case 3: return "Fires 2 Fireballs simultaneously at different targets.";
            case 4: return "Reduces cooldown to 1.35s and deals +15 Damage.";
            case 5: return "Inferno Blast: 3 Fireballs, 110 Damage, huge explosion radius!";
            default: return "Empowers Fireball spell.";
        }
    }

    private string GetIceSpearDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Fires sharp ice spears that pierce all enemies and slow by 40%.";
            case 2: return "+16 Damage and reduces cooldown to 1.9s.";
            case 3: return "Fires 2 Ice Spears in twin spread angles.";
            case 4: return "Slow increased to 60% with extended 3.5s duration.";
            case 5: return "Glacial Spike: 3 Ice Spears, Freezes enemies on contact!";
            default: return "Empowers Ice Spear spell.";
        }
    }

    private string GetDarkOrbDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Summons a dark orb orbiting around you, damaging contact foes.";
            case 2: return "+1 Dark Orb (2 total) and +25% rotation speed.";
            case 3: return "+1 Dark Orb (3 total) and +10 damage per hit.";
            case 4: return "+1 Dark Orb (4 total) and +20% orbit radius.";
            case 5: return "Void Ring: 5 Dark Orbs spinning furiously, 55 contact damage!";
            default: return "Empowers Dark Orb spell.";
        }
    }

    private string GetMagicShieldDesc(int rank)
    {
        switch (rank)
        {
            case 1: return "Creates a barrier absorbing up to 35 damage. Regenerates out of combat.";
            case 2: return "Shield capacity increased to 60 HP and 6s regen delay.";
            case 3: return "Shield Shockwave: When broken, knocks back & damages nearby foes.";
            case 4: return "Shield capacity increased to 125 HP and 5s regen delay.";
            case 5: return "Aegis Barrier: 170 HP Shield and 15% passive damage reduction!";
            default: return "Empowers Magic Shield spell.";
        }
    }
}
