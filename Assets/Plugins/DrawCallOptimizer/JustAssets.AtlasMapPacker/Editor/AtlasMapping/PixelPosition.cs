namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public struct PixelPosition
    {
        public ulong X { get; }
        public ulong Y { get; }

        public PixelPosition(ulong x, ulong y)
        {
            this.X = x;
            this.Y = y;
        }

        public static PixelPosition operator +(PixelPosition lhs, PixelPosition rhs)
        {
            return new PixelPosition(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
    }
}