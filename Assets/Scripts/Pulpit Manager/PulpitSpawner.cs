using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PulpitSpawner : MonoBehaviour
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
    public int score = 0; // Doofus's score: number of pulpits walked on

    public static PulpitSpawner Instance;

    Root data;
    List<GameObject> active = new List<GameObject>();
    Vector3 lastPos = Vector3.zero;
    Vector3[] dirs = new Vector3[] { new Vector3(9f,0f,0f), new Vector3(-9f,0f,0f), new Vector3(0f,0f,9f), new Vector3(0f,0f,-9f) };

    void Start()
    {
        Instance = this;

        string path = Path.Combine(Application.dataPath, "Scripts", "JSON Files", "doofus_diary.json");
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<Root>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to read/parse doofus_diary.json: " + ex.Message);
            }
        }
        SpawnInitial();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddScore()
    {
        score++;
        Debug.LogFormat("Score: {0}", score);
    }

    void SpawnInitial()
    {
        GameObject p = Instantiate(pulpitPrefab, Vector3.zero, Quaternion.identity);
        p.transform.localScale = new Vector3(9f, 1f, 9f);
        active.Add(p);
        lastPos = p.transform.position;
        float life = GetRandomLife();

        // initialize pulpit countdown (new part) — does not change spawn/destroy logic
        var pcInit = p.GetComponent<PulpitCountdown_TMP>();
        if (pcInit != null) pcInit.Init(life);

        StartCoroutine(DestroyAfter(p, life));
        StartCoroutine(SpawnWhenTimerReaches(p, life));
    }

    IEnumerator SpawnWhenTimerReaches(GameObject pulpit, float life)
    {
        float spawnDelay = 0f;

        if (data != null && data.pulpit_data != null)
        {
            //use the fixed pulpit_spawn_time from JSON spawn when remaining life == pulpit_spawn_time
            spawnDelay = Mathf.Max(0f, life - data.pulpit_data.pulpit_spawn_time);
        }

        yield return new WaitForSeconds(spawnDelay);

        //attempt to spawn adjacent to this pulpit if maxActive is reached, wait until a slot frees up but only while this pulpit still exists in the active list if it gets destroyed first, abort
        while (active.Count >= maxActive)
        {
            if (pulpit == null || !active.Contains(pulpit))
                yield break; //pulpit destroyed or no longer active dont spawn

            yield return null;//wait a frame and check again
        }

        //if we reach here theres room to spawn and the pulpit is still active
        TrySpawnAdjacent(pulpit);
    }

    void TrySpawnAdjacent(GameObject anchorPulpit = null)
    {
        if (active.Count >= maxActive) return;

        Vector3 anchor;
        if (anchorPulpit != null && active.Contains(anchorPulpit))
        {
            anchor = anchorPulpit.transform.position;
        }
        else if (active.Count > 0)
        {
            anchor = active[active.Count - 1].transform.position;
        }
        else
        {
            anchor = lastPos;
        }

        Vector3 spawnPos = Vector3.zero;
        bool found = false;
        int attempts = 0;

        while (!found && attempts < 8)
        {
            attempts++;
            Vector3 offset = dirs[Random.Range(0, dirs.Length)];
            Vector3 candidate = anchor + offset;

            bool occupied = false;
            foreach (var a in active)
            {
                if (Vector3.Distance(a.transform.position, candidate) < 0.1f) { occupied = true; break; }
            }

            if (!occupied)
            {
                spawnPos = candidate;
                found = true;
            }
        }

        if (found)
        {
            GameObject p = Instantiate(pulpitPrefab, spawnPos, Quaternion.identity);
            p.transform.localScale = new Vector3(9f, 1f, 9f);
            active.Add(p);
            lastPos = p.transform.position;
            float life = GetRandomLife();

            // initialize pulpit countdown (new part) — keep behavior identical otherwise
            var pcInit = p.GetComponent<PulpitCountdown_TMP>();
            if (pcInit != null) pcInit.Init(life);

            StartCoroutine(DestroyAfter(p, life));
            StartCoroutine(SpawnWhenTimerReaches(p, life));
            Debug.LogFormat("Spawned pulpit at {0} (life {1:0.00}). Active: {2}", spawnPos, life, active.Count);
        }
    }

    IEnumerator DestroyAfter(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);

        if (active.Contains(g)) active.Remove(g);
        if (g != null) Destroy(g);

        Debug.LogFormat("Destroyed pulpit. Active now: {0}", active.Count);

        if (active.Count > 0)
        {
            lastPos = active[active.Count - 1].transform.position;
        }

        //no spawn a replacement immediately on destruction
        //pulpits are spawned only when an existing pulpit's timer reaches `pulpit_spawn_time`
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
