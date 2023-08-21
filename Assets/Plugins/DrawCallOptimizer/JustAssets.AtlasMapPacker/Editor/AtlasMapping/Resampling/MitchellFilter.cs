namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    internal class MitchellFilter : ResamplingFilter
    {
        private const double C = 1 / 3;

        private double temp;

        public MitchellFilter()
        {
            defaultFilterRadius = 2;
        }

        public override double GetValue(double x)
        {
            if (x < 0) x = -x;
            temp = x * x;
            if (x < 1)
            {
                x = (12 - 9 * C - 6 * C) * (x * temp) + (-18 + 12 * C + 6 * C) * temp + (6 - 2 * C);
                return x / 6;
            }

            if (x < 2)
            {
                x = (-C - 6 * C) * (x * temp) + (6 * C + 30 * C) * temp + (-12 * C - 48 * C) * x + (8 * C + 24 * C);
                return x / 6;
            }

            return 0;
        }
    }
}