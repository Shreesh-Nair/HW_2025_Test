using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float speed = 3f;
    public float fallSpeed = -12f;

    Rigidbody rb;
    float inputX;
    float inputZ;
    bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!isFalling)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");
        }

        if (transform.position.y < 1f && !isFalling)
        {
            isFalling = true;
        }
    }

    void FixedUpdate()
    {
        if (isFalling)
        {
            rb.velocity = new Vector3(0f, fallSpeed, 0f);
            return;
        }

        Vector3 move = new Vector3(inputX, 0f, inputZ).normalized * 2 *speed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }
}
