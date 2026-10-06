using UnityEngine;

// Basket floor: the grub landing here ends the level.
public class LevelComplete : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Grub")) return;

        LevelManager level = LevelManager.Instance;
        if (level != null)
        {
            level.ReachBasket();
            if (level.CurrentState == LevelManager.State.Won)
            {
                LevelEffects.Sparkle(collision.transform.position);
                LevelEffects.Shake(0.15f);
            }
        }
        Destroy(collision.gameObject);
    }
}
