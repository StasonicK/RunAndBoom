using System;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public readonly struct ColorConfig : IAttributeConfig
    {
        private readonly float _colorSimilarityThreshold;

        public bool Equals(ColorConfig other)
        {
            var diff = Math.Abs(Color.a - other.Color.a) + Math.Abs(Color.r - other.Color.r) + Math.Abs(Color.g - other.Color.g) +
                          Math.Abs(Color.b - other.Color.b);
            return diff < _colorSimilarityThreshold;
        }

        public override bool Equals(object obj)
        {
            return obj is ColorConfig other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }

        public Color Color { get; }

        public Material Material { get; }

        public ColorConfig(Material material, Color color, float colorSimilarityThreshold)
        {
            _colorSimilarityThreshold = colorSimilarityThreshold;
            Material = material;
            Color = color;
        }

        public override string ToString()
        {
            return $"'{Color} (\"{(Material != null ? Material.name : "null")}\")";
        }
    }
}