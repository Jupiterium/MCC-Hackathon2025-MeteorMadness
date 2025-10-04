// Shader "Unlit/HeatMapShader"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         LOD 100

//         // Pass
//         // {
//         //     CGPROGRAM
//         //     #pragma vertex vert
//         //     #pragma fragment frag
//         //     // make fog work
//         //     #pragma multi_compile_fog

//         //     #include "UnityCG.cginc"

//         //     struct appdata
//         //     {
//         //         float4 vertex : POSITION;
//         //         float2 uv : TEXCOORD0;
//         //     };

//         //     struct v2f
//         //     {
//         //         float2 uv : TEXCOORD0;
//         //         UNITY_FOG_COORDS(1)
//         //         float4 vertex : SV_POSITION;
//         //     };

//         //     sampler2D _MainTex;
//         //     float4 _MainTex_ST;

//         //     v2f vert (appdata v)
//         //     {
//         //         v2f o;
//         //         o.vertex = UnityObjectToClipPos(v.vertex);
//         //         o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//         //         UNITY_TRANSFER_FOG(o,o.vertex);
//         //         return o;
//         //     }

//         //     float3 colors[5]; //colors for point ranges
//         //     float pointranges[5];  //ranges of values used to determine color values
//         //     float _Hits[3 * 32]; //passed in array of pointranges 3floats/point, x,y,intensity
//         //     int _HitCount = 0;

//         //     void initalize()
//         //     {
//         //       colors[0] = float4(0, 0, 0, 0);
//         //       colors[1] = float4(0, 0.9, 0.2, 0.1);
//         //       colors[2] = float4(0.9, 1, 0.3, 1);
//         //       colors[3] = float4(0.9, 0.7, 0.1, 1);
//         //       colors[4] = float4(1, 0, 0, 1);
//         //       pointranges[0] = 0;
//         //       pointranges[1] = 0.25;
//         //       pointranges[2] = 0.5;
//         //       pointranges[3] = 0.75;
//         //       pointranges[4] = 1.0;

//         //       _HitCount = 1;
//         //       _Hits[0] = 0;
//         //       _Hits[1] = 0;
//         //       _Hits[2] = 2;
//         //     }

//         //     float distq(float2 a, float2 b){
//         //         float area_of_effect_size =  1.0f;
//         //         float d = pow(max(0.0, 1.0 - distance(a, b)/ area_of_effect_size), 2);
//         //         return d;
//         //     }

//         //     float3 getHeatForPixel(float weight) 
//         //     {
//         //         if(weight <= pointranges[0]) 
//         //         {
//         //             return colors[0];
//         //         }
//         //         if (weight >= pointranges[4])
//         //         {
//         //             return colors[4];
//         //         }

//         //         for (int i=1; i<5; i++)
//         //         {
//         //             if(weight < pointranges[i]) 
//         //             {
//         //                 float dist_from_lower_point = weight - pointranges[i-1];
//         //                 float size_of_point_range = pointranges[i] - pointranges[i-1];
//         //                 float ratio_over_lower_point = dist_from_lower_point / size_of_point_range;
                        
//         //                 float3 color_range = colors[i] - colors[i-1];
//         //                 float3 color_contribution = color_range * ratio_over_lower_point;
//         //                 float3 new_color = colors[i-1] + color_contribution;

//         //                 return new_color;
//         //             }
//         //         }
//         //         return colors[0];
//         //     }

//         //     fixed4 frag (v2f i) : SV_Target
//         //     {
//         //         fixed4 col = tex2D(_MainTex, i.uv);
//         //         float2 uv = i.uv;
//         //         uv = uv * 4.0 - float2(2.0,2.0);  //our texture uv range is -2 to 2
//         //         float totalWeight = 0;
//         //         for(float i = 0; i < _HitCount; i++){
//         //             float2 work_pt = float2(_Hits[i * 3 + 0], _Hits[i * 3 + 1]);
//         //             float pt_intensity = _Hits[i * 3 + 2];

