using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    public class Numeric : IPart
    {
        public string Number { get; set; }

        public Numeric(string number, int indentLevel)
        {
            Number = number;
            IndentLevel = indentLevel;
        }

        public string Serialize()
        {
            return Number;
        }

        public int IndentLevel { get; }
    }
}