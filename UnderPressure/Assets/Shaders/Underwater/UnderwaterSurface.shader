Shader "Custom/UnderwaterSurface"
{
    // Water surface seen from BELOW (URP).
    // Inside Snell's window you see the refracted sky + sun.
    // Outside it the surface acts as a mirror (total internal reflection).
    Properties
    {
        [NoScaleOffset] _NormalMap ("Wave Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.6
        _Tiling ("Tiling (1 / metres)", Float) = 0.05
        _Scroll1 ("Scroll Speed 1", Vector) = (0.02, 0.01, 0, 0)
        _Scroll2 ("Scroll Speed 2", Vector) = (-0.015, 0.02, 0, 0)

        _IOR ("Water Index of Refraction", Float) = 1.33
        _MirrorColor ("Mirror Colour (total internal reflection)", Color) = (0.02, 0.15, 0.25, 1)
        _DeepColor ("Deep Water Colour (fog)", Color) = (0.0, 0.05, 0.1, 1)
        _Absorption ("Absorption (per metre)", Float) = 0.01

        _SkyBoost ("Sky Brightness", Float) = 1
        _SunSize ("Sun Size (cos of angle)", Range(0.99, 1)) = 0.9995
        _SunBoost ("Sun Brightness", Float) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing   // needed for VR single-pass instanced rendering

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float _NormalStrength;
                float _Tiling;
                float4 _Scroll1;
                float4 _Scroll2;
                float _IOR;
                float4 _MirrorColor;
                float4 _DeepColor;
                float _Absorption;
                float _SkyBoost;
                float _SunSize;
                float _SunBoost;
            CBUFFER_END

            // Set globally by SunCycle.cs
            float4 _SunDir;    // direction TOWARD the sun
            float4 _SunColor;  // colour * fade

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // Ray from camera up to the surface
                float3 toSurface = IN.positionWS - _WorldSpaceCameraPos;
                float dist = length(toSurface);
                float3 I = toSurface / dist;

                // Two scrolling normal maps -> wave normal (tangent space, z = up)
                float2 uv = IN.positionWS.xz * _Tiling;
                float3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv + _Time.y * _Scroll1.xy), _NormalStrength);
                float3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv * 1.7 + _Time.y * _Scroll2.xy), _NormalStrength);
                float3 nt = normalize(float3(n1.xy + n2.xy, n1.z * n2.z));

                // Tangent space -> world (surface lies in XZ), then flip so it faces DOWN into the water
                float3 N = -normalize(float3(nt.x, nt.z, nt.y));

                // Refraction from water into air
                float cosI = saturate(-dot(I, N));
                float k = 1.0 - _IOR * _IOR * (1.0 - cosI * cosI);   // k < 0 -> total internal reflection

                // Fresnel (Schlick, using the transmitted angle for dense -> light medium)
                float f0 = pow((1.0 - _IOR) / (1.0 + _IOR), 2.0);
                float fresnel = 1.0;
                float3 skyCol = 0;
                if (k > 0.0)
                {
                    float cosT = sqrt(k);
                    fresnel = f0 + (1.0 - f0) * pow(1.0 - cosT, 5.0);

                    float3 R = refract(I, N, _IOR);
                    skyCol = GlossyEnvironmentReflection(R, 0.0, 1.0) * _SkyBoost;

                    // Sun disc
                    float sunDot = dot(R, normalize(_SunDir.xyz));
                    skyCol += smoothstep(_SunSize, 1.0, sunDot) * _SunColor.rgb * _SunBoost;
                }

                // Outside the window (and at grazing angles) you see a mirror of the underwater scene
                float3 col = lerp(skyCol, _MirrorColor.rgb, saturate(fresnel));

                // Water between camera and surface absorbs light (darker/bluer with distance)
                float fog = 1.0 - exp(-dist * _Absorption);
                col = lerp(col, _DeepColor.rgb, fog);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
