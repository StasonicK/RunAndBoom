namespace JustAssets.ShaderPatcher.Parts
{
    internal class LineBreak : IPart
    {
        public LineBreak(int indentLevel)
        {
            IndentLevel = indentLevel;
        }

        public string Serialize()
        {
            return "";
        }

        public int IndentLevel { get; }
    }
}