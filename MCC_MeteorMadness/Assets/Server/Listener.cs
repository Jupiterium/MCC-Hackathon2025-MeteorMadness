using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NasaAPIHandler : MonoBehaviour
{
    // URL of the Flask server running on Python
    private string apiUrl = "http://localhost:5000/get_neo_data";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetNasaData());
    }

    // Coroutine to make HTTP request and parse the response
    IEnumerator GetNasaData()
    {
        // Sending GET request to the Flask API
        UnityWebRequest www = UnityWebRequest.Get(apiUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Parse the JSON data from the server
            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Received Data: " + jsonResponse);
            
            // You can now process and use the JSON data
            // For example, you can deserialize it into C# objects
            ProcessNasaData(jsonResponse);
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }
    }

    // A function to process the received JSON data (you can create classes based on NASA response)
    void ProcessNasaData(string jsonData)
    {
        // Assuming the structure of the NASA JSON response, deserialize into C# objects
        // Example: Use Newtonsoft.Json (JSON.NET) or Unity's built-in JsonUtility to parse
        NasaResponse data = JsonUtility.FromJson<NasaResponse>(jsonData);

        // Process the data (this will depend on your specific use case)
        Debug.Log("Asteroids count: " + data.near_earth_objects.Count);
    }

    // C# classes that match the JSON response structure
    [System.Serializable]
    public class NasaResponse
    {
        public Dictionary<string, List<Asteroid>> near_earth_objects;
    }

    [System.Serializable]
    public class Asteroid
    {
        public string id;
        public string name;
        public string nasa_jpl_url;
    }
}