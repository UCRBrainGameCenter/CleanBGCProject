Shader "Unlit/WindowedUnlitTexture"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Contrast("Contrast", Float) = 0.5
		_Envelope("Envelope", 2D) = "white" {}
		_Color("Color", Color) = (0.5, 0.5, 0.5, 1.0)


		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
			"PreviewType" = "Plane"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 worldPosition : TEXCOORD1;
			};

			sampler2D _Envelope;
			float4 _Envelope_ST;
			float _Contrast;
			float4 _Color;

			bool _UseClipRect;
			float4 _ClipRect;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Envelope);

				float2 sampOffset = float2(-0.5, -0.5);

				o.worldPosition = v.vertex.xy;

				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed tex = _Contrast * (tex2D(_MainTex, i.uv).x - 0.5);
				fixed window = tex2D(_Envelope, i.uv).x;

				fixed4 channelFilter = fixed4(1.0, 1.0, 1.0, 0.0);

				fixed4 phaseOffset = fixed4(0.0 , 1.0 / (3.0 * 255.0), -1.0 / (3.0 * 255.0), 0.0);

				fixed4 col = _Color + channelFilter * tex * window + phaseOffset;


				if (_UseClipRect)
				{
					col *= UnityGet2DClipping(i.worldPosition, _ClipRect);
				}

				clip(col.a - 0.001);

				return col;
			}
			ENDCG
		}
	}
}