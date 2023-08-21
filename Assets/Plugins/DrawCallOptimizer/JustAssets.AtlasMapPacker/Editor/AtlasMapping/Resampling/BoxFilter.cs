namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class BoxFilter : ResamplingFilter
    {
        public BoxFilter()
        {
            defaultFilterRadius = 0.5;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            if (x <= 0.5) return 1;

            return 0;
        }
    }
}