using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;



public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM11", 9600);
    Thread serialThread;
    bool keepReading = true;
    readonly object stateLock = new object();
    int latestMicValue;
    bool hasNewMicValue = false;
    int latestPotValue;
    bool hasNewPotValue = false;

    int micBaseline;
    Vector3 initialPosition;
    float floatSpeed = 0f;
    float sinkSpeed = 0f;
    float sensitivity = 2f; // Adjust this value to change the sensitivity of the movement
    float surface = 50f;
    float bottom = 0f;
    Vector3 target;


    void Start()
    { 
        serial.Open();
        serial.ReadTimeout = 50;
        string initalData = serial.ReadLine();
        string [] initialInput = initalData.Split(':');
        if (initialInput[0] == "Microphone")
        {
            micBaseline = int.Parse(initialInput[1]);
            Debug.Log("baseline: " +micBaseline);  
        }
        else
        {
            micBaseline = 770; //the baseline often seems to be around here
            Debug.Log("No baseline found: " +micBaseline);  
        }
        initialPosition = transform.position;
        serialThread = new Thread(SerialReadLoop);
        serialThread.IsBackground = true;
        serialThread.Start();
    }

    void SerialReadLoop()
    {
        while (keepReading)
        {
            string line;
            try
            {
                line = serial.ReadLine();
            }
            catch (TimeoutException)
            {
                continue;
            }
            catch (Exception)
            {
                break;
            }

            string[] input = line.Split(':');

            if (input[0] == "Microphone" && int.TryParse(input[1], out int micValue))
            {
                Debug.Log("entered if mic");
                lock (stateLock)
                {
                    latestMicValue = micValue;
                    hasNewMicValue = true;
                    Debug.Log("new mic value");
                }
            }

            else if (input[0] == "Potentiometer" && int.TryParse(input[1], out int potValue))
            {
                Debug.Log("entered if pot");
                lock (stateLock)
                {
                    latestPotValue = potValue;
                    hasNewPotValue = true;
                    Debug.Log("new pot value");
                }
            }
        }
    }
   
    
    void Update()
    {

        int micValue = 0;
        int potValue = 0;
        bool gotMicValue = false;
        bool gotPotValue = false;

        lock (stateLock)
        {
            if (hasNewMicValue)
            {
                micValue = latestMicValue;
                gotMicValue = true;
                hasNewMicValue = false;
            }
            if (hasNewPotValue)
            {
                potValue = latestPotValue;
                gotPotValue = true;
                hasNewMicValue = false;
            }
        }

        if (gotMicValue)
        {

            Debug.Log("micvalue: " + micValue);
            if (micValue < micBaseline + 50)
            {
                floatSpeed = 0f;
            }
            else if (micValue > micBaseline + 50 && micValue < micBaseline+100)
            {
                floatSpeed = 1.0f;
            }
            else if (micValue > micBaseline + 100 && micValue <micBaseline+150)
            {
                floatSpeed = 1.5f;
            }
            else if (micValue > micBaseline + 150)
            {
                floatSpeed = 2.0f;
            }
            

                                
            target =  new Vector3(transform.position.x, surface, transform.position.z); 
            transform.position = Vector3.MoveTowards(transform.position, target, floatSpeed*Time.deltaTime); 

            // snap once close enough
            if (Mathf.Abs(transform.position.y - surface) < 0.001f)
            {
                transform.position = target;
            }

        }

        if (gotPotValue)
        {
            Debug.Log("potvalue: " + potValue);
            if (potValue < 128)
            {
                sinkSpeed = 0f;
            }
            else if (potValue > 256)
            {
                sinkSpeed = 0.75f;
            }
            else if (potValue > 384)
            {
                sinkSpeed = 1.0f;
            }
            else if (potValue > 512)
            {
                sinkSpeed = 1.25f;
            }
            else if (potValue > 640)
            {
                sinkSpeed = 1.5f;
            }
            else if (potValue > 768)
            {
                sinkSpeed = 1.75f;
            }
            else if (potValue > 896)
            {
                sinkSpeed = 2.0f;
            }

                                
            target =  new Vector3(transform.position.x, bottom, transform.position.z); 
            transform.position = Vector3.MoveTowards(transform.position, target, sinkSpeed*Time.deltaTime); 

            // snap once close enough
            if (Mathf.Abs(transform.position.y - bottom) < 0.001f)
            {
                transform.position = target;
            }
            
            
        }

       
        
    }

   void OnApplicationQuit()
    {
        keepReading = false;
        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join(200);
        }
        if (serial!= null && serial.IsOpen)
        {
            serial.Close();
        }
    }
}
