using UnityEngine;
using System.Collections.Generic;

/// <summary>
///                       鼭:
/// - LOADOUT_UNITS                д´ 
/// - GameManager.unitButtons[i]   SummonButtonController     Ī
/// -      ư              / ڽ Ʈ/  ȯ       Init()
///
///      ũ  Ʈ             GameObject  ϳ           δ .
///  ν    Ϳ    gameManager    巡     ָ    .
/// </summary>
public class SummonButtonBinder : MonoBehaviour
{
    [Header("    ")]
    public GameManager gameManager;


    private void Start()
    {
        if (gameManager == null)
        {
            
            return;
        }

        // 1) PlayerPrefs           ε 
        string raw = PlayerPrefs.GetString("LOADOUT_UNITS", "");
        string[] slots = new string[8];
        if (!string.IsNullOrEmpty(raw))
        {
            var arr = raw.Split(',');
            for (int i = 0; i < 8; i++)
                slots[i] = i < arr.Length ? (arr[i] ?? "") : "";
        }
        else
        {
            for (int i = 0; i < 8; i++)
                slots[i] = "";
        }

        // 2) unitId -> prefabIndex        ųʸ 
        Dictionary<string, int> idToIndex = new Dictionary<string, int>();

        int countMap = Mathf.Min(
            gameManager.unitPrefabs != null ? gameManager.unitPrefabs.Length : 0,
            gameManager.unitIdsSameOrder != null ? gameManager.unitIdsSameOrder.Length : 0,
            gameManager.unitIconsSameOrder != null ? gameManager.unitIconsSameOrder.Length : 0
        );

        for (int i = 0; i < countMap; i++)
        {
            string id = (gameManager.unitIdsSameOrder[i] ?? "").Trim();
            if (!string.IsNullOrEmpty(id) && !idToIndex.ContainsKey(id))
                idToIndex.Add(id, i);
        }

        // 3)   ư    ʱ ȭ
        for (int i = 0; i < gameManager.unitButtons.Length; i++)
        {
            var btn = gameManager.unitButtons[i];
            if (btn == null)
            {
                
                continue;
            }

            //   ư     SummonButtonController         
            var summonCtrl = btn.GetComponent<SummonButtonController>();
            if (summonCtrl == null)
            {
                // Ȥ     ư    ڽ ( Ǵ   θ )    ٿ    ٸ   ѹ     ã ƺ 
                summonCtrl = btn.GetComponentInChildren<SummonButtonController>(true);
            }

            //           
            string unitId = (i < slots.Length) ? (slots[i] ?? "") : "";
            

            if (summonCtrl == null)
            {
                
                continue;
            }

            //       Կ                      ִ    Ȯ  
            bool hasUnit =
                !string.IsNullOrWhiteSpace(unitId) &&
                idToIndex.ContainsKey(unitId);

            if (!hasUnit)
            {
                //      ִ                  +   Ȱ  ȭ
                summonCtrl.ResetEmpty(gameManager.emptyUnitIcon);
                continue;
            }

            //              ε    ã  
            int prefabIndex = idToIndex[unitId];
            var unitPrefab = gameManager.unitPrefabs[prefabIndex];
            var iconSprite = gameManager.unitIconsSameOrder[prefabIndex];

            //       ڽ Ʈ  ˾ƿ   
            int productionCost = 0;
            if (unitPrefab != null)
            {
                var applier = unitPrefab.GetComponent<UnitStatsApplier>();
                if (applier != null)
                {
                    productionCost = applier.GetProductionCost();
                }
                Debug.Log($"[Binder] idx={i} id={unitId} cost={productionCost}");
            }

            //   ư Ȱ  ȭ/              
            //   ư Ȱ  ȭ/              
            summonCtrl.Init(
                gameManager,
                prefabIndex,
                unitId,
                iconSprite,
                productionCost
            );


        }
    }
}
