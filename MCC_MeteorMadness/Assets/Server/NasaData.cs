using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NasaData
{
    public Dictionary<string, List<Asteroid>> near_earth_objects;
}

[System.Serializable]
public class Asteroid
{
    public float absolute_magnitude_h;
    public List<CloseApproachData> close_approach_data;
    public EstimatedDiameter estimated_diameter;
    public string name; // Ensure name exists if needed
}

[System.Serializable]
public class CloseApproachData
{
    public string close_approach_date;
    public string close_approach_date_full;
    public long epoch_date_close_approach;
    public MissDistance miss_distance;
    public string orbiting_body;
    public RelativeVelocity relative_velocity;
}

[System.Serializable]
public class MissDistance
{
    public string astronomical;
    public string kilometers;
    public string lunar;
    public string miles;
}

[System.Serializable]
public class RelativeVelocity
{
    public string kilometers_per_hour;
    public string kilometers_per_second;
    public string miles_per_hour;
}

[System.Serializable]
public class EstimatedDiameter
{
    public Diameter feet;
    public Diameter kilometers;
    public Diameter meters;
    public Diameter miles;
}

[System.Serializable]
public class Diameter
{
    public float estimated_diameter_max;
    public float estimated_diameter_min;
}