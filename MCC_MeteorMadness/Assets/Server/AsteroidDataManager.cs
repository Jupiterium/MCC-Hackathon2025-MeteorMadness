using System.Collections.Generic;
using UnityEngine;

public class AsteroidDataManager : MonoBehaviour
{
    // Singleton instance
    public static AsteroidDataManager Instance { get; private set; }

    // List of AsteroidData that will be accessed globally
    public List<AsteroidData> asteroidDataList = new List<AsteroidData>();

    private void Awake()
    {
        // Ensure only one instance of AsteroidDataManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }

        DontDestroyOnLoad(gameObject); // Keep the manager across scenes (optional)
    }

    // Method to update the asteroid data
    public void UpdateAsteroidData(List<AsteroidData> newAsteroids)
    {
        asteroidDataList = newAsteroids;
    }
}
