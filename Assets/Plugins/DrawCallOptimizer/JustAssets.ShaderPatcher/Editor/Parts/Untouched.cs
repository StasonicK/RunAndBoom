using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    public class Untouched : IPart
    {
        public string Content { get; }

        public Untouched(string content, int indentLevel)
        {
            Content = content;
            IndentLevel = indentLevel;
        }

        public string Serialize()
        {
            return Content;
        }

        public int IndentLevel { get; }
    }
}