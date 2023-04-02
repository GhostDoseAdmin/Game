

Shader "Custom/Ghost" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
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
            float _MaxDistance;
            int _LightCount;
            float4 _LightPositions[10]; // Array of light positions
            float4 _LightDirections[10]; // Array of light directions
            float _LightAngles[10]; // Array of light angles
            float _StrengthScalarLight[10]; // Array of light strength scalars

            void surf(Input IN, inout SurfaceOutputStandard o) {

                // Loop through the light sources
                float minStrength = 1;
                float alphaStrength = 1;
                float _strength[10];

                for (int i = 0; i < _LightCount; i++) {
                    float3 direction = normalize(_LightPositions[i] - IN.worldPos);
                    float distance = length(IN.worldPos - _LightPositions[i]);
                    float scale = dot(direction, _LightDirections[i]);
                    float strength = scale - cos(_LightAngles[i] * (3.14 / 360.0));
                    _strength[i] = abs(1 - min(max(strength * _StrengthScalarLight[i], 0), 1));

                    minStrength = min(minStrength, _strength[i]);
                    alphaStrength *= _strength[i];
                }
                float strength = minStrength;

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Emission = c.rgb * c.a * strength;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;

                // The alpha value is determined by the product of both light strengths
                o.Alpha = (1-alphaStrength) * c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
