using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Spawning,
    Idle,
    Telegraphing,
    Attacking,
    Recovery,
    PhaseTransition,
    Stunned,
    Death
}

[RequireComponent(typeof(BossHealth))]
[RequireComponent(typeof(BossAnimationController))]
[RequireComponent(typeof(BossTelegraphController))]
[RequireComponent(typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private BossData bossConfig;

    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossAnimationController animController;
    [SerializeField] private BossTelegraphController telegraphController;
    [SerializeField] private Rigidbody2D rb;

    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private BossState currentState = BossState.Spawning;
    private int currentPhaseIndex = 0;
    private BossPhaseData currentPhase;
    private BossAttackData currentAttack;

    private float stateTimer = 0f;
    private float attackCooldownTimer = 0f;
    private bool isExecutingAction = false;

    private float currentDamageMultiplier = 1f;
    private int playerLevelAtSpawn = 1;

    public BossData BossConfig => bossConfig;
    public BossState CurrentState => currentState;
    public int CurrentPhaseIndex => currentPhaseIndex;
    public BossHealth Health => bossHealth;
    public float CurrentDamageMultiplier => currentDamageMultiplier;
    public int PlayerLevelAtSpawn => playerLevelAtSpawn;

    private void Awake()
    {
        if (bossHealth == null) bossHealth = GetComponent<BossHealth>();
        if (animController == null) animController = GetComponent<BossAnimationController>();
        if (telegraphController == null) telegraphController = GetComponent<BossTelegraphController>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    public void Initialize(BossData config)
    {
        this.bossConfig = config;

        if (bossConfig != null)
        {
            if (animController != null && bossConfig.defaultSprite != null)
            {
                animController.SetDefaultSprite(bossConfig.defaultSprite);
            }

            int pLevel = 1;
            if (PlayerLevelSystem.Instance != null)
            {
                pLevel = Mathf.Max(1, PlayerLevelSystem.Instance.CurrentLevel);
            }
            playerLevelAtSpawn = pLevel;

            // Proportional scaling with player's current level
            float hpMult = 1f + (pLevel - 1) * bossConfig.healthScalePerPlayerLevel;
            currentDamageMultiplier = 1f + (pLevel - 1) * bossConfig.damageScalePerPlayerLevel;

            int scaledHp = Mathf.RoundToInt(bossConfig.maxHealth * hpMult);
            bossHealth.Initialize(scaledHp, this);
            SetupPhases();
        }

        FindPlayer();
        StartCoroutine(SpawnSequence());
    }

    private void Start()
    {
        if (playerTransform == null) FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    private void SetupPhases()
    {
        if (bossConfig.phases != null && bossConfig.phases.Count > 0)
        {
            currentPhaseIndex = 0;
            currentPhase = bossConfig.phases[0];
            ApplyPhaseSettings(currentPhase);
        }
    }

    private void ApplyPhaseSettings(BossPhaseData phase)
    {
        if (phase == null) return;
        if (animController != null)
        {
            animController.SetTint(phase.phaseTint);
        }
    }

    private IEnumerator SpawnSequence()
    {
        currentState = BossState.Spawning;
        bossHealth.SetInvulnerable(true);

        if (animController != null) animController.PlayIntro();

        float introTime = bossConfig != null ? bossConfig.introDuration : 2.0f;
        yield return new WaitForSeconds(introTime);

        bossHealth.SetInvulnerable(false);
        currentState = BossState.Idle;
        if (animController != null) animController.PlayIdle();
    }

    private void Update()
    {
        if (currentState == BossState.Death || currentState == BossState.Spawning || currentState == BossState.PhaseTransition)
            return;

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // Always face player in idle or repositioning
        if (currentState == BossState.Idle || currentState == BossState.Telegraphing)
        {
            if (animController != null && playerTransform != null)
            {
                animController.FaceTarget(playerTransform.position);
            }
        }

        // Tick attack cooldowns in current phase
        if (currentPhase != null && currentPhase.attacks != null)
        {
            foreach (var atk in currentPhase.attacks)
            {
                if (atk.currentCooldownTimer > 0f)
                {
                    atk.currentCooldownTimer -= Time.deltaTime;
                }
            }
        }

        // State behavior
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState == BossState.Idle && playerTransform != null && currentPhase != null)
        {
            // Reposition / hover around preferred distance
            Vector2 toPlayer = (playerTransform.position - transform.position);
            float dist = toPlayer.magnitude;

            if (dist > currentPhase.preferredDistanceToPlayer + 1.0f)
            {
                Vector2 moveDir = toPlayer.normalized;
                rb.MovePosition(rb.position + moveDir * (currentPhase.moveSpeed * Time.fixedDeltaTime));
            }
            else if (dist < currentPhase.preferredDistanceToPlayer - 1.5f)
            {
                Vector2 moveDir = -toPlayer.normalized;
                rb.MovePosition(rb.position + moveDir * (currentPhase.moveSpeed * Time.fixedDeltaTime));
            }
        }
    }

    private void HandleIdleState()
    {
        attackCooldownTimer -= Time.deltaTime;

        if (attackCooldownTimer <= 0f && !isExecutingAction)
        {
            BossAttackData selectedAttack = ChooseNextAttack();
            if (selectedAttack != null)
            {
                StartCoroutine(ExecuteAttackRoutine(selectedAttack));
            }
            else
            {
                attackCooldownTimer = 0.5f; // Retry shortly
            }
        }
    }

    private BossAttackData ChooseNextAttack()
    {
        if (currentPhase == null || currentPhase.attacks == null || currentPhase.attacks.Count == 0)
            return null;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : 5f;

        List<BossAttackData> validAttacks = new List<BossAttackData>();
        foreach (var atk in currentPhase.attacks)
        {
            if (atk.currentCooldownTimer <= 0f && dist <= atk.attackRange && dist >= atk.minRange)
            {
                validAttacks.Add(atk);
            }
        }

        if (validAttacks.Count == 0) return null;
        return validAttacks[Random.Range(0, validAttacks.Count)];
    }

    private IEnumerator ExecuteAttackRoutine(BossAttackData attack)
    {
        isExecutingAction = true;
        currentAttack = attack;

        // 1. Telegraph Phase
        currentState = BossState.Telegraphing;
        Vector3 targetPos = playerTransform != null ? playerTransform.position : transform.position + Vector3.right * 3f;
        Vector2 attackDir = (targetPos - transform.position).normalized;

        if (telegraphController != null && attack.telegraphType != AttackTelegraphType.None)
        {
            Color warningColor = new Color(1f, 0.2f, 0.2f, 0.9f);
            if (attack.telegraphType == AttackTelegraphType.Circle)
            {
                Vector3 center = (attack.patternType == BossAttackPatternType.RadialShockwave) ? transform.position : targetPos;
                telegraphController.ShowCircleTelegraph(center, attack.areaRadius, attack.telegraphDuration, warningColor);
            }
            else if (attack.telegraphType == AttackTelegraphType.Line)
            {
                telegraphController.ShowLineTelegraph(transform.position, attackDir, attack.areaSize, attack.telegraphDuration, warningColor);
            }
        }

        yield return new WaitForSeconds(attack.telegraphDuration);

        // 2. Cast Time & Animation
        currentState = BossState.Attacking;
        if (animController != null)
        {
            animController.PlayAttack(attack.animationTrigger);
        }

        yield return new WaitForSeconds(attack.castTime);

        // 3. Attack Pattern Execution
        SpawnAttackPattern(attack, targetPos, attackDir);

        yield return new WaitForSeconds(attack.attackDuration);

        // 4. Recovery Phase
        currentState = BossState.Recovery;
        attack.currentCooldownTimer = attack.cooldown;

        yield return new WaitForSeconds(attack.recoveryDuration);

        // 5. Back to Idle
        currentState = BossState.Idle;
        if (animController != null) animController.PlayIdle();

        attackCooldownTimer = currentPhase != null ? currentPhase.attackInterval : 1.5f;
        isExecutingAction = false;
        currentAttack = null;
    }

    private void SpawnAttackPattern(BossAttackData attack, Vector3 targetPos, Vector2 attackDir)
    {
        switch (attack.patternType)
        {
            case BossAttackPatternType.ProjectileBarrage:
                StartCoroutine(FireProjectileBarrage(attack, attackDir));
                break;

            case BossAttackPatternType.TargetedAoE:
                ExecuteAoEImpact(attack, targetPos);
                break;

            case BossAttackPatternType.ChargeDash:
                StartCoroutine(PerformChargeDash(attack, attackDir));
                break;

            case BossAttackPatternType.RadialShockwave:
                FireRadialShockwave(attack);
                break;

            case BossAttackPatternType.SummonMinions:
                SummonMinions(attack);
                break;

            case BossAttackPatternType.MassHordeInvasion:
                ExecuteMassHordeInvasion(attack);
                break;

            case BossAttackPatternType.ContinuousBeam:
                ExecuteBeamAttack(attack, attackDir);
                break;
        }
    }

    private IEnumerator FireProjectileBarrage(BossAttackData attack, Vector2 attackDir)
    {
        int count = Mathf.Max(1, attack.projectileCount);
        float spread = attack.projectileSpreadAngle;
        float baseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float offset = (count > 1) ? Mathf.Lerp(-spread * 0.5f, spread * 0.5f, (float)i / (count - 1)) : 0f;
            float angle = (baseAngle + offset) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            SpawnSingleProjectile(attack, transform.position, dir);
            if (count > 3) yield return new WaitForSeconds(0.08f);
        }
    }

    private void SpawnSingleProjectile(BossAttackData attack, Vector3 origin, Vector2 dir)
    {
        GameObject projObj;
        if (attack.projectilePrefab != null)
        {
            projObj = Instantiate(attack.projectilePrefab, origin, Quaternion.identity);
        }
        else
        {
            // Fallback projectile
            projObj = new GameObject("BossProjectile");
            projObj.transform.position = origin;
            var sr = projObj.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.sortingOrder = 7;
            var col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
        }

        BossProjectile proj = projObj.GetComponent<BossProjectile>();
        if (proj == null) proj = projObj.AddComponent<BossProjectile>();
        int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
        proj.Setup(dir, attack.projectileSpeed, scaledDamage, attack.projectileLifetime);
    }

    private void ExecuteAoEImpact(BossAttackData attack, Vector3 targetPos)
    {
        if (attack.vfxPrefab != null)
        {
            Instantiate(attack.vfxPrefab, targetPos, Quaternion.identity);
        }

        Collider2D hit = Physics2D.OverlapCircle(targetPos, attack.areaRadius);
        if (hit != null && (hit.CompareTag("Player") || hit.name == "Player"))
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
                ph.TakeDamage(scaledDamage);
            }
        }
    }

    private IEnumerator PerformChargeDash(BossAttackData attack, Vector2 dir)
    {
        float dashSpeed = attack.projectileSpeed > 0 ? attack.projectileSpeed : 12f;
        float dashTime = 0.5f;
        float timer = 0f;

        while (timer < dashTime)
        {
            timer += Time.deltaTime;
            rb.MovePosition(rb.position + dir * (dashSpeed * Time.deltaTime));
            yield return null;
        }
    }

    private void FireRadialShockwave(BossAttackData attack)
    {
        int ringCount = Mathf.Max(8, attack.projectileCount);
        for (int i = 0; i < ringCount; i++)
        {
            float angle = (i * 360f / ringCount) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnSingleProjectile(attack, transform.position, dir);
        }
    }

    private void SummonMinions(BossAttackData attack)
    {
        int count = Mathf.Max(2, attack.summonCount);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(2.5f, 4.5f);
            Vector3 spawnPos = transform.position + (Vector3)offset;

            if (EnemySpawner.Instance != null)
            {
                int minionHp = Mathf.RoundToInt(50 * (1f + (playerLevelAtSpawn - 1) * 0.2f));
                int minionDmg = Mathf.RoundToInt(10 * currentDamageMultiplier);
                EnemySpawner.Instance.SpawnCustomEnemy(spawnPos, minionHp, minionDmg, 1.1f, new Color(1f, 0.6f, 0.6f, 1f));
            }
            else if (attack.summonPrefab != null)
            {
                Instantiate(attack.summonPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    private void ExecuteMassHordeInvasion(BossAttackData attack)
    {
        if (playerTransform == null) FindPlayer();
        Vector3 targetCenter = (playerTransform != null) ? playerTransform.position : transform.position;

        int swarmCount = Mathf.Max(12, attack.summonCount > 0 ? attack.summonCount : 16);
        float ringRadius = (attack.areaRadius > 0f) ? attack.areaRadius : 7.0f;

        // Minions scale with player level
        int swarmHp = Mathf.RoundToInt(45 * (1f + (playerLevelAtSpawn - 1) * 0.25f));
        int swarmDmg = Mathf.RoundToInt(12 * currentDamageMultiplier);

        for (int i = 0; i < swarmCount; i++)
        {
            float angle = (i * (360f / swarmCount)) * Mathf.Deg2Rad;
            Vector3 spawnPos = targetCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ringRadius;

            if (EnemySpawner.Instance != null)
            {
                // Minions rush inward with crimson tint and high speed
                EnemySpawner.Instance.SpawnCustomEnemy(
                    spawnPos,
                    swarmHp,
                    swarmDmg,
                    speedMult: 1.3f,
                    tint: new Color(0.95f, 0.15f, 0.15f, 1f),
                    scaleMult: 1.15f,
                    expReward: 15
                );
            }
            else if (attack.summonPrefab != null)
            {
                Instantiate(attack.summonPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    private void ExecuteBeamAttack(BossAttackData attack, Vector2 attackDir)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(attack.areaSize.x, 0.5f), Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg, attackDir, attack.areaSize.y);
        if (hit.collider != null && (hit.collider.CompareTag("Player") || hit.collider.name == "Player"))
        {
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
                ph.TakeDamage(scaledDamage);
            }
        }
    }

    public void OnTakeDamage(int damage, int currentHp, int maxHp)
    {
        if (animController != null)
        {
            animController.PlayHitFlash();
        }

        // Check phase transitions
        if (bossConfig != null && bossConfig.phases != null && bossConfig.phases.Count > currentPhaseIndex + 1)
        {
            float hpRatio = (float)currentHp / maxHp;
            var nextPhase = bossConfig.phases[currentPhaseIndex + 1];
            if (hpRatio <= nextPhase.healthThreshold)
            {
                StartCoroutine(TriggerPhaseTransition(currentPhaseIndex + 1));
            }
        }
    }

    private IEnumerator TriggerPhaseTransition(int newIndex)
    {
        currentState = BossState.PhaseTransition;
        bossHealth.SetInvulnerable(true);
        currentPhaseIndex = newIndex;
        currentPhase = bossConfig.phases[newIndex];

        if (animController != null)
        {
            animController.PlayPhaseTransition();
        }

        yield return new WaitForSeconds(1.5f);

        ApplyPhaseSettings(currentPhase);
        bossHealth.SetInvulnerable(false);
        currentState = BossState.Idle;
        if (animController != null) animController.PlayIdle();
    }

    public void OnBossKilled()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        currentState = BossState.Death;
        bossHealth.SetInvulnerable(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (animController != null)
        {
            animController.PlayDeath();
        }

        float deathDuration = bossConfig != null ? bossConfig.deathDuration : 2.5f;

        // Visual fade / shrink death feedback
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        SpriteRenderer sr = animController != null ? animController.MainSpriteRenderer : GetComponent<SpriteRenderer>();

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;
            float p = timer / deathDuration;
            if (sr != null)
            {
                sr.color = new Color(1f, 1f, 1f, 1f - p);
            }
            transform.localScale = initialScale * (1f - p * 0.3f);
            yield return null;
        }

        // Notify GameplaySessionManager of victory!
        if (GameplaySessionManager.Instance != null)
        {
            GameplaySessionManager.Instance.OnBossDefeated(bossConfig);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == BossState.Death || currentState == BossState.Spawning) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null && bossConfig != null)
            {
                int scaledContactDamage = Mathf.RoundToInt(bossConfig.contactDamage * currentDamageMultiplier);
                ph.TakeDamage(scaledContactDamage);
            }
        }
    }
}
