using System.Collections;
using UnityEngine;

// Lives on the LevelManager object. Spawns particle bursts and shakes the camera.
// Effects run on unscaled time so they still play when the level ends and time stops.
public class LevelEffects : MonoBehaviour
{
    public static LevelEffects Instance { get; private set; }

    [SerializeField] private ParticleSystem sparklePrefab;
    [SerializeField] private ParticleSystem puffPrefab;
    [SerializeField] private float shakeDuration = 0.2f;

    private Transform cam;
    private Vector3 camRestPosition;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
            camRestPosition = cam.localPosition;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Sparkle(Vector3 position)
    {
        if (Instance != null) Instance.Spawn(Instance.sparklePrefab, position);
    }

    public static void Puff(Vector3 position)
    {
        if (Instance != null) Instance.Spawn(Instance.puffPrefab, position);
    }

    public static void Shake(float strength)
    {
        if (Instance == null || Instance.cam == null) return;

        if (Instance.shakeRoutine != null) Instance.StopCoroutine(Instance.shakeRoutine);
        Instance.shakeRoutine = Instance.StartCoroutine(Instance.ShakeRoutine(strength));
    }

    void Spawn(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;

        position.z = 0f;
        Instantiate(prefab, position, Quaternion.identity);
    }

    IEnumerator ShakeRoutine(float strength)
    {
        for (float t = 0f; t < shakeDuration; t += Time.unscaledDeltaTime)
        {
            float falloff = 1f - t / shakeDuration;
            cam.localPosition = camRestPosition + (Vector3)(Random.insideUnitCircle * strength * falloff);
            yield return null;
        }

        cam.localPosition = camRestPosition;
        shakeRoutine = null;
    }
}
