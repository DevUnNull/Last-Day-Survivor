using System.Collections;
using UnityEngine;

public class BossTelegraphController : MonoBehaviour
{
    private static Sprite circleRingSprite;
    private static Sprite circleFillSprite;
    private static Sprite lineTrackSprite;
    private static Sprite lineFillSprite;

    private static void EnsureSprites()
    {
        if (circleRingSprite != null && circleFillSprite != null && lineTrackSprite != null && lineFillSprite != null)
            return;

        // 1. 64x64 Crisp Pixel-Art Rune Danger Ring (Point Filtered)
        Texture2D ringTex = new Texture2D(64, 64, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector2 d = new Vector2(x, y) - center;
                float dist = d.magnitude;
                float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;

                bool outerRim = dist >= 28.5f && dist <= 31.0f;
                bool innerDashed = (dist >= 22.0f && dist <= 23.6f) && ((Mathf.RoundToInt(angle) / 10) % 2 == 0);
                bool crosshair = (dist >= 8.0f && dist <= 28.5f) && (Mathf.Abs(d.x) <= 1.0f || Mathf.Abs(d.y) <= 1.0f);
                bool centerDot = dist <= 3.2f;

                if (outerRim || centerDot)
                    ringTex.SetPixel(x, y, Color.white);
                else if (innerDashed || crosshair)
                    ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.75f));
                else if (dist < 28.5f)
                    ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.14f));
                else
                    ringTex.SetPixel(x, y, Color.clear);
            }
        }
        ringTex.Apply();
        circleRingSprite = Sprite.Create(ringTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);

        // 2. 64x64 Crisp Pixel Circle Fill (for expanding charge core)
        Texture2D fillTex = new Texture2D(64, 64, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist >= 27.5f && dist <= 30.8f)
                    fillTex.SetPixel(x, y, Color.white);
                else if (dist < 27.5f)
                    fillTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.45f));
                else
                    fillTex.SetPixel(x, y, Color.clear);
            }
        }
        fillTex.Apply();
        circleFillSprite = Sprite.Create(fillTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);

        // 3. 32x64 Pixel-Art Directional Warning Track (with side rails & chevron arrows)
        Texture2D trackTex = new Texture2D(32, 64, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                bool sideRail = (x <= 2 || x >= 29);
                bool topBottomCap = (y <= 1 || y >= 62);
                int chevronY = (y + Mathf.Abs(x - 16)) % 16;
                bool chevron = (x > 5 && x < 26) && (chevronY == 0 || chevronY == 1);

                if (sideRail || topBottomCap)
                    trackTex.SetPixel(x, y, Color.white);
                else if (chevron)
                    trackTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.72f));
                else
                    trackTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.16f));
            }
        }
        trackTex.Apply();
        lineTrackSprite = Sprite.Create(trackTex, new Rect(0, 0, 32, 64), new Vector2(0.5f, 0.5f), 32f);

        // 4. 16x16 Pixel Square Fill
        Texture2D sqTex = new Texture2D(16, 16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                sqTex.SetPixel(x, y, Color.white);
        sqTex.Apply();
        lineFillSprite = Sprite.Create(sqTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }

    public GameObject ShowCircleTelegraph(Vector3 worldPosition, float radius, float duration, Color warningColor)
    {
        EnsureSprites();

        GameObject marker = new GameObject("Telegraph_Circle");
        marker.transform.position = worldPosition;
        marker.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = circleRingSprite;
        sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.82f);
        sr.sortingOrder = 5;

        // Inner charging ring/fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(marker.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = Vector3.zero;

        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = circleFillSprite;
        fillSr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.65f);
        fillSr.sortingOrder = 6;

        StartCoroutine(AnimateTelegraph(marker, sr, fill.transform, fillSr, duration, warningColor));
        return marker;
    }

    public GameObject ShowLineTelegraph(Vector3 startPos, Vector2 direction, Vector2 size, float duration, Color warningColor)
    {
        EnsureSprites();

        GameObject marker = new GameObject("Telegraph_Line");
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        marker.transform.position = startPos + (Vector3)(direction.normalized * (size.y * 0.5f));
        marker.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        marker.transform.localScale = new Vector3(size.x, size.y * 0.5f, 1f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = lineTrackSprite;
        sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.82f);
        sr.sortingOrder = 5;

        // Inner charge
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(marker.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(0.92f, 0f, 1f);

        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = lineFillSprite;
        fillSr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.45f);
        fillSr.sortingOrder = 6;

        StartCoroutine(AnimateTelegraphLine(marker, fill.transform, fillSr, duration, warningColor));
        return marker;
    }

    private IEnumerator AnimateTelegraph(GameObject root, SpriteRenderer outerSr, Transform fill, SpriteRenderer fillSr, float duration, Color baseColor)
    {
        float timer = 0f;
        while (timer < duration && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            // Rotate outer rune ring for dynamic menace
            root.transform.Rotate(0f, 0f, 75f * Time.deltaTime);

            if (fill != null) fill.localScale = Vector3.one * progress;

            // Rapid flash in the final 20% right before impact
            if (progress > 0.78f && fillSr != null)
            {
                bool flash = Mathf.FloorToInt(timer * 28f) % 2 == 0;
                fillSr.color = flash ? new Color(1f, 0.98f, 0.85f, 0.88f) : new Color(baseColor.r, baseColor.g, baseColor.b, 0.72f);
            }

            yield return null;
        }

        if (root != null) Destroy(root);
    }

    private IEnumerator AnimateTelegraphLine(GameObject root, Transform fill, SpriteRenderer fillSr, float duration, Color baseColor)
    {
        float timer = 0f;
        while (timer < duration && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (fill != null) fill.localScale = new Vector3(0.92f, progress * 2f, 1f);

            if (progress > 0.78f && fillSr != null)
            {
                bool flash = Mathf.FloorToInt(timer * 28f) % 2 == 0;
                fillSr.color = flash ? new Color(1f, 0.98f, 0.85f, 0.78f) : new Color(baseColor.r, baseColor.g, baseColor.b, 0.55f);
            }

            yield return null;
        }

        if (root != null) Destroy(root);
    }
}
