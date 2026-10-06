using UnityEngine;

// One end of a portal pair. A Grub entering is moved to the linked portal and leaves
// through its face (transform.up) at the same speed. Its angle relative to the
// portal is kept, so shots into a portal stay predictable.
[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [SerializeField] private Portal exit;
    [SerializeField] private float exitDistance = 0.7f;
    [SerializeField] private float cooldown = 0.25f;

    [Header("Visuals")]
    [SerializeField] private Transform swirl;
    [SerializeField] private float swirlSpeed = -240f;

    private float ignoreUntil;

    void Update()
    {
        if (swirl != null) swirl.Rotate(0f, 0f, swirlSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exit == null || Time.time < ignoreUntil || !other.CompareTag("Grub")) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        // Turn the velocity around relative to this portal, then express it relative to the exit.
        // Both portals work from either side: the Grub always leaves the exit's face.
        Vector2 local = transform.InverseTransformDirection(rb.linearVelocity);
        Vector2 exitLocal = new Vector2(-local.x, Mathf.Abs(local.y));
        Vector2 exitVelocity = exit.transform.TransformDirection(exitLocal);
        Vector2 exitPosition = exit.transform.position + exit.transform.up * exitDistance;

        rb.position = exitPosition;
        other.transform.position = exitPosition;
        rb.linearVelocity = exitVelocity;
        exit.ignoreUntil = Time.time + cooldown;

        AudioManager.Play(Sfx.Portal);
        LevelEffects.Sparkle(transform.position);
        LevelEffects.Sparkle(exitPosition);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * exitDistance);
        if (exit != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 1f, 0.6f);
            Gizmos.DrawLine(transform.position, exit.transform.position);
        }
    }
}
