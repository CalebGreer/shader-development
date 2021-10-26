Shader "Holistic/Section4/DotProduct"
{
		Subshader{

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _myBump;
		samplerCUBE _myCube;

		struct Input {
			float3 viewDir;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			half dotp = dot(IN.viewDir, o.Normal);
			o.Albedo = float3(dotp, 1, 1);
		}

		ENDCG
	}
		FallBack "Diffuse"
}