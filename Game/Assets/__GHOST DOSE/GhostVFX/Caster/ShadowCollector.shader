Shader "Custom/ShadowCollectorStandard" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
	    //_ShadowValue("Shadow Value", Range(0, 1)) = 0
	}
		SubShader{ 
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma target 3.0

			sampler2D _ShadowTex;
			float4x4 _ShadowMatrix;
			float _ShadowBias;

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float4 shadowCoords;
			};

			half _Metallic;
			half _Glossiness;
			float _ShadowValue;

			void vert(inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input, o);
				float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
				o.shadowCoords = mul(_ShadowMatrix, float4(worldPos, 1.0));
			}

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				float lightDepth = 1.0 - tex2Dproj(_ShadowTex, IN.shadowCoords).r;
				float shadow = (IN.shadowCoords.z - _ShadowBias) < lightDepth ? 1.0 : 0.5;
				//shadow = _ShadowValue;

				float4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb *shadow;
				o.Alpha = c.a;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;


			}
			ENDCG
		}
			FallBack "Diffuse"
}
