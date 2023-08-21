using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    internal class CommandEndToken : IPart
    {
        public string LineEndToken { get; }

        public CommandEndToken(string lineEndToken = ";")
        {
            LineEndToken = lineEndToken;
        }

        public string Serialize()
        {
            return LineEndToken;
        }
    }
}