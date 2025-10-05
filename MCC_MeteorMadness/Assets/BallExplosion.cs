using UnityEngine;

public class BallExplosion : MonoBehaviour
{
    public GameObject explosionEffect;   // Drag 2D explosion prefab here
    public float explosionForce = 500f;
    public float explosionRadius = 5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Earth"))
        {
            // Get exact contact point on Earth's surface
            ContactPoint contact = collision.contacts[0];
            Vector3 impactPoint = contact.point;
            Vector3 impactNormal = contact.normal;

            // Spawn explosion prefab at impact point, oriented along the surface normal
            if (explosionEffect != null)
            {
                Quaternion rotation = Quaternion.LookRotation(impactNormal);
                GameObject explosion = Instantiate(explosionEffect, impactPoint, rotation);

                // Automatically destroy after particle system finishes
                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                    Destroy(explosion, totalDuration);
                }
                else
                {
                    Destroy(explosion, 2f);
                }
            }

            // Optional: apply explosion force to nearby rigidbodies
            Collider[] colliders = Physics.OverlapSphere(impactPoint, explosionRadius);
            foreach (Collider nearby in colliders)
            {
                Rigidbody rbNearby = nearby.GetComponent<Rigidbody>();
                if (rbNearby != null)
                {
                    rbNearby.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
                }
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}
