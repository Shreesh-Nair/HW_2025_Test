using UnityEngine;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float speed = 3f;
    public float fallSpeed = -12f;
    public float groundCheckDistance = 0.6f;
    public LayerMask groundLayers = ~0;

    Rigidbody rb;
    float inputX;
    float inputZ;
    bool isFalling = false;
    bool isGrounded = false;

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

        // Reduce sticking to walls by assigning a zero-friction PhysicMaterial at runtime
        // Always replace the player's material to ensure consistent friction behavior
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            var mat = new PhysicsMaterial("Player_NoFriction");
            mat.dynamicFriction = 0f;
            mat.staticFriction = 0f;
            mat.frictionCombine = PhysicsMaterialCombine.Minimum;
            mat.bounciness = 0f;
            mat.bounceCombine = PhysicsMaterialCombine.Minimum;
            col.material = mat;
        }

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
        // Ground check using a short downward raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            isGrounded = true;
        else
            isGrounded = false;

        // Determine falling state from vertical velocity and grounded state
        if (!isGrounded && rb.linearVelocity.y < -0.1f)
            isFalling = true;
        else if (isGrounded)
            isFalling = false;

        // Only read horizontal input when not falling
        if (!isFalling)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");
        }
    }

    void FixedUpdate()
    {
        if (isFalling)
        {
            // enforce free-fall: no horizontal motion while falling
            rb.linearVelocity = new Vector3(0f, fallSpeed, 0f);
            return;
        }

        Vector3 move = new Vector3(inputX, 0f, inputZ).normalized * 2 * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
