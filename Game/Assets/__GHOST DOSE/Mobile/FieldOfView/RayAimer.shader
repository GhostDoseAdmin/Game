Shader "Custom/RayAimer" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Emission("Emission", Range(0,1)) = 1
        _Alpha("Alpha", Range(0,1)) = 1


    }
        SubShader{
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 200
            //Cull Off 

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma surface surf Standard fullforwardshadows alpha:fade
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
                float3 worldNormal;

            };

            half _Glossiness;
            half _Metallic;
            half _Emission;
            half _Alpha;
            fixed4 _Color;




            void surf(Input IN, inout SurfaceOutputStandard o) {

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                    o.Albedo = c.rgb;
                    o.Emission = c.rgb * c.a * _Emission;
                    o.Metallic = _Metallic;
                    o.Smoothness = _Glossiness;
                    o.Alpha =  _Alpha;

                }

                ENDCG
        }
            FallBack "Diffuse"
}
