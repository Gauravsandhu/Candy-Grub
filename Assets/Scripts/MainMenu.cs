using UnityEngine;

// Main Menu scene only. In-level UI is handled by LevelUI.
public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void PlayGame(int levelNumber)
    {
        SceneLoader.Load(levelNumber);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
