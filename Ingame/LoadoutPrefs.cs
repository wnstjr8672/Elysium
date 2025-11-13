using System;
using System.Linq;
using UnityEngine;

public static class LoadoutPrefs
{
    private const string UNITS_KEY = "LOADOUT_UNITS";   // "Unit_Warrior,Unit_XXX,..."
    private const string TACTIC_KEY = "LOADOUT_TACTIC";  // "Tactic_Blizzard" ¶Ç´Â ""

    public static void SaveUnits(string[] unitIds, int maxSlots = 8)
    {
        if (unitIds == null) unitIds = Array.Empty<string>();
        var cleaned = unitIds.Select(s => (s ?? "").Trim())
                             .Take(Mathf.Max(0, maxSlots))
                             .ToArray();
        PlayerPrefs.SetString(UNITS_KEY, string.Join(",", cleaned));
        PlayerPrefs.Save();
    }

    public static string[] LoadUnits(int maxSlots = 8)
    {
        var raw = PlayerPrefs.GetString(UNITS_KEY, "");
        var arr = string.IsNullOrEmpty(raw)
            ? Array.Empty<string>()
            : raw.Split(new[] { ',' }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();

        // ½½·Ô ¼ö¿¡ ¸ÂÃç ±æÀÌ °íÁ¤ (ºóÄ­Àº "")
        if (arr.Length < maxSlots)
            Array.Resize(ref arr, maxSlots);
        return arr;
    }

    public static void SaveTactic(string tacticIdOrEmpty)
    {
        PlayerPrefs.SetString(TACTIC_KEY, tacticIdOrEmpty ?? "");
        PlayerPrefs.Save();
    }

    public static string LoadTactic()
    {
        return PlayerPrefs.GetString(TACTIC_KEY, "");
    }
}
