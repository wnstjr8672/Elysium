using UnityEngine;

public class StageSpawnerSelector : MonoBehaviour
{
    [Header("스테이지별 스포너 (index 0 = 스테이지1, index 1 = 스테이지2 ... )")]
    [SerializeField] private GameObject[] stageSpawners; // size = 5

    private void Awake()
    {
        // 1) 저장된 스테이지 읽기 (기본값 1)
        int stage = PlayerPrefs.GetInt("SelectedStage", 1);

        // 2) 전부 비활성화
        for (int i = 0; i < stageSpawners.Length; i++)
        {
            if (stageSpawners[i] != null)
                stageSpawners[i].SetActive(false);
        }

        // 3) 해당 스테이지만 활성화
        int index = Mathf.Clamp(stage - 1, 0, stageSpawners.Length - 1);

        if (stageSpawners != null && stageSpawners.Length > 0)
        {
            if (stageSpawners[index] != null)
                stageSpawners[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning("[StageSpawnerSelector] stageSpawners 비어있음.");
        }
    }
}
