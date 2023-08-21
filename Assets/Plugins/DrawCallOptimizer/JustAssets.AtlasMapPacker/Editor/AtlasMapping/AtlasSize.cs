namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public readonly struct AtlasSize
    {
        public float Width { get; }
        public float Height { get; }
        public float SqrMagnitude => Width * Width + Height * Height;
        public static AtlasSize Zero => new AtlasSize(0, 0);

        public AtlasSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public static AtlasSize operator *(AtlasSize a, float b)
        {
            return new AtlasSize(a.Width * b, a.Height * b);
        }
    }
}