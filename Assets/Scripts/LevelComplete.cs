using UnityEngine;

public class LevelComplete : MonoBehaviour
{


    // Update is called once per frame
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grub"))
        {
            Destroy(collision.gameObject);
            Time.timeScale = 0;
        }
    }
}
