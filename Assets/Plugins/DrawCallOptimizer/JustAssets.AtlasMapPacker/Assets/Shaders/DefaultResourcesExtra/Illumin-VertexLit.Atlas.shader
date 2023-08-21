// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Self-Illumin/VertexLit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Spec Color", 2D) = "white" {}
		_Shininess("Shininess", 2D) = "white" {}
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Illum("Illumin (A)", 2D) = "white" {}
		_Emission("Emission (Lightmapper)", Float) = 1.0
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}

SubShader {
    LOD 100
    Tags { "RenderType"="Opaque" }

    Pass {
        Name "BASE"
        Tags {"LightMode" = "Vertex"}
        Material {
            Diffuse [_Color]
            Shininess [_Shininess]
            Specular [_SpecColor]
        }
        SeparateSpecular On
        Lighting On
        SetTexture [_Illum] {
            constantColor [_Color]
            combine constant lerp (texture) previous
        }
        SetTexture [_MainTex] {
            constantColor (1,1,1,1)
            Combine texture * previous, constant // UNITY_OPAQUE_ALPHA_FFP
        }
    }

    // Extracts information for lightmapping, GI (emission, albedo, ...)
    // This pass it not used during regular rendering.
    Pass
    {
        Name "META"
        Tags { "LightMode" = "Meta" }
        CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
		#include "UnityCG.cginc"
		#include "UnityMetaPass.cginc"
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
			float2 uvMain : TEXCOORD0;
			float2 uvIllum : TEXCOORD1;
			float4 atlas : TEXCOORD2;
#ifdef EDITOR_VISUALIZATION
			float2 vizUV : TEXCOORD3;
			float4 lightCoord : TEXCOORD4;
#endif
			UNITY_VERTEX_OUTPUT_STEREO
		};


		float4 _MainTex_ST;
		float4 _Illum_ST;
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
			o.uvMain = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uvIllum = TRANSFORM_TEX(v.texcoord, _Illum);
#ifdef EDITOR_VISUALIZATION
			o.vizUV = 0;
			o.lightCoord = 0;
			if(unity_VisualizationMode == EDITORVIZ_TEXTURE) o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
			else if(unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
			{
				o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
			}

#endif
			o.atlas = v.texcoord2;
			return o;
		}


		sampler2D _MainTex;
		sampler2D _Illum;
		sampler2D _ColorMap;
		fixed _Emission;
		half4 frag(v2f i) : SV_Target
		{
			UnityMetaInput metaIN;
			UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
			
			fixed4 tex = tex2Datlas(_MainTex, i.uvMain, i.atlas);
			fixed4 c = tex * tex2Datlas(_ColorMap, i.pos, i.atlas);
			metaIN.Albedo = c.rgb;
			metaIN.Emission = c.rgb * tex2Datlas(_Illum, i.uvIllum, i.atlas).a;
#if defined(EDITOR_VISUALIZATION)
			metaIN.VizUV = i.vizUV;
			metaIN.LightCoord = i.lightCoord;
#endif
			return UnityMetaFragment(metaIN);
		}


        ENDCG
    }
}

Fallback "Legacy Shaders/VertexLit"
CustomEditor "LegacyIlluminShaderGUI"
}
