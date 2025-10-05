using System;
using UnityEngine;

[ExecuteAlways]
public class EarthRotation : MonoBehaviour
{
    // Sidereal day = 23h 56m 4.0905s = 86164.0905 seconds
    private const double SiderealDaySeconds = 86164.0905;

    [Tooltip("Rotation speed multiplier (1 = real speed, >1 = faster spin)")]
    public float timeScale = 1000f; // Speed it up for testing

    [Tooltip("Axis to rotate around (default Y for upright Earth model)")]
    public Vector3 rotationAxis = new Vector3(0, 0.3987f, 0.917f); // 23.4° tilt example

    void Update()
    {
        // 360 degrees per sidereal day
        double degreesPerSecond = 360.0 / SiderealDaySeconds;
        float rotationThisFrame = (float)(degreesPerSecond * Time.deltaTime * timeScale);

        // Rotate Earth around its axis
        transform.Rotate(rotationAxis, rotationThisFrame, Space.Self);
    }
}
