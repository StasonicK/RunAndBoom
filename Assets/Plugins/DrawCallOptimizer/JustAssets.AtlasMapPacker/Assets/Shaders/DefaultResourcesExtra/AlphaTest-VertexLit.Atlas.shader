// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Transparent/Cutout/VertexLit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Spec Color", 2D) = "white" {}
		_EmissionMap("Emissive Color", 2D) = "black" {}
		_Shininess_Cutoff("Shininess|Alpha cutoff", 2D) = "white" {}
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100

    // Non-lightmapped
    Pass {
        Tags { "LightMode" = "Vertex" }
        Alphatest Greater [_Cutoff]
        AlphaToMask True
        ColorMask RGB
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
        Tags{ "LIGHTMODE" = "VertexLM" "QUEUE" = "AlphaTest" "IGNOREPROJECTOR" = "true" "RenderType" = "TransparentCutout" }
        AlphaToMask On
        ColorMask RGB

        CGPROGRAM
		float _MaxMipLevel;
		float _AtlasHeight;
		float _AtlasWidth;
		float4 pickUVlod(float2 uv, float4 bounds)
		{
			float2 relativeSize = frac(bounds.zw);
			float2 scale = floor(bounds.zw) / 1000;
			float2 fracuv = frac(uv.xy * scale) * relativeSize;
			float2 uv_atlas = fracuv + bounds.xy;
			float2 subTextureSize = relativeSize * scale * float2(_AtlasWidth, _AtlasHeight);
			float2 dx = ddx(uv * subTextureSize.x);
			float2 dy = ddy(uv * subTextureSize.y);
			int d = max(dot(dx, dx), dot(dy, dy));
			
			const float rangeClamp = pow(2, _MaxMipLevel * 2);
			d = clamp(d, 1.0, rangeClamp);
			
			float mipLevel = 0.5 * log2(d);
			mipLevel = floor(mipLevel);
			
			return float4(uv_atlas, 0, mipLevel);
		}

		float4 tex2Datlas(sampler2D tex, float2 uv, float4 atlas)
		{
			return tex2Dlod(tex, pickUVlod(uv, atlas));
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
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4 atlas : TEXCOORD2;
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
			o.atlas = IN.atlas;
			return o;
		}


		sampler2D _MainTex;
		sampler2D _ColorMap;
		sampler2D _Shininess_Cutoff;
		fixed4 frag(v2f IN) : SV_Target
		{
			half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
			half4 bakedColor = half4(DecodeLightmap(bakedColorTex), 1.0);
			
			fixed4 col = bakedColor * tex2Datlas(_ColorMap, IN.color, IN.atlas);
			
			fixed4 tex = tex2Datlas(_MainTex, IN.uv2.xy, IN.atlas);
			
			col.rgb = tex.rgb * col.rgb;
			col.a = tex.a * IN.color.a;
			
			clip(col.a - tex2Datlas(_Shininess_Cutoff, IN.color, IN.atlas).y);
			
#if USING_FOG
			col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
			return col;
		}




        ENDCG
    }

    // Pass to render object as a shadow caster
    Pass {
        Name "Caster"
        Tags { "LightMode" = "ShadowCaster" }

CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
		#pragma multi_compile_shadowcaster
		#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
		#include "UnityCG.cginc"
		float _MaxMipLevel;
		float _AtlasHeight;
		float _AtlasWidth;
		float4 pickUVlod(float2 uv, float4 bounds)
		{
			float2 relativeSize = frac(bounds.zw);
			float2 scale = floor(bounds.zw) / 1000;
			float2 fracuv = frac(uv.xy * scale) * relativeSize;
			float2 uv_atlas = fracuv + bounds.xy;
			float2 subTextureSize = relativeSize * scale * float2(_AtlasWidth, _AtlasHeight);
			float2 dx = ddx(uv * subTextureSize.x);
			float2 dy = ddy(uv * subTextureSize.y);
			int d = max(dot(dx, dx), dot(dy, dy));
			
			const float rangeClamp = pow(2, _MaxMipLevel * 2);
			d = clamp(d, 1.0, rangeClamp);
			
			float mipLevel = 0.5 * log2(d);
			mipLevel = floor(mipLevel);
			
			return float4(uv_atlas, 0, mipLevel);
		}

		float4 tex2Datlas(sampler2D tex, float2 uv, float4 atlas)
		{
			return tex2Dlod(tex, pickUVlod(uv, atlas));
		}

		struct v2f
		{
			V2F_SHADOW_CASTER;
			float2 uv : TEXCOORD1;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		float4 _MainTex_ST;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			TRANSFER_SHADOW_CASTER_NORMALOFFSET(o) o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _MainTex;
		sampler2D _Shininess_Cutoff;
		sampler2D _ColorMap;
		float4 frag(v2f i) : SV_Target
		{
			fixed4 texcol = tex2Datlas(_MainTex, i.uv, i.atlas);
			clip(texcol.a * tex2Datlas(_ColorMap, i.uv, i.atlas).a - tex2Datlas(_Shininess_Cutoff, i.uv, i.atlas).y);
			
			SHADOW_CASTER_FRAGMENT(i)
		}


ENDCG

    }

}

}
