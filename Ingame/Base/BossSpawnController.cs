using UnityEngine;

public class BossSpawnController : MonoBehaviour
{
    [Header("레퍼런스")]
    [SerializeField] private BaseHealth enemyBaseHealth; // 적 기지 BaseHealth (isPlayerBase = false)

    [Header("보스 프리팹")]
    public GameObject midBossPrefab;    // 스테이지4
    public GameObject finalBossPrefab;  // 스테이지5

    [Header("보스 소환 위치")]
    public Transform spawnPoint;        // 어디에 소환할지 (예: 적 기지 조금 앞)

    private bool bossSpawned = false;
    private int stageNumber = 1;

    private void Awake()
    {
        // 스테이지 번호 가져오기
        stageNumber = PlayerPrefs.GetInt("SelectedStage", 1);

        // enemyBaseHealth 자동 할당 보정 (실수 대비)
        if (enemyBaseHealth == null)
        {
            enemyBaseHealth = GetComponent<BaseHealth>();
        }

        if (enemyBaseHealth == null)
        {
            Debug.LogError("[BossSpawnController] enemyBaseHealth가 없음. 적 기지 오브젝트에 붙였는지 확인해.");
            return;
        }

        // 적 기지가 처음 맞았을 때 콜백 등록
        enemyBaseHealth.OnFirstHit += HandleEnemyBaseFirstHit;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (씬 언로드 시 안전)
        if (enemyBaseHealth != null)
        {
            enemyBaseHealth.OnFirstHit -= HandleEnemyBaseFirstHit;
        }
    }

    private void HandleEnemyBaseFirstHit(BaseHealth baseHealth, GameObject attacker)
    {
        if (bossSpawned) return; // 혹시라도 중복 방지
        bossSpawned = true;

        Debug.Log($"[BossSpawnController] 스테이지 {stageNumber} 첫 피격 감지 -> 보스 소환 시작");

        // 어디에 소환할지 위치 결정
        Vector3 pos;
        if (spawnPoint != null)
            pos = spawnPoint.position;
        else
            pos = baseHealth.transform.position + new Vector3(-1.5f, -2f, 0f);
        // Z는 필요에 맞게. -1.5f는 플레이어 쪽으로 약간 나오게 한 임시값

        if (stageNumber == 4)
        {
            if (midBossPrefab != null)
            {
                Instantiate(midBossPrefab, pos, Quaternion.identity);
                Debug.Log("[BossSpawnController] 중간보스 소환됨.");
            }
        }
        else if (stageNumber == 5)
        {
            if (finalBossPrefab != null)
            {
                Instantiate(finalBossPrefab, pos, Quaternion.identity);
                Debug.Log("[BossSpawnController] 최종보스 소환됨.");
            }
        }
        else
        {
            // 1~3스테이지 같은 경우 보스 없음
            Debug.Log("[BossSpawnController] 이 스테이지는 보스 없음.");
        }
    }
}
