// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LightShader2D/AboutPixels_Simplified"
{
	Properties
	{
		_MainTex("Diffuse Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_ContrastFactor("Contrast Factor", Float) = 1.0
		_ColorFactor("Color Factor", Float) = 0.5
		_IntensityFactor("Intensity Variation Factor", Float) = 0.5
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"ForceNoShadowCasting" = "True"
	}

		Pass
	{
		AlphaTest Greater 0.0     // Pixel with an alpha of 0 should be ignored
		Blend DstColor One // Keep deep black values

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		// User-specified properties
		uniform sampler2D _MainTex;
		uniform float4 _Color;
		uniform float _ContrastFactor;
		uniform float _IntensityFactor;


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
			output.pos = UnityObjectToClipPos(input.vertex);
			output.uv = input.uv;
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

	}
}