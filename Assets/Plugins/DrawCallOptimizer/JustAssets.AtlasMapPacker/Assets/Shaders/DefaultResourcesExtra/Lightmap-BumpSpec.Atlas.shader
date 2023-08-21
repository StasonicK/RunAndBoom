// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Lightmapped/Bumped Specular (Atlas)" {
    Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_SpecColorMap("Specular Color", 2D) = "white" {}
		_Shininess("Shininess", 2D) = "black" {}
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_LightMap("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode }
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
    }
    FallBack "Legacy Shaders/Lightmapped/Bumped Diffuse"
}
