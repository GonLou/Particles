Shader "Custom/gradientShader" {
	SubShader
	{
		Pass
		{
			ZTest Always
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D Velocity;
			uniform sampler2D Pressure;
			uniform sampler2D Obstacles;
			uniform float GradientScale;
			uniform float2 InverseSize;

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
				// Find neighboring pressure:
				float pN = tex2D(Pressure, IN.uv + float2(0, InverseSize.y)).x;
				float pS = tex2D(Pressure, IN.uv + float2(0, -InverseSize.y)).x;
				float pE = tex2D(Pressure, IN.uv + float2(InverseSize.x, 0)).x;
				float pW = tex2D(Pressure, IN.uv + float2(-InverseSize.x, 0)).x;
				float pC = tex2D(Pressure, IN.uv).x;

				// Find neighboring obstacles:
				float bN = tex2D(Obstacles, IN.uv + float2(0, InverseSize.y)).x;
				float bS = tex2D(Obstacles, IN.uv + float2(0, -InverseSize.y)).x;
				float bE = tex2D(Obstacles, IN.uv + float2(InverseSize.x, 0)).x;
				float bW = tex2D(Obstacles, IN.uv + float2(-InverseSize.x, 0)).x;

				// Use center pressure for solid cells:
				if (bN > 0.0) pN = pC;
				if (bS > 0.0) pS = pC;
				if (bE > 0.0) pE = pC;
				if (bW > 0.0) pW = pC;

				// Enforce the free-slip boundary condition:
				float2 oldV = tex2D(Velocity, IN.uv).xy;
					float2 grad = float2(pE - pW, pN - pS) * GradientScale;
					float2 newV = oldV - grad;

					return float4(newV, 0, 1);
			}
			ENDCG
		}
	}
}
