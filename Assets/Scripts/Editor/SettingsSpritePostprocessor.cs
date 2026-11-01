using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SettingsSpritePostprocessor
{
    [MenuItem("Tools/Setup Settings Sprites Importer")]
    public static void ConfigureSprites()
    {
        string folder = "Assets/Sprite/Settings/";
        string[] files = new string[]
        {
            "icon_volume.png",
            "icon_bgm.png",
            "icon_sfx.png",
            "icon_vfx.png",
            "icon_close.png",
            "slider_knob.png",
            "toggle_on.png",
            "toggle_off.png",
            "slider_track.png",
            "slider_fill.png",
            "panel_frame_9slice.png"
        };

        foreach (string file in files)
        {
            string path = folder + file;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 16f;

            if (file == "panel_frame_9slice.png")
            {
                // 32x32 image with 6px border on all sides for 9-slice
                importer.spriteBorder = new Vector4(6, 6, 6, 6);
            }
            else if (file == "slider_track.png" || file == "slider_fill.png")
            {
                importer.spriteBorder = new Vector4(4, 2, 4, 2);
            }

            importer.SaveAndReimport();
        }

        Debug.Log("[Settings] All pixel art sprites configured as Point (no filter) Pixel Art Sprites!");
    }
}

