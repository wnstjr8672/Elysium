using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [Header("소환 위치")]
    public Transform spawnPoint;

    [Header("유닛 프리팹 목록")]
    public GameObject[] unitPrefabs;

    private static float zOffset = 0f; // Z값 누적용
    public float zIncrement = 0.1f;    // 인스펙터에서 조절 가능

    public void SpawnUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= unitPrefabs.Length)
        {
            Debug.LogWarning("잘못된 유닛 인덱스: " + unitIndex);
            return;
        }

        // Z값 누적 적용
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.z = zOffset;

        Instantiate(unitPrefabs[unitIndex], spawnPos, Quaternion.identity);

        // Z값 증가 및 제한
        zOffset += zIncrement;
        if (zOffset > 1f) zOffset = 0f;
    }
}
