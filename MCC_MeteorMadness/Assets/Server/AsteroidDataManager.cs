//using System.Collections.Generic;
//using UnityEngine;

//public class AsteroidDataManager : MonoBehaviour
//{
//    // Singleton instance
//    public static AsteroidDataManager Instance { get; private set; }

//    // List of AsteroidData that will be accessed globally
//    public static List<AsteroidData> asteroidDataList = new List<AsteroidData>();

//    private void Awake()
//    {
//        // Ensure only one instance of AsteroidDataManager exists
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject); // Destroy any duplicate instances
//        }

//        DontDestroyOnLoad(gameObject); // Keep the manager across scenes (optional)
//    }

//    // Method to update the asteroid data
//    public static void UpdateAsteroidData(List<AsteroidData> newAsteroids)
//    {
//        asteroidDataList = newAsteroids;
//    }

//    public static void DebugInfo()
//    {
//        foreach (var dataItem in asteroidDataList)
//        {
//            Debug.Log($"Asteroid: {dataItem.name}, Position: {dataItem.position}, Size: {dataItem.size}, Speed: {dataItem.speed}, Miss Distance: {dataItem.missDistance}");
//        }
//    }
//}



using System.Collections.Generic;
using UnityEngine;

public class AsteroidDataManager : MonoBehaviour
{
    [Header("Camera")]
    public AsteroidNavCamera navCamera;

    public enum AsteroidScaleMode { Linear, Sqrt, Log10 }

    [Header("Asteroid Visual Scale")]
    public AsteroidScaleMode scaleMode = AsteroidScaleMode.Sqrt;  // good default

    [Tooltip("How many scene units should 1 meter of diameter produce. Try 0.02–0.05.")]
    public float sizeUnitsPerMeter = 0.03f; // 100 m → 3 units (nice & visible)
    public float metersToUnits = 1f / 1_000_000f; // 1 unit = 1000 km = 1,000,000 m
    public float visualScale = 120f; // boost; try 80–200

    [Tooltip("0.5 = sqrt compression, 1.0 = linear. Use 0.5 if big ones dominate.")]
    public float sizeExponent = 1.0f; // start linear; set 0.5 for sqrt
    public float minScaleUnits = 0.5f; // don’t go below this
    public float maxScaleUnits = 10f; // cap huge ones


    // Singleton
    public static AsteroidDataManager Instance { get; private set; }

    // Data store (already used by your handler)
    public static List<AsteroidData> asteroidDataList = new List<AsteroidData>();

    [Header("Scene & Prefab")]
    public Transform earth;                 // Earth centre transform
    public float earthRadiusUnits = 6.378f; // 6378 km / 1000 km-per-unit = 6.378
    public GameObject asteroidPrefab;       // Simple sphere/mesh
    //public Transform parentForAsteroids;    // Optional hierarchy parent

    [Header("Visibility / Navigation")]
    public bool showAllAtOnce = true;       // If false -> only one active and Next/Prev toggles
    public int currentIndex = 0;

    // Internals
    private static List<GameObject> _spawned = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Called by your "Load Data" button
    public void SpawnFromData()
    {
        // Clean previous
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i]);
            }
        }
        _spawned.Clear();

        if (asteroidPrefab == null || earth == null)
        {
            Debug.LogError("Assign Earth and Asteroid Prefab on AsteroidDataManager.");
            return;
        }

        // Spawn all items
        for (int i = 0; i < asteroidDataList.Count; i++)
        {
            var data = asteroidDataList[i];

            // Clamp so no object sits inside the Earth
            float missUnits = data.missDistance / 1000f; // km -> units
            float radius = Mathf.Max(missUnits, earthRadiusUnits + 0.1f);

            // World position = Earth centre + direction * radius
            Vector3 worldPos = earth.position + data.position.normalized * radius;

            GameObject go = Instantiate(asteroidPrefab, worldPos, Quaternion.identity);

            // data.size == diameter in meters (from NASA)
            float d = Mathf.Max(0.01f, data.size);
            float basis = Mathf.Pow(d, sizeExponent);              // 1.0 = linear, 0.5 = sqrt
            float s = Mathf.Clamp(basis * sizeUnitsPerMeter, minScaleUnits, maxScaleUnits);
            go.transform.localScale = Vector3.one * s;

            // If you use the camera focus helper, pass this size to it:
            if (!showAllAtOnce && i == 0 && navCamera != null)
            {
                navCamera.Focus(go.transform, s);
            }

            go.transform.localScale = Vector3.one * s;

            // Optional name label
            go.name = $"{data.name}  ({data.size:0} m)";

            _spawned.Add(go);
        }

        // Handle visibility mode
        if (!showAllAtOnce && _spawned.Count > 0)
        {
            for (int i = 0; i < _spawned.Count; i++) _spawned[i].SetActive(i == 0);
            currentIndex = 0;
            if (navCamera != null)
            {
                float sizeUnits0 = _spawned[0].transform.localScale.x;
                navCamera.Focus(_spawned[0].transform, sizeUnits0);
            }
        }

        Debug.Log($"Spawned {_spawned.Count} asteroids around Earth.");
    }

    // Called by your "Next" button
    public void ShowNext()
    {
        if (showAllAtOnce || _spawned.Count == 0) { return; }
        int next = (currentIndex + 1) % _spawned.Count;
        ShowIndex(next);
    }

    // Called by your "Previous" button
    public void ShowPrevious()
    {
        if (showAllAtOnce || _spawned.Count == 0) { return; }
        int prev = (currentIndex - 1 + _spawned.Count) % _spawned.Count;
        ShowIndex(prev);
    }

    private void ShowIndex(int idx)
    {
        if (_spawned.Count == 0) { return; }
        if (idx < 0 || idx >= _spawned.Count) { return; }

        for (int i = 0; i < _spawned.Count; i++)
        {
            _spawned[i].SetActive(i == idx);
        }
        currentIndex = idx;

        // Focus camera on the active asteroid
        if (navCamera != null)
        {
            var go = _spawned[idx];
            float sizeUnits = go.transform.lossyScale.x; // your uniform scale is diameter in units
            navCamera.Focus(go.transform, sizeUnits);
        }
    }

    // Keep (and use) your original static updater for consistency with NasaDataHandler
    public static void UpdateAsteroidData(List<AsteroidData> newAsteroids)
    {
        asteroidDataList = newAsteroids;
    }

    public static GameObject getAsteroidIndex(int index)
    {
        return _spawned[index];
    }

    float ComputeVisualScale(float sizeMeters)
    {
        // sizeMeters = diameter in meters (from your data)
        float d = Mathf.Max(0.01f, sizeMeters);

        float basis;
        switch (scaleMode)
        {
            case AsteroidScaleMode.Sqrt:
                basis = Mathf.Sqrt(d);          // compresses range, preserves differences
                break;
            case AsteroidScaleMode.Log10:
                basis = Mathf.Log10(d + 1f);    // strong compression, great for 10m..10km
                break;
            default: // Linear
                basis = d;                       // true-to-scale (usually too tiny)
                break;
        }

        float units = basis * metersToUnits * visualScale;
        return Mathf.Clamp(units, minScaleUnits, maxScaleUnits);
    }


    public static void DebugInfo()
    {
        foreach (var d in asteroidDataList)
        {
            Debug.Log($"Asteroid: {d.name}, Pos(units): {d.position}, Size(m): {d.size}, Speed(km/h): {d.speed}, Miss(km): {d.missDistance}");
        }
    }
}
