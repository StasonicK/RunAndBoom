namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public readonly struct AtlasTile<T> : IAtlasTile
    {
        public AtlasTile(PixelSize size, T payload)
        {
            Size = size;
            Payload = payload;
        }

        public PixelSize Size { get; }

        object IAtlasTile.Payload => Payload;

        public IAtlasTile Clone(PixelSize newSize)
        {
            return new AtlasTile<T>(newSize, Payload);
        }

        public T Payload { get; }
    }
}