using System.Collections.Generic;
using JustAssets.AtlasMapPacker.Rendering;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class TextureTile
    {
        public TextureTile(MaterialUsage materialUsage, List<AttributePointerPair> attributes, PixelRect rectangle)
        {
            MaterialUsage = materialUsage;
            Attributes = attributes;
            Rectangle = rectangle;
        }

        public MaterialUsage MaterialUsage { get; }

        public List<AttributePointerPair> Attributes { get; }

        public PixelRect Rectangle { get; }
    }
}