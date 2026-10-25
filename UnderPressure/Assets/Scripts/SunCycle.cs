using UnityEngine;
using UnityEngine.Rendering;

/// Moves a directional light across the sky (sunrise -> noon -> sunset -> night),
/// drives the procedural skybox, and publishes the sun direction/colour to shaders.
public class SunCycle : MonoBehaviour
{
    [Header("References")]
    public Light sun;                         // your Directional Light

    [Header("Time")]
    [Min(1f)] public float dayLengthSeconds = 120f;
    [Range(0f, 1f)] public float timeOfDay = 0.3f;   // 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
    public bool autoAdvance = true;

    [Header("Sun")]
    [Range(0f, 360f)] public float azimuth = 170f;   // compass direction of the sun's arc
    public float maxIntensity = 1.5f;
    public Gradient sunColor;                        // sampled by timeOfDay

    [Header("Environment")]
    public float environmentUpdateInterval = 0.5f;   // seconds between skybox/ambient refreshes

    float envTimer;

    void Reset()
    {
        // Sensible default colours: dark -> orange sunrise -> white noon -> orange sunset -> dark
        sunColor = new Gradient();
        sunColor.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.05f, 0.08f, 0.2f), 0.00f),
                new GradientColorKey(new Color(1.00f, 0.45f, 0.15f), 0.25f),
                new GradientColorKey(new Color(1.00f, 0.95f, 0.85f), 0.35f),
                new GradientColorKey(new Color(1.00f, 1.00f, 1.00f), 0.50f),
                new GradientColorKey(new Color(1.00f, 0.95f, 0.85f), 0.65f),
                new GradientColorKey(new Color(1.00f, 0.45f, 0.15f), 0.75f),
                new GradientColorKey(new Color(0.05f, 0.08f, 0.2f), 1.00f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
    }

    void Start()
    {
        if (sun != null) RenderSettings.sun = sun;   // procedural skybox follows this light
        Apply();
    }

    void Update()
    {
        if (sun == null) return;

        if (autoAdvance)
            timeOfDay = (timeOfDay + Time.deltaTime / dayLengthSeconds) % 1f;

        Apply();

        envTimer += Time.deltaTime;
        if (envTimer >= environmentUpdateInterval)
        {
            envTimer = 0f;
            DynamicGI.UpdateEnvironment();           // refresh skybox reflection + ambient
        }
    }

    void Apply()
    {
        if (sun == null) return;

        // 0.25 -> horizon (sunrise), 0.5 -> straight up, 0.75 -> horizon (sunset)
        float pitch = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(pitch, azimuth, 0f);

        Vector3 toSun = -sun.transform.forward;
        float elevation = toSun.y;                   // -1 (below) .. 1 (overhead)

        Color col = sunColor.Evaluate(timeOfDay);

        // Light fades out as the sun reaches the horizon; sun disc stays visible a bit longer
        float lightFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elevation * 3f));
        float discFade  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elevation * 20f + 0.2f));

        sun.color = col;
        sun.intensity = maxIntensity * lightFade;

        // Read by the underwater surface shader
        Shader.SetGlobalVector("_SunDir", toSun);
        Shader.SetGlobalColor("_SunColor", col * discFade);
    }
}
