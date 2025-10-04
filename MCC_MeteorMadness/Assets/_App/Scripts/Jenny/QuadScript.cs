//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class QuadScript : MonoBehaviour
//{
//    Material mMaterial;
//    MeshRenderer mMeshRenderer;

//    float[] mPoints;
//    int mHitCount;

//    float mDelay;


//    void Start()
//    {
//        mDelay = 3;

//        mMeshRenderer = GetComponent<MeshRenderer>();
//        mMaterial = mMeshRenderer.material;

//        mPoints = new float[32 * 3]; //32 point 

//    }

//    void Update()
//    {
//        mDelay -= Time.deltaTime;
//        if (mDelay <= 0)
//        {
//            GameObject go = Instantiate(Resources.Load<GameObject>("Projectile"));
//            go.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-3f, -1f));

//            mDelay = 0.5f;
//        }

//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        foreach (ContactPoint cp in collision.contacts)
//        {
//            Debug.Log("Contact with object " + cp.otherCollider.gameObject.name);

//            // Get the local position of the hit point relative to the Earth sphere
//            Vector3 localPoint = transform.InverseTransformPoint(cp.point).normalized;

//            // Calculate latitude and longitude
//            float longitude = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg; // -180 to 180
//            float latitude = Mathf.Asin(localPoint.y) * Mathf.Rad2Deg; // -90 to 90

//            Debug.Log($"Impact Lat: {latitude}, Lon: {longitude}");

//            // If you want to remap lat/long to UV space (0–1 range):
//            float u = (longitude + 180f) / 360f;
//            float v = (latitude + 90f) / 180f;
//            Debug.Log($"Impact UV (simulated): {u}, {v}");

//            // Destroy the projectile after hit
//            Destroy(cp.otherCollider.gameObject);
//        }
//    }

//    public void addHitPoint(float xp, float yp)
//    {
//        mPoints[mHitCount * 3] = xp;
//        mPoints[mHitCount * 3 + 1] = yp;
//        mPoints[mHitCount * 3 + 2] = Random.Range(1f, 3f);

//        mHitCount++;
//        mHitCount %= 32;

//        mMaterial.SetFloatArray("_Hits", mPoints);
//        mMaterial.SetInt("_HitCount", mHitCount);

//    }

//}

//using UnityEngine;

//public class QuadScript : MonoBehaviour
//{
//    [Header("Heatmap")]
//    [Range(1, 64)] public int maxHits = 32;

//    // UV-space radius range for blobs (visual size on the sphere)
//    [Range(0.01f, 0.5f)] public float rMin = 0.05f;
//    [Range(0.01f, 0.5f)] public float rMax = 0.20f;
//    [Range(0.5f, 3f)] public float radiusCurve = 1.3f; // >1 dampens sensitivity near low end

//    // Brightness/intensity shaping
//    [Range(0f, 1f)] public float intensityFloor = 0.25f; // keep small hits visible
//    [Range(0.5f, 3f)] public float intensityCurve = 1.3f;  // growth shaping

//    [Header("Normalization ranges (expected slider ranges)")]
//    public Vector2 diameterRangeM = new Vector2(10f, 2000f);  // min..max expected diameter
//    public Vector2 speedRangeKmS = new Vector2(5f, 70f);     // min..max expected speed

//    [Header("Raycast")]
//    public LayerMask earthMask;                        // only the Earth MeshCollider layer
//    [SerializeField] private MeshCollider earthMeshCollider; // drag Earth's MeshCollider here

//    // Optional fallbacks if ImpactData is missing (leave 0 to ignore)
//    [Header("Optional scene scale (fallbacks only)")]
//    public float metersToUnits = 0f;  // 1 m -> N units (set if you want diameter fallback)
//    public float kmToUnits = 0f;  // 1 km -> N units (set if you want speed fallback)

//    MeshRenderer _mr;
//    Material _mat;
//    float[] _hits;   // quadruplets: (u, v, r, s)
//    int _hitCount;

//    void Start()
//    {
//        if (earthMeshCollider == null) { earthMeshCollider = GetComponent<MeshCollider>(); }

