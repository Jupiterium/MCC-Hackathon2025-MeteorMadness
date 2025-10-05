using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform earth;
    public float speed = 5f;
    Rigidbody rb;
    public bool isEnabled;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
        earth = GameObject.FindGameObjectWithTag("Earth").transform;

    }

    void FixedUpdate()
    {
        if (earth == null) return;


        if (isEnabled)
        {
            
            Vector3 dir = (earth.position - rb.position).normalized;

            // Move
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

            // Face the direction (optional)
            rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
        }
    }

    //void Awake()
    //{
    //    rb = GetComponent<Rigidbody>();
    //    rb.useGravity = false;
    //    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    //    rb.isKinematic = false;
    //}

    //void FixedUpdate()
    //{
    //    // Move forward every frame using Rigidbody
    //    Vector3 moveStep = Vector3.back * speed * Time.deltaTime;
    //    rb.MovePosition(rb.position + moveStep);
    //}
}
