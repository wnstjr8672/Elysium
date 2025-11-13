using UnityEngine;

[CreateAssetMenu(fileName = "UnitSpec", menuName = "Game/Unit Spec")]
public class UnitSpec : ScriptableObject
{
    [Header("ID / Keys")]
    public string unitId = "Unit_Archer";
    public string ownedKey = "OWNED_Unit_Archer";
    public string upgradeKey = "UPGRADE_UniT_Archer";

    [Header("Display")]
    public string displayName = "Archer";
    public Sprite iconSprite;        // UnitIcon 버튼의 소스 이미지
    public Sprite detailSprite;      // UnitUpGradeWindow의 UnitImage

    [Header("Refs")]
    public UnitStatsDefinition stats;    // 스탯 정의(SO)

    [Header("Upgrade Costs (per level up)")]
    public int[] upgradeCosts;       // [현재Lv]→ 다음Lv로 올리는 비용 (예: 0→1 비용은 index 0)
}