//        _mr = GetComponent<MeshRenderer>();
//        _mat = _mr.material;        // unique instance
//        _hits = new float[maxHits * 4];

//        _mat.SetFloatArray("_Hits", _hits);
//        _mat.SetInt("_HitCount", 0);

//        // Optional sanity blob to prove the shader is alive (center UV)
//        // int idx = 0; _hits[idx+0]=0.5f; _hits[idx+1]=0.5f; _hits[idx+2]=0.12f; _hits[idx+3]=0.6f;
//        // _mat.SetFloatArray("_Hits", _hits); _mat.SetInt("_HitCount", 1);
//    }

//    void OnCollisionEnter(Collision collision)
//    {
//        foreach (var cp in collision.contacts)
//        {
//            // Ray UV from the actual mesh triangle we hit
//            Vector3 start = cp.point + cp.normal * 0.02f;
//            Vector3 dir = -cp.normal;

//            RaycastHit hit;
//            bool ok = false;

//            if (earthMeshCollider != null)
//                ok = earthMeshCollider.Raycast(new Ray(start, dir), out hit, 0.5f);
//            else
//                ok = Physics.Raycast(start, dir, out hit, 0.5f, earthMask, QueryTriggerInteraction.Ignore);

//            if (!ok)
//            {
//                // Try the opposite direction once (grazing contacts)
//                ok = (earthMeshCollider != null)
//                     ? earthMeshCollider.Raycast(new Ray(cp.point - cp.normal * 0.02f, cp.normal), out hit, 0.5f)
//                     : Physics.Raycast(cp.point - cp.normal * 0.02f, cp.normal, out hit, 0.5f, earthMask, QueryTriggerInteraction.Ignore);
//            }

//            if (!ok)
//            {
//                Debug.LogWarning("UV raycast missed Earth MeshCollider. Check collider ref/layer/offset.");
//                continue;
//            }

//            // --- (u, v) from mesh UVs ---
//            float u = hit.textureCoord.x;
//            float v = hit.textureCoord.y;

//            // --- Pull impact parameters from the meteor ---
//            float diameterM, speedKmS;
//            var rb = cp.otherCollider.attachedRigidbody;
//            var data = rb ? rb.GetComponentInParent<ImpactData>() : null;

//            if (data != null)
//            {
//                diameterM = data.diameterMeters;
//                speedKmS = data.speedKmPerSec;
//            }
//            else
//            {
//                // Fallbacks if ImpactData is missing
//                // Diameter: estimate from meteor's bounds if metersToUnits is set (>0)
//                var otherTf = rb ? rb.transform : cp.otherCollider.transform;
//                float approxUnits = otherTf ? otherTf.lossyScale.x : 1f; // rough
//                diameterM = (metersToUnits > 0f) ? approxUnits / metersToUnits
//                                                 : Mathf.Lerp(diameterRangeM.x, diameterRangeM.y, 0.5f);

//                // Speed: use relative velocity if kmToUnits is set (>0)
//                float unitsPerSec = collision.relativeVelocity.magnitude;
//                speedKmS = (kmToUnits > 0f) ? unitsPerSec / kmToUnits
//                                            : Mathf.Lerp(speedRangeKmS.x, speedRangeKmS.y, 0.5f);
//            }

//            // --- Normalize size & speed to 0..1 ---
//            float dN = Mathf.InverseLerp(diameterRangeM.x, diameterRangeM.y, Mathf.Max(0f, diameterM));
//            float vN = Mathf.InverseLerp(speedRangeKmS.x, speedRangeKmS.y, Mathf.Max(0f, speedKmS));

//            // --- Radius mapping (larger with size & speed) ---
//            // proxy ~ d^1 * v^2 → then curved
//            float tRad = Mathf.Clamp01(Mathf.Pow(dN, 1.0f) * Mathf.Pow(vN, 2.0f));
//            tRad = Mathf.Pow(tRad, radiusCurve);
//            float r = Mathf.Lerp(rMin, rMax, tRad);

