using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    internal class Content : ICollectionPart
    {
        private readonly int _indentLevel;

        private readonly string _lineBreak;

        public IList<IPart> Parts { get; }

        public Content(string body, int indentLevel, string lineBreak, ICollection<string> macroWords)
        {
            var operationSpan = new StringSpan(body);
            operationSpan.ReadWhile(char.IsWhiteSpace).Without("\r\n");
            _indentLevel = indentLevel;
            _lineBreak = lineBreak;

            Parts = CGCommandParser.Parse(operationSpan, indentLevel+1, lineBreak, macroWords).ToList();
        }

        public string Serialize()
        {
            string result = null;
            foreach (IPart x in Parts)
            {
                var command = x.Serialize();
         
                result += $"{command}{_lineBreak}";
            }

            var indenting = string.Empty.PadLeft(_indentLevel, '\t');
            return $"{{{_lineBreak}{result}{indenting}}}";
        }
    }
}