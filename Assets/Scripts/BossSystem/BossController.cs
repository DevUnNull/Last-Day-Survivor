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
    [Header("Visual Scale")]
    [SerializeField] private float bossScale = 0.22f;

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
    private Material bossShaderMaterial;
    private Vector3 baseBossScale;

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

        transform.localScale = Vector3.one * bossScale;

        if (GetComponent<BossFloatingHealthBar>() == null)
        {
            gameObject.AddComponent<BossFloatingHealthBar>();
        }
    }

    public void Initialize(BossData config)
    {
        this.bossConfig = config;
        baseBossScale = Vector3.one * bossScale;
        transform.localScale = baseBossScale;
        SetupBossChargeShader();

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

            int scaledHp = Mathf.Max(100, Mathf.RoundToInt(bossConfig.maxHealth * hpMult));
            bossHealth.Initialize(scaledHp, this);
            SetupPhases();
        }
        else
        {
            if (bossHealth != null)
            {
                bossHealth.Initialize(2000, this);
            }
        }

        FindPlayer();
        StartCoroutine(SpawnSequence());

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.ShowBossUI(this);
        }
    }

    private void Start()
    {
        if (playerTransform == null) FindPlayer();

        if (bossConfig == null)
        {
            bossConfig = Resources.Load<BossData>("Settings/Bosses/Boss_SkyDragon");
#if UNITY_EDITOR
            if (bossConfig == null)
            {
                bossConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<BossData>("Assets/Settings/Bosses/Boss_SkyDragon.asset");
            }
#endif
            if (bossConfig != null)
            {
                Initialize(bossConfig);
            }
            else if (bossHealth != null && bossHealth.MaxHealth <= 50)
            {
                bossHealth.Initialize(2000, this);
            }
        }

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.ShowBossUI(this);
        }
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
        if (currentState == BossState.Death || currentState == BossState.Spawning) return;

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        // Face player
        if (animController != null)
        {
            animController.FaceTarget(playerTransform.position);
        }

        // Pursuit movement in Idle / Recovery: pursue player to maintain combat pressure
        if ((currentState == BossState.Idle || currentState == BossState.Recovery) && rb != null)
        {
            Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position);
            float dist = toPlayer.magnitude;

            // Engaging range: pursue player until within melee/close combat range
            float stopDist = 1.3f;
            float curSpeed = currentPhase != null ? currentPhase.moveSpeed : 2.6f;

            if (dist > stopDist)
            {
                Vector2 moveDir = toPlayer.normalized;
                rb.MovePosition(rb.position + moveDir * (curSpeed * Time.fixedDeltaTime));
            }
        }

        // Always keep Boss strictly bounded within camera visible screen
        ClampToScreenView();
    }

    public void ClampToScreenView()
    {
        if (rb == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;
        Vector3 camPos = cam.transform.position;

        // Margin accounting for boss size (radius ~ 1.3 units)
        float marginX = 1.3f;
        float marginY = 1.3f;

        float minX = camPos.x - horzExtent + marginX;
        float maxX = camPos.x + horzExtent - marginX;
        float minY = camPos.y - vertExtent + marginY;
        float maxY = camPos.y + vertExtent - marginY;

        Vector2 curPos = rb.position;
        float clampedX = Mathf.Clamp(curPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(curPos.y, minY, maxY);

        if (clampedX != curPos.x || clampedY != curPos.y)
        {
            rb.position = new Vector2(clampedX, clampedY);
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

    private BossThemeType GetBossTheme()
    {
        if (bossConfig != null && !string.IsNullOrEmpty(bossConfig.bossId))
        {
            string id = bossConfig.bossId.ToLowerInvariant();
            if (id.Contains("storm") || id.Contains("colossus")) return BossThemeType.StormColossus;
            if (id.Contains("abyss") || id.Contains("titan")) return BossThemeType.AbyssalTitan;
        }
        return BossThemeType.SkyDragon;
    }

    private Color GetBossWarningColor()
    {
        switch (GetBossTheme())
        {
            case BossThemeType.StormColossus:
                return new Color(0.2f, 0.88f, 1f, 0.95f);
            case BossThemeType.AbyssalTitan:
                return new Color(0.88f, 0.22f, 1f, 0.95f);
            default:
                return new Color(1f, 0.42f, 0.1f, 0.95f);
        }
    }

    private void SetupBossChargeShader()
    {
        SpriteRenderer sr = animController != null ? animController.MainSpriteRenderer : GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Shader chargeShader = Shader.Find("Custom/BossPixelChargeAura");
        if (chargeShader == null)
        {
            chargeShader = Resources.Load<Shader>("Shaders/BossPixelChargeAura");
        }

        if (chargeShader != null)
        {
            bossShaderMaterial = new Material(chargeShader);
            sr.material = bossShaderMaterial;
            UpdateBossShaderThemeColors();
            SetBossShaderCharge(0f);
            SetBossShaderDissolve(0f);
        }
    }

    private void UpdateBossShaderThemeColors()
    {
        if (bossShaderMaterial == null) return;
        switch (GetBossTheme())
        {
            case BossThemeType.StormColossus:
                bossShaderMaterial.SetColor("_OutlineColor", new Color(0.18f, 0.88f, 1f, 1f));
                bossShaderMaterial.SetColor("_InnerGlowColor", new Color(0.92f, 1f, 1f, 1f));
                break;
            case BossThemeType.AbyssalTitan:
                bossShaderMaterial.SetColor("_OutlineColor", new Color(0.92f, 0.2f, 1f, 1f));
                bossShaderMaterial.SetColor("_InnerGlowColor", new Color(1f, 0.78f, 1f, 1f));
                break;
            default:
                bossShaderMaterial.SetColor("_OutlineColor", new Color(1f, 0.48f, 0.08f, 1f));
                bossShaderMaterial.SetColor("_InnerGlowColor", new Color(1f, 0.96f, 0.65f, 1f));
                break;
        }
    }

    private void SetBossShaderCharge(float amount)
    {
        if (bossShaderMaterial != null)
        {
            bossShaderMaterial.SetFloat("_ChargeAmount", Mathf.Clamp01(amount));
        }
    }

    private void SetBossShaderDissolve(float amount)
    {
        if (bossShaderMaterial != null)
        {
            bossShaderMaterial.SetFloat("_DissolveAmount", Mathf.Clamp01(amount));
        }
    }

    private IEnumerator ExecuteAttackRoutine(BossAttackData attack)
    {
        isExecutingAction = true;
        currentAttack = attack;
        if (baseBossScale.sqrMagnitude < 0.0001f) baseBossScale = Vector3.one * bossScale;

        // 1. Telegraph & Vận Chiêu (Charge-Up) Phase
        currentState = BossState.Telegraphing;
        BossThemeType theme = GetBossTheme();
        UpdateBossShaderThemeColors();

        Vector3 targetPos = playerTransform != null ? playerTransform.position : transform.position + Vector3.right * 3f;
        Vector2 attackDir = (targetPos - transform.position).normalized;

        if (telegraphController != null && attack.telegraphType != AttackTelegraphType.None)
        {
            Color warningColor = GetBossWarningColor();
            if (attack.telegraphType == AttackTelegraphType.Circle)
            {
                Vector3 center = (attack.patternType == BossAttackPatternType.RadialShockwave) ? transform.position : targetPos;
                telegraphController.ShowCircleTelegraph(center, attack.areaRadius, attack.telegraphDuration, warningColor);
            }
            else if (attack.telegraphType == AttackTelegraphType.Line)
            {
                if (attack.patternType == BossAttackPatternType.ProjectileBarrage && attack.projectileCount > 1)
                {
                    telegraphController.ShowProjectileFanTelegraph(
                        transform.position,
                        attackDir,
                        attack.projectileCount,
                        attack.projectileSpreadAngle,
                        attack.areaSize.y,
                        attack.telegraphDuration,
                        warningColor
                    );
                }
                else
                {
                    telegraphController.ShowLineTelegraph(transform.position, attackDir, attack.areaSize, attack.telegraphDuration, warningColor);
                }
            }
        }

        // Bắt đầu hiệu ứng Vận Chiêu dưới chân & quanh thân Boss
        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossChargeStartFX(transform.position, theme, attack.telegraphDuration);
        }

        float telegraphElapsed = 0f;
        float fxTickTimer = 0f;
        float totalTelegraph = Mathf.Max(0.15f, attack.telegraphDuration);

        while (telegraphElapsed < totalTelegraph)
        {
            float dt = Time.deltaTime;
            telegraphElapsed += dt;
            fxTickTimer += dt;

            float progress = Mathf.Clamp01(telegraphElapsed / totalTelegraph);

            // Điều khiển Shader Vận Chiêu (Pixel Outline + Scrolling Energy Wave + Sine Vibration)
            SetBossShaderCharge(progress);

            // Hiệu ứng thân Boss gồng lực (Squash nén lực + Rung nhẹ theo tiến độ vận chiêu)
            float squashX = 1f + progress * 0.14f + Mathf.Sin(Time.time * 42f) * (0.03f * progress);
            float squashY = 1f - progress * 0.10f - Mathf.Sin(Time.time * 42f) * (0.03f * progress);
            transform.localScale = new Vector3(baseBossScale.x * squashX, baseBossScale.y * squashY, baseBossScale.z);

            if (fxTickTimer >= 0.065f)
            {
                fxTickTimer = 0f;
                if (SpellFXManager.Instance != null)
                {
                    SpellFXManager.Instance.SpawnBossChargeTickFX(
                        transform.position,
                        targetPos,
                        attackDir,
                        attack.patternType,
                        theme,
                        progress
                    );
                }
            }

            yield return null;
        }

        // 2. Cast Time & Animation (Bung lực khai hỏa)
        currentState = BossState.Attacking;
        SetBossShaderCharge(1f);
        transform.localScale = baseBossScale * 1.18f;

        if (animController != null)
        {
            animController.PlayAttack(attack.animationTrigger);
        }

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossCastReleaseFX(transform.position, attackDir, theme);
        }

        yield return new WaitForSeconds(attack.castTime);

        // 3. Attack Pattern Execution
        transform.localScale = baseBossScale;
        SpawnAttackPattern(attack, targetPos, attackDir);

        yield return new WaitForSeconds(attack.attackDuration);

        // 4. Recovery Phase
        SetBossShaderCharge(0f);
        transform.localScale = baseBossScale;
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

    private static Sprite defaultProjectileSprite;

    private void SpawnSingleProjectile(BossAttackData attack, Vector3 origin, Vector2 dir)
    {
        BossThemeType theme = GetBossTheme();
        GameObject projObj;
        if (attack.projectilePrefab != null)
        {
            projObj = Instantiate(attack.projectilePrefab, origin, Quaternion.identity);
        }
        else
        {
            projObj = new GameObject($"BossProjectile_{theme}");
            projObj.transform.position = origin;
            projObj.transform.localScale = Vector3.one * 1.15f;

            var sr = projObj.AddComponent<SpriteRenderer>();
            if (SpellFXManager.Instance != null)
            {
                sr.sprite = SpellFXManager.Instance.GetBossProjectileSprite(theme);
            }
            sr.color = Color.white;
            sr.sortingOrder = 15;

            var col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.42f;
        }

        BossProjectile proj = projObj.GetComponent<BossProjectile>();
        if (proj == null) proj = projObj.AddComponent<BossProjectile>();
        int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
        proj.Setup(dir, attack.projectileSpeed, scaledDamage, attack.projectileLifetime, theme);
    }

    private void ExecuteAoEImpact(BossAttackData attack, Vector3 targetPos)
    {
        if (attack.vfxPrefab != null)
        {
            Instantiate(attack.vfxPrefab, targetPos, Quaternion.identity);
        }

        if (SpellFXManager.Instance != null)
        {
            BossThemeType theme = GetBossTheme();
            string atkName = attack.attackName != null ? attack.attackName.ToLowerInvariant() : "";

            if (atkName.Contains("fist") || atkName.Contains("slam"))
            {
                SpellFXManager.Instance.SpawnBossFistSlamFX(targetPos, attack.areaRadius);
            }
            else if (atkName.Contains("lightning") || atkName.Contains("thunder") || theme == BossThemeType.StormColossus)
            {
                SpellFXManager.Instance.SpawnBossLightningStormFX(targetPos, attack.areaRadius);
            }
            else if (atkName.Contains("abyss") || atkName.Contains("eclipse") || theme == BossThemeType.AbyssalTitan)
            {
                SpellFXManager.Instance.SpawnBossAbyssalCataclysmFX(targetPos, attack.areaRadius);
            }
            else
            {
                SpellFXManager.Instance.SpawnBossMeteorRainFX(targetPos, attack.areaRadius);
            }
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, attack.areaRadius);
        foreach (var hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject) continue;
            PlayerHealth ph = hit.GetComponent<PlayerHealth>() ?? hit.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyAttack();
                int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
                ph.TakeDamage(scaledDamage);
                break;
            }
        }
    }

    private IEnumerator PerformChargeDash(BossAttackData attack, Vector2 dir)
    {
        float dashSpeed = attack.projectileSpeed > 0 ? attack.projectileSpeed : 12f;
        float dashTime = 0.5f;
        float timer = 0f;
        float ghostTimer = 0f;
        BossThemeType theme = GetBossTheme();
        SpriteRenderer bossSr = animController != null ? animController.MainSpriteRenderer : GetComponent<SpriteRenderer>();

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, 2.2f, theme);
        }

        while (timer < dashTime)
        {
            timer += Time.deltaTime;
            ghostTimer += Time.deltaTime;
            rb.MovePosition(rb.position + dir * (dashSpeed * Time.deltaTime));
            ClampToScreenView();

            if (ghostTimer >= 0.045f)
            {
                ghostTimer = 0f;
                if (SpellFXManager.Instance != null)
                {
                    SpellFXManager.Instance.SpawnBossDashAfterImage(bossSr, transform.position, transform.localScale, dir, theme);
                }
            }

            // Check contact damage during dash
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.8f);
            foreach (var h in hits)
            {
                if (h == null || h.gameObject == gameObject) continue;
                PlayerHealth ph = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyAttack();
                    int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
                    ph.TakeDamage(scaledDamage);
                    break;
                }
            }

            yield return null;
        }

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.TriggerCameraShake(0.14f, 0.10f);
        }
    }

    private void FireRadialShockwave(BossAttackData attack)
    {
        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, attack.areaRadius, GetBossTheme());
        }

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
        BossThemeType theme = GetBossTheme();
        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, 2.5f, theme);
        }

        int count = Mathf.Max(2, attack.summonCount);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(2.5f, 4.5f);
            Vector3 spawnPos = transform.position + (Vector3)offset;

            if (SpellFXManager.Instance != null)
            {
                SpellFXManager.Instance.SpawnBossSummonPortalFX(spawnPos, theme);
            }

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
        BossThemeType theme = GetBossTheme();

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, 3.5f, theme);
        }

        int swarmCount = Mathf.Max(12, attack.summonCount > 0 ? attack.summonCount : 16);
        float ringRadius = (attack.areaRadius > 0f) ? attack.areaRadius : 7.0f;

        // Minions scale with player level
        int swarmHp = Mathf.RoundToInt(45 * (1f + (playerLevelAtSpawn - 1) * 0.25f));
        int swarmDmg = Mathf.RoundToInt(12 * currentDamageMultiplier);

        for (int i = 0; i < swarmCount; i++)
        {
            float angle = (i * (360f / swarmCount)) * Mathf.Deg2Rad;
            Vector3 spawnPos = targetCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ringRadius;

            if (SpellFXManager.Instance != null)
            {
                SpellFXManager.Instance.SpawnBossSummonPortalFX(spawnPos, theme);
            }

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
        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossLaserBeamFX(
                transform.position,
                attackDir,
                Mathf.Max(1.2f, attack.areaSize.x),
                Mathf.Max(10f, attack.areaSize.y),
                Mathf.Max(0.45f, attack.attackDuration),
                GetBossTheme()
            );
        }

        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(attack.areaSize.x, 0.8f), Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg, attackDir, attack.areaSize.y);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.gameObject == gameObject) continue;
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>() ?? hit.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyAttack();
                int scaledDamage = Mathf.RoundToInt(attack.damage * currentDamageMultiplier);
                ph.TakeDamage(scaledDamage);
                break;
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

        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, 4.5f, GetBossTheme());
        }

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

        BossThemeType theme = GetBossTheme();
        if (SpellFXManager.Instance != null)
        {
            SpellFXManager.Instance.SpawnBossRadialBurstFX(transform.position, 5.0f, theme);
        }

        float deathDuration = bossConfig != null ? bossConfig.deathDuration : 2.5f;

        // Visual fade / shrink death feedback with periodic mini explosions
        float timer = 0f;
        float burstTimer = 0f;
        Vector3 initialScale = transform.localScale;
        SpriteRenderer sr = animController != null ? animController.MainSpriteRenderer : GetComponent<SpriteRenderer>();

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;
            burstTimer += Time.deltaTime;
            if (burstTimer >= 0.35f && SpellFXManager.Instance != null)
            {
                burstTimer = 0f;
                Vector3 randOffset = (Vector3)(Random.insideUnitCircle * 1.2f);
                SpellFXManager.Instance.SpawnBossProjectileHitFX(transform.position + randOffset, Vector2.up, theme);
            }

            float p = timer / deathDuration;
            SetBossShaderCharge(1f - p * 0.5f);
            SetBossShaderDissolve(p);
            if (sr != null && bossShaderMaterial == null)
            {
                sr.color = new Color(1f, 1f, 1f, 1f - p);
            }
            transform.localScale = initialScale * (1f - p * 0.25f);
            yield return null;
        }

        // Notify GameplaySessionManager of victory!
        if (GameplaySessionManager.Instance != null)
        {
            GameplaySessionManager.Instance.OnBossDefeated(bossConfig);
        }

        Destroy(gameObject);
    }

    private float lastContactDamageTime = 0f;
    [SerializeField] private float contactDamageInterval = 0.5f;

    private void TryDamagePlayer(GameObject target)
    {
        if (currentState == BossState.Death || currentState == BossState.Spawning) return;
        if (target == null || target == gameObject) return;
        if (Time.time < lastContactDamageTime + contactDamageInterval) return;

        PlayerHealth ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyAttack();
            }

            int baseContactDmg = (bossConfig != null && bossConfig.contactDamage > 0) ? bossConfig.contactDamage : 25;
            int scaledContactDamage = Mathf.RoundToInt(baseContactDmg * currentDamageMultiplier);
            ph.TakeDamage(scaledContactDamage);
            lastContactDamageTime = Time.time;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        TryDamagePlayer(collider.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        TryDamagePlayer(collider.gameObject);
    }
}
