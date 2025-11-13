using UnityEngine;

[CreateAssetMenu(fileName = "TacticStats", menuName = "Game/Tactic Stats Definition")]
public class TacticStatsDefinition : ScriptableObject
{
    public TacticType type;

    [Range(1, 9)] public int baseUses = 1;
    public int baseCooldown = 10;

    // Blizzard
    public float baseDuration = 9f;
    public float addDurationPerUpgrade = 0f;
    public int baseDps = 0;
    public int addDpsPerUpgrade = 0;

    // Inferno
    public int baseDamage = 0;
    public int addDamagePerUpgrade = 0;
    public float baseSpeed = 8f;

    public int maxUpgradeCount = 1;

    public TacticStatsSnapshot GetStats(int lv)
    {
        int clv = Mathf.Clamp(lv, 0, Mathf.Max(0, maxUpgradeCount));
        var s = new TacticStatsSnapshot
        {
            type = type,
            uses = Mathf.Clamp(baseUses, 1, 9),
            cooldown = baseCooldown,
            level = clv,
            levelMax = maxUpgradeCount,
            duration = baseDuration + addDurationPerUpgrade * clv,
            dps = baseDps + addDpsPerUpgrade * clv,
            damage = baseDamage + addDamagePerUpgrade * clv,
            speed = baseSpeed
        };
        return s;
    }
}

public struct TacticStatsSnapshot
{
    public TacticType type;
    public int uses;
    public int cooldown;
    public float duration; // Blizzard
    public int dps;        // Blizzard
    public int damage;     // Inferno
    public float speed;    // Inferno
    public int level;
    public int levelMax;
}
