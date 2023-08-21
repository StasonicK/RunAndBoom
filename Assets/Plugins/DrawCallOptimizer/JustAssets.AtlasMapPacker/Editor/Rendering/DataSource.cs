namespace JustAssets.AtlasMapPacker.Rendering
{
    public enum DataSource
    {
        Invalid = -1,

        Color = 1,

        UV3 = 2,

        UV4,

        UV5,

        UV6,

        UV7,

        TextureAttribute,

        FloatAttribute,

        VectorAttribute
    }

    public static class DataSourceUtility
    {
        public static bool IsTexCoord(this DataSource source)
        {
            if (source >= DataSource.Color && source <= DataSource.UV7)
                return true;

            return false;
        }
    }
}