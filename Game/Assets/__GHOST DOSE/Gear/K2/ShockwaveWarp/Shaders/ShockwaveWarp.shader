Shader "Custom/ShockwaveWarp"
{
	Properties 
	{
		[CustomInspector(RENDERMODE_0100)]RENDERMODE("RENDERMODE", Float) = 0

		[CustomInspector(Alpha Cutoff, RENDERMODE)]_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

		[Header(Main Maps)]

		[CustomInspector(The primary texture.)]_MainTex("Main Texture", 2D) = "white" {}
		
		[CustomInspector(Use equitangular UVs (better for spheres)., _, _, false, true)]_EQUITANGULAR("Equitangular Normals", Float) = 0
		[CustomInspector(Main texture tint color.)]_Color("Tint Color", Color) = (1, 1, 1, 1)


		[Toggle(_NORMAL_MAP)]
		_NORMAL_MAP("Enable Normal Map", Float) = 1
		[CustomInspector(Normal map texture., _NORMAL_MAP, _, false)] _BumpMap("Normal Texture", 2D) = "bump" {}
		[CustomInspector(The scale of bumpiness from the Normal Map., _NORMAL_MAP)]_BumpScale("Bump Scale", Float) = 1

		[Space]
		[Toggle(_METALLIC_MAP)]
		_METALLIC_MAP("Enable Metallic Map", Float) = 1
		[CustomInspector(Metallic Map texture., _METALLIC_MAP, _, false)] _MetallicGlossMap("Metallic Texture", 2D) = "white" {}
		[CustomInspector(Controls how metallic the texture appears.)][Gamma] _Metallic("Metallic Scale", Range(0, 1)) = 0
		[CustomInspector(Controls how smooth the texture appears.)]_Glossiness("Smoothness", Range(0, 1)) = 0.1

		[Toggle(_OCCLUSION_MAP)]
		_OCCLUSION_MAP("Enable Oclusion Map", Float) = 1
		[CustomInspector(Occlusion Map texture. Controls which areas of the texture receive high or low lighting., _OCCLUSION_MAP, _, false)][NoScaleOffset] _OcclusionMap("Occlusion Texure", 2D) = "white" {}
		[CustomInspector(Occlusion Map texture strength., _OCCLUSION_MAP)]_OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1

		[Space]
		[Toggle(_EmissionColor_MAP)]
		_EmissionColor_MAP("Enable Emmission Map", Float) = 1
		[CustomInspector(Emission Map texture., _EmissionColor_MAP, _, false)] _EmissionColorMap("Emission Texture", 2D) = "black" {}
		[CustomInspector(Tint color of Emission Texture.)]_EmissionColor("Emission Tint Color", Color) = (0, 0, 0)

		[Space]
		[Toggle(_FADE_ON)]
		_FADE_ON("Enable Fade Texture", Float) = 1
		[CustomInspector(Texture that fade effect is based on., _FADE_ON)]_FadeTex("Fade Texture", 2D) = "white" {}
		[CustomInspector(Fade amount adjustment.)]_Fade("Fade Amount", Range(0,1)) = 1.0

		[Header(Secondary Maps)]
		[Space]
		[Toggle(_DETAIL_ALBEDO_MAP)]
		_DETAIL_ALBEDO_MAP("Enable Detail Texture", Float) = 1
		[CustomInspector(Secondary detail texture overlayed on main texture., _DETAIL_ALBEDO_MAP)]_DetailAlbedoMap("Detail Texture", 2D) = "gray" {}
		[CustomInspector(Detail texture tint color., _DETAIL_ALBEDO_MAP)]_DetailColor("Detail Tint Color", Color) = (1, 1, 1, 1)

		[Space]
		[Toggle(_DETAIL_NORMAL_MAP)]
		_DETAIL_NORMAL_MAP("Enable Detail Normal Texture", Float) = 1
		[CustomInspector(Normal map for Detail Texture., _DETAIL_NORMAL_MAP, _, false)] _DetailNormalMap ("Detail Normal Texture", 2D) = "bump" {}
		[CustomInspector(The scale of bumpiness from the Detail Normal Texture., _DETAIL_NORMAL_MAP)]_DetailNormalMapScale ("Detail Bump Scale", Float) = 1

		[Space]
		[CustomInspector(Enable Detail mask., _DETAIL_ALBEDO_MAP _DETAIL_NORMAL_MAP, _, false, true)]_DETAIL_MASK("Enable Detail Mask", Float) = 1
		[CustomInspector(Mask texture that determines where the Detail texture shows (Alpha of texture)., _DETAIL_MASK, _DETAIL_ALBEDO_MAP _DETAIL_NORMAL_MAP, false)] _DetailMask("Detail Mask", 2D) = "white" {}

		[Header(Effects)]
		[Space]
		[CustomInspector(Local position that sine wave and shrink effect propagates from.)]_Origin("Origin Position", Vector) = (0,0,0,0)
		[CustomInspector(Adjust how much the object shrinks.)]_Shrink("Shrink Amount", Range(0,1)) = 1.0
		[CustomInspector(Amplitude of sine wave effect.)]_SineAmp("Sine Wave Amplitude", Float) = 0.1
		[CustomInspector(Frequency of sine wave effect.)]_SineFreq("Sine Wave Frequency", Float) = 1
		[CustomInspector(Offset of sine wave effect (controls progression of sine wave).)]_SineOffset("Sine Wave Offset", Float) = 0
		[CustomInspector(How many times the sine wave effect repeats (number of waves).)]_SineRepetition("Sine Wave Repetition", Float) = 1


		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
	}
	
	CGINCLUDE
	#define BINORMAL_PER_FRAGMENT
	ENDCG

	SubShader 
	{
		Pass 
		{
			Tags {"LightMode" = "ForwardBase"}
			Cull Off
			Blend [_SrcBlend] [_DstBlend]
			ZWrite on

			CGPROGRAM

			#pragma target 3.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _Glossiness_ALBEDO _Glossiness_METALLIC
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _OCCLUSION_MAP
			#pragma shader_feature _EmissionColor_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP
			#pragma shader_feature _DETAIL_NORMAL_MAP
			#pragma shader_feature _FADE_ON
			#pragma shader_feature _EQUITANGULAR

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile_instancing


			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#define FORWARD_BASE_PASS

			#include "ShockwaveLighting.cginc"

			ENDCG
		}

		Pass 
		{
			Tags {"LightMode" = "ForwardAdd"}
			Cull Off
			Blend [_SrcBlend] One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _Glossiness_ALBEDO _Glossiness_METALLIC
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP
			#pragma shader_feature _DETAIL_NORMAL_MAP

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#include "ShockwaveLighting.cginc"

			ENDCG
		}

		Pass 
		{
			Tags {"LightMode" = "ShadowCaster"}
			Cull Off
			CGPROGRAM

			#pragma target 3.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _SEMITRANSPARENT_SHADOWS
			#pragma shader_feature _Glossiness_ALBEDO

			#pragma multi_compile_shadowcaster

			#pragma vertex ShadowVertexProgram
			#pragma fragment ShadowFragmentProgram

			#include "ShockwaveShadows.cginc"

			ENDCG
		}
	}
}