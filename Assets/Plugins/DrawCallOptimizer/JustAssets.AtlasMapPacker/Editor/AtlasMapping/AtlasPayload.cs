using System.Collections.Generic;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class AtlasPayload
    {
        public AtlasPayload(TextureTile textureTile, string textureName, List<Material> allMaterials)
        {
            TextureTile = textureTile;
            TextureName = textureName;
            AllMaterials = allMaterials;
        }

        public TextureTile TextureTile { get; }

        public string TextureName { get; }

        public List<Material> AllMaterials { get; }
    }
}