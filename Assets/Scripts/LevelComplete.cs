using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    public MainMenu mainMenu;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grub"))
        {
            if (Star.i >= 1)
            {
                mainMenu.ShowLevelCompleteMenu();
            }
            else
            {
                mainMenu.ShowLevelFailMenu();
            }
            Destroy(collision.gameObject);
        }
    }
}