using UnityEngine;

public class BouncyPad : MonoBehaviour
{
    public float bounceForce = 15f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Grub")) return;

        Rigidbody2D rb = collision.rigidbody;
        if (rb != null)
            rb.linearVelocity = (Vector2)transform.right * bounceForce;

        AudioManager.Play(Sfx.Bounce);
        LevelEffects.Puff(collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)collision.transform.position);
        LevelEffects.Shake(0.1f);
    }
}
