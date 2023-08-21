namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class HermiteFilter : ResamplingFilter
    {
        public HermiteFilter()
        {
            defaultFilterRadius = 1;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x < 1) return (2 * x - 3) * x * x + 1;

            return 0;
        }
    }
}