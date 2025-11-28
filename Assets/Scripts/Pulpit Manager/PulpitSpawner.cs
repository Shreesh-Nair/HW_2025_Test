using UnityEngine;

public class PulpitSpawner : MonoBehaviour
{
    public GameObject pulpitPrefab;

    void Start()
    {
        GameObject p = Instantiate(pulpitPrefab, Vector3.zero, Quaternion.identity);
        p.transform.localScale = new Vector3(9f, 1f, 9f);
    }
}
