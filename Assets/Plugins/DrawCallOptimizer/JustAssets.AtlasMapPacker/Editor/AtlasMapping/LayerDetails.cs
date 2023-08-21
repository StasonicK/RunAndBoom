using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class LayerDetails
    {
        public LayerDetails(List<AtlasMapEntry> tiles, PixelSize atlasSize, uint margin, List<string> textureSlotNames)
        {
            Tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
            AtlasSize = atlasSize;
            Margin = margin;
            TextureSlotNames = textureSlotNames;

            var atlasSurface = atlasSize.Width * atlasSize.Height;

            Coverage = ComputeCoverage(atlasSurface, tiles);

            AtlasTextures = new Dictionary<string, Texture>();
        }

        private float ComputeCoverage(long atlasSurface, List<AtlasMapEntry> tiles)
        {
            if (atlasSurface == 0) 
                return 0;

            long sum = 0L;
            foreach (AtlasMapEntry x in tiles)
            {
                sum += x.Rectangle.Width * x.Rectangle.Height;
            }

            return sum * 100 / atlasSurface / 100f;
        }

        public List<AtlasMapEntry> Tiles { get; }

        public PixelSize AtlasSize { get; }

        public uint Margin { get; }

        public Dictionary<string, Texture> AtlasTextures { get; }

        public List<string> TextureSlotNames { get; }

        public Material AtlasMaterial { get; set; }

        public int HighestAtlasMipLevel { get; set; }

        public float Coverage { get; }
    }
}