using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    public AudioClip clip; // background music

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

        PlayMusic(clip);
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
}
