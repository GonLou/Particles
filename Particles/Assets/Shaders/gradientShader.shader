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

			uniform sampler2D _Velocity;
			uniform sampler2D _Pressure;
			uniform sampler2D _Obstacles;
			uniform float _GradientScale;
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
				float pressureUp = tex2D(_Pressure, IN.uv + float2(0, _InverseSize.y)).x;
				float pressureDown = tex2D(_Pressure, IN.uv + float2(0, -_InverseSize.y)).x;
				float pressureRight = tex2D(_Pressure, IN.uv + float2(_InverseSize.x, 0)).x;
				float pressureLeft = tex2D(_Pressure, IN.uv + float2(-_InverseSize.x, 0)).x;
				float pressureCenter = tex2D(_Pressure, IN.uv).x;

				float obstacleUp = tex2D(_Obstacles, IN.uv + float2(0, _InverseSize.y)).x;
				float obstacleDown = tex2D(_Obstacles, IN.uv + float2(0, -_InverseSize.y)).x;
				float obstacleRight = tex2D(_Obstacles, IN.uv + float2(_InverseSize.x, 0)).x;
				float obstacleLeft = tex2D(_Obstacles, IN.uv + float2(-_InverseSize.x, 0)).x;

				if (obstacleUp > 0.0)
					pressureUp = pressureCenter;
				if (obstacleDown > 0.0)
					pressureDown = pressureCenter;
				if (obstacleRight > 0.0)
					pressureRight = pressureCenter;
				if (obstacleLeft > 0.0)
					pressureLeft = pressureCenter;

				float2 oldV = tex2D(_Velocity, IN.uv).xy;
				float2 grad = float2(pressureRight - pressureLeft, pressureUp - pressureDown) * _GradientScale;
				float2 newV = oldV - grad;

				return float4(newV, 0, 1);

			}
			ENDCG
		}
	}
}