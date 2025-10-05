using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization; 

public class NasaDataHandler : MonoBehaviour
{
    private string apiUrl = "https://api.nasa.gov/neo/rest/v1/feed";
    private string apiKey = "5P4uLC1RY5tlh0rigvI5Cfd08vzuf0zWHvE4Cbwe";
    private string startDate = "2025-09-07";
    private string endDate = "2025-09-08";

    void Start()
    {
        StartCoroutine(GetNasaData());
    }

    IEnumerator GetNasaData()
    {
        string requestUrl = $"{apiUrl}?api_key={apiKey}&start_date={startDate}&end_date={endDate}";
        UnityWebRequest www = UnityWebRequest.Get(requestUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Received Data: " + jsonResponse);

            NasaData nasaData = ProcessNasaData(jsonResponse);
            AddAsteroids(nasaData, AsteroidDataManager.asteroidDataList);
            AsteroidDataManager.DebugInfo();
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }
    }

    NasaData ProcessNasaData(string jsonData)
    {
        try
        {
            NasaData data = JsonConvert.DeserializeObject<NasaData>(jsonData);
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error during deserialization: " + ex.Message);
            return null;
        }
    }

    //void AddAsteroids(NasaData data, List<AsteroidData> asteroidDataList)
    //{
    //    if (data == null || data.near_earth_objects == null)
    //    {
    //        Debug.LogError("No asteroid data found.");
    //        return;
    //    }

    //    foreach (var date in data.near_earth_objects)
    //    {
    //        foreach (var asteroid in date.Value)
    //        {
    //            if (asteroid == null)
    //            {
    //                Debug.LogWarning("Asteroid is null, skipping...");
    //                continue;
    //            }

    //            // Retrieve properties from the asteroid
    //            string name = asteroid.name;
    //            Vector3 position = GetAsteroidPosition(asteroid);  // Calculate position (placeholder here)

    //            // Parse the size value, ensure it's a valid float, otherwise use a default (100f)
    //            float size = (float)asteroid.estimated_diameter?.meters?.estimated_diameter_max;

    //            // Parse the speed value, ensure it's a valid float, otherwise use a default (0f)
    //            float speed = float.Parse(asteroid.close_approach_data?[0].relative_velocity?.kilometers_per_hour);

    //            // Parse the miss distance value, ensure it's a valid float, otherwise use a default (0f)
    //            float missDistance = float.Parse(asteroid.close_approach_data?[0].miss_distance?.kilometers);

    //            // Create an AsteroidData object
    //            AsteroidData asteroidData = new AsteroidData(name, position, size, speed, missDistance);
    //            asteroidDataList.Add(asteroidData);
    //        }
    //    }

    //    // Update the global singleton with the new asteroid data
    //    //AsteroidDataManager.Instance.UpdateAsteroidData(asteroidDataList);
    //}

    void AddAsteroids(NasaData data, List<AsteroidData> asteroidDataList)
    {
        if (data == null || data.near_earth_objects == null)
        {
            Debug.LogError("No asteroid data found.");
            return;
        }

        // Clear previous results if you want a fresh load each time
        asteroidDataList.Clear();

        foreach (var kv in data.near_earth_objects)
        {
            string dateKey = kv.Key; // e.g., "2025-09-07"
            var list = kv.Value;
            if (list == null) { continue; }

            foreach (var asteroid in list)
            {
                if (asteroid == null || asteroid.close_approach_data == null || asteroid.close_approach_data.Count == 0)
                {
                    continue;
                }

                // Only consider approaches relative to Earth (the feed can contain others)
                CloseApproachData cad = asteroid.close_approach_data.Find(c => c != null && c.orbiting_body == "Earth");
                if (cad == null) { continue; }

                // --- Parse numbers safely (invariant culture) ---
                float sizeMeters = 100f; // default so tiny NEOs are still visible
                if (asteroid.estimated_diameter != null && asteroid.estimated_diameter.meters != null)
                {
                    sizeMeters = asteroid.estimated_diameter.meters.estimated_diameter_max;
                    if (sizeMeters <= 0f) { sizeMeters = 100f; }
                }

                float speedKmH = 0f;
                if (cad.relative_velocity != null)
                {
                    float.TryParse(cad.relative_velocity.kilometers_per_hour, NumberStyles.Float, CultureInfo.InvariantCulture, out speedKmH);
                }

                float missKm = 0f;
                if (cad.miss_distance != null)
                {
                    float.TryParse(cad.miss_distance.kilometers, NumberStyles.Float, CultureInfo.InvariantCulture, out missKm);
                }

                // --- Convert to scene units ---
                // 1 unit = 1000 km
                float missUnits = missKm / 1000f;

                // Keep a minimum so nothing ends up inside Earth in edge cases
                // If you set Earth radius in the spawner, we will clamp again there.
                if (missUnits < 0f) { missUnits = 0f; }

                // --- Place around Earth on a deterministic direction ---
                // Use asteroid name + date to get a reproducible unit direction.
                string key = (asteroid.name ?? "NEO") + "|" + (cad.close_approach_date_full ?? cad.close_approach_date ?? dateKey);
                Vector3 dir = HashToUnitVector3(key);

                // Store a world position *relative to Earth centre* (we’ll add the real Earth position when we instantiate)
                Vector3 position = dir * missUnits;

                // Create and store
                string name = asteroid.name ?? "NEO";
                AsteroidData item = new AsteroidData(name, position, sizeMeters, speedKmH, missKm);
                asteroidDataList.Add(item);
            }
        }

        // If you prefer, push into the singleton “officially”
        AsteroidDataManager.UpdateAsteroidData(asteroidDataList);
    }

    // --- Helpers ---

    // Deterministic “random” direction from a string key
    static Vector3 HashToUnitVector3(string key)
    {
        unchecked
        {
            int h1 = (int)0x811C9DC5;
            for (int i = 0; i < key.Length; i++) { h1 = (h1 ^ key[i]) * 16777619; }

            // Split to two angles
            float u = Mathf.Abs(h1 % 100000) / 100000f;          // 0..1
            float v = Mathf.Abs((h1 / 100000) % 100000) / 100000f;

            float theta = u * 2f * Mathf.PI;                      // 0..2π
            float z = v * 2f - 1f;                                // -1..1
            float r = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));      // circle radius at z
            Vector3 dir = new Vector3(r * Mathf.Cos(theta), z, r * Mathf.Sin(theta));
            return dir.normalized;
        }
    }

    // Calculate or return the asteroid position (just a placeholder for now)
    Vector3 GetAsteroidPosition(Asteroid asteroid)
    {
        // In the real world, you would calculate the position based on its orbital path or distance.
        // For now, this is just a random position to visualize the asteroid in space.
        return new Vector3(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
    }
}
