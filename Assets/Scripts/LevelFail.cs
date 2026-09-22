using UnityEngine;

public class LevelFail : MonoBehaviour
{
    
    private Rigidbody2D rb;
    private float stationaryTimer = 0f;
    public float stationaryThreshold = 1f;
    public float velocityThreshold = 0.1f;
    public bool failed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        
    }

    void Update()
    {
        if (failed) return;

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

    public void Fail()
    {
        failed = true;
         Destroy(gameObject);
       
    }
}