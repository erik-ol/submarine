using UnityEngine;

public class CausticsAnimate : MonoBehaviour
{
    public Texture2D[] frames;      // drag all 16 in, in order
    public float fps = 12f;

    Light lt;
    float timer;
    int index;

    void Start() { lt = GetComponent<Light>(); }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            index = (index + 1) % frames.Length;
            lt.cookie = frames[index];
        }
    }
}