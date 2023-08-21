// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Transparent/Parallax Specular (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Specular Color", 2D) = "black" {}
		_Shininess_Parallax("Shininess|Height", 2D) = "black" {}
		_MainTex("Base (RGB) TransGloss (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_ParallaxMap("Heightmap (A)", 2D) = "black" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 600

CGPROGRAM
		#pragma surface surf BlinnPhong alpha:fade vertex:vert
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
		sampler2D _ParallaxMap;
		sampler2D _ColorMap;
		sampler2D _Shininess_Parallax;
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
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
			float2 offset = ParallaxOffset(h, tex2Datlas(_Shininess_Parallax, IN.uv_MainTex, IN.atlas).y, IN.viewDir);
			IN.uv_MainTex += offset;
			IN.uv_BumpMap += offset;
			
			fixed4 tex = tex2Datlas(_MainTex, IN.uv_MainTex, IN.atlas);
			o.Albedo = tex.rgb * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas).rgb;
			o.Gloss = tex.a;
			o.Alpha = tex.a * tex2Datlas(_ColorMap, IN.uv_MainTex, IN.atlas).a;
			o.Specular = tex2Datlas(_Shininess_Parallax, IN.uv_MainTex, IN.atlas).x;
			o.Normal = UnpackNormal(tex2Datlas(_BumpMap, IN.uv_BumpMap, IN.atlas));
		}


ENDCG
}

FallBack "Legacy Shaders/Transparent/Bumped Specular"
}
