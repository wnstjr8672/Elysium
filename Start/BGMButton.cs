using UnityEngine;
using UnityEngine.UI;

public class BGMButton : MonoBehaviour
{
    public Sprite bgmOnSprite;
    public Sprite bgmOffSprite;

    private Image image;
    private bool isBgmOn = true;

    void Start()
    {
        image = GetComponent<Image>();
        UpdateButtonImage();
    }

    public void ToggleBGM()
    {
        isBgmOn = !isBgmOn;

        // 실제 BGM 켜고 끄는 로직도 같이 넣어야 함
        if (isBgmOn)
        {
            // AudioManager.Instance.PlayBGM(); 등
        }
        else
        {
            // AudioManager.Instance.StopBGM(); 등
        }

        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        image.sprite = isBgmOn ? bgmOnSprite : bgmOffSprite;
    }
}
