using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            DisplayAsteroids(nasaData);
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

    void DisplayAsteroids(NasaData data)
    {
        if (data == null || data.near_earth_objects == null)
        {
            Debug.LogError("No asteroid data found.");
            return;
        }

        List<AsteroidData> asteroidDataList = new List<AsteroidData>();

        foreach (var date in data.near_earth_objects)
        {
            foreach (var asteroid in date.Value)
            {
                if (asteroid == null)
                {
                    Debug.LogWarning("Asteroid is null, skipping...");
                    continue;
                }

                // Retrieve properties from the asteroid
                string name = asteroid.name;
                Vector3 position = GetAsteroidPosition(asteroid);  // Calculate position (placeholder here)

                // Parse the size value, ensure it's a valid float, otherwise use a default (100f)
                float size = (float) asteroid.estimated_diameter?.meters?.estimated_diameter_max;

                // Parse the speed value, ensure it's a valid float, otherwise use a default (0f)
                float speed = float.Parse(asteroid.close_approach_data?[0].relative_velocity?.kilometers_per_hour);

                // Parse the miss distance value, ensure it's a valid float, otherwise use a default (0f)
                float missDistance = float.Parse(asteroid.close_approach_data?[0].miss_distance?.kilometers);

                // Create an AsteroidData object
                AsteroidData asteroidData = new AsteroidData(name, position, size, speed, missDistance);
                asteroidDataList.Add(asteroidData);
            }
        }

        // Update the global singleton with the new asteroid data
        //AsteroidDataManager.Instance.UpdateAsteroidData(asteroidDataList);

        // Optionally, print the list for debugging purposes
        foreach (var dataItem in asteroidDataList)
        {
            Debug.Log($"Asteroid: {dataItem.name}, Position: {dataItem.position}, Size: {dataItem.size}, Speed: {dataItem.speed}, Miss Distance: {dataItem.missDistance}");
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
