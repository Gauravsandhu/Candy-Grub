using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const int MainMenuIndex = 0;

    public static int LevelCount => SceneManager.sceneCountInBuildSettings - 1;

    public static void Load(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Scene index {buildIndex} is not in Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    public static void Restart()
    {
        Load(SceneManager.GetActiveScene().buildIndex);
    }

    // After the last level, return to the main menu.
    public static void LoadNextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        Load(next < SceneManager.sceneCountInBuildSettings ? next : MainMenuIndex);
    }

    public static void LoadMainMenu()
    {
        Load(MainMenuIndex);
    }
}
