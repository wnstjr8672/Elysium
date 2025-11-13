using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(Collider2D))]
public class SpineInfernoBreathEffect : MonoBehaviour
{
    [Header("Move")]
    public float speed = 8f;
    public float endX = 20f;  // 이 x를 지나치면 삭제

    [Header("Damage")]
    public float damage = 120f;
    public LayerMask hitLayers;  // Enemy 레이어 등

    [Header("Spine")]
    public AnimationReferenceAsset loopAnimation;

    private HashSet<GameObject> hitOnce = new HashSet<GameObject>();
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Rigidbody2D는 프리팹에 직접 추가해둔 상태 유지:
        // - Body Type: Kinematic
        // - Gravity Scale: 0

        var skel = GetComponentInChildren<SkeletonAnimation>();
        if (skel != null && loopAnimation != null)
        {
            if (!skel.valid) skel.Initialize(true);
            skel.AnimationState.SetAnimation(0, loopAnimation, true);
        }
    }

    private void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (transform.position.x >= endX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적/기지만 치도록 레이어 필터
        if (((1 << other.gameObject.layer) & hitLayers.value) == 0)
            return;

        TryHit(other.gameObject);
    }

    private void TryHit(GameObject go)
    {
        // 이미 같은 파츠에 맞았다면 무시
        if (hitOnce.Contains(go)) return;
        hitOnce.Add(go);

        // (1) 유닛에 히트된 경우: 자식 히트박스라도 부모까지 올라가서 BaseUnit 찾는다
        var unit = go.GetComponentInParent<BaseUnit>();
        if (unit != null)
        {
            unit.TakeDamage(damage, gameObject); // attacker = 이 브레스 오브젝트
            return;
        }

        // (2) 기지 같은 거: BaseHealth 찾기
        var baseHealth = go.GetComponentInParent<BaseHealth>();
        if (baseHealth != null)
        {
            baseHealth.TakeDamage(damage, gameObject);
        }
    }
}
