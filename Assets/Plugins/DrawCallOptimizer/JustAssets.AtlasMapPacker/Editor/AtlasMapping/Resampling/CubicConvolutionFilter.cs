namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class CubicConvolutionFilter : ResamplingFilter
    {
        private double temp;

        public CubicConvolutionFilter()
        {
            defaultFilterRadius = 3;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            temp = x * x;
            if (x <= 1) return 4f / 3f * temp * x - 7f / 3f * temp + 1;
            if (x <= 2) return -(7f / 12f) * temp * x + 3 * temp - 59f / 12f * x + 2.5;
            if (x <= 3) return 1f / 12f * temp * x - 2f / 3f * temp + 1.75 * x - 1.5;

            return 0;
        }
    }
}