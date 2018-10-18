// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/2DLight-StencilObject"
{
	Properties
	{
		_MainTex("Diffuse Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_ContrastFactor("Contrast Factor", Float) = 1.0
		_ColorFactor("Color Factor", Float) = 0.5
		_IntensityFactor("Intensity Variation Factor", Float) = 0.5
		_RotationSpeed("Rotation Speed", Float) = 2.0

	}

		SubShader
		{
			Tags
		{
			"Queue" = "Transparent-1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"ForceNoShadowCasting" = "True"
		}
			Cull Off
			Lighting Off
			ZWrite Off

			Pass
		{
			Stencil
		{
			Ref 1
			Comp equal
		}
			Blend DstColor One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// User-specified properties
			uniform sampler2D _MainTex;
			uniform float4 _Color;
			uniform float _ContrastFactor;
			uniform float _IntensityFactor;
			float _RotationSpeed;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float intensity : TEXCOORD1;
			};

			VertexOutput vert(VertexInput input)
			{
				VertexOutput output;
				float2 tempuv = input.uv;
				tempuv -= 0.5;
				output.pos = UnityObjectToClipPos(input.vertex);
				float s = sin(_RotationSpeed * _Time);
				float c = cos(_RotationSpeed * _Time);
				float2x2 rotationMatrix = float2x2(c, -s, s, c);
				rotationMatrix *= 0.5;
				rotationMatrix += 0.5;
				rotationMatrix = rotationMatrix * 2 - 1;
				output.uv = mul(tempuv, rotationMatrix);
				output.uv += 0.5;
				output.color = input.color;
				output.intensity = _IntensityFactor * _ContrastFactor * cos(_Time.z) * sin(_Time.w) * _CosTime.w
					+ 1.0 * _ContrastFactor;
				return output;
			}

			float4 frag(VertexOutput input) : COLOR
			{
				float4 diffuseColor = tex2D(_MainTex, input.uv);
				// Retrieve color from texture and multiply it by tint color and by sprite color
				// Multiply everything by texture alpha to emulate transparency
				diffuseColor.rgb = diffuseColor.rgb * _Color.rgb * input.color.rgb;
				diffuseColor.rgb *= diffuseColor.a * _Color.a * input.color.a;
				diffuseColor *= input.intensity;

				return float4(diffuseColor);
			}

			ENDCG
		}

				Pass
			{
				Stencil
				{
					Ref 1
					Comp equal
				}
				Blend SrcAlpha One  // Add colours to the previous pixels

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				// User-specified properties
				uniform sampler2D _MainTex;
				uniform float4 _Color;
				uniform float _ColorFactor;

				struct VertexInput
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct VertexOutput
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				VertexOutput vert(VertexInput input)
				{
					VertexOutput output;
					output.pos = UnityObjectToClipPos(input.vertex);
					output.uv = input.uv;
					output.color = input.color;

					return output;
				}

				float4 frag(VertexOutput input) : COLOR
				{
					float4 diffuseColor = tex2D(_MainTex, input.uv);
					diffuseColor.rgb = _Color.rgb * diffuseColor.rgb * input.color.rgb * diffuseColor.a;
					diffuseColor *= _ColorFactor;
					diffuseColor.a = _Color.a * input.color.a;

					return float4(diffuseColor);
				}

					ENDCG
				}

		}
}