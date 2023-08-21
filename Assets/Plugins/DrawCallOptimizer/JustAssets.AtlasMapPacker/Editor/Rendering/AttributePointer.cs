namespace JustAssets.AtlasMapPacker.Rendering
{
    public class AttributePointer
    {
        public AttributePointer()
        {}

        public AttributePointer(DataSource dataSource, ValueComponent component = ValueComponent.All)
        {
            DataSource = dataSource;
            Name = null;
            Component = component;
        }

        public AttributePointer(string name, ValueComponent component = ValueComponent.All) : this(name,
            DataSource.TextureAttribute, component)
        {
        }

        public AttributePointer(string name, DataSource dataSource = DataSource.TextureAttribute, ValueComponent component = ValueComponent.All)
        {
            DataSource = dataSource;
            Name = name;
            Component = component;
        }

        public DataSource DataSource { get; set; }

        public ValueComponent Component { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name ?? DataSource.ToString()}.{Component}";
        }
    }
}