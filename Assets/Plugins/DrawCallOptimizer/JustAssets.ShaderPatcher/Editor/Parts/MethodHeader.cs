using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    public class MethodHeader : ICollectionPart
    {
        public int IndentLevel { get; }

        public string Name { get; set; }

        public IList<IPart> Parts { get; }

        public string LineBreak { get; }

        public IPart this[int i] => Parts[i];

        public MethodHeader(string name, string parameters, string lineEnding, ICollection<string> macroWords,
            int indentLevel)
        {
            IndentLevel = indentLevel;
            LineBreak = lineEnding;
            Name = name;
            Parts = ParseParameters(new StringSpan(parameters), lineEnding, macroWords, indentLevel);
        }

        private List<IPart> ParseParameters(StringSpan parameters, string lineEnding, ICollection<string> macroWords,
            int indentLevel)
        {
            var allParsed = (ICollectionPart)CGCommandParser.Parse(parameters, indentLevel, lineEnding, macroWords).FirstOrDefault();

            var result = new List<IPart>();
            if (allParsed == null)
                return result;

            var parts = new List<IPart>();

            foreach (var part in allParsed.Parts)
            {
                if (part is SyntaxToken syntaxToken && syntaxToken.Token == ",")
                {
                    result.Add(parts.Count > 1 ? new Formula(parts, lineEnding, indentLevel) : parts.FirstOrDefault());
                    parts.Clear();
                    continue;
                }
                parts.Add(part);
            }

            if (parts.Count > 0)
                result.Add(parts.Count > 1 ? new Formula(parts, lineEnding, IndentLevel) : parts.FirstOrDefault());

            return result;
        }

        public string Serialize()
        {
            var parameters = Parts.Select(x => x.Serialize()).ToList();
            var paramString = parameters.Count > 0 ? parameters.Aggregate((a, b) => a + ", " + b) : "";
            return Name + "(" + paramString + ")";
        }
    }
}