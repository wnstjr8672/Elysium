using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI SFX")]
    public AudioClip uiClickClip;
    private AudioSource uiAudioSource;

    [Header("BGM")]
    public AudioClip bgmClip;
    private AudioSource bgmSource;

    [Header("SFX")]
    public AudioClip playerDeathClip;
    public AudioClip enemyDeathClip;
    public AudioClip knockbackClip;

    [Range(0f, 1f)] public float sfxVolume = 1.0f;
    public float SfxVolume => sfxVolume;

    [Header("SFX Volume Boost")]
    [Range(0.5f, 3f)] public float playerDeathVolumeBoost = 1.5f;
    [Range(0.5f, 3f)] public float enemyDeathVolumeBoost = 1.5f;
    [Range(0.5f, 3f)] public float knockbackVolumeBoost = 1.5f;

    [Header("UI")]
    public GameObject victoryUI;
    public GameObject defeatUI;

    [Header("Speed Control")]
    public TextMeshProUGUI speedText;
    public Image speedButtonImage;
    public Sprite speed1xIcon;
    public Sprite speed2xIcon;

    [Header("Settings Popup")]
    public GameObject settingsPopup;
    private bool isPaused = false;
    private float savedTimeScale = 1f;

    [Header("Volume Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Unit Loadout")]
    [Tooltip("      ִ  UnitSpawner")]
    public UnitSpawner spawner;

    [Tooltip("                               ȯ   ư 8   (  ->          )")]
    public Button[] unitButtons = new Button[8];

    [Tooltip("    ִ                    ⺻       (ȸ     ĭ   )")]
    public Sprite emptyUnitIcon;

    [Tooltip("  ȯ                       ")]
    public GameObject[] unitPrefabs;

    [Tooltip("   unitPrefabs              ,           itemId.   : \"Unit_Warrior\", \"Unit_Archer\", ...")]
    public string[] unitIdsSameOrder;

    [Tooltip("   unitPrefabs              ,   ư                     Ʈ.")]
    public Sprite[] unitIconsSameOrder;

    private float[] speedLevels = { 1f, 2f };
    private int currentSpeedIndex = 0;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        BaseUnit.playerDeathClip = playerDeathClip;
        BaseUnit.enemyDeathClip = enemyDeathClip;
        BaseUnit.knockbackClip = knockbackClip;
    }

    void Start()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.volume = 0.15f;
        bgmSource.playOnAwake = false;
        if (bgmClip != null)
            bgmSource.Play();

        sfxVolume = 0.2f;

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmSource.volume;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        SetGameSpeed(0);

       
        

        uiAudioSource = Camera.main.gameObject.GetComponent<AudioSource>();
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }

    }

    public void PlayUIClick()
    {
        if (uiClickClip != null && uiAudioSource != null)
            uiAudioSource.PlayOneShot(uiClickClip, sfxVolume); 
    }

    public void SpawnWithUpgrade(int prefabIndex, string unitId)
    {
        if (spawner == null)
        {
            
            return;
        }

        if (prefabIndex < 0 ||
            unitPrefabs == null ||
            prefabIndex >= unitPrefabs.Length ||
            unitPrefabs[prefabIndex] == null)
        {
            
            return;
        }

        Vector3 spawnPos = spawner.spawnPoint != null
            ? spawner.spawnPoint.position
            : Vector3.zero;

        GameObject go = Instantiate(unitPrefabs[prefabIndex], spawnPos, Quaternion.identity);

        UnitStatsApplier applier = go.GetComponent<UnitStatsApplier>();
        if (applier != null)
        {
            applier.loadUpgradeFromPlayerPrefs = true;
            applier.upgradePlayerPrefsKey = "UPGRADE_" + unitId;
            applier.ApplyNow();
        }
    }

    public void ToggleGameSpeed()
    {
        currentSpeedIndex = 1 - currentSpeedIndex;
        SetGameSpeed(currentSpeedIndex);
    }

    private void SetGameSpeed(int index)
    {
        Time.timeScale = speedLevels[index];

        if (speedText != null)
            speedText.text = $"{speedLevels[index]}X";

        if (speedButtonImage != null)
            speedButtonImage.sprite = (index == 0) ? speed1xIcon : speed2xIcon;
    }

    public void SetBgmVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = value;
    }

    public float GetSfxVolume() => sfxVolume;

    public void GameOver(bool playerBaseDestroyed)
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;

        if (playerBaseDestroyed)
        {
            
            if (defeatUI != null)
                defeatUI.SetActive(true);
        }
        else
        {
            
            if (victoryUI != null)
                victoryUI.SetActive(true);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadMainScene() => SceneManager.LoadScene("MainScene");

    public void RetryCurrentScene()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void ToggleSettings()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = savedTimeScale;
        }

        if (settingsPopup != null)
            settingsPopup.SetActive(isPaused);
    }
}
