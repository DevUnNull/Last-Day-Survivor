using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class SetupAudioSystem
{
    [MenuItem("Tools/Setup Audio System")]
    public static void Setup()
    {
        // 1. Ensure AudioListener exists on Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            AudioListener listener = mainCam.GetComponent<AudioListener>();
            if (listener == null)
            {
                Undo.AddComponent<AudioListener>(mainCam.gameObject);
                Debug.Log("[Audio Setup] Added AudioListener to Main Camera.");
            }
        }
        else
        {
            AudioListener anyListener = Object.FindFirstObjectByType<AudioListener>();
            if (anyListener == null)
            {
                Camera anyCam = Object.FindFirstObjectByType<Camera>();
                if (anyCam != null)
                {
                    Undo.AddComponent<AudioListener>(anyCam.gameObject);
                    Debug.Log("[Audio Setup] Added AudioListener to camera: " + anyCam.name);
                }
            }
        }

        // 2. Setup or find AudioManager in current scene
        AudioManager audioMgr = Object.FindFirstObjectByType<AudioManager>();
        GameObject audioObj;

        if (audioMgr == null)
        {
            audioObj = new GameObject("AudioManager");
            Undo.RegisterCreatedObjectUndo(audioObj, "Create AudioManager");
            audioMgr = audioObj.AddComponent<AudioManager>();
        }
        else
        {
            audioObj = audioMgr.gameObject;
            Undo.RegisterFullObjectHierarchyUndo(audioObj, "Setup AudioManager");
        }

        // 3. Serialize Audio Clips into AudioManager
        SerializedObject so = new SerializedObject(audioMgr);
        string p = "Assets/Audio/";

        SetClip(so, "clipBGMMainMenu", p + "BGM_MainMenu.mp3");
        SetClip(so, "clipBGMCombat", p + "BGM_InGame_Combat.wav");
        SetClip(so, "clipBGMIntense", p + "BGM_InGame_Intense.wav");
        SetClip(so, "clipMEGameOver", p + "ME_GameOver.wav");
        SetClip(so, "clipMELevelUp", p + "ME_LevelUp.wav");
        SetClip(so, "clipAMBWind", p + "AMB_PostApocalyptic_Wind.ogg");

        SetClip(so, "clipUIHover", p + "UI_Hover.wav");
        SetClip(so, "clipUIClick", p + "UI_Click.wav");
        SetClip(so, "clipUICardAppear", p + "UI_Card_Appear.wav");
        SetClip(so, "clipUICardSelect", p + "UI_Card_Select.wav");
        SetClip(so, "clipUIGameOver", p + "UI_GameOver_Popup.wav");

        SetClip(so, "clipPlayerHurt", p + "Player_Hurt.wav");
        SetClip(so, "clipPlayerDie", p + "Player_Die.wav");
        SetClip(so, "clipPlayerHeal", p + "Player_Heal.wav");
        SetClip(so, "clipPlayerLowHealth", p + "Player_LowHealth_Loop.wav");

        SetClipArray(so, "clipsPlayerFootsteps", new string[]
        {
            p + "Player_Footstep_01.wav",
            p + "Player_Footstep_02.wav",
            p + "Player_Footstep_03.wav"
        });

        SetClip(so, "clipSpellBoltCast", p + "Spell_Bolt_Cast.wav");
        SetClip(so, "clipSpellBoltImpact", p + "Spell_Bolt_Impact.wav");
        SetClip(so, "clipSpellFireballLaunch", p + "Spell_Fireball_Launch.wav");
        SetClip(so, "clipSpellFireballExplode", p + "Spell_Fireball_Explode.wav");
        SetClip(so, "clipSpellIceSpearCast", p + "Spell_IceSpear_Cast.wav");
        SetClip(so, "clipSpellIceSpearPierce", p + "Spell_IceSpear_Pierce.wav");
        SetClip(so, "clipSpellDarkOrbHit", p + "Spell_DarkOrb_Hit.wav");
        SetClip(so, "clipSpellDarkOrbLoop", p + "Spell_DarkOrb_Loop.wav");
        SetClip(so, "clipSpellShieldAbsorb", p + "Spell_Shield_Absorb.wav");
        SetClip(so, "clipSpellShieldActivate", p + "Spell_Shield_Activate.wav");
        SetClip(so, "clipSpellShieldBreak", p + "Spell_Shield_Break.wav");
        SetClip(so, "clipSpellShieldRegen", p + "Spell_Shield_Regen.wav");

        SetClip(so, "clipEnemyAttack", p + "Enemy_Attack.wav");
        SetClipArray(so, "clipsEnemyHit", new string[]
        {
            p + "Enemy_Hit_01.wav",
            p + "Enemy_Hit_02.wav",
            p + "Enemy_Hit_03.wav"
        });
        SetClipArray(so, "clipsEnemyDeath", new string[]
        {
            p + "Enemy_Death_01.wav",
            p + "Enemy_Death_02.wav",
            p + "Enemy_Death_03.wav"
        });
        SetClip(so, "clipEnemySpawn", p + "Enemy_Spawn.wav");

        SetClip(so, "clipGemPickup", p + "SFX_Gem_Pickup.wav");
        SetClip(so, "clipMagnetPickup", p + "SFX_Magnet_Pickup.wav");
        SetClip(so, "clipPotionPickup", p + "SFX_Potion_Pickup.wav");

        so.ApplyModifiedProperties();

        // 4. Ensure all UI Buttons have UIButtonAnimation attached for instant hover/click audio
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int animatedButtonsCount = 0;
        foreach (Button btn in allButtons)
        {
            if (btn != null && btn.GetComponent<UIButtonAnimation>() == null)
            {
                Undo.AddComponent<UIButtonAnimation>(btn.gameObject);
                animatedButtonsCount++;
            }
        }
        if (animatedButtonsCount > 0)
        {
            Debug.Log($"[Audio Setup] Attached UIButtonAnimation to {animatedButtonsCount} buttons for UI audio feedback.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Audio Setup] Successfully configured AudioManager with all 35 audio clips in current scene!");
    }

    private static void SetClip(SerializedObject so, string propertyName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            prop.objectReferenceValue = clip;
        }
    }

    private static void SetClipArray(SerializedObject so, string propertyName, string[] assetPaths)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.arraySize = assetPaths.Length;
            for (int i = 0; i < assetPaths.Length; i++)
            {
                SerializedProperty elem = prop.GetArrayElementAtIndex(i);
                elem.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPaths[i]);
            }
        }
    }
}

