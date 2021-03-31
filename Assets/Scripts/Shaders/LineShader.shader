Shader "Custom/LineShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200
			ZTest Always

			CGPROGRAM
			#pragma surface surf Unlit
			#pragma target 3.0

			struct Input {
				float4 color : COLOR;
			};

			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = _Color;
				o.Albedo = c.rgb * IN.color;
				o.Alpha = c.a;
			}

			half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
				return half4(s.Albedo, s.Alpha);
			}

			ENDCG
		}
			FallBack "Diffuse"
}