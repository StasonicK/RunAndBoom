using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class CosineFilter : ResamplingFilter
    {
        public CosineFilter()
        {
            defaultFilterRadius = 1;
        }

        public override double GetValue(double x)
        {
            if (x >= -1 && x <= 1) return (Math.Cos(x * Math.PI) + 1) / 2f;

            return 0;
        }
    }
}