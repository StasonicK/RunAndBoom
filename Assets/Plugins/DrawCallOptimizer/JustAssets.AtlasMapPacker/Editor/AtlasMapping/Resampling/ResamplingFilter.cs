namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    public abstract class ResamplingFilter
    {
        public double defaultFilterRadius;

        public static ResamplingFilter Create(ResamplingFilters filter)
        {
            ResamplingFilter resamplingFilter = null;

            switch (filter)
            {
                case ResamplingFilters.Box:
                    resamplingFilter = new BoxFilter();
                    break;
                case ResamplingFilters.Triangle:
                    resamplingFilter = new TriangleFilter();
                    break;
                case ResamplingFilters.Hermite:
                    resamplingFilter = new HermiteFilter();
                    break;
                case ResamplingFilters.Bell:
                    resamplingFilter = new BellFilter();
                    break;
                case ResamplingFilters.CubicBSpline:
                    resamplingFilter = new CubicBSplineFilter();
                    break;
                case ResamplingFilters.Lanczos1:
                    resamplingFilter = new LanczosFilter(1);
                    break;
                case ResamplingFilters.Lanczos2:
                    resamplingFilter = new LanczosFilter(2);
                    break;
                case ResamplingFilters.Lanczos5:
                    resamplingFilter = new LanczosFilter(5);
                    break;
                case ResamplingFilters.Lanczos3:
                    resamplingFilter = new LanczosFilter(3);
                    break;
                case ResamplingFilters.Mitchell:
                    resamplingFilter = new MitchellFilter();
                    break;
                case ResamplingFilters.Cosine:
                    resamplingFilter = new CosineFilter();
                    break;
                case ResamplingFilters.CatmullRom:
                    resamplingFilter = new CatmullRomFilter();
                    break;
                case ResamplingFilters.Quadratic:
                    resamplingFilter = new QuadraticFilter();
                    break;
                case ResamplingFilters.QuadraticBSpline:
                    resamplingFilter = new QuadraticBSplineFilter();
                    break;
                case ResamplingFilters.CubicConvolution:
                    resamplingFilter = new CubicConvolutionFilter();
                    break;
                case ResamplingFilters.Lanczos8:
                    resamplingFilter = new LanczosFilter(8);
                    break;
            }

            return resamplingFilter;
        }

        public abstract double GetValue(double x);
    }
}