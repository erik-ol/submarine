using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public float speed = 40f;

    float surface = 100f;

    // move and face in direction of velocity
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * speed, Space.World);
        if (velocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(velocity);
            //transform.Rotate(90, 0, 0); // relative to own coordinate space
            //Debug.DrawRay(transform.position, velocity);
        }

        // cant go above water surface
        if (transform.position.y > surface)
        {
            transform.position = new Vector3(transform.position.x, surface, transform.position.z);
        }
    }
}
