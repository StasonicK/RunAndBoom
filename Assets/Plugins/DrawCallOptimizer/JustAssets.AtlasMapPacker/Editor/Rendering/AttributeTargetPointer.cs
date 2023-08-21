namespace JustAssets.AtlasMapPacker.Rendering
{
    public sealed class AttributeTargetPointer : AttributePointer
    {
        public AttributeTargetPointer()
        {
        }

        public AttributeTargetPointer(DataSource dataSource, ValueComponent component = ValueComponent.All) : base(
            dataSource, component)
        {
            MaximumAttribute = null;
        }

        public AttributeTargetPointer(string name, ValueComponent component = ValueComponent.All,
            string maximumAttribute = null) : this(name, DataSource.TextureAttribute, component, maximumAttribute)
        {
        }

        public AttributeTargetPointer(string name, DataSource dataSource = DataSource.TextureAttribute,
            ValueComponent component = ValueComponent.All, string maximumAttribute = null) : base(name, dataSource,
            component)
        {
            MaximumAttribute = maximumAttribute;
        }

        public string MaximumAttribute { get; set; }

        public bool UseTiling { get; set; }

        public override string ToString()
        {
            return $"{Name ?? DataSource.ToString()}.{Component}";
        }
    }
}