using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    RaycastHit hit;
    Vector3 offset = new Vector3(0, 0, 5);
    Vector3 p1;
    Vector3 p2;
    float radius = 0.5f;
    float maxDistance= 100.0f;
    Vector3 lastHitPoint;
    [SerializeField] LayerMask terrainMask;
    [SerializeField] ArduinoConnector connector;

    
    void Start()
    {
         
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* if (Physics.Raycast(transform.position, Vector3.down, out hit, 100.0f, terrainMask))
         {   
             if (lastHitPoint.y != hit.point.y) 
             {
                 lastHitPoint = hit.point;
                 connector.UpdateBottom(hit.point);
                 Debug.Log("new bottom at: " + hit.point);
             }

         }*/
        p1 = transform.position - offset ;
        p2 = transform.position + offset ;
        if (Physics.CapsuleCast(p1, p2, radius, Vector3.down, out hit, maxDistance, terrainMask))
        {
            Debug.Log("hit something at: " + hit.point.y);

            if (lastHitPoint.y != hit.point.y)
            {
                lastHitPoint = hit.point;
                connector.UpdateBottom(hit.point);
                Debug.Log("new bottom at: " + hit.point);
            }

        }
    }
}
