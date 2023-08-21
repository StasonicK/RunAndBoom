using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class CubicBSplineFilter : ResamplingFilter
    {
        private double temp;

        public CubicBSplineFilter()
        {
            defaultFilterRadius = 2;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x < 1)
            {
                temp = x * x;
                return 0.5 * temp * x - temp + 2f / 3f;
            }

            if (x < 2)
            {
                x = 2f - x;
                return Math.Pow(x, 3) / 6f;
            }

            return 0;
        }
    }
}