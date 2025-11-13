using UnityEngine;
using System.Collections;

public class BaseHealth : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 10000f;
    public float currentHealth;
    public bool isPlayerBase = true;

    [Header("공용 효과음")]
    public AudioClip destroyedClip;

    [Header("기지 이미지")]
    public Sprite normalSprite;
    public Sprite halfDamagedSprite;
    public Sprite destroyedSprite;

    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.1f;

    private SpriteRenderer spriteRenderer;
    private bool isHalfDamaged = false;

    // === 추가: 첫 피격 감지용 ===
    private bool firstHitTriggered = false;
    public delegate void FirstHitDelegate(BaseHealth baseHealth, GameObject attacker);
    public event FirstHitDelegate OnFirstHit;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    // 공격자 정보까지 받아서 이펙트 처리 포함
    public void TakeDamage(float amount, GameObject attacker = null)
    {
        // === 첫 피격 체크: 아직 한 번도 안 맞았으면 지금이 "첫 타"다 ===
        if (!firstHitTriggered)
        {
            firstHitTriggered = true;
            // 구독자(=보스 스폰 매니저)에게 알림
            OnFirstHit?.Invoke(this, attacker);
        }

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 기지 체력: {currentHealth}");

        // 공격 이펙트 처리
        if (attacker != null)
        {
            var attackerUnit = attacker.GetComponent<BaseUnit>();
            if (attackerUnit != null && attackerUnit.effectPrefab != null)
            {
                Vector3 effectPos = transform.position + new Vector3(0, 0.5f, 0);
                GameObject effect = Instantiate(attackerUnit.effectPrefab, effectPos, Quaternion.identity);
            }
        }

        // 반피 연출
        if (!isHalfDamaged && currentHealth <= maxHealth * 0.5f && currentHealth > 0)
        {
            isHalfDamaged = true;
            if (spriteRenderer != null && halfDamagedSprite != null)
                spriteRenderer.sprite = halfDamagedSprite;

            StartCoroutine(Shake(shakeDuration, shakeMagnitude));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        string baseName = isPlayerBase ? "아군" : "적군";
        Debug.Log($"{baseName} 기지 파괴됨!");

        if (spriteRenderer != null && destroyedSprite != null)
            spriteRenderer.sprite = destroyedSprite;

        if (destroyedClip != null)
            AudioSource.PlayClipAtPoint(destroyedClip, transform.position);

        StartCoroutine(Shake(shakeDuration, shakeMagnitude));

        GameManager.Instance.GameOver(isPlayerBase);
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPos + new Vector3(x, 0, 0);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