//            // --- Intensity mapping (brightness) ---
//            // gentler growth: ~ d^1 * v^1.5 with floor
//            float tInt = Mathf.Clamp01(Mathf.Pow(dN, 1.0f) * Mathf.Pow(vN, 1.5f));
//            tInt = Mathf.Pow(tInt, intensityCurve);
//            float s = Mathf.Lerp(intensityFloor, 1.0f, tInt);

//            // --- Write (u, v, r, s) ---
//            int idx = (_hitCount % maxHits) * 4;
//            _hits[idx + 0] = u;
//            _hits[idx + 1] = v;
//            _hits[idx + 2] = r;
//            _hits[idx + 3] = s;
//            _hitCount++;

//            _mat.SetFloatArray("_Hits", _hits);
//            _mat.SetInt("_HitCount", Mathf.Min(_hitCount, maxHits));

//            // Kill the projectile
//            Destroy(rb ? rb.gameObject : cp.otherCollider.gameObject);

//            // Debug
//            Debug.DrawLine(start, hit.point, Color.magenta, 2f);
//            Debug.Log($"Impact uv=({u:F2},{v:F2}) diameter={diameterM:F0}m speed={speedKmS:F1}km/s  r={r:F3} s={s:F2}");
//        }
//    }
//}


using UnityEngine;

public class QuadScript : MonoBehaviour
{
    [Header("Heatmap buffer")]
    [Range(1, 64)] public int maxHits = 32;

    [Header("Radius mapping (UV units)")]
    [Range(0.01f, 0.5f)] public float rMin = 0.05f;
    [Range(0.01f, 0.5f)] public float rMax = 0.20f;
    [Range(0.5f, 3f)] public float radiusCurve = 1.3f;

    [Header("Intensity mapping (brightness)")]
    [Range(0f, 1f)] public float intensityFloor = 0.25f;
    [Range(0.5f, 3f)] public float intensityCurve = 1.3f;

    [Header("Expected ranges (normalize without ImpactData)")]
    public Vector2 diameterRangeM = new Vector2(10f, 2000f);
    public Vector2 speedRangeKmS = new Vector2(5f, 70f);

    [Header("Optional scene scale (only if you want real units)")]
    public float metersToUnits = 0f;
    public float kmToUnits = 0f;

    [Header("Proxies when you DON'T use real units")]
    public Vector2 scaleUnitsRange = new Vector2(0.1f, 1.5f);
    public Vector2 relVelUnitsRange = new Vector2(1f, 50f);
    public bool preferProjectileSpeed = true;

    [Header("If the collider is a separate prefab, read diameter from this")]
    public Transform diameterSource;

    [Header("Raycast")]
    public LayerMask earthMask;
    [SerializeField] private MeshCollider earthMeshCollider;

    MeshRenderer _mr;
    Material _mat;
    float[] _hits; // (u, v, r, s)
    int _hitCount;

