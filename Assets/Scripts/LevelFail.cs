using UnityEngine;

// On the grub: reports it as lost when it stops moving or leaves the screen.
public class LevelFail : MonoBehaviour
{
    public float stationaryThreshold = 1f;
    public float velocityThreshold = 0.1f;
    // How far past the camera edges the grub may go before it counts as lost.
    public float outOfBoundsMargin = 2f;

    private Rigidbody2D rb;
    private Camera cam;
    private float stationaryTimer = 0f;
    private bool failed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        if (failed) return;

        if (IsOutOfBounds())
        {
            Fail();
            return;
        }

        if (rb.linearVelocity.magnitude < velocityThreshold)
        {
            stationaryTimer += Time.deltaTime;
            if (stationaryTimer >= stationaryThreshold)
                Fail();
        }
        else
        {
            stationaryTimer = 0f;
        }
    }

    // Leaving the top is allowed; gravity brings the grub back down.
    bool IsOutOfBounds()
    {
        if (cam == null || !cam.orthographic) return false;

        Vector3 camPos = cam.transform.position;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 pos = transform.position;

        return pos.y < camPos.y - halfHeight - outOfBoundsMargin
            || pos.x < camPos.x - halfWidth - outOfBoundsMargin
            || pos.x > camPos.x + halfWidth + outOfBoundsMargin;
    }

    public void Fail()
    {
        if (failed) return;

        failed = true;
        if (LevelManager.Instance != null)
            LevelManager.Instance.GrubLost();
        Destroy(gameObject);
    }
}
