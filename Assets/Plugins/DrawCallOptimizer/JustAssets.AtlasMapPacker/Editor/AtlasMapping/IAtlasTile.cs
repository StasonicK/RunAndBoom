namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public interface IAtlasTile
    {
        PixelSize Size { get; }

        object Payload { get; }

        IAtlasTile Clone(PixelSize newSize);
    }
}