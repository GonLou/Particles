Shader "Custom/GUIShader" {
	Properties 
	{
		MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			sampler2D MainTex;
			uniform sampler2D Obstacles;

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{
				float col = tex2D(MainTex, IN.uv).x;

				float obs = tex2D(Obstacles, IN.uv).x;

				float3 result = float3(obs, obs, obs);

					result.x += col;

				return float4(result, 1);
			}
			ENDCG
		}
	}
}
