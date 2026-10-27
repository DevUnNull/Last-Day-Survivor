using System.Collections;
using UnityEngine;

public class BossTelegraphController : MonoBehaviour
{
    private static Sprite circleSprite;
    private static Sprite squareSprite;

    private static void EnsureSprites()
    {
        if (circleSprite != null && squareSprite != null) return;

        // Create 64x64 circle texture
        Texture2D circleTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        circleTex.filterMode = FilterMode.Bilinear;
        circleTex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(31.5f, 31.5f);
        float radius = 30f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    // Soft antialiased edge
                    float alpha = Mathf.Clamp01(radius - dist);
                    circleTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    circleTex.SetPixel(x, y, Color.clear);
                }
            }
        }
        circleTex.Apply();
        circleSprite = Sprite.Create(circleTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32f);

        // Create 16x16 square texture
        Texture2D sqTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                sqTex.SetPixel(x, y, Color.white);
            }
        }
        sqTex.Apply();
        squareSprite = Sprite.Create(sqTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }

    public GameObject ShowCircleTelegraph(Vector3 worldPosition, float radius, float duration, Color warningColor)
    {
        EnsureSprites();

        GameObject marker = new GameObject("Telegraph_Circle");
        marker.transform.position = worldPosition;
        marker.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.25f);
        sr.sortingOrder = 5;

        // Inner charging ring/fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(marker.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = Vector3.zero;

        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = circleSprite;
        fillSr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.55f);
        fillSr.sortingOrder = 6;

        StartCoroutine(AnimateTelegraph(marker, fill.transform, duration));
        return marker;
    }

    public GameObject ShowLineTelegraph(Vector3 startPos, Vector2 direction, Vector2 size, float duration, Color warningColor)
    {
        EnsureSprites();

        GameObject marker = new GameObject("Telegraph_Line");
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        marker.transform.position = startPos + (Vector3)(direction.normalized * (size.y * 0.5f));
        marker.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        marker.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;
        sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.25f);
        sr.sortingOrder = 5;

        // Inner charge
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(marker.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(1f, 0f, 1f);

        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = squareSprite;
        fillSr.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.55f);
        fillSr.sortingOrder = 6;

        StartCoroutine(AnimateTelegraphLine(marker, fill.transform, duration));
        return marker;
    }

    private IEnumerator AnimateTelegraph(GameObject root, Transform fill, float duration)
    {
        float timer = 0f;
        while (timer < duration && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (fill != null) fill.localScale = Vector3.one * progress;
            yield return null;
        }

        if (root != null) Destroy(root);
    }

    private IEnumerator AnimateTelegraphLine(GameObject root, Transform fill, float duration)
    {
        float timer = 0f;
        while (timer < duration && root != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (fill != null) fill.localScale = new Vector3(1f, progress, 1f);
            yield return null;
        }

        if (root != null) Destroy(root);
    }
}
