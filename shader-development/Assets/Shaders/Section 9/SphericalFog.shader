Shader "Holistic/Section9/SphericalFog"
{
    Properties
    {
        _FogCentre("Fog Centre/Radius", Vector) = (0,0,0,0.5)
        _FogColour("Fog Colour", Color) = (1,1,1,1)
        _InnerRatio("Inner Ratio", Range(0.0, 0.9)) = 0.5
        _Density("Density", Range(0.0, 1.0)) = 0.5
        _Segments("Segments", Range(5, 20)) = 10
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Segments;

            float CalculateFogIntensity(
                float3 sphereCentre, 
                float sphereRadius, 
                float innerRatio, 
                float density, 
                float3 cameraPosition, 
                float3 viewDirection, 
                float maxDistance)
            {
                // Calculate ray-sphere intersection
                float3 localCam = cameraPosition - sphereCentre;
                float a = dot(viewDirection, viewDirection); // dot product of the same vector = vector squared
                float b = 2 * dot(viewDirection, localCam);
                float c = dot(localCam, localCam) - sphereRadius * sphereRadius;
                float d = b * b - 4 * a * c;

                if (d <= 0.0f) 
                    return 0;

                float DSqrt = sqrt(d);
                float dist = max((-b - DSqrt) / 2 * a, 0);
                float dist2 = max((-b + DSqrt) / 2 * a, 0);
                
                float backDepth = min(maxDistance, dist2);
                float sample = dist;
                float step_distance = (backDepth - dist) / _Segments;
                float step_contribution = density;

                float centreValue = 1 / (1 - innerRatio);

                float clarity = 1;
                for (int seg = 0; seg < _Segments; seg++)
                {
                    float3 position = localCam + viewDirection * sample;
                    float val = saturate(centreValue * (1 - length(position) / sphereRadius));
                    float fog_amount = saturate(val * step_contribution);
                    clarity *= (1 - fog_amount);
                    sample += step_distance;
                }

                return 1 - clarity;
            }

            struct v2f
            {
                float3 view : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD1;
            };

            float4 _FogCentre;
            fixed4 _FogColour;
            float _InnerRatio;
            float _Density;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata_base v)
            {
                v2f o;
                float4 wPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.view = wPos.xyz - _WorldSpaceCameraPos;
                o.projPos = ComputeScreenPos(o.pos);

                float inFrontOf = (o.pos.z / o.pos.w) > 0;
                o.pos.z *= inFrontOf;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 color = half4 (1,1,1,1);
                float depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
                float3 viewDir = normalize(i.view);

                float fog = CalculateFogIntensity(
                    _FogCentre.xyz,
                    _FogCentre.w,
                    _InnerRatio,
                    _Density,
                    _WorldSpaceCameraPos,
                    viewDir,
                    depth);

                color.rgb = _FogColour.rgb;
                color.a = fog;
                return color;
            }
            ENDCG
        }
    }
}
