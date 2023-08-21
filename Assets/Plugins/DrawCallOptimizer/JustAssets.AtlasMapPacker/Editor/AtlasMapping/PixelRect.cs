using System.Diagnostics;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    [DebuggerDisplay("{X}, {Y}: {Width}x{Height}")]
    public readonly struct PixelRect
    {
        public long X { get; }

        public long Y { get; }

        public long Width { get; }

        public long Height { get; }

        public long XMax { get; }

        public long YMax { get; }

        public AtlasPosition Position => new AtlasPosition(X, Y);

        public PixelSize Size => new PixelSize(Width, Height);

        public static PixelRect Zero => new PixelRect(0, 0, 0, 0);

        public PixelRect(long x, long y, long width, long height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            XMax = X + Width;
            YMax = Y + Height;
        }
        
        public static AtlasRect operator /(PixelRect x, PixelSize div)
        {
            return new AtlasRect((float)((double)x.X / div.Width), (float)((double)x.Y / div.Height), (float)((double)x.Width / div.Width),
                (float)((double)x.Height / div.Height));
        }
    }
}