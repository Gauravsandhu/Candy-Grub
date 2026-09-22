using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour



{
    private GameObject pauseMenu;
    private GameObject levelFailMenu;
    private GameObject levelCompleteMenu;


    public LevelFail levelFailScript;




    public void Awake()
    {
        pauseMenu = GameObject.Find("PauseMenu");
        levelFailMenu = GameObject.Find("LevelFailMenu");
        levelCompleteMenu = GameObject.Find("LevelCompleteMenu");
        pauseMenu.SetActive(false);
        levelFailMenu.SetActive(false);
        levelCompleteMenu.SetActive(false);
        
    }

    public void Update()
    {
        if (levelFailScript.failed)
        {
            ShowLevelFailMenu();
    
        }
       
    }
    public void PlayGame(int levelNumber)
    {   
        

        SceneManager.LoadSceneAsync(levelNumber);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartLevel()
{
    
    Debug.Log("Restart pressed, timeScale before: " + Time.timeScale);
    Time.timeScale = 1f;
    Debug.Log("timeScale after: " + Time.timeScale);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   
}

public void NextLevel()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);    
}


public void PauseGame()
{
    Time.timeScale = 0f;
    pauseMenu.SetActive(true);
}

public void ResumeGame()
{
    Time.timeScale = 1f;
    pauseMenu.SetActive(false);
}

public void ShowLevelFailMenu()
    {
        Time.timeScale = 0f;
        levelFailMenu.SetActive(true);
        levelFailScript.failed = false;
    }

public void ShowLevelCompleteMenu()
{
    Time.timeScale = 0f;
    levelCompleteMenu.SetActive(true); // add this field + Find/hide it in Awake like the others
}
    

}
