using UnityEngine;
using System.Collections.Generic;

public class EnemyUnit : BaseUnit
{
    [SerializeField] private LayerMask targetLayer;

    [Header("전투 이탈 대기 시간")]
    [Range(0f, 3f)]
    public float attackIdleTimeout = 0.5f;

    private float attackWaitTimer = 0f;
    private UnitStatsApplier statsApplier;

    protected override void Start()
    {
        base.Start();
        statsApplier = GetComponent<UnitStatsApplier>();
    }

    protected override void AcquireTarget()
    {
        float detectionRange = attackRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayer);
        float nearestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }
        }

        target = nearest;
    }

    protected override void MoveForward()
    {
        if (isMovementLocked) return;

        var ad = GetComponent<EnemyDebuffAdapter>();
        float moveMul = 1f;
        if (ad != null)
        {
            moveMul = ad.GetBlendedMoveMultiplier(tacticsMoveEffectScale);
        }

        transform.position += Vector3.left * (moveSpeed * moveMul) * Time.deltaTime;
    }

    public override bool IsEnemy() => true;

    protected override void Update()
    {
        if (isDead) return;
        if (Time.time < animLockTime) return;

        AcquireTarget();
        if (isMovementLocked) return;

        if (target != null)
        {
            float dist = Mathf.Abs(transform.position.x - target.position.x);
            if (dist <= attackRange)
            {
                if (currentAnimState != AnimState.Attack)
                    PlayAnimation(null, true, AnimState.Attack);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    int count = (statsApplier != null) ? Mathf.Max(1, statsApplier.GetAttackTargetCount()) : 1;
                    AttackMultiple(count);
                    attackWaitTimer = 0f;
                }
                else
                {
                    attackWaitTimer += Time.deltaTime;
                    if (attackWaitTimer >= attackIdleTimeout && currentAnimState == AnimState.Attack)
                    {
                        PlayAnimation(null, true, AnimState.Idle);
                        currentAnimState = AnimState.Idle;
                    }
                }
                return;
            }
            else
            {
                attackWaitTimer += Time.deltaTime;
                if (attackWaitTimer >= attackIdleTimeout && currentAnimState == AnimState.Attack)
                {
                    PlayAnimation(null, true, AnimState.Idle);
                    currentAnimState = AnimState.Idle;
                }
            }
        }
        else
        {
            attackWaitTimer += Time.deltaTime;
            if (attackWaitTimer >= attackIdleTimeout && currentAnimState == AnimState.Attack)
            {
                PlayAnimation(null, true, AnimState.Idle);
                currentAnimState = AnimState.Idle;
            }
        }

        if (!IsTooCloseToEnemyOrBase())
        {
            if (currentAnimState != AnimState.Walk)
                PlayAnimation(walk, true, AnimState.Walk);

            MoveForward();
        }
        else
        {
            if (currentAnimState != AnimState.Idle)
                PlayAnimation(null, true, AnimState.Idle);
        }
    }

    private void AttackMultiple(int count)
    {
        // 공통: 애니메이션 1회 + SFX 예약 + 쿨다운 설정
        lastAttackTime = Time.time;
        PlayAnimation(attack, false, AnimState.Attack);
        animLockTime = Time.time + 0.5f;

        float attackDelay = skeletonAnimation.Skeleton.Data.FindAnimation(attack.name)?.Duration * 0.3f ?? 0.1f;
        Invoke(nameof(PlayAttackSFX), attackDelay);

        // 사거리 내 플레이어 유닛 중 가까운 순으로 N개 추출
        List<GameObject> targets = GetClosestTargets(count, "Player", targetLayer);
        for (int i = 0; i < targets.Count; i++)
        {
            ApplyDamageToTarget(targets[i]);
        }
    }

    private List<GameObject> GetClosestTargets(int count, string requiredTag, LayerMask mask)
    {
        var list = new List<GameObject>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);

        // 거리순 정렬용 버퍼
        var temp = new List<(float dist, GameObject go)>();
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            if (!h.CompareTag(requiredTag)) continue;

            float d = Vector3.SqrMagnitude(h.transform.position - transform.position);
            temp.Add((d, h.gameObject));
        }
        temp.Sort((a, b) => a.dist.CompareTo(b.dist));

        int n = Mathf.Min(count, temp.Count);
        for (int i = 0; i < n; i++) list.Add(temp[i].go);
        return list;
    }
}
