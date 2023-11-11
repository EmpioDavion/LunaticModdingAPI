Shader "Lunatic/Inverted" {
	Properties{
		_MainTex("MainTex", 2D) = "white" {}
		_UNLIT_F("UNLIT_F", Float) = 0
		_Reflection("Reflection", Cube) = "_Skybox" {}
		_Shine("Shine", Range(0, 1)) = 0
		[MaterialToggle] _Clip("Clip", Float) = 1
		_Emission("Emission", 2D) = "white" {}
		[HideInInspector] _Cutoff("Alpha cutoff", Range(0, 1)) = 0.5
	}
		//DummyShaderTextExporter
	SubShader
	{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM

			#pragma surface surf Standard
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = 1.0 - c.rgb;
				o.Alpha = c.a;
			}

			ENDCG
	}

	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}