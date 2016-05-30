Shader "Custom/FluidShader" 
{
	Properties 
	{
		_InverseSize ("InverseSize", Vector) = (0.0,0.0,0.0,0.0)
		_Point ("Point", Vector) = (0.0, 0.0, 0.0, 0.0)

		_TimeStep ("Time Step", Float) = 0.0
		_Dissipation ("Dissipation", Float) = 0.0
		_AmbientTemperature ("AmbientTemperature", Float) = 0.0
		_Sigma ("Sigma", Float) = 0.0
		_Kappa ("Kappa", Float) = 0.0
		_Radius ("Radius", Float) = 0.0
		_Fill ("Fill", Float) = 0.0
		_HalfInverseCellSize ("HalfInverseCellSize", Float) = 0.0
		_Alpha ("Alpha", Float) = 0.0
		_InverseBeta ("InverseBeta", Float) = 0.0
		_GradientScale ("Gradient Scale", Float) = 0.0

		_Velocity ("Velocity", 2D) = "" {}
		_Source ("Source", 2D) = "" {}
		_Obstacles ("Obstacles", 2D) = "" {}
		_Temperature ("Temperature", 2D) = "" {}
		_Density ("Density", 2D) = "" {}
		_Pressure ("Pressure", 2D) = "" {}
		_Divergence ("Divergence", 2D) = "" {}
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

			uniform sampler2D _Velocity;
			uniform sampler2D _Source;
			uniform sampler2D _Obstacles;

			uniform float2 _InverseSize;
			uniform float _TimeStep;
			uniform float _Dissipation;

			struct v2f
			{
				float4 pos : SV_POSITION;
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
				float2 u = tex2D(_Velocity, IN.uv).xy;

				float2 coord = IN.uv - (u * _InverseSize * _TimeStep);

				float4 result = _Dissipation * tex2D(_Source, coord);

				float solid = tex2D(_Obstacles, IN.uv).x;

				if (solid > 0.0) result = float4(0, 0, 0, 0);

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

			uniform sampler2D _Pressure;
			uniform sampler2D _Divergence;
			uniform sampler2D _Obstacles;

			uniform float _Alpha;
			uniform float _InverseBeta;
			uniform float2 _InverseSize;

			struct v2f
			{
				float4 pos : SV_POSITION;
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

				float bC = tex2D(_Divergence, IN.uv).x;
				return (pressureLeft.x + pressureRight.x + pressureDown.x + pressureUp.x + _Alpha * bC) * _InverseBeta;
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

			uniform sampler2D _Velocity;
			uniform sampler2D _Pressure;
			uniform sampler2D _Obstacles;
			uniform float _GradientScale;
			uniform float2 _InverseSize;

			struct v2f
			{
				float4 pos : SV_POSITION;
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

		Pass //ComputeDivergence
		{
			Name "ComputeDivergence"
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
				float4 pos : SV_POSITION;
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
				float2 velocityUp = tex2D(_Velocity, IN.uv + float2(0, _InverseSize.y)).xy;
				float2 velocityDown = tex2D(_Velocity, IN.uv + float2(0, -_InverseSize.y)).xy;
				float2 velocityRight = tex2D(_Velocity, IN.uv + float2(_InverseSize.x, 0)).xy;
				float2 velocityLeft = tex2D(_Velocity, IN.uv + float2(-_InverseSize.x, 0)).xy;

				float obstacleUp = tex2D(_Obstacles, IN.uv + float2(0, _InverseSize.y)).x;
				float obstacleDown = tex2D(_Obstacles, IN.uv + float2(0, -_InverseSize.y)).x;
				float obstacleRight = tex2D(_Obstacles, IN.uv + float2(_InverseSize.x, 0)).x;
				float obstacleLeft = tex2D(_Obstacles, IN.uv + float2(-_InverseSize.x, 0)).x;

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

		Pass //Splat
		{
			Name "Splat"
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float2 _Point;
			uniform float _Radius;
			uniform float _Fill;
			uniform sampler2D _SourceTexture;

			struct v2f
			{
				float4 pos : SV_POSITION;
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
				float d = distance(_Point, IN.uv);

				float impulse = 0;
				if (d < _Radius)
				{
					float a = (_Radius - d) * 0.5;
					impulse = min(a, 10);
				}

				float source = tex2D(_SourceTexture, IN.uv).x;

				return max(0, lerp(source, _Fill, impulse)).xxxx;
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

			uniform sampler2D _Velocity;
			uniform sampler2D _Temperature;
			uniform sampler2D _Density;
			uniform float _AmbientTemperature;
			uniform float _TimeStep;
			uniform float _Sigma;
			uniform float _Kappa;

			struct v2f
			{
				float4 pos : SV_POSITION;
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
				float T = tex2D(_Temperature, IN.uv).x;
				float2 V = tex2D(_Velocity, IN.uv).xy;
				float D = tex2D(_Density, IN.uv).x;

				float2 result = V;
				if (T > _AmbientTemperature)
				{
					result += (_TimeStep * (T - _AmbientTemperature) * _Sigma - D * _Kappa) * float2(0, 1);
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

			uniform float2 _InverseSize;
			uniform float2 _Point;
			uniform float _Radius;

			struct v2f
			{
				float4 pos : SV_POSITION;
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
				if (IN.uv.x <= _InverseSize.x) result = float4(1, 1, 1, 1);
				if (IN.uv.x >= 1.0 - _InverseSize.x) result = float4(1, 1, 1, 1);
				if (IN.uv.y <= _InverseSize.y) result = float4(1, 1, 1, 1);
				if (IN.uv.y >= 1.0 - _InverseSize.y) result = float4(1, 1, 1, 1);

				//draw point
				float d = distance(_Point, IN.uv);

				if (d < _Radius) result = float4(1, 1, 1, 1);

				return result;
			}
			ENDCG
		}
	}
}
