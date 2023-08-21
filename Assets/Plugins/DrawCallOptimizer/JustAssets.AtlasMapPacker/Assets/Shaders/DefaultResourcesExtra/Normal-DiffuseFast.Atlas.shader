// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Does not do anything in 3.x
Shader "Legacy Shaders/Diffuse Fast (Atlas)" {
Properties {
		_ColorMap("Main Color", 2D) = "white" {}
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaxMipLevel("Max Mip Level", float) = 3
		_AtlasHeight("Atlas Height", float) = 2048
		_AtlasWidth("Atlas Width", float) = 2048
}
Fallback "Legacy Shaders/VertexLit"
}
