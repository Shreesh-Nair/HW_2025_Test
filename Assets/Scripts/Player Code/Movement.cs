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
                    Debug.LogFormat("Movement: loaded speed {0} from JSON", speed);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Movement: failed to read speed from JSON: " + ex.Message);
            }
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
