Shader "Lunatic/VertexLit"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _SpecColor("Spec Color", Color) = (1,1,1,1)
        _Emission("Emissive Color", Color) = (0,0,0,0)
        [PowerSlider(5.0)] _Shininess("Shininess", Range(0.01, 1)) = 0.7
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        
        Pass
        {
            Tags { "LightMode" = "Vertex" }
            
            Lighting On

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                fixed4 vertex : POSITION;
                fixed3 normal : NORMAL;
                fixed2 uv : TEXCOORD0;
            };

            struct v2f
            {
                fixed4 vertex : SV_POSITION;
                fixed2 uv : TEXCOORD0;
                fixed3 lighting : COLOR0;
                fixed3 spec : COLOR1;
            };

            sampler2D _MainTex;
            fixed4 _MainTex_ST;

            fixed4 _Color;
            fixed4 _Emission;
            fixed _Shininess;

            #include "VertexLighting.cginc"

            v2f vert(appdata input)
            {
                v2f output;

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                GetVertexLighting(input.vertex, input.normal, output.lighting, output.spec);

                return output;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col;
                col.rgb = tex.rgb * i.lighting + i.spec * tex.a;
                col.a = tex.a * _Color.a;

                return col;
            }

            ENDCG
        }
    }
}
