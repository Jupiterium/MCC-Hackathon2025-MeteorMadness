using UnityEngine;

/// <summary>
/// Stick this on the projectile prefab (the object that COLLIDES with Earth).
/// Your spawner / UI should set these before launch.
/// </summary>
public class MeteorImpactParams : MonoBehaviour
{
    [Tooltip("Diameter in meters for heatmap/impact math.")]
    public float diameterMeters = 50f;

    [Tooltip("Speed in km/s (optional for heatmap; can still be derived from Rigidbody).")]
    public float speedKmPerSec = 20f;
}
