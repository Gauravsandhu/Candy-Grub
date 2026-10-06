using UnityEngine;

// Basket floor: the grub landing here ends the level.
public class LevelComplete : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Grub")) return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.ReachBasket();
        Destroy(collision.gameObject);
    }
}
