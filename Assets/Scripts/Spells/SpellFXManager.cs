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

public enum BossThemeType
{
    SkyDragon,
    StormColossus,
    AbyssalTitan
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
    [SerializeField] private int maxParticles = 280;

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

    // Procedural Boss VFX & Projectile Sprites (Point Filtered)
    private Sprite pixelRuneCircleSprite;
    private Sprite pixelSkyLightningSprite;
    private Sprite pixelCraterCrackSprite;
    private Sprite pixelLaserBeamSprite;
    private Sprite dragonFireOrbSprite;
    private Sprite stormPlasmaOrbSprite;
    private Sprite abyssVoidOrbSprite;

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

        // 7. 48x48 Pixel Rune Magic Circle (for Boss Telegraphs, Summon Portals & AoE Sigils)
        Texture2D runeTex = new Texture2D(48, 48, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Vector2 rc = new Vector2(23.5f, 23.5f);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                Vector2 p = new Vector2(x, y) - rc;
                float dist = p.magnitude;
                float angle = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;

                bool outerRing = dist >= 20.5f && dist <= 22.8f;
                bool innerRing = dist >= 14.5f && dist <= 16.0f;
                bool runeNotch = (dist >= 16.0f && dist <= 20.5f) && ((Mathf.RoundToInt(angle) % 45) <= 4);
                bool starSpoke = (dist >= 6.0f && dist <= 14.5f) && ((Mathf.RoundToInt(angle) % 90) <= 3 || (Mathf.RoundToInt(angle + 45f) % 90) <= 2);
                bool coreRing = dist >= 4.5f && dist <= 6.2f;

                if (outerRing || innerRing || coreRing)
                    runeTex.SetPixel(x, y, Color.white);
                else if (runeNotch || starSpoke)
                    runeTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.85f));
                else if (dist < 20.5f)
                    runeTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.14f));
                else
                    runeTex.SetPixel(x, y, Color.clear);
            }
        }
        runeTex.Apply();
        pixelRuneCircleSprite = Sprite.Create(runeTex, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), 16f);

        // 8. 24x96 Towering Sky-Lightning Pillar (for Storm Colossus & Cataclysm)
        Texture2D skyBoltTex = new Texture2D(24, 96, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 96; y++)
            for (int x = 0; x < 24; x++)
                skyBoltTex.SetPixel(x, y, Color.clear);

        int curX = 12;
        for (int y = 95; y >= 0; y--)
        {
            if (y % 5 == 0)
            {
                int shift = ((y / 5) % 2 == 0) ? 2 : -2;
                curX = Mathf.Clamp(curX + shift, 5, 18);
            }
            // Bright white core (3px wide) + glow (7px wide)
            for (int dx = -3; dx <= 3; dx++)
            {
                int px = curX + dx;
                if (px < 0 || px >= 24) continue;
                int absDx = Mathf.Abs(dx);
                if (absDx <= 1)
                    skyBoltTex.SetPixel(px, y, Color.white);
                else
                    skyBoltTex.SetPixel(px, y, new Color(1f, 1f, 1f, 0.55f));
            }
            // Side branches every 18 pixels
            if (y % 18 == 0 && y > 15 && y < 85)
            {
                int bDir = (y % 36 == 0) ? 1 : -1;
                for (int b = 1; b <= 6; b++)
                {
                    int bx = curX + b * bDir;
                    int by = y - b * 2;
                    if (bx >= 0 && bx < 24 && by >= 0 && by < 96)
                        skyBoltTex.SetPixel(bx, by, new Color(1f, 1f, 1f, 0.8f));
                }
            }
        }
        skyBoltTex.Apply();
        pixelSkyLightningSprite = Sprite.Create(skyBoltTex, new Rect(0, 0, 24, 96), new Vector2(0.5f, 0.05f), 16f);

        // 9. 48x48 Ground Crater Fissure Cracks (for Fist Slam & Meteor Rain)
        Texture2D crackTex = new Texture2D(48, 48, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 48; x++)
                crackTex.SetPixel(x, y, Color.clear);

        float[] crackAngles = { 15f, 68f, 125f, 175f, 222f, 278f, 330f };
        foreach (float ang in crackAngles)
        {
            float rad = ang * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 perp = new Vector2(-dir.y, dir.x);
            for (int r = 2; r < 22; r++)
            {
                float jag = ((r / 4) % 2 == 0) ? 1.2f : -1.2f;
                Vector2 pt = rc + dir * r + perp * jag;
                int px = Mathf.RoundToInt(pt.x);
                int py = Mathf.RoundToInt(pt.y);
                if (px >= 1 && px < 47 && py >= 1 && py < 47)
                {
                    crackTex.SetPixel(px, py, Color.white);
                    crackTex.SetPixel(px + 1, py, new Color(1f, 1f, 1f, 0.7f));
                    crackTex.SetPixel(px, py + 1, new Color(1f, 1f, 1f, 0.7f));
                }
            }
        }
        crackTex.Apply();
        pixelCraterCrackSprite = Sprite.Create(crackTex, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), 16f);

        // 10. 32x16 Pixel Energy Laser Beam Segment (Pivot at Left-Center 0, 0.5)
        Texture2D beamTex = new Texture2D(32, 16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 16; y++)
        {
            float distY = Mathf.Abs(y - 7.5f);
            for (int x = 0; x < 32; x++)
            {
                if (distY <= 2.5f)
                    beamTex.SetPixel(x, y, Color.white);
                else if (distY <= 5.2f)
                    beamTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.78f));
                else if (distY <= 7.2f)
                {
                    bool edgePulse = ((x + y) % 4 != 0);
                    beamTex.SetPixel(x, y, edgePulse ? new Color(1f, 1f, 1f, 0.38f) : Color.clear);
                }
                else
                    beamTex.SetPixel(x, y, Color.clear);
            }
        }
        beamTex.Apply();
        pixelLaserBeamSprite = Sprite.Create(beamTex, new Rect(0, 0, 32, 16), new Vector2(0f, 0.5f), 32f);

        // 11. Custom Boss Projectiles (20x20 Multi-Color Pixel Art)
        dragonFireOrbSprite = CreateBossOrbSprite(
            new Color(1f, 0.98f, 0.75f, 1f),
            new Color(1f, 0.52f, 0.08f, 1f),
            new Color(0.85f, 0.12f, 0.04f, 0.9f)
        );
        stormPlasmaOrbSprite = CreateBossOrbSprite(
            new Color(0.95f, 1f, 1f, 1f),
            new Color(0.2f, 0.85f, 1f, 1f),
            new Color(0.1f, 0.38f, 0.95f, 0.9f)
        );
        abyssVoidOrbSprite = CreateBossOrbSprite(
            new Color(0.12f, 0.02f, 0.22f, 1f),
            new Color(0.92f, 0.22f, 1f, 1f),
            new Color(0.45f, 0.05f, 0.75f, 0.9f)
        );

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

    // =========================================================================
    // 7. HỆ THỐNG HIỆU ỨNG KỸ NĂNG 3 ĐẠI BOSS (BOSS SKILL VFX)
    // =========================================================================

    private Sprite CreateBossOrbSprite(Color coreCol, Color midCol, Color rimCol)
    {
        Texture2D tex = new Texture2D(20, 20, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Vector2 c = new Vector2(9.5f, 9.5f);
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                Vector2 d = new Vector2(x, y) - c;
                float dist = d.magnitude;
                bool spike = (Mathf.Abs(d.x) <= 1.6f && dist <= 9.2f) || (Mathf.Abs(d.y) <= 1.6f && dist <= 9.2f) ||
                             (Mathf.Abs(Mathf.Abs(d.x) - Mathf.Abs(d.y)) <= 1.2f && dist <= 8.4f);

                if (dist <= 3.6f)
                    tex.SetPixel(x, y, coreCol);
                else if (dist <= 6.4f)
                    tex.SetPixel(x, y, midCol);
                else if (dist <= 8.0f || spike)
                    tex.SetPixel(x, y, rimCol);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 20, 20), new Vector2(0.5f, 0.5f), 16f);
    }

    public Sprite GetBossProjectileSprite(BossThemeType theme)
    {
        switch (theme)
        {
            case BossThemeType.StormColossus:
                return stormPlasmaOrbSprite != null ? stormPlasmaOrbSprite : pixelDiamondSprite;
            case BossThemeType.AbyssalTitan:
                return abyssVoidOrbSprite != null ? abyssVoidOrbSprite : pixelDiamondSprite;
            default:
                return dragonFireOrbSprite != null ? dragonFireOrbSprite : pixelCircleSprite;
        }
    }

    public Sprite GetPixelRuneCircleSprite() => pixelRuneCircleSprite;

    public void AttachBossProjectileTrail(GameObject projObj, BossThemeType theme)
    {
        if (projObj == null) return;
        switch (theme)
        {
            case BossThemeType.StormColossus:
                AttachPixelTrail(
                    projObj,
                    new Color(0.45f, 0.95f, 1f, 0.9f),
                    new Color(0.1f, 0.35f, 0.95f, 0f),
                    0.34f,
                    0.22f
                );
                break;
            case BossThemeType.AbyssalTitan:
                AttachPixelTrail(
                    projObj,
                    new Color(0.9f, 0.25f, 1f, 0.9f),
                    new Color(0.25f, 0.02f, 0.45f, 0f),
                    0.36f,
                    0.24f
                );
                break;
            default:
                AttachPixelTrail(
                    projObj,
                    new Color(1f, 0.65f, 0.12f, 0.9f),
                    new Color(0.85f, 0.12f, 0.04f, 0f),
                    0.38f,
                    0.22f
                );
                break;
        }
    }

    public void SpawnBossProjectileFlightFX(Vector3 pos, Vector2 moveDir, BossThemeType theme)
    {
        Vector2 offset = Random.insideUnitCircle * 0.16f;
        Vector3 vel = (Vector3)(-moveDir * Random.Range(0.8f, 2.2f) + Random.insideUnitCircle * 0.6f);

        switch (theme)
        {
            case BossThemeType.StormColossus:
                SpawnParticle(
                    pixelDiamondSprite,
                    pos + (Vector3)offset,
                    vel,
                    Vector3.one * Random.Range(0.45f, 0.75f),
                    Vector3.zero,
                    new Color(0.75f, 0.98f, 1f, 0.95f),
                    new Color(0.15f, 0.55f, 1f, 0f),
                    0.20f,
                    Random.Range(0f, 360f),
                    240f,
                    4f,
                    0f,
                    12
                );
                break;
            case BossThemeType.AbyssalTitan:
                SpawnParticle(
                    pixelDiamondSprite,
                    pos + (Vector3)offset,
                    vel,
                    Vector3.one * Random.Range(0.5f, 0.8f),
                    Vector3.zero,
                    new Color(0.95f, 0.35f, 1f, 0.9f),
                    new Color(0.2f, 0.02f, 0.38f, 0f),
                    0.22f,
                    Random.Range(0f, 360f),
                    -200f,
                    4f,
                    0f,
                    12
                );
                break;
            default:
                SpawnParticle(
                    pixelSquareSprite,
                    pos + (Vector3)offset,
                    vel,
                    Vector3.one * Random.Range(0.55f, 0.85f),
                    Vector3.zero,
                    new Color(1f, 0.85f, 0.25f, 0.95f),
                    new Color(0.9f, 0.15f, 0.05f, 0f),
                    0.20f,
                    Random.Range(0f, 360f),
                    180f,
                    4f,
                    0f,
                    12
                );
                break;
        }
    }

    public void SpawnBossProjectileHitFX(Vector3 pos, Vector2 moveDir, BossThemeType theme)
    {
        Color ringStart, ringEnd, sparkStart, sparkEnd;
        switch (theme)
        {
            case BossThemeType.StormColossus:
                ringStart = new Color(0.65f, 0.98f, 1f, 0.95f);
                ringEnd = new Color(0.15f, 0.5f, 1f, 0f);
                sparkStart = new Color(0.85f, 1f, 1f, 1f);
                sparkEnd = new Color(0.2f, 0.6f, 1f, 0f);
                break;
            case BossThemeType.AbyssalTitan:
                ringStart = new Color(0.95f, 0.35f, 1f, 0.95f);
                ringEnd = new Color(0.35f, 0.05f, 0.65f, 0f);
                sparkStart = new Color(1f, 0.55f, 1f, 1f);
                sparkEnd = new Color(0.25f, 0.02f, 0.45f, 0f);
                break;
            default:
                ringStart = new Color(1f, 0.85f, 0.25f, 0.95f);
                ringEnd = new Color(0.95f, 0.15f, 0.05f, 0f);
                sparkStart = new Color(1f, 0.9f, 0.3f, 1f);
                sparkEnd = new Color(0.9f, 0.15f, 0.05f, 0f);
                break;
        }

        SpawnParticle(
            pixelRingSprite,
            pos,
            Vector3.zero,
            Vector3.one * 0.25f,
            Vector3.one * 1.15f,
            ringStart,
            ringEnd,
            0.22f,
            0f,
            90f,
            0f,
            0f,
            13
        );

        for (int i = 0; i < 8; i++)
        {
            float angle = (i * 45f + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnParticle(
                (theme == BossThemeType.StormColossus && i % 2 == 0) ? pixelBoltSprite : pixelDiamondSprite,
                pos,
                dir * Random.Range(2.5f, 5.5f),
                Vector3.one * Random.Range(0.6f, 0.95f),
                Vector3.zero,
                sparkStart,
                sparkEnd,
                Random.Range(0.18f, 0.28f),
                angle * Mathf.Rad2Deg,
                Random.Range(-180f, 180f),
                6f,
                0f,
                13
            );
        }
    }

    /// <summary>
    /// Hiệu ứng lướt húc tốc độ cao (Dragon Charge): tạo bóng mờ pixel (After-Image) + lốc lửa hai bên.
    /// </summary>
    public void SpawnBossDashAfterImage(SpriteRenderer bossSr, Vector3 pos, Vector3 scale, Vector2 dashDir, BossThemeType theme)
    {
        Color startCol = theme == BossThemeType.StormColossus
            ? new Color(0.35f, 0.9f, 1f, 0.65f)
            : (theme == BossThemeType.AbyssalTitan ? new Color(0.85f, 0.25f, 1f, 0.65f) : new Color(1f, 0.45f, 0.12f, 0.65f));
        Color endCol = new Color(startCol.r, startCol.g, startCol.b, 0f);

        if (bossSr != null && bossSr.sprite != null)
        {
            SpawnParticle(
                bossSr.sprite,
                pos,
                Vector3.zero,
                scale,
                scale * 0.92f,
                startCol,
                endCol,
                0.24f,
                0f,
                0f,
                0f,
                0f,
                Mathf.Max(1, bossSr.sortingOrder - 1)
            );
        }

        // Lốc lửa & bụi văng sang 2 bên sườn khi lao
        Vector2 perp = new Vector2(-dashDir.y, dashDir.x).normalized;
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 vel = (perp * side * Random.Range(2.2f, 4.5f)) - (dashDir * Random.Range(1.0f, 2.5f));
            SpawnParticle(
                pixelDiamondSprite,
                pos + (Vector3)(perp * side * 0.5f),
                vel,
                Vector3.one * Random.Range(0.75f, 1.15f),
                Vector3.zero,
                new Color(1f, 0.85f, 0.25f, 0.95f),
                endCol,
                0.26f,
                Random.Range(0f, 360f),
                240f,
                5f,
                0f,
                11
            );
        }
    }

    /// <summary>
    /// Hiệu ứng tỏa sóng xung kích 360 độ khi Boss tung Wing Sweep / Thunder Shockwave / Spiral Barrage.
    /// </summary>
    public void SpawnBossRadialBurstFX(Vector3 bossPos, float radius, BossThemeType theme)
    {
        TriggerCameraShake(0.16f, 0.12f);

        Color c1, c2;
        switch (theme)
        {
            case BossThemeType.StormColossus:
                c1 = new Color(0.45f, 0.95f, 1f, 0.95f);
                c2 = new Color(0.1f, 0.45f, 1f, 0f);
                break;
            case BossThemeType.AbyssalTitan:
                c1 = new Color(0.95f, 0.3f, 1f, 0.95f);
                c2 = new Color(0.3f, 0.02f, 0.55f, 0f);
                break;
            default:
                c1 = new Color(1f, 0.75f, 0.2f, 0.95f);
                c2 = new Color(0.95f, 0.15f, 0.05f, 0f);
                break;
        }

        // Vòng cổ ngữ xoay dưới chân Boss
        SpawnParticle(
            pixelRuneCircleSprite,
            bossPos,
            Vector3.zero,
            Vector3.one * 0.5f,
            Vector3.one * Mathf.Max(1.6f, radius * 0.65f),
            c1,
            c2,
            0.38f,
            0f,
            180f,
            0f,
            0f,
            5
        );

        // Vòng sóng xung kích bung cực nhanh
        SpawnParticle(
            pixelRingSprite,
            bossPos,
            Vector3.zero,
            Vector3.one * 0.4f,
            Vector3.one * Mathf.Max(2.2f, radius * 1.1f),
            c1,
            c2,
            0.30f,
            0f,
            -120f,
            0f,
            0f,
            12
        );
    }

    /// <summary>
    /// BOSS 1 (SKY DRAGON) - Mưa Thiên Thạch (Meteor Rain):
    /// Thiên thạch lửa lao chéo từ bầu trời xuống + Nứt đất nham thạch + Sóng nhiệt + 18 mảnh nham thạch.
    /// </summary>
    public void SpawnBossMeteorRainFX(Vector3 targetPos, float radius)
    {
        TriggerCameraShake(0.25f, 0.18f);

        // 1. Vệt thiên thạch xé gió lao từ góc trên xuống tâm nổ
        Vector3 skyOffset = new Vector3(2.8f, 7.5f, 0f);
        SpawnParticle(
            dragonFireOrbSprite != null ? dragonFireOrbSprite : pixelCircleSprite,
            targetPos + skyOffset,
            -skyOffset / 0.10f,
            Vector3.one * 2.2f,
            Vector3.one * 1.4f,
            new Color(1f, 0.95f, 0.5f, 1f),
            new Color(1f, 0.35f, 0.05f, 0.9f),
            0.11f,
            0f,
            360f,
            0f,
            0f,
            14
        );

        // 2. Vết nứt đất nham thạch đỏ rực dưới mặt đất
        SpawnParticle(
            pixelCraterCrackSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.75f),
            Vector3.one * (radius * 0.85f),
            new Color(1f, 0.55f, 0.1f, 0.95f),
            new Color(0.25f, 0.05f, 0.02f, 0f),
            0.85f,
            Random.Range(0f, 360f),
            0f,
            0f,
            0f,
            3
        );

        // 3. Vòng cổ ngữ hỏa ngục + Lõi chớp sáng trắng cam
        SpawnParticle(
            pixelRuneCircleSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.4f),
            Vector3.one * (radius * 0.95f),
            new Color(1f, 0.85f, 0.25f, 0.9f),
            new Color(0.95f, 0.12f, 0.02f, 0f),
            0.40f,
            0f,
            90f,
            0f,
            0f,
            6
        );

        SpawnParticle(
            pixelCircleSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.5f),
            Vector3.one * (radius * 1.45f),
            new Color(1f, 0.98f, 0.75f, 0.95f),
            new Color(1f, 0.25f, 0.05f, 0f),
            0.24f,
            0f,
            0f,
            0f,
            0f,
            11
        );

        // 4. Sóng xung kích kép (Double Shockwave Ring)
        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * 0.4f,
            Vector3.one * (radius * 1.35f),
            new Color(1f, 0.92f, 0.35f, 1f),
            new Color(1f, 0.15f, 0.02f, 0f),
            0.32f,
            0f,
            120f,
            0f,
            0f,
            12
        );

        // 5. 18 mảnh đá nham thạch & tàn lửa văng tung tóe
        for (int i = 0; i < 18; i++)
        {
            float angle = (i * 20f + Random.Range(-8f, 8f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float spd = Random.Range(3.5f, 9.0f);

            SpawnParticle(
                (i % 2 == 0) ? pixelShardSprite : pixelDiamondSprite,
                targetPos + (Vector3)(dir * 0.25f),
                dir * spd,
                Vector3.one * Random.Range(0.85f, 1.35f),
                Vector3.zero,
                new Color(1f, 0.92f, 0.3f, 1f),
                new Color(0.9f, 0.12f, 0.02f, 0f),
                Random.Range(0.28f, 0.48f),
                angle * Mathf.Rad2Deg,
                Random.Range(-320f, 320f),
                5.5f,
                2.0f,
                13
            );
        }
    }

    /// <summary>
    /// BOSS 2 (STORM COLOSSUS) - Nắm Đấm Thiên Lôi (Giant Fist Slam):
    /// Chấn động nứt toạc mặt đất phát quang điện + 8 tia hồ quang điện + đá văng.
    /// </summary>
    public void SpawnBossFistSlamFX(Vector3 targetPos, float radius)
    {
        TriggerCameraShake(0.26f, 0.20f);

        // 1. Vết nứt đất phát quang điện xanh lam trắng
        SpawnParticle(
            pixelCraterCrackSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.8f),
            Vector3.one * (radius * 0.92f),
            new Color(0.65f, 0.98f, 1f, 1f),
            new Color(0.1f, 0.4f, 0.95f, 0f),
            0.80f,
            Random.Range(0f, 360f),
            0f,
            0f,
            0f,
            4
        );

        // 2. Sóng chấn động điện từ kép
        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * 0.35f,
            Vector3.one * (radius * 1.35f),
            new Color(0.85f, 1f, 1f, 1f),
            new Color(0.15f, 0.6f, 1f, 0f),
            0.28f,
            0f,
            -140f,
            0f,
            0f,
            12
        );

        // 3. 8 tia hồ quang sét giật lan trên mặt đất
        for (int i = 0; i < 8; i++)
        {
            float deg = i * 45f + Random.Range(-10f, 10f);
            float rad = deg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            SpawnParticle(
                pixelBoltSprite,
                targetPos + (Vector3)(dir * (radius * 0.45f)),
                dir * 2.5f,
                new Vector3(1.45f, 1.15f, 1f),
                new Vector3(1.65f, 0.1f, 1f),
                new Color(0.9f, 1f, 1f, 1f),
                new Color(0.2f, 0.7f, 1f, 0f),
                0.24f,
                deg,
                0f,
                4f,
                0f,
                13
            );
        }

        // 4. 14 mảnh đá & tia lửa điện văng ra
        for (int i = 0; i < 14; i++)
        {
            float rad = (i * (360f / 14f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            SpawnParticle(
                (i % 2 == 0) ? pixelSquareSprite : pixelDiamondSprite,
                targetPos,
                dir * Random.Range(3.5f, 8.0f),
                Vector3.one * Random.Range(0.8f, 1.25f),
                Vector3.zero,
                new Color(0.6f, 0.95f, 1f, 1f),
                new Color(0.15f, 0.45f, 0.9f, 0f),
                Random.Range(0.25f, 0.42f),
                Random.Range(0f, 360f),
                Random.Range(-260f, 260f),
                6f,
                0f,
                12
            );
        }
    }

    /// <summary>
    /// BOSS 2 (STORM COLOSSUS) - Bão Sét Giáng Trần (Lightning Storm):
    /// Cột sét khổng lồ giáng thẳng từ đỉnh trời xuống + trận đồ điện từ + 4 cột sét phụ.
    /// </summary>
    public void SpawnBossLightningStormFX(Vector3 targetPos, float radius)
    {
        TriggerCameraShake(0.24f, 0.18f);

        // 1. Cột thiên lôi chính giáng thẳng vào tâm
        SpawnParticle(
            pixelSkyLightningSprite,
            targetPos,
            Vector3.zero,
            new Vector3(1.8f, 1.6f, 1f),
            new Vector3(0.3f, 1.6f, 1f),
            new Color(0.95f, 1f, 1f, 1f),
            new Color(0.2f, 0.75f, 1f, 0f),
            0.28f,
            0f,
            0f,
            0f,
            0f,
            15
        );

        // 2. 4 cột sét phụ giáng xung quanh bán kính nổ
        for (int i = 0; i < 4; i++)
        {
            float rad = (i * 90f + 45f) * Mathf.Deg2Rad;
            Vector3 subPos = targetPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * (radius * 0.55f);
            SpawnParticle(
                pixelSkyLightningSprite,
                subPos,
                Vector3.zero,
                new Vector3(1.1f, 1.25f, 1f),
                new Vector3(0.2f, 1.25f, 1f),
                new Color(0.65f, 0.98f, 1f, 0.95f),
                new Color(0.15f, 0.55f, 1f, 0f),
                0.24f,
                Random.Range(-6f, 6f),
                0f,
                0f,
                0f,
                14
            );
        }

        // 3. Trận đồ điện từ dưới đất + Vòng shockwave
        SpawnParticle(
            pixelRuneCircleSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.65f),
            Vector3.one * (radius * 0.95f),
            new Color(0.55f, 0.95f, 1f, 0.9f),
            new Color(0.1f, 0.45f, 1f, 0f),
            0.45f,
            0f,
            160f,
            0f,
            0f,
            5
        );

        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * 0.4f,
            Vector3.one * (radius * 1.35f),
            new Color(0.8f, 1f, 1f, 1f),
            new Color(0.2f, 0.65f, 1f, 0f),
            0.30f,
            0f,
            0f,
            0f,
            0f,
            13
        );
    }

    /// <summary>
    /// BOSS 3 (ABYSSAL TITAN) - Vực Thẳm Xung Kích & Nhật Thực Diệt Vong (Abyssal Explosion / Eclipse Cataclysm):
    /// Hố đen co rút (Implosion) + Bùng nổ Supernova tím đen + 4 cột sét hư không + 18 mảnh tinh thể.
    /// </summary>
    public void SpawnBossAbyssalCataclysmFX(Vector3 targetPos, float radius)
    {
        TriggerCameraShake(0.28f, 0.22f);

        // 1. Trận đồ Nhật Thực Đen xoay dưới mặt đất
        SpawnParticle(
            pixelRuneCircleSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.85f),
            Vector3.one * (radius * 1.05f),
            new Color(0.92f, 0.25f, 1f, 0.95f),
            new Color(0.2f, 0.02f, 0.35f, 0f),
            0.55f,
            0f,
            -180f,
            0f,
            0f,
            5
        );

        // 2. Lõi Hố Đen Vũ Trụ (Dark Singularity Core)
        SpawnParticle(
            pixelCircleSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 0.95f),
            Vector3.one * 0.2f,
            new Color(0.08f, 0.01f, 0.14f, 0.95f),
            new Color(0.6f, 0.05f, 0.85f, 0f),
            0.35f,
            0f,
            0f,
            0f,
            0f,
            11
        );

        // 3. Sóng xung kích kép Hư Không (Tím Neon & Đỏ Thẫm)
        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * 0.3f,
            Vector3.one * (radius * 1.4f),
            new Color(0.95f, 0.3f, 1f, 1f),
            new Color(0.35f, 0.02f, 0.65f, 0f),
            0.34f,
            0f,
            150f,
            0f,
            0f,
            13
        );

        SpawnParticle(
            pixelRingSprite,
            targetPos,
            Vector3.zero,
            Vector3.one * (radius * 1.25f),
            Vector3.one * 0.2f,
            new Color(1f, 0.15f, 0.45f, 0.85f),
            new Color(0.25f, 0.0f, 0.4f, 0f),
            0.22f,
            0f,
            -150f,
            0f,
            0f,
            12
        );

        // 4. 3 cột sét Hư Không giáng xuống
        for (int i = 0; i < 3; i++)
        {
            float rad = (i * 120f + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector3 boltPos = targetPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * (radius * 0.4f);
            SpawnParticle(
                pixelSkyLightningSprite,
                boltPos,
                Vector3.zero,
                new Vector3(1.35f, 1.4f, 1f),
                new Vector3(0.2f, 1.4f, 1f),
                new Color(0.95f, 0.45f, 1f, 0.95f),
                new Color(0.3f, 0.02f, 0.55f, 0f),
                0.26f,
                0f,
                0f,
                0f,
                0f,
                14
            );
        }

        // 5. 18 mảnh tinh thể Hư Không văng 360 độ
        for (int i = 0; i < 18; i++)
        {
            float rad = (i * 20f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            float spd = Random.Range(4.0f, 9.5f);

            SpawnParticle(
                (i % 2 == 0) ? pixelShardSprite : pixelDiamondSprite,
                targetPos + (Vector3)(dir * 0.3f),
                dir * spd,
                Vector3.one * Random.Range(0.85f, 1.3f),
                Vector3.zero,
                new Color(1f, 0.45f, 1f, 1f),
                new Color(0.25f, 0.02f, 0.45f, 0f),
                Random.Range(0.28f, 0.45f),
                rad * Mathf.Rad2Deg,
                Random.Range(-240f, 240f),
                5.5f,
                0f,
                13
            );
        }
    }

    /// <summary>
    /// Chùm Tia Năng Lượng Boss (Electric Beam của Storm Colossus & Void Beam của Abyssal Titan):
    /// Vẽ chùm tia Laser Pixel đa lớp (Lõi trắng + Hào quang nguyên tố + Hồ quang giật dọc thân tia + Quả cầu tụ lực ở đầu nòng).
    /// </summary>
    public void SpawnBossLaserBeamFX(Vector3 origin, Vector2 direction, float width, float length, float duration, BossThemeType theme)
    {
        TriggerCameraShake(Mathf.Max(0.25f, duration), 0.15f);

        Vector2 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float beamDur = Mathf.Max(0.35f, duration);

        Color outerColor = (theme == BossThemeType.AbyssalTitan)
            ? new Color(0.88f, 0.18f, 1f, 0.92f)
            : new Color(0.2f, 0.85f, 1f, 0.92f);
        Color outerEnd = new Color(outerColor.r, outerColor.g, outerColor.b, 0f);

        Color coreColor = (theme == BossThemeType.AbyssalTitan)
            ? new Color(1f, 0.82f, 1f, 1f)
            : new Color(0.92f, 1f, 1f, 1f);

        // 1. Lớp Hào Quang Laser Ngoài (Outer Wide Beam)
        SpawnParticle(
            pixelLaserBeamSprite,
            origin,
            Vector3.zero,
            new Vector3(length, width * 2.2f, 1f),
            new Vector3(length, width * 0.35f, 1f),
            outerColor,
            outerEnd,
            beamDur,
            angle,
            0f,
            0f,
            0f,
            13
        );

        // 2. Lớp Lõi Laser Trắng Siêu Nhiệt (Inner Core Beam)
        SpawnParticle(
            pixelLaserBeamSprite,
            origin,
            Vector3.zero,
            new Vector3(length, width * 1.15f, 1f),
            new Vector3(length, 0.05f, 1f),
            coreColor,
            outerEnd,
            beamDur * 0.9f,
            angle,
            0f,
            0f,
            0f,
            14
        );

        // 3. Quả cầu tụ năng lượng xoay tại đầu nòng Boss
        SpawnParticle(
            pixelRuneCircleSprite,
            origin + (Vector3)(dir * 0.4f),
            Vector3.zero,
            Vector3.one * 0.85f,
            Vector3.one * 0.25f,
            coreColor,
            outerEnd,
            beamDur,
            0f,
            360f,
            0f,
            0f,
            15
        );

        // 4. Các tia hồ quang điện / hư không giật dọc suốt chiều dài thân tia Laser
        int segments = Mathf.Clamp(Mathf.RoundToInt(length * 0.8f), 6, 14);
        for (int i = 1; i <= segments; i++)
        {
            float distAlong = (length / segments) * i;
            Vector3 pt = origin + (Vector3)(dir * distAlong);
            SpawnParticle(
                pixelBoltSprite,
                pt + (Vector3)(Random.insideUnitCircle * (width * 0.25f)),
                (Vector3)(dir * Random.Range(2f, 5f) + Random.insideUnitCircle * 1.5f),
                new Vector3(1.25f, 1.1f, 1f),
                Vector3.zero,
                coreColor,
                outerEnd,
                beamDur * 0.85f,
                angle + Random.Range(-25f, 25f),
                0f,
                3f,
                0f,
                15
            );
        }
    }

    /// <summary>
    /// Hiệu ứng Cổng Triệu Hồi Ma Thuật (Summon Minions & Mass Horde Invasion) dưới chân mỗi quái được Boss gọi ra.
    /// </summary>
    public void SpawnBossSummonPortalFX(Vector3 spawnPos, BossThemeType theme)
    {
        Color portalCol, fadeCol;
        switch (theme)
        {
            case BossThemeType.StormColossus:
                portalCol = new Color(0.35f, 0.92f, 1f, 0.9f);
                fadeCol = new Color(0.1f, 0.4f, 0.95f, 0f);
                break;
            case BossThemeType.AbyssalTitan:
                portalCol = new Color(0.9f, 0.25f, 1f, 0.9f);
                fadeCol = new Color(0.25f, 0.02f, 0.45f, 0f);
                break;
            default:
                portalCol = new Color(1f, 0.45f, 0.1f, 0.9f);
                fadeCol = new Color(0.85f, 0.1f, 0.02f, 0f);
                break;
        }

        // Vòng cổ ngữ triệu hồi xoay dưới mặt đất
        SpawnParticle(
            pixelRuneCircleSprite,
            spawnPos,
            Vector3.zero,
            Vector3.one * 0.15f,
            Vector3.one * 0.65f,
            portalCol,
            fadeCol,
            0.45f,
            0f,
            180f,
            0f,
            0f,
            4
        );

        // 4 hạt năng lượng bốc thẳng từ cổng triệu hồi lên
        for (int i = 0; i < 4; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.35f;
            SpawnParticle(
                pixelDiamondSprite,
                spawnPos + (Vector3)offset,
                new Vector3(0f, Random.Range(1.8f, 3.5f), 0f),
                Vector3.one * 0.65f,
                Vector3.zero,
                portalCol,
                fadeCol,
                0.35f,
                Random.Range(0f, 360f),
                180f,
                2f,
                0f,
                9
            );
        }
    }

    // =========================================================================
    // 8. HỆ THỐNG HIỆU ỨNG VẬN CHIÊU BOSS (BOSS CHARGE / TELEGRAPH / CAST VFX)
    // =========================================================================

    /// <summary>
    /// Kích hoạt khi Boss bắt đầu Vận Chiêu (Telegraphing):
    /// Sinh vòng Ma Trận Cổ Ngữ xoay dưới chân Boss + vòng sóng tụ lực co rút vào tâm Boss.
    /// </summary>
    public void SpawnBossChargeStartFX(Vector3 bossPos, BossThemeType theme, float chargeDuration)
    {
        Color mainCol, coreCol, fadeCol;
        GetBossThemeColors(theme, out mainCol, out coreCol, out fadeCol);

        float dur = Mathf.Max(0.4f, chargeDuration);

        // 1. Vòng Ma Trận Cổ Ngữ xoay dưới chân Boss trong suốt thời gian vận chiêu
        SpawnParticle(
            pixelRuneCircleSprite,
            bossPos,
            Vector3.zero,
            Vector3.one * 1.45f,
            Vector3.one * 0.55f,
            mainCol,
            fadeCol,
            dur,
            0f,
            240f,
            0f,
            0f,
            6
        );

        // 2. Vòng hào quang phụ xoay ngược chiều
        SpawnParticle(
            pixelRuneCircleSprite,
            bossPos,
            Vector3.zero,
            Vector3.one * 0.65f,
            Vector3.one * 1.35f,
            coreCol,
            fadeCol,
            dur * 0.9f,
            45f,
            -180f,
            0f,
            0f,
            7
        );
    }

    /// <summary>
    /// Được gọi liên tục mỗi nhịp (~0.07s) trong lúc Boss đang Vận Chiêu (progress từ 0 -> 1):
    /// - Hút các hạt năng lượng nguyên tố & tia sét từ xung quanh lao ngược vào ngực/miệng Boss (Inward Implosion).
    /// - Nuôi quả cầu tụ lực (Charge Orb) trước mặt Boss to dần lên.
    /// - Tạo hiệu ứng mồi báo trước tại vùng mục tiêu (Target Zone).
    /// </summary>
    public void SpawnBossChargeTickFX(
        Vector3 bossPos,
        Vector3 targetPos,
        Vector2 attackDir,
        BossAttackPatternType pattern,
        BossThemeType theme,
        float progress)
    {
        Color mainCol, coreCol, fadeCol;
        GetBossThemeColors(theme, out mainCol, out coreCol, out fadeCol);

        Vector2 dir = attackDir.sqrMagnitude > 0.001f ? attackDir.normalized : Vector2.down;
        Vector3 muzzlePos = bossPos + (Vector3)(dir * 0.75f);

        // 1. Hạt năng lượng nguyên tố từ ngoài bị hút xoáy ngược vào tâm/đầu nòng Boss (Inward Implosion)
        int pullCount = progress > 0.5f ? 4 : 3;
        for (int i = 0; i < pullCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float spawnDist = Random.Range(1.8f, 3.0f) * (1.15f - progress * 0.35f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnDist;
            Vector3 startPos = muzzlePos + offset;

            float travelTime = 0.22f;
            Vector3 inwardVel = -offset / travelTime;

            Sprite sp = pixelDiamondSprite;
            if (theme == BossThemeType.StormColossus && i == 0) sp = pixelBoltSprite;
            else if (theme == BossThemeType.AbyssalTitan && i == 0) sp = pixelShardSprite;
            else if (theme == BossThemeType.SkyDragon && i % 2 == 0) sp = pixelSquareSprite;

            SpawnParticle(
                sp,
                startPos,
                inwardVel,
                Vector3.one * Random.Range(0.55f, 0.95f),
                Vector3.one * 0.15f,
                (i == 0) ? coreCol : mainCol,
                fadeCol,
                travelTime,
                angle * Mathf.Rad2Deg,
                320f,
                0f,
                0f,
                14
            );
        }

        // 2. Vòng sóng tụ lực co rút (Imploding Ring) từ ngoài thu nhỏ vào người Boss
        if (Random.value < 0.45f)
        {
            SpawnParticle(
                pixelRingSprite,
                muzzlePos,
                Vector3.zero,
                Vector3.one * Mathf.Lerp(1.65f, 1.15f, progress),
                Vector3.one * 0.15f,
                mainCol,
                fadeCol,
                0.22f,
                0f,
                180f,
                0f,
                0f,
                13
            );
        }

        // 3. Quả Cầu Tụ Lực (Charging Core Orb) ở đầu nòng/ngực Boss phình to dần theo progress
        Sprite orbSprite = GetBossProjectileSprite(theme);
        float orbScale = Mathf.Lerp(0.35f, 1.35f, progress);
        SpawnParticle(
            orbSprite,
            muzzlePos,
            Vector3.zero,
            Vector3.one * orbScale,
            Vector3.one * (orbScale * 1.25f),
            coreCol,
            mainCol,
            0.09f,
            Time.time * 420f,
            360f,
            0f,
            0f,
            15
        );

        // 4. Hiệu ứng vận chiêu báo trước tại Vùng Mục Tiêu (Target Zone Pre-Impact FX)
        if (pattern == BossAttackPatternType.TargetedAoE)
        {
            Vector2 randCir = Random.insideUnitCircle * 1.8f;
            Vector3 groundPt = targetPos + (Vector3)randCir;

            if (theme == BossThemeType.SkyDragon)
            {
                // Lửa nham thạch & khói nóng bốc lên từ vòng cảnh báo trước khi thiên thạch rơi
                SpawnParticle(
                    pixelDiamondSprite,
                    groundPt,
                    new Vector3(0f, Random.Range(1.5f, 3.2f), 0f),
                    Vector3.one * 0.7f,
                    Vector3.zero,
                    mainCol,
                    fadeCol,
                    0.26f,
                    Random.Range(0f, 360f),
                    180f,
                    2f,
                    0f,
                    8
                );
            }
            else if (theme == BossThemeType.StormColossus)
            {
                // Tia điện tĩnh giật lách tách trên mặt đất báo hiệu sấm sét sắp giáng
                SpawnParticle(
                    pixelBoltSprite,
                    groundPt,
                    new Vector3(0f, Random.Range(1.0f, 2.5f), 0f),
                    new Vector3(0.95f, 0.85f, 1f),
                    Vector3.zero,
                    coreCol,
                    fadeCol,
                    0.18f,
                    Random.Range(-60f, 60f),
                    0f,
                    0f,
                    0f,
                    9
                );
            }
            else
            {
                // Hạt hư không bị hút xoáy vào tâm hố đen tại vùng mục tiêu
                Vector3 pullToCenter = (targetPos - groundPt) / 0.22f;
                SpawnParticle(
                    pixelShardSprite,
                    groundPt,
                    pullToCenter,
                    Vector3.one * 0.75f,
                    Vector3.zero,
                    mainCol,
                    fadeCol,
                    0.22f,
                    Random.Range(0f, 360f),
                    -240f,
                    0f,
                    0f,
                    9
                );
            }
        }
        else if (pattern == BossAttackPatternType.ContinuousBeam)
        {
            // Hạt năng lượng hội tụ dọc đường ngắm tia Laser trước khi khai hỏa
            float dAlong = Random.Range(1.5f, 10f);
            Vector2 perp = new Vector2(-dir.y, dir.x) * Random.Range(-1.2f, 1.2f);
            Vector3 beamAxisPt = bossPos + (Vector3)(dir * dAlong);
            SpawnParticle(
                pixelDiamondSprite,
                beamAxisPt + (Vector3)perp,
                -(Vector3)(perp / 0.16f),
                Vector3.one * 0.55f,
                Vector3.zero,
                coreCol,
                fadeCol,
                0.16f,
                0f,
                200f,
                0f,
                0f,
                12
            );
        }
    }

    /// <summary>
    /// Kích hoạt khoảnh khắc Boss vận chiêu đạt 100% và tung đòn (Cast Release Burst):
    /// Nổ bùng sóng xung kích tụ lực tại đầu nòng + chớp sáng hạt năng lượng.
    /// </summary>
    public void SpawnBossCastReleaseFX(Vector3 bossPos, Vector2 attackDir, BossThemeType theme)
    {
        Color mainCol, coreCol, fadeCol;
        GetBossThemeColors(theme, out mainCol, out coreCol, out fadeCol);

        Vector2 dir = attackDir.sqrMagnitude > 0.001f ? attackDir.normalized : Vector2.down;
        Vector3 muzzlePos = bossPos + (Vector3)(dir * 0.75f);

        TriggerCameraShake(0.10f, 0.07f);

        SpawnParticle(
            pixelRingSprite,
            muzzlePos,
            Vector3.zero,
            Vector3.one * 0.25f,
            Vector3.one * 1.45f,
            coreCol,
            fadeCol,
            0.22f,
            0f,
            180f,
            0f,
            0f,
            15
        );

        for (int i = 0; i < 10; i++)
        {
            float spread = Random.Range(-55f, 55f);
            Vector2 sparkDir = Quaternion.Euler(0f, 0f, spread) * dir;
            SpawnParticle(
                (theme == BossThemeType.StormColossus && i % 2 == 0) ? pixelBoltSprite : pixelDiamondSprite,
                muzzlePos,
                sparkDir * Random.Range(3.5f, 7.5f),
                Vector3.one * Random.Range(0.65f, 1.05f),
                Vector3.zero,
                coreCol,
                fadeCol,
                Random.Range(0.18f, 0.28f),
                spread,
                Random.Range(-240f, 240f),
                6f,
                0f,
                15
            );
        }
    }

    private void GetBossThemeColors(BossThemeType theme, out Color mainCol, out Color coreCol, out Color fadeCol)
    {
        switch (theme)
        {
            case BossThemeType.StormColossus:
                mainCol = new Color(0.2f, 0.88f, 1f, 0.95f);
                coreCol = new Color(0.92f, 1f, 1f, 1f);
                fadeCol = new Color(0.1f, 0.4f, 0.95f, 0f);
                break;
            case BossThemeType.AbyssalTitan:
                mainCol = new Color(0.92f, 0.22f, 1f, 0.95f);
                coreCol = new Color(1f, 0.78f, 1f, 1f);
                fadeCol = new Color(0.28f, 0.02f, 0.5f, 0f);
                break;
            default:
                mainCol = new Color(1f, 0.52f, 0.1f, 0.95f);
                coreCol = new Color(1f, 0.96f, 0.65f, 1f);
                fadeCol = new Color(0.9f, 0.12f, 0.02f, 0f);
                break;
        }
    }
}


