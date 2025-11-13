using UnityEngine;

public static class UpgradeLevelProvider
{
    public static int GetUpgradeLevel(string key, int defaultLv = 0)
    {
        return PlayerPrefs.GetInt(key, defaultLv);
    }

    public static void SetUpgradeLevel(string key, int level)
    {
        if (level < 0) level = 0;
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }

    public static void IncrementUpgrade(string key, int maxLevel)
    {
        int lv = GetUpgradeLevel(key, 0);
        lv = Mathf.Clamp(lv + 1, 0, Mathf.Max(0, maxLevel));
        SetUpgradeLevel(key, lv);
    }
}
