using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource AudioSource;

    private void Awake ()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
    }
    public void PlaySound(AudioClip clip)
    {
        AudioSource.PlayOneShot(clip);
    }
}
