using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    //video 13:50
    SerialPort serial = new SerialPort("COM5", 9600);   
    void Start()
    {
        serial.Open();
        serial.ReadTimeout = 10;
        
    }

    
    void Update()
    {
        //reading the data from arduino
        string data = serial.ReadLine();
        //turning the value into int
        int value = int.Parse(data);
        transform.rotation 

    }
}
