using UnityEngine;

// Level progress stored in PlayerPrefs, keyed by build index.
public static class SaveSystem
{
    const string StarsKeyPrefix = "level_stars_";

    public static bool IsCompleted(int buildIndex)
    {
        return PlayerPrefs.HasKey(StarsKeyPrefix + buildIndex);
    }

    public static int GetBestStars(int buildIndex)
    {
        return PlayerPrefs.GetInt(StarsKeyPrefix + buildIndex, 0);
    }

    // The first level is always unlocked; each later level unlocks when the previous one is completed.
    public static bool IsUnlocked(int buildIndex)
    {
        return buildIndex <= 1 || IsCompleted(buildIndex - 1);
    }

    public static void RecordLevelComplete(int buildIndex, int stars)
    {
        if (IsCompleted(buildIndex) && stars <= GetBestStars(buildIndex)) return;

        PlayerPrefs.SetInt(StarsKeyPrefix + buildIndex, stars);
        PlayerPrefs.Save();
    }

    public static void ResetProgress(int levelCount)
    {
        for (int i = 0; i <= levelCount; i++)
            PlayerPrefs.DeleteKey(StarsKeyPrefix + i);
        PlayerPrefs.Save();
    }
}
