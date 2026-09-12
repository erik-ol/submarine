using UnityEngine;
using System.IO.Ports;
using System;


public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM3", 9600);   
    int micBaseline;
    Vector3 initialPosition;
    float sinkSpeed = 5f; 
    float sensitivity = 5f; // Adjust this value to change the sensitivity of the movement
    void Start()
    { 
        serial.Open();
        serial.ReadTimeout = 50;
        micBaseline = int.Parse(serial.ReadLine());
        Debug.Log("baseline: " +micBaseline);
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
