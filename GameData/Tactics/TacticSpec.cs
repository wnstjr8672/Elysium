using UnityEngine;

[CreateAssetMenu(fileName = "TacticSpec", menuName = "Game/Tactic Spec")]
public class TacticSpec : ScriptableObject
{
    // PlayerPrefs 키들
    public string tacticId = "Tactic_Blizzard";
    public string ownedKey = "OWNED_Tactic_Blizzard";
    public string upgradeKey = "UPGRADE_Tactic_Blizzard";

    // 표시용
    public string displayName = "Blizzard";
    public Sprite iconSprite;
    public Sprite detailSprite;

    // 수치 참조
    public TacticStatsDefinition stats;

    // 강화 비용: 현재 레벨 인덱스 → 다음 레벨 비용
    public int[] upgradeCosts;
}
