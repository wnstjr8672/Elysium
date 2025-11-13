using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayButtonSFX : MonoBehaviour
{
    [Header("���� ����")]
    public AudioClip clickSound;           // Ŭ�� ȿ����
    [Range(0f, 1f)] public float volume = 0.6f; // �ν����Ϳ��� ���� ����

    private AudioSource audioSource;

    void Start()
    {
        audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}