//         //             totalWeight += 0.5 * distq(uv, work_pt) * pt_intensity;
//         //         }

//         //         float3 heat = getHeatForPixel(totalWeight);

//         //         return col + float4(heat, 0.5);
//         //     }
//         //     ENDCG

//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "UnityCG.cginc"

//             struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
//             struct v2f     { float2 uv:TEXCOORD0; float4 vertex:SV_POSITION; };

//             sampler2D _MainTex; float4 _MainTex_ST;

//             // --- uniforms from C# ---
//             float _Hits[3 * 32]; // (u, v, r) triplets in [0..1]
//             int   _HitCount;

//             // --- constant palette/thresholds (was never initialized before) ---
//             static const float3 COLORS[5] = {
//                 float3(0, 0, 0),
//                 float3(0.0, 0.9, 0.2),
//                 float3(0.9, 1.0, 0.3),
//                 float3(0.9, 0.7, 0.1),
//                 float3(1.0, 0.0, 0.0)
//             };
//             static const float RANGES[5] = { 0.0, 0.25, 0.5, 0.75, 1.0 };

//             v2f vert (appdata v) {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                 return o;
//             }

//             // falloff using radius r in UV space (r is the 3rd value in _Hits)
//             float falloff(float2 a, float2 b, float r)
//             {
//                 r = max(r, 1e-5);
//                 float t = 1.0 - (distance(a, b) / r);
//                 t = saturate(t);
//                 return t * t; // softer edge
//             }

//             float3 ramp(float w)
//             {
//                 if (w <= RANGES[0]) return COLORS[0];
//                 if (w >= RANGES[4]) return COLORS[4];

//                 // find segment
//                 [unroll]
//                 for (int i = 1; i < 5; i++)
//                 {
//                     if (w < RANGES[i])
//                     {
//                         float t = (w - RANGES[i-1]) / (RANGES[i] - RANGES[i-1]);
//                         return lerp(COLORS[i-1], COLORS[i], t);
//                     }
//                 }
//                 return COLORS[0];
//             }

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 float2 uv = i.uv; // keep in [0..1]  (REMOVED the *4 - 2 remap)

//                 // accumulate heat from all points
//                 float w = 0.0;
//                 [loop]
//                 for (int k = 0; k < _HitCount; k++)
//                 {
//                     float2 pt = float2(_Hits[k*3+0], _Hits[k*3+1]); // in [0..1]
//                     float  r  = _Hits[k*3+2];                       // radius in UV units
//                     w += falloff(uv, pt, r);
//                 }

//                 // optional scale if you want stronger effect (tune in C# by making r bigger)
//                 w = saturate(w);

//                 float3 baseCol = tex2D(_MainTex, uv).rgb;
//                 float3 heatCol = ramp(w);

//                 // additive-ish tint; you can also blend by alpha if you prefer
//                 return float4(baseCol + heatCol, 1.0);
//             }
//             ENDCG
//         }

//         }
//     }
// }
// }

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
                float3 c0 = float3(0.0, 0.0, 0.0);
                float3 c1 = float3(0.0, 0.9, 0.2);
                float3 c2 = float3(0.9, 1.0, 0.3);
                float3 c3 = float3(0.9, 0.7, 0.1);
                float3 c4 = float3(1.0, 0.0, 0.0);

                if (w <= 0.0) return c0;
                if (w >= 1.0) return c4;

                if (w < 0.25)  return lerp(c0, c1, w / 0.25);
                if (w < 0.50)  return lerp(c1, c2, (w - 0.25) / 0.25);
                if (w < 0.75)  return lerp(c2, c3, (w - 0.50) / 0.25);
                return            lerp(c3, c4, (w - 0.75) / 0.25);
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

                    w += s * Falloff(uv, pt, r);
                }
                w = saturate(w);

                float3 heat = HeatRamp(w) * _AdditiveStrength;
                return float4(baseCol + heat, 1.0);
            }
            ENDHLSL
        }
    }
}







