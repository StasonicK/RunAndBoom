namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class QuadraticFilter : ResamplingFilter
    {
        public QuadraticFilter()
        {
            defaultFilterRadius = 1.5;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x <= 0.5) return -2 * x * x + 1;
            if (x <= 1.5) return x * x - 2.5 * x + 1.5;

            return 0;
        }
    }
}