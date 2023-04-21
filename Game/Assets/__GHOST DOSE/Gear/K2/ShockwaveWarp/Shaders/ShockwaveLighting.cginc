#if !defined(SHOCKWAVELIGHTING_INCLUDED)
#define SHOCKWAVELIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

sampler2D _MainTex, _FadeTex, _DetailAlbedoMap, _DetailMask;
float4 _MainTex_ST, _FadeTex_ST, _DetailAlbedoMap_ST;
sampler2D _BumpMap, _DetailNormalMap;
sampler2D _MetallicGlossMap;
sampler2D _OcclusionMap;
sampler2D _EmissionColorMap;
uniform sampler2D _CameraDepthTexture;

static const float _Pi = 3.1415926;

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _DetailColor)
UNITY_DEFINE_INSTANCED_PROP(float, _BumpScale)
UNITY_DEFINE_INSTANCED_PROP(float, _DetailNormalMapScale)
UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
UNITY_DEFINE_INSTANCED_PROP(float, _Glossiness)
UNITY_DEFINE_INSTANCED_PROP(float, _OcclusionStrength)
UNITY_DEFINE_INSTANCED_PROP(float3, _EmissionColor)
UNITY_DEFINE_INSTANCED_PROP(float3, _Cutoff)
UNITY_DEFINE_INSTANCED_PROP(fixed, _Fade)
UNITY_DEFINE_INSTANCED_PROP(fixed, _Shrink)
UNITY_DEFINE_INSTANCED_PROP(float3, _Origin)
UNITY_DEFINE_INSTANCED_PROP(float, _SineAmp)
UNITY_DEFINE_INSTANCED_PROP(float, _SineFreq)
UNITY_DEFINE_INSTANCED_PROP(float, _SineOffset)
UNITY_DEFINE_INSTANCED_PROP(fixed, _SineRepetition)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexData 
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Interpolators 
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
	float4 uv2: TEXCOORD2;
	float3 normal : TEXCOORD1;

	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD3;
	#else
		float3 tangent : TEXCOORD3;
		float3 binormal : TEXCOORD4;
	#endif

	float3 worldPos : TEXCOORD4;

	SHADOW_COORDS(5)

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

float2 RadialCoords(float3 a_coords)
{
	float3 a_coords_n = normalize(a_coords);
	float lon = atan2(a_coords_n.z, a_coords_n.x);
	float lat = acos(a_coords_n.y);
	float2 sphereCoords = float2(lon, lat) * (1.0 / _Pi);
	return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
}

float GetDetailMask (Interpolators i) 
{
	#if defined (_DETAIL_MASK)
		#if defined(_EQUITANGULAR)
			float2 uv = RadialCoords(float3(i.normal)) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
		#else
			float2 uv = i.uv.zw;
		#endif
		return tex2D(_DetailMask, uv).a;
	#else
		return 1;
	#endif
}

float3 GetAlbedo (Interpolators i) 
{
	#if defined(_EQUITANGULAR)
		float2 uv = RadialCoords(float3(i.normal)) * _MainTex_ST.xy + _MainTex_ST.zw;
	#else
		float2 uv = i.uv.xy;
	#endif
	float3 albedo = tex2D(_MainTex, uv).rgb * UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb;
	#if defined (_DETAIL_ALBEDO_MAP)
		#if defined(_EQUITANGULAR)
			uv = RadialCoords(float3(i.normal)) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
		#else
			uv = i.uv.zw;
		#endif
		float4 details = tex2D(_DetailAlbedoMap, uv);
		details.rgb *= UNITY_ACCESS_INSTANCED_PROP(Props, _DetailColor).rgb;

		float alpha = GetDetailMask(i) * details.a;

		albedo = (albedo * (1 - alpha)) + (details.rgb * alpha);
	#endif
	return albedo;
}

float3 GetTangentSpaceNormal (Interpolators i) 
{
	float3 normal = float3(0, 0, 1);
	#if defined(_NORMAL_MAP)
		#if defined(_EQUITANGULAR)
			float2 uv = RadialCoords(float3(i.normal)) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
		#else
			float2 uv = i.uv.xy;
		#endif
		normal = UnpackScaleNormal(tex2D(_BumpMap, uv), UNITY_ACCESS_INSTANCED_PROP(Props, _BumpScale));
	#endif
	#if defined(_DETAIL_NORMAL_MAP)
		#if defined(_EQUITANGULAR)
			float2 uv = RadialCoords(float3(i.normal)) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
		#else
			float2 uv = i.uv.zw;
		#endif
		float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, uv), UNITY_ACCESS_INSTANCED_PROP(Props, _DetailNormalMapScale));
		detailNormal = lerp(float3(0, 0, 1), detailNormal, GetDetailMask(i));
		normal = BlendNormals(normal, detailNormal);
	#endif
	return normal;
}

float GetMetallic (Interpolators i) 
{
	#if defined(_METALLIC_MAP)
		return tex2D(_MetallicGlossMap, i.uv.xy).r * UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
	#else
		return UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
	#endif
}

float GetSmoothness (Interpolators i) 
{
	float smoothness = 1;
	#if defined(_Glossiness_ALBEDO)
		smoothness = tex2D(_MainTex, i.uv.xy).a;
	#elif defined(_Glossiness_METALLIC) && defined(_METALLIC_MAP)
		smoothness = tex2D(_MetallicGlossMap, i.uv.xy).a;
	#endif
	return smoothness * UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness);
}

