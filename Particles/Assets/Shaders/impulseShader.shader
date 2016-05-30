Shader "Custom/impulseShader" {
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

			uniform float2 Point;
			uniform float Radius;
			uniform float Fill;
			uniform sampler2D SourceTexture;

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
				float d = distance(Point, IN.uv);

				float impulse = 0;

				if (d < Radius)
				{
					float a = (Radius - d) * 0.5;
					impulse = min(a, 1.0);
				}

				float source = tex2D(SourceTexture, IN.uv).x;

				return max(0, lerp(source, Fill, impulse)).xxxx;
			}
				ENDCG
		}
	}
}
