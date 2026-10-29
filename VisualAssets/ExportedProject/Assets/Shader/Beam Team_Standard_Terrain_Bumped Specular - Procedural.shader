Shader "Beam Team/Standard/Terrain/Bumped Specular - Procedural" {
	Properties {
		[Header(Noise Texture)] _NoiseTex ("Noise Texture", 2D) = "white" {}
		[Header(Diffuse Maps)] _Tex1 ("Texture 1 (RGB)", 2D) = "white" {}
		_Tex2 ("Texture 2 (RGB)", 2D) = "white" {}
		_Tex3 ("Texture 3 (RGB)", 2D) = "white" {}
		_Tex4 ("Texture 4 (RGB)", 2D) = "white" {}
		_Tex5 ("Texture 5 (RGB)", 2D) = "white" {}
		_Tex6 ("Texture 6 (RGB)", 2D) = "white" {}
		[Header(Packed Maps)] _Pack1 ("Packed 1", 2D) = "white" {}
		_Pack2 ("Packed 2", 2D) = "white" {}
		_Pack3 ("Packed 3", 2D) = "white" {}
		_Pack4 ("Packed 4", 2D) = "white" {}
		_Pack5 ("Packed 5", 2D) = "white" {}
		_Pack6 ("Packed 6", 2D) = "white" {}
		[Header(Blending)] _Blend1 ("Blend Height 1", Float) = 0
		_Blend2 ("Blend Height 2", Float) = 0
		_Blend3 ("Blend Height 3", Float) = 0
		_Blend4 ("Blend Height 4", Float) = 0
		_Blend5 ("Blend Height 5", Float) = 0
		_Blend6 ("Blend Height 6", Float) = 0
		[Header(Parallax)] _Height1 ("Height 1", Float) = 0.02
		_Height2 ("Height 2", Float) = 0.02
		_Height3 ("Height 3", Float) = 0.02
		_Height4 ("Height 4", Float) = 0.02
		[Header(Specular)] _Contrast1 ("Contrast 1", Float) = 1
		_Contrast2 ("Contrast 2", Float) = 1
		_Contrast3 ("Contrast 3", Float) = 1
		_Contrast4 ("Contrast 4", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Beam Team/Standard/Terrain/Vertex Lit"
}