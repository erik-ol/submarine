using UnityEngine;


public class CameraMover : MonoBehaviour

    
{
    public GameObject sub;
    Vector3 new_position;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        new_position = new Vector3(sub.transform.position.x,100f, sub.transform.position.z);
        this.transform.position = new_position;
    }
}
