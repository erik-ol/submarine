using UnityEngine;
using System.IO.Ports;
using System;


public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM5", 9600);   
    int micBaseline;
    void Start()
    {
        serial.Open();
        serial.ReadTimeout = 50;
        micBaseline = int.Parse(serial.ReadLine());
        Debug.Log("baseline: " +micBaseline);
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
            Debug.Log("change value " + value);
            float yOffset = value*10f/1023f;
            transform.position +=  new Vector3(0, yOffset, 0); //the 100 is the range for height, so we change this depending on the depth of the water
        }
    }
}
