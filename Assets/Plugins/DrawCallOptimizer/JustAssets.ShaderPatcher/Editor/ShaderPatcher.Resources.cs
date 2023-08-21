using System.Collections.Generic;

namespace JustAssets.ShaderPatcher
{
    public partial class ShaderPatcher
    {
        private List<string> _ignoredMethods = new List<string>
        {
            "pickUVlod",
            "tex2Datlas"
        };

        private const string InputStructDefaultName = "INPUTSTRUCT";

        private const string VertexProgramTemplate = 
            @"void vert (inout appdata_full v, out INPUTSTRUCT o) 
		    {
			    UNITY_INITIALIZE_OUTPUT(Input, o);
		    }";

        private const string AtlasCode = 
            @"float4 pickUVlod(float2 uv, float4 bounds)
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
            ";
    }
}