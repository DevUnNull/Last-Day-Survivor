using UnityEngine;

public enum AttackTelegraphType
{
    None,
    Circle,     // Area explosion, meteor impact, fist slam
    Line,       // Linear charge, beam, projectile barrage path
    Cone,       // Breath attack, sweep shockwave
    RadialRing  // Expanding shockwave
}

public enum BossAttackPatternType
{
    ProjectileBarrage,
    TargetedAoE,
    ChargeDash,
    ContinuousBeam,
    RadialShockwave,
    SummonMinions,
    MassHordeInvasion
}

[System.Serializable]
public class BossAttackData
{
    [Header("Identification & Visuals")]
    public string attackName = "Basic Attack";
    public BossAttackPatternType patternType = BossAttackPatternType.ProjectileBarrage;
    public string animationTrigger = "Attack";

    [Header("Combat Stats")]
    public int damage = 20;
    public float attackRange = 10f;
    public float minRange = 0f;
    public float cooldown = 5f;

    [Header("Timings (State Machine)")]
    [Tooltip("Time to show warning telegraph before executing the attack")]
    public float telegraphDuration = 1.0f;
    [Tooltip("Cast preparation delay before damage or projectiles emerge")]
    public float castTime = 0.3f;
    [Tooltip("Duration while active attack or beam persists")]
    public float attackDuration = 0.5f;
    [Tooltip("Vulnerability window after attack completes before returning to Idle")]
    public float recoveryDuration = 0.8f;

    [Header("Telegraph Geometry")]
    public AttackTelegraphType telegraphType = AttackTelegraphType.Circle;
    public float areaRadius = 3f;
    public Vector2 areaSize = new Vector2(2f, 12f); // For Line/Beam
    public float coneAngle = 60f;

    [Header("Prefabs & Effects")]
    public GameObject projectilePrefab;
    public GameObject vfxPrefab;
    public GameObject summonPrefab;
    public int summonCount = 2;

    [Header("Projectile Customization")]
    public int projectileCount = 1;
    public float projectileSpreadAngle = 0f;
    public float projectileSpeed = 8f;
    public float projectileLifetime = 5f;

    // Runtime state (cooldown timer)
    [System.NonSerialized] public float currentCooldownTimer = 0f;
}
