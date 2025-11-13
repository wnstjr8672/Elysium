using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject startScreen;

    public TextMeshProUGUI versionText;
    public AudioSource bgmSource;

    [Header("설정")]
    public string SceneName = "GameScene";
    public AudioClip bgmClip;

    private bool bgmOn = true;

    private void Start()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        //  이 줄 삭제 (inGame은 아예 존재하지 않음)
        // inGame.SetActive(false);

        if (startScreen != null)
            startScreen.SetActive(true);

        // 버전 텍스트를 쓰고 싶으면 여기서 갱신
        // if (versionText != null) versionText.text = Application.version;
    }

    public void OnStartClicked()
    {
        if (bgmSource != null) bgmSource.Stop();
        SceneManager.LoadScene(SceneName);
    }

    public void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnToggleBgm()
    {
        if (bgmSource == null) return;

        bgmOn = !bgmOn;
        bgmSource.mute = !bgmOn;

        // TODO: 버튼 이미지 상태 변경 가능
    }
}
