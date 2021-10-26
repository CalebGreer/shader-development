Shader "Holistic/Section4/Rim"
{
	Properties{
		_myTex("Base (RGB)", 2D) = "white" {}
		_RimColor("Rim Color", Color) = (0, 0.5,0.5,0.0)
		_RimPower("Rim Power", Range(0.5, 8)) = 3.0
		_StripeWidth("Stripe Width", Range(1, 20)) = 10.0
	}
		Subshader{

		CGPROGRAM
		#pragma surface surf Lambert



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
			o.Albedo = tex2D(_myTex, IN.uv_myTex).rgb;
			half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = frac(IN.worldPos.y * (20 - _StripeWidth) * 0.5) > 0.4 ? float3(0, 1, 0) * rim : float3(1, 0, 0) * rim;
		}

	ENDCG
	}
		FallBack "Diffuse"
}