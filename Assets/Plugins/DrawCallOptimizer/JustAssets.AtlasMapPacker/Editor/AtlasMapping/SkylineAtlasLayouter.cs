using System;
using System.Collections.Generic;
using System.Linq;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal class SkylineAtlasLayouter : IAtlasLayouter
    {
        private readonly List<IAtlasTile> _input;
        private readonly PixelSize _atlasSize;
        private readonly uint _margin;
        private readonly PixelSize _maxTileSize;
        private readonly float _tileScale;

        private readonly Action<string, float> _progress;

        public float Coverage { get; private set; }

        public SkylineAtlasLayouter(List<IAtlasTile> input,
            PixelSize atlasSize,
            uint margin,
            PixelSize maxTileSize,
            float tileScale, Action<string, float> progress)
        {
            if (atlasSize.Width <= margin * 2 || atlasSize.Height <= margin * 2)
            {
                throw new Exception("The dimensions need to be larger than twice the margin.");
            }

            _input = input;
            _atlasSize = atlasSize;
            _margin = margin;
            _maxTileSize = maxTileSize;
            _tileScale = tileScale;
            _progress = progress;
        }

        public bool TryLayoutEntries(out IAtlasMapLayer layer)
        {
            var coverage = 0f;
            
            _input.Sort((x, y) => y.Size.SqrMagnitude.CompareTo(x.Size.SqrMagnitude));
            var changedTiles = _input.Select(element =>
            {
                PixelSize tileSize = element.Size;

                double verticalScale = _maxTileSize.Height / (double)tileSize.Height;
                double horizontalScale = _maxTileSize.Width / (double)tileSize.Width;
                double scale = Math.Min(1, Math.Min(horizontalScale, verticalScale));
                tileSize *= scale;

                var width = tileSize.Height * _tileScale + _margin * 2;
                var height = tileSize.Width * _tileScale + _margin * 2;
                var rectangle = new PixelSize((long)width, (long)height);

                coverage += width * height / (_atlasSize.Width * _atlasSize.Height);

                return element.Clone(rectangle);
            }).ToList();

            var skylinePacker = new SkylineBinPacker(_atlasSize.Width, _atlasSize.Height, true, _progress);

            List<(PixelRect, IAtlasTile)> result = new List<(PixelRect, IAtlasTile)>();
            if (!skylinePacker.TryInsert(changedTiles, result, SkylineBinPacker.LevelChoiceHeuristic.LevelBottomLeft))
            {
                layer = null;
                return false;
            }

            var atlasLayer = new SkylineAtlasMapLayer(result, _margin, _atlasSize);
            layer = atlasLayer;

            Coverage = coverage;

            return true;
        }
    }
}