// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DefaultSprite-StencilObject"
{
	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_UVScale("UV Scale", Range(1, 1.1)) = 1
		_IntensityFactor("Intensity Variation Factor", Float) = 0.5

	}

		SubShader
		{
			Tags
		{
			"Queue" = "Transparent-1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

			Cull Off
			Lighting Off
			ZWrite Off


			Pass
		{
				Stencil
				{
					Ref 1
					Comp LEqual
					Pass IncrSat
				}

				Blend DstAlpha OneMinusSrcAlpha
				//Blend DstColor One
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				uniform half _IntensityFactor;

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float intensity : TESCOORD1;
				};

				fixed4 _Color;
				half _UVScale;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					float2 scaleCenter = float2(.5f, .5f);
					//half uvmult = 1-clamp(2*sin(_Time)-.1, 0,10);
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord; // (IN.texcoord - scaleCenter)*uvmult + scaleCenter;
					OUT.color = IN.color * _Color;
					OUT.intensity = _IntensityFactor * _CosTime.w*_SinTime.w*_SinTime.z+1.0;

					return OUT;
				}


				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);
					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					if (c.a == 0) discard;

					c.rgb *= c.a;
					c *= IN.intensity;
					return c;
				}
				ENDCG
		}
	}
}