using UnityEngine;
using UnityEngine.UI;

// Options panel used in the main menu and the pause menu: music and SFX volume, and progress reset.
public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Awake()
    {
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    void OnEnable()
    {
        AudioManager audio = AudioManager.instance;
        musicSlider.interactable = sfxSlider.interactable = audio != null;
        if (audio == null) return;

        musicSlider.SetValueWithoutNotify(audio.MusicVolume);
        sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }

    // Button targets
    public void Close() => gameObject.SetActive(false);

    public void ResetProgress()
    {
        SaveSystem.ResetProgress(SceneLoader.LevelCount);
    }

    void OnMusicChanged(float value)
    {
        if (AudioManager.instance != null) AudioManager.instance.MusicVolume = value;
    }

    void OnSfxChanged(float value)
    {
        if (AudioManager.instance != null) AudioManager.instance.SfxVolume = value;
    }
}
