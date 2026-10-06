using UnityEngine;

public class BouncyPad : MonoBehaviour
{
    public float bounceForce = 15f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grub"))
        {
            Rigidbody2D rb = collision.rigidbody;
            if (rb != null)
                rb.linearVelocity = (Vector2)transform.right * bounceForce;
        }
    }
}
