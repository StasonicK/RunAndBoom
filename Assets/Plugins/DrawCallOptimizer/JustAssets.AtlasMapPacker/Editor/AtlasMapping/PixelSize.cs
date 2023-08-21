using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public readonly struct PixelSize
    {
        public long Width { get; }
        public long Height { get; }
        public long SqrMagnitude => Width * Width + Height * Height;
        public static PixelSize Zero => new PixelSize(0, 0);

        public PixelSize(long width, long height)
        {
            Width = width;
            Height = height;
        }

        public static AtlasSize operator *(PixelSize a, float b)
        {
            return new AtlasSize(a.Width * b, a.Height * b);
        }

        public static PixelSize operator *(PixelSize a, long b)
        {
            return new PixelSize(a.Width * b, a.Height * b);
        }
        public static PixelSize operator *(PixelSize a, double b)
        {
            return new PixelSize((long)Math.Ceiling(a.Width * b), (long)Math.Ceiling(a.Height * b));
        }
    }
}