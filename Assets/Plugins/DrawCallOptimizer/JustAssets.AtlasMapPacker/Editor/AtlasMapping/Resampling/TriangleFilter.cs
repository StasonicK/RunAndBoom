namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class TriangleFilter : ResamplingFilter
    {
        public TriangleFilter()
        {
            defaultFilterRadius = 1;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x < 1) return 1 - x;

            return 0;
        }
    }
}