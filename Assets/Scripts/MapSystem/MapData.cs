using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "LastDaySurvivor/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Basic Information")]
    public string mapId = "map_1";
    public string mapName = "Sky Ruins";
    [TextArea(2, 4)]
    public string description = "Ancient magical ruins floating in the sky amidst howling winds.";
    public Sprite previewSprite;
    public Sprite backgroundSprite;
    public string difficulty = "NORMAL ★☆☆";
    public int difficultyRating = 1;

    [Header("Environment & Scene")]
    public string sceneName = "2_InGame";
    public Color ambientColor = new Color(0.9f, 0.95f, 1f, 1f);
    public Color backgroundColorTint = Color.white;

    [Header("Progression & Survival")]
    [Tooltip("Target survival time in seconds before boss spawns. Default 300s (5 min).")]
    public float survivalDuration = 300f;
    public string unlockRequirementMapId = ""; // Empty means unlocked by default
    public bool isUnlockedByDefault = false;

    [Header("Boss Configuration")]
    public BossData bossData;

    [Header("Enemy Waves Scaling")]
    public float baseSpawnInterval = 2.5f;
    public float minSpawnInterval = 0.3f;
    public float intervalDecreaseRate = 0.05f;
    public int baseEnemyHealth = 50;
    public float enemySpeedMultiplier = 1.0f;
}
