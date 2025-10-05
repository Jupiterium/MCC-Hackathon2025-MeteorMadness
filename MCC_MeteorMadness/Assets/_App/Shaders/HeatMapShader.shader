Shader "Unlit/HeatMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseTint ("Base Tint", Color) = (1,1,1,1)
        _AdditiveStrength ("Add Strength", Range(0,2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float2 uv:TEXCOORD0; float4 pos:SV_POSITION; };

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _BaseTint;
            float  _AdditiveStrength;

            float _Hits[4 * 64];
            int   _HitCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float Falloff(float2 uv, float2 pt, float r)
            {
                r = max(r, 1e-5);
                float2 diff = abs(uv - pt);
                diff = min(diff, 1.0 - diff);
                float d = length(diff) / r;
                float t = saturate(1.0 - d);
                return t * t;
            }

            // Improved gradient emphasizing orange and red
            float3 HeatRamp(float w)
            {
                w = saturate(w);

                // Key gradient points
                float3 c1 = float3(0.0, 0.6, 0.0); // dark green
                float3 c2 = float3(1.0, 1.0, 0.0); // yellow
                float3 c3 = float3(1.0, 0.5, 0.0); // orange
                float3 c4 = float3(1.0, 0.0, 0.0); // red

                if (w < 0.3)
                    return lerp(c1, c2, w / 0.3);       // green -> yellow
                else if (w < 0.6)
                    return lerp(c2, c3, (w - 0.3) / 0.3); // yellow -> orange
                else
                    return lerp(c3, c4, (w - 0.6) / 0.4); // orange -> red
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float3 baseCol = tex2D(_MainTex, uv).rgb * _BaseTint.rgb;

                float w = 0.0;
                [loop]
                for (int k = 0; k < _HitCount; k++)
                {
                    float2 pt = float2(_Hits[k*4+0], _Hits[k*4+1]);
                    float  r  = _Hits[k*4+2];
                    float  s  = _Hits[k*4+3];

                    float falloff = Falloff(uv, pt, r);
                    w += s * falloff * 2.2; // slightly increased brightness
                }

                w = saturate(w);
                float3 heat = (w > 0.001) ? HeatRamp(w) * _AdditiveStrength : float3(0,0,0);
                float3 finalCol = baseCol + heat;

                return float4(finalCol, 1.0);
            }
            ENDHLSL
        }
    }
}
