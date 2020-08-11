Shader "Unlit/Billboard_Trans_ZOFF"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		//-------------------------
		_Color("Color", Color) = (1,1,1,1)
		_ColorMutl("Premultiply Colors", Float) = 1.0
		_AplhaMutl("Premultiply Alpha", Float) = 1.0
		//-------------------------
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
			"DisableBatching" = "True"}
		LOD 100
	
		//-------------------------
		ZTest always
		//-------------------------
		Cull Off Lighting Off ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
	
		Pass
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog
			
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				//-------------------------
				uniform float3 _Color;
				uniform float _ColorMutl;
				uniform float _AplhaMutl;
				//-------------------------

				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					//-------------------------
					float scaleX = length(float4(UNITY_MATRIX_M[0].r, UNITY_MATRIX_M[1].r, UNITY_MATRIX_M[2].r, UNITY_MATRIX_M[3].r));
					float scaleY = length(float4(UNITY_MATRIX_M[0].g, UNITY_MATRIX_M[1].g, UNITY_MATRIX_M[2].g, UNITY_MATRIX_M[3].g));

					o.vertex = mul(UNITY_MATRIX_P,
						mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
						- float4(v.vertex.x, -v.vertex.y, 0.0, 0.0)
						* float4(scaleX, scaleY, 1.0, 1.0));
					//-------------------------
					//o.vertex = UnityObjectToClipPos(v.vertex);

					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o, o.vertex);
					return o;
				}
 
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord);
					//-------------------------
					col.rgb *= _Color;
					col *= float4(_ColorMutl, _ColorMutl, _ColorMutl, _AplhaMutl);
					//-------------------------
					return col;
				}
 
			ENDCG
	  }
   }
}