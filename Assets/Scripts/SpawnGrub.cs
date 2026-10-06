using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SpawnGrub : MonoBehaviour
{
    [SerializeField] private GameObject grubPrefab;
    [SerializeField] private Transform muzzle;
    public float firePower = 13f;

    private Controls controls;

    void Awake()
    {
        controls = new Controls();
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
        grub.GetComponent<Rigidbody2D>().linearVelocity = muzzle.up * firePower;
        level.NotifyLaunched();
    }

    static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
