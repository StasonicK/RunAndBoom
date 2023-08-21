using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    internal class SyntaxToken : IPart
    {
        public string Token { get; }

        public SyntaxToken(string token)
        {
            Token = token;
        }

        public string Serialize()
        {
            return StringUtils.PadIfSet(Token);
        }
    }
}