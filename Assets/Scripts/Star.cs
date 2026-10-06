using System.Collections;
using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] private float flyDuration = 0.4f;

    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Grub")) return;

        LevelManager level = LevelManager.Instance;
        if (level == null || level.IsLevelOver) return;

        collected = true;
        // Stop the star from being picked up again at its slot.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Transform slot = level.CollectStar();
        if (slot != null)
            StartCoroutine(FlyTo(slot.position));
    }

    IEnumerator FlyTo(Vector3 target)
    {
        Vector3 start = transform.position;
        Vector3 baseScale = transform.localScale;

        for (float t = 0f; t < 1f; t += Time.deltaTime / flyDuration)
        {
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.Lerp(start, target, eased);
            transform.localScale = baseScale * (1f + 0.4f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        transform.position = target;
        transform.localScale = baseScale;
    }
}
