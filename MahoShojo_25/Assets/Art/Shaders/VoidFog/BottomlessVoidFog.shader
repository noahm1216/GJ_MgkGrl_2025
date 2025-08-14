Shader "Custom/URP/BottomlessVoidFog"
{
    Properties
    {
        _DarkColor("Dark Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _MidColor("Mid Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _LightColor("Light Color", Color) = (1.0, 1.0, 1.0, 0.0)
        
        _HeightStart("Height Start", Float) = 0.0
        _HeightEnd("Height End", Float) = 1.0
        _MidColorPosition("Mid Color Vertical Position", Range(0,1)) = 0.5

        _NoiseScale("Noise Scale", Float) = 1.0
        _NoiseIntensity("Noise Intensity", Float) = 0.5
        _NoiseSpeed("Noise Speed", Float) = 1.0
    }

    SubShader
    {
        Tags{"RenderType"="Transparent" "Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _DarkColor;
            float4 _MidColor;
            float4 _LightColor;
            float _HeightStart;
            float _HeightEnd;
            float _MidColorPosition;
            float _NoiseScale;
            float _NoiseIntensity;
            float _NoiseSpeed;

            // Hash-based noise helpers
            float hash(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Height factor
                float heightFactor = saturate((i.worldPos.y - _HeightStart) / (_HeightEnd - _HeightStart));

                // Animate noise
                float time = _Time.y * _NoiseSpeed;
                float n = noise(i.worldPos.xz * _NoiseScale + time);
                heightFactor += (n - 0.5) * _NoiseIntensity;

                // Clamp final factor
                heightFactor = saturate(heightFactor);

                // Three-color blend with mid position shift
                float midPos = saturate(_MidColorPosition);
                float3 rgb;
                float alpha;

                if (heightFactor < midPos)
                {
                    float t = saturate(heightFactor / midPos);
                    rgb = lerp(_DarkColor.rgb, _MidColor.rgb, t);
                    alpha = lerp(_DarkColor.a, _MidColor.a, t);
                }
                else
                {
                    float t = saturate((heightFactor - midPos) / (1.0 - midPos));
                    rgb = lerp(_MidColor.rgb, _LightColor.rgb, t);
                    alpha = lerp(_MidColor.a, _LightColor.a, t);
                }

                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
