using UnityEngine;

public class BouncyPad : MonoBehaviour
{
    public float bounceForce = 15f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grub"))
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            rb.linearVelocity = (Vector2)transform.right * bounceForce;
        }
    }
}