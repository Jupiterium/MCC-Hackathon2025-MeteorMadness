using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using TMPro; 
public class Listener : MonoBehaviour
{ 
    private int port = 25002;
    // Object reefrences and text references to be updated when app is runnnig 
    public Transform objectToUpdate1; 
    public Transform objectToUpdate2;
    public Transform objectToUpdate3;
    public Transform objectToUpdate4;
    public TMP_Text obj1NameText;
    public TMP_Text obj2NameText;
    public TMP_Text obj3NameText;
    public TMP_Text obj1DataText;
    public TMP_Text obj2DataText;
    public TMP_Text obj3DataText;

    private string temporaryDataStreamName1;
    private string temporaryDataStreamName2;
    private string temporaryDataStreamName3;

    // Connections references 
    private Thread receiveThread;
    private TcpListener server;
    private TcpClient client;
    private bool isRunning = true;
    //private string dataStreamName = "";

    // Queues for each data stream
    private Queue<float> chpTemp1Queue = new Queue<float>();
    private Queue<float> loopTemp1Queue = new Queue<float>();
    private Queue<float> stoomQueue = new Queue<float>();
    private Queue<float> CHP1SPredQueue = new Queue<float>();

    void Start(){
        // Initialize connection with python client
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData(){
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        while (isRunning){
            try{
                client = server.AcceptTcpClient();
                using (NetworkStream stream = client.GetStream()){
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    StringBuilder dataBuilder = new StringBuilder();
                    while (isRunning){
                        int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                        if (bytesRead == 0){
                            // Client disconnected
                            Debug.Log("Breaking connection to client");
                            break;
                        }
                        string dataString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        dataBuilder.Append(dataString);
                        // THIS CODE IS EXPECTING THE DATA IN THE FORMAT : 
                        // COLUMN1 VALUE1
                        // COLUMN2 VALUE2
                        // COLUMN3 VALUE3
                        // IF U CHANGE THE FORMAT OF YOUR DATA BEFORE TO SEND THEM REMEMBER TO ADAPT THIS PART OF THE CODE AS WELL
                        // Process data when a newline is received
                        if (dataBuilder.ToString().EndsWith("\n")){
                            string[] dataLines = dataBuilder.ToString().Split('\n');
                            foreach (string line in dataLines){
                                if (line.Trim() != ""){ // Ignore empty lines
                                    string[] dataParts = Regex.Split(line.Trim(), @"\s+"); // Split by whitespace
                                    if (dataParts.Length >= 2){
                                        // Expect two parts: data stream name and data point
                                        string dataStreamName = dataParts[0];
                                        float dataPoint;
                                        // Attempt to parse data point as float
                                        if (float.TryParse(dataParts[1], out dataPoint)){
                                            // Enqueue data point based on the data stream name 
                                            // (CHANGE NAMES DEPENDING ON THE COLUMN NAME DATA YOU WANT TO SEND, THEY HAVE TO MATCH WITH THE NAMES SENT FROM THE PYTHON FILE)
                                            // ALSO REMMEBER TIO CHANGE THE OBJECT TO UPDATE FOR THE TEXT IF YOU ARE CHANGING OBJECTS
                                            if (dataStreamName == "CHPTemp1"){
                                                chpTemp1Queue.Enqueue(dataPoint);
                                                temporaryDataStreamName1 = dataStreamName;
                                            }else if (dataStreamName == "LoopTemp1"){
                                                loopTemp1Queue.Enqueue(dataPoint);
                                                temporaryDataStreamName2 = dataStreamName;
                                            }else if (dataStreamName == "Stoom"){
                                                stoomQueue.Enqueue(dataPoint);
                                                temporaryDataStreamName3 = dataStreamName;
                                            }else{
                                                Debug.LogWarning("Unknown data stream name: " + dataStreamName);
                                            }
                                        }else{
                                            Debug.LogError("Failed to parse data point: " + line);
                                        }
                                    }else{
                                        Debug.LogError("Invalid data format: " + line);
                                    }
                                }
                            }
                            dataBuilder.Clear(); // Reset for the next frame
                        }
                    }
                }
            }catch (Exception e){
                Debug.LogError("Error receiving data: " + e.Message);
            }finally{
                if (client != null){
                    Debug.Log("Finally");
                    client.Close();
                }
            }
        }
        server.Stop();
    }



    public static float CalculateYPosition(float receivedData){
        // This function calculates the y position using linear interpolation.
        // Ensure yScale is within the valid range (0.0000 to 1.0000)
        receivedData = Mathf.Clamp01(receivedData);
        // Define minimum and maximum y positions
        // THIS VALUE DEPENDS ON THE OBJET YOU ARE TRYINT TO UPDATE
        // SINCE FOR ALL OF MY OBJECTS THE STARTING Y POS IS 0 AND THE MAXIMUM Y POS OF THE OBJETCS THAT I CAN STILL SEE IN MY CURRENT CAMERA VIEW IS AROUND 0.7, I'M USING THIS 2 VALUES, BUT THEY NEEDS TO BE EDITED DEPENDING ON YOUR OBJETCS AND DATA
        float yPosMin = 0.7f;
        float yPosMax = 0.01f;
        float interpolationFactor = receivedData;
        // Perform linear interpolation to get the final y position
        float yPosition = Mathf.Lerp(yPosMax, yPosMin, interpolationFactor);
        return yPosition;
    }


    // This function is called every frame (like a while true in python)
    void Update(){
        // Update object Y positions based on their respective queues
        if (chpTemp1Queue.Count > 0){
            float data = chpTemp1Queue.Dequeue();
            // Normalize data to a value between 0 and 1
            float normalizedValue = data / 100f;
            float yPosition = CalculateYPosition(normalizedValue);
            // Create a temporary Vector3 with only the updated Y values
            Vector3 updatedPosition = new Vector3(objectToUpdate1.localPosition.x, yPosition, objectToUpdate1.localPosition.z);
            // Update the object's position with the temporary Vector3
            objectToUpdate1.localPosition = updatedPosition;
            // Update data text 
            obj1DataText.text = data.ToString("F2");
            obj1NameText.text = temporaryDataStreamName1;
        }
        
        if (loopTemp1Queue.Count > 0){
            float data = loopTemp1Queue.Dequeue();
            // Normalize data to a value between 0 and 1
            float normalizedValue = data / 100f;
            float yPosition = CalculateYPosition(normalizedValue);
            // Create a temporary Vector3 with only the updated Y values
            Vector3 updatedPosition = new Vector3(objectToUpdate2.localPosition.x, yPosition, objectToUpdate2.localPosition.z);
            // Update the object's position with the temporary Vector3
            objectToUpdate2.localPosition = updatedPosition;
            // Update data text 
            obj2DataText.text = data.ToString("F2");
            obj2NameText.text = temporaryDataStreamName2;
        }

        if (stoomQueue.Count > 0){
            float data = stoomQueue.Dequeue();
            // Normalize data to a value between 0 and 1
            float normalizedValue = data * 5f  ;  // Just because it's a very smalll value, so this make it the filling nrmalized to the others,  
            float yPosition = CalculateYPosition(normalizedValue);
            // Create a temporary Vector3 with only the updated Y values
            Vector3 updatedPosition = new Vector3(objectToUpdate3.localPosition.x, yPosition, objectToUpdate3.localPosition.z);
            // Update the object's position with the temporary Vector3
            objectToUpdate3.localPosition = updatedPosition;
            // Update data text 
            obj3DataText.text = (data * 1000).ToString("F2");
            obj3NameText.text = temporaryDataStreamName3;
        }

        // if(CHP1SPredQueue.Count > 0){
        //     float data = CHP1SPredQueue.Dequeue();
        //     // Normalize data to a value between 0 and 1
        //     float normalizedValue = data / 100f;
        //     float yPosition = CalculateYPosition(normalizedValue);
        //     // Create a temporary Vector3 with only the updated Y values
        //     Vector3 updatedPosition = new Vector3(objectToUpdate4.localPosition.x, yPosition, objectToUpdate4.localPosition.z);
        //     // Update the object's position with the temporary Vector3
        //     objectToUpdate4.localPosition = updatedPosition;
        //     // Update data text 
        //     obJ4DataText.text = data.ToString("F2");
        // }
    }
}

