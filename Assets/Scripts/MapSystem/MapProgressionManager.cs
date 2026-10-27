using UnityEngine;
using System.Collections.Generic;

public static class MapProgressionManager
{
    private const string UNLOCK_KEY_PREFIX = "LDS_MapUnlocked_";
    private const string COMPLETE_KEY_PREFIX = "LDS_MapCompleted_";
    private const string BEST_TIME_KEY_PREFIX = "LDS_MapBestTime_";

    public static bool IsMapUnlocked(MapData map)
    {
        if (map == null) return false;
        if (map.isUnlockedByDefault) return true;
        if (string.IsNullOrEmpty(map.unlockRequirementMapId)) return true;

        // Check if player has explicit unlock saved
        if (PlayerPrefs.GetInt(UNLOCK_KEY_PREFIX + map.mapId, 0) == 1)
        {
            return true;
        }

        // Check if requirement map was completed
        return IsMapCompleted(map.unlockRequirementMapId);
    }

    public static bool IsMapCompleted(string mapId)
    {
        if (string.IsNullOrEmpty(mapId)) return false;
        return PlayerPrefs.GetInt(COMPLETE_KEY_PREFIX + mapId, 0) == 1;
    }

    public static void CompleteMap(string mapId, float timeSurvived)
    {
        if (string.IsNullOrEmpty(mapId)) return;

        PlayerPrefs.SetInt(COMPLETE_KEY_PREFIX + mapId, 1);

        float currentBest = PlayerPrefs.GetFloat(BEST_TIME_KEY_PREFIX + mapId, 0f);
        if (timeSurvived > currentBest)
        {
            PlayerPrefs.SetFloat(BEST_TIME_KEY_PREFIX + mapId, timeSurvived);
        }

        PlayerPrefs.Save();
        Debug.Log($"[MapProgression] Map '{mapId}' marked as completed with time {timeSurvived:F1}s!");
    }

    public static void UnlockMap(string mapId)
    {
        if (string.IsNullOrEmpty(mapId)) return;
        PlayerPrefs.SetInt(UNLOCK_KEY_PREFIX + mapId, 1);
        PlayerPrefs.Save();
    }

    public static void ResetProgression()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[MapProgression] All map progression reset.");
    }
}
