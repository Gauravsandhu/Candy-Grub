using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Star : MonoBehaviour
{
    public GameObject[] destination = new GameObject[3];
    public static int i  = 0;
    
    void Start()
    {
        i = 0;
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
