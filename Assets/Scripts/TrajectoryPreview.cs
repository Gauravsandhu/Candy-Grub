using UnityEngine;

// Shows a short dotted arc from the muzzle while the player is aiming.
// The arc ignores collisions; it only previews the first part of the flight.
[RequireComponent(typeof(SpawnGrub))]
public class TrajectoryPreview : MonoBehaviour
{
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private int dotCount = 6;
    [SerializeField] private float secondsBetweenDots = 0.08f;
    [SerializeField] private float dotSize = 0.36f;
    [SerializeField] private Color dotColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private int sortingOrder = 50;

    private SpawnGrub spawner;
    private Transform dotRoot;
    private SpriteRenderer[] dots;

    void Awake()
    {
        spawner = GetComponent<SpawnGrub>();

        // Kept outside the cannon so its rotation and scale don't affect the dots.
        dotRoot = new GameObject("TrajectoryDots").transform;
        dots = new SpriteRenderer[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            SpriteRenderer dot = new GameObject("Dot " + i).AddComponent<SpriteRenderer>();
            dot.transform.SetParent(dotRoot, false);
            dot.sprite = dotSprite;
            dot.sortingOrder = sortingOrder;

            // Later dots are smaller and fainter.
            float fade = 1f - (float)i / dotCount;
            Color c = dotColor;
            c.a *= fade;
            dot.color = c;
            float spriteSize = dotSprite != null ? dotSprite.bounds.size.x : 1f;
            dot.transform.localScale = Vector3.one * (dotSize * Mathf.Lerp(0.6f, 1f, fade) / spriteSize);
            dots[i] = dot;
        }
    }

    void OnDestroy()
    {
        if (dotRoot != null) Destroy(dotRoot.gameObject);
    }

    void LateUpdate()
    {
        LevelManager level = LevelManager.Instance;
        bool show = level != null && level.CanLaunch;
        dotRoot.gameObject.SetActive(show);
        if (show) PlaceDots();
    }

    // Steps the flight the same way the physics engine does: gravity, then damping, then move.
    void PlaceDots()
    {
        float dt = Time.fixedDeltaTime;
        int stepsPerDot = Mathf.Max(1, Mathf.RoundToInt(secondsBetweenDots / dt));
        Vector2 gravity = Physics2D.gravity * spawner.GrubGravityScale;
        float damping = 1f / (1f + dt * spawner.GrubLinearDamping);

        Vector2 position = spawner.MuzzlePosition;
        Vector2 velocity = spawner.LaunchVelocity;

        for (int i = 0; i < dots.Length; i++)
        {
            for (int s = 0; s < stepsPerDot; s++)
            {
                velocity = (velocity + gravity * dt) * damping;
                position += velocity * dt;
            }
            dots[i].transform.position = position;
        }
    }
}
