using UnityEngine;
using UnityEngine.SceneManagement;

// Shared by the Main Menu scene and every level scene. Level-only panels are
// optional so the same component works in both.
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelFailMenu;
    [SerializeField] private GameObject levelCompleteMenu;

    public bool IsLevelOver { get; private set; }
    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (pauseMenu == null) pauseMenu = GameObject.Find("PauseMenu");
        if (levelFailMenu == null) levelFailMenu = GameObject.Find("LevelFailMenu");
        if (levelCompleteMenu == null) levelCompleteMenu = GameObject.Find("LevelCompleteMenu");

        SetPanelActive(pauseMenu, false);
        SetPanelActive(levelFailMenu, false);
        SetPanelActive(levelCompleteMenu, false);

        Time.timeScale = 1f;
    }

    public void PlayGame(int levelNumber)
    {
        if (levelNumber < 0 || levelNumber >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Scene index {levelNumber} is not in Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(levelNumber);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        // After the last level, return to the main menu.
        SceneManager.LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : 0);
    }

    public void PauseGame()
    {
        if (IsLevelOver) return;

        IsPaused = true;
        Time.timeScale = 0f;
        SetPanelActive(pauseMenu, true);
    }

    public void ResumeGame()
    {
        if (IsLevelOver) return;

        IsPaused = false;
        Time.timeScale = 1f;
        SetPanelActive(pauseMenu, false);
    }

    public void ShowLevelFailMenu()
    {
        if (!EndLevel()) return;
        SetPanelActive(levelFailMenu, true);
    }

    public void ShowLevelCompleteMenu()
    {
        if (!EndLevel()) return;
        SetPanelActive(levelCompleteMenu, true);
    }

    // Returns false if the level already ended, so win and fail can't both show.
    bool EndLevel()
    {
        if (IsLevelOver) return false;

        IsLevelOver = true;
        IsPaused = false;
        Time.timeScale = 0f;
        SetPanelActive(pauseMenu, false);
        return true;
    }

    static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
