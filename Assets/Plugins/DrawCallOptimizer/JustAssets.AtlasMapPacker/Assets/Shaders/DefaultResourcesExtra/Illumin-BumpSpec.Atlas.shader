// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Self-Illumin/Bumped Specular (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Specular Color", 2D) = "white" {}
		[PowerSlider(5.0)] _Shininess("Shininess", 2D) = "black" {}
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Illum("Illumin (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Emission("Emission (Lightmapper)", Float) = 1.0
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 400
CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
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

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Illum;
		sampler2D _ColorMap;
		sampler2D _Shininess;
		fixed _Emission;
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_Illum;
			float2 uv_BumpMap;
			float4 atlas;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.atlas = v.texcoord1;
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 tex = tex2Datlas(_MainTex, IN.uv_MainTex, IN.atlas);
			fixed4 c = tex * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas);
			o.Albedo = c.rgb;
			o.Emission = c.rgb * tex2Datlas(_Illum, IN.uv_Illum, IN.atlas).a;
#if defined (UNITY_PASS_META)
			o.Emission *= _Emission.rrr;
#endif
			o.Gloss = tex.a;
			o.Alpha = c.a;
			o.Specular = tex2Datlas(_Shininess, IN.uv_MainTex, IN.atlas).x;
			o.Normal = UnpackNormal(tex2Datlas(_BumpMap, IN.uv_BumpMap, IN.atlas));
		}


ENDCG
}
FallBack "Legacy Shaders/Self-Illumin/Specular"
CustomEditor "LegacyIlluminShaderGUI"
}
