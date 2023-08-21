using System.Text.RegularExpressions;

namespace JustAssets.ShaderPatcher.Segments
{
    internal class PropertySegment : Segment
    {
        public PropertySegment(string value, GroupCollection matchGroups, string lineBreak, int indent) : base(indent, value)
        {
            Match match = (Match) matchGroups[0];
            LineBreak = lineBreak;
            Attributes = match.Groups["Attributes"].Value;
            Description = match.Groups["Description"].Value;
            Name = match.Groups["Name"].Value;
            AttributeType = match.Groups["Type"].Value;
            DefaultValue = match.Groups["DefaultValue"].Value;
        }

        public PropertySegment(string lineBreak, int indent, string description, string defaultValue, string attributeType, string name, string attributes) : base(indent, null)
        {
            LineBreak = lineBreak;
            Description = description;
            DefaultValue = defaultValue;
            AttributeType = attributeType;
            Name = name;
            Attributes = attributes;
        }

        public string Description { get; set; }

        public string DefaultValue { get; set; }

        public string AttributeType { get; set; }

        public string Name { get; set; }

        public string Attributes { get; set; }

        public override string Value => $"{Indentation}{Attributes}{Name}(\"{Description}\", {AttributeType}) = {DefaultValue}{LineBreak}";

        public string LineBreak { get; }
    }
}