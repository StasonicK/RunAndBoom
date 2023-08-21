using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    public class Brackets : ICollectionPart
    {
        private ICollectionPart _parsed;

        public Brackets(StringSpan content, string lineEnding, ICollection<string> macroWords, int indentLevel)
        {
            _parsed = (ICollectionPart) CGCommandParser.Parse(content, indentLevel, lineEnding, macroWords).First();

            IndentLevel = indentLevel;
        }

        public string Serialize()
        {
            return $"({_parsed.Serialize()})";
        }

        public int IndentLevel { get; }

        public IList<IPart> Parts => _parsed.Parts;
    }
}