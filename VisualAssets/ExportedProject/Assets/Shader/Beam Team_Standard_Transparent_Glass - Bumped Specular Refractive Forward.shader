Shader "Beam Team/Standard/Transparent/Glass - Bumped Specular Refractive Forward" {
	Properties {
		_Color ("Glass Color (RGB)", Vector) = (1,1,1,1)
		_NormalStrength ("Normal Map Strength", Range(0, 128)) = 64
		_Abberation ("Abberation", Float) = 0.0015
		_MainTex ("Diffuse Albedo (NOT IN USE)", 2D) = "white" {}
		_SpecTex ("Specular Albedo (RGB) Gloss (A)", 2D) = "white" {}
		_BumpTex ("Normal Map", 2D) = "bump" {}
		_HighlightRimColor ("Highlight Rim Color", Vector) = (0,0,0,0)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "VertexLit"
}