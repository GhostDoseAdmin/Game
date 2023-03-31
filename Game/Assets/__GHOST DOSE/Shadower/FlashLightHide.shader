Shader "Custom/FlashLightHide" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0

        _LightDirection1("Light Direction 1", Vector) = (0,0,1,0)
        _LightPosition1("Light Position 1", Vector) = (0,0,0,0)
        _LightAngle1("Light Angle 1", Range(0,180)) = 45

        _LightDirection2("Light Direction 2", Vector) = (0,0,1,0)
        _LightPosition2("Light Position 2", Vector) = (0,0,0,0)
        _LightAngle2("Light Angle 2", Range(0,180)) = 45
        _StrengthScalar("Strength", Float) = 100
        _MaxDistance("Max Distance", Float) = 10
    }
        SubShader{
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows alpha:fade

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            float4 _LightPosition1;
            float4 _LightPosition2;
            float4 _LightDirection1;
            float4 _LightDirection2;
            float _LightAngle1;
            float _LightAngle2;
            float _StrengthScalar;
            float _MaxDistance;

            void surf(Input IN, inout SurfaceOutputStandard o) {
                float3 direction1 = normalize(_LightPosition1 - IN.worldPos);
                float distance1 = length(IN.worldPos - _LightPosition1);
                float scale1 = dot(direction1, _LightDirection1);
                float strength1 = scale1 - cos(_LightAngle1 * (3.14 / 360.0));
                strength1 = abs(1 - min(max(strength1 * _StrengthScalar, 0), 1));

                float3 direction2 = normalize(_LightPosition2 - IN.worldPos);
                float distance2 = length(IN.worldPos - _LightPosition2);
                float scale2 = dot(direction2, _LightDirection2);
                float strength2 = scale2 - cos(_LightAngle2 * (3.14 / 360.0));
                strength2 = abs(1 - min(max(strength2 * _StrengthScalar, 0), 1));

                float strength = min(strength1, strength2);

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Emission = c.rgb * c.a * strength;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;

                // The alpha value is determined by the product of both light strengths
                o.Alpha = strength1 * strength2 * c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
