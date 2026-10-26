#ifndef UNDERWATER_MS_INCLUDED
#define UNDERWATER_MS_INCLUDED

#include "Assets/Shaders/Underwater/UnderwaterWaterData.hlsl"

// Attenuation of the surface colour (Monzon et al. 2024, Eq. 10)
// In-scattering from multiple scattering (Eq. 9)
// World Y points up, the water surface is at SurfaceY
// "depth" is metres below the surface (positive going down)

// ---------------------------------------------------------------------------
// Caustics helpers
// ---------------------------------------------------------------------------

// Sample the caustics texture 3 times with small offsets -> r, g, b (dispersion)
float3 SampleCausticsRGB(UnityTexture2D tex, float2 uv, float split)
{
    float r = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, uv + float2( split,  split)).r;
    float g = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, uv + float2( split, -split)).r;
    float b = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, uv + float2(-split, -split)).r;
    return float3(r, g, b);
}

// Caustics pattern at a world position
float3 CausticsPattern(UnityTexture2D tex, float3 posWS, float scale, float speed, float split, float time)
{
    // Basis perpendicular to the light = pattern is "projected" by the sun
    #ifdef SHADERGRAPH_PREVIEW
    float3 L = float3(0, 1, 0);                                 // preview has no lights
    #else
    float3 L = _MainLightPosition.xyz;                          // direction towards the light
    L = dot(L, L) < 1e-6 ? float3(0, 1, 0) : normalize(L);      // fallback: straight down
    #endif
    float3 up = abs(L.y) > 0.99 ? float3(1, 0, 0) : float3(0, 1, 0);
    float3 T  = normalize(cross(up, L));
    float3 B  = cross(L, T);
    float2 uv = float2(dot(posWS, T), dot(posWS, B)) / max(scale, 0.01);

    // Two layers with different speed, direction and orientation, combined with min
    float2 uv1 =  uv + float2(1, 0) * time * speed * 0.75;
    float2 uv2 = -uv + float2(1, 0) * time * speed;
    return min(SampleCausticsRGB(tex, uv1, split), SampleCausticsRGB(tex, uv2, split));
}

