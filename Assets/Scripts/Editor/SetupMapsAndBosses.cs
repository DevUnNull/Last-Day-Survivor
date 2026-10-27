using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SetupMapsAndBosses
{
    public static Sprite LoadSubSprite(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var a in assets)
        {
            if (a is Sprite s) return s;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    [MenuItem("Tools/Generate Map and Boss Configurations")]
    public static void GenerateAll()
    {
        EnsureDirectories();

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyPrefab.prefab");

        // 1. Create Bosses
        BossData skyDragon = CreateSkyDragon(enemyPrefab);
        BossData stormColossus = CreateStormColossus(enemyPrefab);
        BossData abyssalTitan = CreateAbyssalTitan(enemyPrefab);

        // 2. Create Maps
        CreateSkyRuinsMap(skyDragon);
        CreateDarkStormMap(stormColossus);
        CreateAbyssalEclipseMap(abyssalTitan);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SetupMapsAndBosses] All 3 Maps and 3 Boss ScriptableObjects generated successfully!");
    }

    private static void EnsureDirectories()
    {
        if (!Directory.Exists("Assets/Settings/Bosses"))
        {
            Directory.CreateDirectory("Assets/Settings/Bosses");
        }
        if (!Directory.Exists("Assets/Settings/Maps"))
        {
            Directory.CreateDirectory("Assets/Settings/Maps");
        }
    }

    private static BossData CreateSkyDragon(GameObject minionPrefab)
    {
        string path = "Assets/Settings/Bosses/Boss_SkyDragon.asset";
        BossData boss = AssetDatabase.LoadAssetAtPath<BossData>(path);
        if (boss == null)
        {
            boss = ScriptableObject.CreateInstance<BossData>();
            AssetDatabase.CreateAsset(boss, path);
        }

        boss.bossId = "sky_dragon";
        boss.bossName = "SKY DRAGON";
        boss.bossTitle = "Scourge of the Ancient Skies";
        boss.defaultSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/Boss/Sky Dragon.png");
        boss.bossIcon = boss.defaultSprite;
        boss.maxHealth = 4500;
        boss.contactDamage = 25;
        boss.healthScalePerPlayerLevel = 0.35f;
        boss.damageScalePerPlayerLevel = 0.15f;
        boss.introDuration = 2.0f;
        boss.deathDuration = 2.5f;
        boss.expReward = 500;
        boss.goldReward = 1000;

        boss.phases.Clear();

        // Phase 1
        BossPhaseData p1 = new BossPhaseData
        {
            phaseName = "Phase 1: Dragon Sovereign",
            healthThreshold = 1.0f,
            moveSpeed = 2.8f,
            preferredDistanceToPlayer = 6.0f,
            attackInterval = 2.0f,
            phaseTint = Color.white
        };

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Fire Breath",
            patternType = BossAttackPatternType.ProjectileBarrage,
            animationTrigger = "Attack",
            damage = 25,
            attackRange = 12f,
            cooldown = 4f,
            telegraphDuration = 1.0f,
            castTime = 0.3f,
            attackDuration = 0.6f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(2f, 12f),
            projectileCount = 5,
            projectileSpreadAngle = 25f,
            projectileSpeed = 10f,
            projectileLifetime = 4f
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Dragon Charge",
            patternType = BossAttackPatternType.ChargeDash,
            animationTrigger = "Attack",
            damage = 35,
            attackRange = 10f,
            minRange = 3f,
            cooldown = 6f,
            telegraphDuration = 1.2f,
            castTime = 0.2f,
            attackDuration = 0.5f,
            recoveryDuration = 1.0f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(3f, 10f),
            projectileSpeed = 14f
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Wing Sweep Shockwave",
            patternType = BossAttackPatternType.RadialShockwave,
            animationTrigger = "Attack",
            damage = 18,
            attackRange = 8f,
            cooldown = 5.5f,
            telegraphDuration = 0.8f,
            castTime = 0.2f,
            attackDuration = 0.4f,
            recoveryDuration = 0.7f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 4f,
            projectileCount = 12,
            projectileSpeed = 8f,
            projectileLifetime = 3.5f
        });

        // Phase 2: Enraged
        BossPhaseData p2 = new BossPhaseData
        {
            phaseName = "Phase 2: Raging Inferno",
            healthThreshold = 0.5f,
            moveSpeed = 3.5f,
            preferredDistanceToPlayer = 5.0f,
            attackInterval = 1.4f,
            phaseTint = new Color(1f, 0.75f, 0.75f, 1f)
        };

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Meteor Rain",
            patternType = BossAttackPatternType.TargetedAoE,
            animationTrigger = "Attack",
            damage = 40,
            attackRange = 15f,
            cooldown = 5f,
            telegraphDuration = 1.2f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.6f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 3.5f
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Empowered Dragon Flame",
            patternType = BossAttackPatternType.ProjectileBarrage,
            animationTrigger = "Attack",
            damage = 30,
            attackRange = 14f,
            cooldown = 3.5f,
            telegraphDuration = 0.9f,
            castTime = 0.2f,
            attackDuration = 0.7f,
            recoveryDuration = 0.6f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(2.5f, 14f),
            projectileCount = 9,
            projectileSpreadAngle = 40f,
            projectileSpeed = 12f,
            projectileLifetime = 4f
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Summon Dragon Minions",
            patternType = BossAttackPatternType.SummonMinions,
            animationTrigger = "Attack",
            damage = 0,
            attackRange = 20f,
            cooldown = 12f,
            telegraphDuration = 0.8f,
            castTime = 0.4f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 3f,
            summonPrefab = minionPrefab,
            summonCount = 3
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Wyrmling Swarm Invasion",
            patternType = BossAttackPatternType.MassHordeInvasion,
            animationTrigger = "Attack",
            damage = 15,
            attackRange = 20f,
            cooldown = 11f,
            telegraphDuration = 1.0f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 7.5f,
            summonCount = 14
        });

        boss.phases.Add(p1);
        boss.phases.Add(p2);

        EditorUtility.SetDirty(boss);
        return boss;
    }

    private static BossData CreateStormColossus(GameObject minionPrefab)
    {
        string path = "Assets/Settings/Bosses/Boss_StormColossus.asset";
        BossData boss = AssetDatabase.LoadAssetAtPath<BossData>(path);
        if (boss == null)
        {
            boss = ScriptableObject.CreateInstance<BossData>();
            AssetDatabase.CreateAsset(boss, path);
        }

        boss.bossId = "storm_colossus";
        boss.bossName = "STORM COLOSSUS";
        boss.bossTitle = "The Living Thunder Tempest";
        boss.defaultSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/Boss/Storm Colossus.png");
        boss.bossIcon = boss.defaultSprite;
        boss.maxHealth = 6500;
        boss.contactDamage = 35;
        boss.healthScalePerPlayerLevel = 0.40f;
        boss.damageScalePerPlayerLevel = 0.18f;
        boss.introDuration = 2.0f;
        boss.deathDuration = 2.5f;
        boss.expReward = 800;
        boss.goldReward = 1500;

        boss.phases.Clear();

        // Phase 1
        BossPhaseData p1 = new BossPhaseData
        {
            phaseName = "Phase 1: Thunder Titan",
            healthThreshold = 1.0f,
            moveSpeed = 2.2f,
            preferredDistanceToPlayer = 5.5f,
            attackInterval = 2.2f,
            phaseTint = Color.white
        };

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Giant Fist Slam",
            patternType = BossAttackPatternType.TargetedAoE,
            animationTrigger = "Attack",
            damage = 45,
            attackRange = 12f,
            cooldown = 4.5f,
            telegraphDuration = 1.2f,
            castTime = 0.3f,
            attackDuration = 0.4f,
            recoveryDuration = 0.9f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 3.5f
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Thunder Shockwave",
            patternType = BossAttackPatternType.RadialShockwave,
            animationTrigger = "Attack",
            damage = 20,
            attackRange = 10f,
            cooldown = 6f,
            telegraphDuration = 1.0f,
            castTime = 0.2f,
            attackDuration = 0.4f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 4.5f,
            projectileCount = 14,
            projectileSpeed = 7.5f,
            projectileLifetime = 4f
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Electric Beam",
            patternType = BossAttackPatternType.ContinuousBeam,
            animationTrigger = "Attack",
            damage = 30,
            attackRange = 14f,
            cooldown = 5f,
            telegraphDuration = 1.1f,
            castTime = 0.2f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(2f, 14f)
        });

        // Phase 2
        BossPhaseData p2 = new BossPhaseData
        {
            phaseName = "Phase 2: Overcharged Tempest",
            healthThreshold = 0.5f,
            moveSpeed = 3.0f,
            preferredDistanceToPlayer = 4.8f,
            attackInterval = 1.5f,
            phaseTint = new Color(0.7f, 0.9f, 1f, 1f)
        };

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Lightning Storm",
            patternType = BossAttackPatternType.TargetedAoE,
            animationTrigger = "Attack",
            damage = 50,
            attackRange = 16f,
            cooldown = 3.8f,
            telegraphDuration = 1.1f,
            castTime = 0.2f,
            attackDuration = 0.5f,
            recoveryDuration = 0.6f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 4.0f
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Summon Storm Orbs",
            patternType = BossAttackPatternType.SummonMinions,
            animationTrigger = "Attack",
            damage = 0,
            attackRange = 20f,
            cooldown = 10f,
            telegraphDuration = 0.8f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.7f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 3f,
            summonPrefab = minionPrefab,
            summonCount = 3
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Thunder Surge Swarm",
            patternType = BossAttackPatternType.MassHordeInvasion,
            animationTrigger = "Attack",
            damage = 18,
            attackRange = 20f,
            cooldown = 12f,
            telegraphDuration = 1.0f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 8.0f,
            summonCount = 16
        });

        boss.phases.Add(p1);
        boss.phases.Add(p2);

        EditorUtility.SetDirty(boss);
        return boss;
    }

    private static BossData CreateAbyssalTitan(GameObject minionPrefab)
    {
        string path = "Assets/Settings/Bosses/Boss_AbyssalTitan.asset";
        BossData boss = AssetDatabase.LoadAssetAtPath<BossData>(path);
        if (boss == null)
        {
            boss = ScriptableObject.CreateInstance<BossData>();
            AssetDatabase.CreateAsset(boss, path);
        }

        boss.bossId = "abyssal_titan";
        boss.bossName = "ABYSSAL TITAN";
        boss.bossTitle = "Cosmic Herald of the Void";
        boss.defaultSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/Boss/Abyssal Titan.png");
        boss.bossIcon = boss.defaultSprite;
        boss.maxHealth = 9000;
        boss.contactDamage = 45;
        boss.healthScalePerPlayerLevel = 0.45f;
        boss.damageScalePerPlayerLevel = 0.20f;
        boss.introDuration = 2.2f;
        boss.deathDuration = 2.8f;
        boss.expReward = 1200;
        boss.goldReward = 2000;

        boss.phases.Clear();

        // Phase 1
        BossPhaseData p1 = new BossPhaseData
        {
            phaseName = "Phase 1: Void Entity",
            healthThreshold = 1.0f,
            moveSpeed = 2.4f,
            preferredDistanceToPlayer = 6.0f,
            attackInterval = 2.0f,
            phaseTint = Color.white
        };

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Void Beam",
            patternType = BossAttackPatternType.ContinuousBeam,
            animationTrigger = "Attack",
            damage = 40,
            attackRange = 15f,
            cooldown = 4.5f,
            telegraphDuration = 1.2f,
            castTime = 0.3f,
            attackDuration = 0.6f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(2.5f, 15f)
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Abyssal Explosion",
            patternType = BossAttackPatternType.TargetedAoE,
            animationTrigger = "Attack",
            damage = 50,
            attackRange = 14f,
            cooldown = 5f,
            telegraphDuration = 1.3f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 4.0f
        });

        p1.attacks.Add(new BossAttackData
        {
            attackName = "Shadow Projectiles",
            patternType = BossAttackPatternType.ProjectileBarrage,
            animationTrigger = "Attack",
            damage = 25,
            attackRange = 13f,
            cooldown = 4f,
            telegraphDuration = 0.9f,
            castTime = 0.2f,
            attackDuration = 0.6f,
            recoveryDuration = 0.7f,
            telegraphType = AttackTelegraphType.Line,
            areaSize = new Vector2(2f, 13f),
            projectileCount = 7,
            projectileSpreadAngle = 32f,
            projectileSpeed = 9f,
            projectileLifetime = 4.5f
        });

        // Phase 2: Eternal Eclipse
        BossPhaseData p2 = new BossPhaseData
        {
            phaseName = "Phase 2: Eternal Eclipse",
            healthThreshold = 0.5f,
            moveSpeed = 3.2f,
            preferredDistanceToPlayer = 5.2f,
            attackInterval = 1.4f,
            phaseTint = new Color(0.85f, 0.65f, 1f, 1f)
        };

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Eclipse Cataclysm",
            patternType = BossAttackPatternType.TargetedAoE,
            animationTrigger = "Attack",
            damage = 65,
            attackRange = 18f,
            cooldown = 5.5f,
            telegraphDuration = 1.4f,
            castTime = 0.3f,
            attackDuration = 0.6f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 5.0f
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Void Portal Summoning",
            patternType = BossAttackPatternType.SummonMinions,
            animationTrigger = "Attack",
            damage = 0,
            attackRange = 20f,
            cooldown = 10f,
            telegraphDuration = 0.8f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.7f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 3f,
            summonPrefab = minionPrefab,
            summonCount = 3
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Abyssal Spiral Barrage",
            patternType = BossAttackPatternType.RadialShockwave,
            animationTrigger = "Attack",
            damage = 22,
            attackRange = 12f,
            cooldown = 4.2f,
            telegraphDuration = 0.8f,
            castTime = 0.2f,
            attackDuration = 0.5f,
            recoveryDuration = 0.6f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 5.0f,
            projectileCount = 20,
            projectileSpeed = 8.5f,
            projectileLifetime = 4f
        });

        p2.attacks.Add(new BossAttackData
        {
            attackName = "Void Rift Swarm Invasion",
            patternType = BossAttackPatternType.MassHordeInvasion,
            animationTrigger = "Attack",
            damage = 22,
            attackRange = 22f,
            cooldown = 10f,
            telegraphDuration = 1.1f,
            castTime = 0.3f,
            attackDuration = 0.5f,
            recoveryDuration = 0.8f,
            telegraphType = AttackTelegraphType.Circle,
            areaRadius = 8.5f,
            summonCount = 18
        });

        boss.phases.Add(p1);
        boss.phases.Add(p2);

        EditorUtility.SetDirty(boss);
        return boss;
    }

    private static void CreateSkyRuinsMap(BossData boss)
    {
        string path = "Assets/Settings/Maps/Map_1_SkyRuins.asset";
        MapData map = AssetDatabase.LoadAssetAtPath<MapData>(path);
        if (map == null)
        {
            map = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(map, path);
        }

        map.mapId = "map_sky_ruins";
        map.mapName = "Sky Ruins";
        map.description = "Ancient magical ruins floating in the sky. Survive the monster swarms until the Sky Dragon awakens.";
        map.difficulty = "NORMAL ★☆☆";
        map.difficultyRating = 1;
        map.sceneName = "2_InGame";
        map.previewSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/map1.png");
        map.backgroundSprite = map.previewSprite;
        map.survivalDuration = 300f; // 5 minutes default (can be lowered or skipped with B)
        map.isUnlockedByDefault = true;
        map.unlockRequirementMapId = "";
        map.bossData = boss;
        map.ambientColor = new Color(0.95f, 0.98f, 1f, 1f);
        map.backgroundColorTint = Color.white;

        EditorUtility.SetDirty(map);
    }

    private static void CreateDarkStormMap(BossData boss)
    {
        string path = "Assets/Settings/Maps/Map_2_DarkStorm.asset";
        MapData map = AssetDatabase.LoadAssetAtPath<MapData>(path);
        if (map == null)
        {
            map = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(map, path);
        }

        map.mapId = "map_dark_storm";
        map.mapName = "Dark Storm";
        map.description = "A corrupted sky consumed by raging lightning and forbidden storms. Ruled by the Storm Colossus.";
        map.difficulty = "HARD ★★☆";
        map.difficultyRating = 2;
        map.sceneName = "2_InGame";
        map.previewSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/map2.png");
        map.backgroundSprite = map.previewSprite;
        map.survivalDuration = 300f;
        map.isUnlockedByDefault = false;
        map.unlockRequirementMapId = "map_sky_ruins";
        map.bossData = boss;
        map.ambientColor = new Color(0.75f, 0.8f, 0.95f, 1f);
        map.backgroundColorTint = new Color(0.85f, 0.9f, 1f, 1f);

        EditorUtility.SetDirty(map);
    }

    private static void CreateAbyssalEclipseMap(BossData boss)
    {
        string path = "Assets/Settings/Maps/Map_3_AbyssalEclipse.asset";
        MapData map = AssetDatabase.LoadAssetAtPath<MapData>(path);
        if (map == null)
        {
            map = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(map, path);
        }

        map.mapId = "map_abyssal_eclipse";
        map.mapName = "Abyssal Eclipse";
        map.description = "An eternal twilight realm of cosmic void magic and ancient celestial temples guarded by the Abyssal Titan.";
        map.difficulty = "NIGHTMARE ★★★";
        map.difficultyRating = 3;
        map.sceneName = "2_InGame";
        map.previewSprite = LoadSubSprite("Assets/AnimationUI/Demo/UI/map3.png");
        map.backgroundSprite = map.previewSprite;
        map.survivalDuration = 300f;
        map.isUnlockedByDefault = false;
        map.unlockRequirementMapId = "map_dark_storm";
        map.bossData = boss;
        map.ambientColor = new Color(0.75f, 0.65f, 0.9f, 1f);
        map.backgroundColorTint = new Color(0.9f, 0.85f, 1f, 1f);

        EditorUtility.SetDirty(map);
    }
}
