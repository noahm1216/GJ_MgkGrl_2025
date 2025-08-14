Shader "Custom/URP/WispyFogAnimated"
{
    Properties
    {
        _DarkColor ("Dark Color", Color) = (0.2, 0.2, 0.2, 1)
        _MidColor ("Mid Color", Color) = (0.5, 0.5, 0.5, 1)
        _LightColor ("Light Color", Color) = (0.8, 0.8, 0.8, 1)
        _DissolveHeight ("Dissipation Height", Float) = 0.0
        _NoiseScale ("Noise Scale", Float) = 2.0
        _NoiseIntensity ("Noise Intensity", Float) = 1.0
        _NoiseSpeed ("Noise Scroll Speed", Float) = 0.2
        _MorphSpeed ("Noise Morph Speed", Float) = 0.5
        _MorphScale ("Noise Morph Scale", Float) = 0.5
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float4 _DarkColor;
            float4 _MidColor;
            float4 _LightColor;
            float _DissolveHeight;
            float _NoiseScale;
            float _NoiseIntensity;
            float _NoiseSpeed;
            float _MorphSpeed;
            float _MorphScale;

            // Perlin helpers
            float2 fade(float2 t) { return t * t * t * (t * (t * 6 - 15) + 10); }

            float grad(int hash, float2 p)
            {
                int h = hash & 7;
                float u = h < 4 ? p.x : p.y;
                float v = h < 4 ? p.y : p.x;
                return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
            }

            float perlinNoise(float2 P)
            {
                int2 Pi = (int2)floor(P) & 255;
                float2 Pf = frac(P);

                int aa = (Pi.x + Pi.y * 57) & 255;
                int ab = (Pi.x + (Pi.y+1) * 57) & 255;
                int ba = ((Pi.x+1) + Pi.y * 57) & 255;
                int bb = ((Pi.x+1) + (Pi.y+1) * 57) & 255;

                float2 f = fade(Pf);

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
                float t = _Time.y;

                // Scroll + morph offsets
                float2 scroll = float2(t * _NoiseSpeed, t * _NoiseSpeed * 0.5);
                float morph = perlinNoise(IN.positionWS.xz * _MorphScale + float2(t * _MorphSpeed, t * _MorphSpeed));
                float2 morphOffset = float2(morph, morph);

                // Noise coords
                float2 noisePos = IN.positionWS.xz * _NoiseScale + scroll + morphOffset;

                // Base height fade
                float heightFactor = (IN.positionWS.y - _DissolveHeight);

                // Perlin noise for edge waviness
                float noiseValue = perlinNoise(noisePos);
                float fade = saturate(heightFactor + (noiseValue - 0.5) * _NoiseIntensity);

                // --- 3-Color Gradient ---
                float3 finalColorRGB;
                float finalAlpha;

                if (fade < 0.5)
                {
                    // First half: Dark -> Mid
                    float subFade = fade / 0.5;
                    finalColorRGB = lerp(_DarkColor.rgb, _MidColor.rgb, subFade);
                    finalAlpha = lerp(_DarkColor.a, _MidColor.a, subFade);
                }
                else
                {
                    // Second half: Mid -> Light
                    float subFade = (fade - 0.5) / 0.5;
                    finalColorRGB = lerp(_MidColor.rgb, _LightColor.rgb, subFade);
                    finalAlpha = lerp(_MidColor.a, _LightColor.a, subFade);
                }

                // Combine gradient alpha with dissipation fade
                finalAlpha *= (1.0 - fade);

                // Cutoff at full fade
                if (fade >= 1.0)
                    finalAlpha = 0.0;

                return float4(finalColorRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