void UnderwaterSurface_float(float3 SceneColor, float2 UV, float RawDepth,
                             float SurfaceY, float WaterType, float Turbidity,
                             float Samples, float MaxDist, float SunIrradiance,
                             UnityTexture2D CausticsTex, float CausticsScale, float CausticsSpeed,
                             float CausticsStrength, float CausticsSplit,
                             float CausticsDepthFade, float TimeSec, out float3 Out)
{
    #ifdef SHADERGRAPH_PREVIEW
    Out = SceneColor;
    #else
    float3 posWS = ComputeWorldSpacePosition(UV, RawDepth, UNITY_MATRIX_I_VP);
    float3 camWS = _WorldSpaceCameraPos;

    // Background pixels - treat as far away and dark
    #if UNITY_REVERSED_Z
    bool isBackground = RawDepth <= 0.00001;
    #else
    bool isBackground = RawDepth >= 0.99999;
    #endif

    // Anything at or above SurfaceY is out of the water (or is a separate water-surface mesh),
    // so leave it untouched: no caustics, no spectral tint.
    if (!isBackground && posWS.y > SurfaceY - 0.25)
    {
        Out = SceneColor;
        return;
    }

    float  P      = isBackground ? MaxDist : min(distance(posWS, camWS), MaxDist); // metres of water to the camera
    float3 colour = isBackground ? 0 : SceneColor;
    float  depth  = max(SurfaceY - posWS.y, 0); // metres below the surface

    float3 rd        = normalize(posWS - camWS); // direction from camera to pixel
    float  camDepth  = max(SurfaceY - camWS.y, 0); // camera: metres below the surface
    float  cosDown   = -rd.y; // +1 looking straight down, -1 straight up

    // -----------------------------------------------------------------------
    // Snell's window: what the camera sees when it looks UP at the water surface.
    // Sky pixels (no geometry) that look upward are refracted through a wavy surface at
    // SurfaceY. Inside the ~97 degree window you see the sky and the sun; outside it the
    // surface mirrors the water, so it fades to the normal underwater haze.
    // The sky is a simple analytic gradient driven by the main light, so it follows SunCycle
    // (sunrise / noon / sunset / night). The spectral model below then tints it like anything else.
    // -----------------------------------------------------------------------
    if (isBackground && rd.y > 0.0 && camDepth > 0.0)
    {
        float3 wL = _MainLightPosition.xyz;                       // direction towards the sun
        wL = dot(wL, wL) < 1e-6 ? float3(0, 1, 0) : normalize(wL);
        float3 wSunCol = _MainLightColor.rgb;                     // sun colour * intensity

        // where the view ray meets the water surface
        float  wT   = camDepth / rd.y;                            // metres of water up to the surface
        float3 wHit = camWS + rd * wT;

        // gentle analytic waves -> surface slope (no texture needed)
        float2 wp = wHit.xz;
        float2 wSlope = 0;
        wSlope += float2( 0.80,  0.60) * 0.080 * cos(dot(wp, float2( 0.80,  0.60)) * 0.60 + TimeSec * 0.90);
        wSlope += float2(-0.50,  0.87) * 0.050 * cos(dot(wp, float2(-0.50,  0.87)) * 0.95 + TimeSec * 1.30);
        wSlope += float2( 0.20, -0.98) * 0.040 * cos(dot(wp, float2( 0.20, -0.98)) * 1.70 + TimeSec * 1.90);
        float3 wN = -normalize(float3(-wSlope.x, 1.0, -wSlope.y)); // surface normal, facing down into the water

        // refraction water -> air (index 1.33); k < 0 means total internal reflection
        float wCosI = saturate(-dot(rd, wN));
        float wK    = 1.0 - 1.33 * 1.33 * (1.0 - wCosI * wCosI);

        float3 wWindow = 0;
        if (wK > 0.0)
        {
            float  wCosT = sqrt(wK);
            float  wF    = 0.02 + 0.98 * pow(1.0 - wCosT, 5.0);   // Fresnel: more mirror towards the window edge
            float3 wR    = refract(rd, wN, 1.33);                 // ray direction in the air

            float  wH   = saturate(wR.y);
            float  wDay = smoothstep(-0.10, 0.25, wL.y);          // 0 at night, 1 by midday
            float  wLow = 1.0 - smoothstep(0.0, 0.45, wL.y);      // 1 when the sun is low

            float3 wZenith  = lerp(float3(0.01, 0.015, 0.05), float3(0.10, 0.32, 0.85), wDay);
            float3 wHorizon = lerp(float3(0.02, 0.03, 0.08),  float3(0.60, 0.75, 0.95), wDay);
            wHorizon = lerp(wHorizon, float3(1.0, 0.45, 0.20), wLow * wDay);   // warm horizon at sunrise / sunset
            float3 wSky = lerp(wHorizon, wZenith, pow(wH, 0.45));

            float wSd = saturate(dot(wR, wL));                    // how close this ray is to the sun
            wSky += wSunCol * (pow(wSd, 64.0) * 0.5 + smoothstep(0.9995, 1.0, wSd) * 30.0); // glow + sun disc

            wWindow = wSky * (1.0 - wF) * 1.5;                    // 1.5 = overall window brightness
        }

        // treat the surface as the "object": it is camera-to-surface metres away, at depth 0
        colour = wWindow;
        P      = min(wT, MaxDist);
        depth  = 0;
    }

    // caustics
    if (!isBackground && CausticsStrength > 0)
    {
        float3 caustics = CausticsPattern(CausticsTex, posWS, CausticsScale, CausticsSpeed, CausticsSplit, TimeSec);

        // caustics get blurrier and weaker further from the surface
        float depthFade = exp(-depth / max(CausticsDepthFade, 0.01));

        colour *= 1.0 + CausticsStrength * caustics * depthFade;
    }

    int n = clamp((int)Samples, 1, 32);
    float3 xyz = 0;
    float3 white = 0;

    [loop]
    for (int i = 0; i < n; i++)
    {
        float wl = lerp(400.0, 700.0, (i + 0.5) / n); // centred wavelength samples
        float3 bckd = Turbidity * GetWaterProps(wl, WaterType); // (b, c, Kd)
        float b  = bckd.x;
        float c  = bckd.y;
        float kd = bckd.z;

        // Light from the object (Eq. 10)
        float surface = UpsampleRGB(colour, wl)
                      * exp(-kd * depth)   // sunlight: surface -> object
                      * exp(-c * P);       // object -> camera

        // Light scattered towards the camera by the water itself (Eq. 9)
        // cMod combines loss along the ray (c) with the sunlight getting weaker
        // as the ray goes deeper (kd * cosDown)
        float cMod = c + kd * cosDown;
        cMod = (abs(cMod) < 1e-4) ? 1e-4 : cMod;
        float inScatter = b * SunIrradiance
                        * exp(-kd * camDepth) // sunlight reaching the camera's depth
                        * (1.0 - exp(-P * cMod)) // more water on the path = more glow
                        / (4.0 * PI * cMod); // isotropic phase function (1 / 4pi)

        float3 sens = Sensitivity(wl);
        xyz   += (surface + inScatter) * sens;
        white += sens; // same sum for a white object with no water
    }

    // XYZ -> screen colour, white-balanced so white stays white with no water
    Out = max(0, XYZToLinearSRGB(xyz) / XYZToLinearSRGB(white));
    #endif
}

#endif