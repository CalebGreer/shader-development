Shader "Holistic/Section6/Hologram"
{
	Properties{
		_RimColor("Rim Color", Color) = (0, 0.5,0.5,0.0)
		_RimPower("Rim Power", Range(0.5, 8)) = 3.0
	}
		Subshader{
			Tags {"Queue" = "Transparent"}

			Pass{
				ZWrite On
				ColorMask 0
			}

		CGPROGRAM
		#pragma surface surf Lambert alpha:fade



		struct Input {
			float2 uv_myTex;
			float3 viewDir;
			float3 worldPos;
		};

		sampler2D _myTex;
		float4 _RimColor;
		float _RimPower;
		float _StripeWidth;

		void surf(Input IN, inout SurfaceOutput o) {
			half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower) * 10;
			o.Alpha = pow(rim, _RimPower);
		}

	ENDCG
		}
			FallBack "Diffuse"
}