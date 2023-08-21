using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    internal class MemberAccessor : IPart
    {
        public MemberAccessor()
        {
            IndentLevel = 0;
        }

        public string Serialize()
        {
            return ".";
        }

        public int IndentLevel { get; }
    }
}