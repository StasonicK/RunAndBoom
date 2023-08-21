namespace JustAssets.AtlasMapPacker.Rendering
{
    public class AttributePointerPair
    {
        public AttributePointer Source { get; set; }

        public AttributeTargetPointer Target { get; set; }

        public override string ToString()
        {
            return $"{Source} -> {Target}";
        }
    }
}