using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider musicSlider;
    public AudioSource musicAudioSource;

    void Start()
    {
        // Load saved volume or default to 1
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicSlider.value = savedVolume;
        musicAudioSource.volume = savedVolume;

        // Add listener
        musicSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        musicAudioSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}
