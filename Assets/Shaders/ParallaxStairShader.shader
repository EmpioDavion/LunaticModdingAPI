Shader "Lunatic/Parallax Stair"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _SpecColor("Spec Color", Color) = (1,1,1,1)
        _Emission("Emissive Color", Color) = (0,0,0,0)
        [PowerSlider(5.0)] _Shininess("Shininess", Range(0.01, 1)) = 0.7
        _MainTex("Base (RGB)", 2D) = "white" {}
        _NormalTex("Normal Tex", 2D) = "bump" {}
        [NoScaleOffset] _HeightTex("Height Tex", 2D) = "white" {}
        _Height("Height", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        CGINCLUDE

        fixed4 _Color;
        fixed4 _SpecColor;
        fixed4 _Emission;
        fixed _Shininess;

        sampler2D _MainTex;
        sampler2D _NormalTex;

        float4 _MainTex_ST;

        sampler2D _HeightTex;
        float _Height;

        float2 Line2DIntersection(float2 a1, float2 a2, float2 b1, float2 b2)
        {
            float2 r = a2 - a1;
            float2 s = b2 - b1;

            float2 p1 = b1 - a1;
            float num = p1.x * r.y - p1.y * r.x;
            float den = r.x * s.y - r.y * s.x;

            float u = num / den;

            return b1 + s * u;
        }

        float2 ParallaxUV(float2 uv, float3 tangentViewDir)
        {
            const int STEPS = 8;

            tangentViewDir = normalize(tangentViewDir);
            float2 dir = tangentViewDir.xy / (tangentViewDir.z + 0.42) * _Height;
            float3 dirStep = float3(dir, 1.0) / STEPS;
            float3 pos = float3(uv, 1.0);
            float3 lastPos = pos;
            float lastHeight = 1.0;

            for (int i = 0; i < STEPS; i++)
            {
                float height = tex2Dlod(_HeightTex, float4(pos.xy, 0, 0)).r;

                if (pos.z <= height)
                {
                    float prevDiff = lastPos.z - lastHeight;
                    float diff = height - pos.z;
                    float t = prevDiff / (prevDiff + diff);

                    return lerp(lastPos.xy, pos.xy, t);
                }

                lastPos = pos;
                pos -= dirStep;
                lastHeight = height;
            }

            return pos.xy;
        }

        ENDCG

        // Non-lightmapped
        Pass {
            Tags { "LightMode" = "Vertex" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdadd

            #include "UnityCG.cginc"
        
            struct VertexInput
            {
                float4 positionOS : POSITION;
                float3 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float3 viewDirTS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3x3 GetTBN(float3 normal, float4 tangent)
            {
                float3 binormal = cross(normal, tangent.xyz) * tangent.w;
                return float3x3(tangent.xyz, binormal, normal);
            }

            FragmentInput vert(VertexInput i)
            {
                FragmentInput o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.texcoord = i.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;

                float4 tangent = float4(1, 0, 0, 1);// i.tangentOS.xyz;// normalize(cross(i.normalOS, float3(0, 1, 0)));
                tangent = i.tangentOS;// normalize(cross(i.normalOS, float3(0, 1, 0)));
                float3x3 tbn = GetTBN(i.normalOS, tangent);

                o.viewDirTS = mul(tbn, ObjSpaceViewDir(i.positionOS));
                o.normalWS = mul((float3x3)unity_ObjectToWorld, i.normalOS);
                o.tangentWS = float4(mul((float3x3)unity_ObjectToWorld, i.tangentOS.xyz), i.tangentOS.w);
                o.positionCS = UnityObjectToClipPos(i.positionOS);
                o.positionWS = mul(unity_ObjectToWorld, i.positionOS).xyz;

                return o;
            }

            fixed4 frag(FragmentInput i) : SV_Target
            {
                fixed2 uv = ParallaxUV(i.texcoord, i.viewDirTS);
                fixed4 tex = tex2D(_MainTex, uv);
                fixed4 col = _Color * tex;

                float3x3 tbn = GetTBN(i.normalWS, i.tangentWS);
                float3 normal = UnpackNormal(tex2D(_NormalTex, uv));

                normal = mul(tbn, normal);

                float3 diffuse = 0.0;
                float spec = 0.0;

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

                for (int j = 0; j < 4; j++)
                {
                    float3 lightPosWS = mul(UNITY_MATRIX_I_V, unity_LightPosition[0]);
                    float3 dist = lightPosWS - i.positionWS;
                    float3 lightDir = normalize(dist);
                    float3 halfDir = normalize(lightDir + viewDir);

                    float ndl = saturate(dot(normal, lightDir));
                    float ndh = saturate(dot(normal, halfDir));
                    float distance = dot(dist, dist);
                    float atten = 1.0 / (1.0 + distance * unity_LightAtten[j].z);

                    diffuse += col.rgb * unity_LightColor[j].rgb * atten;
                    spec += pow(ndh, _Shininess * 128.0);
                }

                col.rgb = (col.rgb + spec) * unity_LightColor0.rgb + col.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb;

                col.a = 1.0f;

                return col;
            }

            ENDCG
        }

        // Lightmapped
        Pass
        {
            Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            #pragma multi_compile_fog
            #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

            float4 unity_Lightmap_ST;

            struct appdata
            {
                float4 pos : POSITION;
                float3 uv1 : TEXCOORD1;
                float3 uv0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 tangentViewDir : TEXCOORD4;
    #if USING_FOG
                fixed fog : TEXCOORD5;
    #endif
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata IN)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.uv1 = IN.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
                o.uv2 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

    #if USING_FOG
                float3 eyePos = UnityObjectToViewPos(IN.pos);
                float fogCoord = length(eyePos.xyz);
                UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
                o.fog = saturate(unityFogFactor);
    #endif

                // stairs have weird tangents
                float3 tangent = float3(1, 0, 0);// normalize(cross(IN.normal, float3(0, 1, 0)));
                float3 binormal = cross(IN.normal, tangent.xyz) * IN.tangent.w;
                float3x3 tbn = float3x3(tangent.xyz, binormal, IN.normal);

                o.tangentViewDir = mul(tbn, ObjSpaceViewDir(IN.pos));
                o.normal = mul((float3x3)unity_ObjectToWorld, IN.normal);
                o.pos = UnityObjectToClipPos(IN.pos);

                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col;

                fixed4 tex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
                half4 bakedColor = half4(DecodeLightmap(tex), 1.0);

                col = bakedColor * _Color;

                float2 uv = ParallaxUV(IN.uv2, IN.tangentViewDir);
                tex = tex2D(_MainTex, uv);
                col.rgb = tex.rgb * col.rgb;

                col.a = 1.0f;

                #if USING_FOG
                    col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
                #endif

                return col;
            }

            ENDCG
        }

        // Pass to render object as a shadow caster
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0
    #pragma multi_compile_shadowcaster
    #pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
    #include "UnityCG.cginc"

    struct v2f {
        V2F_SHADOW_CASTER;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2f vert(appdata_base v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
        return o;
    }

    float4 frag(v2f i) : SV_Target
    {
        SHADOW_CASTER_FRAGMENT(i)
    }
    ENDCG

        }

    }

}