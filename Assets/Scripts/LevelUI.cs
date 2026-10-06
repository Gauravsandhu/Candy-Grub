using UnityEngine;

// Lives on the level canvas. Shows panels in response to LevelManager events
// and is the target for every in-level button.
public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelCompleteMenu;
    [SerializeField] private GameObject levelFailMenu;

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
        if (level != null && controls.Player1.Pause.WasPressedThisFrame())
            level.SetPaused(!level.IsPaused);
    }

    // Button targets
    public void Pause() => level?.SetPaused(true);
    public void Resume() => level?.SetPaused(false);
    public void Restart() => SceneLoader.Restart();
    public void NextLevel() => SceneLoader.LoadNextLevel();
    public void GoToMainMenu() => SceneLoader.LoadMainMenu();

    void OnPauseChanged(bool paused) => ShowOnly(paused ? pauseMenu : null);
    void OnLevelWon(int stars) => ShowOnly(levelCompleteMenu);
    void OnLevelLost() => ShowOnly(levelFailMenu);

    // Activates one panel (or none); the pause button shows only during play.
    void ShowOnly(GameObject panel)
    {
        SetActive(pauseMenu, panel == pauseMenu);
        SetActive(levelCompleteMenu, panel == levelCompleteMenu);
        SetActive(levelFailMenu, panel == levelFailMenu);
        SetActive(pauseButton, panel == null);
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
