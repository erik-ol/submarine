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
    float floatSpeed = 0f;
    float sinkSpeed = 0f;
    float surface = 50f;
    float bottom = 0f; 
    Vector3 target;

    public float keyboardMoveSpeed = 10f;

    // position submarine resets to when you press Reset button (r)
    public float resetX = 30f;
    public float resetY = 30f;
    public float resetZ = 50f;


    void Start()
    { 
        serial.Open();
        serial.ReadTimeout = 50;
        string initalData = serial.ReadLine();
        string [] initialInput = initalData.Split(':');
        if (initialInput[0] == "Microphone")
        {
            micBaseline = int.Parse(initialInput[1]);
            //Debug.Log("baseline: " +micBaseline);  
        }
        else
        {
            micBaseline = 0; //the baseline often seems to be around here
            //Debug.Log("No baseline found: " +micBaseline);  
        }
        serialThread = new Thread(SerialReadLoop);
        serialThread.IsBackground = true;
        serialThread.Start();
    }

    public void UpdateBottom(Vector3 hitpoint)
    {   
        if (hitpoint.y > 0)
        {
            bottom = hitpoint.y+1;//1 is the thickness of the submarine
            //Debug.Log("new bottom is here");
        }
        else
        { 
            bottom = 1; 
        }
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
                //Debug.Log("entered if mic");
                lock (stateLock)
                {
                    latestMicValue = micValue;
                    hasNewMicValue = true;
                    //Debug.Log("new mic value");
                }
            }

            else if (input[0] == "Potentiometer" && int.TryParse(input[1], out int potValue))
            {
                //Debug.Log("entered if pot");
                lock (stateLock)
                {
                    latestPotValue = potValue;
                    hasNewPotValue = true;
                    //Debug.Log("new pot value");
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

        floatSpeed = 0f;
        sinkSpeed = 0f;

        if (gotMicValue)
        {

            //Debug.Log("micvalue: " + micValue);
            if (micBaseline == 0)
            { 
                micBaseline = micValue;
                //Debug.Log("micbaseline: " + micBaseline);
            }
            if (micValue < micBaseline + 40)
            {
                floatSpeed = 0f;
            }
            else if (micValue > micBaseline + 40 && micValue < micBaseline+80)
            {
                floatSpeed = 1.5f;
            }
            else if (micValue > micBaseline + 80 && micValue <micBaseline+120)
            {
                floatSpeed = 2.0f;
            }
            else if (micValue > micBaseline + 120)
            {
                floatSpeed = 3.0f;
            }

        }

        if (gotPotValue)
        {
            //Debug.Log("potvalue: " + potValue);
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
            
        }

        // todo: make foward movement with keys depend on vr look direction
        float dirForward = 0f;
        float dirSide = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            dirForward += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dirForward -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dirSide -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dirSide += 1;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            floatSpeed = 3.0f;
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sinkSpeed = 3.0f;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(resetX, resetY, resetZ);
        } else
        {
            transform.position = new Vector3(transform.position.x + dirSide*Time.deltaTime*keyboardMoveSpeed, Mathf.Clamp(transform.position.y + (floatSpeed-sinkSpeed)*Time.deltaTime, bottom, surface), transform.position.z + dirForward*Time.deltaTime*keyboardMoveSpeed);
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
