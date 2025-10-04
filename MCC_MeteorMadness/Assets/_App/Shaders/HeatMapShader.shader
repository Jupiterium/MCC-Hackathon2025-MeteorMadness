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
            struct v2f     { float2 uv:TEXCOORD0;    float4 pos:SV_POSITION; };

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _BaseTint;
            float  _AdditiveStrength;

            // (u, v, r, s) per hit: uv coords, radius (UV units), intensity (0..1)
            float _Hits[4 * 64];
            int   _HitCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // radial falloff using per-hit radius r (in UV units)
            float Falloff(float2 uv, float2 pt, float r)
            {
                r = max(r, 1e-5);
                float d = distance(uv, pt) / r;
                float t = saturate(1.0 - d);
                return t * t; // quadratic falloff
            }

            // 0..1 -> color ramp
            float3 HeatRamp(float w)
            {
                // define three colors: green (low), yellow (mid), red (high)
                float3 cLow  = float3(0.0, 1.0, 0.0); // green
                float3 cMid  = float3(1.0, 1.0, 0.0); // yellow
                float3 cHigh = float3(1.0, 0.0, 0.0); // red

                w = saturate(w);

                if (w < 0.5)
                    return lerp(cLow, cMid, w / 0.5);   // green to yellow
                else
                    return lerp(cMid, cHigh, (w - 0.5) / 0.5); // yellow to red
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
                    float  s  = _Hits[k*4+3]; // intensity weight (0..1)

                    // Quadratic falloff for epicenter dominance
                    float falloff = Falloff(uv, pt, r);
                    w += s * falloff * 2.5; // amplify the impact for visible saturation
                }

                w = saturate(w);

                // Only apply heatmap if there’s an impact
                float3 heat = (w > 0.001) ? HeatRamp(w) * _AdditiveStrength : float3(0,0,0);

                // Final color: base planet + impact heat
                float3 finalCol = baseCol + heat;

                return float4(finalCol, 1.0);
            }
            ENDHLSL
        }
    }
}







