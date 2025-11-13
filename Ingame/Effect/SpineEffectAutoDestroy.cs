using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

[DisallowMultipleComponent]
public class SpineEffectAutoDestroy : MonoBehaviour
{
    [Header("Spine")]
    [Tooltip("1회 재생할 Spine 애니메이션")]
    public AnimationReferenceAsset spineAnimation;

    [Header("Fallback")]
    [Tooltip("애니메이션 정보를 못 찾았을 때 대기 시간")]
    public float fallbackLifetime = 1.2f;

    private void OnEnable()
    {
        StartCoroutine(PlayAndKill());
    }

    private IEnumerator PlayAndKill()
    {
        var skel = GetComponentInChildren<SkeletonAnimation>();
        if (skel != null && spineAnimation != null)
        {
            if (!skel.valid) skel.Initialize(true);
            skel.AnimationState.ClearTracks();
            var entry = skel.AnimationState.SetAnimation(0, spineAnimation, false);

            bool done = false;
            entry.Complete += _ => done = true;
            while (!done) yield return null;

            Destroy(gameObject);
            yield break;
        }

        // 폴백
        yield return new WaitForSeconds(fallbackLifetime);
        Destroy(gameObject);
    }
}
