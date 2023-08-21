// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

/*
Renders doubled sides objects without lighting. Useful for
grass, trees or foliage.

This shader renders two passes for all geometry, one
for opaque parts and one with semitransparent details.

This makes it possible to render transparent objects
like grass without them being sorted by depth.
*/

Shader "Legacy Shaders/Transparent/Cutout/Soft Edge Unlit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff("Base Alpha cutoff", 2D) = "white" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
    Lighting off

    // Render both front and back facing polygons.
    Cull Off

    // first pass:
    //   render any pixels that are more than [_Cutoff] opaque
    Pass {
        CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog
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

		struct appdata_t
		{
			float4 vertex : POSITION;
			float4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		struct v2f
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _Cutoff;
		v2f vert(appdata_t v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			UNITY_TRANSFER_FOG(o, o.vertex);
			o.atlas = v.atlas;
			return o;
		}


		sampler2D _ColorMap;
		float4 frag(v2f i) : SV_Target
		{
			float4 col = tex2Datlas(_ColorMap, i.texcoord, i.atlas) * tex2Datlas(_MainTex, i.texcoord, i.atlas);
			clip(col.a - tex2Datlas(_Cutoff, i.texcoord, i.atlas).x);
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}


        ENDCG
    }

    // Second pass:
    //   render the semitransparent details.
    Pass {
        Tags { "RequireOption" = "SoftVegetation" }

        // Dont write to the depth buffer
        ZWrite off

        // Set up alpha blending
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog
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

		struct appdata_t
		{
			float4 vertex : POSITION;
			float4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		struct v2f
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _Cutoff;
		v2f vert(appdata_t v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.atlas = v.atlas;
			UNITY_TRANSFER_FOG(o, o.vertex);
			return o;
		}


		sampler2D _ColorMap;
		fixed4 frag(v2f i) : SV_Target
		{
			half4 col = tex2Datlas(_ColorMap, i.texcoord, i.atlas) * tex2Datlas(_MainTex, i.texcoord, i.atlas);
			clip( - (col.a - tex2Datlas(_Cutoff, i.texcoord, i.atlas).x));
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}


        ENDCG
    }
}

}
