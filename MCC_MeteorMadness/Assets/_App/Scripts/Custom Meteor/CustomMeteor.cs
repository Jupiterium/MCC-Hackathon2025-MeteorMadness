using UnityEngine;

public class CustomMeteor : MonoBehaviour
{
    [Header("Scene References")]
    public Transform earth; // Drag your Earth here
    public Transform meteor; // Drag your Meteor here (or leave empty to use this.transform)
    //public MeteorController controller; // move the meteor with a controller

    [Header("Scaling (world units)")]
    public float metersToUnits = 0.001f; // 1 meter -> 0.001 Unity units
    public float minVisualScale = 0.1f; // Prevents disappearing when diameter is tiny
    public float maxVisualScale = 100f; // Safety clamp

    [Header("Distance mapping")]
    public float kmToUnits = 0.00001f; // 1 km -> 0.00001 Unity units
    public float minDistanceUnits = 5f; // Keep the meteor visible
    public float maxDistanceUnits = 2000f; // Prevents shooting off into the void

    // Constants
    private const float AU_TO_KM = 149_597_870f;

    // Internal: stable direction from Earth along which we slide the meteor
    private Vector3 baselineDir;
    private bool hasBaseline;

    private void Awake()
    {
        // Default the meteor to "this" if not assigned
        if (meteor == null)
        {
            meteor = this.transform;
        }
    }

    private void Start()
    {
        // Compute a stable baseline direction once (Earth -> Meteor)
        EnsureBaselineDirection();
        //// Try to pick up a controller if not wired
        //if (controller == null && meteor != null)
        //{
        //    controller = meteor.GetComponent<MeteorController>();
        //}
        //// If your controller expects an Earth target, set it once
        //if (controller != null && controller.target == null && earth != null)
        //{
        //    controller.target = earth;
        //}
    }

    //void Update()
    //{
    //    SetDiameterMeters();

    //}


    // ----- Slider hooks ----
    // 1) SIZE: diameter (meters) -> local scale
    public void SetDiameterMeters(float diameterMeters)
    {

        if (meteor == null)
        {
            return;
        }

        // Convert meters to world units and clamp
        float s = diameterMeters * metersToUnits;
        if (s < minVisualScale)
        {
            s = minVisualScale;
        }
        if (s > maxVisualScale)
        {
            s = maxVisualScale;
        }

        meteor.localScale = new Vector3(s, s, s);
        Debug.Log("Updated the size of the Meteor!");
    }

    private bool _distanceInitialized = false;

    // 2) DISTANCE: AU -> km -> world units, placed along a fixed ray from Earth
    public void SetDistanceAu(float distanceAu)
    {
        if (meteor == null || earth == null)
        {
            return;
        }

        EnsureBaselineDirection();

        // Convert AU to km -> scene units
        float km = distanceAu * AU_TO_KM;
        float units = km * kmToUnits;
        units = Mathf.Clamp(units, minDistanceUnits, maxDistanceUnits);

        // --- Prevent the initial teleport ---
        if (_distanceInitialized == false)
        {
            _distanceInitialized = true;
            // Do NOT reposition on the first call.
            return;
        }

        // Reposition along the stable ray (now it’s user-driven)
        Vector3 newPos = earth.position + baselineDir * units;
        meteor.position = newPos;
        Debug.Log("Updated the position of the Meteor!");
    }


    // 3) SPEED: km/s -> world units per second
    public void SetSpeedKmPerSec(float kmPerSec)
    {
        //// Convert km/s to scene units/s using the same kmToUnits scale as distance.
        //if (controller != null)
        //{
        //    float unitsPerSec = kmPerSec * kmToUnits;
        //    controller.speed = unitsPerSec;
        //}
        //Debug.Log("Updated the velocity of the Meteor!");
    }
    

    // ----- Helpers -----
    private void EnsureBaselineDirection()
    {
        if (hasBaseline == true)
        {
            return;
        }

        if (meteor == null || earth == null)
        {
            return;
        }

        Vector3 dir = meteor.position - earth.position;
        if (dir.sqrMagnitude < 0.0001f)
        {
            // Fallback if meteor starts exactly at Earth's center
            dir = Vector3.forward;
        }

        baselineDir = dir.normalized;
        hasBaseline = true;
    }

    // Call this from a button if you ever manually move the meteor
    // and want distance changes to follow the new direction.
    public void RecomputeBaselineFromCurrent()
    {
        hasBaseline = false;
        EnsureBaselineDirection();
    }
}
