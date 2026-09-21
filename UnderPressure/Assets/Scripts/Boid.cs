using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public float speed = 1f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * speed);
        transform.localRotation *= Quaternion.FromToRotation(transform.forward, velocity.normalized);
    }
}
