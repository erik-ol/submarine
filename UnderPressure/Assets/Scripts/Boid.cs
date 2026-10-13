using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public float speed = 40f;

    // move and face in direction of velocity
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * speed, Space.World);
        if (velocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(velocity);
            //transform.Rotate(90, 0, 0); // relative to own coordinate space
            Debug.DrawRay(transform.position, velocity);
        }
    }
}
