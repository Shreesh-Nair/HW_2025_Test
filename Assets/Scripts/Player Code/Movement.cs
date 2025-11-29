using UnityEngine;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float speed = 3f;
    public float fallSpeed = -12f;

    Rigidbody rb;
    float inputX;
    float inputZ;
    bool isFalling = false;

    [System.Serializable]
    class PlayerData { public float speed; }
    [System.Serializable]
    class Root { public PlayerData player_data; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        bool loaded = false;
        string path = Path.Combine(Application.dataPath, "Scripts", "JSON Files", "doofus_diary.json");
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                Root r = JsonUtility.FromJson<Root>(json);
                if (r != null && r.player_data != null)
                {
                    speed = r.player_data.speed;
                    loaded = true;
                    Debug.LogFormat("Movement: loaded speed {0} from JSON at {1}", speed, path);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Movement: failed to read speed from JSON: " + ex.Message);
            }
        }

        // StreamingAssets fallback (included in builds)
        if (!loaded)
        {
            string sPath = Path.Combine(Application.streamingAssetsPath, "doofus_diary.json");
            if (File.Exists(sPath))
            {
                try
                {
                    string json = File.ReadAllText(sPath);
                    Root r = JsonUtility.FromJson<Root>(json);
                    if (r != null && r.player_data != null)
                    {
                        speed = r.player_data.speed;
                        loaded = true;
                        Debug.LogFormat("Movement: loaded speed {0} from StreamingAssets", speed);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Movement: failed to read speed from StreamingAssets: " + ex.Message);
                }
            }
        }

        // Resources fallback
        if (!loaded)
        {
            try
            {
                var ta = Resources.Load<TextAsset>("doofus_diary");
                if (ta != null)
                {
                    Root r = JsonUtility.FromJson<Root>(ta.text);
                    if (r != null && r.player_data != null)
                    {
                        speed = r.player_data.speed;
                        loaded = true;
                        Debug.LogFormat("Movement: loaded speed {0} from Resources/doofus_diary", speed);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Movement: failed to read speed from Resources: " + ex.Message);
            }
        }

        if (!loaded)
        {
            Debug.Log("Movement: doofus_diary.json not found; using default speed.");
        }
    }

    void Update()
    {
        if (!isFalling)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");
        }

        if (transform.position.y < 0.9f && !isFalling)
        {
            isFalling = true;
        }
    }

    void FixedUpdate()
    {
        if (isFalling)
        {
            rb.linearVelocity = new Vector3(0f, fallSpeed, 0f);
            return;
        }

        Vector3 move = new Vector3(inputX, 0f, inputZ).normalized * 2 * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
