using UnityEngine;

public class AsteroidNavCamera : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform earth;                       // Earth center

    [Header("Framing")]
    [Tooltip("How far behind the asteroid (in the Earth→asteroid direction) to place the camera.")]
    public float backDistance = 1f;               // units
    [Tooltip("Small lateral offset so Earth isn't perfectly blocked by the asteroid.")]
    public float lateralOffset = 0.6f;            // units
    [Tooltip("Extra distance proportional to asteroid size so big ones fit.")]
    public float sizePadding = 2.5f;              // multiplier on asteroid radius

    [Header("Smoothing")]
    public float moveLerp = 8f;                   // higher = snappier
    public float lookLerp = 12f;

    Transform _target;
    float _targetSizeUnits = 0.1f;

    public void Focus(Transform asteroid, float asteroidScaleUnits)
    {
        _target = asteroid;
        _targetSizeUnits = Mathf.Max(asteroidScaleUnits, 0.001f);
        // Jump-look on first call so it feels responsive
        transform.rotation = Quaternion.LookRotation((_target.position - transform.position).normalized, Vector3.up);
    }

    void LateUpdate()
    {
        if (_target == null || earth == null) return;

        Vector3 dirEA = (_target.position - earth.position).normalized; // Earth→Asteroid
        Vector3 right = Vector3.Cross(Vector3.up, dirEA).normalized;
        Vector3 upAxis = Vector3.Cross(dirEA, right).normalized;

        float radiusAst = _targetSizeUnits * 0.5f;
        float desiredBack = backDistance + radiusAst * sizePadding;

        // Camera position slightly “behind” the asteroid (so we see Earth in background),
        // and nudged sideways so the asteroid isn’t dead-center on Earth.
        Vector3 desiredPos = _target.position + (-dirEA * desiredBack) + (right * lateralOffset) + (upAxis * 0.2f);

        // Smooth move
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-moveLerp * Time.deltaTime));

        // Smooth look at asteroid
        Vector3 aim = (_target.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(aim, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-lookLerp * Time.deltaTime));
    }
}
