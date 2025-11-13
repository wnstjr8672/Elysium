using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TacticsSkillController : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject blizzardPrefab;        // SpineBlizzardEffect �پ��ִ� ������
    public GameObject infernoBreathPrefab;   // SpineInfernoBreathEffect �پ��ִ� ������

    [Header("UI Refs")]
    public Button tacticButton;              // ���� ��ư
    public Image tacticButtonImage;          // ��ư ������ �̹���(������ ���� ���)

    [Header("Runtime Selected Tactic (Binder���� ä��)")]
    public TacticType currentType;           // Blizzard or InfernoBreath

    // Binder�� ���� ���� �� �־��ִ� �Ķ���͵�
    [HideInInspector] public float cooldown;     // ��ٿ�(��)
    [HideInInspector] public float duration;     // �����ڵ��(������ SpineBlizzardEffect�� �ٽ� ���)
    [HideInInspector] public int dps;          // �����ڵ��
    [HideInInspector] public int damage;       // ���丣�� �극�� ������
    [HideInInspector] public float speed;        // ���丣�� �극�� �̵��ӵ�
    [HideInInspector] public int usesMax;      // �� ��� ���� Ƚ�� (0�̸� ������ ���)
    [HideInInspector] public int usesLeft;     // ���� ��� Ƚ��

    private bool onCooldown = false;
    private Coroutine cdRoutine;

    // ��ư���� ȣ��
    public void OnClickBlizzard()
    {
        if (!CanUse()) return;

        currentType = TacticType.Blizzard;
        CastBlizzard();
        AfterUse();
    }

    public void OnClickInfernoBreath()
    {
        if (!CanUse()) return;

        currentType = TacticType.InfernoBreath;
        CastInferno();
        AfterUse();
    }

    private bool CanUse()
    {
        if (onCooldown) return false;
        if (usesMax > 0 && usesLeft <= 0) return false;
        return true;
    }

    private void AfterUse()
    {
        if (usesMax > 0 && usesLeft > 0)
            usesLeft--;

        if (cooldown > 0f)
            StartCooldown(cooldown);
        else
            RefreshButtonInteractableState();
    }

    // ���� ���� ����
    private void CastBlizzard()
{
    if (blizzardPrefab == null)
    {
        Debug.LogWarning("[TacticsSkillController] blizzardPrefab not assigned.");
        return;
    }

    var go = Instantiate(blizzardPrefab, transform.position, Quaternion.identity);
    var eff = go.GetComponent<SpineBlizzardEffect>();
    if (eff != null)
    {
        // slowFactor가 스펙에 있다면 거기서 가져오기, 없으면 프로젝트 기본값
        float slow = 0.5f;
        // 예) if (blizzardSpec != null) slow = blizzardSpec.slowFactor;

        eff.Init(
            enemyTag: "Enemy",
            duration: duration,
            dps: dps,
            slowFactor: slow,
            tickInterval: 0.5f
        );
    }
    else
    {
        Debug.LogWarning("[TacticsSkillController] SpineBlizzardEffect missing on prefab.");
    }
}

    private void CastInferno()
    {
        if (infernoBreathPrefab == null)
        {
            Debug.LogWarning("[TacticsSkillController] infernoBreathPrefab not assigned.");
            return;
        }

        GameObject go = Instantiate(infernoBreathPrefab, transform.position, Quaternion.identity);

        // ���⼭ ���� ����ü�� ���� ��ġ ����
        var eff = go.GetComponent<SpineInfernoBreathEffect>();
        if (eff != null)
        {
            eff.damage = damage;
            eff.speed = speed;
        }
        else
        {
            Debug.LogWarning("[TacticsSkillController] SpineInfernoBreathEffect missing on prefab.");
        }
    }

    // ��ٿ�� ��ư/������ ����
    private void StartCooldown(float cd)
    {
        if (cdRoutine != null) StopCoroutine(cdRoutine);
        cdRoutine = StartCoroutine(CooldownRoutine(cd));
    }

    private IEnumerator CooldownRoutine(float cd)
    {
        onCooldown = true;

        SetButtonInteractable(false);
        SetIconAlpha(0.3f); // ���������� �ȵ�

        float t = 0f;
        while (t < cd)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0.3f, 1f, t / cd); // �ε巴�� ����
            SetIconAlpha(a);
            yield return null;
        }

        onCooldown = false;
        cdRoutine = null;

        RefreshButtonInteractableState();
    }

    private void RefreshButtonInteractableState()
    {
        bool usable = !(usesMax > 0 && usesLeft <= 0);

        if (usable && !onCooldown)
        {
            SetButtonInteractable(true);
            SetIconAlpha(1f);
        }
        else
        {
            SetButtonInteractable(false);
            SetIconAlpha(0.3f);
        }
    }

    private void SetButtonInteractable(bool value)
    {
        if (tacticButton != null)
            tacticButton.interactable = value;
    }

    private void SetIconAlpha(float a)
    {
        if (tacticButtonImage == null) return;
        Color c = tacticButtonImage.color;
        c.a = a;
        tacticButtonImage.color = c;
    }
}
