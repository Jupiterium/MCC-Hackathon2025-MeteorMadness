using UnityEngine;

public class SimulatingImpact : MonoBehaviour
{
    public bool isSimulating = false;

    public void StartSimulator()
    {
        // Guard the manager
        if (AsteroidDataManager.Instance == null)
        {
            Debug.LogError("[Sim] AsteroidDataManager.Instance is null.");
            return;
        }

        // Get the current asteroid safely
        int idx = AsteroidDataManager.Instance.currentIndex;
        var asteroid = AsteroidDataManager.getAsteroidIndex(idx);
        if (asteroid == null)
        {
            Debug.LogError($"[Sim] Current asteroid is null at index {idx}.");
            return;
        }

        // Ensure a Rigidbody exists before Projectile uses it
        var rb = asteroid.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = asteroid.AddComponent<Rigidbody>();
            Debug.Log("[Sim] Rigidbody added to asteroid.");
        }
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;

        // Ensure Projectile exists and is properly wired
        var proj = asteroid.GetComponent<Projectile>();
        if (proj == null)
        {
            proj = asteroid.AddComponent<Projectile>();
            Debug.Log("[Sim] Projectile added to asteroid.");
        }

        proj.earth = AsteroidDataManager.Instance.earth; // IMPORTANT: give it Earth
        proj.isEnabled = true;                            // use your flag, not component.enabled
        proj.speed = 1000f;

        isSimulating = true;

        Debug.Log($"[Sim] START -> index={idx} name={asteroid.name}");
    }
}
