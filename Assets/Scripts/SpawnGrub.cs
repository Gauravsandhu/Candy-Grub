using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SpawnGrub : MonoBehaviour
{
    [SerializeField] private GameObject grubPrefab;
    [SerializeField] private Transform muzzle;
    public float firePower = 13f;

    [Header("Recoil")]
    [SerializeField] private Transform recoilTarget;
    [SerializeField] private Vector2 recoilScale = new Vector2(1.15f, 0.85f);
    [SerializeField] private float recoilDuration = 0.15f;

    private Controls controls;
    private Rigidbody2D grubBody;
    private Coroutine recoilRoutine;
    private Vector3 recoilRestScale;

    public Vector3 MuzzlePosition => muzzle.position;
    public Vector2 LaunchVelocity => muzzle.up * firePower;
    public float GrubGravityScale => grubBody != null ? grubBody.gravityScale : 1f;
    public float GrubLinearDamping => grubBody != null ? grubBody.linearDamping : 0f;

    void Awake()
    {
        controls = new Controls();
        grubBody = grubPrefab.GetComponent<Rigidbody2D>();
        if (recoilTarget == null) recoilTarget = muzzle.parent;
        if (recoilTarget != null) recoilRestScale = recoilTarget.localScale;
    }

    void OnEnable()
    {
        controls.Player1.Enable();
    }

    void OnDisable()
    {
        controls.Player1.Disable();
    }

    void OnDestroy()
    {
        controls.Dispose();
    }

    void Update()
    {
        LevelManager level = LevelManager.Instance;
        if (level == null || !level.CanLaunch) return;

        InputAction shoot = controls.Player1.Shoot;
        if (!shoot.WasPressedThisFrame()) return;

        // Ignore clicks and taps on UI such as the pause button.
        if (shoot.activeControl?.device is Pointer && IsPointerOverUI()) return;

        Launch(level);
    }

    void Launch(LevelManager level)
    {
        GameObject grub = Instantiate(grubPrefab, muzzle.position, muzzle.rotation);
        grub.GetComponent<Rigidbody2D>().linearVelocity = LaunchVelocity;
        level.NotifyLaunched();

        AudioManager.Play(Sfx.Fire);
        LevelEffects.Puff(muzzle.position);
        if (recoilTarget != null)
        {
            if (recoilRoutine != null) StopCoroutine(recoilRoutine);
            recoilRoutine = StartCoroutine(Recoil());
        }
    }

    // Squashes the barrel and springs it back.
    IEnumerator Recoil()
    {
        Vector3 squashed = Vector3.Scale(recoilRestScale, new Vector3(recoilScale.x, recoilScale.y, 1f));

        for (float t = 0f; t < 1f; t += Time.deltaTime / recoilDuration)
        {
            recoilTarget.localScale = Vector3.LerpUnclamped(squashed, recoilRestScale, 1f - Mathf.Pow(1f - t, 3f));
            yield return null;
        }

        recoilTarget.localScale = recoilRestScale;
        recoilRoutine = null;
    }

    static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
