using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 블리자드 같은 전술 효과에서 이동속도 감소 값을 적 유닛에 붙여두는 용도.
/// 지금은 SpineBlizzardEffect가 호출하는 AddSlow / RemoveSlow / GetBlendedMoveMultiplier 만 제공.
/// 이동속도 계산 외에 다른 전투 로직은 전혀 안 건드림.
/// </summary>
public class EnemyDebuffAdapter : MonoBehaviour
{
    // 현재 이 유닛에게 적용된 슬로우 비율들 (0~1 사이 값)
    // 예: 0.5f => 이동 속도 50%
    private readonly List<float> activeSlows = new List<float>();

    /// <summary>
    /// SpineBlizzardEffect에서 슬로우를 추가할 때 호출.
    /// 같은 값이 여러 번 들어올 수도 있으므로 그냥 누적 저장한다.
    /// </summary>
    public void AddSlow(float slowFactor)
    {
        // 방어 처리: 말도 안 되는 값 방지
        if (slowFactor <= 0f) slowFactor = 0.01f;
        if (slowFactor > 1f) slowFactor = 1f;

        activeSlows.Add(slowFactor);
    }

    /// <summary>
    /// SpineBlizzardEffect에서 해제할 때 호출.
    /// 동일한 slowFactor 한 개만 제거한다.
    /// </summary>
    public void RemoveSlow(float slowFactor)
    {
        activeSlows.Remove(slowFactor);
    }

    /// <summary>
    /// EnemyUnit.MoveForward() 쪽에서 호출할 이동 속도 보정 값.
    ///
    /// tacticsMoveEffectScale(유닛별 추가 보정)은 EnemyUnit 안에서 곱하므로,
    /// 여기선 순수하게 "현재 슬로우 영향만" 계산해서 돌려준다.
    ///
    /// activeSlows 중 제일 작은 값(=가장 강한 슬로우)만 실제로 적용.
    /// 아무 것도 없으면 1f 반환.
    /// </summary>
    public float GetBlendedMoveMultiplier(float tacticsScale)
    {
        // 현재 걸린 슬로우 중 최저값
        float slowMul = 1f;
        if (activeSlows.Count > 0)
        {
            slowMul = 1f;
            for (int i = 0; i < activeSlows.Count; i++)
            {
                if (activeSlows[i] < slowMul)
                    slowMul = activeSlows[i];
            }
        }

        // EnemyUnit.MoveForward()는 이 값을 그대로 moveMul로 쓸 거고
        // 이미 tacticsMoveEffectScale을 인자로 넘겨주고 있으므로 여기서 곱해준다.
        return slowMul * tacticsScale;
    }
}
