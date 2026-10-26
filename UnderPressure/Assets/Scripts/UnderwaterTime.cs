using UnityEngine;

// Sends the game time to the underwater material every frame
public class UnderwaterTime : MonoBehaviour
{
    public Material underwaterMaterial;          // M_UnderwaterPass
    public string timeProperty = "_TimeSec";    

    void Update()
    {
        Shader.SetGlobalFloat(timeProperty, Time.time);           // in case the property is global
        if (underwaterMaterial != null)
            underwaterMaterial.SetFloat(timeProperty, Time.time); // in case it's per material
    }
}