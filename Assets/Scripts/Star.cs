using UnityEngine;

public class Star : MonoBehaviour
{
    public GameObject[] destination = new GameObject[3];
    public static int i  = 0;

    private bool collected = false;

    void Start()
    {
        i = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Grub")) return;

        collected = true;
        // Stop the star from being picked up again at its placeholder.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        MoveStarToDestination(destination);
        i++;
    }

    void MoveStarToDestination(GameObject[] destination)
    {
        if (i >= destination.Length || destination[i] == null) return;

        Vector2 pos = destination[i].transform.position;
        gameObject.transform.position = pos;
    }
}
