using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Linq;

public class AsteroidInfoUpdate : MonoBehaviour
{
    [Header("Texts (TMP)")]
    public TMP_Text nameText;
    public TMP_Text diameterText;   // e.g., "Diameter: 350 m"
    public TMP_Text distanceText;   // e.g., "Distance: 42,000 km"
    public TMP_Text velocityText;   // e.g., "Velocity: 27,500 km/h"

    [Header("Debug")]
    public bool verboseLogs = false;

    // current selection
    int currentSelection = 0;

    // Scene scale: 1 unit = 1000 km
    const float UNITS_TO_KM = 1000f;
    public GameObject buttonPrevious;
    public GameObject buttonNext;


    public void Start()
    {

    }

    public void showCurrentText() {
        if (currentSelection == 0)
        {
            nameText.text = "Name: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).name;
            diameterText.text = "Diameter: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).size + "-m";
            distanceText.text = "Distance: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).missDistance + "-km";
            velocityText.text = "Velocity: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).speed + "-km/h";
        }
    }
    public void IncreaseText()
    {
        currentSelection += 1;
        if (currentSelection > AsteroidDataManager.asteroidDataList.Count - 1)
            currentSelection = AsteroidDataManager.asteroidDataList.Count - 1;

        nameText.text = "Name: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).name;
        diameterText.text = "Diameter: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).size + "-m";
        distanceText.text = "Distance: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).missDistance + "-km";
        velocityText.text = "Velocity: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).speed + "-km/h";
    }

    public void DecreaseText()
    {
        currentSelection -= 1;
        if (currentSelection < 0)
            currentSelection = 0;

        nameText.text = "Name: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).name;
        diameterText.text = "Diameter: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).size + "-m";
        distanceText.text = "Distance: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).missDistance + "-km";
        velocityText.text = "Velocity: " + AsteroidDataManager.asteroidDataList.ElementAt(currentSelection).speed + "-km/h";
    }

}
