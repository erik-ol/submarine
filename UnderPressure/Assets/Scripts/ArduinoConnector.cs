using UnityEngine;
using System.IO.Ports;


public class ArduinoConnector : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM5", 9600);   
    int micBaseline;
    void Start()
    {
        serial.Open();
        serial.ReadTimeout = 0;
        micBaseline = int.Parse(serial.ReadLine());
        Debug.Log("baseline: " +micBaseline);
    }

    
    void Update()
    {
        //reading the data from arduino
        string data = serial.ReadLine();
        //turning the value into int
        int value = int.Parse(data);
        Debug.Log("value: " + value);
       if (value > micBaseline + 100)
        {
            Debug.Log("value " + value);
            transform.position = transform.position + new Vector3(0, transform.position.y + value*100/1023, 0); //the 100 is the range for height, so we change this depending on the depth of the water
        }
    }
}