    void Start()
    {
        if (earthMeshCollider == null) earthMeshCollider = GetComponent<MeshCollider>();

        _mr = GetComponent<MeshRenderer>();
        _mat = _mr.material; // unique instance
        _hits = new float[maxHits * 4];

        _mat.SetFloatArray("_Hits", _hits);
        _mat.SetInt("_HitCount", 0);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (var cp in collision.contacts)
        {
            // UV from the actual mesh triangle we hit
            Vector3 start = cp.point + cp.normal * 0.02f;
            Vector3 dir = -cp.normal;

            RaycastHit hit;
            bool ok = (earthMeshCollider != null)
                      ? earthMeshCollider.Raycast(new Ray(start, dir), out hit, 0.5f)
                      : Physics.Raycast(start, dir, out hit, 0.5f, earthMask, QueryTriggerInteraction.Ignore);

            if (!ok)
            {
                ok = (earthMeshCollider != null)
                     ? earthMeshCollider.Raycast(new Ray(cp.point - cp.normal * 0.02f, cp.normal), out hit, 0.5f)
                     : Physics.Raycast(cp.point - cp.normal * 0.02f, cp.normal, out hit, 0.5f, earthMask, QueryTriggerInteraction.Ignore);
            }
            if (!ok) continue;

            float u = hit.textureCoord.x;
            float v = hit.textureCoord.y;

            // Pull size & speed from the colliding meteor (no ImpactData)
            var rb = cp.otherCollider.attachedRigidbody;
            Transform meteorTf = (diameterSource != null) ? diameterSource
                                : (rb ? rb.transform : cp.otherCollider.transform);

            // Diameter [m]
            float diameterM;
            if (metersToUnits > 0f)
            {
                float approxUnits = meteorTf.lossyScale.x; // assume uniform scale
                diameterM = Mathf.Max(0.01f, approxUnits / metersToUnits);
            }
            else
            {
                float sx = meteorTf.lossyScale.x;
                float t = Mathf.InverseLerp(scaleUnitsRange.x, scaleUnitsRange.y, sx);
                diameterM = Mathf.Lerp(diameterRangeM.x, diameterRangeM.y, Mathf.Clamp01(t));
            }

            // Speed [km/s]
            float speedKmS;
            if (kmToUnits > 0f)
            {
                float unitsPerSec = collision.relativeVelocity.magnitude;
                speedKmS = Mathf.Max(0f, unitsPerSec / kmToUnits);
            }
            else if (preferProjectileSpeed && rb)
            {
                var proj = rb.GetComponentInParent<Projectile>();
                if (proj != null)
                {
                    float t = Mathf.InverseLerp(relVelUnitsRange.x, relVelUnitsRange.y, proj.speed);
                    speedKmS = Mathf.Lerp(speedRangeKmS.x, speedRangeKmS.y, Mathf.Clamp01(t));
                }
                else
                {
                    float unitsPerSec = collision.relativeVelocity.magnitude;
                    float t = Mathf.InverseLerp(relVelUnitsRange.x, relVelUnitsRange.y, unitsPerSec);
                    speedKmS = Mathf.Lerp(speedRangeKmS.x, speedRangeKmS.y, Mathf.Clamp01(t));
                }
            }
            else
            {
                float unitsPerSec = collision.relativeVelocity.magnitude;
                float t = Mathf.InverseLerp(relVelUnitsRange.x, relVelUnitsRange.y, unitsPerSec);
                speedKmS = Mathf.Lerp(speedRangeKmS.x, speedRangeKmS.y, Mathf.Clamp01(t));
            }

            // Normalize 0..1
            float dN = Mathf.InverseLerp(diameterRangeM.x, diameterRangeM.y, diameterM);
            float vN = Mathf.InverseLerp(speedRangeKmS.x, speedRangeKmS.y, speedKmS);

            //// Radius (UV) grows fast with speed, modest with size
            //float tRad = Mathf.Clamp01(Mathf.Pow(dN, 1.0f) * Mathf.Pow(vN, 2.0f));
            //tRad = Mathf.Pow(tRad, radiusCurve);
            //float r = Mathf.Lerp(rMin, rMax, tRad);

            // Size-led blend so diameter always pushes radius up
            float mix = Mathf.Clamp01(0.75f * dN + 0.25f * vN); // size 75%, speed 25%
            float tRad = Mathf.Pow(mix, radiusCurve);
            float r = Mathf.Lerp(rMin, rMax, tRad);

            Debug.Log($"dN={dN:F2} vN={vN:F2} rUV={r:F3} scaleX={meteorTf.lossyScale.x:F1}");

            // Intensity grows with both but gentler; always ≥ floor
            float tInt = Mathf.Clamp01(Mathf.Pow(dN, 1.0f) * Mathf.Pow(vN, 1.5f));
            tInt = Mathf.Pow(tInt, intensityCurve);
            float s = Mathf.Lerp(intensityFloor, 1.0f, tInt);

            // Write (u, v, r, s)
            int idx = (_hitCount % maxHits) * 4;
            _hits[idx + 0] = u;
            _hits[idx + 1] = v;
            _hits[idx + 2] = r;
            _hits[idx + 3] = s;
            _hitCount++;

            _mat.SetFloatArray("_Hits", _hits);
            _mat.SetInt("_HitCount", Mathf.Min(_hitCount, maxHits));

            // optional: destroy projectile
            Destroy(rb ? rb.gameObject : cp.otherCollider.gameObject);
        }
    }
}













