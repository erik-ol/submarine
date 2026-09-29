#ifndef UNDERWATER_MS_INCLUDED
#define UNDERWATER_MS_INCLUDED

void UnderwaterDebug_float(float2 UV, float RawDepth, float SurfaceY, out float3 Out)
{
    #ifdef SHADERGRAPH_PREVIEW
    Out = 0; // the node preview has no scene -> show black 
    #else
    // screen position + depth -> world position of this pixel
    float3 posWS = ComputeWorldSpacePosition(UV, RawDepth, UNITY_MATRIX_I_VP);

    // how far below the water surface this point is (metres)
    float depthBelowSurface = SurfaceY - posWS.y;

    Out = depthBelowSurface.xxx / 20.0; // 0-20 m -> black to white
    #endif
}

#endif