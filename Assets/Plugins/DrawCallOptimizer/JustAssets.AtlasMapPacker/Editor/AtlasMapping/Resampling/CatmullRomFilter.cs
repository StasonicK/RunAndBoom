namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class CatmullRomFilter : ResamplingFilter
    {
        private const double C = 1 / 2;

        private double temp;

        public CatmullRomFilter()
        {
            defaultFilterRadius = 2;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            temp = x * x;
            if (x <= 1) return 1.5 * temp * x - 2.5 * temp + 1;
            if (x <= 2) return -0.5 * temp * x + 2.5 * temp - 4 * x + 2;

            return 0;
        }
    }
}