Shader "Custom/gradientShader" {
	SubShader{
			Pass //SubtractGradient
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
					float2 fragCoord = IN.uv;
					float pressureUp = tex2D(Pressure, fragCoord + float2(0, InverseSize.y)).x;
					float pressureDown = tex2D(Pressure, fragCoord + float2(0, -InverseSize.y)).x;
					float pressureRight = tex2D(Pressure, fragCoord + float2(InverseSize.x, 0)).x;
					float pressureLeft = tex2D(Pressure, fragCoord + float2(-InverseSize.x, 0)).x;
					float pressureCenter = tex2D(Pressure, fragCoord).x;

					float obstacleUp = tex2D(Obstacles, fragCoord + float2(0, InverseSize.y)).x;
					float obstacleDown = tex2D(Obstacles, fragCoord + float2(0, -InverseSize.y)).x;
					float obstacleRight = tex2D(Obstacles, fragCoord + float2(InverseSize.x, 0)).x;
					float obstacleLeft = tex2D(Obstacles, fragCoord + float2(-InverseSize.x, 0)).x;

					if (obstacleUp > 0)
						pressureUp = pressureCenter;
					if (obstacleDown > 0)
						pressureDown = pressureCenter;
					if (obstacleRight > 0)
						pressureRight = pressureCenter;
					if (obstacleLeft > 0)
						pressureLeft = pressureCenter;

					float2 oldV = tex2D(Velocity, fragCoord).xy;
						float2 grad = float2(pressureRight - pressureLeft, pressureUp - pressureDown) * GradientScale;
						float2 newV = oldV - grad;

						return float4(newV, 0, 1);
				}
					ENDCG
			}
		}
}
