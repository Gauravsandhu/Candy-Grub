using UnityEngine;

// Main Menu scene only. In-level UI is handled by LevelUI.
public class MainMenu : MonoBehaviour
{
    [SerializeField] private OptionsMenu options;
    [SerializeField] private GameObject menuButtons;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void PlayGame(int levelNumber)
    {
        SceneLoader.Load(levelNumber);
    }

    public void OpenOptions()
    {
        if (options != null) options.Open(menuButtons);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
