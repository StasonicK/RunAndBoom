using System.Diagnostics;
using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Meshes
{
    [DebuggerDisplay("{Size.Width}x{Size.Height}")]
    internal readonly struct LightmapTile : IAtlasTile
    {
        public LightmapTile(PixelSize size, Mesh payload)
        {
            Size = size;
            Payload = payload;
        }

        public PixelSize Size { get; }

        object IAtlasTile.Payload => Payload;

        public IAtlasTile Clone(PixelSize newSize)
        {
            return new LightmapTile(newSize, Payload);
        }

        public Mesh Payload { get; }
    }
}