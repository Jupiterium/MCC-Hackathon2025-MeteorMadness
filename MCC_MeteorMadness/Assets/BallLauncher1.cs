using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public float speed = 10f;          // Speed of the ball
    private Transform target;           // Target position

    void Start()
    {
        // Find the target ball by tag
        GameObject targetObj = GameObject.FindGameObjectWithTag("TargetBall");
        if (targetObj != null)
        {
            target = targetObj.transform;
        }
        else
        {
            Debug.LogError("No GameObject with tag 'TargetBall' found!");
        }
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Move toward the target
            Vector3 direction = (target.position - transform.position).normalized;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.linearVelocity = direction * speed; // Correct property
        }
    }
}
