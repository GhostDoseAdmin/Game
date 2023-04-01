Shader "Custom/FlashLightHide" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0

        _LightDirectionPlayer("Light Direction 1", Vector) = (0,0,1,0)
        _LightPositionPlayer("Light Position 1", Vector) = (0,0,0,0)
        _LightAnglePlayer("Light Angle 1", Range(0,180)) = 45

        _LightDirectionClient("Light Direction 2", Vector) = (0,0,1,0)
        _LightPositionClient("Light Position 2", Vector) = (0,0,0,0)
        _LightAngleClient("Light Angle 2", Range(0,180)) = 45

        _LightDirectionEnv("Light Direction 2", Vector) = (0,0,1,0)
        _LightPositionEnv("Light Position 2", Vector) = (0,0,0,0)
        _LightAngleEnv("Light Angle 2", Range(0,180)) = 45

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
            float4 _LightPositionPlayer;
            float4 _LightPositionClient;
            float4 _LightPositionEnv;
            float4 _LightDirectionPlayer;
            float4 _LightDirectionClient;
            float4 _LightDirectionEnv;
            float _LightAnglePlayer;
            float _LightAngleClient;
            float _LightAngleEnv;
            float _MaxDistance;

            void surf(Input IN, inout SurfaceOutputStandard o) {
                float _StrengthScalarFlashlight = 20;//strength of invisibility

                float3 direction1 = normalize(_LightPositionPlayer - IN.worldPos);
                float distance1 = length(IN.worldPos - _LightPositionPlayer);
                float scale1 = dot(direction1, _LightDirectionPlayer);
                float strength1 = scale1 - cos(_LightAnglePlayer * (3.14 / 360.0));
                strength1 = abs(1 - min(max(strength1 * _StrengthScalarFlashlight, 0), 1));

                float3 direction2 = normalize(_LightPositionClient - IN.worldPos);
                float distance2 = length(IN.worldPos - _LightPositionClient);
                float scale2 = dot(direction2, _LightDirectionClient);
                float strength2 = scale2 - cos(_LightAngleClient * (3.14 / 360.0));
                strength2 = abs(1 - min(max(strength2 * _StrengthScalarFlashlight, 0), 1));

                float3 direction3 = normalize(_LightPositionEnv - IN.worldPos);
                float distance3 = length(IN.worldPos - _LightPositionEnv);
                float scale3 = dot(direction3, _LightDirectionEnv);
                float strength3 = scale3 - cos((_LightAngleEnv) * (3.14 / 360.0));
                strength3 = abs(1 - min(max(strength3 * _StrengthScalarFlashlight, 0), 1));


                float strength = min(min(strength1, strength2), strength3);


                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Emission = c.rgb * c.a * strength;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;

                // The alpha value is determined by the product of both light strengths
                o.Alpha = strength1 * strength2 * strength3 * c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
