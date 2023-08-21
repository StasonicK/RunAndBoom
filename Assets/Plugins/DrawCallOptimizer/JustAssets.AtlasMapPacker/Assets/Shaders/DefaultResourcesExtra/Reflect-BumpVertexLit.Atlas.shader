// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Reflective/Bumped VertexLit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Spec Color", 2D) = "white" {}
		_Shininess("Shininess", 2D) = "white" {}
		_ReflectColorMap("Reflection Color", 2D) = "white" {}
		_MainTex("Base (RGB) RefStrength (A)", 2D) = "white" {}
		_Cube("Reflection Cubemap", Cube) = "" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

Category {
    Tags { "RenderType"="Opaque" }
    LOD 250
    SubShader {
        UsePass "Reflective/Bumped Unlit/BASE"

        Pass {
            Tags { "LightMode" = "Vertex" }
            Blend One One ZWrite Off
            Lighting On

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

		struct v2f
		{
			float2 uv : TEXCOORD0;
			UNITY_FOG_COORDS(1) 
			fixed4 diff : COLOR0;
			float4 pos : SV_POSITION;
			UNITY_VERTEX_OUTPUT_STEREO
			float4 atlas : TEXCOORD2;
		};


		float4 _MainTex_ST;
		sampler2D _ColorMap;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.diff = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 0);
			UNITY_TRANSFER_FOG(o, o.pos)
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _MainTex;
		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 diff = tex2Datlas(_ColorMap, i.uv, i.atlas);
			fixed4 temp = tex2Datlas(_MainTex, i.uv, i.atlas);
			fixed4 c;
			c.xyz = (temp.xyz * i.diff.xyz * diff.xyz);
			c.w = temp.w * diff.w;
			UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(0, 0, 0, 0));
			// fog towards black due to our blend mode
			UNITY_OPAQUE_ALPHA(c.a);
			return c;
		}


ENDCG

        }
    }
}

FallBack "Legacy Shaders/Reflective/VertexLit"
}