float GetOcclusion (Interpolators i) 
{
	#if defined(_OCCLUSION_MAP)
		return lerp(1, tex2D(_OcclusionMap, i.uv.xy).g, UNITY_ACCESS_INSTANCED_PROP(Props, _OcclusionStrength));
	#else
		return 1;
	#endif
}

float3 GetEmission (Interpolators i) 
{
	#if defined(FORWARD_BASE_PASS)
		#if defined(_EmissionColor_MAP)
			return tex2D(_EmissionColorMap, i.uv.xy) * UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionColor);
		#else
			return UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionColor);
		#endif
	#else
		return 0;
	#endif
}

void ComputeVertexLightColor (inout Interpolators i) 
{
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb, unity_4LightAtten0, i.worldPos, i.normal);
	#endif
}

float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) 
{
	return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
}

Interpolators VertexProgram (VertexData v) 
{
	float3 direction = v.vertex.xyz - UNITY_ACCESS_INSTANCED_PROP(Props, _Origin);

	half distance = length(direction);

	direction = normalize(direction);
	
	v.vertex.xyz += direction * sin(clamp((distance * UNITY_ACCESS_INSTANCED_PROP(Props, _SineFreq)) + UNITY_ACCESS_INSTANCED_PROP(Props, _SineOffset), 0, 2 *
		_Pi * UNITY_ACCESS_INSTANCED_PROP(Props, _SineRepetition))) * UNITY_ACCESS_INSTANCED_PROP(Props, _SineAmp);

	v.vertex.xyz = lerp(half3(0,0,0), v.vertex.xyz, UNITY_ACCESS_INSTANCED_PROP(Props, _Shrink));

	Interpolators i;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, i);
	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = UnityObjectToWorldNormal(v.normal);

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
	#endif

	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailAlbedoMap);
	i.uv2 = float4(TRANSFORM_TEX(v.uv, _FadeTex), 0, 0);

	TRANSFER_SHADOW(i);

	ComputeVertexLightColor(i);


	return i;
}

UnityLight CreateLight (Interpolators i) 
{
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
	
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

float3 BoxProjection (float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax) 
{
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubemapPosition.w > 0) 
		{
			float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemapPosition);
		}
	#endif
	return direction;
}

UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) 
{
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));

		float3 reflectionDir = reflect(-viewDir, i.normal);
		Unity_GlossyEnvironmentData envData;
		envData.roughness = 1 - GetSmoothness(i);
		envData.reflUVW = BoxProjection(reflectionDir, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);

		float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
		envData.reflUVW = BoxProjection(reflectionDir, i.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

		#if UNITY_SPECCUBE_BLENDING
			float interpolator = unity_SpecCube0_BoxMin.w;
			UNITY_BRANCH
			if (interpolator < 0.99999) 
			{
				float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),unity_SpecCube0_HDR, envData);
				indirectLight.specular = lerp(probe1, probe0, interpolator);
			}
			else 
			{
				indirectLight.specular = probe0;
			}
		#else
			indirectLight.specular = probe0;
		#endif

		float occlusion = GetOcclusion(i);
		indirectLight.diffuse *= occlusion;
		indirectLight.specular *= occlusion;
	#endif

	return indirectLight;
}

void InitializeFragmentNormal(inout Interpolators i) 
{
	float3 tangentSpaceNormal = GetTangentSpaceNormal(i);
	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
	#else
		float3 binormal = i.binormal;
	#endif
	
	i.normal = normalize(tangentSpaceNormal.x * i.tangent + tangentSpaceNormal.y * binormal + tangentSpaceNormal.z * i.normal);
}

float4 FragmentProgram (Interpolators i) : SV_TARGET 
{
	UNITY_SETUP_INSTANCE_ID(i);

	float alpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Color).a;
	#if defined(_RENDERING_CUTOUT)
		clip(alpha - UNITY_ACCESS_INSTANCED_PROP(Props, _Cutoff));
	#endif

	InitializeFragmentNormal(i);

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	float3 specularTint;
	float oneMinusReflectivity;
	float3 albedo = DiffuseAndSpecularFromMetallic(GetAlbedo(i), GetMetallic(i), specularTint, oneMinusReflectivity);

	float texAlpha = tex2D(_MainTex, i.uv.xy).a;
	alpha *= texAlpha;

	#if defined(_RENDERING_TRANSPARENT)
		albedo *= alpha;
		alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
	#endif

	float4 color = UNITY_BRDF_PBS(albedo, specularTint, oneMinusReflectivity, GetSmoothness(i),i.normal, viewDir,CreateLight(i), CreateIndirectLight(i, viewDir));

	color.rgb += GetEmission(i);
	
	#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
		fixed fade = UNITY_ACCESS_INSTANCED_PROP(Props, _Fade);
		#ifdef _FADE_ON		 		  
		half fadeAlpha = tex2D(_FadeTex, i.uv2.xy).a;
		color.a = lerp(fadeAlpha, alpha, clamp((fade * 2) - 1, 0, 1));	
		color.a *= clamp(fade * 2, 0, 1);
		#else
		color.a = alpha * fade;
		#endif
		color.a *= texAlpha;
		color.rgb *= color.a;
	#else
		color.a *= texAlpha;
	#endif




	return color;
}
#endif