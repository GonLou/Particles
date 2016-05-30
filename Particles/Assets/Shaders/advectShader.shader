Shader "Custom/advectShader" {
	SubShader {
		Pass
		{
			ZTest Always

			CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag

			uniform sampler2D VelocityTexture;
			uniform sampler2D SourceTexture;
			uniform sampler2D Obstacles;

			uniform float2 InverseSize;
			uniform float TimeStep;
			uniform float Dissipation;

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
				float solid = tex2D(Obstacles, InverseSize * IN.uv).x;
				if (solid > 0.0)
				{
					float4 result = float4(0, 0, 0, 0);
						return result;
				}

				float2 u = tex2D(VelocityTexture, InverseSize * IN.uv).xy;
					float2 coord = InverseSize * (IN.uv - TimeStep * u);
					float4 result = Dissipation * tex2D(SourceTexture, coord);
					return result;
			}
				ENDCG
		}
	}
}
