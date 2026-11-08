using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum SpellElementType
{
    Arcane,
    Fire,
    Ice,
    Void,
    Shield
}

/// <summary>
/// Quản lý toàn bộ hiệu ứng hình ảnh (Fly FX, Hit Impact FX, Floating Damage, Camera Shake)
/// cho các kỹ năng trong game với cơ chế Object Pooling (Zero GC) tối ưu cho Mobile Android.
/// </summary>
public class SpellFXManager : MonoBehaviour
{
    private static SpellFXManager instance;
    public static SpellFXManager Instance
    {
        get
        {
            if (instance == null && Application.isPlaying)
            {
                GameObject go = new GameObject("[SpellFXManager]");
                instance = go.AddComponent<SpellFXManager>();
            }
            return instance;
        }
    }

    [Header("Cấu hình Object Pool (Tối ưu Mobile)")]
    [Tooltip("Số lượng hạt hiệu ứng pixel tối đa hoạt động cùng lúc")]
    [SerializeField] private int maxParticles = 180;

    [Tooltip("Số lượng chữ sát thương bay lên tối đa cùng lúc")]
    [SerializeField] private int maxFloatingTexts = 40;

    [Tooltip("Bật/Tắt chữ sát thương bay lên khi trúng đòn")]
    [SerializeField] private bool enableFloatingDamage = true;

    [Tooltip("Bật/Tắt rung chấn camera nhẹ khi nổ chiêu lớn")]
    [SerializeField] private bool enableCameraShake = true;

    private class PooledParticle
    {
        public GameObject gameObject;
        public Transform transform;
        public SpriteRenderer renderer;
        public bool active;

        public Vector3 position;
        public Vector3 velocity;
        public float drag;
        public float gravity;

        public Vector3 startScale;
        public Vector3 endScale;

        public Color startColor;
        public Color endColor;

        public float rotation;
        public float angularVelocity;

        public float duration;
        public float elapsed;
    }

    private class PooledDamageText
    {
        public GameObject gameObject;
        public Transform transform;
        public TextMeshPro textMesh;
        public bool active;

        public Vector3 position;
        public Vector3 velocity;
        public Color baseColor;
        public float baseScale;
        public float duration;
        public float elapsed;
    }

    private readonly List<PooledParticle> particlePool = new List<PooledParticle>();
    private readonly List<PooledDamageText> textPool = new List<PooledDamageText>();
    private int nextParticleIndex = 0;
    private int nextTextIndex = 0;

    // Procedural Pixel Art Sprites (Point Filtered)
    private Sprite pixelSquareSprite;
    private Sprite pixelDiamondSprite;
    private Sprite pixelCircleSprite;
    private Sprite pixelRingSprite;
    private Sprite pixelShardSprite;
    private Sprite pixelBoltSprite;

    private Material sharedTrailMaterial;

    // Camera Shake state
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 lastShakeOffset = Vector3.zero;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        GenerateProceduralPixelSprites();
        InitializePools();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private bool IsVfxEnabled()
    {
        return PlayerPrefs.GetInt("Settings_VFX", 1) == 1;
    }

