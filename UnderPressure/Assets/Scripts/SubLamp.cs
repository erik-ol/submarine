using UnityEngine;

// Submarine lamp for the underwater full-screen pass.
// Put this on an empty object at the front of the sub, with its blue (Z) arrow
// pointing where the light should shine. Works in edit mode and Play mode.
[ExecuteAlways]
public class SubLamp : MonoBehaviour
{
    public bool lampOn = true;

    [Tooltip("Lamp colour. Slightly warm white looks like a real lamp.")]
    public Color color = new Color(1f, 0.95f, 0.85f);

    [Tooltip("Brightness of the light on surfaces in the cone.")]
    public float intensity = 50f;

    [Tooltip("Extra multiplier for the visible beam in the water (0 = no beam).")]
    public float beamStrength = 1f;

    [Range(5f, 120f)]
    [Tooltip("Full opening angle of the cone, in degrees.")]
    public float coneAngle = 40f;

    [Tooltip("How far the visible beam is computed (metres).")]
    public float beamRange = 30f;

    void Update()
    {
        bool on = lampOn && isActiveAndEnabled;

        Shader.SetGlobalVector("_UWLampPos", transform.position);
        Shader.SetGlobalVector("_UWLampDir", transform.forward);
        Shader.SetGlobalVector("_UWLampParams", new Vector4(
            Mathf.Cos(coneAngle * 0.5f * Mathf.Deg2Rad),
            intensity,
            beamRange,
            on ? 1f : 0f));

        Color c = color.linear;   // the project uses linear colour space
        Shader.SetGlobalVector("_UWLampColor", new Vector4(c.r, c.g, c.b, beamStrength));
    }

    void OnDisable()
    {
        // turn the lamp off in the shader when this object is disabled
        Shader.SetGlobalVector("_UWLampParams", Vector4.zero);
    }

    // Shows the cone in the Scene view when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        float r = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * beamRange;
        Vector3 end = transform.position + transform.forward * beamRange;
        Gizmos.DrawLine(transform.position, end + transform.up * r);
        Gizmos.DrawLine(transform.position, end - transform.up * r);
        Gizmos.DrawLine(transform.position, end + transform.right * r);
        Gizmos.DrawLine(transform.position, end - transform.right * r);
    }
}
