using UnityEngine;

[RequireComponent(typeof(BaseUnit))]
public class UnitStatsApplier : MonoBehaviour
{
    [Header("����")]
    public UnitStatsDefinition statsDefinition;

    [Header("���׷��̵� ���� �ҽ�")]
    public bool loadUpgradeFromPlayerPrefs = false;
    [Tooltip("PlayerPrefs Ű (loadUpgradeFromPlayerPrefs=true�� �� ���)")]
    public string upgradePlayerPrefsKey = "UPGRADE_LV_Default";

    [Tooltip("�ν����� �׽�Ʈ/������ ���� ���׷��̵� ����")]
    [Min(0)] public int currentUpgradeLevel = 0;

    [Header("�б� ���� (����� ��)")]
    [SerializeField] private UnitStatsSnapshot applied;

    private BaseUnit unit;

    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        ApplyNow();
    }

    /// <summary> �ν�����/��Ÿ�ӿ��� ��� ������ </summary>
    public void ApplyNow()
    {
        if (statsDefinition == null || unit == null)
        {
            Debug.LogWarning($"[{name}] UnitStatsApplier: statsDefinition �Ǵ� BaseUnit ������ �����ϴ�.");
            return;
        }

        int lv = currentUpgradeLevel;
        if (loadUpgradeFromPlayerPrefs)
        {
            lv = PlayerPrefs.GetInt(upgradePlayerPrefsKey, currentUpgradeLevel);
        }

        applied = statsDefinition.GetStats(lv);

        // BaseUnit�� �ݿ�
        unit.maxHealth = applied.maxHealth;
        unit.health = applied.maxHealth;    // ���� �� Ǯü��
        unit.attackDamage = applied.attackDamage;
        unit.attackCooldown = applied.reActionTime; // ���ൿ �ð� -> ��ٿ�
        unit.attackRange = applied.attackRange;
        unit.moveSpeed = applied.moveSpeed;
    }

    // �ٸ� �ý��ۿ��� �о �� �ֵ��� ���� ����
   public int GetProductionCost()
{
    EnsureAppliedUpToDate();
    return applied.productionCost;
}

public int GetAttackTargetCount()
{
    EnsureAppliedUpToDate();
    return applied.attackTargetCount;
}

public int GetUpgradeLevel()
{
    EnsureAppliedUpToDate();
    return applied.upgradeLevel;
}

public int GetUpgradeMax()
{
    EnsureAppliedUpToDate();
    return applied.upgradeMax;
}

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (statsDefinition != null && !Application.isPlaying)
        {
            applied = statsDefinition.GetStats(currentUpgradeLevel);
        }
    }
#endif
private void EnsureAppliedUpToDate()
{
    if (statsDefinition == null) return;

    int lv = loadUpgradeFromPlayerPrefs
        ? PlayerPrefs.GetInt(upgradePlayerPrefsKey, currentUpgradeLevel)
        : currentUpgradeLevel;

    // struct라 null 비교 불가 → 레벨 불일치나 값 비정상일 때만 재계산
    if (applied.upgradeLevel != lv || applied.productionCost <= 0)
    {
        applied = statsDefinition.GetStats(lv);
    }
}
}
