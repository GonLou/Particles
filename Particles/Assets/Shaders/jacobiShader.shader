Shader "Custom/jacobiShader" {
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

			uniform sampler2D _Pressure;
			uniform sampler2D _Divergence;
			uniform sampler2D _Obstacles;

			uniform float _Alpha;
			uniform float _InverseBeta;
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
				float2 fragCoord = IN.uv;
				// Find neighboring pressure:
				float pressureUp = tex2D(_Pressure, IN.uv + float2(0, _InverseSize.y)).x;
				float pressureDown = tex2D(_Pressure, IN.uv + float2(0, -_InverseSize.y)).x;
				float pressureRight = tex2D(_Pressure, IN.uv + float2(_InverseSize.x, 0)).x;
				float pressureLeft = tex2D(_Pressure, IN.uv + float2(-_InverseSize.x, 0)).x;
				float pressureCenter = tex2D(_Pressure, IN.uv).x;

				// Find neighboring obstacles:
				float obstacleUp = tex2D(_Obstacles, IN.uv + float2(0, _InverseSize.y)).x;
				float obstacleDown = tex2D(_Obstacles, IN.uv + float2(0, -_InverseSize.y)).x;
				float obstacleRight = tex2D(_Obstacles, IN.uv + float2(_InverseSize.x, 0)).x;
				float obstacleLeft = tex2D(_Obstacles, IN.uv + float2(-_InverseSize.x, 0)).x;

				// Use center pressure for solid cells:
				if (obstacleUp > 0.0)
					pressureUp = pressureCenter;
				if (obstacleDown > 0.0)
					pressureDown = pressureCenter;
				if (obstacleRight > 0.0)
					pressureRight = pressureCenter;
				if (obstacleLeft > 0.0)
					pressureLeft = pressureCenter;

				float bC = tex2D(_Divergence, IN.uv).x;
				return (pressureLeft.x + pressureRight.x + pressureDown.x + pressureUp.x + _Alpha * bC) * _InverseBeta;
			}
			ENDCG
		}
	}
}