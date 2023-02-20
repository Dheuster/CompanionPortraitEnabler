#ifndef OWLCAT_DECAL_INPUT_INCLUDED
#define OWLCAT_DECAL_INPUT_INCLUDED

#include "Assets/RenderPipeline/UnityShaders/Common.hlsl"
#include "Assets/RenderPipeline/UnityShaders/Packing.hlsl"
#include "Assets/RenderPipeline/UnityShaders/CommonMaterial.hlsl"

TEXTURE2D(_MainTex1); SAMPLER(sampler_MainTex1);
TEXTURE2D(_Noise0Tex); SAMPLER(sampler_Noise0Tex);
TEXTURE2D(_Noise1Tex); SAMPLER(sampler_Noise1Tex);
TEXTURE2D(_ColorAlphaRamp); SAMPLER(sampler_ColorAlphaRamp);

//#ifdef DEFERRED_ON
//	// ���� �� ������������ � ��� ��� ������� � HDRP (DecalNormalBuffer.shader)
//	// ������� SetRenderTarget � SetRandomWriteTarget ����� �������� ������
//	// �.�. �� ��� ���������� GBuffer'� ���������� _CameraAlbedoRT, _CameraColorRT (��� emission), �.�. ����� 2 RT
//	// �� ���������� _CameraNormalsUAV � ������� � �������� u2 (0 � 1 ������ _CameraAlbedoRT � _CameraColorRT ��������������)
//	// https://docs.unity3d.com/ScriptReference/Graphics.SetRandomWriteTarget.html
//	#if defined(PLATFORM_NEEDS_UNORM_UAV_SPECIFIER) && defined(PLATFORM_SUPPORTS_EXPLICIT_BINDING)
//		RW_TEXTURE2D(unorm float4, _CameraAlbedoUAV) : register(u1);
//		RW_TEXTURE2D(unorm float4, _CameraNormalsUAV) : register(u2);
//	#else
//		RW_TEXTURE2D(float4, _CameraAlbedoUAV);
//		RW_TEXTURE2D(float4, _CameraNormalsUAV);
//	#endif

//	//#if defined(PLATFORM_NEEDS_UNORM_UAV_SPECIFIER) && defined(PLATFORM_SUPPORTS_EXPLICIT_BINDING)
//	//	RasterizerOrderedTexture2D<unorm float4> _CameraNormalsUAV : register(u2);
//	//#else
//	//	RasterizerOrderedTexture2D<float4> _CameraNormalsUAV;
//	//#endif
//#endif

#define DECAL_UV_MAPPING_LOCAL 0
#define DECAL_UV_MAPPING_WORLD 1
#define DECAL_UV_MAPPING_RADIAL 2

CBUFFER_START(UnityPerMaterial)
float _UvMapping;
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
float4 _BaseMap_MipInfo;
float4 _BaseColor;
float _HdrColorScale;
float _AlphaScale;
float _Cutoff;
float _Roughness;
float _Metallic;
float _BumpScale;
int _DecalOutputMask;

float4 _EmissionColor;
float _EmissionColorScale;
float2 _EmissionUVSpeed;
float4 _EmissionMap_ST;
float _EmissionMapUsage;
float _EmissionColorFactor;
float _EmissionAlbedoSuppression;

float _DecalSlopeFadeStart;
float _DecalSlopeFadePower;
float _DecalSlopeHardEdgeNormalFactor;
float _DecalGradientMode;
float _DecalExpGradient;

float2 _UV0Speed;

float4 _MainTex1_ST;
float2 _UV1Speed;
float _Tex1MixMode;
float _MainTex1Weight;

float4 _Noise0Tex_ST;
float _Noise0Scale;
float2 _Noise0Speed;

float4 _Noise1Tex_ST;
float _Noise1Scale;
float2 _Noise1Speed;

float _SubstractAlphaFlag;

float4 _ColorAlphaRamp_ST;
float _RampAlbedoWeight;
float _RampScrollSpeed;
float _RandomizeRampOffset;

// radial alpha
float _RadialAlphaGradientStart;
float _RadialAlphaGradientPower;
float _RadialAlphaSubstract;
CBUFFER_END

#include "../../ShaderLibrary/Input.hlsl"
#include "../../ShaderLibrary/Core.hlsl"
#include "../../ShaderLibrary/SurfaceInput.hlsl"

#endif // OWLCAT_DECAL_INPUT_INCLUDED
