// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Lightmapped/VertexLit (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Spec Color", 2D) = "white" {}
		_Shininess("Shininess", 2D) = "white" {}
		_MainTex("Base (RGB)", 2D) = "white" {}
		_LightMap("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode }
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

        Lighting On
        SeparateSpecular On

        BindChannels {
            Bind "Vertex", vertex
            Bind "normal", normal
            Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
            Bind "texcoord1", texcoord1 // lightmap uses 2nd uv
            Bind "texcoord", texcoord2 // main uses 1st uv
        }

        SetTexture [_LightMap] {
            constantColor [_Color]
            combine texture * constant
        }
        SetTexture [_LightMap] {
            constantColor (0.5,0.5,0.5,0.5)
            combine previous * constant + primary
        }
        SetTexture [_MainTex] {
            constantColor (1,1,1,1)
            combine texture * previous DOUBLE, constant // UNITY_OPAQUE_ALPHA_FFP
        }
    }
}

Fallback "VertexLit"
}
