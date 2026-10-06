using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lives on the level canvas. Shows panels in response to LevelManager events
// and is the target for every in-level button.
public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelCompleteMenu;
    [SerializeField] private GameObject levelFailMenu;
    [SerializeField] private GameObject optionsPanel;

    [Header("HUD")]
    [SerializeField] private TMP_Text levelTitle;

    [Header("Result stars")]
    [SerializeField] private Image[] resultStars;
    [SerializeField] private Color earnedStarColor = Color.white;
    [SerializeField] private Color missedStarColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private float starRevealDelay = 0.25f;

    private Controls controls;
    private LevelManager level;

    void Awake()
    {
        controls = new Controls();
        ShowOnly(null);
    }

    void OnEnable()
    {
        controls.Player1.Enable();
    }

    void OnDisable()
    {
        controls.Player1.Disable();
    }

    void Start()
    {
        if (levelTitle != null)
            levelTitle.text = "Level " + SceneManager.GetActiveScene().buildIndex;

        level = LevelManager.Instance;
        if (level == null)
        {
            Debug.LogError("LevelUI needs a LevelManager in the scene.", this);
            return;
        }

        level.PauseChanged += OnPauseChanged;
        level.LevelWon += OnLevelWon;
        level.LevelLost += OnLevelLost;
    }

    void OnDestroy()
    {
        if (level != null)
        {
            level.PauseChanged -= OnPauseChanged;
            level.LevelWon -= OnLevelWon;
            level.LevelLost -= OnLevelLost;
        }
        controls.Dispose();
    }

    void Update()
    {
        if (level == null || !controls.Player1.Pause.WasPressedThisFrame()) return;

        // Esc backs out of the options panel before it unpauses.
        if (optionsPanel != null && optionsPanel.activeSelf)
            CloseOptions();
        else
            level.SetPaused(!level.IsPaused);
    }

    // Button targets
    public void Pause() => level?.SetPaused(true);
    public void Resume() => level?.SetPaused(false);
    public void Restart() => SceneLoader.Restart();
    public void NextLevel() => SceneLoader.LoadNextLevel();
    public void GoToMainMenu() => SceneLoader.LoadMainMenu();
    public void OpenOptions() => SetActive(optionsPanel, true);
    public void CloseOptions() => SetActive(optionsPanel, false);

    void OnPauseChanged(bool paused) => ShowOnly(paused ? pauseMenu : null);

    void OnLevelWon(int stars)
    {
        ShowOnly(levelCompleteMenu);
        AudioManager.Play(Sfx.Win);
        StartCoroutine(RevealStars(stars));
    }

    void OnLevelLost()
    {
        ShowOnly(levelFailMenu);
        AudioManager.Play(Sfx.Fail);
    }

    // Pops the result stars in one at a time. Time is stopped, so this uses unscaled time.
    IEnumerator RevealStars(int earned)
    {
        if (resultStars == null) yield break;

        foreach (Image star in resultStars)
            star.transform.localScale = Vector3.zero;

        for (int i = 0; i < resultStars.Length; i++)
        {
            yield return new WaitForSecondsRealtime(starRevealDelay);

            bool isEarned = i < earned;
            resultStars[i].color = isEarned ? earnedStarColor : missedStarColor;
            if (isEarned) AudioManager.Play(Sfx.Star);

            Transform t = resultStars[i].transform;
            for (float p = 0f; p < 1f; p += Time.unscaledDeltaTime / 0.25f)
            {
                t.localScale = Vector3.one * EaseOutBack(p);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }

    // Goes from 0 to 1, overshooting slightly before settling.
    static float EaseOutBack(float p)
    {
        const float c1 = 1.70158f;
        float q = p - 1f;
        return 1f + (c1 + 1f) * q * q * q + c1 * q * q;
    }

    // Activates one panel (or none); the pause button shows only during play.
    void ShowOnly(GameObject panel)
    {
        SetActive(pauseMenu, panel == pauseMenu);
        SetActive(levelCompleteMenu, panel == levelCompleteMenu);
        SetActive(levelFailMenu, panel == levelFailMenu);
        SetActive(optionsPanel, false);
        SetActive(pauseButton, panel == null);
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