//void OnCollisionEnter(Collision collision)
//{
//    foreach (var cp in collision.contacts)
//    {
//        // 1) Convert world-space hit point to Earth-local, then to spherical UV
//        Vector3 local = transform.InverseTransformPoint(cp.point).normalized;

//        // Longitude [-PI,PI], Latitude [-PI/2, PI/2]
//        float lon = Mathf.Atan2(local.x, local.z); // radians
//        float lat = Mathf.Asin(local.y);           // radians

//        // Map to UV [0,1]
//        float u = (lon + Mathf.PI) / (2f * Mathf.PI);
//        float v = (lat + Mathf.PI * 0.5f) / Mathf.PI;

//        // 2) Pick a radius in UV space (tweak this)
//        float r = Random.Range(minMaxRadius.x, minMaxRadius.y);

//        // 3) Write to ring buffer (u,v,r)
//        int idx = (_hitCount % maxHits) * 3;
//        _hits[idx + 0] = u;
//        _hits[idx + 1] = v;
//        _hits[idx + 2] = r;
//        _hitCount++;

//        // 4) Push to shader
//        _mat.SetFloatArray("_Hits", _hits);
//        _mat.SetInt("_HitCount", Mathf.Min(_hitCount, maxHits));

//        // Optional: remove projectile
//        Destroy(cp.otherCollider.attachedRigidbody ? cp.otherCollider.attachedRigidbody.gameObject
//                                                   : cp.otherCollider.gameObject);
//    }
//}




//// Raycast a few cm along the contact normal into the Earth mesh
//Vector3 start = cp.point + cp.normal * 0.02f;   // just outside
//Vector3 dir = -cp.normal;                     // into the surface

//RaycastHit hit;
//// Use a LayerMask that ONLY includes your Earth mesh collider
//if (Physics.Raycast(start, dir, out hit, 0.1f, earthMask, QueryTriggerInteraction.Ignore))
//{
//    // exact UV from the mesh
//    float u = hit.textureCoord.x;
//    float v = hit.textureCoord.y;

//    // choose a UV radius (tune this)
//    float r = Random.Range(minMaxRadius.x, minMaxRadius.y);

//    int idx = (_hitCount % maxHits) * 3;
//    _hits[idx + 0] = u;
//    _hits[idx + 1] = v;
//    _hits[idx + 2] = r;
//    _hitCount++;

//    _mat.SetFloatArray("_Hits", _hits);
//    _mat.SetInt("_HitCount", Mathf.Min(_hitCount, maxHits));
//}
//else
//{
//    // Fallback: your old spherical math (kept if ray ever misses)
//    Vector3 local = transform.InverseTransformPoint(cp.point).normalized;
//    float lon = Mathf.Atan2(local.x, local.z);
//    float lat = Mathf.Asin(local.y);
//    float u = (lon + Mathf.PI) / (2f * Mathf.PI);
//    float v = (lat + Mathf.PI * 0.5f) / Mathf.PI;

//    float r = Random.Range(minMaxRadius.x, minMaxRadius.y);
//    int idx = (_hitCount % maxHits) * 3;
//    _hits[idx + 0] = u; _hits[idx + 1] = v; _hits[idx + 2] = r;
//    _hitCount++;
//    _mat.SetFloatArray("_Hits", _hits);
//    _mat.SetInt("_HitCount", Mathf.Min(_hitCount, maxHits));
//}

//Debug.DrawLine(start, hit.point, Color.magenta, 2f);
//Debug.Log($"UV hit on: {hit.collider.name}  uv=({hit.textureCoord.x:F2},{hit.textureCoord.y:F2})");


//// remove projectile
//Destroy(cp.otherCollider.attachedRigidbody ? cp.otherCollider.attachedRigidbody.gameObject : cp.otherCollider.gameObject);           


