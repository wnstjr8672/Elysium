using UnityEngine;
using System;

/// <summary>
/// 전투 중 자동으로 차오르는 "소환 자원(Cost)"을 관리하는 매니저.
/// - 시간이 지나면 CurrentCost가 regenPerSecond만큼 증가
/// - TrySpend(cost)로 지불 시도 가능
/// - OnCostChanged 이벤트로 UI(버튼 등)가 상태를 갱신
/// </summary>
public class CostManager : MonoBehaviour
{
    public static CostManager I;

    [Header("Cost 설정")]
    [Min(0)] public int startCost = 50;          // 전투 시작 시 코스트
    [Min(1)] public int maxCost = 200;           // 코스트 최대치
    [Min(0f)] public float regenPerSecond = 5f;  // 초당 회복량

    [Header("런타임 (디버그 표시용)")]
    [SerializeField] private float currentCostFloat;
    public int CurrentCost { get; private set; }

    /// <summary>
    /// 코스트가 갱신될 때마다 호출됨.
    /// arg1: 현재 코스트
    /// arg2: 최대 코스트
    /// </summary>
    public event Action<int, int> OnCostChanged;

    private void Awake()
    {
        if (I != null && I != this)
        {
Debug.LogWarning($"[CostManager] Duplicate! Destroying {name} in scene {gameObject.scene.name} (keep id={I.GetInstanceID()})");
            Destroy(gameObject);
            return;
        }
        I = this;
Debug.Log($"[CostManager] Awake id={GetInstanceID()} scene={gameObject.scene.name}");
        currentCostFloat = startCost;
        ClampAndSync();
    }

    private void Update()
    {
        if (CurrentCost < maxCost)
        {
            currentCostFloat += regenPerSecond * Time.deltaTime;
            ClampAndSync();
        }
    }

    /// <summary>
    /// cost 만큼의 코스트를 소모할 수 있으면 true 반환하고 즉시 차감.
    /// 부족하면 false.
    /// </summary>
    public bool TrySpend(int cost)
    {
        if (CurrentCost < cost)
            return false;

        currentCostFloat -= cost;
        if (currentCostFloat < 0f)
            currentCostFloat = 0f;

        ClampAndSync();
        return true;
    }

    private void ClampAndSync()
    {
        if (currentCostFloat > maxCost)
            currentCostFloat = maxCost;

        CurrentCost = Mathf.FloorToInt(currentCostFloat);

        OnCostChanged?.Invoke(CurrentCost, maxCost);
    }
}
