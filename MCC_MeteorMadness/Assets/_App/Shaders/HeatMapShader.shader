Shader "Unlit/HeatMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float3 colors[5]; //colors for point ranges
            float pointranges[5];  //ranges of values used to determine color values
            float _Hits[3 * 32]; //passed in array of pointranges 3floats/point, x,y,intensity
            int _HitCount = 0;

            void initalize()
            {
              colors[0] = float4(0, 0, 0, 0);
              colors[1] = float4(0, 0.9, 0.2, 0.1);
              colors[2] = float4(0.9, 1, 0.3, 1);
              colors[3] = float4(0.9, 0.7, 0.1, 1);
              colors[4] = float4(1, 0, 0, 1);
              pointranges[0] = 0;
              pointranges[1] = 0.25;
              pointranges[2] = 0.5;
              pointranges[3] = 0.75;
              pointranges[4] = 1.0;

              _HitCount = 1;
              _Hits[0] = 0;
              _Hits[1] = 0;
              _Hits[2] = 2;
            }

            float distq(float2 a, float2 b){
                float area_of_effect_size =  1.0f;
                float d = pow(max(0.0, 1.0 - distance(a, b)/ area_of_effect_size), 2);
                return d;
            }

            float3 getHeatForPixel(float weight) 
            {
                if(weight <= pointranges[0]) 
                {
                    return colors[0];
                }
                if (weight >= pointranges[4])
                {
                    return colors[4];
                }

                for (int i=1; i<5; i++)
                {
                    if(weight < pointranges[i]) 
                    {
                        float dist_from_lower_point = weight - pointranges[i-1];
                        float size_of_point_range = pointranges[i] - pointranges[i-1];
                        float ratio_over_lower_point = dist_from_lower_point / size_of_point_range;
                        
                        float3 color_range = colors[i] - colors[i-1];
                        float3 color_contribution = color_range * ratio_over_lower_point;
                        float3 new_color = colors[i-1] + color_contribution;

                        return new_color;
                    }
                }
                return colors[0];
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv;
                uv = uv * 4.0 - float2(2.0,2.0);  //our texture uv range is -2 to 2
                float totalWeight = 0;
                for(float i = 0; i < _HitCount; i++){
                    float2 work_pt = float2(_Hits[i * 3 + 0], _Hits[i * 3 + 1]);
                    float pt_intensity = _Hits[i * 3 + 2];

                    totalWeight += 0.5 * distq(uv, work_pt) * pt_intensity;
                }

                float3 heat = getHeatForPixel(totalWeight);

                return col + float4(heat, 0.5);
            }
            ENDCG
        }
    }
}
