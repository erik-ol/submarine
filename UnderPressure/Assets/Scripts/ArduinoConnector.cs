using UnityEngine;
using System.IO.Ports;
using System;



public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM5", 9600);   
    int micBaseline;
    Vector3 initialPosition;
    float sinkSpeed = 6f; 
    float sensitivity = 2f; // Adjust this value to change the sensitivity of the movement
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
            micBaseline = 700;
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

        //splitting the string into what type of input it is
        string[] input = data.Split(':'); //e.g. input "microphone: 832"

        if (input[0] == "Microphone")
        {

            //turning the value into int
            int value = int.Parse(input[1]);
            Debug.Log("micvalue: " + value);
            if (value > micBaseline + 100)
            {
                    
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

        if (input[0] == "Potentiometer")
        {
            //turning the value into int
            int value = int.Parse(input[1]);
            Debug.Log("potvalue: " + value);
            if (value > micBaseline + 100)
            {
                    
                    float yOffset = value*sensitivity/1023f;
                    Debug.Log("yOffset: " + yOffset);
                    
                    transform.position -=  new Vector3(0, yOffset, 0); //the 100 is the range for height, so we change this depending on the depth of the water
            }

            if (transform.position.y < initialPosition.y) // slowly go down to the initial hight
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
}
