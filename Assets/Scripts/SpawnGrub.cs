using UnityEngine;

public class SpawnGrub : MonoBehaviour
{
    [SerializeField] private GameObject grubPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private MainMenu mainMenu; // drag MainCamera into this slot in Inspector
    public float firePower = 13f;

    private bool grubAlreadyLaunched = false;

    void Start()
    {
        Star.i = 0;
    }

    void Update()
    {
        if (grubAlreadyLaunched || !CanFire()) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject grub = Instantiate(grubPrefab, muzzle.position, muzzle.rotation);
            Rigidbody2D rb = grub.GetComponent<Rigidbody2D>();
            rb.linearVelocity = muzzle.up * firePower;
            grubAlreadyLaunched = true;

            LevelFail levelFail = grub.GetComponent<LevelFail>();
            if (levelFail != null && mainMenu != null)
                levelFail.Failed += mainMenu.ShowLevelFailMenu;
        }
    }

    bool CanFire()
    {
        if (Time.timeScale == 0f) return false;
        return mainMenu == null || (!mainMenu.IsPaused && !mainMenu.IsLevelOver);
    }
}
