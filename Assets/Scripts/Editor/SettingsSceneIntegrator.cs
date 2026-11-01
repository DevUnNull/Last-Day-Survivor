using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public static class SettingsSceneIntegrator
{
    private const string PREFAB_PATH = "Assets/Prefabs/SettingsPanel.prefab";

    [MenuItem("Tools/Integrate Settings Panel into Scenes")]
    public static void Integrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Settings] Cannot integrate scenes while in Play Mode!");
            return;
        }

        // 1. Build or re-build prefab first
        GameObject prefab = SettingsPrefabBuilder.BuildPrefab();
        if (prefab == null)
        {
            Debug.LogError("[Settings] Failed to create SettingsPanel.prefab!");
            return;
        }

        // 2. Integrate into 1_1_MainMenu.unity
        IntegrateMainMenu();

        // 3. Integrate into 2_InGame.unity
        IntegrateInGame();

        Debug.Log("[Settings] Successfully integrated SettingsPanel into MainMenu and InGame scenes!");
    }

    private static void IntegrateMainMenu()
    {
        string scenePath = "Assets/Scenes/1_1_MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Settings] Canvas not found in " + scenePath);
            return;
        }

        // Instantiate SettingsPanel if not present
        SettingPanelUI existing = Object.FindFirstObjectByType<SettingPanelUI>();
        GameObject panelObj;
        if (existing == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            panelObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            panelObj.name = "SettingsPanel";
            existing = panelObj.GetComponent<SettingPanelUI>();
            Undo.RegisterCreatedObjectUndo(panelObj, "Add SettingsPanel");
        }
        else
        {
            panelObj = existing.gameObject;
        }

        // Find Setting button in MainMenu
        GameObject settingBtnObj = GameObject.Find("Setting");
        if (settingBtnObj != null)
        {
            Button btn = settingBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                // Clear and hook onClick
                UnityEventToolsAddPersistentListener(btn, existing);
                Debug.Log("[Settings] Hooked MainMenu Setting button to Open SettingsPanel!");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void IntegrateInGame()
    {
        string scenePath = "Assets/Scenes/2_InGame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas targetCanvas = null;
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                targetCanvas = c;
                break;
            }
        }
        if (targetCanvas == null && canvases.Length > 0) targetCanvas = canvases[0];

        if (targetCanvas != null)
        {
            SettingPanelUI existing = Object.FindFirstObjectByType<SettingPanelUI>();
            if (existing == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                GameObject panelObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, targetCanvas.transform);
                panelObj.name = "SettingsPanel";
                Undo.RegisterCreatedObjectUndo(panelObj, "Add SettingsPanel to InGame");
                Debug.Log("[Settings] Added SettingsPanel to InGame scene Canvas!");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void UnityEventToolsAddPersistentListener(Button btn, SettingPanelUI target)
    {
        // Add persistent listener via SerializedProperty to guarantee scene persistence
        SerializedObject so = new SerializedObject(btn);
        SerializedProperty onClick = so.FindProperty("m_OnClick");
        SerializedProperty calls = onClick.FindPropertyRelative("m_PersistentCalls.m_Calls");

        // Clear existing calls to avoid duplicates
        calls.ClearArray();
        calls.InsertArrayElementAtIndex(0);

        SerializedProperty call = calls.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = "Open";
        call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
        call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly

        so.ApplyModifiedProperties();
    }
}
