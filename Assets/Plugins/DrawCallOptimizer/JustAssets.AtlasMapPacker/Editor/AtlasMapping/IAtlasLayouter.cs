namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public interface IAtlasLayouter
    {
        float Coverage { get; }

        bool TryLayoutEntries(out IAtlasMapLayer layer);
    }
}