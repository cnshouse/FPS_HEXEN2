// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unity GL/Interlace Pattern Additive Fog Off" {
// 用来显示uv动画的mask 修改字愤怒的机器人 add by lpj 2014 // 
	Properties {
	_TintColor ("TintColor", Color) = (1,1,1,1) // needed simply for shader replacement   
        _MainTex ("Base", 2D) = "white" {}
        
        _InterlacePattern ("InterlacePattern", 2D) = "white" {}
        
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _InterlacePattern;
						
		half4 _InterlacePattern_ST;
		fixed4 _TintColor;				
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv2 : TEXCOORD1;
			fixed4 color : COLOR;
		};

		v2f vert(appdata_full v)
		{
			v2f o;
			
			o.pos = UnityObjectToClipPos (v.vertex);	
			o.color = v.color * _TintColor;
			o.uv.xy = v.texcoord.xy;
			o.uv2.xy = TRANSFORM_TEX(v.texcoord.xy, _InterlacePattern) + _Time.xx * _InterlacePattern_ST.zw;
					
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR
		{	
			fixed4 colorTex = tex2D (_MainTex, i.uv) * i.color;
			fixed4 interlace = tex2D (_InterlacePattern, i.uv2);
			colorTex *= interlace;
			
			return colorTex;
		}
	
	ENDCG
	
	SubShader {
    	Tags {"RenderType" = "Transparent" "Queue" = "Transparent" "Reflection" = "RenderReflectionTransparentAdd" }
		Cull Off
		ZWrite Off
       	Blend One One
       	Fog { Mode Off }
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
