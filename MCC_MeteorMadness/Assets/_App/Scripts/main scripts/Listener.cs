using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class Listener : MonoBehaviour
{ 
    private int port = 25002;

    public GameObject meteorPrefab;      // Meteor prefab
    public Transform earthTransform;      // Earth reference

    // Connections
    private Thread receiveThread;
    private TcpListener server;
    private TcpClient client;
    private bool isRunning = true;

    // Queues for incoming meteor data
    private Queue<float> caDistanceAuQueue = new Queue<float>();
    private Queue<float> vRelativeQueue = new Queue<float>();
    private Queue<float> diameterQueue = new Queue<float>();

    void Start()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        while (isRunning)
        {
            try
            {
                client = server.AcceptTcpClient();
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    StringBuilder dataBuilder = new StringBuilder();

                    while (isRunning)
                    {
                        int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                        if (bytesRead == 0) break;

                        string dataString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        dataBuilder.Append(dataString);

                        if (dataBuilder.ToString().EndsWith("\n"))
                        {
                            string[] lines = dataBuilder.ToString().Split('\n');
                            foreach (string line in lines)
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;

                                string[] parts = Regex.Split(line.Trim(), @"\s+");
                                if (parts.Length >= 2)
                                {
                                    string streamName = parts[0];
                                    string value = string.Join(" ", parts, 1, parts.Length - 1); // join back for cases like "11 m - 24 m"

                                    switch(streamName)
                                    {
                                        case "CADistanceNominalAU":
                                            caDistanceAuQueue.Enqueue(ParseFloatSafe(value));
                                            break;
                                        case "VrelativeKms":
                                            vRelativeQueue.Enqueue(ParseFloatSafe(value));
                                            break;
                                        case "Diameter":
                                            diameterQueue.Enqueue(ParseDiameter(value));
                                            break;
                                        default:
                                            Debug.LogWarning("Unknown stream: " + streamName);
                                            break;
                                    }
                                }
                            }
                            dataBuilder.Clear();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving data: " + e.Message);
            }
            finally
            {
                if(client != null) client.Close();
            }
        }
        server.Stop();
    }

    void Update()
    {
        // Spawn meteors only when all queues have data
        while(caDistanceAuQueue.Count > 0 && vRelativeQueue.Count > 0 && diameterQueue.Count > 0)
        {
            float distanceAu = caDistanceAuQueue.Dequeue();
            float velocity = vRelativeQueue.Dequeue();
            float diameter = diameterQueue.Dequeue();

            SpawnMeteor(distanceAu, velocity, diameter);
        }
    }

    void SpawnMeteor(float distanceAu, float velocity, float diameter)
    {
        if(meteorPrefab == null || earthTransform == null) return;

        GameObject meteor = Instantiate(meteorPrefab);

        // Scale meteor
        meteor.transform.localScale = Vector3.one * Mathf.Clamp(diameter * 0.1f, 0.1f, 5f);

        // Convert AU to Unity units and spawn randomly around Earth
        float distanceKm = distanceAu * 149597870f;
        float scaledDistance = distanceKm * 0.00001f; // tweak for scene scale
        meteor.transform.position = earthTransform.position + UnityEngine.Random.onUnitSphere * scaledDistance;

        // Add controller to move toward Earth
        MeteorController mc = meteor.AddComponent<MeteorController>();
        mc.target = earthTransform;
        mc.speed = velocity;
    }

    // Helper: safely parse floats
    private float ParseFloatSafe(string str)
    {
        str = str.Replace(",", ".");  
        str = Regex.Replace(str, @"[^\d\.\-eE]", ""); // remove letters/symbols
        if(float.TryParse(str, out float value))
            return value;
        return 0f;
    }

    // Helper: parse diameter ranges like "11 m - 24 m"
    private float ParseDiameter(string str)
    {
        str = str.Replace("m", "").Trim();
        string[] parts = str.Split('-');
        if(parts.Length == 2)
        {
            float min = ParseFloatSafe(parts[0]);
            float max = ParseFloatSafe(parts[1]);
            return (min + max) / 2f; // average
        }
        return ParseFloatSafe(str);
    }
}
