Shader "Custom/GhostAlphaCutoff" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _MaxDistance("Max Distance", Float) = 10
        _Emission("Emission", Range(0,1)) = 1
        _Alpha("Alpha", Range(0,1)) = 1
        _MinDistance("Minimum Distance", Range(0,2)) = 1.3
        _Shadower("Shadower", Range(0,1)) = 0
        _EMFAlpha("EmfAlpha", Range(0,1)) = 0

    }
        SubShader{
            Tags { "RenderType" = "Opaque"}
            //ZWrite Off
            //Cull Front
            LOD 200

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma surface surf Standard fullforwardshadows
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
            half _YOffset;
            half _Alpha;
            half _EMFAlpha;
            half _MinDistance;
            half _Shadower;
            fixed4 _Color;
            float _MaxDistance;
            //PLAYER LIGHT
            float4 _PlayerLightPosition;
            float4 _PlayerLightDirection;
            float _PlayerLightAngle;
            float _PlayerStrengthScalarLight;
            float _PlayerLightRange;

            //CLIENT LIGHT
            float4 _ClientLightPosition;
            float4 _ClientLightDirection;
            float _ClientLightAngle;
            float _ClientStrengthScalarLight;
            float _ClientLightRange;

            //ENVIRONMENT LIGHTS
            int _EnvLightCount;
            float4 _LightPositions[20];
            float4 _LightDirections[20];
            float _LightAngles[20];
            float _StrengthScalarLight[20];
            float _LightRanges[20];



            //SHADOW MAP DETAILS
            float4x4 _ShadowMatrix;
            sampler2D _ShadowTex;
            float _ShadowBias;
            

            void surf(Input IN, inout SurfaceOutputStandard o) {

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                float minStrength = 1;
                float alphaStrength = 1;
                float _strength[20];

                //ENVIRONMENT LIGHTS
                for (int i = 0; i < _EnvLightCount; i++) {
                    float3 direction = normalize(_LightPositions[i] - IN.worldPos);
                    float distance = length(IN.worldPos - _LightPositions[i]);
                    if (distance > _LightRanges[i]) {
                        continue; // move on to the next light
                    }
                    float scale = dot(direction, _LightDirections[i]);
                    float strength = scale - cos(_LightAngles[i] * (3.14 / 360.0));
                    _strength[i] = abs(1 - min(max(strength * _StrengthScalarLight[i], 0), 1));

                    //SAMPLE SHADOW MAP
                    float4 shadowCoords = mul(_ShadowMatrix, float4(IN.worldPos, 1.0));
                    float lightDepth = 1.0 - tex2Dproj(_ShadowTex, shadowCoords).r;
                    float shadow = (shadowCoords.z - 0.005) < lightDepth ? 1.0 : 0.0;

                    _strength[i] = 1 - (shadow * (1 - _strength[i]));


                    minStrength = min(minStrength, _strength[i]);
                    alphaStrength *= _strength[i];
                }

                float minStrengthPlayers = 1;
                float alphaStrengthPlayers = 1;
                //PLAYER LIGHTS
                float3 direction1 = normalize(_PlayerLightPosition - IN.worldPos);
                float distance1 = length(IN.worldPos - _PlayerLightPosition);
                float scale1 = dot(direction1, _PlayerLightDirection);
                float strength1 = scale1 - cos(_PlayerLightAngle * (3.14 / 360.0));
                strength1 = abs(1 - min(max(strength1 * _PlayerStrengthScalarLight, 0), 1));
                strength1 = 1 - (1 - strength1);
                if (distance1 > _PlayerLightRange && _Shadower==0) {
                    strength1 = 1;
                   // if (_Shadower==1) { strength1 = 0; }
                }
                if (distance1 < _MinDistance) {
                    strength1 = (distance1 - 1) * 0.1;
                }
                alphaStrengthPlayers *= strength1;

                //CLIENT LIGHTS
                float3 direction2 = normalize(_ClientLightPosition - IN.worldPos);
                float distance2 = length(IN.worldPos - _ClientLightPosition);
                float scale2 = dot(direction2, _ClientLightDirection);
                float strength2 = scale2 - cos(_ClientLightAngle * (3.14 / 360.0));
                strength2 = abs(1 - min(max(strength2 * _ClientStrengthScalarLight, 0), 1));
                strength2 = 1 - (1 - strength2);
                if (distance2 > _ClientLightRange && _Shadower == 0) {
                    strength2 = 1;
                   // if (_Shadower==1) { strength2 = 0; }
                }
                if (distance2 < _MinDistance) {
                    strength2 = (distance2 - 1) * 0.1;
                }
                alphaStrengthPlayers *= strength2;

                minStrengthPlayers = min(strength1, strength2);



                float strength = minStrength * minStrengthPlayers;

                //ALPHA CUTOFF -- only render pixels that are more visible
                float total_alpha = ((1 - (alphaStrength * alphaStrengthPlayers)) * c.a * _Alpha) +_EMFAlpha;; //
                if (_Shadower == 1) { total_alpha = alphaStrength * alphaStrengthPlayers * c.a * _Alpha; }

                if (total_alpha < 1) {//dont render the pixels
                    discard;
                    return;
                }

                if (_Shadower == 1) { c.rgb = 1.0 - c.rgb; }
                o.Albedo = c.rgb;
                o.Emission = c.rgb * c.a * _Emission;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = total_alpha;


            }
            ENDCG
        }
            FallBack "Diffuse"
}
