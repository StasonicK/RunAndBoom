// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - SUPPORTS lightmap
// - no lighting
// - no per-material color

Shader "Mobile/Unlit (Supports Lightmap) (Atlas)" {
Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    // Non-lightmapped
    Pass {
        Tags { "LightMode" = "Vertex" }
        Lighting Off
        SetTexture [_MainTex] {
            constantColor (1,1,1,1)
            combine texture, constant // UNITY_OPAQUE_ALPHA_FFP
        }
    }

    // Lightmapped
    Pass
    {
        Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }

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
        
		// uniforms		
		float4 _MainTex_ST;
        
		// vertex shader input data		
		struct appdata
		{
			float3 pos : POSITION;
			float3 uv1 : TEXCOORD1;
			float3 uv0 : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4 atlas : TEXCOORD2;
		};




        // vertex-to-fragment interpolators		
		struct v2f
		{
			float2 uv0 : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			float4 atlas : TEXCOORD2;
#if USING_FOG
			fixed fog : TEXCOORD3;
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
			
			// compute texture coordinates
			o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			o.uv1 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			
			// fog
#if USING_FOG
			float3 eyePos = UnityObjectToViewPos(float4(IN.pos, 1));
			float fogCoord = length(eyePos.xyz);
			// radial fog distance
			UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
			o.fog = saturate(unityFogFactor);
#endif
			// transform position
			o.pos = UnityObjectToClipPos(IN.pos);
			o.atlas = IN.atlas;
			return o;
		}




        // textures		
		sampler2D _MainTex;
        // fragment shader		
		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 col , tex;
			
			// Fetch lightmap
			half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
			col.rgb = DecodeLightmap(bakedColorTex);
			
			// Fetch color texture
			tex = tex2Datlas(_MainTex, IN.uv1.xy, IN.atlas);
			col.rgb = tex.rgb * col.rgb;
			col.a = 1;
			
			// fog
#if USING_FOG
			col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
			return col;
		}




        ENDCG
    }
}
}
