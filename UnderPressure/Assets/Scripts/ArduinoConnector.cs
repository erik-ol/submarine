using UnityEngine;
using System.IO.Ports;
using System;


public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM3", 9600);   
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
    }

    
    void Update()
    {
        //reading the data from arduino
        string data; 
        try
        {
            data = serial.ReadLine();
        }
        catch (TimeoutException)
        {
            return;
        }
        //turning the value into int
        int value = int.Parse(data);
        Debug.Log("value: " + value);
       if (value > micBaseline + 100)
        {

            //turning the value into int
            int value = int.Parse(input[1]);
            Debug.Log("micvalue: " + value);
            if (value < micBaseline + 50)
            {
                floatSpeed = 0f;
            }
            else if (value > micBaseline + 50 && value<micBaseline+100)
            {
                floatSpeed = 1.0f;
            }
            else if (value > micBaseline + 100 && value<micBaseline+150)
            {
                floatSpeed = 1.5f;
            }
            else if (value > micBaseline + 150)
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

        if (input[0] == "Potentiometer")
        {
            int value = int.Parse(input[1]);
            Debug.Log("potvalue: " + value);
            if (value < 128)
            {
                sinkSpeed = 0f;
            }
            else if (value > 256)
            {
                sinkSpeed = 0.75f;
            }
            else if (value > 384)
            {
                sinkSpeed = 1.0f;
            }
            else if (value > 512)
            {
                sinkSpeed = 1.25f;
            }
            else if (value > 640)
            {
                sinkSpeed = 1.5f;
            }
            else if (value > 768)
            {
                sinkSpeed = 1.75f;
            }
            else if (value > 896)
            {
                sinkSpeed = 2.0f;
            }

                                
            target =  new Vector3(transform.position.x, bottom, transform.position.z); 
            transform.position = Vector3.MoveTowards(transform.position, target, sinkSpeed*Time.deltaTime); 

            // snap once close enough
            if (Mathf.Abs(transform.position.y - surface) < 0.001f)
            {
                transform.position = target;
            }
            
            float yOffset = value*sensitivity/1023f;
            Debug.Log("yOffset: " + yOffset);
            transform.position +=  new Vector3(0, yOffset, 0); //the 100 is the range for height, so we change this depending on the depth of the water
        }

        if (transform.position.y > initialPosition.y) // slowly go down to the initial hight
        {
            Vector3 target = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, sinkSpeed*Time.deltaTime);

            // snap once close enough
            if (Mathf.Abs(transform.position.y - initialPosition.y) < 0.001f)
            {
                transform.position = target;
            }
        }
    }
}
