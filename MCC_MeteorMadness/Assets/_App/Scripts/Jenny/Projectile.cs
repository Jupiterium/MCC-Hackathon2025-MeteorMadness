using UnityEngine;

public class Projectile : MonoBehaviour
{

    public float speed = 5f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        // Move forward every frame using Rigidbody
        Vector3 moveStep = Vector3.back * speed * Time.deltaTime;
        rb.MovePosition(rb.position + moveStep);
    }
}
