﻿Shader "Holistic/Section8/Glass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_ScaleUV ("Scale", Range(1,20)) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent"}
		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
				float2 uvbump : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float _ScaleUV;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
o.uvgrab.xy = (float2(o.vertex.x, 1.0 - o.vertex.y) + o.vertex.w) * 0.5;
#else
o.uvgrab.xy = (float2(o.vertex.x, 1.0 - o.vertex.y) + o.vertex.w) * 0.5;
#endif
				o.uvgrab.zw = o.vertex.zw;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvbump = TRANSFORM_TEX(v.uv, _BumpMap);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
				float2 offset = bump * _ScaleUV * _GrabTexture_TexelSize.xy;
				i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;

				fixed4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				fixed4 tint = tex2D(_MainTex, i.uv);
				col *= tint;
				return col;
			}
			ENDCG
		}
	}
}
