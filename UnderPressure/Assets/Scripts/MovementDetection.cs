using UnityEngine;

public class MovementDetection : MonoBehaviour
{
    private Vector3 previousPosition;
    public float minMoveDistance = 0.1f;
    [SerializeField] AudioSource radar;
    private bool playing = false;

    void Start()
    {
        radar.Pause();
        previousPosition = transform.position;
        
    }


    void Update()
    {
        float moveDistance = Vector3.Distance(transform.position, previousPosition);
        Debug.Log("move " + moveDistance);
        if (moveDistance >= minMoveDistance)
        {
            if (!playing)
            {
                Debug.Log("playing");
                radar.Play(0);
                playing = true;
            }

            previousPosition = transform.position;

        }
        else
        {
            radar.Stop();
            playing = false;
        }
    }
}
