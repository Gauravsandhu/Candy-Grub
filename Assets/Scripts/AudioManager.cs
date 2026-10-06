using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Sfx { Fire, Bounce, Star, Win, Fail, Click, Portal }

// Persists across scenes. Plays looping music and one-shot sound effects,
// and stores the music and SFX volumes in PlayerPrefs.
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    const string MusicVolumeKey = "music_volume";
    const string SfxVolumeKey = "sfx_volume";

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    public AudioClip clip; // background music

    [Header("Sound effects")]
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private AudioClip starClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip portalClip;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null || sfxSource == musicSource)
            sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        // Fires for the scene being loaded now as well as every later one.
        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMusic(clip);
    }

    void OnDestroy()
    {
        if (instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    public static void Play(Sfx sfx)
    {
        if (instance != null) instance.PlaySFX(instance.ClipFor(sfx));
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null) return;

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip sfx)
    {
        if (sfx == null || sfxSource == null) return;
        sfxSource.PlayOneShot(sfx);
    }

    AudioClip ClipFor(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Fire: return fireClip;
            case Sfx.Bounce: return bounceClip;
            case Sfx.Star: return starClip;
            case Sfx.Win: return winClip;
            case Sfx.Fail: return failClip;
            case Sfx.Click: return clickClip;
            case Sfx.Portal: return portalClip;
            default: return null;
        }
    }

    // Gives every button in the new scene a click sound, so no button needs wiring by hand.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                button.onClick.AddListener(PlayClick);
    }

    void PlayClick() => PlaySFX(clickClip);
}
