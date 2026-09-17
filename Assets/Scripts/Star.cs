using System;
using UnityEngine;

public class Star : MonoBehaviour
{
    public GameObject[] destination = new GameObject[3];
    private static int i  = 0;
    

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Grub"))
        {
            MoveStarToDestination(destination);
            i++;
        }
    }


    void MoveStarToDestination(GameObject[] destination)
    {
        Vector2 pos = destination[i].transform.position;
        gameObject.transform.position = pos;
    }
}
