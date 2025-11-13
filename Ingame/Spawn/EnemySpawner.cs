using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 적 유닛 프리팹
    public float minSpawnTime = 1f; // 최소 스폰 주기
    public float maxSpawnTime = 5f; // 최대 스폰 주기

    private static float zOffset = 0f; // 적 유닛 Z값 누적용
    public float zIncrement = 0.1f;    // 한 번 소환마다 Z값 증가량 (인스펙터에서 조절 가능)

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // 랜덤한 스폰 시간
            float spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(spawnTime);

            // 적 유닛 소환
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = -2f;     // Y축 -2 고정
        spawnPosition.z = zOffset; // Z축 누적값 설정

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        zOffset += zIncrement; // 다음 소환 시 Z값 증가
    }
}
