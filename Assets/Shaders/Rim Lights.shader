Shader "Unlit/Rim Lights"
{
	Properties
	{
		_MainColor("MainColor", Color) = (1,1,1,1)
	}
	SubShader
	{ 
		Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		GrabPass{ "_GrabTexture" }
		Pass
		{
			Lighting Off ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed4 normal: NORMAL;
			};

			struct v2f
			{
				fixed4 vertex : SV_POSITION;
				fixed dotProductN : TEXCOORD0;
			};

			fixed4 _MainColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//fresnel 
				fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				fixed dotProduct = 1 - saturate(dot(v.normal, viewDir));
				o.dotProductN = dotProduct;
				//o.dotProductN *= o.dotProductN;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(_MainColor * i.dotProductN);
			}
			ENDCG
		}
	}
}
