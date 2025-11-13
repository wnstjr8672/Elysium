using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

[DisallowMultipleComponent]
public class SpineEffectSequencer : MonoBehaviour
{
    [Header("Spine")]
    [Tooltip("순서대로 재생할 Spine 애니메이션들 (1번, 2번, 3번...)")]
    public AnimationReferenceAsset[] animations;

    [Header("Behavior")]
    [Tooltip("마지막 애니메이션을 반복 재생할지 여부 (true면 마지막을 루프, false면 마지막도 1회 후 종료)")]
    public bool loopLast = true;

    [Tooltip("마지막 애니메이션까지 끝난 뒤 자동 파괴 (loopLast=false 일 때만 의미가 있음)")]
    public bool autoDestroyOnFinish = true;

    [Tooltip("애니메이션들 사이에 넣을 지연(초)")]
    public float delayBetween = 0f;

    [Header("Fallback")]
    [Tooltip("Spine 세팅을 못 찾거나 애니가 비어 있을 때 대기 후 파괴")]
    public float fallbackLifetime = 1.2f;

    private SkeletonAnimation skel;

    private void OnEnable()
    {
        skel = GetComponentInChildren<SkeletonAnimation>();
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (skel == null || animations == null || animations.Length == 0)
        {
            // 스파인 컴포넌트나 애니가 없으면 폴백
            yield return new WaitForSeconds(fallbackLifetime);
            if (autoDestroyOnFinish) Destroy(gameObject);
            yield break;
        }

        if (!skel.valid) skel.Initialize(true);
        skel.AnimationState.ClearTracks();

        // 1 ~ (N-1)번: 1회 재생
        int lastIndex = animations.Length - 1;
        for (int i = 0; i < lastIndex; i++)
        {
            var clip = animations[i];
            if (clip == null) continue;

            bool finished = false;
            TrackEntry entry = skel.AnimationState.SetAnimation(0, clip, false);
            entry.Complete += _ => finished = true;

            while (!finished) yield return null;

            if (delayBetween > 0f) yield return new WaitForSeconds(delayBetween);
        }

        // 마지막 슬롯 처리
        var lastClip = animations[lastIndex];
        if (lastClip == null)
        {
            // 마지막이 비었으면 그냥 종료
            if (autoDestroyOnFinish) Destroy(gameObject);
            yield break;
        }

        if (loopLast)
        {
            // 마지막 애니메이션을 루프 재생하고 그대로 유지(파괴 안 함)
            skel.AnimationState.SetAnimation(0, lastClip, true);
            yield break;
        }
        else
        {
            // 마지막도 1회 재생 후 종료(옵션에 따라 파괴)
            bool done = false;
            var entry = skel.AnimationState.SetAnimation(0, lastClip, false);
            entry.Complete += _ => done = true;

            while (!done) yield return null;

            if (autoDestroyOnFinish) Destroy(gameObject);
        }
    }
}
