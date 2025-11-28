using UnityEngine;

// Attach this to the pulpit prefab. It increments the global score once when the player contacts it.
public class Pulpit : MonoBehaviour
{
    bool counted = false;

    void HandleContact(GameObject other)
    {
        if (counted) return;
        if (other == null) return;

        // Detect player by Movement component so tag is not required
        if (other.GetComponent<Movement>() != null)
        {
            counted = true;
            if (PulpitSpawner.Instance != null)
            {
                PulpitSpawner.Instance.AddScore();
            }
            else
            {
                Debug.LogWarning("Pulpit: PulpitSpawner.Instance is null. Score not incremented.");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleContact(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleContact(other.gameObject);
    }
}
