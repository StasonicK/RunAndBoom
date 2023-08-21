using System.Collections.Generic;
using System.Linq;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    class SkylineAtlasMapLayer : IAtlasMapLayer
    {
        public SkylineAtlasMapLayer(List<(PixelRect, IAtlasTile)> tiles, uint margin, PixelSize atlasSize)
        {

            Tiles = tiles.Select(x =>
            {
                var rectangle = new PixelRect(x.Item1.X + margin, x.Item1.Y + margin, x.Item1.Width - 2 * margin,
                    x.Item1.Height - 2 * margin);
                var uvRectangle = rectangle / atlasSize;
                return new AtlasMapEntry(rectangle, x.Item2.Payload, uvRectangle);
            }).ToList();
        }

        public List<AtlasMapEntry> Tiles { get; }
    }
}