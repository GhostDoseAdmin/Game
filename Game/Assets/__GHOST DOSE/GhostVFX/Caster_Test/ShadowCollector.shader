Shader "Custom/ShadowCollector" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma surface surf Lambert fullforwardshadows 
			#pragma target 3.0

			sampler2D _ShadowTex1;
			sampler2D _ShadowTex2;
			float4x4 _ShadowMatrix1;
			float4x4 _ShadowMatrix2;
			float _NumShadowMaps;

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
			};


			void surf(Input IN, inout SurfaceOutput o)
			{

				// MAP 1
				float4 shadowCoords1 = mul(_ShadowMatrix1, float4(IN.worldPos, 1.0));
				float lightDepth1 = 1.0 - tex2Dproj(_ShadowTex1, shadowCoords1).r;
				float shadow1 = (shadowCoords1.z - 0.005) < lightDepth1 ? 1.0 : 0.0;

				// MAP 2
				float4 shadowCoords2 = mul(_ShadowMatrix2, float4(IN.worldPos, 1.0));
				float lightDepth2 = 1.0 - tex2Dproj(_ShadowTex2, shadowCoords2).r;
				float shadow2 = (shadowCoords2.z - 0.005) < lightDepth2 ? 1.0 : 0.0;

				float4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb * shadow1 *shadow2;
				o.Alpha = c.a;
			}
			ENDCG
	}
		FallBack "Diffuse"
}
