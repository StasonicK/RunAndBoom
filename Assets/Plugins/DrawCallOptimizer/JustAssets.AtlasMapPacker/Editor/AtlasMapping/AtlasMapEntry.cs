using System.Diagnostics;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    [DebuggerDisplay("Entry: Rect {Rectangle.X}, {Rectangle.Y}: {Rectangle.Width}x{Rectangle.Height}")]
    public class AtlasMapEntry
    {
        private readonly object _payload;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        /// <param name="rectangle">In pixels.</param>
        /// <param name="payload">Some data.</param>
        public AtlasMapEntry(PixelRect rectangle, object payload = default, AtlasRect uvRectangle = default)
        {
            Rectangle = rectangle;
            UVRectangle = uvRectangle;
            _payload = payload;
        }

        /// <summary>
        ///     Pixel space rectangle on the atlas map.
        /// </summary>
        public PixelRect Rectangle { get; }

        /// <summary>
        ///     UV space rectangle on the atlas map.
        /// </summary>
        public AtlasRect UVRectangle { get; }

        /// <summary>
        ///     Any data to keep along with this entry.
        /// </summary>
        public T Payload<T>()
        {
            return (T)_payload;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Rectangle: {Rectangle}, Payload: {_payload}";
        }
    }
}