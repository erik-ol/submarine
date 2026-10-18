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

    float  P      = isBackground ? MaxDist : min(distance(posWS, camWS), MaxDist); // metres of water to the camera
    float3 colour = isBackground ? 0 : SceneColor;
    float  depth  = max(SurfaceY - posWS.y, 0); // metres below the surface
    
    float3 rd        = normalize(posWS - camWS); // direction from camera to pixel
    float  camDepth  = max(SurfaceY - camWS.y, 0); // camera: metres below the surface
    float  cosDown   = -rd.y; // +1 looking straight down, -1 straight up
    
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