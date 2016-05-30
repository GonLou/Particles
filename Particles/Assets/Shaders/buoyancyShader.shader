Shader "Custom/buoyancyShader" {
	SubShader{
		Pass
		{
			ZTest Always
			Name "Buoyancy"
			CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag

			uniform sampler2D Velocity;
			uniform sampler2D Temperature;
			uniform sampler2D Density;
			uniform float AmbientTemperature;
			uniform float TimeStep;
			uniform float Sigma;
			uniform float Kappa;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 frag(v2f IN) : COLOR
			{
				float2 fragCoord = IN.uv;
				float T = tex2D(Temperature, fragCoord).x;
				float2 V = tex2D(Velocity, fragCoord).xy;

					float2 result = V;
					if (T > AmbientTemperature)
					{
						float D = tex2D(Density, fragCoord).x;
						result += (TimeStep * (T - AmbientTemperature) * Sigma - D * Kappa) * float2(0, 1);
					}

				return float4(result, 0, 1);
			}
				ENDCG
		}
	}
}
