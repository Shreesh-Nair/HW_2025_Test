using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SimplePulpitSpawner : MonoBehaviour
{
    [System.Serializable] class PlayerData { public float speed; }
    [System.Serializable]
    class PulpitData
    {
        public float min_pulpit_destroy_time;
        public float max_pulpit_destroy_time;
        public float pulpit_spawn_time;
    }
    [System.Serializable] class Root { public PlayerData player_data; public PulpitData pulpit_data; }

    public GameObject pulpitPrefab;
    public int maxActive = 2;

    Root data;
    List<GameObject> active = new List<GameObject>();
    Vector3 lastPos = Vector3.zero;
    Vector3[] dirs = new Vector3[] { new Vector3(9f,0f,0f), new Vector3(-9f,0f,0f), new Vector3(0f,0f,9f), new Vector3(0f,0f,-9f) };

    void Start()
    {
        string path = Path.Combine(Application.dataPath, "Scripts/JSON Files/doofus_diary.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<Root>(json);
        }
        SpawnInitial();
    }

    void SpawnInitial()
    {
        GameObject p = Instantiate(pulpitPrefab, Vector3.zero, Quaternion.identity);
        p.transform.localScale = new Vector3(9f, 1f, 9f);
        active.Add(p);
        lastPos = p.transform.position;
        float life = GetRandomLife();
        StartCoroutine(DestroyAfter(p, life));
        StartCoroutine(SpawnWhenTimerReaches(p, life));
    }

    IEnumerator SpawnWhenTimerReaches(GameObject pulpit, float life)
    {
        float spawnDelay = 0f;
        if (data != null && data.pulpit_data != null)
            spawnDelay = Mathf.Max(0f, life - data.pulpit_data.pulpit_spawn_time);

        yield return new WaitForSeconds(spawnDelay);

        if (active.Count < maxActive)
            TrySpawnAdjacent();
    }

    void TrySpawnAdjacent()
    {
        Vector3 spawnPos = Vector3.zero;
        bool found = false;
        int attempts = 0;

        while (!found && attempts < 8)
        {
            attempts++;
            Vector3 offset = dirs[Random.Range(0, dirs.Length)];
            Vector3 candidate = lastPos + offset;

            bool occupied = false;
            foreach (var a in active)
            {
                if (Vector3.Distance(a.transform.position, candidate) < 0.1f) { occupied = true; break; }
            }

            if (!occupied)
            {
                spawnPos = candidate;
                found = true;
                lastPos = candidate;
            }
        }

        if (found)
        {
            GameObject p = Instantiate(pulpitPrefab, spawnPos, Quaternion.identity);
            p.transform.localScale = new Vector3(9f, 1f, 9f);
            active.Add(p);
            float life = GetRandomLife();
            StartCoroutine(DestroyAfter(p, life));
            StartCoroutine(SpawnWhenTimerReaches(p, life));
        }
    }

    IEnumerator DestroyAfter(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);
        if (active.Contains(g)) active.Remove(g);
        if (g != null) Destroy(g);
    }

    float GetRandomLife()
    {
        if (data != null && data.pulpit_data != null)
        {
            return Random.Range(data.pulpit_data.min_pulpit_destroy_time, data.pulpit_data.max_pulpit_destroy_time);
        }
        return 4f;
    }
}
