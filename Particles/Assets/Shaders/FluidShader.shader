Shader "Custom/FluidShader" 
{
	Properties 
	{
		InverseSize ("Inverse Size", Vector) = (0.0,0.0,0.0,0.0)
		Point("Point", Vector) = (0.0, 0.0, 0.0, 0.0)

		TimeStep ("Time Step", Float) = 0.0
		Dissipation("Dissipation", Float) = 0.0
		AmbientTemperature("AmbientTemperature", Float) = 0.0
		Sigma("Sigma", Float) = 0.0
		Kappa("Kappa", Float) = 0.0
		Radius("Radius", Float) = 0.0
		Fill("Fill", Float) = 0.0
		HalfInverseCellSize("Half Inverse Cell Size", Float) = 0.0
		Alpha("Alpha", Float) = 0.0
		InverseBeta("Inverse Beta", Float) = 0.0
		Gradient Scale("Gradient Scale", Float) = 0.0

		Velocity("Velocity", 2D) = "" {}
		Source("Source", 2D) = "" {}
		Obstacles("Obstacles", 2D) = "" {}
		Temperature("Temperature", 2D) = "" {}
		Density("Density", 2D) = "" {}
		Source("Source", 2D) = "" {}
		Pressure("Pressure", 2D) = "" {}
		Divergence("Divergence", 2D) = "" {}
	}

	SubShader 
	{
		
		Pass //Advect
		{
			Name "Advect"
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
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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

		Pass //Jacobi
		{
			Name "Jacobi"
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D Pressure;
			uniform sampler2D Divergence;
			uniform sampler2D Obstacles;

			uniform float Alpha;
			uniform float InverseBeta;
			uniform float2 InverseSize;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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

				float bC = tex2D(Divergence, fragCoord).x;
				return (pressureLeft.x + pressureRight.x + pressureDown.x + pressureUp.x + Alpha * bC) * InverseBeta;
			}
			ENDCG
		}

		Pass //SubtractGradient
		{
			Name "SubtractGradient"
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
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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

		Pass //ComputeDivergence
		{
			Name "ComputeDivergence"
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
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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

		Pass //Splat
		{
			Name "Splat"
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float2 Point;
			uniform float Radius;
			uniform float Fill;
			uniform sampler2D Source;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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
					impulse = min(a, 10);
				}

				float source = tex2D(Source, IN.uv).x;

				return max(0, lerp(source, Fill, impulse)).xxxx;
			}
			ENDCG
		}

		Pass //Buoyancy
		{
			Name "Buoyancy"
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D Velocity;
			uniform sampler2D Temperature;
			uniform sampler2D Density;
			uniform float AmbientTemperature;
			uniform float TimeStep;
			uniform float Sigma;
			uniform float Kappa;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 frag(v2f IN) : COLOR
			{
				float2 fragCoord = IN.uv;
				float T = tex2D(Temperature, fragCoord).x;
				float2 V = tex2D(Velocity, fragCoord).xy;

				float2 result = V;
				if (T > AmbientTemperature)
				{
					float D = tex2D(Density, fragCoord).x;
					result += (TimeStep * (T - AmbientTemperature) * Sigma - D * Kappa) * float2(0, 1);
				}

				return float4(result, 0, 1);
			}
			ENDCG
		}

		Pass //Obstacle
		{
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
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
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