    private void GenerateProceduralPixelSprites()
    {
        // 1. 4x4 Pixel Square
        Texture2D sqTex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] sqCols = new Color[16];
        for (int i = 0; i < 16; i++) sqCols[i] = Color.white;
        sqTex.SetPixels(sqCols);
        sqTex.Apply();
        pixelSquareSprite = Sprite.Create(sqTex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

        // 2. 8x8 Pixel Diamond / Star
        Texture2D diaTex = new Texture2D(8, 8, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                float dx = Mathf.Abs(x - 3.5f);
                float dy = Mathf.Abs(y - 3.5f);
                bool inside = (dx + dy) <= 3.6f;
                diaTex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        diaTex.Apply();
        pixelDiamondSprite = Sprite.Create(diaTex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);

        // 3. 16x16 Pixel Circle
        Texture2D cirTex = new Texture2D(16, 16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                if (dist <= 6.2f)
                    cirTex.SetPixel(x, y, Color.white);
                else if (dist <= 7.4f)
                    cirTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.6f));
                else
                    cirTex.SetPixel(x, y, Color.clear);
            }
        }
        cirTex.Apply();
        pixelCircleSprite = Sprite.Create(cirTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);

        // 4. 32x32 Pixel Shockwave Ring
        Texture2D ringTex = new Texture2D(32, 32, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                if (dist >= 11.5f && dist <= 14.8f)
                    ringTex.SetPixel(x, y, Color.white);
                else if (dist >= 9.8f && dist < 11.5f)
                    ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.35f));
                else
                    ringTex.SetPixel(x, y, Color.clear);
            }
        }
        ringTex.Apply();
        pixelRingSprite = Sprite.Create(ringTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);

        // 5. 12x6 Sharp Crystal Shard
        Texture2D shardTex = new Texture2D(12, 6, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 12; x++)
            {
                float normX = Mathf.Abs(x - 5.5f) / 5.5f;
                float maxHalfY = (1f - normX) * 2.8f;
                bool inside = Mathf.Abs(y - 2.5f) <= maxHalfY;
                shardTex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        shardTex.Apply();
        pixelShardSprite = Sprite.Create(shardTex, new Rect(0, 0, 12, 6), new Vector2(0.5f, 0.5f), 16f);

        // 6. 20x8 Jagged Pixel Lightning Bolt
        Texture2D boltTex = new Texture2D(20, 8, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 20; x++)
                boltTex.SetPixel(x, y, Color.clear);

        int[] centerYs = { 4, 4, 5, 6, 5, 4, 3, 2, 3, 4, 5, 6, 5, 4, 3, 2, 3, 4, 4, 4 };
        for (int x = 0; x < 20; x++)
        {
            int cy = centerYs[x];
            boltTex.SetPixel(x, cy, Color.white);
            if (cy + 1 < 8) boltTex.SetPixel(x, cy + 1, new Color(1f, 1f, 1f, 0.75f));
        }
        boltTex.Apply();
        pixelBoltSprite = Sprite.Create(boltTex, new Rect(0, 0, 20, 8), new Vector2(0.5f, 0.5f), 16f);

        // Shared Trail Material
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            sharedTrailMaterial = new Material(spriteShader);
        }
    }

    private void InitializePools()
    {
        Transform particleRoot = new GameObject("PooledParticles").transform;
        particleRoot.SetParent(transform, false);

        for (int i = 0; i < maxParticles; i++)
        {
            GameObject go = new GameObject($"FX_P_{i}");
            go.transform.SetParent(particleRoot, false);
            go.SetActive(false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 9;

            particlePool.Add(new PooledParticle
            {
                gameObject = go,
                transform = go.transform,
                renderer = sr,
                active = false
            });
        }

        Transform textRoot = new GameObject("PooledDamageTexts").transform;
        textRoot.SetParent(transform, false);

        for (int i = 0; i < maxFloatingTexts; i++)
        {
            GameObject go = new GameObject($"DmgText_{i}");
            go.transform.SetParent(textRoot, false);
            go.SetActive(false);

            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 4.2f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.sortingOrder = 15;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(4f, 1.5f);

            textPool.Add(new PooledDamageText
            {
                gameObject = go,
                transform = go.transform,
                textMesh = tmp,
                active = false
            });
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 1. Update pooled particles
        for (int i = 0; i < particlePool.Count; i++)
        {
            PooledParticle p = particlePool[i];
            if (!p.active) continue;

            p.elapsed += dt;
            if (p.elapsed >= p.duration)
            {
                p.active = false;
                p.gameObject.SetActive(false);
                continue;
            }

            float t = p.elapsed / p.duration;

            // Physics
            if (p.drag > 0f)
            {
                p.velocity = Vector3.Lerp(p.velocity, Vector3.zero, p.drag * dt);
            }
            if (p.gravity != 0f)
            {
                p.velocity.y -= p.gravity * dt;
            }

            p.position += p.velocity * dt;
            p.rotation += p.angularVelocity * dt;

            p.transform.position = p.position;
            p.transform.rotation = Quaternion.Euler(0f, 0f, p.rotation);
            p.transform.localScale = Vector3.Lerp(p.startScale, p.endScale, t);
            p.renderer.color = Color.Lerp(p.startColor, p.endColor, t);
        }

        // 2. Update floating damage texts
        for (int i = 0; i < textPool.Count; i++)
        {
            PooledDamageText dtItem = textPool[i];
            if (!dtItem.active) continue;

            dtItem.elapsed += dt;
            if (dtItem.elapsed >= dtItem.duration)
            {
                dtItem.active = false;
                dtItem.gameObject.SetActive(false);
                continue;
            }

            float t = dtItem.elapsed / dtItem.duration;
            dtItem.position += dtItem.velocity * dt;
            dtItem.velocity = Vector3.Lerp(dtItem.velocity, new Vector3(0f, 0.8f, 0f), dt * 6f);

            // Pop scale curve: quick punch up then settle
            float scaleMult = t < 0.2f
                ? Mathf.Lerp(0.6f, 1.25f, t / 0.2f)
                : Mathf.Lerp(1.25f, 0.9f, (t - 0.2f) / 0.8f);

            dtItem.transform.position = dtItem.position;
            dtItem.transform.localScale = Vector3.one * (dtItem.baseScale * scaleMult);

            Color c = dtItem.baseColor;
            c.a = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            dtItem.textMesh.color = c;
        }

        // 3. Update Camera Shake
        UpdateCameraShake(dt);
    }

    private void SpawnParticle(
        Sprite sprite,
        Vector3 pos,
        Vector3 vel,
        Vector3 startScale,
        Vector3 endScale,
        Color startColor,
        Color endColor,
        float duration,
        float rotation = 0f,
        float angularVel = 0f,
        float drag = 4f,
        float gravity = 0f,
        int sortingOrder = 9)
    {
        if (!IsVfxEnabled() || particlePool.Count == 0) return;

        PooledParticle p = particlePool[nextParticleIndex];
        nextParticleIndex = (nextParticleIndex + 1) % particlePool.Count;

        p.active = true;
        p.position = pos;
        p.velocity = vel;
        p.drag = drag;
        p.gravity = gravity;
        p.startScale = startScale;
        p.endScale = endScale;
        p.startColor = startColor;
        p.endColor = endColor;
        p.rotation = rotation;
        p.angularVelocity = angularVel;
        p.duration = Mathf.Max(0.02f, duration);
        p.elapsed = 0f;

        p.renderer.sprite = sprite;
        p.renderer.color = startColor;
        p.renderer.sortingOrder = sortingOrder;

        p.transform.position = pos;
        p.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        p.transform.localScale = startScale;
        p.gameObject.SetActive(true);
    }

    // =========================================================================
    // FLOATING DAMAGE TEXT & CAMERA SHAKE
    // =========================================================================

    public void SpawnDamageNumber(Vector3 worldPos, int damage, SpellElementType element = SpellElementType.Arcane)
    {
        if (!enableFloatingDamage || !IsVfxEnabled() || textPool.Count == 0 || damage <= 0) return;

        PooledDamageText item = textPool[nextTextIndex];
        nextTextIndex = (nextTextIndex + 1) % textPool.Count;

        Color col = GetElementColor(element);
        float scale = element == SpellElementType.Fire ? 1.15f : 0.95f;
        if (damage >= 50) scale *= 1.15f;

        Vector3 jitter = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.15f, 0.35f), 0f);

        item.active = true;
        item.position = worldPos + jitter;
        item.velocity = new Vector3(Random.Range(-0.6f, 0.6f), 2.4f, 0f);
        item.baseColor = col;
        item.baseScale = scale;
        item.duration = 0.55f;
        item.elapsed = 0f;

        item.textMesh.text = damage.ToString();
        item.textMesh.color = col;
        item.transform.position = item.position;
        item.transform.localScale = Vector3.one * (scale * 0.6f);
        item.gameObject.SetActive(true);
    }

    public static Color GetElementColor(SpellElementType element)
    {
        switch (element)
        {
            case SpellElementType.Fire:
                return new Color(1f, 0.45f, 0.15f, 1f);
            case SpellElementType.Ice:
                return new Color(0.35f, 0.95f, 1f, 1f);
            case SpellElementType.Void:
                return new Color(0.85f, 0.42f, 1f, 1f);
            case SpellElementType.Shield:
                return new Color(0.35f, 0.75f, 1f, 1f);
            default:
                return new Color(1f, 0.95f, 0.6f, 1f);
        }
    }

    public void TriggerCameraShake(float duration = 0.12f, float magnitude = 0.1f)
    {
        if (!enableCameraShake || !IsVfxEnabled()) return;
        if (magnitude >= shakeMagnitude || shakeTimer <= 0f)
        {
            shakeDuration = duration;
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }
    }

    private void UpdateCameraShake(float dt)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Remove previous frame's offset first
        if (lastShakeOffset != Vector3.zero)
        {
            cam.transform.position -= lastShakeOffset;
            lastShakeOffset = Vector3.zero;
        }

        if (shakeTimer > 0f)
        {
            shakeTimer -= dt;
            float fade = Mathf.Clamp01(shakeTimer / Mathf.Max(0.01f, shakeDuration));
            Vector2 rand = Random.insideUnitCircle * (shakeMagnitude * fade);
            lastShakeOffset = new Vector3(rand.x, rand.y, 0f);
            cam.transform.position += lastShakeOffset;
        }
    }

    // =========================================================================
    // TRAIL RENDERER HELPER
    // =========================================================================

    public TrailRenderer AttachPixelTrail(GameObject target, Color startColor, Color endColor, float startWidth, float time)
    {
        if (target == null || !IsVfxEnabled()) return null;

        TrailRenderer tr = target.GetComponent<TrailRenderer>();
        if (tr == null) tr = target.AddComponent<TrailRenderer>();

        if (sharedTrailMaterial != null)
        {
            tr.sharedMaterial = sharedTrailMaterial;
        }

        tr.time = time;
        tr.startWidth = startWidth;
        tr.endWidth = 0.02f;
        tr.minVertexDistance = 0.08f;
        tr.numCapVertices = 0;
        tr.numCornerVertices = 0;
        tr.sortingOrder = 6;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(endColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        tr.colorGradient = grad;
        return tr;
    }

    // =========================================================================
    // 1. ĐẠN PHÉP CƠ BẢN (ARCANE BOLT FX)
    // =========================================================================

    public void AttachArcaneBoltTrail(GameObject bulletObj)
    {
        AttachPixelTrail(
            bulletObj,
            new Color(1f, 0.92f, 0.35f, 0.75f),
            new Color(0.2f, 0.85f, 1f, 0f),
            0.22f,
            0.14f
        );
    }

    public void SpawnArcaneFlightSpark(Vector3 pos, Vector2 moveDir)
    {
        Vector2 perp = new Vector2(-moveDir.y, moveDir.x) * Random.Range(-0.6f, 0.6f);
        Vector3 vel = (Vector3)(-moveDir * Random.Range(0.5f, 1.5f) + perp);

        SpawnParticle(
            pixelDiamondSprite,
            pos + (Vector3)(Random.insideUnitCircle * 0.08f),
            vel,
            Vector3.one * 0.45f,
            Vector3.zero,
            new Color(1f, 0.95f, 0.45f, 0.9f),
            new Color(0.3f, 0.9f, 1f, 0f),
            0.18f,
            Random.Range(0f, 360f),
            180f,
            5f
        );
    }

    public void SpawnArcaneHitImpact(Vector3 pos, Vector2 moveDir)
    {
        // Mini impact flash ring
        SpawnParticle(
            pixelRingSprite,
            pos,
            Vector3.zero,
            Vector3.one * 0.25f,
            Vector3.one * 0.85f,
            new Color(1f, 0.95f, 0.5f, 0.85f),
            new Color(0.2f, 0.85f, 1f, 0f),
            0.14f,
            0f,
            0f,
            0f,
            0f,
            10
        );

        // 6 pixel sparks flying out
        Vector2 baseDir = moveDir.sqrMagnitude > 0.01f ? -moveDir.normalized : Vector2.up;
        for (int i = 0; i < 6; i++)
        {
            float angleOffset = Random.Range(-75f, 75f);
            Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
            float spd = Random.Range(2.2f, 5.2f);
            bool isCyan = (i % 2 == 0);

            SpawnParticle(
                isCyan ? pixelDiamondSprite : pixelSquareSprite,
                pos,
                dir * spd,
                Vector3.one * Random.Range(0.55f, 0.8f),
                Vector3.zero,
                isCyan ? new Color(0.4f, 0.95f, 1f, 1f) : new Color(1f, 0.92f, 0.3f, 1f),
                new Color(1f, 0.6f, 0.1f, 0f),
                Random.Range(0.16f, 0.26f),
                Random.Range(0f, 360f),
                Random.Range(-240f, 240f),
                8f,
                0f,
                10
            );
        }
    }

    // =========================================================================
    // 2. CẦU LỬA (FIREBALL FX)
    // =========================================================================

    public void AttachFireballTrail(GameObject fireballObj)
    {
        AttachPixelTrail(
            fireballObj,
            new Color(1f, 0.65f, 0.1f, 0.85f),
            new Color(0.85f, 0.15f, 0.05f, 0f),
            0.42f,
            0.22f
        );
    }

    public void SpawnFireballFlightEmber(Vector3 pos, Vector2 moveDir)
    {
        Vector2 offset = Random.insideUnitCircle * 0.18f;
        Vector3 vel = (Vector3)(-moveDir * Random.Range(1.0f, 2.5f) + Random.insideUnitCircle * 0.8f);

        // Fire ember
        SpawnParticle(
            pixelSquareSprite,
            pos + (Vector3)offset,
            vel,
            Vector3.one * Random.Range(0.6f, 0.95f),
            Vector3.zero,
            new Color(1f, 0.85f, 0.2f, 0.95f),
            new Color(0.9f, 0.15f, 0.05f, 0f),
            Random.Range(0.18f, 0.28f),
            Random.Range(0f, 360f),
            180f,
            4f
        );

        // Subtle smoke puff
        if (Random.value < 0.45f)
        {
            SpawnParticle(
                pixelCircleSprite,
                pos - (Vector3)(moveDir * 0.15f),
                (Vector3)(Random.insideUnitCircle * 0.5f),
                Vector3.one * 0.35f,
                Vector3.one * 0.75f,
                new Color(0.3f, 0.22f, 0.2f, 0.45f),
                new Color(0.15f, 0.12f, 0.12f, 0f),
                0.28f,
                0f,
                0f,
                3f,
                0f,
                6
            );
        }
    }

    public void SpawnFireballExplosion(Vector3 pos, float radius)
    {
        TriggerCameraShake(0.14f, 0.11f);

        // 1. Ground scorch mark (fades out slowly underneath)
        SpawnParticle(
            pixelCircleSprite,
            pos,
            Vector3.zero,
            Vector3.one * (radius * 1.1f),
            Vector3.one * (radius * 1.25f),
            new Color(0.12f, 0.08f, 0.06f, 0.55f),
            new Color(0.08f, 0.06f, 0.05f, 0f),
            0.5f,
            0f,
            0f,
            0f,
            0f,
            2
        );

        // 2. Expanding fiery shockwave ring
        SpawnParticle(
            pixelRingSprite,
            pos,
            Vector3.zero,
            Vector3.one * 0.35f,
            Vector3.one * (radius * 1.15f),
            new Color(1f, 0.9f, 0.3f, 0.95f),
            new Color(1f, 0.2f, 0.05f, 0f),
            0.26f,
            0f,
            90f,
            0f,
            0f,
            10
        );

        // 3. Bright explosion core flash
        SpawnParticle(
            pixelCircleSprite,
            pos,
            Vector3.zero,
            Vector3.one * (radius * 0.6f),
            Vector3.one * (radius * 1.4f),
            new Color(1f, 0.95f, 0.6f, 0.9f),
            new Color(1f, 0.3f, 0.05f, 0f),
            0.20f,
            0f,
            0f,
            0f,
            0f,
            9
        );

        // 4. 14 flying fire embers & sparks
        for (int i = 0; i < 14; i++)
        {
            float angle = (i * (360f / 14f) + Random.Range(-12f, 12f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float spd = Random.Range(3.0f, 7.5f);

            SpawnParticle(
                (i % 3 == 0) ? pixelDiamondSprite : pixelSquareSprite,
                pos + (Vector3)(dir * 0.15f),
                dir * spd,
                Vector3.one * Random.Range(0.75f, 1.2f),
                Vector3.zero,
                new Color(1f, 0.9f, 0.25f, 1f),
                new Color(0.95f, 0.15f, 0.02f, 0f),
                Random.Range(0.25f, 0.42f),
                Random.Range(0f, 360f),
                Random.Range(-300f, 300f),
                6f,
                1.5f,
                11
            );
        }

        // 5. 6 dark-orange smoke puffs
        for (int i = 0; i < 6; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            SpawnParticle(
                pixelCircleSprite,
                pos + (Vector3)(dir * Random.Range(0.1f, 0.4f)),
                dir * Random.Range(1.2f, 2.8f) + Vector2.up * 0.8f,
                Vector3.one * 0.45f,
                Vector3.one * 1.05f,
                new Color(0.45f, 0.22f, 0.15f, 0.65f),
                new Color(0.15f, 0.12f, 0.12f, 0f),
                Random.Range(0.32f, 0.48f),
                0f,
                0f,
                3.5f,
                0f,
                8
            );
        }
    }

    // =========================================================================
    // 3. BĂNG TIỄN (ICE SPEAR FX)
    // =========================================================================

    public void AttachIceSpearTrail(GameObject spearObj)
    {
        AttachPixelTrail(
            spearObj,
            new Color(0.55f, 0.95f, 1f, 0.8f),
            new Color(0.15f, 0.6f, 1f, 0f),
            0.28f,
            0.18f
        );
    }

    public void SpawnIceSpearFlightCrystal(Vector3 pos, Vector2 moveDir)
    {
        Vector2 offset = Random.insideUnitCircle * 0.12f;
        Vector3 vel = (Vector3)(-moveDir * Random.Range(0.6f, 1.6f) + Random.insideUnitCircle * 0.5f);

        SpawnParticle(
            pixelDiamondSprite,
            pos + (Vector3)offset,
            vel,
            Vector3.one * Random.Range(0.45f, 0.7f),
            Vector3.zero,
            new Color(0.75f, 0.98f, 1f, 0.9f),
            new Color(0.2f, 0.75f, 1f, 0f),
            0.24f,
            Random.Range(0f, 360f),
            220f,
            4f
        );
    }

    public void SpawnIcePierceImpact(Vector3 pos, Vector2 moveDir)
    {
        // Frost ring ripple
        SpawnParticle(
            pixelRingSprite,
            pos,
            Vector3.zero,
            Vector3.one * 0.2f,
            Vector3.one * 0.95f,
            new Color(0.7f, 0.98f, 1f, 0.85f),
            new Color(0.2f, 0.75f, 1f, 0f),
            0.18f,
            0f,
            0f,
            0f,
            0f,
            10
        );

        // 7 sharp ice shards shattering forward and sideways
        Vector2 forward = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized : Vector2.right;
        for (int i = 0; i < 7; i++)
        {
            float spread = Random.Range(-65f, 65f);
            Vector2 dir = Quaternion.Euler(0, 0, spread) * forward;
            float spd = Random.Range(2.8f, 6.2f);
            float rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            SpawnParticle(
                (i % 2 == 0) ? pixelShardSprite : pixelDiamondSprite,
                pos,
                dir * spd,
                Vector3.one * Random.Range(0.6f, 0.95f),
                Vector3.zero,
                new Color(0.85f, 1f, 1f, 1f),
                new Color(0.2f, 0.8f, 1f, 0f),
                Random.Range(0.18f, 0.30f),
                rot,
                Random.Range(-180f, 180f),
                7f,
                0f,
                11
            );
        }
    }

    public void SpawnFrostSlowAuraMote(Vector3 enemyPos)
    {
        Vector2 offset = Random.insideUnitCircle * 0.35f;
        SpawnParticle(
            pixelDiamondSprite,
            enemyPos + (Vector3)offset,
            new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.8f, -0.3f), 0f),
            Vector3.one * 0.42f,
            Vector3.zero,
            new Color(0.65f, 0.95f, 1f, 0.85f),
            new Color(0.25f, 0.75f, 1f, 0f),
            0.35f,
            Random.Range(0f, 360f),
            120f,
            1f,
            0f,
            9
        );
    }

    // =========================================================================
    // 4. QUẢ CẦU HẮC ÁM (DARK ORB FX)
    // =========================================================================

    public void AttachDarkOrbTrail(GameObject orbObj)
    {
        AttachPixelTrail(
            orbObj,
            new Color(0.75f, 0.25f, 1f, 0.7f),
            new Color(0.25f, 0.05f, 0.45f, 0f),
            0.34f,
            0.25f
        );
    }

    public void SpawnDarkOrbVoidMote(Vector3 orbPos)
    {
        Vector2 offset = Random.insideUnitCircle * 0.16f;
        SpawnParticle(
            pixelCircleSprite,
            orbPos + (Vector3)offset,
            (Vector3)(Random.insideUnitCircle * 0.6f),
            Vector3.one * 0.32f,
            Vector3.zero,
            new Color(0.6f, 0.15f, 0.95f, 0.75f),
            new Color(0.15f, 0.02f, 0.25f, 0f),
            0.25f,
            0f,
            0f,
            3f,
            0f,
            7
        );
    }

    public void SpawnDarkOrbHitImpact(Vector3 orbPos, Vector3 targetPos)
    {
        // 1. Lightning arc between orb and target
        Vector3 mid = (orbPos + targetPos) * 0.5f;
        Vector3 diff = targetPos - orbPos;
        float dist = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        SpawnParticle(
            pixelBoltSprite,
            mid,
            Vector3.zero,
            new Vector3(Mathf.Clamp(dist * 0.9f, 0.5f, 1.8f), 0.9f, 1f),
            new Vector3(Mathf.Clamp(dist * 0.9f, 0.5f, 1.8f), 0.1f, 1f),
            new Color(0.92f, 0.55f, 1f, 1f),
            new Color(0.45f, 0.1f, 0.85f, 0f),
            0.14f,
            angle,
            0f,
            0f,
            0f,
            11
        );

        // 2. Void shockwave ring on target
        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * 0.2f,
            Vector3.one * 0.95f,
            new Color(0.85f, 0.35f, 1f, 0.9f),
            new Color(0.25f, 0.05f, 0.5f, 0f),
            0.2f,
            0f,
            -180f,
            0f,
            0f,
            10
        );

        // 3. 6 void sparks
        for (int i = 0; i < 6; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            SpawnParticle(
                pixelDiamondSprite,
                targetPos,
                dir * Random.Range(2.0f, 4.5f),
                Vector3.one * 0.65f,
                Vector3.zero,
                new Color(0.9f, 0.4f, 1f, 1f),
                new Color(0.2f, 0.0f, 0.4f, 0f),
                0.22f,
                Random.Range(0f, 360f),
                240f,
                6f,
                0f,
                10
            );
        }
    }

    // =========================================================================
    // 5. KHIÊN MA PHÁP (MAGIC SHIELD FX)
    // =========================================================================

    public void SpawnShieldBlockImpact(Vector3 playerPos)
    {
        TriggerCameraShake(0.08f, 0.06f);

        // Mana pulse ring
        SpawnParticle(
            pixelRingSprite,
            playerPos,
            Vector3.zero,
            Vector3.one * 0.7f,
            Vector3.one * 1.6f,
            new Color(0.45f, 0.9f, 1f, 0.9f),
            new Color(0.15f, 0.5f, 1f, 0f),
            0.22f,
            0f,
            0f,
            0f,
            0f,
            11
        );

        // 10 mana sparks
        for (int i = 0; i < 10; i++)
        {
            float angle = (i * 36f + Random.Range(-10f, 10f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnParticle(
                pixelDiamondSprite,
                playerPos + (Vector3)(dir * 0.6f),
                dir * Random.Range(2.5f, 5.0f),
                Vector3.one * 0.65f,
                Vector3.zero,
                new Color(0.6f, 0.95f, 1f, 1f),
                new Color(0.2f, 0.6f, 1f, 0f),
                0.24f,
                Random.Range(0f, 360f),
                180f,
                6f,
                0f,
                11
            );
        }
    }

    public void SpawnShieldBreakBlastFX(Vector3 playerPos, float blastRadius)
    {
        TriggerCameraShake(0.18f, 0.15f);

        // Double shockwave ring
        SpawnParticle(
            pixelRingSprite,
            playerPos,
            Vector3.zero,
            Vector3.one * 0.5f,
            Vector3.one * (blastRadius * 1.15f),
            new Color(0.55f, 0.95f, 1f, 0.95f),
            new Color(0.15f, 0.55f, 1f, 0f),
            0.32f,
            0f,
            120f,
            0f,
            0f,
            11
        );

        // 16 high-velocity crystal shards
        for (int i = 0; i < 16; i++)
        {
            float angle = (i * (360f / 16f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float rot = angle * Mathf.Rad2Deg;

            SpawnParticle(
                pixelShardSprite,
                playerPos + (Vector3)(dir * 0.5f),
                dir * Random.Range(5.0f, 9.5f),
                Vector3.one * 0.9f,
                Vector3.zero,
                new Color(0.75f, 0.98f, 1f, 1f),
                new Color(0.2f, 0.65f, 1f, 0f),
                0.32f,
                rot,
                0f,
                5f,
                0f,
                11
            );
        }
    }

    // =========================================================================
    // 6. QUÁI VẬT CHẾT (ENEMY DEATH PIXEL BURST)
    // =========================================================================

    public void SpawnEnemyDeathBurst(Vector3 pos)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = (i * 45f + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnParticle(
                pixelSquareSprite,
                pos,
                dir * Random.Range(1.8f, 4.2f),
                Vector3.one * Random.Range(0.65f, 0.95f),
                Vector3.zero,
                new Color(0.95f, 0.25f, 0.35f, 0.95f),
                new Color(0.35f, 0.05f, 0.2f, 0f),
                Random.Range(0.20f, 0.32f),
                Random.Range(0f, 360f),
                Random.Range(-200f, 200f),
                6f,
                0f,
                8
            );
        }
    }
}
