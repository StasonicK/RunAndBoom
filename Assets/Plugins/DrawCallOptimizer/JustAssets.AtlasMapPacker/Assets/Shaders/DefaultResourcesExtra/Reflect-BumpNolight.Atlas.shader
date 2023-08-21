// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Reflective/Bumped Unlit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_ReflectColorMap("Reflection Color", 2D) = "white" {}
		_MainTex("Base (RGB), RefStrength (A)", 2D) = "white" {}
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
        // Always drawn reflective pass
        Pass {
            Name "BASE"
            Tags {"LightMode" = "Always"}
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
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
			float4 atlas : TEXCOORD2;
			float3 I : TEXCOORD3;
			float3 TtoW0 : TEXCOORD4;
			float3 TtoW1 : TEXCOORD5;
			float3 TtoW2 : TEXCOORD6;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		float4 _MainTex_ST, _BumpMap_ST;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv2 = TRANSFORM_TEX(v.texcoord, _BumpMap);
			
			o.I =  - WorldSpaceViewDir(v.vertex);
			
			float3 worldNormal = UnityObjectToWorldNormal(v.normal);
			float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
			float3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
			o.TtoW0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
			o.TtoW1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
			o.TtoW2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
			
			UNITY_TRANSFER_FOG(o, o.pos);
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _BumpMap;
		sampler2D _MainTex;
		samplerCUBE _Cube;
		sampler2D _ReflectColorMap;
		sampler2D _ColorMap;
		fixed4 frag(v2f i) : SV_Target
		{
			// Sample and expand the normal map texture
			fixed3 normal = UnpackNormal(tex2Datlas(_BumpMap, i.uv2, i.atlas));
			
			fixed4 texcol = tex2Datlas(_MainTex, i.uv, i.atlas);
			
			// transform normal to world space
			half3 wn;
			wn.x = dot(i.TtoW0, normal);
			wn.y = dot(i.TtoW1, normal);
			wn.z = dot(i.TtoW2, normal);
			
			// calculate reflection vector in world space
			half3 r = reflect(i.I, wn);
			
			fixed4 c = UNITY_LIGHTMODEL_AMBIENT * texcol;
			
			fixed4 reflcolor = texCUBE(_Cube, r) * tex2Datlas(_ReflectColorMap, i.pos, i.atlas) * texcol.a;
			c = c + reflcolor;
			UNITY_APPLY_FOG(i.fogCoord, c);
			UNITY_OPAQUE_ALPHA(c.a);
			return c;
		}


ENDCG
        }
    }
}

FallBack "Legacy Shaders/VertexLit"
}
