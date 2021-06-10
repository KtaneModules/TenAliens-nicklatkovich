Shader "Unlit/River" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture 1 (RGB) Trans (A)", 2D) = "white" {}
		_Tex2 ("Texture 2 (RGB) Trans (A)", 2D) = "white" {}
		_Tex3 ("Texture 3 (RGB) Trans (A)", 2D) = "white" {}
		_MainTexSpeed("Texture 1 Speed", Float) = 2
		_Tex2Speed("Texture 2 Speed", Float) = 3
		_Tex3Speed("Texture 3 Speed", Float) = 4
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		ZWrite Off
		Lighting Off
		Fog { Mode Off }

		Blend SrcAlpha OneMinusSrcAlpha 

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Tex2;
			sampler2D _Tex3;
			float4 _MainTex_ST;
			float _MainTexSpeed;
			float _Tex2Speed;
			float _Tex3Speed;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col_1 = tex2D(_MainTex, i.uv + float2(_MainTexSpeed, 0) * _Time);
				fixed4 col_2 = tex2D(_Tex2, i.uv + float2(_Tex2Speed, 0) * _Time);
				fixed4 col_3 = tex2D(_Tex3, i.uv + float2(_Tex3Speed, 0) * _Time);
				fixed4 col = lerp(col_2, col_3, col_3.a);
				col = lerp(col_1, col, col.a);
				return col;
			}
			ENDCG
		}
	}
}
