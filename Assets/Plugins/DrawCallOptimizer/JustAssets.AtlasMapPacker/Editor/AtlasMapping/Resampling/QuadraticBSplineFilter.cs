namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class QuadraticBSplineFilter : ResamplingFilter
    {
        public QuadraticBSplineFilter()
        {
            defaultFilterRadius = 1.5;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x <= 0.5) return -x * x + 0.75;
            if (x <= 1.5) return 0.5 * x * x - 1.5 * x + 1.125;

            return 0;
        }
    }
}