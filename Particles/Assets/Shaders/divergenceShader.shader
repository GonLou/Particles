Shader "Custom/divergenceShader" {
	SubShader{
			Pass //ComputeDivergence
			{
				ZTest Always
				CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag

				uniform sampler2D Velocity;
				uniform sampler2D Obstacles;
				uniform float HalfInverseCellSize;
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
					float2 velocityUp = tex2D(Velocity, fragCoord + float2(0, InverseSize.y)).xy;
					float2 velocityDown = tex2D(Velocity, fragCoord + float2(0, -InverseSize.y)).xy;
					float2 velocityRight = tex2D(Velocity, fragCoord + float2(InverseSize.x, 0)).xy;
					float2 velocityLeft = tex2D(Velocity, fragCoord + float2(-InverseSize.x, 0)).xy;

					float obstacleUp = tex2D(Obstacles, fragCoord + float2(0, InverseSize.y)).x;
					float obstacleDown = tex2D(Obstacles, fragCoord + float2(0, -InverseSize.y)).x;
					float obstacleRight = tex2D(Obstacles, fragCoord + float2(InverseSize.x, 0)).x;
					float obstacleLeft = tex2D(Obstacles, fragCoord + float2(-InverseSize.x, 0)).x;

					if (obstacleUp > 0.0)
						velocityUp = 0.0;
					if (obstacleDown > 0.0)
						velocityDown = 0.0;
					if (obstacleRight > 0.0)
						velocityRight = 0.0;
					if (obstacleLeft > 0.0)
						velocityLeft = 0.0;

					float result = HalfInverseCellSize *(velocityRight.x - velocityLeft.x + velocityUp.y - velocityDown.y);
					return float4(result, 0, 0, 1);
				}
					ENDCG
			}
		}
}
