using UnityEngine;
using TMPro;

public class CostUIViewerTMP : MonoBehaviour
{
    [Header("UI Target")]
    public TMP_Text costText; // 인스펙터에 안 넣었으면 Awake에서 자동으로 잡아줌

    private bool subscribed = false; // 중복 구독 방지

    private void Awake()
    {
        // 자동 할당 (버튼 안에 TMP_Text 들어있는 구조 고려)
        if (costText == null)
        {
            costText = GetComponent<TMP_Text>();
            if (costText == null)
                costText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        TrySubscribeAndInit();
    }

    private void Start()
    {
        // OnEnable에서 실패했으면 여기서 한 번 더 시도
        TrySubscribeAndInit();
    }

    private void LateUpdate()
    {
        // 아직도 못 붙었으면(예: CostManager가 정말 늦게 생기는 구조) 마지막으로 한 번만 더 시도
        if (!subscribed)
        {
            TrySubscribeAndInit();
        }
    }

    private void OnDisable()
    {
        if (CostManager.I != null && subscribed)
        {
            CostManager.I.OnCostChanged -= HandleCostChanged;
        }
        subscribed = false;
    }

    private void TrySubscribeAndInit()
    {
        if (subscribed) return;

        if (CostManager.I == null)
        {
            // 디버그용
            // Debug.LogWarning("[CostUIViewerTMP] CostManager.I still null in TrySubscribeAndInit()");
            return;
        }

        // 여기 도달했다 = CostManager.I 생겼다
        Debug.Log("[CostUIViewerTMP] Subscribed to CostManager now");

        CostManager.I.OnCostChanged += HandleCostChanged;
        subscribed = true;

        HandleCostChanged(CostManager.I.CurrentCost, CostManager.I.maxCost);
    }

    private void HandleCostChanged(int current, int max)
    {
        if (costText == null)
        {
            Debug.LogWarning("[CostUIViewerTMP] costText is null, can't display cost");
            return;
        }

        // 너가 원하는 포맷으로 설정
        costText.text = current.ToString() + " / " + max.ToString();
    }
}
