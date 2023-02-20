Shader "Hidden/Owlcat/DBufferBlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
            ZTest Always
            ZWrite Off
            Cull Off
            Stencil
			{
				Ref 1 // see StencilRef
				ReadMask 1
				Comp equal
			}
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

            #pragma target 4.5

			#include "Assets/RenderPipeline/UnityShaders/Common.hlsl"
            #include "../../ShaderLibrary/Input.hlsl"
            #include "../../ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                uint vertexID : VERTEXID_SEMANTIC;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            //���� �� ������������ � ��� ��� ������� � HDRP (DecalNormalBuffer.shader)
            // ������� SetRenderTarget � SetRandomWriteTarget ����� �������� ������
            // �.�. �� ��� ���������� GBuffer'� ���������� _CameraAlbedoRT, _CameraColorRT (��� emission), �.�. ����� 2 RT
            // �� ���������� _CameraNormalsUAV � ������� � �������� u2 (0 � 1 ������ _CameraAlbedoRT � _CameraColorRT ��������������)
            // https://docs.unity3d.com/ScriptReference/Graphics.SetRandomWriteTarget.html
            #if defined(PLATFORM_NEEDS_UNORM_UAV_SPECIFIER) && defined(PLATFORM_SUPPORTS_EXPLICIT_BINDING)
            	RW_TEXTURE2D(unorm float4, _CameraAlbedoUAV) : register(u1);
            	RW_TEXTURE2D(unorm float4, _CameraNormalsUAV) : register(u2);
            #else
            	RW_TEXTURE2D(float4, _CameraAlbedoUAV);
            	RW_TEXTURE2D(float4, _CameraNormalsUAV);
            #endif

            TEXTURE2D(_DecalsNormalsRT);
            TEXTURE2D(_DecalsMasksRT);

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }

            [earlydepthstencil]
            void Frag(Varyings input)
            {
                float4 gBufferNormalAndSmoothness = DecodeNormalAndSmoothness(_CameraNormalsUAV[input.positionCS.xy]);
		        float4 decalNormalAndAlpha =  LOAD_TEXTURE2D(_DecalsNormalsRT, input.positionCS.xy);
                float4 decalMasks = LOAD_TEXTURE2D(_DecalsMasksRT, input.positionCS.xy);
                float4 decalNormalAndSmoothness = float4(decalNormalAndAlpha.rgb, decalMasks.r);
		        gBufferNormalAndSmoothness = lerp(gBufferNormalAndSmoothness, decalNormalAndSmoothness, decalNormalAndAlpha.a);
		        gBufferNormalAndSmoothness.rgb = normalize(gBufferNormalAndSmoothness.rgb);
		        gBufferNormalAndSmoothness.rgb = EncodeNormal(gBufferNormalAndSmoothness.rgb);
		        _CameraNormalsUAV[input.positionCS.xy] = gBufferNormalAndSmoothness;

		        float4 gBufferAlbedoAndMetallic = _CameraAlbedoUAV[input.positionCS.xy];
		        gBufferAlbedoAndMetallic = lerp(gBufferAlbedoAndMetallic.a, decalMasks.g, decalNormalAndAlpha.a);
		        _CameraAlbedoUAV[input.positionCS.xy] = gBufferAlbedoAndMetallic;
            }

			ENDHLSL
		}
	}
}
