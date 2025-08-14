Shader "Custom/URP/WispyFog"
{
    Properties
    {
        _DarkColor ("Dark Color", Color) = (0.2, 0.2, 0.2, 1)
        _LightColor ("Light Color", Color) = (0.8, 0.8, 0.8, 1)
        _DissolveHeight ("Dissipation Height", Float) = 0.0
        _NoiseScale ("Noise Scale", Float) = 2.0
        _NoiseIntensity ("Noise Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
            };

            float4 _DarkColor;
            float4 _LightColor;
            float _DissolveHeight;
            float _NoiseScale;
            float _NoiseIntensity;

            // --- Perlin Noise Function ---
            float2 fade(float2 t) { return t * t * t * (t * (t * 6 - 15) + 10); }

            float grad(int hash, float2 p)
            {
                int h = hash & 7;
                float u = h < 4 ? p.x : p.y;
                float v = h < 4 ? p.y : p.x;
                return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
            }

            // Classic Perlin 2D
            float perlinNoise(float2 P)
            {
                int2 Pi = (int2)floor(P) & 255;
                float2 Pf = frac(P);

                int aa = (Pi.x + Pi.y * 57) & 255;
                int ab = (Pi.x + (Pi.y+1) * 57) & 255;
                int ba = ((Pi.x+1) + Pi.y * 57) & 255;
                int bb = ((Pi.x+1) + (Pi.y+1) * 57) & 255;

                float2 f = fade(Pf);

                // Hashing
                float gradAA = grad(aa, Pf);
                float gradBA = grad(ba, Pf - float2(1,0));
                float gradAB = grad(ab, Pf - float2(0,1));
                float gradBB = grad(bb, Pf - float2(1,1));

                float lerpX1 = lerp(gradAA, gradBA, f.x);
                float lerpX2 = lerp(gradAB, gradBB, f.x);

                return lerp(lerpX1, lerpX2, f.y) * 0.5 + 0.5;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Height-based fade
                float heightFactor = saturate((IN.positionWS.y - _DissolveHeight));

                // Perlin noise to make wispy fade
                float noiseValue = perlinNoise(IN.positionWS.xz * _NoiseScale);
                heightFactor = saturate(heightFactor + (noiseValue - 0.5) * _NoiseIntensity);

                // Color blend
                float4 col = lerp(_DarkColor, _LightColor, heightFactor);

                // Alpha fades out at top
                col.a = 1.0 - heightFactor;

                return col;
            }
            ENDHLSL
        }
    }
}
