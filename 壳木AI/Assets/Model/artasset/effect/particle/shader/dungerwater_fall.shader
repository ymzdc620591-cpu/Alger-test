Shader "Particle/Shader_Forge/dungerwaterfall" {
	Properties {
		_main ("main", 2D) = "white" {}
		_wave1 ("wave1", 2D) = "white" {}
		_wave2 ("wave2", 2D) = "white" {}
		_niuqu ("niuqu", 2D) = "white" {}
		_node_7599 ("node_7599", Vector) = (0.5,0.5,0.5,1)
		_node_6088 ("node_6088", 2D) = "white" {}
		_blue_speed ("blue_speed", Range(0, 3)) = 0.5553657
		_red_speed ("red_speed", Range(0, 3)) = 0.5553657
		_niuqu_speed ("niuqu_speed", Range(0, 3)) = 0
		_green_speed ("green_speed", Range(0, 3)) = 0.2813658
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
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