using UnityEngine;

public class AsteroidNavCamera : MonoBehaviour
{
    public enum FollowMode { Off, OneShot, Continuous }

    [Header("Scene refs")]
    public Transform earth;

    [Header("Framing")]
    public float backDistance = 3f;
    public float lateralOffset = 0.6f;
    public float sizePadding = 2.5f;

    [Header("Smoothing")]
    public float moveLerp = 8f;
    public float lookLerp = 12f;

    [Header("Behavior")]
    public FollowMode mode = FollowMode.OneShot;   // <- set this in Inspector
    public float stopDistance = 0.01f;             // when OneShot is “close enough”

    Transform _target;
    float _targetSizeUnits = 0.1f;
    bool _pendingOneShot = false;

    public void Focus(Transform asteroid, float asteroidScaleUnits)
    {
        _target = asteroid;
        _targetSizeUnits = Mathf.Max(asteroidScaleUnits, 0.001f);

        if (mode == FollowMode.OneShot)
        {
            _pendingOneShot = true; // do a single recenter, then release
        }
        else if (mode == FollowMode.Off)
        {
            // jump-look once, no position recenter
            transform.rotation = Quaternion.LookRotation((_target.position - transform.position).normalized, Vector3.up);
        }
        // Continuous: handled in LateUpdate every frame
    }

    public void CancelFollow()
    {
        mode = FollowMode.Off;
        _pendingOneShot = false;
    }

    void LateUpdate()
    {
        if (_target == null || earth == null) return;

        // Compute desired framing
        Vector3 dirEA = (_target.position - earth.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, dirEA).normalized;
        Vector3 upAxis = Vector3.Cross(dirEA, right).normalized;

        float radiusAst = _targetSizeUnits * 0.5f;
        float desiredBack = backDistance + radiusAst * sizePadding;
        Vector3 desiredPos = _target.position + (-dirEA * desiredBack) + (right * lateralOffset) + (upAxis * 0.2f);
        Quaternion desiredRot = Quaternion.LookRotation((_target.position - desiredPos).normalized, Vector3.up);

        // Decide behavior
        if (mode == FollowMode.Continuous)
        {
            // keep following
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-moveLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-lookLerp * Time.deltaTime));
        }
        else if (mode == FollowMode.OneShot && _pendingOneShot)
        {
            // move once until close enough, then release
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-moveLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-lookLerp * Time.deltaTime));

            if ((transform.position - desiredPos).sqrMagnitude < (stopDistance * stopDistance))
            {
                _pendingOneShot = false; // release control → your SpectatorCamera owns movement
            }
        }
        // Off: do nothing (SpectatorCamera fully controls the cam)
    }
}
