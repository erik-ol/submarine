#ifndef UNDERWATER_MS_INCLUDED
#define UNDERWATER_MS_INCLUDED

#include "Assets/Shaders/Underwater/UnderwaterWaterData.hlsl"

// Attenuation of the surface colour (Monzon et al. 2024, Eq. 10)
// In-scattering from multiple scattering (Eq. 9)
// World Y points up, the water surface is at SurfaceY
// "depth" is metres below the surface (positive going down)
void UnderwaterSurface_float(float3 SceneColor, float2 UV, float RawDepth,
                             float SurfaceY, float WaterType, float Turbidity,
                             float Samples, float MaxDist, float SunIrradiance, 
                             out float3 Out)
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