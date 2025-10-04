using UnityEngine;

public class BallExplosion : MonoBehaviour
{
    public GameObject explosionEffect;   // Drag 2D explosion prefab here
    public float explosionForce = 500f;
    public float explosionRadius = 5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TargetBall"))
        {
            // Spawn explosion facing the camera
            if (explosionEffect != null)
            {
                GameObject explosion = Instantiate(
                    explosionEffect,
                    transform.position,
                    Quaternion.LookRotation(Camera.main.transform.forward)
                );

                // Destroy explosion after particle duration
                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(explosion, 2f);
                }
            }

            // Optional: apply force to nearby rigidbodies
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider nearby in colliders)
            {
                Rigidbody rbNearby = nearby.GetComponent<Rigidbody>();
                if (rbNearby != null)
                    rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Destroy the moving ball
            Destroy(gameObject);
        }
    }
}
