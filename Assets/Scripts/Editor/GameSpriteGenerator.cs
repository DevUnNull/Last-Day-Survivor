using UnityEngine;
using UnityEditor;
using System.IO;

public static class GameSpriteGenerator
{
    private const string FolderPath = "Assets/Sprite/Generated";

    [MenuItem("Tools/Generate Game Sprites")]
    public static void GenerateAllSprites()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        GenerateExpGem();
        GenerateIceSpear();
        GenerateDarkOrb();
        GenerateMagicShield();
        GenerateFireball();

        // Icons
        GenerateIconBolt();
        GenerateIconFireball();
        GenerateIconIceSpear();
        GenerateIconDarkOrb();
        GenerateIconShield();
        GenerateIconTome();
        GenerateIconBoots();
        GenerateIconHaste();
        GenerateIconHeart();
        GenerateIconMagnet();
        GenerateIconPotion();

        AssetDatabase.Refresh();
        Debug.Log("Successfully generated all game sprites in " + FolderPath);
    }

    private static void SaveTexture(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(FolderPath, fileName);
        File.WriteAllBytes(fullPath, bytes);

        string relativePath = FolderPath + "/" + fileName;
        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void GenerateExpGem()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Diamond shape EXP crystal
        Color border = new Color(0.05f, 0.2f, 0.5f, 1f);
        Color darkCyan = new Color(0.0f, 0.6f, 0.9f, 1f);
        Color brightCyan = new Color(0.2f, 0.9f, 1f, 1f);
        Color whiteShine = new Color(0.9f, 1f, 1f, 1f);

        for (int y = 1; y < 15; y++)
        {
            int distFromCenterY = Mathf.Abs(y - 8);
            int width = 7 - distFromCenterY;
            for (int dx = -width; dx <= width; dx++)
            {
                int x = 8 + dx;
                if (Mathf.Abs(dx) == width || y == 1 || y == 14)
                {
                    tex.SetPixel(x, y, border);
                }
                else if (x < 8 && y > 7)
                {
                    tex.SetPixel(x, y, whiteShine);
                }
                else if (x <= 8)
                {
                    tex.SetPixel(x, y, brightCyan);
                }
                else
                {
                    tex.SetPixel(x, y, darkCyan);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, "exp_gem.png");
    }

    private static void GenerateIceSpear()
    {
        int w = 24;
        int h = 10;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        Color border = new Color(0.1f, 0.3f, 0.6f, 1f);
        Color iceBody = new Color(0.4f, 0.8f, 1f, 1f);
        Color iceHighlight = new Color(0.85f, 0.95f, 1f, 1f);

        for (int x = 0; x < w; x++)
        {
            int halfH = Mathf.RoundToInt((float)x / (w - 1) * 4f);
            for (int y = 5 - halfH; y <= 5 + halfH; y++)
            {
                if (y == 5 - halfH || y == 5 + halfH || x == w - 1)
                    tex.SetPixel(x, y, border);
                else if (y >= 5)
                    tex.SetPixel(x, y, iceHighlight);
                else
                    tex.SetPixel(x, y, iceBody);
            }
        }

        tex.Apply();
        SaveTexture(tex, "ice_spear.png");
    }

    private static void GenerateDarkOrb()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        Color core = new Color(1f, 0.4f, 1f, 1f);
        Color inner = new Color(0.6f, 0.1f, 0.8f, 1f);
        Color outer = new Color(0.25f, 0.05f, 0.45f, 0.9f);
        Color glow = new Color(0.1f, 0.0f, 0.2f, 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                if (d < 2.5f) tex.SetPixel(x, y, core);
                else if (d < 4.5f) tex.SetPixel(x, y, inner);
                else if (d < 6.5f) tex.SetPixel(x, y, outer);
                else if (d < 7.5f) tex.SetPixel(x, y, glow);
            }
        }

        tex.Apply();
        SaveTexture(tex, "dark_orb.png");
    }

    private static void GenerateMagicShield()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        Color rim = new Color(0.3f, 0.8f, 1f, 0.9f);
        Color innerGlow = new Color(0.1f, 0.5f, 0.9f, 0.35f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                if (d >= 13.5f && d <= 15.5f)
                {
                    tex.SetPixel(x, y, rim);
                }
                else if (d < 13.5f && d >= 10.0f)
                {
                    tex.SetPixel(x, y, innerGlow);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, "magic_shield.png");
    }

    private static void GenerateFireball()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        Color white = new Color(1f, 1f, 0.8f, 1f);
        Color yellow = new Color(1f, 0.85f, 0.1f, 1f);
        Color orange = new Color(1f, 0.45f, 0.0f, 1f);
        Color darkRed = new Color(0.7f, 0.1f, 0.0f, 0.9f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                if (d < 2f) tex.SetPixel(x, y, white);
                else if (d < 4f) tex.SetPixel(x, y, yellow);
                else if (d < 6f) tex.SetPixel(x, y, orange);
                else if (d < 7.5f) tex.SetPixel(x, y, darkRed);
            }
        }

        tex.Apply();
        SaveTexture(tex, "fireball.png");
    }

    // ------------------- ICONS -------------------

    private static void GenerateIconBolt()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.1f, 0.15f, 0.3f, 1f));
        Color bolt = new Color(0.3f, 0.9f, 1f, 1f);
        Color glow = new Color(1f, 1f, 1f, 1f);

        // Lightning bolt diagonal
        DrawLine(tex, 20, 26, 12, 16, bolt);
        DrawLine(tex, 12, 16, 17, 16, glow);
        DrawLine(tex, 17, 16, 10, 6, bolt);

        tex.Apply();
        SaveTexture(tex, "icon_magic_bolt.png");
    }

    private static void GenerateIconFireball()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.3f, 0.1f, 0.05f, 1f));
        Color c1 = new Color(1f, 0.8f, 0.1f, 1f);
        Color c2 = new Color(1f, 0.3f, 0.05f, 1f);

        for (int y = 8; y < 24; y++)
        {
            for (int x = 8; x < 24; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (d < 5f) tex.SetPixel(x, y, c1);
                else if (d < 8f) tex.SetPixel(x, y, c2);
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_fireball.png");
    }

    private static void GenerateIconIceSpear()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.05f, 0.2f, 0.35f, 1f));
        Color ice1 = new Color(0.4f, 0.85f, 1f, 1f);
        Color ice2 = new Color(0.9f, 0.98f, 1f, 1f);

        for (int i = 0; i < 18; i++)
        {
            tex.SetPixel(7 + i, 7 + i, ice2);
            tex.SetPixel(6 + i, 7 + i, ice1);
            tex.SetPixel(7 + i, 6 + i, ice1);
        }
        tex.Apply();
        SaveTexture(tex, "icon_ice_spear.png");
    }

    private static void GenerateIconDarkOrb()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.2f, 0.05f, 0.25f, 1f));
        Color orb1 = new Color(0.8f, 0.2f, 1f, 1f);
        Color orb2 = new Color(0.4f, 0.05f, 0.6f, 1f);

        for (int y = 8; y < 24; y++)
        {
            for (int x = 8; x < 24; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (d < 4f) tex.SetPixel(x, y, Color.white);
                else if (d < 6f) tex.SetPixel(x, y, orb1);
                else if (d < 8.5f) tex.SetPixel(x, y, orb2);
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_dark_orb.png");
    }

    private static void GenerateIconShield()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.05f, 0.15f, 0.3f, 1f));
        Color shieldColor = new Color(0.2f, 0.6f, 1f, 1f);
        Color rim = new Color(0.7f, 0.9f, 1f, 1f);

        for (int y = 8; y < 24; y++)
        {
            int span = y < 16 ? (y - 8) + 3 : 11;
            for (int x = 16 - span; x <= 16 + span; x++)
            {
                if (x == 16 - span || x == 16 + span || y == 23)
                    tex.SetPixel(x, y, rim);
                else
                    tex.SetPixel(x, y, shieldColor);
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_magic_shield.png");
    }

    private static void GenerateIconTome()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.25f, 0.1f, 0.25f, 1f));
        Color cover = new Color(0.6f, 0.15f, 0.7f, 1f);
        Color gold = new Color(1f, 0.85f, 0.2f, 1f);
        Color page = new Color(0.95f, 0.9f, 0.8f, 1f);

        for (int y = 8; y < 24; y++)
        {
            for (int x = 9; x < 23; x++)
            {
                if (x == 9 || x == 22 || y == 8 || y == 23) tex.SetPixel(x, y, gold);
                else if (x == 15 || x == 16) tex.SetPixel(x, y, cover);
                else tex.SetPixel(x, y, page);
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_arcane_tome.png");
    }

    private static void GenerateIconBoots()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.1f, 0.2f, 0.15f, 1f));
        Color boot = new Color(0.3f, 0.8f, 0.5f, 1f);
        Color wing = new Color(0.85f, 1f, 0.9f, 1f);

        // Boot L-shape
        for (int y = 8; y < 22; y++)
            for (int x = 11; x < 17; x++)
                tex.SetPixel(x, y, boot);

        for (int y = 8; y < 14; y++)
            for (int x = 16; x < 24; x++)
                tex.SetPixel(x, y, boot);

        // Small wing
        for (int y = 16; y < 24; y++)
            for (int x = 17; x < 25; x++)
                if (x + y > 36) tex.SetPixel(x, y, wing);

        tex.Apply();
        SaveTexture(tex, "icon_hermes_boots.png");
    }

    private static void GenerateIconHaste()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.25f, 0.2f, 0.05f, 1f));
        Color hasteColor = new Color(1f, 0.9f, 0.2f, 1f);

        // Speed arrows >>
        for (int i = 0; i < 7; i++)
        {
            tex.SetPixel(10 + i, 16 + i, hasteColor);
            tex.SetPixel(10 + i, 16 - i, hasteColor);
            tex.SetPixel(17 + i, 16 + i, hasteColor);
            tex.SetPixel(17 + i, 16 - i, hasteColor);
        }
        tex.Apply();
        SaveTexture(tex, "icon_spell_haste.png");
    }

    private static void GenerateIconHeart()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.3f, 0.08f, 0.1f, 1f));
        Color heartRed = new Color(0.95f, 0.2f, 0.25f, 1f);
        Color shine = new Color(1f, 0.7f, 0.7f, 1f);

        for (int y = 7; y < 24; y++)
        {
            for (int x = 8; x < 24; x++)
            {
                // Simple heart condition
                float dx = (x - 16) / 7f;
                float dy = (y - 14) / 7f;
                if ((dx * dx + dy * dy - 1) * (dx * dx + dy * dy - 1) * (dx * dx + dy * dy - 1) - dx * dx * dy * dy * dy <= 0)
                {
                    tex.SetPixel(x, y, (x < 15 && y > 16) ? shine : heartRed);
                }
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_vitality_heart.png");
    }

    private static void GenerateIconMagnet()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.15f, 0.15f, 0.25f, 1f));
        Color redPole = new Color(0.9f, 0.2f, 0.2f, 1f);
        Color bluePole = new Color(0.2f, 0.4f, 0.9f, 1f);

        // U-shape magnet
        for (int y = 9; y < 23; y++)
        {
            for (int x = 9; x < 23; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 17));
                if (d >= 4.5f && d <= 7.5f && y <= 17)
                {
                    tex.SetPixel(x, y, x < 16 ? redPole : bluePole);
                }
                else if (y > 17 && ((x >= 9 && x <= 12) || (x >= 20 && x <= 23)))
                {
                    tex.SetPixel(x, y, x < 16 ? redPole : bluePole);
                }
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_magnetic_pendant.png");
    }

    private static void GenerateIconPotion()
    {
        Texture2D tex = CreateBaseIcon(new Color(0.1f, 0.2f, 0.1f, 1f));
        Color green = new Color(0.2f, 0.9f, 0.4f, 1f);
        Color glass = new Color(0.8f, 0.95f, 0.9f, 0.9f);

        for (int y = 8; y < 18; y++)
        {
            for (int x = 11; x < 21; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 13));
                if (d < 4.5f) tex.SetPixel(x, y, green);
            }
        }
        for (int y = 18; y < 23; y++)
        {
            for (int x = 14; x < 18; x++)
            {
                tex.SetPixel(x, y, glass);
            }
        }
        tex.Apply();
        SaveTexture(tex, "icon_heal_potion.png");
    }

    private static Texture2D CreateBaseIcon(Color bg)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color border = new Color(0.6f, 0.65f, 0.75f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, bg);
            }
        }
        return tex;
    }

    private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            tex.SetPixel(x0, y0, col);
            tex.SetPixel(x0 + 1, y0, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
