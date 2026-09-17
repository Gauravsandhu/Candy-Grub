using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnGrub : MonoBehaviour
{
    
    [SerializeField] private GameObject grubPrefab;
    [SerializeField] private Transform muzzle;
    private bool grubAlreadyLaunched = false;
    public float firePower = 13f;
    

    // Update is called once per frame
    void Update()
    {
        if(!grubAlreadyLaunched){
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject grub = Instantiate(grubPrefab,muzzle.position,muzzle.rotation);
            Rigidbody2D rb = grub.GetComponent<Rigidbody2D>();
            rb.linearVelocity = muzzle.up * firePower;
            grubAlreadyLaunched = true;
        }
        
    }   
    }

}