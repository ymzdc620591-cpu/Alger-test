Shader "Wonderm/Plant/TreeLeaves_Stylized_Builtin"
{
    Properties
    {
        [Header(Base)]
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.45

        [Header(Shading)]
        _TopColor ("Top Tint", Color) = (1.08,1.08,0.98,1)
        _BottomColor ("Bottom Tint", Color) = (0.84,0.92,0.84,1)
        _ShadowColor ("Shadow Tint", Color) = (0.58,0.72,0.58,1)
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 1.0
        _DirectStrength ("Direct Strength", Range(0, 2)) = 0.9
        _ShadeThreshold ("Shade Threshold", Range(0, 1)) = 0.5
        _ShadeSoftness ("Shade Softness", Range(0.001, 0.5)) = 0.12
        _NormalUpBlend ("Normal Up Blend", Range(0, 1)) = 0.35
        _VerticalTintPower ("Vertical Tint Power", Range(0.1, 4)) = 1.2

        [Header(Back Light)]
        _BackLightColor ("Back Light Color", Color) = (0.7,0.95,0.55,1)
        _BackLightStrength ("Back Light Strength", Range(0, 2)) = 0.35
        _RimPower ("Back Light Rim Power", Range(0.5, 8)) = 2.5

        [Header(Wind)]
        _WindDirection ("Wind Direction", Vector) = (1,0,0,0)
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.2
        _WindStrength ("Wind Swing", Range(0, 1)) = 0.08
        _FlutterStrength ("Wind Flutter", Range(0, 1)) = 0.03
        _WindScale ("Wind World Scale", Range(0.01, 4)) = 0.35
        _WindMaskPower ("Wind Mask Power", Range(0.1, 4)) = 1.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
            "IgnoreProjector" = "True"
        }

        LOD 250
        Cull Off

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _ShadowColor;
            fixed4 _BackLightColor;
            half _Cutoff;
            half _AmbientStrength;
            half _DirectStrength;
            half _ShadeThreshold;
            half _ShadeSoftness;
            half _NormalUpBlend;
            half _VerticalTintPower;
            half _BackLightStrength;
            half _RimPower;
            float4 _WindDirection;
            half _WindSpeed;
            half _WindStrength;
            half _FlutterStrength;
            half _WindScale;
            half _WindMaskPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                LIGHTING_COORDS(4, 5)
            };

            float3 ApplyWind(float3 localPos, float3 normalOS, float2 uv, float3 worldPos)
            {
                float2 windDir = _WindDirection.xz;
                float windDirLen = max(length(windDir), 0.0001);
                windDir /= windDirLen;

                float mask = saturate(pow(saturate(uv.y), _WindMaskPower));
                float phase = _Time.y * _WindSpeed + dot(worldPos.xz, windDir) * _WindScale;
                float sway = sin(phase) * 0.65 + cos(phase * 1.37) * 0.35;
                float flutter = sin(phase * 2.41 + worldPos.y * 1.73) * 0.5;

                localPos.xz += windDir * sway * (_WindStrength * mask);
                localPos.xyz += normalOS * (flutter * _FlutterStrength * mask);
                return localPos;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 localPos = ApplyWind(v.vertex.xyz, v.normal, v.uv, worldPos);

                float4 finalVertex = float4(localPos, 1.0);
                o.pos = UnityObjectToClipPos(finalVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, finalVertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                UNITY_TRANSFER_FOG(o, o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * _MainColor;
                clip(tex.a - _Cutoff);

                half3 normalWS = normalize(i.worldNormal);
                normalWS *= (facing >= 0 ? 1.0h : -1.0h);
                normalWS = normalize(lerp(normalWS, half3(0, 1, 0), _NormalUpBlend));

                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                half3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));

                half atten = LIGHT_ATTENUATION(i);
                half halfLambert = saturate(dot(normalWS, lightDir) * 0.5h + 0.5h);
                half shade = smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness, halfLambert * atten);

                half verticalMask = saturate(pow(saturate(i.uv.y), _VerticalTintPower));
                half3 verticalTint = lerp(_BottomColor.rgb, _TopColor.rgb, verticalMask);

                half3 ambient = ShadeSH9(half4(normalWS, 1.0h)).rgb * _AmbientStrength;
                half3 direct = lerp(_ShadowColor.rgb, _LightColor0.rgb, shade) * _DirectStrength;

                half backScatter = saturate(dot(-normalWS, lightDir));
                half rim = pow(saturate(1.0h - dot(normalWS, viewDir)), _RimPower);
                half3 backLight = _BackLightColor.rgb * backScatter * rim * _BackLightStrength;

                half3 color = tex.rgb * verticalTint * (ambient + direct) + backLight * tex.rgb;

                UNITY_APPLY_FOG(i.fogCoord, color);
                return half4(color, 1.0h);
            }
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;
            half _Cutoff;
            float4 _WindDirection;
            half _WindSpeed;
            half _WindStrength;
            half _FlutterStrength;
            half _WindScale;
            half _WindMaskPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            float3 ApplyWind(float3 localPos, float3 normalOS, float2 uv, float3 worldPos)
            {
                float2 windDir = _WindDirection.xz;
                float windDirLen = max(length(windDir), 0.0001);
                windDir /= windDirLen;

                float mask = saturate(pow(saturate(uv.y), _WindMaskPower));
                float phase = _Time.y * _WindSpeed + dot(worldPos.xz, windDir) * _WindScale;
                float sway = sin(phase) * 0.65 + cos(phase * 1.37) * 0.35;
                float flutter = sin(phase * 2.41 + worldPos.y * 1.73) * 0.5;

                localPos.xz += windDir * sway * (_WindStrength * mask);
                localPos.xyz += normalOS * (flutter * _FlutterStrength * mask);
                return localPos;
            }

            v2f vertShadow(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                v.vertex.xyz = ApplyWind(v.vertex.xyz, v.normal, v.uv, worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 fragShadow(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * _MainColor;
                clip(tex.a - _Cutoff);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Transparent/Cutout/VertexLit"
}
