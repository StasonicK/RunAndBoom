// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Transparent/VertexLit (Atlas)" {
Properties {
		_Color("Main Color", 2D) = "white" {}
		_SpecColor("Spec Color", 2D) = "white" {}
		_Emission("Emissive Color", 2D) = "black" {}
		[PowerSlider(5.0)] _Shininess("Shininess", 2D) = "white" {}
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB

    // Non-lightmapped
    Pass {
        Tags { "LightMode" = "Vertex" }
        Material {
            Diffuse [_Color]
            Ambient [_Color]
            Shininess [_Shininess]
            Specular [_SpecColor]
            Emission [_Emission]
        }
        Lighting On
        SeparateSpecular On
        SetTexture [_MainTex] {
            Combine texture * primary DOUBLE, texture * primary
        }
    }

    // Lightmapped
    Pass
    {
        Tags{ "LIGHTMODE" = "VertexLM" "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB

        CGPROGRAM
		float _MaxMipLevel;
		float _AtlasHeight;
		float _AtlasWidth;
		float4 pickUVlod(float2 uv, float4 bounds)
		{
			float2 fracuv = frac(uv.xy + floor(bounds.zw) / 1000) * frac(bounds.zw);
			float2 uv_atlas = fracuv + bounds.xy;
			float2 subTextureSize = bounds.zw * float2(_AtlasWidth, _AtlasHeight);
			float2 dx = ddx(uv * subTextureSize.x);
			float2 dy = ddy(uv * subTextureSize.y);
			int d = max(dot(dx, dx), dot(dy, dy));
			
			const float rangeClamp = pow(2, _MaxMipLevel * 2);
			d = clamp(d, 1.0, rangeClamp);
			
			float mipLevel = 0.5 * log2(d);
			mipLevel = floor(mipLevel);
			
			return float4(uv_atlas, 0, mipLevel);
		}

		float4 pickUV(float2 uv, float4 bounds)
		{
			float2 fracuv = frac(uv.xy + floor(bounds.zw) / 1000) * frac(bounds.zw);
			float2 uv_atlas = fracuv + bounds.xy;
			
			return float4(uv_atlas, 0, 0);
		}

		float4 tex2Datlas(sampler2D tex, float2 uv, float4 atlas)
		{
			return tex2Dlod(tex, pickUVlod(uv, atlas));
		}

		float4 tex2Dnomip(sampler2D tex, float2 uv, float4 atlas)
		{
			return tex2Dlod(tex, pickUV(uv, atlas));
		}


		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
		#include "UnityCG.cginc"
		#pragma multi_compile_fog
		#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
		float4 unity_Lightmap_ST;
		float4 _MainTex_ST;
		struct appdata
		{
			float3 pos : POSITION;
			half4 color : COLOR;
			float3 uv1 : TEXCOORD1;
			float3 uv0 : TEXCOORD0;
			float4 uv2 : TEXCOORD2;
			
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		struct v2f
		{
			fixed4 color : COLOR0;
			float2 uv0 : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			float4 atlas : TEXCOORD2;
			float2 uv2 : TEXCOORD3;
#if USING_FOG
			fixed fog : TEXCOORD4;
#endif
			float4 pos : SV_POSITION;
			
			UNITY_VERTEX_OUTPUT_STEREO
		};




        // vertex shader
		v2f vert(appdata IN)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			half4 color = IN.color;
			float3 eyePos = UnityObjectToViewPos(IN.pos);
			half3 viewDir = 0.0;
			o.color = saturate(color);
			
			o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			o.uv1 = IN.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
			o.uv2 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			
#if USING_FOG
			float fogCoord = length(eyePos.xyz);
			UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
			o.fog = saturate(unityFogFactor);
#endif
			o.pos = UnityObjectToClipPos(IN.pos);
			o.atlas = IN.uv2;
			return o;
		}


		sampler2D _MainTex;
		sampler2D _Color;
		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 col , tex;
			tex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
			col = fixed4(DecodeLightmap(tex), 1);
			col = col * tex2Datlas(_Color, IN.color, IN.atlas).x;
			tex = tex2Datlas(_MainTex, IN.uv2.xy, IN.atlas);
			col.rgb = tex * col;
			col.a = tex.a * IN.color.a;
			
#if USING_FOG
			col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
			return col;
		}




        ENDCG
    }
}

}
