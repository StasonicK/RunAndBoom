using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class LanczosFilter : ResamplingFilter
    {
        public LanczosFilter(int windowSize)
        {
            defaultFilterRadius = windowSize;
        }

      
        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x < defaultFilterRadius) return SinC(x) * SinC(x / defaultFilterRadius);

            return 0;
        }

        private double SinC(double x)
        {
            if (Math.Abs(x) > 0.0001)
            {
                x *= Math.PI;
                return Math.Sin(x) / x;
            }

            return 1;
        }
    }
}