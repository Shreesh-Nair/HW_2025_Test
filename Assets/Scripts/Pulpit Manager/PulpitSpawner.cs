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
    bool initialSpawned = false;

    Root data;
    List<GameObject> active = new List<GameObject>();
    Vector3 lastPos = Vector3.zero;
    static int pulpitGlobalId = 0; // diagnostic: unique id for each pulpit instance
    Vector3[] dirs = new Vector3[] { new Vector3(9f,0f,0f), new Vector3(-9f,0f,0f), new Vector3(0f,0f,9f), new Vector3(0f,0f,-9f) };

    // ===== singleton in Awake to avoid double Start/Spawn calls =====
    void Awake()
    {
        var all = FindObjectsOfType<PulpitSpawner>();
        Debug.LogFormat("PulpitSpawner Awake: found {0} spawner(s) in scene. This object: {1} (InstanceHash {2})", all.Length, gameObject.name, GetHashCode());

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("PulpitSpawner: Duplicate instance found, destroying duplicate on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Only the singleton should do initialization / spawning
        if (Instance != this) return;

        string path = Path.Combine(Application.dataPath, "Scripts", "JSON Files", "doofus_diary.json");
        bool loaded = false;
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<Root>(json);
                loaded = data != null;
                Debug.LogFormat("Loaded doofus_diary.json from Assets path: {0}", path);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Failed to read/parse doofus_diary.json from Assets path: " + ex.Message);
            }
        }

        // Fallback: check StreamingAssets (included in builds)
        if (!loaded)
        {
            string sPath = Path.Combine(Application.streamingAssetsPath, "doofus_diary.json");
            if (File.Exists(sPath))
            {
                try
                {
                    string json = File.ReadAllText(sPath);
                    data = JsonUtility.FromJson<Root>(json);
                    loaded = data != null;
                    Debug.LogFormat("Loaded doofus_diary.json from StreamingAssets: {0}", sPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Failed to read/parse doofus_diary.json from StreamingAssets: " + ex.Message);
                }
            }
        }

        // Final fallback: Resources (requires placing file as a TextAsset at Resources/doofus_diary.json)
        if (!loaded)
        {
            try
            {
                var ta = Resources.Load<TextAsset>("doofus_diary");
                if (ta != null)
                {
                    data = JsonUtility.FromJson<Root>(ta.text);
                    loaded = data != null;
                    Debug.Log("Loaded doofus_diary.json from Resources/doofus_diary");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Failed to read/parse doofus_diary.json from Resources: " + ex.Message);
            }
        }

        if (!loaded)
        {
            // Provide safe defaults so builds behave like the editor when JSON is missing
            data = new Root();
            data.player_data = new PlayerData();
            data.player_data.speed = 3f;
            data.pulpit_data = new PulpitData();
            data.pulpit_data.min_pulpit_destroy_time = 4f;
            data.pulpit_data.max_pulpit_destroy_time = 5f;
            data.pulpit_data.pulpit_spawn_time = 2.5f;
            Debug.LogWarning("doofus_diary.json not found; using built-in defaults (speed=3, min=4,max=5,spawn=2.5)");
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
        // defensive: prevent multiple initial spawns across instances/builds
        if (initialSpawned) return;
        // also ensure this instance hasn't already spawned
        if (active.Count > 0) return;

        // mark early to avoid race where two spawners both pass the checks and instantiate on same frame
        initialSpawned = true;

        GameObject p = Instantiate(pulpitPrefab, Vector3.zero, Quaternion.identity);
            // diagnostic: give each pulpit a unique name so build logs can be traced
            int id = ++pulpitGlobalId;
            p.name = string.Format("Pulpit_{0}", id);
            Debug.LogFormat("SpawnInitial: instantiated pulpit {0} by spawner {1} (initialSpawned={2})", p.name, GetHashCode(), initialSpawned);
        // defensive: if the pulpit prefab mistakenly contains a PulpitSpawner, remove it from the instance
        var stray = p.GetComponent<PulpitSpawner>();
        if (stray != null)
        {
            Debug.LogWarning("Removed stray PulpitSpawner component from instantiated pulpit prefab.");
            Destroy(stray);
        }
        p.transform.localScale = new Vector3(9f, 1f, 9f);
        active.Add(p);
        lastPos = p.transform.position;
        float life = GetRandomLife();

        // initialize pulpit countdown (new part) — does not change spawn/destroy logic
        var pcInit = p.GetComponent<PulpitCountdown_TMP>();
        if (pcInit != null) pcInit.Init(life);

        // play spawn FX if present
        var fxInit = p.GetComponent<PulpitFX>();
        if (fxInit != null) fxInit.PlaySpawnEffect();

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
                        int id = ++pulpitGlobalId;
                        p.name = string.Format("Pulpit_{0}", id);
                        Debug.LogFormat("TrySpawnAdjacent: instantiated pulpit {0} at {1} by spawner {2}", p.name, spawnPos, GetHashCode());
            // defensive: remove stray PulpitSpawner from the instantiated pulpit prefab if present
            var stray = p.GetComponent<PulpitSpawner>();
            if (stray != null)
            {
                Debug.LogWarning("Removed stray PulpitSpawner component from instantiated pulpit prefab.");
                Destroy(stray);
            }
            p.transform.localScale = new Vector3(9f, 1f, 9f);
            active.Add(p);
            lastPos = p.transform.position;
            float life = GetRandomLife();

            // initialize pulpit countdown (new part) — keep behavior identical otherwise
            var pcInit = p.GetComponent<PulpitCountdown_TMP>();
            if (pcInit != null) pcInit.Init(life);

            // play spawn FX if present
            var fxInit = p.GetComponent<PulpitFX>();
            if (fxInit != null) fxInit.PlaySpawnEffect();

            StartCoroutine(DestroyAfter(p, life));
            StartCoroutine(SpawnWhenTimerReaches(p, life));
            Debug.LogFormat("Spawned pulpit at {0} (life {1:0.00}). Active: {2}", spawnPos, life, active.Count);
        }
    }

    IEnumerator DestroyAfter(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);

        if (active.Contains(g)) active.Remove(g);

        if (g != null)
        {
            // play destroy FX if present and wait for it
            var fx = g.GetComponent<PulpitFX>();
            if (fx != null)
                yield return fx.PlayDestroyEffect();

            Destroy(g);
        }

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
