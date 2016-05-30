﻿Shader "Custom/obstacleShader" {
	SubShader {
			Pass
			{
				ZTest Always
				Name "Obstacle"
				CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag

				uniform float2 InverseSize;
				uniform float Point;
				uniform float Radius;

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
					float4 result = float4(0, 0, 0, 0);

					//draw border 
					if (IN.uv.x <= InverseSize.x) result = float4(1, 1, 1, 1);
					if (IN.uv.x >= 1.0 - InverseSize.x) result = float4(1, 1, 1, 1);
					if (IN.uv.y <= InverseSize.y) result = float4(1, 1, 1, 1);
					if (IN.uv.y >= 1.0 - InverseSize.y) result = float4(1, 1, 1, 1);

					//draw point
					float d = distance(Point, IN.uv);

					if (d < Radius) result = float4(1, 1, 1, 1);

					return result;
				}
					ENDCG
			}
	}
}
