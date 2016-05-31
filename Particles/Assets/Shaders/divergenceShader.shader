Shader "Custom/divergenceShader" {
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

			uniform sampler2D _Velocity;
			uniform sampler2D _Obstacles;
			uniform float _HalfInverseCellSize;
			uniform float2 _InverseSize;

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
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

				// Find neighboring velocities:
				float2 velocityUp = tex2D(_Velocity, IN.uv + float2(0, _InverseSize.y)).xy;
				float2 velocityDown = tex2D(_Velocity, IN.uv + float2(0, -_InverseSize.y)).xy;
				float2 velocityRight = tex2D(_Velocity, IN.uv + float2(_InverseSize.x, 0)).xy;
				float2 velocityLeft = tex2D(_Velocity, IN.uv + float2(-_InverseSize.x, 0)).xy;

				// Find neighboring obstacles:
				float obstacleUp = tex2D(_Obstacles, IN.uv + float2(0, _InverseSize.y)).x;
				float obstacleDown = tex2D(_Obstacles, IN.uv + float2(0, -_InverseSize.y)).x;
				float obstacleRight = tex2D(_Obstacles, IN.uv + float2(_InverseSize.x, 0)).x;
				float obstacleLeft = tex2D(_Obstacles, IN.uv + float2(-_InverseSize.x, 0)).x;

				// Set velocities to 0 for solid cells:
				if (obstacleUp > 0.0)
					velocityUp = 0.0;
				if (obstacleDown > 0.0)
					velocityDown = 0.0;
				if (obstacleRight > 0.0)
					velocityRight = 0.0;
				if (obstacleLeft > 0.0)
					velocityLeft = 0.0;

				float result = _HalfInverseCellSize *(velocityRight.x - velocityLeft.x + velocityUp.y - velocityDown.y);
				return float4(result, 0, 0, 1);
			}
			ENDCG
		}
	}
}