using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossPhaseData
{
    [Header("Phase Settings")]
    public string phaseName = "Phase 1";
    [Range(0f, 1f)]
    [Tooltip("Health percentage below which this phase activates (e.g. 1.0 for start, 0.5 for phase 2)")]
    public float healthThreshold = 1.0f;

    [Header("Movement & Combat")]
    public float moveSpeed = 2.5f;
    public float preferredDistanceToPlayer = 6f;
    public float attackInterval = 2.0f; // Idle time between selecting attacks

    [Header("Visual Tint")]
    public Color phaseTint = Color.white;

    [Header("Available Attacks in this Phase")]
    public List<BossAttackData> attacks = new List<BossAttackData>();
}

[CreateAssetMenu(fileName = "NewBossData", menuName = "LastDaySurvivor/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("Boss Identity")]
    public string bossId = "sky_dragon";
    public string bossName = "SKY DRAGON";
    public string bossTitle = "Scourge of the Ancient Skies";
    public Sprite bossIcon;
    public Sprite defaultSprite; // Frame-by-frame placeholder single static sprite

    [Header("Health Settings")]
    public int maxHealth = 3000;
    public int contactDamage = 15;

    [Header("Player Level Dynamic Scaling")]
    [Tooltip("Health scaling multiplier per player level above Lv 1 (e.g. 0.35 = +35% HP per level)")]
    public float healthScalePerPlayerLevel = 0.35f;
    [Tooltip("Damage scaling multiplier per player level above Lv 1 (e.g. 0.15 = +15% Damage per level)")]
    public float damageScalePerPlayerLevel = 0.15f;

    [Header("Phases Configuration")]
    public List<BossPhaseData> phases = new List<BossPhaseData>();

    [Header("Cinematic & Death")]
    public float introDuration = 2.0f;
    public float deathDuration = 2.5f;
    public GameObject spawnVFXPrefab;
    public GameObject deathVFXPrefab;

    [Header("Rewards on Defeat")]
    public int expReward = 500;
    public int goldReward = 1000;
}
