Shader "Custom/Shadower" {
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
            #include "UnityCG.cginc"
            #pragma surface surf Standard fullforwardshadows alpha:fade
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
            float4 _LightPositions[20];
            float4 _LightDirections[20];
            float _LightAngles[20];
            float _StrengthScalarLight[20];

            // Add these lines for shadows
            float4x4 _ShadowMatrix;
            sampler2D _ShadowTex;
            float _ShadowBias;

            void surf(Input IN, inout SurfaceOutputStandard o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                float minStrength = 1;
                float alphaStrength = 1;
                float _strength[10];

                for (int i = 0; i < _LightCount; i++) {
                    float3 direction = normalize(_LightPositions[i] - IN.worldPos);
                    float distance = length(IN.worldPos - _LightPositions[i]);
                    float scale = dot(direction, _LightDirections[i]);
                    float strength = scale - cos(_LightAngles[i] * (3.14 / 360.0));
                    _strength[i] = abs(1 - min(max(strength * _StrengthScalarLight[i], 0), 1));

                    // Add these lines for shadows
                    float4 shadowCoords = mul(_ShadowMatrix, float4(IN.worldPos, 1.0));
                    float lightDepth = 1.0 - tex2Dproj(_ShadowTex, shadowCoords).r;
                    float shadow = (shadowCoords.z - _ShadowBias) < lightDepth ? 1.0 : 0.0;

                    _strength[i] = 1 - (shadow * (1 - _strength[i]));


                    minStrength = min(minStrength, _strength[i]);  
                    alphaStrength *= _strength[i];
                }

                float strength = minStrength;

                o.Albedo = c.rgb;
                o.Emission = c.rgb * c.a * strength;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = alphaStrength * c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
