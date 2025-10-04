using UnityEngine;
using UnityEngine.UI;

public class MeteorUIBinder : MonoBehaviour
{
    [Header("Scene")]
    public CustomMeteor meteorProps;   // drag the same object that has your CustomMeteor

    [Header("Sliders (-100..100)")]
    public Slider diameterSlider;      // meters (or your range)
    public Slider distanceSlider;      // AU
    public Slider speedSlider;         // km/s

    void Start()
    {
        // Remove any leftover listeners just in case
        diameterSlider.onValueChanged.RemoveAllListeners();
        distanceSlider.onValueChanged.RemoveAllListeners();
        speedSlider.onValueChanged.RemoveAllListeners();

        // Hook dynamic callbacks so they fire continuously while dragging
        diameterSlider.onValueChanged.AddListener(meteorProps.SetDiameterMeters);
        distanceSlider.onValueChanged.AddListener(meteorProps.SetDistanceAu);
        speedSlider.onValueChanged.AddListener(meteorProps.SetSpeedKmPerSec);

        // Push initial values once at start (so the meteor reflects current slider positions)
        meteorProps.SetDiameterMeters(diameterSlider.value);
        meteorProps.SetDistanceAu(distanceSlider.value);
        meteorProps.SetSpeedKmPerSec(speedSlider.value);
    }
}
