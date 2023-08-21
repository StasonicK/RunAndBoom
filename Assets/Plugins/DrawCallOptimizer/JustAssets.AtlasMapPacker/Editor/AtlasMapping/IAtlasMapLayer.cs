using System.Collections.Generic;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public interface IAtlasMapLayer
    {
        List<AtlasMapEntry> Tiles { get; }
    }
}