using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public readonly struct TextureConfig : IAttributeConfig
    {
        public bool Equals(TextureConfig other)
        {
            return Equals(Texture, other.Texture) && Offset.Equals(other.Offset) && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj)
        {
            return obj is TextureConfig other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Texture != null ? Texture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                hashCode = (hashCode * 397) ^ Scale.GetHashCode();
                return hashCode;
            }
        }

        public Texture Texture { get; }

        public Color TextureFallbackColor { get; }

        public Vector2 Offset { get; }

        public Vector2 Scale { get; }

        public Vector2Int Dimensions { get; }

        public Material Material { get; }

        public TextureConfig(Material material, Texture texture, Color textureFallbackColor, Vector2 offset, Vector2 scale)
        {
            Material = material;
            Texture = texture;
            TextureFallbackColor = textureFallbackColor;
            Offset = offset;
            Scale = scale;
            Dimensions = new Vector2Int(texture?.width ?? 0, texture?.height ?? 0);
        }

        public override string ToString()
        {
            return $"'{(Texture != null ? Texture.name : "null")}' {Dimensions} (\"{(Material != null ? Material.name : "null")}\")";
        }
    }
}