Shader "Custom/obstacleShader" {
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

			uniform float _Active;
			uniform float _Top;
			uniform float _Bottom;
			uniform float _Left;
			uniform float _Right;
			uniform float2 _InverseSize;
			uniform float2 _Point;
			uniform float _Radius;

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
				float4 result = float4(0, 0, 0, 0);

				//draw border 
				if (_Left)
					if (IN.uv.x <= _InverseSize.x) result = float4(1, 1, 1, 1);
				if (_Right)
					if (IN.uv.x >= 1.0 - _InverseSize.x) result = float4(1, 1, 1, 1);
				if (_Bottom)
					if (IN.uv.y <= _InverseSize.y) result = float4(1, 1, 1, 1);
				if (_Top)
					if (IN.uv.y >= 1.0 - _InverseSize.y) result = float4(1, 1, 1, 1);

				//draw point
				if (_Active)
				{
					float d = distance(_Point, IN.uv);

					if (d < _Radius) result = float4(1, 1, 1, 1);
				}
				

				return result;
			}
			ENDCG
		}
	}
}