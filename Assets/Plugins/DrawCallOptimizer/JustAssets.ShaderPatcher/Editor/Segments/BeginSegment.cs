namespace JustAssets.ShaderPatcher.Segments
{
    internal class BeginSegment : Segment
    {
        private string _lineBreak;

        public string Word { get; }

        public string Content { get; set; }

        public BeginSegment(string word, string content, int indent, string lineBreak) : base(indent, "")
        {
            _lineBreak = lineBreak;
            Word = word;
            Content = content;
        }

        public override string Value => Indentation + Content + _lineBreak;
    }
}