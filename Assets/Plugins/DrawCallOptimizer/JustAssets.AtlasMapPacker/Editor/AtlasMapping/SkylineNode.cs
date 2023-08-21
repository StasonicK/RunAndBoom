namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    /// Represents a single level (a horizontal line) of the skyline/horizon/envelope.
    internal struct SkylineNode
    {
        /// The starting X-coordinate (leftmost).
        public long X;

        /// The Y-coordinate of the skyline level line.
        public long Y;

        /// The line Width. The ending coordinate (inclusive) will be X+Width-1.
        public long Width;

        public SkylineNode(long x, long y, long width)
        {
            X = x;
            Y = y;
            Width = width;
        }
    }
}