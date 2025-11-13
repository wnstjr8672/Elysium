using UnityEngine;
using System.Collections.Generic;

public class PlayerUnit : BaseUnit
{
    [Header("Unit Type")]
    [SerializeField] private bool isHealer = false;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetLayer; // 적 탐지용
    [SerializeField] private LayerMask allyLayer;   // 힐 대상 탐지용

    private float attackTimer = 0f;                 // 재행동 타이머
    private UnitStatsApplier statsApplier;          // 멀티타겟/비용/업글 참조용

    protected override void Start()
    {
        base.Start();
        statsApplier = GetComponent<UnitStatsApplier>();
        if (statsApplier == null)
        {
            Debug.LogWarning($"[{name}] PlayerUnit: UnitStatsApplier가 없습니다. 기본값으로 동작합니다.");
        }
    }

    protected override void AcquireTarget()
    {
        float detectionRange = attackRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayer);

        float nearestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
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
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }

    public override bool IsEnemy() => false;

    protected override void Update()
    {
        if (isDead || Time.time < animLockTime || isMovementLocked)
            return;

        attackTimer += Time.deltaTime;

        AcquireTarget();

        if (target != null)
        {
            float dist = Mathf.Abs(transform.position.x - target.position.x);
            if (dist <= attackRange)
            {
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;

                    if (isHealer)
                    {
                        //  공격 직전 반드시 1배속으로 복귀
                        skeletonAnimation.timeScale = 1f;
                        PlayAnimation(attack, false, AnimState.Attack);
                        HealNearbyAllies();
                    }
                    else
                    {
                        //  공격 직전 반드시 1배속으로 복귀
                        skeletonAnimation.timeScale = 1f;
                        int count = (statsApplier != null) ? Mathf.Max(1, statsApplier.GetAttackTargetCount()) : 1;
                        AttackMultiple(count);
                    }
                }
                else
                {
                    // 쿨타임 대기 중엔 공격 포즈 유지 + 타임스케일 0으로 정지
                    if (currentAnimState != AnimState.Attack)
                    {
                        PlayAnimation(attack, true, AnimState.Attack);
                        skeletonAnimation.timeScale = 0f;
                    }
                }
                return;
            }
        }

        // 타겟 없음/사거리 밖 → 이동/대기
        skeletonAnimation.timeScale = 1f;

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

    // ===== 멀티 타겟 공격(비힐러) =====
    private void AttackMultiple(int count)
    {
        lastAttackTime = Time.time;
        PlayAnimation(attack, false, AnimState.Attack);
        animLockTime = Time.time + 0.5f;

        float attackDelay = skeletonAnimation.Skeleton.Data.FindAnimation(attack.name)?.Duration * 0.3f ?? 0.1f;
        Invoke(nameof(PlayAttackSFX), attackDelay);

        List<GameObject> targets = GetClosestTargets(count, "Enemy", targetLayer);
        for (int i = 0; i < targets.Count; i++)
        {
            ApplyDamageToTarget(targets[i]); // BaseUnit의 헬퍼 사용
        }
    }

    private List<GameObject> GetClosestTargets(int count, string requiredTag, LayerMask mask)
    {
        var list = new List<GameObject>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);

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

    // ===== 힐러: 멀티 타겟 회복 =====
    private void HealNearbyAllies()
    {
        int targetCount = (statsApplier != null) ? Mathf.Max(1, statsApplier.GetAttackTargetCount()) : 1;
        float healAmount = Mathf.Max(0f, attackDamage); // 공격력을 회복량으로 사용

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, allyLayer);
        List<BaseUnit> candidates = new List<BaseUnit>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            BaseUnit unit = hit.GetComponent<BaseUnit>();
            if (unit != null && !unit.IsEnemy() && !unit.IsDead && unit.health < unit.maxHealth)
            {
                candidates.Add(unit);
            }
        }

        if (candidates.Count == 0) return;

        // 결손 큰 순으로 우선 회복
        candidates.Sort((a, b) =>
        {
            float da = a.maxHealth - a.health;
            float db = b.maxHealth - b.health;
            return db.CompareTo(da);
        });

        int healNum = Mathf.Min(targetCount, candidates.Count);
        for (int i = 0; i < healNum; i++)
        {
            BaseUnit ally = candidates[i];
            ally.health = Mathf.Min(ally.health + healAmount, ally.maxHealth);
        }

        if (audioSource != null && healClip != null)
        {
            audioSource.PlayOneShot(healClip, GameManager.Instance.sfxVolume);
        }
    }
}
