using System;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public readonly struct AtlasRect : IComparable, IComparable<AtlasRect>
    {
        public float X { get; }

        public float Y { get; }

        public float Width { get; }

        public float Height { get; }

        public float XMax { get; }

        public float YMax { get; }

        public AtlasPosition Position => new AtlasPosition(X, Y);

        public AtlasSize Size => new AtlasSize(Width, Height);

        public static AtlasRect Zero => new AtlasRect(0, 0, 0, 0);

        public AtlasRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            XMax = X + Width;
            YMax = Y + Height;
        }

        public AtlasRect(AtlasPosition position, AtlasSize size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.Width;
            Height = size.Height;
            XMax = X + Width;
            YMax = Y + Height;
        }

        public bool Intersects(AtlasRect other)
        {
            return X + Width >= other.X && other.X + other.Width >= X && Y + Height >= other.Y && other.Y + other.Height >= Y;
        }

        /// <summary>
        ///     Returns if this rectangle lies inside of the given one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsInside(AtlasRect other)
        {
            return X >= other.X && Y >= other.Y && XMax <= other.XMax && YMax <= other.YMax;
        }

        public bool Equals(AtlasRect other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            return obj is AtlasRect other && Equals(other);
        }

        public int CompareTo(AtlasRect other)
        {
            var xComparison = X.CompareTo(other.X);
            if (xComparison != 0)
                return xComparison;

            var yComparison = Y.CompareTo(other.Y);
            if (yComparison != 0)
                return yComparison;

            var widthComparison = Width.CompareTo(other.Width);
            if (widthComparison != 0)
                return widthComparison;

            return Height.CompareTo(other.Height);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;

            return obj is AtlasRect other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(AtlasRect)}");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(AtlasRect x, AtlasRect y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(AtlasRect x, AtlasRect y)
        {
            return !x.Equals(y);
        }

        public bool Contains(AtlasRect nodeBounds)
        {
            return nodeBounds.X >= X && nodeBounds.Y >= Y && nodeBounds.XMax <= XMax && nodeBounds.YMax <= YMax;
        }

        public static AtlasRect operator /(AtlasRect x, AtlasSize div)
        {
            return new AtlasRect(x.X / div.Width, x.Y / div.Height, x.Width / div.Width, x.Height / div.Height);
        }
    }
}