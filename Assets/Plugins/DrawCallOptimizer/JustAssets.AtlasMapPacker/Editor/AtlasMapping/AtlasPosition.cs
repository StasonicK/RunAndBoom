namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public struct AtlasPosition
    {
        public float X { get; }
        public float Y { get; }

        public AtlasPosition(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public static AtlasPosition operator +(AtlasPosition lhs, AtlasPosition rhs)
        {
            return new AtlasPosition(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
    }
}

