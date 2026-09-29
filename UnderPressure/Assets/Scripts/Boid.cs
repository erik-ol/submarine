using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public float speed = 1f;

    // move and face in direction of velocity
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * speed);
        if (velocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(velocity) * Quaternion.AngleAxis(90, Vector3.right);
        }
    }
}
