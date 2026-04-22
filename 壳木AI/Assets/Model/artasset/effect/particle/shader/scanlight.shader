Shader "Particle/Shader_Forge/scanlight" {
	Properties {
		_text_Color ("text_Color", Vector) = (1,1,1,1)
		_text ("text", 2D) = "white" {}
		_light_text ("light_text", 2D) = "white" {}
		_light_point ("light_point", Range(0, 1)) = 1
		_light_color ("light_color", Vector) = (0.5,0.5,0.5,1)
		_light_rotation ("light_rotation", Range(0, 3.14)) = 3.14
		_v ("v", Range(-1, 1)) = 1
		_u ("u", Range(-1, 1)) = 0.8362315
		_emission_power ("emission_power", Range(0, 4)) = 1
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
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}