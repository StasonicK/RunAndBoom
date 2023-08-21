namespace JustAssets.ShaderPatcher.Segments
{
    internal class VariableDefinitionSegment : Segment
    {
        private string _lineBreak;

        public string Type { get; set; }

        public string Name { get; set; }

        public VariableDefinitionSegment(int indent, string type, string name, string lineBreak) : base(indent, null)
        {
            Type = type;
            Name = name;
            _lineBreak = lineBreak;
        }

        public override string Value => $"{Indentation}{Type} {Name};{_lineBreak}";
    }
}