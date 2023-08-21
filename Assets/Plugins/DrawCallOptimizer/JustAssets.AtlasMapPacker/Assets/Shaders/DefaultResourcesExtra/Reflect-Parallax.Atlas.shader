// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Reflective/Parallax Diffuse (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_ReflectColorMap("Reflection Color", 2D) = "white" {}
		_Parallax("Height", 2D) = "black" {}
		_MainTex("Base (RGB) RefStrength (A)", 2D) = "white" {}
		_Cube("Reflection Cubemap", Cube) = "_Skybox" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
	    _BumpMap ("Normalmap", 2D) = "bump" {}
		_ParallaxMap ("Heightmap (A)", 2D) = "black" {}
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 500

CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0
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

		sampler2D _MainTex;
		sampler2D _BumpMap;
		samplerCUBE _Cube;
		sampler2D _ParallaxMap;
		sampler2D _ColorMap;
		sampler2D _ReflectColorMap;
		sampler2D _Parallax;
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldRefl;
			float3 viewDir;
			INTERNAL_DATA
			float4 atlas;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.atlas = v.texcoord2;
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			half h = tex2Datlas(_ParallaxMap, IN.uv_BumpMap, IN.atlas).w;
			float2 offset = ParallaxOffset(h, tex2Datlas(_Parallax, IN.uv_MainTex, IN.atlas).x, IN.viewDir);
			IN.uv_MainTex += offset;
			IN.uv_BumpMap += offset;
			
			fixed4 tex = tex2Datlas(_MainTex, IN.uv_MainTex, IN.atlas);
			fixed4 c = tex * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas);
			o.Albedo = c.rgb;
			
			o.Normal = UnpackNormal(tex2Datlas(_BumpMap, IN.uv_BumpMap, IN.atlas));
			
			float3 worldRefl = WorldReflectionVector(IN, o.Normal);
			fixed4 reflcol = texCUBE(_Cube, worldRefl);
			reflcol *= tex.a;
			o.Emission = reflcol.rgb * tex2Datlas(_ReflectColorMap, IN.uv_MainTex, IN.atlas).rgb;
			o.Alpha = reflcol.a * tex2Datlas(_ReflectColorMap, IN.uv_MainTex, IN.atlas).a;
		}


ENDCG
}

FallBack "Legacy Shaders/Reflective/Bumped Diffuse"
}
