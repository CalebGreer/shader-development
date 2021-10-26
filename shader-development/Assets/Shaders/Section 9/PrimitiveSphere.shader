Shader "Holistic/Section9/PrimitiveSphere"
{
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
                float3 wPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

			#define STEPS 64
			#define STEP_SIZE 0.01

			bool SphereHit(float3 p, float3 centre, float radius)
			{

			}

			float RaymarchHit(float3 position, float3 direction)
			{
				for (int i = 0; i < STEPS; i++)
				{
					if (SphereHit(position, float(0, 0, 0), 0.5))
					{
						return position;
					}
					position += direction * STEP_SIZE;
				}

				return 0;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
