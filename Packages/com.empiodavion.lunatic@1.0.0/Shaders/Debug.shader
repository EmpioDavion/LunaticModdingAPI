Shader "Unlit/Debug"
{
    Properties
    {
        [Enum(Normal, 0, Tangent, 1, UV, 2)] _Mode ("Mode", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
            };

            int _Mode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                o.tangent = v.tangent.xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = 1.0;

                switch (_Mode)
                {
                case 0: col.rgb = i.normal * 0.5 + 0.5; break;
                case 1: col.rgb = i.tangent * 0.5 + 0.5; break;
                case 2: col.rgb = float3(i.uv, 0); break;
                default: break;
                }

                return col;
            }

            ENDCG
        }
    }
}
