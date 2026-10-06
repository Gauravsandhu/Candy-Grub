using UnityEngine;
using UnityEngine.UI;

// Options panel used in the main menu and the pause menu: music and SFX volume, and progress reset.
public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // The menu hidden while options are open; shown again on Close.
    private GameObject returnTo;

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

    public bool IsOpen => gameObject.activeSelf;

    public void Open(GameObject from)
    {
        returnTo = from;
        if (from != null) from.SetActive(false);
        gameObject.SetActive(true);
    }

    // Button target
    public void Close()
    {
        gameObject.SetActive(false);
        if (returnTo != null) returnTo.SetActive(true);
        returnTo = null;
    }

    // Hides the panel without bringing back the menu it was opened from.
    public void Hide()
    {
        returnTo = null;
        gameObject.SetActive(false);
    }

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
