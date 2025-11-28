using UnityEngine;
using System.IO;
using System.Collections;

public class SimplePulpitSpawner : MonoBehaviour
{
    [System.Serializable]
    class PlayerData
    {
        public float speed;
    }

    [System.Serializable]
    class PulpitData
    {
        public float min_pulpit_destroy_time;
        public float max_pulpit_destroy_time;
        public float pulpit_spawn_time;
    }

    [System.Serializable]
    class Root
    {
        public PlayerData player_data;
        public PulpitData pulpit_data;
    }

    public GameObject pulpitPrefab;
    Root data;

    void Start()
    {
        string path = Path.Combine(Application.dataPath, "Scripts/JSON Files/doofus_diary.json");
        string json = File.ReadAllText(path);
        data = JsonUtility.FromJson<Root>(json);

        GameObject p = Instantiate(pulpitPrefab, Vector3.zero, Quaternion.identity);
        p.transform.localScale = new Vector3(9f, 1f, 9f);

        StartCoroutine(PrintRandomDelay());
    }

    IEnumerator PrintRandomDelay()
    {
        float t = Random.Range(
            data.pulpit_data.min_pulpit_destroy_time,
            data.pulpit_data.max_pulpit_destroy_time
        );

        yield return new WaitForSeconds(t);

        Debug.Log("Random Time: " + t);
    }
}
