using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUnitButtonsBinder : MonoBehaviour
{
    [Header("Spawner & Catalog")]
    public UnitSpawner spawner;
    public GameObject[] unitPrefabs;      // 전사.prefab 등
    public string[] unitIdsSameOrder;     // unitPrefabs와 동일 순서로 ["Unit_Warrior", ...]

    [Header("8 Buttons (UI만 연결)")]
    public Button[] unitButtons = new Button[8];   // 0~7 (아이콘/라벨은 각자 버튼 밑에 직접 배치/설정)

    private Dictionary<string, int> idToIndex = new();

    void Awake()
    {
        idToIndex.Clear();
        int n = Mathf.Min(unitPrefabs.Length, unitIdsSameOrder.Length);
        for (int i = 0; i < n; i++)
        {
            var id = (unitIdsSameOrder[i] ?? "").Trim();
            if (!string.IsNullOrEmpty(id) && !idToIndex.ContainsKey(id))
                idToIndex.Add(id, i);
        }
    }

    void Start()
    {
        ApplyLoadout();
    }

    public void ApplyLoadout()
    {
        var slots = LoadoutPrefs.LoadUnits(8); // 항상 길이 8

        for (int i = 0; i < unitButtons.Length; i++)
        {
            var btn = unitButtons[i];
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();

            string id = (i < slots.Length) ? (slots[i] ?? "") : "";

            //  미리 선언+초기화 후 TryGetValue
            int prefabIndex = -1;
            bool hasUnit = !string.IsNullOrWhiteSpace(id) && idToIndex.TryGetValue(id, out prefabIndex);

            // 빈 슬롯: 클릭만 막기
            btn.interactable = hasUnit;

            if (hasUnit)
            {
                int captured = prefabIndex;  // 이제 확정 할당됨
                string capturedId = id;
                btn.onClick.AddListener(() => SpawnWithUpgrade(captured, capturedId));
            }
        }
    }


    private static string BuildUnitUpgradeKey(string unitId)
    {
        // 규칙: UPGRADE_Unit_Warrior
        return $"UPGRADE_{unitId}";
    }

    private void SpawnWithUpgrade(int prefabIndex, string unitId)
    {
        if (spawner == null) return;
        if (prefabIndex < 0 || prefabIndex >= unitPrefabs.Length) return;

        var go = Instantiate(unitPrefabs[prefabIndex], spawner.spawnPoint.position, Quaternion.identity);

        var applier = go.GetComponent<UnitStatsApplier>();
        if (applier != null)
        {
            applier.loadUpgradeFromPlayerPrefs = true;
            applier.upgradePlayerPrefsKey = BuildUnitUpgradeKey(unitId);
            applier.ApplyNow(); // 강화 재적용
        }
    }

#if UNITY_EDITOR
    // 에디터에서 빠르게 확인하려면, 인스펙터 우클릭 컨텍스트로 미리보기
    [ContextMenu("Re-Apply Loadout (Editor Preview)")]
    private void __EditorApply() => ApplyLoadout();
#endif
}
