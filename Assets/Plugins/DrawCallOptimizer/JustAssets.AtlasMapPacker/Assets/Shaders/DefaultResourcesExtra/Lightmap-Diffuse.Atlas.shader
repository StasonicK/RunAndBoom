// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Lightmapped/Diffuse (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_MainTex("Base (RGB)", 2D) = "white" {}
		_LightMap("Lightmap (RGB)", 2D) = "black" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    LOD 200
    Tags { "RenderType" = "Opaque" }
CGPROGRAM
		#pragma surface surf Lambert nodynlightmap vertex:vert
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

		struct Input
		{
			float2 uv_MainTex;
			float2 uv2_LightMap;
			float4 atlas;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.atlas = v.texcoord1;
		}

		sampler2D _MainTex;
		sampler2D _LightMap;
		sampler2D _ColorMap;
		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = tex2Datlas(_MainTex, IN.uv_MainTex, IN.atlas).rgb * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas);
			half4 lm = tex2Datlas(_LightMap, IN.uv2_LightMap, IN.atlas);
			o.Emission = lm.rgb * o.Albedo.rgb;
			o.Alpha = lm.a * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas).a;
		}


ENDCG
}
FallBack "Legacy Shaders/Lightmapped/VertexLit"
}
