using Spine.Unity;
using UnityEngine;

public enum AnimState { Idle, Walk, Attack, Dead }

public abstract class BaseUnit : MonoBehaviour
{
    [Header("Stats")]
    public float health;
    public float maxHealth;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float moveSpeed;

    protected float lastAttackTime;
    protected Transform target;
    protected bool isDead = false;

    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;
    public AnimationReferenceAsset walk;
    public AnimationReferenceAsset attack;
    public AnimationReferenceAsset dead;
    public AnimationReferenceAsset hit;

    [Header("SFX")]
    public AudioClip attackClip;
    public static AudioClip knockbackClip;
    public static AudioClip playerDeathClip;
    public static AudioClip enemyDeathClip;
    public AudioClip healClip;

    public bool IsDead => isDead;

    [Header("Unit Options")]
    public bool hasKnockback = false;

    [Header("Knockback Settings")]
    [Range(0f, 3f)] public float knockbackSoundDelay = 0.25f;
    [Range(0f, 2f)] public float knockbackDistance = 0.2f;
    [Range(0f, 1f)] public float knockbackDelay = 0.15f;

    [Header("Stun Settings")]
    [Range(0f, 2f)] public float StunTime = 1.0f;

    [Header("Effect")]
    public GameObject effectPrefab;

    [Header("Tactics Slow Effect (Per-Unit)")]
    [Range(0f, 1f)] public float tacticsMoveEffectScale = 1f;

    protected AudioSource audioSource;
    protected float animLockTime = 0f;
    protected AnimState currentAnimState = AnimState.Idle;
    protected bool isMovementLocked = false;

    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Update() { }

    protected abstract void AcquireTarget();
    protected abstract void MoveForward();
    public abstract bool IsEnemy();

    public virtual void TakeDamage(float damage, GameObject attacker)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"{gameObject.name} 데미지 {damage} 받음. 남은 체력: {health}");

        if (attacker != null)
        {
            var attackerUnit = attacker.GetComponent<BaseUnit>();
            if (attackerUnit != null && attackerUnit.effectPrefab != null)
            {
                Vector3 effectPos = transform.position + new Vector3(0, 0.5f, 0);
                Instantiate(attackerUnit.effectPrefab, effectPos, Quaternion.identity);
            }

            if (attackerUnit != null && attackerUnit.hasKnockback)
            {
                float delay = attackerUnit.knockbackSoundDelay;
                knockbackDistance = attackerUnit.knockbackDistance;

                Invoke(nameof(ApplyKnockback), attackerUnit.knockbackDelay);
                Invoke(nameof(PlayKnockbackSFX_WithBoost), delay);

                if (hit != null)
                {
                    PlayAnimation(hit, true, AnimState.Idle);
                    skeletonAnimation.timeScale = 0f;
                }

                animLockTime = Time.time + StunTime;
                isMovementLocked = true;
                Invoke(nameof(UnlockAfterStun), StunTime);
            }
        }

        if (health <= 0 && !isDead) Die();
    }

    protected void PlayKnockbackSFX_WithBoost()
    {
        if (knockbackClip != null && audioSource != null)
        {
            float volume = GameManager.Instance.sfxVolume * GameManager.Instance.knockbackVolumeBoost;
            audioSource.PlayOneShot(knockbackClip, volume);
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 사망!");

        skeletonAnimation.timeScale = 1f;
        PlayAnimation(dead, false, AnimState.Dead);

        AudioClip clip = IsEnemy() ? enemyDeathClip : playerDeathClip;
        float volumeBoost = IsEnemy() ? GameManager.Instance.enemyDeathVolumeBoost
                                      : GameManager.Instance.playerDeathVolumeBoost;

        if (clip != null && audioSource != null)
        {
            float volume = GameManager.Instance.sfxVolume * volumeBoost;
            audioSource.PlayOneShot(clip, volume);
        }

        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// **단일 대상 공격 이벤트**: 쿨타임/애니메이션 처리 포함.
    /// 실제 피해 적용은 ApplyDamageToTarget()에 위임.
    /// </summary>
    protected virtual void Attack(GameObject target)
    {
        if (Time.time < lastAttackTime + attackCooldown || isMovementLocked) return;

        lastAttackTime = Time.time;
        PlayAnimation(attack, false, AnimState.Attack);
        animLockTime = Time.time + 0.5f;

        float attackDelay = skeletonAnimation.Skeleton.Data.FindAnimation(attack.name)?.Duration * 0.3f ?? 0.1f;
        Invoke(nameof(PlayAttackSFX), attackDelay);

        // 피해 적용
        ApplyDamageToTarget(target);
    }

    /// <summary>
    /// **피해 적용 전용 헬퍼** (애니메이션/쿨타임 관여 X)
    /// 멀티타깃 공격에서 여러 번 호출 가능.
    /// </summary>
    protected void ApplyDamageToTarget(GameObject targetGO)
    {
        if (targetGO == null) return;

        var unit = targetGO.GetComponent<BaseUnit>();
        if (unit != null)
        {
            unit.TakeDamage(attackDamage, this.gameObject);
        }

        var baseHealth = targetGO.GetComponent<BaseHealth>();
        if (baseHealth != null)
        {
            baseHealth.TakeDamage(attackDamage, this.gameObject);
        }
        else if (unit == null)
        {
            Debug.LogWarning($"{gameObject.name} → 타겟은 {targetGO.name}인데 BaseHealth 없음");
        }
    }

    protected virtual bool IsTooCloseToEnemyOrBase()
    {
        float spacing = 0.7f;
        string enemyTag = IsEnemy() ? "Player" : "Enemy";

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spacing);
        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            if (hit.CompareTag(enemyTag))
            {
                float dx = Mathf.Abs(hit.transform.position.x - transform.position.x);
                if (dx < spacing) return true;
            }
        }
        return false;
    }

    protected void PlayAnimation(AnimationReferenceAsset anim, bool loop, AnimState newState)
    {
        if (skeletonAnimation == null || anim == null) return;

        var current = skeletonAnimation.AnimationState.GetCurrent(0);
        if (currentAnimState == newState && newState != AnimState.Attack)
        {
            if (current != null && current.Animation.Name == anim.name)
                return;
        }

        currentAnimState = newState;
        skeletonAnimation.state.SetAnimation(0, anim, loop);

        if (!loop)
        {
            animLockTime = float.MaxValue;
            var trackEntry = skeletonAnimation.state.GetCurrent(0);
            if (trackEntry != null)
            {
                trackEntry.Complete += entry =>
                {
                    animLockTime = Time.time;
                    isMovementLocked = false;
                    currentAnimState = AnimState.Idle;
                };
            }
        }
    }

    protected virtual void ApplyKnockback()
    {
        Vector3 dir = IsEnemy() ? Vector3.right : Vector3.left;
        transform.position += dir * knockbackDistance;
    }

    private void UnlockAfterStun()
    {
        isMovementLocked = false;
        animLockTime = Time.time;
        skeletonAnimation.timeScale = 1f;
    }

    protected void PlayAttackSFX()
    {
        if (attackClip != null && audioSource != null)
            audioSource.PlayOneShot(attackClip, GameManager.Instance.sfxVolume);
    }

    protected void PlayKnockbackSFX()
    {
        if (knockbackClip != null && audioSource != null)
            audioSource.PlayOneShot(knockbackClip, GameManager.Instance.sfxVolume);
    }
}
