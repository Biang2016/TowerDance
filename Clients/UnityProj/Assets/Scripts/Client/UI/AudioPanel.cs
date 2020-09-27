using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : MonoBehaviour
{
    public Button AudioButton;
    public Sprite[] AudioButtonSprites;
    public Slider VolumeSlider;

    private float LastVolume = 1f;
    private bool Muted = false;

    void Awake()
    {
        VolumeSlider.onValueChanged.AddListener((value) =>
        {
            ClientGameManager.Instance.BGMAudioSource.volume = value;
            LastVolume = value;
            if (value.Equals(0))
            {
                Muted = true;
                AudioButton.image.sprite = AudioButtonSprites[0];
            }
            else
            {
                Muted = false;
                AudioButton.image.sprite = AudioButtonSprites[1];
            }
        });

        AudioButton.onClick.AddListener(() =>
        {
            if (Muted)
            {
                Muted = false;
                AudioButton.image.sprite = AudioButtonSprites[1];
                ClientGameManager.Instance.BGMAudioSource.volume = LastVolume;
                VolumeSlider.SetValueWithoutNotify(LastVolume);
            }
            else
            {
                Muted = true;
                AudioButton.image.sprite = AudioButtonSprites[0];
                ClientGameManager.Instance.BGMAudioSource.volume = 0;
                VolumeSlider.SetValueWithoutNotify(0);
            }
        });
    }
}