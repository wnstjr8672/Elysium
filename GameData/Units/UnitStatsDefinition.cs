using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "Game/Unit Stats Definition")]
public class UnitStatsDefinition : ScriptableObject
{
    [Header("기본 능력치")]
    [Range(1, 999)] public int baseMaxHealth = 100;     // 체력(최대 999)
    [Range(1, 999)] public int baseAttackDamage = 10;    // 공격력(최대 999)

    [Tooltip("재행동 시간: 공격 모션 종료 후 다음 공격까지 대기 시간(초)")]
    [Range(1f, 9f)] public float baseReActionTime = 2f;  // 재행동 시간(1~9)

    [Range(1f, 9f)] public float baseAttackRange = 2f;   // 사정 거리(1~9)
    [Range(1f, 9f)] public float baseMoveSpeed = 3f;     // 이동 속도(1~9)

    [Range(0, 999)] public int baseProductionCost = 50;  // 생산 비용(최대 999)
    [Range(1, 9)] public int baseAttackTargetCount = 1;  // 공격 대상 수(1~9)

    [Header("업그레이드 (체력/공격력만 상승)")]
    [Min(0)] public int maxUpgradeCount = 1;             // 강화 가능 횟수
    [Min(0)] public int addHealthPerUpgrade = 10;        // 단계당 체력 상승량
    [Min(0)] public int addAttackPerUpgrade = 5;         // 단계당 공격력 상승량

    public UnitStatsSnapshot GetStats(int currentUpgradeCount)
    {
        int clampedLv = Mathf.Clamp(currentUpgradeCount, 0, Mathf.Max(0, maxUpgradeCount));

        int finalMaxHp = Mathf.Clamp(baseMaxHealth + addHealthPerUpgrade * clampedLv, 1, 999);
        int finalAtk = Mathf.Clamp(baseAttackDamage + addAttackPerUpgrade * clampedLv, 1, 999);

        return new UnitStatsSnapshot
        {
            maxHealth = finalMaxHp,
            attackDamage = finalAtk,
            reActionTime = Mathf.Clamp(baseReActionTime, 1f, 9f),
            attackRange = Mathf.Clamp(baseAttackRange, 1f, 9f),
            moveSpeed = Mathf.Clamp(baseMoveSpeed, 1f, 9f),
            productionCost = Mathf.Clamp(baseProductionCost, 0, 999),
            attackTargetCount = Mathf.Clamp(baseAttackTargetCount, 1, 9),
            upgradeLevel = clampedLv,
            upgradeMax = Mathf.Max(0, maxUpgradeCount)
        };
    }
}

public struct UnitStatsSnapshot
{
    public int maxHealth;
    public int attackDamage;
    public float reActionTime;
    public float attackRange;
    public float moveSpeed;
    public int productionCost;
    public int attackTargetCount;

    public int upgradeLevel;
    public int upgradeMax;
}
