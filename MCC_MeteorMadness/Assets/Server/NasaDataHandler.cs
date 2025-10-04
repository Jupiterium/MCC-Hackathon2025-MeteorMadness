using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NasaDataHandler : MonoBehaviour
{
    // URL of NASA's Neo API endpoint
    private string apiUrl = "https://api.nasa.gov/neo/rest/v1/feed";

    // Parameters for NASA API
    private string apiKey = "5P4uLC1RY5tlh0rigvI5Cfd08vzuf0zWHvE4Cbwe";  // Replace with your actual API key
    private string startDate = "2025-09-07";  // Example start date
    private string endDate = "2025-09-08";    // Example end date

    // Start is called before the first frame update
    void Start()
    {
        // Start fetching NASA data via coroutine
        StartCoroutine(GetNasaData());
    }

    // Coroutine to make HTTP request and parse the response
    IEnumerator GetNasaData()
    {
        // Construct the API URL with parameters
        string requestUrl = $"{apiUrl}?api_key={apiKey}&start_date={startDate}&end_date={endDate}";

        // Sending GET request to the NASA API
        UnityWebRequest www = UnityWebRequest.Get(requestUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Parse the JSON data from the server
            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Received Data: " + jsonResponse);

            // Process the NASA data
            NasaData nasaData = ProcessNasaData(jsonResponse);

            // Example usage: Display information about asteroids
            DisplayAsteroids(nasaData);
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }
    }

    // Function to process the NASA data (deserialize JSON into usable objects)
    NasaData ProcessNasaData(string jsonData)
    {
        NasaData data = JsonUtility.FromJson<NasaData>(jsonData);
        return data;
    }

    // Example function to display asteroid details
    void DisplayAsteroids(NasaData data)
    {
        if (data == null || data.near_earth_objects == null)
        {
            Debug.LogError("No asteroid data found.");
            return;
        }

        foreach (var date in data.near_earth_objects)
        {
            Debug.Log($"Date: {date.Key}");

            foreach (var asteroid in date.Value)
            {
                if (asteroid == null)
                {
                    Debug.LogWarning("Asteroid is null, skipping...");
                    continue;
                }

                Debug.Log($"- Asteroid Magnitude: {asteroid.absolute_magnitude_h}");

                if (asteroid.close_approach_data != null)
                {
                    foreach (var approach in asteroid.close_approach_data)
                    {
                        if (approach != null)
                        {
                            Debug.Log($"  - Close Approach Date: {approach.close_approach_date}");
                            Debug.Log($"  - Miss Distance (km): {approach.miss_distance?.kilometers}");
                            Debug.Log($"  - Relative Velocity (km/h): {approach.relative_velocity?.kilometers_per_hour}");
                        }
                    }
                }

                Debug.Log($"- Estimated Diameter (max meters): {asteroid.estimated_diameter?.meters?.estimated_diameter_max}");
                Debug.Log($"- Estimated Diameter (min meters): {asteroid.estimated_diameter?.meters?.estimated_diameter_min}");
            }
        }
    }
}