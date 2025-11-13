using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineBlizzardEffect : MonoBehaviour
{
    [Header("Runtime Params")]
    public string enemyTag = "Enemy";
    public float duration = 6f;
    public int dps = 40;
    public float tickInterval = 0.5f;
    public float slowFactor = 0.5f; // 있다면

    private float timer;
    private float tick;

    public void Init(string enemyTag, float duration, int dps, float slowFactor, float tickInterval = 0.5f)
    {
        this.enemyTag = enemyTag;
        this.duration = duration;
        this.dps = dps;
        this.slowFactor = slowFactor;
        this.tickInterval = tickInterval;
    }

    private void Start()
    {
        timer = duration;
        tick = 0f;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        timer -= dt;
        tick += dt;

        if (tick >= tickInterval)
        {
            tick = 0f;
            DoTick();
        }

        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void DoTick()
    {
        // 전역 데미지: ENEMY 태그를 가진 모든 유닛에 틱 데미지
        foreach (var go in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            var u = go.GetComponent<BaseUnit>();
            if (u != null /* && u.IsAlive 등 필요시 */)
            {
                int dmg = Mathf.RoundToInt(dps * tickInterval); // dps 환산
                u.TakeDamage(dmg, attacker: null);
                // 슬로우가 따로 있으면 여기서 적용
            }
        }
    }
}