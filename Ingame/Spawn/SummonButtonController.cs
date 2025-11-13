using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SummonButtonController : MonoBehaviour
{
    [Header("참조 (자동 할당 가능)")]
    public Button summonButton;
    public Image buttonImage;

    [Header("런타임 할당 (Binder가 채움)")]
    public GameManager gameManager;

    private int prefabIndex = -1;
    private string unitId;
    private int productionCost = 0;
    private bool isAssigned = false;

    // 구독 여부 추적
    private bool subscribed = false;
private CostManager cm;
    private void Awake()
    {
        if (summonButton == null)
            summonButton = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        summonButton.onClick.AddListener(OnClickSummon);

       
    }

    private void OnEnable()
    {
        
        TrySubscribeCost();
    }

    private void Update()
    {
        // CostManager가 나중에 뜬 경우를 커버
        if (!subscribed)
        {
            TrySubscribeCost();
        }
    }

    private void TrySubscribeCost()
    {
        if (subscribed) return;

        if (CostManager.I == null)
        {
            return;
        }
cm = CostManager.I;
        // 이제는 구독 가능
Debug.Log($"[SBC Subscribe] {gameObject.name} -> CostManager id={cm.GetInstanceID()} scene={cm.gameObject.scene.name}");
        cm.OnCostChanged += HandleCostChanged;
        subscribed = true;

        // 현재 코스트 상태로 즉시 한 번 반영
        HandleCostChanged(CostManager.I.CurrentCost, int.MaxValue);
    }

    private void OnDisable()
    {
        if (subscribed && CostManager.I != null)
        {
            CostManager.I.OnCostChanged -= HandleCostChanged;
        }
        subscribed = false;
    }

    public void ResetEmpty(Sprite emptySprite)
    {
        

        isAssigned = false;
        prefabIndex = -1;
        unitId = null;
        productionCost = 0;

        if (buttonImage != null && emptySprite != null)
            buttonImage.sprite = emptySprite;

        if (summonButton != null)
            summonButton.interactable = false;
    }

    public void Init(GameManager gm, int prefabIdx, string unitIdStr, Sprite iconSprite, int cost)
    {
        
        gameManager = gm;
        prefabIndex = prefabIdx;
        unitId = unitIdStr;
        productionCost = Mathf.Max(cost);

        isAssigned = true;

        if (buttonImage != null && iconSprite != null)
            buttonImage.sprite = iconSprite;

        // Init 시점에서도 한 번 반영
        if (CostManager.I != null)
        {
            HandleCostChanged(CostManager.I.CurrentCost, int.MaxValue);
        }
        else
        {
            summonButton.interactable = false;
        }

        
    }

    private void HandleCostChanged(int currentCost, int _maxCost)
    {
        if (!isAssigned)
        {
            if (summonButton != null)
                summonButton.interactable = false;

            return;
        }

        bool canAfford = (currentCost >= productionCost);

        if (summonButton != null)
            summonButton.interactable = canAfford;

        
    }

    private void OnClickSummon()
{
    if (!isAssigned || gameManager == null || prefabIndex < 0) return;
    if (cm == null) return;               //  아직 구독 전이면 무시

    if (!cm.TrySpend(productionCost))     //  구독한 CM에게만 지불
        return;

    gameManager.PlayUIClick();
    gameManager.SpawnWithUpgrade(prefabIndex, unitId);
}
}