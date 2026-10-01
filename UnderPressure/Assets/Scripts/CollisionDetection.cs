using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    RaycastHit hit;
    Vector3 lastHitPoint;
    [SerializeField] LayerMask terrainMask;
    [SerializeField] ArduinoConnector connector;
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100.0f, terrainMask))
        {   
            if (lastHitPoint.y != hit.point.y) 
            {
                lastHitPoint = hit.point;
                connector.UpdateBottom(hit.point);
                Debug.Log("new bottom at: " + hit.point);
            }
           
        }

    }
}
