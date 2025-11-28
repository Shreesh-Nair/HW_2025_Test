using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float speed = 3f;
    public float fallForce = 50f;

    Rigidbody rb;
    float inputX;
    float inputZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector3 horizontal = new Vector3(inputX, 0f, inputZ).normalized * speed;
        Vector3 newVel = new Vector3(horizontal.x, rb.velocity.y, horizontal.z);
        rb.velocity = newVel;

        if (transform.position.y < 1f)
        {
            rb.AddForce(Vector3.down * fallForce, ForceMode.Acceleration);
        }
    }
}
