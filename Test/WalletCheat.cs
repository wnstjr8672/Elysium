// Assets/Scripts/DevTools/WalletCheat.cs
//
// 개발용 치트
// - F1: 골드 추가
// - F2: 크리스탈 추가
// - F3: 전체 초기화
//   (재화 0, OWNED_* 삭제, 전술 키(Blizzard 표기) 삭제,
//    기본 유닛 보유 재설정 + 강화 레벨 0으로 초기화, 상점 버튼 즉시 갱신)

using UnityEngine;

public class WalletCheat : MonoBehaviour
{
    [Header("Cheat Amounts")]
    public int addGold = 1000;
    public int addCrystal = 100;

    void Update()
    {
#if CHEATS
        // F1: 골드 추가
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (PlayerWallet.I != null)
            {
                PlayerWallet.I.Add(CurrencyType.Gold, addGold);
                Debug.Log($"[Cheat] 골드 +{addGold}");
            }
            else
            {
                Debug.LogWarning("[Cheat] PlayerWallet 인스턴스가 없습니다.");
            }
        }

        // F2: 크리스탈 추가
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (PlayerWallet.I != null)
            {
                PlayerWallet.I.Add(CurrencyType.Crystal, addCrystal);
                Debug.Log($"[Cheat] 크리스탈 +{addCrystal}");
            }
            else
            {
                Debug.LogWarning("[Cheat] PlayerWallet 인스턴스가 없습니다.");
            }
        }

        // F3: 전체 초기화
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ResetAll();
        }
#endif
    }

    private void ResetAll()
    {
        // 1) 재화 0
        if (PlayerWallet.I != null)
        {
            PlayerWallet.I.Add(CurrencyType.Gold, -PlayerWallet.I.Get(CurrencyType.Gold));
            PlayerWallet.I.Add(CurrencyType.Crystal, -PlayerWallet.I.Get(CurrencyType.Crystal));
        }
        else
        {
            Debug.LogWarning("[Cheat] PlayerWallet 인스턴스가 없어 재화 초기화는 건너뜁니다.");
        }

        // 2) OWNED_* 삭제 (상점 아이템 기준)
        var items = GameObject.FindObjectsOfType<ShopItem>(true);
        foreach (var it in items)
        {
            if (string.IsNullOrEmpty(it.itemId)) continue;
            PlayerPrefs.DeleteKey("OWNED_" + it.itemId);
        }

        // 3) 전술 보유/업그레이드 키 삭제 (Blizzard 표기로 통일)
        //    프로젝트에서 사용하는 정확한 전술 키로 유지하세요.
        PlayerPrefs.DeleteKey("OWNED_Tactic_Blizzard");
        PlayerPrefs.DeleteKey("UPGRADE_Tactic_Blizzard");

        PlayerPrefs.DeleteKey("OWNED_Tactic_InfernoBreath");
        PlayerPrefs.DeleteKey("UPGRADE_Tactic_InfernoBreath");

        // 4) 기본 유닛 보유/강화 0 초기화
        PlayerPrefs.SetInt("OWNED_Unit_Warrior", 1);
        PlayerPrefs.SetInt("OWNED_Unit_Archer", 1);
        PlayerPrefs.SetInt("OWNED_Unit_Wizard", 1);

        PlayerPrefs.SetInt("UPGRADE_Unit_Warrior", 0);
        PlayerPrefs.SetInt("UPGRADE_Unit_Archer", 0);
        PlayerPrefs.SetInt("UPGRADE_Unit_Wizard", 0);

        PlayerPrefs.Save();

        // 5) 상점 UI 즉시 갱신
        foreach (var it in items)
            it.ReloadOwnedFromPrefs();

        Debug.Log("[Cheat] 초기화 완료(F3): 재화0, OWNED_* 삭제, UPGRADE_Tactic_* 삭제(Blizzard 표기), 기본 유닛 보유/강화0 재설정");
    }
}