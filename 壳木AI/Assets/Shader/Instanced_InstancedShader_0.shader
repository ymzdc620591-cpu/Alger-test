Shader "Instanced/InstancedShader" {
	Properties {
		_OriginPos ("_OriginPos", Vector) = (0,0,0,0)
		_sliceParameter ("_sliceParameter", Vector) = (0,0,0,0)
		_ColorMap ("_ColorMap", 2D) = "white" {}
		_HeightParam ("_HeightParam", Vector) = (0,0,0,0)
		_HsvGain ("_HsvGain", Vector) = (1,1,1,1)
		_DensityTex_0 ("_DensityTex_0", 2D) = "white" {}
		[NoScaleOffset] _WindTex ("_WindTex", 2D) = "white" {}
		_WindSpeed ("_WindSpeed", Float) = 2
		_WindSize ("_WindSize", Float) = 5
		Vector1_607785E7 ("normalOffset", Range(-1, 1)) = 0
		_BaseColor ("_BaseColor", Vector) = (0.07637937,0.5377358,0.0228284,0)
		_Color ("_Color", Vector) = (0.05,0.2,0.03,1)
		_ColorScale ("_ColorScale", Float) = 0.05
		_ColorOffset ("_ColorOffset", Float) = -0.02
		_PlayerPostion ("Player Position (xyz), Radius (w)", Vector) = (0,0,0,0)
		_cullDistance ("Cull Distance", Range(0, 100)) = 30
		_Height ("Height", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows vertex:vert addshadow
		#pragma target 3.0
		#pragma multi_compile_instancing

		sampler2D _ColorMap;
		sampler2D _WindTex;
		half4 _BaseColor;
		half4 _Color;
		float4 _PlayerPostion;
		float _WindSpeed;
		float _WindSize;
		float _Height;
		float _ColorScale;
		float _ColorOffset;

		struct Input {
			float2 uv_ColorMap;
			float heightLerp;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float bladeHeight = saturate(v.texcoord.y);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			float windSize = max(_WindSize, 0.001);
			float2 windUv0 = worldPos.xz / windSize + _Time.y * _WindSpeed * float2(0.08, 0.05);
			float2 windUv1 = worldPos.xz / (windSize * 0.55) - _Time.y * _WindSpeed * float2(0.03, 0.09);

			float windPrimary = tex2Dlod(_WindTex, float4(windUv0, 0, 0)).r * 2.0 - 1.0;
			float windSecondary = tex2Dlod(_WindTex, float4(windUv1, 0, 0)).g * 2.0 - 1.0;
			float windAmount = (windPrimary * 0.7 + windSecondary * 0.3) * bladeHeight;

			float3 windDir = normalize(float3(0.85 + windSecondary * 0.25, 0.0, 0.55 - windPrimary * 0.25));
			worldPos.xz += windDir.xz * windAmount * (_Height * 0.12);

			float2 toPlayer = worldPos.xz - _PlayerPostion.xz;
			float playerRadius = max(_PlayerPostion.w, 0.001);
			float playerDistance = length(toPlayer);
			float playerMask = saturate(1.0 - playerDistance / playerRadius);
			playerMask = playerMask * playerMask * (3.0 - 2.0 * playerMask);

			float2 pushDir = playerDistance > 0.0001 ? (toPlayer / playerDistance) : float2(0.0, 1.0);
			float bendStrength = playerMask * bladeHeight;
			worldPos.xz += pushDir * bendStrength * (_Height * 0.22);
			worldPos.y -= bendStrength * (_Height * 0.18);

			v.vertex = mul(unity_WorldToObject, float4(worldPos, 1.0));
			o.heightLerp = bladeHeight;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			fixed3 texColor = tex2D(_ColorMap, IN.uv_ColorMap).rgb;
			fixed3 rootColor = saturate(_Color.rgb + _ColorOffset);
			fixed3 tipColor = saturate(_BaseColor.rgb * (1.0 + _ColorScale));
			fixed heightLerp = saturate(IN.heightLerp);
			o.Albedo = texColor * lerp(rootColor, tipColor, heightLerp);
			o.Alpha = 1.0;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
