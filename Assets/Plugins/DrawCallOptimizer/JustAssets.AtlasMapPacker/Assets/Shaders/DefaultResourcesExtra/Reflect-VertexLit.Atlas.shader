// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Reflective/VertexLit (Atlas)"
{
    Properties
    {
		_ColorMap("Main Color", 2D) = "white" {}
		_ReflectColorMap("Reflection Color", 2D) = "white" {}
		_MainTex("Base (RGB) RefStrength (A)", 2D) = "white" {}
		_Cube("Reflection Cubemap", Cube) = "_Skybox" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
    }

    Category
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 150

        SubShader
        {

            // First pass does reflection cubemap
            Pass
            {
                Name "BASE"
                Tags{ "LightMode" = "Always" }

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
		#pragma multi_compile_fog
		#include "UnityCG.cginc"
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 I : TEXCOORD1;
			UNITY_FOG_COORDS(3)
			
			UNITY_VERTEX_OUTPUT_STEREO
			float4 atlas : TEXCOORD2;
		};


		float4 _MainTex_ST;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			
			// calculate world space reflection vector
			float3 viewDir = WorldSpaceViewDir(v.vertex);
			float3 worldN = UnityObjectToWorldNormal(v.normal);
			o.I = reflect( - viewDir, worldN);
			
			UNITY_TRANSFER_FOG(o, o.pos);
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _MainTex;
		samplerCUBE _Cube;
		sampler2D _ReflectColorMap;
		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 texcol = tex2Datlas(_MainTex, i.uv, i.atlas);
			fixed4 reflcol = texCUBE(_Cube, i.I);
			reflcol *= texcol.a;
			fixed4 col = reflcol * tex2Datlas(_ReflectColorMap, i.pos, i.atlas);
			UNITY_APPLY_FOG(i.fogCoord, col);
			UNITY_OPAQUE_ALPHA(col.a);
			return col;
		}




            ENDCG

        }

        // Vertex Lit, emulated in shaders (4 lights max, no specular)
        Pass
        {
            Tags{ "LightMode" = "Vertex" }
            Blend One One ZWrite Off
            Lighting On

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
		#pragma multi_compile_fog
		#include "UnityCG.cginc"
		struct v2f
		{
			float2 uv : TEXCOORD0;
			UNITY_FOG_COORDS(1) 
			fixed4 diff : COLOR0;
			float4 pos : SV_POSITION;
			float4 atlas: TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		float4 _MainTex_ST;
		sampler2D _ColorMap;
		sampler2D _ReflectColorMap;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.diff = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 0);
			UNITY_TRANSFER_FOG(o, o.pos);
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _MainTex;
		fixed4 frag(v2f i) : SV_Target
		{
			float reflColor = tex2Datlas(_ReflectColorMap, i.uv, i.atlas).w;
			float4 color = tex2Datlas(_ColorMap, i.uv, i.atlas);
			fixed4 temp = tex2Datlas(_MainTex, i.uv, i.atlas);
			fixed4 c;
			c.xyz = temp.xyz * i.diff.xyz * color.xyz;
			c.w = temp.w * reflColor;
			UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(0, 0, 0, 0));
			// fog towards black due to our blend mode
			UNITY_OPAQUE_ALPHA(c.a);
			return c;
		}

            ENDCG
        }

        // Lightmapped
        Pass
        {
            Tags{ "LightMode" = "VertexLM" }
            Blend One One ZWrite Off
            ColorMask RGB

            CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
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

		struct a2v
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
			float4 atlas : TEXCOORD2;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		struct v2f
		{
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
			float4 atlas : TEXCOORD2;
			UNITY_FOG_COORDS(3) 
			float4 pos : SV_POSITION;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		float4 _MainTex_ST;
		float4x4 unity_LightmapMatrix;
		v2f vert(a2v v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.uv2 = mul(unity_LightmapMatrix, float4(v.uv2, 0, 1)).xy;
			o.atlas = v.atlas;
			UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}


		sampler2D _MainTex;
		sampler2D _ColorMap;
		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv2);
			fixed4 lm = fixed4(DecodeLightmap(bakedColorTex), 1);
			lm *= tex2Datlas(_ColorMap, i.uv, i.atlas);
			fixed4 c = tex2Datlas(_MainTex, i.uv, i.atlas);
			c.rgb *= lm.rgb;
			UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(0, 0, 0, 0));
			UNITY_OPAQUE_ALPHA(c.a);
			return c;
		}


            ENDCG
        }
    }
    }

    FallBack "Legacy Shaders/VertexLit"
}
