using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    public class Variable : IPart
    {
        public string Name { get; set; }

        public Variable(string name)
        {
            Name = name;
            IndentLevel = 0;
        }

        public string Serialize()
        {
            return Name;
        }

        public int IndentLevel { get; }
    }
}