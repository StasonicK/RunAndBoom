using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class BellFilter : ResamplingFilter
    {
        public BellFilter()
        {
            defaultFilterRadius = 1.5;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x < 0.5) return 0.75 - x * x;
            if (x < 1.5) return 0.5 * Math.Pow(x - 1.5, 2);

            return 0;
        }
    }
}