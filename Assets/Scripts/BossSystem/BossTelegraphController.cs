using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTelegraphController : MonoBehaviour
{
    private static Sprite unitCenterSprite;
    private static Sprite unitBottomPivotSprite;
    private static Shader telegraphShader;

    private static void EnsureResources()
    {
        if (telegraphShader == null)
        {
            telegraphShader = Shader.Find("Custom/BossTelegraphShader");
            if (telegraphShader == null)
            {
                telegraphShader = Resources.Load<Shader>("Shaders/BossTelegraphShader");
            }
        }

        if (unitCenterSprite != null && unitBottomPivotSprite != null)
            return;

        // 16x16 pure white 1x1 world-unit quad texture (Point filtered)
        Texture2D quadTex = new Texture2D(16, 16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                quadTex.SetPixel(x, y, Color.white);
        quadTex.Apply();

        // Center pivot (0.5, 0.5) for Circles
        unitCenterSprite = Sprite.Create(quadTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        // Bottom-center pivot (0.5, 0.0) for Lines & Rays so they start right at the Boss and extend forward
        unitBottomPivotSprite = Sprite.Create(quadTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.0f), 16f);
    }

    private static Color GetCoreHighlightColor(Color baseCol)
    {
        return Color.Lerp(baseCol, new Color(1f, 0.98f, 0.9f, 1f), 0.72f);
    }

    /// <summary>
    /// Hiển thị vòng tròn cảnh báo AoE / Shockwave bằng Shader Pixel-Art (xoay cổ ngữ + sóng nạp từ tâm ra viền).
    /// </summary>
    public GameObject ShowCircleTelegraph(Vector3 worldPosition, float radius, float duration, Color warningColor)
    {
        EnsureResources();

        float diameter = Mathf.Max(1f, radius * 2f);
        GameObject marker = new GameObject("Telegraph_Circle");
        marker.transform.position = worldPosition;
        marker.transform.localScale = new Vector3(diameter, diameter, 1f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = unitCenterSprite;
        sr.sortingOrder = 5;

        Material mat = CreateTelegraphMaterial(warningColor, 1f, new Vector2(diameter, diameter));
        if (mat != null) sr.material = mat;

        StartCoroutine(AnimateShaderTelegraph(marker, mat, duration));
        return marker;
    }

    /// <summary>
    /// Hiển thị hành lang năng lượng Laser / Lướt Húc (Charge Dash / Continuous Beam) thon gọn, bo tròn 2 đầu.
    /// </summary>
    public GameObject ShowLineTelegraph(Vector3 startPos, Vector2 direction, Vector2 size, float duration, Color warningColor)
    {
        EnsureResources();

        Vector2 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        float width = Mathf.Clamp(size.x, 0.8f, 2.2f);
        float length = Mathf.Max(2f, size.y);

        GameObject marker = new GameObject("Telegraph_Line");
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        marker.transform.position = startPos;
        marker.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        marker.transform.localScale = new Vector3(width, length, 1f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = unitBottomPivotSprite;
        sr.sortingOrder = 5;

        Material mat = CreateTelegraphMaterial(warningColor, 0f, new Vector2(width, length));
        if (mat != null) sr.material = mat;

        StartCoroutine(AnimateShaderTelegraph(marker, mat, duration));
        return marker;
    }

    /// <summary>
    /// Hiển thị chùm tia đường đạn mảnh (Fan of Bullet Trajectories) cho các chiêu bắn đạn tỏa hình quạt (Fire Breath, Shadow Projectiles).
    /// </summary>
    public GameObject ShowProjectileFanTelegraph(
        Vector3 startPos,
        Vector2 direction,
        int projectileCount,
        float spreadAngle,
        float length,
        float duration,
        Color warningColor)
    {
        EnsureResources();

        Vector2 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        int count = Mathf.Clamp(projectileCount, 1, 12);
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float rayWidth = 0.34f;
        float rayLen = Mathf.Max(4f, length);

        GameObject root = new GameObject("Telegraph_ProjectileFan");
        root.transform.position = startPos;

        List<Material> rayMats = new List<Material>(count);

        for (int i = 0; i < count; i++)
        {
            float offset = (count > 1)
                ? Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, (float)i / (count - 1))
                : 0f;
            float rayDeg = baseAngle + offset - 90f;

            GameObject rayObj = new GameObject($"AimRay_{i}");
            rayObj.transform.SetParent(root.transform, false);
            rayObj.transform.localPosition = Vector3.zero;
            rayObj.transform.rotation = Quaternion.Euler(0f, 0f, rayDeg);
            rayObj.transform.localScale = new Vector3(rayWidth, rayLen, 1f);

            SpriteRenderer sr = rayObj.AddComponent<SpriteRenderer>();
            sr.sprite = unitBottomPivotSprite;
            sr.sortingOrder = 5;

            Material mat = CreateTelegraphMaterial(warningColor, 2f, new Vector2(rayWidth, rayLen));
            if (mat != null)
            {
                sr.material = mat;
                rayMats.Add(mat);
            }
        }

        StartCoroutine(AnimateMultiMaterialTelegraph(root, rayMats, duration));
        return root;
    }

    private static Material CreateTelegraphMaterial(Color warningColor, float shapeType, Vector2 worldSize)
    {
        if (telegraphShader == null) return null;

        Material mat = new Material(telegraphShader);
        mat.SetColor("_Color", warningColor);
        mat.SetColor("_CoreColor", GetCoreHighlightColor(warningColor));
        mat.SetFloat("_ShapeType", shapeType);
        mat.SetVector("_WorldSize", new Vector4(worldSize.x, worldSize.y, 0f, 0f));
        mat.SetFloat("_Progress", 0f);
        mat.SetFloat("_PixelDensity", 16f);
        return mat;
    }

    private IEnumerator AnimateShaderTelegraph(GameObject root, Material mat, float duration)
    {
        float timer = 0f;
        float total = Mathf.Max(0.1f, duration);
        while (timer < total && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / total);
            if (mat != null)
            {
                mat.SetFloat("_Progress", progress);
            }
            yield return null;
        }

        if (mat != null) Destroy(mat);
        if (root != null) Destroy(root);
    }

    private IEnumerator AnimateMultiMaterialTelegraph(GameObject root, List<Material> mats, float duration)
    {
        float timer = 0f;
        float total = Mathf.Max(0.1f, duration);
        while (timer < total && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / total);
            for (int i = 0; i < mats.Count; i++)
            {
                if (mats[i] != null) mats[i].SetFloat("_Progress", progress);
            }
            yield return null;
        }

        for (int i = 0; i < mats.Count; i++)
        {
            if (mats[i] != null) Destroy(mats[i]);
        }
        if (root != null) Destroy(root);
    }
}
